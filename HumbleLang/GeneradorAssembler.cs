using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace HumbleLang
{
    public class GeneradorAssembler
    {
        private List<Cuadruplo> cuadruplos;
        // Necesitamos saber qué variables existen para declararlas en el .ASM
        // Como tu TablaSimbolos actual se limpia en el intérprete, 
        // usaremos una lista simple de variables detectadas en los cuádruplos.
        private HashSet<string> variablesDeclaradas = new HashSet<string>();

        public GeneradorAssembler(List<Cuadruplo> cuadruplos)
        {
            this.cuadruplos = cuadruplos;
        }

        public void GenerarArchivo(string rutaSalida)
        {
            StringBuilder asm = new StringBuilder();

            // 1. Recolectar todas las variables y temporales (T1, T2...) usados
            AnalizarVariables();

            // 2. Encabezado MASM
            asm.AppendLine("; --- CÓDIGO ENSAMBLADOR GENERADO POR HUMBLELANG ---");
            asm.AppendLine(".model small");
            asm.AppendLine(".stack 100h");
            asm.AppendLine("");

            // 3. Segmento de DATOS (Variables)
            asm.AppendLine(".data");
            foreach (var variable in variablesDeclaradas)
            {
                // Declaramos todo como DW (Define Word - 16 bits) inicializado en 0
                // En un compilador real, distinguirías entre DB, DW, DD según el tipo.
                asm.AppendLine($"    {variable} dw 0");
            }
            // Variables auxiliares para impresión (truco para imprimir números)
            asm.AppendLine("    msg_print db 'Salida: $'");
            asm.AppendLine("    newline db 13, 10, '$'");
            asm.AppendLine("");

            // 4. Segmento de CÓDIGO (Lógica)
            asm.AppendLine(".code");
            asm.AppendLine("main proc");
            asm.AppendLine("    mov ax, @data");
            asm.AppendLine("    mov ds, ax");
            asm.AppendLine("");

            // 5. Traducir Cuádruplos
            foreach (var c in cuadruplos)
            {
                asm.Append(TraducirInstruccion(c));
            }

            // 6. Cierre del programa (Exit DOS)
            asm.AppendLine("    ; --- FIN DEL PROGRAMA ---");
            asm.AppendLine("    mov ax, 4C00h");
            asm.AppendLine("    int 21h");
            asm.AppendLine("main endp");

            // Rutina auxiliar para imprimir números (Complejidad: ALTA)
            // La agregaremos después, por ahora solo imprimiremos caracteres básicos.
            asm.AppendLine("end main");

            // Guardar archivo
            File.WriteAllText(rutaSalida, asm.ToString());
        }

        private void AnalizarVariables()
        {
            foreach (var c in cuadruplos)
            {
                // Si el resultado es una variable o temporal (no vacío), la agregamos
                if (!string.IsNullOrEmpty(c.Resultado) && !EsNumero(c.Resultado) && !c.Resultado.StartsWith("\""))
                {
                    variablesDeclaradas.Add(c.Resultado);
                }
                // Si los argumentos son variables (no números ni temporales ya agregados)
                if (!string.IsNullOrEmpty(c.Arg1) && !EsNumero(c.Arg1) && !c.Arg1.StartsWith("\"") && !c.Arg1.StartsWith("'"))
                    variablesDeclaradas.Add(c.Arg1);

                if (!string.IsNullOrEmpty(c.Arg2) && !EsNumero(c.Arg2) && !c.Arg2.StartsWith("\"") && !c.Arg2.StartsWith("'"))
                    variablesDeclaradas.Add(c.Arg2);
            }
        }

        private string TraducirInstruccion(Cuadruplo c)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine($"    ; {c.Operador} {c.Arg1} {c.Arg2} -> {c.Resultado}");

            switch (c.Operador)
            {
                // --- ASIGNACIÓN (OPAS) ---
                case "OPAS":
                    // MOV destino, origen
                    // En Assembly no se puede MOV mem, mem. Hay que usar registro intermedio (AX).
                    if (EsNumero(c.Arg1))
                    {
                        sb.AppendLine($"    MOV AX, {c.Arg1}");
                    }
                    else
                    {
                        sb.AppendLine($"    MOV AX, {c.Arg1}");
                    }
                    sb.AppendLine($"    MOV {c.Resultado}, AX");
                    break;

                // --- ARITMÉTICA ---
                case "+":
                    sb.AppendLine($"    MOV AX, {c.Arg1}"); // Cargar Arg1
                    sb.AppendLine($"    ADD AX, {c.Arg2}"); // Sumar Arg2
                    sb.AppendLine($"    MOV {c.Resultado}, AX"); // Guardar
                    break;

                case "-":
                    sb.AppendLine($"    MOV AX, {c.Arg1}");
                    sb.AppendLine($"    SUB AX, {c.Arg2}");
                    sb.AppendLine($"    MOV {c.Resultado}, AX");
                    break;

                case "*":
                    sb.AppendLine($"    MOV AX, {c.Arg1}");
                    // MUL usa AX implícitamente y guarda en DX:AX
                    // Asumiremos números pequeños que caben en 16 bits para simplificar
                    if (EsNumero(c.Arg2))
                    {
                        // MUL no acepta inmediatos, hay que mover a registro
                        sb.AppendLine($"    MOV BX, {c.Arg2}");
                        sb.AppendLine($"    MUL BX");
                    }
                    else
                    {
                        sb.AppendLine($"    MUL {c.Arg2}");
                    }
                    sb.AppendLine($"    MOV {c.Resultado}, AX");
                    break;

                case "/":
                    sb.AppendLine($"    MOV AX, {c.Arg1}");
                    sb.AppendLine("    CWD"); // Extender signo a DX antes de dividir
                    if (EsNumero(c.Arg2))
                    {
                        sb.AppendLine($"    MOV BX, {c.Arg2}");
                        sb.AppendLine($"    DIV BX");
                    }
                    else
                    {
                        sb.AppendLine($"    DIV {c.Arg2}");
                    }
                    sb.AppendLine($"    MOV {c.Resultado}, AX"); // Cociente en AX
                    break;

                // --- SALTOS Y ETIQUETAS (Para IF y CICLOS) ---
                case "LABEL":
                    sb.AppendLine($"{c.Resultado}:");
                    break;

                case "GOTO":
                    sb.AppendLine($"    JMP {c.Resultado}");
                    break;

                case "JUMP_FALSE":
                    // Asumimos que la comparación se hizo antes y el resultado está en una variable
                    // O simplificamos: HumbleLang usa Tmp para la condición.
                    // Para hacerlo real, necesitaríamos implementar CMP (Comparar).
                    // Por ahora, asumiremos que Arg1 es el resultado de una comparación anterior.
                    sb.AppendLine($"    MOV AX, {c.Arg1}");
                    sb.AppendLine($"    CMP AX, 0");     // ¿Es falso (0)?
                    sb.AppendLine($"    JE {c.Resultado}"); // Jump Equal (Si es 0, salta)
                    break;

                // --- COMPARACIONES (RELACIONALES) ---
                // HumbleLang genera: (>, a, b, T1)
                case ">":
                case "<":
                case "==":
                    sb.AppendLine($"    MOV AX, {c.Arg1}");
                    sb.AppendLine($"    CMP AX, {c.Arg2}");
                    // Esto es complejo en ASM: hay que setear 1 o 0 según flags.
                    // Truco rápido:
                    string salto = c.Operador == ">" ? "JG" : (c.Operador == "<" ? "JL" : "JE");
                    string lblTrue = "L_TRUE_" + Guid.NewGuid().ToString().Substring(0, 4);
                    string lblFin = "L_FIN_" + Guid.NewGuid().ToString().Substring(0, 4);

                    sb.AppendLine($"    {salto} {lblTrue}");
                    sb.AppendLine($"    MOV {c.Resultado}, 0"); // Falso
                    sb.AppendLine($"    JMP {lblFin}");
                    sb.AppendLine($"{lblTrue}:");
                    sb.AppendLine($"    MOV {c.Resultado}, 1"); // Verdadero
                    sb.AppendLine($"{lblFin}:");
                    break;

                // --- IMPRIMIR (PR09) ---
                case "PR09": // imp
                    // Imprimir cadenas es "fácil" (INT 21h, AH=09h)
                    // Imprimir números es difícil (requiere convertir binario a ASCII)
                    // Por simplicidad en esta etapa, solo dejaremos el comentario.
                    sb.AppendLine($"    ; TODO: Imprimir {c.Arg1}");
                    break;
            }
            sb.AppendLine("");
            return sb.ToString();
        }

        private bool EsNumero(string s)
        {
            return int.TryParse(s, out _);
        }
    }
}
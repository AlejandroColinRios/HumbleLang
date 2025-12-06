using System;
using System.Collections.Generic;
using System.Linq;

public static class GeneradorCodigoIntermedio
{
    // Define los tokens de operadores que se deben procesar
    private static readonly HashSet<string> Operadores = new HashSet<string>
    {
        "OPRE1", "OPRE2", "OPRE3", "OPRE4", "OPRE5", // Aritméticos: +, -, *, /, %
        "OPREL1", "OPREL2", "OPREL3", "OPREL4", "OPREL5", "OPREL6", // Relacionales: ==, !=, <, >, <=, >=
        "OPLO1", "OPLO2", "PR16" // Lógicos: y, o, negación (unario)
    };

    // Estructura para almacenar el Cuádruplo
    public record Cuadruplo(string Operador, string Arg1, string Arg2, string Resultado);

    public static List<Cuadruplo> GenerarCuadruplos(List<(string Token, int Linea)> tokensPostfijos)
    {
        var cuadruplos = new List<Cuadruplo>();
        // Pila para almacenar operandos (IDEN, Constantes) y resultados temporales (T1, T2, etc.)
        var pilaOperandos = new Stack<string>();
        int contadorTemporal = 1;

        foreach (var tokenActual in tokensPostfijos)
        {
            string tipoToken = tokenActual.Token;
            string valorToken = tokenActual.Item1; // En este contexto, usamos el Token como su valor para simplificar

            // 1. Si es un operando (IDEN, CN_ENTERA, CADE, etc.), va a la pila.
            if (!Operadores.Contains(tipoToken))
            {
                pilaOperandos.Push(valorToken);
            }
            // 2. Si es un operador, se desapilan argumentos, se genera el Cuádruplo y se apila el resultado.
            else
            {
                string resultado = $"T{contadorTemporal}";

                if (tipoToken == "PR16") // Operador Unario (Negación 'no')
                {
                    if (pilaOperandos.Count < 1)
                        throw new InvalidOperationException($"Error: Operador unario '{tipoToken}' sin operando.");

                    string arg1 = pilaOperandos.Pop();
                    cuadruplos.Add(new Cuadruplo(tipoToken, arg1, "", resultado));
                }
                else // Operador Binario
                {
                    if (pilaOperandos.Count < 2)
                        throw new InvalidOperationException($"Error: Operador binario '{tipoToken}' sin suficientes operandos.");

                    // Nota: Se desapila primero el Arg2 y luego el Arg1 para mantener el orden.
                    string arg2 = pilaOperandos.Pop();
                    string arg1 = pilaOperandos.Pop();
                    cuadruplos.Add(new Cuadruplo(tipoToken, arg1, arg2, resultado));
                }

                // Se apila el nuevo temporal (T1, T2, etc.)
                pilaOperandos.Push(resultado);
                contadorTemporal++;
            }
        }

        return cuadruplos;
    }
}
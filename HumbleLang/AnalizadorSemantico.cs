using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HumbleLang
{
    public enum TipoDato { ENTERO, CADENA, BOOLEANO, DESCONOCIDO }

    // CLASE CUADRUPLO (Para Código Intermedio)
    public class Cuadruplo
    {
        public string Operador { get; set; }
        public string Arg1 { get; set; }
        public string Arg2 { get; set; }
        public string Resultado { get; set; }

        public Cuadruplo(string op, string arg1, string arg2, string res)
        {
            Operador = op;
            Arg1 = arg1;
            Arg2 = arg2;
            Resultado = res;
        }

        public override string ToString()
        {
            return $"({Operador}, {Arg1}, {Arg2}, {Resultado})";
        }
    }

    public class ErrorSemantico
    {
        public string Mensaje { get; set; }
        public int Linea { get; set; }

        public ErrorSemantico(string mensaje, int linea)
        {
            Mensaje = mensaje;
            Linea = linea;
        }
    }

    public class Simbolo
    {
        public string Nombre { get; set; }
        public TipoDato Tipo { get; set; }
        public int Linea { get; set; }

        public Simbolo(string nombre, TipoDato tipo, int linea)
        {
            Nombre = nombre;
            Tipo = tipo;
            Linea = linea;
        }
    }

    public class AnalizadorSemantico
    {
        private Stack<Dictionary<string, Simbolo>> tablaSimbolosStack = new Stack<Dictionary<string, Simbolo>>();
        public List<ErrorSemantico> Errores { get; private set; } = new List<ErrorSemantico>();

        // NUEVOS MIEMBROS PARA CÓDIGO INTERMEDIO
        public List<Cuadruplo> CuadruplosGenerados { get; private set; } = new List<Cuadruplo>();
        private int contadorTemporal = 1;
        private int contadorEtiqueta = 1;

        private string GenerarTemporal()
        {
            return $"T{contadorTemporal++}";
        }

        private string GenerarEtiqueta()
        {
            return $"L{contadorEtiqueta++}";
        }

        private TipoDato ObtenerTipoDato(string nombreResultado, TipoDato tipoOriginal = TipoDato.DESCONOCIDO)
        {
            if (nombreResultado == "ERROR" || nombreResultado == "null") return TipoDato.DESCONOCIDO;
            if (nombreResultado.StartsWith("T")) return tipoOriginal;
            if (nombreResultado.StartsWith("\"") || nombreResultado.StartsWith("'")) return TipoDato.CADENA;
            if (int.TryParse(nombreResultado, out _)) return TipoDato.ENTERO;
            if (nombreResultado == "verdadero" || nombreResultado == "falso") return TipoDato.BOOLEANO;

            if (BuscarSimbolo(nombreResultado, out var simbolo)) return simbolo.Tipo;

            return TipoDato.DESCONOCIDO;
        }

        public void Analizar(NodoAST raiz)
        {
            tablaSimbolosStack.Push(new Dictionary<string, Simbolo>());
            RevisarNodo(raiz);
            tablaSimbolosStack.Pop();
        }

        private bool BuscarSimbolo(string nombre, out Simbolo simbolo)
        {
            foreach (var ambito in tablaSimbolosStack)
            {
                if (ambito.TryGetValue(nombre, out simbolo)) return true;
            }
            simbolo = null;
            return false;
        }

        private bool BuscarSimboloEnAmbitoActual(string nombre)
        {
            if (tablaSimbolosStack.Count > 0) return tablaSimbolosStack.Peek().ContainsKey(nombre);
            return false;
        }

        private void RevisarNodo(NodoAST nodo)
        {
            if (nodo == null) return;

            switch (nodo.Tipo)
            {
                case "PROG":
                case "STMT_LIST":
                case "BLOCK":
                    if (nodo.Tipo == "BLOCK") tablaSimbolosStack.Push(new Dictionary<string, Simbolo>());
                    foreach (var hijo in nodo.Hijos) RevisarNodo(hijo);
                    if (nodo.Tipo == "BLOCK") tablaSimbolosStack.Pop();
                    break;
                case "DECLARACION": RevisarDeclaracion(nodo); break;
                case "ASIGNACION": RevisarAsignacion(nodo); break;
                case "IF_STMT": RevisarIf(nodo); break;
                case "PRINT_STMT": RevisarPrint(nodo); break;
                case "CICLO": RevisarCiclo(nodo); break;
                case "ROMPER":
                case "LIMPIAR":
                    if (nodo.Tipo == "LIMPIAR") CuadruplosGenerados.Add(new Cuadruplo("PR14", "", "", ""));
                    if (nodo.Tipo == "ROMPER") CuadruplosGenerados.Add(new Cuadruplo("GOTO", "", "", "L_BREAK")); // Requiere parche de etiqueta
                    break;
                default:
                    if (nodo.Hijos.Any()) foreach (var hijo in nodo.Hijos) RevisarNodo(hijo);
                    break;
            }
        }

        private void RevisarDeclaracion(NodoAST nodo)
        {
            if (nodo.Hijos.Count < 2) return;
            var tipoNodo = nodo.Hijos[0];
            var decPrimeNodo = nodo.Hijos[1];
            if (decPrimeNodo.Hijos.Count == 0) return;
            var idNodo = decPrimeNodo.Hijos[0];

            if (BuscarSimboloEnAmbitoActual(idNodo.Valor)) { /* Error */ return; }
            TipoDato tipoDeclarado = TipoDato.DESCONOCIDO;
            switch (tipoNodo.Tipo)
            {
                case "PR05": tipoDeclarado = TipoDato.ENTERO; break;
                case "PR01": tipoDeclarado = TipoDato.CADENA; break;
                case "PR15": tipoDeclarado = TipoDato.BOOLEANO; break;
                case "PR03": tipoDeclarado = TipoDato.ENTERO; break;
            }
            if (tipoDeclarado != TipoDato.DESCONOCIDO) tablaSimbolosStack.Peek().Add(idNodo.Valor, new Simbolo(idNodo.Valor, tipoDeclarado, idNodo.Linea));

            if (decPrimeNodo.Hijos.Count > 1)
            {
                var exprNodo = decPrimeNodo.Hijos[2];
                string resultadoExpr = RevisarExpresion(exprNodo, out TipoDato tipoExpr);
                if (tipoExpr != tipoDeclarado && tipoExpr != TipoDato.DESCONOCIDO) { /* Error */ }

                // Generar Cuádruplo de ASIGNACIÓN
                CuadruplosGenerados.Add(new Cuadruplo("OPAS", resultadoExpr, "", idNodo.Valor));
            }
        }

        private void RevisarAsignacion(NodoAST nodo)
        {
            if (nodo.Hijos.Count < 3) return;
            var id = nodo.Hijos[0];
            var opAsig = nodo.Hijos[1];
            var expr = nodo.Hijos[2];

            if (!BuscarSimbolo(id.Valor, out var simbolo)) { /* Error */ return; }
            string resultadoExpr = RevisarExpresion(expr, out TipoDato tipoExprResultante);
            if (tipoExprResultante != simbolo.Tipo && tipoExprResultante != TipoDato.DESCONOCIDO) { /* Error */ }

            // Generar Cuádruplo de ASIGNACIÓN
            CuadruplosGenerados.Add(new Cuadruplo(opAsig.Tipo, resultadoExpr, "", id.Valor));
        }

        private void RevisarIf(NodoAST nodo)
        {
            // IF_STMT -> PR18 CE6 COND_EXPR CE7 CE8 STMT_LIST CE9 (IF_STMT_PRIME)?
            if (nodo.Hijos.Count < 7) return;

            // 1. Generar Cuádruplos para la Condición (Hijos[2])
            string resultadoCondicion = RevisarExpresion(nodo.Hijos[2], out _);

            // 2. Generar etiquetas de control
            string etiquetaFalso = GenerarEtiqueta();
            string etiquetaSalida = GenerarEtiqueta();

            // 3. JUMP_FALSE
            CuadruplosGenerados.Add(new Cuadruplo("JUMP_FALSE", resultadoCondicion, "", etiquetaFalso));

            // 4. Cuerpo del IF (Hijos[5])
            RevisarNodo(nodo.Hijos[5]);

            // 5. GOTO Salida (Si hay sino, salta el bloque else)
            if (nodo.Hijos.Count > 7) CuadruplosGenerados.Add(new Cuadruplo("GOTO", "", "", etiquetaSalida));

            // 6. Etiqueta FALSO (Inicio del bloque 'sino')
            CuadruplosGenerados.Add(new Cuadruplo("LABEL", "", "", etiquetaFalso));

            // 7. Bloque ELSE (Hijos[7] es IF_STMT_PRIME)
            if (nodo.Hijos.Count > 7) RevisarNodo(nodo.Hijos[7]);

            // 8. Etiqueta de Salida final (solo si había bloque sino)
            if (nodo.Hijos.Count > 7) CuadruplosGenerados.Add(new Cuadruplo("LABEL", "", "", etiquetaSalida));
        }

        private void RevisarCiclo(NodoAST nodo)
        {
            tablaSimbolosStack.Push(new Dictionary<string, Simbolo>());

            var idNodo = nodo.Hijos[2];
            var exprInicial = nodo.Hijos[4];
            var exprCondicion = nodo.Hijos[6];
            var exprIncremento = nodo.Hijos[8];

            // 1. Inicialización (genera: OPAS, resultado, , idNodo.Valor)
            string resultadoInicial = RevisarExpresion(exprInicial, out _);
            CuadruplosGenerados.Add(new Cuadruplo("OPAS", resultadoInicial, "", idNodo.Valor));

            // 2. Etiquetas de control
            string etiquetaInicio = GenerarEtiqueta();
            string etiquetaSalida = GenerarEtiqueta();

            // Etiqueta de inicio del ciclo
            CuadruplosGenerados.Add(new Cuadruplo("LABEL", "", "", etiquetaInicio));

            // 3. Condición (genera: T# = Expr)
            string resultadoCondicion = RevisarExpresion(exprCondicion, out _);

            // Salto si la condición es falsa
            CuadruplosGenerados.Add(new Cuadruplo("JUMP_FALSE", resultadoCondicion, "", etiquetaSalida));

            // 4. Cuerpo del ciclo (Hijos[10])
            RevisarNodo(nodo.Hijos[10]);

            // 5. Incremento (genera: T# = Expr)
            RevisarExpresion(exprIncremento, out _);

            // 6. Salto incondicional al inicio del ciclo
            CuadruplosGenerados.Add(new Cuadruplo("GOTO", "", "", etiquetaInicio));

            // 7. Etiqueta de salida del ciclo
            CuadruplosGenerados.Add(new Cuadruplo("LABEL", "", "", etiquetaSalida));

            tablaSimbolosStack.Pop();
        }

        private void RevisarPrint(NodoAST nodo)
        {
            if (nodo.Hijos.Count > 1)
            {
                string resultadoExpr = RevisarExpresion(nodo.Hijos[1], out _);
                CuadruplosGenerados.Add(new Cuadruplo(nodo.Hijos[0].Tipo, resultadoExpr, "", ""));
            }
        }

        private string RevisarExpresion(NodoAST nodo, out TipoDato tipoResultante)
        {
            tipoResultante = TipoDato.DESCONOCIDO;
            if (nodo == null) return "null";

            // Lógica de terminales y unarios (simplificado)
            switch (nodo.Tipo)
            {
                case "CN_ENTERA": tipoResultante = TipoDato.ENTERO; return nodo.Valor;
                case "CN_DECIMAL": tipoResultante = TipoDato.ENTERO; return nodo.Valor;
                case "CADE": tipoResultante = TipoDato.CADENA; return nodo.Valor;
                case "PR20": tipoResultante = TipoDato.BOOLEANO; return nodo.Valor;
                case "PR06": tipoResultante = TipoDato.BOOLEANO; return nodo.Valor;

                case "IDEN":
                    if (BuscarSimbolo(nodo.Valor, out var simbolo)) { tipoResultante = simbolo.Tipo; return nodo.Valor; }
                    else { Errores.Add(new ErrorSemantico($"Variable '{nodo.Valor}' usada sin declarar.", nodo.Linea)); return "ERROR"; }
                case "PR16":
                    if (nodo.Hijos.Count > 0)
                    {
                        string arg1 = RevisarExpresion(nodo.Hijos[0], out TipoDato exprTipo);
                        if (exprTipo != TipoDato.BOOLEANO) { Errores.Add(new ErrorSemantico("Operador de negación requiere un booleano.", nodo.Linea)); return "ERROR"; }
                        string resultadoTemp = GenerarTemporal();
                        CuadruplosGenerados.Add(new Cuadruplo(nodo.Tipo, arg1, "", resultadoTemp));
                        tipoResultante = TipoDato.BOOLEANO;
                        return resultadoTemp;
                    }
                    return "ERROR";
            }

            // Lógica para expresiones binarias
            if (nodo.Hijos.Any())
            {
                if (nodo.Hijos.Count > 1 && nodo.Hijos[1].Tipo.StartsWith("OP"))
                {
                    string arg1 = RevisarExpresion(nodo.Hijos[0], out TipoDato izqTipo);
                    var op = nodo.Hijos[1];
                    string arg2 = RevisarExpresion(nodo.Hijos[2], out TipoDato derTipo);

                    // Verificación de Tipos (Semántica)
                    if (op.Tipo.StartsWith("OPRE"))
                    {
                        if (izqTipo == TipoDato.ENTERO && derTipo == TipoDato.ENTERO) tipoResultante = TipoDato.ENTERO;
                        else if (izqTipo == TipoDato.CADENA && derTipo == TipoDato.CADENA && op.Tipo == "OPRE1") tipoResultante = TipoDato.CADENA;
                        else Errores.Add(new ErrorSemantico("Operación aritmética con tipos incompatibles.", nodo.Linea));
                    }
                    else if (op.Tipo.StartsWith("OPREL"))
                    {
                        if (izqTipo == derTipo) tipoResultante = TipoDato.BOOLEANO;
                        else Errores.Add(new ErrorSemantico("Comparación entre tipos incompatibles.", nodo.Linea));
                    }
                    else if (op.Tipo.StartsWith("OPLO"))
                    {
                        if (izqTipo == TipoDato.BOOLEANO && derTipo == TipoDato.BOOLEANO) tipoResultante = TipoDato.BOOLEANO;
                        else Errores.Add(new ErrorSemantico("Operación lógica con tipos no booleanos.", nodo.Linea));
                    }

                    if (tipoResultante == TipoDato.DESCONOCIDO) return "ERROR";

                    string temporal = GenerarTemporal();
                    CuadruplosGenerados.Add(new Cuadruplo(op.Tipo, arg1, arg2, temporal));
                    return temporal;
                }

                return RevisarExpresion(nodo.Hijos[0], out tipoResultante);
            }

            return "null";
        }

        // MÉTODO para imprimir Triplos
        public string ImprimirTriplos()
        {
            var sb = new StringBuilder();
            sb.AppendLine("--- Código Intermedio (Triplos) ---");

            for (int i = 0; i < CuadruplosGenerados.Count; i++)
            {
                var c = CuadruplosGenerados[i];

                string arg1 = c.Arg1.StartsWith("T") ? $"({CuadruplosGenerados.FindIndex(x => x.Resultado == c.Arg1) + 1})" : c.Arg1;
                string arg2 = c.Arg2.StartsWith("T") ? $"({CuadruplosGenerados.FindIndex(x => x.Resultado == c.Arg2) + 1})" : c.Arg2;

                if (arg1 == "0" && c.Arg1.StartsWith("T")) arg1 = "ERROR";
                if (arg2 == "0" && c.Arg2.StartsWith("T")) arg2 = "ERROR";

                sb.AppendLine($"{i + 1}: ({c.Operador}, {arg1}, {arg2})");
            }
            return sb.ToString();
        }
    }
}
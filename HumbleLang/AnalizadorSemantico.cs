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

        // MIEMBROS PARA CÓDIGO INTERMEDIO
        public List<Cuadruplo> CuadruplosGenerados { get; private set; } = new List<Cuadruplo>();
        private int contadorTemporal = 1;
        private int contadorEtiqueta = 1;

        private string GenerarTemporal() => $"T{contadorTemporal++}";
        private string GenerarEtiqueta() => $"L{contadorEtiqueta++}";

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
                case "READ_STMT": RevisarRead(nodo); break;
                case "CICLO": RevisarCiclo(nodo); break;
                case "LIMPIAR":
                    CuadruplosGenerados.Add(new Cuadruplo("PR14", "", "", ""));
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

            if (BuscarSimboloEnAmbitoActual(idNodo.Valor))
            {
                Errores.Add(new ErrorSemantico($"La variable '{idNodo.Valor}' ya ha sido declarada en este ámbito.", idNodo.Linea));
                return;
            }

            TipoDato tipoDeclarado = TipoDato.DESCONOCIDO;
            switch (tipoNodo.Tipo)
            {
                case "PR05": tipoDeclarado = TipoDato.ENTERO; break;
                case "PR01": tipoDeclarado = TipoDato.CADENA; break;
                case "PR15": tipoDeclarado = TipoDato.BOOLEANO; break;
                case "PR03": tipoDeclarado = TipoDato.ENTERO; break;
            }
            if (tipoDeclarado != TipoDato.DESCONOCIDO)
                tablaSimbolosStack.Peek().Add(idNodo.Valor, new Simbolo(idNodo.Valor, tipoDeclarado, idNodo.Linea));

            if (decPrimeNodo.Hijos.Count > 1)
            {
                var exprNodo = decPrimeNodo.Hijos[2];
                string resultadoExpr = RevisarExpresion(exprNodo, out TipoDato tipoExpr);

                bool tiposIncompatibles = false;
                if (tipoDeclarado == TipoDato.ENTERO && tipoExpr != TipoDato.ENTERO) tiposIncompatibles = true;
                else if (tipoDeclarado == TipoDato.CADENA && tipoExpr != TipoDato.CADENA) tiposIncompatibles = true;
                else if (tipoDeclarado == TipoDato.BOOLEANO && tipoExpr != TipoDato.BOOLEANO) tiposIncompatibles = true;

                if (tiposIncompatibles && tipoExpr != TipoDato.DESCONOCIDO)
                {
                    Errores.Add(new ErrorSemantico(
                        $"Tipos incompatibles: No puedes asignar '{tipoExpr}' a una variable declarada como '{tipoDeclarado}'.",
                        idNodo.Linea
                    ));
                }

                CuadruplosGenerados.Add(new Cuadruplo("OPAS", resultadoExpr, "", idNodo.Valor));
            }
        }

        private void RevisarAsignacion(NodoAST nodo)
        {
            if (nodo.Hijos.Count < 3) return;
            var id = nodo.Hijos[0];
            var opAsig = nodo.Hijos[1];
            var expr = nodo.Hijos[2];

            if (!BuscarSimbolo(id.Valor, out var simbolo))
            {
                Errores.Add(new ErrorSemantico($"La variable '{id.Valor}' no ha sido declarada.", id.Linea));
                return;
            }

            string resultadoExpr = RevisarExpresion(expr, out TipoDato tipoExprResultante);

            bool tiposIncompatibles = false;
            if (simbolo.Tipo == TipoDato.ENTERO && tipoExprResultante != TipoDato.ENTERO) tiposIncompatibles = true;
            else if (simbolo.Tipo == TipoDato.CADENA && tipoExprResultante != TipoDato.CADENA) tiposIncompatibles = true;
            else if (simbolo.Tipo == TipoDato.BOOLEANO && tipoExprResultante != TipoDato.BOOLEANO) tiposIncompatibles = true;

            if (tiposIncompatibles && tipoExprResultante != TipoDato.DESCONOCIDO)
            {
                Errores.Add(new ErrorSemantico(
                    $"Tipos incompatibles: La variable '{id.Valor}' es '{simbolo.Tipo}' y no acepta '{tipoExprResultante}'.",
                    id.Linea
                ));
            }

            CuadruplosGenerados.Add(new Cuadruplo(opAsig.Tipo, resultadoExpr, "", id.Valor));
        }

        private void RevisarRead(NodoAST nodo)
        {
            if (nodo.Hijos.Count < 2) return;
            var idNodo = nodo.Hijos[1];

            if (!BuscarSimbolo(idNodo.Valor, out var simbolo))
            {
                Errores.Add(new ErrorSemantico($"Intento de leer en variable no declarada '{idNodo.Valor}'.", idNodo.Linea));
                return;
            }
            CuadruplosGenerados.Add(new Cuadruplo("READ", "", "", idNodo.Valor));
        }

        private void RevisarCiclo(NodoAST nodo)
        {
            tablaSimbolosStack.Push(new Dictionary<string, Simbolo>());

            var idNodo = nodo.Hijos[2];
            var exprInicial = nodo.Hijos[4];
            var exprCondicion = nodo.Hijos[6];
            var exprIncremento = nodo.Hijos[8];

            if (!BuscarSimboloEnAmbitoActual(idNodo.Valor))
            {
                tablaSimbolosStack.Peek().Add(idNodo.Valor, new Simbolo(idNodo.Valor, TipoDato.ENTERO, idNodo.Linea));
            }

            string resultadoInicial = RevisarExpresion(exprInicial, out _);
            CuadruplosGenerados.Add(new Cuadruplo("OPAS", resultadoInicial, "", idNodo.Valor));

            string etiquetaInicio = GenerarEtiqueta();
            string etiquetaSalida = GenerarEtiqueta();

            CuadruplosGenerados.Add(new Cuadruplo("LABEL", "", "", etiquetaInicio));

            string resultadoCondicion = RevisarExpresion(exprCondicion, out _);
            CuadruplosGenerados.Add(new Cuadruplo("JUMP_FALSE", resultadoCondicion, "", etiquetaSalida));

            RevisarNodo(nodo.Hijos[10]);

            RevisarExpresion(exprIncremento, out _);
            CuadruplosGenerados.Add(new Cuadruplo("GOTO", "", "", etiquetaInicio));
            CuadruplosGenerados.Add(new Cuadruplo("LABEL", "", "", etiquetaSalida));

            tablaSimbolosStack.Pop();
        }

        private void RevisarIf(NodoAST nodo)
        {
            if (nodo.Hijos.Count < 7) return;
            string resultadoCondicion = RevisarExpresion(nodo.Hijos[2], out _);
            string etiquetaFalso = GenerarEtiqueta();
            string etiquetaSalida = GenerarEtiqueta();

            CuadruplosGenerados.Add(new Cuadruplo("JUMP_FALSE", resultadoCondicion, "", etiquetaFalso));
            RevisarNodo(nodo.Hijos[5]);
            if (nodo.Hijos.Count > 7) CuadruplosGenerados.Add(new Cuadruplo("GOTO", "", "", etiquetaSalida));
            CuadruplosGenerados.Add(new Cuadruplo("LABEL", "", "", etiquetaFalso));
            if (nodo.Hijos.Count > 7) RevisarNodo(nodo.Hijos[7]);
            if (nodo.Hijos.Count > 7) CuadruplosGenerados.Add(new Cuadruplo("LABEL", "", "", etiquetaSalida));
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

            // 1. Caso Base: Valores Primitivos (Hojas)
            if (nodo.Hijos.Count == 0)
            {
                switch (nodo.Tipo)
                {
                    case "CN_ENTERA": tipoResultante = TipoDato.ENTERO; return nodo.Valor;
                    case "CN_REALES": tipoResultante = TipoDato.ENTERO; return nodo.Valor;
                    case "CADE": tipoResultante = TipoDato.CADENA; return nodo.Valor;
                    case "PR20": tipoResultante = TipoDato.BOOLEANO; return nodo.Valor;
                    case "PR06": tipoResultante = TipoDato.BOOLEANO; return nodo.Valor;
                    case "IDEN":
                        if (BuscarSimbolo(nodo.Valor, out var simbolo)) { tipoResultante = simbolo.Tipo; return nodo.Valor; }
                        else { Errores.Add(new ErrorSemantico($"Variable '{nodo.Valor}' usada sin declarar.", nodo.Linea)); return "ERROR"; }
                    case "PR16": return "ERROR";
                }
            }

            // 2. Caso Unario (Negación)
            if (nodo.Tipo == "PR16" || (nodo.Hijos.Count == 1 && nodo.Hijos[0].Tipo == "PR16"))
            {
                NodoAST hijo = nodo.Hijos.Count > 0 ? nodo.Hijos[0] : nodo;
                if (nodo.Hijos.Count > 0)
                {
                    string arg1 = RevisarExpresion(nodo.Hijos[0], out TipoDato exprTipo);
                    if (exprTipo != TipoDato.BOOLEANO && exprTipo != TipoDato.DESCONOCIDO)
                        Errores.Add(new ErrorSemantico("Operador de negación requiere un booleano.", nodo.Linea));
                    string resTemp = GenerarTemporal();
                    CuadruplosGenerados.Add(new Cuadruplo("PR16", arg1, "", resTemp));
                    tipoResultante = TipoDato.BOOLEANO;
                    return resTemp;
                }
            }

            // 3. Evaluar lado Izquierdo (Recursión) - Estrategia para árbol "escalera" (Term + Prime)
            string valIzq = "";
            TipoDato tipoIzq = TipoDato.DESCONOCIDO;

            // Si tiene estructura plana binaria (Arg1 Op Arg2)
            if (nodo.Hijos.Count > 2 && !nodo.Hijos[1].Tipo.EndsWith("PRIME"))
            {
                // Lógica antigua para árbol plano
                valIzq = RevisarExpresion(nodo.Hijos[0], out tipoIzq);
            }
            else if (nodo.Hijos.Count > 0)
            {
                // Lógica para árbol escalera: evaluamos hijo 0 primero
                valIzq = RevisarExpresion(nodo.Hijos[0], out tipoIzq);
                tipoResultante = tipoIzq;
            }

            // 4. Revisar si hay un nodo PRIME a la derecha (Operación)
            // Estructura común LL1: EXPR -> TERM EXPR_PRIME
            if (nodo.Hijos.Count > 1)
            {
                NodoAST nodoPrime = nodo.Hijos[1];
                // Si es un nodo PRIME y tiene hijos, ahí está la operación
                if (nodoPrime.Hijos.Count >= 2)
                {
                    var opNodo = nodoPrime.Hijos[0];    // El operador
                    var rightNode = nodoPrime.Hijos[1]; // El operando derecho

                    // Evaluar lado Derecho
                    string valDer = RevisarExpresion(rightNode, out TipoDato tipoDer);

                    // --- MAPEO DE TOKENS A SÍMBOLOS REALES ---
                    string opToken = opNodo.Tipo;
                    string simbolo = opNodo.Valor; // Por defecto

                    if (opToken == "OPRE1") simbolo = "<";
                    else if (opToken == "OPRE2") simbolo = ">";
                    else if (opToken == "OPRE3") simbolo = "<=";
                    else if (opToken == "OPRE4") simbolo = ">=";
                    else if (opToken == "OPRE5") simbolo = "==";
                    else if (opToken == "OPRE6") simbolo = "!=";
                    else if (opToken == "OPAR1") simbolo = "+";
                    else if (opToken == "OPAR2") simbolo = "-";
                    else if (opToken == "OPAR3") simbolo = "*";
                    else if (opToken == "OPAR4") simbolo = "/";
                    else if (opToken == "OPLO1") simbolo = "&&";
                    else if (opToken == "OPLO2") simbolo = "||";

                    // --- VALIDACIÓN DE TIPOS ---
                    bool error = false;

                    // A. ARITMÉTICA
                    if (simbolo == "+" || simbolo == "-" || simbolo == "*" || simbolo == "/" || simbolo == "^")
                    {
                        if (tipoIzq == TipoDato.ENTERO && tipoDer == TipoDato.ENTERO) tipoResultante = TipoDato.ENTERO;
                        else if (simbolo == "+" && tipoIzq == TipoDato.CADENA && tipoDer == TipoDato.CADENA) tipoResultante = TipoDato.CADENA;
                        else error = true;
                    }
                    // B. RELACIONAL
                    else if (simbolo == ">" || simbolo == "<" || simbolo == ">=" || simbolo == "<=")
                    {
                        if (tipoIzq == TipoDato.ENTERO && tipoDer == TipoDato.ENTERO) tipoResultante = TipoDato.BOOLEANO;
                        else error = true;
                    }
                    // C. IGUALDAD
                    else if (simbolo == "==" || simbolo == "!=")
                    {
                        if (tipoIzq == tipoDer) tipoResultante = TipoDato.BOOLEANO;
                        else error = true;
                    }
                    // D. LÓGICA
                    else if (simbolo == "&&" || simbolo == "||")
                    {
                        if (tipoIzq == TipoDato.BOOLEANO && tipoDer == TipoDato.BOOLEANO) tipoResultante = TipoDato.BOOLEANO;
                        else error = true;
                    }

                    if (error && tipoIzq != TipoDato.DESCONOCIDO && tipoDer != TipoDato.DESCONOCIDO)
                    {
                        Errores.Add(new ErrorSemantico($"Operación '{simbolo}' inválida entre {tipoIzq} y {tipoDer}.", nodo.Linea));
                    }

                    string temp = GenerarTemporal();
                    CuadruplosGenerados.Add(new Cuadruplo(simbolo, valIzq, valDer, temp));
                    return temp;
                }
            }

            // Lógica de respaldo para árboles planos (Arg1 Op Arg2)
            if (nodo.Hijos.Count > 2 && !nodo.Hijos[1].Tipo.EndsWith("PRIME"))
            {
                var opNodo = nodo.Hijos[1];
                string valDer = RevisarExpresion(nodo.Hijos[2], out TipoDato tipoDer);
                // ... (repite lógica de mapeo y validación si es necesario, 
                // pero con tu parser LL1 es probable que siempre caiga en el bloque PRIME o Base)

                // Para simplificar, asumimos que tu parser LL1 usa la estructura PRIME.
                // Si usas estructura plana en algunos casos, el código de arriba (bloque 3)
                // necesita integrarse aquí también.
            }

            // Caso paréntesis: ( EXPR )
            if (nodo.Hijos.Count == 3 && nodo.Hijos[0].Tipo == "CE6")
                return RevisarExpresion(nodo.Hijos[1], out tipoResultante);

            return valIzq; // Retorna el valor izquierdo si no hubo operaciones
        }

        public string ImprimirTriplos()
        {
            var sb = new StringBuilder();
            sb.AppendLine("--- Código Intermedio (Triplos) ---");
            for (int i = 0; i < CuadruplosGenerados.Count; i++)
            {
                var c = CuadruplosGenerados[i];
                string arg1 = c.Arg1.StartsWith("T") ? $"({CuadruplosGenerados.FindIndex(x => x.Resultado == c.Arg1) + 1})" : c.Arg1;
                string arg2 = c.Arg2.StartsWith("T") ? $"({CuadruplosGenerados.FindIndex(x => x.Resultado == c.Arg2) + 1})" : c.Arg2;
                sb.AppendLine($"{i + 1}: ({c.Operador}, {arg1}, {arg2})");
            }
            return sb.ToString();
        }
    }
}
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows.Forms; // Necesario para MessageBox si ocurre error fatal

namespace HumbleLang
{
    // =========================================================
    // PARTE 1: TABLA DE SÍMBOLOS (MEMORIA)
    // =========================================================
    public class TablaSimbolos
    {
        private Dictionary<string, object> variables = new Dictionary<string, object>();
        private Dictionary<string, string> tipos = new Dictionary<string, string>();

        public void Declarar(string nombre, string tipo, object valor)
        {
            if (variables.ContainsKey(nombre))
            {
                throw new Exception($"Error Semántico: La variable '{nombre}' ya existe.");
            }
            variables[nombre] = valor;
            tipos[nombre] = tipo;
        }

        public void Asignar(string nombre, object valor)
        {
            if (!variables.ContainsKey(nombre))
            {
                throw new Exception($"Error Semántico: Variable '{nombre}' no declarada.");
            }
            // Aquí podrías validar que el tipo coincida (ej. no asignar texto a entero)
            variables[nombre] = valor;
        }

        public object Obtener(string nombre)
        {
            if (!variables.ContainsKey(nombre))
            {
                throw new Exception($"Error Semántico: Variable '{nombre}' no encontrada.");
            }
            return variables[nombre];
        }

        public bool Existe(string nombre) => variables.ContainsKey(nombre);

        // Limpia la memoria al reiniciar
        public void Limpiar()
        {
            variables.Clear();
            tipos.Clear();
        }
    }

    // =========================================================
    // PARTE 2: INTÉRPRETE (CEREBRO)
    // =========================================================
    public class Interprete
    {
        private TablaSimbolos tabla;
        private Action<string> outputCallback; // Función para enviar texto a la ventana

        public Interprete()
        {
            tabla = new TablaSimbolos();
        }

        /// <summary>
        /// Inicia la ejecución del árbol AST.
        /// </summary>
        /// <param name="nodo">Nodo raíz (PROG)</param>
        /// <param name="outputMethod">Función que recibe el texto de los 'imp'</param>
        public void Interpretar(NodoAST nodo, Action<string> outputMethod)
        {
            this.outputCallback = outputMethod;
            this.tabla.Limpiar(); // Reiniciar variables en cada ejecución

            if (nodo == null) return;

            try
            {
                Ejecutar(nodo);
            }
            catch (Exception ex)
            {
                outputCallback($"\n[ERROR DE EJECUCIÓN]: {ex.Message}");
            }
        }

        private void Ejecutar(NodoAST nodo)
        {
            if (nodo == null) return;

            switch (nodo.Tipo)
            {
                // Nodos contenedores: simplemente recorrer hijos
                case "PROG":
                case "STMT_LIST":
                case "STMT":
                    foreach (var hijo in nodo.Hijos)
                    {
                        Ejecutar(hijo);
                    }
                    break;

                case "DECLARACION":
                    ProcesarDeclaracion(nodo);
                    break;

                case "ASIGNACION":
                    ProcesarAsignacion(nodo);
                    break;

                case "PRINT_STMT":
                    // Estructura: PRINT -> "imp" (0) -> EXPR (1)
                    if (nodo.Hijos.Count > 1)
                    {
                        var val = Evaluar(nodo.Hijos[1]);
                        outputCallback(val?.ToString() ?? "nul");
                    }
                    break;

                case "LIMPIAR":
                    outputCallback("--- LIMPIAR PANTALLA ---");
                    // Nota: En WinForms no podemos borrar el MessageBox, 
                    // así que enviamos una señal o texto informativo.
                    break;

                case "IF_STMT":
                    ProcesarIf(nodo);
                    break;

                case "CICLO":
                    ProcesarCiclo(nodo);
                    break;

                default:
                    // Si es un nodo desconocido, intentamos ejecutar sus hijos por si acaso
                    foreach (var hijo in nodo.Hijos) Ejecutar(hijo);
                    break;
            }
        }

        private void ProcesarDeclaracion(NodoAST nodo)
        {
            // Estructura: TIPO (0) -> DEC_PRIME (1)
            // DEC_PRIME tiene: IDEN (0) -> [OPAS (1) -> EXPR (2)] (Opcional)

            string tipoDato = nodo.Hijos[0].Valor; // ent, dec, etc.
            var decPrime = nodo.Hijos[1];
            string nombreVar = decPrime.Hijos[0].Valor;

            object valorInicial = null;

            // Valores por defecto
            if (tipoDato == "ent") valorInicial = 0;
            else if (tipoDato == "dec") valorInicial = 0.0;
            else if (tipoDato == "log") valorInicial = false; // vdd/fal
            else valorInicial = "";

            // Si hay asignación explícita (ej: ent x = 10)
            if (decPrime.Hijos.Count > 2)
            {
                valorInicial = Evaluar(decPrime.Hijos[2]);
            }

            // Casteo forzoso inicial
            if (tipoDato == "ent") valorInicial = Convert.ToInt32(valorInicial);
            else if (tipoDato == "dec") valorInicial = Convert.ToDouble(valorInicial, CultureInfo.InvariantCulture);

            tabla.Declarar(nombreVar, tipoDato, valorInicial);
        }

        private void ProcesarAsignacion(NodoAST nodo)
        {
            // IDEN (0) -> OPAS (1) -> EXPR (2)
            string nombre = nodo.Hijos[0].Valor;
            object val = Evaluar(nodo.Hijos[2]);
            tabla.Asignar(nombre, val);
        }

        private void ProcesarIf(NodoAST nodo)
        {
            // IF -> si (0) -> ( (1) -> COND (2) -> ) (3) -> { (4) -> STMT_LIST (5) -> } (6) -> ELSE? (7)
            object condVal = Evaluar(nodo.Hijos[2]);
            bool condicion = Convert.ToBoolean(condVal);

            if (condicion)
            {
                Ejecutar(nodo.Hijos[5]); // Ejecutar bloque Verdadero
            }
            else
            {
                // Verificar si existe bloque Sino (IF_STMT_PRIME)
                if (nodo.Hijos.Count > 7 && nodo.Hijos[7].Tipo == "IF_STMT_PRIME")
                {
                    var nodoElse = nodo.Hijos[7];
                    // IF_STMT_PRIME -> sino (0) -> { (1) -> STMT_LIST (2) -> } (3)
                    if (nodoElse.Hijos.Count > 2)
                    {
                        Ejecutar(nodoElse.Hijos[2]);
                    }
                }
            }
        }

        private void ProcesarCiclo(NodoAST nodo)
        {
            // CICLO -> cic(0) -> des(1) -> IDEN(2) -> =(3) -> INICIO(4) -> has(5) -> FIN(6) -> inc(7) -> PASO(8) -> {(9) -> LISTA(10) -> }(11)

            string idVar = nodo.Hijos[2].Valor;

            // 1. Inicializar variable del ciclo
            object valInicio = Evaluar(nodo.Hijos[4]);

            if (tabla.Existe(idVar)) tabla.Asignar(idVar, valInicio);
            else tabla.Declarar(idVar, "ent", valInicio);

            while (true)
            {
                // 2. Evaluar Condición de parada
                double actual = Convert.ToDouble(tabla.Obtener(idVar));
                double limite = Convert.ToDouble(Evaluar(nodo.Hijos[6]));
                double paso = Convert.ToDouble(Evaluar(nodo.Hijos[8]));

                // HumbleLang ciclo es inclusivo (<= limite) o exclusivo? 
                // Asumiremos <= para "has" (hasta)
                if (actual > limite) break;

                // 3. Ejecutar Cuerpo
                Ejecutar(nodo.Hijos[10]);

                // 4. Incrementar
                // Releemos el valor por si cambió dentro del ciclo
                actual = Convert.ToDouble(tabla.Obtener(idVar));
                tabla.Asignar(idVar, actual + paso);
            }
        }

        // =========================================================
        // EVALUADOR DE EXPRESIONES (MATEMÁTICAS Y LÓGICAS)
        // =========================================================
        private object Evaluar(NodoAST nodo)
        {
            if (nodo == null) return null;

            // 1. CASOS BASE (HOJAS DEL ÁRBOL)
            if (nodo.Hijos.Count == 0)
            {
                switch (nodo.Token) // Usamos la propiedad .Token que agregamos al NodoAST
                {
                    case "CN_ENTERA": return int.Parse(nodo.Valor);
                    case "CN_REALES": return double.Parse(nodo.Valor, CultureInfo.InvariantCulture);
                    case "CADE": return nodo.Valor.Replace("\"", ""); // Limpiar comillas
                    case "PR20": return true;  // vdd
                    case "PR06": return false; // fal
                    case "PR16": return null;  // nul
                    case "IDEN": return tabla.Obtener(nodo.Valor);
                    default: return null;
                }
            }

            // 2. EXCEPCIONES ESTRUCTURALES

            // Caso paréntesis: FACTOR -> ( EXPR )
            if (nodo.Tipo == "FACTOR" && nodo.Hijos[0].Token == "CE6") // CE6 es '('
            {
                return Evaluar(nodo.Hijos[1]);
            }

            // Caso negación: FACTOR -> !! FACTOR
            if (nodo.Tipo == "FACTOR" && nodo.Hijos[0].Token == "OPLO3") // OPLO3 es '!!'
            {
                return !Convert.ToBoolean(Evaluar(nodo.Hijos[1]));
            }

            // 3. RECORRIDO RECURSIVO (Izquierda -> Operador -> Derecha)
            // Dado que la gramática LL1 separa TERM de TERM_PRIME, evaluamos de izq a der.

            object valorIzquierdo = Evaluar(nodo.Hijos[0]);

            // Si hay más hijos (ej: ARITH_EXPR -> TERM ARITH_EXPR_PRIME),
            // pasamos el valor acumulado a la función que procesa la parte derecha ("PRIME").
            if (nodo.Hijos.Count > 1)
            {
                return EvaluarPrime(valorIzquierdo, nodo.Hijos[1]);
            }

            return valorIzquierdo;
        }

        private object EvaluarPrime(object acumulado, NodoAST nodoPrime)
        {
            // Si es epsilon (vacío), regresamos lo que llevamos
            if (nodoPrime.Hijos.Count == 0) return acumulado;

            // Estructura PRIME: OPERADOR (0) -> VALOR_SIGUIENTE (1) -> OTRO_PRIME (2)
            string operador = nodoPrime.Hijos[0].Valor; // Ej: +, -, *
            object valorDerecho = Evaluar(nodoPrime.Hijos[1]);

            // Realizar la operación matemática/lógica
            object nuevoAcumulado = Operar(operador, acumulado, valorDerecho);

            // Si hay más operaciones encadenadas (ej: 1 + 2 + 3), recursión
            if (nodoPrime.Hijos.Count > 2)
            {
                return EvaluarPrime(nuevoAcumulado, nodoPrime.Hijos[2]);
            }

            return nuevoAcumulado;
        }

        private object Operar(string op, object izq, object der)
        {
            // Intentar trabajar con doubles para simplificar
            double nIzq = 0, nDer = 0;
            bool sonNumeros = double.TryParse(izq.ToString(), out nIzq) &&
                              double.TryParse(der.ToString(), out nDer);

            switch (op)
            {
                // Aritmética
                case "+": return sonNumeros ? nIzq + nDer : izq.ToString() + der.ToString(); // Suma o Concatenación
                case "-": return nIzq - nDer;
                case "*": return nIzq * nDer;
                case "/":
                    if (nDer == 0) throw new DivideByZeroException("División por cero");
                    return nIzq / nDer;
                case "^": return Math.Pow(nIzq, nDer);

                // Relacional
                case ">": return nIzq > nDer;
                case "<": return nIzq < nDer;
                case ">=": return nIzq >= nDer;
                case "<=": return nIzq <= nDer;
                case "==": return izq.Equals(der);
                case "!=": return !izq.Equals(der);

                // Lógica
                case "&&": return Convert.ToBoolean(izq) && Convert.ToBoolean(der);
                case "||": return Convert.ToBoolean(izq) || Convert.ToBoolean(der);
            }
            return null;
        }
    }
}
using System;
using System.Collections.Generic;
using System.Globalization;

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

        // Método nuevo para saber el tipo de una variable (necesario para 'Lee')
        public string ObtenerTipoDato(string nombre)
        {
            if (tipos.ContainsKey(nombre)) return tipos[nombre];
            return "null";
        }

        public void Limpiar()
        {
            variables.Clear();
            tipos.Clear();
        }
    }

    // =========================================================
    // PARTE 2: INTÉRPRETE (MOTOR DE EJECUCIÓN)
    // =========================================================
    public class Interprete
    {
        private TablaSimbolos tabla;
        private Action<string> outputCallback;      // Para 'imp' (Salida)
        private Func<string, string> inputCallback; // Para 'Lee' (Entrada)

        public Interprete()
        {
            tabla = new TablaSimbolos();
        }

        /// <summary>
        /// Inicia la ejecución del árbol AST.
        /// </summary>
        /// <param name="nodo">Raíz del árbol</param>
        /// <param name="outputMethod">Función para imprimir texto</param>
        /// <param name="inputMethod">Función para pedir texto al usuario</param>
        public void Interpretar(NodoAST nodo, Action<string> outputMethod, Func<string, string> inputMethod)
        {
            this.outputCallback = outputMethod;
            this.inputCallback = inputMethod;
            this.tabla.Limpiar(); // Reiniciar memoria

            if (nodo == null) return;

            try
            {
                Ejecutar(nodo);
            }
            catch (Exception ex)
            {
                outputCallback($"\n[ERROR EJECUCIÓN]: {ex.Message}");
            }
        }

        private void Ejecutar(NodoAST nodo)
        {
            if (nodo == null) return;

            switch (nodo.Tipo)
            {
                // Nodos contenedores
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

                case "PRINT_STMT": // imp
                    if (nodo.Hijos.Count > 1)
                    {
                        var val = Evaluar(nodo.Hijos[1]);
                        outputCallback(val?.ToString() ?? "nul");
                    }
                    break;

                case "READ_STMT": // Lee
                    // Estructura: PR13(Lee) -> IDEN(variable)
                    if (nodo.Hijos.Count > 1)
                    {
                        string nombreVar = nodo.Hijos[1].Valor;

                        // 1. Pedir dato al usuario
                        string valorIngresado = inputCallback($"Ingrese valor para '{nombreVar}':");

                        // 2. Convertir al tipo correcto
                        string tipoMeta = tabla.ObtenerTipoDato(nombreVar);
                        object valorConvertido = valorIngresado;

                        try
                        {
                            if (tipoMeta == "ent") valorConvertido = int.Parse(valorIngresado);
                            else if (tipoMeta == "dec") valorConvertido = double.Parse(valorIngresado, CultureInfo.InvariantCulture);
                            else if (tipoMeta == "Log") valorConvertido = (valorIngresado == "vdd" || valorIngresado == "true");
                        }
                        catch
                        {
                            throw new Exception($"Error al convertir '{valorIngresado}' a tipo {tipoMeta}");
                        }

                        // 3. Guardar en memoria
                        tabla.Asignar(nombreVar, valorConvertido);
                    }
                    break;

                case "LIMPIAR":
                    outputCallback("--- LIMPIAR PANTALLA ---");
                    break;

                case "IF_STMT":
                    ProcesarIf(nodo);
                    break;

                case "CICLO":
                    ProcesarCiclo(nodo);
                    break;

                default:
                    foreach (var hijo in nodo.Hijos) Ejecutar(hijo);
                    break;
            }
        }

        private void ProcesarDeclaracion(NodoAST nodo)
        {
            string tipoDato = nodo.Hijos[0].Valor;
            var decPrime = nodo.Hijos[1];
            string nombreVar = decPrime.Hijos[0].Valor;

            object valorInicial = null;

            if (tipoDato == "ent") valorInicial = 0;
            else if (tipoDato == "dec") valorInicial = 0.0;
            else if (tipoDato == "Log") valorInicial = false;
            else valorInicial = "";

            if (decPrime.Hijos.Count > 2)
            {
                valorInicial = Evaluar(decPrime.Hijos[2]);
            }

            if (tipoDato == "ent") valorInicial = Convert.ToInt32(valorInicial);
            else if (tipoDato == "dec") valorInicial = Convert.ToDouble(valorInicial, CultureInfo.InvariantCulture);

            tabla.Declarar(nombreVar, tipoDato, valorInicial);
        }

        private void ProcesarAsignacion(NodoAST nodo)
        {
            string nombre = nodo.Hijos[0].Valor;
            object val = Evaluar(nodo.Hijos[2]);
            tabla.Asignar(nombre, val);
        }

        private void ProcesarIf(NodoAST nodo)
        {
            object condVal = Evaluar(nodo.Hijos[2]);
            bool condicion = Convert.ToBoolean(condVal);

            if (condicion)
            {
                Ejecutar(nodo.Hijos[5]); // Bloque SI
            }
            else
            {
                if (nodo.Hijos.Count > 7 && nodo.Hijos[7].Tipo == "IF_STMT_PRIME")
                {
                    var nodoElse = nodo.Hijos[7];
                    if (nodoElse.Hijos.Count > 2)
                    {
                        Ejecutar(nodoElse.Hijos[2]); // Bloque SINO
                    }
                }
            }
        }

        private void ProcesarCiclo(NodoAST nodo)
        {
            string idVar = nodo.Hijos[2].Valor;
            object valInicio = Evaluar(nodo.Hijos[4]);

            if (tabla.Existe(idVar)) tabla.Asignar(idVar, valInicio);
            else tabla.Declarar(idVar, "ent", valInicio);

            while (true)
            {
                double actual = Convert.ToDouble(tabla.Obtener(idVar));
                double limite = Convert.ToDouble(Evaluar(nodo.Hijos[6]));
                double paso = Convert.ToDouble(Evaluar(nodo.Hijos[8]));

                if (actual > limite) break;

                Ejecutar(nodo.Hijos[10]); // Cuerpo

                actual = Convert.ToDouble(tabla.Obtener(idVar));
                tabla.Asignar(idVar, actual + paso);
            }
        }

        private object Evaluar(NodoAST nodo)
        {
            if (nodo == null) return null;

            if (nodo.Hijos.Count == 0)
            {
                switch (nodo.Token)
                {
                    case "CN_ENTERA": return int.Parse(nodo.Valor);
                    case "CN_REALES": return double.Parse(nodo.Valor, CultureInfo.InvariantCulture);
                    case "CADE": return nodo.Valor.Replace("\"", "");
                    case "PR20": return true;
                    case "PR06": return false;
                    case "PR16": return null;
                    case "IDEN": return tabla.Obtener(nodo.Valor);
                    default: return null;
                }
            }

            // Excepciones estructurales
            if (nodo.Tipo == "FACTOR" && nodo.Hijos[0].Token == "CE6") // ( )
            {
                return Evaluar(nodo.Hijos[1]);
            }
            if (nodo.Tipo == "FACTOR" && nodo.Hijos[0].Token == "OPLO3") // !!
            {
                return !Convert.ToBoolean(Evaluar(nodo.Hijos[1]));
            }

            object valorIzquierdo = Evaluar(nodo.Hijos[0]);

            if (nodo.Hijos.Count > 1)
            {
                return EvaluarPrime(valorIzquierdo, nodo.Hijos[1]);
            }

            return valorIzquierdo;
        }

        private object EvaluarPrime(object acumulado, NodoAST nodoPrime)
        {
            if (nodoPrime.Hijos.Count == 0) return acumulado;

            string operador = nodoPrime.Hijos[0].Valor;
            object valorDerecho = Evaluar(nodoPrime.Hijos[1]);

            object nuevoAcumulado = Operar(operador, acumulado, valorDerecho);

            if (nodoPrime.Hijos.Count > 2)
            {
                return EvaluarPrime(nuevoAcumulado, nodoPrime.Hijos[2]);
            }

            return nuevoAcumulado;
        }

        private object Operar(string op, object izq, object der)
        {
            double nIzq = 0, nDer = 0;
            bool sonNumeros = double.TryParse(izq.ToString(), out nIzq) &&
                              double.TryParse(der.ToString(), out nDer);

            switch (op)
            {
                case "+": return sonNumeros ? nIzq + nDer : izq.ToString() + der.ToString();
                case "-": return nIzq - nDer;
                case "*": return nIzq * nDer;
                case "/": if (nDer == 0) throw new DivideByZeroException(); return nIzq / nDer;
                case "^": return Math.Pow(nIzq, nDer);
                case ">": return nIzq > nDer;
                case "<": return nIzq < nDer;
                case ">=": return nIzq >= nDer;
                case "<=": return nIzq <= nDer;
                case "==": return izq.Equals(der);
                case "!=": return !izq.Equals(der);
                case "&&": return Convert.ToBoolean(izq) && Convert.ToBoolean(der);
                case "||": return Convert.ToBoolean(izq) || Convert.ToBoolean(der);
            }
            return null;
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;

namespace HumbleLang
{
    public class AnalizadorLL1
    {
        // CAMBIO CRÍTICO: Ahora almacenamos (Tipo, ValorReal, Linea)
        private (string Tipo, string Valor, int Linea)[] tokens;
        private int index;
        private string tokenActual;
        private int lineaActual;

        // La clave es (NoTerminal, Terminal) y el valor es la acción de producción.
        private Dictionary<(string, string), Func<NodoAST>> tablaLL1;

        public List<string> RegistroPasos { get; private set; }
        public List<(string Mensaje, int Linea)> ErroresSintacticos { get; private set; }

        public AnalizadorLL1()
        {
            InicializarTabla();
            RegistroPasos = new List<string>();
            ErroresSintacticos = new List<(string, int)>();
        }

        // CAMBIO CRÍTICO: El método Analizar ahora recibe la lista con los valores reales
        public NodoAST Analizar(List<(string Tipo, string Valor, int Linea)> listaTokens)
        {
            tokens = listaTokens.ToArray();
            index = 0;
            // tokenActual sigue basándose en el TIPO para tomar decisiones gramaticales
            tokenActual = tokens.Length > 0 ? tokens[0].Tipo : null;
            lineaActual = tokens.Length > 0 ? tokens[0].Linea : 0;

            RegistroPasos.Clear();
            ErroresSintacticos.Clear();
            RegistroPasos.Add($"Inicio análisis. Tokens: {string.Join(", ", tokens.Select(t => t.Tipo))}");

            NodoAST raiz = AnalizarNoTerminal("PROG");

            if (tokenActual != null)
            {
                string tokensRestantes = string.Join(", ", tokens.Skip(index).Select(t => t.Tipo));
                ErroresSintacticos.Add(($"Error: tokens restantes después del análisis: '{tokensRestantes}'.", lineaActual));
            }

            return raiz;
        }

        private void InicializarTabla()
        {
            // =================================================================================
            // TABLA DE ANÁLISIS SINTÁCTICO LL(1)
            // =================================================================================

            tablaLL1 = new Dictionary<(string, string), Func<NodoAST>>
            {
                // PROG -> 'PR12'(ini) 'CE8'({) STMT_LIST 'CE9'(}) 'PR07'(fin)
                { ("PROG", "PR12"), () => {
                    var nodo = new NodoAST("PROG");
                    nodo.AgregarHijo(Match("PR12")); // ini
                    nodo.AgregarHijo(Match("CE8"));  // {
                    nodo.AgregarHijo(AnalizarNoTerminal("STMT_LIST"));
                    nodo.AgregarHijo(Match("CE9"));  // }
                    nodo.AgregarHijo(Match("PR07")); // fin
                    return nodo;
                }},

                // STMT_LIST (Recursivo)
                { ("STMT_LIST", "PR18"), () => RecurseStmtList() },
                { ("STMT_LIST", "PR09"), () => RecurseStmtList() },
                { ("STMT_LIST", "PR10"), () => RecurseStmtList() },
                { ("STMT_LIST", "PR05"), () => RecurseStmtList() },
                { ("STMT_LIST", "PR03"), () => RecurseStmtList() },
                { ("STMT_LIST", "PR01"), () => RecurseStmtList() },
                { ("STMT_LIST", "PR15"), () => RecurseStmtList() },
                { ("STMT_LIST", "IDEN"), () => RecurseStmtList() },
                { ("STMT_LIST", "PR13"), () => RecurseStmtList() },
                { ("STMT_LIST", "PR14"), () => RecurseStmtList() },
                { ("STMT_LIST", "PR02"), () => RecurseStmtList() },
                // Expresiones
                { ("STMT_LIST", "OPLO3"), () => RecurseStmtList() },
                { ("STMT_LIST", "CN_ENTERA"), () => RecurseStmtList() },
                { ("STMT_LIST", "CN_REALES"), () => RecurseStmtList() },
                { ("STMT_LIST", "CADE"), () => RecurseStmtList() },
                { ("STMT_LIST", "PR20"), () => RecurseStmtList() },
                { ("STMT_LIST", "PR06"), () => RecurseStmtList() },
                { ("STMT_LIST", "PR16"), () => RecurseStmtList() },
                { ("STMT_LIST", "CE6"), () => RecurseStmtList() },
                
                // FOLLOW de STMT_LIST
                { ("STMT_LIST", "CE9"), () => new NodoAST("STMT_LIST") },
                { ("STMT_LIST", "PR07"), () => new NodoAST("STMT_LIST") }, 

                // STMT Dispatcher
                { ("STMT", "PR05"), () => AnalizarNoTerminal("DECLARACION") },
                { ("STMT", "PR03"), () => AnalizarNoTerminal("DECLARACION") },
                { ("STMT", "PR01"), () => AnalizarNoTerminal("DECLARACION") },
                { ("STMT", "PR15"), () => AnalizarNoTerminal("DECLARACION") },
                { ("STMT", "IDEN"), () => AnalizarNoTerminal("ASIGNACION") },
                { ("STMT", "PR18"), () => AnalizarNoTerminal("IF_STMT") },
                { ("STMT", "PR09"), () => AnalizarNoTerminal("PRINT_STMT") },
                { ("STMT", "PR10"), () => AnalizarNoTerminal("PRINT_STMT") },
                { ("STMT", "PR13"), () => AnalizarNoTerminal("READ_STMT") },
                { ("STMT", "PR14"), () => AnalizarNoTerminal("LIMPIAR") },
                { ("STMT", "PR02"), () => AnalizarNoTerminal("CICLO") },
                // Expresiones sueltas
                { ("STMT", "OPLO3"), () => AnalizarNoTerminal("EXPR") },
                { ("STMT", "CN_ENTERA"), () => AnalizarNoTerminal("EXPR") },
                { ("STMT", "CN_REALES"), () => AnalizarNoTerminal("EXPR") },
                { ("STMT", "CADE"), () => AnalizarNoTerminal("EXPR") },
                { ("STMT", "PR20"), () => AnalizarNoTerminal("EXPR") },
                { ("STMT", "PR06"), () => AnalizarNoTerminal("EXPR") },
                { ("STMT", "PR16"), () => AnalizarNoTerminal("EXPR") },
                { ("STMT", "CE6"), () => AnalizarNoTerminal("EXPR") },
                
                // DECLARACION
                { ("DECLARACION", "PR05"), () => DeclProd("PR05") },
                { ("DECLARACION", "PR03"), () => DeclProd("PR03") },
                { ("DECLARACION", "PR01"), () => DeclProd("PR01") },
                { ("DECLARACION", "PR15"), () => DeclProd("PR15") },
                
                // DEC_PRIME
                { ("DEC_PRIME", "IDEN"), () => {
                    var nodo = new NodoAST("DEC_PRIME");
                    nodo.AgregarHijo(Match("IDEN"));
                    if (tokenActual == "OPAS")
                    {
                        nodo.AgregarHijo(Match("OPAS"));
                        nodo.AgregarHijo(AnalizarNoTerminal("EXPR"));
                    }
                    return nodo;
                }},

                // ASIGNACION
                { ("ASIGNACION", "IDEN"), () => {
                    var nodo = new NodoAST("ASIGNACION");
                    nodo.AgregarHijo(Match("IDEN"));
                    nodo.AgregarHijo(Match("OPAS"));
                    nodo.AgregarHijo(AnalizarNoTerminal("EXPR"));
                    return nodo;
                }},
                
                // IF_STMT
                { ("IF_STMT", "PR18"), () => {
                    var nodo = new NodoAST("IF_STMT");
                    nodo.AgregarHijo(Match("PR18"));
                    nodo.AgregarHijo(Match("CE6"));
                    nodo.AgregarHijo(AnalizarNoTerminal("COND_EXPR"));
                    nodo.AgregarHijo(Match("CE7"));
                    nodo.AgregarHijo(Match("CE8"));
                    nodo.AgregarHijo(AnalizarNoTerminal("STMT_LIST"));
                    nodo.AgregarHijo(Match("CE9"));
                    if (tokenActual == "PR19")
                    {
                        nodo.AgregarHijo(AnalizarNoTerminal("IF_STMT_PRIME"));
                    }
                    return nodo;
                }},
                
                // IF_STMT_PRIME
                { ("IF_STMT_PRIME", "PR19"), () => {
                    var nodo = new NodoAST("IF_STMT_PRIME");
                    nodo.AgregarHijo(Match("PR19"));
                    nodo.AgregarHijo(Match("CE8"));
                    nodo.AgregarHijo(AnalizarNoTerminal("STMT_LIST"));
                    nodo.AgregarHijo(Match("CE9"));
                    return nodo;
                }},
                { ("IF_STMT_PRIME", "PR18"), () => new NodoAST("IF_STMT_PRIME") },
                { ("IF_STMT_PRIME", "PR09"), () => new NodoAST("IF_STMT_PRIME") },
                { ("IF_STMT_PRIME", "IDEN"), () => new NodoAST("IF_STMT_PRIME") },
                { ("IF_STMT_PRIME", "CE9"), () => new NodoAST("IF_STMT_PRIME") },
                { ("IF_STMT_PRIME", "PR07"), () => new NodoAST("IF_STMT_PRIME") },
                { ("IF_STMT_PRIME", null), () => new NodoAST("IF_STMT_PRIME") }, 

                // PRINT_STMT
                { ("PRINT_STMT", "PR09"), () => PrintProd("PR09") },
                { ("PRINT_STMT", "PR10"), () => PrintProd("PR10") },

                // READ_STMT
                { ("READ_STMT", "PR13"), () => {
                    var nodo = new NodoAST("READ_STMT");
                    nodo.AgregarHijo(Match("PR13"));
                    nodo.AgregarHijo(Match("IDEN"));
                    return nodo;
                }},

                // LIMPIAR
                { ("LIMPIAR", "PR14"), () => Match("PR14") },
                
                // CICLO
                { ("CICLO", "PR02"), () => {
                    var nodo = new NodoAST("CICLO");
                    nodo.AgregarHijo(Match("PR02"));
                    nodo.AgregarHijo(Match("PR04"));
                    nodo.AgregarHijo(Match("IDEN"));
                    nodo.AgregarHijo(Match("OPAS"));
                    nodo.AgregarHijo(AnalizarNoTerminal("EXPR"));
                    nodo.AgregarHijo(Match("PR08"));
                    nodo.AgregarHijo(AnalizarNoTerminal("EXPR"));
                    nodo.AgregarHijo(Match("PR11"));
                    nodo.AgregarHijo(AnalizarNoTerminal("EXPR"));
                    nodo.AgregarHijo(Match("CE8"));
                    nodo.AgregarHijo(AnalizarNoTerminal("STMT_LIST"));
                    nodo.AgregarHijo(Match("CE9"));
                    return nodo;
                }},
                
                // EXPR Routing
                { ("COND_EXPR", "OPLO3"), () => AnalizarNoTerminal("LOG_EXPR") },
                { ("COND_EXPR", "IDEN"), () => AnalizarNoTerminal("LOG_EXPR") },
                { ("COND_EXPR", "CN_ENTERA"), () => AnalizarNoTerminal("LOG_EXPR") },
                { ("COND_EXPR", "CN_REALES"), () => AnalizarNoTerminal("LOG_EXPR") },
                { ("COND_EXPR", "CADE"), () => AnalizarNoTerminal("LOG_EXPR") },
                { ("COND_EXPR", "PR20"), () => AnalizarNoTerminal("LOG_EXPR") },
                { ("COND_EXPR", "PR06"), () => AnalizarNoTerminal("LOG_EXPR") },
                { ("COND_EXPR", "PR16"), () => AnalizarNoTerminal("LOG_EXPR") },
                { ("COND_EXPR", "CE6"), () => AnalizarNoTerminal("LOG_EXPR") },

                { ("EXPR", "OPLO3"), () => AnalizarNoTerminal("LOG_EXPR") },
                { ("EXPR", "IDEN"), () => AnalizarNoTerminal("LOG_EXPR") },
                { ("EXPR", "CN_ENTERA"), () => AnalizarNoTerminal("LOG_EXPR") },
                { ("EXPR", "CN_REALES"), () => AnalizarNoTerminal("LOG_EXPR") },
                { ("EXPR", "CADE"), () => AnalizarNoTerminal("LOG_EXPR") },
                { ("EXPR", "PR20"), () => AnalizarNoTerminal("LOG_EXPR") },
                { ("EXPR", "PR06"), () => AnalizarNoTerminal("LOG_EXPR") },
                { ("EXPR", "PR16"), () => AnalizarNoTerminal("LOG_EXPR") },
                { ("EXPR", "CE6"), () => AnalizarNoTerminal("LOG_EXPR") },

                // LOG_EXPR
                { ("LOG_EXPR", "OPLO3"), () => LogExprProd() },
                { ("LOG_EXPR", "IDEN"), () => LogExprProd() },
                { ("LOG_EXPR", "CN_ENTERA"), () => LogExprProd() },
                { ("LOG_EXPR", "CN_REALES"), () => LogExprProd() },
                { ("LOG_EXPR", "CADE"), () => LogExprProd() },
                { ("LOG_EXPR", "PR20"), () => LogExprProd() },
                { ("LOG_EXPR", "PR06"), () => LogExprProd() },
                { ("LOG_EXPR", "PR16"), () => LogExprProd() },
                { ("LOG_EXPR", "CE6"), () => LogExprProd() },
                
                // LOG_EXPR_PRIME
                { ("LOG_EXPR_PRIME", "OPLO1"), () => LogExprPrimeProd("OPLO1") },
                { ("LOG_EXPR_PRIME", "OPLO2"), () => LogExprPrimeProd("OPLO2") },
                { ("LOG_EXPR_PRIME", "CE7"), () => new NodoAST("LOG_EXPR_PRIME") },
                { ("LOG_EXPR_PRIME", "CE9"), () => new NodoAST("LOG_EXPR_PRIME") },
                { ("LOG_EXPR_PRIME", "PR18"), () => new NodoAST("LOG_EXPR_PRIME") },
                { ("LOG_EXPR_PRIME", "PR08"), () => new NodoAST("LOG_EXPR_PRIME") },
                { ("LOG_EXPR_PRIME", "PR11"), () => new NodoAST("LOG_EXPR_PRIME") },
                { ("LOG_EXPR_PRIME", null), () => new NodoAST("LOG_EXPR_PRIME") },

                // REL_EXPR
                { ("REL_EXPR", "OPLO3"), () => RelExprProd() },
                { ("REL_EXPR", "IDEN"), () => RelExprProd() },
                { ("REL_EXPR", "CN_ENTERA"), () => RelExprProd() },
                { ("REL_EXPR", "CN_REALES"), () => RelExprProd() },
                { ("REL_EXPR", "CADE"), () => RelExprProd() },
                { ("REL_EXPR", "PR20"), () => RelExprProd() },
                { ("REL_EXPR", "PR06"), () => RelExprProd() },
                { ("REL_EXPR", "PR16"), () => RelExprProd() },
                { ("REL_EXPR", "CE6"), () => RelExprProd() },
                
                // REL_EXPR_PRIME
                { ("REL_EXPR_PRIME", "OPRE1"), () => RelExprPrimeProd("OPRE1") },
                { ("REL_EXPR_PRIME", "OPRE2"), () => RelExprPrimeProd("OPRE2") },
                { ("REL_EXPR_PRIME", "OPRE3"), () => RelExprPrimeProd("OPRE3") },
                { ("REL_EXPR_PRIME", "OPRE4"), () => RelExprPrimeProd("OPRE4") },
                { ("REL_EXPR_PRIME", "OPRE5"), () => RelExprPrimeProd("OPRE5") },
                { ("REL_EXPR_PRIME", "OPRE6"), () => RelExprPrimeProd("OPRE6") }, 
                // Follow sets
                { ("REL_EXPR_PRIME", "OPLO1"), () => new NodoAST("REL_EXPR_PRIME") },
                { ("REL_EXPR_PRIME", "OPLO2"), () => new NodoAST("REL_EXPR_PRIME") },
                { ("REL_EXPR_PRIME", "CE7"), () => new NodoAST("REL_EXPR_PRIME") },
                { ("REL_EXPR_PRIME", "CE9"), () => new NodoAST("REL_EXPR_PRIME") },
                { ("REL_EXPR_PRIME", "PR08"), () => new NodoAST("REL_EXPR_PRIME") },
                { ("REL_EXPR_PRIME", "PR11"), () => new NodoAST("REL_EXPR_PRIME") },
                { ("REL_EXPR_PRIME", null), () => new NodoAST("REL_EXPR_PRIME") },

                // ARITH_EXPR
                { ("ARITH_EXPR", "OPLO3"), () => ArithExprProd() },
                { ("ARITH_EXPR", "IDEN"), () => ArithExprProd() },
                { ("ARITH_EXPR", "CN_ENTERA"), () => ArithExprProd() },
                { ("ARITH_EXPR", "CN_REALES"), () => ArithExprProd() },
                { ("ARITH_EXPR", "CADE"), () => ArithExprProd() },
                { ("ARITH_EXPR", "PR20"), () => ArithExprProd() },
                { ("ARITH_EXPR", "PR06"), () => ArithExprProd() },
                { ("ARITH_EXPR", "PR16"), () => ArithExprProd() },
                { ("ARITH_EXPR", "CE6"), () => ArithExprProd() },

                // ARITH_EXPR_PRIME
                { ("ARITH_EXPR_PRIME", "OPAR1"), () => ArithExprPrimeProd("OPAR1") },
                { ("ARITH_EXPR_PRIME", "OPAR2"), () => ArithExprPrimeProd("OPAR2") },
                // Follow sets (incluyen OPRE y CE)
                { ("ARITH_EXPR_PRIME", "OPRE1"), () => new NodoAST("ARITH_EXPR_PRIME") },
                { ("ARITH_EXPR_PRIME", "OPRE2"), () => new NodoAST("ARITH_EXPR_PRIME") },
                { ("ARITH_EXPR_PRIME", "OPRE3"), () => new NodoAST("ARITH_EXPR_PRIME") },
                { ("ARITH_EXPR_PRIME", "OPRE4"), () => new NodoAST("ARITH_EXPR_PRIME") },
                { ("ARITH_EXPR_PRIME", "OPRE5"), () => new NodoAST("ARITH_EXPR_PRIME") },
                { ("ARITH_EXPR_PRIME", "OPRE6"), () => new NodoAST("ARITH_EXPR_PRIME") },
                { ("ARITH_EXPR_PRIME", "CE7"), () => new NodoAST("ARITH_EXPR_PRIME") },
                { ("ARITH_EXPR_PRIME", "CE9"), () => new NodoAST("ARITH_EXPR_PRIME") },
                { ("ARITH_EXPR_PRIME", "PR08"), () => new NodoAST("ARITH_EXPR_PRIME") },
                { ("ARITH_EXPR_PRIME", "PR11"), () => new NodoAST("ARITH_EXPR_PRIME") },
                { ("ARITH_EXPR_PRIME", null), () => new NodoAST("ARITH_EXPR_PRIME") },

                // TERM
                { ("TERM", "OPLO3"), () => TermProd() },
                { ("TERM", "IDEN"), () => TermProd() },
                { ("TERM", "CN_ENTERA"), () => TermProd() },
                { ("TERM", "CN_REALES"), () => TermProd() },
                { ("TERM", "CADE"), () => TermProd() },
                { ("TERM", "PR20"), () => TermProd() },
                { ("TERM", "PR06"), () => TermProd() },
                { ("TERM", "PR16"), () => TermProd() },
                { ("TERM", "CE6"), () => TermProd() },

                // TERM_PRIME
                { ("TERM_PRIME", "OPAR3"), () => TermPrimeProd("OPAR3") },
                { ("TERM_PRIME", "OPAR4"), () => TermPrimeProd("OPAR4") },
                { ("TERM_PRIME", "OPAR5"), () => TermPrimeProd("OPAR5") },
                // Follow sets
                { ("TERM_PRIME", "OPAR1"), () => new NodoAST("TERM_PRIME") },
                { ("TERM_PRIME", "OPAR2"), () => new NodoAST("TERM_PRIME") },
                { ("TERM_PRIME", "OPRE1"), () => new NodoAST("TERM_PRIME") },
                { ("TERM_PRIME", "OPRE2"), () => new NodoAST("TERM_PRIME") },
                { ("TERM_PRIME", "OPRE3"), () => new NodoAST("TERM_PRIME") },
                { ("TERM_PRIME", "OPRE4"), () => new NodoAST("TERM_PRIME") },
                { ("TERM_PRIME", "OPRE5"), () => new NodoAST("TERM_PRIME") },
                { ("TERM_PRIME", "OPRE6"), () => new NodoAST("TERM_PRIME") },
                { ("TERM_PRIME", "CE7"), () => new NodoAST("TERM_PRIME") },
                { ("TERM_PRIME", "CE9"), () => new NodoAST("TERM_PRIME") },
                { ("TERM_PRIME", null), () => new NodoAST("TERM_PRIME") },

                // FACTOR
                { ("FACTOR", "IDEN"), () => Match("IDEN") },
                { ("FACTOR", "CN_ENTERA"), () => AnalizarNoTerminal("LITERAL") },
                { ("FACTOR", "CN_REALES"), () => AnalizarNoTerminal("LITERAL") },
                { ("FACTOR", "CADE"), () => AnalizarNoTerminal("LITERAL") },
                { ("FACTOR", "PR20"), () => AnalizarNoTerminal("LITERAL") },
                { ("FACTOR", "PR06"), () => AnalizarNoTerminal("LITERAL") },
                { ("FACTOR", "PR16"), () => AnalizarNoTerminal("LITERAL") },
                { ("FACTOR", "OPLO3"), () => {
                    var nodo = new NodoAST("FACTOR");
                    nodo.AgregarHijo(Match("OPLO3"));
                    nodo.AgregarHijo(AnalizarNoTerminal("FACTOR"));
                    return nodo;
                }},
                { ("FACTOR", "CE6"), () => {
                    var nodo = new NodoAST("FACTOR");
                    nodo.AgregarHijo(Match("CE6"));
                    nodo.AgregarHijo(AnalizarNoTerminal("EXPR"));
                    nodo.AgregarHijo(Match("CE7"));
                    return nodo;
                }},

                // LITERAL
                { ("LITERAL", "CN_ENTERA"), () => Match("CN_ENTERA") },
                { ("LITERAL", "CN_REALES"), () => Match("CN_REALES") },
                { ("LITERAL", "CADE"), () => Match("CADE") },
                { ("LITERAL", "PR20"), () => Match("PR20") },
                { ("LITERAL", "PR06"), () => Match("PR06") },
                { ("LITERAL", "PR16"), () => Match("PR16") },
            };
        }

        // --- Funciones Helpers de Producción (Para evitar repetición en el constructor) ---

        private NodoAST RecurseStmtList()
        {
            var nodo = new NodoAST("STMT_LIST");
            nodo.AgregarHijo(AnalizarNoTerminal("STMT"));
            nodo.AgregarHijo(AnalizarNoTerminal("STMT_LIST"));
            return nodo;
        }

        private NodoAST DeclProd(string tokenTipo)
        {
            var nodo = new NodoAST("DECLARACION");
            nodo.AgregarHijo(Match(tokenTipo));
            nodo.AgregarHijo(AnalizarNoTerminal("DEC_PRIME"));
            return nodo;
        }

        private NodoAST PrintProd(string tokenPrint)
        {
            var nodo = new NodoAST("PRINT_STMT");
            nodo.AgregarHijo(Match(tokenPrint));
            nodo.AgregarHijo(AnalizarNoTerminal("EXPR"));
            return nodo;
        }

        private NodoAST LogExprProd()
        {
            var nodo = new NodoAST("LOG_EXPR");
            nodo.AgregarHijo(AnalizarNoTerminal("REL_EXPR"));
            nodo.AgregarHijo(AnalizarNoTerminal("LOG_EXPR_PRIME"));
            return nodo;
        }

        private NodoAST LogExprPrimeProd(string op)
        {
            var nodo = new NodoAST("LOG_EXPR_PRIME");
            nodo.AgregarHijo(Match(op));
            nodo.AgregarHijo(AnalizarNoTerminal("REL_EXPR"));
            nodo.AgregarHijo(AnalizarNoTerminal("LOG_EXPR_PRIME"));
            return nodo;
        }

        private NodoAST RelExprProd()
        {
            var nodo = new NodoAST("REL_EXPR");
            nodo.AgregarHijo(AnalizarNoTerminal("ARITH_EXPR"));
            nodo.AgregarHijo(AnalizarNoTerminal("REL_EXPR_PRIME"));
            return nodo;
        }

        private NodoAST RelExprPrimeProd(string op)
        {
            var nodo = new NodoAST("REL_EXPR_PRIME");
            nodo.AgregarHijo(Match(op));
            nodo.AgregarHijo(AnalizarNoTerminal("ARITH_EXPR"));
            return nodo;
        }

        private NodoAST ArithExprProd()
        {
            var nodo = new NodoAST("ARITH_EXPR");
            nodo.AgregarHijo(AnalizarNoTerminal("TERM"));
            nodo.AgregarHijo(AnalizarNoTerminal("ARITH_EXPR_PRIME"));
            return nodo;
        }

        private NodoAST ArithExprPrimeProd(string op)
        {
            var nodo = new NodoAST("ARITH_EXPR_PRIME");
            nodo.AgregarHijo(Match(op));
            nodo.AgregarHijo(AnalizarNoTerminal("TERM"));
            nodo.AgregarHijo(AnalizarNoTerminal("ARITH_EXPR_PRIME"));
            return nodo;
        }

        private NodoAST TermProd()
        {
            var nodo = new NodoAST("TERM");
            nodo.AgregarHijo(AnalizarNoTerminal("FACTOR"));
            nodo.AgregarHijo(AnalizarNoTerminal("TERM_PRIME"));
            return nodo;
        }

        private NodoAST TermPrimeProd(string op)
        {
            var nodo = new NodoAST("TERM_PRIME");
            nodo.AgregarHijo(Match(op));
            nodo.AgregarHijo(AnalizarNoTerminal("FACTOR"));
            nodo.AgregarHijo(AnalizarNoTerminal("TERM_PRIME"));
            return nodo;
        }

        // --- Métodos del motor de análisis ---

        private NodoAST AnalizarNoTerminal(string noTerminal)
        {
            RegistroPasos.Add($"[Análisis] No terminal: '{noTerminal}', Token actual: '{tokenActual ?? "EOF"}'");
            var nodo = new NodoAST(noTerminal);

            var clave = (noTerminal, tokenActual);

            // 1. Busqueda exacta
            if (!tablaLL1.TryGetValue(clave, out var accion))
            {
                // 2. Busqueda de producción Epsilon (null) si no hay exacta
                if (tokenActual != null && tablaLL1.ContainsKey((noTerminal, null)))
                {
                    clave = (noTerminal, null);
                    accion = tablaLL1[clave];
                }
                else
                {
                    // Error: Calcular tokens esperados para el mensaje
                    var tokensEsperados = tablaLL1.Keys
                        .Where(k => k.Item1 == noTerminal && k.Item2 != null)
                        .Select(k => k.Item2)
                        .Distinct()
                        .ToList();

                    string descripcionEsperados = tokensEsperados.Count > 0 ?
                        string.Join(", ", tokensEsperados) : "fin de sentencia";

                    string mensajeError = $"Error en línea {lineaActual}: se esperaba uno de [{descripcionEsperados}] para '{noTerminal}', pero se encontró '{tokenActual ?? "EOF"}'";
                    ErroresSintacticos.Add((mensajeError, lineaActual));
                    RegistroPasos.Add($"[Error] {mensajeError}");

                    RecuperarError(noTerminal);
                    return nodo;
                }
            }

            if (accion != null)
            {
                nodo = accion();
            }

            return nodo;
        }

        private NodoAST Match(string esperado)
        {
            RegistroPasos.Add($"[Match] Esperado: '{esperado}', Token actual: '{tokenActual ?? "EOF"}'");
            NodoAST nodo = null;

            if (tokenActual == esperado)
            {
                // CAMBIO CRÍTICO: Usamos tokens[index].Valor en lugar de .Tipo para guardar el valor real (ej: "10" en vez de "CN_ENTERA")
                nodo = new NodoAST(esperado, tokens[index].Valor, tokens[index].Linea);
                index++;
                tokenActual = index < tokens.Length ? tokens[index].Tipo : null;
                lineaActual = index < tokens.Length ? tokens[index].Linea : lineaActual;
            }
            else
            {
                string mensajeError = $"Error en línea {lineaActual}: Se esperaba '{esperado}', pero se encontró '{tokenActual ?? "EOF"}'";
                ErroresSintacticos.Add((mensajeError, lineaActual));
                RegistroPasos.Add($"[Error] {mensajeError}");

                // Pánico simple: saltar token
                if (tokenActual != null)
                {
                    index++;
                    tokenActual = index < tokens.Length ? tokens[index].Tipo : null;
                    lineaActual = index < tokens.Length ? tokens[index].Linea : lineaActual;
                }
            }
            return nodo;
        }

        private void RecuperarError(string noTerminal)
        {
            // Conjunto de sincronización actualizado con IDs del PDF
            var tokensSincronizacion = new HashSet<string> {
                "CE10", "CE9", "PR07", null, "PR18", "PR09", "PR10", "PR05", "PR03", "PR01", "PR15", "IDEN",
                "PR13", "PR14", "PR02", "OPLO3", "CN_ENTERA", "CN_REALES", "CADE", "PR20", "PR06", "CE6"
            };

            while (tokenActual != null && !tokensSincronizacion.Contains(tokenActual))
            {
                RegistroPasos.Add($"[Recuperación] Omitiendo token inesperado: '{tokenActual}'");
                index++;
                tokenActual = index < tokens.Length ? tokens[index].Tipo : null;
                lineaActual = index < tokens.Length ? tokens[index].Linea : lineaActual;
            }
        }
    }
}
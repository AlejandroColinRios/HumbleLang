using HumbleLang;
using System;
using System.Collections.Generic;
using System.Linq;

public class AnalizadorLL1
{
    private (string Token, int Linea)[] tokens;
    private int index;
    private string tokenActual;
    private int lineaActual;
    private Dictionary<(string, string), Func<NodoAST>> tablaLL1;

    public List<string> RegistroPasos { get; private set; }
    public List<(string Mensaje, int Linea)> ErroresSintacticos { get; private set; }

    public AnalizadorLL1()
    {
        InicializarTabla();
        RegistroPasos = new List<string>();
        ErroresSintacticos = new List<(string, int)>();
    }

    public NodoAST Analizar(List<(string Token, int Linea)> listaTokens)
    {
        tokens = listaTokens.ToArray();
        index = 0;
        tokenActual = tokens.Length > 0 ? tokens[0].Token : null;
        lineaActual = tokens.Length > 0 ? tokens[0].Linea : 0;

        RegistroPasos.Clear();
        ErroresSintacticos.Clear();
        RegistroPasos.Add($"Inicio análisis. Tokens: {string.Join(", ", tokens.Select(t => t.Token))}");

        NodoAST raiz = AnalizarNoTerminal("PROG");

        if (tokenActual != null)
        {
            string tokensRestantes = string.Join(", ", tokens.Skip(index).Select(t => t.Token));
            ErroresSintacticos.Add(($"Error: tokens restantes después del análisis: '{tokensRestantes}'.", lineaActual));
        }

        return raiz;
    }

    private void InicializarTabla()
    {
        tablaLL1 = new Dictionary<(string, string), Func<NodoAST>>
        {
            // PROG -> 'ini' '{' STMT_LIST '}' 'fin'
            { ("PROG", "PR12"), () => {
                var nodo = new NodoAST("PROG");
                nodo.AgregarHijo(Match("PR12"));
                nodo.AgregarHijo(Match("CE8"));
                nodo.AgregarHijo(AnalizarNoTerminal("STMT_LIST"));
                nodo.AgregarHijo(Match("CE9"));
                nodo.AgregarHijo(Match("PR07"));
                return nodo;
            }},

            // STMT_LIST -> STMT STMT_LIST | ε
            { ("STMT_LIST", "PR18"), () => { // si
                var nodo = new NodoAST("STMT_LIST");
                nodo.AgregarHijo(AnalizarNoTerminal("STMT"));
                nodo.AgregarHijo(AnalizarNoTerminal("STMT_LIST"));
                return nodo;
            }},
            { ("STMT_LIST", "PR09"), () => { // imp
                var nodo = new NodoAST("STMT_LIST");
                nodo.AgregarHijo(AnalizarNoTerminal("STMT"));
                nodo.AgregarHijo(AnalizarNoTerminal("STMT_LIST"));
                return nodo;
            }},
            { ("STMT_LIST", "PR10"), () => { // imprp
                var nodo = new NodoAST("STMT_LIST");
                nodo.AgregarHijo(AnalizarNoTerminal("STMT"));
                nodo.AgregarHijo(AnalizarNoTerminal("STMT_LIST"));
                return nodo;
            }},
            { ("STMT_LIST", "PR05"), () => { // ent
                var nodo = new NodoAST("STMT_LIST");
                nodo.AgregarHijo(AnalizarNoTerminal("STMT"));
                nodo.AgregarHijo(AnalizarNoTerminal("STMT_LIST"));
                return nodo;
            }},
            { ("STMT_LIST", "PR03"), () => { // dec
                var nodo = new NodoAST("STMT_LIST");
                nodo.AgregarHijo(AnalizarNoTerminal("STMT"));
                nodo.AgregarHijo(AnalizarNoTerminal("STMT_LIST"));
                return nodo;
            }},
            { ("STMT_LIST", "PR01"), () => { // cad
                var nodo = new NodoAST("STMT_LIST");
                nodo.AgregarHijo(AnalizarNoTerminal("STMT"));
                nodo.AgregarHijo(AnalizarNoTerminal("STMT_LIST"));
                return nodo;
            }},
            { ("STMT_LIST", "PR15"), () => { // log
                var nodo = new NodoAST("STMT_LIST");
                nodo.AgregarHijo(AnalizarNoTerminal("STMT"));
                nodo.AgregarHijo(AnalizarNoTerminal("STMT_LIST"));
                return nodo;
            }},
            { ("STMT_LIST", "IDEN"), () => { // asignación
                var nodo = new NodoAST("STMT_LIST");
                nodo.AgregarHijo(AnalizarNoTerminal("STMT"));
                nodo.AgregarHijo(AnalizarNoTerminal("STMT_LIST"));
                return nodo;
            }},
            { ("STMT_LIST", "PR13"), () => { // lee
                var nodo = new NodoAST("STMT_LIST");
                nodo.AgregarHijo(AnalizarNoTerminal("STMT"));
                nodo.AgregarHijo(AnalizarNoTerminal("STMT_LIST"));
                return nodo;
            }},
            { ("STMT_LIST", "PR14"), () => { // lim
                var nodo = new NodoAST("STMT_LIST");
                nodo.AgregarHijo(AnalizarNoTerminal("STMT"));
                nodo.AgregarHijo(AnalizarNoTerminal("STMT_LIST"));
                return nodo;
            }},
            { ("STMT_LIST", "PR02"), () => { // cic
                var nodo = new NodoAST("STMT_LIST");
                nodo.AgregarHijo(AnalizarNoTerminal("STMT"));
                nodo.AgregarHijo(AnalizarNoTerminal("STMT_LIST"));
                return nodo;
            }},
            { ("STMT_LIST", "PR17"), () => { // rom
                var nodo = new NodoAST("STMT_LIST");
                nodo.AgregarHijo(AnalizarNoTerminal("STMT"));
                nodo.AgregarHijo(AnalizarNoTerminal("STMT_LIST"));
                return nodo;
            }},
            { ("STMT_LIST", "CE9"), () => new NodoAST("STMT_LIST") },
            { ("STMT_LIST", "PR07"), () => new NodoAST("STMT_LIST") },

            // STMT -> DECLARACION | ASIGNACION | IF_STMT | PRINT_STMT | READ_STMT | LIMPIAR | CICLO | ROMPER
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
            { ("STMT", "PR17"), () => AnalizarNoTerminal("ROMPER") },
            
            // DECLARACION -> TIPO DEC_PRIME
            { ("DECLARACION", "PR05"), () => {
                var nodo = new NodoAST("DECLARACION");
                nodo.AgregarHijo(Match("PR05"));
                nodo.AgregarHijo(AnalizarNoTerminal("DEC_PRIME"));
                return nodo;
            }},
            { ("DECLARACION", "PR03"), () => {
                var nodo = new NodoAST("DECLARACION");
                nodo.AgregarHijo(Match("PR03"));
                nodo.AgregarHijo(AnalizarNoTerminal("DEC_PRIME"));
                return nodo;
            }},
            { ("DECLARACION", "PR01"), () => {
                var nodo = new NodoAST("DECLARACION");
                nodo.AgregarHijo(Match("PR01"));
                nodo.AgregarHijo(AnalizarNoTerminal("DEC_PRIME"));
                return nodo;
            }},
            { ("DECLARACION", "PR15"), () => {
                var nodo = new NodoAST("DECLARACION");
                nodo.AgregarHijo(Match("PR15"));
                nodo.AgregarHijo(AnalizarNoTerminal("DEC_PRIME"));
                return nodo;
            }},
            
            // DEC_PRIME -> IDEN | IDEN OPAS EXPR
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

            // ASIGNACION -> IDEN OPAS EXPR
            { ("ASIGNACION", "IDEN"), () => {
                var nodo = new NodoAST("ASIGNACION");
                nodo.AgregarHijo(Match("IDEN"));
                nodo.AgregarHijo(Match("OPAS"));
                nodo.AgregarHijo(AnalizarNoTerminal("EXPR"));
                return nodo;
            }},
            
            // IF_STMT -> 'si' '(' COND_EXPR ')' '{' STMT_LIST '}' ( 'sino' '{' STMT_LIST '}' )?
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
            
            // IF_STMT_PRIME -> 'sino' '{' STMT_LIST '}'
            { ("IF_STMT_PRIME", "PR19"), () => {
                var nodo = new NodoAST("IF_STMT_PRIME");
                nodo.AgregarHijo(Match("PR19"));
                nodo.AgregarHijo(Match("CE8"));
                nodo.AgregarHijo(AnalizarNoTerminal("STMT_LIST"));
                nodo.AgregarHijo(Match("CE9"));
                return nodo;
            }},

            // PRINT_STMT -> 'imp' EXPR | 'imprp' EXPR
            { ("PRINT_STMT", "PR09"), () => {
                var nodo = new NodoAST("PRINT_STMT");
                nodo.AgregarHijo(Match("PR09"));
                nodo.AgregarHijo(AnalizarNoTerminal("EXPR"));
                return nodo;
            }},
            { ("PRINT_STMT", "PR10"), () => {
                var nodo = new NodoAST("PRINT_STMT");
                nodo.AgregarHijo(Match("PR10"));
                nodo.AgregarHijo(AnalizarNoTerminal("EXPR"));
                return nodo;
            }},

            // READ_STMT -> 'lee' IDEN
            { ("READ_STMT", "PR13"), () => {
                var nodo = new NodoAST("READ_STMT");
                nodo.AgregarHijo(Match("PR13"));
                nodo.AgregarHijo(Match("IDEN"));
                return nodo;
            }},

            // LIMPIAR -> 'lim'
            { ("LIMPIAR", "PR14"), () => Match("PR14") },
            
            // CICLO -> 'cic' 'des' IDEN '=' EXPR 'has' EXPR 'inc' EXPR '{' STMT_LIST '}'
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
            
            // ROMPER -> 'rom'
            { ("ROMPER", "PR17"), () => Match("PR17") },

            // COND_EXPR -> LOG_EXPR
            { ("COND_EXPR", "PR16"), () => AnalizarNoTerminal("LOG_EXPR") },
            { ("COND_EXPR", "IDEN"), () => AnalizarNoTerminal("LOG_EXPR") },
            { ("COND_EXPR", "CN_ENTERA"), () => AnalizarNoTerminal("LOG_EXPR") },
            { ("COND_EXPR", "CN_DECIMAL"), () => AnalizarNoTerminal("LOG_EXPR") },
            { ("COND_EXPR", "CADE"), () => AnalizarNoTerminal("LOG_EXPR") },
            { ("COND_EXPR", "PR20"), () => AnalizarNoTerminal("LOG_EXPR") },
            { ("COND_EXPR", "PR06"), () => AnalizarNoTerminal("LOG_EXPR") },
            { ("COND_EXPR", "CE6"), () => AnalizarNoTerminal("LOG_EXPR") },
            
            // EXPR -> LOG_EXPR | ARITH_EXPR
            { ("EXPR", "PR16"), () => AnalizarNoTerminal("LOG_EXPR") },
            { ("EXPR", "IDEN"), () => AnalizarNoTerminal("LOG_EXPR") },
            { ("EXPR", "CN_ENTERA"), () => AnalizarNoTerminal("LOG_EXPR") },
            { ("EXPR", "CN_DECIMAL"), () => AnalizarNoTerminal("LOG_EXPR") },
            { ("EXPR", "CADE"), () => AnalizarNoTerminal("LOG_EXPR") },
            { ("EXPR", "PR20"), () => AnalizarNoTerminal("LOG_EXPR") },
            { ("EXPR", "PR06"), () => AnalizarNoTerminal("LOG_EXPR") },
            { ("EXPR", "CE6"), () => AnalizarNoTerminal("LOG_EXPR") },

            // LOG_EXPR -> REL_EXPR LOG_EXPR_PRIME
            { ("LOG_EXPR", "PR16"), () => {
                var nodo = new NodoAST("LOG_EXPR");
                nodo.AgregarHijo(AnalizarNoTerminal("REL_EXPR"));
                nodo.AgregarHijo(AnalizarNoTerminal("LOG_EXPR_PRIME"));
                return nodo;
            }},
            { ("LOG_EXPR", "IDEN"), () => {
                var nodo = new NodoAST("LOG_EXPR");
                nodo.AgregarHijo(AnalizarNoTerminal("REL_EXPR"));
                nodo.AgregarHijo(AnalizarNoTerminal("LOG_EXPR_PRIME"));
                return nodo;
            }},
            { ("LOG_EXPR", "CN_ENTERA"), () => {
                var nodo = new NodoAST("LOG_EXPR");
                nodo.AgregarHijo(AnalizarNoTerminal("REL_EXPR"));
                nodo.AgregarHijo(AnalizarNoTerminal("LOG_EXPR_PRIME"));
                return nodo;
            }},
            { ("LOG_EXPR", "CN_DECIMAL"), () => {
                var nodo = new NodoAST("LOG_EXPR");
                nodo.AgregarHijo(AnalizarNoTerminal("REL_EXPR"));
                nodo.AgregarHijo(AnalizarNoTerminal("LOG_EXPR_PRIME"));
                return nodo;
            }},
            { ("LOG_EXPR", "CADE"), () => {
                var nodo = new NodoAST("LOG_EXPR");
                nodo.AgregarHijo(AnalizarNoTerminal("REL_EXPR"));
                nodo.AgregarHijo(AnalizarNoTerminal("LOG_EXPR_PRIME"));
                return nodo;
            }},
            { ("LOG_EXPR", "PR20"), () => {
                var nodo = new NodoAST("LOG_EXPR");
                nodo.AgregarHijo(AnalizarNoTerminal("REL_EXPR"));
                nodo.AgregarHijo(AnalizarNoTerminal("LOG_EXPR_PRIME"));
                return nodo;
            }},
            { ("LOG_EXPR", "PR06"), () => {
                var nodo = new NodoAST("LOG_EXPR");
                nodo.AgregarHijo(AnalizarNoTerminal("REL_EXPR"));
                nodo.AgregarHijo(AnalizarNoTerminal("LOG_EXPR_PRIME"));
                return nodo;
            }},
            { ("LOG_EXPR", "CE6"), () => {
                var nodo = new NodoAST("LOG_EXPR");
                nodo.AgregarHijo(AnalizarNoTerminal("REL_EXPR"));
                nodo.AgregarHijo(AnalizarNoTerminal("LOG_EXPR_PRIME"));
                return nodo;
            }},
            
            // LOG_EXPR_PRIME -> OPLO REL_EXPR LOG_EXPR_PRIME | ε
            { ("LOG_EXPR_PRIME", "OPLO1"), () => {
                var nodo = new NodoAST("LOG_EXPR_PRIME");
                nodo.AgregarHijo(Match("OPLO1"));
                nodo.AgregarHijo(AnalizarNoTerminal("REL_EXPR"));
                nodo.AgregarHijo(AnalizarNoTerminal("LOG_EXPR_PRIME"));
                return nodo;
            }},
            { ("LOG_EXPR_PRIME", "OPLO2"), () => {
                var nodo = new NodoAST("LOG_EXPR_PRIME");
                nodo.AgregarHijo(Match("OPLO2"));
                nodo.AgregarHijo(AnalizarNoTerminal("REL_EXPR"));
                nodo.AgregarHijo(AnalizarNoTerminal("LOG_EXPR_PRIME"));
                return nodo;
            }},
            { ("LOG_EXPR_PRIME", "CE7"), () => new NodoAST("LOG_EXPR_PRIME") },
            { ("LOG_EXPR_PRIME", "CE9"), () => new NodoAST("LOG_EXPR_PRIME") },
            { ("LOG_EXPR_PRIME", "PR19"), () => new NodoAST("LOG_EXPR_PRIME") },
            { ("LOG_EXPR_PRIME", "PR04"), () => new NodoAST("LOG_EXPR_PRIME") },
            { ("LOG_EXPR_PRIME", "PR08"), () => new NodoAST("LOG_EXPR_PRIME") },
            { ("LOG_EXPR_PRIME", "PR11"), () => new NodoAST("LOG_EXPR_PRIME") },
            { ("LOG_EXPR_PRIME", null), () => new NodoAST("LOG_EXPR_PRIME") },

            // REL_EXPR -> ARITH_EXPR REL_EXPR_PRIME
            { ("REL_EXPR", "PR16"), () => {
                var nodo = new NodoAST("REL_EXPR");
                nodo.AgregarHijo(AnalizarNoTerminal("ARITH_EXPR"));
                nodo.AgregarHijo(AnalizarNoTerminal("REL_EXPR_PRIME"));
                return nodo;
            }},
            { ("REL_EXPR", "IDEN"), () => {
                var nodo = new NodoAST("REL_EXPR");
                nodo.AgregarHijo(AnalizarNoTerminal("ARITH_EXPR"));
                nodo.AgregarHijo(AnalizarNoTerminal("REL_EXPR_PRIME"));
                return nodo;
            }},
            { ("REL_EXPR", "CN_ENTERA"), () => {
                var nodo = new NodoAST("REL_EXPR");
                nodo.AgregarHijo(AnalizarNoTerminal("ARITH_EXPR"));
                nodo.AgregarHijo(AnalizarNoTerminal("REL_EXPR_PRIME"));
                return nodo;
            }},
            { ("REL_EXPR", "CN_DECIMAL"), () => {
                var nodo = new NodoAST("REL_EXPR");
                nodo.AgregarHijo(AnalizarNoTerminal("ARITH_EXPR"));
                nodo.AgregarHijo(AnalizarNoTerminal("REL_EXPR_PRIME"));
                return nodo;
            }},
            { ("REL_EXPR", "CADE"), () => {
                var nodo = new NodoAST("REL_EXPR");
                nodo.AgregarHijo(AnalizarNoTerminal("ARITH_EXPR"));
                nodo.AgregarHijo(AnalizarNoTerminal("REL_EXPR_PRIME"));
                return nodo;
            }},
            { ("REL_EXPR", "PR20"), () => {
                var nodo = new NodoAST("REL_EXPR");
                nodo.AgregarHijo(AnalizarNoTerminal("ARITH_EXPR"));
                nodo.AgregarHijo(AnalizarNoTerminal("REL_EXPR_PRIME"));
                return nodo;
            }},
            { ("REL_EXPR", "PR06"), () => {
                var nodo = new NodoAST("REL_EXPR");
                nodo.AgregarHijo(AnalizarNoTerminal("ARITH_EXPR"));
                nodo.AgregarHijo(AnalizarNoTerminal("REL_EXPR_PRIME"));
                return nodo;
            }},
            { ("REL_EXPR", "CE6"), () => {
                var nodo = new NodoAST("REL_EXPR");
                nodo.AgregarHijo(AnalizarNoTerminal("ARITH_EXPR"));
                nodo.AgregarHijo(AnalizarNoTerminal("REL_EXPR_PRIME"));
                return nodo;
            }},
            
            // REL_EXPR_PRIME -> OPREL ARITH_EXPR | ε
            { ("REL_EXPR_PRIME", "OPREL1"), () => {
                var nodo = new NodoAST("REL_EXPR_PRIME");
                nodo.AgregarHijo(Match("OPREL1"));
                nodo.AgregarHijo(AnalizarNoTerminal("ARITH_EXPR"));
                return nodo;
            }},
            { ("REL_EXPR_PRIME", "OPREL2"), () => {
                var nodo = new NodoAST("REL_EXPR_PRIME");
                nodo.AgregarHijo(Match("OPREL2"));
                nodo.AgregarHijo(AnalizarNoTerminal("ARITH_EXPR"));
                return nodo;
            }},
            { ("REL_EXPR_PRIME", "OPREL3"), () => {
                var nodo = new NodoAST("REL_EXPR_PRIME");
                nodo.AgregarHijo(Match("OPREL3"));
                nodo.AgregarHijo(AnalizarNoTerminal("ARITH_EXPR"));
                return nodo;
            }},
            { ("REL_EXPR_PRIME", "OPREL4"), () => {
                var nodo = new NodoAST("REL_EXPR_PRIME");
                nodo.AgregarHijo(Match("OPREL4"));
                nodo.AgregarHijo(AnalizarNoTerminal("ARITH_EXPR"));
                return nodo;
            }},
            { ("REL_EXPR_PRIME", "OPREL5"), () => {
                var nodo = new NodoAST("REL_EXPR_PRIME");
                nodo.AgregarHijo(Match("OPREL5"));
                nodo.AgregarHijo(AnalizarNoTerminal("ARITH_EXPR"));
                return nodo;
            }},
            { ("REL_EXPR_PRIME", "OPREL6"), () => {
                var nodo = new NodoAST("REL_EXPR_PRIME");
                nodo.AgregarHijo(Match("OPREL6"));
                nodo.AgregarHijo(AnalizarNoTerminal("ARITH_EXPR"));
                return nodo;
            }},
            { ("REL_EXPR_PRIME", "CE7"), () => new NodoAST("REL_EXPR_PRIME") },
            { ("REL_EXPR_PRIME", "CE9"), () => new NodoAST("REL_EXPR_PRIME") },
            { ("REL_EXPR_PRIME", "OPLO1"), () => new NodoAST("REL_EXPR_PRIME") },
            { ("REL_EXPR_PRIME", "OPLO2"), () => new NodoAST("REL_EXPR_PRIME") },
            { ("REL_EXPR_PRIME", "PR19"), () => new NodoAST("REL_EXPR_PRIME") },
            { ("REL_EXPR_PRIME", "PR04"), () => new NodoAST("REL_EXPR_PRIME") },
            { ("REL_EXPR_PRIME", "PR08"), () => new NodoAST("REL_EXPR_PRIME") },
            { ("REL_EXPR_PRIME", "PR11"), () => new NodoAST("REL_EXPR_PRIME") },
            { ("REL_EXPR_PRIME", null), () => new NodoAST("REL_EXPR_PRIME") },

            // ARITH_EXPR -> TERM ARITH_EXPR_PRIME
            { ("ARITH_EXPR", "PR16"), () => {
                var nodo = new NodoAST("ARITH_EXPR");
                nodo.AgregarHijo(AnalizarNoTerminal("TERM"));
                nodo.AgregarHijo(AnalizarNoTerminal("ARITH_EXPR_PRIME"));
                return nodo;
            }},
            { ("ARITH_EXPR", "IDEN"), () => {
                var nodo = new NodoAST("ARITH_EXPR");
                nodo.AgregarHijo(AnalizarNoTerminal("TERM"));
                nodo.AgregarHijo(AnalizarNoTerminal("ARITH_EXPR_PRIME"));
                return nodo;
            }},
            { ("ARITH_EXPR", "CN_ENTERA"), () => {
                var nodo = new NodoAST("ARITH_EXPR");
                nodo.AgregarHijo(AnalizarNoTerminal("TERM"));
                nodo.AgregarHijo(AnalizarNoTerminal("ARITH_EXPR_PRIME"));
                return nodo;
            }},
            { ("ARITH_EXPR", "CN_DECIMAL"), () => {
                var nodo = new NodoAST("ARITH_EXPR");
                nodo.AgregarHijo(AnalizarNoTerminal("TERM"));
                nodo.AgregarHijo(AnalizarNoTerminal("ARITH_EXPR_PRIME"));
                return nodo;
            }},
            { ("ARITH_EXPR", "CADE"), () => {
                var nodo = new NodoAST("ARITH_EXPR");
                nodo.AgregarHijo(AnalizarNoTerminal("TERM"));
                nodo.AgregarHijo(AnalizarNoTerminal("ARITH_EXPR_PRIME"));
                return nodo;
            }},
            { ("ARITH_EXPR", "PR20"), () => {
                var nodo = new NodoAST("ARITH_EXPR");
                nodo.AgregarHijo(AnalizarNoTerminal("TERM"));
                nodo.AgregarHijo(AnalizarNoTerminal("ARITH_EXPR_PRIME"));
                return nodo;
            }},
            { ("ARITH_EXPR", "PR06"), () => {
                var nodo = new NodoAST("ARITH_EXPR");
                nodo.AgregarHijo(AnalizarNoTerminal("TERM"));
                nodo.AgregarHijo(AnalizarNoTerminal("ARITH_EXPR_PRIME"));
                return nodo;
            }},
            { ("ARITH_EXPR", "CE6"), () => {
                var nodo = new NodoAST("ARITH_EXPR");
                nodo.AgregarHijo(AnalizarNoTerminal("TERM"));
                nodo.AgregarHijo(AnalizarNoTerminal("ARITH_EXPR_PRIME"));
                return nodo;
            }},

            // ARITH_EXPR_PRIME -> OPAR1 TERM ARITH_EXPR_PRIME | OPAR2 TERM ARITH_EXPR_PRIME | ε
            { ("ARITH_EXPR_PRIME", "OPAR1"), () => { // <-- CORRECCIÓN: OPAR1 para la suma (+)
                var nodo = new NodoAST("ARITH_EXPR_PRIME");
                nodo.AgregarHijo(Match("OPAR1"));
                nodo.AgregarHijo(AnalizarNoTerminal("TERM"));
                nodo.AgregarHijo(AnalizarNoTerminal("ARITH_EXPR_PRIME"));
                return nodo;
            }},
            { ("ARITH_EXPR_PRIME", "OPAR2"), () => { // <-- CORRECCIÓN: OPAR2 para la resta (-)
                var nodo = new NodoAST("ARITH_EXPR_PRIME");
                nodo.AgregarHijo(Match("OPAR2"));
                nodo.AgregarHijo(AnalizarNoTerminal("TERM"));
                nodo.AgregarHijo(AnalizarNoTerminal("ARITH_EXPR_PRIME"));
                return nodo;
            }},
            { ("ARITH_EXPR_PRIME", "CE7"), () => new NodoAST("ARITH_EXPR_PRIME") },
            { ("ARITH_EXPR_PRIME", "CE9"), () => new NodoAST("ARITH_EXPR_PRIME") },
            { ("ARITH_EXPR_PRIME", "OPLO1"), () => new NodoAST("ARITH_EXPR_PRIME") },
            { ("ARITH_EXPR_PRIME", "OPLO2"), () => new NodoAST("ARITH_EXPR_PRIME") },
            { ("ARITH_EXPR_PRIME", "OPREL1"), () => new NodoAST("ARITH_EXPR_PRIME") },
            { ("ARITH_EXPR_PRIME", "OPREL2"), () => new NodoAST("ARITH_EXPR_PRIME") },
            { ("ARITH_EXPR_PRIME", "OPREL3"), () => new NodoAST("ARITH_EXPR_PRIME") },
            { ("ARITH_EXPR_PRIME", "OPREL4"), () => new NodoAST("ARITH_EXPR_PRIME") },
            { ("ARITH_EXPR_PRIME", "OPREL5"), () => new NodoAST("ARITH_EXPR_PRIME") },
            { ("ARITH_EXPR_PRIME", "OPREL6"), () => new NodoAST("ARITH_EXPR_PRIME") },
            { ("ARITH_EXPR_PRIME", "PR19"), () => new NodoAST("ARITH_EXPR_PRIME") },
            { ("ARITH_EXPR_PRIME", "PR04"), () => new NodoAST("ARITH_EXPR_PRIME") },
            { ("ARITH_EXPR_PRIME", "PR08"), () => new NodoAST("ARITH_EXPR_PRIME") },
            { ("ARITH_EXPR_PRIME", "PR11"), () => new NodoAST("ARITH_EXPR_PRIME") },
            { ("ARITH_EXPR_PRIME", null), () => new NodoAST("ARITH_EXPR_PRIME") },
            
            // TERM -> FACTOR TERM_PRIME
            { ("TERM", "PR16"), () => {
                var nodo = new NodoAST("TERM");
                nodo.AgregarHijo(AnalizarNoTerminal("FACTOR"));
                nodo.AgregarHijo(AnalizarNoTerminal("TERM_PRIME"));
                return nodo;
            }},
            { ("TERM", "IDEN"), () => {
                var nodo = new NodoAST("TERM");
                nodo.AgregarHijo(AnalizarNoTerminal("FACTOR"));
                nodo.AgregarHijo(AnalizarNoTerminal("TERM_PRIME"));
                return nodo;
            }},
            { ("TERM", "CN_ENTERA"), () => {
                var nodo = new NodoAST("TERM");
                nodo.AgregarHijo(AnalizarNoTerminal("FACTOR"));
                nodo.AgregarHijo(AnalizarNoTerminal("TERM_PRIME"));
                return nodo;
            }},
            { ("TERM", "CN_DECIMAL"), () => {
                var nodo = new NodoAST("TERM");
                nodo.AgregarHijo(AnalizarNoTerminal("FACTOR"));
                nodo.AgregarHijo(AnalizarNoTerminal("TERM_PRIME"));
                return nodo;
            }},
            { ("TERM", "CADE"), () => {
                var nodo = new NodoAST("TERM");
                nodo.AgregarHijo(AnalizarNoTerminal("FACTOR"));
                nodo.AgregarHijo(AnalizarNoTerminal("TERM_PRIME"));
                return nodo;
            }},
            { ("TERM", "PR20"), () => {
                var nodo = new NodoAST("TERM");
                nodo.AgregarHijo(AnalizarNoTerminal("FACTOR"));
                nodo.AgregarHijo(AnalizarNoTerminal("TERM_PRIME"));
                return nodo;
            }},
            { ("TERM", "PR06"), () => {
                var nodo = new NodoAST("TERM");
                nodo.AgregarHijo(AnalizarNoTerminal("FACTOR"));
                nodo.AgregarHijo(AnalizarNoTerminal("TERM_PRIME"));
                return nodo;
            }},
            { ("TERM", "CE6"), () => {
                var nodo = new NodoAST("TERM");
                nodo.AgregarHijo(AnalizarNoTerminal("FACTOR"));
                nodo.AgregarHijo(AnalizarNoTerminal("TERM_PRIME"));
                return nodo;
            }},

            // TERM_PRIME -> OPRE3 FACTOR TERM_PRIME | OPRE4 FACTOR TERM_PRIME | OPRE5 FACTOR TERM_PRIME | ε
            { ("TERM_PRIME", "OPRE3"), () => {
                var nodo = new NodoAST("TERM_PRIME");
                nodo.AgregarHijo(Match("OPRE3"));
                nodo.AgregarHijo(AnalizarNoTerminal("FACTOR"));
                nodo.AgregarHijo(AnalizarNoTerminal("TERM_PRIME"));
                return nodo;
            }},
            { ("TERM_PRIME", "OPRE4"), () => {
                var nodo = new NodoAST("TERM_PRIME");
                nodo.AgregarHijo(Match("OPRE4"));
                nodo.AgregarHijo(AnalizarNoTerminal("FACTOR"));
                nodo.AgregarHijo(AnalizarNoTerminal("TERM_PRIME"));
                return nodo;
            }},
            { ("TERM_PRIME", "OPRE5"), () => {
                var nodo = new NodoAST("TERM_PRIME");
                nodo.AgregarHijo(Match("OPRE5"));
                nodo.AgregarHijo(AnalizarNoTerminal("FACTOR"));
                nodo.AgregarHijo(AnalizarNoTerminal("TERM_PRIME"));
                return nodo;
            }},
            { ("TERM_PRIME", "OPAR1"), () => new NodoAST("TERM_PRIME") }, // <-- CORRECCIÓN
            { ("TERM_PRIME", "OPAR2"), () => new NodoAST("TERM_PRIME") }, // <-- CORRECCIÓN
            { ("TERM_PRIME", "CE7"), () => new NodoAST("TERM_PRIME") },
            { ("TERM_PRIME", "CE9"), () => new NodoAST("TERM_PRIME") },
            { ("TERM_PRIME", "OPLO1"), () => new NodoAST("TERM_PRIME") },
            { ("TERM_PRIME", "OPLO2"), () => new NodoAST("TERM_PRIME") },
            { ("TERM_PRIME", "OPREL1"), () => new NodoAST("TERM_PRIME") },
            { ("TERM_PRIME", "OPREL2"), () => new NodoAST("TERM_PRIME") },
            { ("TERM_PRIME", "OPREL3"), () => new NodoAST("TERM_PRIME") },
            { ("TERM_PRIME", "OPREL4"), () => new NodoAST("TERM_PRIME") },
            { ("TERM_PRIME", "OPREL5"), () => new NodoAST("TERM_PRIME") },
            { ("TERM_PRIME", "OPREL6"), () => new NodoAST("TERM_PRIME") },
            { ("TERM_PRIME", "PR19"), () => new NodoAST("TERM_PRIME") },
            { ("TERM_PRIME", "PR04"), () => new NodoAST("TERM_PRIME") },
            { ("TERM_PRIME", "PR08"), () => new NodoAST("TERM_PRIME") },
            { ("TERM_PRIME", "PR11"), () => new NodoAST("TERM_PRIME") },
            { ("TERM_PRIME", null), () => new NodoAST("TERM_PRIME") },

            // FACTOR -> IDEN | LITERAL | PR16 EXPR | CE6 EXPR CE7
            { ("FACTOR", "IDEN"), () => Match("IDEN") },
            { ("FACTOR", "CN_ENTERA"), () => AnalizarNoTerminal("LITERAL") },
            { ("FACTOR", "CN_DECIMAL"), () => AnalizarNoTerminal("LITERAL") },
            { ("FACTOR", "CADE"), () => AnalizarNoTerminal("LITERAL") },
            { ("FACTOR", "PR20"), () => AnalizarNoTerminal("LITERAL") },
            { ("FACTOR", "PR06"), () => AnalizarNoTerminal("LITERAL") },
            { ("FACTOR", "PR16"), () => {
                var nodo = new NodoAST("FACTOR");
                nodo.AgregarHijo(Match("PR16"));
                nodo.AgregarHijo(AnalizarNoTerminal("EXPR"));
                return nodo;
            }},
            { ("FACTOR", "CE6"), () => {
                var nodo = new NodoAST("FACTOR");
                nodo.AgregarHijo(Match("CE6"));
                nodo.AgregarHijo(AnalizarNoTerminal("EXPR"));
                nodo.AgregarHijo(Match("CE7"));
                return nodo;
            }},

            // LITERAL -> CN_ENTERA | CN_DECIMAL | CADE | PR20 | PR06
            { ("LITERAL", "CN_ENTERA"), () => Match("CN_ENTERA") },
            { ("LITERAL", "CN_DECIMAL"), () => Match("CN_DECIMAL") },
            { ("LITERAL", "CADE"), () => Match("CADE") },
            { ("LITERAL", "PR20"), () => Match("PR20") },
            { ("LITERAL", "PR06"), () => Match("PR06") },
        };
    }

    private NodoAST AnalizarNoTerminal(string noTerminal)
    {
        RegistroPasos.Add($"[Análisis] No terminal: '{noTerminal}', Token actual: '{tokenActual ?? "EOF"}'");
        var nodo = new NodoAST(noTerminal);

        var clave = (noTerminal, tokenActual);
        if (!tablaLL1.TryGetValue(clave, out var accion))
        {
            if (tablaLL1.ContainsKey((noTerminal, null)))
            {
                clave = (noTerminal, null);
                accion = tablaLL1[clave];
            }
            else
            {
                var tokensEsperados = tablaLL1.Keys
                    .Where(k => k.Item1 == noTerminal && k.Item2 != null)
                    .Select(k => k.Item2)
                    .Distinct()
                    .ToList();

                string descripcionEsperados = tokensEsperados.Count > 0 ?
                    string.Join(", ", tokensEsperados.Select(t => t)) : "fin de sentencia";

                string mensajeError = $"Error en línea {lineaActual}: se esperaba {descripcionEsperados} después de '{noTerminal}', pero se encontró {tokenActual ?? "EOF"}.";
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
            nodo = new NodoAST(esperado, tokens[index].Token, tokens[index].Linea);

            index++;
            tokenActual = index < tokens.Length ? tokens[index].Token : null;
            lineaActual = index < tokens.Length ? tokens[index].Linea : lineaActual;
            RegistroPasos.Add($"[Match Exitoso] Avanzando a: '{tokenActual ?? "EOF"}'");
        }
        else
        {
            string mensajeError = $"Error en línea {lineaActual}: Se esperaba '{esperado}', pero se encontró '{tokenActual ?? "EOF"}.'";
            ErroresSintacticos.Add((mensajeError, lineaActual));
            RegistroPasos.Add($"[Error] {mensajeError}");

            if (tokenActual != null)
            {
                RegistroPasos.Add($"[Recuperación] Omitiendo token inesperado: '{tokenActual}'");
                index++;
                tokenActual = index < tokens.Length ? tokens[index].Token : null;
                lineaActual = index < tokens.Length ? tokens[index].Linea : lineaActual;
            }
        }

        return nodo;
    }

    private void RecuperarError(string noTerminal)
    {
        var tokensSincronizacion = new HashSet<string> { "CE9", "PR41", "PR42", "PR39", "PR40", "PR07", null };
        while (tokenActual != null && !tokensSincronizacion.Contains(tokenActual))
        {
            RegistroPasos.Add($"[Recuperación] Omitiendo token inesperado: '{tokenActual}'");
            index++;
            tokenActual = index < tokens.Length ? tokens[index].Token : null;
            lineaActual = index < tokens.Length ? tokens[index].Linea : lineaActual;
        }
    }
}
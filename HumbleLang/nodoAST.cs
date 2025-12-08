using System;
using System.Collections.Generic;
using System.Text;

namespace HumbleLang
{
    public class NodoAST
    {
        public string Tipo { get; set; }       // Ejemplo: "STMT", "IF_STMT" (No Terminales)
        public string Valor { get; set; }      // Ejemplo: "10", "hola", "x" (Valores reales)
        public int Linea { get; set; }

        // --- PROPIEDAD AGREGADA PARA EL INTÉRPRETE ---
        // Esta propiedad ayuda a identificar el ID del token específico (ej: "CN_ENTERA", "OPAR1")
        public string Token { get; set; }

        public List<NodoAST> Hijos { get; private set; } = new List<NodoAST>();

        // Constructor actualizado
        public NodoAST(string tipo, string valor = null, int linea = 0)
        {
            Tipo = tipo;
            Valor = valor;
            Linea = linea;

            // IMPORTANTE: Por defecto, el Token es igual al Tipo.
            // Esto asegura que cuando haces Match("CN_ENTERA"), 
            // tanto nodo.Tipo como nodo.Token valgan "CN_ENTERA".
            Token = tipo;
        }

        public void AgregarHijo(NodoAST hijo)
        {
            if (hijo != null) Hijos.Add(hijo);
        }

        public string ImprimirArbol(int indent = 0)
        {
            var sb = new StringBuilder();
            sb.Append(new string(' ', indent * 2));

            // Mostramos Tipo y Token si son diferentes, para depurar mejor
            if (Tipo != Token && Token != null)
                sb.Append($"[{Tipo} : {Token}]");
            else
                sb.Append($"[{Tipo}]");

            if (!string.IsNullOrEmpty(Valor))
            {
                sb.Append($" - Valor: '{Valor}'");
            }
            sb.AppendLine();

            foreach (var hijo in Hijos)
            {
                sb.Append(hijo.ImprimirArbol(indent + 1));
            }

            return sb.ToString();
        }
    }
}
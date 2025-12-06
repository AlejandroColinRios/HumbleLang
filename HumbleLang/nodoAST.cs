using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HumbleLang
{
    public class NodoAST
    {
        public string Tipo { get; set; }               // Tipo de nodo (STMT, IF, EXPR, etc.)
        public string Valor { get; set; }              // Valor del token (si aplica)
        public int Linea { get; set; }

        public List<NodoAST> Hijos { get; private set; } = new List<NodoAST>();

        public NodoAST(string tipo, string valor = null, int linea = 0)
        {
            Tipo = tipo;
            Valor = valor;
            Linea = linea;
        }

        public void AgregarHijo(NodoAST hijo)
        {
            if (hijo != null) Hijos.Add(hijo);
        }

        // Nuevo método para imprimir el árbol
        public string ImprimirArbol(int indent = 0)
        {
            var sb = new StringBuilder();
            sb.Append(new string(' ', indent * 2));
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
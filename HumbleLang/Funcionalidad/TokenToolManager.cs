using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace HumbleLang.Funcionalidad
{
    public class TokenToolTipManager
    {
        private readonly RichTextBox editor;
        private readonly ToolTip toolTip;
        private readonly Dictionary<string, string> descripciones;

        public TokenToolTipManager(RichTextBox editor)
        {
            this.editor = editor;
            toolTip = new ToolTip();
            descripciones = CargarDescripciones();

            // Suscribirse al evento MouseMove
            this.editor.MouseMove += Editor_MouseMove;
        }

        private void Editor_MouseMove(object sender, MouseEventArgs e)
        {
            int index = editor.GetCharIndexFromPosition(e.Location);
            if (index < 0 || index >= editor.Text.Length)
                return;

            int start = index, end = index;

            // Expandir hacia atrás hasta espacio o inicio
            while (start > 0 && !char.IsWhiteSpace(editor.Text[start - 1])) start--;
            // Expandir hacia adelante hasta espacio o fin
            while (end < editor.Text.Length && !char.IsWhiteSpace(editor.Text[end])) end++;

            string palabra = editor.Text.Substring(start, end - start);
            if (descripciones.TryGetValue(palabra, out string descripcion))
            {
                toolTip.SetToolTip(editor, descripcion);
            }
            else
            {
                toolTip.SetToolTip(editor, "");
            }
        }



        
        private Dictionary<string, string> CargarDescripciones()
{
    return new Dictionary<string, string>()
    {
        // Palabras clave
        {"ent", "ent: declara una variable entera."},
        {"log", "log: declara una variable booleana."},
        {"cad", "cad: declara una cadena de texto."},
        {"ini", "ini: marca el inicio del bloque de ejecución."},
        {"fin", "fin: marca el final del bloque de ejecución."},
        {"lee", "lee: función para entrada de datos."},
        {"imp", "imprimir: función para salida de datos."},
        {"si", "si: inicia una estructura condicional."},
        {"sino", "sino: bloque alternativo del 'si'."},
        {"vdd", "vdd: valor verdadero (booleano)."},
        {"fal", "fal: valor falso (booleano)."},
        {"rom", "rom: rompe un ciclo o bucle."},
        {"cic", "cic: bucle con condición."},
        {"des", "des: indica el inicio de un rango o ciclo desde un valor."},
        {"has", "has: establece un límite de ciclos o repeticiones."},
        {"inc", "inc: incremento de una variable."},
        {"nul", "nul: valor nulo o vacío."},
        {"lim", "lim: limpia la pantalla o reset de variables."},
        {"imprp", "imprp: imprime un valor específico con formato personalizado."},

        
    };
}


    }
}

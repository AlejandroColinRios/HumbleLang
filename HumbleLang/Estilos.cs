using System;
using System.Drawing;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Collections.Generic;


public class EditorDeCodigo
{
    private RichTextBox _richTextBoxCodigo;
    private RichTextBox _richTextBoxTokens;
    private Panel _panelLineasCodigo;
    private Panel _panelLineasTokens;

    [DllImport("user32.dll")]
    private static extern int SendMessage(IntPtr hWnd, int wMsg, int wParam, int lParam);
    private const int EM_GETFIRSTVISIBLELINE = 0xCE;

    private List<(int inicio, int longitud)> errores = new List<(int, int)>();

    private readonly Font fuenteLineas = new Font("Consolas", 10);
    private readonly Brush pincelLineas = new SolidBrush(Color.Gray);

    public EditorDeCodigo(RichTextBox richTextBoxCodigo, Panel panelLineasCodigo,
                          RichTextBox richTextBoxTokens, Panel panelLineasTokens)
    {
        _richTextBoxCodigo = richTextBoxCodigo;
        _panelLineasCodigo = panelLineasCodigo;
        _richTextBoxTokens = richTextBoxTokens;
        _panelLineasTokens = panelLineasTokens;

        ConfigurarRichTextBox(_richTextBoxCodigo);
        ConfigurarRichTextBox(_richTextBoxTokens);

        _richTextBoxCodigo.VScroll += (s, e) => DibujarLineasPara(_richTextBoxCodigo, _panelLineasCodigo);
        _richTextBoxCodigo.TextChanged += (s, e) =>
        {
            DibujarLineasPara(_richTextBoxCodigo, _panelLineasCodigo);
            ResaltarPalabrasReservadas(_richTextBoxCodigo);
        };

        _richTextBoxTokens.VScroll += (s, e) => DibujarLineasPara(_richTextBoxTokens, _panelLineasTokens);
        _richTextBoxTokens.TextChanged += (s, e) =>
        {
            DibujarLineasPara(_richTextBoxTokens, _panelLineasTokens);
        };

        _panelLineasCodigo.Paint += PanelLineas_Paint;
        _panelLineasTokens.Paint += PanelLineas_Paint;
    }

    private void ConfigurarRichTextBox(RichTextBox richTextBox)
    {
        try { richTextBox.Font = new Font("Orbitron", 10); }
        catch { richTextBox.Font = new Font("Consolas", 10); }

        richTextBox.BackColor = Color.White;
        richTextBox.ForeColor = Color.Black;
        richTextBox.WordWrap = true;
        richTextBox.AcceptsTab = true;
        richTextBox.HideSelection = false;
        richTextBox.ScrollBars = RichTextBoxScrollBars.Both;
        richTextBox.ReadOnly = false;
    }

    public void DibujarLineasPara(RichTextBox richTextBox, Panel panelDestino)
    {
        panelDestino.Tag = richTextBox;
        panelDestino.Invalidate();
    }

    private void PanelLineas_Paint(object sender, PaintEventArgs e)
    {
        if (sender is Panel panel && panel.Tag is RichTextBox richTextBox)
        {
            e.Graphics.Clear(panel.BackColor);

            int firstVisibleLine = SendMessage(richTextBox.Handle, EM_GETFIRSTVISIBLELINE, 0, 0);

            int charIndex = richTextBox.GetFirstCharIndexFromLine(firstVisibleLine);
            int y = richTextBox.GetPositionFromCharIndex(charIndex).Y;

            int totalLines = richTextBox.GetLineFromCharIndex(richTextBox.TextLength) + 1;
            for (int i = firstVisibleLine; i < totalLines; i++)
            {
                int index = richTextBox.GetFirstCharIndexFromLine(i);
                Point position = richTextBox.GetPositionFromCharIndex(index);

                // Dibujar solo si es visible
                if (position.Y - richTextBox.GetPositionFromCharIndex(charIndex).Y > panel.Height)
                    break;

                e.Graphics.DrawString((i + 1).ToString(), richTextBox.Font, pincelLineas, new PointF(0, position.Y - y));
            }
        }
    }


    private void ResaltarPalabrasReservadas(RichTextBox richTextBox)
    {
        string[] palabrasReservadas = {
            "ini","fin","imp","lee","cad","ent","dec","log","si","sino",
            "vdd","fal","rom","cic","des","has","inc","nul","lim","imprp"
        };

        Font normal = new Font("Orbitron", 10);
        Font negrita = new Font(normal, FontStyle.Bold);
        Color defaultColor = Color.Black;
        Color colorReservada = Color.Blue;
        Color colorComentario = Color.DarkGreen;
        Color colorCadena = Color.DarkRed;

        int posActual = richTextBox.SelectionStart;
        int lengthActual = richTextBox.SelectionLength;

        richTextBox.SuspendLayout();

        // Resetear formato
        richTextBox.SelectAll();
        richTextBox.SelectionColor = defaultColor;
        richTextBox.SelectionFont = normal;

        // Comentarios
        foreach (Match m in Regex.Matches(richTextBox.Text, @"#.*?#"))
        {
            richTextBox.Select(m.Index, m.Length);
            richTextBox.SelectionColor = colorComentario;
        }

        // Cadenas
        foreach (Match m in Regex.Matches(richTextBox.Text, @"""[^""]*"""))
        {
            richTextBox.Select(m.Index, m.Length);
            richTextBox.SelectionColor = colorCadena;
        }

        // Palabras reservadas
        foreach (string palabra in palabrasReservadas)
        {
            foreach (Match match in Regex.Matches(richTextBox.Text, $@"\b{palabra}\b"))
            {
                // Verifica que no esté dentro de un comentario o cadena
                bool enBloque = false;
                foreach (Match bloque in Regex.Matches(richTextBox.Text, @"#.*?#|""[^""]*"""))
                {
                    if (match.Index >= bloque.Index && match.Index < bloque.Index + bloque.Length)
                    {
                        enBloque = true;
                        break;
                    }
                }

                if (!enBloque)
                {
                    richTextBox.Select(match.Index, match.Length);
                    richTextBox.SelectionColor = colorReservada;
                    richTextBox.SelectionFont = negrita;
                }
            }
        }

        // Restaurar selección del usuario
        richTextBox.SelectionStart = posActual;
        richTextBox.SelectionLength = lengthActual;
        richTextBox.SelectionColor = defaultColor;
        richTextBox.SelectionFont = normal;

        richTextBox.ResumeLayout();
    }
}

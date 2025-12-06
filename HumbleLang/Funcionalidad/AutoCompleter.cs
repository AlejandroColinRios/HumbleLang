using Microsoft.VisualBasic.Logging;

public class AutoCompleter
{
    private RichTextBox editor;
    private ListBox suggestionBox;
    private List<string> keywords;
    private bool isSelecting = false; // Bandera nueva

    public AutoCompleter(RichTextBox editor, Control parent)
    {
        this.editor = editor;
        this.suggestionBox = new ListBox();
        this.keywords = new List<string> { "ini", "fin", "imp", "lee", "cad", "ent", "dec", "log", "si", "sino",
        "vdd", "fal", "rom", "cic", "des", "has", "inc", "nul", "lim", "imprp" };

        suggestionBox.Visible = false;
        suggestionBox.Width = 100;
        suggestionBox.Height = 100;
        suggestionBox.SelectedIndexChanged += SuggestionBox_SelectedIndexChanged;
        suggestionBox.MouseClick += (s, e) => { isSelecting = true; }; // <-- Activar al hacer click
        suggestionBox.KeyDown += SuggestionBox_KeyDown; // <-- Soportar Enter
        parent.Controls.Add(suggestionBox);

        editor.KeyUp += Editor_KeyUp;
    }

    private void Editor_KeyUp(object sender, KeyEventArgs e)
    {
        string lastWord = GetLastWord(editor.Text);
        var suggestions = keywords.Where(x => x.StartsWith(lastWord)).ToList();

        if (suggestions.Any() && lastWord.Length > 0)
        {
            isSelecting = false; // <-- Importante: al llenar sugerencias, no seleccionar

            suggestionBox.DataSource = suggestions;

            Point caretPos = editor.GetPositionFromCharIndex(editor.SelectionStart);
            Point screenPos = editor.PointToScreen(caretPos);
            Point formPos = editor.FindForm().PointToClient(screenPos);

            formPos.Y += (int)(editor.Font.Height * 1.5);

            suggestionBox.Location = formPos;
            suggestionBox.BringToFront();
            suggestionBox.Visible = true;
        }
        else
        {
            suggestionBox.Visible = false;
        }
    }

    private string GetLastWord(string text)
    {
        var parts = text.Split(' ', '\n', '\r');
        return parts.LastOrDefault() ?? "";
    }

    private void SuggestionBox_SelectedIndexChanged(object sender, EventArgs e)
    {
        // No hacer nada si no está seleccionando explícitamente
        if (!isSelecting) return;

        {
            if (suggestionBox.SelectedItem != null)
            {
                InsertSuggestion(suggestionBox.SelectedItem.ToString());
            }
        };

    }

    private void SuggestionBox_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.KeyCode == Keys.Enter && suggestionBox.SelectedItem != null)
        {
            isSelecting = true;
            InsertSuggestion(suggestionBox.SelectedItem.ToString());
            e.Handled = true;
        }
        else if (e.KeyCode == Keys.Escape)
        {
            suggestionBox.Visible = false;
            e.Handled = true;
        }
    }

    private void InsertSuggestion(string word)
    {
        int pos = editor.SelectionStart;
        string text = editor.Text;

        // Encuentra el inicio de la línea actual
        int lineIndex = editor.GetLineFromCharIndex(pos);
        int lineStart = editor.GetFirstCharIndexFromLine(lineIndex);
        int lengthToCursor = pos - lineStart;

        string lineText = text.Substring(lineStart, lengthToCursor);
        int lastWordStart = lineText.LastIndexOfAny(new char[] { ' ', '\t' }) + 1;

        if (lastWordStart < 0) lastWordStart = 0;

        editor.Select(lineStart + lastWordStart, pos - (lineStart + lastWordStart));
        editor.SelectedText = word + " ";
        editor.SelectionStart = lineStart + lastWordStart + word.Length + 1; // Mueve el cursor al final del autocompletado

        suggestionBox.Visible = false;
    }

}

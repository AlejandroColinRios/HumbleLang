using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;
using HumbleLang.Funcionalidad;

namespace HumbleLang
{

    public partial class humbleLang : Form
    {
        private EditorDeCodigo editor;
        private AutoCompleter autoCompleter;
        int cantErrores = 0;

        private int ultimaPosScroll = -1;

        private const int WM_VSCROLL = 0x115;
        private const int SB_THUMBPOSITION = 4;

        [DllImport("user32.dll")]
        private static extern int GetScrollPos(IntPtr hWnd, int nBar);

        [DllImport("user32.dll")]
        private static extern int SetScrollPos(IntPtr hWnd, int nBar, int nPos, bool bRedraw);

        private const int SB_VERT = 1;

        protected override void WndProc(ref Message m)
        {
            if (m.Msg == WM_VSCROLL)
            {
                int pos = GetScrollPos(rtEntrada.Handle, SB_VERT);
                panelErrores.AutoScrollPosition = new Point(0, pos);
                panelErrores.Refresh();
            }
            base.WndProc(ref m);
        }

        string conexion = "server=localhost;DataBase=AUTOMATAS;Trusted_Connection=True;";

        public humbleLang()
        {
            InitializeComponent();
            CargarMatriz();
            dtgSimbolos.RowHeadersVisible = false;
            dtgTokens.RowHeadersVisible = false;
            rtbTokens.Text = "     ";
            new TokenToolTipManager(rtEntrada);
            autoCompleter = new AutoCompleter(rtEntrada, this);
            editor = new EditorDeCodigo(rtEntrada, panelLineas, rtbTokens, panelLineasTokens);
            AnalizadorLL1 mianalizadorLL1 = new AnalizadorLL1();
            lblNumErrores.ForeColor = Color.DarkRed;
            string contenidoTokens = rtbTokens.Text;
        }

        private void RtEntrada_VScroll(object sender, EventArgs e)
        {
            int pos = GetScrollPos(rtEntrada.Handle, SB_VERT);
            panelErrores.AutoScrollPosition = new Point(0, pos);
            panelErrores.Refresh();
        }

        private Dictionary<string, Dictionary<string, string>> matriz = new();

        private void CargarMatriz()
        {
            matriz.Clear();

            using (SqlConnection conn = new SqlConnection(conexion))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand("SELECT * FROM MatrizTransicion", conn);
                SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                DataTable dt = new DataTable();
                adapter.Fill(dt);

                foreach (DataRow row in dt.Rows)
                {
                    string estado = row["N.T"].ToString();
                    Dictionary<string, string> transiciones = new();

                    foreach (DataColumn col in dt.Columns)
                    {
                        if (col.ColumnName != "N.T")
                        {
                            string simbolo = col.ColumnName;
                            string destino = row[col].ToString();
                            transiciones[simbolo] = destino;
                        }
                    }
                    matriz[estado] = transiciones;
                }
            }
        }

        // Dentro de HumbleLang/humbleLang.cs
        private void btnValidar_Click(object sender, EventArgs e)
        {
            LimpiarResaltado();
            editor = new EditorDeCodigo(rtEntrada, panelLineas, rtbTokens, panelLineasTokens);

            rtbTokens.Clear();
            panelErrores.Controls.Clear();
            cantErrores = 0;
            lblNumErrores.Text = "Numero de errores: " + cantErrores;

            StringBuilder archivoTokens = new StringBuilder();
            string entrada = rtEntrada.Text;
            if (string.IsNullOrWhiteSpace(entrada))
            {
                lblResultado.Text = "❌ Cadena vacía";
                lblResultado.ForeColor = Color.Red;
                return;
            }

            DataTable tablaTokens = new DataTable();
            tablaTokens.Columns.Add("Descripcion", typeof(string));
            tablaTokens.Columns.Add("Línea", typeof(int));

            DataTable tablaIdentificadores = new DataTable();
            tablaIdentificadores.Columns.Add("Numero", typeof(int));
            tablaIdentificadores.Columns.Add("Identificador", typeof(string));
            tablaIdentificadores.Columns.Add("Línea", typeof(int));

            // =========================================================
            // 1. LISTA MAESTRA DE TOKENS (TIPO, VALOR, LINEA)
            // Esta lista guardará "CN_ENTERA" y "10" juntos.
            // =========================================================
            List<(string Tipo, string Valor, int Linea)> tokensParaSintaxis = new List<(string, string, int)>();

            string estadoActual = "0";
            StringBuilder lexemaOriginal = new StringBuilder();
            StringBuilder lexemaParaEvaluar = new StringBuilder();
            int numeroLinea = 1;

            // --- INICIO DEL ANÁLISIS LÉXICO ---
            for (int i = 0; i < entrada.Length; i++)
            {
                char caracter = entrada[i];
                string simbolo = caracter.ToString();

                if (caracter == '\n')
                {
                    if (lexemaParaEvaluar.Length > 0)
                    {
                        // PASAMOS LA LISTA 'tokensParaSintaxis'
                        ValidarLexema(lexemaOriginal.ToString(), estadoActual, tablaTokens, archivoTokens, numeroLinea, tablaIdentificadores, tokensParaSintaxis);
                        lexemaOriginal.Clear();
                        lexemaParaEvaluar.Clear();
                        estadoActual = "0";
                    }
                    archivoTokens.AppendLine();
                    numeroLinea++;
                    continue;
                }
                else if (caracter == '\r') continue;

                string siguienteEstado = "";

                if (estadoActual == "121" || estadoActual == "124")
                {
                    lexemaOriginal.Append(caracter);
                    lexemaParaEvaluar.Append(caracter);

                    if (matriz[estadoActual].ContainsKey(simbolo))
                    {
                        siguienteEstado = matriz[estadoActual][simbolo];
                        estadoActual = siguienteEstado;
                        if (estadoActual == "123" || estadoActual == "126")
                        {
                            ValidarLexema(lexemaOriginal.ToString(), estadoActual, tablaTokens, archivoTokens, numeroLinea, tablaIdentificadores, tokensParaSintaxis);
                            lexemaOriginal.Clear();
                            lexemaParaEvaluar.Clear();
                            estadoActual = "0";
                        }
                    }
                    else
                    {
                        if (i == entrada.Length - 1)
                        {
                            ValidarLexema(lexemaOriginal.ToString(), estadoActual, tablaTokens, archivoTokens, numeroLinea, tablaIdentificadores, tokensParaSintaxis);
                            lexemaOriginal.Clear();
                            lexemaParaEvaluar.Clear();
                            estadoActual = "0";
                        }
                    }
                    continue;
                }

                lexemaOriginal.Append(caracter);

                bool esDelimitador = (caracter == ' ' || caracter == '\t' || caracter == ';');
                bool esFinDeCadena = (i == entrada.Length - 1);

                if (esDelimitador) simbolo = "DEL";
                else if (char.IsUpper(caracter)) simbolo = $"{caracter}_1";

                if (estadoActual == "0" && simbolo == "\"")
                {
                    estadoActual = "121";
                    lexemaParaEvaluar.Append(caracter);
                    continue;
                }

                if (estadoActual == "0" && simbolo == "#")
                {
                    estadoActual = "124";
                    lexemaParaEvaluar.Append(caracter);
                    continue;
                }

                if (!matriz.ContainsKey(estadoActual) || !matriz[estadoActual].ContainsKey(simbolo))
                {
                    if (lexemaParaEvaluar.Length > 0)
                    {
                        ValidarLexema(lexemaOriginal.ToString().Trim(), estadoActual, tablaTokens, archivoTokens, numeroLinea, tablaIdentificadores, tokensParaSintaxis);
                        lexemaOriginal.Clear();
                        lexemaParaEvaluar.Clear();
                        estadoActual = "0";
                    }
                    if (!esDelimitador)
                    {
                        lexemaOriginal.Append(caracter);
                        lexemaParaEvaluar.Append(caracter);
                        if (i == entrada.Length - 1)
                        {
                            ValidarLexema(lexemaOriginal.ToString(), estadoActual, tablaTokens, archivoTokens, numeroLinea, tablaIdentificadores, tokensParaSintaxis);
                        }
                    }
                    continue;
                }

                siguienteEstado = matriz[estadoActual][simbolo];
                estadoActual = siguienteEstado;
                lexemaParaEvaluar.Append(caracter);

                if ((esDelimitador || esFinDeCadena) && lexemaParaEvaluar.Length > 0)
                {
                    string lexema = lexemaParaEvaluar.ToString().Trim();
                    if (!string.IsNullOrWhiteSpace(lexema))
                    {
                        ValidarLexema(lexema, estadoActual, tablaTokens, archivoTokens, numeroLinea, tablaIdentificadores, tokensParaSintaxis);
                    }
                    lexemaOriginal.Clear();
                    lexemaParaEvaluar.Clear();
                    estadoActual = "0";
                }
            }

            if (lexemaParaEvaluar.Length > 0)
            {
                ValidarLexema(lexemaOriginal.ToString().Trim(), estadoActual, tablaTokens, archivoTokens, numeroLinea, tablaIdentificadores, tokensParaSintaxis);
            }

            rtbTokens.Text = archivoTokens.ToString();
            // --- FIN DEL ANÁLISIS LÉXICO ---


            // =========================================================================
            // 2. ANÁLISIS SINTÁCTICO (PARSER LL1)
            // Usamos directamente la lista 'tokensParaSintaxis' que llenamos arriba.
            // =========================================================================

            AnalizadorLL1 analizadorSintactico = new AnalizadorLL1();
            NodoAST arbol = analizadorSintactico.Analizar(tokensParaSintaxis);


            // =========================================================================
            // 3. CONVERSIÓN DE NOTACIÓN (Adaptada a la nueva lista)
            // =========================================================================

            // Necesitamos mapear la lista de 3 elementos a 2 para la conversión (si tu convertidor lo requiere así)
            // O usamos la lista directamente accediendo a .Tipo en lugar de .Token
            var listaParaNotacion = tokensParaSintaxis.Select(t => (Token: t.Tipo, Linea: t.Linea)).ToList();

            List<(string Token, int Linea)> tokensCondicion = new List<(string Token, int Linea)>();
            int startIndex = listaParaNotacion.FindIndex(t => t.Linea == 7 && t.Token == "CE6");

            if (startIndex != -1)
            {
                int balance = 0;
                for (int i = startIndex; i < listaParaNotacion.Count; i++)
                {
                    var token = listaParaNotacion[i];
                    tokensCondicion.Add(token);

                    if (token.Token == "CE6") balance++;
                    else if (token.Token == "CE7")
                    {
                        balance--;
                        if (balance == 0) break;
                    }
                }
            }

            var tokensAConvertir = tokensCondicion;
            StringBuilder conversionInfo = new StringBuilder();

            if (!tokensAConvertir.Any())
            {
                conversionInfo.AppendLine("--- ERROR: No se pudo aislar una expresión válida para la conversión (Línea 7). ---");
            }
            else
            {
                try
                {
                    var postfija = NotacionConvertidor.InfijaAPostfija(tokensAConvertir);
                    conversionInfo.AppendLine("--- Conversión de EXPRESIÓN (POSTFIJA) ---");
                    conversionInfo.AppendLine($"Original: {string.Join(" ", tokensAConvertir.Select(t => t.Token))}");
                    conversionInfo.AppendLine(string.Join(" ", postfija.Select(t => t.Token)));
                }
                catch (Exception ex) { conversionInfo.AppendLine($"Error Postfija: {ex.Message}"); }

                try
                {
                    var prefija = NotacionConvertidor.InfijaAPrefija(tokensAConvertir);
                    conversionInfo.AppendLine("\n--- Conversión de EXPRESIÓN (PREFIJA) ---");
                    conversionInfo.AppendLine(string.Join(" ", prefija.Select(t => t.Token)));
                }
                catch (Exception ex) { conversionInfo.AppendLine($"Error Prefija: {ex.Message}"); }
            }

            MessageBox.Show(conversionInfo.ToString(), "Conversiones de Notación", MessageBoxButtons.OK, MessageBoxIcon.Information);


            // =========================================================================
            // 4. GUARDADO DE AST
            // =========================================================================
            if (arbol != null)
            {
                string userProfile = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
                string downloadsPath = Path.Combine(userProfile, "Downloads");
                string filePath = Path.Combine(downloadsPath, "arbol_ast.txt");
                System.IO.File.WriteAllText(filePath, arbol.ImprimirArbol());
                MessageBox.Show($"Árbol AST guardado en: {filePath}", "Información");
            }

            // =========================================================================
            // 5. ANÁLISIS SEMÁNTICO
            // =========================================================================
            AnalizadorSemantico analizadorSemantico = new AnalizadorSemantico();
            analizadorSemantico.Analizar(arbol);

            // =========================================================================
            // 6. RESULTADO FINAL & INTÉRPRETE
            // =========================================================================
            if (analizadorSintactico.ErroresSintacticos.Count == 0 && analizadorSemantico.Errores.Count == 0)
            {
                lblResultado.Text = "✅ Cadena válida (sintaxis y semántica)";
                lblResultado.ForeColor = Color.Green;

                // --- INICIO DEL INTÉRPRETE ---
                try
                {
                    StringBuilder consolaSalida = new StringBuilder();
                    consolaSalida.AppendLine("--- INICIO DE EJECUCIÓN (HumbleLang) ---");
                    consolaSalida.AppendLine("");

                    // Instancia del intérprete
                    Interprete miInterprete = new Interprete();

                    // Ejecutar: cada vez que el código haga 'imp', se guardará en consolaSalida
                    miInterprete.Interpretar(arbol, (texto) =>
                    {
                        consolaSalida.AppendLine("> " + texto);
                    });

                    consolaSalida.AppendLine("");
                    consolaSalida.AppendLine("--- FIN DE EJECUCIÓN ---");

                    // Mostrar el resultado de la ejecución
                    MessageBox.Show(consolaSalida.ToString(), "Ejecución del Programa", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error crítico durante la ejecución: " + ex.Message, "Error Runtime", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                // --- FIN DEL INTÉRPRETE ---
            }
            else if (analizadorSintactico.ErroresSintacticos.Count > 0)
            {
                lblResultado.Text = "❌ Error de sintaxis";
                lblResultado.ForeColor = Color.Red;
            }
            else if (analizadorSemantico.Errores.Count > 0)
            {
                lblResultado.Text = "❌ Error semántico";
                lblResultado.ForeColor = Color.Red;
            }

            // =========================================================================
            // 7. GENERACIÓN DE REPORTES EN FORMULARIO
            // =========================================================================
            StringBuilder pasos = new StringBuilder();

            if (conversionInfo.Length > 0)
            {
                pasos.AppendLine("==============================================");
                pasos.AppendLine("CONVERSIONES DE NOTACIÓN");
                pasos.AppendLine(conversionInfo.ToString());
            }

            foreach (string paso in analizadorSintactico.RegistroPasos)
            {
                pasos.AppendLine(paso);
            }

            if (analizadorSemantico.CuadruplosGenerados.Any())
            {
                pasos.AppendLine("\n==============================================");
                pasos.AppendLine("CÓDIGO INTERMEDIO: CUÁDRUPLOS");
                pasos.AppendLine("(Operador, Arg1, Arg2, Resultado)");
                int quadIndex = 1;
                foreach (var cuadruplo in analizadorSemantico.CuadruplosGenerados)
                {
                    pasos.AppendLine($"{quadIndex++}: {cuadruplo.ToString()}");
                }

                pasos.AppendLine("\n==============================================");
                pasos.AppendLine(analizadorSemantico.ImprimirTriplos());
            }

            // Mostrar FormResultados con el string de tokens (solo tipos para visualización)
            string tokensVisuales = string.Join(Environment.NewLine, tokensParaSintaxis.Select(t => t.Tipo));

            FormResultados resultados = new FormResultados(
                tokensVisuales,
                pasos.ToString()
            );

            resultados.CargarErroresSintacticos(analizadorSintactico.ErroresSintacticos);
            foreach (var err in analizadorSemantico.Errores)
            {
                resultados.CargarError("Semántico", err.Mensaje, err.Linea);
                MostrarAdvertencia(err.Linea, "[Semántico] " + err.Mensaje);
            }

            resultados.Show();

            ConfigurarDataGridView();
            dtgTokens.DataSource = tablaTokens;
            dtgTokens.ClearSelection();
            dtgSimbolos.DataSource = tablaIdentificadores;
            dtgSimbolos.ClearSelection();

            editor.DibujarLineasPara(rtEntrada, panelLineas);
        }
        // Se añade el parámetro 'tokensSalida' al final para guardar la información real (Valor)
        private void ValidarLexema(string lexema, string estadoFinal, DataTable tablaTokens, StringBuilder archivoTokens, int linea, DataTable tablaIdentificadores, List<(string Tipo, string Valor, int Linea)> tokensSalida)
        {
            string tipoToken = "";
            if (string.IsNullOrWhiteSpace(lexema))
                return;

            using (SqlConnection conn = new SqlConnection(conexion))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand("SELECT ACEP FROM MatrizTransicion WHERE [N.T] = @estado", conn);
                cmd.Parameters.AddWithValue("@estado", estadoFinal);
                object result = cmd.ExecuteScalar();
                tipoToken = result != null ? result.ToString() : "";
            }

            // Mapeo de tokens a nombres legibles (Igual que antes)
            Dictionary<string, string> mapeoTokens = new Dictionary<string, string>()
    {
        {"1", "CN_ENTERA"}, {"10", "CN_REALES"}, // OJO: CN_DECIMAL -> CN_REALES para coincidir con tu Parser
        {"11", "IDEN"}, {"13", "CADE"},
        {"14", "OPA1"},
        {"15", "OPRE1"}, {"16", "OPRE2"}, {"17", "OPRE3"}, {"18", "OPRE4"}, {"19", "OPRE5"}, {"20", "OPREL1"}, {"21", "OPREL2"}, {"22", "OPREL3"}, {"23", "OPREL4"}, {"24", "OPREL5"}, {"25", "OPREL6"},
        {"26", "OPLO1"}, {"27", "OPLO2"}, {"28", "OPLO3"},
        {"30", "CE6"}, {"31", "CE7"}, {"32", "CE8"}, {"33", "CE9"},
        {"35", "PR_ENT"}, {"36", "PR_DEC"}, {"37", "PR_CAD"}, {"38", "PR_LOG"},
        {"39", "PR_SI"}, {"40", "PR_SINO"}, {"41", "PR_IMP"}, {"42", "PR_IMPRP"},
        {"43", "PR_INI"}, {"44", "PR_FIN"}, {"45", "PR_LIM"}, {"46", "PR_LEE"},
        {"47", "PR_CIC"}, {"48", "PR_DES"}, {"49", "PR_HAS"}, {"50", "PR_INC"},
        {"51", "PR_ROM"}, {"52", "PR_VDD"}, {"53", "PR_FAL"}, {"54", "PR_NEG"},
        {"126", "COMEN"}
    };

            if (mapeoTokens.ContainsKey(tipoToken))
            {
                tipoToken = mapeoTokens[tipoToken];
            }

            if (tipoToken == "COMEN") return; // Ignorar comentario

            Dictionary<string, string> errores = new Dictionary<string, string>()
    {
        {"141", "ERROR: Identificador no válido"},
        {"142", "ERROR: Comentario no válido"},
        {"143", "ERROR: Cadena no válida"},
        {"144", "ERROR: Dígito entero no válido"},
        {"145", "ERROR: Decimal no válido"},
        {"146", "ERROR: Exponente no válido"},
        {"147", "ERROR: Operador aritmético no válido"},
        {"148", "ERROR: Operador lógico no válido"},
        {"149", "ERROR: Operador relacional no válido"},
        {"150", "ERROR: Operador de asignación no válido"},
        {"151", "ERROR: Carácter no válido"}
    };

            if (errores.ContainsKey(tipoToken))
            {
                cantErrores++;
                lblNumErrores.Text = ("Cantidad de errores: " + cantErrores);
                tablaTokens.Rows.Add($"❌ {errores[tipoToken]}", linea);
                MostrarAdvertencia(linea, errores[tipoToken]);
                ResaltarErrorEnTexto(lexema, linea);
            }
            else if (!string.IsNullOrEmpty(tipoToken))
            {
                archivoTokens.Append(tipoToken + " ");

                // =========================================================
                // ¡CORRECCIÓN CRÍTICA! 
                // Guardamos el lexema real ("10") junto con el tipo ("CN_ENTERA")
                // =========================================================
                tokensSalida.Add((tipoToken, lexema, linea));

                if (tipoToken == "IDEN" && lexema.Length > 0)
                {
                    AgregarIdentificador(lexema, linea, tablaIdentificadores);
                }
            }
            else
            {
                cantErrores++;
                lblNumErrores.Text = ("Cantidad de errores: " + cantErrores);
                tablaTokens.Rows.Add(lexema, "❌ Token no reconocido", linea);
                MostrarAdvertencia(linea, "Token no reconocido");
            }
        }
        private void ResaltarErrorEnTexto(string lexema, int linea)
        {
            int pos = ObtenerPosicionEnTexto(rtEntrada.Text, lexema, linea);
            if (pos != -1)
            {
                rtEntrada.Select(pos, lexema.Length);
                rtEntrada.SelectionBackColor = Color.LightPink;
                rtEntrada.SelectionColor = Color.DarkRed;
                rtEntrada.DeselectAll();
            }
        }
        private int ObtenerPosicionEnTexto(string texto, string lexema, int linea)
        {
            string[] lineas = texto.Split('\n');
            if (linea - 1 < 0 || linea - 1 >= lineas.Length)
                return -1;

            int offset = 0;
            for (int i = 0; i < linea - 1; i++)
                offset += lineas[i].Length + 1;

            int indexEnLinea = lineas[linea - 1].IndexOf(lexema);
            return indexEnLinea != -1 ? offset + indexEnLinea : -1;
        }
        private void LimpiarResaltado()
        {
            int pos = rtEntrada.SelectionStart;
            int length = rtEntrada.TextLength;

            rtEntrada.SelectAll();
            rtEntrada.SelectionBackColor = Color.White;
            rtEntrada.DeselectAll();

            rtEntrada.SelectionStart = pos;
            rtEntrada.SelectionLength = 0;
        }

        private void MostrarAdvertencia(int linea, string mensaje)
        {
            PictureBox pictureBox = new PictureBox();
            pictureBox.Image = SystemIcons.Warning.ToBitmap();
            pictureBox.SizeMode = PictureBoxSizeMode.Zoom;

            int altoLinea = rtEntrada.Font.Height;
            pictureBox.Size = new Size(16, altoLinea);
            pictureBox.Location = new Point(2, (linea - 1) * rtEntrada.Font.Height);
            pictureBox.Anchor = AnchorStyles.Top | AnchorStyles.Left;

            panelErrores.Controls.Add(pictureBox);

            ToolTip toolTip = new ToolTip();
            toolTip.SetToolTip(pictureBox, mensaje);
        }

        private void AgregarIdentificador(string lexema, int linea, DataTable tablaIdentificadores)
        {
            foreach (DataRow columna in tablaIdentificadores.Rows)
            {
                if (columna["Identificador"].ToString().Trim() == lexema.Trim())
                    return;
            }

            int numero = tablaIdentificadores.Rows.Count + 1;
            tablaIdentificadores.Rows.Add(numero, lexema, linea);
        }

        private void ConfigurarDataGridView()
        {
            dtgTokens.EnableHeadersVisualStyles = false;
            dtgSimbolos.EnableHeadersVisualStyles = false;

            dtgTokens.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dtgSimbolos.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;

            dtgTokens.ColumnHeadersDefaultCellStyle.BackColor = Color.Navy;
            dtgTokens.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;

            dtgSimbolos.ColumnHeadersDefaultCellStyle.BackColor = Color.Navy;
            dtgSimbolos.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;

            dtgTokens.DefaultCellStyle.BackColor = Color.White;
            dtgTokens.AlternatingRowsDefaultCellStyle.BackColor = Color.LightGray;

            dtgSimbolos.DefaultCellStyle.BackColor = Color.White;
            dtgSimbolos.AlternatingRowsDefaultCellStyle.BackColor = Color.LightGray;

            dtgTokens.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dtgTokens.DefaultCellStyle.SelectionBackColor = Color.FromArgb(0, 192, 0);
            dtgTokens.DefaultCellStyle.SelectionForeColor = Color.Black;

            dtgSimbolos.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dtgSimbolos.DefaultCellStyle.SelectionBackColor = Color.FromArgb(0, 192, 0);
            dtgSimbolos.DefaultCellStyle.SelectionForeColor = Color.Black;

            dtgTokens.RowHeadersVisible = false;
            dtgSimbolos.RowHeadersVisible = false;

            dtgTokens.Cursor = Cursors.Hand;
            dtgSimbolos.Cursor = Cursors.Hand;

            dtgTokens.CellFormatting += (sender, e) =>
            {
                if (e.ColumnIndex == dtgTokens.Columns["Descripcion"].Index && e.Value != null)
                {
                    if (e.Value.ToString() == "✅ Válido")
                    {
                        e.CellStyle.BackColor = Color.LightGreen;
                        e.CellStyle.ForeColor = Color.DarkGreen;
                    }
                    else
                    {
                        e.CellStyle.BackColor = Color.LightPink;
                        e.CellStyle.ForeColor = Color.DarkRed;
                    }
                }
            };
        }

        private void btnGuardar_Click(object sender, EventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "Archivos de texto (*.txt)|*.txt|Todos los archivos (*.*)|*.*";
            saveFileDialog.Title = "Guardar código fuente";
            saveFileDialog.DefaultExt = "txt";

            if (saveFileDialog.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    File.WriteAllText(saveFileDialog.FileName, rtEntrada.Text);

                    string tokensFilePath = Path.Combine(
                      Path.GetDirectoryName(saveFileDialog.FileName),
                      Path.GetFileNameWithoutExtension(saveFileDialog.FileName) + "_tokens.txt");

                    File.WriteAllText(tokensFilePath, rtbTokens.Text);

                    MessageBox.Show("Archivo guardado correctamente", "Éxito",
                            MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error al guardar el archivo: {ex.Message}", "Error",
                            MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void btnCargar_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Archivos de texto (*.txt)|*.txt|Todos los archivos (*.*)|*.*";
            openFileDialog.Title = "Cargar código fuente";

            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    rtEntrada.Text = File.ReadAllText(openFileDialog.FileName);
                    rtbTokens.Clear();

                    if (dtgTokens.DataSource != null)
                    {
                        ((DataTable)dtgTokens.DataSource).Rows.Clear();
                        ((DataTable)dtgSimbolos.DataSource).Rows.Clear();
                    }

                    lblResultado.Text = "Archivo cargado correctamente";
                    lblResultado.ForeColor = Color.DarkBlue;
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error al cargar el archivo: {ex.Message}", "Error",
                            MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            MessageBox.Show(
            "HumbleLang\nVersión 1.3a\n\nCreadores:\nAlejandro Colin Rios #21100182\nJosé Alejandro Cumpian Ramos #21100186\nLuis Ángel Rendón Arrazola #20100253",
            "Acerca de HumbleLang",
            MessageBoxButtons.OK,
            MessageBoxIcon.Information
      );
        }

        private void btnBuscarSimbolo_Click(object sender, EventArgs e)
        {
            string busqueda = txtBuscarSimbolo.Text + " ";
            dtgSimbolos.ClearSelection();

            if (string.IsNullOrWhiteSpace(busqueda))
            {
                MessageBox.Show("Ingresa un identificador para buscar.", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            bool encontrado = false;

            dtgSimbolos.EnableHeadersVisualStyles = false;
            dtgSimbolos.ColumnHeadersDefaultCellStyle.BackColor = Color.DarkBlue;
            dtgSimbolos.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;

            dtgSimbolos.DefaultCellStyle.SelectionBackColor = Color.FromArgb(0, 192, 0);
            dtgSimbolos.DefaultCellStyle.SelectionForeColor = Color.Black;

            dtgSimbolos.AlternatingRowsDefaultCellStyle.BackColor = Color.LightGray;
            dtgSimbolos.DefaultCellStyle.BackColor = Color.White;

            dtgSimbolos.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dtgSimbolos.RowHeadersVisible = false;
            dtgSimbolos.Cursor = Cursors.Hand;

            for (int i = 0; i < dtgSimbolos.Rows.Count; i++)
            {
                var fila = dtgSimbolos.Rows[i];
                fila.Selected = false;

                Color bgColor = (i % 2 == 0) ? Color.White : Color.LightGray;
                fila.DefaultCellStyle.BackColor = bgColor;
                fila.DefaultCellStyle.ForeColor = Color.Black;

                fila.Cells[0].Style.BackColor = bgColor;
                fila.Cells[0].Style.ForeColor = Color.Black;
            }

            foreach (DataGridViewRow fila in dtgSimbolos.Rows)
            {
                if (fila.Cells["Identificador"].Value?.ToString().Equals(busqueda, StringComparison.OrdinalIgnoreCase) == true)
                {
                    fila.Selected = true;
                    fila.DefaultCellStyle.BackColor = Color.FromArgb(0, 192, 0);
                    fila.DefaultCellStyle.ForeColor = Color.White;

                    fila.Cells[0].Style.BackColor = Color.FromArgb(0, 192, 0);
                    fila.Cells[0].Style.ForeColor = Color.White;

                    dtgSimbolos.FirstDisplayedScrollingRowIndex = fila.Index;
                    encontrado = true;
                    break;
                }
            }

            if (!encontrado)
            {
                MessageBox.Show($"El identificador '{busqueda}' no fue encontrado.", "Resultado de búsqueda", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void TimerScroll_Tick(object sender, EventArgs e)
        {
            int posActual = GetScrollPos(rtEntrada.Handle, SB_VERT);
            if (posActual != ultimaPosScroll)
            {
                panelErrores.AutoScrollPosition = new Point(0, posActual);
                panelErrores.Refresh();
                ultimaPosScroll = posActual;
            }
        }

        private void humbleLang_Load(object sender, EventArgs e)
        {
        }

        private void rtEntrada_VScroll_1(object sender, EventArgs e)
        {
            int scrollPos = GetScrollPos(rtEntrada.Handle, SB_VERT);
            panelErrores.AutoScrollPosition = new Point(0, scrollPos);
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void lblResultado_Click(object sender, EventArgs e)
        {
        }

        private void rtbTokens_TextChanged(object sender, EventArgs e)
        {
            rtbTokens.SelectionStart = rtbTokens.Text.Length;
            rtbTokens.ScrollToCaret();
        }

        private void rtbprueba_TextChanged(object sender, EventArgs e)
        {
        }
    }
}

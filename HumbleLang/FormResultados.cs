using System;
using System.Collections.Generic;
using System.Data;
using System.Windows.Forms;

namespace HumbleLang
{
    public partial class FormResultados : Form
    {
        private DataTable tablaErrores;

        public FormResultados(string tokensEsperados, string procesoAnalisis)
        {
            InitializeComponent();

            rtbTokensEsperados.Text = tokensEsperados;
            rtbProcesoAnalisis.Text = procesoAnalisis;

            // Initialize the DataTable and set up the columns
            tablaErrores = new DataTable();
            tablaErrores.Columns.Add("Tipo", typeof(string));
            tablaErrores.Columns.Add("Descripcion", typeof(string));
            tablaErrores.Columns.Add("Linea", typeof(int));

            // Set the DataGridView's DataSource once
            dtgErroresSintacticos.DataSource = tablaErrores;
            dtgErroresSintacticos.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dtgErroresSintacticos.AutoGenerateColumns = true; // Use auto-generated columns for simplicity
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        public void CargarErroresSintacticos(List<(string Mensaje, int Linea)> errores)
        {
            // Add rows to the DataTable, not the DataGridView
            foreach (var error in errores)
            {
                tablaErrores.Rows.Add("Sintáctico", error.Mensaje, error.Linea);
            }
        }

        public void CargarError(string tipo, string mensaje, int linea)
        {
            // Add rows to the DataTable, not the DataGridView
            tablaErrores.Rows.Add(tipo, mensaje, linea);
        }
    }
}
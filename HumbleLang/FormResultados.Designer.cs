namespace HumbleLang
{
    partial class FormResultados
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormResultados));
            pictureBox2 = new PictureBox();
            pictureBox1 = new PictureBox();
            panel1 = new Panel();
            lblResultados = new Label();
            grbTokens = new GroupBox();
            rtbTokensEsperados = new RichTextBox();
            grbProcesoAnalisis = new GroupBox();
            rtbProcesoAnalisis = new RichTextBox();
            dtgErroresSintacticos = new DataGridView();
            ((System.ComponentModel.ISupportInitialize)pictureBox2).BeginInit();
            ((System.ComponentModel.ISupportInitialize)pictureBox1).BeginInit();
            panel1.SuspendLayout();
            grbTokens.SuspendLayout();
            grbProcesoAnalisis.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dtgErroresSintacticos).BeginInit();
            SuspendLayout();
            // 
            // pictureBox2
            // 
            resources.ApplyResources(pictureBox2, "pictureBox2");
            pictureBox2.Name = "pictureBox2";
            pictureBox2.TabStop = false;
            // 
            // pictureBox1
            // 
            pictureBox1.Image = Properties.Resources.remove;
            resources.ApplyResources(pictureBox1, "pictureBox1");
            pictureBox1.Name = "pictureBox1";
            pictureBox1.TabStop = false;
            pictureBox1.Click += pictureBox1_Click;
            // 
            // panel1
            // 
            panel1.Controls.Add(lblResultados);
            panel1.Controls.Add(pictureBox2);
            panel1.Controls.Add(pictureBox1);
            resources.ApplyResources(panel1, "panel1");
            panel1.Name = "panel1";
            // 
            // lblResultados
            // 
            resources.ApplyResources(lblResultados, "lblResultados");
            lblResultados.Name = "lblResultados";
            // 
            // grbTokens
            // 
            grbTokens.Controls.Add(rtbTokensEsperados);
            resources.ApplyResources(grbTokens, "grbTokens");
            grbTokens.Name = "grbTokens";
            grbTokens.TabStop = false;
            // 
            // rtbTokensEsperados
            // 
            resources.ApplyResources(rtbTokensEsperados, "rtbTokensEsperados");
            rtbTokensEsperados.Name = "rtbTokensEsperados";
            // 
            // grbProcesoAnalisis
            // 
            grbProcesoAnalisis.Controls.Add(rtbProcesoAnalisis);
            resources.ApplyResources(grbProcesoAnalisis, "grbProcesoAnalisis");
            grbProcesoAnalisis.Name = "grbProcesoAnalisis";
            grbProcesoAnalisis.TabStop = false;
            // 
            // rtbProcesoAnalisis
            // 
            resources.ApplyResources(rtbProcesoAnalisis, "rtbProcesoAnalisis");
            rtbProcesoAnalisis.Name = "rtbProcesoAnalisis";
            // 
            // dtgErroresSintacticos
            // 
            dtgErroresSintacticos.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            resources.ApplyResources(dtgErroresSintacticos, "dtgErroresSintacticos");
            dtgErroresSintacticos.Name = "dtgErroresSintacticos";
            // 
            // FormResultados
            // 
            resources.ApplyResources(this, "$this");
            AutoScaleMode = AutoScaleMode.Font;
            AutoValidate = AutoValidate.EnablePreventFocusChange;
            ControlBox = false;
            Controls.Add(dtgErroresSintacticos);
            Controls.Add(grbProcesoAnalisis);
            Controls.Add(grbTokens);
            Controls.Add(panel1);
            FormBorderStyle = FormBorderStyle.FixedSingle;
            Name = "FormResultados";
            ((System.ComponentModel.ISupportInitialize)pictureBox2).EndInit();
            ((System.ComponentModel.ISupportInitialize)pictureBox1).EndInit();
            panel1.ResumeLayout(false);
            panel1.PerformLayout();
            grbTokens.ResumeLayout(false);
            grbProcesoAnalisis.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)dtgErroresSintacticos).EndInit();
            ResumeLayout(false);
        }

        #endregion

        private PictureBox pictureBox2;
        private PictureBox pictureBox1;
        private Panel panel1;
        private Label lblResultados;
        private GroupBox grbTokens;
        private RichTextBox rtbTokensEsperados;
        private GroupBox grbProcesoAnalisis;
        private RichTextBox rtbProcesoAnalisis;
        public DataGridView dtgErroresSintacticos;
    }
}
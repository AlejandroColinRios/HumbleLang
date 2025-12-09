namespace HumbleLang
{
    partial class humbleLang
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
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
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(humbleLang));
            btnValidar = new Button();
            lblResultado = new Label();
            rtEntrada = new RichTextBox();
            panelLineas = new Panel();
            panelLineasTokens = new Panel();
            rtbTokens = new RichTextBox();
            dtgTokens = new DataGridView();
            btnCargar = new Button();
            btnGuardar = new Button();
            label1 = new Label();
            label2 = new Label();
            label3 = new Label();
            label4 = new Label();
            dtgSimbolos = new DataGridView();
            linkLabel1 = new LinkLabel();
            panel1 = new Panel();
            pictureBox1 = new PictureBox();
            pictureBox2 = new PictureBox();
            lblNumErrores = new Label();
            txtBuscarSimbolo = new TextBox();
            btnBuscarSimbolo = new Button();
            panelErrores = new Panel();
            btnGenerarASM = new Button();
            ((System.ComponentModel.ISupportInitialize)dtgTokens).BeginInit();
            ((System.ComponentModel.ISupportInitialize)dtgSimbolos).BeginInit();
            panel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)pictureBox1).BeginInit();
            ((System.ComponentModel.ISupportInitialize)pictureBox2).BeginInit();
            SuspendLayout();
            // 
            // btnValidar
            // 
            btnValidar.Location = new Point(287, 512);
            btnValidar.Name = "btnValidar";
            btnValidar.Size = new Size(109, 23);
            btnValidar.TabIndex = 0;
            btnValidar.Text = "Validar";
            btnValidar.UseVisualStyleBackColor = true;
            btnValidar.Click += btnValidar_Click;
            // 
            // lblResultado
            // 
            lblResultado.AutoSize = true;
            lblResultado.Location = new Point(402, 512);
            lblResultado.Name = "lblResultado";
            lblResultado.Size = new Size(0, 15);
            lblResultado.TabIndex = 3;
            lblResultado.Click += lblResultado_Click;
            // 
            // rtEntrada
            // 
            rtEntrada.AcceptsTab = true;
            rtEntrada.Location = new Point(57, 84);
            rtEntrada.Name = "rtEntrada";
            rtEntrada.Size = new Size(457, 424);
            rtEntrada.TabIndex = 5;
            rtEntrada.Text = "";
            rtEntrada.WordWrap = false;
            rtEntrada.VScroll += rtEntrada_VScroll_1;
            // 
            // panelLineas
            // 
            panelLineas.Location = new Point(25, 84);
            panelLineas.Name = "panelLineas";
            panelLineas.Size = new Size(26, 424);
            panelLineas.TabIndex = 6;
            // 
            // panelLineasTokens
            // 
            panelLineasTokens.Location = new Point(522, 84);
            panelLineasTokens.Name = "panelLineasTokens";
            panelLineasTokens.Size = new Size(26, 424);
            panelLineasTokens.TabIndex = 7;
            // 
            // rtbTokens
            // 
            rtbTokens.BorderStyle = BorderStyle.FixedSingle;
            rtbTokens.EnableAutoDragDrop = true;
            rtbTokens.Location = new Point(554, 84);
            rtbTokens.Name = "rtbTokens";
            rtbTokens.Size = new Size(673, 424);
            rtbTokens.TabIndex = 9;
            rtbTokens.Text = "";
            rtbTokens.WordWrap = false;
            rtbTokens.TextChanged += rtbTokens_TextChanged;
            // 
            // dtgTokens
            // 
            dtgTokens.AllowUserToAddRows = false;
            dtgTokens.AllowUserToDeleteRows = false;
            dtgTokens.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dtgTokens.Location = new Point(57, 562);
            dtgTokens.Name = "dtgTokens";
            dtgTokens.ReadOnly = true;
            dtgTokens.Size = new Size(457, 424);
            dtgTokens.TabIndex = 8;
            // 
            // btnCargar
            // 
            btnCargar.Location = new Point(57, 511);
            btnCargar.Name = "btnCargar";
            btnCargar.Size = new Size(109, 23);
            btnCargar.TabIndex = 10;
            btnCargar.Text = "Cargar codigo";
            btnCargar.UseVisualStyleBackColor = true;
            btnCargar.Click += btnCargar_Click;
            // 
            // btnGuardar
            // 
            btnGuardar.Location = new Point(172, 511);
            btnGuardar.Name = "btnGuardar";
            btnGuardar.Size = new Size(109, 24);
            btnGuardar.TabIndex = 11;
            btnGuardar.Text = "Guardar Codigo";
            btnGuardar.UseVisualStyleBackColor = true;
            btnGuardar.Click += btnGuardar_Click;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.BackColor = Color.White;
            label1.Font = new Font("Consolas", 9F, FontStyle.Bold, GraphicsUnit.Point, 0);
            label1.ForeColor = Color.Black;
            label1.Location = new Point(63, 65);
            label1.Name = "label1";
            label1.Size = new Size(98, 14);
            label1.TabIndex = 13;
            label1.Text = "Codigo Fuente";
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Font = new Font("Consolas", 9F, FontStyle.Bold, GraphicsUnit.Point, 0);
            label2.Location = new Point(557, 66);
            label2.Name = "label2";
            label2.Size = new Size(112, 14);
            label2.TabIndex = 14;
            label2.Text = "Lista de Tokens";
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Font = new Font("Consolas", 9F, FontStyle.Bold, GraphicsUnit.Point, 0);
            label3.Location = new Point(57, 544);
            label3.Name = "label3";
            label3.Size = new Size(119, 14);
            label3.TabIndex = 15;
            label3.Text = "Lista de errores";
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.Font = new Font("Consolas", 9F, FontStyle.Bold, GraphicsUnit.Point, 0);
            label4.Location = new Point(551, 544);
            label4.Name = "label4";
            label4.Size = new Size(126, 14);
            label4.TabIndex = 16;
            label4.Text = "Tabla de simbolos";
            // 
            // dtgSimbolos
            // 
            dtgSimbolos.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dtgSimbolos.Enabled = false;
            dtgSimbolos.Location = new Point(557, 562);
            dtgSimbolos.Name = "dtgSimbolos";
            dtgSimbolos.Size = new Size(454, 424);
            dtgSimbolos.TabIndex = 17;
            // 
            // linkLabel1
            // 
            linkLabel1.AutoSize = true;
            linkLabel1.Location = new Point(942, 989);
            linkLabel1.Name = "linkLabel1";
            linkLabel1.Size = new Size(59, 15);
            linkLabel1.TabIndex = 18;
            linkLabel1.TabStop = true;
            linkLabel1.Text = "Acerca de";
            linkLabel1.LinkClicked += linkLabel1_LinkClicked;
            // 
            // panel1
            // 
            panel1.BackColor = SystemColors.ButtonHighlight;
            panel1.Controls.Add(pictureBox1);
            panel1.Controls.Add(pictureBox2);
            panel1.Location = new Point(3, 1);
            panel1.Name = "panel1";
            panel1.Size = new Size(1867, 50);
            panel1.TabIndex = 20;
            // 
            // pictureBox1
            // 
            pictureBox1.Image = Properties.Resources.remove;
            pictureBox1.Location = new Point(1833, 0);
            pictureBox1.Name = "pictureBox1";
            pictureBox1.Size = new Size(31, 34);
            pictureBox1.SizeMode = PictureBoxSizeMode.StretchImage;
            pictureBox1.TabIndex = 21;
            pictureBox1.TabStop = false;
            pictureBox1.Click += pictureBox1_Click;
            // 
            // pictureBox2
            // 
            pictureBox2.Image = (Image)resources.GetObject("pictureBox2.Image");
            pictureBox2.Location = new Point(0, 0);
            pictureBox2.Name = "pictureBox2";
            pictureBox2.Size = new Size(224, 51);
            pictureBox2.SizeMode = PictureBoxSizeMode.StretchImage;
            pictureBox2.TabIndex = 20;
            pictureBox2.TabStop = false;
            // 
            // lblNumErrores
            // 
            lblNumErrores.AutoSize = true;
            lblNumErrores.Location = new Point(386, 544);
            lblNumErrores.Name = "lblNumErrores";
            lblNumErrores.Size = new Size(0, 15);
            lblNumErrores.TabIndex = 21;
            // 
            // txtBuscarSimbolo
            // 
            txtBuscarSimbolo.Location = new Point(716, 533);
            txtBuscarSimbolo.Name = "txtBuscarSimbolo";
            txtBuscarSimbolo.Size = new Size(100, 23);
            txtBuscarSimbolo.TabIndex = 22;
            // 
            // btnBuscarSimbolo
            // 
            btnBuscarSimbolo.Location = new Point(822, 533);
            btnBuscarSimbolo.Name = "btnBuscarSimbolo";
            btnBuscarSimbolo.Size = new Size(102, 23);
            btnBuscarSimbolo.TabIndex = 23;
            btnBuscarSimbolo.Text = "Buscar Simbolo";
            btnBuscarSimbolo.UseVisualStyleBackColor = true;
            btnBuscarSimbolo.Click += btnBuscarSimbolo_Click;
            // 
            // panelErrores
            // 
            panelErrores.AutoScroll = true;
            panelErrores.BackColor = Color.Transparent;
            panelErrores.Location = new Point(3, 84);
            panelErrores.Name = "panelErrores";
            panelErrores.Size = new Size(16, 424);
            panelErrores.TabIndex = 24;
            // 
            // btnGenerarASM
            // 
            btnGenerarASM.Location = new Point(172, 533);
            btnGenerarASM.Name = "btnGenerarASM";
            btnGenerarASM.Size = new Size(109, 23);
            btnGenerarASM.TabIndex = 25;
            btnGenerarASM.Text = "Assembler";
            btnGenerarASM.UseVisualStyleBackColor = true;
            btnGenerarASM.Click += btnGenerarASM_Click;
            // 
            // humbleLang
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = SystemColors.ButtonHighlight;
            ClientSize = new Size(1874, 1005);
            Controls.Add(btnGenerarASM);
            Controls.Add(panelErrores);
            Controls.Add(btnBuscarSimbolo);
            Controls.Add(txtBuscarSimbolo);
            Controls.Add(lblNumErrores);
            Controls.Add(linkLabel1);
            Controls.Add(panel1);
            Controls.Add(dtgSimbolos);
            Controls.Add(label4);
            Controls.Add(label3);
            Controls.Add(label2);
            Controls.Add(label1);
            Controls.Add(rtEntrada);
            Controls.Add(btnGuardar);
            Controls.Add(btnCargar);
            Controls.Add(rtbTokens);
            Controls.Add(dtgTokens);
            Controls.Add(panelLineasTokens);
            Controls.Add(panelLineas);
            Controls.Add(lblResultado);
            Controls.Add(btnValidar);
            FormBorderStyle = FormBorderStyle.None;
            Name = "humbleLang";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Form1";
            Load += humbleLang_Load;
            ((System.ComponentModel.ISupportInitialize)dtgTokens).EndInit();
            ((System.ComponentModel.ISupportInitialize)dtgSimbolos).EndInit();
            panel1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)pictureBox1).EndInit();
            ((System.ComponentModel.ISupportInitialize)pictureBox2).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Button btnValidar;
        private Label lblResultado;
        private RichTextBox rtEntrada;
        private Panel panelLineas;
        private Panel panelLineasTokens;
        private RichTextBox rtbTokens;
        private DataGridView dtgTokens;
        private Button btnCargar;
        private Button btnGuardar;
        private Label label1;
        private Label label2;
        private Label label3;
        private Label label4;
        private DataGridView dtgSimbolos;
        private LinkLabel linkLabel1;
        private Panel panel1;
        private PictureBox pictureBox2;
        private Label lblNumErrores;
        private TextBox txtBuscarSimbolo;
        private Button btnBuscarSimbolo;
        private Panel panelErrores;
        private PictureBox pictureBox1;
        private Button btnGenerarASM;
    }
}

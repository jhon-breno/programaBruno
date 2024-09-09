namespace Pangea.Swat.UI.Apps
{
    partial class JsonConverter
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
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.rbModoTexto = new System.Windows.Forms.RadioButton();
            this.rbModoLista = new System.Windows.Forms.RadioButton();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.rbTipoClienteGB = new System.Windows.Forms.RadioButton();
            this.rbTipoClienteGA = new System.Windows.Forms.RadioButton();
            this.chkArquivoUnico = new System.Windows.Forms.CheckBox();
            this.txtHistorico = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.lblModoGeracao = new System.Windows.Forms.Label();
            this.btnSair = new System.Windows.Forms.Button();
            this.button1 = new System.Windows.Forms.Button();
            this.txtResultado = new System.Windows.Forms.TextBox();
            this.txtCaminhoArq = new System.Windows.Forms.TextBox();
            this.txtListaClientes = new System.Windows.Forms.TextBox();
            this.chkFormatJson = new System.Windows.Forms.CheckBox();
            this.groupBox1.SuspendLayout();
            this.groupBox3.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.SuspendLayout();
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.groupBox3);
            this.groupBox1.Controls.Add(this.groupBox2);
            this.groupBox1.Controls.Add(this.chkFormatJson);
            this.groupBox1.Controls.Add(this.chkArquivoUnico);
            this.groupBox1.Controls.Add(this.txtHistorico);
            this.groupBox1.Controls.Add(this.label2);
            this.groupBox1.Controls.Add(this.label1);
            this.groupBox1.Controls.Add(this.lblModoGeracao);
            this.groupBox1.Controls.Add(this.btnSair);
            this.groupBox1.Controls.Add(this.button1);
            this.groupBox1.Controls.Add(this.txtResultado);
            this.groupBox1.Controls.Add(this.txtCaminhoArq);
            this.groupBox1.Controls.Add(this.txtListaClientes);
            this.groupBox1.Location = new System.Drawing.Point(9, 1);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(645, 231);
            this.groupBox1.TabIndex = 0;
            this.groupBox1.TabStop = false;
            // 
            // groupBox3
            // 
            this.groupBox3.Controls.Add(this.rbModoTexto);
            this.groupBox3.Controls.Add(this.rbModoLista);
            this.groupBox3.Location = new System.Drawing.Point(127, 10);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(292, 41);
            this.groupBox3.TabIndex = 15;
            this.groupBox3.TabStop = false;
            // 
            // rbModoTexto
            // 
            this.rbModoTexto.AutoSize = true;
            this.rbModoTexto.Location = new System.Drawing.Point(108, 15);
            this.rbModoTexto.Name = "rbModoTexto";
            this.rbModoTexto.Size = new System.Drawing.Size(91, 17);
            this.rbModoTexto.TabIndex = 1;
            this.rbModoTexto.TabStop = true;
            this.rbModoTexto.Text = "Arquivo Texto";
            this.rbModoTexto.UseVisualStyleBackColor = true;
            this.rbModoTexto.CheckedChanged += new System.EventHandler(this.rbModoTexto_CheckedChanged);
            // 
            // rbModoLista
            // 
            this.rbModoLista.AutoSize = true;
            this.rbModoLista.Checked = true;
            this.rbModoLista.Location = new System.Drawing.Point(15, 15);
            this.rbModoLista.Name = "rbModoLista";
            this.rbModoLista.Size = new System.Drawing.Size(87, 17);
            this.rbModoLista.TabIndex = 0;
            this.rbModoLista.TabStop = true;
            this.rbModoLista.Text = "Lista Clientes";
            this.rbModoLista.UseVisualStyleBackColor = true;
            this.rbModoLista.CheckedChanged += new System.EventHandler(this.rbModoLista_CheckedChanged);
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.rbTipoClienteGB);
            this.groupBox2.Controls.Add(this.rbTipoClienteGA);
            this.groupBox2.Location = new System.Drawing.Point(425, 11);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(107, 40);
            this.groupBox2.TabIndex = 14;
            this.groupBox2.TabStop = false;
            // 
            // rbTipoClienteGB
            // 
            this.rbTipoClienteGB.AutoSize = true;
            this.rbTipoClienteGB.Location = new System.Drawing.Point(57, 14);
            this.rbTipoClienteGB.Name = "rbTipoClienteGB";
            this.rbTipoClienteGB.Size = new System.Drawing.Size(40, 17);
            this.rbTipoClienteGB.TabIndex = 3;
            this.rbTipoClienteGB.TabStop = true;
            this.rbTipoClienteGB.Text = "GB";
            this.rbTipoClienteGB.UseVisualStyleBackColor = true;
            // 
            // rbTipoClienteGA
            // 
            this.rbTipoClienteGA.AutoSize = true;
            this.rbTipoClienteGA.Location = new System.Drawing.Point(11, 14);
            this.rbTipoClienteGA.Name = "rbTipoClienteGA";
            this.rbTipoClienteGA.Size = new System.Drawing.Size(40, 17);
            this.rbTipoClienteGA.TabIndex = 2;
            this.rbTipoClienteGA.TabStop = true;
            this.rbTipoClienteGA.Text = "GA";
            this.rbTipoClienteGA.UseVisualStyleBackColor = true;
            // 
            // chkArquivoUnico
            // 
            this.chkArquivoUnico.AutoSize = true;
            this.chkArquivoUnico.Location = new System.Drawing.Point(538, 25);
            this.chkArquivoUnico.Name = "chkArquivoUnico";
            this.chkArquivoUnico.Size = new System.Drawing.Size(93, 17);
            this.chkArquivoUnico.TabIndex = 4;
            this.chkArquivoUnico.Text = "Arquivo Único";
            this.chkArquivoUnico.UseVisualStyleBackColor = true;
            // 
            // txtHistorico
            // 
            this.txtHistorico.Location = new System.Drawing.Point(9, 107);
            this.txtHistorico.Multiline = true;
            this.txtHistorico.Name = "txtHistorico";
            this.txtHistorico.Size = new System.Drawing.Size(112, 105);
            this.txtHistorico.TabIndex = 6;
            this.txtHistorico.TabStop = false;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(6, 84);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(48, 13);
            this.label2.TabIndex = 5;
            this.label2.Text = "Histórico";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(73, 84);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(48, 13);
            this.label1.TabIndex = 4;
            this.label1.Text = "Caminho";
            // 
            // lblModoGeracao
            // 
            this.lblModoGeracao.AutoSize = true;
            this.lblModoGeracao.Location = new System.Drawing.Point(79, 60);
            this.lblModoGeracao.Name = "lblModoGeracao";
            this.lblModoGeracao.Size = new System.Drawing.Size(44, 13);
            this.lblModoGeracao.TabIndex = 4;
            this.lblModoGeracao.Text = "Clientes";
            // 
            // btnSair
            // 
            this.btnSair.Location = new System.Drawing.Point(538, 189);
            this.btnSair.Name = "btnSair";
            this.btnSair.Size = new System.Drawing.Size(93, 23);
            this.btnSair.TabIndex = 7;
            this.btnSair.Text = "Sair";
            this.btnSair.UseVisualStyleBackColor = true;
            this.btnSair.Click += new System.EventHandler(this.btnSair_Click);
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(538, 160);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(93, 23);
            this.button1.TabIndex = 6;
            this.button1.Text = "Gerar JSON";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // txtResultado
            // 
            this.txtResultado.Location = new System.Drawing.Point(127, 107);
            this.txtResultado.Multiline = true;
            this.txtResultado.Name = "txtResultado";
            this.txtResultado.Size = new System.Drawing.Size(405, 105);
            this.txtResultado.TabIndex = 3;
            this.txtResultado.TabStop = false;
            // 
            // txtCaminhoArq
            // 
            this.txtCaminhoArq.Location = new System.Drawing.Point(127, 81);
            this.txtCaminhoArq.Name = "txtCaminhoArq";
            this.txtCaminhoArq.Size = new System.Drawing.Size(405, 20);
            this.txtCaminhoArq.TabIndex = 7;
            this.txtCaminhoArq.TabStop = false;
            this.txtCaminhoArq.Text = "c:\\temp\\alta\\";
            // 
            // txtListaClientes
            // 
            this.txtListaClientes.Location = new System.Drawing.Point(127, 57);
            this.txtListaClientes.Name = "txtListaClientes";
            this.txtListaClientes.Size = new System.Drawing.Size(405, 20);
            this.txtListaClientes.TabIndex = 6;
            this.txtListaClientes.TabStop = false;
            // 
            // chkFormatJson
            // 
            this.chkFormatJson.AutoSize = true;
            this.chkFormatJson.Checked = true;
            this.chkFormatJson.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkFormatJson.Location = new System.Drawing.Point(538, 48);
            this.chkFormatJson.Name = "chkFormatJson";
            this.chkFormatJson.Size = new System.Drawing.Size(99, 17);
            this.chkFormatJson.TabIndex = 5;
            this.chkFormatJson.Text = "JSON Identado";
            this.chkFormatJson.UseVisualStyleBackColor = true;
            // 
            // JsonConverter
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(660, 237);
            this.Controls.Add(this.groupBox1);
            this.Name = "JsonConverter";
            this.Text = "JsonConverter";
            this.Load += new System.EventHandler(this.JsonConverter_Load);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.groupBox3.ResumeLayout(false);
            this.groupBox3.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Label lblModoGeracao;
        private System.Windows.Forms.Button btnSair;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.TextBox txtResultado;
        private System.Windows.Forms.TextBox txtListaClientes;
        private System.Windows.Forms.TextBox txtHistorico;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.RadioButton rbModoTexto;
        private System.Windows.Forms.RadioButton rbModoLista;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox txtCaminhoArq;
        private System.Windows.Forms.RadioButton rbTipoClienteGB;
        private System.Windows.Forms.RadioButton rbTipoClienteGA;
        private System.Windows.Forms.CheckBox chkArquivoUnico;
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.CheckBox chkFormatJson;
    }
}
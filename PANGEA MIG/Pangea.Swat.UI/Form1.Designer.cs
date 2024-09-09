using System.Collections.Generic;
using System.Data;
namespace Pangea.Swat.UI
{
    partial class Form1
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
            this.txtComando = new System.Windows.Forms.TextBox();
            this.btnExecutar = new System.Windows.Forms.Button();
            this.btnSair = new System.Windows.Forms.Button();
            this.lblComando = new System.Windows.Forms.Label();
            this.txtResult = new System.Windows.Forms.TextBox();
            this.lblResult = new System.Windows.Forms.Label();
            this.txtSeparador = new System.Windows.Forms.TextBox();
            this.lblSeparador = new System.Windows.Forms.Label();
            this.txtCaminho = new System.Windows.Forms.TextBox();
            this.lblArquivo = new System.Windows.Forms.Label();
            this.dataGridView1 = new System.Windows.Forms.DataGridView();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.chkSaida = new System.Windows.Forms.CheckBox();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.rbEmpresaCE = new System.Windows.Forms.RadioButton();
            this.rbEmpresaRJ = new System.Windows.Forms.RadioButton();
            this.lblEmpresa = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.rbTipoClienteGB = new System.Windows.Forms.RadioButton();
            this.rbTipoClienteGA = new System.Windows.Forms.RadioButton();
            this.groupBox4 = new System.Windows.Forms.GroupBox();
            this.label2 = new System.Windows.Forms.Label();
            this.lblAmbiente = new System.Windows.Forms.Label();
            this.groupBox5 = new System.Windows.Forms.GroupBox();
            this.button1 = new System.Windows.Forms.Button();
            this.button2 = new System.Windows.Forms.Button();
            this.button3 = new System.Windows.Forms.Button();
            this.btnClienteAnterior = new System.Windows.Forms.Button();
            this.btExternalId = new System.Windows.Forms.Button();
            this.lblLinhas = new System.Windows.Forms.Label();
            this.lblLinhasResult = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).BeginInit();
            this.groupBox1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.groupBox3.SuspendLayout();
            this.groupBox4.SuspendLayout();
            this.groupBox5.SuspendLayout();
            this.SuspendLayout();
            // 
            // txtComando
            // 
            this.txtComando.Location = new System.Drawing.Point(66, 16);
            this.txtComando.MaxLength = 32767000;
            this.txtComando.Multiline = true;
            this.txtComando.Name = "txtComando";
            this.txtComando.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.txtComando.Size = new System.Drawing.Size(694, 172);
            this.txtComando.TabIndex = 0;
            this.txtComando.Text = "update cliente set mail = \'{0}\', tipo_cliente = \'{1}\', tipo_empalme = \'{2}\' where" +
    " numero_cliente = {3};";
            // 
            // btnExecutar
            // 
            this.btnExecutar.Location = new System.Drawing.Point(625, 242);
            this.btnExecutar.Name = "btnExecutar";
            this.btnExecutar.Size = new System.Drawing.Size(75, 23);
            this.btnExecutar.TabIndex = 8;
            this.btnExecutar.Text = "&Executar";
            this.btnExecutar.UseVisualStyleBackColor = true;
            this.btnExecutar.Click += new System.EventHandler(this.btnExecutar_Click);
            // 
            // btnSair
            // 
            this.btnSair.Location = new System.Drawing.Point(706, 242);
            this.btnSair.Name = "btnSair";
            this.btnSair.Size = new System.Drawing.Size(75, 23);
            this.btnSair.TabIndex = 9;
            this.btnSair.Text = "&Sair";
            this.btnSair.UseVisualStyleBackColor = true;
            this.btnSair.Click += new System.EventHandler(this.btnSair_Click);
            // 
            // lblComando
            // 
            this.lblComando.AutoSize = true;
            this.lblComando.Location = new System.Drawing.Point(8, 16);
            this.lblComando.Name = "lblComando";
            this.lblComando.Size = new System.Drawing.Size(52, 13);
            this.lblComando.TabIndex = 9;
            this.lblComando.Text = "Comando";
            // 
            // txtResult
            // 
            this.txtResult.BackColor = System.Drawing.SystemColors.InactiveBorder;
            this.txtResult.Enabled = false;
            this.txtResult.ForeColor = System.Drawing.SystemColors.Window;
            this.txtResult.Location = new System.Drawing.Point(66, 16);
            this.txtResult.Multiline = true;
            this.txtResult.Name = "txtResult";
            this.txtResult.Size = new System.Drawing.Size(694, 344);
            this.txtResult.TabIndex = 10;
            // 
            // lblResult
            // 
            this.lblResult.AutoSize = true;
            this.lblResult.Location = new System.Drawing.Point(8, 16);
            this.lblResult.Name = "lblResult";
            this.lblResult.Size = new System.Drawing.Size(55, 13);
            this.lblResult.TabIndex = 17;
            this.lblResult.Text = "Resultado";
            // 
            // txtSeparador
            // 
            this.txtSeparador.Location = new System.Drawing.Point(739, 194);
            this.txtSeparador.MaxLength = 1;
            this.txtSeparador.Name = "txtSeparador";
            this.txtSeparador.Size = new System.Drawing.Size(21, 20);
            this.txtSeparador.TabIndex = 3;
            this.txtSeparador.Text = "|";
            this.txtSeparador.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // lblSeparador
            // 
            this.lblSeparador.AutoSize = true;
            this.lblSeparador.Location = new System.Drawing.Point(677, 197);
            this.lblSeparador.Name = "lblSeparador";
            this.lblSeparador.Size = new System.Drawing.Size(56, 13);
            this.lblSeparador.TabIndex = 50;
            this.lblSeparador.Text = "Separador";
            // 
            // txtCaminho
            // 
            this.txtCaminho.Location = new System.Drawing.Point(66, 194);
            this.txtCaminho.MaxLength = 4000;
            this.txtCaminho.Name = "txtCaminho";
            this.txtCaminho.Size = new System.Drawing.Size(470, 20);
            this.txtCaminho.TabIndex = 1;
            this.txtCaminho.Text = "C:\\!adl\\temp\\swat\\adriel.txt";
            // 
            // lblArquivo
            // 
            this.lblArquivo.AutoSize = true;
            this.lblArquivo.Location = new System.Drawing.Point(17, 197);
            this.lblArquivo.Name = "lblArquivo";
            this.lblArquivo.Size = new System.Drawing.Size(43, 13);
            this.lblArquivo.TabIndex = 10;
            this.lblArquivo.Text = "Arquivo";
            // 
            // dataGridView1
            // 
            this.dataGridView1.AllowUserToAddRows = false;
            this.dataGridView1.AllowUserToDeleteRows = false;
            this.dataGridView1.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridView1.Location = new System.Drawing.Point(69, 16);
            this.dataGridView1.Name = "dataGridView1";
            this.dataGridView1.ReadOnly = true;
            this.dataGridView1.Size = new System.Drawing.Size(691, 344);
            this.dataGridView1.TabIndex = 4;
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.txtResult);
            this.groupBox1.Controls.Add(this.lblResult);
            this.groupBox1.Controls.Add(this.dataGridView1);
            this.groupBox1.Location = new System.Drawing.Point(10, 272);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(770, 370);
            this.groupBox1.TabIndex = 6;
            this.groupBox1.TabStop = false;
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.chkSaida);
            this.groupBox2.Controls.Add(this.txtSeparador);
            this.groupBox2.Controls.Add(this.lblComando);
            this.groupBox2.Controls.Add(this.txtComando);
            this.groupBox2.Controls.Add(this.txtCaminho);
            this.groupBox2.Controls.Add(this.lblArquivo);
            this.groupBox2.Controls.Add(this.lblSeparador);
            this.groupBox2.Location = new System.Drawing.Point(10, 3);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(770, 224);
            this.groupBox2.TabIndex = 11;
            this.groupBox2.TabStop = false;
            // 
            // chkSaida
            // 
            this.chkSaida.AutoSize = true;
            this.chkSaida.Location = new System.Drawing.Point(555, 197);
            this.chkSaida.Name = "chkSaida";
            this.chkSaida.Size = new System.Drawing.Size(109, 17);
            this.chkSaida.TabIndex = 2;
            this.chkSaida.Text = "Arquivo de Saída";
            this.chkSaida.UseVisualStyleBackColor = true;
            this.chkSaida.CheckedChanged += new System.EventHandler(this.chkSaida_CheckedChanged);
            // 
            // groupBox3
            // 
            this.groupBox3.Controls.Add(this.rbEmpresaCE);
            this.groupBox3.Controls.Add(this.rbEmpresaRJ);
            this.groupBox3.Controls.Add(this.lblEmpresa);
            this.groupBox3.Location = new System.Drawing.Point(10, 227);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(159, 45);
            this.groupBox3.TabIndex = 12;
            this.groupBox3.TabStop = false;
            // 
            // rbEmpresaCE
            // 
            this.rbEmpresaCE.AutoSize = true;
            this.rbEmpresaCE.Location = new System.Drawing.Point(111, 15);
            this.rbEmpresaCE.Name = "rbEmpresaCE";
            this.rbEmpresaCE.Size = new System.Drawing.Size(39, 17);
            this.rbEmpresaCE.TabIndex = 5;
            this.rbEmpresaCE.TabStop = true;
            this.rbEmpresaCE.Text = "CE";
            this.rbEmpresaCE.UseVisualStyleBackColor = true;
            // 
            // rbEmpresaRJ
            // 
            this.rbEmpresaRJ.AutoSize = true;
            this.rbEmpresaRJ.Location = new System.Drawing.Point(72, 15);
            this.rbEmpresaRJ.Name = "rbEmpresaRJ";
            this.rbEmpresaRJ.Size = new System.Drawing.Size(38, 17);
            this.rbEmpresaRJ.TabIndex = 4;
            this.rbEmpresaRJ.TabStop = true;
            this.rbEmpresaRJ.Text = "RJ";
            this.rbEmpresaRJ.UseVisualStyleBackColor = true;
            // 
            // lblEmpresa
            // 
            this.lblEmpresa.AutoSize = true;
            this.lblEmpresa.Location = new System.Drawing.Point(12, 17);
            this.lblEmpresa.Name = "lblEmpresa";
            this.lblEmpresa.Size = new System.Drawing.Size(48, 13);
            this.lblEmpresa.TabIndex = 12;
            this.lblEmpresa.Text = "Empresa";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(7, 17);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(63, 13);
            this.label1.TabIndex = 13;
            this.label1.Text = "Tipo Cliente";
            // 
            // rbTipoClienteGB
            // 
            this.rbTipoClienteGB.AutoSize = true;
            this.rbTipoClienteGB.Location = new System.Drawing.Point(86, 15);
            this.rbTipoClienteGB.Name = "rbTipoClienteGB";
            this.rbTipoClienteGB.Size = new System.Drawing.Size(40, 17);
            this.rbTipoClienteGB.TabIndex = 6;
            this.rbTipoClienteGB.TabStop = true;
            this.rbTipoClienteGB.Text = "GB";
            this.rbTipoClienteGB.UseVisualStyleBackColor = true;
            this.rbTipoClienteGB.CheckedChanged += new System.EventHandler(this.rbAmbienteProd_CheckedChanged);
            // 
            // rbTipoClienteGA
            // 
            this.rbTipoClienteGA.AutoSize = true;
            this.rbTipoClienteGA.Location = new System.Drawing.Point(127, 15);
            this.rbTipoClienteGA.Name = "rbTipoClienteGA";
            this.rbTipoClienteGA.Size = new System.Drawing.Size(40, 17);
            this.rbTipoClienteGA.TabIndex = 7;
            this.rbTipoClienteGA.TabStop = true;
            this.rbTipoClienteGA.Text = "GA";
            this.rbTipoClienteGA.UseVisualStyleBackColor = true;
            this.rbTipoClienteGA.CheckedChanged += new System.EventHandler(this.rbAmbienteDes_CheckedChanged);
            // 
            // groupBox4
            // 
            this.groupBox4.Controls.Add(this.rbTipoClienteGB);
            this.groupBox4.Controls.Add(this.label1);
            this.groupBox4.Controls.Add(this.rbTipoClienteGA);
            this.groupBox4.Location = new System.Drawing.Point(175, 227);
            this.groupBox4.Name = "groupBox4";
            this.groupBox4.Size = new System.Drawing.Size(177, 45);
            this.groupBox4.TabIndex = 19;
            this.groupBox4.TabStop = false;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(12, 16);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(51, 13);
            this.label2.TabIndex = 14;
            this.label2.Text = "Ambiente";
            // 
            // lblAmbiente
            // 
            this.lblAmbiente.AutoSize = true;
            this.lblAmbiente.ForeColor = System.Drawing.Color.Red;
            this.lblAmbiente.Location = new System.Drawing.Point(63, 16);
            this.lblAmbiente.Name = "lblAmbiente";
            this.lblAmbiente.Size = new System.Drawing.Size(76, 13);
            this.lblAmbiente.TabIndex = 15;
            this.lblAmbiente.Text = "Não informado";
            // 
            // groupBox5
            // 
            this.groupBox5.Controls.Add(this.label2);
            this.groupBox5.Controls.Add(this.lblAmbiente);
            this.groupBox5.Location = new System.Drawing.Point(358, 228);
            this.groupBox5.Name = "groupBox5";
            this.groupBox5.Size = new System.Drawing.Size(188, 44);
            this.groupBox5.TabIndex = 23;
            this.groupBox5.TabStop = false;
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(800, 19);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(126, 23);
            this.button1.TabIndex = 10;
            this.button1.Text = "Retornos de Ordens";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // button2
            // 
            this.button2.Location = new System.Drawing.Point(800, 48);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(126, 23);
            this.button2.TabIndex = 11;
            this.button2.Text = "Comparador Street";
            this.button2.UseVisualStyleBackColor = true;
            // 
            // button3
            // 
            this.button3.Location = new System.Drawing.Point(800, 77);
            this.button3.Name = "button3";
            this.button3.Size = new System.Drawing.Size(126, 40);
            this.button3.TabIndex = 12;
            this.button3.Text = "JSON paraAlta de Contratação";
            this.button3.UseVisualStyleBackColor = true;
            this.button3.Click += new System.EventHandler(this.button3_Click);
            // 
            // btnClienteAnterior
            // 
            this.btnClienteAnterior.Location = new System.Drawing.Point(800, 123);
            this.btnClienteAnterior.Name = "btnClienteAnterior";
            this.btnClienteAnterior.Size = new System.Drawing.Size(126, 23);
            this.btnClienteAnterior.TabIndex = 13;
            this.btnClienteAnterior.Text = "Rel. Cliente Anterior";
            this.btnClienteAnterior.UseVisualStyleBackColor = true;
            this.btnClienteAnterior.Click += new System.EventHandler(this.btnClienteAnterior_Click);
            // 
            // btExternalId
            // 
            this.btExternalId.Location = new System.Drawing.Point(800, 153);
            this.btExternalId.Name = "btExternalId";
            this.btExternalId.Size = new System.Drawing.Size(126, 23);
            this.btExternalId.TabIndex = 14;
            this.btExternalId.Text = "Corrigir External ID";
            this.btExternalId.UseVisualStyleBackColor = true;
            this.btExternalId.Click += new System.EventHandler(this.btExternalId_Click);
            // 
            // lblLinhas
            // 
            this.lblLinhas.AutoSize = true;
            this.lblLinhas.Location = new System.Drawing.Point(797, 288);
            this.lblLinhas.Name = "lblLinhas";
            this.lblLinhas.Size = new System.Drawing.Size(44, 13);
            this.lblLinhas.TabIndex = 12;
            this.lblLinhas.Text = "Linhas: ";
            this.lblLinhas.Visible = false;
            // 
            // lblLinhasResult
            // 
            this.lblLinhasResult.AutoSize = true;
            this.lblLinhasResult.Location = new System.Drawing.Point(844, 288);
            this.lblLinhasResult.Name = "lblLinhasResult";
            this.lblLinhasResult.Size = new System.Drawing.Size(13, 13);
            this.lblLinhasResult.TabIndex = 12;
            this.lblLinhasResult.Text = "0";
            this.lblLinhasResult.Visible = false;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(944, 647);
            this.Controls.Add(this.lblLinhasResult);
            this.Controls.Add(this.btExternalId);
            this.Controls.Add(this.btnClienteAnterior);
            this.Controls.Add(this.lblLinhas);
            this.Controls.Add(this.button3);
            this.Controls.Add(this.button2);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.groupBox5);
            this.Controls.Add(this.groupBox4);
            this.Controls.Add(this.groupBox3);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.btnExecutar);
            this.Controls.Add(this.btnSair);
            this.Name = "Form1";
            this.Text = "PANGEA SWAT";
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).EndInit();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.groupBox3.ResumeLayout(false);
            this.groupBox3.PerformLayout();
            this.groupBox4.ResumeLayout(false);
            this.groupBox4.PerformLayout();
            this.groupBox5.ResumeLayout(false);
            this.groupBox5.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox txtComando;
        private System.Windows.Forms.Button btnExecutar;
        private System.Windows.Forms.Button btnSair;
        private System.Windows.Forms.Label lblComando;
        private System.Windows.Forms.DataGridView dataGridView1;
        private System.Windows.Forms.TextBox txtCaminho;
        private System.Windows.Forms.Label lblArquivo;
        private System.Windows.Forms.Label lblSeparador;
        private System.Windows.Forms.TextBox txtSeparador;
        private System.Windows.Forms.TextBox txtResult;
        private System.Windows.Forms.Label lblResult;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label lblEmpresa;
        private System.Windows.Forms.RadioButton rbTipoClienteGB;
        private System.Windows.Forms.RadioButton rbTipoClienteGA;
        private System.Windows.Forms.RadioButton rbEmpresaCE;
        private System.Windows.Forms.RadioButton rbEmpresaRJ;
        private System.Windows.Forms.GroupBox groupBox4;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label lblAmbiente;
        private System.Windows.Forms.GroupBox groupBox5;
        private System.Windows.Forms.CheckBox chkSaida;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.Button button3;
        private System.Windows.Forms.Button btnClienteAnterior;
        private System.Windows.Forms.Button btExternalId;
        private System.Windows.Forms.Label lblLinhas;
        private System.Windows.Forms.Label lblLinhasResult;
    }
}


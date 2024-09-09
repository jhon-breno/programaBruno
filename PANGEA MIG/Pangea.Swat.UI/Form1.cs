using Pangea.Entidades.Enumeracao;
using Pangea.Swat.Dados;
using Pangea.Swat.UI.Apps;
using Pangea.Util;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;

namespace Pangea.Swat.UI
{
    public partial class Form1 : Form
    {
        private Empresa empresa = Empresa.NaoIdentificada;
        private TipoCliente tipoCliente = TipoCliente.GB;

        public Form1()
        {
            #region teste do WsSolucoes
            //WsSolucoes.SalesForceEmergenciaSoapClient ws = new WsSolucoes.SalesForceEmergenciaSoapClient();
            //WsSolucoes.Corte corte = ws.GetCorteEnergia2("768924", "1", "2003", "1");
            #endregion

            InitializeComponent();
            carregarDados();
        }

        private void carregarDados()
        {
            rbEmpresaRJ.CheckedChanged += new EventHandler(EmpresaSelection);
            rbEmpresaCE.CheckedChanged += new EventHandler(EmpresaSelection);
            rbTipoClienteGA.CheckedChanged += new EventHandler(TipoClienteSelection);
            rbTipoClienteGB.CheckedChanged += new EventHandler(TipoClienteSelection);

            rbEmpresaRJ.Checked = true;
            rbTipoClienteGB.Checked = true;

            lblAmbiente.Text = ConfigurationManager.AppSettings.Get("Ambiente");
        }

        private void TipoClienteSelection(object sender, EventArgs e)
        {
            this.tipoCliente = rbTipoClienteGA.Checked ? TipoCliente.GA : TipoCliente.GB;
        }

        private void EmpresaSelection(object sender, EventArgs e)
        {
            this.empresa = rbEmpresaRJ.Checked ? Empresa.RJ : Empresa.CE;
        }

        private void btnExecutar_Click(object sender, EventArgs e)
        {
            resetExecucao();
            DialogResult resposta = MessageBox.Show(string.Format("Prosseguir com a execução do arquivo?"), "Confirmação", MessageBoxButtons.YesNo);
            if (DialogResult.No == resposta)
                return;

            try
            {
                if (string.IsNullOrEmpty(txtComando.Text) && txtComando.Text.Length < 50)
                    throw new Exception("É obrigatório existir um comando SQL para ser executado.");

                if (!string.IsNullOrEmpty(txtCaminho.Text) && !chkSaida.Checked)
                {
                    if (string.IsNullOrEmpty(txtSeparador.Text))
                    {
                        txtSeparador.Focus();
                        throw new Exception("Informe o caracter separador.");
                    }

                    ExecutarComandoBatch();
                }
                else
                {
                    // 26 eh o tamanho mínimo de um select update, com WHERE
                    // select * from xx where 1=1
                    if (!string.IsNullOrEmpty(txtComando.Text) && txtComando.Text.Length > 25 && 
                        txtComando.Text.ToLower().Contains("select"))
                    {
                        DataTable result = GetExecutor(this.tipoCliente).ExecutarDataTable(txtComando.Text.Trim());
                        if (result != null)
                        {
                            dataGridView1.DataSource = result;
                            if (chkSaida.Checked)
                            {
                                if (File.Exists(txtCaminho.Text))
                                {
                                    File.Copy(txtCaminho.Text, string.Concat("bkp_", DateTime.Now.ToString("yyyyMMddHHmmss"), ".txt"));
                                }

                                //gravar arquiv de saída
                                foreach (DataRow linha in result.Rows)
                                {
                                    StringBuilder sb = new StringBuilder();
                                    for(int i = 0; i < result.Columns.Count; i++)
                                    {
                                        if(sb.Length > 0)
                                            sb.Append("|");

                                        sb.Append(linha[i].ToString());
                                    }
                                    IO.EscreverArquivo(txtCaminho.Text, sb.ToString(), null);
                                    sb.Clear();
                                }
                            }
                        }

                        exibirResultado(false);
                    }
                    else
                    {
                        txtResult.Text = string.Format("Linhas afetadas: {0}.", GetExecutor(this.tipoCliente).ExecutarComandoSql(txtComando.Text.Trim()));
                        exibirResultado(true);
                    }
                }
            }
            catch(Exception ex)
            {
                txtResult.Text = string.Format("ERRO: {0}", ex.Message);
                exibirResultado(true);
            }
        }


        private void resetExecucao()
        {
            txtResult.Text = string.Empty;
            dataGridView1.DataSource = null;
        }

        private void exibirResultado(bool exibeResultado)
        {
            lblLinhasResult.Text = dataGridView1.Rows.Count > 0 ? dataGridView1.Rows.Count.ToString("N0") : "0";
            
            dataGridView1.Visible = !exibeResultado;
            lblLinhas.Visible = !exibeResultado;
            lblLinhasResult.Visible = !exibeResultado;

            txtResult.Visible = exibeResultado;
        }


        private ExecutorDB GetExecutor(TipoCliente tipoCliente)
        {
            return new ExecutorDB(this.empresa, EnumString.GetStringValue(this.tipoCliente));
        }


        /// <summary>
        /// Valida o arquivo de entrada e executa o comando no banco de dados
        /// </summary>
        private int ExecutarComandoBatch()
        {
            var fullPath  = txtCaminho.Text.Trim();

            //validar arquivo de entrada
            if(!File.Exists(fullPath))
                throw new Exception(string.Format("Arquivo '{0}' não encontrado.", fullPath));

            int cont = 0;
            using (StreamReader sr = new StreamReader(fullPath, Encoding.ASCII))
            {
                StringBuilder sb = new StringBuilder();
                List<string> lstQueries = new List<string>();

                while (true)
                {
                    try
                    {
                        var linha = sr.ReadLine();
                        if (string.IsNullOrEmpty(linha))
                            break;

                        if (sb.Length > 0)
                            sb.Clear();

                        var arrCampos = linha.Trim().Split(new char[] { Convert.ToChar(txtSeparador.Text) });

                        lstQueries.Add(string.Format(txtComando.Text, arrCampos));
                        cont++;
                    }
                    catch (Exception ex)
                    {
                        throw ex;
                        //registrar log
                    }
                }

                try
                {
                    GetExecutor(this.tipoCliente).ExecutarComandosSql(lstQueries);
                }
                catch(Exception ex)
                {
                    throw new Exception(string.Format("{0}{1}{1} O comando foi executado em {2} registros.", ex, Environment.NewLine, cont));
                }
            }

            txtResult.Text = string.Format("O comando foi executado na base do {0}, atualizando {1} registros.", this.empresa, cont );

            exibirResultado(true);

            return cont;
        }


        private void btnSair_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void rbAmbienteProd_CheckedChanged(object sender, EventArgs e)
        {
            //lblAlertAmbiente.Visible = rbTipoClienteGB.Checked;
        }

        private void rbAmbienteDes_CheckedChanged(object sender, EventArgs e)
        {
            //lblAlertAmbiente.Visible = rbTipoClienteGB.Checked;
        }



        private void chkSaida_CheckedChanged(object sender, EventArgs e)
        {
            if(chkSaida.Checked && string.IsNullOrEmpty(txtCaminho.Text.Trim()))
            {
                txtCaminho.Focus();
                chkSaida.Checked = false;
                MessageBox.Show("Informe o nome do arquivo de saída.");
            }
        }



        private void button1_Click(object sender, EventArgs e)
        {
            this.Cursor = Cursors.WaitCursor;
            try
            {
                OrdemRetornos form2 = new OrdemRetornos();
                form2.Show();
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                this.Cursor = Cursors.Default;
            }
        }


        /// <summary>
        /// Geração do arquivo de alta de contratação em formato JSON para implantação no SAP
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button3_Click(object sender, EventArgs e)
        {
            this.Cursor = Cursors.WaitCursor;
            try
            {
                JsonConverter form3 = new Apps.JsonConverter();
                form3.Show();
            }
            catch(Exception ex)
            {
                throw ex;
            }
            finally
            {
                this.Cursor = Cursors.Default;
            }
        }


        /// <summary>
        /// Com base no número de cliente, busca o cliente-pai por recursividade.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnClienteAnterior_Click(object sender, EventArgs e)
        {
            this.Cursor = Cursors.WaitCursor;
            try
            {
                Negocio.ClienteBO neg = new Negocio.ClienteBO((rbEmpresaRJ.Checked) ? Empresa.RJ : Empresa.CE, (rbTipoClienteGA.Checked ? TipoCliente.GA : TipoCliente.GB));
                //txtResult.Text = neg.GerarRelatorioClienteAnterior(txtCaminho.Text);
                txtResult.Text = neg.GerarRelatorioClienteAnteriorTodosClientes(txtCaminho.Text);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.StackTrace);
            }
            finally
            {
                this.Cursor = Cursors.Default;
            }
        }

        private void btExternalId_Click(object sender, EventArgs e)
        {
            CorrigirExternalId form = new CorrigirExternalId(this.empresa);
            form.ShowDialog();
        }   
    }
}

using Pangea.Dados;
using Pangea.Dados.Base;
using Pangea.Entidades;
using Pangea.Entidades.Enumeracao;
using Pangea.Swat.Dados;
using Pangea.Swat.UI.Negocio;
using SalesforceExtractor.apex;
//using SalesforceExtractor;
//using SalesforceExtractor.apex;
//using SalesforceExtractor.Dados.SalesForce;
//using SalesforceExtractor.Entidades;
//using SalesforceExtractor.apexMetadata;
using SalesforceExtractor.apex;
using SalesforceExtractor.apexMetadata;
using SalesforceExtractor;
using SalesforceExtractor.Entidades;
using SalesforceExtractor.Entidades.Enumeracoes;
using SalesforceExtractor.Dados.SalesForce;
using SalesforceExtractor.Dados;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Pangea.Swat.UI.Apps
{
    public partial class CorrigirExternalId : Form
    {
        Empresa empresa = Empresa.NaoIdentificada;

        public CorrigirExternalId()
        {
            InitializeComponent();
        }

        public CorrigirExternalId(Empresa empresa)
        {
            this.empresa = empresa;
            InitializeComponent();
        }


        /// <summary>
        /// Atualiza o External Id selecionado do Synergia para o Salesforce
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btAtualizar_Click(object sender, EventArgs e)
        {

        }

        private void btSair_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void CorrigirExternalId_Load(object sender, EventArgs e)
        {
            this.lbEmpresaDesc.Text = Util.EnumString.GetStringValue(this.empresa);
        }

        private void check_CheckedChanged(object sender, EventArgs e)
        {
            controlarAcaoAtualizar();
        }

        private void controlarAcaoAtualizar()
        {
            this.btnAtualizar.Enabled = chkAccount.Checked || chkAsset.Checked || chkPod.Checked;
        }

        private void btnConsultar_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtNumeroClienteConsulta.Text))
                return;

            if (this.empresa == Empresa.NaoIdentificada)
                return;

            string codigoEmpresa = ((int)this.empresa).ToString();
            string caminhoLog = string.Format("C:\\Temp\\ExternalId_Update_{0}_{1}_{2}_LOG"
                , this.empresa
                , this.txtNumeroClienteConsulta.Text
                , DateTime.Now.ToString("yyyy-MM-dd-HH-mm"));

            #region Consultar Synergia
            ExecutorDB exe = new ExecutorDB(this.empresa);
            DBProviderInformix conn = exe.ObterProviderInformix();
            SalesGeralDAO dao = new SalesGeralDAO(this.empresa);
            try
            {
                conn.OpenConnection();
                conn.BeginTransacion(IsolationLevel.ReadUncommitted);
                List<SalesGeral> lstSales = dao.GetSalesgeralByCliente(txtNumeroClienteConsulta.Text);
                conn.Commit();
                conn.CloseConnection();

                if (lstSales == null || lstSales.Count == 0)
                    return;

                SalesGeral obj = lstSales.FirstOrDefault();
                txtAccountSyn.Text = obj.ExternalId_Conta;
                txtAssetSyn.Text = obj.ExternalId_Asset;
                txtPoDSyn.Text = obj.ExternalId_Pod;
                txtNumeroCliente.Text = obj.Numero_Cliente;
                txtNome.Text = obj.Nome;

                limparForm(false);
            }
            catch (Exception ex)
            {
                conn.Rollback();
            }
            finally
            {
                conn.Dispose();
            }
            #endregion
        }

        private void limparForm(bool limparTodosCampos)
        {
            txtNumeroClienteConsulta.Text = string.Empty;
            if(limparTodosCampos)
            {
                txtAccountSyn.Text = string.Empty;
                txtAssetSyn.Text = string.Empty;
                txtPoDSyn.Text = string.Empty;
                txtNumeroCliente.Text = string.Empty;
                txtNome.Text = string.Empty;
            }
        }
    }
}

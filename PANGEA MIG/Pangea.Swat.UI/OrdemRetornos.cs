using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.UI.WebControls;
using System.Windows.Forms;

namespace Pangea.Swat.UI
{
    public partial class OrdemRetornos : Form
    {
        private DataTable dt = new DataTable();
        private Hashtable lista = new Hashtable();

        public OrdemRetornos()
        {
            InitializeComponent();
        }


        /// <summary>
        /// Carrega os serviços conforme dados da tabela SERVICOS
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// <remarks>Depende de um arquivo texto no formato conforme o exemplo:
        /// COD ORDEM|COD SERVICO|ETAPA|COD RETORNO|DESCRICAO|ACAO
        /// ATC|ACP|EXE|1|ATUALIZADO OS DADOS DO CURTO PRAZO||</remarks>
        private void OrdemRetornos_Load(object sender, EventArgs e)
        {
            var fullPath = "c:\\!adl\\temp\\OrdemRetornos.txt";

            //validar arquivo de entrada
            if (!File.Exists(fullPath))
            {
                textBox1.Text = (string.Format("Arquivo '{0}' não encontrado.", fullPath));
                return;
            }

            StreamReader sr = new StreamReader(fullPath, Encoding.GetEncoding("iso-8859-1"));
            StringBuilder sb = new StringBuilder();
            int cont = 0;
            

            #region preparar Datatable
            
            dt.Columns.Add("codOrdem");
            dt.Columns.Add("codServico");
            dt.Columns.Add("codRetorno");
            dt.Columns.Add("descricaoRetorno");
            dt.Columns.Add("codOrdemAssoc");
            dt.Columns.Add("codServicoAssoc");
            dt.Columns.Add("etapa");

            #endregion
            
            string chave = string.Empty;
            cbOrdem.Items.Add("Selecione");
            
            while (true)
            {
                var linha = sr.ReadLine();
                if (string.IsNullOrEmpty(linha))
                    break;

                if (sb.Length > 0)
                    sb.Remove(0, sb.Length);

                var arrCampos = linha.Trim().Split(new char[] { '|' });
                    
                DataRow dr = dt.NewRow();
                dr.SetField<string>("codOrdem", arrCampos[0] != null ? arrCampos[0].ToString().Trim() : string.Empty);
                dr.SetField<string>("codServico", arrCampos[1] != null ? arrCampos[1].ToString().Trim() : string.Empty);
                dr.SetField<string>("etapa", arrCampos[2] != null ? arrCampos[2].ToString().Trim() : string.Empty);
                dr.SetField<string>("codRetorno", arrCampos[3] != null ? arrCampos[3].ToString().Trim() : string.Empty);
                dr.SetField<string>("descricaoRetorno", arrCampos[4] != null ? arrCampos[4].ToString().Trim() : string.Empty);
                dr.SetField<string>("codOrdemAssoc", arrCampos[5] != null ? arrCampos[5].ToString().Trim() : string.Empty);
                dr.SetField<string>("codServicoAssoc", arrCampos[6] != null ? arrCampos[6].ToString().Trim() : string.Empty);
                chave = string.Concat(dr["codOrdem"], dr["codServico"]);

                OrdemServico os = new OrdemServico(dr);
                if (!lista.ContainsKey(chave))
                {
                    lista.Add(chave, new List<OrdemServico>(){os});
                    cbOrdem.Items.Add(string.Concat(arrCampos[0], "-", arrCampos[1]));
                }
                else
                    ((List<OrdemServico>)lista[chave]).Add(os) ;

                cont++;
            }

            foreach (string k in lista.Keys)
            {
                List<OrdemServico> lst = (List<OrdemServico>)lista[k];

                if (sb.Length > 0)
                    sb.Append(string.Format("{0}{0}{0}--------------------------------", Environment.NewLine));

                sb.AppendFormat("------- {0}/{1} '{2}' --------", lst.First().codOrdem, lst.First().codServico, lst.First().etapa, Environment.NewLine);
                sb.Append(getOrdens(k, 0));
            }

            cbOrdem.SelectedIndex = 0;
            textBox1.Text = sb.ToString();
        }


        private string getOrdens(string chave, int tabulacao)
        {
            List<OrdemServico> lst = (List<OrdemServico>)lista[chave];
            StringBuilder sb = new StringBuilder();

            if (lst != null && lst.Count > 0)
            {
                foreach (OrdemServico o in lst)
                {
                    if (!string.IsNullOrEmpty(o.codOrdemAssoc) || !string.IsNullOrEmpty(o.codServicoAssoc))
                    {
                        sb.AppendFormat("{5}{6}{0}/{1} [{2}] Etapa '{3}' {4}", o.codOrdem, o.codServico, o.codRetorno, o.etapa, o.descricaoRetorno, Environment.NewLine, "".PadLeft(tabulacao, ' '));
                        sb.Append(getOrdens(string.Concat(o.codOrdemAssoc, o.codServicoAssoc), tabulacao+=10));
                        tabulacao -= 10;
                    }
                    else
                        sb.AppendFormat("{5}{6}{0}/{1} [{2}] Etapa '{3}' - {4}", o.codOrdem, o.codServico, o.codRetorno, o.etapa, o.descricaoRetorno, Environment.NewLine, "".PadLeft(tabulacao, ' '));
                }
                return sb.ToString();
            }
            return string.Empty;
        }



        private void cbOrdem_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cbOrdem.SelectedIndex <= 0)
                return;

            List<OrdemServico> itemSelecionado = (List<OrdemServico>)lista[string.Concat(cbOrdem.SelectedItem.ToString().Substring(0, 3), cbOrdem.SelectedItem.ToString().Substring(4, 3))];
            dataGridView1.DataSource = itemSelecionado;
        }

        private void dataGridView1_DataBindingComplete(object sender, DataGridViewBindingCompleteEventArgs e)
        {
            //oculta a coluna com a coleção associada de OrdemServico
            dataGridView1.Columns[7].Visible = false;
        }

        private void OrdemRetornos_Resize(object sender, EventArgs e)
        {
            dataGridView1.Width = OrdemRetornos.ActiveForm.Width - 50;
            textBox1.Width = OrdemRetornos.ActiveForm.Width - 50;
            textBox1.Height = OrdemRetornos.ActiveForm.Height - textBox1.Top - 50;
        }
    }
}


public class OrdemServico
{
    public OrdemServico(DataRow r)
    {
        this.codOrdem = r["codOrdem"].ToString();
        this.codServico = r["codServico"].ToString();
        this.etapa = r["etapa"].ToString();
        this.codRetorno = r["codRetorno"].ToString();
        this.descricaoRetorno = r["descricaoRetorno"].ToString();
        this.codOrdemAssoc = r["codOrdemAssoc"].ToString();
        this.codServicoAssoc = r["codServicoAssoc"].ToString();
        this.listaInterna = new Hashtable();
        if(!string.IsNullOrEmpty(this.codOrdemAssoc) && !string.IsNullOrEmpty(this.codServicoAssoc))
        {
            this.listaInterna.Add(this.codOrdemAssoc, this.codServicoAssoc);
        }
    }

    public string codOrdem { get; set; }
    public string codServico { get; set; }
    public string codRetorno { get; set; }
    public string descricaoRetorno { get; set; }
    public string codOrdemAssoc { get; set; }
    public string codServicoAssoc { get; set; }
    public string etapa { get; set; }
    public Hashtable listaInterna { get; set; }
}

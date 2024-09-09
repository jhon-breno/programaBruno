using System;
using System.Data;
using System.IO;
using System.Text.RegularExpressions;

namespace BotaoEmpresa.controls
{
    public partial class ConsultarClientesFaturaPorEmail : System.Web.UI.UserControl
    {
        protected void Page_Load(object sender, EventArgs e)
        {

        }

        protected void Consultar_OnClick(object sender, EventArgs e)
        {
            DataTable dt = ConvertCSVtoDataTable(Server.MapPath(string.Format("~/content/lst/botao-empresa/coelce/clientes-fatura-por-email.csv")));

            DataTable selectedTable = null;
            try
            {
                selectedTable = dt.AsEnumerable()
                    .Where(r => r.Field<string>("Conta-Contrato") == UCDescadFat.Text)
                    .CopyToDataTable();
            }
            catch { }

            grdDados.DataSource = selectedTable;
            grdDados.DataBind();

            //List<Item> referencias = new List<Item>();
            //using (StreamReader tr = new StreamReader(Server.MapPath(string.Format("~/content/lst/botao-empresa/coelce/clientes-fatura-por-email.csv")), Encoding.GetEncoding("iso-8859-1")))
            //{
            //    string line;
            //    while ((line = tr.ReadLine()) != null)
            //    {
            //        string[] items = line.Trim().Split(';');

            //        Item item = new Item();
            //        item.NumeroCliente = items[0];
            //        item.Referencia = items[1];
            //        referencias.Add(item);
            //    }

            //}

            //bool ifExists = false;
            //ResultadoConsultaOk.Text = "Referencias Encontradas:";
            //foreach (var referencia in referencias)
            //{
            //    if (UCDescadFat.Text.Equals(referencia.NumeroCliente))
            //    {
            //        ResultadoConsultaKo.Text = string.Empty;
            //        ResultadoConsultaOk.Text += "</br>" + referencia.Referencia;
            //        ifExists = true;
            //    }
            //}
            //if (!ifExists)
            //{
            //    ResultadoConsultaOk.Text = string.Empty;
            //    ResultadoConsultaKo.Text = "Nenhuma Referencia Encontrada.";
            //}
        }

        public static DataTable ConvertCSVtoDataTable(string strFilePath)
        {
            StreamReader sr = new StreamReader(strFilePath);
            string[] headers = sr.ReadLine().Split(';');
            DataTable dt = new DataTable();
            foreach (string header in headers)
            {
                dt.Columns.Add(header);
            }
            while (!sr.EndOfStream)
            {
                string[] rows = Regex.Split(sr.ReadLine(), ";(?=(?:[^\"]*\"[^\"]*\")*[^\"]*$)");
                DataRow dr = dt.NewRow();
                for (int i = 0; i < headers.Length; i++)
                {
                    dr[i] = rows[i];
                }
                dt.Rows.Add(dr);
            }

            sr.Dispose();
            return dt;
        }


        protected void btnUpload_OnClick(object sender, EventArgs e)
        {
            //File.Copy(filepath, "\\\\10.152.20.250\\onehub\\assets\\lst\\botao-empresa\\coelce");

        }


        protected void btnEnviarArquivo_OnClickuivo_Click(object sender, EventArgs e)
        {
            if (FileUpload1.HasFile)
            {
                string nomeArquivoDestino = Server.MapPath("~/content/lst/botao-empresa/coelce/clientes-fatura-por-email.csv");

                try
                {
                    if (File.Exists(nomeArquivoDestino))
                    {
                        File.Delete(nomeArquivoDestino);
                    }
                }
                catch (IOException ioExp)
                {
                    Console.WriteLine(ioExp.Message);
                }

                string nomeArquivo = Path.GetFileName(FileUpload1.PostedFile.FileName);
                long tamanhoArquivo = FileUpload1.PostedFile.ContentLength;
                FileUpload1.PostedFile.SaveAs(Server.MapPath("~/content/lst/botao-empresa/coelce/clientes-fatura-por-email.csv"));
                //string filepath = nomeArquivo;
                lblmsg.Text = "Arquivo atualizado com sucesso.";
            }
            else
            {
                lblmsg.Text = "Por Favor, selecione um arquivo a enviar.";
            }
        }

    }




    public class Item
    {
        public string NumeroCliente { get; set; }
        public string Referencia { get; set; }
    }
}
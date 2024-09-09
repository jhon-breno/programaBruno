using System;
using System.Data;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace BotaoEmpresa.controls
{
    public partial class ConsultarBeneficio : System.Web.UI.UserControl
    {

        private string[] textData = new string[] { };
        protected void Page_Load(object sender, EventArgs e)
        {

        }

        protected void Consultar_OnClick(object sender, EventArgs e)
        {
            DataTable dt = ConvertCSVtoDataTable(Server.MapPath(string.Format("~/content/lst/botao-empresa/coelce/relatorio_bareserestaurantes.csv")));


            ResultadoConsultaOk.Text = string.Empty;
            ResultadoConsultaKo.Text = string.Empty;
            bool achou = false;
            if ((from DataRow item in dt.Rows select item["PONTO DE FORNECIMENTO"].ToString()).Any(uc => uc.TrimStart(new Char[] { '0' }) == UC.Text))
            {
                ResultadoConsultaOk.Text = "Cliente apto ao beneficio.";
                achou = true;
            }

            if (!achou)
            {
                ResultadoConsultaKo.Text = "Cliente fora dos critérios para o benefício.";
            }




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
    }
}
﻿using System;
using System.Data;
using System.IO;
using System.Text;

namespace BotaoEmpresa.controls
{
    public partial class DescadastramentoEnel : System.Web.UI.UserControl
    {
        DataTable table = new DataTable();

        private string[] textData = new string[] { };

        protected void Page_Load(object sender, EventArgs e)
        {
            textData = File.ReadAllLines(Server.MapPath("~/content/lst/clientes-descadastramento.lst"), Encoding.UTF8);

            DataTable table = new DataTable();
            table.Columns.Add("Número Cliente");


            foreach (var line in textData)
            {
                table.Rows.Add(line);

            }

            //GrdMultas.DataSource = table;
            //GrdMultas.DataBind();

        }


        protected void Consultar_OnClick(object sender, EventArgs e)
        {
            ResultadoConsultaOk.Text = string.Empty;
            ResultadoConsultaKo.Text = string.Empty;
            bool achou = false;
            foreach (var valor in textData)
            {
                string[] valorSplit = valor.Split('#');
                if (UCDescad.Text.Equals(valorSplit[0]))
                {
                    ResultadoConsultaKo.Text = string.Format("Cliente está no processo de descadastramento do baixa renda.</br></br>Motivo: {0}", valorSplit[1]);
                    achou = true;
                    break;
                }
            }

            if (!achou)
            {
                ResultadoConsultaOk.Text = "Cliente não está no processo de descadastramento do baixa renda.";
            }
        }
    }
}
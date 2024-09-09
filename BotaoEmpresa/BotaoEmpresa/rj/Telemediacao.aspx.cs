using System;
using System.Configuration;
using System.Web.UI;
using BotaoEmpresa.ServiceOrderRj;

namespace BotaoEmpresa.rj
{
    public partial class Telemediacao : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            string numero = Request.QueryString["cli"];
            ConsultarDados(numero);

        }

        public void ConsultarDados(string numeroCliente)
        {
           
            try
            {

                if (string.IsNullOrEmpty(numeroCliente))
                {
                    ScriptManager.RegisterClientScriptBlock(Page, Page.GetType(), "alert", @"alert('Não foi possível efetuar a consulta do cliente. \n\nPara realizar a consulta necessitamos o parametro do cliente no endereço.');", true);
                }
                else
                {

                    WsServiceOrderSoapClient leitura = new WsServiceOrderSoapClient();
                    AuthHeader Ah = new AuthHeader();
                    Ah.userName = ConfigurationManager.AppSettings["user_WsServiceOrderRJ"];
                    Ah.password = ConfigurationManager.AppSettings["pass_WsServiceOrderRJ"];

                    //metodo antigo
                    //RegularReadingsModel[] a = leitura.obtainRegularReadingsFromClient(Ah, numero);

                    RegularReadingsTypeModel[] a = leitura.obtainRegularReadingsTypeFromClient(Ah, numeroCliente);

                    GridTelemedicaoCe.DataSource = a;
                    GridTelemedicaoCe.DataBind();
                }

            }
            catch (Exception ex)
            {
                ScriptManager.RegisterClientScriptBlock(Page, Page.GetType(), "alert", @"alert('Não foi possível efetuar a consulta do cliente. \n\n "+ ex.Message+ "');", true);

            }
        }

    }
}
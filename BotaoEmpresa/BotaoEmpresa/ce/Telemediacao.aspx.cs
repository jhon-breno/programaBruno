using System;
using System.Configuration;
using BotaoEmpresa.ServiceOrderCe;

namespace BotaoEmpresa.ce
{
    public partial class Telemediacao : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            WsServiceOrderSoapClient leitura = new WsServiceOrderSoapClient();


            AuthHeader Ah = new AuthHeader();
            Ah.userName = ConfigurationManager.AppSettings["Usuario"];
            Ah.password = ConfigurationManager.AppSettings["Senha"];

            string numero = Request.QueryString["cli"];
            RegularReadingsModel[] a = leitura.obtainRegularReadingsFromClient(Ah, numero);

            GridTelemedicaoCe.DataSource = a;
            GridTelemedicaoCe.DataBind();
        }
    }
}
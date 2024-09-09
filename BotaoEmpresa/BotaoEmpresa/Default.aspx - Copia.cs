using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace ConsultarOrdem
{
    public partial class _Default : Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (Session["AdminHistOrdem"] == null)
            {
                try
                {
                    Control loginRedeBr = (Control)Page.LoadControl("~/Controls/LoginRedeBR.ascx");
                    pnlHistOrdem.Controls.Add(loginRedeBr);
                }
                catch (Exception exception)
                {
                    Response.Redirect("/");
                }
            }
            else
            {
                Control historicoDeOrdem = (Control)Page.LoadControl("~/Controls/HistoricoDeOrdem.ascx");
                pnlHistOrdem.Controls.Add(historicoDeOrdem);
                //ScriptManager.RegisterStartupScript(UpdatePanel1, UpdatePanel1.GetType(), System.Guid.NewGuid().ToString(), "$('.nav-tabs a[href=\"#HistoricoDeOrdens\"]').tab('show');", true);
            }
            UpdatePanel1.Update();
        }
    }
}
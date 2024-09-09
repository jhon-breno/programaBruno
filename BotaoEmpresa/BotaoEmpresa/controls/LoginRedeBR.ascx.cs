using System;
using System.DirectoryServices;
using System.Web.UI;
using BotaoEmpresa.@enum;

namespace BotaoEmpresa.controls
{
    public partial class LoginRedeBR : System.Web.UI.UserControl
    {

        protected void Acessar_OnClick(object sender, EventArgs e)
        {
            string erro = string.Empty;
            Session["AdminHistOrdem"] = RetornaUsuarioAD(BR.Text, Senha.Text, out erro);


            if (Session["AdminHistOrdem"] != null)
            {
                Response.Redirect("/");
            }
            else
            {
                if (!string.IsNullOrEmpty(erro))
                    ScriptManager.RegisterStartupScript(UpdPanelToast, UpdPanelToast.GetType(), System.Guid.NewGuid().ToString(), string.Format("ShowToast('{0}','{1}');LimparForm();", erro.Replace("\n", string.Empty).Replace("\r", String.Empty).Replace("\t", String.Empty), ToastTypes.Alerta), true);
            }

        }

        public static AdUserInfo RetornaUsuarioAD(string br, string senha, out string erro)
        {
            erro = string.Empty;
            DirectoryEntry AdEntry = new DirectoryEntry("LDAP://enelint.global", br, senha);

            DirectorySearcher mySearcher = new DirectorySearcher(AdEntry);

            if (!br.Contains("enelint\\"))
            {
                br = "enelint\\" + br;
            }

            mySearcher.Filter = "(&(objectClass=User)(objectCategory=person)(sAMAccountName=" + br.Split('\\')[1] + "))";
            mySearcher.CacheResults = true;
            mySearcher.ClientTimeout.Add(new TimeSpan(0, 0, 50));
            mySearcher.PageSize = 500;
            mySearcher.SearchScope = SearchScope.Subtree;
            mySearcher.PropertiesToLoad.Add("Name");
            mySearcher.PropertiesToLoad.Add("Mail");
            mySearcher.PropertiesToLoad.Add("telephoneNumber");
            mySearcher.PropertiesToLoad.Add("samaccountname");

            SearchResult objResultadoSr = null;
            try
            {
                objResultadoSr = mySearcher.FindOne();
            }
            catch (Exception e)
            {
                if (e.Message.Contains("O servidor não está operacional."))
                    erro = "Acesso não permitido, você não está conectado a uma rede Enel.";
                else
                    erro = e.Message;
            }

            if (objResultadoSr == null)
            {
                return null;
            }

            DirectoryEntry objDr = objResultadoSr.GetDirectoryEntry();
            try
            {
                var Nome = objResultadoSr.Properties["Name"][0].ToString();
                var Email = (objResultadoSr.Properties["Mail"].Count <= 0 ? "" : objResultadoSr.Properties["Mail"][0].ToString());
                var Telephone = (objResultadoSr.Properties["telephoneNumber"].Count <= 0 ? "" : objResultadoSr.Properties["telephoneNumber"][0].ToString());
                var LoginBR = objResultadoSr.Properties["samaccountname"][0].ToString();

                return new AdUserInfo() { Dominio = "enelint.global".ToUpper(), Login = LoginBR.ToUpper(), Nome = Nome };
            }
            catch (Exception Ex)
            {
                throw Ex;
            }
        }

        public class AdUserInfo
        {
            public string Login { get; set; }
            public string Nome { get; set; }
            public string Dominio { get; set; }

            public AdUserInfo()
            {
            }

            public AdUserInfo(string strBRLogin, string strNome, string strDominio)
            {
                Nome = strNome;
                Dominio = strDominio;
                Login = strBRLogin.Split(char.Parse("\\"))[1].ToString();
            }

        }
    }
}
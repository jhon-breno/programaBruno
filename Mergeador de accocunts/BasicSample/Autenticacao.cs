using SalesforceExtractor.apex;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;

namespace SalesforceExtractor
{
    public class Autenticacao
    {
        private string url = string.Empty;

        public Autenticacao(string ambiente)
        {
            if (string.IsNullOrWhiteSpace(ambiente))
                throw new ArgumentException("Ambiente de execução não informado.");

            if ("prod".Equals(ambiente.ToLower()))
            {
                un = "brasilmigracion4@enel.com";
                pw = "Deloiyye2020mUkLb2eNfT3yBZnPSQMtAAum";

                //un = "brasil.delta@enel.com";
                //pw = "D3lt4s@EnelcgOH94EnwFX94vneiyVxVImdi";
                //pw = "D3lt4s@EnelpQMvvnKL3Hf3Wm67iIct0mKB";

                //un = "gds.brasil@enel.com";
                //pw = "Enel2019+";

                //un = "brasilmigracion2@enel.com";
                //pw = "Deloiyye2020mUkLb2eNfT3yBZnPSQMtAAum";
                //pw = "EnelBR_Migracion18244WNLv5JdO9wi6gh3QOTJq3Lup";
                
                //url = "https://enelsud.my.salesforce.com/services/Soap/u/39.0"; 
                url = "https://enelsud.my.salesforce.com/services/Soap/u/45.0";
            }

            if ("uat".Equals(ambiente.ToLower()))
            {
                //un = "gds.brasil@enel.com.uat";
                //pw = "Enel2019+";

                un = "teste.brasil@accenture.com.uat";
                pw = "Teste#02";

                //un = "integracionbrasil@enellatam.uat";
                //pw = "Deloitte01!";

                un = "migracionbrasiluat@enellatam.uat";
                pw = "Migracion2019kIVovRzBU2vKnLaFCcxm96ip";

                //url = "https://cs101.salesforce.com/services/Soap/u/39.0";
                url = "https://cs101.salesforce.com/services/Soap/u/45.0";
                //url = "https://cs102.salesforce.com/services/Soap/u/39.0";
            }

            if ("pre".Equals(ambiente.ToLower()))
            {
                un = "bruno.werneck@enel.com.pre";
                pw = "Enel20195nDw9fho4J9HTPfbDImIsvfW";

                un = "gds.brasil@enel.com.pre";
                pw = "Enel2020+q5fZ4oeKJzdZyfelba7goaYL";

                un = "brasilmigracion4@enel.com.preprod";
                pw = "Enel2020+q5fZ4oeKJzdZyfelba7goaYL";

                url = "https://enelsud--preprod.cs102.my.salesforce.com/services/Soap/u/45.0";
                url = "https://enelsud--preprod.my.salesforce.com/services/Soap/u/45.0";
            }
        }

        //private SforceService binding = null;
        private LoginResult loginResult = null;
        private String un = string.Empty;
        private String pw = string.Empty;

        public bool ValidarLogin(ref SforceService binding)
        {
            //Provide feed back while we create the web service binding
            Console.WriteLine("Autenticando...");
            
            if(binding == null)
                binding = new SforceService();

            binding.Url = this.url;

            #region Outros Logins
            //un = "br0072087097@enel.com";
            //pw = "pwdSf1804$";    OBSOLETO
            //pw = "Enel2018#";
            
            //un = "br0072087097@enel.com";
            //pw = "Enel2018#";

            //LOGINS DESENVOLVIMENTO ------------------------------
            
            //un = "cnt.bombero@deloitte.uat";
            //pw = "Ug3wN8No2Tox";

            //un = "integracionbrasil@enellatam.uat";
            //pw = "Deloitte01!";

            //un = "teste.brasil@accenture.com.uat";
            //pw = "Teste#01";

            //un = "guilherme.baccas@accenture.dtt";
            // pw = "salesforce#09";
            #endregion

            #region Habilitar Proxy
            string ipProxy = "10.71.1.2";
            //WebProxy oWebProxy = new WebProxy(ipProxy, 8080);
            //oWebProxy.BypassProxyOnLocal = true;
            //oWebProxy.Credentials = new NetworkCredential("br0072087097", "adlMt018");
            //Console.WriteLine(string.Format("Utilizando Proxy: {0}", oWebProxy.Address));
            //binding.Proxy = oWebProxy;
            #endregion

            if(binding.Proxy == null)
                Console.WriteLine("Sem Proxy");
            if(binding.Proxy != null)
                Console.WriteLine(string.Format("Via Proxy: {0}", ipProxy));

            //Setando o tempo de timeout, inclusive para as consultas.
            System.Net.ServicePointManager.SecurityProtocol = (SecurityProtocolType)3072;

            //Tentando Login depois do primeiro feedback
            try
            {
                Console.WriteLine("Logando...");
                binding.Timeout = int.MaxValue;
                loginResult = binding.login(un, pw);
            }
            catch (System.Web.Services.Protocols.SoapException e)
            {
                // This is likley to be caused by bad username or password
                Console.Write(e.Message + ", Por favor tente de novo.\n\nClique para continuar...");
                Console.ReadLine();
                return false;
            }
            catch (Exception e)
            {
                // This is something else, probably comminication
                Console.Write(e.Message + ", Por favor tente de novo.\n\nClique para continuar...");
                Console.ReadLine();
                return false;
            }

            Console.WriteLine("Url Serviço: " + loginResult.serverUrl);
            Console.WriteLine("Sessão: " + loginResult.sessionId);


            //Change the binding to the new endpoint
            binding.Url = loginResult.serverUrl;

            //Create a new session header object and set the session id to that returned by the login
            binding.SessionHeaderValue = new apex.SessionHeader();
            binding.SessionHeaderValue.sessionId = loginResult.sessionId;

            return true;
        }
    }
}

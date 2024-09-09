using IBM.Data.Informix;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Solus.Controle;
using System.Data;
using System.Configuration;
using System.IO;
//using Synapsis.FtpLib;
//using ExtracaoSalesForce.Modelo.SalesForce;
//using Chilkat;
using System.Diagnostics;
using SalesforceExtractor.Utils;
using SalesforceExtractor.Entidades;
using SalesforceExtractor.Dados.SalesForce;
using SalesforceExtractor.Entidades.Modif;
using SalesforceExtractor.Entidades.Enumeracoes;
using System.Threading;
using SalesforceExtractor.Dados;
using SalesforceExtractor.apex;
using SalesforceExtractor;
using System.Security.Authentication;

namespace BasicSample.Negocio
{
    public class NegocioBase : IDisposable
    {
        private static bool loggedIn = false;
        private static DateTime ultimoLogin = DateTime.MinValue;

        private Log log = new Log();
        private string codigoEmpresa = string.Empty;
        private string ambiente = string.Empty;
        private SforceService binding = null;
        private int tentativaConexao = 0;
        private int totalTentativas = 2;

        public NegocioBase(string ambiente, SforceService binding, string codigoEmpresa)
        {
            this.ambiente = ambiente;
            this.codigoEmpresa = codigoEmpresa;
            this.binding = binding;
        }

        public NegocioBase(string ambiente, SforceService binding)
        {
            this.ambiente = ambiente;
            this.binding = binding;
        }


        public void SetArquivoLog(string caminhoCompleto)
        {
            if (string.IsNullOrWhiteSpace(caminhoCompleto))
                return;

            this.log.NomeArquivo = caminhoCompleto;
        }


        public Log Log
        {
            get { return log; }
            set { log = value; }
        }

        public string CodigoEmpresa
        {
            get { return codigoEmpresa; }
            set { codigoEmpresa = value; }
        }

        public string Ambiente
        {
            get { return ambiente; }
            set { ambiente = value; }
        }

        public SforceService Binding
        {
            get
            {
                if (binding == null)
                    binding = new SforceService();
                
                return binding;
            }
            set { binding = value; }
        }

        /// <summary>
        /// Método de autenticação interna da classe quando for preciso acessar dados no Salesforce.
        /// </summary>
        public bool Autenticar()
        {
            if (DateTime.Now > ultimoLogin.AddHours(1) && !keepAlive())
            {
                while (tentativaConexao <= totalTentativas)
                {
                    tentativaConexao++;
                    if (tentativaConexao == totalTentativas)
                        return false;

                    Autenticacao auth = new Autenticacao(this.ambiente);
                    loggedIn = auth.ValidarLogin(ref this.binding);

                    if (!loggedIn)
                        throw new AuthenticationException(string.Format("Não foi possível autenticar o login em {0}\n", this.ambiente));
                    else
                    {
                        ultimoLogin = DateTime.Now;
                        break;
                    }
                }

                loggedIn = false;
                return true;
            }
            else
                return true;
        }


        private bool keepAlive()
        {
            string sqlBase = "select Id from Case limit 1";
            try
            {
                List<sObject> result = SalesforceDAO.Consultar(sqlBase, ref binding);
            }
            catch
            {
                return false;
            }
            return true;
        }


        public void Dispose()
        {
            this.binding = null;
            this.codigoEmpresa = null;
        }

    }
}

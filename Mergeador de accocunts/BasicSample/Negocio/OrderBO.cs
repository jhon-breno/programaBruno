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
using basicSample_cs_p;

namespace BasicSample.Negocio
{
    public class OrderBO : NegocioBase
    {
        public OrderBO(string ambiente, SforceService binding, string codigoEmpresa)
            : base(ambiente, binding, codigoEmpresa)
        {
        }


        public OrderBO(string ambiente, SforceService binding)
            : base(ambiente, binding, null)
        {
        }

        public void ZerarCharger(DateTime data, string logDiretorio)
        {
            if (string.IsNullOrWhiteSpace(logDiretorio))
                throw new ArgumentException("logDiretorio");

            this.Autenticar();

            //string nomeLog = string.Format(@"{0}Pangea.ZerarCharger_{1}_{2}.txt"
            //    , logDiretorio
            //    , data.ToString("yyyy-MM-dd")
            //    , DateTime.Now.ToString("yyyyMMdd"));

            //Arquivo arquivoLog = new Arquivo(nomeLog, char.MinValue, false);

            this.Log.NomeArquivo = string.Format(@"{0}Pangea.ZerarCharger_{1}_{2}.txt"
                , logDiretorio
                , data.ToString("yyyy-MM-dd")
                , DateTime.Now.ToString("yyyyMMdd"));
            this.Log.LogFull(string.Format("Processo iniciado - {0}", DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss")));

            sObject update;
            List<sObject> listaUpdate = new List<sObject>();
            SaveResult[] saveResults = null;
            SforceService binding = this.Binding;

            this.Log.LogFull(string.Format("{0} Consultando Orders com NE__RecurringChargeOv__c nulos..", DateTime.Now.ToString("HH:mm:ss")));
            List<OrderSalesforce> lstOrder = SalesforceDAO.GetOrdersChargeNulos(DateTime.Now, ref binding);

            this.Log.LogFull(string.Format("{0} Orders encontrados: {1}"
                    , DateTime.Now.ToString("HH:mm:ss")
                    , lstOrder.Count));

            //if (lstOrder == null || lstOrder.Count == 0)
            //{
            //    this.Log.LogFull(string.Format("{0} [ERRO]\tNão encontrados Orders informados no arquivo {1}"
            //        , DateTime.Now.ToString("HH:mm:ss")
            //        , arquivoLog.CaminhoCompleto));
            //}

            foreach (OrderSalesforce order in lstOrder)
            {
                update = new sObject();
                this.Log.LogFull(string.Format("{0} Order {1}"
                    , DateTime.Now.ToString("HH:mm:ss")
                    , order.Id
                    , order.Name));

                update.type = "NE__OrderItem__c";
                update.Id = order.Id;
                update.Any = new System.Xml.XmlElement[] {
                    SFDCSchemeBuild.GetNewXmlElement("NE__RecurringChargeOv__c", "0")
                };

                listaUpdate.Add(update);
            }

            //======================================
            //---------- ATUALIZAR ORDER -----------
            //--------------------------------------
            saveResults = SalesforceDAO.Atualizar(listaUpdate, 29, ref binding);
            List<Error[]> erros = saveResults.Where(sr => sr.errors != null).Select(s => s.errors).ToList();
            foreach (Error[] err in erros)
            {
                this.Log.LogFull(string.Format("{0} [ERRO]\t{1}"
                    , DateTime.Now.ToString("HH:mm:ss")
                    , string.Join(", ", err.Select(e => e.message))));
            }

            this.Log.LogFull(string.Format("Processo finalizado em {0}\n", DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss")));
        }
    }
}

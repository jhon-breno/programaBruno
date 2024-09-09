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
using System.Xml;

namespace BasicSample.Negocio
{
    public class AccountBO : NegocioBase
    {
        public AccountBO(string ambiente, string codigoEmpresa, SforceService binding)
            : base(ambiente, binding, codigoEmpresa)
        {
            this.CodigoEmpresa = codigoEmpresa;
        }



        /// <summary>
        /// Atualiza para vazio o Process Status para Casos candidatos ao Complete Order, do fluxo de Troca de Titularidade
        /// </summary>
        /// <param name="lstCaso"></param>
        /// <param name="logDiretorio"></param>
        /// <returns></returns>
        public void LimparProcessStatus(List<CaseSalesforce> lstCaso)
        {
            this.Autenticar();

            List<XmlElement> lstCampos = new List<XmlElement>();
            List<sObject> listaUpdate = new List<sObject>();
            sObject update = new sObject();
            SaveResult[] saveResults = null;
            List<Error[]> erros;

            SforceService binding = this.Binding;
            update.type = "Case";
            foreach (CaseSalesforce caso in lstCaso)
            {
                lstCampos.Add(SFDCSchemeBuild.GetNewXmlElement("CNT_ProcessStatus__c", " "));

                update.Id = caso.Id;
                update.Any = lstCampos.ToArray();

                listaUpdate.Add(update);

                saveResults = SalesforceDAO.Atualizar(listaUpdate, 29, ref binding);
                erros = saveResults.Where(sr => sr.errors != null).Select(s => s.errors).ToList();

                if (erros.Count > 0)
                    this.Log.LogFull(string.Format("{0}\t[CASO ERRO]\tId: {1}\tContaContrato: {2}: {3}"
                        , DateTime.Now.ToString("HH:mm:ss")
                        , caso.Id
                        , caso.CNT_Contract__c
                        , string.Join(", ", erros.FirstOrDefault().Select(e => e.message))));

                lstCampos.Clear();
                listaUpdate.Clear();
            }
        }



        public void MergearContas(Arquivo arquivo)
        {
            this.Autenticar();

            SforceService binding = this.Binding;
            this.Log.NomeArquivo = string.Format("{0}\\{1}_{2}_{3}.txt"
                , arquivo.Caminho
                , "MERGE_ACCOUNTS_RJ_"
                , this.Ambiente
                , DateTime.Now.ToString("yyyyMMdd_HHmmss")
                );

            this.Log.LogFull(string.Format("{0}\tMerge iniciado"
                    , DateTime.Now.ToString("HH:mm:ss")));

            List<ClienteSalesforce> listaArq = ArquivoLoader.GetAccountsParaMerge(arquivo);

            this.Log.Print(string.Format("{0}\tCarregando PoDs..", DateTime.Now.ToString("HH:mm:ss")));

            TrocaTitularidadeBO neg = new TrocaTitularidadeBO("prod", binding, "2005");
            neg.Autenticar();

            //SforceService bind = new SforceService();
            List<string> lstDocsMerged = new List<string>();

            foreach (ClienteSalesforce obj in listaArq)
            {
                if (lstDocsMerged.Contains(obj.Documento))
                    continue;

                if (string.IsNullOrWhiteSpace(obj.Documento))
                {
                    //Debugger.Break();
                    this.Log.LogFull(string.Format("{0} [validacao]\tExternalId {1}\tDocumento vazios"
                        , DateTime.Now.ToString("HH:mm:ss")
                        , obj.ExternalId));
                    continue;
                }
                
                lstDocsMerged.Add(obj.Documento);

                List<string> ids = listaArq.Where(acc => obj.ExternalId.Equals(acc.ExternalId))
                    .Select(d => d.IdConta)
                    .ToList();

                
                if(!Util.ValidaCNPJ(obj.Documento) && !Util.ValidaCPF(obj.Documento))
                {
                    //Debugger.Break();
                    this.Log.LogFull(string.Format("{0} [validacao]\tExternalId {1}\tDoc {2}"
                        , DateTime.Now.ToString("HH:mm:ss")
                        , obj.ExternalId
                        , obj.Documento));
                    continue;
                }


                if (ids.Count < 2)
                {
                    //Debugger.Break();
                    this.Log.LogFull(string.Format("{0} [acc unica]\tExternalId {1}\tDoc {2}"
                        , DateTime.Now.ToString("HH:mm:ss")
                        , obj.ExternalId
                        , obj.Documento));
                    continue;
                }

                string idPrincipal = ids.First();
                ids.Remove(idPrincipal);

                //if (ids.Count > 10)
                    //Debugger.Break();

                MergeResult[] r = SalesforceDAO.MergeContas(idPrincipal, ids, ref binding);

                if(r != null && r.Count() > 0 && !r[0].success)
                {
                    Error[] erros = r[0].errors;
                    if (erros != null && erros.Count() > 0)
                    {
                        this.Log.LogFull(string.Format("{0} [erro]\tExternalId {1}\t[{2}]:[{3}]\t{4}"
                            , DateTime.Now.ToString("HH:mm:ss")
                            , obj.ExternalId
                            , idPrincipal
                            , string.Join(",", ids)
                            , erros[0].message));
                        continue;
                    }
                }

                this.Log.LogFull(string.Format("{0} [ok]\tExternalId {1}\t[{2}]:[{3}]"
                    , DateTime.Now.ToString("HH:mm:ss")
                    , obj.ExternalId
                    , idPrincipal
                    , string.Join(",", ids)));
            }

            this.Log.LogFull(string.Format("{0}\tProcesso finalizado"
                    , DateTime.Now.ToString("HH:mm:ss")));

            this.Log.EscreverLog();
            
            #if(DEBUG)
            {
                Console.ReadKey();
            }
            #endif
        }



    }
}

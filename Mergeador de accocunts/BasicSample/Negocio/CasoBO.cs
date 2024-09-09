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
using System.Xml;

namespace BasicSample.Negocio
{
    public class CasoBO : NegocioBase
    {
        public CasoBO(string ambiente, string codigoEmpresa, SforceService binding)
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


        /// <summary>
        /// 
        /// </summary>
        /// <param name="lstCaso"></param>
        /// <param name="logDiretorio"></param>
        /// <returns></returns>
        public List<CaseSalesforce> IngressarCasoTrocaTitularidade(List<CaseSalesforce> lstCaso, string logDiretorio)
        {
            if (string.IsNullOrWhiteSpace(logDiretorio))
                throw new ArgumentException("logDiretorio");

            this.Autenticar();

            SforceService binding = this.Binding;
            
            List<CaseSalesforce> lstResult = new List<CaseSalesforce>();
            List<sObject> lstCasos = new List<sObject>();

            this.Log.NomeArquivo = string.Format(@"{0}Pangea.IngressarCasosTroca_{1}_{2}.txt"
                , logDiretorio
                , DateTime.Now.ToString("yyyy-MM-dd")
                , DateTime.Now.ToString("yyyyMMdd"));
            this.Log.LogFull(string.Format("Processo iniciado - {0}", DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss")));

            SalesforceExtractor.Negocio neg = new SalesforceExtractor.Negocio("uat", binding, this.CodigoEmpresa);

            #region Criar CASE
            foreach (CaseSalesforce caso in lstCaso)
            {
                CaseSalesforce novoCaso = caso;
                IngressarCaso(ref novoCaso);

                this.Log.LogFull(string.Format("Novo Caso: {0}", novoCaso.Id));

                #region CRIAR Order, OrderItem, OrderItemAttribute
                
                AssetDTO asset = SalesforceDAO.GetAssetsPorId(string.Concat("'", caso.AssetId, "'"), ref binding).FirstOrDefault();
                List<OrderItemAttributeSalesforce> orderItemAttr = SalesforceDAO.GetOrderItemsAttributePorId(caso.NE__Order_Config__c, ref binding);
                OrderSalesforce novoOrder = neg.IngressarOrder(asset, orderItemAttr.FirstOrDefault(), novoCaso.Id);
                #endregion

                caso.CNT_Quote__c = novoOrder.Id;
                caso.NE__Order_Config__c = novoOrder.Id;

                #region CRIAR CONTRACT
                ContractSalesforce contrato = new ContractSalesforce();
                contrato.ExternalId = caso.Contract.ExternalId;
                contrato.Account = caso.Contract.Account;
                contrato.Status = "Draft";
                contrato.DataInicio = DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss.000+0000");
                contrato.ContractType = caso.Contract.ContractType;
                contrato.Company_ID__c = this.CodigoEmpresa.Equals("2003") ? "COELCE" : this.CodigoEmpresa.Equals("2005") ? "AMPLA" : this.CodigoEmpresa.Equals("2018") ? "CELG" : "INVALIDO";
                contrato.RecordTypeId = caso.Contract.RecordTypeId;
                contrato.CNT_Case__c = caso.Id;
                contrato.CNAE = caso.Contract.CNAE;
                contrato.CNT_Quote__c = caso.CNT_Quote__c;
                
                neg.IngressarContract(ref contrato);
                #endregion

                this.Log.LogFull(string.Format("Novo Contrato: {0}", contrato.Id));


                //======================================
                //------ INGRESSAR BILLING PROFILE -----
                //======================================
                #region CRIAR BILLING PROFILE
                
                //TODO: validar Billing Id

                //BillingSalesforce novoBilling = SalesforceDAO.GetBillingProfileById(caso.Billing.Id, ref binding).Where(x => (x.Company__c == "COELCE" || x.Company__c == "2003")).FirstOrDefault();
                //BillingSalesforce novoBilling = SalesforceDAO.GetBillingProfileByContractId(contrato.Id, ref binding).Where(x => (x.Company__c == "COELCE" || x.Company__c == "2003")).FirstOrDefault();

                BillingSalesforce novoBilling = new BillingSalesforce();
                //novoBilling.CNT_GroupPayType__c = contrato.TipoImpressao;
                //novoBilling.CNT_Lot__c = contrato.Lote;
                //novoBilling.CNT_GroupClass__c = "";   //TODO:
                //novoBilling.CNT_Due_Date__c = contrato.DataVencimento.Replace("CP", string.Empty).Replace("cp", string.Empty);
                //novoBilling.Type__c = "Statement";
                //novoBilling.Address__c = contaColetivaPai.Address__c; //TODO:
                //novoBilling.BillingAddress__c = contaColetivaPai.PaymentAddress__c;   //TODO:
                novoBilling.Account__c = contrato.Account;
                novoBilling.RecordTypeId = caso.Billing.RecordTypeId;       // "01236000000On9B";
                novoBilling.CNT_Contract__c = contrato.Id ;
                novoBilling.Company__c = this.CodigoEmpresa;    //"2003".Equals(this.codigoEmpresa) ? "COELCE" : "2005".Equals(this.codigoEmpresa) ? "AMPLA" : "2018".Equals(this.codigoEmpresa) ? "CELG" : string.Empty;
                novoBilling.ExternalID__c = caso.Billing.ExternalID__c;
                novoBilling.AccountContract__c = contrato.Id;
                novoBilling.CNT_Due_Date__c = caso.Billing.CNT_Due_Date__c;
                novoBilling.PoDSF = caso.Billing.PoDSF;
                novoBilling.Address__c = caso.Billing.Address__c;
                novoBilling.BillingAddress__c = caso.Billing.BillingAddress__c;
                novoBilling.Type__c = caso.Billing.Type__c;
                try
                {
                    novoBilling.Id = neg.IngressarBillingProfileControladora(novoBilling);

                    this.Log.LogFull(string.Format("Novo Billing: {0}", novoBilling.Id));
                }
                catch (Exception ex)
                {
                    //log.LogFull(string.Format("[ERRO Billing] {0} {1}", ex.Message, ex.StackTrace));
                    continue;
                }

                #endregion

                #region Criar ContractLine

                //ingressa um novo ContractLine somente se o Billing for novo, com as propriedades preenchidas.  
                //Ignora se for um já existente, que a consulta 'GetBillingsPorNumeroCliente' não preenche o campo ExternalID__c
                if (!string.IsNullOrWhiteSpace(novoBilling.ExternalID__c))
                {
                    ContractLineItemSalesforce novoCline = SalesforceDAO.GetContractLineByContractId(contrato.Id, ref binding);
                    if (novoCline != null && !string.IsNullOrWhiteSpace(novoCline.Id))
                    {
                        try
                        {
                            binding.delete(new string[] { novoCline.Id }.ToArray());
                        }
                        catch (Exception ex)
                        {
                            //TODO: logar ;  abortar??
                        }
                    }
                    
                    novoCline.ContractId = contrato.Id;
                    novoCline.CNT_Status__c = "Active";
                    novoCline.BillingProfile__c = novoBilling.Id;

                    //======================================
                    //------ INGRESSAR CONTRACT LINE -------
                    //======================================
                    try
                    {
                        novoCline.Id = neg.IngressarContractLineControladora(ref novoCline);
                        caso.ContractLineItem = novoCline;
                        
                        this.Log.LogFull(string.Format("Novo CLine: {0}", novoCline.Id));
                    }
                    catch (Exception ex)
                    {
                        //log.LogFull(string.Format("[ERRO CLine] {0} {1}", ex.Message, ex.StackTrace));
                        continue;
                    }
                }
                #endregion CRIAR BILLING PROFILE


                //#region CRIAR CONTRACT LINE ITEM
                //ContractLineItemSalesforce novoCLine = new ContractLineItemSalesforce();
                ////neg.IngressarContractLine
                //#endregion CRIAR CONTRACT LINE ITEM

                caso.Contract = contrato;
                //caso.ContractLineItem = novoCLine;
                caso.Billing = novoBilling;

                sObject obj = new sObject();
                obj.type = "Case";
                obj.Id = caso.Id;

                obj.Any = new System.Xml.XmlElement[] {
                    SFDCSchemeBuild.GetNewXmlElement("CNT_Contract__c", caso.Contract.Id),
                    SFDCSchemeBuild.GetNewXmlElement("Status", "CNT0002")
                };
                lstCasos.Add(obj);

                if (lstCasos.Count == 30)
                {
                    SalesforceDAO.Atualizar(lstCasos, 30, ref binding);
                    lstCasos.Clear();
                }

                lstResult.Add(caso);
            }
            #endregion

            if (lstCasos.Count > 0)
                    SalesforceDAO.Atualizar(lstCasos, 30, ref binding);

            this.Log.LogFull(string.Format("Processo finalizado em {0}\n", DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss")));

            return lstResult;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="novoCaso"></param>
        /// <returns></returns>
        private CaseSalesforce IngressarCaso(ref CaseSalesforce novoCaso)
        {
            sObject sfObj = new sObject();
            List<sObject> lstObjetos = new List<sObject>();
            try
            {
                sfObj.type = "Case";
                sfObj.Any = new System.Xml.XmlElement[] {
                    SFDCSchemeBuild.GetNewXmlElement("Reason", novoCaso.Reason),
                    SFDCSchemeBuild.GetNewXmlElement("SubCauseBR__c", novoCaso.SubCauseBR__c),
                    SFDCSchemeBuild.GetNewXmlElement("AccountId", novoCaso.AccountId),
                    SFDCSchemeBuild.GetNewXmlElement("ContactId", novoCaso.ContactId),
                    SFDCSchemeBuild.GetNewXmlElement("AssetId", novoCaso.AssetId),
                    SFDCSchemeBuild.GetNewXmlElement("PointofDelivery__c", novoCaso.PointofDelivery__c),
                    SFDCSchemeBuild.GetNewXmlElement("RecordTypeId", novoCaso.RecordTypeId),
                    SFDCSchemeBuild.GetNewXmlElement("CNT_Contract__c", novoCaso.CNT_Contract__c),
                    SFDCSchemeBuild.GetNewXmlElement("Address__c", novoCaso.Address)
                };

                SforceService binding = this.Binding;
                lstObjetos.Add(sfObj);
                SaveResult[] saveResults = SalesforceDAO.InserirObjetosSF(lstObjetos, ref binding);
                SaveResult resultado = saveResults.First();
                if (resultado != null && resultado.errors != null && resultado.errors.Count() > 0)
                {
                    throw new Exception(string.Join(";", resultado.errors.Select(e => e.message).ToArray()));
                }
                novoCaso.Id = saveResults[0].id;
            }
            catch (Exception ex)
            {
                Debugger.Break();

                this.Log.Add(string.Format("\nErro ao ingressar Caso : {0}: {1} {2}\n", "", ex.Message, ex.StackTrace));
                Console.WriteLine(this.Log.GetUltimoLog());
            }
            finally
            {
                lstObjetos.Clear();
            }

            return novoCaso;
        }
    }
}

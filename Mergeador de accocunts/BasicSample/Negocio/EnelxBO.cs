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
    public class EnelxBO : NegocioBase
    {
        public EnelxBO(string ambiente, SforceService binding, string codigoEmpresa)
            : base(ambiente, binding, codigoEmpresa)
        {
        }


        public void CarregarProdutos(List<EnelxAsset> lstAsset, List<EnelxContract> lstContract, List<EnelxContractLine> lstCline, Arquivo log)
        {
            this.Autenticar();

            this.Log.NomeArquivo = log.CaminhoCompleto;
            SforceService binding = this.Binding;
            sObject update = new sObject();
            sObject upsert = new sObject();
            List<sObject> listaUpdate = new List<sObject>();
            List<sObject> listaInsert = new List<sObject>();
            UpsertResult[] saveResults = null;
            List<Error[]> erros;
            List<XmlElement> lstCampos = new List<XmlElement>();
            List<XmlElement> lstInsert = new List<XmlElement>();

            List<string> lstExternalId = new List<string>();

            this.Log.LogFull(string.Format("{0}\tENELX - ATUALIZAÇÃO DE PRODUTOS"
                    , DateTime.Now.ToString("HH:mm:ss")));

            this.Log.LogFull(string.Format("{0}\tTotal de Assets: {1}"
                    , DateTime.Now.ToString("HH:mm:ss")
                    , lstAsset.Count));

            int total = 0;
            string idAcc = string.Empty;
            string idPod = string.Empty;
            string idContract = string.Empty;
            string idAsset = string.Empty;
            string idCline = string.Empty;
            string idBill = string.Empty;


            this.Log.Print(string.Format("{0}\tCarregando Accounts..", DateTime.Now.ToString("HH:mm:ss")));
            Dictionary<string, string> allAccounts = SalesforceDAO.GetContasPorExternalId("'2003','COELCE'", lstAsset.Select(a => a.AccountExternalId).ToList(), ref binding).ToList().Select(b => new { b.ExternalId, b.Id }).ToDictionary(t => t.ExternalId, t => t.Id);

            //TODO: imprimir external ids com [DUPLIC]

            this.Log.Print(string.Format("{0}\tCarregando PoDs..", DateTime.Now.ToString("HH:mm:ss")));
            Dictionary<string, string> allPoDs = SalesforceDAO.GetPodsPorExternalId(lstAsset.Select(a => a.PointofDeliveryExternalId).ToList(), ref binding).ToList().Select(b => new { b.ExternalId, b.IdPod }).ToDictionary(t => t.ExternalId, t => t.IdPod);

            this.Log.Print(string.Format("{0}\tCarregando Assets..", DateTime.Now.ToString("HH:mm:ss")));
            Dictionary<string, string> allAssets = SalesforceDAO.GetAssetsPorExternalId(lstAsset.Select(a => a.ExternalId).ToList(), ref binding).ToList().Select(b => new { b.ExternalId, b.Id }).ToDictionary(t => t.ExternalId, t => t.Id);

            this.Log.Print(string.Format("{0}\tCarregando Asset x PoDs..", DateTime.Now.ToString("HH:mm:ss")));
            List<AssetDTO> lstAssetsPoD = SalesforceDAO.GetAssetsPorExternalId(lstAsset.Select(a => a.ExternalId).ToList(), ref binding).ToList();

            this.Log.Print(string.Format("{0}\tCarregando Contracts..", DateTime.Now.ToString("HH:mm:ss")));
            Dictionary<string, string> allContracts = SalesforceDAO.GetContratosPorExternalId(lstContract.Select(a => a.ExternalID__c).ToList(), ref binding).ToList().Select(b => new { b.ExternalId, b.Id }).ToDictionary(t => t.ExternalId, t => t.Id);

            this.Log.Print(string.Format("{0}\tCarregando Contract Lines..", DateTime.Now.ToString("HH:mm:ss")));
            Dictionary<string, string> allCLines = SalesforceDAO.GetContractLineByExternalId(lstCline.Select(a => a.ExternalId).ToList(), ref binding).ToList().Select(b => new { b.ExternalId__c, b.Id }).ToDictionary(t => t.ExternalId__c, t => t.Id);

            this.Log.Print(string.Format("{0}\tCarregando Billings..", DateTime.Now.ToString("HH:mm:ss")));
            Dictionary<string, string> allBillings = SalesforceDAO.GetBillingsPorExternalId(lstCline.Select(a => a.BillingExternalId).ToList(), ref binding).ToList().Select(b => new { b.ExternalID__c, b.Id }).ToDictionary(t => t.ExternalID__c, t => t.Id);

            //    List<ContractSalesforce> lst = SalesforceDAO.GetContratosPorExternalId(obj.ExternalID__c.Replace("\"", string.Empty), ref binding);

            //=======================================================================================
            //=======================================================================================
            #region ---------- ATUALIZAR ASSET ------------------------------------------------------

            foreach (EnelxAsset asset in lstAsset)
            {
                total++;
                lstExternalId.Clear();
                listaUpdate.Clear();
                //this.Log.Print(string.Format("{0}\tAsset {1}"
                //        , DateTime.Now.ToString("HH:mm:ss")
                //        , asset.ExternalId));

                update = new sObject();
                update.type = "Asset";
                lstCampos = new List<XmlElement>();

                //List<AssetDTO> lst = SalesforceDAO.GetAssetsPorExternalId(asset.ExternalId.Replace("\"", string.Empty), ref binding);
                idAsset = allAssets.ContainsKey(asset.ExternalId) ? allAssets[asset.ExternalId] : string.Empty;
                idAcc = allAccounts.ContainsKey(asset.AccountExternalId) ? allAccounts[asset.AccountExternalId] : string.Empty;
                idPod = allPoDs.ContainsKey(asset.PointofDeliveryExternalId) ? allPoDs[asset.PointofDeliveryExternalId] : string.Empty;

                //List<ClienteSalesforce> lstPod = SalesforceDAO.GetPodsPorExternalId(asset.PointofDeliveryExternalId.Replace("\"", string.Empty), ref binding);
                if (string.IsNullOrWhiteSpace(idPod))
                {
                    this.Log.LogFull(string.Format("{0} {1}\t[ASSET ERRO]\t{2} PoD não encontrado {3}"
                        , DateTime.Now.ToString("HH:mm:ss")
                        , total
                        , asset.ExternalId.Replace("\"", string.Empty)
                        , asset.PointofDeliveryExternalId));
                    continue;
                }
                if (string.IsNullOrWhiteSpace(idAcc))
                {
                    this.Log.LogFull(string.Format("{0} {1}\t[ASSET ERRO]\t{2} Account não encontrada {3}"
                        , DateTime.Now.ToString("HH:mm:ss")
                        , total
                        , asset.ExternalId.Replace("\"", string.Empty)
                        , asset.AccountExternalId));
                    continue;
                }


                if (string.IsNullOrWhiteSpace(idAsset))
                {
                    upsert = new sObject();
                    upsert.type = "Asset";
                    listaInsert.Clear();
                    lstInsert.Clear();

                    lstInsert.Add(SFDCSchemeBuild.GetNewXmlElement("ExternalId__c", asset.ExternalId));
                    lstInsert.Add(SFDCSchemeBuild.GetNewXmlElement("Name", asset.Name));
                    lstInsert.Add(SFDCSchemeBuild.GetNewXmlElement("Description", asset.Description));
                    lstInsert.Add(SFDCSchemeBuild.GetNewXmlElement("NE__Zip_Code__c", asset.NE__Zip_Code__c));
                    lstInsert.Add(SFDCSchemeBuild.GetNewXmlElement("NE__Description__c", asset.NE__Description__c));
                    lstInsert.Add(SFDCSchemeBuild.GetNewXmlElement("Status", asset.Status));
                    lstInsert.Add(SFDCSchemeBuild.GetNewXmlElement("SerialNumber", asset.SerialNumber));
                    lstInsert.Add(SFDCSchemeBuild.GetNewXmlElement("Company__c", asset.Company__c));
                    lstInsert.Add(SFDCSchemeBuild.GetNewXmlElement("AccountId", idAcc));
                    lstInsert.Add(SFDCSchemeBuild.GetNewXmlElement("PointofDelivery__c", idPod));

                    upsert.Any = lstInsert.ToArray();
                    listaInsert.Add(upsert);
                    asset.Id = SalesforceDAO.Upsert("ExternalId__c", listaInsert, 29, ref binding).First().id;

                    if(string.IsNullOrWhiteSpace(asset.Id))
                    {
                        this.Log.LogFull(string.Format("{0}\t[ASSET ERRO]\tErro ao criar novo Asset para {1}"
                            , DateTime.Now.ToString("HH:mm:ss")
                            , asset.ExternalId));
                        continue;
                    }

                    this.Log.LogFull(string.Format("{0} {1}\t[ASSET NOVO]\t{2} {3}"
                        , DateTime.Now.ToString("HH:mm:ss")
                        , total
                        , asset.ExternalId
                        , asset.Id));
                    
                    allAssets.Add(asset.ExternalId, asset.Id);
                    
                    upsert.Any = null;
                }
                else
                {
                    lstCampos.Add(SFDCSchemeBuild.GetNewXmlElement("ExternalId__c", asset.ExternalId));
                    lstCampos.Add(SFDCSchemeBuild.GetNewXmlElement("Name", asset.Name));
                    lstCampos.Add(SFDCSchemeBuild.GetNewXmlElement("Description", asset.Description));
                    lstCampos.Add(SFDCSchemeBuild.GetNewXmlElement("NE__Zip_Code__c", asset.NE__Zip_Code__c));
                    lstCampos.Add(SFDCSchemeBuild.GetNewXmlElement("NE__Description__c", asset.NE__Description__c));
                    lstCampos.Add(SFDCSchemeBuild.GetNewXmlElement("Status", asset.Status));
                    lstCampos.Add(SFDCSchemeBuild.GetNewXmlElement("SerialNumber", asset.SerialNumber));
                    lstCampos.Add(SFDCSchemeBuild.GetNewXmlElement("Company__c", asset.Company__c));
                    lstCampos.Add(SFDCSchemeBuild.GetNewXmlElement("AccountId", idAcc));
                    lstCampos.Add(SFDCSchemeBuild.GetNewXmlElement("PointofDelivery__c", idPod));

                    //update.Id = lst.FirstOrDefault().Id;
                    update.Any = lstCampos.ToArray();

                    lstExternalId.Add(asset.ExternalId);
                    listaUpdate.Add(update);

                    saveResults = SalesforceDAO.Upsert("ExternalId__c", listaUpdate, 29, ref binding);
                    erros = saveResults.Where(sr => sr.errors != null).Select(s => s.errors).ToList();

                    if (erros.Count <= 0)
                        lstExternalId.ForEach(e =>
                            this.Log.LogFull(string.Format("{0} {1}\t[ASSET OK]\t{2}"
                                , DateTime.Now.ToString("HH:mm:ss")
                                , total
                                , e))
                                );

                    if (erros.Count > 0)
                        this.Log.LogFull(string.Format("{0}\t[ASSET ERRO]\t{1} {2}"
                            , DateTime.Now.ToString("HH:mm:ss")
                            , asset.ExternalId
                            , string.Join(", ", erros.FirstOrDefault().Select(e => e.message))));
                }

                if (asset == null || string.IsNullOrWhiteSpace(asset.AccountExternalId))
                {
                    this.Log.LogFull(string.Format("{0} {1}\t[ASSET ERRO]\tAccount Id vazio para o Asset {2}"
                        , DateTime.Now.ToString("HH:mm:ss")
                        , total
                        , asset.ExternalId.Replace("\"", string.Empty)));
                    continue;
                }
            }
            #endregion
            //=======================================================================================

            this.Log.EscreverLog();

            //=======================================================================================
            //=======================================================================================
            #region ---------- ATUALIZAR CONTRACT  --------------------------------------------------

            total = 0;
            foreach (EnelxContract obj in lstContract)
            {
                total++;
                listaUpdate.Clear();
                lstExternalId.Clear();
                //this.Log.Print(string.Format("{0}\tContract {1}"
                //        , DateTime.Now.ToString("HH:mm:ss")
                //        , obj.ExternalID__c));

                if (string.IsNullOrWhiteSpace(obj.StartDate))
                {
                    this.Log.LogFull(string.Format("{0} {1}\t[CONTRACT]\t{2} Contrato sem Data"
                        , DateTime.Now.ToString("HH:mm:ss")
                        , total
                        , obj.ExternalID__c.Replace("\"", string.Empty)));
                    continue;
                }

                if (obj == null || string.IsNullOrWhiteSpace(obj.AccountExternalId.Replace("\"", string.Empty)))
                {
                    this.Log.LogFull(string.Format("{0} {1}\t[CONTRACT ERRO]\t{2} Contract sem ExternalId de Account"
                        , DateTime.Now.ToString("HH:mm:ss")
                        , total
                        , obj.Id));
                    continue;
                }
                update = new sObject();
                update.type = "Contract";
                lstCampos = new List<XmlElement>();

                //List<AccountSalesforce> lstAcc = SalesforceDAO.GetContasPorExternalId(lstAsset, ref binding);
                idAcc = allAccounts.ContainsKey(obj.AccountExternalId) ? allAccounts[obj.AccountExternalId] : string.Empty;
                idContract = allContracts.ContainsKey(obj.ExternalID__c) ? allContracts[obj.ExternalID__c] : string.Empty;

                if (string.IsNullOrWhiteSpace(idAcc))
                {
                    this.Log.LogFull(string.Format("{0} {1}\t[CONTRACT ERRO]\tAccount não encontrada {2}"
                        , DateTime.Now.ToString("HH:mm:ss")
                        , total
                        , obj.ExternalID__c.Replace("\"", string.Empty)));
                    continue;
                }

                if (string.IsNullOrWhiteSpace(idContract))
                {
                    upsert = new sObject();
                    upsert.type = "Contract";
                    listaInsert.Clear();
                    lstInsert.Clear();

                    lstInsert.Add(SFDCSchemeBuild.GetNewXmlElement("ExternalId__c", obj.ExternalID__c));
                    lstInsert.Add(SFDCSchemeBuild.GetNewXmlElement("Name", obj.Name));
                    lstInsert.Add(SFDCSchemeBuild.GetNewXmlElement("AccountId", idAcc));
                    lstInsert.Add(SFDCSchemeBuild.GetNewXmlElement("Description", obj.Description));
                    lstInsert.Add(SFDCSchemeBuild.GetNewXmlElement("Status", obj.Status));
                    lstInsert.Add(SFDCSchemeBuild.GetNewXmlElement("StartDate", obj.StartDate));
                    lstInsert.Add(SFDCSchemeBuild.GetNewXmlElement("Contract_Type__c", obj.Contract_Type__c));
                    lstInsert.Add(SFDCSchemeBuild.GetNewXmlElement("CompanyID__c", this.CodigoEmpresa)); 

                    upsert.Any = lstInsert.ToArray();
                    listaInsert.Add(upsert);
                    obj.Id = SalesforceDAO.Upsert("ExternalId__c", listaInsert, 29, ref binding).First().id;
                    allContracts.Add(obj.ExternalID__c, obj.Id);
                    upsert.Any = null;
                }

                //update.Id = lst.FirstOrDefault().Id;
                lstExternalId.Add(obj.ExternalID__c);
                update.Any = new System.Xml.XmlElement[] {
                    SFDCSchemeBuild.GetNewXmlElement("ExternalId__c", obj.ExternalID__c),
                    SFDCSchemeBuild.GetNewXmlElement("Name", obj.Name),
                    SFDCSchemeBuild.GetNewXmlElement("AccountId", idAcc),
                    SFDCSchemeBuild.GetNewXmlElement("Description", obj.Description),
                    SFDCSchemeBuild.GetNewXmlElement("Status", obj.Status),
                    SFDCSchemeBuild.GetNewXmlElement("StartDate", obj.StartDate),
                    SFDCSchemeBuild.GetNewXmlElement("Contract_Type__c", obj.Contract_Type__c)
                };

                listaUpdate.Add(update);

                saveResults = SalesforceDAO.Upsert("ExternalId__c", listaUpdate, 29, ref binding);
                erros = saveResults.Where(sr => sr.errors != null).Select(s => s.errors).ToList();

                if (erros == null || erros.Count == 0)
                    lstExternalId.ForEach(e =>
                    this.Log.LogFull(string.Format("{0} {1}\t[CONTRACT OK]\t{2}"
                        , DateTime.Now.ToString("HH:mm:ss")
                        , total
                        , e))
                        );

                if (erros != null && erros.Count > 0)
                    this.Log.LogFull(string.Format("{0} {3}\t[CONTRACT ERRO]\t{1} {2}"
                        , DateTime.Now.ToString("HH:mm:ss")
                        , obj.ExternalID__c
                        , string.Join(", ", erros.FirstOrDefault().Select(e => e.message))
                        , total));
            }
            #endregion
            //=======================================================================================

            this.Log.EscreverLog();

            //=======================================================================================
            //=======================================================================================
            #region ---------- ATUALIZAR CONTRACT LINE ----------------------------------------------
            total = 0;
            foreach (EnelxContractLine obj in lstCline)
            {
                total++;

                lstExternalId.Clear();
                listaUpdate.Clear();

                if (obj == null || string.IsNullOrWhiteSpace(obj.ContractExternalId.Replace("\"", string.Empty)))
                {
                    this.Log.LogFull(string.Format("{0} {1}\t[CONTRACT ERRO]\t{2} Contract Line sem ExternalId de Contract"
                        , DateTime.Now.ToString("HH:mm:ss")
                        , total
                        , obj.Id));
                    continue;
                }
                if (obj == null || string.IsNullOrWhiteSpace(obj.AssetExternalId.Replace("\"", string.Empty)))
                {
                    this.Log.LogFull(string.Format("{0} {1}\t[CONTRACT ERRO]\t{2} Contract Line sem ExternalId de Asset"
                        , DateTime.Now.ToString("HH:mm:ss")
                        , total
                        , obj.Id));
                    continue;
                }
                if (obj == null || string.IsNullOrWhiteSpace(obj.BillingExternalId.Replace("\"", string.Empty)))
                {
                    this.Log.LogFull(string.Format("{0} {1}\t[CONTRACT ERRO]\t{2} Contract Line sem ExternalId de Billing"
                        , DateTime.Now.ToString("HH:mm:ss")
                        , total
                        , obj.Id));
                    continue;
                }

                update = new sObject();
                update.type = "Contract_Line_Item__c";

                idContract = allContracts.ContainsKey(obj.ContractExternalId) ? allContracts[obj.ContractExternalId] : string.Empty;
                idAsset = allAssets.ContainsKey(obj.AssetExternalId) ? allAssets[obj.AssetExternalId] : string.Empty;
                idCline = allCLines.ContainsKey(obj.ExternalId) ? allCLines[obj.ExternalId] : string.Empty;
                idBill = allBillings.ContainsKey(obj.BillingExternalId) ? allBillings[obj.BillingExternalId] : string.Empty;

                //List<ContractSalesforce> lstCont = SalesforceDAO.GetContractByExternalId(obj.ContractExternalId.Replace("\"", string.Empty), ref binding);
                //List<ContractLineItemSalesforce> lst = SalesforceDAO.GetContractLineByExternalId(obj.ExternalId.Replace("\"", string.Empty), ref binding);
                //List<BillingSalesforce> lstBill = SalesforceDAO.GetBillingsPorExternalId(obj.BillingExternalId, ref binding);

                if (string.IsNullOrWhiteSpace(idContract) || string.IsNullOrWhiteSpace(idAsset))
                {
                    this.Log.LogFull(string.Format("{0} {1}\t[CONTRACT LINE ERRO]\t{2} {3} Contract - {4} Asset"
                        , DateTime.Now.ToString("HH:mm:ss")
                        , total
                        , obj.ExternalId.Replace("\"", string.Empty)
                        , string.IsNullOrWhiteSpace(idContract) ? "Sem" : "Possui"
                        , string.IsNullOrWhiteSpace(idAsset) ? "Sem" : "Possui"));
                    continue;
                }

                if (string.IsNullOrWhiteSpace(idBill))
                {
                    this.Log.LogFull(string.Format("{0} {1}\t[CONTRACT LINE ERRO]\t{2} Contract Line sem Billing"
                        , DateTime.Now.ToString("HH:mm:ss")
                        , total
                        , obj.ExternalId.Replace("\"", string.Empty)));
                    continue;
                    //if (string.IsNullOrWhiteSpace(idContract))
                    //{
                    //    upsert = new sObject();
                    //    upsert.type = "BillingProfile__c";
                    //    listaInsert.Clear();
                    //    lstInsert.Clear();

                    //    lstInsert.Add(SFDCSchemeBuild.GetNewXmlElement("ExternalId__c", obj.ExternalID__c));
                    //    lstInsert.Add(SFDCSchemeBuild.GetNewXmlElement("Name", obj.Name));
                    //    lstInsert.Add(SFDCSchemeBuild.GetNewXmlElement("AccountId", idAcc));
                    //    lstInsert.Add(SFDCSchemeBuild.GetNewXmlElement("Description", obj.Description));
                    //    lstInsert.Add(SFDCSchemeBuild.GetNewXmlElement("Status", obj.Status));
                    //    lstInsert.Add(SFDCSchemeBuild.GetNewXmlElement("StartDate", obj.StartDate));
                    //    lstInsert.Add(SFDCSchemeBuild.GetNewXmlElement("Contract_Type__c", obj.Contract_Type__c));

                    //    upsert.Any = lstInsert.ToArray();
                    //    listaInsert.Add(upsert);
                    //    obj.Id = SalesforceDAO.Upsert("ExternalId__c", listaInsert, 29, ref binding).First().id;
                    //    allContracts.Add(obj.ExternalID__c, obj.Id);
                    //    upsert.Any = null;
                    //}
                }


                lstExternalId.Add(obj.ExternalId);
                update.Any = new System.Xml.XmlElement[] {
                    SFDCSchemeBuild.GetNewXmlElement("ExternalId__c", obj.ExternalId),
                    SFDCSchemeBuild.GetNewXmlElement("Contract__c", idContract),
                    SFDCSchemeBuild.GetNewXmlElement("Asset__c", idAsset),
                    SFDCSchemeBuild.GetNewXmlElement("Billing_Profile__c", idBill)
                };

                listaUpdate.Add(update);

                saveResults = SalesforceDAO.Upsert("ExternalId__c", listaUpdate, 29, ref binding);
                erros = saveResults.Where(sr => sr.errors != null).Select(s => s.errors).ToList();

                if (erros.Count == 0)
                    lstExternalId.ForEach(e =>
                    this.Log.LogFull(string.Format("{0} {2}\t[CONTRACT LINE OK]\t{1}"
                        , DateTime.Now.ToString("HH:mm:ss")
                        , obj.ExternalId
                        , total)
                        ));

                if (erros.Count > 0)
                {
                    this.Log.LogFull(string.Format("{0} {3}\t[CONTRACT LINE ERRO]\t{1} {2}"
                    , DateTime.Now.ToString("HH:mm:ss")
                    , obj.ExternalId
                    , string.Join(", ", erros.FirstOrDefault().Select(e => e.message))
                    , total));
                }
            }
            #endregion
            //---------------------------------------------------------------------------------------

            this.Log.LogFull(string.Format("{0}\tProcesso finalizado"
                    , DateTime.Now.ToString("HH:mm:ss")));

            this.Log.EscreverLog();
        }
    }
}

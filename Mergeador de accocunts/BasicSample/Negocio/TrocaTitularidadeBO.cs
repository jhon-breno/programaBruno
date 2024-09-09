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
using System.Globalization;

namespace BasicSample.Negocio
{
    public class TrocaTitularidadeBO : NegocioBase
    {
        List<ContractLineItemSalesforce> lstContratos;

        #region Resumo Final
        int total = 0;
        int resumoAssetOk = 0;
        int resumoInconsistencia = 0;
        int resumoSemAssetAtivoGB = 0;
        int resumoSemAssetEBilling = 0;
        int resumoMaisUmAsset = 0;
        int resumoNomeErrado = 0;
        int resumoContratoNaoAtivado = 0;
        #endregion

        #region Complete Order
        List<string> lstCompleteOrder = new List<string>();
        bool waitProcess = false;
        StringBuilder contratosCompleteOrder = new StringBuilder();
        int qtdCompleteOrdersConcorrentes = 50;
        #endregion

        public TrocaTitularidadeBO(string ambiente, SforceService binding, string codigoEmpresa)
            : base(ambiente, binding, codigoEmpresa)
        {
        }


        public void CorrigirTrocaIncompleta(Arquivo arquivoTrocas, Arquivo arquivoErros, Arquivo arquivoNovasLig)
        {
            this.Autenticar();
          
            this.Log.NomeArquivo = string.Format(@"{0}\{1}_{2}_FINAL.txt", arquivoTrocas.Caminho, arquivoTrocas.Nome, DateTime.Now.ToString("yyyyMMdd_HHmm"));
            this.Log.LogFull(string.Format("Processo iniciado - {0}", DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss")));

            List<string> lstClientes = ArquivoLoader.GetNumeroClientes(arquivoTrocas, false);
            lstClientes.AddRange(ArquivoLoader.GetNumeroClientes(arquivoErros, true));
            lstClientes.AddRange(ArquivoLoader.GetNumeroClientes(arquivoNovasLig, true));
            
            if (lstClientes.Count == 0)
            {
                this.Log.LogFull(string.Format("{0} Nenhum contrato/cliente informado.   Arquivos:\n{1}\n{2}\n{3}"
                        , DateTime.Now.ToString("HH:mm:ss")
                        , arquivoTrocas.CaminhoCompleto
                        , arquivoErros.CaminhoCompleto
                        , arquivoNovasLig.CaminhoCompleto));
                return;
            }

            this.Log.LogFull(string.Format("{0} Total de clientes: {1}.   Arquivos:\n{2}\n{3}\n{4}"
                    , DateTime.Now.ToString("HH:mm:ss")
                    , lstClientes.Count.ToString("N0")
                    , arquivoTrocas.CaminhoCompleto
                    , arquivoErros.CaminhoCompleto
                    , arquivoNovasLig.CaminhoCompleto));

            sObject update = new sObject();
            List<sObject> listaUpdate = new List<sObject>();
            SaveResult[] saveResults = null;
            List<sObject> listaDelete = new List<sObject>();

            SforceService binding = this.Binding;

            this.Log.LogFull(string.Format("{0} Consultando contratos..", DateTime.Now.ToString("HH:mm:ss")));
            lstContratos = SalesforceDAO.GetContractLinesByNumeroContrato(string.Concat("'", this.CodigoEmpresa, "'"), lstClientes, ref binding);
            this.total = lstContratos.Count;

            this.Log.LogFull(string.Format("{0} Total de contratos: {1}\n"
                    , DateTime.Now.ToString("HH:mm:ss")
                    , lstContratos.Count.ToString("N0")));

            foreach (string perdido in lstClientes.Where(x => lstContratos.Where(c => c.ContractNumber == x.Replace("'", "")).ToArray().Count() == 0))
            {
                this.Log.LogFull(string.Format("{0} [ERRO]\t{1} Contrato nao encontrado pela consulta-base"
                    , DateTime.Now.ToString("HH:mm:ss")
                    , perdido.Replace("'", "")));
            }

            if (lstContratos == null || lstContratos.Count == 0)
            {
                this.Log.LogFull(string.Format("{0} [ERRO]\tNão encontrados Contratos informados nos arquivos\t{1}\t{2}"
                    , DateTime.Now.ToString("HH:mm:ss")
                    , arquivoTrocas.CaminhoCompleto
                    , arquivoErros.CaminhoCompleto));
                return;
            }

            List<Error[]> erros;
            int totalContratos = 0;

            foreach (ContractLineItemSalesforce contrato in lstContratos)
            {
                totalContratos++;

                if (!string.IsNullOrWhiteSpace(contrato.ContractStatus) && !"Activated".Equals(contrato.ContractStatus))
                {
                    this.Log.LogFull(string.Format("{0} {1}\t[ERRO]\tContrato {2}\tnão Ativado.  Id '{3}' Status '{4}'"
                        , DateTime.Now.ToString("HH:mm:ss")
                        , totalContratos.ToString("N0")
                        , contrato.ContractNumber
                        , contrato.Id
                        , contrato.ContractStatus));
                    resumoContratoNaoAtivado++;
                    //this.Log.LogFull(string.Format("Contratos: {0} Resumos: {1}", totalContratos, totalResumos()));
                    continue;
                }

                List<ContractLineItemSalesforce> controleContratos = lstContratos.Where(x => x.ContractNumber == contrato.ContractNumber).ToList();

                if (controleContratos.Where(x => x.AssetStatus.Equals("Activated") || "Active".Equals(x.AssetStatus)).Where(y => y.AssetName.Equals("Grupo B")).Select(a => a.AssetId).Distinct().ToList().Count() > 1)
                {
                    this.Log.LogFull(string.Format("{0}\t[ERRO]\tContrato {1}\tMais de 1 ContractLine (Grupo B e Ativo) para o cliente"
                        , DateTime.Now.ToString("HH:mm:ss")
                        , contrato.ContractNumber));
                    resumoMaisUmAsset++;
                    //this.Log.LogFull(string.Format("Contratos: {0} Resumos: {1}", totalContratos, totalResumos()));
                    continue;
                }

                #region Outros Assets
                List<AssetDTO> lstOutrosAssets = SalesforceDAO.GetAssetsPorNumeroCliente(CodigoEmpresa, contrato.BillingNumeroCliente, ref binding);

                if (string.IsNullOrWhiteSpace(contrato.AssetId))
                {
                    if (string.IsNullOrWhiteSpace(contrato.BillingId))
                    {
                        this.Log.LogFull(string.Format("{0} {1}\t[ERRO]\tContrato\t{2}\t'{4}' Status {3} Contract Line sem Asset e Billing"
                            , DateTime.Now.ToString("HH:mm:ss")
                            , totalContratos.ToString("N0")
                            , contrato.ContractNumber
                            , contrato.ContractStatus
                            , contrato.Id));
                        resumoSemAssetEBilling++;
                        //this.Log.LogFull(string.Format("Contratos: {0} Resumos: {1}", totalContratos, totalResumos()));
                        continue;
                    }

                    AssetDTO assetApto;

                    if (lstOutrosAssets == null || lstOutrosAssets.Count == 0)
                    {
                        #region Nenhum Asset com Numero Cliente

                        //Buscar Asset por Numero Contrato, sem o PoD associado
                        List<AssetDTO> assetsPorContrato = SalesforceDAO.GetAssetsPorNumeroContrato(contrato.ContractNumber, ref binding);

                        assetApto = assetsPorContrato
                            .Where(a => a.Status.Equals("Pending"))
                            .OrderByDescending(p => DateTime.ParseExact(p.DataCriacao,  "yyyy-MM-ddTHH:mm:ss.000Z", CultureInfo.GetCultureInfo("pt-BR")))
                            .FirstOrDefault();

                        if (assetApto != null && !string.IsNullOrWhiteSpace(assetApto.Id))
                        {
                            foreach (AssetDTO asset in assetsPorContrato)
                            {
                                if (asset.Id.Equals(assetApto.Id))
                                    continue;
                                
                                this.Log.LogFull(string.Format("{0} {1}\t[DELETE ASSET]\tContrato {2}\tCLine {3} AssetId Sem PoD {4} BillingId {5} ContactId {6}"
                                    , DateTime.Now.ToString("HH:mm:ss")
                                    , totalContratos.ToString("N0")
                                    , contrato.ContractNumber
                                    , contrato.Id
                                    , asset.Id
                                    , contrato.BillingId
                                    , contrato.ContactId));
                            }
                        }
                        else
                        {
                            this.Log.LogFull(string.Format("{0} {1}\t[COMPLETE_ORDER]\tContrato {2}\t(*)Sem Asset|Ativo|Grupo B.\tAccount Billing: {3}"
                                , DateTime.Now.ToString("HH:mm:ss")
                                , totalContratos.ToString("N0")
                                , contrato.ContractNumber
                                , contrato.BillingId));

                            lstCompleteOrder.Add(contrato.ContractNumber);

                            resumoSemAssetAtivoGB++;
                            
                            //this.Log.LogFull(string.Format("Contratos: {0} Resumos: {1}", totalContratos, totalResumos()));
                            continue;
                        }

                        #endregion Nenhum Asset com Numero Cliente
                    }
                    else
                    {
                        //(lstOutrosAssets.Where(s => "Activated".Equals(s.Status) || "Active".Equals(s.Status)).Where(x => "Grupo B".Equals(x.Name)).Count() == 0)
                        #region Asset com mesmo número de cliente
                        //----------------------------------------
                        assetApto = lstOutrosAssets.Where(s => "Activated".Equals(s.Status) || "Active".Equals(s.Status))
                            .Where(x => "Grupo B".Equals(x.Name)).ToList()
                            .OrderByDescending(p => DateTime.ParseExact(p.DataCriacao,  "yyyy-MM-ddTHH:mm:ss.000Z", CultureInfo.GetCultureInfo("pt-BR")))
                            .FirstOrDefault();

                        if((assetApto == null || string.IsNullOrWhiteSpace(assetApto.Id)) && !string.IsNullOrWhiteSpace(contrato.AccountIdBilling))
                        {
                                //Identifica o Asset cuja Conta associada é a mesma do Contrato
                                assetApto = lstOutrosAssets.Where(c => c.AccountId.Equals(contrato.AccountIdBilling))
                                    .OrderByDescending(p => DateTime.ParseExact(p.DataCriacao,  "yyyy-MM-ddTHH:mm:ss.000Z", CultureInfo.GetCultureInfo("pt-BR")))
                                    .FirstOrDefault();
                        }
                        
                        if(assetApto == null || string.IsNullOrWhiteSpace(assetApto.Id))
                        {
                            this.Log.LogFull(string.Format("{0} {1}\t[COMPLETE_ORDER]\tContrato {2}\tNenhum Asset Ativo disponível\tAccount Billing: {3}"
                                , DateTime.Now.ToString("HH:mm:ss")
                                , totalContratos.ToString("N0")
                                , contrato.ContractNumber
                                , contrato.BillingId));
                            lstCompleteOrder.Add(contrato.ContractNumber);

                            resumoSemAssetAtivoGB++;
                            //this.Log.LogFull(string.Format("Contratos: {0} Resumos: {1}", totalContratos, totalResumos()));
                            continue;
                        }

                        #endregion Asset com mesmo numero de cliente

                        contrato.AssetId = assetApto.Id;
                        contrato.AssetName = assetApto.Name;
                        contrato.AccountIdAsset = assetApto.AccountId;
                    }


                    if (string.IsNullOrWhiteSpace(assetApto.Id))
                    {
                        this.Log.LogFull(string.Format("{0} {1}\t[ERRO SEM ASSET]\tContrato {2}\tId Contrato {3}  Asset {4} AssetAccount {5} BillingAccount {6}"
                            , DateTime.Now.ToString("HH:mm:ss")
                            , totalContratos.ToString("N0")
                            , contrato.ContractNumber
                            , contrato.ContractId
                            , contrato.AssetId
                            , (contrato.AccountIdAsset == null) ? " NULO" : contrato.AccountIdAsset
                            , contrato.AccountIdBilling));

                        resumoSemAssetAtivoGB++;
                        //this.Log.LogFull(string.Format("Contratos: {0} Resumos: {1}", totalContratos, totalResumos()));
                        continue;
                    }

                    //=============================================
                    //---------- ASSOCIAR POD AO ASSET ------------
                    //---------------------------------------------
                    listaUpdate.Clear();
                    update.type = "Asset";
                    update.Id = assetApto.Id;
                    update.Any = new System.Xml.XmlElement[] {};


                    if (string.IsNullOrWhiteSpace(contrato.PointOfDelivery))
                    {
                        List<XmlElement> temp = update.Any.ToList();
                        temp.Add(SFDCSchemeBuild.GetNewXmlElement("PointofDelivery__c", contrato.PointOfDelivery));
                        update.Any = temp.ToArray();
                    }

                    if (!"Active".Equals(assetApto.Status))
                    {
                        List<XmlElement> temp = update.Any.ToList();
                        temp.Add(SFDCSchemeBuild.GetNewXmlElement("NE__Status__c", "Active"));
                        update.Any = temp.ToArray();
                    }

                    if (update.Any.Count() > 0)
                    {
                        listaUpdate.Add(update);
                        saveResults = SalesforceDAO.Atualizar(listaUpdate, 29, ref binding);
                        List<Error[]> err = saveResults.Where(sr => sr.errors != null).Select(s => s.errors).ToList();

                        foreach (Error[] er in err)
                        {
                            this.Log.LogFull(string.Format("{0} [ERRO ASSET]\t{1}"
                                , DateTime.Now.ToString("HH:mm:ss")
                                , string.Join(", ", er.Select(e => e.message))));
                            totalContratos--;
                            continue;
                        }
                    }

                    contrato.AssetId = assetApto.Id;
                    contrato.AssetName = assetApto.Name;
                    contrato.AccountIdAsset = assetApto.AccountId;


                    //======================================
                    //---------- ATUALIZAR CLINE -----------
                    //--------------------------------------
                    listaUpdate.Clear();
                    update.type = "Contract_Line_Item__c";
                    update.Id = contrato.Id;
                    update.Any = new System.Xml.XmlElement[] {
                                    SFDCSchemeBuild.GetNewXmlElement("Asset__c", contrato.AssetId)
                                };

                    listaUpdate.Add(update);
                    saveResults = SalesforceDAO.Atualizar(listaUpdate, 29, ref binding);
                    erros = saveResults.Where(sr => sr.errors != null).Select(s => s.errors).ToList();

                    if (erros == null || erros.Count == 0)
                    {
                        //this.Log.LogFull(string.Format("{0} [UPDATE]\tAsset {1} associado ao{2}"
                        //    , DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss")
                        //    , contrato.ContactId
                        //    , contrato.AccountIdAsset));
                    }

                    foreach (Error[] err in erros)
                    {
                        this.Log.LogFull(string.Format("{0} [ERRO CONTRACT LINE]\t{1}"
                            , DateTime.Now.ToString("HH:mm:ss")
                            , string.Join(", ", err.Select(e => e.message))));
                    }

                    this.Log.LogFull(string.Format("{0} {1}\t[NOVO ASSET]\tContrato {2}\tCLine {3} AssetId {4} BillingId {5} Asset Name '{6}'"
                        , DateTime.Now.ToString("HH:mm:ss")
                        , totalContratos.ToString("N0")
                        , contrato.ContractNumber
                        , contrato.Id
                        , contrato.AssetId
                        , contrato.BillingId
                        , contrato.AssetName));

                    resumoAssetOk++;
                }
                else
                {
                    #region Contract Line Possui Asset
                    StringBuilder dadosAtualizados = new StringBuilder();

                    List<System.Xml.XmlElement> lstXml = new List<XmlElement>();
                    if (!"01236000000On99AAC".Equals(contrato.AssetRecordType))
                    {
                        contrato.AssetRecordType = "01236000000On99AAC";
                        lstXml.Add(SFDCSchemeBuild.GetNewXmlElement("RecordTypeId", contrato.AssetRecordType));
                        dadosAtualizados.Append("RecordTypeId");
                    }

                    if (!"Active".Equals(contrato.AssetStatus))
                    {
                        contrato.AssetStatus = "Active";
                        lstXml.Add(SFDCSchemeBuild.GetNewXmlElement("NE__Status__c", contrato.AssetStatus));
                        
                        dadosAtualizados.Append(dadosAtualizados.Length > 0 ? "|" : string.Empty);
                        dadosAtualizados.Append("NE_Status");
                    }

                    if(lstXml.Count == 0)
                    {
                        if (assetTrocaValido(contrato.AssetName))
                        {
                            this.Log.LogFull(string.Format("{0} {1}\t[ASSETS OK]\tContrato {2}\tAssetId {3} Status {4}"
                                , DateTime.Now.ToString("HH:mm:ss")
                                , totalContratos.ToString("N0")
                                , contrato.ContractNumber
                                , contrato.AssetId
                                , contrato.AssetStatus));

                            resumoAssetOk++;
                            //this.Log.LogFull(string.Format("Contratos: {0} Resumos: {1}", totalContratos, totalResumos()));
                        }
                        else
                        {
                            this.Log.LogFull(string.Format("{0} {1}\t[COMPLETE_ORDER] Contrato Inconsistente\t{2}\tAsset Id '{3}' Asset Name '{4}'"
                                , DateTime.Now.ToString("HH:mm:ss")
                                , totalContratos.ToString("N0")
                                , contrato.ContractNumber
                                , contrato.AssetId
                                , contrato.AssetName));
                            lstCompleteOrder.Add(contrato.ContractNumber);
                            resumoNomeErrado++;
                            //this.Log.LogFull(string.Format("Contratos: {0} Resumos: {1}", totalContratos, totalResumos()));
                            continue;
                        }
                    }

                    if (lstXml.Count > 0)
                    {
                        //======================================
                        //---------- ATUALIZAR ASSET -----------
                        //--------------------------------------
                        listaUpdate.Clear();
                        update.type = "Asset";
                        update.Id = contrato.AssetId;
                        update.Any = lstXml.ToArray();

                        listaUpdate.Add(update);
                        saveResults = SalesforceDAO.Atualizar(listaUpdate, 29, ref binding);
                        erros = saveResults.Where(sr => sr.errors != null).Select(s => s.errors).ToList();

                        if (erros == null || erros.Count == 0)
                        {
                            this.Log.LogFull(string.Format("{0} {1}\t[ASSET CORRIGIDO]\tContrato {2}\tCLine {3} AssetId {4} BillingId {5} AssetStatus {6}.  Correção: {7}"
                                , DateTime.Now.ToString("HH:mm:ss")
                                , totalContratos.ToString("N0")
                                , contrato.ContractNumber
                                , contrato.Id
                                , contrato.AssetId
                                , contrato.BillingId
                                , contrato.AssetStatus
                                , dadosAtualizados.ToString()));

                            resumoAssetOk++;
                            //this.Log.LogFull(string.Format("Contratos: {0} Resumos: {1}", totalContratos, totalResumos()));
                        }

                        foreach (Error[] err in erros)
                        {
                            this.Log.LogFull(string.Format("{0}\t[ERRO ASSET]\t{1}"
                                , DateTime.Now.ToString("HH:mm:ss")
                                , string.Join(", ", err.Select(e => e.message))));
                        }
                    }
                    dadosAtualizados = null;

                    #endregion Contract Line Possui Asset
                }

                foreach (AssetDTO asset in lstOutrosAssets)
                {
                    #region Outros Assets
                    if (contrato.AccountIdAsset.Equals(asset.AccountId))
                        continue;

                    if (!contrato.AssetId.Equals(asset.Id) && !"Disconnected".Equals(asset.Status))
                    {
                        #region Retirar Asset não retirado

                        this.Log.LogFull(string.Format("{0} {1}\t[RETIRAR ASSET]\t(*)Contrato {2}\tAssetId {3}"
                            , DateTime.Now.ToString("HH:mm:ss")
                            , totalContratos.ToString("N0")
                            , contrato.ContractNumber
                            , asset.Id));

                        //======================================
                        //---------- ATUALIZAR ASSET -----------
                        //--------------------------------------
                        listaUpdate.Clear();
                        update.type = "Asset";
                        update.Id = asset.Id;
                        update.Any = new System.Xml.XmlElement[] {
                                SFDCSchemeBuild.GetNewXmlElement("NE__Status__c", "Disconnected")
                            };

                        listaUpdate.Add(update);
                        saveResults = SalesforceDAO.Atualizar(listaUpdate, 29, ref binding);
                        erros = saveResults.Where(sr => sr.errors != null).Select(s => s.errors).ToList();

                        if (erros == null || erros.Count == 0)
                        {
                            //this.Log.LogFull(string.Format("{0} [UPDATE]\tAsset {1} associado ao{2}"
                            //    , DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss")
                            //    , contrato.ContactId
                            //    , contrato.AccountIdAsset));
                        }

                        foreach (Error[] err in erros)
                        {
                            this.Log.LogFull(string.Format("{0}\t[ERRO ASSET]\t{1}"
                                , DateTime.Now.ToString("HH:mm:ss")
                                , string.Join(", ", err.Select(e => e.message))));
                        }

                        #endregion Asset não retirado
                    }

                    if (contrato.AssetId.Equals(asset.Id))
                    {
                        #region Asset escolhido

                        listaUpdate.Clear();
                        update.type = "Asset";
                        update.Id = contrato.AssetId;

                        List<XmlElement> any = new List<XmlElement>();

                        if (!"Active".Equals(contrato.AssetStatus))
                            any.Add(SFDCSchemeBuild.GetNewXmlElement("NE__Status__c", "Active"));

                        if ("Grupo B".Equals(contrato.AssetName))
                        {
                            if (contrato.AccountIdAsset == null || !contrato.AccountIdAsset.Equals(contrato.AccountIdBilling))
                            {
                                this.Log.LogFull(string.Format("{0} {1}\t[INCONSISTENCIA]\tContrato {2}\tId Contrato {3}  Asset {4} AssetAccount {5} BillingAccount {6}"
                                    , DateTime.Now.ToString("HH:mm:ss")
                                    , totalContratos.ToString("N0")
                                    , contrato.ContractNumber
                                    , contrato.ContractId
                                    , contrato.AssetId
                                    , (contrato.AccountIdAsset == null) ? " NULO" : contrato.AccountIdAsset
                                    , contrato.AccountIdBilling));
                                resumoInconsistencia++;
                                //this.Log.LogFull(string.Format("Contratos: {0} Resumos: {1}", totalContratos, totalResumos()));

                                //any.Add(SFDCSchemeBuild.GetNewXmlElement("ContactId", " "));  //2020-01-28 
                                any.Add(SFDCSchemeBuild.GetNewXmlElement("AccountId", contrato.AccountIdBilling));
                            }

                            //======================================
                            //---------- ATUALIZAR ASSET -----------
                            //--------------------------------------
                            update.Any = any.ToArray();
                            listaUpdate.Add(update);

                            if (listaUpdate.Count > 0)
                            {
                                saveResults = SalesforceDAO.Atualizar(listaUpdate, 29, ref binding);
                                erros = saveResults.Where(sr => sr.errors != null).Select(s => s.errors).ToList();
                                foreach (Error[] err in erros)
                                {
                                    this.Log.LogFull(string.Format("{0} [ERRO ASSET]\t{1}"
                                        , DateTime.Now.ToString("HH:mm:ss")
                                        , string.Join(", ", err.Select(e => e.message))));
                                }
                                if (erros != null && erros.Count == 0)
                                {
                                    contrato.AssetStatus = "Active";
                                }
                            }
                            //else
                            //    continue;
                            //======================================
                        }

                        this.Log.LogFull(string.Format("{0} {1}\t[ASSETS OK]\tContrato {2}\tAssetId {3} NomeAsset {4} Status {5}"
                        , DateTime.Now.ToString("HH:mm:ss")
                        , totalContratos.ToString("N0")
                        , contrato.ContractNumber
                        , contrato.AssetId
                        , contrato.AssetName
                        , contrato.AssetStatus));

                        #endregion Asset escolhido

                        resumoAssetOk++;
                        //this.Log.LogFull(string.Format("Contratos: {0} Resumos: {1}", totalContratos, totalResumos()));
                    }

                    #endregion Outros Assets
                }

                #endregion Outros Assets

                if (!assetTrocaValido(contrato.AssetName))
                {
                    this.Log.LogFull(string.Format("{0} {1}\t[COMPLETE_ORDER]\tContrato {2}\tAsset Id '{3}' Asset Name '{4}' não é 'Grupo A', 'Grupo B' ou 'Eletricity Service'"
                        , DateTime.Now.ToString("HH:mm:ss")
                        , totalContratos.ToString("N0")
                        , contrato.ContractNumber
                        , contrato.AssetId
                        , contrato.AssetName));
                    lstCompleteOrder.Add(contrato.ContractNumber);

                    resumoNomeErrado++;

                    if (totalResumos() > totalContratos)
                        resumoAssetOk--;

                    //this.Log.LogFull(string.Format("Contratos: {0} Resumos: {1}", totalContratos, totalResumos()));
                    continue;
                }

                if (this.Log.Tamanho > 1000)
                    this.Log.EscreverArquivo(this.Log.GetLog());
            }

            this.Log.EscreverArquivo(this.Log.GetLog());

            resumir();

            executarCompleteOrder();

            this.Log.LogFull(string.Format("Processo finalizado em {0}", DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss")));
        }

        private int totalResumos()
        {
            return (resumoAssetOk + resumoInconsistencia + resumoMaisUmAsset + resumoNomeErrado + resumoSemAssetAtivoGB + resumoSemAssetEBilling);
        }

        private bool assetTrocaValido(string assetName)
        {
            return ("Grupo B".Equals(assetName) || "Grupo A".Equals(assetName) || "Eletricity Service".Equals(assetName));
        }


        private void executarCompleteOrder()
        {
            waitProcess = true;
            int contCompleteOrder = 0;
            ParameterizedThreadStart t;
            foreach (string contrato in lstCompleteOrder)
            {
                if (contCompleteOrder < qtdCompleteOrdersConcorrentes)
                {
                    if (contratosCompleteOrder.Length > 0)
                        contratosCompleteOrder.Append(",");

                    contratosCompleteOrder.Append(contrato);
                    contCompleteOrder++;
                    continue;
                }

                this.Log.LogFull(string.Format("{0} Executando o Anonimos para {1}", DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss"), contratosCompleteOrder.ToString()));
                //C:\Projetos\Salesforce\Processos\AnonimoScriptSales\AnonimoScriptSales\bin\De bug\AnonimoScriptSales.exe "..\..\..\Scripts\gerarOrdersContrato.cs" "C:\temp\NovasLigacoes\191211\ALTA_UNICA_SAP_20191211_1242.txt" ex

                t = new ParameterizedThreadStart(ExecutarComando);
                t.Invoke(contratosCompleteOrder);

                contratosCompleteOrder.Clear();
                contCompleteOrder = 0;

                contratosCompleteOrder.Append(contrato);
                contCompleteOrder++;
            }

            if (contratosCompleteOrder.Length > 0)
            {
                this.waitProcess = true;
                this.Log.LogFull(string.Format("{0} Executando o Anonimos para {1}", DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss"), contratosCompleteOrder.ToString()));
                t = new ParameterizedThreadStart(ExecutarComando);
                t.Invoke(contratosCompleteOrder);
            }

            //OBSOLETO - 2020-01-23: após novo script do Complete order do Bruno
            //if (lstCompleteOrder.Count > 0)
            //{
            //    this.Log.LogFull(string.Format("{0}\tFinalizando o Complete Order..", DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss")));
            //    List<string> lstIdCasos = lstContratos.Where(c => lstCompleteOrder.Contains(c.ContractNumber)).Select(c => c.ContractCase).ToList();
            //    finalizarCompleteOrder(lstIdCasos);
            //}
        }


        /// <summary>
        /// Totalizadores do processo de correção das Trocas de Titularidade
        /// </summary>
        private void resumir()
        {
            this.Log.LogFull("*REVISÃO DAS TROCAS EFETIVADAS - DIARIO*");

            this.Log.LogFull("Total Contratos: " + this.total.ToString("N0"));

            if(resumoAssetOk > 0)
                this.Log.LogFull("[OK] Assets corretos: " + resumoAssetOk.ToString("N0"));

            if (resumoInconsistencia > 0)
                this.Log.LogFull("[OK] Inconsistencias corrigidas: " + resumoInconsistencia.ToString("N0"));

            if (resumoSemAssetAtivoGB > 0)
                this.Log.LogFull("[SALES] Sem Asset ou Asset Ativo/Grupo B: " + resumoSemAssetAtivoGB.ToString("N0") + " (CompleteOrder)");

            if (resumoSemAssetEBilling > 0)
                this.Log.LogFull("[SALES] Sem Asset nem Billing: " + resumoSemAssetEBilling.ToString("N0"));

            if (resumoMaisUmAsset > 0)
                this.Log.LogFull("[SALES] Mais de 1 Asset: " + resumoMaisUmAsset.ToString("N0"));

            if (resumoNomeErrado > 0)
                this.Log.LogFull("[SALES] Asset Nome errado: " + resumoNomeErrado.ToString("N0") + " (CompleteOrder)");

            if (resumoContratoNaoAtivado > 0)
                this.Log.LogFull("[NEGOCIO] Contrato Não Ativado: " + resumoContratoNaoAtivado.ToString("N0"));            
        }



        public void ExecutarComando(object obj)
        {
            if (obj == null)
                return;

            StringBuilder contratosCompleteOrder = (StringBuilder)obj;
            ProcessStartInfo startInfo = new ProcessStartInfo();
            startInfo.CreateNoWindow = true;
            startInfo.UseShellExecute = true;
            startInfo.WindowStyle = ProcessWindowStyle.Normal;

            startInfo.FileName = @"C:\Projetos\Salesforce\Processos\AnonimoScriptSales\AnonimoScriptSales\bin\Debug\AnonimoScriptSales.exe";
            startInfo.Arguments = string.Format("\"C:\\Projetos\\Salesforce\\Processos\\AnonimoScriptSales\\AnonimoScriptSales\\Scripts\\gerarOrdersContrato.cs\" {0} tr", contratosCompleteOrder.ToString());

            //startInfo.FileName = @"E:\Consoles\Salesforce\Pangea.Trocatitularidade\anonimos\AnonimoScriptSales.exe";
            //startInfo.Arguments = string.Format(@"E:\Consoles\Salesforce\Pangea.Trocatitularidade\anonimos\gerarOrdersContrato.cs {0} tr", contratosCompleteOrder.ToString());

            startInfo.FileName = ConfigurationManager.AppSettings.Get("AnonimosArquivo");
            startInfo.Arguments = string.Format("\"{0}\" {1} tr", ConfigurationManager.AppSettings.Get("AnonimosScriptTroca"), contratosCompleteOrder.ToString());
            try
            {
                // Start the process with the info we specified.
                // Call WaitForExit and then the using statement will close.
                using (Process exeProcess = Process.Start(startInfo))
                {
                    //Console.WriteLine("Argumentos: " + startInfo.Arguments);
                    Console.WriteLine();
                    if(this.waitProcess)
                        exeProcess.WaitForExit();
                }
                this.Log.LogFull(string.Format("{0} Complete Order executado para: {1}"
                    , DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss")
                    , contratosCompleteOrder.ToString()));
            }
            catch
            {
                //TODO: Log error.
                //TODO: zera os controles e contadores??
            }
        }



        /// <summary>
        /// Complementa o script do Complete Order, via Anônimos.
        /// </summary>
        /// <param name="lstIdCasos"></param>
        [Obsolete("2020-01-23: após novo script do Complete order do Bruno")]
        private void finalizarCompleteOrder(List<string> lstIdCasos)
        {
            SforceService binding = this.Binding;
            string nomeEmpresa = "2003".Equals(this.CodigoEmpresa) ? "'COELCE'" : "2005".Equals(this.CodigoEmpresa) ? "'AMPLA'" : "2018".Equals(this.CodigoEmpresa) ? "'CELG'" : "INDEFINIDO";

            List<CaseSalesforce> lstCasos = SalesforceDAO.GetCasosById(nomeEmpresa, lstIdCasos, ref binding);
            CasoBO casoBO = new CasoBO(this.Ambiente, this.CodigoEmpresa, binding);

            casoBO.LimparProcessStatus(lstCasos);
        }


        
        /// <summary>
        /// Atualiza o status dos Medidores (Device__c) de uma lista de PointofDelivery cujas trocas de titularidades não efetivou o status corretamente.
        /// </summary>
        /// <remarks>criado em: 2020-01-14</remarks>
        /// <param name="estadoMedidor"></param>
        public void AtualizarEstadoMedidores(Arquivo arqClientes, string estadoMedidor)
        {
            this.Autenticar();

            this.Log.NomeArquivo = string.Format(@"{0}\{1}_{2}_FINAL.txt", arqClientes.Caminho, arqClientes.Nome, DateTime.Now.ToString("yyyyMMdd_HHmm"));
            this.Log.LogFull(string.Format("Processo iniciado - {0}", DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss")));

            List<string> lstClientes = ArquivoLoader.GetNumeroClientes(arqClientes, true);
            if (lstClientes.Count == 0)
            {
                this.Log.LogFull(string.Format("{0} Nenhum contrato/cliente informado.   Arquivo:\n{1}}"
                        , DateTime.Now.ToString("HH:mm:ss")
                        , arqClientes.CaminhoCompleto));
                return;
            }

            this.Log.LogFull(string.Format("{0} Total de clientes: {1}."
                    , DateTime.Now.ToString("HH:mm:ss")
                    , lstClientes.Count.ToString("N0")));

            SforceService binding = this.Binding;

            //consultar Id Pods por numero cliente
            this.Log.LogFull(string.Format("{0} Consultando PoDs..", DateTime.Now.ToString("HH:mm:ss")));

            string codEmpresas = "2003".Equals(this.CodigoEmpresa) ? "'2003','COELCE'" : "'2005','AMPLA'";
            List<ClienteSalesforce> lstPods = SalesforceDAO.GetClientesPorNumero(codEmpresas, lstClientes, ref binding);

            this.Log.LogFull(string.Format("{0} Consultando Medidores..", DateTime.Now.ToString("HH:mm:ss")));
            List<DeviceSalesforce> lstMedidores = SalesforceDAO.GetDevicesPorIdPoD(codEmpresas, lstPods.Select(p => string.Concat("'", p.Id, "'")).ToList(), ref binding);

            lstMedidores.ForEach(x => this.Log.LogFull(string.Format("{0}|{1}|{2}|{3}|{4}|{5}", x.Id, x.PointOfDeliveryId, x.Numero, x.Estado, x.Nome, lstPods.Where( p => p.Id.Equals(x.PointOfDeliveryId)).Select(p2 => p2.NumeroCliente).FirstOrDefault())));

            //gerar saída com:
            //string	id pod
            //string	cliente
            //string	id medidor
            //string	numero medidor
            //bool	    atualizado
            //string	obs

        }



        /// <summary>
        /// Com base numa lista de clientes, identifica aqueles que não possuem medidor instalado no Salesforce
        /// </summary>
        /// <param name="arqContratos"></param>
        /// <param name="estadoMedidor"></param>
        public void IdentificarClientesSemMedidor(Arquivo arqContratos)
        {
            this.Autenticar();

            this.Log.NomeArquivo = string.Format(@"{0}\{1}_{2}_FINAL.txt", arqContratos.Caminho, arqContratos.Nome, DateTime.Now.ToString("yyyyMMdd_HHmm"));
            this.Log.LogFull(string.Format("Processo iniciado - {0}", DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss")));

            List<string> lstContratos = ArquivoLoader.GetNumeroClientes(arqContratos, false);
            if (lstContratos.Count == 0)
            {
                this.Log.LogFull(string.Format("{0} Nenhum contrato/cliente informado.   Arquivo:\n{1}}"
                        , DateTime.Now.ToString("HH:mm:ss")
                        , arqContratos.CaminhoCompleto));
                return;
            }

            this.Log.LogFull(string.Format("{0} Total de clientes: {1}"
                    , DateTime.Now.ToString("HH:mm:ss")
                    , lstContratos.Distinct().ToList().Count.ToString("N0")));

            SforceService binding = this.Binding;

            #region Consultar Pods por numero cliente

            this.Log.Print(string.Format("{0} Consultando PoDs..", DateTime.Now.ToString("HH:mm:ss")));

            string codEmpresas = "2003".Equals(this.CodigoEmpresa) ? "'2003','COELCE'" : "'2005','AMPLA'";
            List<ClienteSalesforce> lstPods = GetClientes(codEmpresas, lstContratos);

            List<string> _auxIdPods = lstPods.Where(p => !string.IsNullOrWhiteSpace(p.NumeroCliente)).Select(a => { return a.IdPod; }).ToList();
            List<string> _auxIdPodsVazios = lstPods.Where(p => string.IsNullOrWhiteSpace(p.NumeroCliente)).Select(a => { return a.IdPod; }).ToList();
            this.Log.LogFull(string.Format("{0} Total PoDs encontrados: {1}", DateTime.Now.ToString("HH:mm:ss"), _auxIdPods.Count.ToString("N0")));

            //Clientes sem cadastro no SF
            List<string> contratosEncontrados = lstPods.Where(p => !string.IsNullOrWhiteSpace(p.ContaContrato)).Select(p => p.ContaContrato).ToList();
            List<string> lstContratosNaoEncontrados = new List<string>();
            lstContratosNaoEncontrados.AddRange(lstContratos);
            lstContratosNaoEncontrados.RemoveAll(c => contratosEncontrados.Contains(c.Replace("'", "")));
            this.Log.LogFull(string.Format("{0} Total Contratos NÃO encontrados (2): {1}", DateTime.Now.ToString("HH:mm:ss"), lstContratosNaoEncontrados.Count.ToString("N0")));

            #endregion Clientes


            #region Consultar Medidores dos Pods cadastrados

            this.Log.Print(string.Format("{0} Consultando Medidores..", DateTime.Now.ToString("HH:mm:ss")));
            List<DeviceSalesforce> lstMedidores = SalesforceDAO.GetDevicesPorIdPoD(codEmpresas, lstPods.Where(pp => !string.IsNullOrWhiteSpace(pp.IdPod.Replace("'", "").Trim())).ToList().Select(p => string.Concat("'", p.IdPod, "'")).ToList(), ref binding);

            List<string> _auxPodsEncontrados = lstPods.Select(a => { return a.IdPod; }).Intersect(lstMedidores.Select(a => { return a.PointOfDeliveryId; })).ToList();
            this.Log.LogFull(string.Format("{0} Total PoDs com Medidor: {1}", DateTime.Now.ToString("HH:mm:ss"), _auxPodsEncontrados.Count.ToString("N0")));
            
            //Remove os External Ids encontrados e envia para a lista de Delta somente os não encontrados no SF
            if (_auxPodsEncontrados != null && _auxPodsEncontrados.Count > 0)
                _auxIdPods.RemoveAll(c => _auxPodsEncontrados.Contains(c));

            this.Log.LogFull(string.Format("{0} Total PoDs SEM Medidor: {1}", DateTime.Now.ToString("HH:mm:ss"), _auxIdPods.Count.ToString("N0")));

            #endregion Medidores

            //_auxIdPods.ForEach(p1 => lstClientesNaoEncontrados.AddRange(lstPods.Where(p2 => p2.IdPod.Equals(p1)).Select(p3 => p3.NumeroCliente).ToList()));
            //lstClientesNaoEncontrados.ForEach(x => this.Log.LogFull(x.Replace("'","")));

            this.Log.LogFull(string.Format("{0} Total INCONSISTENTE: {1}", DateTime.Now.ToString("HH:mm:ss"), lstContratosNaoEncontrados.Count.ToString("N0"))); 
            this.Log.LogFull(lstContratosNaoEncontrados);

            this.Log.LogFull(string.Format("{0} Total SEM Medidor: {1}", DateTime.Now.ToString("HH:mm:ss"), _auxIdPods.Count.ToString("N0")));
            _auxIdPods.ForEach(p => this.Log.LogFull(lstPods.Where(c => c.IdPod.Equals(p)).Select(p2 => { return string.Concat(p2.NumeroCliente, " - ", p2.IdPod); }).ToList()));


            this.Log.LogFull("Processo finalizado.");
        }


        /// <summary>
        /// Consulta clientes por Numero Cliente e por Numero Contrato, para casos de Troca de Titularidade.
        /// </summary>
        /// <param name="lstClientes"></param>
        /// <returns></returns>
        public List<ClienteSalesforce> GetClientes(string codigosEmpresaIn, List<string> lstClientes)
        {
            SforceService binding = this.Binding;
            List<ClienteSalesforce> lstPods = new List<ClienteSalesforce>();

            lstPods.AddRange(EntidadeConversor.FromBillingProfilesToPointOfDeliveries(SalesforceDAO.GetBillingsPorNumeroCliente(codigosEmpresaIn, lstClientes, ref binding)));
            //lstPods.AddRange(SalesforceDAO.GetClientesPorNumero(codigosEmpresaIn, lstClientes, ref binding));

            //lstPods.AddRange(SalesforceDAO.GetClientesPorNumeroContrato(codigosEmpresaIn, lstClientes, ref binding));
            
            binding = null;
            return lstPods;
        }
    }
}

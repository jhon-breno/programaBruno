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
using System.ComponentModel.DataAnnotations;

namespace BasicSample.Negocio
{
    public class CadastroBO : NegocioBase
    {
        public CadastroBO(string ambiente, SforceService binding, string codigoEmpresa)
            : base(ambiente, binding, codigoEmpresa)
        {
        }


        public List<AddressSalesforce> ObterEnderecosAlterados(DateTime dataInicio, DateTime dataFim)
        {
            string codigosEmpresa = "2003".Equals(this.CodigoEmpresa) ? "'2003','COELCE'" : "2005".Equals(this.CodigoEmpresa) ? "'2005','AMPLA'" : "'INDEFINIDO'";

            this.Autenticar();
            SforceService binding = this.Binding;
            List<AddressSalesforce> lst = SalesforceDAO.GetAlteracaoEnderecosPorPeriodo(codigosEmpresa, dataInicio.ToString("yyyy-MM-dd"), dataFim.ToString("yyyy-MM-dd"), ref binding);

            for (int i = 0; i < lst.Count; i++)
            {
                if (lst[i].Number__c.Length > 5)
                {
                    lst[i].Complemento += string.Concat(" ", lst[i].Number__c.Substring(5, lst[i].Number__c.Length - 5));
                    lst[i].Number__c = lst[i].Number__c.Substring(0, 5);
                }
            }

            return lst;
        }


        public void AtualizarRepercussao(List<string> lstPod)
        {
            this.Autenticar();

            SforceService binding = this.Binding;
            sObject update = new sObject();
            sObject upsert = new sObject();
            List<sObject> listaUpdate = new List<sObject>();
            List<sObject> listaInsert = new List<sObject>();
            UpsertResult[] upsertResults = null;
            List<Error[]> erros;
            List<XmlElement> lstCampos = new List<XmlElement>();
            List<XmlElement> lstInsert = new List<XmlElement>();

            List<string> lstExternalId = new List<string>();

            this.Log.LogFull(string.Format("{0}\tRepercussão iniciada"
                    , DateTime.Now.ToString("HH:mm:ss")));

            this.Log.LogFull(string.Format("{0}\tTotal de Clientes: {1}"
                    , DateTime.Now.ToString("HH:mm:ss")
                    , lstPod.Count));

            int total = 0;


            this.Log.Print(string.Format("{0}\tCarregando PoDs..", DateTime.Now.ToString("HH:mm:ss")));
            List<AssetDTO> lstAsset = SalesforceDAO.GetAssetsPorNumeroCliente(
                "'2003','COELCE'"
                , "'Active', 'Activated'"
                , lstPod, ref binding);

            foreach (AssetDTO obj in lstAsset)
            {
                total++;
                if ("true".Equals(obj.PointofDeliveryBaixaRendaFlag))
                {
                    //=======================================================================================
                    //=======================================================================================
                    #region ---------- ATUALIZAR POD ------------------------------------------------------
                    lstExternalId.Clear();
                    listaUpdate.Clear();

                    update = new sObject();
                    update.type = "PointOfDelivery__c";
                    lstCampos = new List<XmlElement>();

                    if (string.IsNullOrWhiteSpace(obj.PointofDeliveryExternalId))
                    {
                        this.Log.LogFull(string.Format("{0} {1}\t[POD ERRO]\t{2} PoD não encontrado {3}"
                            , DateTime.Now.ToString("HH:mm:ss")
                            , total
                            , obj.ExternalId.Replace("\"", string.Empty)
                            , obj.PointofDeliveryExternalId));
                        continue;
                    }

                    //TODO: campos a atualizar
                    lstCampos.Add(SFDCSchemeBuild.GetNewXmlElement("ExternalID__c", obj.PointofDeliveryExternalId));
                    lstCampos.Add(SFDCSchemeBuild.GetNewXmlElement("CNT_LowIncomeType__c", "false"));

                    update.Any = lstCampos.ToArray();
                    listaUpdate.Add(update);
                    lstExternalId.Add(obj.ExternalId);
                    upsertResults = SalesforceDAO.Upsert("ExternalId__c", listaUpdate, 29, ref binding);
                    erros = upsertResults.Where(sr => sr.errors != null).Select(s => s.errors).ToList();

                    if (erros.Count <= 0)
                        lstExternalId.ForEach(e =>
                            this.Log.LogFull(string.Format("{0} {1}\t[POD OK]\t{2}\t{3}"
                                , DateTime.Now.ToString("HH:mm:ss")
                                , total
                                , obj.NumeroCliente
                                , e))
                                );

                    if (erros.Count > 0)
                        this.Log.LogFull(string.Format("{0}\t[POD ERRO]\t{1} {2}"
                            , DateTime.Now.ToString("HH:mm:ss")
                            , obj.ExternalId
                            , string.Join(", ", erros.FirstOrDefault().Select(e => e.message))));
                    #endregion

                    this.Log.EscreverLog();
                }

                //=======================================================================================
                //=======================================================================================
                #region ---------- ATUALIZAR CONTRACT  --------------------------------------------------

                listaUpdate.Clear();
                lstExternalId.Clear();

                update = new sObject();
                update.type = "Contract";
                lstCampos = new List<XmlElement>();

                lstExternalId.Add(obj.ContractExternalId);
                update.Any = new System.Xml.XmlElement[] {
                    SFDCSchemeBuild.GetNewXmlElement("ExternalID__c", obj.ContractExternalId),
                    SFDCSchemeBuild.GetNewXmlElement("CNT_LowIncomeType__c", "false"),
                    SFDCSchemeBuild.GetNewXmlElement("CNT_Numero_Documento_Beneficio__c", "")
                };

                listaUpdate.Add(update);

                upsertResults = SalesforceDAO.Upsert("ExternalId__c", listaUpdate, 29, ref binding);
                erros = upsertResults.Where(sr => sr.errors != null).Select(s => s.errors).ToList();

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
                        , obj.ExternalId
                        , string.Join(", ", erros.FirstOrDefault().Select(e => e.message))
                        , total));

                #endregion

                this.Log.EscreverLog();

                //=======================================================================================
                //=======================================================================================
                #region ---------- ATUALIZAR ASSET/ORDER ATTRIBUTES -------------------------------------
                listaUpdate.Clear();

                foreach (string entidade in new List<string> { "NE__AssetItemAttribute__c", "NE__Order_Item_Attribute__c" })
                {
                    if("NE__Order_Item_Attribute__c".Equals(entidade) && string.IsNullOrWhiteSpace(obj.OrderId))
                    {
                        this.Log.Print(string.Format("[ERRO]\tFalta Configuration Order para o PoD {0}\tAsset {1}", obj.AccountExternalId, obj.ExternalId));
                        continue;
                    }
                    Dictionary<string, string> itemsBase = (entidade.Equals("NE__AssetItemAttribute__c")) ?
                        SalesforceDAO.GetAssetItemsAttributePorAsset(obj.ExternalId, ref binding) :
                        SalesforceDAO.GetOrderItemsAttributePorItem(obj.OrderId, ref binding);

                    if (itemsBase.Count == 0)
                    {
                        this.Log.Print(string.Format("[ERRO]\tNão encontrou atributos em {0} para PoD {1} OrderId {2}", entidade, obj.NumeroCliente, obj.OrderId));
                        continue;
                    }

                    //tipo de objeto do SalesForce
                    Type type = Type.GetType("SalesforceExtractor.Entidades.ItemAttribute");
                    foreach (PropertyInfo prop in type.GetProperties())
                    {
                        update = new sObject();
                        update.type = entidade;
                        #region AssetItemAttribute
                        foreach (DisplayAttribute idAttr in ((DisplayAttribute[])(prop.GetCustomAttributes(typeof(DisplayAttribute), true)))) //[0])).Name;
                        {
                            lstExternalId.Clear();

                            if (!"Classe BR".Equals(idAttr.Name) && !"SubClasse BR".Equals(idAttr.Name))
                                continue;

                            if (!itemsBase.ContainsKey(idAttr.Name))
                            {
                                this.Log.Print(string.Format("[ERRO]\tId Atributo vazio para {0} em {1}", idAttr.Name, entidade));
                                continue;
                            }

                            update.Id = itemsBase[idAttr.Name];
                            string valor = "Classe BR".Equals(idAttr.Name) ? "10 - Residencial" : "REPLN - Residencial Pleno";

                            update.Any = new System.Xml.XmlElement[] {
                                SFDCSchemeBuild.GetNewXmlElement("NE__Value__c", CultureInfo.CurrentCulture.TextInfo.ToTitleCase(valor))
                            };

                            lstExternalId.Add(itemsBase[idAttr.Name]);
                            listaUpdate.Add(update);

                            SaveResult[] saveResults = SalesforceDAO.Atualizar(listaUpdate, 29, ref binding);
                            erros = saveResults.Where(sr => sr.errors != null).Select(s => s.errors).ToList();

                            if (erros == null || erros.Count == 0)
                                lstExternalId.ForEach(e =>
                                    this.Log.LogFull(string.Format("{0} {1}\t[{2} OK]\t{3}\t{4}"
                                        , DateTime.Now.ToString("HH:mm:ss")
                                        , total
                                        , entidade
                                        , idAttr.Name
                                        , e))
                                        );
                            
                            foreach (Error[] err in erros)
                            {
                                this.Log.Print(string.Format("[ERRO ITEM ATTRIBUTES] {0}", string.Join(", ", err.Select(e => e.message))));
                            }

                            listaUpdate.Clear();
                        
                        }
                        #endregion
                        }
                }
                #endregion
            }

            this.Log.LogFull(string.Format("{0}\tProcesso finalizado"
                    , DateTime.Now.ToString("HH:mm:ss")));

            this.Log.EscreverLog();
        }


        public void AtualizarEstadoClientes(Arquivo arquivoSaida)
        {
            this.Autenticar();
            //SforceService binding = this.Binding;
            //sObject sfObj = null;
            //List<sObject> lstUpdate = new List<sObject>();
            //List<string> msgLogs = new List<string>();
            //var lstClientes = ArquivoLoader.GetNumeroClientes(arquivoSaida);

            //try
            //{
            //    this.Log.LogFull(string.Format("{0} [ARQUIVO]\t{1}", DateTime.Now.ToLongTimeString(), arquivoSaida.CaminhoCompleto));

            //    msgLogs.Clear();
            //    UpsertResult[] sfResult = null;

            //    foreach (string cli in lstClientes)
            //    {
            //        sfObj = new sObject();
            //        sfObj.type = "Invoice__c";
            //        sfObj.Id = cli.Rut__c;

            //        sfObj.Any = new System.Xml.XmlElement[] {
            //            SFDCSchemeBuild.GetNewXmlElement("Rut__c", cli.Rut__c),
            //            SFDCSchemeBuild.GetNewXmlElement("Status__c", cli.Status__c)
            //        };
            //        this.Log.LogFull((string.Format("{0} {1}", DateTime.Now.ToLongTimeString(), cli.ToString())));

            //        lstUpdate.Add(sfObj);
            //    }

            //    try
            //    {
            //        sfResult = SalesforceDAO.Upsert("Rut__c", lstUpdate, ref binding);
            //        List<Error[]> erros = sfResult.Where(sr => sr.errors != null).Select(s => s.errors).ToList();
            //        foreach (Error[] err in erros)
            //        {
            //            this.Log.LogFull((string.Format("{0} [ERRO] {1}", DateTime.Now.ToLongTimeString(), string.Join(", ", err.Select(e => e.message)))));
            //        }
            //        lstUpdate.Clear();
            //    }
            //    catch (Exception ex)
            //    {
            //        this.Log.LogFull((string.Format("{0} [ERRO UPSERT] {1}{2}{3}"
            //            , DateTime.Now.ToLongTimeString()
            //            , ex.GetType().ToString()
            //            , ex.Message
            //            , ex.StackTrace)));
            //    }
            //}
            //catch (Exception ex)
            //{
            //    this.Log.LogFull((string.Format("{0} [ERRO DESCONHECIDO] {1}{2}"
            //        , DateTime.Now.ToLongTimeString()
            //        , ex.Message
            //        , ex.StackTrace)));
            //}

        }
        }
}

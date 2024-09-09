using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IBM.Data.Informix;
using System.Globalization;
using System.Diagnostics;
using SalesforceExtractor.Dados.InformixBase;
using SalesforceExtractor.Entidades;
using SalesforceExtractor.Entidades.Enumeracoes;
using SalesforceExtractor.Utils;
using SalesforceExtractor.apex;
using basicSample_cs_p;
using System.Xml;
using SalesforceExtractor.Entidades.Modif;
using Solus.Controle.SalesForce;
using System.Configuration;
using System.Threading;
using SalesforceExtractor.Dados.Querier;
using System.Reflection;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;


namespace SalesforceExtractor.Dados.SalesForce
{
    public class SalesforceDAO
    {
        static bool _queryCompleta = false;
        static List<Atividade> _resultAtividades = new List<Atividade>();
        static Dictionary<Guid, SforceService> dicBindings = new Dictionary<Guid, SforceService>();
        private string codigoEmpresa;

        public SalesforceDAO(string codigoEmpresa)
        {
            this.codigoEmpresa = codigoEmpresa;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="numeroDocumento"></param>
        /// <param name="codigoEmpresa"></param>
        /// <returns></returns>
        public static ClienteSalesforce GetContatoPorDocumento(string codigoEmpresa, string numeroDocumento, ref SforceService binding)
        {
            SFDCSchemeBuild schema = new SFDCSchemeBuild();
            string ufEmpresa = codigoEmpresa;

            //String data = DateTime.Now.ToString("yyyyMMdd");

            QueryResult qr = null;
            binding.QueryOptionsValue = new SalesforceExtractor.apex.QueryOptions();
            binding.QueryOptionsValue.batchSize = 3;
            binding.QueryOptionsValue.batchSizeSpecified = true;

            //StringBuilder sbNomeArquivo = new StringBuilder();

            try
            {
                string cliente = "";
                String sql = string.Format(@"SELECT Contact_ID__c,Id,IdentityNumber__c,IdentityType__c 
                    FROM Contact  WHERE CompanyID__c='{0}'
                    AND IdentityNumber__c = '{1}' ", codigoEmpresa, numeroDocumento);

                bool bContinue = false;

                try
                {
                    qr = binding.query(sql);

                    bContinue = true;
                }
                catch { }

                cliente = "";

                while (bContinue)
                {
                    if (qr.records == null)
                    {
                        bContinue = false;
                        continue;
                    }

                    for (int i = 0; i < qr.records.Length; i++)
                    {
                        SalesforceExtractor.apex.sObject con = qr.records[i];
                        ClienteSalesforce cli = new ClienteSalesforce();

                        String id_conta = "";
                        String id_externo_conta = "";

                        String id_contato = "";
                        String id_externo_contato = "";
                        String nome = "";

                        String id_suministro = "";
                        String id_externo_suministro = "";
                        String numero_cliente = "";

                        String empresa = "";

                        String id_sales_asset = "";
                        String id_externo_asset = "";

                        String documento = "";
                        String tipo_documento = "";


                        id_suministro = schema.getFieldValueMore("asset__r", "PointofDelivery__r", "", "id", con.Any);
                        id_externo_suministro = schema.getFieldValueMore("asset__r", "PointofDelivery__r", "", "externalid__c", con.Any);
                        numero_cliente = schema.getFieldValueMore("asset__r", "PointofDelivery__r", "", "PointofDeliveryNumber__c", con.Any);
                        empresa = schema.getFieldValueMore("asset__r", "PointofDelivery__r", "", "company__c", con.Any);
                        id_sales_asset = schema.getFieldValueMore("asset__r", "", "", "id", con.Any); ;
                        id_externo_asset = schema.getFieldValueMore("asset__r", "", "", "externalid__c", con.Any);

                        //Contact_ID__c, Id, IdentityNumber__c, IdentityType__c
                        //id_externo_conta = getFieldValueMore("contact__r", "", "", "externalid__c", con.Any);
                        //id_externo_contato = getFieldValueMore("contact__r", "", "", "externalid__c", con.Any);
                        //nome = getFieldValueMore("contact__r", "", "", "name", con.Any);
                        //id_conta = getFieldValueMore("contact__r", "", "", "Accountid", con.Any);
                        id_contato = schema.getFieldValue("Id", con.Any);
                        documento = schema.getFieldValue("IdentityNumber__c", con.Any);
                        tipo_documento = schema.getFieldValue("Sf:IdentityType__c", con.Any);

                        cli.Documento = documento;
                        cli.TipoDocumento = documento;
                        cli.IdContato = id_contato;

                        return cli;

                        if (qr.done)
                        {
                            bContinue = false;
                            //Console.WriteLine("Registrou: " + loopCounter);
                        }
                        else
                            qr = binding.queryMore(qr.queryLocator);
                    }

                    return new ClienteSalesforce();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(string.Format("\nProblema para executar a query: {0}\n", ex.Message));
                throw new Exception(ex.Message);
            }

            return null;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="empresa"></param>
        /// <param name="ExternalId"></param>
        /// <returns></returns>
        public static List<ClienteSalesforce> GetContatosPorExternalId(string codigoEmpresa, string ExternalId, ref SforceService binding)
        {
            SFDCSchemeBuild schema = new SFDCSchemeBuild();
            String data = DateTime.Now.ToString("yyyyMMdd");

            QueryResult qr = null;
            binding.QueryOptionsValue = new apex.QueryOptions();
            binding.QueryOptionsValue.batchSize = 3;
            binding.QueryOptionsValue.batchSizeSpecified = true;

            try
            {
                String sql = string.Format(@"SELECT CompanyID__c, externalid__c, Accountid, Contact_ID__c, Id, Name, IdentityNumber__c, IdentityType__c 
                    FROM Contact  WHERE CompanyID__c='{0}'
                    AND ExternalId__c = '{1}' ", codigoEmpresa, ExternalId);

                bool bContinue = false;
                try
                {
                    qr = binding.query(sql);
                    bContinue = true;
                }
                catch
                {

                }

                //cliente = string.Empty;
                List<ClienteSalesforce> lstResult = new List<ClienteSalesforce>();
                ClienteSalesforce cli = null;

                while (bContinue)
                {
                    if (qr.records == null || qr.records.Length == 0)
                    {
                        bContinue = false;
                        continue;
                    }

                    for (int i = 0; i < qr.records.Length; i++)
                    {
                        apex.sObject con = qr.records[i];

                        String empresa = "";
                        String idExterno = "";
                        String id_conta = "";
                        String id_contato = "";
                        String nome = "";
                        String documento = "";
                        String tipo_documento = "";

                        empresa = schema.getFieldValue("CompanyID__c", con.Any);
                        idExterno = schema.getFieldValue("externalid__c", con.Any);
                        id_conta = schema.getFieldValue("Accountid", con.Any);
                        id_contato = schema.getFieldValue("Id", con.Any);
                        nome = schema.getFieldValue("Name", con.Any);
                        documento = schema.getFieldValue("IdentityNumber__c", con.Any);
                        tipo_documento = schema.getFieldValue("IdentityType__c", con.Any);

                        cli = new ClienteSalesforce();
                        cli.Empresa = empresa;
                        cli.ExternalId = idExterno;
                        cli.IdConta = id_conta;
                        cli.IdContato = id_contato;
                        cli.Nome = nome;
                        cli.Documento = documento;
                        cli.TipoDocumento = tipo_documento;

                        lstResult.Add(cli);

                        if (qr.records.Length == 1)
                        {
                            bContinue = false;
                            continue;
                        }
                    }

                    //handle the loop + 1 problem by checking to see if the most recent queryResult
                    if (qr.done)
                    {
                        bContinue = false;
                        //Console.WriteLine("Registrou: " + loopCounter);
                    }
                    else
                        qr = binding.queryMore(qr.queryLocator);
                }

                return lstResult;
            }
            catch (Exception ex)
            {
                Console.WriteLine("\nProblema para executar a query: \n" + ex.Message);
                Console.Write("\nClique para continuar...");
                throw ex;
            }
        }


        public static ContractSalesforce GetContratoPorId(string id, ref SforceService binding)
        {
            SFDCSchemeBuild schema = new SFDCSchemeBuild();
            String data = DateTime.Now.ToString("yyyyMMdd");

            QueryResult qr = null;
            binding.QueryOptionsValue = new apex.QueryOptions();
            binding.QueryOptionsValue.batchSize = 3;
            binding.QueryOptionsValue.batchSizeSpecified = true;

            try
            {
                String sql = string.Format(@"select   Id
                                                    , ContractNumber
                                                    , ExternalID__c
                                                    , Contract_Type__c
                                               from Contract 
                                              where Id = '{0}' ", id);
                bool bContinue = false;
                try
                {
                    qr = binding.query(sql);
                    bContinue = true;
                }
                catch
                { }

                List<ContractSalesforce> lstResult = new List<ContractSalesforce>();
                ContractSalesforce obj = null;

                while (bContinue)
                {
                    if (qr.records == null || qr.records.Length == 0)
                    {
                        bContinue = false;
                        continue;
                    }

                    for (int i = 0; i < qr.records.Length; i++)
                    {
                        apex.sObject con = qr.records[i];
                        obj = new ContractSalesforce()
                        {
                            Id = schema.getFieldValue("Id", con.Any),
                            ContractNumber = schema.getFieldValue("ContractNumber", con.Any),
                            ExternalId = schema.getFieldValue("ExternalID__c", con.Any),
                            ContractType = schema.getFieldValue("Contract_Type__c", con.Any)
                        };

                        lstResult.Add(obj);
                        if (qr.records.Length == 1)
                        {
                            bContinue = false;
                            continue;
                        }
                    }

                    //handle the loop + 1 problem by checking to see if the most recent queryResult
                    if (qr.done)
                        bContinue = false;
                    else
                        qr = binding.queryMore(qr.queryLocator);
                }

                return lstResult.FirstOrDefault();
            }
            catch (Exception ex)
            {
                Console.WriteLine("\nProblema para executar a query: \n" + ex.Message);
                Console.Write("\nClique para continuar...");
                Console.ReadLine();
            }

            return null;
        }


        public static List<ContractSalesforce> GetContratosPorExternalId(string externalId, ref SforceService binding)
        {
            SFDCSchemeBuild schema = new SFDCSchemeBuild();
            List<ContractSalesforce> lstResult = new List<ContractSalesforce>();

            QueryResult qr = null;
            binding.QueryOptionsValue = new apex.QueryOptions();
            binding.QueryOptionsValue.batchSize = 3;
            binding.QueryOptionsValue.batchSizeSpecified = true;

            try
            {
                String sql = string.Format(@"select   Id
                                                    , ContractNumber
                                                    , ExternalID__c
                                                    , Contract_Type__c
                                               from Contract 
                                              where ExternalId__c = '{0}' ", externalId);
                bool bContinue = false;
                try
                {
                    qr = binding.query(sql);
                    bContinue = true;
                }
                catch
                { }

                ContractSalesforce obj = null;

                while (bContinue)
                {
                    if (qr.records == null || qr.records.Length == 0)
                    {
                        bContinue = false;
                        continue;
                    }

                    for (int i = 0; i < qr.records.Length; i++)
                    {
                        apex.sObject con = qr.records[i];
                        obj = new ContractSalesforce()
                        {
                            Id = schema.getFieldValue("Id", con.Any),
                            ContractNumber = schema.getFieldValue("ContractNumber", con.Any),
                            ExternalId = schema.getFieldValue("ExternalID__c", con.Any),
                            ContractType = schema.getFieldValue("Contract_Type__c", con.Any)
                        };

                        lstResult.Add(obj);
                        if (qr.records.Length == 1)
                        {
                            bContinue = false;
                            continue;
                        }
                    }

                    //handle the loop + 1 problem by checking to see if the most recent queryResult
                    if (qr.done)
                        bContinue = false;
                    else
                        qr = binding.queryMore(qr.queryLocator);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("\nProblema para executar a query: \n" + ex.Message);
                Console.Write("\nClique para continuar...");
                Console.ReadLine();
            }

            return lstResult;
        }


        public static List<ContractSalesforce> GetContratosPorExternalId(List<string> lstExternalId, ref SforceService binding)
        {
            SFDCSchemeBuild schema = new SFDCSchemeBuild();

            QueryResult qr = null;
            binding.QueryOptionsValue = new apex.QueryOptions();
            binding.QueryOptionsValue.batchSize = 3;
            binding.QueryOptionsValue.batchSizeSpecified = true;

            List<ContractSalesforce> lstResult = new List<ContractSalesforce>();
            List<string> lstConsulta = new List<string>();
            StringBuilder cond = new StringBuilder();
            ContractSalesforce obj = null;

            String sqlbase = @"select   Id
                                , ContractNumber
                                , ExternalID__c
                                , Contract_Type__c
                            from Contract 
                            where ExternalId__c IN ({0}) ";

            foreach (string ext in lstExternalId.Distinct())
            {
                if ((sqlbase.Length + cond.Length + ext.Length) < (20000 - 3))
                {
                    cond.Append((cond.Length > 0) ? "," : string.Empty);
                    cond.Append(string.Concat("'", ext, "'"));

                    if (lstExternalId.Distinct().ToList().IndexOf(ext) != (lstExternalId.Distinct().Count() - 1))
                        continue;
                }

                if (cond.Length == 0)
                    continue;
                try
                {
                    bool bContinue = false;
                    try
                    {
                        string sql = string.Format(@"{0}",sqlbase).Replace("{0}", cond.ToString());

                        qr = binding.query(sql);
                        bContinue = true;
                    }
                    catch
                    { }

                    while (bContinue)
                    {
                        if (qr.records == null || qr.records.Length == 0)
                        {
                            bContinue = false;
                            continue;
                        }

                        for (int i = 0; i < qr.records.Length; i++)
                        {
                            apex.sObject con = qr.records[i];
                            obj = new ContractSalesforce()
                            {
                                Id = schema.getFieldValue("Id", con.Any),
                                ContractNumber = schema.getFieldValue("ContractNumber", con.Any),
                                ExternalId = schema.getFieldValue("ExternalID__c", con.Any),
                                ContractType = schema.getFieldValue("Contract_Type__c", con.Any)
                            };

                            lstResult.Add(obj);
                        }

                        //handle the loop + 1 problem by checking to see if the most recent queryResult
                        if (qr.done)
                            bContinue = false;
                        else
                            qr = binding.queryMore(qr.queryLocator);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("\nProblema para executar a query: \n" + ex.Message);
                    Console.Write("\nClique para continuar...");
                    Console.ReadLine();
                }

                cond.Clear();
                cond.Append(string.Concat("'", ext, "'"));
            }

            return lstResult;
        }




        public static List<ContractSalesforce> GetContratoAgrupamentoPorExternalId(string externalId, string codigoContrato, ref SforceService binding)
        {
            SFDCSchemeBuild schema = new SFDCSchemeBuild();
            String data = DateTime.Now.ToString("yyyyMMdd");

            QueryResult qr = null;
            binding.QueryOptionsValue = new apex.QueryOptions();
            binding.QueryOptionsValue.batchSize = 3;
            binding.QueryOptionsValue.batchSizeSpecified = true;

            try
            {
                String sql = string.Format(@"Select Id, ExternalID__c, Owner.Name, CreatedDate, ContractNumber
                                               from Contract
                                              where CNT_GroupTypeContract__c = 'CNT002'
                                                and CNT_GroupSegment__c = '2'
                                                and CNT_GroupArea__c = '4'
                                                and ExternalId__c = '{0}'
                                                and CNT_GroupNumerCntr__c  = '{1}'
                                           Order By CreatedDate DESC", externalId, codigoContrato);
                bool bContinue = false;
                try
                {
                    qr = binding.query(sql);
                    bContinue = true;
                }
                catch
                { }

                List<ContractSalesforce> lstResult = new List<ContractSalesforce>();
                ContractSalesforce obj = null;

                while (bContinue)
                {
                    if (qr.records == null || qr.records.Length == 0)
                    {
                        bContinue = false;
                        continue;
                    }

                    for (int i = 0; i < qr.records.Length; i++)
                    {
                        apex.sObject con = qr.records[i];
                        obj = new ContractSalesforce()
                        {
                            Id = schema.getFieldValue("Id", con.Any),
                            ContractNumber = schema.getFieldValue("ContractNumber", con.Any)
                        };

                        lstResult.Add(obj);
                        if (qr.records.Length == 1)
                        {
                            bContinue = false;
                            continue;
                        }
                    }

                    //handle the loop + 1 problem by checking to see if the most recent queryResult
                    if (qr.done)
                        bContinue = false;
                    else
                        qr = binding.queryMore(qr.queryLocator);
                }

                return lstResult;
            }
            catch (Exception ex)
            {
                Console.WriteLine("\nProblema para executar a query: \n" + ex.Message);
                Console.Write("\nClique para continuar...");
                Console.ReadLine();
            }

            return null;
        }



        /// <summary>
        /// Retorna um único Billing Profile relacionado ao agrupamento de uma Account
        /// </summary>
        /// <param name="contrato"></param>
        /// <param name="binding"></param>
        /// <returns></returns>
        public static BillingSalesforce GetBillingAgrupamentoByAccountId(ContractLineItemGoverno contrato, ref SforceService binding)
        {
            SFDCSchemeBuild schema = new SFDCSchemeBuild();
            String data = DateTime.Now.ToString("yyyyMMdd");

            QueryResult qr = null;
            binding.QueryOptionsValue = new apex.QueryOptions();
            binding.QueryOptionsValue.batchSize = 3;
            binding.QueryOptionsValue.batchSizeSpecified = true;

            try
            {
                //record type fixo e lote preenchido
                String sql = string.Format(@"select   Id, Type__c, CNT_Lot__c, CNT_Contract__c
                                            from Billing_Profile__c 
                                            where Account__c = '{0}'
                                                and AccountContract__c = '{1}'
                                                and RecordTypeId = '01236000000On9B'
                                                and CNT_Lot__c != '' "
                    , contrato.ContaAgrupamento
                    , contrato.ContratoAgrupamento);
                bool bContinue = false;
                try
                {
                    qr = binding.query(sql);
                    bContinue = true;
                }
                catch
                { }

                BillingSalesforce obj = null;

                while (bContinue)
                {
                    if (qr.records == null || qr.records.Length == 0)
                    {
                        bContinue = false;
                        continue;
                    }

                    for (int i = 0; i < qr.records.Length; i++)
                    {
                        apex.sObject con = qr.records[i];
                        obj = new BillingSalesforce()
                        {
                            Id = schema.getFieldValue("Id", con.Any),
                            CNT_Contract__c = schema.getFieldValue("CNT_Contract__c", con.Any)
                        };

                        return obj;
                        //if (qr.records.Length == 1)
                        //{
                        //    bContinue = false;
                        //    continue;
                        //}
                    }

                    //handle the loop + 1 problem by checking to see if the most recent queryResult
                    if (qr.done)
                        bContinue = false;
                    else
                        qr = binding.queryMore(qr.queryLocator);
                }

                return new BillingSalesforce();
            }
            catch (Exception ex)
            {
                Console.WriteLine("\nProblema para executar a query: \n" + ex.Message);
                Console.Write("\nClique para continuar...");
                Console.ReadLine();
            }

            return null;
        }


        public static List<BillingSalesforce> GetBillingProfileById(string id, ref SforceService binding)
        {
            SFDCSchemeBuild schema = new SFDCSchemeBuild();
            String data = DateTime.Now.ToString("yyyyMMdd");

            QueryResult qr = null;
            binding.QueryOptionsValue = new apex.QueryOptions();
            binding.QueryOptionsValue.batchSize = 3;
            binding.QueryOptionsValue.batchSizeSpecified = true;

            try
            {
                String sql = string.Format(@"select   Id
                                                , Type__c
                                                , CNT_Lot__c
                                                , CNT_Contract__c
                                                , CNT_GroupPayType__c
                                                , CNT_Due_Date__c
                                                , Address__c
                                                , BillingAddress__c
                                                , AccountContract__c
                                                , RecordTypeId
                                                , Company__c
                                                , ExternalID__c
                                            from Billing_Profile__c 
                                            where Id = '{0}'", id);
                bool bContinue = false;
                try
                {
                    qr = binding.query(sql);
                    bContinue = true;
                }
                catch
                { }

                BillingSalesforce obj = null;
                List<BillingSalesforce> lstResult = new List<BillingSalesforce>();

                while (bContinue)
                {
                    if (qr.records == null || qr.records.Length == 0)
                    {
                        bContinue = false;
                        continue;
                    }

                    for (int i = 0; i < qr.records.Length; i++)
                    {
                        apex.sObject con = qr.records[i];
                        obj = new BillingSalesforce()
                        {
                            Id = schema.getFieldValue("Id", con.Any),
                            Type__c = schema.getFieldValue("Type__c", con.Any),
                            CNT_Lot__c = schema.getFieldValue("CNT_Lot__c", con.Any),
                            CNT_GroupPayType__c = schema.getFieldValue("CNT_GroupPayType__c", con.Any),
                            CNT_Due_Date__c = schema.getFieldValue("CNT_Due_Date__c", con.Any),
                            Address__c = schema.getFieldValue("Address__c", con.Any),
                            BillingAddress__c = schema.getFieldValue("BillingAddress__c", con.Any),
                            AccountContract__c = schema.getFieldValue("AccountContract__c", con.Any),
                            RecordTypeId = schema.getFieldValue("RecordTypeId", con.Any),
                            Company__c = schema.getFieldValue("Company__c", con.Any),
                            ExternalID__c = schema.getFieldValue("ExternalID__c", con.Any),
                            CNT_Contract__c = schema.getFieldValue("CNT_Contract__c", con.Any)
                        };

                        lstResult.Add(obj);
                        //if (qr.records.Length == 1)
                        //{
                        //    bContinue = false;
                        //    continue;
                        //}
                    }

                    //handle the loop + 1 problem by checking to see if the most recent queryResult
                    if (qr.done)
                        bContinue = false;
                    else
                        qr = binding.queryMore(qr.queryLocator);
                }

                return lstResult;
            }
            catch (Exception ex)
            {
                Console.WriteLine("\nProblema para executar a query: \n" + ex.Message);
                Console.Write("\nClique para continuar...");
                Console.ReadLine();
            }

            return null;
        }


        public static List<BillingSalesforce> GetBillingProfileByContractId(string contractId, ref SforceService binding)
        {
            SFDCSchemeBuild schema = new SFDCSchemeBuild();
            String data = DateTime.Now.ToString("yyyyMMdd");

            QueryResult qr = null;
            binding.QueryOptionsValue = new apex.QueryOptions();
            binding.QueryOptionsValue.batchSize = 3;
            binding.QueryOptionsValue.batchSizeSpecified = true;

            try
            {
                String sql = string.Format(@"select   Id
                                                , Type__c
                                                , CNT_Lot__c
                                                , CNT_Contract__c
                                                , CNT_GroupPayType__c
                                                , CNT_Due_Date__c
                                                , Address__c
                                                , BillingAddress__c
                                                , AccountContract__c
                                                , RecordTypeId
                                                , Company__c
                                                , ExternalID__c
                                            from Billing_Profile__c 
                                            where AccountContract__c = '{0}'", contractId);
                bool bContinue = false;
                try
                {
                    qr = binding.query(sql);
                    bContinue = true;
                }
                catch
                { }

                BillingSalesforce obj = null;
                List<BillingSalesforce> lstResult = new List<BillingSalesforce>();

                while (bContinue)
                {
                    if (qr.records == null || qr.records.Length == 0)
                    {
                        bContinue = false;
                        continue;
                    }

                    for (int i = 0; i < qr.records.Length; i++)
                    {
                        apex.sObject con = qr.records[i];
                        obj = new BillingSalesforce()
                        {
                            Id = schema.getFieldValue("Id", con.Any),
                            Type__c = schema.getFieldValue("Type__c", con.Any),
                            CNT_Lot__c = schema.getFieldValue("CNT_Lot__c", con.Any),
                            CNT_GroupPayType__c = schema.getFieldValue("CNT_GroupPayType__c", con.Any),
                            CNT_Due_Date__c = schema.getFieldValue("CNT_Due_Date__c", con.Any),
                            Address__c = schema.getFieldValue("Address__c", con.Any),
                            BillingAddress__c = schema.getFieldValue("BillingAddress__c", con.Any),
                            AccountContract__c = schema.getFieldValue("AccountContract__c", con.Any),
                            RecordTypeId = schema.getFieldValue("RecordTypeId", con.Any),
                            Company__c = schema.getFieldValue("Company__c", con.Any),
                            ExternalID__c = schema.getFieldValue("ExternalID__c", con.Any),
                            CNT_Contract__c = schema.getFieldValue("CNT_Contract__c", con.Any)
                        };

                        lstResult.Add(obj);
                        //if (qr.records.Length == 1)
                        //{
                        //    bContinue = false;
                        //    continue;
                        //}
                    }

                    //handle the loop + 1 problem by checking to see if the most recent queryResult
                    if (qr.done)
                        bContinue = false;
                    else
                        qr = binding.queryMore(qr.queryLocator);
                }

                return lstResult;
            }
            catch (Exception ex)
            {
                Console.WriteLine("\nProblema para executar a query: \n" + ex.Message);
                Console.Write("\nClique para continuar...");
                Console.ReadLine();
            }

            return null;
        }


        public static ContractSalesforce GetContratoAgrupamentoPorContrato(ContractLineItemGoverno contrato, ref SforceService binding)
        {
            SFDCSchemeBuild schema = new SFDCSchemeBuild();
            String data = DateTime.Now.ToString("yyyyMMdd");

            QueryResult qr = null;
            binding.QueryOptionsValue = new apex.QueryOptions();
            binding.QueryOptionsValue.batchSize = 3;
            binding.QueryOptionsValue.batchSizeSpecified = true;

            try
            {
                BillingSalesforce contractBilling = GetBillingAgrupamentoByAccountId(contrato, ref binding);
                String sql = string.Format(@"Select Id
                                                    , ExternalID__c
                                                    , Owner.Name
                                                    , CreatedDate
                                                    , ContractNumber
                                                    , CNT_GroupSegment__c
                                                    , CNT_GroupArea__c
                                                    , AccountId
                                                    , CNT_GroupNumerCntr__c
                                                    , Name
                                               from Contract
                                              where CNT_GroupNumerCntr__c = '{0}'
                                                and CNT_GroupTypeContract__c = 'CNT002' ", contrato.ContratoAgrupamento);

                bool bContinue = false;
                try
                {
                    qr = binding.query(sql);
                    bContinue = true;
                }
                catch
                { }

                ContractSalesforce obj = null;

                while (bContinue)
                {
                    if (qr.records == null || qr.records.Length == 0)
                    {
                        bContinue = false;
                        continue;
                    }

                    for (int i = 0; i < qr.records.Length; i++)
                    {
                        apex.sObject con = qr.records[i];
                        obj = new ContractSalesforce()
                        {
                            Id = schema.getFieldValue("Id", con.Any),
                            ContractNumber = schema.getFieldValue("ContractNumber", con.Any),
                            Account = schema.getFieldValue("AccountId", con.Any),
                            SegmentoAgrupamento = schema.getFieldValue("CNT_GroupSegment__c", con.Any),
                            AreaAgrupamento = schema.getFieldValue("CNT_GroupArea__c", con.Any),
                            ExternalId = schema.getFieldValue("ExternalID__c", con.Any),
                            NumeroAgrupamento = schema.getFieldValue("CNT_GroupNumerCntr__c", con.Any),
                            Name = schema.getFieldValue("Name", con.Any)
                        };

                        return obj;
                        //if (qr.records.Length == 1)
                        //{
                        //    bContinue = false;
                        //    continue;
                        //}
                    }

                    //handle the loop + 1 problem by checking to see if the most recent queryResult
                    if (qr.done)
                        bContinue = false;
                    else
                        qr = binding.queryMore(qr.queryLocator);
                }

                return new ContractSalesforce();
            }
            catch (Exception ex)
            {
                Console.WriteLine("\nProblema para executar a query: \n" + ex.Message);
                Console.Write("\nClique para continuar...");
                Console.ReadLine();
            }

            return null;
        }

               
        public static List<ContractLineItemSalesforce> GetContratosPorCliente(string codigoEmpresa, string tipoContratoIN, string numeroClienteIN, ref SforceService binding)
        {
            SFDCSchemeBuild schema = new SFDCSchemeBuild();
            String data = DateTime.Now.ToString("yyyyMMdd");

            QueryResult qr = null;
            binding.QueryOptionsValue = new apex.QueryOptions();
            binding.QueryOptionsValue.batchSize = 3;
            binding.QueryOptionsValue.batchSizeSpecified = true;

            try
            {
                String sql = string.Format(@"select   Contract__r.Id
                                                    , Asset__r.PointofDelivery__r.Name
                                                    , Contract__r.ContractNumber
                                                    , Contract__r.ExternalID__c
                                                    , Asset__r.PointOfDelivery__c
                                                    , Contract__r.Contract_Type__c
                                               from Contract_Line_Item__c 
                                              where Asset__r.Country__c = 'BRASIL'
                                                and Asset__r.Account.CompanyID__c = '{0}' 
                                                and Contract__r.Contract_Type__c in ({1}) 
                                                and Asset__r.Pointofdelivery__r.Pointofdeliverynumber__c IN ({2}) ", codigoEmpresa, tipoContratoIN, numeroClienteIN);
                #region obsoleto
                                            //                String sql = string.Format(@"SELECT id, name, ContractNumber, ExternalID__c, Contract_Type__c
                                            //                                               FROM contract
                                            //                                              WHERE Account.Country__c = 'BRASIL'
                                            //                                                AND Account.CompanyID__c = '{0}'
                                            //                                                AND Asset__r.Contract__r.Contract_Type__c IN ({1})
                                            //                                                AND name in ({2})", codigoEmpresa, tipoContratoIN, numeroClienteIN);
                                            #endregion
                
                bool bContinue = false;
                try
                {
                    qr = binding.query(sql);
                    bContinue = true;
                }
                catch
                { }

                List<ContractLineItemSalesforce> lstResult = new List<ContractLineItemSalesforce>();
                ContractLineItemSalesforce obj = null;

                while (bContinue)
                {
                    if (qr.records == null || qr.records.Length == 0)
                    {
                        bContinue = false;
                        continue;
                    }

                    for (int i = 0; i < qr.records.Length; i++)
                    {
                        apex.sObject con = qr.records[i];
                        obj = new ContractLineItemSalesforce()
                        {
                            ContractId = schema.getFieldValueMore("Contract__r", "", "", "Id", con.Any),
                            NumeroCliente = schema.getFieldValueMore("Asset__r", "PointofDelivery__r", "", "Name", con.Any),
                            PointOfDelivery = schema.getFieldValueMore("Asset__r", "", "", "PointOfDelivery__c", con.Any),
                            ContractNumber = schema.getFieldValueMore("Contract__r", "", "", "ContractNumber", con.Any),
                            ContractExternalId = schema.getFieldValueMore("Contract__r", "", "", "ExternalID__c", con.Any),
                            ContractType = schema.getFieldValueMore("Contract__r", "", "", "Contract_Type__c", con.Any),
                        };

                        lstResult.Add(obj);
                        if (qr.records.Length == 1)
                        {
                            bContinue = false;
                            continue;
                        }
                    }

                    //handle the loop + 1 problem by checking to see if the most recent queryResult
                    if (qr.done)
                        bContinue = false;
                    else
                        qr = binding.queryMore(qr.queryLocator);
                }

                return lstResult;
            }
            catch (Exception ex)
            {
                Console.WriteLine("\nProblema para executar a query: \n" + ex.Message);
                Console.Write("\nClique para continuar...");
                Console.ReadLine();
            }

            return null;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="empresa"></param>
        /// <param name="ExternalId"></param>
        /// <returns></returns>
        public static List<ClienteSalesforce> GetPodsPorExternalId(List<string> lstExternalId, ref SforceService binding)
        {
            SFDCSchemeBuild schema = new SFDCSchemeBuild();
            String data = DateTime.Now.ToString("yyyyMMdd");

            QueryResult qr = null;
            binding.QueryOptionsValue = new apex.QueryOptions();
            binding.QueryOptionsValue.batchSize = 3;
            binding.QueryOptionsValue.batchSizeSpecified = true;

            String sqlbase = @"SELECT ExternalId__c, Id, CompanyID__c, DetailAddress__c FROM PointofDelivery__c 
                                              WHERE ExternalId__c IN ({0}) ";

            List<ClienteSalesforce> lstResult = new List<ClienteSalesforce>();
            List<string> lstConsulta = new List<string>();
            StringBuilder cond = new StringBuilder();
            ClienteSalesforce cli = null;

            foreach (string ext in lstExternalId.Distinct())
            {
                if ((sqlbase.Length + cond.Length + ext.Length) < (20000 - 3))
                {
                    cond.Append((cond.Length > 0) ? "," : string.Empty);
                    cond.Append(string.Concat("'", ext, "'"));

                    if (lstExternalId.Distinct().ToList().IndexOf(ext) != (lstExternalId.Distinct().Count() - 1))
                        continue;
                }

                if (cond.Length == 0)
                    continue;
                try
                {
                    String sql = string.Format(@"{0} ", sqlbase).Replace("{0}", cond.ToString());

                    bool bContinue = false;
                    try
                    {
                        qr = binding.query(sql);

                        bContinue = true;
                    }
                    catch
                    {

                    }

                    while (bContinue)
                    {
                        if (qr.records == null || qr.records.Length == 0)
                        {
                            bContinue = false;
                            continue;
                        }

                        for (int i = 0; i < qr.records.Length; i++)
                        {
                            apex.sObject con = qr.records[i];

                            String empresa = "";
                            String idExterno = "";
                            String id = "";
                            String detailAddress = "";
                            //String nome = "";
                            //String documento = "";
                            //String tipo_documento = "";

                            empresa = schema.getFieldValue("CompanyID__c", con.Any);
                            idExterno = schema.getFieldValue("ExternalId__c", con.Any);
                            id = schema.getFieldValue("Id", con.Any);
                            detailAddress = schema.getFieldValue("DetailAddress__c", con.Any);
                            //nome = schema.getFieldValue("Name", con.Any);
                            //documento = schema.getFieldValue("IdentityNumber__c", con.Any);
                            //tipo_documento = schema.getFieldValue("IdentityType__c", con.Any);

                            cli = new ClienteSalesforce();
                            cli.Empresa = empresa;
                            cli.ExternalId = idExterno;
                            cli.IdPod = id;
                            cli.DetailAddress__c = detailAddress;
                            //cli.Nome = nome;
                            //cli.Documento = documento;
                            //cli.TipoDocumento = tipo_documento;

                            lstResult.Add(cli);
                        }

                        //handle the loop + 1 problem by checking to see if the most recent queryResult
                        if (qr.done)
                        {
                            bContinue = false;
                            //Console.WriteLine("Registrou: " + loopCounter);
                        }
                        else
                            qr = binding.queryMore(qr.queryLocator);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("\nProblema para executar a query: \n" + ex.Message);
                    Console.Write("\nClique para continuar...");
                    Console.ReadLine();
                }

                cond.Clear();
                cond.Append(string.Concat("'", ext, "'"));
            }

            return lstResult;
        }


        public static List<ClienteSalesforce> GetPodsPorExternalId(string ExternalId, ref SforceService binding)
        {
            SFDCSchemeBuild schema = new SFDCSchemeBuild();
            String data = DateTime.Now.ToString("yyyyMMdd");

            QueryResult qr = null;
            binding.QueryOptionsValue = new apex.QueryOptions();
            binding.QueryOptionsValue.batchSize = 3;
            binding.QueryOptionsValue.batchSizeSpecified = true;

            List<ClienteSalesforce> lstResult = new List<ClienteSalesforce>();
            try
            {
                String sql = string.Format(@"SELECT ExternalId__c, Id, CompanyID__c, DetailAddress__c FROM PointofDelivery__c 
                                              WHERE ExternalId__c = '{0}' ", ExternalId);

                bool bContinue = false;
                try
                {
                    qr = binding.query(sql);

                    bContinue = true;
                }
                catch
                {

                }

                //cliente = string.Empty;
                ClienteSalesforce cli = null;

                while (bContinue)
                {
                    if (qr.records == null || qr.records.Length == 0)
                    {
                        bContinue = false;
                        continue;
                    }

                    for (int i = 0; i < qr.records.Length; i++)
                    {
                        apex.sObject con = qr.records[i];

                        String empresa = "";
                        String idExterno = "";
                        String id = "";
                        String detailAddress = "";
                        //String nome = "";
                        //String documento = "";
                        //String tipo_documento = "";

                        empresa = schema.getFieldValue("CompanyID__c", con.Any);
                        idExterno = schema.getFieldValue("ExternalId__c", con.Any);
                        id = schema.getFieldValue("Id", con.Any);
                        detailAddress = schema.getFieldValue("DetailAddress__c", con.Any);
                        //nome = schema.getFieldValue("Name", con.Any);
                        //documento = schema.getFieldValue("IdentityNumber__c", con.Any);
                        //tipo_documento = schema.getFieldValue("IdentityType__c", con.Any);

                        cli = new ClienteSalesforce();
                        cli.Empresa = empresa;
                        cli.ExternalId = idExterno;
                        cli.IdPod = id;
                        cli.DetailAddress__c = detailAddress;
                        //cli.Nome = nome;
                        //cli.Documento = documento;
                        //cli.TipoDocumento = tipo_documento;

                        lstResult.Add(cli);

                        if (qr.records.Length == 1)
                        {
                            bContinue = false;
                            continue;
                        }
                    }

                    //handle the loop + 1 problem by checking to see if the most recent queryResult
                    if (qr.done)
                    {
                        bContinue = false;
                        //Console.WriteLine("Registrou: " + loopCounter);
                    }
                    else
                        qr = binding.queryMore(qr.queryLocator);
                }

                return lstResult;
            }
            catch (Exception ex)
            {
                Console.WriteLine("\nProblema para executar a query: \n" + ex.Message);
                Console.Write("\nClique para continuar...");
                Console.ReadLine();
            }

            return lstResult;
        }





        public static List<AssetDTO> GetAssetsPorNumeroCliente(string codigoEmpresaIn, string statusAssetIn, List<string> lstNumeroClientes, ref SforceService binding)
        {
            String sql2 = string.Format(@"select  Asset__r.Id
                                                , Asset__r.ExternalID__c
                                                , Asset__r.PointofDelivery__r.ExternalId__c
                                                , Asset__r.PointofDelivery__r.Name
                                                , Asset__r.NE__Order_Config__c
                                                , Asset__r.PointofDelivery__r.CNT_LowIncomeType__c
                                                , Contract__r.Id
                                                , Contract__r.ContractNumber
                                                , Contract__r.CNT_LowIncomeType__c
                                                , Contract__r.ExternalId__c
                                           from Contract_Line_Item__c");

            List<CondicaoSql> condicoes = new List<CondicaoSql>();

            CondicaoSimplesSql condicao1 = new CondicaoSimplesSql();
            condicao1.Campo = "Asset__r.PointofDelivery__r.Name";
            condicao1.Fixo = false;
            condicao1.Valor = lstNumeroClientes.Select(x => string.Concat("'", x.Replace("'", ""), "'")).ToList();
            condicoes.Add(condicao1);

            CondicaoSimplesSql condicao = new CondicaoSimplesSql();
            condicao.Campo = "Asset__r.Name";
            condicao.Fixo = true;
            condicao.Valor = "'Grupo A','Grupo B','Eletricity Service'".Split(new char[] { ',' }).ToList();
            condicoes.Add(condicao);

            condicao = new CondicaoSimplesSql();
            condicao.Campo = "Asset__r.NE__Status__c";
            condicao.Fixo = true;
            condicao.Valor = statusAssetIn.Split(new char[] { ',' }).ToList();
            condicoes.Add(condicao);

            condicao = new CondicaoSimplesSql();
            condicao.Campo = "Asset__r.PointofDelivery__r.Country__c";
            condicao.Fixo = true;
            condicao.Valor = "'BRASIL'".Split(new char[] { ',' }).ToList();
            condicoes.Add(condicao);

            condicao = new CondicaoSimplesSql();
            condicao.Campo = "Asset__r.PointofDelivery__r.CompanyID__c";
            condicao.Fixo = true;
            condicao.Valor = codigoEmpresaIn.Split(new char[] { ',' }).ToList();
            condicoes.Add(condicao);

            List<sObject> temp = ConsultarS(sql2, condicoes, ref binding);

            List<AssetDTO> resultado = EntidadeConversor.ToAssetList(temp);
            return resultado;
        }







        public static List<ClienteSalesforce> GetPodsPorNumeroCliente(string codigoEmpresaIn, string numeroClientesIn, ref SforceService binding)
        {
            SFDCSchemeBuild schema = new SFDCSchemeBuild();
            String data = DateTime.Now.ToString("yyyyMMdd");

            QueryResult qr = null;
            binding.QueryOptionsValue = new apex.QueryOptions();
            binding.QueryOptionsValue.batchSize = 3;
            binding.QueryOptionsValue.batchSizeSpecified = true;

            List<ClienteSalesforce> lstResult = new List<ClienteSalesforce>();
            try
            {
                String sql = string.Format(@"SELECT ExternalId__c, Id, CompanyID__c, DetailAddress__c 
                                               FROM PointofDelivery__c 
                                              WHERE CompanyId__c in ({0})
                                                AND Name in ({1}) "
                    , codigoEmpresaIn
                    , numeroClientesIn);

                bool bContinue = false;
                try
                {
                    qr = binding.query(sql);

                    bContinue = true;
                }
                catch
                {

                }

                //cliente = string.Empty;
                ClienteSalesforce cli = null;

                while (bContinue)
                {
                    if (qr.records == null || qr.records.Length == 0)
                    {
                        bContinue = false;
                        continue;
                    }

                    for (int i = 0; i < qr.records.Length; i++)
                    {
                        apex.sObject con = qr.records[i];

                        String empresa = "";
                        String idExterno = "";
                        String id = "";
                        String detailAddress = "";
                        //String nome = "";
                        //String documento = "";
                        //String tipo_documento = "";

                        empresa = schema.getFieldValue("CompanyID__c", con.Any);
                        idExterno = schema.getFieldValue("ExternalId__c", con.Any);
                        id = schema.getFieldValue("Id", con.Any);
                        detailAddress = schema.getFieldValue("DetailAddress__c", con.Any);
                        //nome = schema.getFieldValue("Name", con.Any);
                        //documento = schema.getFieldValue("IdentityNumber__c", con.Any);
                        //tipo_documento = schema.getFieldValue("IdentityType__c", con.Any);

                        cli = new ClienteSalesforce();
                        cli.Empresa = empresa;
                        cli.ExternalId = idExterno;
                        cli.IdPod = id;
                        cli.DetailAddress__c = detailAddress;
                        //cli.Nome = nome;
                        //cli.Documento = documento;
                        //cli.TipoDocumento = tipo_documento;

                        lstResult.Add(cli);

                        if (qr.records.Length == 1)
                        {
                            bContinue = false;
                            continue;
                        }
                    }

                    //handle the loop + 1 problem by checking to see if the most recent queryResult
                    if (qr.done)
                    {
                        bContinue = false;
                        //Console.WriteLine("Registrou: " + loopCounter);
                    }
                    else
                        qr = binding.queryMore(qr.queryLocator);
                }

                return lstResult;
            }
            catch (Exception ex)
            {
                Console.WriteLine("\nProblema para executar a query: \n" + ex.Message);
                Console.Write("\nClique para continuar...");
                Console.ReadLine();
            }

            return lstResult;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="empresa"></param>
        /// <param name="ExternalId"></param>
        /// <returns></returns>
        public static List<ClienteSalesforce> GetContasPorExternalId(string codigoEmpresa, string ExternalId, ref SforceService binding)
        {
            SFDCSchemeBuild schema = new SFDCSchemeBuild();
            String data = DateTime.Now.ToString("yyyyMMdd");

            QueryResult qr = null;
            binding.QueryOptionsValue = new apex.QueryOptions();
            binding.QueryOptionsValue.batchSize = 3;
            binding.QueryOptionsValue.batchSizeSpecified = true;

            try
            {
                String sql = string.Format(@"SELECT Account__c,CompanyID__c,ExternalId__c,Id FROM Account WHERE CompanyID__c='{0}'
                    AND ExternalId__c in ({1}) ", codigoEmpresa, ExternalId);

                bool bContinue = false;
                int loopCounter = 0;

                try
                {
                    qr = binding.query(sql);

                    bContinue = true;
                    loopCounter = 0;
                }
                catch
                {

                }

                //cliente = string.Empty;
                List<ClienteSalesforce> lstResult = new List<ClienteSalesforce>();
                ClienteSalesforce cli = null;

                while (bContinue)
                {
                    if (qr.records == null || qr.records.Length == 0)
                    {
                        bContinue = false;
                        continue;
                    }

                    for (int i = 0; i < qr.records.Length; i++)
                    {
                        apex.sObject con = qr.records[i];

                        String empresa = "";
                        String idExterno = "";
                        String id = "";
                        //String nome = "";
                        //String documento = "";
                        //String tipo_documento = "";

                        empresa = schema.getFieldValue("CompanyID__c", con.Any);
                        idExterno = schema.getFieldValue("ExternalId__c", con.Any);
                        id = schema.getFieldValue("Id", con.Any);
                        //nome = schema.getFieldValue("Name", con.Any);
                        //documento = schema.getFieldValue("IdentityNumber__c", con.Any);
                        //tipo_documento = schema.getFieldValue("IdentityType__c", con.Any);

                        cli = new ClienteSalesforce();
                        cli.Empresa = empresa;
                        cli.ExternalId = idExterno;
                        cli.Id = id;
                        //cli.Nome = nome;
                        //cli.Documento = documento;
                        //cli.TipoDocumento = tipo_documento;

                        lstResult.Add(cli);

                        if (qr.records.Length == 1)
                        {
                            bContinue = false;
                            continue;
                        }
                    }

                    //handle the loop + 1 problem by checking to see if the most recent queryResult
                    if (qr.done)
                    {
                        bContinue = false;
                        //Console.WriteLine("Registrou: " + loopCounter);
                    }
                    else
                        qr = binding.queryMore(qr.queryLocator);
                }

                return lstResult;
            }
            catch (Exception ex)
            {
                Console.WriteLine("\nProblema para executar a query: \n" + ex.Message);
                Console.Write("\nClique para continuar...");
                Console.ReadLine();
            }

            return null;
        }


        public static List<AccountSalesforce> GetContasPorExternalId(string ExternalId, ref SforceService binding)
        {
            SFDCSchemeBuild schema = new SFDCSchemeBuild();
            String data = DateTime.Now.ToString("yyyyMMdd");

            QueryResult qr = null;
            binding.QueryOptionsValue = new apex.QueryOptions();
            binding.QueryOptionsValue.batchSize = 3;
            binding.QueryOptionsValue.batchSizeSpecified = true;

            List<AccountSalesforce> lstResult = new List<AccountSalesforce>();
            try
            {
                String sql = string.Format(@"SELECT Account__c, CompanyID__c, ExternalId__c, Id, Name, RecordTypeId, ParentId
                                               FROM Account WHERE ExternalId__c = '{0}' ", ExternalId);

                bool bContinue = false;
                try
                {
                    qr = binding.query(sql);
                    bContinue = true;
                }
                catch
                {
                }

                //cliente = string.Empty;
                AccountSalesforce cli = null;

                while (bContinue)
                {
                    if (qr.records == null || qr.records.Length == 0)
                    {
                        bContinue = false;
                        continue;
                    }

                    for (int i = 0; i < qr.records.Length; i++)
                    {
                        apex.sObject con = qr.records[i];

                        cli = new AccountSalesforce();
                        cli.CodigoEmpresa = schema.getFieldValue("CompanyID__c", con.Any);
                        cli.ExternalId = schema.getFieldValue("ExternalId__c", con.Any);
                        cli.Id = schema.getFieldValue("Id", con.Any);
                        cli.Nome = schema.getFieldValue("Name", con.Any);
                        cli.TipoRegistro = schema.getFieldValue("RecordTypeId", con.Any);
                        cli.ParentId = schema.getFieldValue("ParentId", con.Any);
                        //documento = schema.getFieldValue("IdentityNumber__c", con.Any);
                        //tipo_documento = schema.getFieldValue("IdentityType__c", con.Any);

                        lstResult.Add(cli);

                        if (qr.records.Length == 1)
                        {
                            bContinue = false;
                            continue;
                        }
                    }

                    //handle the loop + 1 problem by checking to see if the most recent queryResult
                    if (qr.done)
                    {
                        bContinue = false;
                        //Console.WriteLine("Registrou: " + loopCounter);
                    }
                    else
                        qr = binding.queryMore(qr.queryLocator);
                }

                return lstResult;
            }
            catch (Exception ex)
            {
                Console.WriteLine("\nProblema para executar a query: \n" + ex.Message);
                Console.Write("\nClique para continuar...");
                Console.ReadLine();
            }

            return lstResult;
        }

        public static List<AccountSalesforce> GetContasPorExternalId(string empresaIN, List<string> lstExternalId, ref SforceService binding)
        {
            SFDCSchemeBuild schema = new SFDCSchemeBuild();
            String data = DateTime.Now.ToString("yyyyMMdd");

            string sqlbase = string.Format(@"SELECT Account__c, CompanyID__c, ExternalId__c, Id, Name, RecordTypeId, ParentId
                                 FROM Account WHERE CompanyID__c IN ({0}) {1}", empresaIN, " AND ExternalId__c in ({1}) ");

            QueryResult qr = null;
            binding.QueryOptionsValue = new apex.QueryOptions();
            binding.QueryOptionsValue.batchSize = 3;
            binding.QueryOptionsValue.batchSizeSpecified = true;

            List<AccountSalesforce> lstResult = new List<AccountSalesforce>();
            List<string> lstConsulta = new List<string>();
            StringBuilder cond = new StringBuilder();

            foreach (string ext in lstExternalId.Distinct())
            {
                if ((sqlbase.Length + cond.Length + ext.Length) < (20000 - 3))
                {
                    cond.Append((cond.Length > 0) ? "," : string.Empty);
                    cond.Append(string.Concat("'", ext, "'"));

                    if (lstExternalId.Distinct().ToList().IndexOf(ext) != (lstExternalId.Distinct().Count() - 1))
                        continue;
                }

                try
                {
                    String sql = string.Format("{0}", sqlbase).Replace("{1}", cond.ToString());

                    bool bContinue = false;
                    try
                    {
                        qr = binding.query(sql);
                        bContinue = true;
                    }
                    catch
                    {
                    }

                    //cliente = string.Empty;
                    AccountSalesforce cli = null;

                    while (bContinue)
                    {
                        if (qr.records == null || qr.records.Length == 0)
                        {
                            bContinue = false;
                            continue;
                        }

                        for (int i = 0; i < qr.records.Length; i++)
                        {
                            apex.sObject con = qr.records[i];

                            cli = new AccountSalesforce();
                            cli.CodigoEmpresa = schema.getFieldValue("CompanyID__c", con.Any);
                            cli.ExternalId = schema.getFieldValue("ExternalId__c", con.Any);
                            cli.Id = schema.getFieldValue("Id", con.Any);
                            cli.Nome = schema.getFieldValue("Name", con.Any);
                            cli.TipoRegistro = schema.getFieldValue("RecordTypeId", con.Any);
                            cli.ParentId = schema.getFieldValue("ParentId", con.Any);
                            //documento = schema.getFieldValue("IdentityNumber__c", con.Any);
                            //tipo_documento = schema.getFieldValue("IdentityType__c", con.Any);

                            if (string.IsNullOrEmpty(cli.ExternalId))
                            {
                                continue;
                            }
                            if (lstResult.Where(x => x.ExternalId.Equals(cli.ExternalId)).Count() > 0)
                            {
                                lstResult.Remove(lstResult.Where(x => x.ExternalId.Equals(cli.ExternalId)).FirstOrDefault());
                                cli.ExternalId = string.Concat(cli.ExternalId, "[DUPLIC]");
                                lstResult.Add(cli);
                            }
                            else
                            {
                                lstResult.Add(cli);
                            }

                            //if (qr.records.Length == 1)
                            //{
                            //    bContinue = false;
                            //    continue;
                            //}
                        }

                        //handle the loop + 1 problem by checking to see if the most recent queryResult
                        if (qr.done)
                        {
                            bContinue = false;
                            //Console.WriteLine("Registrou: " + loopCounter);
                        }
                        else
                            qr = binding.queryMore(qr.queryLocator);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("\nProblema para executar a query: \n" + ex.Message);
                    Console.Write("\nClique para continuar...");
                    Console.ReadLine();
                }
                cond.Clear();
                cond.Append(string.Concat("'", ext, "'"));
            }
            return lstResult;
        }


        public static List<ClienteSalesforce> GetContasPorId(string codigoEmpresa, string Id, ref SforceService binding)
        {
            SFDCSchemeBuild schema = new SFDCSchemeBuild();
            String data = DateTime.Now.ToString("yyyyMMdd");

            QueryResult qr = null;
            binding.QueryOptionsValue = new apex.QueryOptions();
            binding.QueryOptionsValue.batchSize = 3;
            binding.QueryOptionsValue.batchSizeSpecified = true;

            try
            {
                String sql = string.Format(@"SELECT Account__c,CompanyID__c,ExternalId__c,Id, Name FROM Account WHERE CompanyID__c='{0}'
                    AND Id = '{1}' ", codigoEmpresa, Id);

                bool bContinue = false;
                int loopCounter = 0;

                try
                {
                    qr = binding.query(sql);

                    bContinue = true;
                    loopCounter = 0;
                }
                catch
                {

                }

                //cliente = string.Empty;
                List<ClienteSalesforce> lstResult = new List<ClienteSalesforce>();
                ClienteSalesforce cli = null;

                while (bContinue)
                {
                    if (qr.records == null || qr.records.Length == 0)
                    {
                        bContinue = false;
                        continue;
                    }

                    for (int i = 0; i < qr.records.Length; i++)
                    {
                        apex.sObject con = qr.records[i];

                        String empresa = "";
                        String idExterno = "";
                        String id = "";
                        //String nome = "";
                        //String documento = "";
                        //String tipo_documento = "";

                        empresa = schema.getFieldValue("CompanyID__c", con.Any);
                        idExterno = schema.getFieldValue("ExternalId__c", con.Any);
                        id = schema.getFieldValue("Id", con.Any);
                        //nome = schema.getFieldValue("Name", con.Any);
                        //documento = schema.getFieldValue("IdentityNumber__c", con.Any);
                        //tipo_documento = schema.getFieldValue("IdentityType__c", con.Any);

                        cli = new ClienteSalesforce();
                        cli.Empresa = empresa;
                        cli.ExternalId = idExterno;
                        cli.Id = id;
                        cli.Nome = schema.getFieldValue("Name", con.Any);
                        //cli.Documento = documento;
                        //cli.TipoDocumento = tipo_documento;

                        lstResult.Add(cli);

                        if (qr.records.Length == 1)
                        {
                            bContinue = false;
                            continue;
                        }
                    }

                    //handle the loop + 1 problem by checking to see if the most recent queryResult
                    if (qr.done)
                    {
                        bContinue = false;
                        //Console.WriteLine("Registrou: " + loopCounter);
                    }
                    else
                        qr = binding.queryMore(qr.queryLocator);
                }

                return lstResult;
            }
            catch (Exception ex)
            {
                Console.WriteLine("\nProblema para executar a query: \n" + ex.Message);
                Console.Write("\nClique para continuar...");
                Console.ReadLine();
            }

            return null;
        }


        public static List<ClienteSalesforce> GetPoDsSemEmpresa(string codigoEmpresa, ref SforceService binding)
        {
            SFDCSchemeBuild schema = new SFDCSchemeBuild();
            String data = DateTime.Now.ToString("yyyyMMdd");

            QueryResult qr = null;
            binding.QueryOptionsValue = new apex.QueryOptions();
            binding.QueryOptionsValue.batchSize = 3;
            binding.QueryOptionsValue.batchSizeSpecified = true;

            
            string nomeEmpresa = string.Empty;
            switch (codigoEmpresa)
            {
                case "2005":
                    nomeEmpresa = "AMPLA";
                    break;
                case "2003":
                    nomeEmpresa = "COELCE";
                    break;
                case "2018":
                    nomeEmpresa = "CELG";
                    break;
            }

            if (string.IsNullOrWhiteSpace(nomeEmpresa))
                return new List<ClienteSalesforce>();

            try
            {
                String sql = string.Format(@"select id 
                                               from PointofDelivery__c 
                                              where Country__c = 'BRASIL'
                                                and CompanyId__c ='' and company__c='{0}'", nomeEmpresa);
                bool bContinue = false;
                try
                {
                    qr = binding.query(sql);
                    bContinue = true;
                }
                catch
                {

                }

                List<ClienteSalesforce> lstResult = new List<ClienteSalesforce>();
                ClienteSalesforce cli = null;
                while (bContinue)
                {
                    if (qr.records == null || qr.records.Length == 0)
                    {
                        bContinue = false;
                        continue;
                    }

                    for (int i = 0; i < qr.records.Length; i++)
                    {
                        apex.sObject con = qr.records[i];
                        cli = new ClienteSalesforce();
                        cli.Id = schema.getFieldValue("Id", con.Any);
                        lstResult.Add(cli);                    
                    }

                    //handle the loop + 1 problem by checking to see if the most recent queryResult
                    if (qr.done)
                    {
                        bContinue = false;
                        //Console.WriteLine("Registrou: " + loopCounter);
                    }
                    else
                        qr = binding.queryMore(qr.queryLocator);
                }

                return lstResult;
            }
            catch (Exception ex)
            {
                Console.WriteLine("\nProblema para executar a query: \n" + ex.Message);
                Console.Write("\nClique para continuar...");
                Console.ReadLine();
            }

            return null;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="codigoEmpresa"></param>
        /// <param name="numeroCliente"></param>
        /// <param name="binding"></param>
        /// <returns></returns>
        public static List<ClienteSalesforce> GetContasPorNumeroCliente(string codigoEmpresa, string numeroCliente, ref SforceService binding)
        {
            SFDCSchemeBuild schema = new SFDCSchemeBuild();
            String data = DateTime.Now.ToString("yyyyMMdd");

            QueryResult qr = null;
            binding.QueryOptionsValue = new apex.QueryOptions();
            binding.QueryOptionsValue.batchSize = 3;
            binding.QueryOptionsValue.batchSizeSpecified = true;

            try
            {
                bool bContinue = false;
                String sql = string.Format(@"SELECT Account__c, CompanyID__c, ExternalId__c, Id, AccountNumber 
                                               FROM Account 
                                              WHERE CompanyID__c='{0}'
                                                AND AccountNumber IN ({1})
                                                AND Country__c = 'BRASIL'", codigoEmpresa, string.Concat("'", numeroCliente, "'"));
                try
                {
                    qr = binding.query(sql);
                    bContinue = true;
                }
                catch
                {}

                List<ClienteSalesforce> lstResult = new List<ClienteSalesforce>();
                ClienteSalesforce cli = null;

                while (bContinue)
                {
                    if (qr.records == null || qr.records.Length == 0)
                    {
                        bContinue = false;
                        continue;
                    }

                    for (int i = 0; i < qr.records.Length; i++)
                    {
                        apex.sObject con = qr.records[i];

                        String empresa = string.Empty;
                        String idExterno = string.Empty;
                        String id = string.Empty;
                        string nroCliente = string.Empty;

                        empresa = schema.getFieldValue("CompanyID__c", con.Any);
                        idExterno = schema.getFieldValue("ExternalId__c", con.Any);
                        id = schema.getFieldValue("Id", con.Any);
                        nroCliente = schema.getFieldValue("AccountNumber", con.Any);

                        cli = new ClienteSalesforce();
                        cli.Empresa = empresa;
                        cli.ExternalId = idExterno;
                        cli.Id = id;
                        cli.NumeroCliente = nroCliente;

                        lstResult.Add(cli);

                        if (qr.records.Length == 1)
                        {
                            bContinue = false;
                            continue;
                        }
                    }

                    //handle the loop + 1 problem by checking to see if the most recent queryResult
                    if (qr.done)
                    {
                        bContinue = false;
                        //Console.WriteLine("Registrou: " + loopCounter);
                    }
                    else
                        qr = binding.queryMore(qr.queryLocator);
                }

                return lstResult;
            }
            catch (Exception ex)
            {
                Console.WriteLine("\nProblema para executar a query: \n" + ex.Message);
                Console.Write("\nClique para continuar...");
                Console.ReadLine();
            }

            return null;
        }


        public static List<ClienteSalesforce> GetClientesPorNumero(string codigoEmpresa, List<string> lstClientes, ref SforceService binding)
        {
            String sql2 = string.Format(@"select   Id
                                                , Name
                                            from PointOfDelivery__c"); 
            
            CondicaoSimplesSql condicao1 = new CondicaoSimplesSql();
            condicao1.Campo = "Name";
            condicao1.Fixo = false;
            condicao1.Valor = lstClientes.Select(x => string.Concat("'", x.Replace("'", ""), "'")).ToList();

            CondicaoSimplesSql condicao2 = new CondicaoSimplesSql();
            condicao2.Campo = "CompanyID__c";
            condicao2.Fixo = true;
            condicao2.Valor = new List<string> { codigoEmpresa };

            List<CondicaoSql> condicoes = new List<CondicaoSql>();
            condicoes.Add(condicao1);
            condicoes.Add(condicao2);


            List<sObject> temp = ConsultarS(sql2, condicoes, ref binding);
            return EntidadeConversor.ToPointOfDelivery(temp);
        }



        /// <summary>
        /// Retorna os Medidores (Device) associados aos Clientes (PointOfDelivery)
        /// </summary>
        /// <param name="codigoEmpresa"></param>
        /// <param name="lstClientes"></param>
        /// <param name="binding"></param>
        /// <returns></returns>
        public static List<DeviceSalesforce> GetDevicesPorIdPoD(string codigoEmpresa, List<string> lstClientes, ref SforceService binding)
        {
            String sql2 = string.Format(@"select Id, PointofDelivery__c, Name, MeterNumber__c, Status__c
                                            from Device__c ");

            CondicaoSimplesSql condicao1 = new CondicaoSimplesSql();
            condicao1.Campo = "PointofDelivery__c";
            condicao1.Fixo = false;
            condicao1.Valor = lstClientes;

            List<CondicaoSql> condicoes = new List<CondicaoSql>();
            condicoes.Add(condicao1);


            List<sObject> temp = ConsultarS(sql2, condicoes, ref binding);
            return EntidadeConversor.ToDevice(temp);
        }



        /// <summary>
        /// 
        /// </summary>
        /// <param name="codigoEmpresa"></param>
        /// <param name="numeroCliente"></param>
        /// <param name="binding"></param>
        /// <returns></returns>
        public static List<BillingSalesforce> GetBillingsPorNumeroCliente(string nomeCompanhia, string codigoEmpresa, string numeroCliente, ref SforceService binding)
        {
            SFDCSchemeBuild schema = new SFDCSchemeBuild();
            String data = DateTime.Now.ToString("yyyyMMdd");

            QueryResult qr = null;
            binding.QueryOptionsValue = new apex.QueryOptions();
            binding.QueryOptionsValue.batchSize = 3;
            binding.QueryOptionsValue.batchSizeSpecified = true;

            try
            {
                bool bContinue = false;
                String sql = string.Format(@"SELECT Id, Pointofdelivery__r.Name 
                                               FROM Billing_Profile__c
                                              WHERE Pointofdelivery__r.Country__c = '{0}'
                                                and Pointofdelivery__r.CompanyId__c = '{1}'
                                                AND AccountContract__c = '{2}' ", nomeCompanhia, codigoEmpresa, numeroCliente);
                try
                {
                    qr = binding.query(sql);
                    bContinue = true;
                }
                catch
                { }

                List<BillingSalesforce> lstResult = new List<BillingSalesforce>();
                BillingSalesforce cli = null;

                while (bContinue)
                {
                    if (qr.records == null || qr.records.Length == 0)
                    {
                        bContinue = false;
                        continue;
                    }

                    for (int i = 0; i < qr.records.Length; i++)
                    {
                        apex.sObject con = qr.records[i];

                        String empresa = string.Empty;
                        String id = string.Empty;
                        string nroCliente = string.Empty;

                        id = schema.getFieldValue("Id", con.Any);
                        empresa = codigoEmpresa;
                        //nroCliente = schema.getFieldValue("AccountContract__c", con.Any);
                        nroCliente = numeroCliente.ToString();

                        cli = new BillingSalesforce();
                        cli.Company__c = empresa;
                        cli.Id = id;
                        cli.NumeroCliente = nroCliente;

                        lstResult.Add(cli);

                        if (qr.records.Length == 1)
                        {
                            bContinue = false;
                            continue;
                        }
                    }

                    //handle the loop + 1 problem by checking to see if the most recent queryResult
                    if (qr.done)
                    {
                        bContinue = false;
                        //Console.WriteLine("Registrou: " + loopCounter);
                    }
                    else
                        qr = binding.queryMore(qr.queryLocator);
                }

                return lstResult;
            }
            catch (Exception ex)
            {
                Console.WriteLine("\nProblema para executar a query: \n" + ex.Message);
                Console.Write("\nClique para continuar...");
                Console.ReadLine();
            }

            return null;
        }


        public static List<BillingSalesforce> GetBillingsPorNumeroContrato(string nomePais, string codigoEmpresa, string numeroContrato, ref SforceService binding)
        {
            SFDCSchemeBuild schema = new SFDCSchemeBuild();
            String data = DateTime.Now.ToString("yyyyMMdd");

            QueryResult qr = null;
            binding.QueryOptionsValue = new apex.QueryOptions();
            binding.QueryOptionsValue.batchSize = 3;
            binding.QueryOptionsValue.batchSizeSpecified = true;

            List<BillingSalesforce> lstResult = new List<BillingSalesforce>();
            try
            {
                bool bContinue = false;
                String sql = string.Format(@"SELECT Id, Pointofdelivery__r.Name, CreatedDate, ExternalId__c
                                               FROM Billing_Profile__c
                                              WHERE Pointofdelivery__r.Country__c = '{0}'
                                                and Pointofdelivery__r.CompanyId__c = '{1}'
                                                AND CNT_Contract__c = '{2}' 
                                           order by CreatedDate desc", nomePais, codigoEmpresa, numeroContrato);
                try
                {
                    qr = binding.query(sql);
                    bContinue = true;
                }
                catch
                { }

                BillingSalesforce cli = null;

                while (bContinue)
                {
                    if (qr.records == null || qr.records.Length == 0)
                    {
                        bContinue = false;
                        continue;
                    }

                    for (int i = 0; i < qr.records.Length; i++)
                    {
                        apex.sObject con = qr.records[i];

                        cli = new BillingSalesforce();
                        cli.Company__c = codigoEmpresa;
                        cli.Id = schema.getFieldValue("Id", con.Any);
                        cli.ExternalID__c = schema.getFieldValue("ExternalID__c", con.Any);
                        cli.CNT_Contract__c = numeroContrato;

                        lstResult.Add(cli);

                        //if (qr.records.Length == 1)
                        //{
                        //    bContinue = false;
                        //    continue;
                        //}
                    }

                    //handle the loop + 1 problem by checking to see if the most recent queryResult
                    if (qr.done)
                    {
                        bContinue = false;
                        //Console.WriteLine("Registrou: " + loopCounter);
                    }
                    else
                        qr = binding.queryMore(qr.queryLocator);
                }

                return lstResult;
            }
            catch (Exception ex)
            {
                Console.WriteLine("\nProblema para executar a query: \n" + ex.Message);
                Console.Write("\nClique para continuar...");
                Console.ReadLine();
            }

            return lstResult;
        }



        public static List<BillingSalesforce> GetBillingsPorExternalId(string externalId, ref SforceService binding)
        {
            SFDCSchemeBuild schema = new SFDCSchemeBuild();

            QueryResult qr = null;
            binding.QueryOptionsValue = new apex.QueryOptions();
            binding.QueryOptionsValue.batchSize = 3;
            binding.QueryOptionsValue.batchSizeSpecified = true;

            List<BillingSalesforce> lstResult = new List<BillingSalesforce>();
            try
            {
                bool bContinue = false;
                String sql = string.Format(@"SELECT Id, Pointofdelivery__r.Name, CreatedDate, ExternalId__c
                                               FROM Billing_Profile__c
                                              WHERE ExternalId__c = '{0}'", externalId);
                try
                {
                    qr = binding.query(sql);
                    bContinue = true;
                }
                catch
                { }

                BillingSalesforce cli = null;

                while (bContinue)
                {
                    if (qr.records == null || qr.records.Length == 0)
                    {
                        bContinue = false;
                        continue;
                    }

                    for (int i = 0; i < qr.records.Length; i++)
                    {
                        apex.sObject con = qr.records[i];

                        cli = new BillingSalesforce();
                        cli.Company__c = schema.getFieldValue("Company__c", con.Any);
                        cli.Id = schema.getFieldValue("Id", con.Any);
                        cli.ExternalID__c = schema.getFieldValue("ExternalID__c", con.Any);
                        cli.CNT_Contract__c = schema.getFieldValue("CNT_Contract__c", con.Any);

                        lstResult.Add(cli);

                        //if (qr.records.Length == 1)
                        //{
                        //    bContinue = false;
                        //    continue;
                        //}
                    }

                    //handle the loop + 1 problem by checking to see if the most recent queryResult
                    if (qr.done)
                    {
                        bContinue = false;
                        //Console.WriteLine("Registrou: " + loopCounter);
                    }
                    else
                        qr = binding.queryMore(qr.queryLocator);
                }

                return lstResult;
            }
            catch (Exception ex)
            {
                Console.WriteLine("\nProblema para executar a query: \n" + ex.Message);
                Console.Write("\nClique para continuar...");
                Console.ReadLine();
            }

            return lstResult;
        }


        public static List<BillingSalesforce> GetBillingsPorExternalId(List<string> lstExternalId, ref SforceService binding)
        {
            SFDCSchemeBuild schema = new SFDCSchemeBuild();

            QueryResult qr = null;
            binding.QueryOptionsValue = new apex.QueryOptions();
            binding.QueryOptionsValue.batchSize = 3;
            binding.QueryOptionsValue.batchSizeSpecified = true;

            List<BillingSalesforce> lstResult = new List<BillingSalesforce>();
            List<string> lstConsulta = new List<string>();
            StringBuilder cond = new StringBuilder();
            BillingSalesforce cli = null;

            String sqlbase = @"SELECT Id, Pointofdelivery__r.Name, CreatedDate, ExternalId__c
                                               FROM Billing_Profile__c
                                              WHERE ExternalId__c IN ({0}) ";

            foreach (string ext in lstExternalId.Distinct())
            {
                if ((sqlbase.Length + cond.Length + ext.Length) < (20000 - 3))
                {
                    cond.Append((cond.Length > 0) ? "," : string.Empty);
                    cond.Append(string.Concat("'", ext, "'"));

                    if (lstExternalId.Distinct().ToList().IndexOf(ext) != (lstExternalId.Distinct().Count() - 1))
                        continue;
                }

                if (cond.Length == 0)
                    continue;
                try
                {
                    bool bContinue = false;
                    String sql = string.Format(@"{0}", sqlbase).Replace("{0}", cond.ToString());

                    try
                    {
                        qr = binding.query(sql);
                        bContinue = true;
                    }
                    catch
                    { }

                    while (bContinue)
                    {
                        if (qr.records == null || qr.records.Length == 0)
                        {
                            bContinue = false;
                            continue;
                        }

                        for (int i = 0; i < qr.records.Length; i++)
                        {
                            apex.sObject con = qr.records[i];

                            cli = new BillingSalesforce();
                            cli.Company__c = schema.getFieldValue("Company__c", con.Any);
                            cli.Id = schema.getFieldValue("Id", con.Any);
                            cli.ExternalID__c = schema.getFieldValue("ExternalID__c", con.Any);
                            cli.CNT_Contract__c = schema.getFieldValue("CNT_Contract__c", con.Any);

                            if (lstResult.Where(x => x.ExternalID__c == cli.ExternalID__c).Count() > 0)
                            {
                                lstResult.Remove(lstResult.Where(x => x.ExternalID__c.Equals(cli.ExternalID__c)).FirstOrDefault());
                                cli.ExternalID__c = string.Concat(cli.ExternalID__c, "[DUPLIC]", new Random(1).Next(lstExternalId.Count));
                                lstResult.Add(cli);
                            }
                            else
                            {
                                lstResult.Add(cli);
                            }
                        }

                        //handle the loop + 1 problem by checking to see if the most recent queryResult
                        if (qr.done)
                            bContinue = false;
                        else
                            qr = binding.queryMore(qr.queryLocator);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("\nProblema para executar a query: \n" + ex.Message);
                    Console.Write("\nClique para continuar...");
                    Console.ReadLine();
                }

                cond.Clear();
                cond.Append(string.Concat("'", ext, "'"));
            }

            return lstResult;
        }


        
        
        /// <summary>
        /// TODO:
        /// </summary>
        /// <param name="codigoEmpresa"></param>
        /// <param name="numeroDocumento"></param>
        /// <param name="binding"></param>
        /// <returns></returns>
        public static List<ClienteSalesforce> GetContasPorDocumento(string codigoEmpresa, string numeroDocumento, ref SforceService binding)
        {
            SFDCSchemeBuild schema = new SFDCSchemeBuild();
            string ufEmpresa = codigoEmpresa;

            //String data = DateTime.Now.ToString("yyyyMMdd");

            QueryResult qr = null;
            binding.QueryOptionsValue = new SalesforceExtractor.apex.QueryOptions();
            binding.QueryOptionsValue.batchSize = 3;
            binding.QueryOptionsValue.batchSizeSpecified = true;

            //StringBuilder sbNomeArquivo = new StringBuilder();
            List<ClienteSalesforce> result = new List<ClienteSalesforce>();

            try
            {
                string cliente = "";
                String sql = string.Format(@"SELECT Id, ExternalId__c, IdentityNumber__c, IdentityType__c, Name, RecordTypeId
                    FROM Account  WHERE CompanyID__c = '{0}'
                    AND (IdentityNumber__c  = '{1}' OR IdentityNumber__c  = '{2}' OR IdentityNumber__c  = '{3}')"
                    , codigoEmpresa
                    , numeroDocumento
                    , numeroDocumento.PadLeft(14, '0')
                    , numeroDocumento.PadLeft(20, '0'));

                bool bContinue = false;

                try
                {
                    qr = binding.query(sql);

                    bContinue = true;
                }
                catch { }

                cliente = "";

                while (bContinue)
                {
                    if (qr.records == null)
                    {
                        bContinue = false;
                        continue;
                    }

                    for (int i = 0; i < qr.records.Length; i++)
                    {
                        SalesforceExtractor.apex.sObject con = qr.records[i];
                        ClienteSalesforce cli = new ClienteSalesforce();

                        cli.IdConta = schema.getFieldValue("Id", con.Any);
                        cli.ExternalId = schema.getFieldValue("ExternalId__c", con.Any);
                        cli.Documento = schema.getFieldValue("IdentityNumber__c", con.Any);
                        cli.TipoDocumento = schema.getFieldValue("IdentityType__c", con.Any);
                        cli.Nome = schema.getFieldValue("Name", con.Any);
                        cli.TipoRegistroId = schema.getFieldValue("RecordTypeId", con.Any);

                        result.Add(cli);

                        if (qr.done)
                        {
                            bContinue = false;
                            //Console.WriteLine("Registrou: " + loopCounter);
                        }
                        else
                            qr = binding.queryMore(qr.queryLocator);
                    }

                    return result;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(string.Format("\nProblema para executar a query: {0}\n", ex.Message));
                throw new Exception(ex.Message);
            }

            return null;
        }


        /// <summary>
        /// TODO:
        /// </summary>
        /// <param name="codigoEmpresa"></param>
        /// <param name="accountId"></param>
        /// <param name="binding"></param>
        /// <returns></returns>
        public static List<AssetDTO> GetAssetsPorId(string id, ref SforceService binding)
        {
            SFDCSchemeBuild schema = new SFDCSchemeBuild();
            List<AssetDTO> result = new List<AssetDTO>();

            QueryResult qr = null;
            binding.QueryOptionsValue = new SalesforceExtractor.apex.QueryOptions();
            binding.QueryOptionsValue.batchSize = 3;
            binding.QueryOptionsValue.batchSizeSpecified = true;

            try
            {
                string cliente = "";
                String sql = string.Format(@"SELECT Id, ExternalId__c, Account.IdentityNumber__c, Account.IdentityType__c, Name, RecordTypeId, Account.Id
                    FROM Asset  WHERE Id in ({0})"
                    , id);

                bool bContinue = false;

                try
                {
                    qr = binding.query(sql);

                    bContinue = true;
                }
                catch { }

                cliente = "";

                while (bContinue)
                {
                    if (qr.records == null)
                    {
                        bContinue = false;
                        continue;
                    }

                    if (qr.records.Length > 0)
                    {
                        for (int i = 0; i < qr.records.Length; i++)
                        {
                            SalesforceExtractor.apex.sObject con = qr.records[i];
                            AssetDTO obj = new AssetDTO();

                            obj.Id = schema.getFieldValue("Id", con.Any);
                            obj.ExternalId = schema.getFieldValue("ExternalId__c", con.Any);
                            obj.Identidade = schema.getFieldValueMore("Account", "", "", "IdentityNumber__c", con.Any);
                            obj.TipoIdentidade = schema.getFieldValueMore("Account", "", "", "IdentityType__c", con.Any);
                            obj.NomeCliente = schema.getFieldValueMore("Account", "", "", "Name", con.Any);
                            obj.AccountId = schema.getFieldValueMore("Account", "", "", "Id", con.Any);

                            result.Add(obj);

                            if (qr.done)
                            {
                                bContinue = false;
                                //Console.WriteLine("Registrou: " + loopCounter);
                            }
                            else
                                qr = binding.queryMore(qr.queryLocator);
                        }
                    }

                    return result;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(string.Format("\nProblema para executar a query: {0}\n", ex.Message));
                throw new Exception(ex.Message);
            }

            return null;
        }



        public static List<AssetDTO> GetAssetsPorExternalId(string id, ref SforceService binding)
        {
            SFDCSchemeBuild schema = new SFDCSchemeBuild();
            List<AssetDTO> result = new List<AssetDTO>();

            QueryResult qr = null;
            binding.QueryOptionsValue = new SalesforceExtractor.apex.QueryOptions();
            binding.QueryOptionsValue.batchSize = 3;
            binding.QueryOptionsValue.batchSizeSpecified = true;

            try
            {
                String sql = string.Format(@"SELECT   Id
                                                    , ExternalId__c
                                                    , Account.IdentityNumber__c
                                                    , Account.IdentityType__c
                                                    , Account.Name
                                                    , PointofDelivery__r.PointofDeliveryNumber__c
                                                    , PointofDelivery__r.Id
                                                    , PointofDelivery__r.ExternalId__c
                                                    , RecordTypeId
                                                    , Account.Id
                                                    , NE__Order_Config__c
                                                    , Contract__c
                                                    , PointofDelivery__r.SegmentType__c
                                                    , NE__Status__c
                                                    , CreatedDate
                                               FROM Asset  
                                              WHERE ExternalId__c = '{0}'", id);

                bool bContinue = false;
                try
                {
                    qr = binding.query(sql);
                    bContinue = true;
                }
                catch (Exception ex) { throw ex; }

                while (bContinue)
                {
                    if (qr.records == null)
                    {
                        bContinue = false;
                        continue;
                    }

                    if (qr.records.Length > 0)
                    {
                        for (int i = 0; i < qr.records.Length; i++)
                        {
                            SalesforceExtractor.apex.sObject con = qr.records[i];
                            AssetDTO obj = new AssetDTO();

                            obj.Id = schema.getFieldValue("Id", con.Any);
                            obj.ExternalId = schema.getFieldValue("ExternalId__c", con.Any);
                            obj.PointofDeliveryExternalId = schema.getFieldValueMore("PointofDelivery__r", "", "", "ExternalId__c", con.Any);
                            obj.PointofDeliveryId = schema.getFieldValueMore("PointofDelivery__r", "", "", "Id", con.Any);
                            //obj.ContractExternalId = schema.getFieldValue("Contract__c", con.Any);
                            obj.ContractId = schema.getFieldValue("Contract__c", con.Any);
                            obj.TipoCliente = schema.getFieldValueMore("PointofDelivery__r", "", "", "SegmentType__c", con.Any);
                            obj.Identidade = schema.getFieldValueMore("Account", "", "", "IdentityNumber__c", con.Any);
                            obj.TipoIdentidade = schema.getFieldValueMore("Account", "", "", "IdentityType__c", con.Any);
                            obj.NomeCliente = schema.getFieldValueMore("Account", "", "", "Name", con.Any);
                            obj.AccountId = schema.getFieldValueMore("Account", "", "", "Id", con.Any);
                            //obj.OrderItemId = schema.getFieldValue("NE__Order_Config__c", con.Any);
                            obj.OrderId = schema.getFieldValue("NE__Order_Config__c", con.Any);
                            obj.NumeroCliente = schema.getFieldValueMore("PointofDelivery__r", "", "", "PointofDeliveryNumber__c", con.Any);
                            obj.NumeroCliente = string.IsNullOrWhiteSpace(obj.NumeroCliente) ? string.Empty : Convert.ToInt32(Decimal.Parse(obj.NumeroCliente, CultureInfo.GetCultureInfo("en-US"))).ToString();
                            obj.Status = schema.getFieldValue("NE__Status__c", con.Any);
                            obj.DataCriacao = schema.getFieldValue("CreatedDate", con.Any);
                            
                            result.Add(obj);

                            if (qr.done)
                                bContinue = false;
                            else
                                qr = binding.queryMore(qr.queryLocator);
                        }
                    }

                    return result;
                }
            }
            catch (Exception ex)
            {
                Thread.Sleep(20 * 1000);
                return GetAssetsPorExternalId(id, ref binding);
                //Console.WriteLine(string.Format("\nProblema para executar a query: {0}{1}\n", ex.Message, ex.StackTrace));
                //throw new Exception(string.Format("\nProblema para executar a query: {0}{1}\n", ex.Message, ex.StackTrace));
            }

            return result;
        }


        public static List<AssetDTO> GetAssetsPorExternalId(List<string> lstExternalId, ref SforceService binding)
        {
            SFDCSchemeBuild schema = new SFDCSchemeBuild();

            QueryResult qr = null;
            binding.QueryOptionsValue = new SalesforceExtractor.apex.QueryOptions();
            binding.QueryOptionsValue.batchSize = 3;
            binding.QueryOptionsValue.batchSizeSpecified = true;

            String sqlbase = @"SELECT   Id
                                    , ExternalId__c
                                    , Account.IdentityNumber__c
                                    , Account.IdentityType__c
                                    , Account.Name
                                    , PointofDelivery__r.PointofDeliveryNumber__c
                                    , PointofDelivery__r.Id
                                    , PointofDelivery__r.ExternalId__c
                                    , RecordTypeId
                                    , Account.Id
                                    , NE__Order_Config__c
                                    , Contract__c
                                    , PointofDelivery__r.SegmentType__c
                                FROM Asset  
                                WHERE ExternalId__c IN ({0}) ";

            List<AssetDTO> lstResult = new List<AssetDTO>();
            List<string> lstConsulta = new List<string>();
            StringBuilder cond = new StringBuilder();
            AssetDTO obj = new AssetDTO();

            foreach (string ext in lstExternalId.Distinct())
            {
                if ((sqlbase.Length + cond.Length + ext.Length) < (10000 - 3))
                {
                    cond.Append((cond.Length > 0) ? "," : string.Empty);
                    cond.Append(string.Concat("'", ext, "'"));

                    if (lstExternalId.Distinct().ToList().IndexOf(ext) != (lstExternalId.Distinct().Count() - 1))
                        continue;
                }

                if (cond.Length == 0)
                    continue;
                try
                {
                    String sql = string.Format(@"{0} ", sqlbase).Replace("{0}", cond.ToString());

                    bool bContinue = false;
                    try
                    {
                        qr = binding.query(sql);
                        bContinue = true;
                    }
                    catch (Exception ex) { throw ex; }

                    while (bContinue)
                    {
                        if (qr.records == null)
                        {
                            bContinue = false;
                            continue;
                        }

                        for (int i = 0; i < qr.records.Length; i++)
                        {
                            SalesforceExtractor.apex.sObject con = qr.records[i];
                            obj = new AssetDTO();

                            obj.Id = schema.getFieldValue("Id", con.Any);
                            obj.ExternalId = schema.getFieldValue("ExternalId__c", con.Any);
                            obj.PointofDeliveryExternalId = schema.getFieldValueMore("PointofDelivery__r", "", "", "ExternalId__c", con.Any);
                            obj.PointofDeliveryId = schema.getFieldValueMore("PointofDelivery__r", "", "", "Id", con.Any);
                            //obj.ContractExternalId = schema.getFieldValue("Contract__c", con.Any);
                            obj.ContractId = schema.getFieldValue("Contract__c", con.Any);
                            obj.TipoCliente = schema.getFieldValueMore("PointofDelivery__r", "", "", "SegmentType__c", con.Any);
                            obj.Identidade = schema.getFieldValueMore("Account", "", "", "IdentityNumber__c", con.Any);
                            obj.TipoIdentidade = schema.getFieldValueMore("Account", "", "", "IdentityType__c", con.Any);
                            obj.NomeCliente = schema.getFieldValueMore("Account", "", "", "Name", con.Any);
                            obj.AccountId = schema.getFieldValueMore("Account", "", "", "Id", con.Any);
                            //obj.OrderItemId = schema.getFieldValue("NE__Order_Config__c", con.Any);
                            obj.OrderId = schema.getFieldValue("NE__Order_Config__c", con.Any); ;
                            obj.NumeroCliente = schema.getFieldValueMore("PointofDelivery__r", "", "", "PointofDeliveryNumber__c", con.Any);
                            obj.NumeroCliente = string.IsNullOrWhiteSpace(obj.NumeroCliente) ? string.Empty : Convert.ToInt32(Decimal.Parse(obj.NumeroCliente, CultureInfo.GetCultureInfo("en-US"))).ToString();

                            if (lstResult.Where(x => x.ExternalId.Equals(obj.ExternalId)).Count() > 0)
                            {
                                lstResult.Remove(lstResult.Where(x => x.ExternalId.Equals(obj.ExternalId)).FirstOrDefault());
                                obj.ExternalId = string.Concat(obj.ExternalId, "[DUPLIC]", new Random(1).Next(lstExternalId.Count));
                                lstResult.Add(obj);
                            }
                            else
                            {
                                lstResult.Add(obj);
                            }
                        }

                        if (qr.done)
                            bContinue = false;
                        else
                            qr = binding.queryMore(qr.queryLocator);
                    }
                }
                catch (Exception ex)
                {
                    Thread.Sleep(20 * 1000);
                    return GetAssetsPorExternalId(lstExternalId, ref binding);
                    //Console.WriteLine(string.Format("\nProblema para executar a query: {0}{1}\n", ex.Message, ex.StackTrace));
                    //throw new Exception(string.Format("\nProblema para executar a query: {0}{1}\n", ex.Message, ex.StackTrace));
                }

                cond.Clear();
                cond.Append(string.Concat("'", ext, "'"));
            }

            return lstResult;
        }


        /// <summary>
        /// TODO:
        /// </summary>
        /// <param name="codigoEmpresa"></param>
        /// <param name="accountId"></param>
        /// <param name="binding"></param>
        /// <returns></returns>
        public static List<AssetDTO> GetAssetsPorAccountId(string codigoEmpresa, string accountId, ref SforceService binding)
        {
            SFDCSchemeBuild schema = new SFDCSchemeBuild();
            string ufEmpresa = codigoEmpresa;

            List<AssetDTO> result = new List<AssetDTO>();

            QueryResult qr = null;
            binding.QueryOptionsValue = new SalesforceExtractor.apex.QueryOptions();
            binding.QueryOptionsValue.batchSize = 3;
            binding.QueryOptionsValue.batchSizeSpecified = true;

            try
            {
                String sql = string.Format(@"SELECT Id, ExternalId__c, Account.IdentityNumber__c, Account.IdentityType__c, Name, RecordTypeId, Account.Id
                                                    , CreatedDate
                    FROM Asset  WHERE AccountId = '{0}'"
                    , accountId);

                bool bContinue = false;

                try {
                    qr = binding.query(sql);

                    bContinue = true;
                }
                catch (Exception ex){
                    throw ex;
                }

                while (bContinue)
                {
                    if (qr.records == null)
                    {
                        bContinue = false;
                        continue;
                    }

                    if( qr.records.Length > 0)
                    {
                        for (int i = 0; i < qr.records.Length; i++)
                        {
                            SalesforceExtractor.apex.sObject con = qr.records[i];
                            AssetDTO obj = new AssetDTO();

                            obj.Id = schema.getFieldValue("Id", con.Any);
                            obj.ExternalId = schema.getFieldValue("ExternalId__c", con.Any);
                            obj.Identidade = schema.getFieldValueMore("Account", "", "", "IdentityNumber__c", con.Any);
                            obj.TipoIdentidade = schema.getFieldValueMore("Account", "", "", "IdentityType__c", con.Any);
                            obj.NomeCliente = schema.getFieldValueMore("Account", "", "", "Name", con.Any);
                            obj.AccountId = schema.getFieldValueMore("Account", "", "", "Id", con.Any);
                            obj.PointofDeliveryId = schema.getFieldValueMore("PointOfDelivery__c", "", "", "Id", con.Any);
                            obj.DataCriacao = schema.getFieldValue("CreatedDate", con.Any);

                            result.Add(obj);

                            if (qr.done)
                            {
                                bContinue = false;
                                //Console.WriteLine("Registrou: " + loopCounter);
                            }
                            else
                                qr = binding.queryMore(qr.queryLocator);
                        }
                    }

                    return result;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(string.Format("\nProblema para executar a query: {0}\n", ex.Message));
                throw new Exception(ex.Message);
            }

            return result;
        }


        public static List<AssetDTO> GetAssetsPorNumeroCliente(string codigoEmpresa, string numeroCliente, ref SforceService binding)
        {
            SFDCSchemeBuild schema = new SFDCSchemeBuild();
            string codEmpresa = codigoEmpresa;

            List<AssetDTO> result = new List<AssetDTO>();

            QueryResult qr = null;
            binding.QueryOptionsValue = new SalesforceExtractor.apex.QueryOptions();
            binding.QueryOptionsValue.batchSize = 3;
            binding.QueryOptionsValue.batchSizeSpecified = true;

            try
            {
                String sql = string.Format(@"SELECT Id
                                                , ExternalId__c
                                                , Account.IdentityNumber__c
                                                , Account.IdentityType__c
                                                , Name
                                                , RecordTypeId
                                                , Account.Id
                                                , CreatedDate
                                                , NE__Status__c
                                            FROM Asset  
                                            WHERE PointofDelivery__r.Name = '{0}'
                                                AND PointofDelivery__r.CompanyID__c = '{1}'
                                                AND PointofDelivery__r.Country__c = 'BRASIL'"
                                            , numeroCliente
                                            , codigoEmpresa);

                bool bContinue = false;

                try
                {
                    qr = binding.query(sql);

                    bContinue = true;
                }
                catch (Exception ex)
                {
                    throw ex;
                }

                while (bContinue)
                {
                    if (qr.records == null)
                    {
                        bContinue = false;
                        continue;
                    }

                    if (qr.records.Length > 0)
                    {
                        for (int i = 0; i < qr.records.Length; i++)
                        {
                            SalesforceExtractor.apex.sObject con = qr.records[i];
                            AssetDTO obj = new AssetDTO();

                            obj.Id = schema.getFieldValue("Id", con.Any);
                            obj.ExternalId = schema.getFieldValue("ExternalId__c", con.Any);
                            obj.Identidade = schema.getFieldValueMore("Account", "", "", "IdentityNumber__c", con.Any);
                            obj.TipoIdentidade = schema.getFieldValueMore("Account", "", "", "IdentityType__c", con.Any);
                            obj.NomeCliente = schema.getFieldValueMore("Account", "", "", "Name", con.Any);
                            obj.AccountId = schema.getFieldValueMore("Account", "", "", "Id", con.Any);
                            obj.PointofDeliveryId = schema.getFieldValueMore("PointOfDelivery__c", "", "", "Id", con.Any);
                            obj.DataCriacao = schema.getFieldValue("CreatedDate", con.Any);
                            obj.Status = schema.getFieldValue("NE__Status__c", con.Any);
                            obj.Name = schema.getFieldValue("Name", con.Any);

                            result.Add(obj);

                            if (qr.done)
                            {
                                bContinue = false;
                                //Console.WriteLine("Registrou: " + loopCounter);
                            }
                            else
                                qr = binding.queryMore(qr.queryLocator);
                        }
                    }

                    return result;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(string.Format("\nProblema para executar a query: {0}", ex.Message));
                //throw new Exception(ex.Message);
                Thread.Sleep(1000 * 10);
                return GetAssetsPorNumeroCliente(codigoEmpresa, numeroCliente, ref binding);
            }

            return result;
        }


        public static List<AssetDTO> GetAssetsPorNumeroContrato(string numeroContrato, ref SforceService binding)
        {
            SFDCSchemeBuild schema = new SFDCSchemeBuild();
            List<AssetDTO> result = new List<AssetDTO>();

            QueryResult qr = null;
            binding.QueryOptionsValue = new SalesforceExtractor.apex.QueryOptions();
            binding.QueryOptionsValue.batchSize = 3;
            binding.QueryOptionsValue.batchSizeSpecified = true;

            try
            {
                String sql = string.Format(@"select id
                                                    , NE__Status__c
                                                    , name
                                                    , PointofDelivery__c
                                                    , ExternalId__c
                                                    , CreatedDate
                                            from Asset 
                                            where NE__Order_Config__c in (
                                                select id
                                                from NE__Order__c 
                                                where CNT_Case__r.CNT_Contract__r.ContractNumber = '{0}')", numeroContrato);

                bool bContinue = false;

                try
                {
                    qr = binding.query(sql);

                    bContinue = true;
                }
                catch (Exception ex)
                {
                    throw ex;
                }

                while (bContinue)
                {
                    if (qr.records == null)
                    {
                        bContinue = false;
                        continue;
                    }

                    if (qr.records.Length > 0)
                    {
                        for (int i = 0; i < qr.records.Length; i++)
                        {
                            SalesforceExtractor.apex.sObject con = qr.records[i];
                            AssetDTO obj = new AssetDTO();

                            obj.Id = schema.getFieldValue("Id", con.Any);
                            obj.ExternalId = schema.getFieldValue("ExternalId__c", con.Any);
                            obj.PointofDeliveryId = schema.getFieldValue("PointOfDelivery__c", con.Any); 
                            obj.Status = schema.getFieldValue("NE__Status__c", con.Any);
                            obj.Name = schema.getFieldValue("Name", con.Any);
                            obj.DataCriacao = schema.getFieldValue("CreatedDate", con.Any);

                            result.Add(obj);

                            if (qr.done)
                            {
                                bContinue = false;
                                //Console.WriteLine("Registrou: " + loopCounter);
                            }
                            else
                                qr = binding.queryMore(qr.queryLocator);
                        }
                    }

                    return result;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(string.Format("\nProblema para executar a query: {0}", ex.Message));
                //throw new Exception(ex.Message);
                Thread.Sleep(1000 * 10);
                return GetAssetsPorNumeroContrato(numeroContrato, ref binding);
            }

            return result;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="pEmpresa"></param>
        public static void AtualizarModif(string codEmpresa, int qtdDias, TipoCliente tipoCliente, ref SforceService binding)
        {
            string pEmpresa = "2005".Equals(codEmpresa) ? "AMPLA" : "2003".Equals(codEmpresa) ? "COELCE" : string.Empty;

            //replica código de conexão porque o método foi definido como static
            //ConsultarSynergia conn = new ConsultarSynergia(pEmpresa, TipoCliente.GA);

            if (string.IsNullOrEmpty(pEmpresa))
                throw new ArgumentNullException("Empresa deve ser informada.");

            string idModifBase = null;
            int i = 0;
            int totalAtualizado = 0;
            int contCliente = 0;
            sObject update = null;
            List<sObject> listaUpdate = new List<sObject>();
            StringBuilder msgLog = new StringBuilder();

            ConsultarSynergia c = new ConsultarSynergia(pEmpresa, tipoCliente);
            using (Arquivo arqSaida = new Arquivo(string.Concat("C:\\temp\\ATUALIZAR_MODIF_", tipoCliente.ToString(), "_", codEmpresa, "_", DateTime.Now.AddDays((-qtdDias)).ToString("yyyyMMdd"), "_", qtdDias, ".txt")))
            {
                try
                {
                    msgLog.AppendLine(string.Format("{0}\tReplicação da Modif iniciada para {1} {2} em {3}."
                        , DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss")
                        , pEmpresa.Trim().ToUpper()
                        , tipoCliente.ToString()
                        , DateTime.Now.AddDays((-qtdDias)).ToString("dd-MM-yyyy")));
                    Console.WriteLine(msgLog.ToString());
                    IO.EscreverArquivo(arqSaida, msgLog.ToString());

                    List<ModifBase> lstFinal = c.criarTabTempClientesModificados(qtdDias);


                    msgLog.AppendLine(string.Format("{0}\t{1} Itens a atualizar identificados.", DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss"), lstFinal.Count()));
                    Console.WriteLine(msgLog.ToString());
                    IO.EscreverArquivo(arqSaida, msgLog.ToString());

                    if (lstFinal != null && lstFinal.Count == 0)
                    {
                        msgLog.AppendLine(string.Format("{0}\tNenhum item foi atualizado.", DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss")));
                        Console.WriteLine(msgLog.ToString());
                        IO.EscreverArquivo(arqSaida, msgLog.ToString());
                        return;
                    }

                    try
                    {
                        foreach (ModifBase rs in lstFinal)
                        {
                            idModifBase = null;
                            msgLog.Clear();
                            contCliente++;

                            if (string.IsNullOrEmpty(rs.Identificador) || rs.Identificador.ToLower().Contains("invalido"))
                            {
                                msgLog.AppendLine(string.Format("Identificador inválido ou não informado: {0}", string.IsNullOrWhiteSpace(rs.Identificador) ? rs.EntidadeSalesforce : rs.Identificador));
                                Console.WriteLine(msgLog.ToString());
                                IO.EscreverArquivo(arqSaida, msgLog.ToString());
                                continue;
                            }

                            List<ClienteSalesforce> cli = null;
                            if (typeof(Account) == rs.GetType())
                            {
                                cli = GetContasPorExternalId(codEmpresa, string.Concat("'", rs.Identificador, "'"), ref binding);
                                if (cli == null || cli.Count == 0)
                                {
                                    msgLog.AppendFormat("ACCOUNT\tnao identificada: Empresa:{0} ExternalId: {1}", codEmpresa, string.IsNullOrWhiteSpace(rs.Identificador) ? string.Empty : rs.Identificador);
                                    Console.WriteLine(msgLog.ToString());
                                    IO.EscreverArquivo(arqSaida, msgLog.ToString());
                                    continue;
                                }
                                idModifBase = cli[0].Id;
                            }
                            if (typeof(PointOfDelivery) == rs.GetType())
                            {
                                cli = GetPodsPorExternalId(rs.Identificador, ref binding);
                                if (cli == null || cli.Count == 0)
                                {
                                    msgLog.AppendFormat("POD\tnao identificado: Empresa:{0} ExternalId: {1}", codEmpresa, string.IsNullOrWhiteSpace(rs.Identificador) ? string.Empty : rs.Identificador);
                                    Console.WriteLine(msgLog.ToString());
                                    IO.EscreverArquivo(arqSaida, msgLog.ToString());
                                    continue;
                                }
                                idModifBase = cli[0].IdPod;
                            }
                            if (typeof(Contact) == rs.GetType())
                            {
                                cli = GetContatosPorExternalId(codEmpresa, rs.Identificador, ref binding);
                                if (cli == null || cli.Count == 0)
                                {
                                    msgLog.AppendFormat("CONTACT\tnao identificado: Empresa:{0} ExternalId: {1}", codEmpresa, string.IsNullOrWhiteSpace(rs.Identificador) ? string.Empty : rs.Identificador);
                                    Console.WriteLine(msgLog.ToString());
                                    IO.EscreverArquivo(arqSaida, msgLog.ToString());
                                    continue;
                                }
                                idModifBase = cli[0].IdContato;
                            }

                            try
                            {
                                if (cli.Count > 1)
                                {
                                    msgLog.AppendFormat("{0}|{1}", cli.Count, rs.Identificador);
                                    Console.WriteLine(msgLog.ToString());
                                    IO.EscreverArquivo(arqSaida, string.Format("{0}|{1}", cli.Count(), cli[0].ExternalId));
                                }
                            }
                            catch (NullReferenceException ex)
                            {
                                Console.WriteLine(string.Format("0|{0}", rs.Identificador));
                                continue;
                            }
                            catch (Exception ex)
                            {
                                throw ex;
                            }

                            //TODO: limitar atualização a 1 Account mas permitir mais em Contacts, Assets, etc
                            //TODO: guardar o AccountId

                            if (string.IsNullOrWhiteSpace(idModifBase))
                            {
                                msgLog.AppendFormat("[ERRO]\tId não encontrado para o Identificador {0} em {1}", rs.Identificador, rs.GetType().ToString());
                                Console.WriteLine(msgLog.ToString());
                                IO.EscreverArquivo(arqSaida, string.Format("{0}|{1}", cli.Count(), cli[0].ExternalId));
                                return;
                            }
                            update = new sObject();
                            update.type = rs.EntidadeSalesforce;
                            update.Id = idModifBase;

                            List<XmlElement> _lstTempXml = new List<XmlElement>();
                            XmlElement _arrXml;

                            //TODO: validar arquivo saida
                            //TODO: definir melhor parametrização para o arquivo saida

                            foreach (ItemEntidade item in rs.ItemsModificados)
                            {
                                IO.EscreverArquivo(arqSaida, string.Concat(rs.EntidadeSalesforce, " [", update.Id, "] - ", item.CampoSalesforce, " - ", item.NovoValor));

                                _arrXml = SFDCSchemeBuild.GetNewXmlElement(item.CampoSalesforce, item.NovoValor);
                                _lstTempXml.Add(_arrXml);
                            }

                            XmlElement[] arr = _lstTempXml.ToArray();
                            update.Any = arr;

                            listaUpdate.Add(update);
                            i++;
                            totalAtualizado++;

                            //Update a cada 200 registros, por limitação do Sales ce
                            if (listaUpdate.Count > 0 && i == 8)
                            {
                                SaveResult[] saveResults = binding.update(listaUpdate.ToArray());
                                string idsUpdate = string.Join(",", listaUpdate.Select(t => t.Id).ToArray());
                                if (saveResults[0].errors != null)
                                {
                                    Console.WriteLine(string.Format("[ERRO] {0}", idsUpdate));
                                    foreach (Error err in saveResults[0].errors)
                                    {
                                        Console.WriteLine(string.Format("[ERRO] {0}", err.message));
                                        IO.EscreverArquivo(arqSaida, string.Format("[ERRO] {0}", err.message));
                                    }
                                }
                                else
                                {
                                    Console.WriteLine(string.Format("[OK] {0}", idsUpdate));
                                    IO.EscreverArquivo(arqSaida, string.Format("[OK] {0}", idsUpdate));
                                }

                                i = 0;
                                listaUpdate.Clear();
                            }
                        }  // fim foreach


                        //Update do remanescente da lista
                        if (listaUpdate.Count > 0 && i < 8)
                        {
                            totalAtualizado += i;
                            SaveResult[] saveResults = binding.update(listaUpdate.ToArray());
                            string idsUpdate = string.Join(",", listaUpdate.Select(t => t.Id).ToArray());
                            if (saveResults != null && saveResults[0].errors != null)
                            {
                                Console.WriteLine(string.Format("[ERRO] {0}", idsUpdate));
                                foreach (Error err in saveResults[0].errors)
                                {
                                    Console.WriteLine(string.Format("[ERRO] {0}", err.message));
                                    IO.EscreverArquivo(arqSaida, string.Format("[ERRO] {0}", err.message));
                                }
                            }
                            else
                            {
                                Console.WriteLine(string.Format("[OK FINAL]\t{0}", idsUpdate));
                                IO.EscreverArquivo(arqSaida, string.Format("[OK FINAL]\t{0}", idsUpdate));
                            }

                            i = 0;
                            listaUpdate.Clear();
                        }

                        msgLog.Clear();
                        msgLog.AppendLine(string.Format("Processo finalizado em {0}", DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss")));
                        Console.WriteLine(msgLog.ToString());
                        IO.EscreverArquivo(arqSaida, msgLog.ToString());
                    }
                    catch (Exception ex)
                    {
                        msgLog.Clear();
                        msgLog.AppendLine(("\nErro ao atualizar o registro: \n" + ex.Message + ex.StackTrace));
                        Console.WriteLine(msgLog.ToString());
                        IO.EscreverArquivo(arqSaida, msgLog.ToString());
                    }
                }
                catch (Exception ex)
                {
                    msgLog.Clear();
                    msgLog.AppendLine(string.Format("{0} {1}", ex.Message, ex.StackTrace));
                    Console.WriteLine(msgLog.ToString());
                    IO.EscreverArquivo(arqSaida, msgLog.ToString());
                }
            }
        }




        public static Dictionary<string,string> GetAssetItemsAttributePorAsset(string assetExternalId, ref SforceService binding)
        {
            SFDCSchemeBuild schema = new SFDCSchemeBuild();
            QueryResult qr = null;
            binding.QueryOptionsValue = new apex.QueryOptions();
            binding.QueryOptionsValue.batchSize = 3;
            binding.QueryOptionsValue.batchSizeSpecified = true;

            String sql = string.Format(@"SELECT Id, name, NE__Value__c, NE__Old_Value__c
                                               FROM NE__AssetItemAttribute__c
                                              where NE__Asset__r.ExternalId__c = '{0}'
                                                    and NE__Asset__r.NE__Status__c IN ('Active','In Progress')"
                    , assetExternalId);

                bool bContinue = false;
                try
                {
                    qr = binding.query(sql);
                    bContinue = true;
                }
                catch (Exception ex)
                {
                    Debugger.Break();
                    throw ex;
                }

            Dictionary<string, string> lstResult = new Dictionary<string, string>();
            while (bContinue)
            {
                try
                {
                    if (qr.records == null || qr.records.Length == 0)
                    {
                        bContinue = false;
                        continue;
                    }

                    for (int i = 0; i < qr.records.Length; i++)
                    {
                        apex.sObject con = qr.records[i];
                        try
                        {
                            if (lstResult.ContainsKey(con.Any[1].InnerText))
                            {
                                int repetidas = lstResult.Where(x => x.Key.Contains(con.Any[1].InnerText)).Count();
                                lstResult.Add(con.Any[1].InnerText + "_" + ++repetidas, con.Any[0].InnerText);
                            }
                            else
                            {
                                lstResult.Add(con.Any[1].InnerText, con.Any[0].InnerText);
                            }
                        }
                        catch (Exception x)
                        {

                        }
                    }

                    //handle the loop + 1 problem by checking to see if the most recent queryResult
                    if (qr.done)
                    {
                        bContinue = false;
                        //Console.WriteLine("Registrou: " + loopCounter);
                    }
                    else
                        qr = binding.queryMore(qr.queryLocator);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("\nProblema para executar a query: \n" + ex.Message);
                    Thread.Sleep(1000 * 10);
                }

            }
            return lstResult;
        }



        public static Dictionary<string, string> GetAssetItemsAttributeValuesPorAsset(string assetExternalId, ref SforceService binding)
        {
            SFDCSchemeBuild schema = new SFDCSchemeBuild();
            QueryResult qr = null;
            binding.QueryOptionsValue = new apex.QueryOptions();
            binding.QueryOptionsValue.batchSize = 3;
            binding.QueryOptionsValue.batchSizeSpecified = true;

            String sql = string.Format(@"SELECT Id, name, NE__Value__c, NE__Old_Value__c
                                               FROM NE__AssetItemAttribute__c
                                              where NE__Asset__r.ExternalId__c = '{0}'
                                                    and NE__Asset__r.NE__Status__c IN ('Active','In Progress')"
                , assetExternalId);

                bool bContinue = false;
                try
                {
                    qr = binding.query(sql);
                    bContinue = true;
                }
                catch (Exception ex)
                {
                    Debugger.Break();
                    throw ex;
                }

            Dictionary<string, string> lstResult = new Dictionary<string, string>();
            while (bContinue)
            {
                try
                {
                    if (qr.records == null || qr.records.Length == 0)
                    {
                        bContinue = false;
                        continue;
                    }

                    for (int i = 0; i < qr.records.Length; i++)
                    {
                        apex.sObject con = qr.records[i];
                        try
                        {
                            if (!lstResult.ContainsKey(con.Any[1].InnerText))
                                lstResult.Add(con.Any[1].InnerText, con.Any[2].InnerText);
                        }
                        catch (Exception x)
                        {

                        }
                    }

                    //handle the loop + 1 problem by checking to see if the most recent queryResult
                    if (qr.done)
                    {
                        bContinue = false;
                        //Console.WriteLine("Registrou: " + loopCounter);
                    }
                    else
                        qr = binding.queryMore(qr.queryLocator);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("\nProblema para executar a query: \n" + ex.Message);
                    Thread.Sleep(1000 * 10);
                }
            }

            return lstResult;
        }

        
        
        /// <summary>
        /// Retorna o Id do AssetItemAttribute Ativo referente a cada ItemAttribute associado ao cliente.
        /// </summary>
        /// <param name="codigoEmpresa">CE=2003, RJ=2005</param>
        /// <param name="numeroCliente"></param>
        /// <param name="grupoTensao">GA=A, GB=B</param>
        /// <param name="nomeItemAtributo"></param>
        /// <returns></returns>
        public static string GetIdAssetItemAttributePorItem(string assetExternalId, string nomeItemAtributo, ref SforceService binding)
        {
            //TODO: validar parametros

            SFDCSchemeBuild schema = new SFDCSchemeBuild();
            QueryResult qr = null;
            binding.QueryOptionsValue = new apex.QueryOptions();
            binding.QueryOptionsValue.batchSize = 3;
            binding.QueryOptionsValue.batchSizeSpecified = true;

            //and NE__Asset__r.Pointofdelivery__r.SegmentType__c = '{2}'
            String sql = string.Format(@"SELECT Id,name,NE__Value__c,NE__Old_Value__c
                                               FROM NE__AssetItemAttribute__c
                                              where NE__Asset__r.ExternalId__c = '{0}'
                                                    and (NE__Asset__r.NE__Status__c = 'Active' OR NE__Asset__r.NE__Status__c = 'In Progress')
                                                    and Name = '{1}'"
                    , assetExternalId, nomeItemAtributo.Trim());

                bool bContinue = false;
                try
                {
                    qr = binding.query(sql);
                    bContinue = true;
                }
                catch(Exception ex)
                {
                    Debugger.Break();
                    throw ex;
                }

                //cliente = string.Empty;
                List<ItemAttribute> lstResult = new List<ItemAttribute>();
                ItemAttribute _item = null;

            while (bContinue)
            {
                try
                {
                    if (qr.records == null || qr.records.Length == 0)
                    {
                        bContinue = false;
                        continue;
                    }

                    for (int i = 0; i < qr.records.Length; i++)
                    {
                        apex.sObject con = qr.records[i];

                        _item = new ItemAttribute();
                        _item.Id = schema.getFieldValue("Id", con.Any); ;

                        lstResult.Add(_item);

                        if (qr.records.Length == 1)
                        {
                            bContinue = false;
                            continue;
                        }
                    }

                    //handle the loop + 1 problem by checking to see if the most recent queryResult
                    if (qr.done)
                    {
                        bContinue = false;
                        //Console.WriteLine("Registrou: " + loopCounter);
                    }
                    else
                        qr = binding.queryMore(qr.queryLocator);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("\nProblema para executar a query: \n" + ex.Message);
                    Thread.Sleep(1000 * 10);
                }
            }

            return _item != null ? _item.Id : string.Empty;
        }



        public static List<string> GetAssetItemAttributesPorIdAsset(string idAsset, ref SforceService binding)
        {
            SFDCSchemeBuild schema = new SFDCSchemeBuild();
            QueryResult qr = null;
            binding.QueryOptionsValue = new apex.QueryOptions();
            binding.QueryOptionsValue.batchSize = 3;
            binding.QueryOptionsValue.batchSizeSpecified = true;

            String sql = string.Format(@"SELECT Id,name,NE__Value__c,NE__Old_Value__c
                                               FROM NE__AssetItemAttribute__c
                                              where NE__Asset__c = '{0}'"
                    , idAsset);

                bool bContinue = false;
                try
                {
                    qr = binding.query(sql);
                    bContinue = true;
                }
                catch (Exception ex)
                {
                    Debugger.Break();
                    throw ex;
                }

                List<string> lstResult = new List<string>();

            while (bContinue)
            {
                try
                {
                    if (qr.records == null || qr.records.Length == 0)
                    {
                        bContinue = false;
                        continue;
                    }

                    for (int i = 0; i < qr.records.Length; i++)
                    {
                        apex.sObject con = qr.records[i];

                        lstResult.Add(schema.getFieldValue("Id", con.Any));

                        if (qr.records.Length == 1)
                        {
                            bContinue = false;
                            continue;
                        }
                    }

                    //handle the loop + 1 problem by checking to see if the most recent queryResult
                    if (qr.done)
                        bContinue = false;
                    else
                        qr = binding.queryMore(qr.queryLocator);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("\nProblema para executar a query: \n" + ex.Message);
                    Console.Write("\nClique para continuar...");
                    Console.ReadLine();
                }
            }

            return lstResult;
        }



        /// <summary>
        /// 
        /// </summary>
        /// <param name="codigoEmpresa"></param>
        /// <param name="numeroCliente"></param>
        /// <param name="assetExternalId"></param>
        /// <param name="nomeItemAtributo"></param>
        /// <param name="binding"></param>
        /// <returns></returns>
        public static string GetIdOrderItemAttributePorItem(string assetExternalId, string nomeItemAtributo, ref SforceService binding)
        {
            List<AssetDTO> assets = GetAssetsPorExternalId(assetExternalId, ref binding);
            if (assets == null || assets.Count == 0)
                throw new Exception(string.Format("External ID de Asset não encontrado: {0}", assetExternalId));

            SFDCSchemeBuild schema = new SFDCSchemeBuild();
            QueryResult qr = null;
            binding.QueryOptionsValue = new apex.QueryOptions();
            binding.QueryOptionsValue.batchSize = 3;
            binding.QueryOptionsValue.batchSizeSpecified = true;

            String sql = string.Format(@"SELECT Id,name,NE__Value__c,NE__Old_Value__c 
                                               FROM NE__Order_Item_Attribute__c
                                              where Name = '{0}'
                                                and NE__Order_Item__r.NE__OrderId__r.Id = '{1}'"
                , nomeItemAtributo.Trim(), assets.FirstOrDefault().OrderItemId);

                bool bContinue = false;
                try
                {
                    qr = binding.query(sql);
                    bContinue = true;
                }
                catch (Exception ex)
                {
                    throw ex;
                }

                //cliente = string.Empty;
                List<ItemAttribute> lstResult = new List<ItemAttribute>();
                ItemAttribute _item = null;

            while (bContinue)
            {
                try
                {
                    if (qr.records == null || qr.records.Length == 0)
                    {
                        bContinue = false;
                        continue;
                    }

                    for (int i = 0; i < qr.records.Length; i++)
                    {
                        apex.sObject con = qr.records[i];

                        _item = new ItemAttribute();
                        _item.Id = schema.getFieldValue("Id", con.Any); ;

                        lstResult.Add(_item);

                        if (qr.records.Length == 1)
                        {
                            bContinue = false;
                            continue;
                        }
                    }

                    //handle the loop + 1 problem by checking to see if the most recent queryResult
                    if (qr.done)
                    {
                        bContinue = false;
                        //Console.WriteLine("Registrou: " + loopCounter);
                    }
                    else
                        qr = binding.queryMore(qr.queryLocator);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("\nProblema para executar a query: \n" + ex.Message);
                    Console.Write("\nClique para continuar...");
                    Debugger.Break();
                    Console.ReadLine();
                }
            }

            return _item != null ? _item.Id : string.Empty;
        }


        /// <summary>
        /// Recupera os Ids de Geração Distribuída associados a um cliente gerador de energia.   Tabela "CNT_Distributed_Generation__c"
        /// </summary>
        /// <param name="clienteGerados"></param>
        /// <param name="binding"></param>
        /// <returns></returns>
        public static string GetGeracaoDistribuidaByGerador(string clienteGerador, ref SforceService binding)
        {
//            SFDCSchemeBuild schema = new SFDCSchemeBuild();
//            QueryResult qr = null;
//            binding.QueryOptionsValue = new apex.QueryOptions();
//            binding.QueryOptionsValue.batchSize = 3;
//            binding.QueryOptionsValue.batchSizeSpecified = true;

//            try
//            {
//                String sql = string.Format(@"SELECT Id
//                                               FROM CNT_Distributed_Generation__c
//                                              where CNT_Donator__c = '{0}'
//                                                and NE__Order_Item__r.NE__OrderId__r.Id = '{1}'"
//                    , nomeItemAtributo.Trim(), assets.FirstOrDefault().OrderItemId);

//                bool bContinue = false;
//                try
//                {
//                    qr = binding.query(sql);
//                    bContinue = true;
//                }
//                catch (Exception ex)
//                {
//                    throw ex;
//                }

//                //cliente = string.Empty;
//                List<ItemAttribute> lstResult = new List<ItemAttribute>();
//                ItemAttribute _item = null;

//                while (bContinue)
//                {
//                    if (qr.records == null || qr.records.Length == 0)
//                    {
//                        bContinue = false;
//                        continue;
//                    }

//                    for (int i = 0; i < qr.records.Length; i++)
//                    {
//                        apex.sObject con = qr.records[i];

//                        _item = new ItemAttribute();
//                        _item.Id = schema.getFieldValue("Id", con.Any); ;

//                        lstResult.Add(_item);

//                        if (qr.records.Length == 1)
//                        {
//                            bContinue = false;
//                            continue;
//                        }
//                    }

//                    //handle the loop + 1 problem by checking to see if the most recent queryResult
//                    if (qr.done)
//                    {
//                        bContinue = false;
//                        //Console.WriteLine("Registrou: " + loopCounter);
//                    }
//                    else
//                        qr = binding.queryMore(qr.queryLocator);
//                }

//                return _item != null ? _item.Id : string.Empty;
//            }
//            catch (Exception ex)
//            {
//                Console.WriteLine("\nProblema para executar a query: \n" + ex.Message);
//                Console.Write("\nClique para continuar...");
//                Debugger.Break();
//                Console.ReadLine();
//            }

            return null;
        }


        public static OrderSalesforce GetOrderPorId(string orderId, ref SforceService binding)
        {
            SFDCSchemeBuild schema = new SFDCSchemeBuild();
            QueryResult qr = null;
            binding.QueryOptionsValue = new apex.QueryOptions();
            binding.QueryOptionsValue.batchSize = 3;
            binding.QueryOptionsValue.batchSizeSpecified = true;

            String sql = string.Format(@"select NE__Order_Item__r.Id
                                            , NE__Order_Item__r.NE__OrderId__c
                                            , NE__Order_Item__r.NE__OrderId__r.Billing_profile__c
	                                        , NE__Order_Item__r.NE__OrderId__r.NE__Country__c
	                                        , NE__Order_Item__r.NE__OrderId__r.CurrencyIsoCode
	                                        , NE__Order_Item__r.NE__OrderId__r.NE__AccountId__c
	                                        , NE__Order_Item__r.NE__OrderId__r.NE__BillAccId__c
	                                        , NE__Order_Item__r.NE__OrderId__r.NE__CatalogId__c
	                                        , NE__Order_Item__r.NE__OrderId__r.NE__Order_date__c
	                                        , NE__Order_Item__r.NE__OrderId__r.NE__rTypeName__c
	                                        , NE__Order_Item__r.NE__OrderId__r.NE__ServAccId__c
	                                        , NE__Order_Item__r.NE__OrderId__r.NE__Type__c
	                                        , NE__Order_Item__r.NE__OrderId__r.RecordTypeId

                                            , id
	                                        , name
	                                        , NE__Value__c
                                        from NE__Order_Item_Attribute__c ");

            CondicaoSimplesSql cond = new CondicaoSimplesSql();
            cond.Campo = "NE__Order_Item__r.NE__OrderId__c";
            cond.Valor = new List<string> {{ string.Concat("'", orderId, "'") }};

            return EntidadeConversor.FromOrderItemAttributeToOrders(ConsultarS(sql, new List<CondicaoSql> { { cond } }, ref binding)).FirstOrDefault();
        }


        public static List<string> GetOrdersPorOrderItemId(string itemOrderId, ref SforceService binding)
        {
            SFDCSchemeBuild schema = new SFDCSchemeBuild();
            QueryResult qr = null;
            binding.QueryOptionsValue = new apex.QueryOptions();
            binding.QueryOptionsValue.batchSize = 3;
            binding.QueryOptionsValue.batchSizeSpecified = true;

            String sql = string.Format(@"SELECT Id, NE__OrderId__c
                                               FROM NE__OrderItem__c 
                                              WHERE Id in ({0})"
                    , itemOrderId);

                bool bContinue = false;
                try
                {
                    qr = binding.query(sql);
                    bContinue = true;
                }
                catch (Exception ex)
                {
                    throw ex;
                }

                //cliente = string.Empty;
                List<string> lstResult = new List<string>();

            while (bContinue)
            {
                try
                {
                    if (qr.records == null || qr.records.Length == 0)
                    {
                        bContinue = false;
                        continue;
                    }

                    for (int i = 0; i < qr.records.Length; i++)
                    {
                        apex.sObject con = qr.records[i];
                        try
                        {
                            string orderId = schema.getFieldValue("NE__OrderId__c", con.Any);

                            if (!lstResult.Contains(orderId))
                                lstResult.Add(orderId);
                        }
                        catch (Exception x)
                        {
                            //TODO:  logar ou tratar como exceção mesmo, por nao ser possivel ter +1 Attribute por Order e ser um caso de erro em UAT apenas
                            continue;
                        }
                        if (qr.records.Length == 1)
                        {
                            bContinue = false;
                            continue;
                        }
                    }

                    //handle the loop + 1 problem by checking to see if the most recent queryResult
                    if (qr.done)
                    {
                        bContinue = false;
                        //Console.WriteLine("Registrou: " + loopCounter);
                    }
                    else
                        qr = binding.queryMore(qr.queryLocator);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("\nProblema para executar a query: \n" + ex.Message);
                    Console.Write("\nClique para continuar...");
                    Debugger.Break();
                    Console.ReadLine();
                }
            }

            return lstResult;
        }


        public static List<string> GetRelatedContactByAccountId(List<string> accountIds, ref SforceService binding)
        {
            SFDCSchemeBuild schema = new SFDCSchemeBuild();
            QueryResult qr = null;
            binding.QueryOptionsValue = new apex.QueryOptions();
            binding.QueryOptionsValue.batchSize = 3;
            binding.QueryOptionsValue.batchSizeSpecified = true;

            String sql = string.Format(@" select    Id
                                            from    AccountContactRelation
                                           where    AccountId in ('{0}')"
                    , string.Join("','", accountIds));

                bool bContinue = false;
                try
                {
                    qr = binding.query(sql);
                    bContinue = true;
                }
                catch (Exception ex)
                {
                    throw ex;
                }

                List<string> lstResult = new List<string>();

            while (bContinue)
            {
                try
                {
                    if (qr.records == null || qr.records.Length == 0)
                    {
                        bContinue = false;
                        continue;
                    }

                    for (int i = 0; i < qr.records.Length; i++)
                    {
                        apex.sObject con = qr.records[i];
                        try
                        {
                            string orderId = schema.getFieldValue("Id", con.Any);

                            if (!lstResult.Contains(orderId))
                                lstResult.Add(orderId);
                        }
                        catch (Exception x)
                        {
                            //TODO:  logar ou tratar como exceção mesmo, por nao ser possivel ter +1 Attribute por Order e ser um caso de erro em UAT apenas
                            continue;
                        }
                        if (qr.records.Length == 1)
                        {
                            bContinue = false;
                            continue;
                        }
                    }

                    //handle the loop + 1 problem by checking to see if the most recent queryResult
                    if (qr.done)
                    {
                        bContinue = false;
                        //Console.WriteLine("Registrou: " + loopCounter);
                    }
                    else
                        qr = binding.queryMore(qr.queryLocator);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("\nProblema para executar a query: \n" + ex.Message);
                    Console.Write("\nClique para continuar...");
                    Debugger.Break();
                    Console.ReadLine();
                }
            }

            return lstResult;
        }


        public static Dictionary<string, string> GetOrderItemsAttributePorItem(string orderId, ref SforceService binding)
        {
            SFDCSchemeBuild schema = new SFDCSchemeBuild();
            QueryResult qr = null;
            binding.QueryOptionsValue = new apex.QueryOptions();
            binding.QueryOptionsValue.batchSize = 3;
            binding.QueryOptionsValue.batchSizeSpecified = true;

            String sql = string.Format(@"SELECT Id,name,NE__Value__c,NE__Old_Value__c 
                                               FROM NE__Order_Item_Attribute__c
                                              where NE__Order_Item__r.NE__OrderId__r.Id = '{0}'"
                    , orderId);

                bool bContinue = false;
                try
                {
                    qr = binding.query(sql);
                    bContinue = true;
                }
                catch (Exception ex)
                {
                    throw ex;
                }

                //cliente = string.Empty;
                Dictionary<string, string> lstResult = new Dictionary<string,string>();

            while (bContinue)
            {
                try
                {
                    if (qr.records == null || qr.records.Length == 0)
                    {
                        bContinue = false;
                        continue;
                    }

                    for (int i = 0; i < qr.records.Length; i++)
                    {
                        apex.sObject con = qr.records[i];
                        try
                        {
                            if (!lstResult.ContainsKey(con.Any[1].InnerText))
                                lstResult.Add(con.Any[1].InnerText, con.Any[0].InnerText);
                        }
                        catch(Exception x)
                        {
                            //TODO:  logar ou tratar como exceção mesmo, por nao ser possivel ter +1 Attribute por Order e ser um caso de erro em UAT apenas
                            continue;
                        }
                        if (qr.records.Length == 1)
                        {
                            bContinue = false;
                            continue;
                        }
                    }

                    //handle the loop + 1 problem by checking to see if the most recent queryResult
                    if (qr.done)
                    {
                        bContinue = false;
                        //Console.WriteLine("Registrou: " + loopCounter);
                    }
                    else
                        qr = binding.queryMore(qr.queryLocator);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("\nProblema para executar a query: \n" + ex.Message);
                    Thread.Sleep(1000 * 10);
                }
            }

            return lstResult;
        }


        public static List<OrderItemAttributeSalesforce> GetOrderItemsAttributePorId(string orderId, ref SforceService binding)
        {
            SFDCSchemeBuild schema = new SFDCSchemeBuild();
            QueryResult qr = null;
            binding.QueryOptionsValue = new apex.QueryOptions();
            binding.QueryOptionsValue.batchSize = 3;
            binding.QueryOptionsValue.batchSizeSpecified = true;

            try
            {
                String sql = string.Format(@"SELECT Id,name,NE__Value__c,NE__Old_Value__c 
                                               FROM NE__Order_Item_Attribute__c
                                              where NE__Order_Item__r.NE__OrderId__r.Id = '{0}'"
                    , orderId);

                bool bContinue = false;
                try
                {
                    qr = binding.query(sql);
                    bContinue = true;
                }
                catch (Exception ex)
                {
                    throw ex;
                }

                List<OrderItemAttributeSalesforce> lstResult = new List<OrderItemAttributeSalesforce>();

                while (bContinue)
                {
                    if (qr.records == null || qr.records.Length == 0)
                    {
                        bContinue = false;
                        continue;
                    }

                    OrderItemAttributeSalesforce attr = new OrderItemAttributeSalesforce();

                    for (int i = 0; i < qr.records.Length; i++)
                    {
                        apex.sObject con = qr.records[i];
                        try
                        {
                            foreach (PropertyInfo prop in typeof(ItemAttribute).GetProperties())
                            {
                                foreach (DisplayAttribute idAttr in ((DisplayAttribute[])(prop.GetCustomAttributes(typeof(DisplayAttribute), true))))
                                {
                                    if (idAttr.Name.Equals(con.Any[1].InnerText))
                                    {
                                        string valor = con.Any[2].InnerText;
                                        var valorConvertido = string.IsNullOrWhiteSpace(valor) ? " " : Convert.ChangeType(valor, prop.PropertyType);
                                        prop.SetValue(attr, valorConvertido);
                                        continue;
                                    }
                                }
                            }
                        }
                        catch (Exception x)
                        {
                            //TODO:  logar ou tratar como exceção mesmo, por nao ser possivel ter +1 Attribute por Order e ser um caso de erro em UAT apenas
                            continue;
                        }
                        if (qr.records.Length == 1)
                        {
                            bContinue = false;
                            continue;
                        }

                    }
                    lstResult.Add(attr);

                    //handle the loop + 1 problem by checking to see if the most recent queryResult
                    if (qr.done)
                    {
                        bContinue = false;
                        //Console.WriteLine("Registrou: " + loopCounter);
                    }
                    else
                        qr = binding.queryMore(qr.queryLocator);
                }

                return lstResult;
            }
            catch (Exception ex)
            {
                Console.WriteLine("\nProblema para executar a query: \n" + ex.Message);
                Console.Write("\nClique para continuar...");
                Debugger.Break();
                Console.ReadLine();
            }

            return null;
        }
        


        public static List<OrderItemSalesforce> GetOrderItemsAttributePorOrderId(string orderId, string nomeItemAtributo, ref SforceService binding)
        {
            //ATENCAO ATENCAO ATENCAO ATENCAO ATENCAO ATENCAO
            SFDCSchemeBuild schema = new SFDCSchemeBuild();
            QueryResult qr = null;
            binding.QueryOptionsValue = new apex.QueryOptions();
            binding.QueryOptionsValue.batchSize = 3;
            binding.QueryOptionsValue.batchSizeSpecified = true;

            try
            {
                String sql = string.Format(@"SELECT Id,name,NE__Value__c,NE__Old_Value__c 
                                               FROM NE__Order_Item_Attribute__c
                                              where NE__Order_Item__r.NE__OrderId__r.Id = '{0}'"
                    , orderId);

                bool bContinue = false;
                try
                {
                    qr = binding.query(sql);
                    bContinue = true;
                }
                catch (Exception ex)
                {
                    throw ex;
                }

                //cliente = string.Empty;
                //Dictionary<string, string> lstResult = new Dictionary<string, string>();
                List<OrderItemSalesforce> result = new List<OrderItemSalesforce>();
                
                while (bContinue)
                {
                    if (qr.records == null || qr.records.Length == 0)
                    {
                        bContinue = false;
                        continue;
                    }

                    for (int i = 0; i < qr.records.Length; i++)
                    {
                        apex.sObject con = qr.records[i];
                        try
                        {
                            if(nomeItemAtributo.Equals(con.Any[1].InnerText))
                            {
                                OrderItemSalesforce oi = new OrderItemSalesforce();
                                oi.Id = con.Id;
                                oi.OrderId = orderId;
                                result.Add(oi);
                            }
                        }
                        catch (Exception x)
                        {
                            //TODO:  logar ou tratar como exceção mesmo, por nao ser possivel ter +1 Attribute por Order e ser um caso de erro em UAT apenas
                            continue;
                        }
                        if (qr.records.Length == 1)
                        {
                            bContinue = false;
                            continue;
                        }
                    }

                    //handle the loop + 1 problem by checking to see if the most recent queryResult
                    if (qr.done)
                    {
                        bContinue = false;
                        //Console.WriteLine("Registrou: " + loopCounter);
                    }
                    else
                        qr = binding.queryMore(qr.queryLocator);
                }

                return result;
            }
            catch (Exception ex)
            {
                Console.WriteLine("\nProblema para executar a query: \n" + ex.Message);
                Console.Write("\nClique para continuar...");
                Debugger.Break();
                Console.ReadLine();
            }

            return null;
        }


        public static List<AssetDTO> GetAssetsOrderId(string orderId, string nomeItemAtributo, ref SforceService binding)
        {
            return new List<AssetDTO>();
            AssetDTO ob = new AssetDTO();
            //ATENCAO ATENCAO ATENCAO ATENCAO ATENCAO ATENCAO
            SFDCSchemeBuild schema = new SFDCSchemeBuild();
            QueryResult qr = null;
            binding.QueryOptionsValue = new apex.QueryOptions();
            binding.QueryOptionsValue.batchSize = 3;
            binding.QueryOptionsValue.batchSizeSpecified = true;

            try
            {
                String sql = string.Format(@"SELECT Id,name,NE__Value__c,NE__Old_Value__c 
                                               FROM NE__Order_Item_Attribute__c
                                              where NE__Order_Item__r.NE__OrderId__r.Id = '{0}'"
                    , orderId);

                bool bContinue = false;
                try
                {
                    qr = binding.query(sql);
                    bContinue = true;
                }
                catch (Exception ex)
                {
                    throw ex;
                }

                //cliente = string.Empty;
                //Dictionary<string, string> lstResult = new Dictionary<string, string>();
                List<OrderItemSalesforce> result = new List<OrderItemSalesforce>();

                while (bContinue)
                {
                    if (qr.records == null || qr.records.Length == 0)
                    {
                        bContinue = false;
                        continue;
                    }

                    for (int i = 0; i < qr.records.Length; i++)
                    {
                        apex.sObject con = qr.records[i];
                        try
                        {
                            if (nomeItemAtributo.Equals(con.Any[1].InnerText))
                            {
                                OrderItemSalesforce oi = new OrderItemSalesforce();
                                oi.Id = con.Id;
                                oi.OrderId = orderId;
                                result.Add(oi);
                            }
                        }
                        catch (Exception x)
                        {
                            //TODO:  logar ou tratar como exceção mesmo, por nao ser possivel ter +1 Attribute por Order e ser um caso de erro em UAT apenas
                            continue;
                        }
                        if (qr.records.Length == 1)
                        {
                            bContinue = false;
                            continue;
                        }
                    }

                    //handle the loop + 1 problem by checking to see if the most recent queryResult
                    if (qr.done)
                    {
                        bContinue = false;
                        //Console.WriteLine("Registrou: " + loopCounter);
                    }
                    else
                        qr = binding.queryMore(qr.queryLocator);
                }

                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine("\nProblema para executar a query: \n" + ex.Message);
                Console.Write("\nClique para continuar...");
                Debugger.Break();
                Console.ReadLine();
            }

            return null;
        }


        /// <summary>
        /// Apaga o relacionamento de contato de uma determinada Conta.  Normalmente para permitir o Merge de Accounts, cujo Contato esteja relacionado, impedindo o merge.
        /// </summary>
        /// <param name="accountIds"></param>
        /// <returns></returns>
        public static bool? DeleteContatoRelacionadoByAccontId(List<string> accountIds, ref SforceService binding)
        {
            List<DeleteResult> result = new List<DeleteResult>();


            List<string> contatosRelacionados = GetRelatedContactByAccountId(accountIds, ref binding);
            if (contatosRelacionados.Count == 0)
                return null;
            //throw new Exception(string.Format("Nenhum Contato Relacionado Indireto para os Accounts {0}", string.Join(",", accountIds.ToArray())));

            DeleteResult[] saveResults = binding.delete( contatosRelacionados.ToArray().Take(200).ToArray() );

            /*foreach (string id in contatosRelacionados)
            {
                saveResults = binding.delete(new string[] { id });
                if (saveResults != null)
                {
                    //if (saveResults[0].errors != null && saveResults[0].errors.Count() > 0)
                        //throw new Exception(saveResults[0].errors[0].message);

                    result.AddRange(saveResults.ToList());
                }
            }*/

            if(result != null && result.Count > 0)
            {
                //if(result.Where(r => r.success).Count() == 0)
                //    throw new Exception(result[0].errors[0].message);
            }

            //if(result != null && result.Count > 0 && result[0].errors != null && result[0].errors.Count() > 0)
            //{
            //    throw new Exception(result[0].errors[0].message);
            //}
            return true;
        }


        /// <summary>
        /// Ingressa dados do sObject.
        /// </summary>
        /// <returns></returns>
        public static SaveResult[] InserirObjetosSF(List<sObject> lstObjetos, ref SforceService binding)
        {
            try
            {
                return binding.create(lstObjetos.ToArray());
            }
            catch (Exception ex)
            {
                Console.WriteLine("\nErro ao atualizar o registro: \n" + ex.Message);
                SaveResult err = new SaveResult();
                
                Error err1 = new Error();
                err1.message = string.Concat(ex.Message, ex.InnerException != null ? ex.InnerException.Message : string.Empty);
                Error[] erros = new Error[] { err1 };
                err.errors = erros;

                return new SaveResult[] { err };
            }
        }


        public static UpsertResult[] Upsert(string externalIdFieldName, List<sObject> lstObjetos, ref SforceService binding)
        {
            try
            {
                return binding.upsert(externalIdFieldName, lstObjetos.ToArray());
            }
            catch (Exception ex)
            {
                Console.WriteLine("\nErro ao atualizar o registro: \n" + ex.Message);
                UpsertResult err = new UpsertResult();

                Error err1 = new Error();
                err1.message = string.Concat(ex.Message, ex.InnerException != null ? ex.InnerException.Message : string.Empty);
                Error[] erros = new Error[] { err1 };
                err.errors = erros;

                return new UpsertResult[] { err };
            }
        }


        public static SaveResult[] Atualizar(List<sObject> lstObjetos, int agruparQtde, ref SforceService binding)
        {
            List<SaveResult> result = new List<SaveResult>();
            List<sObject> updMassivo = new List<sObject>();
            int s = 0;
            foreach (sObject sfobj in lstObjetos)
            {
                if (updMassivo.Count > 0 && s >= agruparQtde)
                {
                    SaveResult[] saveResults = binding.update(updMassivo.ToArray());
                    if(saveResults != null)
                        result.AddRange(saveResults.ToList());
                    updMassivo.Clear();
                    s = 0;
                }

                updMassivo.Add(sfobj);
                s += sfobj.Any.Count() ;
            }

            if (updMassivo.Count > 0)
            {
                SaveResult[] saveResults = binding.update(updMassivo.ToArray());
                if (saveResults != null)
                    result.AddRange(saveResults.ToList());
            }

            return result.ToArray();
        }



        /// <summary>
        /// Merge de Accounts
        /// </summary>
        /// <param name="accountIdMaster">Id da Account principal que receberá os objetos das demais Accounts.</param>
        /// <param name="ids">Lista de Id das Accounts cujos objetos serão transferidos pasra a Account master.</param>
        /// <param name="binding">Binding atual.</param>
        /// <returns></returns>
        public static MergeResult[] MergeContas(string accountIdMaster, List<string> ids, ref SforceService binding)
        {
            Console.Write("Merge account master: " + accountIdMaster);
            //binding.mergeCompleted += binding_mergeCompleted;
            MergeRequest mr = new MergeRequest();
            List<MergeResult> lstResult = new  List<MergeResult>();

            List<string> idsTemp = new List<string>();

            sObject sobj = new sObject();
            sobj.type = "Account";
            sobj.Id = accountIdMaster;
            mr.masterRecord = sobj;

            //TODO: tratar exceção quando existem contatos relacionados nas contas a mergear :: remove-los e reexecutar novamente o Merge
            foreach (string id in ids)
            {
                idsTemp.Add(id);

                if(idsTemp.Count == 2)
                {
                    mr.recordToMergeIds = idsTemp.ToArray();
                    try
                    {
                        lstResult.AddRange(
                            binding.merge(new MergeRequest[] { mr }).ToList()
                            );

                        idsTemp.Clear();
                    }
                    catch
                    {
                        //ERRO DE XML indica possivel problema por Contatos Relacionados.  
                        //Apagar os contatos comuns entre as contas
                        try
                        {
                            List<string> accIds = new List<string>();
                            accIds.AddRange(idsTemp);
                            accIds.Add(accountIdMaster);

                            if (DeleteContatoRelacionadoByAccontId(accIds, ref binding) != null)
                                lstResult.AddRange(binding.merge(new MergeRequest[] { mr }).ToList());

                            //return MergeContas(accountIdMaster, ids, ref binding);
                            else
                                throw new Exception();
                        }
                        catch (Exception ex)
                        {
                            bool recursive = true;
                            int tentativas = 0;

                            //ultima tentativa de merge, escolhendo outra conta master
                            while (recursive)
                            {
                                foreach (string idtemp in idsTemp)
                                {
                                    List<string> accIds = new List<string>();
                                    accIds.AddRange(ids);
                                    accIds.Add(accountIdMaster);
                                    accountIdMaster = idtemp;
                                    accIds.Remove(idtemp);

                                    try
                                    {
                                        return MergeContas(accountIdMaster, accIds, ref binding);
                                    }
                                    catch
                                    {
                                        if (tentativas == idsTemp.Count)
                                            recursive = false;
                                    }
                                }
                            }
                            idsTemp.Clear();
                            lstResult.Add(new MergeResult() { errors = new Error[] { new Error() { message = ex.Message } } });
                        }
                    }
                }
            }

            if (idsTemp.Count > 0)
            {
                mr.recordToMergeIds = idsTemp.ToArray();
                try 
                { 
                lstResult.AddRange(
                    binding.merge(new List<MergeRequest> { mr }.ToArray()).ToList()
                    );
                }
                catch
                {
                    //ERRO DE XML indica possivel problema por Contatos Relacionados.  
                    //Apagar os contatos comuns entre as contas
                    try
                    {
                        List<string> accIds = new List<string>();
                        accIds.AddRange(idsTemp);
                        accIds.Add(accountIdMaster);

                        if (DeleteContatoRelacionadoByAccontId(accIds, ref binding) != null)
                            lstResult.AddRange(                    binding.merge(new List<MergeRequest> { mr }.ToArray()).ToList()                    ); //return MergeContas(accountIdMaster, ids, ref binding);
                        else
                            throw new Exception("nenhum contato relacionado encontrado ou erro ao apagar");
                    }
                    catch (Exception ex)
                    {
                        string msg = string.Empty;

                        bool recursive = true;
                        int tentativas = 0;

                        if (idsTemp.Count == 1)
                        {
                            recursive = false;
                            tentativas++;
                            msg = "Todas as tentativas de merge sem sucesso.";
                        }

                        //ultima tentativa de merge, escolhendo outra conta master
                        while (recursive)
                        {
                            foreach (string idtemp in idsTemp)
                            {
                                List<string> accIds = new List<string>();
                                accIds.AddRange(ids);
                                accIds.Add(accountIdMaster);
                                accountIdMaster = idtemp;
                                accIds.Remove(idtemp);

                                try
                                {
                                    return MergeContas(accountIdMaster, accIds, ref binding);
                                }
                                catch
                                {
                                    if (tentativas == idsTemp.Count)
                                        recursive = false;
                                }
                            }

                            msg = ex.Message;
                        }
                        idsTemp.Clear();
                        lstResult.Add(new MergeResult() { errors = new Error[] { new Error() { message = msg } } });
                    }
                }

            }
            Console.WriteLine(" - " + lstResult.LastOrDefault().success.ToString());
            return lstResult.ToArray();
        }



        public static UpsertResult[] Upsert(string campoExternalId, List<sObject> lstObjetos, int agruparQtde, ref SforceService binding)
        {
            UpsertResult[] saveResults;
            List<UpsertResult> result = new List<UpsertResult>();
            List<sObject> updMassivo = new List<sObject>();
            int s = 0;
            foreach (sObject sfobj in lstObjetos)
            {
                if (updMassivo.Count > 0 && s >= agruparQtde)
                {
                    saveResults = binding.upsert(campoExternalId, updMassivo.ToArray());
                    if (saveResults != null)
                        result.AddRange(saveResults.ToList());
                    updMassivo.Clear();
                    s = 0;
                }

                updMassivo.Add(sfobj);
                s += sfobj.Any.Count();
            }

            if (updMassivo.Count > 0)
            {
                try
                {
                    saveResults = binding.upsert(campoExternalId, updMassivo.ToArray());
                    if (saveResults != null)
                        result.AddRange(saveResults.ToList());
                }
                catch(Exception ex)
                {
                    Error e = new Error();
                    e.message = ex.Message;
                    e.statusCode = StatusCode.UNKNOWN_EXCEPTION;
                    UpsertResult r = new UpsertResult();
                    r.errors = new Error[] { e };
                    result.Add(r);
                }
            }

            return result.ToArray();
        }


        #region Internal Métodos

        internal static List<BillingSalesforce> GetPerfisFaturamentoByAccoundId(string codigoEmpresa, string accountId, ref SforceService binding)
        {
            SFDCSchemeBuild schema = new SFDCSchemeBuild();
            string ufEmpresa = codigoEmpresa;

            List<BillingSalesforce> result = new List<BillingSalesforce>();

            QueryResult qr = null;
            binding.QueryOptionsValue = new SalesforceExtractor.apex.QueryOptions();
            binding.QueryOptionsValue.batchSize = 3;
            binding.QueryOptionsValue.batchSizeSpecified = true;

            try
            {
                String sql = string.Format(@"SELECT Id, Account__c, AccountContract__c, ExternalID__c, Company__c
                    FROM Billing_Profile__c  WHERE PointofDelivery__r.CompanyID__c = '{0}' AND Account__c = '{1}'"
                    , codigoEmpresa
                    , accountId);

                bool bContinue = false;

                try
                {
                    qr = binding.query(sql);

                    bContinue = true;
                }
                catch { }

                while (bContinue)
                {
                    if (qr.records == null)
                    {
                        bContinue = false;
                        continue;
                    }

                    for (int i = 0; i < qr.records.Length; i++)
                        {
                        SalesforceExtractor.apex.sObject con = qr.records[i];
                        BillingSalesforce obj = new BillingSalesforce();

                        obj.Id = schema.getFieldValue("Id", con.Any);
                        obj.AccountSF = schema.getFieldValue("Account__c", con.Any);
                        obj.AccountContract__c = schema.getFieldValue("AccountContract__c", con.Any);
                        obj.ExternalID__c = schema.getFieldValue("ExternalID__c", con.Any);
                        obj.Company__c = schema.getFieldValue("Company__c", con.Any);

                        result.Add(obj);

                            if (qr.done)
                            {
                                bContinue = false;
                                //Console.WriteLine("Registrou: " + loopCounter);
                            }
                            else
                                qr = binding.queryMore(qr.queryLocator);
                    }
                }

                return result;
            }
            catch (Exception ex)
            {
                Console.WriteLine(string.Format("\nProblema para executar a query: {0}\n", ex.Message));
                throw new Exception(ex.Message);
            }

            return null;
        }


        internal static List<CaseSalesforce> GetCasesByAccountId(string codigoEmpresa, string accountId, ref SforceService binding)
        {
            SFDCSchemeBuild schema = new SFDCSchemeBuild();
            string ufEmpresa = codigoEmpresa;

            List<CaseSalesforce> result = new List<CaseSalesforce>();

            QueryResult qr = null;
            binding.QueryOptionsValue = new SalesforceExtractor.apex.QueryOptions();
            binding.QueryOptionsValue.batchSize = 3;
            binding.QueryOptionsValue.batchSizeSpecified = true;

            try
            {
                String sql = string.Format(@"SELECT AccountId, Id, ClosedDate, IsClosed
                                               FROM Case  
                                              WHERE PointofDelivery__r.CompanyID__c = '{0}' AND AccountId = '{1}'"
                    , codigoEmpresa
                    , accountId);

                bool bContinue = false;

                try
                {
                    qr = binding.query(sql);

                    bContinue = true;
                }
                catch { }

                while (bContinue)
                {
                    if (qr.records == null)
                    {
                        bContinue = false;
                        continue;
                    }

                    for (int i = 0; i < qr.records.Length; i++)
                    {
                        SalesforceExtractor.apex.sObject con = qr.records[i];
                        CaseSalesforce obj = new CaseSalesforce();

                        obj.Id = schema.getFieldValue("Id", con.Any);
                        obj.AccountId = schema.getFieldValue("AccountId", con.Any);
                        obj.ClosedDate = schema.getFieldValue("ClosedDate", con.Any);
                        obj.IsClosed = Convert.ToBoolean(schema.getFieldValue("IsClosed", con.Any));

                        result.Add(obj);

                        if (qr.done)
                        {
                            bContinue = false;
                        }
                        else
                            qr = binding.queryMore(qr.queryLocator);
                    }
                }

                return result;
            }
            catch (Exception ex)
            {
                Console.WriteLine(string.Format("\nProblema para executar a query: {0}\n", ex.Message));
                throw new Exception(ex.Message);
            }
        }


        internal static List<CaseSalesforce> GetCaso(string codigoEmpresa, string Id, ref SforceService binding)
        {
            SFDCSchemeBuild schema = new SFDCSchemeBuild();
            string ufEmpresa = codigoEmpresa;

            List<CaseSalesforce> result = new List<CaseSalesforce>();

            QueryResult qr = null;
            binding.QueryOptionsValue = new SalesforceExtractor.apex.QueryOptions();
            binding.QueryOptionsValue.batchSize = 3;
            binding.QueryOptionsValue.batchSizeSpecified = true;

            try
            {
                String sql = string.Format(@"SELECT AccountId, Id, ClosedDate, IsClosed
                                               FROM Case  
                                              WHERE PointofDelivery__r.CompanyID__c = '{0}' AND Id = '{1}'"
                    , codigoEmpresa
                    , Id);

                bool bContinue = false;

                try
                {
                    qr = binding.query(sql);

                    bContinue = true;
                }
                catch { }

                while (bContinue)
                {
                    if (qr.records == null)
                    {
                        bContinue = false;
                        continue;
                    }

                    for (int i = 0; i < qr.records.Length; i++)
                    {
                        SalesforceExtractor.apex.sObject con = qr.records[i];
                        CaseSalesforce obj = new CaseSalesforce();

                        obj.Id = schema.getFieldValue("Id", con.Any);
                        obj.AccountId = schema.getFieldValue("AccountId", con.Any);
                        obj.ClosedDate = schema.getFieldValue("ClosedDate", con.Any);
                        obj.IsClosed = Convert.ToBoolean(schema.getFieldValue("IsClosed", con.Any));

                        result.Add(obj);

                        if (qr.done)
                        {
                            bContinue = false;
                        }
                        else
                            qr = binding.queryMore(qr.queryLocator);
                    }
                }

                return result;
            }
            catch (Exception ex)
            {
                Console.WriteLine(string.Format("\nProblema para executar a query: {0}\n", ex.Message));
                throw new Exception(ex.Message);
            }
        }

        internal static CaseSalesforce GetCasoByNumero(string numeroCaso, ref SforceService binding)
        {
            SFDCSchemeBuild schema = new SFDCSchemeBuild();
            List<CaseSalesforce> result = new List<CaseSalesforce>();

            QueryResult qr = null;
            binding.QueryOptionsValue = new SalesforceExtractor.apex.QueryOptions();
            binding.QueryOptionsValue.batchSize = 3;
            binding.QueryOptionsValue.batchSizeSpecified = true;

            try
            {
                String sql = string.Format(@"SELECT AccountId, Id, ClosedDate, IsClosed, InserviceNumber__c, CaseNumber, APCaseNumber__c
                                               FROM Case  
                                              WHERE Country__c = 'BRASIL'
                                                and CaseNumber = '{0}'"
                    , numeroCaso);

                bool bContinue = false;

                try
                {
                    qr = binding.query(sql);

                    bContinue = true;
                }
                catch { }

                while (bContinue)
                {
                    if (qr.records == null)
                    {
                        bContinue = false;
                        continue;
                    }

                    for (int i = 0; i < qr.records.Length; i++)
                    {
                        SalesforceExtractor.apex.sObject con = qr.records[i];
                        CaseSalesforce obj = new CaseSalesforce();

                        obj.Id = schema.getFieldValue("Id", con.Any);
                        obj.AccountId = schema.getFieldValue("AccountId", con.Any);
                        obj.ClosedDate = schema.getFieldValue("ClosedDate", con.Any);
                        obj.IsClosed = Convert.ToBoolean(schema.getFieldValue("IsClosed", con.Any));
                        obj.NumeroAviso = schema.getFieldValue("InserviceNumber__c", con.Any);
                        obj.NumeroCaso = schema.getFieldValue("CaseNumber", con.Any);
                        obj.NumeroAvisoIluminacaoPublica = schema.getFieldValue("APCaseNumber__c", con.Any);

                        result.Add(obj);

                        if (qr.done)
                        {
                            bContinue = false;
                        }
                        else
                            qr = binding.queryMore(qr.queryLocator);
                    }
                }

                return result.FirstOrDefault();
            }
            catch (Exception ex)
            {
                Console.WriteLine(string.Format("\nProblema para executar a query: {0}\n", ex.Message));
                throw new Exception(ex.Message);
            }
        }



        /// <summary>
        /// 
        /// </summary>
        /// <param name="param"></param>
        /// <param name="binding"></param>
        /// <returns></returns>
        public static List<Atividade> GetAtividadesPorPeriodo(ParAtividadeSalesforce param, ref SforceService binding)
        {
            if(!dicBindings.ContainsKey(param.IdentificadorParametro))
                dicBindings.Add(param.IdentificadorParametro, binding);

            binding.QueryOptionsValue = new SalesforceExtractor.apex.QueryOptions();
            binding.QueryOptionsValue.batchSize = 3;
            binding.QueryOptionsValue.batchSizeSpecified = true;

            try
            {
                StringBuilder sql = new StringBuilder(string.Format(@"select ActivityDate, CreatedDate, 
	                                                Id,
	                                                RelatedCase__c,
	                                                Subject,
	                                                WhatId 
                                               from task WHERE "));
                
                StringBuilder where = new StringBuilder();

                if (!string.IsNullOrWhiteSpace(param.Id))
                    where.AppendFormat(" {0} Id = '{1}'", where.Length > 0 ? "AND" : string.Empty, param.Assunto.Trim());

                //if (!string.IsNullOrWhiteSpace(param.Assunto))
                where.AppendFormat(" {0} (Subject LIKE '%Aviso Emergencial%')", where.Length > 0 ? "AND" : string.Empty);    //, param.Assunto.Trim());

                if(!string.IsNullOrWhiteSpace(param.DataCriacaoInicioStr))
                    where.AppendFormat(" {0} createddate >= {1} ", where.Length > 0 ? "AND" : string.Empty, param.DataCriacaoInicioStr.Trim());

                if(!string.IsNullOrWhiteSpace(param.DataCriacaoFimStr))
                    where.AppendFormat(" {0} createddate <= {1} ", where.Length > 0 ? "AND" : string.Empty, param.DataCriacaoFimStr.Trim());

                if(!string.IsNullOrWhiteSpace(param.Pais))
                    where.AppendFormat(" {0} CreatedBy.Country__c = '{1}' ", where.Length > 0 ? "AND" : string.Empty, param.Pais.Trim());

                if (!string.IsNullOrEmpty(param.CasoRelacionado) && !"null".Equals(param.CasoRelacionado.Trim().ToLower()))
                    where.AppendFormat(" {0} relatedcase__c = '{1}'", where.Length > 0 ? "AND" : string.Empty, param.CasoRelacionado);

                if (param.CasoRelacionado == null || "null".Equals(param.CasoRelacionado.ToLower()))
                    where.AppendFormat(" {0} relatedcase__c = null", where.Length > 0 ? "AND" : string.Empty);

                if (where.Length == 0)
                    sql.Replace(" WHERE", string.Empty);

                sql.Append(where.ToString());

                try
                {
                    binding.Timeout = int.MaxValue;
                    binding.queryCompleted += binding_queryCompleted;
                    Console.WriteLine(DateTime.Now.ToLongTimeString() + " Consulta iniciada. " + sql.ToString());
                    binding.queryAsync(sql.ToString(), param);

                    while (!_queryCompleta)
                    {
                        Thread.Sleep(10000);
                    }
                }
                catch
                {}

                return _resultAtividades;
            }
            catch (Exception ex)
            {
                Console.WriteLine(string.Format("\nProblema para executar a query: {0}\n", ex.Message));
                throw new Exception(ex.Message);
            }
        }

        static void binding_queryCompleted(object sender, queryCompletedEventArgs args)
        {
            ParAtividadeSalesforce param = ((ParAtividadeSalesforce)args.UserState);
            if (!param.IsAlive)
                return;

            if (param.IsAlive && args.Error != null)
            {
                if (!string.IsNullOrWhiteSpace(args.Error.Message) && args.Error.Message.Contains("TIMEOUT"))
                {
                    SforceService novoBinding = dicBindings[param.IdentificadorParametro];

                    param.DataCriacaoFim = param.DataCriacaoFim.Value.AddDays(-1) < param.DataCriacaoInicio.Value ? 
                        param.DataCriacaoInicio.Value  : param.DataCriacaoFim.Value.AddDays(-1);

                    if (param.IsAlive && param.DataCriacaoFim.Value < param.DataCriacaoInicio.Value)
                        throw args.Error;

                    if (param.IsAlive)
                        GetAtividadesPorPeriodo(param, ref novoBinding);
                }
                if(param.IsAlive)
                    throw args.Error;
            }

            if (!param.IsAlive)
                return;

            param.IsAlive = false;
            Console.WriteLine(DateTime.Now.ToLongTimeString() + " Extração iniciada.");

            QueryResult qr = args.Result;
            if (param.IsAlive)
                return;

            if (qr != null && qr.records == null)
            {
                _queryCompleta = true;
                return;
            }

            _resultAtividades.AddRange(sObjectToAtividadeList(args.Result.records));

            while (!qr.done)
            {
                if (qr.done)
                    _queryCompleta = true;
                else
                {
                    qr = ((SforceService)sender).queryMore(qr.queryLocator);
                    if (param.IsAlive)
                        return;

                    if (qr != null && qr.records == null)
                    {
                        _queryCompleta = true;
                        return;
                    }
                    _resultAtividades.AddRange(sObjectToAtividadeList(qr.records));
                }
            }
            _queryCompleta = true;
        }

        internal static List<Atividade> sObjectToAtividadeList(sObject[] records)
        {
            List<Atividade> result = new List<Atividade>();
            SFDCSchemeBuild schema = new SFDCSchemeBuild();
            for (int i = 0; i < records.Length; i++)
            {
                SalesforceExtractor.apex.sObject con = records[i];
                Atividade obj = new Atividade();

                obj.Id = schema.getFieldValue("Id", con.Any);
                obj.DataCriacao = schema.getFieldValue("CreatedDate", con.Any);
                obj.CasoRelacionado = schema.getFieldValue("RelatedCase__c", con.Any);
                obj.Assunto = schema.getFieldValue("Subject", con.Any);
                obj.CasoId = schema.getFieldValue("WhatId", con.Any);

                if (obj.Assunto.ToLower().Contains("aviso emergencial"))
                {
                    result.Add(obj);
                    Console.WriteLine(string.Format("Task: {0} Caso: {1} Data: {2} Assunto: {3}", obj.Id, obj.CasoId, obj.DataCriacao, obj.Assunto));
                }
            }

            return result;
        }


        internal static List<CaseSalesforce> GetCasosByNumeros(List<string> numerosCaso, ref SforceService binding)
        {
            if (numerosCaso.Count <= 0)
                return new List<CaseSalesforce>();

            SFDCSchemeBuild schema = new SFDCSchemeBuild();
            List<CaseSalesforce> result = new List<CaseSalesforce>();

            QueryResult qr = null;
            binding.QueryOptionsValue = new SalesforceExtractor.apex.QueryOptions();
            binding.QueryOptionsValue.batchSize = 3;
            binding.QueryOptionsValue.batchSizeSpecified = true;

            try
            {
                String sql = string.Format(@"SELECT AccountId, Id, ClosedDate, IsClosed, InserviceNumber__c, CaseNumber, APCaseNumber__c
                                               FROM Case  
                                              WHERE Country__c = 'BRASIL'
                                                and CaseNumber IN ({0})"
                    , string.Join(",", numerosCaso.ToArray()));

                bool bContinue = false;

                try
                {
                    qr = binding.query(sql);

                    bContinue = true;
                }
                catch { }

                while (bContinue)
                {
                    if (qr.records == null)
                    {
                        bContinue = false;
                        continue;
                    }

                    for (int i = 0; i < qr.records.Length; i++)
                    {
                        SalesforceExtractor.apex.sObject con = qr.records[i];
                        CaseSalesforce obj = new CaseSalesforce();

                        obj.Id = schema.getFieldValue("Id", con.Any);
                        obj.AccountId = schema.getFieldValue("AccountId", con.Any);
                        obj.ClosedDate = schema.getFieldValue("ClosedDate", con.Any);
                        obj.IsClosed = Convert.ToBoolean(schema.getFieldValue("IsClosed", con.Any));
                        obj.NumeroAviso = schema.getFieldValue("InserviceNumber__c", con.Any);
                        obj.NumeroCaso = schema.getFieldValue("CaseNumber", con.Any);
                        obj.NumeroAvisoIluminacaoPublica = schema.getFieldValue("APCaseNumber__c", con.Any);

                        result.Add(obj);

                        if (qr.done)
                        {
                            bContinue = false;
                        }
                        else
                            qr = binding.queryMore(qr.queryLocator);
                    }
                }

                return result;
            }
            catch (Exception ex)
            {
                Console.WriteLine(string.Format("\nProblema para executar a query: {0}\n", ex.Message));
                throw new Exception(ex.Message);
            }
        }


        public static List<CaseSalesforce> GetCasosById(string nomeEmpresaIn, List<string> lstIdCasos, ref SforceService binding)
        {
            String sql2 = string.Format(@"select    Id 
                                                    , CaseNumber
                                                    , PointofDelivery__r.Name
                                                    , Status, Status__c
                                                    , CNT_ProcessStatus__c
                                                    , CreatedDate
                                                    , Reason
                                            from    Case");

            CondicaoSimplesSql condicao1 = new CondicaoSimplesSql();
            condicao1.Campo = "Id";
            condicao1.Fixo = false;
            condicao1.Valor = lstIdCasos.Select(x => string.Concat("'", x.Replace("'", ""), "'")).ToList();

            CondicaoSimplesSql condicao2 = new CondicaoSimplesSql();
            condicao2.Campo = "Company__c";
            condicao2.Fixo = true;
            condicao2.Valor = nomeEmpresaIn.Split(new char[] { ',' }).ToList();

            List<CondicaoSql> condicoes = new List<CondicaoSql>();
            condicoes.Add(condicao1);
            condicoes.Add(condicao2);


            List<sObject> temp = ConsultarS(sql2, condicoes, ref binding);

            List<CaseSalesforce> resultado = EntidadeConversor.ToCases(temp);
            return resultado;
        }

        #endregion



        /// <summary>
        /// 
        /// </summary>
        /// <param name="codigoEmpresaIn">Codigo de empresas, com aspas simples</param>
        /// <param name="numeroCliente"></param>
        /// <param name="binding"></param>
        /// <returns></returns>
        internal static ContractLineItemSalesforce GetContractLineByNumeroCliente(string codigoEmpresaIn, string numeroCliente, ref SforceService binding)
        {
            return GetContractLinesByNumeroCliente(codigoEmpresaIn, numeroCliente, ref binding).FirstOrDefault();
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="codigoEmpresaIn">Codigo de empresas, com aspas simples</param>
        /// <param name="numeroCliente"></param>
        /// <param name="binding"></param>
        /// <returns></returns>
        public static List<ContractLineItemSalesforce> GetContractLinesByNumeroCliente(string codigoEmpresaIn, string numeroCliente, ref SforceService binding)
        {
            SFDCSchemeBuild schema = new SFDCSchemeBuild();
            List<ContractLineItemSalesforce> result = new List<ContractLineItemSalesforce>();

            QueryResult qr = null;
            binding.QueryOptionsValue = new SalesforceExtractor.apex.QueryOptions();
            binding.QueryOptionsValue.batchSize = 3;
            binding.QueryOptionsValue.batchSizeSpecified = true;

            try
            {
                String sql = string.Format(@"select   Id
                                                    , Asset__r.Name
                                                    , Asset__r.AccountId
                                                    , Asset__r.ExternalId__c
                                                    , Asset__c
                                                    , Asset__r.PointOfDelivery__c
                                                    , Asset__r.PointOfDelivery__r.Name
                                                    , Contract__c
                                                    , Contract__r.CNT_Quote__c
                                                    , Contract__r.Status
                                                    , Asset__r.NE__Order_Config__c
                                                    , Contract__r.Contract_Type__c
                                                    , Asset__r.Account.ExternalId__c
                                                    , Asset__r.Contact.Name
                                               from Contract_Line_Item__c 
                                              where Asset__r.PointofDelivery__r.CompanyID__c  in ({0})
                                                    and Asset__r.PointofDelivery__r.Country__c = 'BRASIL'
                                                    and Asset__r.PointOfDelivery__r.Name = '{1}'"
                           , codigoEmpresaIn
                           , numeroCliente
                           );

                bool bContinue = true;
                const int maxTentativas = 30;
                int tentativa = 0;

                while (bContinue)
                {
                    bContinue = false;
                    try
                    {
                        qr = binding.query(sql);
                        bContinue = true;
                    }
                    catch (System.Net.WebException ex)
                    {
                        tentativa++;
                        if (tentativa == maxTentativas)
                            throw new System.Net.WebException(string.Format("Após {0} tentativas, não foi possível conectar-se o Salesforce: {1}", maxTentativas, sql));

                        bContinue = true;
                        Thread.Sleep(60 * 1000);
                        continue;
                    }
                    catch (Exception ex)
                    {
                        throw ex;
                    }

                    if (qr.records == null)
                    {
                        bContinue = false;
                        continue;
                    }

                    if (qr.records.Length > 0)
                    {
                        for (int i = 0; i < qr.records.Length; i++)
                        {
                            SalesforceExtractor.apex.sObject con = qr.records[i];
                            ContractLineItemSalesforce obj = new ContractLineItemSalesforce();

                            obj.Id = schema.getFieldValue("Id", con.Any);
                            obj.AccountIdAsset = schema.getFieldValueMore("Asset__r", "", "", "AccountId", con.Any);
                            obj.AssetExternalId = schema.getFieldValueMore("Asset__r", "", "", "ExternalId__c", con.Any);
                            obj.AssetId = schema.getFieldValue("Asset__c", con.Any);
                            obj.AssetName = schema.getFieldValueMore("Asset__r", "", "", "Name", con.Any);
                            obj.PointOfDelivery = schema.getFieldValueMore("Asset__r", "", "", "PointOfDelivery__c", con.Any);
                            obj.NumeroCliente = schema.getFieldValueMore("Asset__r", "PointOfDelivery__r", "", "Name", con.Any);
                            obj.ContractId = schema.getFieldValue("Contract__c", con.Any);
                            obj.ContractQuote = schema.getFieldValueMore("Contract__r", "", "", "CNT_Quote__c", con.Any);
                            obj.ContractType = schema.getFieldValueMore("Contract__r", "", "", "Contract_Type__c", con.Any);
                            obj.CNT_Status__c = schema.getFieldValueMore("Contract__r", "", "", "Status", con.Any);
                            obj.OrderConfig = schema.getFieldValueMore("Asset__r", "", "", "NE__Order_Config__c", con.Any);
                            obj.AccountExternalId = schema.getFieldValueMore("Asset__r", "Account", "", "ExternalId__c", con.Any);
                            obj.ContactName = schema.getFieldValueMore("Asset__r", "Contact", "", "Name", con.Any);

                            result.Add(obj);

                            if (qr.done)
                            {
                                bContinue = false;
                                //Console.WriteLine("Registrou: " + loopCounter);
                            }
                            else
                                qr = binding.queryMore(qr.queryLocator);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(string.Format("\nProblema para executar a query: {0}\n", ex.Message));
                throw new Exception(ex.Message);
            }

            return result;
        }


        public static Dictionary<string,ContractLineItemSalesforce> GetContractLinesTrocaTitularidade(Dictionary<string, string> pares, ref SforceService binding)
        {
            SFDCSchemeBuild schema = new SFDCSchemeBuild();
            Dictionary<string, ContractLineItemSalesforce> result = new Dictionary<string,ContractLineItemSalesforce>();

            QueryResult qr = null;
            binding.QueryOptionsValue = new SalesforceExtractor.apex.QueryOptions();
            binding.QueryOptionsValue.batchSize = 3;
            binding.QueryOptionsValue.batchSizeSpecified = true;

            #region consulta sql
            String sql = string.Format(@"select   Id
                                                , Asset__c
                                                , Asset__r.Name
                                                , Asset__r.AccountId
                                                , Asset__r.ExternalId__c
                                                , Asset__r.PointOfDelivery__c
                                                , Asset__r.PointOfDelivery__r.Name
                                                , Asset__r.PointOfDelivery__r.DetailAddress__c
                                                , Asset__r.NE__Order_Config__c
                                                , Asset__r.Account.ExternalId__c
                                                , Asset__r.Contact.Name
                                                , Asset__r.NE__Status__c
                                                , Asset__r.Account.IdentityNumber__c
                                                , Asset__r.Account.Name
                                                , Asset__r.ContactId
                                                , Contract__c
                                                , Contract__r.CNT_Quote__c
                                                , Contract__r.Status
                                                , Contract__r.Contract_Type__c
                                                , Contract__r.StartDate
                                                , Contract__r.AccountId
                                                , Contract__r.ExternalId__c
                                                , Contract__r.ContractNumber
                                                , Contract__r.RecordType.Id
                                                , Contract__r.CNT_Economical_Activity__c
                                                , Billing_Profile__r.Id
                                                , Billing_Profile__r.Account__r.Name
                                                , Billing_Profile__r.Account__r.Id
                                                , Billing_Profile__r.PointofDelivery__r.Name
                                                , Billing_Profile__r.RecordTypeId
                                                , Billing_Profile__r.ExternalId__c
                                                , Billing_Profile__r.CNT_Due_Date__c
                                                , Billing_Profile__r.Address__c
                                                , Billing_Profile__r.BillingAddress__c
                                                , Billing_Profile__r.Type__c
                                            from Contract_Line_Item__c ");
            #endregion

            CondicaoMultiplaSql condicao1 = new CondicaoMultiplaSql();
            //TODO: pensar numa alternativa para passar o nome dos campos dinamicamente
            condicao1.Grupo = new Dictionary<string, List<string>>();
            condicao1.Grupo.Add("Asset__r.PointOfDelivery__r.Name", pares.ToList().Select(x => string.Format("'{0}'", x.Value)).ToList());
            //condicao1.Grupo.Add("Asset__r.Account.IdentityNumber__c", pares.ToList().Select(x => string.Format("'{0}'", x.Value)).ToList());

            CondicaoMultiplaSql condicao2 = new CondicaoMultiplaSql();
            condicao2.Fixo = true;
            condicao2.Grupo = new Dictionary<string, List<string>>();
            condicao2.Grupo.Add("Asset__r.PointofDelivery__r.CompanyID__c", new List<string> { "'2003'", "'COELCE'" });
            condicao2.Grupo.Add("Asset__r.PointofDelivery__r.Country__c", new List<string> { "'BRASIL'" });
            //condicao2.Grupo.Add("Asset__r.Name", new List<string> { "'Eletricity Service','Grupo B'" });
            //condicao2.Grupo.Add("Asset__r.NE__Status__c", new List<string> { "'Active','Activated'" });
            //condicao2.Grupo.Add("Contract__r.Contract_Type__c", new List<string> { "'B2B','B2C'" });

            //pares.ToList().ForEach(x => { ((Dictionary<string, string>)condicao1.Valores).Add(x.Key, x.Value); });
            List<CondicaoMultiplaSql> condicoes = new List<CondicaoMultiplaSql>();
            condicoes.Add(condicao1);
            condicoes.Add(condicao2);

            //TODO: alterar novo metodo que tem assinatura com o CondicaoMultiplaSql
            List<sObject> temp = ConsultarM(sql, condicoes, ref binding);

            List<ContractLineItemSalesforce> resultado = EntidadeConversor.ToContractLineItemList(temp);

            resultado.ForEach(r => Console.WriteLine(string.Format("Account Id {0} CLine Id {1} NumCli {2} Contract ExtId {4} Asset Id {5}", r.AccountIdAsset, r.Id, r.NumeroCliente, r.ContractId, r.ContractExternalId, r.AssetId)));
            
            foreach(string doc in pares.Keys)
            {
                ContractLineItemSalesforce ct = resultado.Where(c => c.NumeroCliente.Equals(pares[doc])).FirstOrDefault();
                if (ct == null || string.IsNullOrWhiteSpace(ct.Id))
                {
                    //TODO: LOG
                    continue;
                }
                result.Add(doc, ct);
            }

            return result;
        }


        public static List<ContractLineItemSalesforce> GetContractLinesTrocaTitularidadeByDocumento(string documento, ref SforceService binding)
        {
            SFDCSchemeBuild schema = new SFDCSchemeBuild();

            binding.QueryOptionsValue = new SalesforceExtractor.apex.QueryOptions();
            binding.QueryOptionsValue.batchSize = 3;
            binding.QueryOptionsValue.batchSizeSpecified = true;

            #region consulta sql
            String sql = string.Format(@"select   Id
                                                , Asset__c
                                                , Asset__r.Name
                                                , Asset__r.AccountId
                                                , Asset__r.ExternalId__c
                                                , Asset__r.PointOfDelivery__c
                                                , Asset__r.PointOfDelivery__r.Name
                                                , Asset__r.PointOfDelivery__r.DetailAddress__c
                                                , Asset__r.NE__Order_Config__c
                                                , Asset__r.Account.ExternalId__c
                                                , Asset__r.Contact.Name
                                                , Asset__r.NE__Status__c
                                                , Asset__r.Account.IdentityNumber__c
                                                , Asset__r.Account.Name
                                                , Asset__r.ContactId
                                                , Contract__c
                                                , Contract__r.CNT_Quote__c
                                                , Contract__r.Status
                                                , Contract__r.Contract_Type__c
                                                , Contract__r.StartDate
                                                , Contract__r.AccountId
                                                , Contract__r.ExternalId__c
                                                , Contract__r.ContractNumber
                                                , Contract__r.RecordType.Id
                                                , Contract__r.CNT_Economical_Activity__c
                                                , Billing_Profile__r.Id
                                                , Billing_Profile__r.Account__r.Name
                                                , Billing_Profile__r.Account__r.Id
                                                , Billing_Profile__r.PointofDelivery__r.Name
                                                , Billing_Profile__r.RecordTypeId
                                                , Billing_Profile__r.ExternalId__c
                                                , Billing_Profile__r.CNT_Due_Date__c
                                                , Billing_Profile__r.Address__c
                                                , Billing_Profile__r.BillingAddress__c
                                                , Billing_Profile__r.Type__c
                                            from Contract_Line_Item__c ");
            #endregion

            CondicaoMultiplaSql condicao1 = new CondicaoMultiplaSql();
            //TODO: pensar numa alternativa para passar o nome dos campos dinamicamente
            condicao1.Grupo = new Dictionary<string, List<string>>();
            //condicao1.Grupo.Add("Asset__r.PointOfDelivery__r.Name", pares.ToList().Select(x => string.Format("'{0}'", x.Value)).ToList());
            condicao1.Grupo.Add("Asset__r.Account.IdentityNumber__c", new List<string> { documento });

            CondicaoMultiplaSql condicao2 = new CondicaoMultiplaSql();
            condicao2.Fixo = true;
            condicao2.Grupo = new Dictionary<string, List<string>>();
            condicao2.Grupo.Add("Asset__r.PointofDelivery__r.CompanyID__c", new List<string> { "'2003'", "'COELCE'" });
            condicao2.Grupo.Add("Asset__r.PointofDelivery__r.Country__c", new List<string> { "'BRASIL'" });
            //condicao2.Grupo.Add("Asset__r.Name", new List<string> { "'Eletricity Service','Grupo B'" });
            //condicao2.Grupo.Add("Asset__r.NE__Status__c", new List<string> { "'Active','Activated'" });
            //condicao2.Grupo.Add("Contract__r.Contract_Type__c", new List<string> { "'B2B','B2C'" });

            //pares.ToList().ForEach(x => { ((Dictionary<string, string>)condicao1.Valores).Add(x.Key, x.Value); });
            List<CondicaoMultiplaSql> condicoes = new List<CondicaoMultiplaSql>();
            condicoes.Add(condicao1);
            condicoes.Add(condicao2);

            //TODO: alterar novo metodo que tem assinatura com o CondicaoMultiplaSql
            List<sObject> temp = ConsultarM(sql, condicoes, ref binding);

            List<ContractLineItemSalesforce> resultado = EntidadeConversor.ToContractLineItemList(temp);
            return resultado;
        }


        internal static ContractLineItemGoverno GetContractLineGovernoByNumeroCliente(string codigoEmpresa, string numeroCliente, ref SforceService binding)
        {
            SFDCSchemeBuild schema = new SFDCSchemeBuild();
            QueryResult qr = null;
            binding.QueryOptionsValue = new SalesforceExtractor.apex.QueryOptions();
            binding.QueryOptionsValue.batchSize = 3;
            binding.QueryOptionsValue.batchSizeSpecified = true;

            try
            {
                String sql = string.Format(@"select   Id
                                                    , Asset__r.Name
                                                    , Asset__r.AccountId
                                                    , Asset__r.ExternalId__c
                                                    , Asset__c
                                                    , Asset__r.PointOfDelivery__c
                                                    , Asset__r.PointOfDelivery__r.Rate__c
                                                    , Asset__r.PointOfDelivery__r.Name
                                                    , Asset__r.PointOfDelivery__r.DetailAddress__r.Street_type__c
                                                    , Asset__r.PointOfDelivery__r.DetailAddress__r.Name
                                                    , Asset__r.PointOfDelivery__r.DetailAddress__r.Municipality__c
                                                    , Asset__r.Account.Id                                                    
                                                    , Asset__r.Account.Name
                                                    , Asset__r.Account.IdentityNumber__c
                                                    , Asset__r.Account.IdentityType__c
                                                    , Asset__r.Account.ExternalId__c
                                                    , Asset__r.Account.CNT_Executive__r.Name
                                                    , Asset__r.Account.ParentId
                                                    , Contract__c
                                                    , Contract__r.CNT_Quote__c
                                                    , Asset__r.NE__Order_Config__c
                                                    , Contract__r.Contract_Type__c
                                                    , Contract__r.CNT_GroupSegment__c
                                                    , Contract__r.CNT_GroupArea__c
                                                    , Contract__r.ContractNumber 
                                                    , Asset__r.Contact.Name
                                               from Contract_Line_Item__c 
                                              where Asset__r.PointofDelivery__r.CompanyID__c  = '{0}'
                                                    and Asset__r.PointofDelivery__r.Country__c = 'BRASIL'
                                                    and Asset__r.PointOfDelivery__r.Name = '{1}'"
                           , codigoEmpresa
                           , numeroCliente
                           );

                bool bContinue = true;
                const int maxTentativas = 30;
                int tentativa = 0;

                while (bContinue)
                {
                    bContinue = false;
                    try
                    {
                        qr = binding.query(sql);
                        bContinue = true;
                    }
                    catch (System.Net.WebException ex)
                    {
                        tentativa++;
                        if (tentativa == maxTentativas)
                            throw new System.Net.WebException(string.Format("Após {0} tentativas, não foi possível conectar-se o Salesforce: {1}", maxTentativas, sql));

                        bContinue = true;
                        Thread.Sleep(60 * 1000);
                        break;
                    }
                    catch (Exception ex)
                    {
                        throw ex;
                    }

                    if (qr.records == null)
                    {
                        bContinue = false;
                        continue;
                    }

                    if (qr.records.Length > 0)
                    {
                        for (int i = 0; i < qr.records.Length; i++)
                        {
                            SalesforceExtractor.apex.sObject con = qr.records[i];
                            ContractLineItemGoverno obj = new ContractLineItemGoverno();

                            obj.Id = schema.getFieldValue("Id", con.Any);
                            obj.AccountIdAsset = schema.getFieldValueMore("Asset__r", "", "", "AccountId", con.Any);
                            obj.AssetExternalId = schema.getFieldValueMore("Asset__r", "", "", "ExternalId__c", con.Any);
                            obj.AssetId = schema.getFieldValue("Asset__c", con.Any);
                            obj.AssetName = schema.getFieldValueMore("Asset__r", "", "", "Name", con.Any);
                            obj.PointOfDelivery = schema.getFieldValueMore("Asset__r", "", "", "PointOfDelivery__c", con.Any);
                            obj.Tarifa = schema.getFieldValueMore("Asset__r", "PointOfDelivery__r", "", "Rate__c", con.Any);
                            obj.NumeroCliente = schema.getFieldValueMore("Asset__r", "PointOfDelivery__r", "", "Name", con.Any);
                            obj.ContractId = schema.getFieldValue("Contract__c", con.Any);
                            obj.Quote = schema.getFieldValueMore("Contract__r", "", "", "CNT_Quote__c", con.Any);
                            obj.OrderConfig = schema.getFieldValueMore("Asset__r", "", "", "NE__Order_Config__c", con.Any);
                            obj.ContractType = schema.getFieldValueMore("Contract__r", "", "", "Contract_Type__c", con.Any);
                            obj.AccountExternalId = schema.getFieldValueMore("Asset__r", "Account", "", "ExternalId__c", con.Any);
                            obj.ContactName = schema.getFieldValueMore("Asset__r", "Contact", "", "Name", con.Any);
                            obj.AccountId = schema.getFieldValueMore("Asset__r", "Account", "", "Id", con.Any);
                            obj.AccountParentId = schema.getFieldValueMore("Asset__r", "Account", "", "ParentId", con.Any);
                            obj.Executivo = schema.getFieldValueMore("Asset__r", "Account", "CNT_Executive__r", "Name", con.Any);
                            obj.TipoEnderecoPoD = schema.getFieldValueMore("Asset__r", "PointOfDelivery__r", "DetailAddress__r", "Street_type__c", con.Any);
                            obj.EnderecoPoD = schema.getFieldValueMore("Asset__r", "PointOfDelivery__r", "DetailAddress__r", "Name", con.Any);
                            obj.MunicipioPoD = schema.getFieldValueMore("Asset__r", "PointOfDelivery__r", "DetailAddress__r", "Municipality__c", con.Any);
                            obj.AssetAccountName = schema.getFieldValueMore("Asset__r", "Account", "", "Name", con.Any);
                            obj.Identidade = schema.getFieldValueMore("Asset__r", "Account", "", "IdentityNumber__c", con.Any);
                            obj.TipoIdentidade = schema.getFieldValueMore("Asset__r", "Account", "", "IdentityType__c", con.Any);
                            obj.Segmento = schema.getFieldValueMore("Contract__r", "", "", "CNT_GroupSegment__c", con.Any);
                            obj.Area = schema.getFieldValueMore("Contract__r", "", "", "CNT_GroupArea__c", con.Any);
                            obj.ContractNumber = schema.getFieldValueMore("Contract__r", "", "", "ContractNumber", con.Any);
                            return obj;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(string.Format("\nProblema para executar a query: {0}\n", ex.Message));
                throw new Exception(ex.Message);
            }

            return new ContractLineItemGoverno();
        }


        internal static List<ContractLineItemGoverno> GetContractLineGovernosByListaClientes(string codigoEmpresa, List<string> clientes, ref SforceService binding)
        {
            String sql = string.Format(@"select   Id
                                                , Asset__r.Name
                                                , Asset__r.AccountId
                                                , Asset__r.ExternalId__c
                                                , Asset__c
                                                , Asset__r.PointOfDelivery__c
                                                , Asset__r.PointOfDelivery__r.Rate__c
                                                , Asset__r.PointOfDelivery__r.Name
                                                , Asset__r.PointOfDelivery__r.DetailAddress__r.Street_type__c
                                                , Asset__r.PointOfDelivery__r.DetailAddress__r.Name
                                                , Asset__r.PointOfDelivery__r.DetailAddress__r.Municipality__c
                                                , Asset__r.Account.Id                                                    
                                                , Asset__r.Account.Name
                                                , Asset__r.Account.IdentityNumber__c
                                                , Asset__r.Account.IdentityType__c
                                                , Asset__r.Account.ExternalId__c
                                                , Asset__r.Account.CNT_Executive__r.Name
                                                , Contract__c
                                                , Contract__r.CNT_Quote__c
                                                , Asset__r.NE__Order_Config__c
                                                , Contract__r.Contract_Type__c
                                                , Contract__r.CNT_GroupSegment__c
                                                , Contract__r.CNT_GroupArea__c
                                                , Contract__r.ContractNumber 
                                                , Asset__r.Contact.Name
                                                , GroupAccountContract__r.AccountContract__c
                                                , GroupAccountContract__r.Account__c
                                                , GroupAccountContract__r.Account__r.ParentId
                                            from Contract_Line_Item__c ");

            CondicaoSimplesSql cond1 = new CondicaoSimplesSql();
            cond1.Campo = "Asset__r.PointofDelivery__r.CompanyID__c";
            cond1.Valor = new List<string>() {string.Format("'{0}'", codigoEmpresa)};
            cond1.Fixo = true;

            CondicaoSimplesSql cond2 = new CondicaoSimplesSql();
            cond2.Campo = "Asset__r.PointofDelivery__r.Country__c";
            cond2.Valor = new List<string>() {"'BRASIL'"};
            cond2.Fixo = true;

            CondicaoSimplesSql cond3 = new CondicaoSimplesSql();
            cond3.Campo = "Asset__r.PointOfDelivery__r.Name";
            cond3.Valor = clientes.Select(c => string.Concat("'",c.Trim(),"'")).ToList();

            List<CondicaoSql> condicoes = new List<CondicaoSql>();
            condicoes.Add(cond1);
            condicoes.Add(cond2);
            condicoes.Add(cond3);

            return EntidadeConversor.ToContractLineItemGoverno(ConsultarS(sql, condicoes, ref binding));
        }



        internal static List<ClienteSalesforce> GetContasByListaClientes(string codigoEmpresa, List<string> clientes, ref SforceService binding)
        {
            String sql = string.Format(@"select   Id
                                                , Name
                                            from PointOfDelivery__C ");

            CondicaoSimplesSql cond1 = new CondicaoSimplesSql();
            cond1.Campo = "CompanyID__c";
            cond1.Valor = new List<string>() {string.Format("'{0}'", codigoEmpresa)};
            cond1.Fixo = true;

            CondicaoSimplesSql cond2 = new CondicaoSimplesSql();
            cond2.Campo = "Country__c";
            cond2.Valor = new List<string>() {"'BRASIL'"};
            cond2.Fixo = true;

            CondicaoSimplesSql cond3 = new CondicaoSimplesSql();
            cond3.Campo = "Name";
            cond3.Valor = clientes.Select(c => string.Concat("'",c.Trim(),"'")).ToList();

            List<CondicaoSql> condicoes = new List<CondicaoSql>();
            condicoes.Add(cond1);
            condicoes.Add(cond2);
            condicoes.Add(cond3);

            return EntidadeConversor.ToClienteSalesforce(ConsultarS(sql, condicoes, ref binding));
        }


        //internal static List<AssetDTO> GetAssetesByListaClientes(string codigoEmpresa, List<string> clientes, ref SforceService binding)
        //{
//            String sql = string.Format(@"select Id
//                                                , PointofDelivery__r.Id
//                                                , PointofDelivery__r.Name
//                                                , PointofDelivery__r.CNT_Contract__r.ContractNumber
//                                                , NE__Status__c
//                                                , NE__Order_Config__c
//                                                , ExternalID__c
//                                           from Asset  ");

//            List<CondicaoSql> condicoes = new List<CondicaoSql>();
//            CondicaoSimplesSql cond = new CondicaoSimplesSql();
//            cond.Campo = "PointofDelivery__r.CompanyID__c";
//            cond.Valor = new List<string>() { string.Format("'{0}'", codigoEmpresa) };
//            cond.Fixo = true;
//            condicoes.Add(cond);

//            cond = new CondicaoSimplesSql();
//            cond.Campo = "PointofDelivery__r.Country__c";
//            cond.Valor = new List<string>() { "'BRASIL'" };
//            cond.Fixo = true;
//            condicoes.Add(cond);

//            cond = new CondicaoSimplesSql();
//            cond.Campo = "NE__Status__c";
//            cond.Valor = new List<string>() { "'Active','Activated'" };
//            cond.Fixo = true;
//            //condicoes.Add(cond);

//            cond = new CondicaoSimplesSql();
//            cond.Campo = "PointofDelivery__r.Name";
//            cond.Valor = clientes.Select(c => string.Concat("'", c.Trim(), "'")).ToList();
//            condicoes.Add(cond);

//            return EntidadeConversor.ToAssets(ConsultarS(sql, condicoes, ref binding));
        //}


        //internal static List<AssetDTO> GetAssetesByListaContaContratos(string codigoEmpresa, List<string> clientes, ref SforceService binding)
        //{
//            String sql = string.Format(@"select Id
//                                                , PointofDelivery__r.Id
//                                                , PointofDelivery__r.Name
//                                                , PointofDelivery__r.CNT_Contract__r.ContractNumber
//                                                , NE__Status__c 
//                                                , NE__Order_Config__c
//                                                , ExternalID__c
//                                           from Asset  ");

//            List<CondicaoSql> condicoes = new List<CondicaoSql>();
//            CondicaoSimplesSql cond = new CondicaoSimplesSql();
//            cond.Campo = "PointofDelivery__r.CompanyID__c";
//            cond.Valor = new List<string>() { string.Format("'{0}'", codigoEmpresa) };
//            cond.Fixo = true;
//            condicoes.Add(cond);

//            cond = new CondicaoSimplesSql();
//            cond.Campo = "PointofDelivery__r.Country__c";
//            cond.Valor = new List<string>() { "'BRASIL'" };
//            cond.Fixo = true;
//            condicoes.Add(cond);

//            cond = new CondicaoSimplesSql();
//            cond.Campo = "NE__Status__c";
//            cond.Valor = new List<string>() { "'Active','Activated'" };
//            cond.Fixo = true;
//            //condicoes.Add(cond);

//            cond = new CondicaoSimplesSql();
//            cond.Campo = "PointofDelivery__r.CNT_Contract__r.ContractNumber";
//            cond.Valor = clientes.Select(c => string.Concat("'", c.Trim(), "'")).ToList();
//            condicoes.Add(cond);

//            return EntidadeConversor.ToAssets(ConsultarS(sql, condicoes, ref binding));
        //}
        


        internal static List<CaseStatus> GetStatusCasos(ref SforceService binding)
        {
            String sql = string.Format(@"SELECT Id
	                                    , CaseNumber
	                                    , tolabel(SubCauseBR__c)
	                                    , tolabel(status)
	                                    , tolabel(type)
	                                    , pointofdelivery__r.name
	                                    , CNT_Contract__r.ContractNumber
	                                    , tolabel(CNT_Contract__r.status)
	                                    , CNT_Contract__r.CNT_ExternalContract_ID_2__c
	                                    , CreatedDate
	                                    , case.account.name
	                                    , closeddate
                                    FROM CASE 
                                    WHERE SubCauseBR__c IN ('10','11','12','13','14','15') 
                                    and PointofDelivery__r.SegmentType__c ='B'
                                    AND CreatedDate >= 2019-06-30T00:00:00.000+0000 
                                    AND CreatedDate <= 2019-11-13T11:40:53.000+0000");

            //	--, ( SELECT Status__c FROM Contracting_Status_BackOffices__r  )

            return EntidadeConversor.ToCaseStatus(Consultar(sql, ref binding));
        }



        internal static List<ContractLineItemGoverno> GetContractLineGovernosByListaContratos(string codigoEmpresa, List<string> clientes, ref SforceService binding)
        {
            String sql = string.Format(@"select   Id
                                                , Asset__r.Name
                                                , Asset__r.AccountId
                                                , Asset__r.ExternalId__c
                                                , Asset__c
                                                , Asset__r.PointOfDelivery__c
                                                , Asset__r.PointOfDelivery__r.Rate__c
                                                , Asset__r.PointOfDelivery__r.Name
                                                , Asset__r.PointOfDelivery__r.DetailAddress__r.Street_type__c
                                                , Asset__r.PointOfDelivery__r.DetailAddress__r.Name
                                                , Asset__r.PointOfDelivery__r.DetailAddress__r.Municipality__c
                                                , Asset__r.Account.Id                                                    
                                                , Asset__r.Account.Name
                                                , Asset__r.Account.IdentityNumber__c
                                                , Asset__r.Account.IdentityType__c
                                                , Asset__r.Account.ExternalId__c
                                                , Asset__r.Account.CNT_Executive__r.Name
                                                , Asset__r.Account.ParentId
                                                , Contract__c
                                                , Contract__r.CNT_Quote__c
                                                , Asset__r.NE__Order_Config__c
                                                , Contract__r.Contract_Type__c
                                                , Contract__r.CNT_GroupSegment__c
                                                , Contract__r.CNT_GroupArea__c
                                                , Contract__r.ContractNumber 
                                                , Asset__r.Contact.Name
                                            from Contract_Line_Item__c ");

            CondicaoSimplesSql cond1 = new CondicaoSimplesSql();
            cond1.Campo = "Asset__r.PointofDelivery__r.CompanyID__c";
            cond1.Valor = new List<string>() { string.Format("'{0}', 'COELCE'", codigoEmpresa) };
            cond1.Fixo = true;

            CondicaoSimplesSql cond2 = new CondicaoSimplesSql();
            cond2.Campo = "Asset__r.PointofDelivery__r.Country__c";
            cond2.Valor = new List<string>() { "'BRASIL'" };
            cond2.Fixo = true;

            CondicaoSimplesSql cond3 = new CondicaoSimplesSql();
            cond3.Campo = "CNT_Contract__c";
            cond3.Valor = clientes.Select(c => string.Concat("'", c.Trim(), "'")).ToList();

            List<CondicaoSql> condicoes = new List<CondicaoSql>();
            condicoes.Add(cond1);
            condicoes.Add(cond2);
            condicoes.Add(cond3);

            return EntidadeConversor.ToContractLineItemGoverno(ConsultarS(sql, condicoes, ref binding));
        }


        public static List<sObject> Consultar(string sql, ref SforceService binding)
        {
            return executarConsulta(sql, ref binding);
        }


        public static List<sObject> ConsultarS(string sql, List<CondicaoSql> condicoes, ref SforceService binding, bool incluirDeleted = false)
        {
            if (string.IsNullOrWhiteSpace(sql))
                return new List<sObject>();

            int contCondicoes = 0;
            List<sObject> result = new List<sObject>();
            StringBuilder sqlCondicao = new StringBuilder();

            if (condicoes == null || condicoes.Count == 0)
            {
                result.AddRange(executarConsulta(sql, ref binding, incluirDeleted));
            }
            else
            {
                string condicaoFixa = getCondicoesFixas(condicoes);

                foreach (CondicaoSimplesSql condicao in condicoes.Where(c => c.Fixo == false).ToList())
                {
                    if (condicao.Valor.Count == 0)
                        continue;

                    if (sqlCondicao.Length > 0)
                        sqlCondicao.Append(" AND ");

                    sqlCondicao.Append(string.Concat(condicao.Campo, " IN ("));
                    int contIn = 0;
                    foreach (string valor in condicao.Valor)
                    {
                        //controle.Add(valor);
                        if ((sql.Length + sqlCondicao.Length + valor.Length + condicaoFixa.Length) > 20000 - 15)
                        {
                            sqlCondicao.Append(")");

                            if (sqlCondicao.Length > 0 && condicaoFixa.Length > 0)
                                sqlCondicao.AppendFormat(" AND {0}", condicaoFixa.ToString());

                            result.AddRange(executarConsulta(string.Concat(sql, " WHERE ", sqlCondicao.ToString()), ref binding));
                            contCondicoes = contIn = 0;
                            sqlCondicao.Clear();

                            sqlCondicao.Append(string.Concat(condicao.Campo, " IN ("));
                        }

                        if (contIn > 0)
                            sqlCondicao.Append(", ");

                        sqlCondicao.Append(valor.Trim());
                        contCondicoes++;
                        contIn++;
                    }
                    sqlCondicao.Append(")");
                }

                if (sqlCondicao.Length > 0 && condicaoFixa.Length > 0)
                    sqlCondicao.AppendFormat(" AND {0}", condicaoFixa.ToString());

                if (sqlCondicao.Length > 0)
                    sql = string.Concat(sql, " WHERE ");

                result.AddRange(executarConsulta(string.Concat(sql, sqlCondicao.ToString()), ref binding));
            }
            return result;
        }

        public static List<sObject> ConsultarM(string sql, List<CondicaoMultiplaSql> condicoes, ref SforceService binding)
        {
            if (string.IsNullOrWhiteSpace(sql))
                return new List<sObject>();

            int contCondicoes = 0;
            List<sObject> result = new List<sObject>();
            StringBuilder sqlCondicao = new StringBuilder();

            if (condicoes == null || condicoes.Count == 0)
            {
                result.AddRange(executarConsulta(sql, ref binding));
            }
            else
            {
                string condicaoFixa = getCondicoesFixas(condicoes);

                foreach (CondicaoMultiplaSql condicao in condicoes.Where(c => c.Fixo == false).ToList())
                {
                    if (condicao.Grupo == null || condicao.Grupo.Count == 0)
                        continue;

                    foreach (string campo in condicao.Grupo.Keys)
                    {
                        if (sqlCondicao.Length > 0)
                            sqlCondicao.Append(" AND ");

                        sqlCondicao.Append(string.Concat(campo, " IN ("));
                        int contIn = 0;
                        foreach (string valor in condicao.Grupo[campo])
                        {
                            //controle.Add(valor);
                            if ((sql.Length + sqlCondicao.Length + valor.Length + condicaoFixa.Length) > 20000 - 2)
                            {
                                sqlCondicao.Append(")");

                                if (sqlCondicao.Length > 0 && condicaoFixa.Length > 0)
                                    sqlCondicao.AppendFormat(" AND {0}", condicaoFixa.ToString());

                                result.AddRange(executarConsulta(string.Concat(sql, " WHERE ", sqlCondicao.ToString()), ref binding));
                                contCondicoes = contIn = 0;
                                sqlCondicao.Clear();

                                sqlCondicao.Append(string.Concat(campo, " IN ("));
                            }

                            if (contIn > 0)
                                sqlCondicao.Append(", ");

                            sqlCondicao.Append(valor.Trim());
                            contCondicoes++;
                            contIn++;
                        }
                        sqlCondicao.Append(")");
                    }
                }

                if (sqlCondicao.Length > 0 && condicaoFixa.Length > 0)
                    sqlCondicao.AppendFormat(" AND {0}", condicaoFixa.ToString());

                if (sqlCondicao.Length > 0)
                    sql = string.Concat(sql, " WHERE ");

                result.AddRange(executarConsulta(string.Concat(sql, sqlCondicao.ToString()), ref binding));
            }
            return result;
        }
        
        private static string getCondicoesFixas(List<CondicaoSql> condicoes)
        {
            StringBuilder r = new StringBuilder();
            StringBuilder interno = new StringBuilder();

            foreach (CondicaoSimplesSql cond in ((condicoes.Where(c => ((CondicaoSimplesSql)c).Fixo == true).ToList())))
            {
                interno.Clear();
                if(r.Length > 0)
                    r.Append(" AND ");
                r.AppendFormat("{0} in (", cond.Campo);
                
                foreach (string valor in cond.Valor)
                {
                    if (interno.Length > 0)
                        interno.Append(", ");
                    interno.Append(valor);
                }
                r.Append(interno.ToString());
                r.Append(")");
            }
            return r.ToString();
        }

        private static string getCondicoesFixas(List<CondicaoMultiplaSql> condicoes)
        {
            StringBuilder r = new StringBuilder();
            StringBuilder interno = new StringBuilder();

            foreach (CondicaoMultiplaSql cond in condicoes.Where(c => c.Fixo == true).ToList())
            {
                foreach (string campo in cond.Grupo.Keys)
                {
                    interno.Clear();
                    if (r.Length > 0)
                        r.Append(" AND ");
                    r.AppendFormat("{0} in (", campo);

                    foreach (string valor in cond.Grupo[campo])
                    {
                        if (interno.Length > 0)
                            interno.Append(", ");
                        interno.Append(valor);
                    }
                    r.Append(interno.ToString());
                    r.Append(")");
                }
            }
            return r.ToString();
        }


        internal static List<sObject> executarConsulta(string sql, ref SforceService binding, bool incluirDeleted = false)
        {
            SFDCSchemeBuild schema = new SFDCSchemeBuild();
            List<sObject> result = new List<sObject>();
            QueryResult qr = null;
            binding.QueryOptionsValue = new SalesforceExtractor.apex.QueryOptions();
            binding.QueryOptionsValue.batchSize = 1000;
            binding.QueryOptionsValue.batchSizeSpecified = true;

            bool bContinue = true;
            const int maxTentativas = 30;
            int tentativa = 0;

            qr = incluirDeleted ? binding.queryAll(sql) : binding.query(sql);
            bContinue = true;

            while (bContinue)
            {
                if (qr == null || qr.records == null || qr.records.Length == 0)
                {
                    bContinue = false;
                    continue;
                }
                try
                {
                    result.AddRange(qr.records.ToList());
                    //for (int i = 0; i < qr.records.Length; i++)
                    //    result.Add(qr.records[i]);

                    if (qr.done)
                    {
                        bContinue = false;
                    }
                    else
                    {
                        qr = binding.queryMore(qr.queryLocator);
                    }
                }
                catch (System.Net.WebException ex)
                {
                    tentativa++;
                    if (tentativa == maxTentativas)
                        throw new System.Net.WebException(string.Format("Após {0} tentativas, não foi possível conectar-se o Salesforce: {1}", maxTentativas, sql));

                    bContinue = true;
                    Thread.Sleep(60 * 1000);
                    continue;
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }

            return result;
        }
        

//        internal static List<ContractLineItemSalesforce> GetContractLinesByNumeroContrato(string codigoEmpresaIn, string numeroContrato, ref SforceService binding)
//        {
//            SFDCSchemeBuild schema = new SFDCSchemeBuild();
//            List<ContractLineItemSalesforce> result = new List<ContractLineItemSalesforce>();

//            QueryResult qr = null;
//            binding.QueryOptionsValue = new SalesforceExtractor.apex.QueryOptions();
//            binding.QueryOptionsValue.batchSize = 3;
//            binding.QueryOptionsValue.batchSizeSpecified = true;

//            try
//            {
//                String sql = string.Format(@"select   Id
//                                                    , Asset__r.Name
//                                                    , Asset__r.AccountId
//                                                    , Asset__r.ExternalId__c
//                                                    , Asset__c
//                                                    , Asset__r.PointOfDelivery__c
//                                                    , Asset__r.PointOfDelivery__r.Name
//                                                    , Contract__c
//                                                    , Contract__r.CNT_Quote__c
//                                                    , Asset__r.NE__Order_Config__c
//                                                    , Contract__r.Contract_Type__c
//                                                    , Asset__r.Account.ExternalId__c
//                                                    , Asset__r.Account.Name
//                                                    , Asset__r.NE__Status__c
//                                                    , Billing_Profile__r.Id
//                                                    , Billing_Profile__r.Account__r.Name
//                                                    , Billing_Profile__r.Account__r.Id
//                                                    , Billing_Profile__r.PointofDelivery__r.name
//                                                    , Asset__r.ContactId
//                                               from Contract_Line_Item__c 
//                                              where Company__c in ({0})
//                                                    and CNT_Contract__c = '{1}'"
//                           , codigoEmpresaIn
//                           , numeroContrato
//                           );

//                                String sql2 = string.Format(@"select   Id
//                                                    , Asset__r.Name
//                                                    , Asset__r.AccountId
//                                                    , Asset__r.ExternalId__c
//                                                    , Asset__c
//                                                    , Asset__r.PointOfDelivery__c
//                                                    , Asset__r.PointOfDelivery__r.Name
//                                                    , Contract__c
//                                                    , Contract__r.CNT_Quote__c
//                                                    , Asset__r.NE__Order_Config__c
//                                                    , Contract__r.Contract_Type__c
//                                                    , Asset__r.Account.ExternalId__c
//                                                    , Asset__r.Account.Name
//                                                    , Asset__r.NE__Status__c
//                                                    , Billing_Profile__r.Id
//                                                    , Billing_Profile__r.Account__r.Name
//                                                    , Billing_Profile__r.Account__r.Id
//                                                    , Billing_Profile__r.PointofDelivery__r.name
//                                                    , Asset__r.ContactId
//                                               from Contract_Line_Item__c" );

//                Dictionary<string, List<string>> cond = new Dictionary<string,List<string>>();
//                cond.Add("CNT_Contract__c", new List<string>{string.Concat("'",numeroContrato,"'")});
//                cond.Add("Company__c", new List<string>{codigoEmpresaIn});
//                List<sObject> temp = Consultar(sql2, cond, ref binding);

//                List<ContractLineItemSalesforce> resultadoo = EntidadeConversor.ToContractLineItem(temp);
//                bool bContinue = true;
//                const int maxTentativas = 30;
//                int tentativa = 0;

//                while (bContinue)
//                {
//                    bContinue = false;
//                    try
//                    {
//                        qr = binding.query(sql);
//                        bContinue = true;
//                    }
//                    catch (System.Net.WebException ex)
//                    {
//                        tentativa++;
//                        if (tentativa == maxTentativas)
//                            throw new System.Net.WebException(string.Format("Após {0} tentativas, não foi possível conectar-se o Salesforce: {1}", maxTentativas, sql));

//                        bContinue = true;
//                        Thread.Sleep(60 * 1000);
//                        continue;
//                    }
//                    catch (Exception ex)
//                    {
//                        throw ex;
//                    }

//                    if (qr.records == null)
//                    {
//                        bContinue = false;
//                        continue;
//                    }

//                    if (qr.records.Length > 0)
//                    {
//                        for (int i = 0; i < qr.records.Length; i++)
//                        {
//                            SalesforceExtractor.apex.sObject con = qr.records[i];
//                            ContractLineItemSalesforce obj = new ContractLineItemSalesforce();

//                            obj.Id = schema.getFieldValue("Id", con.Any);
//                            obj.AccountIdAsset = schema.getFieldValueMore("Asset__r", "", "", "AccountId", con.Any);
//                            obj.AssetExternalId = schema.getFieldValueMore("Asset__r", "", "", "ExternalId__c", con.Any);
//                            obj.AssetId = schema.getFieldValue("Asset__c", con.Any);
//                            obj.BillingId = schema.getFieldValueMore("Billing_Profile__r", "", "", "Id", con.Any);
//                            obj.AssetName = schema.getFieldValueMore("Asset__r", "", "", "Name", con.Any);
//                            obj.PointOfDelivery = schema.getFieldValueMore("Asset__r", "", "", "PointOfDelivery__c", con.Any);
//                            obj.NumeroCliente = schema.getFieldValueMore("Asset__r", "PointOfDelivery__r", "", "Name", con.Any);
//                            obj.ContractId = schema.getFieldValue("Contract__c", con.Any);
//                            obj.Quote = schema.getFieldValueMore("Contract__r", "", "", "CNT_Quote__c", con.Any);
//                            obj.OrderConfig = schema.getFieldValueMore("Asset__r", "", "", "NE__Order_Config__c", con.Any);
//                            obj.ContractType = schema.getFieldValueMore("Contract__r", "", "", "Contract_Type__c", con.Any);
//                            obj.AccountExternalId = schema.getFieldValueMore("Asset__r", "Account", "", "ExternalId__c", con.Any);
//                            obj.AssetContactName = schema.getFieldValueMore("Asset__r", "Account", "", "Name", con.Any);
//                            obj.AssetStatus = schema.getFieldValueMore("Asset__r", "", "", "NE__Status__c", con.Any);
//                            obj.BillingContactName = schema.getFieldValueMore("Billing_Profile__r", "Account__r", "", "Name", con.Any);
//                            obj.AccountIdBilling = schema.getFieldValueMore("Billing_Profile__r", "Account__r", "", "Id", con.Any);
//                            obj.ContactId = schema.getFieldValueMore("Asset__r", "", "", "ContactId", con.Any);
//                            obj.BillingNumeroCliente = schema.getFieldValueMore("Billing_Profile__r", "PointofDelivery__r", "", "Name", con.Any);

//                            result.Add(obj);

//                            if (qr.done)
//                            {
//                                bContinue = false;
//                                //Console.WriteLine("Registrou: " + loopCounter);
//                            }
//                            else
//                                qr = binding.queryMore(qr.queryLocator);
//                        }
//                    }
//                }
//            }
//            catch (Exception ex)
//            {
//                Console.WriteLine(string.Format("\nProblema para executar a query: {0}\n", ex.Message));
//                throw new Exception(ex.Message);
//            }

//            return result;
//        }


        internal static List<OrderSalesforce> GetOrdersChargeNulos(DateTime data, ref SforceService binding)
        {
            SFDCSchemeBuild schema = new SFDCSchemeBuild();
            binding.QueryOptionsValue = new SalesforceExtractor.apex.QueryOptions();
            binding.QueryOptionsValue.batchSize = 3;
            binding.QueryOptionsValue.batchSizeSpecified = true;

            try
            {
                String sql = string.Format(@"select id, Name, NE__RecurringChargeOv__c, NE__OrderId__r.Country__c 
                                               from NE__OrderItem__c  
                                              where CreatedDate > {0}T00:00:00.000-00:00 
                                                and NE__RecurringChargeOv__c = null"
                           , data.ToString("yyyy-MM-dd"));

                List<sObject> temp = Consultar(sql, ref binding);

                List<OrderSalesforce> resultadoo = EntidadeConversor.ToOrderItems(temp);

                return resultadoo;
            }
            catch (Exception ex)
            {
                Console.WriteLine(string.Format("\nProblema para executar a query: {0}\n", ex.Message));
                throw new Exception(ex.Message);
            }
        }



        public static List<ClienteSalesforce> GetClientesPorNumeroContrato(string codigoEmpresaIn, List<string> contratos, ref SforceService binding)
        {
            List<ContractLineItemSalesforce> lstContratos = GetContractLinesByNumeroContrato(codigoEmpresaIn, contratos, ref binding);

            return EntidadeConversor.FromContractLineItemsToPointOfDeliveries(lstContratos);
        }


        public static List<ContractLineItemSalesforce> GetContractLinesByNumeroContrato(string codigoEmpresaIn, List<string> contratos, ref SforceService binding)
        {
            String sql2 = string.Format(@"select   Id
                                                , Asset__r.Name
                                                , Asset__r.AccountId
                                                , Asset__r.ExternalId__c
                                                , Asset__c
                                                , Asset__r.PointOfDelivery__c
                                                , Asset__r.PointOfDelivery__r.Name
                                                , Contract__c
                                                , Contract__r.CNT_Quote__c
                                                , Asset__r.NE__Order_Config__c
                                                , Contract__r.ContractNumber
                                                , Contract__r.Contract_Type__c
                                                , Contract__r.CNT_Case__c
                                                , Contract__r.Status
                                                , Asset__r.Account.ExternalId__c
                                                , Asset__r.Account.Name
                                                , Asset__r.NE__Status__c
                                                , Asset__r.RecordTypeId
                                                , Billing_Profile__r.Id
                                                , Billing_Profile__r.Account__r.Name
                                                , Billing_Profile__r.Account__r.Id
                                                , Billing_Profile__r.PointofDelivery__c
                                                , Billing_Profile__r.PointofDelivery__r.name
                                                , Asset__r.ContactId
                                            from Contract_Line_Item__c");

            List<CondicaoSql> condicoes = new List<CondicaoSql>();

            CondicaoSimplesSql condicao1 = new CondicaoSimplesSql();
            condicao1.Campo = "CNT_Contract__c";
            condicao1.Fixo = false;
            condicao1.Valor = contratos.OrderBy(c => c.FirstOrDefault()).Select(x => string.Concat("'", x.Replace("'", ""), "'")).ToList();
            condicoes.Add(condicao1);

            CondicaoSimplesSql condicao2 = new CondicaoSimplesSql();
            condicao2.Campo = "Company__c";
            condicao2.Fixo = true;
            condicao2.Valor = codigoEmpresaIn.Split(new char[] { ',' }).ToList();
            condicoes.Add(condicao2);


            List<sObject> temp = ConsultarS(sql2, condicoes, ref binding);

            List<ContractLineItemSalesforce> resultado = EntidadeConversor.ToContractLineItemList(temp);
            return resultado;
        }


        public static List<BillingSalesforce> GetBillingsPorNumeroCliente(string codigoEmpresaIn, List<string> contratos, ref SforceService binding)
        {
            String sql2 = string.Format(@"SELECT Id, Billing_Profile__r.Pointofdelivery__c , Billing_Profile__r.Pointofdelivery__r.Name, Billing_Profile__r.CNT_Contract__c, Billing_Profile__r.AccountContract__c
                                                 , Billing_Profile__r.Account__r.IdentityNumber__c, Billing_Profile__r.Account__r.IdentityType__c, Billing_Profile__r.ExternalID__c
                                                 , Billing_Profile__r.PointofDelivery__r.SegmentType__c
                                            FROM Contract_Line_Item__c ");

            //Pointofdelivery__r.Country__c = '{0}'
            //and Pointofdelivery__r.CompanyId__c = '{1}'
            //AND AccountContract__c = '{2}'

            List<CondicaoSql> condicoes = new List<CondicaoSql>();
            CondicaoSimplesSql condicao = new CondicaoSimplesSql();

            condicao.Campo = "Billing_Profile__r.AccountContract__c";
            condicao.Fixo = false;
            condicao.Valor = contratos.Select(x => string.Concat("'", x.Replace("'", ""), "'")).ToList();
            condicoes.Add(condicao);

            condicao = new CondicaoSimplesSql();
            condicao.Campo = "Asset__r.Name";
            condicao.Fixo = false;
            condicao.Valor = new string[] { "'Grupo B'", "'Grupo A'", "'Eletricity Service'" }.ToList();
            condicoes.Add(condicao);

            condicao = new CondicaoSimplesSql();
            condicao.Campo = "Billing_Profile__r.Account__r.CompanyID__c";
            condicao.Fixo = true;
            condicao.Valor = codigoEmpresaIn.Split(new char[] { ',' }).ToList();
            condicoes.Add(condicao);

            condicao = new CondicaoSimplesSql();
            condicao.Campo = "Billing_Profile__r.Account__r.Country__c";
            condicao.Fixo = true;
            condicao.Valor = new string[] { "'BRASIL'" }.ToList();
            condicoes.Add(condicao);

            condicao = new CondicaoSimplesSql();
            condicao.Campo = "CNT_Status__c";
            condicao.Fixo = true;
            condicao.Valor = new string[] { "'Active'", "''" }.ToList();
            condicoes.Add(condicao);

            List<sObject> temp = ConsultarS(sql2, condicoes, ref binding);

            List<BillingSalesforce> resultado = EntidadeConversor.ToBillingProfile(temp);
            return resultado;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="codigosEmpresaIn"></param>
        /// <param name="dataInicio"></param>
        /// <param name="dataFim"></param>
        /// <param name="binding"></param>
        /// <returns></returns>
        public static List<AddressSalesforce> GetAlteracaoEnderecosPorPeriodo(string codigosEmpresaIn, string dataInicio, string dataFim, ref SforceService binding)
        {
            //string campos = 
            String sql2 = string.Format(@"select 	DetailAddress__r.Id, CNT_Contract__r.CNT_ExternalContract_ID__c, Name, DetailAddress__r.ExternalId__c
	                                                , DetailAddress__r.Street_type__c, DetailAddress__r.StreetMD__r.Street__c
                                                    , DetailAddress__r.StreetMD__r.Neighbourhood__c, DetailAddress__r.Municipality__c
	                                                , DetailAddress__r.Number__c, DetailAddress__r.Postal_Code__c, DetailAddress__r.Region__c, DetailAddress__r.Corner__c
                                            from PointOfDelivery__c 
                                           where DetailAddress__c in
	                                            (select ParentId
		                                           from Address__History 
		                                          where field in ('StreetMD__c', 'created', 'Name', 'Postal_Code__c', 'Municipality__c', 'Number__c', 'Corner__c') 
	                                            )
                                             and CompanyID__c IN ({2})
               AND (
                   Name in (
                       '10334202','38920077'
                   ) 
                   OR CNT_Contract__r.ContractNumber in (
                       '10334202','38920077'
                   )
            )
                "
                , dataInicio
                , dataFim
                , codigosEmpresaIn);

            //   AND (
            //       Name in (
            //           '10334202','38920077'
            //       ) 
            //       OR CNT_Contract__r.ContractNumber in (
            //           '10334202','38920077'
            //       )
            //)

            //and CreatedDate >= {0}T00:00:00.068-03:00 
            //and CreatedDate < {1}T00:00:00.068-03:00

            //and AccountContract__c in ('9910010','38841562','39458589')

            List<CondicaoSql> condicoes = new List<CondicaoSql>();
            List<sObject> temp = ConsultarS(sql2, condicoes, ref binding, true);

            List<AddressSalesforce> resultado = EntidadeConversor.FromBillingToAddress(temp);
            return resultado.GroupBy(r => new { r.Id, r.TipoEndereco, r.Endereco, r.Number__c, r.Postal_Code__c, r.Municipality__c, r.Region, r.Bairro, r.Complemento, r.NumeroCliente }).Select(a => new AddressSalesforce() { Id = a.Key.Id, Endereco = a.Key.Endereco, TipoEndereco = a.Key.TipoEndereco, Number__c = a.Key.Number__c, Postal_Code__c = a.Key.Postal_Code__c, Municipality__c = a.Key.Municipality__c, Region = a.Key.Region, Bairro = a.Key.Bairro, Complemento = a.Key.Complemento, NumeroCliente = a.Key.NumeroCliente }).ToList();
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="codigosEmpresaIn"></param>
        /// <param name="lstClientes"></param>
        /// <param name="binding"></param>
        /// <returns></returns>
        public static List<AddressSalesforce> GetAlteracaoEnderecosPorClientes(string codigosEmpresaIn, List<string> lstClientes, ref SforceService binding)
        {
            if (lstClientes.Count == 0)
                return new List<AddressSalesforce>();

            String sql2 = string.Format(@"select 	DetailAddress__r.Id, CNT_Contract__r.CNT_ExternalContract_ID__c, Name, DetailAddress__r.ExternalId__c
	                                                , DetailAddress__r.Street_type__c, DetailAddress__r.StreetMD__r.Street__c
                                                    , DetailAddress__r.StreetMD__r.Neighbourhood__c, DetailAddress__r.Municipality__c
	                                                , DetailAddress__r.Number__c, DetailAddress__r.Postal_Code__c, DetailAddress__r.Region__c, DetailAddress__r.Corner__c
                                            from PointOfDelivery__c 
                                           where DetailAddress__c in
	                                            (select ParentId
		                                           from Address__History 
		                                          where field in ('Postal_Code__c', 'created') 
		                                            and CreatedDate >= {0}T00:00:00.068-03:00 
                                                    and CreatedDate < {1}T00:00:00.068-03:00
	                                            )
                                             and CompanyID__c IN ({2})"
                , string.Join("','", lstClientes)
                , codigosEmpresaIn);

                                             //   AND (
                                             //       Name in (
                                             //           '{1}'
                                             //       ) 
                                             //       OR CNT_Contract__r.ContractNumber in (
                                             //           '{1}'
                                             //       )
                                             //)

            //and AccountContract__c in ('9910010','38841562','39458589')

            //TODO: alterar consulta por CondicaoSql
            //TODO: alterar metodo de consulta sql para utilizar OR

            List<CondicaoSql> condicoes = new List<CondicaoSql>();
            List<sObject> temp = ConsultarS(sql2, condicoes, ref binding, true);

            List<AddressSalesforce> resultado = EntidadeConversor.FromBillingToAddress(temp);
            return resultado.GroupBy(r => new { r.Id, r.TipoEndereco, r.Endereco, r.Number__c, r.Postal_Code__c, r.Municipality__c, r.Region, r.Bairro, r.Complemento, r.NumeroCliente }).Select(a => new AddressSalesforce() { Id = a.Key.Id, Endereco = a.Key.Endereco, TipoEndereco = a.Key.TipoEndereco, Number__c = a.Key.Number__c, Postal_Code__c = a.Key.Postal_Code__c, Municipality__c = a.Key.Municipality__c, Region = a.Key.Region, Bairro = a.Key.Bairro, Complemento = a.Key.Complemento, NumeroCliente = a.Key.NumeroCliente }).ToList();
        }


        internal static List<ContractSalesforce> GetContractByExternalId(string externalId, ref SforceService binding)
        {
            SFDCSchemeBuild schema = new SFDCSchemeBuild();
            List<ContractSalesforce> result = new List<ContractSalesforce>();

            QueryResult qr = null;
            binding.QueryOptionsValue = new SalesforceExtractor.apex.QueryOptions();
            binding.QueryOptionsValue.batchSize = 3;
            binding.QueryOptionsValue.batchSizeSpecified = true;

            try
            {
                String sql = string.Format(@"select   Id
                                               from Contract
                                              where ExternalId__c = '{0}'"
                    , externalId);

                bool bContinue = true;
                const int maxTentativas = 30;
                int tentativa = 0;

                while (bContinue)
                {
                    bContinue = false;
                    try
                    {
                        qr = binding.query(sql);
                        bContinue = true;
                    }
                    catch (System.Net.WebException ex)
                    {
                        tentativa++;
                        if (tentativa == maxTentativas)
                            throw new System.Net.WebException(string.Format("Após {0} tentativas, não foi possível conectar-se o Salesforce: {1}", maxTentativas, sql));

                        bContinue = true;
                        Thread.Sleep(60 * 1000);
                        continue;
                    }
                    catch (Exception ex)
                    {
                        throw ex;
                    }

                    if (qr.records == null)
                    {
                        bContinue = false;
                        continue;
                    }

                    if (qr.records.Length > 0)
                    {
                        for (int i = 0; i < qr.records.Length; i++)
                        {
                            SalesforceExtractor.apex.sObject con = qr.records[i];
                            ContractSalesforce obj = new ContractSalesforce();

                            obj.Id = schema.getFieldValue("Id", con.Any);
                            obj.ExternalId = schema.getFieldValue("ExternalId__c", con.Any);
                            obj.ContractType = schema.getFieldValue("Contract_Type__c", con.Any);

                            result.Add(obj);

                            if (qr.done)
                            {
                                bContinue = false;
                                return result;
                                //Console.WriteLine("Registrou: " + loopCounter);
                            }
                            else
                                qr = binding.queryMore(qr.queryLocator);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(string.Format("\nProblema para executar a query: {0}\n", ex.Message));
                throw new Exception(ex.Message);
            }

            return result;
        }


        internal static ContractLineItemSalesforce GetContractLineByContractId(string contractId, ref SforceService binding)
        {
            SFDCSchemeBuild schema = new SFDCSchemeBuild();
            List<ContractLineItemSalesforce> result = new List<ContractLineItemSalesforce>();

            QueryResult qr = null;
            binding.QueryOptionsValue = new SalesforceExtractor.apex.QueryOptions();
            binding.QueryOptionsValue.batchSize = 3;
            binding.QueryOptionsValue.batchSizeSpecified = true;

            try
            {
                String sql = string.Format(@"select   Id
                                                    , Asset__r.AccountId
                                                    , Asset__r.ExternalId__c
                                                    , Asset__c
                                                    , Asset__r.PointOfDelivery__c
                                                    , Asset__r.PointOfDelivery__r.Name
                                                    , Contract__c
                                                    , Contract__r.CNT_Quote__c
                                                    , Asset__r.NE__Order_Config__c
                                                    , Contract__r.Contract_Type__c
                                               from Contract_Line_Item__c 
                                              where Contract__r.Id = '{0}'"
                    , contractId);

                bool bContinue = true;
                const int maxTentativas = 30;
                int tentativa = 0;

                while (bContinue)
                {
                    bContinue = false;
                    try
                    {
                        qr = binding.query(sql);
                        bContinue = true;
                    }
                    catch (System.Net.WebException ex)
                    {
                        tentativa++;
                        if (tentativa == maxTentativas)
                            throw new System.Net.WebException(string.Format("Após {0} tentativas, não foi possível conectar-se o Salesforce: {1}", maxTentativas, sql));

                        bContinue = true;
                        Thread.Sleep(60 * 1000);
                        continue;
                    }
                    catch (Exception ex)
                    {
                        throw ex;
                    }

                    if (qr.records == null)
                    {
                        bContinue = false;
                        continue;
                    }

                    if (qr.records.Length > 0)
                    {
                        for (int i = 0; i < qr.records.Length; i++)
                        {
                            SalesforceExtractor.apex.sObject con = qr.records[i];
                            ContractLineItemSalesforce obj = new ContractLineItemSalesforce();

                            obj.Id = schema.getFieldValue("Id", con.Any); 
                            obj.AccountIdAsset = schema.getFieldValueMore("Asset__r", "", "", "AccountId", con.Any);
                            obj.AssetExternalId = schema.getFieldValueMore("Asset__r", "", "", "ExternalId__c", con.Any);
                            obj.AssetId = schema.getFieldValue("Asset__c", con.Any);
                            obj.PointOfDelivery = schema.getFieldValueMore("Asset__r", "", "", "PointOfDelivery__c", con.Any);
                            obj.NumeroCliente = schema.getFieldValueMore("Asset__r", "PointOfDelivery__r", "", "Name", con.Any);
                            obj.ContractId = schema.getFieldValue("Contract__c", con.Any);
                            obj.ContractQuote = schema.getFieldValueMore("Contract__r", "", "", "CNT_Quote__c", con.Any);
                            obj.OrderConfig = schema.getFieldValueMore("Asset__r", "", "", "NE__Order_Config__c", con.Any);
                            obj.ContractType = schema.getFieldValueMore("Contract__r", "", "", "Contract_Type__c", con.Any);

                            result.Add(obj);

                            if (qr.done)
                            {
                                bContinue = false;
                                return result.First();
                                //Console.WriteLine("Registrou: " + loopCounter);
                            }
                            else
                                qr = binding.queryMore(qr.queryLocator);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(string.Format("\nProblema para executar a query: {0}\n", ex.Message));
                throw new Exception(ex.Message);
            }

            return new ContractLineItemSalesforce();
        }


        internal static ContractLineItemSalesforce GetContractLineByAssetsId(string contractId, ref SforceService binding)
        {
            SFDCSchemeBuild schema = new SFDCSchemeBuild();
            List<ContractLineItemSalesforce> result = new List<ContractLineItemSalesforce>();

            QueryResult qr = null;
            binding.QueryOptionsValue = new SalesforceExtractor.apex.QueryOptions();
            binding.QueryOptionsValue.batchSize = 3;
            binding.QueryOptionsValue.batchSizeSpecified = true;

            try
            {
                String sql = string.Format(@"select   Id
                                                    , Asset__r.AccountId
                                                    , Asset__r.ExternalId__c
                                                    , Asset__c
                                                    , Asset__r.PointOfDelivery__c
                                                    , Asset__r.PointOfDelivery__r.Name
                                                    , Contract__c
                                                    , Contract__r.CNT_Quote__c
                                                    , Asset__r.NE__Order_Config__c
                                                    , Contract__r.Contract_Type__c
                                               from Contract_Line_Item__c 
                                              where Asset__r.Id = '{0}'"
                    , contractId);

                bool bContinue = true;
                const int maxTentativas = 30;
                int tentativa = 0;

                while (bContinue)
                {
                    bContinue = false;
                    try
                    {
                        qr = binding.query(sql);
                        bContinue = true;
                    }
                    catch (System.Net.WebException ex)
                    {
                        tentativa++;
                        if (tentativa == maxTentativas)
                            throw new System.Net.WebException(string.Format("Após {0} tentativas, não foi possível conectar-se o Salesforce: {1}", maxTentativas, sql));

                        bContinue = true;
                        Thread.Sleep(60 * 1000);
                        continue;
                    }
                    catch (Exception ex)
                    {
                        throw ex;
                    }

                    if (qr.records == null)
                    {
                        bContinue = false;
                        continue;
                    }

                    if (qr.records.Length > 0)
                    {
                        for (int i = 0; i < qr.records.Length; i++)
                        {
                            SalesforceExtractor.apex.sObject con = qr.records[i];
                            ContractLineItemSalesforce obj = new ContractLineItemSalesforce();

                            obj.Id = schema.getFieldValue("Id", con.Any);
                            obj.AccountIdAsset = schema.getFieldValueMore("Asset__r", "", "", "AccountId", con.Any);
                            obj.AssetExternalId = schema.getFieldValueMore("Asset__r", "", "", "ExternalId__c", con.Any);
                            obj.AssetId = schema.getFieldValue("Asset__c", con.Any);
                            obj.PointOfDelivery = schema.getFieldValueMore("Asset__r", "", "", "PointOfDelivery__c", con.Any);
                            obj.NumeroCliente = schema.getFieldValueMore("Asset__r", "PointOfDelivery__r", "", "Name", con.Any);
                            obj.ContractId = schema.getFieldValue("Contract__c", con.Any);
                            obj.ContractQuote = schema.getFieldValueMore("Contract__r", "", "", "CNT_Quote__c", con.Any);
                            obj.OrderConfig = schema.getFieldValueMore("Asset__r", "", "", "NE__Order_Config__c", con.Any);
                            obj.ContractType = schema.getFieldValueMore("Contract__r", "", "", "Contract_Type__c", con.Any);

                            result.Add(obj);

                            if (qr.done)
                            {
                                bContinue = false;
                                return result.First();
                                //Console.WriteLine("Registrou: " + loopCounter);
                            }
                            else
                                qr = binding.queryMore(qr.queryLocator);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(string.Format("\nProblema para executar a query: {0}\n", ex.Message));
                throw new Exception(ex.Message);
            }

            return new ContractLineItemSalesforce();
        }


        internal static List<ContractLineItemSalesforce> GetContractLineByExternalId(string externalId, ref SforceService binding)
        {
            SFDCSchemeBuild schema = new SFDCSchemeBuild();
            List<ContractLineItemSalesforce> result = new List<ContractLineItemSalesforce>();

            QueryResult qr = null;
            binding.QueryOptionsValue = new SalesforceExtractor.apex.QueryOptions();
            binding.QueryOptionsValue.batchSize = 3;
            binding.QueryOptionsValue.batchSizeSpecified = true;

            try
            {
                String sql = string.Format(@"select   Id
                                                    , Asset__r.AccountId
                                                    , Asset__r.ExternalId__c
                                                    , Asset__c
                                                    , Asset__r.PointOfDelivery__c
                                                    , Asset__r.PointOfDelivery__r.Name
                                                    , Contract__c
                                                    , Contract__r.CNT_Quote__c
                                                    , Asset__r.NE__Order_Config__c
                                                    , Contract__r.Contract_Type__c
                                               from Contract_Line_Item__c 
                                              where ExternalId__c = '{0}'"
                    , externalId);

                bool bContinue = true;
                const int maxTentativas = 30;
                int tentativa = 0;

                while (bContinue)
                {
                    bContinue = false;
                    try
                    {
                        qr = binding.query(sql);
                        bContinue = true;
                    }
                    catch (System.Net.WebException ex)
                    {
                        tentativa++;
                        if (tentativa == maxTentativas)
                            throw new System.Net.WebException(string.Format("Após {0} tentativas, não foi possível conectar-se o Salesforce: {1}", maxTentativas, sql));

                        bContinue = true;
                        Thread.Sleep(60 * 1000);
                        continue;
                    }
                    catch (Exception ex)
                    {
                        throw ex;
                    }

                    if (qr.records == null)
                    {
                        bContinue = false;
                        continue;
                    }

                    if (qr.records.Length > 0)
                    {
                        for (int i = 0; i < qr.records.Length; i++)
                        {
                            SalesforceExtractor.apex.sObject con = qr.records[i];
                            ContractLineItemSalesforce obj = new ContractLineItemSalesforce();

                            obj.Id = schema.getFieldValue("Id", con.Any);
                            obj.AccountIdAsset = schema.getFieldValueMore("Asset__r", "", "", "AccountId", con.Any);
                            obj.AssetExternalId = schema.getFieldValueMore("Asset__r", "", "", "ExternalId__c", con.Any);
                            obj.AssetId = schema.getFieldValue("Asset__c", con.Any);
                            obj.PointOfDelivery = schema.getFieldValueMore("Asset__r", "", "", "PointOfDelivery__c", con.Any);
                            obj.NumeroCliente = schema.getFieldValueMore("Asset__r", "PointOfDelivery__r", "", "Name", con.Any);
                            obj.ContractId = schema.getFieldValue("Contract__c", con.Any);
                            obj.ContractQuote = schema.getFieldValueMore("Contract__r", "", "", "CNT_Quote__c", con.Any);
                            obj.OrderConfig = schema.getFieldValueMore("Asset__r", "", "", "NE__Order_Config__c", con.Any);
                            obj.ContractType = schema.getFieldValueMore("Contract__r", "", "", "Contract_Type__c", con.Any);

                            result.Add(obj);

                            if (qr.done)
                            {
                                bContinue = false;
                                return result;
                                //Console.WriteLine("Registrou: " + loopCounter);
                            }
                            else
                                qr = binding.queryMore(qr.queryLocator);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(string.Format("\nProblema para executar a query: {0}\n", ex.Message));
                throw new Exception(ex.Message);
            }

            return result;
        }


        internal static List<ContractLineItemSalesforce> GetContractLineByExternalId(List<string> lstExternalId, ref SforceService binding)
        {
            SFDCSchemeBuild schema = new SFDCSchemeBuild();
            List<ContractLineItemSalesforce> result = new List<ContractLineItemSalesforce>();

            QueryResult qr = null;
            binding.QueryOptionsValue = new SalesforceExtractor.apex.QueryOptions();
            binding.QueryOptionsValue.batchSize = 3;
            binding.QueryOptionsValue.batchSizeSpecified = true;

            List<string> lstConsulta = new List<string>();
            StringBuilder cond = new StringBuilder();
            ContractLineItemSalesforce obj = new ContractLineItemSalesforce();

            String sqlbase = @"select   Id
                                    , ExternalId__c
                                    , Asset__r.AccountId
                                    , Asset__r.ExternalId__c
                                    , Asset__c
                                    , Asset__r.PointOfDelivery__c
                                    , Asset__r.PointOfDelivery__r.Name
                                    , Contract__c
                                    , Contract__r.CNT_Quote__c
                                    , Asset__r.NE__Order_Config__c
                                    , Contract__r.Contract_Type__c
                                from Contract_Line_Item__c 
                                where ExternalId__c IN ({0}) ";

            foreach (string ext in lstExternalId.Distinct())
            {
                if ((sqlbase.Length + cond.Length + ext.Length) < (20000 - 3))
                {
                    cond.Append((cond.Length > 0) ? "," : string.Empty);
                    cond.Append(string.Concat("'", ext, "'"));

                    if (lstExternalId.Distinct().ToList().IndexOf(ext) != (lstExternalId.Distinct().Count() - 1))
                        continue;
                }

                if (cond.Length == 0)
                    continue;
                try
                {
                    String sql = string.Format(@"{0}", sqlbase).Replace("{0}", cond.ToString());

                    bool bContinue = true;
                    const int maxTentativas = 30;
                    int tentativa = 0;

                    bContinue = false;
                    try
                    {
                        qr = binding.query(sql);
                        bContinue = true;
                    }
                    catch (System.Net.WebException ex)
                    {
                        tentativa++;
                        if (tentativa == maxTentativas)
                            throw new System.Net.WebException(string.Format("Após {0} tentativas, não foi possível conectar-se o Salesforce: {1}", maxTentativas, sql));

                        bContinue = true;
                        Thread.Sleep(60 * 1000);
                        continue;
                    }
                    catch (Exception ex)
                    {
                        throw ex;
                    }

                    while (bContinue)
                    {
                        if (qr.records == null)
                        {
                            bContinue = false;
                            continue;
                        }

                        if (qr.records.Length > 0)
                        {
                            for (int i = 0; i < qr.records.Length; i++)
                            {
                                SalesforceExtractor.apex.sObject con = qr.records[i];
                                obj = new ContractLineItemSalesforce();

                                obj.Id = schema.getFieldValue("Id", con.Any);
                                obj.ExternalId__c = schema.getFieldValue("ExternalId__c", con.Any);
                                obj.AccountIdAsset = schema.getFieldValueMore("Asset__r", "", "", "AccountId", con.Any);
                                obj.AssetExternalId = schema.getFieldValueMore("Asset__r", "", "", "ExternalId__c", con.Any);
                                obj.AssetId = schema.getFieldValue("Asset__c", con.Any);
                                obj.PointOfDelivery = schema.getFieldValueMore("Asset__r", "", "", "PointOfDelivery__c", con.Any);
                                obj.NumeroCliente = schema.getFieldValueMore("Asset__r", "PointOfDelivery__r", "", "Name", con.Any);
                                obj.ContractId = schema.getFieldValue("Contract__c", con.Any);
                                obj.ContractQuote = schema.getFieldValueMore("Contract__r", "", "", "CNT_Quote__c", con.Any);
                                obj.OrderConfig = schema.getFieldValueMore("Asset__r", "", "", "NE__Order_Config__c", con.Any);
                                obj.ContractType = schema.getFieldValueMore("Contract__r", "", "", "Contract_Type__c", con.Any);

                                if (result.Where(x => x.ExternalId__c.Equals(obj.ExternalId__c)).Count() > 0)
                                {
                                    result.Remove(result.Where(x => x.ExternalId__c.Equals(obj.ExternalId__c)).FirstOrDefault());
                                    obj.ExternalId__c = string.Concat(obj.ExternalId__c, "[DUPLIC]", new Random(1).Next(lstExternalId.Count));
                                    result.Add(obj);
                                }
                                else
                                {
                                    result.Add(obj);
                                }
                            }

                            if (qr.done)
                                bContinue = false;
                            else
                                qr = binding.queryMore(qr.queryLocator);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(string.Format("\nProblema para executar a query: {0}\n", ex.Message));
                    throw new Exception(ex.Message);
                }

                cond.Clear();
                cond.Append(string.Concat("'", ext, "'"));
            }

            return result;
        }


        internal static List<ContractLineItemSalesforce> GetContractLineItemsByExternalId(string externalId, ref SforceService binding)
        {
            SFDCSchemeBuild schema = new SFDCSchemeBuild();
            List<ContractLineItemSalesforce> result = new List<ContractLineItemSalesforce>();

            QueryResult qr = null;
            binding.QueryOptionsValue = new SalesforceExtractor.apex.QueryOptions();
            binding.QueryOptionsValue.batchSize = 3;
            binding.QueryOptionsValue.batchSizeSpecified = true;

            try
            {
                String sql = string.Format(@"select   Asset__r.AccountId
                                                    , Asset__r.ExternalId__c
                                                    , Asset__c
                                                    , Asset__r.PointOfDelivery__c
                                                    , Asset__r.PointOfDelivery__r.Name
                                                    , Contract__c
                                                    , Contract__r.CNT_Quote__c
                                                    , Asset__r.NE__Order_Config__c
                                                    , Contract__r.Contract_Type__c
                                               from Contract_Line_Item__c 
                                              where Asset__r.ExternalId__c = '{0}'"
                    , externalId);

                bool bContinue = true;
                const int maxTentativas = 30;
                int tentativa = 0;

                while (bContinue)
                {
                    bContinue = false;
                    try
                    {
                        qr = binding.query(sql);
                        bContinue = true;
                    }
                    catch(System.Net.WebException ex)
                    {
                        tentativa++;
                        if (tentativa == maxTentativas)
                            throw new System.Net.WebException(string.Format("Após {0} tentativas, não foi possível conectar-se o Salesforce: {1}", maxTentativas, sql));

                        bContinue = true;
                        Thread.Sleep(60*1000);
                        continue;
                    }
                    catch (Exception ex) 
                    { 
                        throw ex; 
                    }

                    if (qr.records == null)
                    {
                        bContinue = false;
                        continue;
                    }

                    if (qr.records.Length > 0)
                    {
                        for (int i = 0; i < qr.records.Length; i++)
                        {
                            SalesforceExtractor.apex.sObject con = qr.records[i];
                            ContractLineItemSalesforce obj = new ContractLineItemSalesforce();

                            obj.AccountIdAsset = schema.getFieldValueMore("Asset__r", "", "", "AccountId", con.Any);
                            obj.AssetExternalId = schema.getFieldValueMore("Asset__r", "", "", "ExternalId__c", con.Any);
                            obj.AssetId = schema.getFieldValue("Asset__c", con.Any);
                            obj.PointOfDelivery = schema.getFieldValueMore("Asset__r", "", "", "PointOfDelivery__c", con.Any);
                            obj.NumeroCliente = schema.getFieldValueMore("Asset__r", "PointOfDelivery__r", "", "Name", con.Any);
                            obj.ContractId = schema.getFieldValue("Contract__c", con.Any);
                            obj.ContractQuote = schema.getFieldValueMore("Contract__r", "", "", "CNT_Quote__c", con.Any);
                            obj.OrderConfig = schema.getFieldValueMore("Asset__r", "", "", "NE__Order_Config__c", con.Any);
                            obj.ContractType = schema.getFieldValueMore("Contract__r", "", "", "Contract_Type__c", con.Any);

                            result.Add(obj);

                            if (qr.done)
                            {
                                bContinue = false;
                                //Console.WriteLine("Registrou: " + loopCounter);
                            }
                            else
                                qr = binding.queryMore(qr.queryLocator);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(string.Format("\nProblema para executar a query: {0}\n", ex.Message));
                throw new Exception(ex.Message);
            }

            return result;
        }

        internal static List<ContractLineItemSalesforce> GetContractLineItemsByAssetId(string Id, ref SforceService binding)
        {
            SFDCSchemeBuild schema = new SFDCSchemeBuild();
            List<ContractLineItemSalesforce> result = new List<ContractLineItemSalesforce>();

            QueryResult qr = null;
            binding.QueryOptionsValue = new SalesforceExtractor.apex.QueryOptions();
            binding.QueryOptionsValue.batchSize = 3;
            binding.QueryOptionsValue.batchSizeSpecified = true;

            try
            {
                String sql = string.Format(@"select   Asset__r.AccountId
                                                    , Asset__r.ExternalId__c
                                                    , Asset__c
                                                    , Asset__r.PointOfDelivery__c
                                                    , Asset__r.PointOfDelivery__r.Name
                                                    , Contract__c
                                                    , Contract__r.CNT_Quote__c
                                                    , Asset__r.NE__Order_Config__c
                                                    , Contract__r.Contract_Type__c
                                               from Contract_Line_Item__c 
                                              where Contract__r.Contract_Type__c in ('B2B', 'B2C') 
                                                and Asset__r.Id in ({0})"
                    , Id);

                bool bContinue = true;
                const int maxTentativas = 30;
                int tentativa = 0;

                while (bContinue)
                {
                    bContinue = false;
                    try
                    {
                        qr = binding.query(sql);
                        bContinue = true;
                    }
                    catch (System.Net.WebException ex)
                    {
                        tentativa++;
                        if (tentativa == maxTentativas)
                            throw new System.Net.WebException(string.Format("Após {0} tentativas, não foi possível conectar-se o Salesforce: {1}", maxTentativas, sql));

                        bContinue = true;
                        Thread.Sleep(60 * 1000);
                        continue;
                    }
                    catch (Exception ex)
                    {
                        throw ex;
                    }

                    if (qr.records == null)
                    {
                        bContinue = false;
                        continue;
                    }

                    if (qr.records.Length > 0)
                    {
                        for (int i = 0; i < qr.records.Length; i++)
                        {
                            SalesforceExtractor.apex.sObject con = qr.records[i];
                            ContractLineItemSalesforce obj = new ContractLineItemSalesforce();

                            obj.AccountIdAsset = schema.getFieldValueMore("Asset__r", "", "", "AccountId", con.Any);
                            obj.AssetExternalId = schema.getFieldValueMore("Asset__r", "", "", "ExternalId__c", con.Any);
                            obj.AssetId = schema.getFieldValue("Asset__c", con.Any);
                            obj.PointOfDelivery = schema.getFieldValueMore("Asset__r", "", "", "PointOfDelivery__c", con.Any);
                            obj.NumeroCliente = schema.getFieldValueMore("Asset__r", "PointOfDelivery__r", "", "Name", con.Any);
                            obj.ContractId = schema.getFieldValue("Contract__c", con.Any);
                            obj.ContractQuote = schema.getFieldValueMore("Contract__r", "", "", "CNT_Quote__c", con.Any);
                            obj.OrderConfig = schema.getFieldValueMore("Asset__r", "", "", "NE__Order_Config__c", con.Any);
                            obj.ContractType = schema.getFieldValueMore("Contract__r", "", "", "Contract_Type__c", con.Any);

                            result.Add(obj);

                            if (qr.done)
                            {
                                bContinue = false;
                                //Console.WriteLine("Registrou: " + loopCounter);
                            }
                            else
                                qr = binding.queryMore(qr.queryLocator);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(string.Format("\nProblema para executar a query: {0}\n", ex.Message));
                throw new Exception(ex.Message);
            }

            return result;
        }

        internal static List<AssetDTO> GetAssetsPorOrderId(string numeroOrders, ref SforceService binding)
        {
            //ATENCAO ATENCAO ATENCAO ATENCAO ATENCAO ATENCAO
            SFDCSchemeBuild schema = new SFDCSchemeBuild();
            QueryResult qr = null;
            binding.QueryOptionsValue = new apex.QueryOptions();
            binding.QueryOptionsValue.batchSize = 3;
            binding.QueryOptionsValue.batchSizeSpecified = true;

            try
            {
                String sql = string.Format(@"select Id
                                                 , AccountId
                                                 , ExternalId__c
                                                 , PointOfDelivery__r.Name
                                                 , NE__Order_Config__c
                                              from Asset
                                              where Company__c  = 'COELCE'
                                                and Country__c =  'BRASIL'
                                                and NE__Order_Config__c in ({0})"
                    , numeroOrders);

                bool bContinue = false;
                try
                {
                    qr = binding.query(sql);
                    bContinue = true;
                }
                catch (Exception ex)
                {
                    throw ex;
                }

                List<AssetDTO> result = new List<AssetDTO>();

                while (bContinue)
                {
                    if (qr.records == null || qr.records.Length == 0)
                    {
                        bContinue = false;
                        continue;
                    }

                    for (int i = 0; i < qr.records.Length; i++)
                    {
                        apex.sObject con = qr.records[i];
                        AssetDTO obj = new AssetDTO();
                        try
                        {
                            obj.Id = schema.getFieldValue("Id", con.Any);
                            obj.ExternalId = schema.getFieldValue("ExternalId__c", con.Any);
                            obj.NumeroCliente = schema.getFieldValueMore("PointOfDelivery__r", "", "", "Name", con.Any);
                            obj.AccountId = schema.getFieldValue("AccountId", con.Any);
                            obj.OrderId = schema.getFieldValue("NE__Order_Config__c", con.Any);
                            result.Add(obj);
                        }
                        catch (Exception x)
                        {
                            //TODO:  logar ou tratar como exceção mesmo, por nao ser possivel ter +1 Attribute por Order e ser um caso de erro em UAT apenas
                            continue;
                        }
                        if (qr.records.Length == 1)
                        {
                            bContinue = false;
                            continue;
                        }
                    }

                    //handle the loop + 1 problem by checking to see if the most recent queryResult
                    if (qr.done)
                    {
                        bContinue = false;
                        //Console.WriteLine("Registrou: " + loopCounter);
                    }
                    else
                        qr = binding.queryMore(qr.queryLocator);
                }

                return result;
            }
            catch (Exception ex)
            {
                Console.WriteLine("\nProblema para executar a query: \n" + ex.Message);
                Console.Write("\nClique para continuar...");
                Debugger.Break();
                Console.ReadLine();
            }

            return null;
        }



        internal static List<string> GetContasAgrupadorasCorrespondente(List<AccountSalesforce> contas, ref SforceService binding)
        {
            SFDCSchemeBuild schema = new SFDCSchemeBuild();
            String data = DateTime.Now.ToString("yyyyMMdd");

            QueryResult qr = null;
            binding.QueryOptionsValue = new apex.QueryOptions();
            binding.QueryOptionsValue.batchSize = 3;
            binding.QueryOptionsValue.batchSizeSpecified = true;

            try
            {
                String sql = string.Format(@"SELECT Account__c, CompanyID__c, ExternalId__c, Id, Name, RecordTypeId
                                               FROM Account WHERE ExternalId__c in ('{0}') ", string.Join("', '", contas.Select(x => { return string.Concat(x.ExternalId, "OC"); }).ToList()));

                bool bContinue = false;
                try
                {
                    qr = binding.query(sql);
                    bContinue = true;
                }
                catch
                {
                }

                //cliente = string.Empty;
                List<string> lstResult = new List<string>();

                while (bContinue)
                {
                    if (qr.records == null || qr.records.Length == 0)
                    {
                        bContinue = false;
                        continue;
                    }

                    for (int i = 0; i < qr.records.Length; i++)
                    {
                        apex.sObject con = qr.records[i];
                        lstResult.Add(string.Concat(schema.getFieldValue("Id", con.Any),"|",schema.getFieldValue("ExternalId__c", con.Any).Replace("OC", "")));

                        if (qr.records.Length == 1)
                        {
                            bContinue = false;
                            continue;
                        }
                    }

                    //handle the loop + 1 problem by checking to see if the most recent queryResult
                    if (qr.done)
                    {
                        bContinue = false;
                        //Console.WriteLine("Registrou: " + loopCounter);
                    }
                    else
                        qr = binding.queryMore(qr.queryLocator);
                }

                return lstResult;
            }
            catch (Exception ex)
            {
                Console.WriteLine("\nProblema para executar a query: \n" + ex.Message);
                Console.Write("\nClique para continuar...");
                Console.ReadLine();
            }

            return null;
        }


        public static DeleteResult[] Apagar(List<sObject> lstObjetos, ref SforceService binding)
        {
            if (lstObjetos.Count == 0)
                return new List<DeleteResult>().ToArray();

            List<DeleteResult> result = new List<DeleteResult>();

            DeleteResult[] saveResults = binding.delete(lstObjetos.Select(x => x.Id).ToArray());
                if (saveResults != null)
                    result.AddRange(saveResults.ToList());

            return result.ToArray();
        }
    }
}

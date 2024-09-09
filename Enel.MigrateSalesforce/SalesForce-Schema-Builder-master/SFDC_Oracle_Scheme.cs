/*Copyright (c) 2005, salesforce.com, inc.
All rights reserved.

Redistribution and use in source and binary forms, with or without 
modification, are permitted provided that the following conditions are met:

Redistributions of source code must retain the above copyright notice, 
this list of conditions and the following disclaimer. Redistributions in 
binary form must reproduce the above copyright notice, this list of 
conditions and the following disclaimer in the documentation and/or other 
materials provided with the distribution. 

Neither the name of salesforce.com, inc. nor the names of its contributors 
may be used to endorse or promote products derived from this software 
without specific prior written permission. 

THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" 
AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE 
IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE 
ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR CONTRIBUTORS BE 
LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR 
CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF 
SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS 
INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN 
CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) 
ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE 
POSSIBILITY OF SUCH DAMAGE.
*/

using System;
using basicSample_cs_p.apex;
using basicSample_cs_p.apexMetadata;
using System.Threading;
using System.Text;
using System.Xml;
using System.Web.Services;
using System.Web.Services.Protocols;
using System.Collections.Generic;
using System.Net;
using System.Web.Script.Serialization;
using System.Configuration;
using System.IO;
using System.Threading.Tasks;
using System.Web;
using System.Collections;
using System.Linq;

namespace basicSample_cs_p
{
    public class DadosMigrados
    {
        public string idAddress { get; set; }
        public string idPod { get; set; }
        public string numeroPod { get; set; }
        public string idBilling { get; set; }
        public string idContract { get; set; }
        public string idAsset { get; set; }
        public string idConta { get; set; }
        public Hashtable lstDevice { get; set; }
        public List<string> lstContatos { get; set; }
        public List<string> lstInvoice { get; set; }
        public List<string> lstConsumo { get; set; }
        public List<string> lstLeitura { get; set; }
        public List<string> lstPagamento { get; set; }
        public List<string> lstParcelamento { get; set; }
        public List<string> lstAssetAttr { get; set; }
    }

    class SFDCSchemeBuild
    {

        public enum SFDCDataModels
        {
            Sales,
            TaskAndEvent,
            Support,
            DocumentNoteAttachment,
            UserProfile,
            RecordType,
            ProductAndSchedule,
            ShareAndTeamSelling,
            CustomizableForecasting,
            TerritoryManagement,
            Process,
            Content,
            SalesForceChatter
        }

        private SforceService binding = null;
        private SforceService binding_execute = null;
        private apex.LoginResult loginResult = null;
        private String un = "";
        private String pw = "";
        private bool loggedIn = false;
        private apex.GetUserInfoResult userInfo = null;
        private String[] accounts = null;
        private String[] contacts = null;
        private String[] tasks = null;
        private DateTime serverTime;
        private static String caminho;
        private static DadosMigrados _dados = new DadosMigrados();

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            SFDCSchemeBuild samples1 = new SFDCSchemeBuild();
            System.Threading.Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("en-US");
            caminho = args[0];
            samples1.run();
        }

        private void run()
        {
            //BuscarContatos();
            //updateContatosCanalPreferente();
            //AtualizarFaturas();
            //AtualizarBillings();
            //AtualizarCl();
            //TestUpsert();
            //UpsertBillingsProfiles();
            //UpsertUrlFaturas();
            //UpsertAccountMaeNasc();
            //UpsertStatusFaturas();
            //UpsertCL();
            //testeAnonimo();
            //UpsertAssetProdutos();
            //UpsertDevice();
            //Upsertbilling();

            if (!loggedIn)
            {
                if (!login())
                    return;
            }

            StreamReader rs = new StreamReader(caminho);

            while (!rs.EndOfStream)
             {
                string[] linha = rs.ReadLine().Split(';');
                
                try
                {
                    PROD_TO_DES(linha[0], linha[1]);
                    Console.WriteLine("Processou o cliente: " + linha[0]);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Cliente deu erro: " + linha[0] + "\t" + ex.Message);
                    continue;
                }
            }
            
        }

        private void PROD_TO_DES(string conta_contrato, string compania)
        {  
             
            #region
            CargaStreetAddress(conta_contrato, compania);

            #endregion

            #region Carregando Conta e contatos
            CriarContaEContato(conta_contrato,compania);   
            #endregion

            CriarPodContratoBillingCl(conta_contrato, compania);

            CarregarOrder(conta_contrato, compania);
            CarregarMedidoresELeituras(conta_contrato, compania);

            CarregarFaturasConsumoPagamentos(conta_contrato, compania);

            if(!string.IsNullOrWhiteSpace(_dados.numeroPod))
                CarregarParcelamentos(_dados.numeroPod, compania);
        }

        private void CriarPodContratoBillingCl(string conta_contrato, string compania)
        {
            apex.QueryResult qr = null;
            basicSample_cs_p.apex.UpsertResult[] upsertResults;            

            String sql = $@"select 
                                Billing_Profile__r.PointofDelivery__r.Allows_the_use_of_data__c,
                                Billing_Profile__r.PointofDelivery__r.AsignCondominiumPod__c,
                                Billing_Profile__r.PointofDelivery__r.Associate__c,
                                Billing_Profile__r.PointofDelivery__r.Balance_in_Dispute_Indicator__c,
                                Billing_Profile__r.PointofDelivery__r.BallotName__c,
                                Billing_Profile__r.PointofDelivery__r.Billing_Period_Indicator__c,
                                Billing_Profile__r.PointofDelivery__r.Billing_Type__c,
                                Billing_Profile__r.PointofDelivery__r.Billingperiod__c,
                                Billing_Profile__r.PointofDelivery__r.Block__c,
                                Billing_Profile__r.PointofDelivery__r.BranchOffice__c,
                                Billing_Profile__r.PointofDelivery__r.Branch__c,
                                Billing_Profile__r.PointofDelivery__r.CNR_Indicator__c,
                                Billing_Profile__r.PointofDelivery__r.CNT_AccountsByType__c,
                                Billing_Profile__r.PointofDelivery__r.CNT_Continuous_Operation_Power_Unit__c,
                                Billing_Profile__r.PointofDelivery__r.CNT_Electrodependant_Origin__c,
                                Billing_Profile__r.PointofDelivery__r.CNT_Electrodependant_Type__c,
                                Billing_Profile__r.PointofDelivery__r.CNT_Free_Client__c,
                                Billing_Profile__r.PointofDelivery__r.CNT_Geracao_Distribuida__c,
                                Billing_Profile__r.PointofDelivery__r.CNT_Inactive_Date__c,
                                Billing_Profile__r.PointofDelivery__r.CNT_IsGD__c,
                                Billing_Profile__r.PointofDelivery__r.CNT_LowIncomeType__c,
                                Billing_Profile__r.PointofDelivery__r.CNT_Neighbour_Customer__c,
                                Billing_Profile__r.PointofDelivery__r.CNT_Operating_Range_of_Power_Unit__c,
                                Billing_Profile__r.PointofDelivery__r.CNT_Percentual_de_Participacao__c,
                                Billing_Profile__r.PointofDelivery__r.CNT_Potency_of_Power_Unit__c,
                                Billing_Profile__r.PointofDelivery__r.CNT_Public_Ilumination__c,
                                Billing_Profile__r.PointofDelivery__r.CNT_Rural_Irrigating__c,
                                Billing_Profile__r.PointofDelivery__r.CNT_Sensitive_Customer__c,
                                Billing_Profile__r.PointofDelivery__r.CNT_UniqueAddress__c,
                                Billing_Profile__r.PointofDelivery__r.CNT_Voltage__c,
                                Billing_Profile__r.PointofDelivery__r.CNT_White_Rate__c,
                                Billing_Profile__r.PointofDelivery__r.Central_Risk_Indicator__c,
                                Billing_Profile__r.PointofDelivery__r.Circuit_Name__c,
                                Billing_Profile__r.PointofDelivery__r.ClientQueries__c,
                                Billing_Profile__r.PointofDelivery__r.Client_Generator__c,
                                Billing_Profile__r.PointofDelivery__r.Client_Segment_Type__c,
                                Billing_Profile__r.PointofDelivery__r.Collection_House__c,
                                Billing_Profile__r.PointofDelivery__r.CompanyID__c,
                                Billing_Profile__r.PointofDelivery__r.Company__c,
                                Billing_Profile__r.PointofDelivery__r.CondominiumSector__c,
                                Billing_Profile__r.PointofDelivery__r.Condominium__c,
                                Billing_Profile__r.PointofDelivery__r.ConnectionStatus__c,
                                Billing_Profile__r.PointofDelivery__r.ConnectionType__c,
                                Billing_Profile__r.PointofDelivery__r.Connection_Status_Synergia__c,
                                Billing_Profile__r.PointofDelivery__r.Country__c,
                                Billing_Profile__r.PointofDelivery__r.CurrencyIsoCode,
                                Billing_Profile__r.PointofDelivery__r.CustomerRating__c,
                                Billing_Profile__r.PointofDelivery__r.Customer_classification__c,
                                Billing_Profile__r.PointofDelivery__r.CutRestrictionDate__c,
                                Billing_Profile__r.PointofDelivery__r.CutRestrictionReason__c,
                                Billing_Profile__r.PointofDelivery__r.CutRestriction__c,
                                Billing_Profile__r.PointofDelivery__r.CutoffDate__c,
                                Billing_Profile__r.PointofDelivery__r.CuttingDebt__c,
                                Billing_Profile__r.PointofDelivery__r.CuttingReason__c,
                                Billing_Profile__r.PointofDelivery__r.DCINumberArgentina__c,
                                Billing_Profile__r.PointofDelivery__r.DVNumberPointofDelivery__c,
                                Billing_Profile__r.PointofDelivery__r.DangerZone__c,
                                Billing_Profile__r.PointofDelivery__r.DisciplinaryMeasureF__c,
                                Billing_Profile__r.PointofDelivery__r.DisciplinaryMeasure__c,
                                Billing_Profile__r.PointofDelivery__r.DistributionAddress__c,
                                Billing_Profile__r.PointofDelivery__r.ElectricalSubstationConnection__c,
                                Billing_Profile__r.PointofDelivery__r.ElectricalSubstation__c,
                                Billing_Profile__r.PointofDelivery__r.Electrodependant__c,
                                Billing_Profile__r.PointofDelivery__r.Essential_Client__c,
                                Billing_Profile__r.PointofDelivery__r.Exento_de_corte__c,
                                Billing_Profile__r.PointofDelivery__r.ExternalId__c,
                                Billing_Profile__r.PointofDelivery__r.FeederNumber__c,
                                Billing_Profile__r.PointofDelivery__r.FullElectric__c,
                                Billing_Profile__r.PointofDelivery__r.GroupType__c,
                                Billing_Profile__r.PointofDelivery__r.HasApportionment__c,
                                Billing_Profile__r.PointofDelivery__r.HouseType__c,
                                Billing_Profile__r.PointofDelivery__r.Intallation_Type__c,
                                Billing_Profile__r.PointofDelivery__r.IsCommonService__c,
                                Billing_Profile__r.PointofDelivery__r.Key__c,
                                Billing_Profile__r.PointofDelivery__r.LiteralNetworkType__c,
                                Billing_Profile__r.PointofDelivery__r.LongAddress__c,
                                Billing_Profile__r.PointofDelivery__r.Long_Address__c,
                                Billing_Profile__r.PointofDelivery__r.Manufacturing_Date__c,
                                Billing_Profile__r.PointofDelivery__r.Market__c,
                                Billing_Profile__r.PointofDelivery__r.MassBreakdown__c,
                                Billing_Profile__r.PointofDelivery__r.MassiveBreakdown__c,
                                Billing_Profile__r.PointofDelivery__r.MeterBrand__c,
                                Billing_Profile__r.PointofDelivery__r.MeterModel__c,
                                Billing_Profile__r.PointofDelivery__r.MeterNumber__c,
                                Billing_Profile__r.PointofDelivery__r.MeterProperty__c,
                                Billing_Profile__r.PointofDelivery__r.MeterType__c,
                                Billing_Profile__r.PointofDelivery__r.MunicipalityAllocation__c,
                                Billing_Profile__r.PointofDelivery__r.Name,
                                Billing_Profile__r.PointofDelivery__r.NavigationContactID__c,
                                Billing_Profile__r.PointofDelivery__r.NetworkConnectionPoint__c,
                                Billing_Profile__r.PointofDelivery__r.Network_Type__c,
                                Billing_Profile__r.PointofDelivery__r.NewClientMeasure__c,
                                Billing_Profile__r.PointofDelivery__r.OpenCases__c,
                                Billing_Profile__r.PointofDelivery__r.OperationalCenter__c,
                                Billing_Profile__r.PointofDelivery__r.PODSec__c,
                                Billing_Profile__r.PointofDelivery__r.PODinBuilding__c,
                                Billing_Profile__r.PointofDelivery__r.PaymentProcess__c,
                                Billing_Profile__r.PointofDelivery__r.PointofDeliveryNumber__c,
                                Billing_Profile__r.PointofDelivery__r.PointofDeliveryStatus__c,
                                Billing_Profile__r.PointofDelivery__r.Power__c,
                                Billing_Profile__r.PointofDelivery__r.RateType__c,
                                Billing_Profile__r.PointofDelivery__r.Rate__c,
                                Billing_Profile__r.PointofDelivery__r.RationingSchedule__c,
                                Billing_Profile__r.PointofDelivery__r.Reading_Route__c,
                                Billing_Profile__r.PointofDelivery__r.RecordTypeId,
                                Billing_Profile__r.PointofDelivery__r.RepeatedCases__c,
                                Billing_Profile__r.PointofDelivery__r.ReqFAE__c,
                                Billing_Profile__r.PointofDelivery__r.RetirementDate__c,
                                Billing_Profile__r.PointofDelivery__r.Return_Check_Indicator__c,
                                Billing_Profile__r.PointofDelivery__r.Route__c,
                                Billing_Profile__r.PointofDelivery__r.SegmentType__c,
                                Billing_Profile__r.PointofDelivery__r.ServiceType__c,
                                Billing_Profile__r.PointofDelivery__r.Service_SubClass__c,
                                Billing_Profile__r.PointofDelivery__r.SpliceCapacity__c,
                                Billing_Profile__r.PointofDelivery__r.SplicePower__c,
                                Billing_Profile__r.PointofDelivery__r.SpliceType__c,
                                Billing_Profile__r.PointofDelivery__r.Stratus__c,
                                Billing_Profile__r.PointofDelivery__r.StreetName__c,
                                Billing_Profile__r.PointofDelivery__r.Subtype_of_Connection__c,
                                Billing_Profile__r.PointofDelivery__r.TCEstimatedEndTime__c,
                                Billing_Profile__r.PointofDelivery__r.TechnicalZone__c,
                                Billing_Profile__r.PointofDelivery__r.ToiDebt__c,
                                Billing_Profile__r.PointofDelivery__r.TransformerName__c,
                                Billing_Profile__r.PointofDelivery__r.TransformerNumber__c,
                                Billing_Profile__r.PointofDelivery__r.TransformerType__c,
                                Billing_Profile__r.PointofDelivery__r.Transformer_Property__c,
                                Billing_Profile__r.PointofDelivery__r.Transformer_Status__c,
                                Billing_Profile__r.PointofDelivery__r.Typical_Area__c,
                                Billing_Profile__r.PointofDelivery__r.Zone__c,
                                Billing_Profile__r.PointofDelivery__r.legal_collection__c,
                                Billing_Profile__r.PointofDelivery__r.load__c,
                                Asset__r.AccountId,
                                Asset__r.IsCompetitorProduct,
                                Asset__r.CurrencyIsoCode,
                                Asset__r.Name,
                                Asset__r.SerialNumber,
                                Asset__r.InstallDate,
                                Asset__r.PurchaseDate,
                                Asset__r.Status,
                                Asset__r.Price,
                                Asset__r.Quantity,
                                Asset__r.Description,
                                Asset__r.OwnerId,
                                Asset__r.RecordTypeId,
                                Asset__r.IsInternal,
                                Asset__r.AssetLevel,
                                Asset__r.PointofDelivery__c,
                                Asset__r.Company__c,
                                Asset__r.Country__c,
                                Asset__r.ExternalId__c,
                                Asset__r.NE__Status__c,
                                Billing_Profile__r.AccountContract__c,
                                Billing_Profile__r.Company__c,
                                Billing_Profile__r.CurrencyIsoCode,
                                Billing_Profile__r.RecordTypeId,
                                Billing_Profile__r.Account__c,
                                Billing_Profile__r.Address__c,
                                Billing_Profile__r.Type__c,
                                Billing_Profile__r.PointofDelivery__c,
                                Billing_Profile__r.BillingAddress__c,
                                Billing_Profile__r.EDEEnrolment__c,
                                Billing_Profile__r.ExternalID__c,
                                Billing_Profile__r.Delivery_Document_Type__c,
                                Billing_Profile__r.PACity__c,
                                Billing_Profile__r.PAComplement__c,
                                Billing_Profile__r.PANeighbourhood__c,
                                Billing_Profile__r.PANumber__c,
                                Billing_Profile__r.PAPostalCode__c,
                                Billing_Profile__r.PAState__c,
                                Billing_Profile__r.PAStreet__c,
                                Contract__r.AccountId,
                                Contract__r.CNT_Economical_Activity__c,
                                Contract__r.CNT_Economical_Activity_c__c,
                                Contract__r.CNT_EndContract__c,
                                Contract__r.CNT_EndDate__c,
                                Contract__r.CNT_ExternalContract_ID_2__c,
                                Contract__r.CNT_Generation_Capability__c,
                                Contract__r.CNT_Generation_Sources__c,
                                Contract__r.CNT_Generation_Type__c,
                                Contract__r.CNT_GroupArea__c,
                                Contract__r.CNT_GroupClass__c,
                                Contract__r.CNT_GroupNumerCntr__c,
                                Contract__r.CNT_GroupPayType__c,
                                Contract__r.CNT_GroupSegment__c,
                                Contract__r.CNT_GroupTypeContract__c,
                                Contract__r.CNT_Numero_Documento_Beneficio__c,
                                Contract__r.CNT_Tipo_de_Geracao__c,
                                Contract__r.CompanyID__c,
                                Contract__r.Company_ID__c,
                                Contract__r.ContractTerm,
                                Contract__r.Contract_Type__c,
                                Contract__r.CurrencyIsoCode,
                                Contract__r.Description,
                                Contract__r.EndDate,
                                Contract__r.ExternalID__c,
                                Contract__r.Name,
                                Contract__r.Quantity__c,
                                Contract__r.RecordTypeId,
                                Contract__r.StartDate,
                                Contract__r.Status,
                                CurrencyIsoCode,
                                RecordTypeId,
                                Contract__c,
                                Asset__c,
                                Billing_Profile__c,
                                ExternalID__c,
                                Status__c,
                                AccountContract__c,
                                Company__c,
                                GroupAccountContract__c
                        from Contract_Line_Item__c
                        where Billing_Profile__c 
                        in (select id from billing_profile__c where accountcontract__c='{conta_contrato}' and Company__c ='{compania}')";

            bool bContinue = false;

            try
            {
                qr = binding.query(sql);
                bContinue = true;
                //loopCounter = 0;  
            }
            catch (Exception ex)
            {
                //Debugger.Break();
                throw ex;
            }

            while (bContinue)
            {

                if (qr.records == null)
                {
                    bContinue = false;
                    continue;
                }

                for (int i = 0; i < qr.records.Length; i++)
                {
                    List<PointofDelivery__c> lst_pod = new List<PointofDelivery__c>();                    
                    List<Contract_Line_Item__c> lst_cl = new List<Contract_Line_Item__c>();
                    List<Billing_Profile__c> lst_bp = new List<Billing_Profile__c>();
                    List<Contract> lst_cnt = new List<Contract>();                    
                    List<Asset> lst_asset = new List<Asset>();
                    List<ServiceProduct__c> lst_SP = new List<ServiceProduct__c>();
                    Contract_Line_Item__c a = (Contract_Line_Item__c)qr.records[i];

                    a.Billing_Profile__r.PointofDelivery__r.DetailAddress__c = _dados.idAddress;

                    if (string.IsNullOrEmpty(a.Billing_Profile__r.PointofDelivery__r.ExternalId__c))
                    {
                        a.Billing_Profile__r.PointofDelivery__r.ExternalId__c = conta_contrato + "BRA" + (compania == "2003" ? "COE" : "AMA") + "B2C";
                    }

                    lst_pod.Add(a.Billing_Profile__r.PointofDelivery__r);

                    a.Contract__r.CNT_ExternalContract_ID_2__c = conta_contrato;
                    a.Contract__r.AccountId = _dados.idConta;

                    upsertResults = binding_execute.upsert("ExternalId__c", lst_pod.ToArray());

                    _dados.idPod = upsertResults[0].id;
                    _dados.numeroPod = a.Billing_Profile__r.PointofDelivery__r.Name;

                    lst_cnt.Add(a.Contract__r);
                    

                    upsertResults = binding_execute.upsert("ExternalId__c", lst_cnt.ToArray());

                    foreach (var item in upsertResults)
                    {
                        if (!item.success)
                        {
                            a.Contract__r.Status = "Draft";
                            upsertResults = binding_execute.upsert("ExternalId__c", lst_cnt.ToArray());
                        }

                        _dados.idContract = upsertResults[0].id;
                    }

                    a.Asset__r.Contract__c = _dados.idContract;
                    a.Asset__r.AccountId = _dados.idConta;
                    a.Asset__r.PointofDelivery__c = _dados.idPod;

                    lst_asset.Add(a.Asset__r);

                    upsertResults = binding_execute.upsert("ExternalId__c", lst_asset.ToArray());

                    foreach (var item in upsertResults)
                    {
                        if (!item.success)
                        {  
                            upsertResults = binding_execute.upsert("ExternalId__c", lst_asset.ToArray());
                        }

                        _dados.idAsset = upsertResults[0].id;

                        if(!string.IsNullOrWhiteSpace(_dados.idAsset))
                            CarregarAssetAttributes(_dados.numeroPod, compania, _dados.idAsset);
                    }

                    a.Billing_Profile__r.PointofDelivery__c = _dados.idPod;
                    a.Billing_Profile__r.Account__c = _dados.idConta;
                    a.Billing_Profile__r.Address__c = _dados.idAddress;
                    a.Billing_Profile__r.BillingAddress__c = _dados.idAddress;
                    a.Billing_Profile__r.PointofDelivery__r = null;

                    lst_bp.Add(a.Billing_Profile__r);

                    upsertResults = binding_execute.upsert("ExternalId__c", lst_bp.ToArray());

                    foreach (var item in upsertResults)
                    {
                        if (!item.success)
                        {
                            //upsertResults = binding_execute.upsert("ExternalId__c", lst_asset.ToArray());
                        }

                        _dados.idBilling = upsertResults[0].id;
                    }

                    a.Asset__c = _dados.idAsset;
                    a.Contract__c = _dados.idContract;
                    a.Billing_Profile__c = _dados.idBilling;
                    a.Contract__r = null;
                    a.Billing_Profile__r = null;
                    a.Asset__r = null;
                    lst_cl.Add(a);

                    upsertResults = binding_execute.upsert("ExternalId__c", lst_cl.ToArray());

                    foreach (var item in upsertResults)
                    {
                        if (!item.success)
                        {
                            //upsertResults = binding_execute.upsert("ExternalId__c", lst_asset.ToArray());
                        }

                        _dados.idBilling = upsertResults[0].id;
                    }

                    foreach (var item in _dados.lstContatos)
                    {
                        ServiceProduct__c sp = new ServiceProduct__c();
                        sp.Asset__c = _dados.idAsset;
                        sp.Contact__c = item;
                        sp.ExternalId__c = item + conta_contrato;
                        lst_SP.Add(sp);
                    }

                    upsertResults = binding_execute.upsert("ExternalId__c", lst_SP.ToArray());

                    if (qr.done)
                    {
                        bContinue = false;
                    }
                    else
                    {
                        qr = binding.queryMore(qr.queryLocator);
                    }

                }
            }
        }

        private void CargaStreetAddress(string conta_contrato, string compania)
        {
            apex.QueryResult qr = null;
            basicSample_cs_p.apex.UpsertResult[] upsertResultsStreet;
            basicSample_cs_p.apex.UpsertResult[] upsertResultsAddress;

            String sql = string.Format(@"select 
                                        Name,
                                        CurrencyIsoCode,
                                        RecordTypeId,
                                        CompanyID__c,
                                        ExternalId__c,
                                        CoordinateX__c,
                                        CoordinateY__c,
                                        Corner__c,
                                        Department__c,
                                        LongAddress__c,
                                        Number__c,
                                        Addr_id__c,
                                        Postal_Code__c,
                                        Corner_Name__c,
                                        Block__c,
                                        GroupName__c,
                                        GroupType__c,
                                        Inside_Type__c,
                                        Lot__c,
                                        NumerationType__c,
                                        Reference__c,
                                        StreetMD__r.Name,
                                        StreetMD__r.CurrencyIsoCode,
                                        StreetMD__r.RecordTypeId,
                                        StreetMD__r.Municipality__c,
                                        StreetMD__r.Region__c,
                                        StreetMD__r.Street_Type__c,
                                        StreetMD__r.Street__c,
                                        StreetMD__r.Literal_Municipality__c,
                                        StreetMD__r.Literal_region__c,
                                        StreetMD__r.Literal_Street_type__c,
                                        StreetMD__r.Company__c,
                                        StreetMD__r.Country__c,
                                        StreetMD__r.Location__c,
                                        StreetMD__r.StreetExternalId__c,
                                        StreetMD__r.NeighbourhoodCode__c,
                                        StreetMD__r.Neighbourhood__c,
                                        StreetMD__r.LocationCode__c,
                                        StreetMD__r.LocationDesc__c,
                                        StreetMD__r.Strt_id__c,
                                        StreetMD__r.StreetCode__c,
                                        StreetMD__r.IdDepartamento__c,
                                        StreetMD__r.IdNivel__c,
                                        StreetMD__r.Literal_Departament__c,
                                        StreetMD__r.idComuna__c from address__c where id in (select address__c  from billing_profile__c where accountcontract__c='{0}' and Company__c ='{1}')", conta_contrato, compania);

            bool bContinue = false;

            try
            {
                qr = binding.query(sql);
            }
            catch (Exception ex)
            {
                throw ex;
            }

            while (!bContinue)
            {

                if (qr.records == null)
                {
                    bContinue = true;
                    continue;
                }

                for (int i = 0; i < qr.records.Length; i++)
                {
                    List<Address__c> lst_add = new List<Address__c>();
                    List<Street__c> lst_str = new List<Street__c>();
                    Address__c a = (Address__c)qr.records[i];

                    if (a.StreetMD__r != null && string.IsNullOrEmpty(a.StreetMD__r.StreetExternalId__c))
                    {
                        a.StreetMD__r.StreetExternalId__c = compania + conta_contrato;
                    }

                    if (string.IsNullOrEmpty(a.ExternalId__c))
                    { 
                        a.ExternalId__c = compania + conta_contrato;
                    }

                     lst_str.Add(a.StreetMD__r);                                       
                     upsertResultsStreet = binding_execute.upsert("StreetExternalId__c", lst_str.ToArray());
                  

                    string idStreet = upsertResultsStreet[0].id;

                    a.StreetMD__c = idStreet;
                    a.StreetMD__r = null;
                    lst_add.Add(a);

                    String idAddress = "";
                    

                    if (qr.done)
                    {
                        bContinue = true;

                        upsertResultsAddress = binding_execute.upsert("ExternalId__c", lst_add.ToArray());
                        _dados.idAddress = upsertResultsAddress[0].id;
                        //idConta = upsertResultsConta[0].id;
                    }
                    else
                    {
                        qr = binding.queryMore(qr.queryLocator);
                    } 

                }
            }
        }

        private void CriarContaEContato(string conta_contrato, string compania)
        {
            apex.QueryResult qr = null;

            String sql = string.Format(@"select Name,
                    NE__Name__c,
                    Type,
                    RecordTypeId,
                    ParentId,
                    Phone,
                    AccountNumber,
                    IsPartner,
                    IsCustomerPortal,
                    AccountSource,
                    AccountCode__c,
                    Account__c,
                    AdditionalPhone__c,
                    BirthDate__c,
                    BusinessName__c,
                    CompanyCategory__c,
                    CompanyID__c,
                    Executive__c,
                    FathersLastName__c,
                    IdentityNumber__c,
                    IdentityType__c,
                    MainPhone__c,
                    MothersLastName__c,
                    PrimaryEmail__c,
                    SecondaryEmail__c,
                    SecondaryPhone__c,
                    Sector__c,
                    UserTypeCompany__c,
                    Company__c,
                    Country__c,
                    ExternalId__c,
                    Indice__c,
                    CondominiumRUT__c,
                    CondominiumType__c,
                    DetailAddress__c,
                    EconomicActivity__c,
                    IsCorporate__c,
                    OperationalCenter__c,
                    PersonType__c,
                    Allows_the_use_of_data__c,
                    User_sales_channel__c,
                    BillingAddress__c,
                    CountryB2B__c,
                    RUT__c,
                    AccountClientNumber__c,
                    AccountSector__c,
                    PhoneType__c,
                    NoWishtoGiveMail__c,
                    Pais__c,
                    CNT_Account_Type__c,
                    CNT_Municipality_Inscription__c,
                    CNT_Primary_Cell_Phone__c,
                    CNT_State_Inscription__c,
                    CNT_Additional_Email__c,
                    CNT_Client_Type__c,
                    CNT_Executive__c,
                    CNT_Fantasy_Name__c,
                    CNT_Fathers_Full_Name__c,
                    CNT_ID_NUM_2__c,
                    CNT_ID_Type_2__c,
                    CNT_Legal_Type__c,
                    CNT_Mothers_Full_Name__c,
                    CNT_Resp_ID_Number__c,
                    CNT_Resp_ID_Type__c,
                    CNT_Responsible_Email__c,
                    CNT_Responsible_Name__c,
                    CNT_Responsible_Phone__c,
                    CNT_Test_Usuario__c,
                    AcceptENELCommunication__c,
                    CNT_BR_Customer_Accepts_EnelComm__c,
                    CNT_BR_Customer_Accepts_PartnersEnel__c,
                    CNT_CustomerChannel__c,
                    CNT_Email_Refuse__c,
                    CNT_Is_Postal_Address__c,
                    CNT_NumOrgCntr__c,
                    CNT_Account_Executive_Email__c,
                    CNT_Account_Executive_Fax__c,
                    CNT_Account_Executive_Name__c,
                    CNT_Account_Executive_Phone__c,
                    CNT_Responsible_Charge__c,
                    CNT_State_Inscription_Exemption__c,
                    MarketingConsent__c,
                    Privacy__c,
                    ProfilingConsent__c,
                    ThirdPartiesConsent__c,
                    CNT_GroupAssociate__c,
                    UpdatePersonalInfo__c, 

                    (select AccountId,
                    Birthdate,
                    Active__c,
                    AdditionalPhone__c,
                    AgreestoReceiveSurveys__c,
                    Alias__c,
                    Allows_the_use_of_personal_data__c,
                    AssistantPhone,
                    Cargo__c,
                    Civil_Status__c,
                    Cluster__c,
                    CompanyID__c,
                    Company__c,
                    ContactElectrodependent__c,
                    CountryB2B__c,
                    CountryCodeMain__c,
                    CountryCodeSecondary__c,
                    Country__c,
                    CreateCase__c,
                    CurrencyIsoCode,
                    Date_registration_update__c,
                    Departamento__c,
                    Description,
                    Direccion_Laboral__c,
                    Disease__c,
                    DoNotMail__c,
                    DoNotMobileCall__c,
                    DoNotSMS__c,
                    DocumentType2__c,
                    Document_Type__c,
                    Education_Level__c,
                    Electrodependent__c,
                    Email,
                    EmailBouncedDate,
                    EmailBouncedReason,
                    Email_Laboral__c,
                    Extension__c,
                    ExternalId__c,
                    Fax,
                    FirstName,
                    HomePhone,
                    IdentifyCertificateNumberSeries__c,
                    IdentityNumber2__c,
                    IdentityNumberAux__c,
                    IdentityNumber__c,
                    IdentityTypeAux__c,
                    IdentityType__c,
                    Indice__c,
                    InfoMail__c,
                    InfoMobile__c,
                    InfoSMS__c,
                    LastActivityDate__c,
                    LastName,
                    LastNameB2B__c,
                    MCStatus__c,
                    MaritalStatus__c,
                    MiddleName,
                    MobilePhone,
                    Movil_Laboral__c,
                    Nationality__c,
                    NoWishGiveMailPhone__c,
                    NoWishtoGiveMail__c,
                    NoWishtoGiveRUT__c,
                    Notificar_RRSS__c,
                    NotreceiveSMS__c,
                    Occupation__c,
                    OtherPhone,
                    PersonSegment__c,
                    Phone,
                    PhoneType__c,
                    Position__c,
                    PreferredChannelContact__c,
                    PrimerApellido__c,
                    Producto__c,
                    Profession__c,
                    RUT_Empresa_Laboral__c,
                    RUT__c,
                    Razon_Social__c,
                    Reason_of_removal__c,
                    Receive_Adverts__c,
                    RecordTypeId,
                    Record_removal_date__c,
                    Registration_Date__c,
                    RelationshipDetail__c,
                    RepeatedCases__c,
                    ReportsToId,
                    Salutation,
                    Scoring__c,
                    SecondLastName__c,
                    SecondaryChannelContact__c,
                    SecondaryEmail__c,
                    SecondaryPhone__c,
                    SegundoApellido__c,
                    Socioeconomic_Level__c,
                    Telefono_Laboral_Principal__c,
                    Telefono_Laboral_Secundario__c,
                    TertiaryEmail__c,
                    Title,
                    TypeSpecialAttention__c,
                    Type_of_House2__c,
                    UniqueId_Status__c,
                    UniqueId__c,
                    Updatedbyprotocol__c,
                    Used_machine__c,
                    Validated_Email__c,
                    Verification_Code__c from Contacts) 
            from account where id in (select account__c from billing_profile__c where accountcontract__c='{0}' and Company__c ='{1}')", conta_contrato, compania);

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

                for (int i = 0; i < qr.records.Length; i++)
                {
                    List<Account> lst_acc = new List<Account>();
                    Account conta = (Account)qr.records[i];

                    conta.Id = "";
                    conta.Address__c = "";
                    conta.CreatedById = "";
                    conta.LastModifiedById = "";
                    conta.CreatedDate = null;
                    conta.IsCustomerPortal = null;
                    conta.IsDeleted = null;
                    conta.IsPartner = null;
                    conta.LastModifiedDate = null;
                    conta.NE__BillingAddressComplete__c = null;
                    conta.NE__Billing_address_complete_view__c = null;
                    conta.NE__CurrentUserId__c = null;
                    conta.NE__ShippingAddressComplete__c = null;
                    conta.NE__Shipping_address_complete_view__c = null;
                    conta.PhotoUrl = null;
                    conta.SystemModstamp = null;
                    conta.Owner = null;
                    apex.QueryResult qrContatos = conta.Contacts;
                    conta.Contacts = null;
                    conta.LastActivityDate = null;

                    lst_acc.Add(conta);
                    String idConta = "";

                    if (qr.done)
                    {
                        bContinue = false;
                        basicSample_cs_p.apex.UpsertResult[] upsertResultsConta = binding_execute.upsert("ExternalId__c", lst_acc.ToArray());
                        _dados.idConta = upsertResultsConta[0].id;
                    }
                    else
                    {
                        qr = binding.queryMore(qr.queryLocator);
                    }

                    List<Contact> lst_cont = new List<Contact>();

                    for (int x = 0; x < qrContatos.records.Length; x++)
                    {
                        Contact c = (Contact)qrContatos.records[x];
                        c.AccountId = _dados.idConta;
                        lst_cont.Add(c);
                    }

                    basicSample_cs_p.apex.UpsertResult[] upsertResultsContato = binding_execute.upsert("id", lst_cont.ToArray());

                    _dados.lstContatos = new List<string>();

                    foreach (var item in upsertResultsContato)
                    {
                        if (item.success)
                            _dados.lstContatos.Add(item.id);

                        //TODO: log sucesso e fail
                    }
                }
            }
        }


        private void CarregarMedidoresELeituras(string contaContrato, string empresa)
        {
            apex.QueryResult qr = null;
            basicSample_cs_p.apex.UpsertResult[] upsertResultsMedidor;
            basicSample_cs_p.apex.UpsertResult[] upsertResultsLeitura;

            String sql = $@"SELECT 
                                  PointofDelivery__c
                                , MeterNumber__c
                                , ExternalID__c
                                , Status__c
                                , Instalation_date__c
                                , Retirement_date__c
                                , CreatedYear__c
                                , ConstanteATIVAHP__c
                                , ConstanteDMCRHP__c
                                , Cubicle__c
                                , MeasureType__c
                                , MeterType__c
                                , MeterProperty__c
                                , MeterBrand__c
                                , MeterModel__c
                                , ConstanteDEM__c
                                , ConstantePRODIA__c
                                , ConstantePROANT__c
                                , RecordTypeId
                                , (select BillingDate__c
                                        , ConsumptionType__c
                                        , CreatedbyClient__c
                                        , ClientFieldMeasure__c
                                        , MeasureCode__c
                                        , ExternalId__c
                                        , Measure__c
                                        , EventBR__c
                                        , Status__c
                                        , ReadingDate__c
                                        , DateMeasure__c
                                        , FieldMeasure__c
                                        , InvoicedMeasure__c
                                        , Base__c
                                        , ConsumptionValue__c
                                        , Constant__c
                                        , DateNextMeasure__c
                                        , reading_key__c
                                        , RecordTypeId
                                         from MeasuresAndCounters__r)
                            FROM Device__c
                            WHERE PointofDelivery__c in (select PointofDelivery__c from Billing_Profile__c where AccountContract__c = '{contaContrato}' AND Company__c in ('{empresa}'))";

            bool bContinue = false;

            try
            {
                qr = binding.query(sql);
            }
            catch (Exception ex)
            {
                throw ex;
            }

            _dados.lstDevice = new Hashtable();
            while (!bContinue)
            {

                if (qr.records == null)
                {
                    bContinue = true;
                    continue;
                }

                bContinue = qr.done;

                for (int i = 0; i < qr.records.Length; i++)
                {
                    List<Device__c> lstMedidor = new List<Device__c>();
                    Device__c obj = (Device__c)qr.records[i];

                    obj.PointofDelivery__c = _dados.idPod;
                    apex.QueryResult qrLeituras = obj.MeasuresAndCounters__r;

                    obj.MeasuresAndCounters__r = null;
                    obj.Campaign_Members__r = null;
                    obj.Disciplina_de_mercado__r = null;
                    obj.MeasuresAndCounters__r = null;
                    obj.PointofDelivery__r = null;

                    obj.ExternalID__c = string.IsNullOrEmpty(obj.ExternalID__c) ? string.Concat(empresa, obj.MeterNumber__c, _dados.numeroPod) : obj.ExternalID__c;

                    lstMedidor.Add(obj);
                    try
                    {
                        upsertResultsMedidor = binding_execute.upsert("ExternalID__c", lstMedidor.ToArray());

                        List<MeasuresAndCounters__c> lstLeitura = new List<MeasuresAndCounters__c>();
                        _dados.lstDevice.Add(obj.MeterNumber__c, upsertResultsMedidor[0].id);

                        #region --- LEITURAS ---------------------------

                        if (qrLeituras != null)
                        {
                            for (int x = 0; x < qrLeituras.records.Length; x++)
                            {
                                MeasuresAndCounters__c c = (MeasuresAndCounters__c)qrLeituras.records[x];
                                c.PointofDelivery__c = _dados.idPod;
                                c.Measurer__c = upsertResultsMedidor[0].id;
                                lstLeitura.Add(c);
                            }

                            upsertResultsLeitura = binding_execute.upsert("ExternalId__c", lstLeitura.ToArray());
                            qrLeituras = null;
                            lstLeitura.Clear();
                        }

                        #endregion

                    }
                    catch (Exception ex2)
                    {
                        //TODO: remover try/catch
                    }
                }
            }
        }


        /// <summary>
        /// Migra faturas a partir de Janeiro-2022
        /// </summary>
        /// <param name="contaContrato"></param>
        /// <param name="empresa"></param>
        private void CarregarFaturasConsumoPagamentos(string contaContrato, string empresa)
        {
            apex.QueryResult qr = null;
            basicSample_cs_p.apex.UpsertResult[] upsertResultsLeitura;
            basicSample_cs_p.apex.UpsertResult[] upsertResultsConsumo;
            basicSample_cs_p.apex.UpsertResult[] upsertResultsPagamento;

            String sql = $@"select RUT__c
                            , InvoiceReference__c
                            , DocumentNumber__c
                            , NameOwnerInvoice__c
                            , PaymentOutOfDate__c
                            , InvoicePay__c
                            , InvoiceExpired__c
                            , Status__c
                            , CNR__c
                            , TOI__c
                            , ReadingNote__c
                            , EUSDAmount__c
                            , SegmentType__c
                            , ConsumptionKwh__c
                            , EmissionDate__c
                            , ExpirationDate__c
                            , SecondExpirationDate__c
                            , CurrentReadingDate__c
                            , TotalAmountBilled__c
                            , AgreementAmount__c
                            , Agreement__c
                            , OtherFees__c
                            , Interests__c
                            , Taxes__c
                            , Flag__c
                            , PublicLightingCharge__c
                            , TypeOfDelivery__c
                            , TypeOfDeliveryFailed__c
                            , (select ExternalId__c
                                        from MeasuresAndCounters__r)
                            , (Select ExternalId__c
	                            , RecordTypeId
	                            , CurrencyIsoCode
	                            , DeviceNumber__c
	                            , Event_Date__c
	                            , InvoicedConsumption__c
	                            , MeasureType__c
	                            , SegmentType__c
                                , Consumption_Type__c
                              from Consumptions__r)
                            , (select ExternalId__c
                                    , DocumentNumber__c
                                    , Status__c
                                    , Date__c
                                    , End_Date__c
                                    , PaymentDateUpdate__c
                                    , SegmentType__c
                                    , PaymentInfo__c
                                    , Concept__c
                                    , DocumentType__c
                                    , AmountPaidDIV__c
                                    , Branch_Office__c
                                    , PayChannel__c
                                    , AmountSC__c
                                    , OtherDebits__c
                                    , Interests__c
                                    , Tax__c
	                            from Payments__r)
                            from Invoice__c 
                            where PointofDelivery__c in (select PointofDelivery__c from Billing_Profile__c where AccountContract__c = '{contaContrato}' AND Company__c in ('{empresa}'))
                            and InvoiceReference__c > '202201'";

            bool bContinue = false;

            try
            {
                qr = binding.query(sql);
            }
            catch (Exception ex)
            {
                throw ex;
            }

            _dados.lstInvoice = new List<string>();
            while (!bContinue)
            {

                if (qr.records == null)
                {
                    bContinue = true;
                    continue;
                }
                bContinue = qr.done;

                for (int i = 0; i < qr.records.Length; i++)
                {
                    List<Invoice__c> lstFatura = new List<Invoice__c>();
                    List<Consumption__c> lstConsumo = new List<Consumption__c>();
                    List<MeasuresAndCounters__c> lstLeitura = new List<MeasuresAndCounters__c>();
                    List<Payment__c> lstPagamento = new List<Payment__c>();
                    
                    Invoice__c f = (Invoice__c)qr.records[i];

                    f.PointofDelivery__c = _dados.idPod;
                    f.NameOwnerInvoice__c = _dados.idConta;
                    f.Account_del__r = null;
                    f.Address__r = null;
                    f.Billing_Adjustments__r = null;
                    f.Campaigns__r = null;
                    f.Cases__r = null;
                    f.Contact__r = null;
                    f.Miembros_de_campa_a__r = null;
                    f.NameOwnerInvoice__r = null;
                    f.Or_amentos__r = null;

                    apex.QueryResult qrLeituras = f.MeasuresAndCounters__r;
                    apex.QueryResult qrConsumos = f.Consumptions__r;
                    apex.QueryResult qrPagamentos = f.Payments__r;

                    f.MeasuresAndCounters__r = null;
                    f.Consumptions__r = null;
                    f.Payments__r = null;

                    lstFatura.Add(f);
                    try
                    {
                        basicSample_cs_p.apex.UpsertResult[] upsertResultsConta = binding_execute.upsert("RUT__c", lstFatura.ToArray());

                        _dados.lstInvoice.Add(upsertResultsConta[0].id);
                        f.Id = _dados.lstInvoice.Last();

                        #region --- LEITURAS ---------------------------

                        if (qrLeituras == null)
                            goto Consumos; 

                        for (int x = 0; x < qrLeituras.records.Length; x++)
                        {
                            MeasuresAndCounters__c l = (MeasuresAndCounters__c)qrLeituras.records[x];
                            l.Invoice__c = f.Id;

                            lstLeitura.Add(l);
                        }

                        if (lstLeitura.Count > 0)
                        {
                            upsertResultsLeitura = binding_execute.upsert("ExternalId__c", lstLeitura.ToArray());
                            qrLeituras = null;
                            lstLeitura.Clear();
                        }

                    #endregion


                        #region --- CONSUMOS ---------------------------

                        Consumos:
                            if (qrConsumos == null)
                                goto Pagamentos;

                            for (int x = 0; x < qrConsumos.records.Length; x++)
                            {
                                Consumption__c c = (Consumption__c)qrConsumos.records[x];
                                c.PointofDelivery__c = _dados.idPod;
                                c.Invoice__c = f.Id;
                            
                                lstConsumo.Add(c);
                            }

                            if (lstConsumo.Count > 0)
                            {
                                upsertResultsConsumo = binding_execute.upsert("ExternalId__c", lstConsumo.ToArray());
                                qrConsumos = null;
                                lstConsumo.Clear();
                            }

                        #endregion


                        #region --- PAGAMENTOS ---------------------------

                        Pagamentos:
                            if (qrPagamentos == null)
                                continue;

                            for (int x = 0; x < qrPagamentos.records.Length; x++)
                            {
                                Payment__c p = (Payment__c)qrPagamentos.records[x];
                                p.Account__r = null;
                                p.PointofDelivery__r = null;
                                p.Invoice__r = null;

                                p.Invoice__c = f.Id;
                                p.PointofDelivery__c = _dados.idPod;
                                p.Account__c = _dados.idConta;
                            
                                lstPagamento.Add(p);
                            }

                            upsertResultsPagamento = binding_execute.upsert("ExternalId__c", lstPagamento.ToArray());
                            qrPagamentos = null;
                            lstPagamento.Clear();

                            #endregion

                    }
                    catch (Exception ex2)
                    { 
                        //TODO: remover try/catch
                    }
                }
            }
        }

        private void CarregarParcelamentos(string instalacao, string empresa)
        {
            apex.QueryResult qr = null;

            String sql = $@"SELECT  Name
                                    , AgreementTypePE__c
                                    , AgreementStatus__c
                                    , AgreementOption__c
                                    , StartDate__c
                                    , ExpirationDate__c
                                    , UnsubscriptionDate__c
                                    , AgreementURL__c
                                    , AgreementExternalID__c
                                    , CreatedUser__c
                                    , TermUser__c
                                    , CancellationReason__c
                                    , Rate__c
                                    , NameOfRequester__c
                                    , NumberOfQuotes__c
                                    , FirstPayment__c
                                    , CurrentFee__c
                                    , QuoteValue__c
                                    , ActualQuote__c
                                    , PendingDebt__c
                                    , TaxInterest__c
                                    , TotalInterests__c
                                    , LastQuote__c
                                    , TotalQuotes__c
                            FROM Agreement__c 
                            WHERE PointofDelivery__c in (select PointofDelivery__c from Billing_Profile__c where PointofDelivery__r.Name = '{instalacao}' AND Company__c in ('{empresa}'))";

            bool bContinue = false;

            try
            {
                qr = binding.query(sql);
            }
            catch (Exception ex)
            {
                throw ex;
            }

            _dados.lstDevice = new Hashtable();
            while (!bContinue)
            {

                if (qr.records == null)
                {
                    bContinue = true;
                    continue;
                }

                bContinue = qr.done;

                //_dados.lstParcelamento = new List<string>();

                for (int i = 0; i < qr.records.Length; i++)
                {
                    List<Agreement__c> lstObject = new List<Agreement__c>();
                    Agreement__c obj = (Agreement__c)qr.records[i];

                    obj.PointofDelivery__c = _dados.idPod;
                    obj.Account__c = _dados.idConta;

                    lstObject.Add(obj);
                    try
                    {
                        basicSample_cs_p.apex.UpsertResult[] upsertResults = binding_execute.upsert("AgreementExternalID__c", lstObject.ToArray());

                        //List<Agreement__c> lstLeitura = new List<Agreement__c>();
                        //_dados.lstParcelamento.Add(upsertResults[0].id);
                    }
                    catch (Exception ex2)
                    { 
                        //TODO: remover try/catch
                    }
                }
            }
        }



        private void CarregarOrder(string contaContrato, string codigoEmpresa)
        {
            apex.QueryResult qr = null;
            apex.QueryResult qrExistentes = null;

            String sql = $@"SELECT
                                  Id
                                , CNT_Case__c
                                , CNT_ContractStatus__c
                                , CNT_isOwnershipChange__c
                                , CNT_Quote__c
                                , Company__c
                                , Country__c
                                , CurrencyIsoCode
                                , NE__AccountId__c
                                , NE__AssetEnterpriseCalc__c
                                , NE__AssetStatusCalc__c
                                , NE__ConfigurationStatus__c
                                , NE__Order_date__c
                                , NE__OrderStatus__c
                                , NE__rTypeName__c
                                , NE__Type__c
                                , NE__Url_Payback__c
                                , NE__Version__c
                                , Numero_de_Autorizaci_n_Redeban__c
                                , PoD__c
                                , Point_of_delivery__c
                                , Print__c
                                , RecordTypeAUX__c
                                , RecordTypeId
                                , Step_1__c
                                , Step_2__c
                                , Tipo_de_persona__c
                            FROM NE__Order__c 
                            WHERE Billing_profile__c  in (select Id from Billing_Profile__c where AccountContract__c = '{contaContrato}' AND Company__c = '{codigoEmpresa}')";

            bool bContinue = false;

            try
            {
                qrExistentes = binding_execute.query(sql);
                qr = binding.query(sql);
            }
            catch (Exception ex)
            {
                throw ex;
            }

            _dados.lstAssetAttr = new List<string>();

            #region ##--- APAGAR EXISTENTES ----------------------------
            List<string> lstApagar = new List<string>();
            while (!bContinue)
            {
                if (qrExistentes.records == null)
                {
                    bContinue = true;
                    continue;
                }

                bContinue = qrExistentes.done;

                for (int i = 0; i < qrExistentes.records.Length; i++)
                {
                    NE__Order__c obj = (NE__Order__c)qrExistentes.records[i];
                    lstApagar.Add(obj.Id);
                }

                try
                {
                    basicSample_cs_p.apex.DeleteResult[] delResults = binding_execute.delete(lstApagar.ToArray());
                }
                catch (Exception ex2)
                {
                    //TODO: remover try/catch
                }
                finally
                {
                    lstApagar = null;
                    qrExistentes = null;
                }
            }
            #endregion

            bContinue = false;
            while (!bContinue)
            {

                if (qr.records == null)
                {
                    bContinue = true;
                    continue;
                }

                bContinue = qr.done;

                for (int i = 0; i < qr.records.Length; i++)
                {
                    List<NE__Order__c> lstObject = new List<NE__Order__c>();

                    NE__Order__c obj = (NE__Order__c)qr.records[i];
                    obj.Id = null;
                    obj.Address__c = _dados.idAddress;
                    obj.Billing_profile__c = _dados.idBilling;
                    obj.NE__BillAccId__c = _dados.idConta;

                    lstObject.Add(obj);

                    //apex.QueryResult qrAtributosAsset = obj.NE__AssetItemAttributes__r;

                    //if (qrAtributosAsset == null)
                    //    continue;

                    //for (int x = 0; x < qrAtributosAsset.records.Length; x++)
                    //{
                    //    NE__AssetItemAttribute__c at = (NE__AssetItemAttribute__c)qrAtributosAsset.records[x];
                    //    at.NE__Asset__c = idAssetNovo;

                    //    lstObject.Add(at);
                    //}

                    try
                    {
                        basicSample_cs_p.apex.SaveResult[] upsertResults = binding_execute.create(lstObject.ToArray());
                    }
                    catch (Exception ex2)
                    {
                        //TODO: remover try/catch
                    }
                }
            }
        }



        private void CarregarAssetAttributes(string instalacao, string codigoEmpresa, string idAssetNovo)
        {
            apex.QueryResult qr = null;

            String sql = $@"SELECT  ExternalId__c 
                                    , (SELECT Name
                                            , NE__Value__c
                                            , NE__Old_Value__c
                                            , CurrencyIsoCode
                                            , NE__Action__c
                                            , NE__AttrEnterpriseId__c
                                            , NE__FamPropExtId__c
                                       FROM NE__AssetItemAttributes__r )
                                    FROM Asset 
                            WHERE PointofDelivery__r.Name = '{instalacao}' and PointofDelivery__r.CompanyID__c = '{codigoEmpresa}'";

            bool bContinue = false;

            try
            {
                qr = binding.query(sql);
            }
            catch (Exception ex)
            {
                throw ex;
            }

            //TODO:  apagar atributos existentese carregar novos
            //TODO:  objeto nao possui external id nem campo indexado para upsert

            _dados.lstAssetAttr = new List<string>();
            while (!bContinue)
            {

                if (qr.records == null)
                {
                    bContinue = true;
                    continue;
                }

                bContinue = qr.done;

                for (int i = 0; i < qr.records.Length; i++)
                {
                    List<NE__AssetItemAttribute__c> lstObject = new List<NE__AssetItemAttribute__c>();
                    Asset obj = (Asset)qr.records[i];

                    apex.QueryResult qrAtributosAsset = obj.NE__AssetItemAttributes__r;

                    if (qrAtributosAsset == null)
                        continue;

                    for (int x = 0; x < qrAtributosAsset.records.Length; x++)
                    {
                        NE__AssetItemAttribute__c at = (NE__AssetItemAttribute__c)qrAtributosAsset.records[x];
                        at.NE__Asset__c = idAssetNovo;

                        lstObject.Add(at);
                    }

                    try
                    {
                        var upsertResults = binding_execute.create(lstObject.ToArray());
                    }
                    catch (Exception ex2)
                    {
                        //TODO: remover try/catch
                    }
                }
            }
        }

        private void UpsertCL()
        {
            String data = DateTime.Now.ToString("yyyyMMddHHmmss");

            if (!loggedIn)
            {
                if (!login())
                    return;
            }

            StreamReader arquivo = new StreamReader(caminho);
            List<Contract_Line_Item__c> lst_billing = new List<Contract_Line_Item__c>();
            List<Contract_Line_Item__c> lst_billing2 = new List<Contract_Line_Item__c>();
            List<Contract_Line_Item__c> lst_billing3 = new List<Contract_Line_Item__c>();
            List<Contract_Line_Item__c> lst_billing4 = new List<Contract_Line_Item__c>();
            List<Contract_Line_Item__c> lst_billing5 = new List<Contract_Line_Item__c>();
            List<Contract_Line_Item__c> lst_billing6 = new List<Contract_Line_Item__c>();

            StringBuilder acerto = new StringBuilder();
            StringBuilder acerto2 = new StringBuilder();
            StringBuilder acerto3 = new StringBuilder();
            StringBuilder acerto4 = new StringBuilder();
            StringBuilder acerto5 = new StringBuilder();
            StringBuilder acerto6 = new StringBuilder();

            StringBuilder erro = new StringBuilder();
            StringBuilder erro2 = new StringBuilder();
            StringBuilder erro3 = new StringBuilder();
            StringBuilder erro4 = new StringBuilder();
            StringBuilder erro5 = new StringBuilder();
            StringBuilder erro6 = new StringBuilder();

            List<String> lista_processada = new List<String>();
            List<String> lista_processada2 = new List<String>();
            List<String> lista_processada3 = new List<String>();
            List<String> lista_processada4 = new List<String>();
            List<String> lista_processada5 = new List<String>();
            List<String> lista_processada6 = new List<String>();

            while (!arquivo.EndOfStream)
            {
                String _recebido = arquivo.ReadLine();
                String[] linha = _recebido.Split('\t');


                Asset _ativo = new Asset();
                _ativo.ExternalId__c = linha[3].ToString();

                Billing_Profile__c _ponto = new Billing_Profile__c();
                _ponto.ExternalID__c = linha[4].ToString();

                Contract _conta = new Contract();
                _conta.ExternalID__c = linha[2].ToString();

                Contract_Line_Item__c _obj = new Contract_Line_Item__c();
                _obj.Contract__r = _conta;
                _obj.ExternalID__c  = linha[0].ToString();
                _obj.Asset__r = _ativo;
                _obj.Billing_Profile__r = _ponto;
                _obj.AccountContract__c = linha[7].ToString();
                _obj.Company__c = linha[8].ToString();
                _obj.CurrencyIsoCode = "BRL";
                _obj.RecordTypeId = "01236000001DjVNAA0";



                if (lst_billing.Count < 200)
                {
                    lst_billing.Add(_obj);
                    lista_processada.Add(_recebido);
                }
                else if (lst_billing2.Count < 200)
                {
                    lst_billing2.Add(_obj);
                    lista_processada2.Add(_recebido);
                }
                else if (lst_billing3.Count < 200)
                {
                    lst_billing3.Add(_obj);
                    lista_processada3.Add(_recebido);
                }
                else if (lst_billing4.Count < 200)
                {
                    lst_billing4.Add(_obj);
                    lista_processada4.Add(_recebido);
                }
                else if (lst_billing5.Count < 200)
                {
                    lst_billing5.Add(_obj);
                    lista_processada5.Add(_recebido);
                }
                else if (lst_billing6.Count < 200)
                {
                    lst_billing6.Add(_obj);
                    lista_processada6.Add(_recebido);
                }
                else
                {
                    EscreverArquivo("Avanco_CL_" + data, lst_billing6.ToArray()[199].ExternalID__c + "\n");

                    try
                    {
                        Parallel.Invoke(
                        () =>
                        {
                            basicSample_cs_p.apex.UpsertResult[] upsertResults = binding.upsert("ExternalId__c", lst_billing.ToArray());

                            for (int i = 0; i < upsertResults.Length; i++)
                            {
                                basicSample_cs_p.apex.UpsertResult result = upsertResults[i];

                                if (result.success)
                                {
                                    //Console.WriteLine("\nUpsert succeeded.");
                                    acerto.AppendLine(lista_processada.ToArray()[i].ToString() + ";" + (result.created ? "Inserted" : "Updated") + ";" + result.id);
                                }
                                else
                                {
                                    //Console.WriteLine("The Upsert failed because: " + result.errors[0].message);
                                    erro.AppendLine(lista_processada.ToArray()[i].ToString() + ";" + result.errors[0].message);
                                }
                            }
                        },
                        () =>
                        {
                            basicSample_cs_p.apex.UpsertResult[] upsertResults = binding.upsert("ExternalId__c", lst_billing2.ToArray());

                            for (int i = 0; i < upsertResults.Length; i++)
                            {
                                basicSample_cs_p.apex.UpsertResult result = upsertResults[i];

                                if (result.success)
                                {
                                    //Console.WriteLine("\nUpsert succeeded.");
                                    acerto2.AppendLine(lista_processada2.ToArray()[i].ToString() + ";" + (result.created ? "Inserted" : "Updated") + ";" + result.id);
                                }
                                else
                                {
                                    //Console.WriteLine("The Upsert failed because: " + result.errors[0].message);
                                    erro2.AppendLine(lista_processada2.ToArray()[i].ToString() + ";" + result.errors[0].message);
                                }
                            }
                        },
                        () =>
                        {
                            basicSample_cs_p.apex.UpsertResult[] upsertResults = binding.upsert("ExternalId__c", lst_billing3.ToArray());

                            for (int i = 0; i < upsertResults.Length; i++)
                            {
                                basicSample_cs_p.apex.UpsertResult result = upsertResults[i];

                                if (result.success)
                                {
                                    //Console.WriteLine("\nUpsert succeeded.");
                                    acerto3.AppendLine(lista_processada3.ToArray()[i].ToString() + ";" + (result.created ? "Inserted" : "Updated") + ";" + result.id);
                                }
                                else
                                {
                                    //Console.WriteLine("The Upsert failed because: " + result.errors[0].message);
                                    erro3.AppendLine(lista_processada3.ToArray()[i].ToString() + ";" + result.errors[0].message);
                                }
                            }
                        },
                        () =>
                        {
                            basicSample_cs_p.apex.UpsertResult[] upsertResults = binding.upsert("ExternalId__c", lst_billing4.ToArray());

                            for (int i = 0; i < upsertResults.Length; i++)
                            {
                                basicSample_cs_p.apex.UpsertResult result = upsertResults[i];

                                if (result.success)
                                {
                                    //Console.WriteLine("\nUpsert succeeded.");
                                    acerto4.AppendLine(lista_processada4.ToArray()[i].ToString() + ";" + (result.created ? "Inserted" : "Updated") + ";" + result.id);
                                }
                                else
                                {
                                    //Console.WriteLine("The Upsert failed because: " + result.errors[0].message);
                                    erro4.AppendLine(lista_processada4.ToArray()[i].ToString() + ";" + result.errors[0].message);
                                }
                            }
                        },
                        () =>
                        {
                            basicSample_cs_p.apex.UpsertResult[] upsertResults = binding.upsert("ExternalId__c", lst_billing5.ToArray());

                            for (int i = 0; i < upsertResults.Length; i++)
                            {
                                basicSample_cs_p.apex.UpsertResult result = upsertResults[i];

                                if (result.success)
                                {
                                    //Console.WriteLine("\nUpsert succeeded.");
                                    acerto5.AppendLine(lista_processada5.ToArray()[i].ToString() + ";" + (result.created ? "Inserted" : "Updated") + ";" + result.id);
                                }
                                else
                                {
                                    //Console.WriteLine("The Upsert failed because: " + result.errors[0].message);
                                    erro5.AppendLine(lista_processada5.ToArray()[i].ToString() + ";" + result.errors[0].message);
                                }
                            }
                        },
                        () =>
                        {
                            basicSample_cs_p.apex.UpsertResult[] upsertResults = binding.upsert("ExternalId__c", lst_billing6.ToArray());

                            for (int i = 0; i < upsertResults.Length; i++)
                            {
                                basicSample_cs_p.apex.UpsertResult result = upsertResults[i];

                                if (result.success)
                                {
                                    //Console.WriteLine("\nUpsert succeeded.");
                                    acerto6.AppendLine(lista_processada6.ToArray()[i].ToString() + ";" + (result.created ? "Inserted" : "Updated") + ";" + result.id);
                                }
                                else
                                {
                                    //Console.WriteLine("The Upsert failed because: " + result.errors[0].message);
                                    erro6.AppendLine(lista_processada6.ToArray()[i].ToString() + ";" + result.errors[0].message);
                                }
                            }
                        });

                        EscreverArquivo("resultado_positivo_cl_" + data, acerto.ToString()); //+ "\n" + acerto2.ToString() + "\n" + acerto3.ToString() + "\n" + acerto4.ToString() + "\n" + acerto5.ToString() + "\n" + acerto6.ToString());
                        EscreverArquivo("resultado_positivo_cl_" + data, acerto2.ToString());
                        EscreverArquivo("resultado_positivo_cl_" + data, acerto3.ToString());
                        EscreverArquivo("resultado_positivo_cl_" + data, acerto4.ToString());
                        EscreverArquivo("resultado_positivo_cl_" + data, acerto5.ToString());
                        EscreverArquivo("resultado_positivo_cl_" + data, acerto6.ToString());

                        EscreverArquivo("resultado_negativo_cl_" + data, erro.ToString());// + "\n" + erro2.ToString() + "\n" + erro3.ToString() + "\n" + erro4.ToString() + "\n" + erro5.ToString() + "\n" + erro6.ToString());
                        EscreverArquivo("resultado_negativo_cl_" + data, erro2.ToString());
                        EscreverArquivo("resultado_negativo_cl_" + data, erro3.ToString());
                        EscreverArquivo("resultado_negativo_cl_" + data, erro4.ToString());
                        EscreverArquivo("resultado_negativo_cl_" + data, erro5.ToString());
                        EscreverArquivo("resultado_negativo_cl_" + data, erro6.ToString());

                        lst_billing.Clear();
                        lst_billing2.Clear();
                        lst_billing3.Clear();
                        lst_billing4.Clear();
                        lst_billing5.Clear();
                        lst_billing6.Clear();

                        acerto.Clear();
                        acerto2.Clear();
                        acerto3.Clear();
                        acerto4.Clear();
                        acerto5.Clear();
                        acerto6.Clear();

                        erro.Clear();
                        erro2.Clear();
                        erro3.Clear();
                        erro4.Clear();
                        erro5.Clear();
                        erro6.Clear();

                        lista_processada.Clear();
                        lista_processada2.Clear();
                        lista_processada3.Clear();
                        lista_processada4.Clear();
                        lista_processada5.Clear();
                        lista_processada6.Clear();

                    }
                    catch (Exception)
                    {
                        EscreverArquivo("Erros_CL_" + data, lst_billing.ToArray()[0].ExternalID__c + " até " + lst_billing6.ToArray()[199].ExternalID__c + "\n");
                    }
                }

            }

            try
            {
                //EscreverArquivo("Avanco_billing_" + data, lst_billing6.ToArray()[99].Id + "\n");

                Parallel.Invoke(
                () =>
                {
                    basicSample_cs_p.apex.UpsertResult[] upsertResults = binding.upsert("ExternalId__c", lst_billing.ToArray());

                    for (int i = 0; i < upsertResults.Length; i++)
                    {
                        basicSample_cs_p.apex.UpsertResult result = upsertResults[i];

                        if (result.success)
                        {
                            //Console.WriteLine("\nUpsert succeeded.");
                            acerto.AppendLine(lista_processada.ToArray()[i].ToString() + ";" + (result.created ? "Inserted" : "Updated") + ";" + result.id);
                        }
                        else
                        {
                            //Console.WriteLine("The Upsert failed because: " + result.errors[0].message);
                            erro.AppendLine(lista_processada.ToArray()[i].ToString() + ";" + result.errors[0].message);
                        }
                    }
                },
                   () =>
                   {
                       basicSample_cs_p.apex.UpsertResult[] upsertResults = binding.upsert("ExternalId__c", lst_billing2.ToArray());

                       for (int i = 0; i < upsertResults.Length; i++)
                       {
                           basicSample_cs_p.apex.UpsertResult result = upsertResults[i];

                           if (result.success)
                           {
                               //Console.WriteLine("\nUpsert succeeded.");
                               acerto2.AppendLine(lista_processada2.ToArray()[i].ToString() + ";" + (result.created ? "Inserted" : "Updated") + ";" + result.id);
                           }
                           else
                           {
                               //Console.WriteLine("The Upsert failed because: " + result.errors[0].message);
                               erro2.AppendLine(lista_processada2.ToArray()[i].ToString() + ";" + result.errors[0].message);
                           }
                       }
                   },
                   () =>
                   {
                       basicSample_cs_p.apex.UpsertResult[] upsertResults = binding.upsert("ExternalId__c", lst_billing3.ToArray());

                       for (int i = 0; i < upsertResults.Length; i++)
                       {
                           basicSample_cs_p.apex.UpsertResult result = upsertResults[i];

                           if (result.success)
                           {
                               //Console.WriteLine("\nUpsert succeeded.");
                               acerto3.AppendLine(lista_processada3.ToArray()[i].ToString() + ";" + (result.created ? "Inserted" : "Updated") + ";" + result.id);
                           }
                           else
                           {
                               //Console.WriteLine("The Upsert failed because: " + result.errors[0].message);
                               erro3.AppendLine(lista_processada3.ToArray()[i].ToString() + ";" + result.errors[0].message);
                           }
                       }
                   },
                   () =>
                   {
                       basicSample_cs_p.apex.UpsertResult[] upsertResults = binding.upsert("ExternalId__c", lst_billing4.ToArray());

                       for (int i = 0; i < upsertResults.Length; i++)
                       {
                           basicSample_cs_p.apex.UpsertResult result = upsertResults[i];

                           if (result.success)
                           {
                               //Console.WriteLine("\nUpsert succeeded.");
                               acerto4.AppendLine(lista_processada4.ToArray()[i].ToString() + ";" + (result.created ? "Inserted" : "Updated") + ";" + result.id);
                           }
                           else
                           {
                               //Console.WriteLine("The Upsert failed because: " + result.errors[0].message);
                               erro4.AppendLine(lista_processada4.ToArray()[i].ToString() + ";" + result.errors[0].message);
                           }
                       }
                   },
                   () =>
                   {
                       basicSample_cs_p.apex.UpsertResult[] upsertResults = binding.upsert("ExternalId__c", lst_billing5.ToArray());

                       for (int i = 0; i < upsertResults.Length; i++)
                       {
                           basicSample_cs_p.apex.UpsertResult result = upsertResults[i];

                           if (result.success)
                           {
                               //Console.WriteLine("\nUpsert succeeded.");
                               acerto5.AppendLine(lista_processada5.ToArray()[i].ToString() + ";" + (result.created ? "Inserted" : "Updated") + ";" + result.id);
                           }
                           else
                           {
                               //Console.WriteLine("The Upsert failed because: " + result.errors[0].message);
                               erro5.AppendLine(lista_processada5.ToArray()[i].ToString() + ";" + result.errors[0].message);
                           }
                       }
                   },
                   () =>
                   {
                       basicSample_cs_p.apex.UpsertResult[] upsertResults = binding.upsert("ExternalId__c", lst_billing6.ToArray());

                       for (int i = 0; i < upsertResults.Length; i++)
                       {
                           basicSample_cs_p.apex.UpsertResult result = upsertResults[i];

                           if (result.success)
                           {
                               //Console.WriteLine("\nUpsert succeeded.");
                               acerto6.AppendLine(lista_processada6.ToArray()[i].ToString() + ";" + (result.created ? "Inserted" : "Updated") + ";" + result.id);
                           }
                           else
                           {
                               //Console.WriteLine("The Upsert failed because: " + result.errors[0].message);
                               erro6.AppendLine(lista_processada6.ToArray()[i].ToString() + ";" + result.errors[0].message);
                           }
                       }
                   });

                EscreverArquivo("resultado_positivo_cl_" + data, acerto.ToString() + "\n" + acerto2.ToString() + "\n" + acerto3.ToString() + "\n" + acerto4.ToString() + "\n" + acerto5.ToString() + "\n" + acerto6.ToString());

                EscreverArquivo("resultado_negativo_cl_" + data, erro.ToString() + "\n" + erro2.ToString() + "\n" + erro3.ToString() + "\n" + erro4.ToString() + "\n" + erro5.ToString() + "\n" + erro6.ToString());

                lst_billing.Clear();
                lst_billing2.Clear();
                lst_billing3.Clear();
                lst_billing4.Clear();
                lst_billing5.Clear();
                lst_billing6.Clear();

                acerto.Clear();
                acerto2.Clear();
                acerto3.Clear();
                acerto4.Clear();
                acerto5.Clear();
                acerto6.Clear();

                erro.Clear();
                erro2.Clear();
                erro3.Clear();
                erro4.Clear();
                erro5.Clear();
                erro6.Clear();


            }
            catch (Exception e)
            {
                EscreverArquivo("resultado_positivo_cl_" + data, acerto.ToString() + "\n" + acerto2.ToString() + "\n" + acerto3.ToString() + "\n" + acerto4.ToString() + "\n" + acerto5.ToString() + "\n" + acerto6.ToString());

                EscreverArquivo("resultado_negativo_cl_" + data, erro.ToString() + "\n" + erro2.ToString() + "\n" + erro3.ToString() + "\n" + erro4.ToString() + "\n" + erro5.ToString() + "\n" + erro6.ToString());

                EscreverArquivo("Erros_billing_" + data, e.Message + "\n");
            }

            Console.WriteLine("Fim do Programa");
            Console.ReadLine();
        }
        private void UpsertBillingsProfiles()
        {
            String data = DateTime.Now.ToString("yyyyMMddHHmmss");

            if (!loggedIn)
            {
                if (!login())
                    return;
            }

            StreamReader arquivo = new StreamReader(caminho);
            List<Billing_Profile__c> lst_billing = new List<Billing_Profile__c>();
            List<Billing_Profile__c> lst_billing2 = new List<Billing_Profile__c>();
            List<Billing_Profile__c> lst_billing3 = new List<Billing_Profile__c>();
            List<Billing_Profile__c> lst_billing4 = new List<Billing_Profile__c>();
            List<Billing_Profile__c> lst_billing5 = new List<Billing_Profile__c>();
            List<Billing_Profile__c> lst_billing6 = new List<Billing_Profile__c>();

            StringBuilder acerto = new StringBuilder();
            StringBuilder acerto2 = new StringBuilder();
            StringBuilder acerto3 = new StringBuilder();
            StringBuilder acerto4 = new StringBuilder();
            StringBuilder acerto5 = new StringBuilder();
            StringBuilder acerto6 = new StringBuilder();

            StringBuilder erro = new StringBuilder();
            StringBuilder erro2 = new StringBuilder();
            StringBuilder erro3 = new StringBuilder();
            StringBuilder erro4 = new StringBuilder();
            StringBuilder erro5 = new StringBuilder();
            StringBuilder erro6 = new StringBuilder();

            List<String> lista_processada = new List<String>();
            List<String> lista_processada2 = new List<String>();
            List<String> lista_processada3 = new List<String>();
            List<String> lista_processada4 = new List<String>();
            List<String> lista_processada5 = new List<String>();
            List<String> lista_processada6 = new List<String>();

            while (!arquivo.EndOfStream)
            {
                String _recebido = arquivo.ReadLine();
                String[] linha = _recebido.Split(';');


                Address__c _endereco = new Address__c();
                _endereco.ExternalId__c = linha[2].ToString();

                PointofDelivery__c _ponto = new PointofDelivery__c();
                _ponto.ExternalId__c = linha[4].ToString();

                Account _conta = new Account();
                _conta.ExternalId__c = linha[0].ToString();

                Billing_Profile__c _obj = new Billing_Profile__c();
                _obj.Account__r = _conta;
                _obj.Type__c = linha[1].ToString();
                _obj.Address__r = _endereco;
                _obj.BillingAddress__r = _endereco;
                _obj.ExternalID__c = linha[3].ToString();
                _obj.PointofDelivery__r = _ponto;
                _obj.AccountContract__c = linha[5].ToString();
                _obj.DeliveryType__c = linha[6].ToString();
                _obj.CNT_Due_Date__c = linha[7].ToString();
                _obj.Company__c = linha[8].ToString();
                _obj.RecordTypeId = "01236000000On9BAAS";



                if (lst_billing.Count < 100)
                {
                    lst_billing.Add(_obj);
                    lista_processada.Add(_recebido);
                }
                else if (lst_billing2.Count < 100)
                {
                    lst_billing2.Add(_obj);
                    lista_processada2.Add(_recebido);
                }
                else if (lst_billing3.Count < 100)
                {
                    lst_billing3.Add(_obj);
                    lista_processada3.Add(_recebido);
                }
                else if (lst_billing4.Count < 100)
                {
                    lst_billing4.Add(_obj);
                    lista_processada4.Add(_recebido);
                }
                else if (lst_billing5.Count < 100)
                {
                    lst_billing5.Add(_obj);
                    lista_processada5.Add(_recebido);
                }
                else if (lst_billing6.Count < 100)
                {
                    lst_billing6.Add(_obj);
                    lista_processada6.Add(_recebido);
                }
                else
                {
                    EscreverArquivo("Avanco_billing_" + data, lst_billing6.ToArray()[49].ExternalID__c + "\n");

                    try
                    {
                        Parallel.Invoke(
                        () =>
                        {
                            basicSample_cs_p.apex.UpsertResult[] upsertResults = binding.upsert("ExternalId__c", lst_billing.ToArray());

                            for (int i = 0; i < upsertResults.Length; i++)
                            {
                                basicSample_cs_p.apex.UpsertResult result = upsertResults[i];

                                if (result.success)
                                {
                                    //Console.WriteLine("\nUpsert succeeded.");
                                    acerto.AppendLine(lista_processada.ToArray()[i].ToString() + ";" + (result.created ? "Inserted" : "Updated") + ";" + result.id);
                                }
                                else
                                {
                                    //Console.WriteLine("The Upsert failed because: " + result.errors[0].message);
                                    erro.AppendLine(lista_processada.ToArray()[i].ToString() + ";" + result.errors[0].message);
                                }
                            }
                        },
                        () =>
                        {
                            basicSample_cs_p.apex.UpsertResult[] upsertResults = binding.upsert("ExternalId__c", lst_billing2.ToArray());

                            for (int i = 0; i < upsertResults.Length; i++)
                            {
                                basicSample_cs_p.apex.UpsertResult result = upsertResults[i];

                                if (result.success)
                                {
                                    //Console.WriteLine("\nUpsert succeeded.");
                                    acerto2.AppendLine(lista_processada2.ToArray()[i].ToString() + ";" + (result.created ? "Inserted" : "Updated") + ";" + result.id);
                                }
                                else
                                {
                                    //Console.WriteLine("The Upsert failed because: " + result.errors[0].message);
                                    erro2.AppendLine(lista_processada2.ToArray()[i].ToString() + ";" + result.errors[0].message);
                                }
                            }
                        },
                        () =>
                        {
                            basicSample_cs_p.apex.UpsertResult[] upsertResults = binding.upsert("ExternalId__c", lst_billing3.ToArray());

                            for (int i = 0; i < upsertResults.Length; i++)
                            {
                                basicSample_cs_p.apex.UpsertResult result = upsertResults[i];

                                if (result.success)
                                {
                                    //Console.WriteLine("\nUpsert succeeded.");
                                    acerto3.AppendLine(lista_processada3.ToArray()[i].ToString() + ";" + (result.created ? "Inserted" : "Updated") + ";" + result.id);
                                }
                                else
                                {
                                    //Console.WriteLine("The Upsert failed because: " + result.errors[0].message);
                                    erro3.AppendLine(lista_processada3.ToArray()[i].ToString() + ";" + result.errors[0].message);
                                }
                            }
                        },
                        () =>
                        {
                            basicSample_cs_p.apex.UpsertResult[] upsertResults = binding.upsert("ExternalId__c", lst_billing4.ToArray());

                            for (int i = 0; i < upsertResults.Length; i++)
                            {
                                basicSample_cs_p.apex.UpsertResult result = upsertResults[i];

                                if (result.success)
                                {
                                    //Console.WriteLine("\nUpsert succeeded.");
                                    acerto4.AppendLine(lista_processada4.ToArray()[i].ToString() + ";" + (result.created ? "Inserted" : "Updated") + ";" + result.id);
                                }
                                else
                                {
                                    //Console.WriteLine("The Upsert failed because: " + result.errors[0].message);
                                    erro4.AppendLine(lista_processada4.ToArray()[i].ToString() + ";" + result.errors[0].message);
                                }
                            }
                        },
                        () =>
                        {
                            basicSample_cs_p.apex.UpsertResult[] upsertResults = binding.upsert("ExternalId__c", lst_billing5.ToArray());

                            for (int i = 0; i < upsertResults.Length; i++)
                            {
                                basicSample_cs_p.apex.UpsertResult result = upsertResults[i];

                                if (result.success)
                                {
                                    //Console.WriteLine("\nUpsert succeeded.");
                                    acerto5.AppendLine(lista_processada5.ToArray()[i].ToString() + ";" + (result.created ? "Inserted" : "Updated") + ";" + result.id);
                                }
                                else
                                {
                                    //Console.WriteLine("The Upsert failed because: " + result.errors[0].message);
                                    erro5.AppendLine(lista_processada5.ToArray()[i].ToString() + ";" + result.errors[0].message);
                                }
                            }
                        },
                        () =>
                        {
                            basicSample_cs_p.apex.UpsertResult[] upsertResults = binding.upsert("ExternalId__c", lst_billing6.ToArray());

                            for (int i = 0; i < upsertResults.Length; i++)
                            {
                                basicSample_cs_p.apex.UpsertResult result = upsertResults[i];

                                if (result.success)
                                {
                                    //Console.WriteLine("\nUpsert succeeded.");
                                    acerto6.AppendLine(lista_processada6.ToArray()[i].ToString() + ";" + (result.created ? "Inserted" : "Updated") + ";" + result.id);
                                }
                                else
                                {
                                    //Console.WriteLine("The Upsert failed because: " + result.errors[0].message);
                                    erro6.AppendLine(lista_processada6.ToArray()[i].ToString() + ";" + result.errors[0].message);
                                }
                            }
                        });

                        EscreverArquivo("resultado_positivo_billing" + data, acerto.ToString() + "\n" + acerto2.ToString() + "\n" + acerto3.ToString() + "\n" + acerto4.ToString() + "\n" + acerto5.ToString() + "\n" + acerto6.ToString());

                        EscreverArquivo("resultado_negativo_billing" + data, erro.ToString() + "\n" + erro2.ToString() + "\n" + erro3.ToString() + "\n" + erro4.ToString() + "\n" + erro5.ToString() + "\n" + erro6.ToString());

                        lst_billing.Clear();
                        lst_billing2.Clear();
                        lst_billing3.Clear();
                        lst_billing4.Clear();
                        lst_billing5.Clear();
                        lst_billing6.Clear();

                        acerto.Clear();
                        acerto2.Clear();
                        acerto3.Clear();
                        acerto4.Clear();
                        acerto5.Clear();
                        acerto6.Clear();

                        erro.Clear();
                        erro2.Clear();
                        erro3.Clear();
                        erro4.Clear();
                        erro5.Clear();
                        erro6.Clear();

                        lista_processada.Clear();
                        lista_processada2.Clear();
                        lista_processada3.Clear();
                        lista_processada4.Clear();
                        lista_processada5.Clear();
                        lista_processada6.Clear();
                        
                    }
                    catch (Exception)
                    {
                        EscreverArquivo("Erros_billing_" + data, lst_billing.ToArray()[0].ExternalID__c + " até " + lst_billing6.ToArray()[99].ExternalID__c + "\n");
                    }
                }

            }


            try
            {
                //EscreverArquivo("Avanco_billing_" + data, lst_billing6.ToArray()[99].Id + "\n");

                Parallel.Invoke(
                () =>
                {
                    basicSample_cs_p.apex.UpsertResult[] upsertResults = binding.upsert("ExternalId__c", lst_billing.ToArray());

                    for (int i = 0; i < upsertResults.Length; i++)
                    {
                        basicSample_cs_p.apex.UpsertResult result = upsertResults[i];

                        if (result.success)
                        {
                                 //Console.WriteLine("\nUpsert succeeded.");
                                 acerto.AppendLine(lista_processada.ToArray()[i].ToString() + ";" + (result.created ? "Inserted" : "Updated") + ";" + result.id);
                        }
                        else
                        {
                                 //Console.WriteLine("The Upsert failed because: " + result.errors[0].message);
                                 erro.AppendLine(lista_processada.ToArray()[i].ToString() + ";" + result.errors[0].message);
                        }
                    }
                },
                   () =>
                   {
                       basicSample_cs_p.apex.UpsertResult[] upsertResults = binding.upsert("ExternalId__c", lst_billing2.ToArray());

                       for (int i = 0; i < upsertResults.Length; i++)
                       {
                           basicSample_cs_p.apex.UpsertResult result = upsertResults[i];

                           if (result.success)
                           {
                                    //Console.WriteLine("\nUpsert succeeded.");
                                    acerto2.AppendLine(lista_processada2.ToArray()[i].ToString() + ";" + (result.created ? "Inserted" : "Updated") + ";" + result.id);
                           }
                           else
                           {
                                    //Console.WriteLine("The Upsert failed because: " + result.errors[0].message);
                                    erro2.AppendLine(lista_processada2.ToArray()[i].ToString() + ";" + result.errors[0].message);
                           }
                       }
                   },
                   () =>
                   {
                       basicSample_cs_p.apex.UpsertResult[] upsertResults = binding.upsert("ExternalId__c", lst_billing3.ToArray());

                       for (int i = 0; i < upsertResults.Length; i++)
                       {
                           basicSample_cs_p.apex.UpsertResult result = upsertResults[i];

                           if (result.success)
                           {
                                    //Console.WriteLine("\nUpsert succeeded.");
                                    acerto3.AppendLine(lista_processada3.ToArray()[i].ToString() + ";" + (result.created ? "Inserted" : "Updated") + ";" + result.id);
                           }
                           else
                           {
                                    //Console.WriteLine("The Upsert failed because: " + result.errors[0].message);
                                    erro3.AppendLine(lista_processada3.ToArray()[i].ToString() + ";" + result.errors[0].message);
                           }
                       }
                   },
                   () =>
                   {
                       basicSample_cs_p.apex.UpsertResult[] upsertResults = binding.upsert("ExternalId__c", lst_billing4.ToArray());

                       for (int i = 0; i < upsertResults.Length; i++)
                       {
                           basicSample_cs_p.apex.UpsertResult result = upsertResults[i];

                           if (result.success)
                           {
                                    //Console.WriteLine("\nUpsert succeeded.");
                                    acerto4.AppendLine(lista_processada4.ToArray()[i].ToString() + ";" + (result.created ? "Inserted" : "Updated") + ";" + result.id);
                           }
                           else
                           {
                                    //Console.WriteLine("The Upsert failed because: " + result.errors[0].message);
                                    erro4.AppendLine(lista_processada4.ToArray()[i].ToString() + ";" + result.errors[0].message);
                           }
                       }
                   },
                   () =>
                   {
                       basicSample_cs_p.apex.UpsertResult[] upsertResults = binding.upsert("ExternalId__c", lst_billing5.ToArray());

                       for (int i = 0; i < upsertResults.Length; i++)
                       {
                           basicSample_cs_p.apex.UpsertResult result = upsertResults[i];

                           if (result.success)
                           {
                                    //Console.WriteLine("\nUpsert succeeded.");
                                    acerto5.AppendLine(lista_processada5.ToArray()[i].ToString() + ";" + (result.created ? "Inserted" : "Updated") + ";" + result.id);
                           }
                           else
                           {
                                    //Console.WriteLine("The Upsert failed because: " + result.errors[0].message);
                                    erro5.AppendLine(lista_processada5.ToArray()[i].ToString() + ";" + result.errors[0].message);
                           }
                       }
                   },
                   () =>
                   {
                       basicSample_cs_p.apex.UpsertResult[] upsertResults = binding.upsert("ExternalId__c", lst_billing6.ToArray());

                       for (int i = 0; i < upsertResults.Length; i++)
                       {
                           basicSample_cs_p.apex.UpsertResult result = upsertResults[i];

                           if (result.success)
                           {
                                    //Console.WriteLine("\nUpsert succeeded.");
                                    acerto6.AppendLine(lista_processada6.ToArray()[i].ToString() + ";" + (result.created ? "Inserted" : "Updated") + ";" + result.id);
                           }
                           else
                           {
                                    //Console.WriteLine("The Upsert failed because: " + result.errors[0].message);
                                    erro6.AppendLine(lista_processada6.ToArray()[i].ToString() + ";" + result.errors[0].message);
                           }
                       }
                   });

                EscreverArquivo("resultado_positivo_billing" + data, acerto.ToString() + "\n" + acerto2.ToString() + "\n" + acerto3.ToString() + "\n" + acerto4.ToString() + "\n" + acerto5.ToString() + "\n" + acerto6.ToString());

                EscreverArquivo("resultado_negativo_billing" + data, erro.ToString() + "\n" + erro2.ToString() + "\n" + erro3.ToString() + "\n" + erro4.ToString() + "\n" + erro5.ToString() + "\n" + erro6.ToString());

                lst_billing.Clear();
                lst_billing2.Clear();
                lst_billing3.Clear();
                lst_billing4.Clear();
                lst_billing5.Clear();
                lst_billing6.Clear();

                acerto.Clear();
                acerto2.Clear();
                acerto3.Clear();
                acerto4.Clear();
                acerto5.Clear();
                acerto6.Clear();

                erro.Clear();
                erro2.Clear();
                erro3.Clear();
                erro4.Clear();
                erro5.Clear();
                erro6.Clear();


            }
            catch (Exception e)
            {
                EscreverArquivo("resultado_positivo_billing" + data, acerto.ToString() + "\n" + acerto2.ToString() + "\n" + acerto3.ToString() + "\n" + acerto4.ToString() + "\n" + acerto5.ToString() + "\n" + acerto6.ToString());

                EscreverArquivo("resultado_negativo_billing" + data, erro.ToString() + "\n" + erro2.ToString() + "\n" + erro3.ToString() + "\n" + erro4.ToString() + "\n" + erro5.ToString() + "\n" + erro6.ToString());

                EscreverArquivo("Erros_billing_" + data, e.Message + "\n");
            }

            Console.WriteLine("Fim do Programa");
            Console.ReadLine();
        }


        private void UpsertUrlFaturas()
        {
            String data = DateTime.Now.ToString("yyyyMMddHHmmss");
            bool verifica = true;

            if (!loggedIn)
            {
                if (!login())
                    return;
            }

            StreamReader arquivo = new StreamReader(caminho);
            List<Invoice__c> lst_billing = new List<Invoice__c>();
            List<Invoice__c> lst_billing2 = new List<Invoice__c>();
            List<Invoice__c> lst_billing3 = new List<Invoice__c>();
            List<Invoice__c> lst_billing4 = new List<Invoice__c>();
            List<Invoice__c> lst_billing5 = new List<Invoice__c>();
            List<Invoice__c> lst_billing6 = new List<Invoice__c>();

            StringBuilder acerto = new StringBuilder();
            StringBuilder acerto2 = new StringBuilder();
            StringBuilder acerto3 = new StringBuilder();
            StringBuilder acerto4 = new StringBuilder();
            StringBuilder acerto5 = new StringBuilder();
            StringBuilder acerto6 = new StringBuilder();

            StringBuilder erro = new StringBuilder();
            StringBuilder erro2 = new StringBuilder();
            StringBuilder erro3 = new StringBuilder();
            StringBuilder erro4 = new StringBuilder();
            StringBuilder erro5 = new StringBuilder();
            StringBuilder erro6 = new StringBuilder();

            List<String> lista_processada = new List<String>();
            List<String> lista_processada2 = new List<String>();
            List<String> lista_processada3 = new List<String>();
            List<String> lista_processada4 = new List<String>();
            List<String> lista_processada5 = new List<String>();
            List<String> lista_processada6 = new List<String>();
            String _recebido = "";


            while (!arquivo.EndOfStream)
            {                
                
                if (verifica)
                    _recebido = arquivo.ReadLine();
                else
                    verifica = true;

                String[] linha = _recebido.Split('|');
                                
                Invoice__c _obj = new Invoice__c();
                _obj.RUT__c = linha[0].ToString();
                _obj.InvoiceAccess__c = linha[1].ToString();

                if (lst_billing.Count < 100)
                {
                    lst_billing.Add(_obj);
                    lista_processada.Add(_recebido);
                }
                else if (lst_billing2.Count < 100)
                {
                    lst_billing2.Add(_obj);
                    lista_processada2.Add(_recebido);
                }
                else if (lst_billing3.Count < 100)
                {
                    lst_billing3.Add(_obj);
                    lista_processada3.Add(_recebido);
                }
                else if (lst_billing4.Count < 100)
                {
                    lst_billing4.Add(_obj);
                    lista_processada4.Add(_recebido);
                }
                else if (lst_billing5.Count < 100)
                {
                    lst_billing5.Add(_obj);
                    lista_processada5.Add(_recebido);
                }
                else if (lst_billing6.Count < 100)
                {
                    lst_billing6.Add(_obj);
                    lista_processada6.Add(_recebido);
                }
                else
                {
                    try
                    {
                        Parallel.Invoke(
                        () =>
                        {
                            basicSample_cs_p.apex.UpsertResult[] upsertResults = binding.upsert("RUT__c", lst_billing.ToArray());

                            for (int i = 0; i < upsertResults.Length; i++)
                            {
                                basicSample_cs_p.apex.UpsertResult result = upsertResults[i];

                                if (result.success)
                                {
                                    //Console.WriteLine("\nUpsert succeeded.");
                                    acerto.AppendLine(lista_processada.ToArray()[i].ToString() + ";" + (result.created ? "Inserted" : "Updated") + ";" + result.id);
                                }
                                else
                                {
                                    //Console.WriteLine("The Upsert failed because: " + result.errors[0].message);
                                    erro.AppendLine(lista_processada.ToArray()[i].ToString() + ";" + result.errors[0].message);
                                }
                            }
                        },
                        () =>
                        {
                            basicSample_cs_p.apex.UpsertResult[] upsertResults = binding.upsert("RUT__c", lst_billing2.ToArray());

                            for (int i = 0; i < upsertResults.Length; i++)
                            {
                                basicSample_cs_p.apex.UpsertResult result = upsertResults[i];

                                if (result.success)
                                {
                                    //Console.WriteLine("\nUpsert succeeded.");
                                    acerto2.AppendLine(lista_processada2.ToArray()[i].ToString() + ";" + (result.created ? "Inserted" : "Updated") + ";" + result.id);
                                }
                                else
                                {
                                    //Console.WriteLine("The Upsert failed because: " + result.errors[0].message);
                                    erro2.AppendLine(lista_processada2.ToArray()[i].ToString() + ";" + result.errors[0].message);
                                }
                            }
                        },
                        () =>
                        {
                            basicSample_cs_p.apex.UpsertResult[] upsertResults = binding.upsert("RUT__c", lst_billing3.ToArray());

                            for (int i = 0; i < upsertResults.Length; i++)
                            {
                                basicSample_cs_p.apex.UpsertResult result = upsertResults[i];

                                if (result.success)
                                {
                                    //Console.WriteLine("\nUpsert succeeded.");
                                    acerto3.AppendLine(lista_processada3.ToArray()[i].ToString() + ";" + (result.created ? "Inserted" : "Updated") + ";" + result.id);
                                }
                                else
                                {
                                    //Console.WriteLine("The Upsert failed because: " + result.errors[0].message);
                                    erro3.AppendLine(lista_processada3.ToArray()[i].ToString() + ";" + result.errors[0].message);
                                }
                            }
                        },
                        () =>
                        {
                            basicSample_cs_p.apex.UpsertResult[] upsertResults = binding.upsert("RUT__c", lst_billing4.ToArray());

                            for (int i = 0; i < upsertResults.Length; i++)
                            {
                                basicSample_cs_p.apex.UpsertResult result = upsertResults[i];

                                if (result.success)
                                {
                                    //Console.WriteLine("\nUpsert succeeded.");
                                    acerto4.AppendLine(lista_processada4.ToArray()[i].ToString() + ";" + (result.created ? "Inserted" : "Updated") + ";" + result.id);
                                }
                                else
                                {
                                    //Console.WriteLine("The Upsert failed because: " + result.errors[0].message);
                                    erro4.AppendLine(lista_processada4.ToArray()[i].ToString() + ";" + result.errors[0].message);
                                }
                            }
                        },
                        () =>
                        {
                            basicSample_cs_p.apex.UpsertResult[] upsertResults = binding.upsert("RUT__c", lst_billing5.ToArray());

                            for (int i = 0; i < upsertResults.Length; i++)
                            {
                                basicSample_cs_p.apex.UpsertResult result = upsertResults[i];

                                if (result.success)
                                {
                                    //Console.WriteLine("\nUpsert succeeded.");
                                    acerto5.AppendLine(lista_processada5.ToArray()[i].ToString() + ";" + (result.created ? "Inserted" : "Updated") + ";" + result.id);
                                }
                                else
                                {
                                    //Console.WriteLine("The Upsert failed because: " + result.errors[0].message);
                                    erro5.AppendLine(lista_processada5.ToArray()[i].ToString() + ";" + result.errors[0].message);
                                }
                            }
                        },
                        () =>
                        {
                            basicSample_cs_p.apex.UpsertResult[] upsertResults = binding.upsert("RUT__c", lst_billing6.ToArray());

                            for (int i = 0; i < upsertResults.Length; i++)
                            {
                                basicSample_cs_p.apex.UpsertResult result = upsertResults[i];

                                if (result.success)
                                {
                                    //Console.WriteLine("\nUpsert succeeded.");
                                    acerto6.AppendLine(lista_processada6.ToArray()[i].ToString() + ";" + (result.created ? "Inserted" : "Updated") + ";" + result.id);
                                }
                                else
                                {
                                    //Console.WriteLine("The Upsert failed because: " + result.errors[0].message);
                                    erro6.AppendLine(lista_processada6.ToArray()[i].ToString() + ";" + result.errors[0].message);
                                }
                            }
                        });

                        EscreverArquivo("resultado_positivo_invoice" + data, acerto.ToString());
                        EscreverArquivo("resultado_positivo_invoice" + data, acerto2.ToString());
                        EscreverArquivo("resultado_positivo_invoice" + data, acerto3.ToString());
                        EscreverArquivo("resultado_positivo_invoice" + data, acerto4.ToString());
                        EscreverArquivo("resultado_positivo_invoice" + data, acerto5.ToString());
                        EscreverArquivo("resultado_positivo_invoice" + data, acerto6.ToString());

                        EscreverArquivo("resultado_negativo_invoice" + data, erro.ToString());
                        EscreverArquivo("resultado_negativo_invoice" + data, erro2.ToString());
                        EscreverArquivo("resultado_negativo_invoice" + data, erro3.ToString());
                        EscreverArquivo("resultado_negativo_invoice" + data, erro4.ToString());
                        EscreverArquivo("resultado_negativo_invoice" + data, erro5.ToString());
                        EscreverArquivo("resultado_negativo_invoice" + data, erro6.ToString());

                        lst_billing.Clear();
                        lst_billing2.Clear();
                        lst_billing3.Clear();
                        lst_billing4.Clear();
                        lst_billing5.Clear();
                        lst_billing6.Clear();

                        acerto.Clear();
                        acerto2.Clear();
                        acerto3.Clear();
                        acerto4.Clear();
                        acerto5.Clear();
                        acerto6.Clear();

                        erro.Clear();
                        erro2.Clear();
                        erro3.Clear();
                        erro4.Clear();
                        erro5.Clear();
                        erro6.Clear();

                        lista_processada.Clear();
                        lista_processada2.Clear();
                        lista_processada3.Clear();
                        lista_processada4.Clear();
                        lista_processada5.Clear();
                        lista_processada6.Clear();
                        verifica = false;

                    }
                    catch (Exception)
                    {
                        EscreverArquivo("Erros_invoice_" + data, lst_billing.ToArray()[0].RUT__c + " até " + lst_billing6.ToArray()[99].RUT__c + "\n");
                    }
                }

            }


            try
            {
                //EscreverArquivo("Avanco_billing_" + data, lst_billing6.ToArray()[99].Id + "\n");

                Parallel.Invoke(
                () =>
                {
                    basicSample_cs_p.apex.UpsertResult[] upsertResults = binding.upsert("RUT__c", lst_billing.ToArray());

                    for (int i = 0; i < upsertResults.Length; i++)
                    {
                        basicSample_cs_p.apex.UpsertResult result = upsertResults[i];

                        if (result.success)
                        {
                            //Console.WriteLine("\nUpsert succeeded.");
                            acerto.AppendLine(lista_processada.ToArray()[i].ToString() + ";" + (result.created ? "Inserted" : "Updated") + ";" + result.id);
                        }
                        else
                        {
                            //Console.WriteLine("The Upsert failed because: " + result.errors[0].message);
                            erro.AppendLine(lista_processada.ToArray()[i].ToString() + ";" + result.errors[0].message);
                        }
                    }
                },
                   () =>
                   {
                       basicSample_cs_p.apex.UpsertResult[] upsertResults = binding.upsert("RUT__c", lst_billing2.ToArray());

                       for (int i = 0; i < upsertResults.Length; i++)
                       {
                           basicSample_cs_p.apex.UpsertResult result = upsertResults[i];

                           if (result.success)
                           {
                               //Console.WriteLine("\nUpsert succeeded.");
                               acerto2.AppendLine(lista_processada2.ToArray()[i].ToString() + ";" + (result.created ? "Inserted" : "Updated") + ";" + result.id);
                           }
                           else
                           {
                               //Console.WriteLine("The Upsert failed because: " + result.errors[0].message);
                               erro2.AppendLine(lista_processada2.ToArray()[i].ToString() + ";" + result.errors[0].message);
                           }
                       }
                   },
                   () =>
                   {
                       basicSample_cs_p.apex.UpsertResult[] upsertResults = binding.upsert("RUT__c", lst_billing3.ToArray());

                       for (int i = 0; i < upsertResults.Length; i++)
                       {
                           basicSample_cs_p.apex.UpsertResult result = upsertResults[i];

                           if (result.success)
                           {
                               //Console.WriteLine("\nUpsert succeeded.");
                               acerto3.AppendLine(lista_processada3.ToArray()[i].ToString() + ";" + (result.created ? "Inserted" : "Updated") + ";" + result.id);
                           }
                           else
                           {
                               //Console.WriteLine("The Upsert failed because: " + result.errors[0].message);
                               erro3.AppendLine(lista_processada3.ToArray()[i].ToString() + ";" + result.errors[0].message);
                           }
                       }
                   },
                   () =>
                   {
                       basicSample_cs_p.apex.UpsertResult[] upsertResults = binding.upsert("RUT__c", lst_billing4.ToArray());

                       for (int i = 0; i < upsertResults.Length; i++)
                       {
                           basicSample_cs_p.apex.UpsertResult result = upsertResults[i];

                           if (result.success)
                           {
                               //Console.WriteLine("\nUpsert succeeded.");
                               acerto4.AppendLine(lista_processada4.ToArray()[i].ToString() + ";" + (result.created ? "Inserted" : "Updated") + ";" + result.id);
                           }
                           else
                           {
                               //Console.WriteLine("The Upsert failed because: " + result.errors[0].message);
                               erro4.AppendLine(lista_processada4.ToArray()[i].ToString() + ";" + result.errors[0].message);
                           }
                       }
                   },
                   () =>
                   {
                       basicSample_cs_p.apex.UpsertResult[] upsertResults = binding.upsert("RUT__c", lst_billing5.ToArray());

                       for (int i = 0; i < upsertResults.Length; i++)
                       {
                           basicSample_cs_p.apex.UpsertResult result = upsertResults[i];

                           if (result.success)
                           {
                               //Console.WriteLine("\nUpsert succeeded.");
                               acerto5.AppendLine(lista_processada5.ToArray()[i].ToString() + ";" + (result.created ? "Inserted" : "Updated") + ";" + result.id);
                           }
                           else
                           {
                               //Console.WriteLine("The Upsert failed because: " + result.errors[0].message);
                               erro5.AppendLine(lista_processada5.ToArray()[i].ToString() + ";" + result.errors[0].message);
                           }
                       }
                   },
                   () =>
                   {
                       basicSample_cs_p.apex.UpsertResult[] upsertResults = binding.upsert("RUT__c", lst_billing6.ToArray());

                       for (int i = 0; i < upsertResults.Length; i++)
                       {
                           basicSample_cs_p.apex.UpsertResult result = upsertResults[i];

                           if (result.success)
                           {
                               //Console.WriteLine("\nUpsert succeeded.");
                               acerto6.AppendLine(lista_processada6.ToArray()[i].ToString() + ";" + (result.created ? "Inserted" : "Updated") + ";" + result.id);
                           }
                           else
                           {
                               //Console.WriteLine("The Upsert failed because: " + result.errors[0].message);
                               erro6.AppendLine(lista_processada6.ToArray()[i].ToString() + ";" + result.errors[0].message);
                           }
                       }
                   });

                EscreverArquivo("resultado_positivo_invoice" + data, acerto.ToString());
                EscreverArquivo("resultado_positivo_invoice" + data, acerto2.ToString());
                EscreverArquivo("resultado_positivo_invoice" + data, acerto3.ToString());
                EscreverArquivo("resultado_positivo_invoice" + data, acerto4.ToString());
                EscreverArquivo("resultado_positivo_invoice" + data, acerto5.ToString());
                EscreverArquivo("resultado_positivo_invoice" + data, acerto6.ToString());

                EscreverArquivo("resultado_negativo_invoice" + data, erro.ToString());
                EscreverArquivo("resultado_negativo_invoice" + data, erro2.ToString());
                EscreverArquivo("resultado_negativo_invoice" + data, erro3.ToString());
                EscreverArquivo("resultado_negativo_invoice" + data, erro4.ToString());
                EscreverArquivo("resultado_negativo_invoice" + data, erro5.ToString());
                EscreverArquivo("resultado_negativo_invoice" + data, erro6.ToString());

                lst_billing.Clear();
                lst_billing2.Clear();
                lst_billing3.Clear();
                lst_billing4.Clear();
                lst_billing5.Clear();
                lst_billing6.Clear();

                acerto.Clear();
                acerto2.Clear();
                acerto3.Clear();
                acerto4.Clear();
                acerto5.Clear();
                acerto6.Clear();

                erro.Clear();
                erro2.Clear();
                erro3.Clear();
                erro4.Clear();
                erro5.Clear();
                erro6.Clear();


            }
            catch (Exception e)
            {
                EscreverArquivo("resultado_positivo_invoice" + data, acerto.ToString());
                EscreverArquivo("resultado_positivo_invoice" + data, acerto2.ToString());
                EscreverArquivo("resultado_positivo_invoice" + data, acerto3.ToString());
                EscreverArquivo("resultado_positivo_invoice" + data, acerto4.ToString());
                EscreverArquivo("resultado_positivo_invoice" + data, acerto5.ToString());
                EscreverArquivo("resultado_positivo_invoice" + data, acerto6.ToString());

                EscreverArquivo("resultado_negativo_invoice" + data, erro.ToString());
                EscreverArquivo("resultado_negativo_invoice" + data, erro2.ToString());
                EscreverArquivo("resultado_negativo_invoice" + data, erro3.ToString());
                EscreverArquivo("resultado_negativo_invoice" + data, erro4.ToString());
                EscreverArquivo("resultado_negativo_invoice" + data, erro5.ToString());
                EscreverArquivo("resultado_negativo_invoice" + data, erro6.ToString());

                EscreverArquivo("Erros_invoice_" + data, e.Message + "\n");
            }

            Console.WriteLine("Fim do Programa");
            Console.ReadLine();
        }


        private void UpsertAccountMaeNasc()
        {
            String data = DateTime.Now.ToString("yyyyMMddHHmmss");
            bool verifica = true;

            if (!loggedIn)
            {
                if (!login())
                    return;
            }

            StreamReader arquivo = new StreamReader(caminho);
            List<Account> lst_billing = new List<Account>();
            List<Account> lst_billing2 = new List<Account>();
            List<Account> lst_billing3 = new List<Account>();
            List<Account> lst_billing4 = new List<Account>();
            List<Account> lst_billing5 = new List<Account>();
            List<Account> lst_billing6 = new List<Account>();

            StringBuilder acerto = new StringBuilder();
            StringBuilder acerto2 = new StringBuilder();
            StringBuilder acerto3 = new StringBuilder();
            StringBuilder acerto4 = new StringBuilder();
            StringBuilder acerto5 = new StringBuilder();
            StringBuilder acerto6 = new StringBuilder();

            StringBuilder erro = new StringBuilder();
            StringBuilder erro2 = new StringBuilder();
            StringBuilder erro3 = new StringBuilder();
            StringBuilder erro4 = new StringBuilder();
            StringBuilder erro5 = new StringBuilder();
            StringBuilder erro6 = new StringBuilder();

            List<String> lista_processada = new List<String>();
            List<String> lista_processada2 = new List<String>();
            List<String> lista_processada3 = new List<String>();
            List<String> lista_processada4 = new List<String>();
            List<String> lista_processada5 = new List<String>();
            List<String> lista_processada6 = new List<String>();
            String _recebido = "";


            while (!arquivo.EndOfStream)
            {

                if (verifica)
                    _recebido = arquivo.ReadLine();
                else
                    verifica = true;

                String[] linha = _recebido.Split(';');

                Account _obj = new Account();
                _obj.ExternalId__c = "2005" + linha[1].ToString() + "CPF";
                _obj.CNT_Mothers_Full_Name__c = linha[4].ToString();
               
                if(linha[2].ToString() != "")
                    _obj.BirthDate__c = Convert.ToDateTime(linha[2].ToString());

                if (lst_billing.Count < 50)
                {
                    lst_billing.Add(_obj);
                    lista_processada.Add(_recebido);
                }
                else if (lst_billing2.Count < 50)
                {
                    lst_billing2.Add(_obj);
                    lista_processada2.Add(_recebido);
                }
                else if (lst_billing3.Count < 50)
                {
                    lst_billing3.Add(_obj);
                    lista_processada3.Add(_recebido);
                }
                else if (lst_billing4.Count < 50)
                {
                    lst_billing4.Add(_obj);
                    lista_processada4.Add(_recebido);
                }
                else if (lst_billing5.Count < 50)
                {
                    lst_billing5.Add(_obj);
                    lista_processada5.Add(_recebido);
                }
                else if (lst_billing6.Count < 50)
                {
                    lst_billing6.Add(_obj);
                    lista_processada6.Add(_recebido);
                }
                else
                {
                    try
                    {
                        Parallel.Invoke(
                        () =>
                        {
                            basicSample_cs_p.apex.UpsertResult[] upsertResults = binding.upsert("ExternalId__c", lst_billing.ToArray());

                            for (int i = 0; i < upsertResults.Length; i++)
                            {
                                basicSample_cs_p.apex.UpsertResult result = upsertResults[i];

                                if (result.success)
                                {
                                    //Console.WriteLine("\nUpsert succeeded.");
                                    acerto.AppendLine(lista_processada.ToArray()[i].ToString() + ";" + (result.created ? "Inserted" : "Updated") + ";" + result.id);
                                }
                                else
                                {
                                    //Console.WriteLine("The Upsert failed because: " + result.errors[0].message);
                                    erro.AppendLine(lista_processada.ToArray()[i].ToString() + ";" + result.errors[0].message);
                                }
                            }
                        },
                        () =>
                        {
                            basicSample_cs_p.apex.UpsertResult[] upsertResults = binding.upsert("ExternalId__c", lst_billing2.ToArray());

                            for (int i = 0; i < upsertResults.Length; i++)
                            {
                                basicSample_cs_p.apex.UpsertResult result = upsertResults[i];

                                if (result.success)
                                {
                                    //Console.WriteLine("\nUpsert succeeded.");
                                    acerto2.AppendLine(lista_processada2.ToArray()[i].ToString() + ";" + (result.created ? "Inserted" : "Updated") + ";" + result.id);
                                }
                                else
                                {
                                    //Console.WriteLine("The Upsert failed because: " + result.errors[0].message);
                                    erro2.AppendLine(lista_processada2.ToArray()[i].ToString() + ";" + result.errors[0].message);
                                }
                            }
                        },
                        () =>
                        {
                            basicSample_cs_p.apex.UpsertResult[] upsertResults = binding.upsert("ExternalId__c", lst_billing3.ToArray());

                            for (int i = 0; i < upsertResults.Length; i++)
                            {
                                basicSample_cs_p.apex.UpsertResult result = upsertResults[i];

                                if (result.success)
                                {
                                    //Console.WriteLine("\nUpsert succeeded.");
                                    acerto3.AppendLine(lista_processada3.ToArray()[i].ToString() + ";" + (result.created ? "Inserted" : "Updated") + ";" + result.id);
                                }
                                else
                                {
                                    //Console.WriteLine("The Upsert failed because: " + result.errors[0].message);
                                    erro3.AppendLine(lista_processada3.ToArray()[i].ToString() + ";" + result.errors[0].message);
                                }
                            }
                        },
                        () =>
                        {
                            basicSample_cs_p.apex.UpsertResult[] upsertResults = binding.upsert("ExternalId__c", lst_billing4.ToArray());

                            for (int i = 0; i < upsertResults.Length; i++)
                            {
                                basicSample_cs_p.apex.UpsertResult result = upsertResults[i];

                                if (result.success)
                                {
                                    //Console.WriteLine("\nUpsert succeeded.");
                                    acerto4.AppendLine(lista_processada4.ToArray()[i].ToString() + ";" + (result.created ? "Inserted" : "Updated") + ";" + result.id);
                                }
                                else
                                {
                                    //Console.WriteLine("The Upsert failed because: " + result.errors[0].message);
                                    erro4.AppendLine(lista_processada4.ToArray()[i].ToString() + ";" + result.errors[0].message);
                                }
                            }
                        },
                        () =>
                        {
                            basicSample_cs_p.apex.UpsertResult[] upsertResults = binding.upsert("ExternalId__c", lst_billing5.ToArray());

                            for (int i = 0; i < upsertResults.Length; i++)
                            {
                                basicSample_cs_p.apex.UpsertResult result = upsertResults[i];

                                if (result.success)
                                {
                                    //Console.WriteLine("\nUpsert succeeded.");
                                    acerto5.AppendLine(lista_processada5.ToArray()[i].ToString() + ";" + (result.created ? "Inserted" : "Updated") + ";" + result.id);
                                }
                                else
                                {
                                    //Console.WriteLine("The Upsert failed because: " + result.errors[0].message);
                                    erro5.AppendLine(lista_processada5.ToArray()[i].ToString() + ";" + result.errors[0].message);
                                }
                            }
                        },
                        () =>
                        {
                            basicSample_cs_p.apex.UpsertResult[] upsertResults = binding.upsert("ExternalId__c", lst_billing6.ToArray());

                            for (int i = 0; i < upsertResults.Length; i++)
                            {
                                basicSample_cs_p.apex.UpsertResult result = upsertResults[i];

                                if (result.success)
                                {
                                    //Console.WriteLine("\nUpsert succeeded.");
                                    acerto6.AppendLine(lista_processada6.ToArray()[i].ToString() + ";" + (result.created ? "Inserted" : "Updated") + ";" + result.id);
                                }
                                else
                                {
                                    //Console.WriteLine("The Upsert failed because: " + result.errors[0].message);
                                    erro6.AppendLine(lista_processada6.ToArray()[i].ToString() + ";" + result.errors[0].message);
                                }
                            }
                        });

                        EscreverArquivo("resultado_positivo_mae" + data, acerto.ToString());
                        EscreverArquivo("resultado_positivo_mae" + data, acerto2.ToString());
                        EscreverArquivo("resultado_positivo_mae" + data, acerto3.ToString());
                        EscreverArquivo("resultado_positivo_mae" + data, acerto4.ToString());
                        EscreverArquivo("resultado_positivo_mae" + data, acerto5.ToString());
                        EscreverArquivo("resultado_positivo_mae" + data, acerto6.ToString());

                        EscreverArquivo("resultado_negativo_mae" + data, erro.ToString());
                        EscreverArquivo("resultado_negativo_mae" + data, erro2.ToString());
                        EscreverArquivo("resultado_negativo_mae" + data, erro3.ToString());
                        EscreverArquivo("resultado_negativo_mae" + data, erro4.ToString());
                        EscreverArquivo("resultado_negativo_mae" + data, erro5.ToString());
                        EscreverArquivo("resultado_negativo_mae" + data, erro6.ToString());

                        lst_billing.Clear();
                        lst_billing2.Clear();
                        lst_billing3.Clear();
                        lst_billing4.Clear();
                        lst_billing5.Clear();
                        lst_billing6.Clear();

                        acerto.Clear();
                        acerto2.Clear();
                        acerto3.Clear();
                        acerto4.Clear();
                        acerto5.Clear();
                        acerto6.Clear();

                        erro.Clear();
                        erro2.Clear();
                        erro3.Clear();
                        erro4.Clear();
                        erro5.Clear();
                        erro6.Clear();

                        lista_processada.Clear();
                        lista_processada2.Clear();
                        lista_processada3.Clear();
                        lista_processada4.Clear();
                        lista_processada5.Clear();
                        lista_processada6.Clear();
                        verifica = false;

                    }
                    catch (Exception)
                    {
                        EscreverArquivo("Erros_mae_" + data, lst_billing.ToArray()[0].RUT__c + " até " + lst_billing6.ToArray()[49].RUT__c + "\n");
                    }
                }

            }


            try
            {
                //EscreverArquivo("Avanco_billing_" + data, lst_billing6.ToArray()[99].Id + "\n");

                Parallel.Invoke(
                () =>
                {
                    basicSample_cs_p.apex.UpsertResult[] upsertResults = binding.upsert("ExternalId__c", lst_billing.ToArray());

                    for (int i = 0; i < upsertResults.Length; i++)
                    {
                        basicSample_cs_p.apex.UpsertResult result = upsertResults[i];

                        if (result.success)
                        {
                            //Console.WriteLine("\nUpsert succeeded.");
                            acerto.AppendLine(lista_processada.ToArray()[i].ToString() + ";" + (result.created ? "Inserted" : "Updated") + ";" + result.id);
                        }
                        else
                        {
                            //Console.WriteLine("The Upsert failed because: " + result.errors[0].message);
                            erro.AppendLine(lista_processada.ToArray()[i].ToString() + ";" + result.errors[0].message);
                        }
                    }
                },
                   () =>
                   {
                       basicSample_cs_p.apex.UpsertResult[] upsertResults = binding.upsert("ExternalId__c", lst_billing2.ToArray());

                       for (int i = 0; i < upsertResults.Length; i++)
                       {
                           basicSample_cs_p.apex.UpsertResult result = upsertResults[i];

                           if (result.success)
                           {
                               //Console.WriteLine("\nUpsert succeeded.");
                               acerto2.AppendLine(lista_processada2.ToArray()[i].ToString() + ";" + (result.created ? "Inserted" : "Updated") + ";" + result.id);
                           }
                           else
                           {
                               //Console.WriteLine("The Upsert failed because: " + result.errors[0].message);
                               erro2.AppendLine(lista_processada2.ToArray()[i].ToString() + ";" + result.errors[0].message);
                           }
                       }
                   },
                   () =>
                   {
                       basicSample_cs_p.apex.UpsertResult[] upsertResults = binding.upsert("ExternalId__c", lst_billing3.ToArray());

                       for (int i = 0; i < upsertResults.Length; i++)
                       {
                           basicSample_cs_p.apex.UpsertResult result = upsertResults[i];

                           if (result.success)
                           {
                               //Console.WriteLine("\nUpsert succeeded.");
                               acerto3.AppendLine(lista_processada3.ToArray()[i].ToString() + ";" + (result.created ? "Inserted" : "Updated") + ";" + result.id);
                           }
                           else
                           {
                               //Console.WriteLine("The Upsert failed because: " + result.errors[0].message);
                               erro3.AppendLine(lista_processada3.ToArray()[i].ToString() + ";" + result.errors[0].message);
                           }
                       }
                   },
                   () =>
                   {
                       basicSample_cs_p.apex.UpsertResult[] upsertResults = binding.upsert("ExternalId__c", lst_billing4.ToArray());

                       for (int i = 0; i < upsertResults.Length; i++)
                       {
                           basicSample_cs_p.apex.UpsertResult result = upsertResults[i];

                           if (result.success)
                           {
                               //Console.WriteLine("\nUpsert succeeded.");
                               acerto4.AppendLine(lista_processada4.ToArray()[i].ToString() + ";" + (result.created ? "Inserted" : "Updated") + ";" + result.id);
                           }
                           else
                           {
                               //Console.WriteLine("The Upsert failed because: " + result.errors[0].message);
                               erro4.AppendLine(lista_processada4.ToArray()[i].ToString() + ";" + result.errors[0].message);
                           }
                       }
                   },
                   () =>
                   {
                       basicSample_cs_p.apex.UpsertResult[] upsertResults = binding.upsert("ExternalId__c", lst_billing5.ToArray());

                       for (int i = 0; i < upsertResults.Length; i++)
                       {
                           basicSample_cs_p.apex.UpsertResult result = upsertResults[i];

                           if (result.success)
                           {
                               //Console.WriteLine("\nUpsert succeeded.");
                               acerto5.AppendLine(lista_processada5.ToArray()[i].ToString() + ";" + (result.created ? "Inserted" : "Updated") + ";" + result.id);
                           }
                           else
                           {
                               //Console.WriteLine("The Upsert failed because: " + result.errors[0].message);
                               erro5.AppendLine(lista_processada5.ToArray()[i].ToString() + ";" + result.errors[0].message);
                           }
                       }
                   },
                   () =>
                   {
                       basicSample_cs_p.apex.UpsertResult[] upsertResults = binding.upsert("ExternalId__c", lst_billing6.ToArray());

                       for (int i = 0; i < upsertResults.Length; i++)
                       {
                           basicSample_cs_p.apex.UpsertResult result = upsertResults[i];

                           if (result.success)
                           {
                               //Console.WriteLine("\nUpsert succeeded.");
                               acerto6.AppendLine(lista_processada6.ToArray()[i].ToString() + ";" + (result.created ? "Inserted" : "Updated") + ";" + result.id);
                           }
                           else
                           {
                               //Console.WriteLine("The Upsert failed because: " + result.errors[0].message);
                               erro6.AppendLine(lista_processada6.ToArray()[i].ToString() + ";" + result.errors[0].message);
                           }
                       }
                   });

                EscreverArquivo("resultado_positivo_mae" + data, acerto.ToString());
                EscreverArquivo("resultado_positivo_mae" + data, acerto2.ToString());
                EscreverArquivo("resultado_positivo_mae" + data, acerto3.ToString());
                EscreverArquivo("resultado_positivo_mae" + data, acerto4.ToString());
                EscreverArquivo("resultado_positivo_mae" + data, acerto5.ToString());
                EscreverArquivo("resultado_positivo_mae" + data, acerto6.ToString());

                EscreverArquivo("resultado_negativo_mae" + data, erro.ToString());
                EscreverArquivo("resultado_negativo_mae" + data, erro2.ToString());
                EscreverArquivo("resultado_negativo_mae" + data, erro3.ToString());
                EscreverArquivo("resultado_negativo_mae" + data, erro4.ToString());
                EscreverArquivo("resultado_negativo_mae" + data, erro5.ToString());
                EscreverArquivo("resultado_negativo_mae" + data, erro6.ToString());

                lst_billing.Clear();
                lst_billing2.Clear();
                lst_billing3.Clear();
                lst_billing4.Clear();
                lst_billing5.Clear();
                lst_billing6.Clear();

                acerto.Clear();
                acerto2.Clear();
                acerto3.Clear();
                acerto4.Clear();
                acerto5.Clear();
                acerto6.Clear();

                erro.Clear();
                erro2.Clear();
                erro3.Clear();
                erro4.Clear();
                erro5.Clear();
                erro6.Clear();


            }
            catch (Exception e)
            {
                EscreverArquivo("resultado_positivo_mae" + data, acerto.ToString());
                EscreverArquivo("resultado_positivo_mae" + data, acerto2.ToString());
                EscreverArquivo("resultado_positivo_mae" + data, acerto3.ToString());
                EscreverArquivo("resultado_positivo_mae" + data, acerto4.ToString());
                EscreverArquivo("resultado_positivo_mae" + data, acerto5.ToString());
                EscreverArquivo("resultado_positivo_mae" + data, acerto6.ToString());

                EscreverArquivo("resultado_negativo_mae" + data, erro.ToString());
                EscreverArquivo("resultado_negativo_mae" + data, erro2.ToString());
                EscreverArquivo("resultado_negativo_mae" + data, erro3.ToString());
                EscreverArquivo("resultado_negativo_mae" + data, erro4.ToString());
                EscreverArquivo("resultado_negativo_mae" + data, erro5.ToString());
                EscreverArquivo("resultado_negativo_mae" + data, erro6.ToString());

                EscreverArquivo("Erros_mae_" + data, e.Message + "\n");
            }

            Console.WriteLine("Fim do Programa");
            Console.ReadLine();
        }

        private void UpsertStatusFaturas()
        {
            String data = DateTime.Now.ToString("yyyyMMddHHmmss");
            bool verifica = true;

            if (!loggedIn)
            {
                if (!login())
                    return;
            }

            StreamReader arquivo = new StreamReader(caminho);
            List<Invoice__c> lst_billing = new List<Invoice__c>();
            List<Invoice__c> lst_billing2 = new List<Invoice__c>();
            List<Invoice__c> lst_billing3 = new List<Invoice__c>();
            List<Invoice__c> lst_billing4 = new List<Invoice__c>();
            List<Invoice__c> lst_billing5 = new List<Invoice__c>();
            List<Invoice__c> lst_billing6 = new List<Invoice__c>();

            StringBuilder acerto = new StringBuilder();
            StringBuilder acerto2 = new StringBuilder();
            StringBuilder acerto3 = new StringBuilder();
            StringBuilder acerto4 = new StringBuilder();
            StringBuilder acerto5 = new StringBuilder();
            StringBuilder acerto6 = new StringBuilder();

            StringBuilder erro = new StringBuilder();
            StringBuilder erro2 = new StringBuilder();
            StringBuilder erro3 = new StringBuilder();
            StringBuilder erro4 = new StringBuilder();
            StringBuilder erro5 = new StringBuilder();
            StringBuilder erro6 = new StringBuilder();

            List<String> lista_processada = new List<String>();
            List<String> lista_processada2 = new List<String>();
            List<String> lista_processada3 = new List<String>();
            List<String> lista_processada4 = new List<String>();
            List<String> lista_processada5 = new List<String>();
            List<String> lista_processada6 = new List<String>();
            String _recebido = "";


            while (!arquivo.EndOfStream)
            {

                if (verifica)
                    _recebido = arquivo.ReadLine();
                else
                    verifica = true;

                String[] linha = _recebido.Split(';');

                Invoice__c _obj = new Invoice__c();
                _obj.RUT__c = linha[0].ToString();
                _obj.Status__c = "X";

                if (lst_billing.Count < 100)
                {
                    lst_billing.Add(_obj);
                    lista_processada.Add(_recebido);
                }
                else if (lst_billing2.Count < 100)
                {
                    lst_billing2.Add(_obj);
                    lista_processada2.Add(_recebido);
                }
                else if (lst_billing3.Count < 100)
                {
                    lst_billing3.Add(_obj);
                    lista_processada3.Add(_recebido);
                }
                else if (lst_billing4.Count < 100)
                {
                    lst_billing4.Add(_obj);
                    lista_processada4.Add(_recebido);
                }
                else if (lst_billing5.Count < 100)
                {
                    lst_billing5.Add(_obj);
                    lista_processada5.Add(_recebido);
                }
                else if (lst_billing6.Count < 100)
                {
                    lst_billing6.Add(_obj);
                    lista_processada6.Add(_recebido);
                }
                else
                {
                    try
                    {
                        Parallel.Invoke(
                        () =>
                        {
                            basicSample_cs_p.apex.UpsertResult[] upsertResults = binding.upsert("RUT__c", lst_billing.ToArray());

                            for (int i = 0; i < upsertResults.Length; i++)
                            {
                                basicSample_cs_p.apex.UpsertResult result = upsertResults[i];

                                if (result.success)
                                {
                                    //Console.WriteLine("\nUpsert succeeded.");
                                    acerto.AppendLine(lista_processada.ToArray()[i].ToString() + ";" + (result.created ? "Inserted" : "Updated") + ";" + result.id);
                                }
                                else
                                {
                                    //Console.WriteLine("The Upsert failed because: " + result.errors[0].message);
                                    erro.AppendLine(lista_processada.ToArray()[i].ToString() + ";" + result.errors[0].message);
                                }
                            }
                        },
                        () =>
                        {
                            basicSample_cs_p.apex.UpsertResult[] upsertResults = binding.upsert("RUT__c", lst_billing2.ToArray());

                            for (int i = 0; i < upsertResults.Length; i++)
                            {
                                basicSample_cs_p.apex.UpsertResult result = upsertResults[i];

                                if (result.success)
                                {
                                    //Console.WriteLine("\nUpsert succeeded.");
                                    acerto2.AppendLine(lista_processada2.ToArray()[i].ToString() + ";" + (result.created ? "Inserted" : "Updated") + ";" + result.id);
                                }
                                else
                                {
                                    //Console.WriteLine("The Upsert failed because: " + result.errors[0].message);
                                    erro2.AppendLine(lista_processada2.ToArray()[i].ToString() + ";" + result.errors[0].message);
                                }
                            }
                        },
                        () =>
                        {
                            basicSample_cs_p.apex.UpsertResult[] upsertResults = binding.upsert("RUT__c", lst_billing3.ToArray());

                            for (int i = 0; i < upsertResults.Length; i++)
                            {
                                basicSample_cs_p.apex.UpsertResult result = upsertResults[i];

                                if (result.success)
                                {
                                    //Console.WriteLine("\nUpsert succeeded.");
                                    acerto3.AppendLine(lista_processada3.ToArray()[i].ToString() + ";" + (result.created ? "Inserted" : "Updated") + ";" + result.id);
                                }
                                else
                                {
                                    //Console.WriteLine("The Upsert failed because: " + result.errors[0].message);
                                    erro3.AppendLine(lista_processada3.ToArray()[i].ToString() + ";" + result.errors[0].message);
                                }
                            }
                        },
                        () =>
                        {
                            basicSample_cs_p.apex.UpsertResult[] upsertResults = binding.upsert("RUT__c", lst_billing4.ToArray());

                            for (int i = 0; i < upsertResults.Length; i++)
                            {
                                basicSample_cs_p.apex.UpsertResult result = upsertResults[i];

                                if (result.success)
                                {
                                    //Console.WriteLine("\nUpsert succeeded.");
                                    acerto4.AppendLine(lista_processada4.ToArray()[i].ToString() + ";" + (result.created ? "Inserted" : "Updated") + ";" + result.id);
                                }
                                else
                                {
                                    //Console.WriteLine("The Upsert failed because: " + result.errors[0].message);
                                    erro4.AppendLine(lista_processada4.ToArray()[i].ToString() + ";" + result.errors[0].message);
                                }
                            }
                        },
                        () =>
                        {
                            basicSample_cs_p.apex.UpsertResult[] upsertResults = binding.upsert("RUT__c", lst_billing5.ToArray());

                            for (int i = 0; i < upsertResults.Length; i++)
                            {
                                basicSample_cs_p.apex.UpsertResult result = upsertResults[i];

                                if (result.success)
                                {
                                    //Console.WriteLine("\nUpsert succeeded.");
                                    acerto5.AppendLine(lista_processada5.ToArray()[i].ToString() + ";" + (result.created ? "Inserted" : "Updated") + ";" + result.id);
                                }
                                else
                                {
                                    //Console.WriteLine("The Upsert failed because: " + result.errors[0].message);
                                    erro5.AppendLine(lista_processada5.ToArray()[i].ToString() + ";" + result.errors[0].message);
                                }
                            }
                        },
                        () =>
                        {
                            basicSample_cs_p.apex.UpsertResult[] upsertResults = binding.upsert("RUT__c", lst_billing6.ToArray());

                            for (int i = 0; i < upsertResults.Length; i++)
                            {
                                basicSample_cs_p.apex.UpsertResult result = upsertResults[i];

                                if (result.success)
                                {
                                    //Console.WriteLine("\nUpsert succeeded.");
                                    acerto6.AppendLine(lista_processada6.ToArray()[i].ToString() + ";" + (result.created ? "Inserted" : "Updated") + ";" + result.id);
                                }
                                else
                                {
                                    //Console.WriteLine("The Upsert failed because: " + result.errors[0].message);
                                    erro6.AppendLine(lista_processada6.ToArray()[i].ToString() + ";" + result.errors[0].message);
                                }
                            }
                        });

                        EscreverArquivo("resultado_positivo_invoice" + data, acerto.ToString());
                        EscreverArquivo("resultado_positivo_invoice" + data, acerto2.ToString());
                        EscreverArquivo("resultado_positivo_invoice" + data, acerto3.ToString());
                        EscreverArquivo("resultado_positivo_invoice" + data, acerto4.ToString());
                        EscreverArquivo("resultado_positivo_invoice" + data, acerto5.ToString());
                        EscreverArquivo("resultado_positivo_invoice" + data, acerto6.ToString());

                        EscreverArquivo("resultado_negativo_invoice" + data, erro.ToString());
                        EscreverArquivo("resultado_negativo_invoice" + data, erro2.ToString());
                        EscreverArquivo("resultado_negativo_invoice" + data, erro3.ToString());
                        EscreverArquivo("resultado_negativo_invoice" + data, erro4.ToString());
                        EscreverArquivo("resultado_negativo_invoice" + data, erro5.ToString());
                        EscreverArquivo("resultado_negativo_invoice" + data, erro6.ToString());

                        lst_billing.Clear();
                        lst_billing2.Clear();
                        lst_billing3.Clear();
                        lst_billing4.Clear();
                        lst_billing5.Clear();
                        lst_billing6.Clear();

                        acerto.Clear();
                        acerto2.Clear();
                        acerto3.Clear();
                        acerto4.Clear();
                        acerto5.Clear();
                        acerto6.Clear();

                        erro.Clear();
                        erro2.Clear();
                        erro3.Clear();
                        erro4.Clear();
                        erro5.Clear();
                        erro6.Clear();

                        lista_processada.Clear();
                        lista_processada2.Clear();
                        lista_processada3.Clear();
                        lista_processada4.Clear();
                        lista_processada5.Clear();
                        lista_processada6.Clear();
                        verifica = false;

                    }
                    catch (Exception)
                    {
                        EscreverArquivo("Erros_invoice_" + data, lst_billing.ToArray()[0].RUT__c + " até " + lst_billing6.ToArray()[99].RUT__c + "\n");
                    }
                }

            }


            try
            {
                //EscreverArquivo("Avanco_billing_" + data, lst_billing6.ToArray()[99].Id + "\n");

                Parallel.Invoke(
                () =>
                {
                    basicSample_cs_p.apex.UpsertResult[] upsertResults = binding.upsert("RUT__c", lst_billing.ToArray());

                    for (int i = 0; i < upsertResults.Length; i++)
                    {
                        basicSample_cs_p.apex.UpsertResult result = upsertResults[i];

                        if (result.success)
                        {
                            //Console.WriteLine("\nUpsert succeeded.");
                            acerto.AppendLine(lista_processada.ToArray()[i].ToString() + ";" + (result.created ? "Inserted" : "Updated") + ";" + result.id);
                        }
                        else
                        {
                            //Console.WriteLine("The Upsert failed because: " + result.errors[0].message);
                            erro.AppendLine(lista_processada.ToArray()[i].ToString() + ";" + result.errors[0].message);
                        }
                    }
                },
                   () =>
                   {
                       basicSample_cs_p.apex.UpsertResult[] upsertResults = binding.upsert("RUT__c", lst_billing2.ToArray());

                       for (int i = 0; i < upsertResults.Length; i++)
                       {
                           basicSample_cs_p.apex.UpsertResult result = upsertResults[i];

                           if (result.success)
                           {
                               //Console.WriteLine("\nUpsert succeeded.");
                               acerto2.AppendLine(lista_processada2.ToArray()[i].ToString() + ";" + (result.created ? "Inserted" : "Updated") + ";" + result.id);
                           }
                           else
                           {
                               //Console.WriteLine("The Upsert failed because: " + result.errors[0].message);
                               erro2.AppendLine(lista_processada2.ToArray()[i].ToString() + ";" + result.errors[0].message);
                           }
                       }
                   },
                   () =>
                   {
                       basicSample_cs_p.apex.UpsertResult[] upsertResults = binding.upsert("RUT__c", lst_billing3.ToArray());

                       for (int i = 0; i < upsertResults.Length; i++)
                       {
                           basicSample_cs_p.apex.UpsertResult result = upsertResults[i];

                           if (result.success)
                           {
                               //Console.WriteLine("\nUpsert succeeded.");
                               acerto3.AppendLine(lista_processada3.ToArray()[i].ToString() + ";" + (result.created ? "Inserted" : "Updated") + ";" + result.id);
                           }
                           else
                           {
                               //Console.WriteLine("The Upsert failed because: " + result.errors[0].message);
                               erro3.AppendLine(lista_processada3.ToArray()[i].ToString() + ";" + result.errors[0].message);
                           }
                       }
                   },
                   () =>
                   {
                       basicSample_cs_p.apex.UpsertResult[] upsertResults = binding.upsert("RUT__c", lst_billing4.ToArray());

                       for (int i = 0; i < upsertResults.Length; i++)
                       {
                           basicSample_cs_p.apex.UpsertResult result = upsertResults[i];

                           if (result.success)
                           {
                               //Console.WriteLine("\nUpsert succeeded.");
                               acerto4.AppendLine(lista_processada4.ToArray()[i].ToString() + ";" + (result.created ? "Inserted" : "Updated") + ";" + result.id);
                           }
                           else
                           {
                               //Console.WriteLine("The Upsert failed because: " + result.errors[0].message);
                               erro4.AppendLine(lista_processada4.ToArray()[i].ToString() + ";" + result.errors[0].message);
                           }
                       }
                   },
                   () =>
                   {
                       basicSample_cs_p.apex.UpsertResult[] upsertResults = binding.upsert("RUT__c", lst_billing5.ToArray());

                       for (int i = 0; i < upsertResults.Length; i++)
                       {
                           basicSample_cs_p.apex.UpsertResult result = upsertResults[i];

                           if (result.success)
                           {
                               //Console.WriteLine("\nUpsert succeeded.");
                               acerto5.AppendLine(lista_processada5.ToArray()[i].ToString() + ";" + (result.created ? "Inserted" : "Updated") + ";" + result.id);
                           }
                           else
                           {
                               //Console.WriteLine("The Upsert failed because: " + result.errors[0].message);
                               erro5.AppendLine(lista_processada5.ToArray()[i].ToString() + ";" + result.errors[0].message);
                           }
                       }
                   },
                   () =>
                   {
                       basicSample_cs_p.apex.UpsertResult[] upsertResults = binding.upsert("RUT__c", lst_billing6.ToArray());

                       for (int i = 0; i < upsertResults.Length; i++)
                       {
                           basicSample_cs_p.apex.UpsertResult result = upsertResults[i];

                           if (result.success)
                           {
                               //Console.WriteLine("\nUpsert succeeded.");
                               acerto6.AppendLine(lista_processada6.ToArray()[i].ToString() + ";" + (result.created ? "Inserted" : "Updated") + ";" + result.id);
                           }
                           else
                           {
                               //Console.WriteLine("The Upsert failed because: " + result.errors[0].message);
                               erro6.AppendLine(lista_processada6.ToArray()[i].ToString() + ";" + result.errors[0].message);
                           }
                       }
                   });

                EscreverArquivo("resultado_positivo_invoice" + data, acerto.ToString());
                EscreverArquivo("resultado_positivo_invoice" + data, acerto2.ToString());
                EscreverArquivo("resultado_positivo_invoice" + data, acerto3.ToString());
                EscreverArquivo("resultado_positivo_invoice" + data, acerto4.ToString());
                EscreverArquivo("resultado_positivo_invoice" + data, acerto5.ToString());
                EscreverArquivo("resultado_positivo_invoice" + data, acerto6.ToString());

                EscreverArquivo("resultado_negativo_invoice" + data, erro.ToString());
                EscreverArquivo("resultado_negativo_invoice" + data, erro2.ToString());
                EscreverArquivo("resultado_negativo_invoice" + data, erro3.ToString());
                EscreverArquivo("resultado_negativo_invoice" + data, erro4.ToString());
                EscreverArquivo("resultado_negativo_invoice" + data, erro5.ToString());
                EscreverArquivo("resultado_negativo_invoice" + data, erro6.ToString());

                lst_billing.Clear();
                lst_billing2.Clear();
                lst_billing3.Clear();
                lst_billing4.Clear();
                lst_billing5.Clear();
                lst_billing6.Clear();

                acerto.Clear();
                acerto2.Clear();
                acerto3.Clear();
                acerto4.Clear();
                acerto5.Clear();
                acerto6.Clear();

                erro.Clear();
                erro2.Clear();
                erro3.Clear();
                erro4.Clear();
                erro5.Clear();
                erro6.Clear();


            }
            catch (Exception e)
            {
                EscreverArquivo("resultado_positivo_invoice" + data, acerto.ToString());
                EscreverArquivo("resultado_positivo_invoice" + data, acerto2.ToString());
                EscreverArquivo("resultado_positivo_invoice" + data, acerto3.ToString());
                EscreverArquivo("resultado_positivo_invoice" + data, acerto4.ToString());
                EscreverArquivo("resultado_positivo_invoice" + data, acerto5.ToString());
                EscreverArquivo("resultado_positivo_invoice" + data, acerto6.ToString());

                EscreverArquivo("resultado_negativo_invoice" + data, erro.ToString());
                EscreverArquivo("resultado_negativo_invoice" + data, erro2.ToString());
                EscreverArquivo("resultado_negativo_invoice" + data, erro3.ToString());
                EscreverArquivo("resultado_negativo_invoice" + data, erro4.ToString());
                EscreverArquivo("resultado_negativo_invoice" + data, erro5.ToString());
                EscreverArquivo("resultado_negativo_invoice" + data, erro6.ToString());

                EscreverArquivo("Erros_invoice_" + data, e.Message + "\n");
            }

            Console.WriteLine("Fim do Programa");
            Console.ReadLine();
        }


        private void UpsertAssetProdutos()
        {
            String data = DateTime.Now.ToString("yyyyMMddHHmmss");
            bool verifica = true;
            char sep = '\t';

            if (!loggedIn)
            {
                if (!login())
                    return;
            }

            StreamReader arquivo = new StreamReader(caminho);
            List<Asset> lst_billing = new List<Asset>();
            List<Asset> lst_billing2 = new List<Asset>();
            List<Asset> lst_billing3 = new List<Asset>();
            List<Asset> lst_billing4 = new List<Asset>();
            List<Asset> lst_billing5 = new List<Asset>();
            List<Asset> lst_billing6 = new List<Asset>();

            StringBuilder acerto = new StringBuilder();
            StringBuilder acerto2 = new StringBuilder();
            StringBuilder acerto3 = new StringBuilder();
            StringBuilder acerto4 = new StringBuilder();
            StringBuilder acerto5 = new StringBuilder();
            StringBuilder acerto6 = new StringBuilder();

            StringBuilder erro = new StringBuilder();
            StringBuilder erro2 = new StringBuilder();
            StringBuilder erro3 = new StringBuilder();
            StringBuilder erro4 = new StringBuilder();
            StringBuilder erro5 = new StringBuilder();
            StringBuilder erro6 = new StringBuilder();

            List<String> lista_processada = new List<String>();
            List<String> lista_processada2 = new List<String>();
            List<String> lista_processada3 = new List<String>();
            List<String> lista_processada4 = new List<String>();
            List<String> lista_processada5 = new List<String>();
            List<String> lista_processada6 = new List<String>();
            String _recebido = "";


            while (!arquivo.EndOfStream)
            {

                if (verifica)
                    _recebido = arquivo.ReadLine();
                else
                    verifica = true;

                String[] linha = _recebido.Split(sep);

                Asset _obj = new Asset();
                _obj.ExternalId__c = linha[0].ToString();
                _obj.Name = linha[1].ToString();
                _obj.Description = linha[5].ToString();
                _obj.NE__Zip_Code__c = linha[6].ToString();
                _obj.NE__Description__c = linha[7].ToString();
                _obj.Status = linha[8].ToString();
                _obj.SerialNumber = linha[10].ToString(); 
                _obj.Company__c = "AMPLA";
                
                _obj.Account = new Account();
                _obj.Account.ExternalId__c = linha[2].ToString(); ;

                _obj.PointofDelivery__r = new PointofDelivery__c();
                _obj.PointofDelivery__r.ExternalId__c = linha[4].ToString();

                //_obj.Account:Account: ExternalId__c = "";
                //_obj.PointofDelivery__r:PointofDelivery__c: ExternalId__c = "";


                if (lst_billing.Count < 100)
                {
                    lst_billing.Add(_obj);
                    lista_processada.Add(_recebido);
                }
                else if (lst_billing2.Count < 100)
                {
                    lst_billing2.Add(_obj);
                    lista_processada2.Add(_recebido);
                }
                else if (lst_billing3.Count < 100)
                {
                    lst_billing3.Add(_obj);
                    lista_processada3.Add(_recebido);
                }
                else if (lst_billing4.Count < 100)
                {
                    lst_billing4.Add(_obj);
                    lista_processada4.Add(_recebido);
                }
                else if (lst_billing5.Count < 100)
                {
                    lst_billing5.Add(_obj);
                    lista_processada5.Add(_recebido);
                }
                else if (lst_billing6.Count < 100)
                {
                    lst_billing6.Add(_obj);
                    lista_processada6.Add(_recebido);
                }
                else
                {
                    try
                    {
                        Parallel.Invoke(
                        () =>
                        {
                            basicSample_cs_p.apex.UpsertResult[] upsertResults = binding.upsert("ExternalId__c", lst_billing.ToArray());

                            for (int i = 0; i < upsertResults.Length; i++)
                            {
                                basicSample_cs_p.apex.UpsertResult result = upsertResults[i];

                                if (result.success)
                                {
                                    //Console.WriteLine("\nUpsert succeeded.");
                                    acerto.AppendLine(lista_processada.ToArray()[i].ToString() + ";" + (result.created ? "Inserted" : "Updated") + ";" + result.id);
                                }
                                else
                                {
                                    //Console.WriteLine("The Upsert failed because: " + result.errors[0].message);
                                    erro.AppendLine(lista_processada.ToArray()[i].ToString() + ";" + result.errors[0].message);
                                }
                            }
                        },
                        () =>
                        {
                            basicSample_cs_p.apex.UpsertResult[] upsertResults = binding.upsert("ExternalId__c", lst_billing2.ToArray());

                            for (int i = 0; i < upsertResults.Length; i++)
                            {
                                basicSample_cs_p.apex.UpsertResult result = upsertResults[i];

                                if (result.success)
                                {
                                    //Console.WriteLine("\nUpsert succeeded.");
                                    acerto2.AppendLine(lista_processada2.ToArray()[i].ToString() + ";" + (result.created ? "Inserted" : "Updated") + ";" + result.id);
                                }
                                else
                                {
                                    //Console.WriteLine("The Upsert failed because: " + result.errors[0].message);
                                    erro2.AppendLine(lista_processada2.ToArray()[i].ToString() + ";" + result.errors[0].message);
                                }
                            }
                        },
                        () =>
                        {
                            basicSample_cs_p.apex.UpsertResult[] upsertResults = binding.upsert("ExternalId__c", lst_billing3.ToArray());

                            for (int i = 0; i < upsertResults.Length; i++)
                            {
                                basicSample_cs_p.apex.UpsertResult result = upsertResults[i];

                                if (result.success)
                                {
                                    //Console.WriteLine("\nUpsert succeeded.");
                                    acerto3.AppendLine(lista_processada3.ToArray()[i].ToString() + ";" + (result.created ? "Inserted" : "Updated") + ";" + result.id);
                                }
                                else
                                {
                                    //Console.WriteLine("The Upsert failed because: " + result.errors[0].message);
                                    erro3.AppendLine(lista_processada3.ToArray()[i].ToString() + ";" + result.errors[0].message);
                                }
                            }
                        },
                        () =>
                        {
                            basicSample_cs_p.apex.UpsertResult[] upsertResults = binding.upsert("ExternalId__c", lst_billing4.ToArray());

                            for (int i = 0; i < upsertResults.Length; i++)
                            {
                                basicSample_cs_p.apex.UpsertResult result = upsertResults[i];

                                if (result.success)
                                {
                                    //Console.WriteLine("\nUpsert succeeded.");
                                    acerto4.AppendLine(lista_processada4.ToArray()[i].ToString() + ";" + (result.created ? "Inserted" : "Updated") + ";" + result.id);
                                }
                                else
                                {
                                    //Console.WriteLine("The Upsert failed because: " + result.errors[0].message);
                                    erro4.AppendLine(lista_processada4.ToArray()[i].ToString() + ";" + result.errors[0].message);
                                }
                            }
                        },
                        () =>
                        {
                            basicSample_cs_p.apex.UpsertResult[] upsertResults = binding.upsert("ExternalId__c", lst_billing5.ToArray());

                            for (int i = 0; i < upsertResults.Length; i++)
                            {
                                basicSample_cs_p.apex.UpsertResult result = upsertResults[i];

                                if (result.success)
                                {
                                    //Console.WriteLine("\nUpsert succeeded.");
                                    acerto5.AppendLine(lista_processada5.ToArray()[i].ToString() + ";" + (result.created ? "Inserted" : "Updated") + ";" + result.id);
                                }
                                else
                                {
                                    //Console.WriteLine("The Upsert failed because: " + result.errors[0].message);
                                    erro5.AppendLine(lista_processada5.ToArray()[i].ToString() + ";" + result.errors[0].message);
                                }
                            }
                        },
                        () =>
                        {
                            basicSample_cs_p.apex.UpsertResult[] upsertResults = binding.upsert("ExternalId__c", lst_billing6.ToArray());

                            for (int i = 0; i < upsertResults.Length; i++)
                            {
                                basicSample_cs_p.apex.UpsertResult result = upsertResults[i];

                                if (result.success)
                                {
                                    //Console.WriteLine("\nUpsert succeeded.");
                                    acerto6.AppendLine(lista_processada6.ToArray()[i].ToString() + ";" + (result.created ? "Inserted" : "Updated") + ";" + result.id);
                                }
                                else
                                {
                                    //Console.WriteLine("The Upsert failed because: " + result.errors[0].message);
                                    erro6.AppendLine(lista_processada6.ToArray()[i].ToString() + ";" + result.errors[0].message);
                                }
                            }
                        });

                        EscreverArquivo("resultado_positivo_asset" + data, acerto.ToString());
                        EscreverArquivo("resultado_positivo_asset" + data, acerto2.ToString());
                        EscreverArquivo("resultado_positivo_asset" + data, acerto3.ToString());
                        EscreverArquivo("resultado_positivo_asset" + data, acerto4.ToString());
                        EscreverArquivo("resultado_positivo_asset" + data, acerto5.ToString());
                        EscreverArquivo("resultado_positivo_asset" + data, acerto6.ToString());

                        EscreverArquivo("resultado_negativo_asset" + data, erro.ToString());
                        EscreverArquivo("resultado_negativo_asset" + data, erro2.ToString());
                        EscreverArquivo("resultado_negativo_asset" + data, erro3.ToString());
                        EscreverArquivo("resultado_negativo_asset" + data, erro4.ToString());
                        EscreverArquivo("resultado_negativo_asset" + data, erro5.ToString());
                        EscreverArquivo("resultado_negativo_asset" + data, erro6.ToString());

                        lst_billing.Clear();
                        lst_billing2.Clear();
                        lst_billing3.Clear();
                        lst_billing4.Clear();
                        lst_billing5.Clear();
                        lst_billing6.Clear();

                        acerto.Clear();
                        acerto2.Clear();
                        acerto3.Clear();
                        acerto4.Clear();
                        acerto5.Clear();
                        acerto6.Clear();

                        erro.Clear();
                        erro2.Clear();
                        erro3.Clear();
                        erro4.Clear();
                        erro5.Clear();
                        erro6.Clear();

                        lista_processada.Clear();
                        lista_processada2.Clear();
                        lista_processada3.Clear();
                        lista_processada4.Clear();
                        lista_processada5.Clear();
                        lista_processada6.Clear();
                        verifica = false;

                    }
                    catch (Exception)
                    {
                        EscreverArquivo("Erros_Asset_" + data, lst_billing.ToArray()[0].ExternalId__c + " até " + lst_billing6.ToArray()[49].ExternalId__c + "\n");
                    }
                }

            }


            try
            {
                //EscreverArquivo("Avanco_billing_" + data, lst_billing6.ToArray()[99].Id + "\n");

                Parallel.Invoke(
                () =>
                {
                    basicSample_cs_p.apex.UpsertResult[] upsertResults = binding.upsert("ExternalId__c", lst_billing.ToArray());

                    for (int i = 0; i < upsertResults.Length; i++)
                    {
                        basicSample_cs_p.apex.UpsertResult result = upsertResults[i];

                        if (result.success)
                        {
                            //Console.WriteLine("\nUpsert succeeded.");
                            acerto.AppendLine(lista_processada.ToArray()[i].ToString() + ";" + (result.created ? "Inserted" : "Updated") + ";" + result.id);
                        }
                        else
                        {
                            //Console.WriteLine("The Upsert failed because: " + result.errors[0].message);
                            erro.AppendLine(lista_processada.ToArray()[i].ToString() + ";" + result.errors[0].message);
                        }
                    }
                },
                   () =>
                   {
                       basicSample_cs_p.apex.UpsertResult[] upsertResults = binding.upsert("ExternalId__c", lst_billing2.ToArray());

                       for (int i = 0; i < upsertResults.Length; i++)
                       {
                           basicSample_cs_p.apex.UpsertResult result = upsertResults[i];

                           if (result.success)
                           {
                               //Console.WriteLine("\nUpsert succeeded.");
                               acerto2.AppendLine(lista_processada2.ToArray()[i].ToString() + ";" + (result.created ? "Inserted" : "Updated") + ";" + result.id);
                           }
                           else
                           {
                               //Console.WriteLine("The Upsert failed because: " + result.errors[0].message);
                               erro2.AppendLine(lista_processada2.ToArray()[i].ToString() + ";" + result.errors[0].message);
                           }
                       }
                   },
                   () =>
                   {
                       basicSample_cs_p.apex.UpsertResult[] upsertResults = binding.upsert("ExternalId__c", lst_billing3.ToArray());

                       for (int i = 0; i < upsertResults.Length; i++)
                       {
                           basicSample_cs_p.apex.UpsertResult result = upsertResults[i];

                           if (result.success)
                           {
                               //Console.WriteLine("\nUpsert succeeded.");
                               acerto3.AppendLine(lista_processada3.ToArray()[i].ToString() + ";" + (result.created ? "Inserted" : "Updated") + ";" + result.id);
                           }
                           else
                           {
                               //Console.WriteLine("The Upsert failed because: " + result.errors[0].message);
                               erro3.AppendLine(lista_processada3.ToArray()[i].ToString() + ";" + result.errors[0].message);
                           }
                       }
                   },
                   () =>
                   {
                       basicSample_cs_p.apex.UpsertResult[] upsertResults = binding.upsert("ExternalId__c", lst_billing4.ToArray());

                       for (int i = 0; i < upsertResults.Length; i++)
                       {
                           basicSample_cs_p.apex.UpsertResult result = upsertResults[i];

                           if (result.success)
                           {
                               //Console.WriteLine("\nUpsert succeeded.");
                               acerto4.AppendLine(lista_processada4.ToArray()[i].ToString() + ";" + (result.created ? "Inserted" : "Updated") + ";" + result.id);
                           }
                           else
                           {
                               //Console.WriteLine("The Upsert failed because: " + result.errors[0].message);
                               erro4.AppendLine(lista_processada4.ToArray()[i].ToString() + ";" + result.errors[0].message);
                           }
                       }
                   },
                   () =>
                   {
                       basicSample_cs_p.apex.UpsertResult[] upsertResults = binding.upsert("ExternalId__c", lst_billing5.ToArray());

                       for (int i = 0; i < upsertResults.Length; i++)
                       {
                           basicSample_cs_p.apex.UpsertResult result = upsertResults[i];

                           if (result.success)
                           {
                               //Console.WriteLine("\nUpsert succeeded.");
                               acerto5.AppendLine(lista_processada5.ToArray()[i].ToString() + ";" + (result.created ? "Inserted" : "Updated") + ";" + result.id);
                           }
                           else
                           {
                               //Console.WriteLine("The Upsert failed because: " + result.errors[0].message);
                               erro5.AppendLine(lista_processada5.ToArray()[i].ToString() + ";" + result.errors[0].message);
                           }
                       }
                   },
                   () =>
                   {
                       basicSample_cs_p.apex.UpsertResult[] upsertResults = binding.upsert("ExternalId__c", lst_billing6.ToArray());

                       for (int i = 0; i < upsertResults.Length; i++)
                       {
                           basicSample_cs_p.apex.UpsertResult result = upsertResults[i];

                           if (result.success)
                           {
                               //Console.WriteLine("\nUpsert succeeded.");
                               acerto6.AppendLine(lista_processada6.ToArray()[i].ToString() + ";" + (result.created ? "Inserted" : "Updated") + ";" + result.id);
                           }
                           else
                           {
                               //Console.WriteLine("The Upsert failed because: " + result.errors[0].message);
                               erro6.AppendLine(lista_processada6.ToArray()[i].ToString() + ";" + result.errors[0].message);
                           }
                       }
                   });

                EscreverArquivo("resultado_positivo_asset" + data, acerto.ToString());
                EscreverArquivo("resultado_positivo_asset" + data, acerto2.ToString());
                EscreverArquivo("resultado_positivo_asset" + data, acerto3.ToString());
                EscreverArquivo("resultado_positivo_asset" + data, acerto4.ToString());
                EscreverArquivo("resultado_positivo_asset" + data, acerto5.ToString());
                EscreverArquivo("resultado_positivo_asset" + data, acerto6.ToString());

                EscreverArquivo("resultado_negativo_asset" + data, erro.ToString());
                EscreverArquivo("resultado_negativo_asset" + data, erro2.ToString());
                EscreverArquivo("resultado_negativo_asset" + data, erro3.ToString());
                EscreverArquivo("resultado_negativo_asset" + data, erro4.ToString());
                EscreverArquivo("resultado_negativo_asset" + data, erro5.ToString());
                EscreverArquivo("resultado_negativo_asset" + data, erro6.ToString());

                lst_billing.Clear();
                lst_billing2.Clear();
                lst_billing3.Clear();
                lst_billing4.Clear();
                lst_billing5.Clear();
                lst_billing6.Clear();

                acerto.Clear();
                acerto2.Clear();
                acerto3.Clear();
                acerto4.Clear();
                acerto5.Clear();
                acerto6.Clear();

                erro.Clear();
                erro2.Clear();
                erro3.Clear();
                erro4.Clear();
                erro5.Clear();
                erro6.Clear();


            }
            catch (Exception e)
            {
                EscreverArquivo("resultado_positivo_asset" + data, acerto.ToString());
                EscreverArquivo("resultado_positivo_asset" + data, acerto2.ToString());
                EscreverArquivo("resultado_positivo_asset" + data, acerto3.ToString());
                EscreverArquivo("resultado_positivo_asset" + data, acerto4.ToString());
                EscreverArquivo("resultado_positivo_asset" + data, acerto5.ToString());
                EscreverArquivo("resultado_positivo_asset" + data, acerto6.ToString());

                EscreverArquivo("resultado_negativo_asset" + data, erro.ToString());
                EscreverArquivo("resultado_negativo_asset" + data, erro2.ToString());
                EscreverArquivo("resultado_negativo_asset" + data, erro3.ToString());
                EscreverArquivo("resultado_negativo_asset" + data, erro4.ToString());
                EscreverArquivo("resultado_negativo_asset" + data, erro5.ToString());
                EscreverArquivo("resultado_negativo_asset" + data, erro6.ToString());

                EscreverArquivo("Erros_mae_" + data, e.Message + "\n");
            }

            Console.WriteLine("Fim do Programa");
            Console.ReadLine();
        }

        private void UpsertDevice()
        {
            String data = DateTime.Now.ToString("yyyyMMddHHmmss");
            bool verifica = true;
            char sep = '|';

            if (!loggedIn)
            {
                if (!login())
                    return;
            }

            StreamReader arquivo = new StreamReader(caminho);
            List<Device__c> lst_generica = new List<Device__c>();
            List<Device__c> lst_generica2 = new List<Device__c>();
            List<Device__c> lst_generica3 = new List<Device__c>();
            List<Device__c> lst_generica4 = new List<Device__c>();
            List<Device__c> lst_generica5 = new List<Device__c>();
            List<Device__c> lst_generica6 = new List<Device__c>();

            StringBuilder acerto = new StringBuilder();
            StringBuilder acerto2 = new StringBuilder();
            StringBuilder acerto3 = new StringBuilder();
            StringBuilder acerto4 = new StringBuilder();
            StringBuilder acerto5 = new StringBuilder();
            StringBuilder acerto6 = new StringBuilder();

            StringBuilder erro = new StringBuilder();
            StringBuilder erro2 = new StringBuilder();
            StringBuilder erro3 = new StringBuilder();
            StringBuilder erro4 = new StringBuilder();
            StringBuilder erro5 = new StringBuilder();
            StringBuilder erro6 = new StringBuilder();

            List<String> lista_processada = new List<String>();
            List<String> lista_processada2 = new List<String>();
            List<String> lista_processada3 = new List<String>();
            List<String> lista_processada4 = new List<String>();
            List<String> lista_processada5 = new List<String>();
            List<String> lista_processada6 = new List<String>();
            String _recebido = "";


            while (!arquivo.EndOfStream)
            {

                if (verifica)
                    _recebido = arquivo.ReadLine();
                else
                    verifica = true;

                String[] linha = _recebido.Split(sep);

                Device__c _obj = new Device__c();
                _obj.MeterBrand__c = linha[0].ToString();
                _obj.MeterModel__c = linha[1].ToString();
                _obj.MeterNumber__c = linha[2].ToString();
                _obj.MeterType__c = linha[3].ToString();
                _obj.MeasureType__c = linha[4].ToString();
                _obj.ExternalID__c = linha[6].ToString();

                if (!string.IsNullOrEmpty(linha[7].ToString().Trim()))
                {
                    _obj.Instalation_date__c = Convert.ToDateTime(linha[7].ToString());
                    _obj.Instalation_date__cSpecified = true;
                }
                
                _obj.Cubicle__c = linha[8].ToString();

                if (!string.IsNullOrEmpty(linha[9].ToString().Trim()))
                { 
                    _obj.Retirement_date__c = Convert.ToDateTime(linha[9].ToString());
                    _obj.Retirement_date__cSpecified = true;
                }

                _obj.Status__c = linha[10].ToString();
                _obj.CreatedYear__c = Convert.ToInt32(linha[11].ToString());

                _obj.PointofDelivery__r = new PointofDelivery__c();
                _obj.PointofDelivery__r.ExternalId__c = linha[5].ToString();


                if (lst_generica.Count < 100)
                {
                    lst_generica.Add(_obj);
                    lista_processada.Add(_recebido);
                }
                else if (lst_generica2.Count < 100)
                {
                    lst_generica2.Add(_obj);
                    lista_processada2.Add(_recebido);
                }
                else if (lst_generica3.Count < 100)
                {
                    lst_generica3.Add(_obj);
                    lista_processada3.Add(_recebido);
                }
                else if (lst_generica4.Count < 100)
                {
                    lst_generica4.Add(_obj);
                    lista_processada4.Add(_recebido);
                }
                else if (lst_generica5.Count < 100)
                {
                    lst_generica5.Add(_obj);
                    lista_processada5.Add(_recebido);
                }
                else if (lst_generica6.Count < 100)
                {
                    lst_generica6.Add(_obj);
                    lista_processada6.Add(_recebido);
                }
                else
                {
                    try
                    {
                        Parallel.Invoke(
                        () =>
                        {
                            basicSample_cs_p.apex.UpsertResult[] upsertResults = binding.upsert("ExternalId__c", lst_generica.ToArray());

                            for (int i = 0; i < upsertResults.Length; i++)
                            {
                                basicSample_cs_p.apex.UpsertResult result = upsertResults[i];

                                if (result.success)
                                {
                                    //Console.WriteLine("\nUpsert succeeded.");
                                    acerto.AppendLine(lista_processada.ToArray()[i].ToString() + ";" + (result.created ? "Inserted" : "Updated") + ";" + result.id);
                                }
                                else
                                {
                                    //Console.WriteLine("The Upsert failed because: " + result.errors[0].message);
                                    erro.AppendLine(lista_processada.ToArray()[i].ToString() + ";" + result.errors[0].message);
                                }
                            }
                        },
                        () =>
                        {
                            basicSample_cs_p.apex.UpsertResult[] upsertResults = binding.upsert("ExternalId__c", lst_generica2.ToArray());

                            for (int i = 0; i < upsertResults.Length; i++)
                            {
                                basicSample_cs_p.apex.UpsertResult result = upsertResults[i];

                                if (result.success)
                                {
                                    //Console.WriteLine("\nUpsert succeeded.");
                                    acerto2.AppendLine(lista_processada2.ToArray()[i].ToString() + ";" + (result.created ? "Inserted" : "Updated") + ";" + result.id);
                                }
                                else
                                {
                                    //Console.WriteLine("The Upsert failed because: " + result.errors[0].message);
                                    erro2.AppendLine(lista_processada2.ToArray()[i].ToString() + ";" + result.errors[0].message);
                                }
                            }
                        },
                        () =>
                        {
                            basicSample_cs_p.apex.UpsertResult[] upsertResults = binding.upsert("ExternalId__c", lst_generica3.ToArray());

                            for (int i = 0; i < upsertResults.Length; i++)
                            {
                                basicSample_cs_p.apex.UpsertResult result = upsertResults[i];

                                if (result.success)
                                {
                                    //Console.WriteLine("\nUpsert succeeded.");
                                    acerto3.AppendLine(lista_processada3.ToArray()[i].ToString() + ";" + (result.created ? "Inserted" : "Updated") + ";" + result.id);
                                }
                                else
                                {
                                    //Console.WriteLine("The Upsert failed because: " + result.errors[0].message);
                                    erro3.AppendLine(lista_processada3.ToArray()[i].ToString() + ";" + result.errors[0].message);
                                }
                            }
                        },
                        () =>
                        {
                            basicSample_cs_p.apex.UpsertResult[] upsertResults = binding.upsert("ExternalId__c", lst_generica4.ToArray());

                            for (int i = 0; i < upsertResults.Length; i++)
                            {
                                basicSample_cs_p.apex.UpsertResult result = upsertResults[i];

                                if (result.success)
                                {
                                    //Console.WriteLine("\nUpsert succeeded.");
                                    acerto4.AppendLine(lista_processada4.ToArray()[i].ToString() + ";" + (result.created ? "Inserted" : "Updated") + ";" + result.id);
                                }
                                else
                                {
                                    //Console.WriteLine("The Upsert failed because: " + result.errors[0].message);
                                    erro4.AppendLine(lista_processada4.ToArray()[i].ToString() + ";" + result.errors[0].message);
                                }
                            }
                        },
                        () =>
                        {
                            basicSample_cs_p.apex.UpsertResult[] upsertResults = binding.upsert("ExternalId__c", lst_generica5.ToArray());

                            for (int i = 0; i < upsertResults.Length; i++)
                            {
                                basicSample_cs_p.apex.UpsertResult result = upsertResults[i];

                                if (result.success)
                                {
                                    //Console.WriteLine("\nUpsert succeeded.");
                                    acerto5.AppendLine(lista_processada5.ToArray()[i].ToString() + ";" + (result.created ? "Inserted" : "Updated") + ";" + result.id);
                                }
                                else
                                {
                                    //Console.WriteLine("The Upsert failed because: " + result.errors[0].message);
                                    erro5.AppendLine(lista_processada5.ToArray()[i].ToString() + ";" + result.errors[0].message);
                                }
                            }
                        },
                        () =>
                        {
                            basicSample_cs_p.apex.UpsertResult[] upsertResults = binding.upsert("ExternalId__c", lst_generica6.ToArray());

                            for (int i = 0; i < upsertResults.Length; i++)
                            {
                                basicSample_cs_p.apex.UpsertResult result = upsertResults[i];

                                if (result.success)
                                {
                                    //Console.WriteLine("\nUpsert succeeded.");
                                    acerto6.AppendLine(lista_processada6.ToArray()[i].ToString() + ";" + (result.created ? "Inserted" : "Updated") + ";" + result.id);
                                }
                                else
                                {
                                    //Console.WriteLine("The Upsert failed because: " + result.errors[0].message);
                                    erro6.AppendLine(lista_processada6.ToArray()[i].ToString() + ";" + result.errors[0].message);
                                }
                            }
                        });

                        EscreverArquivo("resultado_positivo_device" + data, acerto.ToString());
                        EscreverArquivo("resultado_positivo_device" + data, acerto2.ToString());
                        EscreverArquivo("resultado_positivo_device" + data, acerto3.ToString());
                        EscreverArquivo("resultado_positivo_device" + data, acerto4.ToString());
                        EscreverArquivo("resultado_positivo_device" + data, acerto5.ToString());
                        EscreverArquivo("resultado_positivo_device" + data, acerto6.ToString());

                        EscreverArquivo("resultado_negativo_device" + data, erro.ToString());
                        EscreverArquivo("resultado_negativo_device" + data, erro2.ToString());
                        EscreverArquivo("resultado_negativo_device" + data, erro3.ToString());
                        EscreverArquivo("resultado_negativo_device" + data, erro4.ToString());
                        EscreverArquivo("resultado_negativo_device" + data, erro5.ToString());
                        EscreverArquivo("resultado_negativo_device" + data, erro6.ToString());

                        lst_generica.Clear();
                        lst_generica2.Clear();
                        lst_generica3.Clear();
                        lst_generica4.Clear();
                        lst_generica5.Clear();
                        lst_generica6.Clear();

                        acerto.Clear();
                        acerto2.Clear();
                        acerto3.Clear();
                        acerto4.Clear();
                        acerto5.Clear();
                        acerto6.Clear();

                        erro.Clear();
                        erro2.Clear();
                        erro3.Clear();
                        erro4.Clear();
                        erro5.Clear();
                        erro6.Clear();

                        lista_processada.Clear();
                        lista_processada2.Clear();
                        lista_processada3.Clear();
                        lista_processada4.Clear();
                        lista_processada5.Clear();
                        lista_processada6.Clear();
                        verifica = false;

                    }
                    catch (Exception)
                    {
                        EscreverArquivo("Erros_Device__c_" + data, lst_generica.ToArray()[0].ExternalID__c + " até " + lst_generica6.ToArray()[49].ExternalID__c + "\n");
                    }
                }

            }


            try
            {
                //EscreverArquivo("Avanco_billing_" + data, lst_generica6.ToArray()[99].Id + "\n");

                Parallel.Invoke(
                () =>
                {
                    basicSample_cs_p.apex.UpsertResult[] upsertResults = binding.upsert("ExternalId__c", lst_generica.ToArray());

                    for (int i = 0; i < upsertResults.Length; i++)
                    {
                        basicSample_cs_p.apex.UpsertResult result = upsertResults[i];

                        if (result.success)
                        {
                            //Console.WriteLine("\nUpsert succeeded.");
                            acerto.AppendLine(lista_processada.ToArray()[i].ToString() + ";" + (result.created ? "Inserted" : "Updated") + ";" + result.id);
                        }
                        else
                        {
                            //Console.WriteLine("The Upsert failed because: " + result.errors[0].message);
                            erro.AppendLine(lista_processada.ToArray()[i].ToString() + ";" + result.errors[0].message);
                        }
                    }
                },
                   () =>
                   {
                       basicSample_cs_p.apex.UpsertResult[] upsertResults = binding.upsert("ExternalId__c", lst_generica2.ToArray());

                       for (int i = 0; i < upsertResults.Length; i++)
                       {
                           basicSample_cs_p.apex.UpsertResult result = upsertResults[i];

                           if (result.success)
                           {
                               //Console.WriteLine("\nUpsert succeeded.");
                               acerto2.AppendLine(lista_processada2.ToArray()[i].ToString() + ";" + (result.created ? "Inserted" : "Updated") + ";" + result.id);
                           }
                           else
                           {
                               //Console.WriteLine("The Upsert failed because: " + result.errors[0].message);
                               erro2.AppendLine(lista_processada2.ToArray()[i].ToString() + ";" + result.errors[0].message);
                           }
                       }
                   },
                   () =>
                   {
                       basicSample_cs_p.apex.UpsertResult[] upsertResults = binding.upsert("ExternalId__c", lst_generica3.ToArray());

                       for (int i = 0; i < upsertResults.Length; i++)
                       {
                           basicSample_cs_p.apex.UpsertResult result = upsertResults[i];

                           if (result.success)
                           {
                               //Console.WriteLine("\nUpsert succeeded.");
                               acerto3.AppendLine(lista_processada3.ToArray()[i].ToString() + ";" + (result.created ? "Inserted" : "Updated") + ";" + result.id);
                           }
                           else
                           {
                               //Console.WriteLine("The Upsert failed because: " + result.errors[0].message);
                               erro3.AppendLine(lista_processada3.ToArray()[i].ToString() + ";" + result.errors[0].message);
                           }
                       }
                   },
                   () =>
                   {
                       basicSample_cs_p.apex.UpsertResult[] upsertResults = binding.upsert("ExternalId__c", lst_generica4.ToArray());

                       for (int i = 0; i < upsertResults.Length; i++)
                       {
                           basicSample_cs_p.apex.UpsertResult result = upsertResults[i];

                           if (result.success)
                           {
                               //Console.WriteLine("\nUpsert succeeded.");
                               acerto4.AppendLine(lista_processada4.ToArray()[i].ToString() + ";" + (result.created ? "Inserted" : "Updated") + ";" + result.id);
                           }
                           else
                           {
                               //Console.WriteLine("The Upsert failed because: " + result.errors[0].message);
                               erro4.AppendLine(lista_processada4.ToArray()[i].ToString() + ";" + result.errors[0].message);
                           }
                       }
                   },
                   () =>
                   {
                       basicSample_cs_p.apex.UpsertResult[] upsertResults = binding.upsert("ExternalId__c", lst_generica5.ToArray());

                       for (int i = 0; i < upsertResults.Length; i++)
                       {
                           basicSample_cs_p.apex.UpsertResult result = upsertResults[i];

                           if (result.success)
                           {
                               //Console.WriteLine("\nUpsert succeeded.");
                               acerto5.AppendLine(lista_processada5.ToArray()[i].ToString() + ";" + (result.created ? "Inserted" : "Updated") + ";" + result.id);
                           }
                           else
                           {
                               //Console.WriteLine("The Upsert failed because: " + result.errors[0].message);
                               erro5.AppendLine(lista_processada5.ToArray()[i].ToString() + ";" + result.errors[0].message);
                           }
                       }
                   },
                   () =>
                   {
                       basicSample_cs_p.apex.UpsertResult[] upsertResults = binding.upsert("ExternalId__c", lst_generica6.ToArray());

                       for (int i = 0; i < upsertResults.Length; i++)
                       {
                           basicSample_cs_p.apex.UpsertResult result = upsertResults[i];

                           if (result.success)
                           {
                               //Console.WriteLine("\nUpsert succeeded.");
                               acerto6.AppendLine(lista_processada6.ToArray()[i].ToString() + ";" + (result.created ? "Inserted" : "Updated") + ";" + result.id);
                           }
                           else
                           {
                               //Console.WriteLine("The Upsert failed because: " + result.errors[0].message);
                               erro6.AppendLine(lista_processada6.ToArray()[i].ToString() + ";" + result.errors[0].message);
                           }
                       }
                   });

                EscreverArquivo("resultado_positivo_device" + data, acerto.ToString());
                EscreverArquivo("resultado_positivo_device" + data, acerto2.ToString());
                EscreverArquivo("resultado_positivo_device" + data, acerto3.ToString());
                EscreverArquivo("resultado_positivo_device" + data, acerto4.ToString());
                EscreverArquivo("resultado_positivo_device" + data, acerto5.ToString());
                EscreverArquivo("resultado_positivo_device" + data, acerto6.ToString());

                EscreverArquivo("resultado_negativo_device" + data, erro.ToString());
                EscreverArquivo("resultado_negativo_device" + data, erro2.ToString());
                EscreverArquivo("resultado_negativo_device" + data, erro3.ToString());
                EscreverArquivo("resultado_negativo_device" + data, erro4.ToString());
                EscreverArquivo("resultado_negativo_device" + data, erro5.ToString());
                EscreverArquivo("resultado_negativo_device" + data, erro6.ToString());

                lst_generica.Clear();
                lst_generica2.Clear();
                lst_generica3.Clear();
                lst_generica4.Clear();
                lst_generica5.Clear();
                lst_generica6.Clear();

                acerto.Clear();
                acerto2.Clear();
                acerto3.Clear();
                acerto4.Clear();
                acerto5.Clear();
                acerto6.Clear();

                erro.Clear();
                erro2.Clear();
                erro3.Clear();
                erro4.Clear();
                erro5.Clear();
                erro6.Clear();


            }
            catch (Exception e)
            {
                EscreverArquivo("resultado_positivo_device" + data, acerto.ToString());
                EscreverArquivo("resultado_positivo_device" + data, acerto2.ToString());
                EscreverArquivo("resultado_positivo_device" + data, acerto3.ToString());
                EscreverArquivo("resultado_positivo_device" + data, acerto4.ToString());
                EscreverArquivo("resultado_positivo_device" + data, acerto5.ToString());
                EscreverArquivo("resultado_positivo_device" + data, acerto6.ToString());

                EscreverArquivo("resultado_negativo_device" + data, erro.ToString());
                EscreverArquivo("resultado_negativo_device" + data, erro2.ToString());
                EscreverArquivo("resultado_negativo_device" + data, erro3.ToString());
                EscreverArquivo("resultado_negativo_device" + data, erro4.ToString());
                EscreverArquivo("resultado_negativo_device" + data, erro5.ToString());
                EscreverArquivo("resultado_negativo_device" + data, erro6.ToString());

                EscreverArquivo("Erros_device_" + data, e.Message + "\n");
            }

            Console.WriteLine("Fim do Programa");
            Console.ReadLine();
        }

        private void Upsertbilling()
        {
            String data = DateTime.Now.ToString("yyyyMMddHHmmss");
            bool verifica = true;
            char sep = '|';

            if (!loggedIn)
            {
                if (!login())
                    return;
            }

            StreamReader arquivo = new StreamReader(caminho);
            List<Billing_Profile__c> lst_generica = new List<Billing_Profile__c>();
            List<Billing_Profile__c> lst_generica2 = new List<Billing_Profile__c>();
            List<Billing_Profile__c> lst_generica3 = new List<Billing_Profile__c>();
            List<Billing_Profile__c> lst_generica4 = new List<Billing_Profile__c>();
            List<Billing_Profile__c> lst_generica5 = new List<Billing_Profile__c>();
            List<Billing_Profile__c> lst_generica6 = new List<Billing_Profile__c>();

            StringBuilder acerto = new StringBuilder();
            StringBuilder acerto2 = new StringBuilder();
            StringBuilder acerto3 = new StringBuilder();
            StringBuilder acerto4 = new StringBuilder();
            StringBuilder acerto5 = new StringBuilder();
            StringBuilder acerto6 = new StringBuilder();

            StringBuilder erro = new StringBuilder();
            StringBuilder erro2 = new StringBuilder();
            StringBuilder erro3 = new StringBuilder();
            StringBuilder erro4 = new StringBuilder();
            StringBuilder erro5 = new StringBuilder();
            StringBuilder erro6 = new StringBuilder();

            List<String> lista_processada = new List<String>();
            List<String> lista_processada2 = new List<String>();
            List<String> lista_processada3 = new List<String>();
            List<String> lista_processada4 = new List<String>();
            List<String> lista_processada5 = new List<String>();
            List<String> lista_processada6 = new List<String>();
            String _recebido = "";


            while (!arquivo.EndOfStream)
            {

                if (verifica)
                    _recebido = arquivo.ReadLine();
                else
                    verifica = true;

                String[] linha = _recebido.Split(sep);

                Billing_Profile__c _obj = new Billing_Profile__c();
                _obj.Id = linha[0].ToString();
                _obj.CurrencyIsoCode = linha[1].ToString();
                _obj.CNT_Due_Date__c = linha[2].ToString();

                if (lst_generica.Count < 100)
                {
                    lst_generica.Add(_obj);
                    lista_processada.Add(_recebido);
                }
                else if (lst_generica2.Count < 100)
                {
                    lst_generica2.Add(_obj);
                    lista_processada2.Add(_recebido);
                }
                else if (lst_generica3.Count < 100)
                {
                    lst_generica3.Add(_obj);
                    lista_processada3.Add(_recebido);
                }
                else if (lst_generica4.Count < 100)
                {
                    lst_generica4.Add(_obj);
                    lista_processada4.Add(_recebido);
                }
                else if (lst_generica5.Count < 100)
                {
                    lst_generica5.Add(_obj);
                    lista_processada5.Add(_recebido);
                }
                else if (lst_generica6.Count < 100)
                {
                    lst_generica6.Add(_obj);
                    lista_processada6.Add(_recebido);
                }
                else
                {
                    try
                    {
                        Parallel.Invoke(
                        () =>
                        {
                            basicSample_cs_p.apex.UpsertResult[] upsertResults = binding.upsert("Id", lst_generica.ToArray());

                            for (int i = 0; i < upsertResults.Length; i++)
                            {
                                basicSample_cs_p.apex.UpsertResult result = upsertResults[i];

                                if (result.success)
                                {
                                    //Console.WriteLine("\nUpsert succeeded.");
                                    acerto.AppendLine(lista_processada.ToArray()[i].ToString() + ";" + (result.created ? "Inserted" : "Updated") + ";" + result.id);
                                }
                                else
                                {
                                    //Console.WriteLine("The Upsert failed because: " + result.errors[0].message);
                                    erro.AppendLine(lista_processada.ToArray()[i].ToString() + ";" + result.errors[0].message);
                                }
                            }
                        },
                        () =>
                        {
                            basicSample_cs_p.apex.UpsertResult[] upsertResults = binding.upsert("Id", lst_generica2.ToArray());

                            for (int i = 0; i < upsertResults.Length; i++)
                            {
                                basicSample_cs_p.apex.UpsertResult result = upsertResults[i];

                                if (result.success)
                                {
                                    //Console.WriteLine("\nUpsert succeeded.");
                                    acerto2.AppendLine(lista_processada2.ToArray()[i].ToString() + ";" + (result.created ? "Inserted" : "Updated") + ";" + result.id);
                                }
                                else
                                {
                                    //Console.WriteLine("The Upsert failed because: " + result.errors[0].message);
                                    erro2.AppendLine(lista_processada2.ToArray()[i].ToString() + ";" + result.errors[0].message);
                                }
                            }
                        },
                        () =>
                        {
                            basicSample_cs_p.apex.UpsertResult[] upsertResults = binding.upsert("Id", lst_generica3.ToArray());

                            for (int i = 0; i < upsertResults.Length; i++)
                            {
                                basicSample_cs_p.apex.UpsertResult result = upsertResults[i];

                                if (result.success)
                                {
                                    //Console.WriteLine("\nUpsert succeeded.");
                                    acerto3.AppendLine(lista_processada3.ToArray()[i].ToString() + ";" + (result.created ? "Inserted" : "Updated") + ";" + result.id);
                                }
                                else
                                {
                                    //Console.WriteLine("The Upsert failed because: " + result.errors[0].message);
                                    erro3.AppendLine(lista_processada3.ToArray()[i].ToString() + ";" + result.errors[0].message);
                                }
                            }
                        },
                        () =>
                        {
                            basicSample_cs_p.apex.UpsertResult[] upsertResults = binding.upsert("Id", lst_generica4.ToArray());

                            for (int i = 0; i < upsertResults.Length; i++)
                            {
                                basicSample_cs_p.apex.UpsertResult result = upsertResults[i];

                                if (result.success)
                                {
                                    //Console.WriteLine("\nUpsert succeeded.");
                                    acerto4.AppendLine(lista_processada4.ToArray()[i].ToString() + ";" + (result.created ? "Inserted" : "Updated") + ";" + result.id);
                                }
                                else
                                {
                                    //Console.WriteLine("The Upsert failed because: " + result.errors[0].message);
                                    erro4.AppendLine(lista_processada4.ToArray()[i].ToString() + ";" + result.errors[0].message);
                                }
                            }
                        },
                        () =>
                        {
                            basicSample_cs_p.apex.UpsertResult[] upsertResults = binding.upsert("Id", lst_generica5.ToArray());

                            for (int i = 0; i < upsertResults.Length; i++)
                            {
                                basicSample_cs_p.apex.UpsertResult result = upsertResults[i];

                                if (result.success)
                                {
                                    //Console.WriteLine("\nUpsert succeeded.");
                                    acerto5.AppendLine(lista_processada5.ToArray()[i].ToString() + ";" + (result.created ? "Inserted" : "Updated") + ";" + result.id);
                                }
                                else
                                {
                                    //Console.WriteLine("The Upsert failed because: " + result.errors[0].message);
                                    erro5.AppendLine(lista_processada5.ToArray()[i].ToString() + ";" + result.errors[0].message);
                                }
                            }
                        },
                        () =>
                        {
                            basicSample_cs_p.apex.UpsertResult[] upsertResults = binding.upsert("Id", lst_generica6.ToArray());

                            for (int i = 0; i < upsertResults.Length; i++)
                            {
                                basicSample_cs_p.apex.UpsertResult result = upsertResults[i];

                                if (result.success)
                                {
                                    //Console.WriteLine("\nUpsert succeeded.");
                                    acerto6.AppendLine(lista_processada6.ToArray()[i].ToString() + ";" + (result.created ? "Inserted" : "Updated") + ";" + result.id);
                                }
                                else
                                {
                                    //Console.WriteLine("The Upsert failed because: " + result.errors[0].message);
                                    erro6.AppendLine(lista_processada6.ToArray()[i].ToString() + ";" + result.errors[0].message);
                                }
                            }
                        });

                        EscreverArquivo("resultado_positivo_billing" + data, acerto.ToString());
                        EscreverArquivo("resultado_positivo_billing" + data, acerto2.ToString());
                        EscreverArquivo("resultado_positivo_billing" + data, acerto3.ToString());
                        EscreverArquivo("resultado_positivo_billing" + data, acerto4.ToString());
                        EscreverArquivo("resultado_positivo_billing" + data, acerto5.ToString());
                        EscreverArquivo("resultado_positivo_billing" + data, acerto6.ToString());

                        EscreverArquivo("resultado_negativo_billing" + data, erro.ToString());
                        EscreverArquivo("resultado_negativo_billing" + data, erro2.ToString());
                        EscreverArquivo("resultado_negativo_billing" + data, erro3.ToString());
                        EscreverArquivo("resultado_negativo_billing" + data, erro4.ToString());
                        EscreverArquivo("resultado_negativo_billing" + data, erro5.ToString());
                        EscreverArquivo("resultado_negativo_billing" + data, erro6.ToString());

                        lst_generica.Clear();
                        lst_generica2.Clear();
                        lst_generica3.Clear();
                        lst_generica4.Clear();
                        lst_generica5.Clear();
                        lst_generica6.Clear();

                        acerto.Clear();
                        acerto2.Clear();
                        acerto3.Clear();
                        acerto4.Clear();
                        acerto5.Clear();
                        acerto6.Clear();

                        erro.Clear();
                        erro2.Clear();
                        erro3.Clear();
                        erro4.Clear();
                        erro5.Clear();
                        erro6.Clear();

                        lista_processada.Clear();
                        lista_processada2.Clear();
                        lista_processada3.Clear();
                        lista_processada4.Clear();
                        lista_processada5.Clear();
                        lista_processada6.Clear();
                        verifica = false;

                    }
                    catch (Exception)
                    {
                        EscreverArquivo("Erros_Billing_Profile__c_" + data, lst_generica.ToArray()[0].Id + " até " + lst_generica6.ToArray()[49].Id + "\n");
                    }
                }

            }


            try
            {
                //EscreverArquivo("Avanco_billing_" + data, lst_generica6.ToArray()[99].Id + "\n");

                Parallel.Invoke(
                () =>
                {
                    basicSample_cs_p.apex.UpsertResult[] upsertResults = binding.upsert("Id", lst_generica.ToArray());

                    for (int i = 0; i < upsertResults.Length; i++)
                    {
                        basicSample_cs_p.apex.UpsertResult result = upsertResults[i];

                        if (result.success)
                        {
                            //Console.WriteLine("\nUpsert succeeded.");
                            acerto.AppendLine(lista_processada.ToArray()[i].ToString() + ";" + (result.created ? "Inserted" : "Updated") + ";" + result.id);
                        }
                        else
                        {
                            //Console.WriteLine("The Upsert failed because: " + result.errors[0].message);
                            erro.AppendLine(lista_processada.ToArray()[i].ToString() + ";" + result.errors[0].message);
                        }
                    }
                },
                   () =>
                   {
                       basicSample_cs_p.apex.UpsertResult[] upsertResults = binding.upsert("Id", lst_generica2.ToArray());

                       for (int i = 0; i < upsertResults.Length; i++)
                       {
                           basicSample_cs_p.apex.UpsertResult result = upsertResults[i];

                           if (result.success)
                           {
                               //Console.WriteLine("\nUpsert succeeded.");
                               acerto2.AppendLine(lista_processada2.ToArray()[i].ToString() + ";" + (result.created ? "Inserted" : "Updated") + ";" + result.id);
                           }
                           else
                           {
                               //Console.WriteLine("The Upsert failed because: " + result.errors[0].message);
                               erro2.AppendLine(lista_processada2.ToArray()[i].ToString() + ";" + result.errors[0].message);
                           }
                       }
                   },
                   () =>
                   {
                       basicSample_cs_p.apex.UpsertResult[] upsertResults = binding.upsert("Id", lst_generica3.ToArray());

                       for (int i = 0; i < upsertResults.Length; i++)
                       {
                           basicSample_cs_p.apex.UpsertResult result = upsertResults[i];

                           if (result.success)
                           {
                               //Console.WriteLine("\nUpsert succeeded.");
                               acerto3.AppendLine(lista_processada3.ToArray()[i].ToString() + ";" + (result.created ? "Inserted" : "Updated") + ";" + result.id);
                           }
                           else
                           {
                               //Console.WriteLine("The Upsert failed because: " + result.errors[0].message);
                               erro3.AppendLine(lista_processada3.ToArray()[i].ToString() + ";" + result.errors[0].message);
                           }
                       }
                   },
                   () =>
                   {
                       basicSample_cs_p.apex.UpsertResult[] upsertResults = binding.upsert("Id", lst_generica4.ToArray());

                       for (int i = 0; i < upsertResults.Length; i++)
                       {
                           basicSample_cs_p.apex.UpsertResult result = upsertResults[i];

                           if (result.success)
                           {
                               //Console.WriteLine("\nUpsert succeeded.");
                               acerto4.AppendLine(lista_processada4.ToArray()[i].ToString() + ";" + (result.created ? "Inserted" : "Updated") + ";" + result.id);
                           }
                           else
                           {
                               //Console.WriteLine("The Upsert failed because: " + result.errors[0].message);
                               erro4.AppendLine(lista_processada4.ToArray()[i].ToString() + ";" + result.errors[0].message);
                           }
                       }
                   },
                   () =>
                   {
                       basicSample_cs_p.apex.UpsertResult[] upsertResults = binding.upsert("Id", lst_generica5.ToArray());

                       for (int i = 0; i < upsertResults.Length; i++)
                       {
                           basicSample_cs_p.apex.UpsertResult result = upsertResults[i];

                           if (result.success)
                           {
                               //Console.WriteLine("\nUpsert succeeded.");
                               acerto5.AppendLine(lista_processada5.ToArray()[i].ToString() + ";" + (result.created ? "Inserted" : "Updated") + ";" + result.id);
                           }
                           else
                           {
                               //Console.WriteLine("The Upsert failed because: " + result.errors[0].message);
                               erro5.AppendLine(lista_processada5.ToArray()[i].ToString() + ";" + result.errors[0].message);
                           }
                       }
                   },
                   () =>
                   {
                       basicSample_cs_p.apex.UpsertResult[] upsertResults = binding.upsert("Id", lst_generica6.ToArray());

                       for (int i = 0; i < upsertResults.Length; i++)
                       {
                           basicSample_cs_p.apex.UpsertResult result = upsertResults[i];

                           if (result.success)
                           {
                               //Console.WriteLine("\nUpsert succeeded.");
                               acerto6.AppendLine(lista_processada6.ToArray()[i].ToString() + ";" + (result.created ? "Inserted" : "Updated") + ";" + result.id);
                           }
                           else
                           {
                               //Console.WriteLine("The Upsert failed because: " + result.errors[0].message);
                               erro6.AppendLine(lista_processada6.ToArray()[i].ToString() + ";" + result.errors[0].message);
                           }
                       }
                   });

                EscreverArquivo("resultado_positivo_billing" + data, acerto.ToString());
                EscreverArquivo("resultado_positivo_billing" + data, acerto2.ToString());
                EscreverArquivo("resultado_positivo_billing" + data, acerto3.ToString());
                EscreverArquivo("resultado_positivo_billing" + data, acerto4.ToString());
                EscreverArquivo("resultado_positivo_billing" + data, acerto5.ToString());
                EscreverArquivo("resultado_positivo_billing" + data, acerto6.ToString());

                EscreverArquivo("resultado_negativo_billing" + data, erro.ToString());
                EscreverArquivo("resultado_negativo_billing" + data, erro2.ToString());
                EscreverArquivo("resultado_negativo_billing" + data, erro3.ToString());
                EscreverArquivo("resultado_negativo_billing" + data, erro4.ToString());
                EscreverArquivo("resultado_negativo_billing" + data, erro5.ToString());
                EscreverArquivo("resultado_negativo_billing" + data, erro6.ToString());

                lst_generica.Clear();
                lst_generica2.Clear();
                lst_generica3.Clear();
                lst_generica4.Clear();
                lst_generica5.Clear();
                lst_generica6.Clear();

                acerto.Clear();
                acerto2.Clear();
                acerto3.Clear();
                acerto4.Clear();
                acerto5.Clear();
                acerto6.Clear();

                erro.Clear();
                erro2.Clear();
                erro3.Clear();
                erro4.Clear();
                erro5.Clear();
                erro6.Clear();


            }
            catch (Exception e)
            {
                EscreverArquivo("resultado_positivo_billing" + data, acerto.ToString());
                EscreverArquivo("resultado_positivo_billing" + data, acerto2.ToString());
                EscreverArquivo("resultado_positivo_billing" + data, acerto3.ToString());
                EscreverArquivo("resultado_positivo_billing" + data, acerto4.ToString());
                EscreverArquivo("resultado_positivo_billing" + data, acerto5.ToString());
                EscreverArquivo("resultado_positivo_billing" + data, acerto6.ToString());

                EscreverArquivo("resultado_negativo_billing" + data, erro.ToString());
                EscreverArquivo("resultado_negativo_billing" + data, erro2.ToString());
                EscreverArquivo("resultado_negativo_billing" + data, erro3.ToString());
                EscreverArquivo("resultado_negativo_billing" + data, erro4.ToString());
                EscreverArquivo("resultado_negativo_billing" + data, erro5.ToString());
                EscreverArquivo("resultado_negativo_billing" + data, erro6.ToString());

                EscreverArquivo("Erros_billing_" + data, e.Message + "\n");
            }

            Console.WriteLine("Fim do Programa");
            Console.ReadLine();
        }



        private void TestUpsert()
        {

            if (!loggedIn)
            {
                if (!login())
                    return;
            }

            //DescribeSObjectResult dsr = binding.describeSObject("pointofdelivery__c");
            //System.Collections.Hashtable fieldMap = this.makeFieldMap(dsr.fields);


            Address__c end = new Address__c();
            end.ExternalId__c = "20051828993";
            Account conta = new Account();

            conta.ExternalId__c = "200511883153743CPF";
            conta.Address__r = end;
            

            try
            {
                // Invoke the upsert call and save the results.
                // Use External_Id custom field for matching records
                basicSample_cs_p.apex.UpsertResult[] upsertResults = binding.upsert("ExternalId__c", new apex.sObject[] { conta });


                for (int i = 0; i < upsertResults.Length; i++)
                {
                    basicSample_cs_p.apex.UpsertResult result = upsertResults[i];
                    if (result.success)
                    {
                        Console.WriteLine("\nUpsert succeeded.");
                        Console.WriteLine((result.created ? "Inserted" : "Updated") + " account, id is " + result.id);
                    }
                    else
                    {
                        Console.WriteLine("The Upsert failed because: " + result.errors[0].message);
                    }
                }
            }
            catch (System.Web.Services.Protocols.SoapException ex)
            {
                Console.WriteLine("Error from web service: " + ex.Message);
            }

        }

    
        public static void EscreverArquivo(string nomeArquivo, string conteudo)
        {
            using (System.IO.StreamWriter file2 =
            
            new System.IO.StreamWriter(ConfigurationSettings.AppSettings["local_arquivo"].ToString() + nomeArquivo + ".txt", true, System.Text.Encoding.GetEncoding("ISO-8859-1")))
            {
                file2.Write(conteudo);
            }
        }

        public static void ApagarArquivo(string nomeArquivo)
        {
            File.Delete(ConfigurationSettings.AppSettings["local_arquivo"].ToString() + nomeArquivo + ".txt");            
        }

      
        /* 
         * login sample
         * Prompts for username and password, set class variable binding
         * resets the url for the binding and adds the session header to 
         * the binding class variable
         */
        private bool login()
        {

            un = "migracionbrasil2@enel.com";
            pw = "EnelBR_Migracion18244WNLv5JdO9wi6gh3QOTJq3Lup";

            //un = "brasilmigracion4@enel.com";
            //pw = "Deloiyye2020mUkLb2eNfT3yBZnPSQMtAAum";

            if (un == null)
            {
                return false;
            }

            //pw = "EnelBR_Migracion18244WNLv5JdO9wi6gh3QOTJq3Lup";
            
            if (pw == null)
            {
                return false;
            }

            //Provide feed back while we create the web service binding
            Console.WriteLine("Realizando o Binding no web service...");

            #region outros logins e url

            binding = new SforceService();
            binding_execute = new SforceService();
           
            //...Fazendo...
            //binding.Url = "https://na30.salesforce.com/services/Soap/u/34.0";

            binding.Url = "https://enelsud.my.salesforce.com/services/Soap/c/53.0";
            //binding_execute.Url = "https://enelsud--preprod.sandbox.my.salesforce.com/services/Soap/c/53.0";
            binding_execute.Url = "https://enelsud--enelbrazil.sandbox.my.salesforce.com/services/Soap/c/53.0";

            #endregion


            #region SAND BOX EXTRACAO ORG CEARA em 2023-06-12

            binding.Url = "https://enelsud.my.salesforce.com/services/Soap/c/53.0";
            binding_execute.Url = "https://enelsud--enelbrazil.sandbox.my.salesforce.com/services/Soap/c/56.0";

            #endregion

            //Setando o tempo de timeout, inclusive para as consultas.
            System.Net.ServicePointManager.SecurityProtocol = (SecurityProtocolType)3072;

            //Tentando Login depois do primeiro feedback
            Console.WriteLine("Logando Agora...");
            apex.LoginResult login2;

            try
            {
                //binding.Proxy = new System.Net.WebProxy(); 
                //binding.Proxy.Credentials = new NetworkCredential("BR0118831537", "Brasil201704", "RIO");
                //binding.Timeout = int.MaxValue;
                //login2 = binding_execute.login("brasilmigracion4@enel.com.preprod", "Enel2020+q5fZ4oeKJzdZyfelba7goaYL");
                //"migracionbrasil2@enel.com.enelbrazil", "Enel@2023Yo8oR27uUJSnQznYuEiX1W7U"

                //login2 = binding_execute.login("migracionbrasil2@enel.com.enelbrazil", "Enel@2023Yo8oR27uUJSnQznYuEiX1W7U");
                //loginResult = binding.login(un, pw);                

                un = "migracionbrasil2@enel.com";
                pw = "EnelBR_Migracion18244WNLv5JdO9wi6gh3QOTJq3Lup";
                loginResult = binding.login(un, pw);

                un = "migracionbrasil3@enel.com.enelbrazil";
                pw = "Enel@20248ugVa1ArKB0yxghH1AphBibWI";
                login2 = binding_execute.login(un, pw);
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

            //Console.WriteLine("\nO id da Sessão é: " + loginResult.sessionId);
            //Console.WriteLine("\nO direcionamento é: " + loginResult.serverUrl);


            //Change the binding to the new endpoint
            binding.Url = loginResult.serverUrl;
            binding_execute.Url = login2.serverUrl;

            //Create a new session header object and set the session id to that returned by the login
            binding.SessionHeaderValue = new apex.SessionHeader();
            binding.SessionHeaderValue.sessionId = loginResult.sessionId;

            binding_execute.SessionHeaderValue = new apex.SessionHeader();
            binding_execute.SessionHeaderValue.sessionId = login2.sessionId;            

            /*apexMetadata.LogInfo[] logs = new apexMetadata.LogInfo[1];
            logs[0] = new apexMetadata.LogInfo();
            logs[0].category= apexMetadata.LogCategory.Apex_code;
            logs[0].level = apexMetadata.LogCategoryLevel.Finest;

            binding_execute.DebuggingHeaderValue = new apexMetadata.DebuggingHeader();
            binding_execute.DebuggingHeaderValue.categories = logs;
            binding_execute.DebuggingHeaderValue.debugLevel = apexMetadata.LogType.Debugonly;*/
            

            //binding_execute.ses // setDebuggingHeader(logs);*/

            loggedIn = true;

            // call the getServerTimestamp method
            //getServerTimestampSample();

            ///call the getUserInfo method
            //getUserInfoSample();

            return true;

        }

        private void getServerTimestampSample()
        {
            //Verify that we are already authenticated, if not
            //call the login function to do so
            if (!loggedIn)
            {
                if (!login())
                    return;
            }

            try
            {
                Console.WriteLine("\nGetting server's timestamp...");
                //call the getServerTimestamp method
                apex.GetServerTimestampResult gstr = binding.getServerTimestamp();
                serverTime = gstr.timestamp;
                //access the return properties
                Console.WriteLine(gstr.timestamp.ToLongDateString() + " " + gstr.timestamp.ToLongTimeString());
            }
            catch (Exception ex2)
            {
                Console.WriteLine("ERROR: getting server timestamp.\n" + ex2.Message);
            }

        }

        private void getUserInfoSample()
        {
            //Verify that we are already authenticated, if not
            //call the login function to do so
            if (!loggedIn)
            {
                if (!login())
                    return;
            }

            try
            {
                Console.WriteLine("\nGetting user info...");
                //call the getUserInfo method
                userInfo = binding.getUserInfo();
                //access the return properties
                Console.WriteLine("User Name: " + userInfo.userFullName);
                Console.WriteLine("User Email: " + userInfo.userEmail);
                Console.WriteLine("User Language: " + userInfo.userLanguage);
                Console.WriteLine("User Organization: " + userInfo.organizationName);
            }
            catch (Exception ex3)
            {
                Console.WriteLine("ERROR: getting user info.\n" + ex3.Message);
            }
        }


     
        private void sendEmailSample()
        {
            //Verify that we are already authenticated, if not
            //call the login function to do so
            if (!loggedIn)
            {
                if (!login())
                    return;
            }

            try
            {
                Console.Write("Enter a valid email address for the bcc: ");
                String bccAddress = Console.ReadLine();
                SingleEmailMessage[] messages = new SingleEmailMessage[1];
                messages[0] = new SingleEmailMessage();
                messages[0].bccAddresses = bccAddress.Length > 0 ? new String[] { bccAddress } : null;
                messages[0].bccSender = true;
                Console.Write("Enter a valid email address for the cc: ");
                String ccAddress = Console.ReadLine();
                messages[0].ccAddresses = ccAddress.Length > 0 ? new String[] { ccAddress } : null;
                messages[0].emailPriority = EmailPriority.Normal;
                messages[0].htmlBody = "<b>This is a message</b><br>from the api sample.";
                //The following field is only valid if the targetObjectId was used.
                //messages[0].saveAsActivity = saveAsActivity;
                messages[0].subject = "Test Message From Sample";
                messages[0].useSignature = false;
                messages[0].plainTextBody = "This is a message from the api";
                Console.Write("Enter a valid email address for the sender: ");
                String replyTo = Console.ReadLine();
                Console.Write("Enter a valid email address for the recipient of this email: ");
                String toAddress = Console.ReadLine();
                if (toAddress.Length > 0 && replyTo.Length > 0)
                {
                    messages[0].replyTo = replyTo;
                    messages[0].toAddresses = new String[] { toAddress };
                    //The next line is used if you have an id for a user, contact or
                    //lead that is the recipient.  If this is the case, you don't need
                    //to set the toAddresses field.
                    //messages[0].targetObjectId = "003000000fexGGH";
                    SendEmailResult[] result = binding.sendEmail(messages);
                    for (int i = 0; i < result.Length; i++)
                    {
                        if (result[i].success)
                        {
                            Console.Write("Successfully sent the email to " + toAddress);
                        }
                        else
                        {
                            Console.Write("Error from the platform: " + result[i].errors[0].message);
                        }
                    }
                    Console.ReadLine();
                }
                else
                {
                    Console.Write("No to address entered, so we are bailing.  Hit return to continue.");
                    Console.ReadLine();
                }
            }
            catch (Exception ex3)
            {
                Console.WriteLine("ERROR: sending email.\n" + ex3.Message);
            }
        }
        
        private System.Xml.XmlElement GetNewXmlElement(string Name, string nodeValue)
        {
            System.Xml.XmlDocument doc = new System.Xml.XmlDocument();
            System.Xml.XmlElement xmlel = doc.CreateElement(Name);
            xmlel.InnerText = nodeValue;
            return xmlel;
        }

        private void deleteSample()
        {
            //Verify that we are already authenticated, if not
            //call the login function to do so
            if (!loggedIn)
            {
                if (!login())
                    return;
            }
            //check to see if we know anything that was created
            if (tasks == null && contacts == null && accounts == null)
            {
                Console.WriteLine("\nDelete operation not completed.  You will need to create a task, account or contact during this session to run the delete sample.");
                Console.Write("\nHit return to continue...");
                Console.ReadLine();
                return;
            }
            try
            {
                if (tasks != null)
                {
                    binding.delete(tasks);
                    Console.WriteLine("\nSuccessfully deleted " + tasks.Length + " tasks.");
                    tasks = null;
                }
                else
                {
                    Console.WriteLine("\nDeleted 0 tasks.");
                }
                if (contacts != null)
                {
                    binding.delete(contacts);
                    Console.WriteLine("\nSuccessfully deleted " + contacts.Length + " contacts.");
                    contacts = null;
                }
                else
                {
                    Console.WriteLine("\nDeleted 0 contacts.");
                }
                if (accounts != null)
                {
                    binding.delete(accounts);
                    Console.WriteLine("\nSuccessfully deleted " + accounts.Length + " accounts.");
                    accounts = null;
                }
                else
                {
                    Console.WriteLine("\nDeleted 0 accounts.");
                }
                Console.WriteLine("\nDelete sample completed successfully.");
                Console.Write("\nHit return to continue...");
                Console.ReadLine();
            }
            catch (Exception ex)
            {
                Console.WriteLine("\nFailed to succesfully create a task, error message was: \n"
                           + ex.Message);
                Console.Write("\nHit return to continue...");
                Console.ReadLine();
            }
        }

      
        private void getDeletedSample()
        {
            //Verify that we are already authenticated, if not
            //call the login function to do so
            if (!loggedIn)
            {
                if (!login())
                    return;
            }

            try
            {
                //Calendar deletedDate 

                apex.GetDeletedResult gdr = binding.getDeleted("Account", serverTime.Subtract(new System.TimeSpan(0, 0, 5, 0, 0)), serverTime);

                if (gdr.deletedRecords != null)
                {
                    for (int i = 0; i < gdr.deletedRecords.Length; i++)
                    {
                        Console.WriteLine(gdr.deletedRecords[i].id + " was deleted on " + gdr.deletedRecords[i].deletedDate.ToString());
                    }
                }
                else
                {
                    Console.WriteLine("No deleted accounts since " + serverTime.Subtract(new System.TimeSpan(0, 0, 5, 0, 0)));
                }
                serverTime = binding.getServerTimestamp().timestamp;
                //Console.WriteLine("\nHit return to continue...");
                //Console.ReadLine();
            }
            catch (Exception ex)
            {
                Console.WriteLine(
                    "\nFailed to execute query succesfully, error message was: \n"
                    + ex.Message);
                //Console.WriteLine("\nHit return to continue...");
                //Console.ReadLine();
            }
        }

        private void getUpdatedSample()
        {
            //Verify that we are already authenticated, if not
            //call the login function to do so
            if (!loggedIn)
            {
                if (!login())
                {
                    return;
                }
            }

            try
            {

                apex.GetUpdatedResult gur = binding.getUpdated("Account", serverTime.Subtract(new System.TimeSpan(0, 0, 5, 0, 0)), serverTime);
                if (gur.ids != null)
                {
                    for (int i = 0; i < gur.ids.Length; i++)
                    {
                        Console.WriteLine(gur.ids[i] + " was updated or created.");
                    }
                }
                else
                {
                    Console.WriteLine("No updates to accounts since " + serverTime.Subtract(new System.TimeSpan(0, 0, 5, 0, 0)));
                }
                serverTime = binding.getServerTimestamp().timestamp;
                //Console.WriteLine("\nHit return to continue...");
                //Console.ReadLine();
            }
            catch (Exception ex)
            {
                Console.WriteLine(
                           "\nFailed to execute query succesfully, error message was: \n"
                           + ex.Message);
                //Console.WriteLine("\nHit return to continue...");
                //Console.ReadLine();
            }
        }

        private string getFieldValue(string fieldName, System.Xml.XmlElement[] fields)
        {
            string returnValue = "";
            if (fields != null)
            {
                for (int i = 0; i < fields.Length; i++)
                {
                    if (fields[i].LocalName.ToLower().Equals(fieldName.ToLower()))
                    {
                        returnValue = fields[i].InnerText;
                    }
                }
            }
            return returnValue;
        }

         
        private void describeSObjectsSample()
        {
            //Verify that we are already authenticated, if not
            //call the login function to do so
            if (!loggedIn)
            {
                if (!login())
                    return;
            }

            try
            {
                apex.DescribeSObjectResult[] describeSObjectResults = binding.describeSObjects(new string[] { "account", "contact", "lead" });
                for (int x = 0; x < describeSObjectResults.Length; x++)
                {
                    apex.DescribeSObjectResult describeSObjectResult = describeSObjectResults[x];
                    Console.WriteLine("\n\n" + describeSObjectResult.name);
                    // Retrieve fields from the results
                    apex.Field[] fields = describeSObjectResult.fields;
                    // Get the name of the object
                    String objectName = describeSObjectResult.name;
                    // Get some flags
                    bool isActivateable = describeSObjectResult.activateable;
                    // Many other values are accessible
                    if (fields != null)
                    {
                        // Iterate through the fields to get properties for each field
                        for (int i = 0; i < fields.Length; i++)
                        {
                            apex.Field field = fields[i];
                            int byteLength = field.byteLength;
                            int digits = field.digits;
                            string label = field.label;
                            int length = field.length;
                            string name = field.name;
                            apex.PicklistEntry[] picklistValues = field.picklistValues;
                            int precision = field.precision;
                            string[] referenceTos = field.referenceTo;
                            int scale = field.scale;
                            apex.fieldType fieldType = field.type;
                            bool fieldIsCreateable = field.createable;
                            // Determine whether there are picklist values
                            if (picklistValues != null && picklistValues[0] != null)
                            {
                                Console.WriteLine("Picklist values = ");
                                for (int j = 0; j < picklistValues.Length; j++)
                                {
                                    if (picklistValues[j].label != null)
                                    {
                                        Console.WriteLine(" Item: " +
                                            picklistValues[j].label);
                                    }
                                }
                            }
                            // Determine whether this field refers to another object
                            if (referenceTos != null && referenceTos[0] != null)
                            {
                                Console.WriteLine("Field references the following objects:");
                                for (int j = 0; j < referenceTos.Length; j++)
                                {
                                    Console.WriteLine(" " + referenceTos[j]);
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error on DescribeSObjects: " + ex.Message);
            }
        }
          
        private void describeTabsSample()
        {
            //Verify that we are already authenticated, if not
            //call the login function to do so
            if (!loggedIn)
            {
                if (!login())
                    return;
            }

            try
            {
                apex.DescribeTabSetResult[] dtsrs = binding.describeTabs();

                Console.WriteLine("There are " + dtsrs.Length.ToString() + " tabsets defined.");
                for (int i = 0; i < dtsrs.Length; i++)
                {
                    Console.WriteLine("Tabset " + (i + 1).ToString() + ":");
                    apex.DescribeTabSetResult dtsr = dtsrs[i];
                    String tabSetLabel = dtsr.label;
                    String logoUrl = dtsr.logoUrl;
                    bool isSelected = dtsr.selected;
                    DescribeTab[] tabs = dtsr.tabs;
                    Console.WriteLine("Label is " + tabSetLabel + " logo url is " + logoUrl + ", there are " + tabs.Length.ToString() + " tabs defined in this set.");
                    for (int j = 0; j < tabs.Length; j++)
                    {
                        apex.DescribeTab tab = tabs[j];
                        String tabLabel = tab.label;
                        String objectName = tab.sobjectName;
                        String tabUrl = tab.url;
                        Console.WriteLine("\tTab " + (j + 1).ToString() + ": \n\t\tLabel = " + tabLabel +
                            "\n\t\tObject details on tab: " + objectName + "\n\t\t" +
                            "Url to tab: " + tabUrl);
                    }
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine("Error on DescribeTab: " + ex.Message);
            }
        }
       
     
        private void createOracleSFDCModel()
        {

            if (!loggedIn)
            {
                if (!login())
                    return;
            }

            try
            {

                StringBuilder sb = new StringBuilder();
                String[] SFDCObjects;



                //Core Objects - verified cs 20100601 - 1830EST
                ////SFDCObjects = new string[] {"Profile", "UserRole", "User", "Organization", "UserLicense", "Contact", 
                ////    "Group", "Product2", "Pricebook2", "Account", "CallCenter"};


                // Sales Objects
                ////SFDCObjects = new string[] {"Asset", "Contract", "Campaign", "Opportunity"
                ////    , "Partner", "Lead", "CampaignMember"
                ////    , "AccountContactRole", "OpportunityContactRole", "OpportunityStage", "OpportunityCompetitor"
                ////    , "Case", "CaseStatus", "Approval", "ContractStatus"
                ////    , "ContractContactRole", "PartnerRole", "LeadStatus"};

                // Forecasting, Product and Schedule Objects
                SFDCObjects = new string[] {"LineItemOverride", "OpportunityLineItem", "PricebookEntry", "OpportunityOverride", 
                    "Period", "FiscalYearSettings", "QuantityForecast", "RevenueForecast", "QuantityForecastHistory", "RevenueForecastHistory"
                };//, "OpportunityLineItemSchedule", "QuoteLineItem", "Quote" };



                // Forecasting Objects
                //SFDCObjects = new string[] {"LineItemOverride", "OpportunityLineItem", "OpportunityOverride", 
                //    "Period", "FiscalYearSettings", "QuantityForecast", "RevenueForecast", "QuantityForecastHistory", "RevenueForecastHistory"};

                // Product and Schedule Objects
                //SFDCObjects = new string[] {"OpportunityLineItemSchedule", "OpportunityLineItem", "PricebookEntry", 
                //    "QuoteLineItem", "Quote"};







                //// Territory Management
                //SFDCObjects = new string[] {"Territory", "UserTerritory", "OpportunityShare", 
                //    "AccountShare", "AccountTerritoryAssignmentRuleItem", "AccountTerritoryAssignmentRule"};



                //#region Objects Loop Call
                ////DescribeGlobalResult describeGlobalResult = binding.describeGlobal();
                //////if (describeGlobalResult != null)
                //////{
                ////    String[] types = describeGlobalResult.types;
                ////    if (types != null)
                ////    {
                ////        for (int i = 0; i < types.Length; i++)
                ////        {
                ////            Console.WriteLine(types[i]);
                ////        }
                ////        //Console.WriteLine("\nDescribe global was successful.\n\nHit the enter key to conutinue....");
                ////        //Console.ReadLine();
                ////    }
                //////}
                //#endregion




                    apex.DescribeSObjectResult[] describeSObjectResults =
                           binding.describeSObjects(SFDCObjects);
                    
                
                
                
                
                    String foriegnKeysAll = "";
                    String dropTablesAll = "/*-- begin drop tables";

                    // object/table loop
                    for (int x = 0; x < describeSObjectResults.Length; x++)
                    {
                        apex.DescribeSObjectResult describeSObjectResult =
                                           describeSObjectResults[x];


                    // Retrieve fields from the results  

                    apex.Field[] fields = describeSObjectResult.fields;
                    // Get the name of the object  


                    int DBRestrictionFieldLength = 30;
                    int constraintNum = 0;
                    String objectName = replaceObject(describeSObjectResult.name);
                    String primaryKey = "";
                    String foriegnKeys = "";
                    

                    // Get some flags  
                    bool isActivateable = describeSObjectResult.activateable;

                    sb.AppendLine("CREATE TABLE " + objectName + " (");
                    
                    
                    // Many other values are accessible  
                    if (fields != null)
                    {
                        // Iterate through the fields to get properties for each field  

                        for (int i = 0; i < fields.Length; i++)
                        {
                            apex.Field field = fields[i];
                            int byteLength = field.byteLength;
                            int digits = field.digits;
                            string label = field.label;
                            int length = field.length;
                            string name = field.name;
                            apex.PicklistEntry[] picklistValues = field.picklistValues;
                            int precision = field.precision;
                            string[] referenceTos = field.referenceTo;
                            int scale = field.scale;
                            apex.fieldType fieldType = field.type;
                            bool fieldIsCreateable = field.createable;
                            bool isFieldNullable = field.nillable;




                            // NOTE:
                            // All field lengths should be verified and assessed for the best shortening approach
                            // i.e.:    Profile table has several fields prefixed with "Permissions", perhaps replacing
                            //          w/ "Can" will allow all subsequent field descriptor to come through correctly.
                            // Other tables:
                            //  - Profile -> Permissions to Can
                            //  - User -> UserPreferences to UserCan
                            //  - User -> UserPermissions to UserDo
                            //  - Organization -> PreferencesRequire to Require
                            //if (name.Length >= DBRestrictionFieldLength)
                            //{
                                // Note:
                                // the switch statement is hard-coded and takes into account the changed names
                                // of the objects due to reservered DB naming, see function replaceObject()
                                switch (objectName.ToLower())
                                {
                                    case "profiles":
                                        name = name.Replace("Permissions", "Can");
                                        break;
                                    case "users":
                                        name = name.Replace("UserPreferences", "");
                                        name = name.Replace("UserPermissions", "Allow");
                                        break;
                                    case "organization":
                                        name = name.Replace("PreferencesRequire", "Require");
                                        break;
                                    case "quantityforecast":
                                    case "revenueforecast":
                                    case "userrole":
                                        name = name.Replace("Opportunity", "Opp");
                                        break;
                                    default:
                                        if (name.Length >= DBRestrictionFieldLength)
                                            name = name.Substring(0, DBRestrictionFieldLength - 7);
                                        break;
                                }
                            //}
                                



                            string oracleDataTypeSet = DBHelper.TranslateSFDCDataTypeToOracle10gDataType(fieldType
                                , length, precision, scale);
      


                            if (i > 0)
                                @sb.Append(",\"" + name + "\" " //+ fieldType.ToString() + " "
                                    + oracleDataTypeSet);
                            else
                                @sb.Append("\"" + name + "\" " //+ fieldType.ToString() + " "
                                    + oracleDataTypeSet);


                            // Note: SFDC doesn't provide this default value via the SDK unless the default is 
                            //       a formula value a user has cutomized and the latter has not been validated.
                            //if (field.defaultedOnCreate && !String.IsNullOrEmpty(field.defaultValueFormula))
                            //    sb.Append(" DEFAULT " + field.defaultValueFormula);


                            constraintNum++;

                            // Develop the field constraint name
                            if (isFieldNullable)
                                sb.Append(" NULL");
                            else
                            {
                                // set temp objectname as to not overwrite the main variable
                                string tmpConstObjectName = objectName;

                                if ((objectName + "_cx"
                                    + constraintNum.ToString() + "_nn").Length > DBRestrictionFieldLength)
                                {
                                    switch (objectName.ToLower())
                                    {
                                        case "quantityforecasthistory":
                                        case "revenueforecasthistory":
                                            tmpConstObjectName = objectName.Replace("History", "Hist");
                                            break;
                                        default:
                                            if (objectName.Length >= DBRestrictionFieldLength)
                                                tmpConstObjectName = objectName.Substring(0, DBRestrictionFieldLength - 5);
                                            break;
                                    }

                                    sb.Append(" CONSTRAINT " + tmpConstObjectName + "_cx"
                                        + constraintNum.ToString() + "_nn NOT NULL");

                                }
                                else
                                    sb.Append(" CONSTRAINT " + objectName + "_cx"
                                        + constraintNum.ToString() + "_nn NOT NULL");
                            }

                            if (referenceTos != null && referenceTos[0] != null)
                            {
                                //Console.WriteLine("Field references the following objects:");
                                //for (int j = 0; j < referenceTos.Length; j++)
                                //{
                                    ////sb.Append(" REFERENCES ");

                                    ////if (referenceTos[0].ToString().ToUpper() == "ACCOUNT")
                                    ////    sb.Append("Account(\"Id\")");   // - was: " + referenceTos[0].ToString().ToUpper());
                                    ////else
                                    ////    sb.Append(referenceTos[0].ToString() + "(\"Id\")");
                                //}


                                // create foriegn key logic to post at the bottom of the table
                                ////foriegnKeys += ", CONSTRAINT fk_" + referenceTos[0].ToString() + "_" + name 
                                ////  + " FOREIGN KEY (\"" + name + "\")"
                                ////  + " REFERENCES " + referenceTos[0].ToString() + "(\"Id\")";


                                // this set of logic prevents the Oracle default constraint of 30 characters for any object name
                                string fkObjectName = objectName + "_" + referenceTos[0].ToString();
                                if (fkObjectName.Length > 20)
                                    fkObjectName = fkObjectName.Substring(0, 19);

                                foriegnKeys += "ALTER TABLE " + objectName + "\nADD CONSTRAINT fk_"
                                  + fkObjectName + "_" + constraintNum.ToString()
                                  + " FOREIGN KEY (\"" + name + "\")"
                                  + " REFERENCES " + replaceObject(referenceTos[0].ToString()) + "(\"Id\");"
                                  + "\n";
                            }



                            if(field.type.ToString() == "id")
                                primaryKey = ", CONSTRAINT pk_" + objectName + "_" + name 
                                    + " PRIMARY KEY (\"" + name + "\")";

                            #region Picklist
                            // Determine whether there are picklist values  
                            //Console.WriteLine("Field: {0}", name);
                            //Console.WriteLine("\tlength: {0}", length.ToString());
                            //Console.WriteLine("\tPrecision: {0}", precision.ToString());
                            //Console.WriteLine("\tType: {0}", fieldType.ToString());
                            //if(field.defaultedOnCreate)
                            //    Console.WriteLine("\tDefault Value: {0}", field.defaultValueFormula);

                            //Console.WriteLine(""); 
                            #endregion

                            sb.AppendLine("");
                        } //end field loop



                    } // end fields check



                    sb.AppendLine(primaryKey);
  
                    // end table DDL
                    sb.AppendLine(");");

                    // blank line
                    sb.AppendLine("");

                    //Console.WriteLine(sb.ToString());


                    // kick out hte foreign key constraints now
                    //sb.AppendLine(foriegnKeysAll);
                    foriegnKeysAll += foriegnKeys;



                    dropTablesAll += "\nDROP TABLE " + objectName + " cascade constraints;";

                    Console.WriteLine("Printing to file now...");

                    ////output to file
                    //string tmpFileOutput = "Top\n";
                    //tmpFileOutput += "Count: " + (x + 1).ToString();
                    //tmpFileOutput += "\n";
                    //tmpFileOutput += "\n";
                    //tmpFileOutput += sb.ToString();

                    



                } // end object/table loop


                // finish up drop tables list
                dropTablesAll += "\n-- end drop tables */";


                // kick out hte foreign key constraints now
                sb.AppendLine(foriegnKeysAll);
                sb.AppendLine();
                sb.AppendLine(dropTablesAll);

                TextFileWriter.WriteToLog(sb.ToString());

            } //end try
            catch (Exception)
            {
            }
            finally
            {

                
            }
        }
        
        static string replaceObject(string objectName)
        {
            string tmp = objectName.ToLower();

            switch (tmp)
            {
                case "user":
                    return "Users";
                case "profile":
                    return "Profiles";
                case "group":
                    return "Groups";
                case "accountterritoryassignmentruleitem":
                    return "AccountTerritoryRuleItem";
                case "accountterritoryassignmentrule":
                    return "AccountTerritoryRule";
                default:
                    return objectName;
            }
        }



        /*
        private void SubQuerySample()
        {

            if (!loggedIn)
            {
                if (!login())
                    return;
            }

            XmlNamespaceManager namespaceManager = new XmlNamespaceManager(new NameTable());
            namespaceManager.AddNamespace("tns", "urn:partner.soap.sforce.com");
            namespaceManager.AddNamespace("sf", "urn:sobject.partner.soap.sforce.com");

            try
            {
                QueryResult qr = binding.query("SELECT Id, Name, (SELECT Id, FirstName, LastName FROM Contacts) FROM Account");
                while (true)
                {
                    foreach (sObject o in qr.records)
                    {
                        string accountId = o.Any[0].InnerText;
                        string accountName = o.Any[1].InnerText;
                        System.Console.WriteLine("Account: " + accountName + " (" + accountId + ")");
                        System.Console.WriteLine("\tContacts: ");
                        // Get already returned contacts...
                        XmlNodeList contactRecords = o.Any[2].SelectNodes("tns:records", namespaceManager);
                        foreach (XmlElement contactRecord in contactRecords)
                        {
                            string contactId = contactRecord.SelectSingleNode("sf:Id", namespaceManager).InnerText;
                            string contactFirstName = contactRecord.SelectSingleNode("sf:FirstName", namespaceManager).InnerText;
                            string contactLastName = contactRecord.SelectSingleNode("sf:LastName", namespaceManager).InnerText;
                            System.Console.WriteLine("\t" + contactFirstName + " " + contactLastName + " (" + contactId + ")");
                        }
                        // (sub)queryMore if more contacts available...
                        XmlNode done = o.Any[2].SelectSingleNode("tns:done", namespaceManager);
                        if (done != null && !XmlConvert.ToBoolean(done.InnerText))
                        {
                            XmlNode xmlQueryLocator = o.Any[2].SelectSingleNode("tns:queryLocator", namespaceManager);
                            if (xmlQueryLocator != null)
                            {
                                string queryLocator = xmlQueryLocator.InnerText;
                                while (true)
                                {
                                    QueryResult qr1 = binding.queryMore(queryLocator);
                                    foreach (sObject o1 in qr1.records)
                                    {
                                        string contactId = o1.Any[0].InnerText;
                                        string contactFirstName = o1.Any[1].InnerText;
                                        string contactLastName = o1.Any[2].InnerText;
                                        System.Console.WriteLine("\t" + contactFirstName + " " + contactLastName + " (" + contactId + ")");
                                    }
                                    if (qr1.done)
                                    {
                                        break;
                                    }
                                    queryLocator = qr1.queryLocator;
                                }
                            }
                        }
                    }
                    if (qr.done)
                    {
                        break;
                    }
                    qr = binding.queryMore(qr.queryLocator);
                }
            }
            catch (Exception ex)
            {
                System.Console.WriteLine("\nException thrown: " + ex.Message);
            }
        }
        */

    }//end of class

}  //end of namespace

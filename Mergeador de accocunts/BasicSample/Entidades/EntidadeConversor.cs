using basicSample_cs_p;
using SalesforceExtractor.apex;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SalesforceExtractor.Entidades
{
    public static class EntidadeConversor
    {
        public static List<ContractLineItemSalesforce> ToContractLineItemList(List<sObject> cons)
        {
            SFDCSchemeBuild schema = new SFDCSchemeBuild();

            List<ContractLineItemSalesforce> result = new List<ContractLineItemSalesforce>();

            foreach (sObject con in cons)
            {
                ContractLineItemSalesforce obj = new ContractLineItemSalesforce();
                obj.Id = schema.getFieldValue("Id", con.Any);
                //obj.PointOfDelivery = schema.getFieldValueMore("Asset__r", "", "", "PointOfDelivery__c", con.Any);
                //obj.NumeroCliente = schema.getFieldValueMore("Asset__r", "PointOfDelivery__r", "", "Name", con.Any);
                obj.PointOfDeliveryAddress = schema.getFieldValueMore("Asset__r", "PointOfDelivery__r", "", "DetailAddress__c", con.Any);

                obj.AssetExternalId = schema.getFieldValueMore("Asset__r", "", "", "ExternalId__c", con.Any);
                obj.AssetId = schema.getFieldValue("Asset__c", con.Any);
                obj.AssetName = schema.getFieldValueMore("Asset__r", "", "", "Name", con.Any);
                obj.AssetStatus = schema.getFieldValueMore("Asset__r", "", "", "NE__Status__c", con.Any);
                obj.AssetRecordType = schema.getFieldValueMore("Asset__r", "", "", "RecordTypeId", con.Any);
                obj.OrderConfig = schema.getFieldValueMore("Asset__r", "", "", "NE__Order_Config__c", con.Any);
                obj.AccountIdAsset = schema.getFieldValueMore("Asset__r", "", "", "AccountId", con.Any);
                obj.ContactId = schema.getFieldValueMore("Asset__r", "", "", "ContactId", con.Any);

                obj.AccountId = schema.getFieldValueMore("Asset__r", "Account", "", "Id", con.Any);
                obj.AssetContactName = schema.getFieldValueMore("Asset__r", "Account", "", "Name", con.Any);
                obj.AccountIdentityNumber = schema.getFieldValueMore("Asset__r", "Account", "", "IdentityNumber__c", con.Any);
                obj.AccountExternalId = schema.getFieldValueMore("Asset__r", "Account", "", "ExternalId__c", con.Any);

                obj.ContractId = schema.getFieldValue("Contract__c", con.Any);
                obj.ContractQuote = schema.getFieldValueMore("Contract__r", "", "", "CNT_Quote__c", con.Any);
                obj.ContractType = schema.getFieldValueMore("Contract__r", "", "", "Contract_Type__c", con.Any);
                obj.ContractNumber = schema.getFieldValueMore("Contract__r", "", "", "ContractNumber", con.Any);
                obj.ContractStatus = schema.getFieldValueMore("Contract__r", "", "", "Status", con.Any);
                obj.ContractStartDate = schema.getFieldValueMore("Contract__r", "", "", "StartDate", con.Any);
                obj.ContractAccountId = schema.getFieldValueMore("Contract__r", "", "", "AccountId", con.Any);
                obj.ContractExternalId = schema.getFieldValueMore("Contract__r", "", "", "ExternalId__c", con.Any);
                obj.ContractRecordTypeId = schema.getFieldValueMore("Contract__r", "RecordType", "", "Id", con.Any);
                obj.ContractAtividadeEconomica = schema.getFieldValueMore("Contract__r", "", "", "CNT_Economical_Activity__c", con.Any);
                obj.ContractCase = schema.getFieldValueMore("Contract__r", "", "", "CNT_Case__c", con.Any);

                obj.BillingId = schema.getFieldValueMore("Billing_Profile__r", "", "", "Id", con.Any);
                obj.AccountIdBilling = schema.getFieldValueMore("Billing_Profile__r", "Account__r", "", "Id", con.Any);
                obj.BillingContactName = schema.getFieldValueMore("Billing_Profile__r", "Account__r", "", "Name", con.Any);
                obj.PointOfDelivery = schema.getFieldValueMore("Billing_Profile__r", "", "", "PointofDelivery__c", con.Any);
                obj.BillingNumeroCliente = schema.getFieldValueMore("Billing_Profile__r", "PointofDelivery__r", "", "Name", con.Any);
                obj.BillingRecordTypeId = schema.getFieldValueMore("Billing_Profile__r", "", "", "RecordTypeId", con.Any);
                obj.BillingExternalId = schema.getFieldValueMore("Billing_Profile__r", "", "", "ExternalId__c", con.Any);
                obj.BillingVencimento = schema.getFieldValueMore("Billing_Profile__r", "", "", "CNT_Due_Date__c", con.Any);
                obj.BillingEnderecoFisico = schema.getFieldValueMore("Billing_Profile__r", "", "", "Address__c", con.Any);
                obj.BillingEnderecoEntrega = schema.getFieldValueMore("Billing_Profile__r", "", "", "BillingAddress__c", con.Any);
                obj.BillingFormaPagamento = schema.getFieldValueMore("Billing_Profile__r", "", "", "Type__c", con.Any);

                result.Add(obj);
            }

            return result;
        }


        public static List<BillingSalesforce> ToBillingProfile(List<sObject> cons)
        {
            SFDCSchemeBuild schema = new SFDCSchemeBuild();

            List<BillingSalesforce> result = new List<BillingSalesforce>();

            foreach (sObject con in cons)
            {
                BillingSalesforce obj = new BillingSalesforce();

                obj.PoDSF = schema.getFieldValueMore("Billing_Profile__r", "", "", "Pointofdelivery__c", con.Any);
                obj.AccountContract__c = schema.getFieldValueMore("Billing_Profile__r", "", "", "AccountContract__c", con.Any);
                obj.CNT_Contract__c = schema.getFieldValueMore("Billing_Profile__r", "", "", "CNT_Contract__c", con.Any);
                obj.ExternalID__c = schema.getFieldValueMore("Billing_Profile__r", "", "", "ExternalID__c", con.Any);
                obj.NumeroCliente = schema.getFieldValueMore("Billing_Profile__r", "Pointofdelivery__r", "", "Name", con.Any);
                obj.TipoCliente = schema.getFieldValueMore("Billing_Profile__r", "Pointofdelivery__r", "", "SegmentType__c", con.Any);
                obj.IdentityNumber__c = schema.getFieldValueMore("Billing_Profile__r", "Account__r", "", "IdentityNumber__c", con.Any);
                obj.IdentityType__c = schema.getFieldValueMore("Billing_Profile__r", "Account__r", "", "IdentityType__c", con.Any);

                result.Add(obj);
            }

            return result;
        }

        public static List<AddressSalesforce> FromBillingToAddress(List<sObject> cons)
        {
            SFDCSchemeBuild schema = new SFDCSchemeBuild();

            List<AddressSalesforce> result = new List<AddressSalesforce>();

            foreach (sObject con in cons)
            {
                AddressSalesforce obj = new AddressSalesforce();
                obj.Id = schema.getFieldValueMore("DetailAddress__r", "", "", "Id", con.Any);
                obj.TipoEndereco = schema.getFieldValueMore("DetailAddress__r", "", "", "Street_type__c", con.Any);
                obj.Endereco = schema.getFieldValueMore("DetailAddress__r", "StreetMD__r", "", "Street__c", con.Any);
                obj.ExternalId__c = schema.getFieldValueMore("DetailAddress__r", "", "", "ExternalId__c", con.Any);
                obj.Municipality__c = schema.getFieldValueMore("DetailAddress__r", "", "", "Municipality__c", con.Any);
                obj.Number__c = schema.getFieldValueMore("DetailAddress__r", "", "", "Number__c", con.Any);
                obj.Postal_Code__c = schema.getFieldValueMore("DetailAddress__r", "", "", "Postal_Code__c", con.Any);
                obj.Region = schema.getFieldValueMore("DetailAddress__r", "", "", "Region__c", con.Any);
                obj.Complemento = schema.getFieldValueMore("DetailAddress__r", "", "", "Corner__c", con.Any);
                obj.Bairro = schema.getFieldValueMore("DetailAddress__r", "StreetMD__r", "", "Neighbourhood__c", con.Any);

                obj.NumeroCliente = schema.getFieldValue("Name", con.Any);
                //obj.ContaContrato = schema.getFieldValue("AccountContract__c", con.Any);
                obj.ContaContrato = schema.getFieldValueMore("CNT_Contract__r", "", "", "CNT_ExternalContract_ID__c", con.Any);

                result.Add(obj);
            }

            return result;
        }

        public static List<ContractLineItemGoverno> ToContractLineItemGoverno(List<sObject> cons)
        {
            SFDCSchemeBuild schema = new SFDCSchemeBuild();
            List<ContractLineItemGoverno> result = new List<ContractLineItemGoverno>();
            foreach (sObject con in cons)
            {
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
                obj.ContratoAgrupamento = schema.getFieldValueMore("GroupAccountContract__r", "", "", "AccountContract__c", con.Any);
                obj.ContaAgrupamento = schema.getFieldValueMore("GroupAccountContract__r", "", "", "Account__c", con.Any);
                obj.ContaAgrupamentoParentId = schema.getFieldValueMore("GroupAccountContract__r", "Account__r", "", "ParentId", con.Any);

                result.Add(obj);
            }

            return result;
        }

        public static List<OrderSalesforce> ToOrderItems(List<sObject> cons)
        {
            SFDCSchemeBuild schema = new SFDCSchemeBuild();

            List<OrderSalesforce> result = new List<OrderSalesforce>();

            foreach (sObject con in cons)
            {
                OrderSalesforce obj = new OrderSalesforce();
                obj.Id = schema.getFieldValue("Id", con.Any);
                obj.Name = schema.getFieldValue("Name", con.Any);
                obj.RecurringChargeOv = schema.getFieldValue("NE__RecurringChargeOv__c", con.Any);

                result.Add(obj);
            }

            return result;
        }


        public static List<AssetDTO> ToAssetList(List<sObject> cons)
        {
            SFDCSchemeBuild schema = new SFDCSchemeBuild();

            List<AssetDTO> result = new List<AssetDTO>();

            foreach (sObject con in cons)
            {
                AssetDTO obj = new AssetDTO();
                obj.Id = schema.getFieldValueMore("Asset__r", "", "", "Id", con.Any);
                obj.ExternalId = schema.getFieldValueMore("Asset__r", "", "", "ExternalId__c", con.Any);
                obj.ContractExternalId = schema.getFieldValueMore("Contract__r", "", "", "ExternalId__c", con.Any);
                obj.OrderId = schema.getFieldValueMore("Asset__r", "", "", "NE__Order_Config__c", con.Any);
                obj.PointofDeliveryExternalId = schema.getFieldValueMore("Asset__r", "PointofDelivery__r", "", "ExternalId__c", con.Any);
                obj.NumeroCliente = schema.getFieldValueMore("Asset__r", "PointofDelivery__r", "", "Name", con.Any);
                obj.PointofDeliveryBaixaRendaFlag = schema.getFieldValueMore("Asset__r", "PointofDelivery__r", "", "CNT_LowIncomeType__c", con.Any);
                
                result.Add(obj);
            }

            return result;
        }


        
        public static List<ClienteSalesforce> ToClienteSalesforce(List<sObject> cons)
        {
            SFDCSchemeBuild schema = new SFDCSchemeBuild();

            List<ClienteSalesforce> result = new List<ClienteSalesforce>();

            foreach (sObject con in cons)
            {
                ClienteSalesforce obj = new ClienteSalesforce();
                obj.Id = schema.getFieldValue("Id", con.Any);
                obj.NumeroCliente = schema.getFieldValue("Name", con.Any);
                obj.PodCompany = schema.getFieldValue("CompanyID__c", con.Any);
                obj.DetailAddress__c = schema.getFieldValue("DetailAddress__c", con.Any);
                obj.ExternalId = schema.getFieldValue("ExternalId__c", con.Any);

                result.Add(obj);
            }

            return result;
        }

        public static List<OrderSalesforce> FromOrderItemAttributeToOrders(List<sObject> cons)
        {
            SFDCSchemeBuild schema = new SFDCSchemeBuild();

            List<OrderSalesforce> result = new List<OrderSalesforce>();

            foreach (sObject con in cons)
            {
                //NE__Order_Item__r.NE__OrderId__c
                //, NE__Order_Item__r.Id
                //, id
                //, name
                //, NE__Value__c
                
                OrderSalesforce obj = new OrderSalesforce();
                obj.Id = schema.getFieldValueMore("NE__Order_Item__r", "", "NE__OrderId__r", "Id", con.Any);
                obj.RecurringChargeOv = schema.getFieldValue("NE__RecurringChargeOv__c", con.Any);
                obj.Billing_profile__c = schema.getFieldValueMore("NE__Order_Item__r", "", "NE__OrderId__r", "Billing_profile__c", con.Any);
                obj.NE__Country__c = schema.getFieldValueMore("NE__Order_Item__r", "", "NE__OrderId__r", "NE__Country__c", con.Any);
                obj.CurrencyIsoCode = schema.getFieldValueMore("NE__Order_Item__r", "", "NE__OrderId__r", "CurrencyIsoCode", con.Any);
                obj.NE__AccountId__c = schema.getFieldValueMore("NE__Order_Item__r", "", "NE__OrderId__r", "NE__AccountId__c", con.Any);
                obj.NE__BillAccId__c = schema.getFieldValueMore("NE__Order_Item__r", "", "NE__OrderId__r", "NE__BillAccId__c", con.Any);
                obj.NE__CatalogId__c = schema.getFieldValueMore("NE__Order_Item__r", "", "NE__OrderId__r", "NE__CatalogId__c", con.Any);
                obj.NE__Order_date__c = schema.getFieldValueMore("NE__Order_Item__r", "", "NE__OrderId__r", "NE__Order_date__c", con.Any);
                obj.NE__rTypeName__c = schema.getFieldValueMore("NE__Order_Item__r", "", "NE__OrderId__r", "NE__rTypeName__c", con.Any);
                obj.NE__ServAccId__c = schema.getFieldValueMore("NE__Order_Item__r", "", "NE__OrderId__r", "NE__ServAccId__c", con.Any);
                obj.NE__Type__c = schema.getFieldValueMore("NE__Order_Item__r", "", "NE__OrderId__r", "NE__Type__c", con.Any);
                obj.RecordTypeId = schema.getFieldValueMore("NE__Order_Item__r", "", "NE__OrderId__r", "RecordTypeId", con.Any);

                result.Add(obj);
            }

            return result;
        }

        public static List<CaseSalesforce> ToCases(List<sObject> cons)
        {
            SFDCSchemeBuild schema = new SFDCSchemeBuild();

            List<CaseSalesforce> result = new List<CaseSalesforce>();

            foreach (sObject con in cons)
            {
                CaseSalesforce obj = new CaseSalesforce();
                obj.Id = schema.getFieldValue("Id", con.Any);
                obj.ContractNumber = schema.getFieldValueMore("CNT_Contract__r", "", "", "ContractNumber", con.Any);
                //obj.ContractExternalId = schema.getFieldValueMore("CNT_Contract__r", "", "", "CNT_ExternalContract_ID_2__c", con.Any);
                //obj.Status9 = schema.getFieldValueMore("Contracting_Status_BackOffices__r", "records", "9", "Status__c", con.Any);

                result.Add(obj);
            }

            return result;
        }


        public static List<CaseStatus> ToCaseStatus(List<sObject> cons)
        {
            SFDCSchemeBuild schema = new SFDCSchemeBuild();

            List<CaseStatus> result = new List<CaseStatus>();

            foreach (sObject con in cons)
            {
                CaseStatus obj = new CaseStatus();
                obj.Id = schema.getFieldValue("Id", con.Any);
                obj.CaseNumber = schema.getFieldValue("CaseNumber", con.Any);
                obj.Status = schema.getFieldValue("Status", con.Any);
                obj.Type = schema.getFieldValue("Type", con.Any);
                obj.PoDName = schema.getFieldValueMore("pointofdelivery__r", "", "", "name", con.Any);
                obj.ContractNumber = schema.getFieldValueMore("CNT_Contract__r", "", "", "ContractNumber", con.Any);
                obj.ContractStatus = schema.getFieldValueMore("CNT_Contract__r", "", "", "status", con.Any);
                obj.ContractExternalId = schema.getFieldValueMore("CNT_Contract__r", "", "", "CNT_ExternalContract_ID_2__c", con.Any);
                obj.CreatedDate = schema.getFieldValue("CreatedDate", con.Any);
                obj.AccountName = schema.getFieldValueMore("Account", "", "", "name", con.Any);
                //obj.Status0 = schema.getFieldValueMore("Contracting_Status_BackOffices__r", "records", "0", "Status__c", con.Any);
                //obj.Status1 = schema.getFieldValueMore("Contracting_Status_BackOffices__r", "records", "1", "Status__c", con.Any);
                //obj.Status2 = schema.getFieldValueMore("Contracting_Status_BackOffices__r", "records", "2", "Status__c", con.Any);
                //obj.Status3 = schema.getFieldValueMore("Contracting_Status_BackOffices__r", "records", "3", "Status__c", con.Any);
                //obj.Status4 = schema.getFieldValueMore("Contracting_Status_BackOffices__r", "records", "4", "Status__c", con.Any);
                //obj.Status5 = schema.getFieldValueMore("Contracting_Status_BackOffices__r", "records", "5", "Status__c", con.Any);
                //obj.Status6 = schema.getFieldValueMore("Contracting_Status_BackOffices__r", "records", "6", "Status__c", con.Any);
                //obj.Status7 = schema.getFieldValueMore("Contracting_Status_BackOffices__r", "records", "7", "Status__c", con.Any);
                //obj.Status8 = schema.getFieldValueMore("Contracting_Status_BackOffices__r", "records", "8", "Status__c", con.Any);
                //obj.Status9 = schema.getFieldValueMore("Contracting_Status_BackOffices__r", "records", "9", "Status__c", con.Any);

                result.Add(obj);
            }

            return result;
        }


        public static List<ClienteSalesforce> ToPointOfDelivery(List<sObject> cons)
        {
            SFDCSchemeBuild schema = new SFDCSchemeBuild();

            List<ClienteSalesforce> result = new List<ClienteSalesforce>();

            foreach (sObject con in cons)
            {
                ClienteSalesforce obj = new ClienteSalesforce();
                obj.IdPod = schema.getFieldValue("Id", con.Any);
                obj.NumeroCliente = schema.getFieldValue("Name", con.Any);

                result.Add(obj);
            }

            return result;
        }


        public static List<ClienteSalesforce> FromContractLineItemsToPointOfDeliveries(List<ContractLineItemSalesforce> contratos)
        {
            SFDCSchemeBuild schema = new SFDCSchemeBuild();

            List<ClienteSalesforce> result = new List<ClienteSalesforce>();

            foreach (ContractLineItemSalesforce con in contratos)
            {
                ClienteSalesforce obj = new ClienteSalesforce();
                obj.IdPod = con.PointOfDelivery;
                obj.NumeroCliente = con.NumeroCliente;

                result.Add(obj);
            }

            return result;
        }


        public static List<ClienteSalesforce> FromBillingProfilesToPointOfDeliveries(List<BillingSalesforce> billings)
        {
            SFDCSchemeBuild schema = new SFDCSchemeBuild();

            List<ClienteSalesforce> result = new List<ClienteSalesforce>();

            foreach (BillingSalesforce con in billings)
            {
                ClienteSalesforce obj = new ClienteSalesforce();
                obj.IdPod = con.PoDSF;
                obj.NumeroCliente = con.NumeroCliente;
                obj.ContaContrato = con.AccountContract__c;
                obj.Documento = con.IdentityNumber__c;
                obj.TipoDocumento = con.IdentityType__c;
                obj.TipoCliente = con.TipoCliente;

                result.Add(obj);
            }

            return result;
        }


        public static List<DeviceSalesforce> ToDevice(List<sObject> cons)
        {
            SFDCSchemeBuild schema = new SFDCSchemeBuild();

            List<DeviceSalesforce> result = new List<DeviceSalesforce>();

            foreach (sObject con in cons)
            {
                DeviceSalesforce obj = new DeviceSalesforce();
                obj.Id = schema.getFieldValue("Id", con.Any);
                obj.PointOfDeliveryId = schema.getFieldValue("PointofDelivery__c", con.Any);
                obj.Numero = schema.getFieldValue("MeterNumber__c", con.Any);
                obj.Nome = schema.getFieldValue("Name", con.Any);
                obj.Estado = schema.getFieldValue("Status__c", con.Any);

                result.Add(obj);
            }

            return result;
        }
    }
}

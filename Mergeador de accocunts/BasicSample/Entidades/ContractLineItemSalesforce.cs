using SalesforceExtractor.apex;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;

namespace SalesforceExtractor.Entidades
{
    public class ContractLineItemSalesforce : sObject
    {
        [Display(Name = "Id")]
        public string Id { get; set; }

        [Display(Name = "ExternalId__c")]
        public string ExternalId__c { get; set; }

        [Display(Name = "CNT_Status__c")]
        public string CNT_Status__c { get; set; }

        [Display(Name = "GroupAccountContract__c")]
        public string GroupAccountContract__c { get; set; }



        [Display(Name = "Asset__r.Contact.Name")]
        public string ContactName { get; set; }

        [Display(Name = "Asset__r.ContactId")]
        public string ContactId { get; set; }

        [Display(Name = "Asset__r.Accountid")]
        public string AccountIdAsset { get; set; }

        [Display(Name = "Asset__r.ExternalId__c")]
        public string AssetExternalId { get; set; }

        [Display(Name = "Asset__c")]
        public string AssetId { get; set; }

        [Display(Name = "Asset__r.Name")]
        public string AssetName { get; set; }

        [Display(Name = "Asset__r.Account.Name")]
        public string AssetContactName { get; set; }

        [Display(Name = "Asset__r.NE_Status__c")]
        public string AssetStatus { get; set; }

        [Display(Name = "Asset__r.PointOfDelivery__c")]
        public string PointOfDelivery { get; set; }

        [Display(Name = "Asset__r.PointOfDelivery__c.DetailAddress__c")]
        public string PointOfDeliveryAddress { get; set; }

        [Display(Name = "Asset.Account.AccountNumber")]
        public string NumeroCliente { get; set; }

        [Display(Name = "Asset__r.Account.ExternalId__c")]
        public string AccountExternalId { get; set; }

        [Display(Name = "Asset__r.Account.Id")]
        public string AccountId { get; set; }

        [Display(Name = "Asset__r.NE__Order_Config__c")]
        public string OrderConfig{ get; set; }

        [Display(Name = "Asset__r.RecordTypeId")]
        public string AssetRecordType { get; set; }



        [Display(Name = "Contract__c")]
        public string ContractId { get; set; }

        [Display(Name = "Contract__r.ContractNumber")]
        public string ContractNumber { get; set; }

        [Display(Name = "Contract__r.ExternalID__c")]
        public string ContractExternalId { get; set; }

        [Display(Name = "Contract__r.CNT_Quote__c")]
        public string ContractQuote { get; set; }

        [Display(Name = "Contract__r.Status")]
        public string ContractStatus { get; set; }
        
        [Display(Name = "Contract__r.Contract_Type__c")]
        public string ContractType { get; set; }

        [Display(Name = "Contract__r.StartDate")]
        public string ContractStartDate { get; set; }

        [Display(Name = "Contract__r.AccountId")]
        public string ContractAccountId { get; set; }

        [Display(Name = "Contract__r.RecordType.Id")]
        public string ContractRecordTypeId { get; set; }

        [Display(Name = "Contract__r.CNT_Economical_Activity__c")]
        public string ContractAtividadeEconomica { get; set; }

        [Display(Name = "Contract__r.CNT_Case__c")]
        public string ContractCase { get; set; }
                


        [Display(Name = "BilllingProfile__c")]
        public string BillingProfile__c { get; set; }

        [Display(Name = "BilllingProfile__r.Id")]
        public string BillingId { get; set; }

        [Display(Name = "Billing_Profile__r.Account__r.Name")]
        public string BillingContactName { get; set; }

        [Display(Name = "Billing_Profile__r.PointOfDelivery__r.Name")]
        public string BillingNumeroCliente { get; set; }

        [Display(Name = "BillingProfile__r.Asset__r.Accountid")]
        public string AccountIdBilling { get; set; }

        [Display(Name = "Billing_Profile__r.RecordTypeId")]
        public string BillingRecordTypeId { get; set; }

        [Display(Name = "Billing_Profile__r.ExternalId__c")]
        public string BillingExternalId { get; set; }

        [Display(Name = "Billing_Profile__r.CNT_Due_Date__c")]
        public string BillingVencimento { get; set; }

        [Display(Name = "Billing_Profile__r.Address__c")]
        public string BillingEnderecoFisico { get; set; }

        [Display(Name = "Billing_Profile__r.BillingAddress__c")]
        public string BillingEnderecoEntrega { get; set; }

        [Display(Name = "Billing_Profile__r.Payment_Type__c")]
        public string BillingFormaPagamento { get; set; }


        [Display(Name = "Contract__r.CNT_Generation_Type__c")]
        public string GenerationType
        {
            get
            {
                return (Beneficiarios == null || Beneficiarios.Count == 0) ? string.Empty :
                    (Beneficiarios.ContainsKey(this.NumeroCliente) && Beneficiarios.Count == 1) ? "Consumo_Local" :
                    (Beneficiarios.ContainsKey(this.NumeroCliente) && Beneficiarios.Count > 1) ?
                    "Auto_Consumo_Remoto" : "Geracao_Compartilhada";
            }
        }

        /// <summary>
        /// Lista de Número Clientes, que são consumidores da geração distribuída pelo Contrato/Cliente atual (pode conter o próprio contrato)
        /// </summary>
        public Dictionary<string, string> Beneficiarios { get; set; }

        [Display(Name = "Asset__r.Account.IdentityNumber__c")]
        public string AccountIdentityNumber { get; set; }
    }
}

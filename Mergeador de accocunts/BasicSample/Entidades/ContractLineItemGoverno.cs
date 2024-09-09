using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;

namespace SalesforceExtractor.Entidades
{
    public class ContractLineItemGoverno
    {
        public ContractLineItemGoverno()
        {

        }

        public ContractLineItemGoverno(ContractLineItemSalesforce c)
        {
            this.NumeroCliente = c.NumeroCliente;
        }

        #region Dados Governo
        public string TipoEnderecoPoD { get; set; }
        public string EnderecoPoD { get; set; }
        public string EnderecoCompleto { get { return string.Concat(this.TipoEnderecoPoD, " ", this.EnderecoPoD); } }
        public string MunicipioPoD { get; set; }
        public string Tarifa { get; set; }
        public string Identidade { get; set; }
        public string TipoIdentidade { get; set; }

        public string Classe { get; set; }
        public string SubClasse { get; set; }
        public string ContratoColetivo { get; set; }
        
        [Display(Name = "GroupAccountContract__r.AccountContract__c")]
        public string ContratoAgrupamento { get; set; }

        [Display(Name = "GroupAccountContract__r.Account__c")]
        public string ContaAgrupamento { get; set; }

        [Display(Name = "GroupAccountContract__r.Account__r.ParentId")]
        public string ContaAgrupamentoParentId { get; set; }

        public string Executivo { get; set; }
        public string OrgaoControlador { get; set; }
        public string Segmento { get; set; }
        public string Area { get; set; }
        #endregion

        [Display(Name = "Id")]
        public string Id { get; set; }

        [Display(Name = "Asset__r.Accountid")]
        public string AccountIdAsset { get; set; }

        [Display(Name = "BillingProfile__r.Asset__r.Accountid")]
        public string AccountIdBilling { get; set; }
        
        [Display(Name = "Asset__r.ExternalId__c")]
        public string AssetExternalId { get; set; }

        [Display(Name = "Asset__c")]
        public string AssetId { get; set; }

        [Display(Name = "Asset__r.Name")]
        public string AssetName { get; set; }

        [Display(Name = "Asset__r.Account.Name")]
        public string AssetAccountName { get; set; }

        [Display(Name = "Asset__r.NE_Status__c")]
        public string AssetStatus { get; set; }

        [Display(Name = "Asset__r.PointOfDelivery__c")]
        public string PointOfDelivery { get; set; }

        [Display(Name = "Asset.Account.Id")]
        public string AccountId { get; set; }

        [Display(Name = "Asset.Account.ParentId")]
        public string AccountParentId { get; set; }

        [Display(Name = "Asset.Account.AccountNumber")]
        public string NumeroCliente { get; set; }

        [Display(Name = "Asset__r.Account.ExternalId__c")]
        public string AccountExternalId { get; set; }

        [Display(Name = "Contract__c")]
        public string ContractId { get; set; }

        [Display(Name = "Contract__r.ContractNumber")]
        public string ContractNumber { get; set; }

        [Display(Name = "Contract__r.ExternalID__c")]
        public string ContractExternalId { get; set; }

        [Display(Name = "Contract__r.CNT_Quote__c")]
        public string Quote { get; set; }

        [Display(Name = "Asset__r.NE__Order_Config__c")]
        public string OrderConfig{ get; set; }

        [Display(Name = "Contract__r.Contract_Type__c")]
        public string ContractType { get; set; }

        [Display(Name = "CNT_Status__c")]
        public string CNT_Status__c { get; set; }

        [Display(Name = "BilllingProfile__c")]
        public string BillingProfile__c { get; set; }

        [Display(Name = "BilllingProfile__r.Id")]
        public string BillingId { get; set; }

        [Display(Name = "Billing_Profile__r.Account__r.Name")]
        public string BillingContactName { get; set; }

        [Display(Name = "Billing_Profile__r.PointOfDelivery__r.Name")]
        public string BillingNumeroCliente { get; set; }

        [Display(Name = "GroupAccountContract__c")]
        public string GroupAccountContract__c { get; set; }

        [Display(Name = "Asset__r.Contact.Name")]
        public string ContactName { get; set; }

        [Display(Name = "Asset__r.ContactId")]
        public string ContactId { get; set; }
    }
}

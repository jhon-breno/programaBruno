using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace SalesforceExtractor.Entidades
{
    [DebuggerDisplay("Id:{Id} AccountId:{AccountId} Cliente:{NumeroCliente}")]
    public class AssetDTO
    {
        //[Display(Name = "Id")]
        public string Id { get; set; }

        //[Display(Name = "ExternalId__c")]
        public string ExternalId { get; set; }

        //[Display(Name = "Account.Id")]
        //Account.ExternalId__c
        public string AccountId { get; set; }

        //[Display(Name = "Account.ExternalId__c")]
        public string AccountExternalId { get; set; }

        public bool AccountInvalido { get { return this.AccountExternalId.ToUpper().Contains("INVALIDO"); } }

        //[Display(Name = "Account.IdentityNumber__c")]
        public string Identidade { get; set; }

        //[Display(Name = "Account.IdentityType__c")]
        public string TipoIdentidade { get; set; }

        //[Display(Name = "Name")]
        public string NumeroCliente { get; set; }

        //[Display(Name = "Account.Name")]
        public string NomeCliente { get; set; }

        //[Display(Name = "Account.CNT_Client_Type__c")]
        public string TipoCliente { get; set; }

        //[Display(Name = "Account.CNT_State_Inscription_Exemption__c")]
        public string IsencaoMunicipal { get; set; }

        //[Display(Name = "CreatedDate")]
        public string DataCriacao { get; set; }

        //[Display(Name = "PointofDelivery__c.Id")]
        public string PointofDeliveryId{ get; set; }

        //[Display(Name = "PointofDelivery__c.ExternalId")]
        public string PointofDeliveryExternalId { get; set; }

        //[Display(Name = "Asset__r.NE__Order_Config__c")]
        public string OrderItemId { get; set; }

        //[Display(Name = "Contract__c")]
        public string ContractExternalId { get; set; }

        //[Display(Name = "Contract__c")]
        public string ContractId { get; set; }

        public string OrderId { get; set; }

        public string Status { get; set; }

        public string Name { get; set; }


        //[Display(Name = "Contract__r.CNT_LowIncomeType__c")]
        public string ContractBaixaRendaFlag { get; set; }

        //[Display(Name = "Asset__r.PointofDelivery__r.CNT_LowIncomeType__c")]
        public string PointofDeliveryBaixaRendaFlag { get; set; }

        //[Display(Name = "Contract__r.ContractNumber")]
        public string ContractNumber { get; set; }


        ////[Display(Name = "NE__Service_Account__r.ExternalId__c")]
        //public string ServiceAccountExternalId { get; set; }

        ////[Display(Name = "NE__Billing_Account__r.ExternalId__c")]
        //public string BillingAccountExternalId { get; set; }
    }
}

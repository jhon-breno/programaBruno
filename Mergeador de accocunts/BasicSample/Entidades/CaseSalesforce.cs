using SalesforceExtractor.apex;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace SalesforceExtractor.Entidades
{
    //Case
    [DebuggerDisplay("Id: {Id} Case: {NumeroCaso} Account: {AccountId}")]
    public class CaseSalesforce : sObject
    {
        [Display(Name = "Id")]
        public string Id { get; set; }

        [Display(Name = "Status")]
        public string Status { get; set; }

        [Display(Name = "AccountId")]
        public string AccountId { get; set; }

        [Display(Name = "ClosedDate")]
        public string ClosedDate { get; set; }

        [Display(Name = "IsClosed")]
        public bool IsClosed { get; set; }

        [Display(Name = "CaseNumber")]
        public string NumeroCaso { get; set; }

        [Display(Name = "InserviceNumber__c")]
        public string NumeroAviso { get; set; }

        [Display(Name = "APCaseNumber__c")]
        public string NumeroAvisoIluminacaoPublica { get; set; }

        [Display(Name = "Reason")]     //    --Motivo
        public string Reason { get; set; }

        [Display(Name = "SubCauseBR__c")]     //     --Submotivo
        public string SubCauseBR__c { get; set; }

        [Display(Name = "ContactId")]     //
        public string ContactId { get; set; }

        [Display(Name = "AssetId")]     //
        public string AssetId { get; set; }

        [Display(Name = "PointofDelivery__c")] 
        public string PointofDelivery__c { get; set; }

        [Display(Name = "PointOfDelivery.IdentityNumber__c")]
        public string IdentityNumber { get; set; }

        [Display(Name = "PointOfDelivery.DetailAddress__c")]     //
        public string Address { get; set; }

        [Display(Name = "CNT_Quote__c")]     //      --NE__Order__c
        public string CNT_Quote__c { get; set; }

        [Display(Name = "CNT_Contract__c")]     //
        public string CNT_Contract__c { get; set; }

        [Display(Name = "RecordTypeId")]     //
        public string RecordTypeId { get; set; }

        [Display(Name = "CNT_LastInvoiceOptions__c")]     //
        public string CNT_LastInvoiceOptions__c { get; set; }

        [Display(Name = "Asset__r.NE__Order_Config__c")]     //
        public string NE__Order_Config__c { get; set; }

        [Display(Name = "CNT_Contract__r.ContractNumber")]     //
        public string ContractNumber { get; set; }

        public ContractSalesforce Contract { get; set; }
        public BillingSalesforce Billing { get; set; }
        public ContractLineItemSalesforce ContractLineItem { get; set; }
    }
}

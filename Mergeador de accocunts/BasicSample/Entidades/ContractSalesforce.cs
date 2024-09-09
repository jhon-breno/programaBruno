using SalesforceExtractor.apex;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;

namespace SalesforceExtractor.Entidades
{
    //Contract
    public class ContractSalesforce : sObject
    {
        [Display(Name = "Contract__r.Id")]
        public string Id { get; set; }

        [Display(Name = "Company_ID__c")]
        public string Company_ID__c { get; set; }
        
        [Display(Name = "Contract__r.name")]
        public string Name { get; set; }

        [Display(Name = "AccountId")]
        public string Account { get; set; }

        [Display(Name = "StartDate")]
        public string DataInicio { get; set; }

        [Display(Name = "CNT_GroupTypeContract__c")]
        public string TipoAgrupamento { get; set; }

        [Display(Name = "CNT_GroupArea__c")]
        public string AreaAgrupamento { get; set; }

        [Display(Name = "CNT_GroupSegment__c")]
        public string SegmentoAgrupamento { get; set; }

        [Display(Name = "CNT_GroupNumerCntr__c")]
        public string NumeroAgrupamento { get; set; }

        [Display(Name = "Status")]
        public string Status { get; set; }

        [Display(Name = "Contract__r.ContractNumber")]
        public string ContractNumber { get; set; }

        [Display(Name = "Asset__r.Account.AccountNumber")]
        public string NumeroCliente { get; set; }

        [Display(Name = "Contract__r.ExternalID__c")]
        public string ExternalId { get; set; }

        [Display(Name = "Contract__r.Contract_Type__c")]
        public string ContractType { get; set; }

        [Display(Name = "RecordTypeId")]
        public string RecordTypeId { get; set; }

        [Display(Name = "CNT_Quote__c")]
        public string CNT_Quote__c { get; set; }

        [Display(Name = "CNT_Case__c")]
        public string CNT_Case__c { get; set; }

        [Display(Name = "CNT_Economical_Activity__c")]
        public string CNAE { get; set; } 
    }
}

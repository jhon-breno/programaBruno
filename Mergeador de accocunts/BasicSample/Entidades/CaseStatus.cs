using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace SalesforceExtractor.Entidades
{
    //Case
    [DebuggerDisplay("Id: {Id} Case: {CaseNumber} PoD: {PoDName}")]
    public class CaseStatus
    {
        [Display(Name = "Id")]
        public string Id { get; set; }

        [Display(Name = "CaseNumber")]
        public string CaseNumber { get; set; }

        [Display(Name = "status")]
        public string Status { get; set; }

        [Display(Name = "type")]
        public string Type { get; set; }

        [Display(Name = "pointofdelivery__r.name")]
        public string PoDName { get; set; }

        [Display(Name = "CNT_Contract__r.ContractNumber")]
        public string ContractNumber { get; set; }

        [Display(Name = "CNT_Contract__r.status")]
        public string ContractStatus { get; set; }

        [Display(Name = "CNT_Contract__r.CNT_ExternalContract_ID_2__c")]
        public string ContractExternalId { get; set; }

        [Display(Name = "CreatedDate")]
        public string CreatedDate { get; set; }

        [Display(Name = "case.account.name")]
        public string AccountName { get; set; }

        [Display(Name = "Contracting_Status_BackOffices__r.records.0.Status__c")]
        public string Status0 { get; set; }

        [Display(Name = "Contracting_Status_BackOffices__r.records.1.Status__c")]
        public string Status1 { get; set; }

        [Display(Name = "Contracting_Status_BackOffices__r.records.2.Status__c")]
        public string Status2 { get; set; }

        [Display(Name = "Contracting_Status_BackOffices__r.records.3.Status__c")]
        public string Status3 { get; set; }

        [Display(Name = "Contracting_Status_BackOffices__r.records.4.Status__c")]
        public string Status4 { get; set; }

        [Display(Name = "Contracting_Status_BackOffices__r.records.5.Status__c")]
        public string Status5 { get; set; }

        [Display(Name = "Contracting_Status_BackOffices__r.records.6.Status__c")]
        public string Status6 { get; set; }

        [Display(Name = "Contracting_Status_BackOffices__r.records.7.Status__c")]
        public string Status7 { get; set; }

        [Display(Name = "Contracting_Status_BackOffices__r.records.8.Status__c")]
        public string Status8 { get; set; }

        [Display(Name = "Contracting_Status_BackOffices__r.records.9.Status__c")]
        public string Status9 { get; set; }
    }
}

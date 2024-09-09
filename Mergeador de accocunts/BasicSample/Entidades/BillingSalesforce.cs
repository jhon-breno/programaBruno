using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;

namespace SalesforceExtractor.Entidades
{
    //Billing_Profile__c
    public class BillingSalesforce
    {
        //[Display(Name = "Contact__c")]
        //public string ContatoSF { get; set; }

        [Display(Name = "Id")]          //<<<<<<<<<<<<<<<<<<<<<<<<<<<
        public string Id { get; set; }

        [Display(Name = "Account__c")]          //<<<<<<<<<<<<<<<<<<<<<<<<<<<
        public string AccountSF { get; set; }
        
        [Display(Name = "Type__c")]
        public string Type__c { get; set; }
        
        [Display(Name = "BallotName__c")]
        public string BallotName__c { get; set; }
        
        [Display(Name = "Bank__c")]
        public string Bank__c { get; set; }

        [Display(Name = "BillingAddress__c")]   
        public string BillingAddress__c { get; set; }

        [Display(Name = "CurrentAccountNum__c")]
        public string CurrentAccountNum__c { get; set; }

        [Display(Name = "CurrentAccountNumber__c")]
        public string CurrentAccountNumber__c { get; set; }

        [Display(Name = "ExternalID__c")]       
        public string ExternalID__c { get; set; }

        [Display(Name = "PointofDelivery__c")]  //<<<<<<<<<<<<<<<<<<<<<<<<<<<
        public string PoDSF { get; set; }

        [Display(Name = "Pointofdelivery__r.Name")]       
        public string NumeroCliente { get; set; }

        [Display(Name = "AccountContract__c")]
        public string AccountContract__c { get; set; }

        [Display(Name = "DeliveryType__c")]
        public string DeliveryType__c { get; set; }

        [Display(Name = "CNT_Braile__c")]
        public string CNT_Braile__c { get; set; }

        [Display(Name = "CNT_Due_Date__c")]
        public string CNT_Due_Date__c { get; set; }

        [Display(Name = "Company__c")]
        public string Company__c { get; set; }

        [Display(Name = "CNT_GroupPayType__c")]
        public string CNT_GroupPayType__c { get; set; }

        [Display(Name = "CNT_Lot__c")]
        public string CNT_Lot__c { get; set; }

        [Display(Name = "Address__c")]
        public string Address__c { get; set; }

        [Display(Name = "Account__c")]
        public string Account__c { get; set; }

        [Display(Name = "RecordTypeId")]
        public string RecordTypeId { get; set; }

        [Display(Name = "CNT_Contract__c")]
        public string CNT_Contract__c { get; set; }

        [Display(Name = "CNT_Contract_Number__c")]
        public string CNT_Contract_Number__c { get; set; }

        [Display(Name = "CNT_GroupClass__c")]
        public string CNT_GroupClass__c { get; set; }

        [Display(Name = "Account__r.IdentityNumber__c")]
        public string IdentityNumber__c { get; set; }

        [Display(Name = "Account__r.IdentityType__c")]
        public string IdentityType__c { get; set; }

        [Display(Name = "PointofDelivery__r.SegmentType__c")]
        public string TipoCliente { get; set; }


        [Display(Name = "Address__r.Id")]
        public string AddressId { get; set; }

        [Display(Name = "Address__r.Region__c")]
        public string AddressRegion { get; set; }

        [Display(Name = "Address__r.ConcatenatedAddress__c")]
        public string AddressConcatenatedAddress__c { get; set; }

        [Display(Name = "Address__r.ExternalId__c")]
        public string AddressExternalId__c { get; set; }

        [Display(Name = "Address__r.Municipality__c")]
        public string AddressMunicipality__c { get; set; }

        [Display(Name = "Address__r.Number__c")]
        public string AddressNumber__c { get; set; }

        [Display(Name = "Address__r.Postal_Code__c")]
        public string AddressPostal_Code__c { get; set; }

        [Display(Name = "Address__r.Region__c")]
        public string AddressRegion__c { get; set; }

        [Display(Name = "Address__r.CNT_Number__c")]
        public string AddressCNT_Number__c { get; set; }
    }
}

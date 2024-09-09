using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;

namespace SalesforceExtractor.Entidades
{
    /// <summary>
    /// NE__Order__c
    /// </summary>
    public class OrderSalesforce
    {
        [Display(Name = "Id")]
        public string Id { get; set; }

        [Display(Name = "NE__AccountId__c")]
        public string AccountId { get; set; }
        
        [Display(Name = "ExternalId")]
        public string ExternalId { get; set; }

        [Display(Name = "Name")]
        public string Name { get; set; }

        [Display(Name = "NE__RecurringChargeOv__c")]
        public string RecurringChargeOv { get; set; }

        [Display(Name = "Billing_profile__c")]
        public string Billing_profile__c { get; set; }
	    

        [Display(Name = "NE__Country__c")]
        public string NE__Country__c { get; set; }
	    

        [Display(Name = "CurrencyIsoCode")]
        public string CurrencyIsoCode { get; set; }
	    

        [Display(Name = "NE__AccountId__c")]
        public string NE__AccountId__c { get; set; }
	    

        [Display(Name = "NE__BillAccId__c")]
        public string NE__BillAccId__c { get; set; }
	    

        [Display(Name = "NE__CatalogId__c")]
        public string NE__CatalogId__c { get; set; }
	    

        [Display(Name = "NE__Order_date__c")]
        public string NE__Order_date__c { get; set; }
	    

        [Display(Name = "NE__rTypeName__c")]
        public string NE__rTypeName__c { get; set; }
	    

        [Display(Name = "NE__ServAccId__c")]
        public string NE__ServAccId__c { get; set; }
	    

        [Display(Name = "NE__Type__c")]
        public string NE__Type__c { get; set; }


        [Display(Name = "RecordTypeId")]
        public string RecordTypeId { get; set; }

        [Display(Name = "CNT_Case__c")]
        public string CNT_Case__c { get; set; }
        
        public OrderItemSalesforce OrderItem { get; set; }
    }
}

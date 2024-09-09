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
    public class EnelxContractLine
    {
        //[Display(Name = "Id")]
        public string Id { get; set; }

        //[Display(Name = "ExternalId__c")]
        //ExternalID__c
        public string ExternalId { get; set; }

        public string ContractId { get; set; }

        //[Display(Name = "ExternalId__c")]
        //Contract__r:Contract:ExternalID__c
        public string ContractExternalId { get; set; }

        public string AssetId { get; set; }

        //[Display(Name = "ExternalId__c")]
        //Asset__r:Asset:ExternalId__c
        public string AssetExternalId { get; set; }

        public string BillingId { get; set; }

        //[Display(Name = "ExternalId__c")]
        //Billing_Profile__r:Billing_Profile__c:ExternalID__c
        public string BillingExternalId { get; set; }
    }
}

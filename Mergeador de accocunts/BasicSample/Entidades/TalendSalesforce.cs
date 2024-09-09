using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;

namespace SalesforceExtractor.Entidades
{
    public class TalendSalesforce
    {
        [Display(Name = "CNT_Body__c")]
        public string Body { get; set; }

        [Display(Name = "CNT_ExternalId__c")]
        public string ExternalId { get; set; }

        [Display(Name = "CNT_Functionality__c")]
        public string Functionality { get; set; }
    }
}

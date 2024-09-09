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
    public class EnelxContract
    {
        //[Display(Name = "Id")]
        public string Id { get; set; }

        //[Display(Name = "ExternalID__c")]        //     1
        public string ExternalID__c { get; set; }

        //[Display(Name = "Name")]        //      2
        public string Name { get; set; }

        public string AccountId { get; set; }

        //[Display(Name = "Account:Account:ExternalId__c")]        //     3
        public string AccountExternalId { get; set; }

        //[Display(Name = "Description")]        //       5
        public string Description { get; set; }

        //[Display(Name = "Status")]        //        6
        public string Status { get; set; }

        //[Display(Name = "StartDate")]        //     7
        public string StartDate { get; set; }

        //[Display(Name = "Contract_Type__c")]        //      10
        public string Contract_Type__c { get; set; }
    }
}

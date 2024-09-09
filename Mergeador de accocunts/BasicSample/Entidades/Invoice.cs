using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace SalesforceExtractor.Entidades
{
    [DebuggerDisplay("{Id} - {Rut__c}")]
    public class Invoice
    {
        [Display(Name = "Id")]
        public string Id { get; set; }

        [Display(Name = "Status__c")]
        public string Status__c { get; set; }
        
        [Display(Name = "Rut__c")]
        public string Rut__c { get; set; }

        public override string ToString()
        {
            return string.Format("Id: {0} Rut: {1} Status: {2}", this.Id, this.Rut__c, this.Status__c);
        }
    }
}

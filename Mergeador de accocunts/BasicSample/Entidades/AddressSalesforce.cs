using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;

namespace SalesforceExtractor.Entidades
{
    //Address__c
    public class AddressSalesforce
    {
        [Display(Name = "Id")]
        public string Id { get; set; }

        [Display(Name = "Region__c")]
        public string Region { get; set; }

        [Display(Name = "Street_type__c")]
        public string TipoEndereco { get; set; }

        [Display(Name = "StreetMD__r.Street__c")]
        public string Endereco { get; set; }

        [Display(Name = "ExternalId__c")]
        public string ExternalId__c { get; set; }

        [Display(Name = "Municipality__c")]
        public string Municipality__c { get; set; }

        [Display(Name = "Number__c")]
        public string Number__c { get; set; }

        [Display(Name = "Postal_Code__c")]
        public string Postal_Code__c { get; set; }

        [Display(Name = "Corner__c")]
        public string Complemento { get; set; }

        public string NumeroCliente { get; set; }

        public string ContaContrato { get; set; }

        [Display(Name = "Address__r.StreetMD__r.Neighbourhood__c")]
        public string Bairro { get; set; }
    }
}

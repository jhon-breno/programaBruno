using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;

namespace SalesforceExtractor.Entidades
{
    //Account__c
    public class DebitoAutomatico
    {
        [Display(Name = "cnt_bank__c")]
        public string CodigoBanco { get; set; }

        [Display(Name = "Agency__c")]
        public string Agencia { get; set; }

        [Display(Name = "CurrentAccountNumber__c")]
        public string ContaCorrente { get; set; }

        [Display(Name = "BankDocumentOwner__c")]
        public string Documento { get; set; }

        [Display(Name = "IdentityType__c")]
        public string TipoDocumento { get; set; }
    }
}

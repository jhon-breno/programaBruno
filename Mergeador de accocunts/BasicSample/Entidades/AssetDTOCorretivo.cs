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
    public class AssetDTOCorretivo
    {
        //[Display(Name = "IdAsset")]
        public string IdAsset { get; set; }

        //[Display(Name = "Id")]
        public string Id { get; set; }

        //[Display(Name = "ExternalId__c")]
        public string ExternalId { get; set; }

        //[Display(Name = "Account.Id")]
        public string AccountId { get; set; }

        //[Display(Name = "Account.ExternalId__c")]
        public string AccountExternalId { get; set; }

        public bool AccountInvalido { get { return this.AccountExternalId.ToUpper().Contains("INVALIDO"); } }

        //[Display(Name = "Account.IdentityNumber__c")]
        public string Identidade { get; set; }

        //[Display(Name = "Account.IdentityType__c")]
        public string TipoIdentidade { get; set; }

        //[Display(Name = "Name")]
        public string NumeroCliente { get; set; }

        //[Display(Name = "Account.Name")]
        public string NomeCliente { get; set; }
    }
}

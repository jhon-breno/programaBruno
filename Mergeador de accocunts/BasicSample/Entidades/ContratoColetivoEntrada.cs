using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace SalesforceExtractor.Entidades
{
    //Case
    [DebuggerDisplay("Codigo: {Id} ParentId: {AccountIdPai} Account: {AccountExternalId}")]
    public class ContratoColetivoEntrada
    {
        [Display(Name = "8888...")]
        public string CodigoContrato { get; set; }

        [Display(Name = "Account__r.ParentId")]
        public string AccountIdPai { get; set; }

        [Display(Name = "Account__r.CondominiumType")]
        public string CondominiumType { get; set; }

        [Display(Name = "AccountId")]
        public string AccountExternalId { get; set; }

        [Display(Name = "")]
        public string Nome { get; set; }

        [Display(Name = "BillingProfile__r.CNT__Due_Date__c")]
        public string DataVencimento { get; set; }

        [Display(Name = "")]
        public string TipoImpressao { get; set; }

        [Display(Name = "BillingProfile__r.CNT__Lot__c")]
        public string Lote { get; set; }
    }
}

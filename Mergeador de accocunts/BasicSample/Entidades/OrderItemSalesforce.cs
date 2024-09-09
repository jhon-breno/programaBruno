using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;

namespace SalesforceExtractor.Entidades
{
    public class OrderItemSalesforce
    {
        /// <summary>
        /// 
        /// </summary>
        [Display(Name = "Id")]
        public string Id { get; set; }

        /// <summary>
        /// Used for matching purpose
        /// </summary>
        [Display(Name = "ExternalId")]
        public string ExternalId { get; set; }

        /// <summary>
        /// Id of the order previously created
        /// </summary>
        [Display(Name = "NE__OrderId")]
        public string OrderId { get; set; }

        /// <summary>
        /// Fixo "Add"
        /// </summary>
        [Display(Name = "NE__Action__c")]
        public string Action { get { return "Add"; } }

        /// <summary>
        /// Fixo "1"
        /// </summary>
        [Display(Name = "NE__Qty__c")]
        public string Qty { get { return "1"; } }

        /// <summary>
        /// Id of the Catalog Item with Type='Product' related to 'Grupo A'
        /// </summary>
        [Display(Name = "NE__CatalogItem__c")]
        public string CatalogItem { get; set; }

        /// <summary>
        /// Id of the Commercial Product related to 'Grupo A'
        /// </summary>
        [Display(Name = "NE__ProdId__c")]
        public string ProductId { get; set; }

        /// <summary>
        /// To be filled with NE__AccountId__c of the Order
        /// </summary>
        [Display(Name = "NE__ACCOUNT__C")]
        public string AccountId { get; set; }

        /// <summary>
        /// To be filled with NE__AccountId__c of the Order
        /// </summary>
        [Display(Name = "NE__BILLING_ACCOUNT__C")]
        public string BillingAccountId { get; set; }

        /// <summary>
        /// Id of the Catalog in target environment (for UAT: a0y5B000000SXbV)
        /// </summary>
        [Display(Name = "NE__CATALOG__C")]
        public string Catalog { get; set; }

        /// <summary>
        /// To be filled with NE__AccountId__c of the Order
        /// </summary>
        [Display(Name = "NE__SERVICE_ACCOUNT__C")]
        public string ServiceAccountId { get; set; }

        /// <summary>
        /// Fixo: Pending
        /// </summary>
        [Display(Name = "NE__STATUS__C")]
        public string Status { get { return "Pending"; } }

        /// <summary>
        /// Can be filled with the value of 'externalid_asset' column in asset extraction
        /// </summary>
        [Display(Name = "NE__ASSETITEMENTERPRISEID__C")]
        public string AssetItemEnterpriseId { get; set; }

        /// <summary>
        /// Fixo "0.0"
        /// </summary>
        [Display(Name = "NE__BASEONETIMEFEE__C")]
        public string BaseOneTimeFee { get { return "0.0"; } }

        /// <summary>
        /// Fixo "0.0"
        /// </summary>
        [Display(Name = "NE__BASERECURRINGCHARGE__C")]
        public string BaseRecurringCharge { get { return "0.0"; } }

        /// <summary>
        /// Fixo "0.0"
        /// </summary>
        [Display(Name = "NE__ONETIMEFEEOV__C")]
        public string OneTimeFeeOv { get { return "0.0"; } }

        /// <summary>
        /// Fixo "Monthly"
        /// </summary>
        [Display(Name = "NE__RECURRINGCHARGEFREQUENCY__C")]
        public string RecurringChargeFrequency { get { return "Monthly"; } }

        /// <summary>
        /// Fixo "0.0"
        /// </summary>
        [Display(Name = "NE__RECURRINGCHARGEOV__C")]
        public string RecurringChargeOv { get { return "0.0"; } }

        /// <summary>
        /// The same of the account Order
        /// </summary>
        [Display(Name = "NE__Country__c")]
        public string Country { get; set; }

        /// <summary>
        /// The same of the account Order
        /// </summary>
        [Display(Name = "CurrencyIsoCode")]
        public string CurrencyIsoCode { get; set; }

        public List<ItemAttribute> OrderItemAttributes { get; set; }
    }
}

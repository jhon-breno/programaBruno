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
    public class EnelxAsset
    {
        //ExternalId__c	    1
        //[Display(Name = "ExternalId__c")]
        public string ExternalId { get; set; }

        //Name	    2
        public string Name { get; set; }

        //[Display(Name = "Account.Id")]
        //Account.ExternalId__c
        public string AccountId { get; set; }

        //Account:Account:ExternalId__c	    3
        public string AccountExternalId { get; set; }

        //PointofDelivery__r:PointofDelivery__c:ExternalId__c	   5
        //[Display(Name = "PointofDelivery__c.Id")]
        public string PointofDeliveryId { get; set; }


        //[Display(Name = "PointofDelivery__c.ExternalId")]
        public string PointofDeliveryExternalId { get; set; }

        //Description	    6
        public string Description { get; set; }

        //NE__Zip_Code__c	    7
        public string NE__Zip_Code__c { get; set; }

        //NE__Description__c	 8   
        public string NE__Description__c { get; set; }

        //Status	9
        public string Status { get; set; }

        //SerialNumber	11
        public string SerialNumber { get; set; }

        //company__c    12
        public string Company__c { get; set; }


        //[Display(Name = "Id")]
        public string Id { get; set; }
    }
}

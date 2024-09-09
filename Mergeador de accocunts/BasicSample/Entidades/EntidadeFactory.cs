using SalesforceExtractor.apex;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SalesforceExtractor.Entidades
{
    public  class EntidadeFactory
    {
        public sObject GetEntidade(string entidadeSalesforce)
        {
            switch (entidadeSalesforce)
            {
                case "PointofDelivery__c":
                    return new ClienteSalesforce();
                case "Account":
                    return new AccountSalesforce();
                case "Contract_Line_Item__c":
                    return new ContractLineItemSalesforce();
                case "Contract":
                    return new ContractSalesforce();
                default:
                    return null;
            }
        }
    }
}

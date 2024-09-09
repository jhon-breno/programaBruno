using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;

namespace SalesforceExtractor.Entidades
{
    public class DeviceSalesforce
    {
        [Display(Name = "Id")]
        public string Id { get; set; }

        [Display(Name = "MeterBrand__c")]
        public string Marca { get; set; }
        
        [Display(Name = "MeterModel__c")]
        public string Modelo { get; set; }
        
        [Display(Name = "MeterProperty__c")]
        public string Propriedade { get; set; }
        
        [Display(Name = "MeterType__c")]
        public string Tipo { get; set; }
        
        [Display(Name = "ExternalID__c")]
        public string ExternalId { get; set; }
        
        [Display(Name = "Instalation_date__c")]
        public string DataInstalacao { get; set; }
        
        [Display(Name = "MeasureType__c")]
        public string TipoMedicao { get; set; }
        
        //[Display(Name = "Constant__c")]
        //public string Constante { get; set; }
        
        //[Display(Name = "Retirement_date__c")]
        //public string DataRetirada { get; set; }

        [Display(Name = "Status__c")]
        public string Estado { get; set; }

        [Display(Name = "Name")]
        public string Nome { get; set; }

        [Display(Name = "MeterNumber__c")]
        public string Numero { get; set; }
        
        [Display(Name = "PointofDelivery__c")]
        public string PointOfDeliveryId { get; set; }
        
        [Display(Name = "ConstanteDEM__c")]
        public string Constante1 { get; set; }
        
        [Display(Name = "ConstantePRODIA__c")]
        public string Constante2 { get; set; }
        
        [Display(Name = "ConstantePROANT__c")]
        public string Constante3 { get; set; }
        
        [Display(Name = "ConstanteATIVAHP__c")]
        public string Constante4 { get; set; }
        
        [Display(Name = "ConstanteDMCRHP__c")]
        public string Constante5 { get; set; }

        [Display(Name = "Manufacturing_date__c")]
        public string DataFabricacao { get; set; }
        
        [Display(Name = "Cubicle__c")]
        public string Cubiculo { get; set; }
        
        [Display(Name = "CurrencyIsoCode")]
        public string CurrencyIsoCode { get { return "BRL"; } set { this.CurrencyIsoCode = value; } }

        [Display(Name = "RecordTypeId")]
        public string RecordTypeId { get { return "01236000000On9NAAS"; } set { this.RecordTypeId = value; } }

        public string Empresa { get; set; }

        //public string NumeroEquipamento { get; set; }
        //public string Historico { get; set; }

        //"MeterBrand__c"           1   OK
        //"MeterModel__c"           2   OK
        //"MeterNumber__c"   ------
        //"MeterProperty__c"        3   OK
        //"MeterType__c"            4   OK
        //"PointofDelivery__c"   --
        //"ExternalID__c"           5   OK
        //"Instalation_date__c"     6   OK
        //"MeasureType__c"          7   OK
        //"Last Meter Change_c" ????
        //"Constant__c"             8   OK
        //"Cubicle__c"   ----------
        //"Retirement_date__c"      9   OK
        //"Status__c"               10  OK
        //"ConstanteDEM__c"  ------
        //"ConstantePRODIA__c"  ---
        //"ConstantePROANT__c"  ---
        //"ConstanteATIVAHP__c" ---
        //"ConstanteDMCRHP__c"  ---
        //"Manufacturing_date__c";  11  OK
    }
}

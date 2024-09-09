using SalesforceExtractor.apex;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;

namespace SalesforceExtractor.Entidades
{
    //Account__c
    public class AccountSalesforce : sObject
    {
        [Display(Name = "Id")]
        public string Id { get; set; }

        [Display(Name = "ExternalId__c")]
        public string ExternalId{ get; set; }

        [Display(Name = "NE__Customer_Number__c")]
        public string NumeroCliente { get; set; }

        [Display(Name = "CNT_ID_NUM_2__c")]
        public string NumeroDocumento { get; set; }

        [Display(Name = "NE__Name__c")]
        public string Nome { get; set; }

        [Display(Name = "CompanyID__c")]
        public string CodigoEmpresa { get; set; }

        [Display(Name = "CNT_Municipality_Inscription__c")]
        public string InscricaoMunicipal { get; set; }

        [Display(Name = "CondominiumType__c")]
        public string TipoCondominio { get; set; }

        [Display(Name = "ParentId")]
        public string ParentId { get; set; }

        [Display(Name = "9999...")]
        public string CodigoOrgaoControlador { get; set; }

        [Display(Name = "CondominiumRUT__c")]
        public string CondominiumRUT__c { get; set; }

        [Display(Name = "CNT_GroupAssociate__c ")]
        public string CNT_GroupAssociate__c { get; set; }

        /// RecordTypeId
        /// B2B = '01236000000yI8mAAE'  Pessoa Jurídica
        /// B2C = '01236000000yI8nAAE'  Pessoa Física
        /// B2G = '0121o000000oWhZAAU'  Cliente Governo
        [Display(Name = "RecordType")]
        public string TipoRegistro { get; set; }

        public AccountSalesforce Clone()
        {
            AccountSalesforce c = new AccountSalesforce();
            c.Id = this.Id;
            c.ExternalId = this.ExternalId;
            c.NumeroCliente = this.NumeroCliente;
            c.NumeroDocumento = this.NumeroDocumento;
            c.Nome  = this.Nome;
            c.CodigoEmpresa  = this.CodigoEmpresa;
            c.InscricaoMunicipal  = this.InscricaoMunicipal;
            c.TipoCondominio  = this.TipoCondominio;
            c.ParentId  = this.ParentId;
            c.CodigoOrgaoControlador  = this.CodigoOrgaoControlador;
            c.CondominiumRUT__c  = this.CondominiumRUT__c;
            c.CNT_GroupAssociate__c = this.CNT_GroupAssociate__c;
            return c;
        }
    }
}

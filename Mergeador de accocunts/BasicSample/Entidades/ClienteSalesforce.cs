using SalesforceExtractor.apex;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace SalesforceExtractor.Entidades
{
    [DebuggerDisplay("Id:{Id} AccountId:{IdConta} PodId:{IdPod} Doc:{Documento}")]
    public class ClienteSalesforce : sObject, IDisposable
    {
        public string Id { get; set; }
        /// <summary>
        /// ContactoId
        /// </summary>
        public string IdContato { get; set; }
        /// <summary>
        /// Accountid
        /// </summary>
        public string IdConta { get; set; }
        /// <summary>
        /// PointOfDelivery__c.Companyid__c
        /// </summary>
        public string PodCompany { get; set; }
        /// <summary>
        /// AccountNumber
        /// </summary>
        public string NumeroCliente { get; set; }
        /// <summary>
        /// PointOfDeliveryId
        /// </summary>
        public string IdPod { get; set; }
        /// <summary>
        /// Externalid__c
        /// </summary>
        public string ExternalId { get; set; }
        /// <summary>
        /// DetailAddress__c
        /// </summary>
        public string DetailAddress__c { get; set; }
        /// <summary>
        /// Name
        /// </summary>
        public string Nome { get; set; }
        /// <summary>
        /// IdentityNumber__c
        /// </summary>
        public string Documento { get; set; }
        /// <summary>
        /// IdentityType__c
        /// </summary>
        public string TipoDocumento { get; set; }
        public string Empresa { get; set; }
        public List<String> Telefones { get; set; }
        /// <summary>
        /// PrimaryEmail__c
        /// </summary>
        public string Email { get; set; }
        public string EmailSecundario { get; set; }
        /// <summary>
        /// CNT_Responsible_Name__c
        /// </summary>
        public string ResponsavelNome { get; set; }
        /// <summary>
        /// CNT_Resp_ID_Number__c
        /// </summary>
        public string ResponsavelDocumento { get; set; }
        /// <summary>
        /// CNT_Resp_ID_Type__c
        /// </summary>
        public string ResponsavelTipoDocumento { get; set; }
        /// <summary>
        /// C.P.F. ou CNPJ
        /// </summary>
        public string ResponsavelDescTipoDocumento
        {
            get
            {
                return "2".Equals(this.ResponsavelTipoDocumento) ? "CNPJ" : "5".Equals(this.ResponsavelTipoDocumento) ? "C.P.F." : "Inválido";
            }
        }
        /// <summary>
        /// CNT_Responsible_Phone__c
        /// </summary>
        public string ResponsavelTelefone { get; set; }
        /// <summary>
        /// CNT_Responsible_Email__c
        /// </summary>
        public String ResponsavelEmail { get; set; }
        /// <summary>
        /// RecordTypeId
    	/// B2B = '01236000000yI8mAAE'  Pessoa Jurídica
        /// B2C = '01236000000yI8nAAE'  Pessoa Física
        /// B2G = '0121o000000oWhZAAU'  Cliente Governo
        /// </summary>
        public string TipoRegistroId { get; set; }
        ///Governo BT	
        ///Massivo BT	
        ///Grandes 
        ///Clientes AT	
        ///Governo AT
        public string TipoCliente { get; set; }

        //[Display(Name = "Account.CNT_State_Inscription_Exemption__c")]
        public string IsencaoMunicipal { get; set; }

        //[Display(Name = "PointofDelivery__c.Route__c")]
        public string Rota { get; set; }

        //[Display(Name = "DetailAddress__r.Department__c")]
        public string Lote { get; set; }

        //[Display(Name = "CNT_Contract__c")]
        public string CNT_Contract__c { get; set; }

        //Numero da Conta Contrato
        public string ContaContrato { get; set; }

        public void Dispose()
        {
            this.Id = null;
            this.IdContato = null;
            this.IdConta = null;
            this.NumeroCliente = null;
            this.IdPod = null;
            this.ExternalId = null;
            this.DetailAddress__c = null;
            this.Nome = null;
            this.Documento = null;
            this.TipoDocumento = null;
            this.Empresa = null;
            this.Email = null;
            this.EmailSecundario = null;
            this.ResponsavelNome = null;
            this.ResponsavelDocumento = null;
            this.ResponsavelTipoDocumento = null;
            this.ResponsavelTelefone = null;
            this.ResponsavelEmail = null;
            this.TipoRegistroId = null;
            this.TipoCliente = null;
            this.IsencaoMunicipal = null;
            this.Rota = null;
            this.Lote = null;
            this.Telefones = null;
        }
    }
}

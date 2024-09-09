using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;

namespace SalesforceExtractor.Entidades
{
    public class SalesGeralDTO
    {
        //[Display(Name = "AssetExternalId")]
        public string AssetExternalId { get; set; }

        //[Display(Name = "AccountExternalId")]
        public string AccountExternalId { get; set; }

        //[Display(Name = "NumeroCliente")]
        public string NumeroCliente { get; set; }

        //[Display(Name = "Invalido")]
        public int Invalido { get; set; }

        //[Display(Name = "Documento")]
        public string Documento { get; set; }

        //[Display(Name = "Tipo Documento")]
        public string TipoDoc { get; set; }

        public bool Despersonalizado { get { return this.AccountExternalId.StartsWith("2003D"); } }
    }
}

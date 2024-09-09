using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;

namespace Pangea.Entidades
{
    public class SalesGeral
    {
        //[Display(Name = "AssetExternalId")]
        public string ExternalId_Asset { get; set; }

        //[Display(Name = "AccountExternalId")]
        public string ExternalId_Conta { get; set; }

        //[Display(Name = "AccountExternal_Pod")]
        public string ExternalId_Pod { get; set; }

        //[Display(Name = "NumeroCliente")]
        public string Numero_Cliente { get; set; }

        //[Display(Name = "Nome")]
        public string Nome { get; set; }
    }
}

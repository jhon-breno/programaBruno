using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SalesforceExtractor.Utils
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Property)]  
    public class ModifAttribute : Attribute
    {
        public SalesforceExtractor.Entidades.Enumeracoes.TipoCliente TipoCliente { get; set; }
        public string CodEmpresa{ get; set; }
   }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.ComponentModel;
using System.Text;

namespace SalesforceExtractor.Entidades.Enumeracoes
{
    public enum TipoCliente
    {
        Ambos = 0,
        [Description("B")]
        GB = 1,
        [Description("A")]
        GA = 2
    }
}

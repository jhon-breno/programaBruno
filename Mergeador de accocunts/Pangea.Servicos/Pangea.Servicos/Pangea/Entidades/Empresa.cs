using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;

namespace Pangea.Entidades
{
    public enum Empresa
    {
        [Description("-1")]
        NaoInformado = -1,

        [Description("2005")]
        Ampla = 2005,

        [Description("2003")]
        Coelce = 2003,

    }
}

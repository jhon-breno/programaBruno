using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Pangea.Entidades
{
    public class CobrancaValorInformado : CobrancaValorInformadoCliente
    {
        
        public long rowid { get; set; }

        public CobrancaValorInformado()
        {
            
            rowid = 0;
        }



    }
}

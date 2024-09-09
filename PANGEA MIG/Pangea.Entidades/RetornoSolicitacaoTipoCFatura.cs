using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Pangea.Entidades
{
    public class RetornoSolicitacaoTipoCFatura : RetornoSolicitacaoTipoC
    {
        public string estadoDocumento { get; set; }

        public RetornoSolicitacaoTipoCFatura()
        {

            estadoDocumento = "";
        }
    }
}

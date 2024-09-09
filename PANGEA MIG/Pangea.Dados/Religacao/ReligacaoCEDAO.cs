using Pangea.Entidades;
using Pangea.Entidades.Enumeracao;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Pangea.Dados.Religacao
{
    public class ReligacaoCEDAO : ReligacaoDAO
    {
        public ReligacaoCEDAO()
            : base(Empresa.CE)
        {

        }

    }
}

using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

namespace SalesforceExtractor.Dados
{
    public abstract class AltaContratacaoDAO
    {
        public abstract string GetConsultaBase();

        public abstract string GetConsultaB2Win();
    }
}

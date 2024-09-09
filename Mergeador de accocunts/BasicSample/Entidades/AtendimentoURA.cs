using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace SalesforceExtractor.Entidades
{
    [DebuggerDisplay("IdTask:{IdAtividade} Caso:{NumeroCaso} Ativ:{NumeroAtividade} Hora:{DataHora}")]
    public class AtendimentoURA
    {
        public string IdAtividade { get; set; }

        public string NumeroCaso { get; set; }

        public string NumeroAtividade { get; set; }

        public string DataHora { get; set; }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExtracaoSalesForce.Modelo.SalesForce
{
    public class RelatorioFinalSF
    {
        public DateTime DataInicio { get; set; }
        public DateTime DataFim { get; set; }
        public TimeSpan TempoExecucao { get; set; }
        public int qtdeContaDuplicada { get; set; }
        public int qtdePodDuplicada { get; set; }
    }
}

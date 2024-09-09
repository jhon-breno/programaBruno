using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SalesforceExtractor.Entidades
{
    public class ParRelatorioEmergencia
    {
        public string codigoEmpresa { get; set; }
        public string tipoRelatorio { get; set; }
        public string dataInicio { get; set; }
        public string dataFim { get; set; }
        public string caminhoArquivoSaida { get; set; }
    }
}

using Entidades;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pangea.Entidades.DTO
{
    public class AjusteFaturadoDTO : Arquivo
    {
        public string NumeroCliente { get; set; }
        public string AnoMesReferencia { get; set; }
        public string TipoOperacao { get; set; }
        public string ConsumoAtivoHP { get; set; }
        public string ConsumoReativoHP { get; set; }
        public string DmcrReativoHP { get; set; }
        public string DemandaHP { get; set; }
        public string ConsumoAtivoFP { get; set; }
        public string ConsumoReativoFP { get; set; }
        public string DmcrReativoFP { get; set; }
        public string DemandaFP { get; set; }
        public string ConsumoAtivoHR { get; set; }
        public string ConsumoReativoHR { get; set; }
        public string DmcrReativoHR { get; set; }
        public string DemandaHR { get; set; }
        public string ConsumoMedioDia { get; set; }
        public string DataLeituraAnterior { get; set; }
        public string DataLeituraAtual { get; set; }
        public string DataFaturamento { get; set; }
    }
}

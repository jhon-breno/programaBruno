using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entidades.DTO
{
    public class ResultadoCorteReconexaoDTO
    {
        public string codigo_empresa { get; set; }
        public string identificador_synergia { get; set; }
        public string corr_corte { get; set; }
        public string instalacao { get; set; }
        public string estado { get; set; }
        public string tipo_corte { get; set; }
        public string data_corte { get; set; }
        public string data_religacao { get; set; }       

    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pangea.Entidades
{
    [Serializable]
    public class ResultadoCorteEntity
    {
        public string codigo_empresa { get; set; }
        public string sucursal { get; set; }
        public string numero_livro { get; set; }
        public int corr_corte { get; set; }
        public string estado { get; set; }
        public string motivo_sol { get; set; }
        public string tipo_corte { get; set; }
        public int numero_cliente { get; set; }
        public string numero_ordem { get; set; }
        public string corr_reaviso { get; set; }
        public int numero_ordem_corte { get; set; }
        public DateTime? data_solic_corte { get; set; }
        public DateTime? fecha_corte { get; set; }
        public DateTime? hora_exec_corte { get; set; }
        public string fase_corte { get; set; }
        public string motivo_corte { get; set; }
        public string acc_realizada_cor { get; set; }
        public string irreg_instalacao { get; set; }
        public string sit_encon_cor { get; set; }
        public int leitura_corte { get; set; }
        public DateTime? fecha_reposicion { get; set; }
        public string tipo_religacao { get; set; }
        public string acc_realizada_rep { get; set; }
        public string sit_encon_rep { get; set; }
        public int leitura_repo { get; set; }
        public int numero_ordem_repo { get; set; }
        public DateTime? fecha_solic_repo { get; set; }
        public DateTime? data_inicio_cort { get; set; }
        public DateTime? data_inicio_relg { get; set; }

    }

}

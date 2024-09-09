using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pangea.Entidades.DTO
{
    [Serializable]
    public class ResultadoReconexionDTO
    {
        public string codigo_empresa { get; set; }
        public int? numero_cliente { get; set; }
        public string num_ordem_serv_crt { get; set; }
        public short? corr_reaviso { get; set; }
        public short? corr_corte { get; set; }
        public int numero_ordem_corte { get; set; }
        public DateTime? data_solic_corte { get; set; }
        public double valor_divida { get; set; }
        public string numero_livro { get; set; }
        public string tipo_corte { get; set; }
        public DateTime? fecha_corte { get; set; }
        public DateTime? hora_exec_corte { get; set; }
        public string fase_corte { get; set; }
        public string motivo_corte { get; set; }
        public DateTime? fecha_reposicion { get; set; }
        public string tipo_religacao { get; set; }
        public string fase_repo { get; set; }
        public string motivo_repo { get; set; }
        public string acc_realizada_rep { get; set; }
        public string sit_encon_rep { get; set; }
        public int leitura_repo { get; set; }
        public int numero_ordem_repo { get; set; }
        public DateTime? fecha_solic_repo { get; set; }
        public DateTime? data_atual_relig { get; set; }
        public DateTime? data_inicio_relg { get; set; }

    }
}

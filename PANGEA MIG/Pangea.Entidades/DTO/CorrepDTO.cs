using Pangea.Entidades.Base;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace Pangea.Entidades.DTO
{
    [DisplayName("numero_cliente")]
    public class CorrepDTO : EntidadeBase
    {
        public int? numero_ordem_repo { get; set; }
        public string numero_cliente { get; set; }
        public string tipo_corte { get; set; }
        public int? corr_corte { get; set; }
        public string fecha_corte { get; set; }
        public string motivo_corte { get; set; }
        public int? corr_reaviso { get; set; }
        public DateTime? fecha_reposicion { get; set; }
        public string tipo_religacao { get; set; }
        public string funcionario_repo { get; set; }
    }
}

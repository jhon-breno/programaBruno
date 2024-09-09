using Pangea.Entidades.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Pangea.Entidades.DTO
{
    public class CorsoreDTO : EntidadeBase
    {
        public string sucursal { get; set; }
        public int? numero_solicitud { get; set; }
        public string tipo_religacao { get; set; }
        public string estado { get; set; }
        public string numero_cliente { get; set; }
        public string motivo_sol { get; set; }
        public string tipo_corte { get; set; }
        public string funcionario { get; set; }
        public string rol_repo { get; set; }
        public string fecha_solicitud { get; set; }
        public string oficina { get; set; }
        public int? corr_corte { get; set; }
        public string numero_ordem { get; set; }
    }
}

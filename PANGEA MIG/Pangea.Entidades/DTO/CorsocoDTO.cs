using Pangea.Entidades.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Pangea.Entidades.DTO
{
    public class CorsocoDTO : EntidadeBase
    {
        public string   numero_cliente { get; set; }
        public string   estado { get; set; }
        public string   tipo_corte { get; set; }
        public string   corr_corte { get; set; }
        public string   fecha_solicitud { get; set; }
        public string   motivo_sol { get; set; }
        public decimal  numero_livro { get; set; }
    }
}

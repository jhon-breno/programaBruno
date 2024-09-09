using Pangea.Entidades.Base;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace Pangea.Entidades.DTO
{
    [DisplayName("cnr")]
    public class CnrDTO : EntidadeBase
    {
        public string numero_cliente  { get; set; }
        public string tipo_corte { get; set; }
        public string corr_corte { get; set; }
        public string fecha_corte { get; set; }
        public string motivo_corte { get; set; }
    }
}

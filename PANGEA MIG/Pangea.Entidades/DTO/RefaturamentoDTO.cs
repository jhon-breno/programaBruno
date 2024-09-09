using Pangea.Entidades.Base;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace Pangea.Entidades.DTO
{
    [DisplayName("refac")]
    public class RefaturamentoDTO : EntidadeBase
    {
        public string numero_cliente { get; set; }
        public string motivo_refacturac { get; set; }
        public string tipo_nota { get; set; }
        public string indica_refact { get; set; }
        //public DateTime fecha_refacturac { get; set; }
    }
}

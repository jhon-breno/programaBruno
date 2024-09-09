using Pangea.Entidades.Base;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace Pangea.Entidades.DTO
{
    [DisplayName("rol")]
    public class RolDTO : EntidadeBase
    {
        public string rol { get; set; }
        public string nombre { get; set; }
        public string codigo_area { get; set; }
        public string desc_area { get; set; }
        public string sucursal { get; set; }
    }
}

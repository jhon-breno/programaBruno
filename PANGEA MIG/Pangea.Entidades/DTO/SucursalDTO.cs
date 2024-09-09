using Pangea.Entidades.Base;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace Pangea.Entidades.DTO
{
    [DisplayName("sucur")]
    public class SucursalDTO : EntidadeBase
    {
        public string sucursal { get; set; }
        public string tipo_sucursal { get; set; }
        public string regional { get; set; }
    }
}
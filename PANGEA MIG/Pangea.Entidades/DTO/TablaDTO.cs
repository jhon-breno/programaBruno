using Pangea.Entidades.Base;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace Pangea.Entidades.DTO
{
    public class TablaDTO : EntidadeBase
    {
        public string sucursal { get; set; }
        public string nomtabla { get; set; }
        public string codigo { get; set; }
        public string descripcion { get; set; }
        public Int64 valor { get; set; }
        public string valor_alf { get; set; }
        public DateTime fecha_activacion { get; set; }
        public DateTime fecha_modificacion { get; set; }
        public DateTime fecha_desactivac { get; set; }
    }
}

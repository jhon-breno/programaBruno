using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pangea.Entidades.DTO
{
    public class CalendarioLeituraDTO
    {
        public string fecha_proceso { get; set; }
        public string sector { get; set; }
        public string zona { get; set; }
        public string municipio { get; set; }
        public string fecha_lectura_atual { get; set; }
        public string fecha_lectura_anterior { get; set; }
        public string localidade { get; set; }
       // public int referencia { get; set; }
    }
}

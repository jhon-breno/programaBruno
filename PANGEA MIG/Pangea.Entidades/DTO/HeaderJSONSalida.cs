using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entidades.DTO
{
    public class HeaderJSONSalida
    {
        public string CodSistema  { get; set; }
        public string SistemaOrigen  { get; set; }
        public string FechaHora  { get; set; }
        public string Funcionalidad { get; set; }  

    }
}

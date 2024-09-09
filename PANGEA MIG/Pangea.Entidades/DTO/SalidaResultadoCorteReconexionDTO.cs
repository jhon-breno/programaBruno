using Pangea.Entidades.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entidades.DTO
{
    public class SalidaResultadoCorteReconexionDTO
    {
        public SalidaResultadoCorteReconexionDTO()
        {
            Header = new HeaderJSONSalida();
            Body = new List<ResultadoCorteReconexaoDTO>();
        }
        public HeaderJSONSalida Header { get; set; }
        public List<ResultadoCorteReconexaoDTO> Body { get; set; }
    }
}

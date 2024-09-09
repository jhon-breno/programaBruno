using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace Entidades.DTO
{
    public class ResultadoCorteRepoDTO
    {
        public ResultadoCorteRepoDTO()
        {
            ListaCortesReposSolicitados = new List<CorteRepoSolicitadoDTO>();
        }
        public string NumeroSuministro { get; set; }
        public string CodigoEmpresa { get; set; }
        public List<CorteRepoSolicitadoDTO> ListaCortesReposSolicitados { get; set; }
    }
}

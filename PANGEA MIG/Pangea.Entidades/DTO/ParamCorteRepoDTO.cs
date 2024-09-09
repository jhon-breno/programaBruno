using Pangea.Entidades.Enumeracao;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entidades.DTO
{
    public class ParamCorteRepoDTO
    {
        public Empresa CodigoEmpresa { get; set; }
        public Int32 NumeroSuministro { get; set; }
        public DateTime FechaInicio { get; set; }
        public DateTime FechaFin { get; set; }
    }
}

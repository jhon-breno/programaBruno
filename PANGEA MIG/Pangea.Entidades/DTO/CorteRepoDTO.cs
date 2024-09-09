using Pangea.Entidades.Enumeracao;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entidades.DTO
{
    public class CorteRepoDTO
    {        
        public Int32 suministro { get; set; }
        public DateTime fechaInicio    { get; set; }
        public DateTime fechaFin { get; set; }
    }
}

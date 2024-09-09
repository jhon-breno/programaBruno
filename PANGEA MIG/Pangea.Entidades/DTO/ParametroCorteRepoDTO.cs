using Pangea.Entidades.Enumeracao;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entidades.DTO
{
    public class ParametroCorteRepoDTO
    {
        public Empresa empresa { get; set; }
        public List<CorteRepoDTO> cortesRepos { get; set; }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pangea.Entidades.DTO
{
    public class ParRetornoOrdem
    {
        public string numero_ordem { get; set; }
        public int corr_visita { get; set; }
        public string cod_retorno { get; set; }
        public string etapa { get; set; }
    }
}

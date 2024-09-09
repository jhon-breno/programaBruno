using Pangea.Entidades.Enumeracao;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pangea.Entidades.DTO
{
    [Serializable]
    public class DadosCorsocoDTO 
    {

        public string numero_livro { get; set; }
        public string sucursal { get; set; }
        public int corr_convenio { get; set; }
        public string ind_natureza { get; set; }
        public string empresa { get; set; }
        public string tipo_corte { get; set; }
        public string funcionario { get; set; }
        public int corr_reaviso { get; set; }
    }

}

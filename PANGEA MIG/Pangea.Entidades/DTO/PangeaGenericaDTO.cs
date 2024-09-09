using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pangea.Entidades.DTO
{
    public class PangeaGenerica
    {
        public int id_pangea_generica { get; set; }
        public string cod_pangea_integracao { get; set; }
        public string parametros { get; set; }
        public string status { get; set; }
        public int id_pangea_tipo_servico { get; set; }
        public string descricao { get; set; }
        public string dtinsert { get; set; }
        public string dtupdate { get; set; }
        public int tentativas { get; set; }       
                                  
    }
}

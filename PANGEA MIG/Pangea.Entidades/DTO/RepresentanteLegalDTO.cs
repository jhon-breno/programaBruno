using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entidades.DTO
{
    public class RepresentanteLegalDTO
    {
        public string numero_cliente { get; set; }
        public string nome { get; set; }
        public string numero_identidade { get; set; }
        public string compl_identidade { get; set; }
        public string numero_cpf { get; set; }
        public string dv_cpf { get; set; }
        public string data_emissao_ident { get; set; }
    }
}

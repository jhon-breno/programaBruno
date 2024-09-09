using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entidades.DTO
{
    public class ClienteCadAtendDTO
    {
        public int numero_cliente { get; set; } 
        public int ind_aut_email { get; set; }
        public string rol { get; set; }
        public string nome_conjuge { get; set; }
        public string tipo_doc_conjuge { get; set; }
        public string documento_conjuge { get; set; }
        public string dv_documento_conjuge { get; set; }
        public int ind_cad_conjuge { get; set; }        
        
    }
}

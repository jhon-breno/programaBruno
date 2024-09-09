using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pangea.Entidades
{
    public class ClientesTelefone
    {
        public string numero_cliente { get; set; }
        public string tipo_telefone { get; set; }
        public string prefixo_ddd { get; set; }
        public string numero_telefone { get; set; }
        public string tipo_doc { get; set; }
        public string rut { get; set; }
        public string dv_rut { get; set; }

        public List<string> lstTelefones { get; set; }

    }
}

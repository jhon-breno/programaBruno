using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entidades.DTO
{
    public class ClienteSmcDTO
    {
        public int numero_cliente { get; set; }
        public string codigo_cp { get; set; }
        public string codigo_cs { get; set; }
        public string codigo_ps { get; set; }
        public string shunt1 { get; set; }
        public string shunt2 { get; set; }
        public string shunt3 { get; set; }
        public string tipo_ligacao { get; set; }
        public string ultima_lectura { get; set; }
        public string fecha_activacion { get; set; }
        public string fecha_modificacion { get; set; }
        public string fecha_desactivac { get; set; }
        public string tipo_med { get; set; }
        public string fact_smc { get; set; }
        public string fator { get; set; }
        public string modulo_controle { get; set; }
    }
}

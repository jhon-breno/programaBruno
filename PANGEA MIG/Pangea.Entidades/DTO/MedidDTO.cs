using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entidades.DTO
{
    public class MedidDTO
    {
        public int numero_medidor { get; set; }
        public string marca_medidor { get; set; }
        public string modelo_medidor { get; set; }
        public float constante { get; set; }
        public float constante_react_hp { get; set; }
        public float ultima_lect_activa { get; set; }
        public float ultima_lect_react { get; set; }
        public float ultima_lect_act_hp { get; set; }
        public string propriedade_medidor { get; set; }
    }
}

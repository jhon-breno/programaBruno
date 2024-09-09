using Pangea.Entidades.Enumeracao;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pangea.Entidades.DTO
{
    [Serializable]
    public class CorparDTO 
    {
        public double deuda { get; set; }
        public int antiguedad_deuda { get; set; }
        public string tarifa { get; set; }
        public string classe { get; set; }
        public string tipo_cliente { get; set; }
        public string subclasse { get; set; }
        public string atividad_economica { get; set; }
        public double deuda_extra { get; set; }
        public int antiguedad_extra { get; set; }
        public string fecha_sistema { get; set; }
        public double divida_max { get; set; }
        public int antigued_deuda_max { get; set; }
        public string ind_corte_vip { get; set; }
        public string ind_corte_br { get; set; }
        public string ind_local_med { get; set; }
        public string ind_padrao_agrup { get; set; }
        public string cod_tipo_caixa { get; set; }
        
  
    }

}

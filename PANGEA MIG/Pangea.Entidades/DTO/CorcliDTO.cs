using Pangea.Entidades.Enumeracao;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pangea.Entidades.DTO
{
    [Serializable]
    public class CorcliDTO 
    {

        public string numero_cliente { get; set; }
        public string sucursal { get; set; }
        public string fecha_a_corte { get; set; }
        public string dia_hoje { get; set; }
        public int corr_corte { get; set; }
        public string estado_cliente { get; set; }
        public string tipo_cliente { get; set; }
        public string subclasse { get; set; }
        public string classe { get; set; }
        public string giro { get; set; }
        public string tarifa { get; set; }
        public string estado_suministro { get; set; }
        public string ind_cliente_vip { get; set; }
        public int corr_reaviso_cli { get; set; }
        public int corr_convenio { get; set; }
        public string tiene_notific { get; set; }
        public string cod_seg { get; set; }
        public string tiene_prepago { get; set; }
        public string ind_local_med { get; set; }
        public string ind_padrao_agrup { get; set; }
        public string tipo_caixa_med { get; set; }
        public string fecha_parafuso { get; set; }
        public string acometida_retirada { get; set; }
        public string fecha_notifica { get; set; }
        
    }

}

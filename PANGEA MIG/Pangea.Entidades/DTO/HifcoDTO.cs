using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entidades.DTO
{
    public class HifcoDTO
    {
        public int numero_cliente { get; set; }
        public string corr_facturacion { get; set; }
        public string fecha_lectura { get; set; }
        public string fecha_facturacion { get; set; }
        public string fecha_vencimiento { get; set; }
        public string clave_lectura { get; set; }
        public string antiguedad_saldo { get; set; }
        public string tipo_docto_asocia { get; set; }
        public string nro_docto_asocia { get; set; }
        public string suma_importe { get; set; }
        public string suma_intereses { get; set; }
        public string suma_convenio { get; set; }
        public string suma_impuestos { get; set; }
        public string suma_cargos_man { get; set; }
        public string suma_corte_repos { get; set; }
        public string saldo_afecto_ant { get; set; }
        public string saldo_noafec_ant { get; set; }
        public string intereses_ant { get; set; }
        public string mal_factor_pot { get; set; }
        public string porc_rec_malfac { get; set; }
        public string multas_ant { get; set; }
        public string total_facturado { get; set; }
        public string tiene_cobro_int { get; set; }
        public string indica_refact { get; set; }
        public string fact_calma { get; set; }
        public string saldo_moroso { get; set; }
        public string saldo_mora { get; set; }
        public string suma_moras { get; set; }    
        
        
    }
}

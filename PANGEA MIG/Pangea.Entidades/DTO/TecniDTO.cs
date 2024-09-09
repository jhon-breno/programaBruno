using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entidades.DTO
{
    public class TecniDTO
    {
        public int numero_cliente { get; set; }
        public string ult_fec_mant_empal { get; set; }
        public string codigo_voltaje { get; set; }
        public string nro_subestacion { get; set; }
        public string tipo_subestacion { get; set; }
        public int numero_equipo { get; set; }
        public string tipo_contrato { get; set; }
        public string tipo_tranformador { get; set; }
        public string propiedad_trafo { get; set; }
        public string tension_suministro { get; set; }
        public string caja_empalme { get; set; }
        public string conductor_empalme { get; set; }
        public string acometida_retirada { get; set; }
        public string ult_tipo_contrato { get; set; }
        public int nro_ult_contrato { get; set; }
        public string fecha_ult_contrato { get; set; }
        public string fecha_ult_aplicac { get; set; }
        public int numero_contrato { get; set; }
        public int nro_orden_conexion { get; set; }
        public string fecha_conexion { get; set; }
        public string nro_sol_servicio { get; set; }
        public string dv_sol_servicio { get; set; }
        public string aux_sol_servicio { get; set; }
        public string mod_tipo_orden { get; set; }
        public float mod_nro_orden { get; set; }
        public string subestac_transmi { get; set; }
        public string alimentador { get; set; }
        public string subestac_distrib { get; set; }
        public string llave { get; set; }
        public int numero_poste { get; set; }
        public int dist_poste_med { get; set; }
        public string medida_tecnica { get; set; }
        public string trilha { get; set; }
        public string fase_conexion { get; set; }
        public string cod_transformador { get; set; }
        public string fecha_parafuso { get; set; }
        public string coordenada_lat_gps { get; set; }
        public string coordenada_lon_gps { get; set; }
        public string fase { get; set; }
        public string pto_transf { get; set; }
        public string ind_local_med { get; set; }
        public string ind_padrao_agrup { get; set; }
        public string nro_caixa { get; set; }
        public string nro_caixa_tampa { get; set; }
        public int tipo_caixa_med { get; set; }
        public string coord_lat_gps_lida { get; set; }
        public string coord_lon_gps_lida { get; set; }
        public string data_atu_coord_sda { get; set; }
        public string coord_lat_trafo { get; set; }
        public string coord_lon_trafo { get; set; }
        public float utmx { get; set; }
        public float utmy { get; set; }
        public string ind_cred_medid { get; set; }
        public string dispositivo_seguranca { get; set; }


    }
}

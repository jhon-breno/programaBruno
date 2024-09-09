using Pangea.Entidades.Enumeracao;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Pangea.Entidades.Base;

namespace Pangea.Entidades.DTO
{
    [Serializable]
    public class TipoCorteDTO  
    {
        public string descricao_tipo { get; set; }
        public int qtd_corte_min { get; set; }
        public int qtd_corte_max { get; set; }
        public int qtd_autorel_min { get; set; }
        public int qtd_autorel_max { get; set; }
        public string clientes_to { get; set; }
        public string clientes_caducados { get; set; }
        public string clientes_atraso { get; set; }
        public string clientes_parafuso { get; set; }
        public string clientes_recorte { get; set; }
        public string clientes_autorel { get; set; }
        public string clientes_asf { get; set; }
        public int meses_corte { get; set; }
        public int meses_autorel { get; set; }
        public int meses_parcel { get; set; }
        public int qtd_parcel_min { get; set; }
        public int qtd_parcel_max { get; set; }
        public string clientes_smc { get; set; }
        public string ind_automatico { get; set; }
        public string clientes_debaut { get; set; }
        public string cod_seg { get; set; }
        public string ind_caixa { get; set; }
        public string ind_natur_corte { get; set; }
        public string parcel_recorte { get; set; }
        public string ind_tipo_corte { get; set; }
        public string clientes_prepago { get; set; }
        public string cod_operacao { get; set; }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="numeroCliente"></param>

    }

}

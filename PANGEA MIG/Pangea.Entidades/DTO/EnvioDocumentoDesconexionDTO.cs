using Pangea.Entidades.Enumeracao;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace Pangea.Entidades.DTO
{
    [Serializable]
    public class EnvioDocumentoDesconexionDTO 
    {
        public List<string> clientes { get; set; }
        public string numero_livro { get; set; }
        public string empresa { get; set; }
        //tipo_corte S ou N
        public string parcelamento_caducados { get; set; } //tipo corte clientes_caducados
        public string instalacao_agrupamento { get; set; } //tipo_corte ind_corte_rest_agrup
        public string parcelamento_vigente { get; set; } //tipo corte parcel_recorte
        public string debito_automatico { get; set; } // opcao = 3
    }

}

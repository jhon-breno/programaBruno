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
    public class RetornoDocumentoDesconexionDTO 
    {
        public string numero_livro { get; set; }
        public string numero_cliente { get; set; }
        public int cod_empresa { get; set; }
        public bool apto { get; set; }
        public bool erro { get; set; }
        public string msg_erro { get; set; }
        public string documento_supensao { get; set; }
        public string linha_digitavel { get; set; }
        public string faturas_venc_pend { get; set; }
        public string referencia_fatura { get; set; }
        public DateTime vencimento { get; set; }
        public double valor { get; set; }
        public string mes_ano { get; set; }
  
    }

}

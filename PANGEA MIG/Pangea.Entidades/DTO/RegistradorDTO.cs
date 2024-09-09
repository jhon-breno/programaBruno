using Pangea.Entidades.Base;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace Pangea.Entidades.DTO
{
    [DisplayName("registrador")]
    public class RegistradorDTO : EntidadeBase
    {
        public string numero_cliente { get; set; }
        public string tabelaValor { get; set; }
        public string tabelaInteiro { get; set; }
        public string tabelaDecimal { get; set; }
        public string campoValor { get; set; }
        public string campoInteiro { get; set; }
        public string campoDecimal { get; set; }
        public int posicaoSAP { get; set; }
        public string valor { get; set; }
        public string valorInteiro { get; set; }
        public string valorDecimal { get; set; }
        public bool constante { get; set; }
    }
}

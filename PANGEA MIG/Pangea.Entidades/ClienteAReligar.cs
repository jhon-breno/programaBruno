using Pangea.Entidades.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Pangea.Entidades
{
    public class ClienteAReligar : EntidadeBase
    {
        public string numero_cliente { get; set; }
        public DateTime? data_emissao { get; set; }
        public DateTime? data_processamento { get; set; }
        public DateTime? data_emissao_min { get; set; }
        public DateTime? data_emissao_max { get; set; }
        public string data_emissao_operador { get; set; }
    }
}

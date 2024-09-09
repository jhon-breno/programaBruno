using Pangea.Entidades.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Pangea.Entidades.DTO
{
    public class ReligacaoPendenteDTO : EntidadeBase
    {
        public char estado { get; set; }
        public string numero_cliente { get; set; }
        public string data_baixa_corte { get; set; }
        public string corr_corte { get; set; }
    }
}

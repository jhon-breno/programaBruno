using Pangea.Entidades.Base;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace Pangea.Entidades.DTO
{
    [DisplayName("ocorrencia_corte")]
    public class OcorrenciaCorteDTO : EntidadeBase
    {
        public string numero_cliente  { get; set; }
        public DateTime data_processamento_s { get; set; }
        public string corr_corte { get; set; }
        public string tipo_ocorrencia { get; set; }
        public string observacao { get; set; }
    }
}

using Pangea.Entidades.Base;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace Pangea.Entidades.DTO
{
    [DisplayName("etapa")]
    public class EtapaDTO : EntidadeBase
    {
        public string tipo_ordem { get; set; }
        public string tipo_servico { get; set; }
        public string cod_etapa { get; set; }
        public string cod_formulario { get; set; }
        public string ind_deafea { get; set; }
        public int? sequencia_etapa { get; set; }
        public string classe { get; set; }
        public string subclasse { get; set; }
    }
}
using Pangea.Entidades.Base;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;

namespace Pangea.Entidades.DTO
{
    [DisplayName("solicitante_ordem")]
    public class SolicitanteOrdemDTO : EntidadeBase
    {
        public string numero_ordem { get; set; }
        public string nome { get; set; }
        public string numero_documento { get; set; }
        public string municipio { get; set; }
        public string endereco { get; set; }
        public string telefone_contato { get; set; }
    }
}

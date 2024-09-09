using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SalesforceExtractor.Entidades
{
    public class Atividade
    {
        public string Id { get; set; }
        public string CasoId { get; set; }
        public string CasoRelacionado { get; set; }
        public string Assunto { get; set; }
        public string DataCriacao { get; set; }
    }
}

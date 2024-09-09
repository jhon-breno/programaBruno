using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SalesforceExtractor.Entidades
{
    public class ParAtividadeSalesforce
    {
        private Guid identificadorParametro;

        public Guid IdentificadorParametro { get { return this.identificadorParametro; } private set { this.identificadorParametro = value; } }
        public bool IsAlive { get;set;}
        public string Id { get; set; }
        public string CasoId { get; set; }
        public string CasoRelacionado { get; set; }
        public string Assunto { get; set; }
        public string DataCriacao { get; set; }
        public DateTime? DataCriacaoInicio { get; set; }
        public DateTime? DataCriacaoFim { get; set; }
        public string DataCriacaoInicioStr { get { return DataCriacaoInicio == null ? string.Empty : DataCriacaoInicio.Value.ToString("yyyy-MM-ddT00:00:00.000Z"); } }
        public string DataCriacaoFimStr { get { return DataCriacaoFim == null ? string.Empty : DataCriacaoFim.Value.ToString("yyyy-MM-ddT23:59:59.000Z"); } }
        public string Pais { get; set; }
        //public int NumeroTentativa { get; set; }

        public ParAtividadeSalesforce()
        {
            this.IdentificadorParametro = new Guid();
            this.IsAlive = true;
        }
    }
}

using Pangea.Entidades.Enumeracao;
using Pangea.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pangea.Entidades.DTO
{
    /// <summary>
    /// CT_TAB_MOT_INTEGRACAO
    /// </summary>
    public class ServicosIntegracaoDTO
    {
        public long id_servico { get; set; }
        public string empresa { get; set; }
        public string canal_atendimento { get; set; }
        public string tipo_cliente { get; set; }
        public string cod_motivo { get; set; }
        public string cod_submotivo { get; set; }
        public string tipo_ordem { get; set; }
        public string tipo_servico { get; set; }
        public DateTime data_ativacao { get; set; }
        public DateTime? data_desativacao  { get; set; }

        public ServicosIntegracaoDTO()
        {
        }

        public ServicosIntegracaoDTO(Empresa empresa)
        {
            this.empresa = empresa.ToString();
        }

        public ServicosIntegracaoDTO(Empresa empresa, string canalAtendimento, string tipoCliente, string motivo, string submotivo)
        {
            this.empresa = empresa.ToString();
            if ("B".Equals(tipoCliente))
                this.tipo_cliente = "BT";
            else if("A".Equals(tipoCliente))
                this.tipo_cliente = "AT";
            this.cod_motivo = motivo;
            this.cod_submotivo = submotivo;
            this.canal_atendimento = canalAtendimento;
        }

        public bool isAtivo()
        {
            return (!this.data_desativacao.HasValue || 
                    (this.data_desativacao > DateTime.MinValue && this.data_desativacao < DateTime.MaxValue)
                   );
        }
    }
}

using Pangea.Dados.Base;
using Pangea.Entidades.DTO;
using Pangea.Entidades.Enumeracao;
using Pangea.Util;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pangea.Dados
{
    public class ServicosIntegracaoDAO: BaseDAO
    {
        public ServicosIntegracaoDAO(Empresa empresa)
            : base(empresa)
        {
        }

        /// <summary>
        /// Recupera uma lista de serviços do Synergia correspondentes aos serviços do SalesForce, vinculados via tipo_ordem e tipo_servico.
        /// </summary>
        /// <param name="ServicoIntegracaoDTO"></param>
        /// <returns></returns>
        public List<ServicosIntegracaoDTO> ObterServicosSynergia(ServicosIntegracaoDTO motivoIntegracao)
        {
            StringBuilder sql = new StringBuilder(@"select  id_servico, empresa, canal_atendimento, tipo_cliente, cod_motivo,
                                                            cod_submotivo, tipo_ordem, tipo_servico, data_ativacao, data_desativacao
                                         from servicos_integracao
                                         where 1=1 ");

            if (motivoIntegracao != null && !string.IsNullOrEmpty(motivoIntegracao.tipo_cliente))
                sql.AppendFormat(" AND TIPO_CLIENTE = '{0}' ", motivoIntegracao.tipo_cliente);

            if (motivoIntegracao != null && !string.IsNullOrEmpty(motivoIntegracao.cod_motivo))
                sql.AppendFormat(" AND COD_MOTIVO = '{0}' ", motivoIntegracao.cod_motivo);

            if (motivoIntegracao != null && !string.IsNullOrEmpty(motivoIntegracao.cod_submotivo))
                sql.AppendFormat(" AND COD_SUBMOTIVO = '{0}' ", motivoIntegracao.cod_submotivo);

            if (motivoIntegracao != null && !string.IsNullOrEmpty(motivoIntegracao.tipo_ordem))
                sql.AppendFormat(" AND TIPO_ORDEM = '{0}' ", motivoIntegracao.tipo_ordem);

            if (motivoIntegracao != null && !string.IsNullOrEmpty(motivoIntegracao.tipo_servico))
                sql.AppendFormat(" AND TIPO_SERVICO = '{0}' ", motivoIntegracao.tipo_servico);

            if (motivoIntegracao != null && !string.IsNullOrEmpty(motivoIntegracao.canal_atendimento))
                sql.AppendFormat(" AND CANAL_ATENDIMENTO = '{0}' ", motivoIntegracao.canal_atendimento);

            return DataHelper.ConvertDataTableToList<ServicosIntegracaoDTO>(ConsultaSql(sql.ToString()));
        }
    }
} 
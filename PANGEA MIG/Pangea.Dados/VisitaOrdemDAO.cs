using Entidades.DTO;
using Pangea.Dados.Base;
using Pangea.Entidades.Enumeracao;
using Pangea.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pangea.Dados
{
    public class VisitaOrdemDAO  : BaseDAO
    {
        public VisitaOrdemDAO(Empresa empresa)
            : base(empresa)
        {
        }

        public VisitaOrdemDTO RetornainforOrdemServico(string numero_caso)
        {
            StringBuilder sql = new StringBuilder();

            sql.Append(@" SELECT 
                               o.numero_ordem,
	                           v.data_exec_visita as data_visita, 
	                           v.hora_exec_visita, 
	                           v.rol_ret_visita as rol_ret_visita, 
                               v.numero_ordem_filha, 
                               o.estado,
                               e.descricao_etapa
                        FROM visita_ordem v, ordem_servico o, etapa_servico e                     
                        WHERE v.numero_ordem = o.numero_ordem
                        AND o.tipo_ordem = e.tipo_ordem
                        AND o.tipo_servico = e.tipo_servico
                        AND v.etapa = o.etapa
                        AND v.corr_visita = o.corr_visita ");
            sql.Append(" and o.nro_caso = '" + numero_caso + "'");

            var dt = ConsultaSql(sql.ToString());
            if (dt.Rows.Count > 0)
            {
                return DataHelper.ConvertDataTableToEntity<VisitaOrdemDTO>(dt);
            }
            else
                return null;

        }

        public bool Ingressar(VisitaOrdemDTO visita, DBProviderInformix conn)
        {

            string sql = string.Format(@"insert into visita_ordem (
                                            numero_ordem, 
                                            etapa, 
                                            corr_visita,
                                            data_visita,
                                            hora_ini_prevista,
                                            hora_fim_prevista, 
                                            empreiteira,
                                            rol_responsavel, 
                                            rol_visita,
                                            area_destino, 
                                            ind_agendado,
                                            data_ini_etapa, 
                                            temp_def_tecnico)
                                      values (NumeroOrdem, CodEtapa, CorrVisita + 1, datavisita,
                                        HoraIniVisita, HoraFimVisita, CodEmpreiteira,
                                        RolResponsavel, RolVisita, AreaExecutante, IndAgendamento,
                                        DataEtapa, TempoDefTec);");
            return ExecutarSql(sql.ToString(), conn);
        }
    }
}

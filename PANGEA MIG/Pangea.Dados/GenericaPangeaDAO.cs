using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Pangea.Dados.Base;
using Pangea.Entidades;
using Pangea.Util;
using Pangea.Entidades.DTO;
using Pangea.Entidades.Enumeracao;

namespace Pangea.Dados
{
    public class GenericaPangeaDAO : BaseDAO
    {
        public GenericaPangeaDAO(Empresa empresa)
            : base(empresa)
        {

        }
        /// <summary>
        /// FOT
        /// </summary>
        /// <param name="codigoExec">String com codigos de execução Ex: "1,2"</param>
        /// <returns></returns>
        public DataTable RetornoComando(String codigoExec)
        {


            String sql = String.Format(@"select id_pangea_generica,
                                  cod_pangea_integracao,       
                                  parametros,     
                                  status  
                        from pangea_generica
                        where cod_pangea_integracao = '{0}'
                        and status in('I','U')", codigoExec);

            var dt = ConsultaSql(sql);

            return dt;
        }
        public DataTable RetornoComandoUpdate(String codigoExec)
        {
            DBProviderInformix informix = ObterProviderInformix();
            DataTable dt = new DataTable();

            informix.BeginTransacion();


            //String sql2 = "set lock mode to wait;";
            //ExecutarSql(sql2.ToString(), informix);

            String sql = String.Format(@" select id_pangea_generica,
                                  cod_pangea_integracao,       
                                  parametros,     
                                  status,
                                  id_pangea_tipo_servico   
                        from pangea_generica
                        where cod_pangea_integracao = '{0}'
                        and status in('I','U')", codigoExec);

             dt = ConsultaSql(sql,informix);

             if (dt.Rows.Count > 0)
             {
                 string id_pangea = string.Empty;
                 for (int i = 0; i < dt.Rows.Count; i++)
                 {
                     id_pangea += "" + dt.Rows[i]["id_pangea_generica"].ToString() + ",";
                 }
                 id_pangea = id_pangea.Remove(id_pangea.Length - 1);    

                 String sql1 = String.Format(@"update pangea_generica set status='E'
                                         where id_pangea_tipo_servico = '" + dt.Rows[0]["id_pangea_tipo_servico"] + @"'                                         
                                         and cod_pangea_integracao ='{0}'  
                                         and id_pangea_generica in ({1}) ", codigoExec, id_pangea);
                 ExecutarSql(sql1.ToString(), informix);

                 informix.Commit();
             }
             else
                 informix.Rollback();

            return dt;
        }

        public List<PangeaGenerica> RetornoComandoUpdateList(String codigoExec)
        {
            DBProviderInformix informix = ObterProviderInformix();
            DataTable dt = new DataTable();

            informix.BeginTransacion();


            //String sql2 = "set lock mode to wait;";
            //ExecutarSql(sql2.ToString(), informix);

            String sql = String.Format(@" select id_pangea_generica,
                                  cod_pangea_integracao,       
                                  parametros,     
                                  status,
                                  id_pangea_tipo_servico,
                                  descricao   
                        from pangea_generica
                        where cod_pangea_integracao = '{0}'
                        and status in('I','U')", codigoExec);

            dt = ConsultaSql(sql, informix);

            if (dt.Rows.Count > 0)
            {
                string id_pangea = string.Empty;
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    id_pangea += "" + dt.Rows[i]["id_pangea_generica"].ToString() + ",";
                }
                id_pangea = id_pangea.Remove(id_pangea.Length - 1);

                String sql1 = String.Format(@"update pangea_generica set status='E'
                                         where id_pangea_tipo_servico = '" + dt.Rows[0]["id_pangea_tipo_servico"] + @"'                                         
                                         and cod_pangea_integracao ='{0}'  
                                         and id_pangea_generica in (" + id_pangea + @") ", codigoExec);
                ExecutarSql(sql1.ToString(), informix);

                informix.Commit();

                List<PangeaGenerica> resultado = DataHelper.ConvertDataTableToList<PangeaGenerica>(dt);
                dt.Dispose();
                return resultado;
            }
            else
            {
                informix.Rollback();
                return null;
            }
        }

        public List<PangeaGenerica> RetornoComandoUpdateList(String codigoExec, string descricao)
        {
            DBProviderInformix informix = ObterProviderInformix();
            DataTable dt = new DataTable();
            StringBuilder sql = new StringBuilder();

            informix.BeginTransacion();

            sql.Append(@" select id_pangea_generica,
                                  cod_pangea_integracao,       
                                  parametros,     
                                  status,
                                  id_pangea_tipo_servico,
                                  descricao     
                        from pangea_generica");
            sql.Append(String.Format(" where cod_pangea_integracao = '{0}'", codigoExec));
            sql.Append(" and status in('I','U')");

            if (!String.IsNullOrEmpty(descricao))
                sql.Append(String.Format(" and descricao = '{0}'", descricao));


            dt = ConsultaSql(sql.ToString(), informix);

            if (dt.Rows.Count > 0)
            {
                string id_pangea = string.Empty;
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    id_pangea += "" + dt.Rows[i]["id_pangea_generica"].ToString() + ",";
                }
                id_pangea = id_pangea.Remove(id_pangea.Length - 1);

                String sql1 = String.Format(@"update pangea_generica set status='E'
                                         where id_pangea_tipo_servico = '" + dt.Rows[0]["id_pangea_tipo_servico"] + @"'                                         
                                         and cod_pangea_integracao ='{0}'  
                                         and id_pangea_generica in (" + id_pangea + @") ", codigoExec);
                ExecutarSql(sql1.ToString(), informix);

                informix.Commit();

                List<PangeaGenerica> resultado = DataHelper.ConvertDataTableToList<PangeaGenerica>(dt);
                dt.Dispose();
                return resultado;
            }
            else
            {
                informix.Rollback();
                return null;
            }
        }

        public List<ExecucaoDTO> ConsultarConfiguracao()
        {
            try
            {
                String sql = @"select cod_pangea_integracao,
                           classe_executante,
                           periodicidade,
                           unidade,
                           status_service,
                           data_ultima_execucao,
                           descricao
                        from pangea_config
                        where status_service='True'";

                var dt = ConsultaSql(sql);

                List<ExecucaoDTO> resultado = gerarExecucao(dt);
                return resultado;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public List<ExecucaoDTO> gerarExecucao(DataTable resultDt)
        {
            ExecucaoDTO DTO = new ExecucaoDTO();
            List<ExecucaoDTO> listaDTO = new List<ExecucaoDTO>();


            if (resultDt.Rows.Count > 0)
            {
                foreach (DataRow dr in resultDt.Rows)
                {
                    DTO = new ExecucaoDTO();
                    DTO.cod_pangea_integracao = TratarString(resultDt, dr, "cod_pangea_integracao");
                    DTO.classeExecutante = TratarString(resultDt, dr, "classe_executante");
                    DTO.periodo = gerarPeriodo(TratarString(resultDt, dr, "periodicidade"), TratarInt(resultDt, dr, "unidade", 0));
                    DTO.ativo = TratarBool(resultDt, dr, "status_service");
                    DTO.ultimaExecucao = TratarDateTime(resultDt, dr, "data_ultima_execucao");//Convert.ToDateTime(dr[5]);

                    listaDTO.Add(DTO);
                }
                return listaDTO;

            }

            return new List<ExecucaoDTO>();

        }

        public bool AtualizarUltimaExecucao(string id, DBProviderInformix informix)
        {
            bool result = false;

                String sql = String.Format(@"update pangea_config set data_ultima_execucao = current where cod_pangea_integracao = '{0}'", id);

            result = ExecutarSql(sql.ToString(), informix);

            return result;
        }

        public bool AtualizaStatus(int id, DBProviderInformix informix, string status)
        {
            bool result = false;

            String sql = String.Format(@"update pangea_generica set status='" + status + @"'
                                        , dtupdate = current 
                                         where id_pangea_generica = {0}", id);

            result = ExecutarSql(sql.ToString(), informix);

            return result;

        }
        public bool AtualizaStatus(int id, string status)
        {
            bool result = false;
            DBProviderInformix informix = ObterProviderInformix();
            informix.BeginTransacion();
            String sql = String.Format(@"update pangea_generica set status='" + status + @"'
                                        , dtupdate = current 
                                         where id_pangea_generica = {0}", id);

            result = ExecutarSql(sql.ToString(), informix);
            informix.Commit();
            return result;

        }
        public bool AtualizaStatus(string comando,int id, DBProviderInformix informix, string status)
        {
            bool result = false;

            String sql = String.Format(@"update pangea_generica set status='" + status + @"'
                                         where cod_pangea_integracao = '{0}'
                                         and id_pangea_generica= {1}", comando, id);

            result = ExecutarSql(sql.ToString(), informix);

            return result;

        }

        
        private TimeSpan gerarPeriodo(string periodicidade, int unidade)
        {
            TimeSpan ts = new TimeSpan();
            DateTime tempo = new DateTime();
            switch (periodicidade)
            {
                case "Segundos":
                    tempo = tempo.AddSeconds(unidade);
                    break;
                case "Minutos":
                    tempo = tempo.AddMinutes(unidade);
                    break;
                case "Horas":
                    tempo = tempo.AddHours(unidade);
                    break;
                case "Dias":
                    tempo = tempo.AddDays(unidade);
                    break;
                case "Meses":
                    tempo = tempo.AddMonths(unidade);
                    break;
            }

            return tempo.TimeOfDay;
        }

        public void insereLog(string codigo, string sucesso, string dadosOrigem, string descricao, string origem, int quantidade, DBProviderInformix informix)
        {
            String sql = String.Format(@"insert into pangea_log_servico (cod_pangea_integracao,data_ingresso,sucesso,dado_origem,descricao,origem,quantidade) values
                                                                        ('{0}',current,'{1}','{2}','{3}','{4}',{5})", codigo, sucesso, dadosOrigem, descricao, origem, quantidade);
            ExecutarSql(sql.ToString(), informix);

        }

        public bool inserePangeaGenerica(PangeaGenerica obj)
        {
            string sql = string.Format(@"insert into pangea_generica (cod_pangea_integracao, parametros, status, dtinsert, dtupdate, descricao, tentativas, id_pangea_tipo_servico) 
                                                                     values('{0}','{1}','{2}',current,'{3}','{4}',{5},{6})"
                                                                            ,obj.cod_pangea_integracao
                                                                            ,obj.parametros
                                                                            ,obj.status                                                                            
                                                                            ,obj.dtupdate
                                                                            ,obj.descricao
                                                                            ,obj.tentativas
                                                                            ,obj.id_pangea_tipo_servico);
            DBProviderInformix informix = ObterProviderInformix();
            return ExecutarSql(sql.ToString(), informix);
        }
    }
}

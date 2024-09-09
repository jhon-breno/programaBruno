using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using Pangea.Entidades;
using Pangea.Entidades.Enumeracao;
using IBM.Data.Informix;
using Pangea.Entidades.DTO;
using Pangea.Dados.Base;

namespace Pangea.Dados
{
    public class AgendamentoDAO : BaseDAO
    {
        public AgendamentoDAO(Empresa empresa)
            : base(empresa)
        {
        }

        public bool ValidarDiaAgendamentoPorMunicipio(AgendamentoDTO agendamento)
        {
            DataTable dtAgendServMunic;

            string sql = " SELECT  DIA_SEMANA " +
                         " FROM    AGEND_SERV_MUNIC " +
                         " WHERE   TIPO_ORDEM = '" + agendamento.TipoOrdem + "'" +
                         " AND     TIPO_SERVICO = '" + agendamento.TipoServico + "'" +
                         " AND     MUNICIPIO = '" + agendamento.Municipio + "'";
            try
            {
                dtAgendServMunic = ConsultaSql(sql.ToString());
                if (dtAgendServMunic.Rows.Count == 0)
                {
                    return true;
                }
                else
                {
                    sql = sql + " AND DIA_SEMANA = '" + ( (int)agendamento.Dia.DayOfWeek + 1)  + "'";
                    sql = sql + " AND ATIVO = 'S'";
                    dtAgendServMunic = ConsultaSql(sql.ToString());
                }

                if (dtAgendServMunic.Rows.Count > 0)
                {
                    return true;
                }

            }
            catch (Exception e)
            {
                throw new Exception("Erro ao ler dados para agendamento da ordem selecionada.");
            }

            return false;
        }

        public bool GravarAgendamento(AgendamentoDTO obj)
        {
            bool result = false;
            DBProviderInformix informix = ObterProviderInformix();

            try
            {
                string sql = " INSERT INTO agendamento_dia "
                    + "( "
                    + "  tipo_ordem, "
                    + "  tipo_servico, "
                    + "  municipio, "
                    + "  etapa, "
                    + "  dia, "
                    + "  periodo,"
                    + "  qtd "
                    + ") "
                    + " VALUES "
                    + "( "
                            + "'" + obj.TipoOrdem + "',"
                            + "'" + obj.TipoServico + "',"
                            + "'" + obj.Municipio + "',"
                            + "'" + obj.Etapa + "',"
                            + "'" + Convert.ToDateTime(obj.Dia).ToString("MM/dd/yyyy") + "',"
                            + "'" + obj.Periodo + "',"
                            + "'" + obj.Qtd + "'"
                    + ")";

                ExecutarSql(sql, informix);
            }
            catch (Exception e)
            {
                throw new Exception("Erro ao gravar dados. " + e.Message);
            }

            return result;
        }
        
        public bool ConsultaSeDiaFeriado(DateTime dataAgenda, string municipio)
        {
            DataTable dtFeriado;
            bool result = false;

            string sql = " SELECT  FECHA " +
                         " FROM    FERIADOS " +
                         " WHERE   FECHA = '" + string.Format("{0:M/d/yyyy}", dataAgenda) + "'" +
                         " AND     MUNICIPIO = '" + municipio + "'";

            try
            {
                dtFeriado = ConsultaSql(sql.ToString());
                result = (dtFeriado.Rows.Count > 0 ? true : false);
            }
            catch (Exception e)
            {
                throw new Exception("Erro ao fazer consulta no banco de dados.");
            }

            return result;
        }


        #region Obter os Dias Disponiveis para Agendamento.

        public List<String> ObterDiasDisponivel(string tipoOrdem, string tipoServico, string etapaServico, string municipio)
        {
            var primeiroDia = DateTime.Now;
            var qtdDiasAgendamento = string.Empty;
            var permite = false;

            PermiteAgendamento(tipoOrdem, tipoServico, out qtdDiasAgendamento, out permite);

            if (permite)
            {
                // Ultimo Dia: Criamos uma variavel DateTime com o ano atual, o mês atual e o dia é a quantidade de dias que o mês corrente possui.
                //A função DateTime.DaysInMonth recebe como parametro o ano(int) e o mês(int) e retorna a quantidade de dias(int). 
                //var ultimoDia = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.DaysInMonth(DateTime.Now.Year, DateTime.Now.Month));
                var ultimoDia = DateTime.Now.AddDays(Convert.ToInt32(qtdDiasAgendamento));

                DataTable dtCalendario = Calendario(ultimoDia);

                var dtFeriado = VerificarFeriado(municipio, primeiroDia, ultimoDia);

                if (dtFeriado.Rows.Count > 0)
                {
                    foreach (DataRow dtRowCa in dtCalendario.Rows)
                    {
                        foreach (DataRow dtRowFe in dtFeriado.Rows)
                        {
                            if (
                                dtRowCa["DiaMesAno"].ToString()
                                    .Equals(Convert.ToDateTime(dtRowFe["fecha"].ToString()).ToString("MM/dd/yyyy")))
                            {
                                dtRowCa["Excluir"] = "S";
                            }
                        }
                    }

                    DataRow[] resultCAFeriado = dtCalendario.Select("Excluir = 'N'");
                    dtCalendario = resultCAFeriado.CopyToDataTable();
                }


                dtCalendario = VerificarDiasCompletosParaAgendamento(tipoOrdem, tipoServico, etapaServico, municipio,
                    primeiroDia, ultimoDia, dtCalendario);

                dtCalendario = RemoverFinaisDeSemana(dtCalendario);


                var dtSemana = DiasDisponiveisParaAgendamento(tipoOrdem, tipoServico, municipio);

                if (dtSemana.Rows.Count > 0)
                {
                    //Dias completos da semana
                    string num = "2;3;4;5;6;7;";

                    //Deixar somente os dias que não iram ser trabalhados 
                    for (int i = 0; i <= dtSemana.Rows.Count - 1; i++)
                    {
                        int posicao = num.IndexOf(dtSemana.Rows[i][0].ToString().Trim());

                        num = num.Remove(posicao, 2);
                    }

                    for (int i = 0; i < dtCalendario.Rows.Count; i++)
                    {
                        if (dtCalendario.Rows[i]["Descricao"].ToString().Contains("SEGUNDA") ||
                            dtCalendario.Rows[i]["Descricao"].ToString().Contains("MONDAY"))
                        {
                            if (num.IndexOf('2') > -1)
                                dtCalendario.Rows[i]["Excluir"] = "S";
                        }

                        if (dtCalendario.Rows[i]["Descricao"].ToString().Contains("TERCA") ||
                            dtCalendario.Rows[i]["Descricao"].ToString().Contains("TUESDAY"))
                        {
                            if (num.IndexOf('3') > -1)
                                dtCalendario.Rows[i]["Excluir"] = "S";
                        }

                        if (dtCalendario.Rows[i]["Descricao"].ToString().Contains("QUARTA") ||
                            dtCalendario.Rows[i]["Descricao"].ToString().Contains("WEDNESDAY"))
                        {
                            if (num.IndexOf('4') > -1)
                                dtCalendario.Rows[i]["Excluir"] = "S";
                        }

                        if (dtCalendario.Rows[i]["Descricao"].ToString().Contains("QUINTA") ||
                            dtCalendario.Rows[i]["Descricao"].ToString().Contains("THURSDAY"))
                        {
                            if (num.IndexOf('5') > -1)
                                dtCalendario.Rows[i]["Excluir"] = "S";
                        }

                        if (dtCalendario.Rows[i]["Descricao"].ToString().Contains("SEXTA") ||
                            dtCalendario.Rows[i]["Descricao"].ToString().Contains("FRIDAY"))
                        {
                            if (num.IndexOf('6') > -1)
                                dtCalendario.Rows[i]["Excluir"] = "S";
                        }
                        if (dtCalendario.Rows[i]["Descricao"].ToString().Contains("SABADO") ||
                            dtCalendario.Rows[i]["Descricao"].ToString().Contains("SATURDAY"))
                        {
                            if (num.IndexOf('7') > -1)
                                dtCalendario.Rows[i]["Excluir"] = "S";
                        }

                    }

                    DataRow[] resultDiasPermitidos = dtCalendario.Select("Excluir = 'N'");
                    var dtCalendarioDiasPermitido = resultDiasPermitidos.CopyToDataTable();


                    var diasDisponiveis = new List<string>();
                    foreach (DataRow dtRows in dtCalendarioDiasPermitido.Rows)
                    {
                        diasDisponiveis.Add(dtRows["DiaMesAno"].ToString());
                    }

                    return diasDisponiveis;
                }
            }

            return null;
        }

        protected DataTable Calendario(DateTime ultimoDia)
        {
            //Para descobrir o primeiro e ultimo dia do mês corrente
            // Primeiro Dia: Criamos uma variavel DateTime com o ano atual, o mês atual e o dia igual a 1 
            //var primeiroDia = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
            var primeiroDia = DateTime.Now;

            // Ultimo Dia: Criamos uma variavel DateTime com o ano atual, o mês atual e o dia é a quantidade de dias que o mês corrente possui.
            //A função DateTime.DaysInMonth recebe como parametro o ano(int) e o mês(int) e retorna a quantidade de dias(int). 
            //var ultimoDia = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.DaysInMonth(DateTime.Now.Year, DateTime.Now.Month));

            int iPrimeiroDia = primeiroDia.Day;
            int iUltimoDia = ultimoDia.Day;

            var dtDiaMesAno = new DataTable();
            dtDiaMesAno.Columns.Add("Dia");
            dtDiaMesAno.Columns.Add("DiaMesAno");
            dtDiaMesAno.Columns.Add("Descricao");
            dtDiaMesAno.Columns.Add("Excluir");

            for (int i = iPrimeiroDia; i <= iUltimoDia; i++)
            {
                DataRow dr = dtDiaMesAno.NewRow();

                if (i != 1)
                    primeiroDia = primeiroDia.AddDays(1);

                dr["Dia"] = primeiroDia.ToString("dd");
                dr["DiaMesAno"] = primeiroDia.ToString("MM/dd/yyyy");
                dr["Descricao"] = RemoverAcentuacao(primeiroDia.ToString("dddd").ToUpper());
                dr["Excluir"] = "N";
                dtDiaMesAno.Rows.Add(dr);
            }

            return dtDiaMesAno;
        }

        protected DataTable RemoverFinaisDeSemana(DataTable dtCalendario)
        {
            foreach (DataRow dtRow in dtCalendario.Rows)
            {
                //if (dtRow["Descricao"].ToString() == "SABADO" || dtRow["Descricao"].ToString() == "SATURDAY")
                //    dtRow["Excluir"] = "S";
                if (dtRow["Descricao"].ToString() == "DOMINGO" || dtRow["Descricao"].ToString() == "SUNDAY")
                    dtRow["Excluir"] = "S";
            }

            DataRow[] result = dtCalendario.Select("Excluir = 'N'");
            DataTable dtDados = result.CopyToDataTable();

            return dtDados;

        }

        protected string RemoverAcentuacao(string stringAcentuada)
        {
            if (string.IsNullOrEmpty(stringAcentuada.Trim()))
                return string.Empty;
            else
            {
                byte[] array = Encoding.GetEncoding("iso-8859-8").GetBytes(stringAcentuada);
                return Encoding.UTF8.GetString(array);
            }
        }

        protected DataTable VerificarFeriado(string municipio, DateTime periodoInicial, DateTime periodoFinal)
        {
            //Verifica Feriados em um determinado periodo do mês.
            var sqlFeriados = new StringBuilder();
            sqlFeriados.AppendFormat(@" SELECT day(fecha) dia ,
                                                    date(fecha) fecha
                                                    FROM feriados
                                                WHERE municipio in ({0} ) 
                                                AND   fecha >= '{1}'
                                                AND   fecha <= '{2}' ", municipio, periodoInicial.ToString("MM/dd/yyyy"), periodoFinal.ToString("MM/dd/yyyy"));

            var dtFeriados = ConsultaSql(sqlFeriados.ToString());

            return dtFeriados;
        }

        protected DataTable DiasDisponiveisParaAgendamento(string tipoOrdem, string tipoServico, string municipio)
        {
            //Realiza-se a leitura dos dados para agendamento da ordem selecionada.

            var sql = new StringBuilder();
            sql.Length = 0;
            sql.AppendFormat(@" SELECT                                                  
                                    DIA_SEMANA                                         
                                FROM AGEND_SERV_MUNIC SM                                   
                                WHERE   TIPO_ORDEM = '{0}'
                                AND TIPO_SERVICO = '{1}'
                                AND MUNICIPIO = '{2}'
                                AND ATIVO = 'S' ", tipoOrdem.ToUpper(), tipoServico.ToUpper(), municipio);

            var dtDiasDisponiveis = ConsultaSql(sql.ToString());

            return dtDiasDisponiveis;
        }

        protected DataTable VerificarDiasCompletosParaAgendamento(string tipoOrdem, string tipoServico, string etapaOrdem, string municipio, DateTime PeriodoInicial, DateTime PeriodoFinal, DataTable dtCalendario)
        {
            var dtDiasCompletos = new DataTable();

            //Verifica os dias que estao completos os agendamentos.
            var sqlAgendamentoDia = new StringBuilder();
            sqlAgendamentoDia.AppendFormat(@" select 
                                            dia,
                                            sum(qtd) as quantidade
                                        from agendamento_dia a
                                        where tipo_ordem = '{0}'
                                        and   tipo_servico = '{1}'
                                        and   etapa = '{2}'
                                        and   municipio = {3}
                                        and   dia >= '{4}'
                                        and   dia <= '{5}'
                                        group by 1
                                        having sum(qtd) = (
                                        select  
                                        sum(capacidade * qtd_capacidade)
                                        From agendamento_etapa
                                        where tipo_ordem = '{0}'
                                        and   tipo_servico = '{1}'
                                        and   etapa = '{2}'
                                        and   municipio = {3}
                                        and nvl(data_desativacao,'') = '' )
                                        order by 1 ", tipoOrdem, tipoServico, etapaOrdem, municipio, PeriodoInicial.ToString("MM/dd/yyyy"), PeriodoFinal.ToString("MM/dd/yyyy"));

            dtDiasCompletos = ConsultaSql(sqlAgendamentoDia.ToString());

            if (dtDiasCompletos.Rows.Count > 0)
            {
                foreach (DataRow dtRows in dtDiasCompletos.Rows)
                {
                    foreach (DataRow dtRowsCA in dtCalendario.Rows)
                    {
                        string diasComp = Convert.ToDateTime(dtRows["Dia"].ToString()).ToString("MM/dd/yyyy");
                        string diasCalen = Convert.ToDateTime(dtRowsCA["DiaMesAno"].ToString()).ToString("MM/dd/yyyy");

                        if (diasComp.Contains(diasCalen))
                        {
                            dtRowsCA["Excluir"] = "S";
                        }
                    }
                }

                DataRow[] resultDiasPermitidos = dtCalendario.Select("Excluir = 'N'");
                dtDiasCompletos = resultDiasPermitidos.CopyToDataTable();
            }
            else
            {
                return dtCalendario;
            }

            return dtDiasCompletos;
        }

        #endregion

        public Boolean PermiteAgendamento(string tipoOrdem, string tipoServico)
        {
            var permite = false;
            var sql = new StringBuilder();
            sql.AppendFormat(@"SELECT 
                                ind_agendamento, qtd_per_agend 
                                FROM servicos WHERE tipo_ordem = '{0}' AND cod_servico = '{1}' "
                , tipoOrdem, tipoServico);

            var dtServicos = ConsultaSql(sql.ToString());

            if (dtServicos.Rows.Count > 0)
            {
                var indAgendamento = dtServicos.Rows[0]["ind_agendamento"].ToString();

                if (!string.IsNullOrEmpty(indAgendamento.Trim()))
                {
                    if (!indAgendamento.Equals("N"))
                    {
                        permite = true;
                    }
                }
            }

            return permite;

        }

        public void PermiteAgendamento(string tipoOrdem, string tipoServico, out string qtdDiasAgendamento, out bool permite)
        {
            qtdDiasAgendamento = null;
            permite = false;
            var sql = new StringBuilder();
            sql.AppendFormat(@"SELECT 
                                ind_agendamento, qtd_per_agend 
                                FROM servicos WHERE tipo_ordem = '{0}' AND cod_servico = '{1}' "
                , tipoOrdem, tipoServico);

            var dtServicos = ConsultaSql(sql.ToString());

            if (dtServicos.Rows.Count > 0)
            {
                var indAgendamento = dtServicos.Rows[0]["ind_agendamento"].ToString().Trim();
                qtdDiasAgendamento = dtServicos.Rows[0]["qtd_per_agend"].ToString().Trim();

                if (!string.IsNullOrEmpty(indAgendamento.Trim()))
                {
                    if (!indAgendamento.Equals("N"))
                    {
                        permite = true;
                    }
                }
            }
        }

        #region Gravar o Agendamento da Visita.

        //public string GravarAgendamento(string tipoOrdem, string tipoServico, string etapaOrdem, string municipio,
        //    int periodo, DateTime dataAgendamento)
        //{
        //    var result = string.Empty;
        //    var sqlProcedure = new StringBuilder();

        //    if (PermiteAgendamento(tipoOrdem, tipoServico))
        //    {
        //        sqlProcedure.AppendFormat("EXECUTE PROCEDURE agendamentoordem('{0}', '{1}', '{2}', '{3}',date('{4}'), {5}, 'I') ",
        //            tipoOrdem, tipoServico, etapaOrdem, municipio, dataAgendamento.ToString("MMddyyyy"), periodo);

        //        DataTable dt = ConsultaSql(sqlProcedure.ToString());

        //        if (dt.Rows.Count > 0)
        //        {
        //            result = dt.Rows[0][0].ToString();
        //        }
        //    }

        //    return result;
        //}

        #endregion

        #region Cancelar Agendamento - AMPLA

        public string CancelarAgendamento(string tipoOrdem, string tipoServico, string etapaOrdem, string municipio,
            int periodo, DateTime dataAgendamento)
        {
            var result = string.Empty;
            var sqlProcedure = new StringBuilder();

            if (PermiteAgendamento(tipoOrdem, tipoServico))
            {
                sqlProcedure.AppendFormat("EXECUTE PROCEDURE agendamentoordem('{0}', '{1}', '{2}', '{3}',date('{4}'), {5}, 'D') ",
                    tipoOrdem, tipoServico, etapaOrdem, municipio, dataAgendamento.ToString("MMddyyyy"), periodo);

                DataTable dt = ConsultaSql(sqlProcedure.ToString());

                if (dt.Rows.Count > 0)
                {
                    result = dt.Rows[0][0].ToString();
                }
            }

            return result;
        }

        #endregion

        public string ExecutarProcedureAgendamentoOrdem(AgendamentoDTO agendamento, string tipoOperacao, DBProviderInformix informix)
        {
            var resultado = string.Empty;

            var sql = String.Format(@"EXECUTE PROCEDURE agendamentoordem(
                                        '{0}', '{1}', '{2}', '{3}',to_date('{4}','%d-%m-%Y'),'{5}', '{6}')",
                                        agendamento.TipoOrdem,
                                        agendamento.TipoServico,
                                        agendamento.Etapa,
                                        agendamento.Municipio,
                                        agendamento.Dia.ToString("dd-MM-yyyy"),
                                        agendamento.Periodo,
                                        tipoOperacao);

            DataTable dt = ConsultaSql(sql.ToString());

            if (dt.Rows.Count > 0)
            {
                resultado = dt.Rows[0][0].ToString();
            }
        
            return resultado;
        }

        public bool InserirOrdemAgendamento(AgendamentoDTO agendamento, DBProviderInformix informix)
        { 
            OrdemServicoDTO ordem = agendamento.ordemServico;

            string empreiteira = RecuperarContratista(ordem, informix);

            string sql = String.Format(@"insert into 
                                            ordem_agendamento(
                                                numero_ordem, 
                                                corr_visita, 
                                                municipio, 
                                                data, 
                                                hora_inicio, hora_fim, 
                                                estado, 
                                                empreiteira, 
                                                rol_estado, 
                                                data_estado, 
                                                periodo) 
                                            values(
                                                '{0}', 
                                                1,
                                                '{1}',
                                                to_date('{2}','%d-%m-%Y'), '{3}','{4}',
                                                'I',
                                                '{5}',
                                                '{6}',
                                                current, 
                                                '{7}')",
                                                ordem.numero_ordem,
                                                ordem.municipio,
                                                ordem.data_ingresso.ToString("dd-MM-yyyy"),
                                                agendamento.HoraInicio,
                                                agendamento.HoraFim,
                                                empreiteira, 
                                                ordem.rol_ingresso,
                                                agendamento.Periodo
                                                );

            return ExecutarSql(sql.ToString(), informix);

        }

        public string RecuperarContratista(OrdemServicoDTO ordemServico, DBProviderInformix informix)
        {

            string resultado = String.Empty;
            string sql = String.Format(@"SELECT 
                                            contratista
                                         FROM 
                                            cto_disp_empre
                                         WHERE 
                                            tipo_ordem = '{0}'
                                            AND tipo_servico = '{1}'
                                            AND municipio = '{2}'
                                            AND ativo = '0'",
                                            ordemServico.tipo_ordem,
                                            ordemServico.tipo_servico,
                                            ordemServico.municipio);

            DataTable dt = ConsultaSql(sql.ToString(), informix);

            if (dt.Rows.Count > 0)
            {
                resultado = dt.Rows[0][0].ToString();
            }
            return resultado;

        }
    }
}
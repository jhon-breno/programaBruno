using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Pangea.Dados.Base;
using Pangea.Entidades;
using Pangea.Dados.Religacao;
using Pangea.Entidades.DTO;
using Pangea.Util;
using Pangea.Entidades.Enumeracao;
using Entidades.DTO;

namespace Pangea.Dados
{
    public class ReligacaoDAO : BaseDAO
    {
        private string _empresa;

        public ReligacaoDAO(Empresa empresa)
            : base(empresa)
        {
            if (empresa != Empresa.NaoIdentificada)
                this._empresa = ((int)empresa).ToString();
        }

        private static ReligacaoRJDAO religacaoAmplaDAO = null;
        private static ReligacaoCEDAO religacaoCoelceDAO = null;


        #region Factory

        private ReligacaoDAO getInstanceReligacaoDAO(string empresa)
        {
            ReligacaoDAO religacaoDAO = null;
            switch (int.Parse(empresa))
            {
                case (int)Empresa.RJ:
                    if (religacaoAmplaDAO == null)
                    {
                        religacaoAmplaDAO = new ReligacaoRJDAO();
                    }

                    religacaoDAO = religacaoAmplaDAO;
                    break;
                case (int)Empresa.CE:
                    if (religacaoCoelceDAO == null)
                    {
                        religacaoCoelceDAO = new ReligacaoCEDAO();
                    }

                    religacaoDAO = religacaoCoelceDAO;
                    break;
                default:
                    religacaoDAO = null;
                    break;
            }

            return religacaoDAO;
        }

        #endregion


        public List<CorsoreDTO> ObterReligacoes(CorsoreDTO obj)
        {
            if (obj == null)
                return new List<CorsoreDTO>();

            if (string.IsNullOrEmpty(this._empresa))
            {
                //TODO: gerar log antes de lançar erro
                throw new ArgumentException("Parâmetro empresa obrigatório para a consulta de religações.");
            }

            #region Prepara a consulta básica

            StringBuilder sql = new StringBuilder("SELECT * FROM corsore WHERE 1=1 ");

            if (!string.IsNullOrEmpty(obj.estado))
                sql.AppendFormat("AND estado = '{0}' ", obj.estado);

            if (!string.IsNullOrEmpty(obj.fecha_solicitud))
                sql.AppendFormat("AND fecha_solicitud = '{0}' ", obj.fecha_solicitud);

            if (!string.IsNullOrEmpty(obj.funcionario))
                sql.AppendFormat("AND funcionario = '{0}' ", obj.funcionario);

            if (!string.IsNullOrEmpty(obj.motivo_sol))
                sql.AppendFormat("AND motivo_sol = '{0}' ", obj.motivo_sol);

            if (!string.IsNullOrEmpty(obj.numero_cliente))
                sql.AppendFormat("AND numero_cliente = {0} ", obj.numero_cliente);

            if (!string.IsNullOrEmpty(obj.numero_ordem))
                sql.AppendFormat("AND numero_ordem = {0} ", obj.numero_ordem);

            if (obj.numero_solicitud.HasValue)
                sql.AppendFormat("AND numero_solicitud = {0} ", obj.numero_solicitud);

            if (!string.IsNullOrEmpty(obj.oficina))
                sql.AppendFormat("AND oficina = '{0}' ", obj.oficina);

            if (!string.IsNullOrEmpty(obj.rol_repo))
                sql.AppendFormat("AND rol_repo = {0} ", obj.rol_repo);

            if (!string.IsNullOrEmpty(obj.sucursal))
                sql.AppendFormat("AND sucursal = '{0}' ", obj.sucursal);

            if (!string.IsNullOrEmpty(obj.tipo_corte))
                sql.AppendFormat("AND tipo_corte = '{0}' ", obj.tipo_corte);

            if (!string.IsNullOrEmpty(obj.tipo_religacao))
                sql.AppendFormat("AND tipo_religacao = '{0}' ", obj.tipo_religacao);

            #endregion

            ReligacaoDAO relig = this.getInstanceReligacaoDAO(this._empresa);

            IList<CorsoreDTO> lstCorsore = new List<CorsoreDTO>();
            lstCorsore = DataHelper.ConvertDataTableToList<CorsoreDTO>(ConsultaSql(sql.ToString()));

            return (List<CorsoreDTO>)lstCorsore;
        }

        public List<ReligacaoPendenteDTO> ObterReligacoesPendentes(ReligacaoPendenteDTO obj)
        {
            if (obj == null)
                return new List<ReligacaoPendenteDTO>();

            if (string.IsNullOrEmpty(this._empresa))
            {
                //TODO: gerar log antes de lançar erro
                throw new ArgumentException("Parâmetro empresa obrigatório para a consulta de religações.");
            }

            #region Prepara a consulta básica

            StringBuilder sql = new StringBuilder("SELECT * FROM religacao_pendente WHERE 1=1 ");

            if (!string.IsNullOrEmpty(obj.estado.ToString()))
                sql.AppendFormat("AND estado = '{0}' ", obj.estado);

            if (!string.IsNullOrEmpty(obj.data_baixa_corte))
                sql.AppendFormat("AND data_baixa_corte = '{0}' ", obj.data_baixa_corte);

            if (!string.IsNullOrEmpty(obj.numero_cliente))
                sql.AppendFormat("AND numero_cliente = {0} ", obj.numero_cliente);

            if (!string.IsNullOrEmpty(obj.corr_corte))
                sql.AppendFormat("AND corr_corte = {0} ", obj.corr_corte);

            #endregion

            ReligacaoDAO relig = this.getInstanceReligacaoDAO(this._empresa);

            IList<ReligacaoPendenteDTO> lstCorsore = new List<ReligacaoPendenteDTO>();
            lstCorsore = DataHelper.ConvertDataTableToList<ReligacaoPendenteDTO>(ConsultaSql(sql.ToString()));

            return (List<ReligacaoPendenteDTO>)lstCorsore;
        }

        public decimal ObterValorReligacao(string  pNumeroCliente)
        {
            string sql = string.Format(@"SELECT c.numero_cliente, t.valor, t.codigo
                                           FROM tabla t, cliente c
                                          WHERE t.sucursal = '0000'    
                                            AND t.nomtabla = 'TIPLIG'  
                                            AND c.tipo_ligacao = t.codigo
                                            AND c.numero_cliente = {0}", pNumeroCliente);
            DataTable dt = ConsultaSql(sql.ToString());
            if (dt == null)
                return 0;

            if (DBNull.Value.Equals(dt.Rows[0]["valor"]))
                return 0;

            return Convert.ToDecimal(dt.Rows[0]["valor"]);
        }
        public ResultadoCorteReconexaoDTO RetornaReconexion(int numeroCliente)
        {
            String sql = String.Format(@"SELECT
                                            (SELECT sociedad_sie2000 FROM insta) AS codigo_empresa,                                           
                                            num_ordem_serv_rep AS identificador_synergia,
                                            corr_corte,
                                            numero_medidor || marca_medidor AS instalacao,
                                            TRIM(acc_realizada_rep) AS estado,
                                            TRIM(tipo_corte) AS tipo_corte,
                                            fecha_corte AS data_corte,                                            
                                            fecha_reposicion AS data_religacao                                            
                                        FROM correp c, medid m
                                        WHERE c.numero_cliente = m.numero_cliente                                        
                                        AND fecha_reposicion IS NOT NULL
                                        AND c.numero_cliente = {0}", numeroCliente.ToString());

            DataTable dtResultado = ConsultaSql(sql);

            //ResultadoReconexionDTO reconexion = GerarEntidadeReconexion(dtResultado);
            //return reconexion;

            if (dtResultado.Rows.Count > 0)
            {
                ResultadoCorteReconexaoDTO resultado = DataHelper.ConvertDataTableToEntity<ResultadoCorteReconexaoDTO>(dtResultado);
                //object resultado = eb.gerarEntidadeGenerica(dt, tipo);

                return resultado;
            }
            else
                return null;
        }

        /// <summary>
        /// //TODO:
        /// </summary>
        /// <param name="numeroCliente"></param>
        /// <param name="dataProcessamento"></param>
        /// <returns></returns>
        public DataTable ObterClienteAptoAReligacaoAutomatica(string numeroCliente, DateTime dataProcessamento)
        {
            if (string.IsNullOrEmpty(numeroCliente) || dataProcessamento == DateTime.MinValue || dataProcessamento.Date > DateTime.Today)
                return new DataTable();

            if (string.IsNullOrEmpty(this._empresa))
            {
                //TODO: gerar log antes de lançar erro
                throw new ArgumentException("Parâmetro empresa obrigatório para a consulta de religações.");
            }

            StringBuilder sql = new StringBuilder("");
            sql.Append(" SELECT C.numero_cliente, round(sum(A.valor_pago),2), C.cod_proyecto, ");   // 1...3
            sql.Append(" C.sucursal, S.regional, C.telefono, ");                                    // 4...6
            sql.Append(" C.tipo_ligacao, C.estado_suministro, C.ind_cli_despersona, ");             // 7...9
            sql.Append(" C.tipo_ident, nvl(C.rut,'0'), C.cont_meses_sem_faturar ");                 //10..12
            sql.Append(" FROM cliente C, arrec_parcial A, sucur S ");
            sql.Append(" WHERE A.estado = 'I' ");
            sql.AppendFormat(" AND A.data_processamento = '{0}' ", dataProcessamento.ToString("yyyy-MM-dd HH:mm"));
            sql.Append(" AND C.numero_cliente = A.numero_cliente ");
            sql.Append(" AND C.estado_cliente in ('0','3') ");
            sql.Append(" AND C.estado_suministro  = '1' ");
            sql.Append(" AND S.sucursal = C.sucursal ");
            sql.Append(" GROUP BY 1,3,4,5,6,7,8,9,10,11,12");

            return ConsultaSql(sql.ToString());
        }

        /// <summary>
        /// Retorna um cliente configurado para ser religado em horário
        /// </summary>
        /// <param name="numeroCliente"></param>
        /// <returns></returns>
        public DataTable ObterClientesAptosAReligar(string[] listaNumeroClientes)
        {
            StringBuilder sql = new StringBuilder(@"SELECT R.numero_cliente, 0, C.cod_proyecto, C.sucursal,
                       S.regional, C.telefono, C.tipo_ligacao, C.estado_suministro,
                       C.ind_cli_despersona, ' ', ' '
                  FROM cliente_a_religar R, cliente C, sucur S
                 WHERE data_emissao is null
                   AND R.numero_cliente     = C.numero_cliente
                   AND C.estado_cliente in ('0','3')
                   AND S.sucursal = C.sucursal");
            
            sql.AppendFormat(" AND R.numero_cliente in ({0}}", string.Join(",", listaNumeroClientes));

            return ConsultaSql(sql.ToString());
        }


        /// <summary>
        /// Exclui os clientes parametrizados da lista de clientes aptos a Religação fora do prazo do serviço de religação automática (tabela CLIENTES_A_RELIGAR).
        /// Em caso de sucesso, a exclusão ocorre imediatamente, pois não utiliza transação de banco de dados.</summary>
        /// <param name="lstClientesReligar"></param>
        /// <param name="data_emissao">A propriedade data_emissão_operador deve ser utilizada em combinação com a data_emissao.  Utilizar 'IS' caso a data_emissao seja 'null'.</param>
        /// <returns></returns>
        public int ExcluirClientesAReligar(List<ClienteAReligar> lstClientesReligar)
        {
            string[] arrClientes = lstClientesReligar.Select(x => x.numero_cliente).ToArray();

            StringBuilder sql = new StringBuilder();
            foreach(ClienteAReligar pCliente in lstClientesReligar)
            {
            sql.AppendFormat(@"DELETE cliente_a_religar
                   WHERE numero_cliente = {0} ", pCliente.numero_cliente);

            if (!string.IsNullOrEmpty(pCliente.data_emissao_operador))
                sql.AppendFormat("AND data_emissao {0} {1} "
                    , pCliente.data_emissao_operador
                    , pCliente.data_emissao.HasValue ? 
                        string.Format("'{0}'", pCliente.data_emissao.Value.ToString("yyyy-MM-dd HH:mm:ss")) : 
                        "null");

            if (pCliente.data_emissao_min.HasValue && pCliente.data_emissao_min.Value > DateTime.MinValue)
                sql.AppendFormat("AND data_emissao > '{0}' ", pCliente.data_emissao_min.Value.ToString("yyyy-MM-dd HH:mm:ss"));

            if (pCliente.data_emissao_max.HasValue && pCliente.data_emissao_max.Value < DateTime.MaxValue)
                sql.AppendFormat("AND data_emissao < '{0}' ", pCliente.data_emissao_max.Value.ToString("yyyy-MM-dd HH:mm:ss"));

            sql.Append("; ");
            }
            return ExecutarSql(sql.ToString());
        }


        public ResultadoReconexionDTO GerarEntidadeReconexion(DataTable dtResultado)
        {

            ResultadoReconexionDTO reconexion = new ResultadoReconexionDTO();
            
            if (dtResultado.Rows.Count > 0)
            {
                if (!String.IsNullOrWhiteSpace(dtResultado.Rows[0]["numero_cliente"].ToString()))
                    reconexion.numero_cliente = Convert.ToInt32(dtResultado.Rows[0]["numero_cliente"]);
                else
                    reconexion.numero_cliente = 0;

                if (!String.IsNullOrWhiteSpace(dtResultado.Rows[0]["num_ordem_serv_crt"].ToString()))
                    reconexion.numero_ordem_repo = Convert.ToInt32(dtResultado.Rows[0]["num_ordem_serv_crt"]);
                else
                    reconexion.numero_ordem_repo = 0;

                if (!String.IsNullOrWhiteSpace(dtResultado.Rows[0]["fecha_reposicion"].ToString()))
                    reconexion.fecha_reposicion = Convert.ToDateTime(dtResultado.Rows[0]["fecha_reposicion"]);
                else
                    reconexion.fecha_reposicion = null;

                if (!String.IsNullOrEmpty(dtResultado.Rows[0]["tipo_religacao"].ToString()))
                    reconexion.tipo_religacao = dtResultado.Rows[0]["tipo_religacao"].ToString();
                else
                    reconexion.tipo_religacao = null;

                if (!String.IsNullOrEmpty(dtResultado.Rows[0]["fase_repo"].ToString()))
                    reconexion.tipo_religacao = dtResultado.Rows[0]["fase_repo"].ToString();
                else
                    reconexion.tipo_religacao = null;

                if (!String.IsNullOrEmpty(dtResultado.Rows[0]["motivo_repo"].ToString()))
                    reconexion.tipo_religacao = dtResultado.Rows[0]["motivo_repo"].ToString();
                else
                    reconexion.tipo_religacao = null;

                if (!String.IsNullOrEmpty(dtResultado.Rows[0]["acc_realizada_rep"].ToString()))
                    reconexion.acc_realizada_rep = dtResultado.Rows[0]["acc_realizada_rep"].ToString();
                else
                    reconexion.acc_realizada_rep = null;

                if (!String.IsNullOrEmpty(dtResultado.Rows[0]["sit_encon_rep"].ToString()))
                    reconexion.sit_encon_rep = dtResultado.Rows[0]["sit_encon_rep"].ToString();
                else
                    reconexion.sit_encon_rep = null;

                if (!String.IsNullOrEmpty(dtResultado.Rows[0]["leitura_repo"].ToString()))
                    reconexion.leitura_repo = Convert.ToInt32(dtResultado.Rows[0]["leitura_repo"]);
                else
                    reconexion.leitura_repo = 0;

                if (!String.IsNullOrEmpty(dtResultado.Rows[0]["numero_ordem_repo"].ToString()))
                    reconexion.numero_ordem_repo = Convert.ToInt32(dtResultado.Rows[0]["numero_ordem_repo"]);
                else
                    reconexion.numero_ordem_repo = 0;

                if (!String.IsNullOrEmpty(dtResultado.Rows[0]["fecha_solic_repo"].ToString()))
                    reconexion.fecha_solic_repo = Convert.ToDateTime(dtResultado.Rows[0]["fecha_solic_repo"]);
                else
                    reconexion.fecha_solic_repo = null;

                if (!String.IsNullOrEmpty(dtResultado.Rows[0]["data_atual_relig"].ToString()))
                    reconexion.data_atual_relig = Convert.ToDateTime(dtResultado.Rows[0]["data_atual_relig"]);
                else
                    reconexion.data_atual_relig = null;

                if (!String.IsNullOrEmpty(dtResultado.Rows[0]["data_inicio_relg"].ToString()))
                    reconexion.data_inicio_relg = Convert.ToDateTime(dtResultado.Rows[0]["data_inicio_relg"]);
                else
                    reconexion.data_inicio_relg = null;


                return reconexion;
            }
            return new ResultadoReconexionDTO();
        }



        /// <summary>
        /// Nova forma para a obtenção do tipo de religação automática.   Executa a procedure RetornaZonaLocalizUC
        /// </summary>
        /// <param name="numeroCliente"></param>
        /// <returns></returns>
        public string ObterTipoReligacaoAutomatica(string numeroCliente)
        {
            Dictionary<string, object> dicParam = new Dictionary<string, object>();
            dicParam.Add("numero_cliente", numeroCliente);

            using (DBProviderInformix informix = base.ObterProviderInformix())
            {
                informix.OpenConnection();
                try
                {
                    using (DataSet dtResult = informix.ExecuteProcedure("RetornaZonaLocalizUC", dicParam))
                    {
                        if (dtResult == null || dtResult.Tables.Count == 0 || dtResult.Tables[0].Rows.Count == 0
                            || DBNull.Value.Equals(dtResult.Tables[0].Rows[0][0]))
                            throw new Exception("Erro ao consultar o tipo de religação, via procedure RetornaZonaLocalizUC.");

                        return dtResult.Tables[0].Rows[0][0].ToString();
                    }
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }
        }


        //public bool IngressarReligacaoSyntegra(V2ComunicaDTO v2, DBProviderInformix conn)
        //{
        //    #region Prepara a instrução

        //    StringBuilder sql = new StringBuilder("insert into v2_comunica (");
        //    sql.Append("comando, codigo_cp, codigo_cs, codigo_ps, estado, ");
        //    sql.Append("data_ingresso, data_inicio, data_executa, data_modifica, prioridade, ");
        //    sql.Append("id_orion, total_cliente ) values (");

        //    sql.AppendFormat("  '{0}', '{1}', '{2}', '{3}', '{4}'", v2.comando,  v2.codigo_cp,   v2.codigo_cs,    v2.codigo_ps,      v2.estado);  //1..5
        //    sql.AppendFormat("   {0} ", DateTime.MinValue.Equals(v2.data_ingresso)  ? "null" : v2.data_ingresso.ToString("yyyy-MM-dd hh:mm:ss"));
        //    sql.AppendFormat("   {0} ", DateTime.MinValue.Equals(v2.data_inicio)    ? "null" : v2.data_inicio.ToString("yyyy-MM-dd hh:mm:ss"));
        //    sql.AppendFormat("   {0} ", DateTime.MinValue.Equals(v2.data_executa)   ? "null" : v2.data_executa.ToString("yyyy-MM-dd hh:mm:ss"));
        //    sql.AppendFormat("   {0} ", DateTime.MinValue.Equals(v2.data_modifica)  ? "null" : v2.data_modifica.ToString("yyyy-MM-dd hh:mm:ss"));
        //    sql.AppendFormat("   {0} ", v2.prioridade);
        //    sql.AppendFormat(",  {0},   {1})", v2.id_orion, v2.total_cliente);                                     //11..13
            
        //    #endregion

        //    return ExecutarSql(sql.ToString(), conn);
        //}


        /// <summary>
        /// //TODO:
        /// </summary>
        /// <param name="numeroCliente"></param>
        /// <param name="numeroOrdem"></param>
        /// <param name="correlativoVisita"></param>
        /// <param name="numeroLivro"></param>
        /// <param name="transferenciaId"></param>
        /// <param name="transferenciaEstado"></param>
        public void ObterDadosTransferenciaEOrder(string numeroCliente, string numeroOrdem, string correlativoVisita, string numeroLivro, ref int transferenciaId, ref string transferenciaEstado)
        {
            StringBuilder sql = new StringBuilder();

            try
            {
                sql.AppendFormat(@"SELECT id_eorder_transferencia
                                      , estado
			                     FROM eorder_transferencia
                                WHERE ");

                //TODO: validar condições e tipos de cada campo para colocação de aspas simples
                if (string.IsNullOrEmpty(numeroLivro))
                {
                    sql.AppendFormat(" numero_ordem = {0} AND corr_visita = {1} ", numeroOrdem, correlativoVisita);
                }
                else
                {
                    sql.AppendFormat(" numero_livro = {0} AND numero_cliente = {1} ", numeroOrdem, correlativoVisita);
                }

                using (DataTable dt = ConsultaSql(sql.ToString()))
                {
                    if (dt != null && dt.Rows.Count > 0)
                    {
                        transferenciaId = Convert.ToInt32(dt.Rows[0]["id_eorder_transferencia"].ToString());
                        transferenciaEstado = dt.Rows[0]["estado"].ToString();
                    }
                }
            }
            catch(Exception ex)
            {
                throw new Exception(string.Format("Falha ao recuperar dados de transferência do EOrder: {0}", ex.StackTrace));
            }
        }


        /// <summary>
        /// //TODO:
        /// </summary>
        /// <param name="numeroLivro"></param>
        /// <param name="numeroOrdem"></param>
        /// <param name="numeroCliente"></param>
        /// <param name="codigoAcao"></param>
        /// <param name="p1"></param>
        /// <param name="p2"></param>
        public string AnularCortePorBaixaPagamento(string numeroLivro, string numeroOrdem, string numeroCliente, string codigoAcao, string p1, string p2)
        {
            //TESTAR
            #region Trecho do código da função "tratamento_Eorder" no "proceso_arq_parcial"
            //if (string.IsNullOrEmpty(numeroLivro))
            //{
            //TODO: log resultado da procedure e seus parametros

            //EXEC SQL set LOCK MODE TO NOT WAIT;
            //do_error("set LOCK MODE TO NOT WAIT");
            //EXEC SQL EXECUTE PROCEDURE os013_anula_corte_baixa_pgto("", :p_numero_ordem, :p_numero_Cliente,:chr_codigo_acao1,'10','ARQ_PARCIAL') 
            //INTO :chr_Retorno_Rcv;
            //if (Mostrar_Mensagem_De_Erro(p_numero_ordem,p_numero_Cliente)==0)  {			
            //    strcpy(chr_Retorno_Rcv,alltrim(chr_Retorno_Rcv));
            //    if (strcmp(chr_Retorno_Rcv,"OK") != 0){
            //        printf (" resultado da procedure %s \n para os parametros :  Cliente %d, Ordem : %s \n ", chr_Retorno_Rcv,p_numero_Cliente,p_numero_ordem);
            //    }
            //}
            //}
            //else
            //{
            //EXEC SQL set LOCK MODE TO NOT WAIT;
            //do_error("set LOCK MODE TO NOT WAIT");
            //EXEC SQL EXECUTE PROCEDURE os013_anula_corte_baixa_pgto(:p_numero_livro, "", :p_numero_Cliente,:chr_codigo_acao1,'10','ARQ_PARCIAL') 
            //INTO :chr_Retorno_Rcv;
            //if (Mostrar_Mensagem_De_Erro(p_numero_livro,p_numero_Cliente)==0)  {			
            //    strcpy(chr_Retorno_Rcv,alltrim(chr_Retorno_Rcv));
            //    if (strcmp(chr_Retorno_Rcv,"OK") != 0){
            //        printf (" resultado da procedure %s \n para os parametros :  Cliente %d, Numero Livro : %s \n ", chr_Retorno_Rcv,p_numero_Cliente,p_numero_livro);
            //    }
            //}
            //}
            #endregion

            Dictionary<string, object> dicParam = new Dictionary<string, object>();
            dicParam.Add("numero_livro", numeroLivro);
            dicParam.Add("numero_ordem", numeroOrdem);
            dicParam.Add("numero_cliente", numeroCliente);
            dicParam.Add("codigo_acao", codigoAcao);
            dicParam.Add("p1", p1);
            dicParam.Add("p2", p2);

            using (DBProviderInformix informix = base.ObterProviderInformix())
            {
                informix.OpenConnection();
                try
                {
                    using (DataSet dtResult = informix.ExecuteProcedure("os013_anula_corte_baixa_pgto", dicParam))
                    {
                        if (dtResult == null || dtResult.Tables.Count == 0 || dtResult.Tables[0].Rows.Count == 0
                            || DBNull.Value.Equals(dtResult.Tables[0].Rows[0][0]))
                            throw new Exception("Erro ao anular corte por baixa de pagamento, via \"os013_anula_corte_baixa_pgto\".");

                        return dtResult.Tables[0].Rows[0][0].ToString();
                    }
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }
        }



        /// <summary>
        /// //TODO:
        /// </summary>
        /// <param name="id"></param>
        /// <param name="estadoNovo"></param>
        /// <param name="estadoAnterior"></param>
        public bool AtualizarTransferenciaEOrder(int id, string estadoNovo, string estadoAnterior, DBProviderInformix conn)
        {
            //TESTAR
            StringBuilder sqlHisRea = new StringBuilder();
            sqlHisRea.AppendFormat(@"UPDATE Eorder_Transferencia
                                  SET estado = '{0}'
                                    , estado_anterior = '{1}'
                                    , data_estado = current
                                WHERE id_eorder_transferencia = {2}", estadoNovo, estadoAnterior, id);

            return ExecutarSql(sqlHisRea.ToString(), conn);
        }


        /// <summary>
        /// //TODO:
        /// </summary>
        /// <param name="idTransferencia"></param>
        /// <param name="codigoAcao"></param>
        /// <param name="conn"></param>
        /// <returns></returns>
        public bool InserirEventoAnulacaoEOrder(int idTransferencia, string codigoAcao, DBProviderInformix conn)
        {
            StringBuilder sql = new StringBuilder();
            sql.AppendFormat(@"SELECT FIRST 1 numero_evento 
                                 FROM eorder_transferencia_evento 
                                WHERE id_eorder_transferencia = {0}
                             ORDER BY numero_evento desc", idTransferencia);
            
            using(DataTable dt = ConsultaSql(sql.ToString()))
            {
                if (dt != null && dt.Rows.Count > 0)
                {
                    StringBuilder sql2 = new StringBuilder();
                    sql2.AppendFormat(@"INSERT INTO eorder_transferencia_evento (
                                            numero_evento,
                                            cod_acao_evento, 
                                            id_eorder_transferencia,
                                            data_evento,
                                            cod_usuario )
                                        values ({0}, '{1}', {2}, {3}, {4})"
                                           , dt.Rows[0]["numero_evento"]
                                           , codigoAcao
                                           , idTransferencia
                                           , "current"
                                           , "'ARQPARCIAL'");

                   return ExecutarSql(sql2.ToString(), conn);
                }
            }

            return false;
        }
		
		
		#region PANGEA.SalesForce.Corte-Repo
        public RespuestaCorteRepoDTO ConsultarRepoExecutado(ParamCorteRepoDTO repo)
        {
            RespuestaCorteRepoDTO Respuesta = null;
            StringBuilder sql = new StringBuilder();            

            sql.Append("SELECT DISTINCT ");
            sql.Append("os.numero_ordem, ");
            sql.Append("c.numero_cliente, ");
            sql.Append("(SELECT sociedad_sie2000 FROM insta) AS empresa, ");
            sql.Append("(SELECT sociedad_sie2000 FROM insta)||'REPO'||os.numero_ordem AS id_corte_repo, ");
            sql.Append("'R' AS tipo_registro,  ");
            sql.Append("TRIM(trepmot.codigo) ||'R'||(SELECT sociedad_sie2000 FROM insta) AS motivo, ");
            sql.Append("TRIM(os.estado) ||'R'||(SELECT sociedad_sie2000 FROM insta) AS estado_ordem, ");
            sql.Append("c.fecha_reposicion, fecha_solic_repo, ");
            sql.Append("TRIM(acc_realizada_rep) ||'R'||(SELECT sociedad_sie2000 FROM insta) AS acc_realizada_rep, ");
            sql.Append("TRIM(ttiprel.codigo) ||'R'||(SELECT sociedad_sie2000 FROM insta) AS tipo_relig ");
            sql.Append("FROM correp c ,  ordem_servico os , OUTER tabla trepmot, OUTER  tabla ttiprel, OUTER  tabla tacrere, OUTER tabla testord   ");
            sql.Append("WHERE c.numero_cliente = os.numero_cliente AND c.num_ordem_serv_rep = os.numero_ordem   ");
            sql.Append("AND c.motivo_repo = trepmot.codigo  ");
            sql.Append("AND c.tipo_religacao = ttiprel.codigo   ");
            sql.Append("AND c.acc_realizada_rep = tacrere.codigo    ");
            sql.Append("AND os.estado = testord.codigo  ");
            sql.Append("AND fecha_reposicion IS NOT NULL    ");
            sql.Append("AND trepmot.nomtabla = 'REPMOT'                                             ");
            sql.Append("AND trepmot.sucursal = '0000'                                               ");
            sql.Append("AND (trepmot.fecha_activacion <= today)                                     ");
            sql.Append("AND (trepmot.fecha_desactivac >= today OR trepmot.fecha_desactivac IS NULL) ");
            sql.Append("AND ttiprel.NomTabla = 'TIPREL'                                             ");
            sql.Append("AND ttiprel.Sucursal = '0000'                                               ");
            sql.Append("AND (ttiprel.fecha_activacion <= today)                                     ");
            sql.Append("AND (ttiprel.fecha_desactivac >= today OR ttiprel.fecha_desactivac IS NULL) ");
            sql.Append("AND tacrere.nomtabla = 'ACRERE'                                             ");
            sql.Append("AND tacrere.sucursal = '0000'                                               ");
            sql.Append("AND (tacrere.fecha_activacion <= today)                                     ");
            sql.Append("AND (tacrere.fecha_desactivac >= today OR tacrere.fecha_desactivac IS NULL) ");
            sql.Append("AND testord.NomTabla = 'ESTORD'                                             ");
            sql.Append("AND testord.Sucursal = '0000'                                               ");
            sql.Append("AND (testord.fecha_activacion <= today)                                     ");
            sql.Append("AND (testord.fecha_desactivac >= today OR testord.fecha_desactivac IS NULL) ");
            sql.AppendFormat("AND c.numero_cliente = {0}" , repo.NumeroSuministro.ToString());
            //sql.AppendFormat(" AND os.data_ingresso >= '{0}'", repo.FechaInicio.ToString("yyyy-MM-dd HH:mm"));
            //sql.AppendFormat(" AND os.data_ingresso <= '{0}'", repo.FechaFin.ToString("yyyy-MM-dd HH:mm"));

            DataTable result = ConsultaSql(sql.ToString());

            if (result.Rows.Count > 0)
            {
                Respuesta = new RespuestaCorteRepoDTO();
                Respuesta.NumeroSuministro = Convert.ToInt64(result.Rows[0]["numero_cliente"]);
                Respuesta.CodigoEmpresa = result.Rows[0]["empresa"].ToString();

                foreach (DataRow row in result.Rows)
                {
                    var repoSolicitado = new CorteRepoSolicitadoDTO();
                    repoSolicitado.NumeroOrden = row["numero_ordem"].ToString();
                    repoSolicitado.IDCorteRepo = row["id_corte_repo"].ToString();
                    repoSolicitado.TipoRegistro = row["tipo_registro"].ToString();
                    repoSolicitado.Motivo = row["motivo"].ToString().Trim();
                    repoSolicitado.Estado = row["estado_ordem"].ToString().Trim();
                    repoSolicitado.FechaEjecucion = String.Format("{0:s}", row["fecha_reposicion"]).ToString();
                    repoSolicitado.FechaSolicitud = String.Format("{0:s}", row["fecha_solic_repo"]).ToString();
                    repoSolicitado.AccionRealizada = row["acc_realizada_rep"].ToString().Trim();
                    repoSolicitado.Tipo = row["tipo_relig"].ToString().Trim();

                    Respuesta.ListaCortesReposSolicitados.Add(repoSolicitado);

                }
            }

            return Respuesta;
        }

        public RespuestaCorteRepoDTO ConsultarRepoExecutadoGA(ParamCorteRepoDTO repo)
        {
            RespuestaCorteRepoDTO Respuesta = null;
            StringBuilder sql = new StringBuilder();           

            sql.Append("SELECT DISTINCT ");
            sql.Append("os.numero_ordem, ");
            sql.Append("c.numero_cliente, ");
            sql.Append("(SELECT sociedad_sie2000 FROM insta) AS empresa, ");
            sql.Append("(SELECT sociedad_sie2000 FROM insta)||'REPO'||os.numero_ordem AS id_corte_repo, ");
            sql.Append("'R' AS tipo_registro,  ");
            sql.Append("TRIM(trepmot.codigo)||'R'||(SELECT sociedad_sie2000 FROM insta) AS motivo, ");
            sql.Append("TRIM(os.estado) ||'R'||(SELECT sociedad_sie2000 FROM insta) AS estado_ordem, ");
            sql.Append("c.fecha_reposicion, fecha_solic_repo, ");
            sql.Append("TRIM(acc_realizada_rep)||'R'||(SELECT sociedad_sie2000 FROM insta) AS acc_realizada_rep, tacrere.descripcion,   ");
            sql.Append("TRIM(ttiprel.codigo)||'R'||(SELECT sociedad_sie2000 FROM insta) AS tipo_relig   ");
            sql.Append("FROM correp c ,  ordem_servico os , OUTER tabla trepmot, OUTER  tabla ttiprel , OUTER  tabla tacrere, OUTER tabla testord   ");
            sql.Append("WHERE c.numero_cliente = os.numero_cliente AND c.num_ordem_serv_rep = os.numero_ordem   ");
            sql.Append("AND c.motivo_repo = trepmot.codigo  ");
            sql.Append("AND c.tipo_religacao = ttiprel.codigo   ");
            sql.Append("AND c.acc_realizada_rep = tacrere.codigo    ");
            sql.Append("AND os.estado = testord.codigo  ");
            sql.Append("AND fecha_reposicion IS NOT NULL    ");
            sql.Append("AND trepmot.nomtabla = 'REPMOT'                                             ");
            sql.Append("AND trepmot.sucursal = '0000'                                               ");
            sql.Append("AND (trepmot.fecha_activacion <= today)                                     ");
            sql.Append("AND (trepmot.fecha_desactivac >= today OR trepmot.fecha_desactivac IS NULL) ");
            sql.Append("AND ttiprel.NomTabla = 'TIPREL'                                             ");
            sql.Append("AND ttiprel.Sucursal = '0000'                                               ");
            sql.Append("AND (ttiprel.fecha_activacion <= today)                                     ");
            sql.Append("AND (ttiprel.fecha_desactivac >= today OR ttiprel.fecha_desactivac IS NULL) ");
            sql.Append("AND tacrere.nomtabla = 'ACRERE'                                             ");
            sql.Append("AND tacrere.sucursal = '0000'                                               ");
            sql.Append("AND (tacrere.fecha_activacion <= today)                                     ");
            sql.Append("AND (tacrere.fecha_desactivac >= today OR tacrere.fecha_desactivac IS NULL) ");
            sql.Append("AND testord.NomTabla = 'ESTORD'                                             ");
            sql.Append("AND testord.Sucursal = '0000'                                               ");
            sql.Append("AND (testord.fecha_activacion <= today)                                     ");
            sql.Append("AND (testord.fecha_desactivac >= today OR testord.fecha_desactivac IS NULL) ");
            sql.AppendFormat("AND c.numero_cliente = {0} ", repo.NumeroSuministro.ToString());
            //sql.AppendFormat(" AND os.data_ingresso >= '{0}'", repo.FechaInicio.ToString("yyyy-MM-dd HH:mm"));
            //sql.AppendFormat(" AND os.data_ingresso <= '{0}'", repo.FechaFin.ToString("yyyy-MM-dd HH:mm"));

            DataTable result = ConsultaSql(sql.ToString());

            if (result.Rows.Count > 0)
            {
                Respuesta = new RespuestaCorteRepoDTO();
                Respuesta.NumeroSuministro = Convert.ToInt64(result.Rows[0]["numero_cliente"]);
                Respuesta.CodigoEmpresa = result.Rows[0]["empresa"].ToString();

                foreach (DataRow row in result.Rows)
                {
                    var repoSolicitado = new CorteRepoSolicitadoDTO();
                    repoSolicitado.NumeroOrden = row["numero_ordem"].ToString();
                    repoSolicitado.IDCorteRepo = row["id_corte_repo"].ToString();
                    repoSolicitado.TipoRegistro = row["tipo_registro"].ToString();
                    repoSolicitado.Motivo = row["motivo"].ToString().Trim();
                    repoSolicitado.Estado = row["estado_ordem"].ToString().Trim();
                    repoSolicitado.FechaEjecucion = String.Format("{0:s}", row["fecha_reposicion"]).ToString();
                    repoSolicitado.FechaSolicitud = String.Format("{0:s}", row["fecha_solic_repo"]).ToString();
                    repoSolicitado.AccionRealizada = row["acc_realizada_rep"].ToString().Trim();
                    repoSolicitado.Tipo = row["tipo_relig"].ToString().Trim();

                    Respuesta.ListaCortesReposSolicitados.Add(repoSolicitado);
                }
            }

            return Respuesta;
        }

        #endregion
		
		
    }
}
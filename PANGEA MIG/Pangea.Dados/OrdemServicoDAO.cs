using Pangea.Util;
using Pangea.Dados.Base;
using Pangea.Entidades;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Pangea.Entidades.DTO;
using Pangea.Entidades.Enumeracao;
using Entidades.DTO;

namespace Pangea.Dados
{
    public class OrdemServicoDAO : BaseDAO
    {
        private string _empresa;
        private string _database;

        public OrdemServicoDAO(Empresa empresa)
            : base(empresa)
        {
            if (Empresa.NaoIdentificada != empresa)
                this._empresa = empresa.ToString();
        }

        public OrdemServicoDAO(Empresa empresa, string database)
            : base(empresa, database)
        {
            this._empresa = empresa.ToString();
            this._database = database;
        }

        public RetornoOrdemDTO RetornaVisitaOrdem(ParRetornoOrdem ordem)
        {
            StringBuilder sql = new StringBuilder();

            sql.Append(@" SELECT v.cod_retorno, 
	                           v.eletricista, 
	                           v.data_exec_visita, 
	                           v.hora_exec_visita, 
	                           v.rol_ret_visita, 
	                           v.data_ret_visita,            
                               v.hora_fim_prevista, 
                               v.rol_responsavel, 
                               v.dat_lib_def_tec, 
                               v.rol_lib_def_tec, 
                               v.codigo_cargo,
                               v.valor_cargo, 
                               v.numero_ordem_filha, 
                               v.numero_form_venda, 
                               v.serie_form_venda, 
                               v.data_despacho,
                               v.numero_tarefa, 
                               v.data_inic_visita, 
                               v.periodo_agend, 
                               v.data_agend, 
                               v.turno_agend,
                               o.nro_gac            
                        FROM visita_ordem v, ordem_servico o                    
                        WHERE v.numero_ordem = o.numero_ordem
                        AND v.etapa = o.etapa
                        AND v.corr_visita = o.corr_visita ");            
            sql.Append(" and v.numero_ordem = '" + ordem.numero_ordem + "'");
            sql.Append(" and v.corr_visita = " + ordem.corr_visita);


            var dt = ConsultaSql(sql.ToString());

            if (dt.Rows.Count > 0)
                return DataHelper.ConvertDataTableToEntity<RetornoOrdemDTO>(dt);
            else
                return null;
        }

        public OrdemServico validaOrdem(string numeroOrdem, LibOrdemDTO obj)
        {
            DataTable dt = new DataTable();

            if (obj.TipoOrdem == "OUT")
            {
                StringBuilder sql = new StringBuilder();
                sql.Append(String.Format(@"Select O.numero_ordem, O.corr_visita, O.estado, O.tipo_ordem, O.tipo_servico, O.corr_pendencia                                                      
                                                from ordem_servico O, tabla T, servicos S, etapa_servico E, tabla T1, OUTER (cliente N, localidades L) 
                                                Where O.numero_ordem = '{0}'  
                                                and O.tipo_ordem != 'NOV'  
                                                and O.estado in ('05', '07')  
                                                and T.nomtabla = 'TIPORD'  
                                                and T.sucursal = '0000'  
                                                and T.codigo = O.tipo_ordem  
                                                and S.tipo_ordem = O.tipo_ordem  
                                                and S.cod_servico = O.tipo_servico  
                                                and E.tipo_ordem = O.tipo_ordem  
                                                and E.tipo_servico = O.tipo_servico  
                                                and E.cod_etapa = O.etapa  
                                                and T1.nomtabla = 'ESTORD'  
                                                and T1.sucursal = '0000'  
                                                and T1.codigo = O.estado  
                                                and N.numero_cliente = O.numero_cliente ", numeroOrdem));
                if (obj.tipoCliente == "BT")
                {
                    sql.Append(" and L.municipio = N.municipio");
                }
                else
                {
                    sql.Append(" and L.municipio =N.comuna");
                }
                sql.Append(String.Format(" and L.localidad = L.municipio * 10"));

                dt = ConsultaSql(sql.ToString());
            }
            else if (obj.TipoOrdem == "NOV")
            {
                StringBuilder sql = new StringBuilder();
                sql.Append(String.Format(@"Select O.numero_ordem, O.corr_visita, O.estado, O.tipo_ordem, O.tipo_servico, corr_pendencia 
                                                from ordem_servico O, tabla T, servicos S, etapa_servico E, tabla T1, cliente_novo N, localidades L 
                                                Where O.numero_ordem = '{0}'  
                                                and O.estado in ('05', '07')  
                                                and T.nomtabla = 'TIPORD'  
                                                and T.sucursal = '0000'  
                                                and T.codigo = O.tipo_ordem  
                                                and S.tipo_ordem = O.tipo_ordem  
                                                and S.cod_servico = O.tipo_servico  
                                                and E.tipo_ordem = O.tipo_ordem  
                                                and E.tipo_servico = O.tipo_servico  
                                                and E.cod_etapa = O.etapa  
                                                and T1.nomtabla = 'ESTORD'  
                                                and T1.sucursal = '0000'  
                                                and T1.codigo = O.estado  
                                                and N.numero_cliente = O.numero_cliente ", numeroOrdem));

                if (obj.tipoCliente == "BT")
                {
                    sql.Append(" and L.municipio = N.municipio");
                }
                else
                {
                    sql.Append(" and L.municipio = N.comuna");
                }
                sql.Append(String.Format(" and L.localidad = L.municipio * 10"));
                dt = ConsultaSql(sql.ToString());
            }

            if (dt.Rows.Count > 0)
                return DataHelper.ConvertDataTableToEntity<OrdemServico>(dt);
            else
                return null;
        }

        public VisitaOrdemDTO RetornaVisita(OrdemServico ordemServico, LibOrdemDTO obj)
        {
            StringBuilder sql = new StringBuilder();
            sql.Append(String.Format(@" select   V.data_visita, 
		                                           V.eletricista, 
		                                           C.razon_social, 
		                                           V.data_exec_visita, 
		                                           V.hora_exec_visita, 
		                                           R.ind_efeito_tempo, 
		                                           V.numero_ordem_filha "));
            if (obj.tipoCliente == "BT")
                sql.Append(" ,V.ind_agendado  ");


            sql.Append(String.Format(@"from visita_ordem V, OUTER cotrat C, OUTER retorno_servico R  
	                                        where V.numero_ordem = '{0}'
                                            and V.corr_visita = {1}  
	                                        and C.contratista = V.empreiteira  
	                                        and R.tipo_ordem = '{2}'  
	                                        and R.tipo_servico = '{3}'  
	                                        and R.etapa = V.etapa  
	                                        and R.codigo_retorno = V.cod_retorno", ordemServico.numero_ordem, ordemServico.corr_visita, ordemServico.tipo_ordem, ordemServico.tipo_servico));

            var dt = ConsultaSql(sql.ToString());
            if (dt.Rows.Count > 0)
                return DataHelper.ConvertDataTableToEntity<VisitaOrdemDTO>(dt);
            else
                return null;
        }

        public ActuacionSolicitudDTO retornaActuacionSolicitud(string numeroOrdem)
        {
            DBProviderInformix informix = ObterProviderInformix();
            DataTable dt = new DataTable();

            String sql = String.Format(@" SELECT os.numero_ordem as NumeroOrden,
                                                os.estado AS Estado,  
                                                es.descricao_etapa AS Descripcion,	
                                                os.nro_caso AS NumeroCaso,
                                                a.descripcion AS Responsable,
                                                rs.ind_procedente AS Favorabilidad,
                                                rs.descricao_retorno as DescripcionRetornoOrden,                 
                                                vo.rol_ret_visita AS ROL                                                                                              
        
                                            FROM ordem_servico os,
                                                etapa_servico es,
                                                retorno_servico rs,
                                                visita_ordem vo,
                                                OUTER area a                                                
                                            WHERE os.tipo_ordem = es.tipo_ordem
                                            AND os.tipo_servico = es.tipo_servico
                                            AND os.etapa = es.cod_etapa
                                            AND os.tipo_ordem = rs.tipo_ordem
                                            AND os.tipo_servico = rs.tipo_servico
                                            AND os.etapa = rs.etapa
                                            AND vo.numero_ordem = os.numero_ordem
                                            AND vo.corr_visita = os.corr_visita
                                            AND vo.etapa = os.etapa    
                                            AND vo.cod_retorno = rs.codigo_retorno
                                            AND os.area_destino = a.area                                            
                                            AND os.numero_ordem= '{0}'", numeroOrdem);

            dt = ConsultaSql(sql, informix);
            if (dt.Rows.Count > 0)
            {
                ActuacionSolicitudDTO resultado = DataHelper.ConvertDataTableToEntity<ActuacionSolicitudDTO>(dt);
                return resultado;
            }
            else
                return null;

        }

        public DataTable Consultar(OrdemServicoDTO obj)
        {
            if (obj == null)
                return new DataTable();

            if (string.IsNullOrEmpty(this._empresa))
            {
                //TODO: gerar log antes de lançar erro
                throw new ArgumentException("Parâmetro empresa obrigatório para a consulta de ordem de serviço.");
            }

            #region Prepara a consulta básica

            StringBuilder sql = new StringBuilder("SELECT * FROM ordem_servico WHERE 1=1 ");

            if (!string.IsNullOrEmpty(obj.tipo_ordem))
                sql.AppendFormat("AND tipo_ordem = '{0}' ", obj.tipo_ordem);

            if (!string.IsNullOrEmpty(obj.tipo_servico))
                sql.AppendFormat("AND tipo_servico = '{0}' ", obj.tipo_servico);

            if (!string.IsNullOrEmpty(obj.estado))
                sql.AppendFormat("AND estado IN ({0}) ", obj.estado);

            if (!string.IsNullOrEmpty(obj.estadoNot))
                sql.AppendFormat("AND estado NOT IN ({0}) ", obj.estadoNot);

            if (!string.IsNullOrEmpty(obj.numero_ordem_relac))
            {
                sql.AppendFormat("AND numero_ordem = '{0}' ", obj.numero_ordem_relac);
            }
            else if (!string.IsNullOrEmpty(obj.numero_ordem))
            {
                sql.AppendFormat("AND numero_ordem = '{0}' ", obj.numero_ordem);
            }

            if (!string.IsNullOrEmpty(obj.numero_cliente))
                sql.AppendFormat("AND numero_cliente = '{0}' ", obj.numero_cliente);

            #endregion

            return ConsultaSql(sql.ToString());
        }

        public DataTable ConsultarOrdemReclamacao(string pNumeroOrdemOriginal)
        {
            if (string.IsNullOrEmpty(pNumeroOrdemOriginal))
                throw new ArgumentNullException("Parâmetro 'numero ordem original' obrigatório para a consulta de ordem de serviço.");

            if (string.IsNullOrEmpty(this._empresa))
            {
                //TODO: gerar log antes de lançar erro
                throw new ArgumentException("Parâmetro 'empresa' obrigatório para a consulta de ordem de serviço.");
            }

            #region Prepara a consulta básica

            StringBuilder sql = new StringBuilder("select O.estado, to_ddmmyyyy(V.data_exec_visita), R.ind_procedente, to_ddmmyyyy(today) ");
            sql.Append("from ordem_servico O, visita_ordem V, visita_ordem V1, retorno_servico R ");
            sql.AppendFormat("where O.numero_ordem = '{0}' ", pNumeroOrdemOriginal);
            sql.Append("and V.numero_ordem = O.numero_ordem ");
            sql.Append("and V.etapa = 'RCE' ");
            sql.Append("and V1.numero_ordem = O.numero_ordem  ");
            sql.Append("and V1.etapa in ('AST','ANT') ");
            sql.Append("and R.tipo_ordem = O.tipo_ordem ");
            sql.Append("and R.tipo_servico = O.tipo_servico ");
            sql.Append("and R.etapa = V1.etapa ");
            sql.Append("and R.codigo_retorno = V1.cod_retorno");

            #endregion

            return ConsultaSql(sql.ToString());
        }
        
        public bool Insert(OrdemServicoDTO obj, DBProviderInformix conn)
        {
            if (obj == null)
                return false;

            #region Prepara a instrução

            StringBuilder sql = new StringBuilder("insert into ordem_servico (");
            sql.Append("sucursal_origem, area_origem, numero_ordem,tipo_ordem,tipo_servico, ");
            sql.Append("numero_cliente, ");
            if (! "AT".Equals(_database)) sql.Append("municipio,"); 
            sql.Append("data_ingresso, estado, data_estado, ");
            sql.Append("etapa, data_etapa, tempo_def_tecnico, tempo_def_tec_etap, area_destino, ");
            sql.Append("Sucursal_Destino, regional_destino, observacoes, telefone_contato, rol_ingresso, ");
            sql.Append("corr_visita, data_cont_def_tec, numero_ordem_relac");
            if ("AT".Equals(_database)) sql.Append(") values (");
            if (! "AT".Equals(_database)) sql.Append(",sucursal_original, ");
            if (!"AT".Equals(_database)) sql.Append("referencia, nro_caso ) values (");

            sql.AppendFormat("  '{0}', '{1}', '{2}', '{3}', '{4}'", obj.sucursal_destino, obj.area_destino, obj.numero_ordem, obj.tipo_ordem, obj.tipo_servico);  //1..5
            sql.AppendFormat(", '{0}'", string.IsNullOrEmpty(obj.numero_cliente) ? string.Empty : obj.numero_cliente.ToString());                                            //6
            if(! "AT".Equals(_database)) sql.AppendFormat(", '{0}'", string.IsNullOrEmpty(obj.numero_cliente) ? obj.municipio : string.Empty);                                                            //7


            string dataIngresso = String.Empty;

            if (obj.data_ingresso == null)
                sql.AppendFormat(",  {0}, '{1}', {2}", "current", "00", "current");                                                                              //8..10
            else
            {
                dataIngresso = obj.data_ingresso.ToString().Replace("/", "-").Remove(16);
                sql.AppendFormat(",  to_date('{0}', '%d-%m-%Y %H:%M'), '{1}', {2}", dataIngresso, "00", "current");                                                                              //8..10
            }

            sql.AppendFormat(", '{0}', {1}, '{2}', '{3}', '{4}'", obj.etapa, "current", "0", "0", obj.area_destino);                                        //11..15
            sql.AppendFormat(", '{0}', '{1}', '{2}', '{3}', '{4}'", obj.regional_destino.Trim(), obj.regional_destino.Trim(), obj.observacoes, obj.telefone_contato, obj.rol_ingresso); //16..20
            sql.AppendFormat(",  {0}, {1}, '{2}'", "0", "current", obj.numero_ordem_relac);                 //21..25
            if (! "AT".Equals(_database)) sql.AppendFormat(",'{0}'", obj.sucursal_origem);
            if (!"AT".Equals(_database)) sql.AppendFormat(", '{0}', '{1}' )", obj.referencia, obj.nro_gac.ToString());			
            if ("AT".Equals(_database)) sql.Append(")");

            #endregion

            return ExecutarSql(sql.ToString(), conn);
        }

        /// <summary>
        /// Gera um novo numero de ordem de serviço, utilizando a procedure PROXNUMORDEM('NEW').
        /// </summary>
        /// <returns>O novo número de ordem.</returns>
        public string GerarNumeroOrdem()
        {
            Dictionary<string, object> dicParam = new Dictionary<string, object>();
            dicParam.Add("_tipo", "NEW");

            using (DBProviderInformix informix = base.ObterProviderInformix())
            {
                informix.OpenConnection();
                try
                {
                    using (DataSet dsNumeroOrdem = informix.ExecuteProcedure("proxnumordem", dicParam))
                    {
                        if (dsNumeroOrdem == null || dsNumeroOrdem.Tables.Count == 0 || dsNumeroOrdem.Tables[0].Rows.Count == 0
                            || DBNull.Value.Equals(dsNumeroOrdem.Tables[0].Rows[0][0]))
                            throw new Exception("Erro ao gerar um novo numero de ordem de serviço via procedure.");

                        return dsNumeroOrdem.Tables[0].Rows[0][0].ToString();
                    }
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }
        }

        /// <summary>
        /// Retorna o último número correspondente ao sequencial de número de ordem de serviço.
        /// </summary>
        /// <param name="codArea"></param>
        /// <param name="tipoOrdem"></param>
        /// <returns>O último número de ordem somente para a Coelce.</returns>
        public string RecuperarUltimoNumeroOrdemNumao(string codArea, string tipoOrdem)
        {
            string sql = string.Format(@"select numero                                           
                                           from numao
                                          where area = '{0}'
                                            and tipo_orden = '{1}'", codArea, tipoOrdem);

            DataTable dt = ConsultaSql(sql);

            if (dt == null || dt.Rows.Count == 0)
                return string.Empty;

            return dt.Rows[0][0].ToString().Trim();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="area"></param>
        /// <param name="tipoOrdem"></param>
        /// <param name="novoNumero"></param>
        /// <param name="informix"></param>
        /// <returns>True|False.  Somente para a Coelce.</returns>
        public bool IngressarNumeroOrdemNumao(string area, string tipoOrdem, string novoNumero, DBProviderInformix informix)
        {
                string sql = string.Format(@"INSERT INTO NUMAO (area, tipo_orden, numero) values ('{0}','{1}','{2}')", area, tipoOrdem, novoNumero);

                return ExecutarSql(sql.ToString(), informix);
        }
		
		public string RetornaNumeroOrdem()
        {
            var result = string.Empty;

            string sqlProcedure = string.Format("EXECUTE PROCEDURE proxnumordem('NEW')");

            DataTable dt = ConsultaSql(sqlProcedure.ToString());

            if (dt.Rows.Count > 0)
                result = dt.Rows[0][0].ToString().Trim();

            return result;
        }
        public DataTable RetornaNumeroExistenteOrdem(int numerGac, int numero_cliente)
        {
            String sql = String.Format(@"SELECT o.numero_ordem, 
                                                o.tipo_ordem, 
                                                o.tipo_servico,
                                                p.codigo 
                                         FROM ordem_servico o 
                                         WHERE o.nro_caso={0}
                                         AND o.numero_cliente={1}", numerGac, numero_cliente);
            var dt = ConsultaSql(sql.ToString());

            if (dt.Rows.Count > 0)
                return dt;
            else
                return null;
        }
        public string RetornaNumeroExistenteOrdem(int NumeroCaso)
        {
            String sql = String.Format(@"SELECT numero_ordem 
                                        FROM ordem_servico 
                                        WHERE 
                                        nro_caso={0} ", NumeroCaso);
            var dt = ConsultaSql(sql.ToString());

            if (dt.Rows.Count > 0)
                return dt.Rows[0]["numero_ordem"].ToString();
            else
                return null;
        }

        public string SynergiaBas_Gerar_Numero_Ordem(string area, string tipo_ordem)
        {
            string result = string.Empty;

            string sql = string.Format(@"select numero                                           
                                         from numao
                                         where area = '{0}'
                                         and tipo_orden = '{1}'",area, tipo_ordem);
            
            DataTable dt = ConsultaSql(sql.ToString());

            if (dt.Rows.Count > 0)
            {
                result = dt.Rows[0][0].ToString().Trim();
                return result;
            }

                return result;
        }

        public bool insertNumao(string Gerar_Numero_Ordem_pr_str_Area, DBProviderInformix informix)
        {
            bool resultado = true;
            try
            {
                string sql = string.Format(@"into numao(area,tipo_orden,numero)values('{0}','NOV','000001'", Gerar_Numero_Ordem_pr_str_Area);

                ExecutarSql(sql.ToString(), informix);
            }
            catch (Exception ex)
            {
                resultado = false;
                throw;
            }

            return resultado;
        }

		/// <summary>
        /// 
        /// </summary>
        /// <param name="numero"></param>
        /// <param name="area"></param>
        /// <param name="tipoOrdem"></param>
        /// <param name="informix"></param>
        /// <returns>True|False.  Somente para a Coelce.</returns>
        public bool UpdateNumao(string numero, string area, string tipoOrdem, DBProviderInformix informix)
        {

                string sql = string.Format(@"update numao
                                            set numero = '{0}'
                                              where area = '{1}'
                                                and tipo_orden = '{2}'", numero, area, tipoOrdem);

            return ExecutarSql(sql.ToString(), informix);
        }

        #region Ordem de serviço de artefatos

        public Boolean VerificaSeClienteAtivo(string tipoOrdem, string tipoServico, string numerocliente)
        {
            bool result = false;

            try
            {
                DBProviderInformix informix = ObterProviderInformix();

                string sql = string.Format(@" 
                                            SELECT 
                                                ind_cliente_desper
                                            FROM     
                                                Servicos
                                            WHERE 
                                                Tipo_Ordem = " + "'" + tipoOrdem + "' AND" + @" 
                                                cod_Servico = " + "'" + tipoServico + "'" + @"                                                
                                         ");

                result = ConsultaSql(sql, informix).Rows.Count > 0;

                if (!result)
                {
                    throw new Exception("Serviço não encontrado.");
                }
                else
                {
                    sql = string.Format(@" 
                                            SELECT 
                                                ind_cli_despersona
                                            FROM     
                                                Cliente
                                            WHERE 
                                                Numero_Cliente = " + numerocliente + @" 
                                        ");

                    DataTable dtResultado = ConsultaSql(sql);

                    if (dtResultado.Rows.Count > 0)
                        if (TratarString(dtResultado, dtResultado.Rows[0], "ind_cli_despersona") == "S")
                            result = true;
                        else
                            result = false;
                }
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }

            return result;
        }

        public bool VerificaSeAparelhoEstaCadastrado(string codArtefato)
        {
            bool result = false;

            try
            {
                DBProviderInformix informix = ObterProviderInformix();

                string sql = string.Format(@" 
                                            SELECT 
                                                descricao_item
                                            FROM     
                                                item_indenizavel
                                            WHERE 
                                                 cod_item = " + "'" + codArtefato + "'" + @"                                               
                                         ");

                DataTable dtResultado = ConsultaSql(sql);

                if (dtResultado.Rows.Count > 0)
                    result = true;
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }

            return result;
        }

        public bool InsertAparelhoOrdem(OrdemServicoArtefatosDTO obj, DBProviderInformix conn)
        {
            bool result = false;

            try
            {
                string sql = string.Format(@" 
                                        INSERT INTO aparelho_ordem
                                        (
                                            Numero_Ordem,    
                                            Solic_Nome,
                                            Solic_Cpf, 
                                            Solic_Rg, 
                                            Solic_Fax, 
                                            Email,
                                            Pagto_Forma,
                                            Pagto_Banco,
                                            Pagto_Agencia, 
                                            Pagto_Conta, 
                                            Ocorrencia_Desc,
                                            Ocorrencia_Data,
                                            Ind_Coelceavisada, 
                                            Ind_HouveAtendimen,
                                            Ind_OutrosClientes,
                                            Ind_OscilacaoTensa,
                                            Nro_Aviso,
                                            Data_Vistoria,
                                            Turno_Vistoria,
                                            Tipo_Doc_Apresent,
                                            Nro_Doc_Apresent,
                                            Data_Doc_Apresent,
                                            Valor_Cobrado
                                        )
                                        VALUES
                                        (   
                                          " + "'" + obj.numero_ordem + "'" + @",
                                          " + "'" + obj.solic_nome + "'" + @",
                                          " + "'" + obj.solic_cpf + "'" + @",
                                          " + "'" + obj.solic_rg + "'" + @",
                                          " + "'" + obj.solic_fax + "'" + @", 
                                          " + "'" + obj.email + "'" + @",
                                          " + "'" + obj.pagto_forma + "'" + @",
                                          " + "'" + obj.pagto_banco + "'" + @",
                                          " + "'" + obj.pagto_agencia + "'" + @",
                                          " + "'" + obj.pagto_conta + "'" + @",
                                          " + "'" + obj.ocorrencia_desc + "'" + @", 
                                          
                                            " + "TO_DATE('" + Convert.ToDateTime(obj.ocorrencia_data).ToString("dd/MM/yyyy") + " " + Convert.ToDateTime(obj.ocorrencia_hora).ToString("HH:mm") + "','%d/%m/%Y %H:%M')" + @",

                                          " + "'" + obj.ind_coelceavisada + "'" + @",
                                          " + "'" + obj.ind_houveatendimen + "'" + @", 
                                          " + "'" + obj.ind_outrosclientes + "'" + @",
                                          " + "'" + obj.ind_oscilacaotensa + "'" + @",
                                          " + "'" + obj.nro_aviso + "'" + @",
                                          " + "'" + Convert.ToDateTime(obj.data_vistoria).ToString("MM/dd/yyyy") + "'" + @", 
                                          " + "'" + obj.turno_vistoria + "'" + @",
                                          " + "'" + obj.tipo_doc_apresent + "'" + @",
                                          " + "'" + obj.nro_doc_apresent + "'" + @",
                                          " + "'" + Convert.ToDateTime(obj.data_doc_apresent).ToString("MM/dd/yyyy") + "'" + @",
                                          " + obj.valor_cobrado + @"
                                        )"
                                   );

                result = Convert.ToBoolean(ExecutarSql(sql.ToString()));
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }

            return result;
        }


        public bool InsertAparelhoQueimado(OrdemServicoArtefatosDTO obj, DBProviderInformix conn)
        {
            bool result = false;

            try
            {
                string sql = string.Format(@" 
                                                INSERT INTO aparelho_queimado
                                                (
                                                    Numero_Ordem,
                                                	Corr_Aparelho,
                                                    Cod_Artefacto,
                                                    Marca, 
                                                    AnoFabricacao, 
                                                    ValorOrcado, 
                                                    QtdOrcada
                                                )
                                                VALUES
                                                ( 
                                                  " + "'" + obj.numero_ordem + "'" + @",  
                                                  " + obj.corr_aparelho + @",
                                                  " + "'" + obj.cod_artefacto + "'" + @",
                                                  " + "'" + obj.marca + "'" + @",
                                                  " + obj.anofabricacao + @",
                                                  " + obj.valororcado + @", 
                                                  " + obj.qtdorcada + @"
                                                )"
                                           );

                result = Convert.ToBoolean(ExecutarSql(sql.ToString()));
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }

            return result;
        }

        #endregion

        public bool IngressarSolicitanteOrdem(SolicitanteOrdemDTO obj, DBProviderInformix conn)
        {
            string sql = string.Format(@"insert into solicitante_ordem 
                                                (numero_ordem, 
                                                 nome, 
                                                 numero_documento, 
                                                 municipio, 
                                                 endereco, 
                                                 telefone_contato) 
                                        values ('{0}', '{1}', '{2}', '{3}', '{4}', '{5}')"
                , obj.numero_ordem
                , obj.nome
                , obj.numero_documento
                , obj.municipio
                , obj.endereco
                , obj.telefone_contato);

            return ExecutarSql(sql.ToString(), conn);
        }

        public Boolean ExecutaProcedureLiberarOrdem(string paramNumeroOrdem, Empresa paramEmpresa, DBProviderInformix informix)
        {
            var sqlProcedure = new StringBuilder();
            sqlProcedure.AppendFormat("EXECUTE PROCEDURE os001_liberaordem('{0}','','')", paramNumeroOrdem);

            DataTable dt = ConsultaSql(sqlProcedure.ToString(),informix);

            if (dt.Rows.Count > 0)
            {
                if (dt.Rows[0]["Column1"].ToString().Contains("ERRO"))
                {
                    return false;
                }
                else
                    return true;
            }

            return false;
        }

        public bool verOrdemAgendamento(string numeroOrdem)
        {
            bool resultado = true;

            String sql = String.Format(@"select * from ordem_agendamento 
                                            where numero_ordem = '{0}'
                                            AND estado in ('G','I')", numeroOrdem);

            var dt = ConsultaSql(sql.ToString());

            if (dt.Rows.Count > 0)
                return resultado = true;
            else
                return resultado = false;
        }

        public string retornaEstadoOrdemFilha(string numeroOrdemFilha)
        {
            String sql = String.Format(@"select estado 
            		                        from ordem_servico
            		                        where numero_ordem = '{0}'", numeroOrdemFilha);

            var dt = ConsultaSql(sql.ToString());

            if (dt.Rows.Count > 0)
                return dt.Rows[0]["estado"].ToString();
            else
                return null;
        }

        public bool retornaPendenciaOrdem(OrdemServico ordemServico)
        {
            bool resultado = false;

            String sql = String.Format(@"SELECT P.numero_ordem				
                                                from pendencia_ordem P, OUTER etapa_servico E, OUTER rol R, OUTER retorno_servico R1         
			                                                where P.numero_ordem =   '{0}'
			                                                 and P.corr_pendencia =  {1}
			                                                 and E.tipo_ordem = '{2}'
			                                                 and E.tipo_servico =  '{3}'
			                                                 and E.cod_etapa = P.etapa         
			                                                 and R.rol = P.rol_pendencia         
			                                                 and R1.tipo_ordem =  '{2}'
			                                                 and R1.tipo_servico =  '{3}'
			                                                 and R1.etapa = P.etapa 
			                                                 and R1.codigo_retorno = P.cod_retorno"
                                                             , ordemServico.numero_ordem
                                                             , ordemServico.corr_pendencia
                                                             , ordemServico.tipo_ordem
                                                             , ordemServico.tipo_servico);

            var dt = ConsultaSql(sql.ToString());

            if (dt.Rows.Count > 0)
                return resultado = true;
            else
                return resultado = false;

        }

        public Boolean GravaAuditoria_OS(string paramNumeroOrdem, DBProviderInformix informix)
        {
            OrdemServico OS = RecuperaDadosOS(paramNumeroOrdem);
            String sql = string.Empty;
            if (!String.IsNullOrEmpty(OS.numero_ordem))
            {
                sql = String.Format(@"INSERT INTO audit_os (numero_ordem, 
                                                            etapa_ant, 
                                                            etapa_pos, 
                                                            estado_ant,
                                                            estado_pos, 
                                                            data_ingresso, 
                                                            rol_ingresso, 
                                                            data_alteracao, 
                                                            rol_alteracao, 
                                                            estacao, 
                                                            acao, 
                                                            numero_ordem_assoc)
                                           VALUES ('{0}','{1}','{2}','{3}','{4}','{5}','{6}','{7}','{8}','{9}','{10}','{11}')",
                                                            OS.numero_ordem //0 numero_ordem
                                                          , OS.etapa //1 - etapa_ant
                                                          , OS.etapa //2 - etapa
                                                          , OS.estado //3 - data_ingresso
                                                          , OS.estado //4 - rol_ingresso
                                                          , OS.data_ingresso //5
                                                          , OS.rol_ingresso //6
                                                          , DateTime.Now //7
                                                          , "SALESFORCE" //8
                                                          , "" //9
                                                          , "" //10
                                                          , OS.numero_ordem_relac); //11
            }
            
            return ExecutarSql(sql, informix);
        }

        public OrdemServico RecuperaDadosOS(string paramNumeroOrdem)
        {

            String sql = String.Format(@"SELECT o.numero_ordem
                                            ,o.etapa
                                            ,o.estado
                                            ,o.data_ingresso 
                                            ,rol_ingresso 
                                        FROM ordem_servico o 
                                       WHERE numero_ordem =  '{0}'", paramNumeroOrdem);

            var dt = ConsultaSql(sql);
            if (dt.Rows.Count > 0)
                return DataHelper.ConvertDataTableToEntity<OrdemServico>(dt);
            else
                return new OrdemServico();
        }

        public string consultaTablaGomret(string cod_retorno)
        {
            string retorno = string.Empty;

            String sql = String.Format(@"select valor_alf 
                                        from tabla 
                                        where nomtabla = 'GOMRET' 
                                        and sucursal = '0000' 
                                        and codigo = '{0}'", cod_retorno);

            var dt = ConsultaSql(sql.ToString());

            if (dt.Rows.Count > 0)
                retorno = dt.Rows[0]["valor_alf"].ToString();
            else
                retorno = null;

            return retorno;


        }

        public string retornaVisitaOrdem(string numero_ordem)
        {
            string retorno = string.Empty;

            String sql = String.Format(@"select cod_retorno 
                                       from visita_ordem v, ordem_servico o 
                                       where v.numero_ordem = o.numero_ordem 
                                         and v.etapa =o.etapa
                                         and v.corr_visita = o.corr_visita     
                                         and o.numero_ordem = '{0}'", numero_ordem);

            var dt = ConsultaSql(sql.ToString());

            if (dt.Rows.Count > 0)
                retorno = dt.Rows[0]["cod_retorno"].ToString();
            else
                retorno = null;

            return retorno;
        }
        public bool ExisteSolOrcDeObra(string numero_ordem)
        {
            String sql = String.Format(@"SELECT numero_ordem FROM sol_orc_ordem WHERE numero_ordem='{0}'", numero_ordem);

            var dt = ConsultaSql(sql.ToString());

            if (dt.Rows.Count > 0)
                return true;
            else
                return false;
        }

        public string RetornaOrdemTrocaDeMedidorAberta(string numCliente)
        {
            if (string.IsNullOrEmpty(numCliente))
                return string.Empty;

            string sql = string.Empty;

            #region Consulta
            sql = "SELECT numero_ordem FROM ordem_servico s WHERE 1=1 ";
            sql += "and tipo_ordem = 'OUT' ";
            sql += "and tipo_servico in ('ACB','ACM','ACT','AFI','AMP','DCM','DCB', 'DCT','SMI','SMP') ";
            sql += "and estado not in ('03','04','06','09') ";
            sql += string.Format("AND numero_cliente = '{0}' ", numCliente);
            #endregion

            DataTable dt = ConsultaSql(sql);

            if (dt.Rows.Count > 0)
                return dt.Rows[0]["numero_ordem"].ToString();

            return string.Empty;
        }

        public string RetornaOrdemAberta(string numCliente, Servico serv)
        {
            if (string.IsNullOrEmpty(numCliente) || serv == null)
                return string.Empty;

            string sql = string.Empty;

            #region Consulta
            sql = "SELECT numero_ordem FROM ordem_servico s WHERE 1=1 ";
            sql += string.Format("AND tipo_ordem = '{0}' ", serv.tipo_ordem);
            sql += string.Format("AND tipo_servico = '{0}' ", serv.cod_servico);
            sql += string.Format("AND numero_cliente = '{0}' ", numCliente);
            sql += "and estado not in ('03','04','06','09') ";
            #endregion

            DataTable dt = ConsultaSql(sql);

            if (dt.Rows.Count > 0)
                return dt.Rows[0]["numero_ordem"].ToString();

            return string.Empty;
        }

        public string ConsultarEtapaDoServico(string tipoOrdem, string tipoServico)
        {
            int SequenciaDaEtapaDoServico = 2;
            if (tipoServico.Equals("VPE"))
            {
                SequenciaDaEtapaDoServico = 3;
            }

            StringBuilder sql = new StringBuilder();
            sql.AppendFormat(@"select
                    cod_etapa 
                from 
                    etapa_servico 
                where 
                    tipo_ordem = '{0}'  
                    and tipo_servico = '{1}'  
                    and sequencia_etapa = {2} 
                    and ( data_desativacao >= current or data_desativacao is null)",
                    tipoOrdem, tipoServico, SequenciaDaEtapaDoServico);

            DataTable dataTable = ConsultaSql(sql.ToString());

            return dataTable.Rows[0]["cod_etapa"].ToString();
        }


        public bool IngressarOrdem(Empresa empresa, OrdemServicoDTO ordem, DBProviderInformix conexao, string DataBase = "BT", bool useDirtyRead = false)
        {
            string sql = string.Format(@"insert into 
                                            ordem_servico(
                                                sucursal_origem, 	
                                                area_origem, 
                                                numero_ordem,
                                                tipo_ordem,
                                                tipo_servico, 
                                                numero_cliente, 
                                                municipio, 
                                                data_ingresso, 
                                                estado, 
                                                data_estado, 
                                                etapa,
                                                data_etapa, 
                                                tempo_def_tecnico,
                                                tempo_def_tec_etap, 
                                                area_destino,
                                                Sucursal_Destino,
                                                regional_destino, 
                                                observacoes, 
                                                telefone_contato, 
                                                rol_ingresso, 
                                                corr_visita, 
                                                data_cont_def_tec, 
                                                numero_ordem_relac,
                                                motivo_ingresso, 
                                                sucursal_original, 
                                                referencia 
                                            )values(
                                                '{0}', 
                                                '{1}', 
                                                '{2}', 
                                                '{3}',  
                                                '{4}',  
                                                '{5}', 
                                                '{6}', 
                                                '{7}', 
                                                '{8}', 
                                                '{9}', 
                                                '{10}',  
                                                '{11}', 
                                                '{12}', 
                                                '{13}', 
                                                '{14}', 
                                                '{15}', 
                                                '{16}',
                                                '{17}',
                                                '{18}',
                                                '{19}',
                                                '{20}',
                                                '{21}',
                                                '{22}', 
                                                '{23}', 
                                                '{24}', 
                                                '{25}', 
                                            );",
                                            ordem.sucursal_origem,
                                            ordem.area_origem,
                                            ordem.numero_ordem,
                                            ordem.tipo_ordem,
                                            ordem.tipo_ordem,
                                            ordem.tipo_servico,
                                            ordem.numero_cliente,
                                            ordem.municipio,
                                            ordem.data_ingresso,
                                            ordem.estado,
                                            ordem.data_estado,
                                            ordem.etapa,
                                            ordem.data_etapa,
                                            ordem.tempo_def_tecnico,
                                            ordem.tempo_def_tec_etap,
                                            ordem.area_destino,
                                            ordem.sucursal_destino,
                                            ordem.regional_destino,
                                            ordem.observacoes,
                                            ordem.telefone_contato,
                                            ordem.rol_ingresso,
                                            ordem.corr_visita,
                                            ordem.data_cont_def_tec,
                                            ordem.numero_ordem_relac,
                                            ordem.motivo_ingresso,
                                            ordem.sucursal_original,
                                            ordem.referencia);
            return ExecutarSql(sql, conexao);
        }

        public bool AtualizarEstadoDaOrdemDeServico(OrdemServicoEstado novoEstado, OrdemServicoDTO ordemServico, DBProviderInformix conexao)
        {
            string sql = string.Format(@"update ordem_servico 
                                            set estado = '{0}',
                                            data_estado = current,
                                            corr_visita = '{1}' 
                                        where 
                                            numero_ordem = '{2}'
                                            and estado in ('00', '01');", 
                                            novoEstado,
                                            (ordemServico.corr_visita + 1),
                                            ordemServico.numero_ordem);


            return ExecutarSql(sql.ToString(), conexao);
        }

        public bool IngressarVisitaOrdem(OrdemServicoDTO ordemServico, DBProviderInformix conexao)
        {
            string sql = string.Format(@"select 
                                            data_etapa, 
                                            data_cont_def_tec,
                                            tempo_def_tec_etap
                                        from 
                                            ordem_servico
                                        where numero_ordem = '{0}'", ordemServico.numero_ordem);

            DataTable dataTable = ConsultaSql(sql);

            if (dataTable == null || dataTable.Rows.Count == 0)
                throw new Exception(string.Format(@"Não há Correlativo de Visita para a ordem '{0}'", ordemServico.numero_ordem));

            var dataDaEtapa = Convert.ToDateTime(dataTable.Rows[0][0]);
            var dataDeContatoDoDefeitoTecnico = Convert.ToDateTime(dataTable.Rows[0][1]);
            var tempoDeDefeitoTecnicoDaEtapa = Convert.ToInt32(dataTable.Rows[0][2]);

            if(dataDaEtapa < dataDeContatoDoDefeitoTecnico)
            {
                dataDaEtapa = dataDeContatoDoDefeitoTecnico;
            }

            sql = string.Format(@"Insert into visita_ordem(
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
                                  values (
                                        '{0}',
                                        '{1}', 
                                        '{2}', 
                                        'current', 
                                        '',
                                        '',
                                        '',
                                        '{3}'
                                        '', 
                                        '{4}',
                                        'S', 
                                        '{6}', 
                                        '{7}')",
                                        ordemServico.numero_ordem,
                                        ordemServico.etapa,
                                        ordemServico.corr_visita + 1,
                                        ordemServico.rol_ingresso,
                                        ordemServico.sucursal_origem,
                                        ordemServico.rol_ingresso,
                                        ordemServico.data_etapa);  
    
            return ExecutarSql(sql.ToString(), conexao);
        }

        public OrdemServicoDTO RetornoInfoOrdens(string nro_caso)
        {
            StringBuilder sql = new StringBuilder();

            sql.Append(@" SELECT v.cod_retorno, 
	                           v.eletricista, 
	                           v.data_exec_visita, 
	                           v.hora_exec_visita, 
	                           v.rol_ret_visita, 
	                           v.data_ret_visita,            
                               v.hora_fim_prevista, 
                               v.rol_responsavel, 
                               v.dat_lib_def_tec, 
                               v.rol_lib_def_tec, 
                               v.codigo_cargo,
                               v.valor_cargo, 
                               v.numero_ordem_filha, 
                               v.numero_form_venda, 
                               v.serie_form_venda, 
                               v.data_despacho,
                               v.numero_tarefa, 
                               v.data_inic_visita, 
                               v.periodo_agend, 
                               v.data_agend, 
                               v.turno_agend,
                               v.etapa,                   
                               o.nro_caso,
                               o.tipo_ordem,
                               o.tipo_servico
                        FROM visita_ordem v, ordem_servico o                    
                        WHERE v.numero_ordem = o.numero_ordem
                        AND v.etapa = o.etapa
                        AND v.corr_visita = o.corr_visita ");
            sql.Append(" and o.nro_caso = '" + nro_caso + "'");
            var dt = ConsultaSql(sql.ToString());

            if (dt.Rows.Count > 0)
                return DataHelper.ConvertDataTableToEntity<OrdemServicoDTO>(dt);
            else
                return null;

        }

        public bool ExecutaProcedureDespacharOrdem(AgendamentoDTO agendamento, Empresa empresa, DBProviderInformix informix)
        {
            var ordemServico = agendamento.ordemServico;
            var sqlProcedure = new StringBuilder();
            sqlProcedure.AppendFormat("EXECUTE PROCEDURE os005_despachaordem ('{0}', '{1}', to_date('{2}','%d-%m-%Y'),'{3}','{4}','','{5}','','{6}','S')",
                            ordemServico.numero_ordem, 
                            agendamento.Etapa,
                            agendamento.Dia.ToString("dd-MM-yyyy"),
                            agendamento.HoraInicio,
                            agendamento.HoraFim,
                            // empreiteira?,
                            ordemServico.area_destino, // ordemServico.area_origem ou ordemServico.codigo_area
                            ordemServico.rol_ingresso);

            DataTable dt = ConsultaSql(sqlProcedure.ToString(),informix);

            if (dt.Rows.Count > 0)
            {
                if ("OK".Equals(dt.Rows[0][0].ToString().Trim()))
                {
                    return true;
                }
                else
                    return false;
            }

            return false;
        }

        /// <summary>
        /// Atualiza a ordem de serviço, alterando seu estado, data e responsável pela alteração.
        /// </summary>
        /// <param name="numeroOrdemServico"></param>
        /// <param name="ordemServicoEstado">Novo estado que se deseja configurar para a ordem de serviço.</param>
        /// <param name="motivoCancelamento">Código do Motivo quando a ordem for de Cancelamento (tabla 'TIPCAN'). </param>
        /// <param name="loginRede">Rol responsável pela alteração.</param>
        /// <returns></returns>
        public bool AtualizarEstado(string numeroOrdemServico, OrdemServicoEstado ordemServicoEstado, string motivoCancelamento, string loginRede)
        {
            //--------------------------------------------------
            // se for necessário adicionar novos parâmetros, 
            // considerar a criação de uma objeto na assinatura.
            //--------------------------------------------------

            StringBuilder sql = new StringBuilder("update ordem_servico set ");
            sql.Append(" estado = '{0}' ");

            if(!string.IsNullOrEmpty(motivoCancelamento))
                sql.AppendFormat(", cod_mot_anulacao = '{0}' ", motivoCancelamento);

            sql.Append(@", rol_alteracao = '{1}'
                         , data_estado = current
                     where numero_ordem = '{2}'");

            return ExecutarSql(
                string.Format(sql.ToString()
                    , EnumString.GetStringValue(ordemServicoEstado)
                    , loginRede
                    , numeroOrdemServico)
            ) > 0;
        }


        public bool ExecutaProcedureSuspendeOrdem(AgendamentoDTO agendamento, string codigoDeRetorno, Empresa empresa, DBProviderInformix informix)
        {
            var ordem = agendamento.ordemServico;

            var sqlProcedure = new StringBuilder();

            sqlProcedure.AppendFormat("EXECUTE PROCEDURE os002_suspendordem ('{0}', '{1}','{2}','','',today,extend(current, hour to minute),'Agendamento')",
                            ordem.numero_ordem,
                            codigoDeRetorno,
                            ordem.rol_ingresso);

            DataTable dt = ConsultaSql(sqlProcedure.ToString(),informix);

            if (dt.Rows.Count > 0)
            {
                if ("OK".Equals(dt.Rows[0][0].ToString().Trim()))
                {
                    return true;
                }
                else
                    return false;
            }

            return false;
        }
        public string ConsultarCodigoRetorno(AgendamentoDTO agendamentoOrdem, DBProviderInformix informix ) 
        {
            string codigo_retorno = "";
            string sql = String.Format(@"select 
                                            codigo_retorno
                                        from 
                                            retorno_servico 
                                        where 
                                            tipo_ordem = '{0}' 
                                            and tipo_servico = '{1}' 
                                            and etapa = '{2}' 
                                            and ind_agendamento = 'S'",
                                            agendamentoOrdem.TipoOrdem,                                                                                                                  
                                            agendamentoOrdem.TipoServico,
                                            agendamentoOrdem.Etapa);

            var result = ConsultaSql(sql, informix);
            if(result.Rows.Count > 0) 
                codigo_retorno = result.Rows[0][0].ToString();

            return codigo_retorno;
        }

        public DataTable Consultar(OrdemServicoDTO ordemServico, List<string> tipoServicoIn, List<string> estadoNotIn)
        {
            string tipoServico = string.Join("','", tipoServicoIn.ToArray());
            tipoServico = tipoServico.Insert(0, "'").Insert(tipoServico.Length+1,"'");

            string estado  = string.Join("','", estadoNotIn.ToArray());
            estado = estado.Insert(0, "'").Insert(estado.Length+1,"'");

            Int32 numero_cliente = Int32.Parse(ordemServico.numero_cliente);

            string sql = String.Format(@"select
                                            numero_ordem
                                        from
                                            ordem_servico
                                        where
                                            tipo_ordem = '{0}' and
                                            tipo_servico in ({1}) and
                                            numero_cliente = {2}
                                            and estado not in ({3})",
                                            ordemServico.tipo_ordem,
                                            tipoServico,
                                            numero_cliente,
                                            estado);

            return ConsultaSql(sql);
        }

        public bool ExecutaProcedureIngressarOrdemAgrupada(string numeroDaOrdem)
        {
            string retornoDaProcedure = String.Empty;
            var sqlProcedure = new StringBuilder();

            sqlProcedure.AppendFormat("EXECUTE PROCEDURE ingressa_ordem_agrupada('{0}', '')", numeroDaOrdem);

            DataTable dataTable = ConsultaSql(sqlProcedure.ToString());

            if (dataTable.Rows.Count > 0)
            {
                retornoDaProcedure = dataTable.Rows[0][0].ToString().Trim();
                return (!"ERRO".Equals(retornoDaProcedure)) ? true : false;
            }
            return false;
        }
    }
}

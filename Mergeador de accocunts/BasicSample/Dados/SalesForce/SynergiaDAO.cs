using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IBM.Data.Informix;
using System.Globalization;
using System.Diagnostics;
using SalesforceExtractor.Dados.InformixBase;
using SalesforceExtractor.Entidades;
using SalesforceExtractor.Entidades.Enumeracoes;
using SalesforceExtractor.Utils;


namespace SalesforceExtractor.Dados.SalesForce
{
    public class SynergiaDAO
    {
        static DbInformix db = null;
        public SynergiaDAO(string connectionString)
        {
            db = new DbInformix(connectionString);
        }
        //FachadaGenerica m_Fachada = new FachadaGenerica(typeof(RelatorioFinalSF));

        //~SalesForceDAO()
        //{
        //    m_Fachada = null;
        //}

        //public SalesForceDAO()
        //{
        //    dbProviderInformix = ObterProviderInformix();
        //    dbProviderInformix.BeginTransacion();
        //    dbProviderInformix.Command.CommandTimeout = 3600;
        //}

        //private static DBProviderInformix dbProviderInformix;

        public IfxTransaction AbrirTransacao()
        {
            return db.AbrirTransacao();
        }

        public void FecharConexao()
        {
            db.FecharConexao();
        }

        public void criarTabTempClientesModificados(int qtdeDiasAnteriores, IfxTransaction transacao)
        {
            string sql = Properties.Resources.QuerySalesForceModif;
            sql = sql.Replace("parametroDias", qtdeDiasAnteriores.ToString());
            
            int qtdeReg = db.EnviarComando(sql, transacao);//m_Fachada.ExecutarSql(sql, transacao);

            //if (qtdeReg > 0)
            //{
            //    return true;
            //}
            //else
            //{
            //    return false;
            //}
            // return ExecutarSql(sql, dbProviderInformix);
        }


        public void criarTabTempClientesModificados(string data_inicial, string data_final, IfxTransaction transacao)
        {
            string sql = Properties.Resources.QuerySalesForceModif;

            string data_ini_formatada = DateTime.Parse(data_inicial, CultureInfo.GetCultureInfo("pt-BR")).ToString("MM/dd/yyyy");

            string data_fim_formatada = DateTime.Parse(data_final, CultureInfo.GetCultureInfo("pt-BR")).ToString("MM/dd/yyyy");

            string formato = " between '" + data_ini_formatada + "' and '" + data_fim_formatada + "'";

            sql = sql.Replace(">= today - parametroDias", formato);

            int qtdeReg = db.EnviarComando(sql, transacao);
        }

        public string[] consultarClientesSynergiaB(IfxTransaction transacao)
        {
            string sql = Properties.Resources.QuerySalesForceB;
            
            if (System.Configuration.ConfigurationSettings.AppSettings["ConnectionStringCoelcePro"].Contains("10.14.32.72") ||
                 System.Configuration.ConfigurationSettings.AppSettings["ConnectionStringCoelcePro"].Contains("10.14.45.45"))
            {
                sql = sql.Replace(@"when nvl(c.celular, '') <> '' then c.celular", "");
            }

            // transacao = db.AbrirTransacao();

            DataTable dt = db.RetornaDataTable(sql, transacao);//m_Fachada.ConsultarSql(sql, transacao).Tables[0];//, ConsultaSqlReader(sql, true, dbProviderInformix);

            string[] dadosSplit = new string[dt.Rows.Count];
            int contaLinha = 0;

            foreach (DataRow dr in dt.Rows)
            {
                foreach (DataColumn col in dt.Columns)
                {
                    // dr[col] = dr[col].ToString().Trim();

                    if (col.DataType == typeof(string))
                    {
                        object o = dr[col];
                        if (!Convert.IsDBNull(o) && o != null)
                        {
                            dr[col] = o.ToString().Trim();
                        }
                    }
                }

                dadosSplit[contaLinha] = string.Join("|", dt.Rows[contaLinha].ItemArray);
                contaLinha++;
            }
            return dadosSplit;
        }


        /// <summary>
        /// Executa uma consulta parametrizada na base do Synergia.
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="transacao"></param>
        /// <returns></returns>
        public DataTable ExecutarConsultaSynergia(string sql, IfxTransaction transacao)
        {
            try
            {
                return db.RetornaDataTable(sql, transacao);//m_Fachada.ConsultarSql(sql, transacao).Tables[0];//, ConsultaSqlReader(sql, true, dbProviderInformix);
            }
            catch(Exception ex)
            {
                Debugger.Break();
                throw ex;
            }
        }


        public string[] consultarClientesSynergiaA(int qtdeDiasAnteriores, IfxTransaction transacao)
        {
            string sql = Properties.Resources.QuerySalesForceA;
            
            if (System.Configuration.ConfigurationSettings.AppSettings["ConnectionStringCoelcePro"].Contains("10.14.32.72") ||
                System.Configuration.ConfigurationSettings.AppSettings["ConnectionStringCoelcePro"].Contains("10.14.45.45"))
            {
                sql = sql.Replace(@"c.ddd_celular||c.celular", "''");
            }

            sql = sql.Replace("parametroDias", qtdeDiasAnteriores.ToString());

            DataTable dt = db.RetornaDataTable(sql, transacao);

            string[] dadosSplit = new string[dt.Rows.Count];
            int contaLinha = 0;

            foreach (DataRow dr in dt.Rows)
            {
                foreach (DataColumn col in dt.Columns)
                {
                    // dr[col] = dr[col].ToString().Trim();

                    if (col.DataType == typeof(string))
                    {
                        object o = dr[col];
                        if (!Convert.IsDBNull(o) && o != null)
                        {
                            dr[col] = o.ToString().Trim();
                        }
                    }
                }

                dadosSplit[contaLinha] = string.Join("|", dt.Rows[contaLinha].ItemArray);
                contaLinha++;
            }
            return dadosSplit;
        }


        public IEnumerable<string> consultarClientesSynergiaA(string data_inicial, string data_final, IfxTransaction transacao)
        {
            string sql = Properties.Resources.QuerySalesForceA;

            if (System.Configuration.ConfigurationSettings.AppSettings["ConnectionStringCoelcePro"].Contains("10.14.32.72") ||
                System.Configuration.ConfigurationSettings.AppSettings["ConnectionStringCoelcePro"].Contains("10.14.45.45"))
            {
                sql = sql.Replace(@"c.ddd_celular||c.celular", "''");
            }

            string data_ini_formatada = DateTime.Parse(data_inicial, CultureInfo.GetCultureInfo("pt-BR")).ToString("MM/dd/yyyy");

            string data_fim_formatada = DateTime.Parse(data_final, CultureInfo.GetCultureInfo("pt-BR")).ToString("MM/dd/yyyy");

            string formato = " < '" + data_ini_formatada + "'";

            sql = sql.Replace("= today - parametroDias", formato);

            DataTable dt = db.RetornaDataTable(sql, transacao);

            string[] dadosSplit = new string[dt.Rows.Count];
            int contaLinha = 0;

            foreach (DataRow dr in dt.Rows)
            {
                foreach (DataColumn col in dt.Columns)
                {                   
                    if (col.DataType == typeof(string))
                    {
                        object o = dr[col];
                        if (!Convert.IsDBNull(o) && o != null)
                        {
                            dr[col] = o.ToString().Trim();
                        }
                    }
                }

                dadosSplit[contaLinha] = string.Join("|", dt.Rows[contaLinha].ItemArray);
                contaLinha++;
            }
            return dadosSplit;
        }

        /// <summary>
        /// Romulo Silva - 24/03/2017
        /// Consulta Street's armazenados na base de dados
        /// </summary>
        /// <param name="rows">Registro com dados do Synergia</param>
        /// <param name="consultaId">Identifica se deve consultar somente pelo ID, ou consulta todos os valores de street</param>
        /// <returns></returns>
        public string consultarIdStreet(string[] rows, IfxTransaction transacao)
        {
            string idStreet = string.Empty;
            string sql = string.Empty;
            DataTable dt = null;

                sql = @"SELECT id_rua FROM streets_sf 
                        WHERE nome_rua = '" + rows[(int)DicColuna.col1_7.nombre_calle] +
                        "' and tipo_logradouro = '" + rows[(int)DicColuna.col1_7.tipo_calle] +
                        "' and cidade = '" + rows[(int)DicColuna.col1_7.ciudad] +
                        "' and UF = '" + rows[(int)DicColuna.col1_7.uf] +
                        "' and comuna = '" + rows[(int)DicColuna.col1_7.comuna] +
                        "' and regiao = '" + rows[(int)DicColuna.col1_7.region] +
                        "' and localidad = '" + rows[(int)DicColuna.col1_7.localidad] +
                        "' and nvl(bairro,'') = '" + rows[(int)DicColuna.col1_7.barrio] + "'";

                dt = db.RetornaDataTable(sql, transacao);//m_Fachada.ConsultarSql(sql, null).Tables[0];//, ConsultaSqlReader(sql, true, dbProviderInformix);

                if (dt.Rows.Count > 0)
                {
                    idStreet = dt.Rows[0][0].ToString().Trim();
                }
                else
                {
                    sql = @"SELECT id_rua FROM streets_sf
                            WHERE id_rua = '" + rows[(int)DicColuna.col1_7.identificador_street] + "'";


                    dt = db.RetornaDataTable(sql, transacao);//m_Fachada.ConsultarSql(sql, null).Tables[0];

                    if (dt.Rows.Count > 0)
                    {
                        sql = @"SELECT MAX(id_rua::bigint) + 1 FROM streets_sf";
                        dt = db.RetornaDataTable(sql, transacao);//m_Fachada.ConsultarSql(sql, null).Tables[0];

                        idStreet = dt.Rows[0][0].ToString().Trim();
                        rows[(int)DicColuna.col1_7.identificador_street] = idStreet;
                        this.inserirIdStreet(rows, transacao);
                    }
                    else
                    {
                        idStreet = rows[(int)DicColuna.col1_7.identificador_street].Trim();
                        rows[(int)DicColuna.col1_7.identificador_street] = idStreet;
                        this.inserirIdStreet(rows, transacao);
                    }
                }

                return idStreet;
        }

        public void inserirIdStreet(string[] rows, IfxTransaction transacao)
        {
            StringBuilder sql = new StringBuilder();

            sql.Append("INSERT INTO streets_sf(");
            sql.Append("id_rua,");
            sql.Append("nome_rua,");
            sql.Append("tipo_logradouro,");
            sql.Append("cidade,");
            sql.Append("UF,");
            sql.Append("pais,");
            sql.Append("comuna,");
            sql.Append("regiao,");
            sql.Append("calle,");
            sql.Append("localidad,");
            sql.Append("bairro");
            sql.AppendLine(") VALUES (");
            sql.Append("'" + rows[(int)DicColuna.col1_7.identificador_street] + "',");
            sql.Append("'" + rows[(int)DicColuna.col1_7.nombre_calle] + "',");
            sql.Append("'" + rows[(int)DicColuna.col1_7.tipo_calle] + "',");
            sql.Append("'" + rows[(int)DicColuna.col1_7.ciudad] + "',");
            sql.Append("'" + rows[(int)DicColuna.col1_7.uf] + "',");
            sql.Append("'" + rows[(int)DicColuna.col1_7.pais] + "',");
            sql.Append("'" + rows[(int)DicColuna.col1_7.comuna] + "',");
            sql.Append("'" + rows[(int)DicColuna.col1_7.region] + "',");
            sql.Append("'" + rows[(int)DicColuna.col1_7.calle] + "',");
            sql.Append("'" + rows[(int)DicColuna.col1_7.localidad] + "',");
            sql.Append("'" + rows[(int)DicColuna.col1_7.barrio] + "');");

            // return sql.ToString();
            //Util.EscreverArquivo("sql", sql.ToString(), ".txt");

            db.EnviarComando(sql.ToString(), transacao);//m_Fachada.ExecutarSql(sql.ToString(), transacao);


            //ExecutarSql(sql.ToString(), dbProviderInformix);
        }

        public Dictionary<string, int> consultaRelatorioModif(IfxTransaction transacao)
        {
            string sql = Properties.Resources.QuerySalesForceRelModif;
            DataTable dt = db.RetornaDataTable(sql, transacao);//m_Fachada.ConsultarSql(sql, null).Tables[0];

            Dictionary<string, int> relatorioModif = new Dictionary<string, int>();
            foreach (DataRow dr in dt.Rows)
            {
                relatorioModif.Add(dr[1].ToString(), Convert.ToInt32(dr[0].ToString()));
            }
            
            return relatorioModif;
        }

        internal string consultarIdConta(string[] rows, bool consultaId, IfxTransaction transacao)
        {
            string idConta = string.Empty;
            string sql = string.Empty;
            DataTable dt = null;

            if (!consultaId)
            {
                sql = @"SELECT id_conta FROM account_sf 
                        WHERE numero_cliente = '" + rows[(int)DicColuna.col1_7.cuenta_principal].Trim() + "'";

                dt = db.RetornaDataTable(sql, transacao);

                if (dt.Rows.Count.Equals(1))
                {
                    idConta = dt.Rows[0][0].ToString().Trim();
                }
            }
            else
            {
                sql = @"SELECT id_conta FROM account_sf
                        WHERE id_conta = '" + rows[(int)DicColuna.col1_7.identificador_conta].Trim() + "'";

                dt = db.RetornaDataTable(sql, transacao);

                if (dt.Rows.Count > 0)
                {
                    sql = @"SELECT MAX(id_rua)+1 FROM identificador_conta";
                    dt = db.RetornaDataTable(sql, transacao);

                    idConta = dt.Rows[0][0].ToString().Trim();
                }
                else
                {
                    idConta = rows[(int)DicColuna.col1_7.identificador_conta].Trim();
                }
            }
            return idConta;
        }

        public void inserirIdConta(string[] rows, IfxTransaction transacao)
        {
            StringBuilder sql = new StringBuilder();

            sql.Append("INSERT INTO account_sf(");
            sql.Append("id_conta,");
            sql.Append("numero_cliente) VALUES (");
            sql.Append("'" + rows[(int)DicColuna.col1_7.identificador_conta] + "',");
            sql.Append("'" + rows[(int)DicColuna.col1_7.cuenta_principal] + "');");


            Util.EscreverArquivo("sql", sql.ToString(), ".txt");

            db.EnviarComando(sql.ToString(), transacao);//m_Fachada.ExecutarSql(sql.ToString(), transacao);

            //ExecutarSql(sql.ToString(), dbProviderInformix);
        }

        public int executarComando(string sql, IfxTransaction transacao)
        {
            int t = db.EnviarComando(sql.ToString(), transacao);
            return t;
        }

        internal Dictionary<long, Street> consultarStreet(IfxTransaction transacao)
        {
            string sql = "SELECT * FROM streets_sf";


            db.RetornaDataTableStreet(sql, transacao);

            DataTable dt = db.RetornaDataTable(sql, transacao);

            Dictionary<long, Street> dicStreet = new Dictionary<long, Street>();

            foreach (DataRow dr in dt.Rows)
            {
                Street street = new Street();

                street.id_rua = Convert.ToInt64(dr["id_rua"].ToString().Trim());
                street.nome_rua = dr["nome_rua"].ToString().Trim();
                street.tipo_logradouro = dr["tipo_logradouro"].ToString().Trim();
                street.cidade = dr["cidade"].ToString().Trim();
                street.uf = dr["uf"].ToString().Trim();
                street.pais = dr["pais"].ToString().Trim();
                street.comuna = dr["comuna"].ToString().Trim();
                street.regiao = dr["regiao"].ToString().Trim();
                street.calle = dr["calle"].ToString().Trim();
                street.localidad = dr["localidad"].ToString().Trim();
                street.bairro = dr["bairro"].ToString().Trim();
                street.data_inclusao = Convert.ToDateTime(dr["data_inclusao"].ToString().Trim());

                if (!dicStreet.ContainsKey(street.id_rua))
                {
                    dicStreet.Add(street.id_rua, street);
                }
            }
            return dicStreet;
        }



        internal Boolean GetExternalIds(IfxTransaction transacao, string numero_cliente, out string externalId_conta, out string externalId_pod)
        {
            StringBuilder sql = new StringBuilder(string.Format(
                         @"SELECT distinct numero_cliente, ExternalId_conta , externalid_pod 
                             FROM sales_geral 
                            WHERE numero_cliente = {0}", numero_cliente));
            externalId_conta = string.Empty;
            externalId_pod = string.Empty;

            DataTable dt = db.RetornaDataTable(sql.ToString(), transacao);
            foreach (DataRow dr in dt.Rows)
            {
                externalId_conta = DBNull.Value.Equals(dr["externalid_conta"]) ? string.Empty : dr["externalid_conta"].ToString().Trim();
                externalId_pod = DBNull.Value.Equals(dr["externalid_pod"]) ? string.Empty : dr["externalid_pod"].ToString().Trim();

                return true;
            }
            return false;
        }


        /// <summary>
        /// Recupera os dados bancários do último cadastro de débito automático.
        /// </summary>
        /// <param name="numeroCliente"></param>
        /// <returns></returns>
        internal DebitoAutomatico ObterDadosBancariosDebitoAutomatico(string numeroCliente)
        {
            DebitoAutomatico result = new DebitoAutomatico();
            string sql = string.Format(@"select c.numero_cliente, c.rut || c.dv_rut ident_cliente, cod_entidad, codigo_agencia, conta_corrente, m.cpf || m.dv_cpf ident, c.tipo_ident
                                           from maeaut m, cliente c
                                          where c.numero_cliente = m.numero_cliente
                                            --and m.fecha_desactivac IS NULL
                                            and c.numero_cliente = {0}
                                       order by fecha_activacion DESC", numeroCliente);

            try
            {
                DataTable dt = db.RetornaDataTable(sql, null);

                if (DBNull.Value != dt.Rows[0]["cod_entidad"])
                    result.CodigoBanco = Convert.ToInt32(dt.Rows[0]["cod_entidad"]).ToString();

                if (DBNull.Value != dt.Rows[0]["codigo_agencia"])
                    result.Agencia = Convert.ToInt32(dt.Rows[0]["codigo_agencia"]).ToString();

                if (DBNull.Value != dt.Rows[0]["conta_corrente"])
                    result.ContaCorrente = Convert.ToInt32(dt.Rows[0]["conta_corrente"]).ToString();
                
                if (DBNull.Value != dt.Rows[0]["ident_cliente"])
                    result.Documento = dt.Rows[0]["ident_cliente"].ToString().Trim();
                
                if (DBNull.Value != dt.Rows[0]["tipo_ident"])
                    result.TipoDocumento = dt.Rows[0]["tipo_ident"].ToString().Trim();

                return result;
            }
            catch
            {
                return null;
            }
        }



        internal DataTable ConsultarAtendimentosComercial(DateTime data)
        {
            DataTable result = new DataTable();
            string sql = string.Format(@"select Data, Hora, Minuto, Canal, te_desc_mot_empres, Mais48h, sum(total) as Total
                                            from (
	                                            select to_char(co_fecha_inicio, '%d/%m/%Y') data, to_char(co_fecha_inicio, '%H') hora, CASE WHEN to_char(co_fecha_inicio, '%M') <= '30' THEN '00' ELSE '30' END as minuto, co_cod_medio as codcanal
							                                            , nvl((select trim(tm_desc_medio) from ct_tab_medio where tm_cod_medio = co_cod_medio), 'Indefinido')
								                                            as CANAL
						                                                , trim(te_desc_mot_empres) AS te_desc_mot_empres
						                                                , (co_fecha_inicio < (current - NUMTODSINTERVAL(2, 'DAY'))) AS mais48h
						                                                , count(*) as total
						                                            from ct_contacto, ct_motivo, ct_tab_mot_empresa
						                                            where 1=1
						                                                and date(co_fecha_inicio) = '{0}'
						                                                and co_numero = mo_co_numero
						                                                and te_cod_motivo = mo_cod_motivo 
						                                                and te_cod_mot_empresa = mo_cod_mot_empresa 
						                                                and (mo_cod_submotivo is null OR nvl(trim(mo_cod_submotivo), '') = '')
						                                            group by data, hora, minuto,  codcanal, canal, te_desc_mot_empres, mais48h

						                                            UNION

						                                            select to_char(co_fecha_inicio, '%d/%m/%Y') data, to_char(co_fecha_inicio, '%H') hora, CASE WHEN to_char(co_fecha_inicio, '%M') <= '30' THEN '00' ELSE '30' END as minuto, co_cod_medio as codcanal
							                                            , nvl((select trim(tm_desc_medio) from ct_tab_medio where tm_cod_medio = co_cod_medio), 'Indefinido')
								                                            as CANAL
						                                                , trim(te_desc_mot_empres) AS te_desc_mot_empres
						                                                , (co_fecha_inicio < (current - NUMTODSINTERVAL(2, 'DAY'))) AS mais48h
						                                                , count(*) as total
						                                            from ct_contacto, ct_motivo, ct_tab_mot_empresa
						                                            where 1=1
						                                                and date(co_fecha_inicio) = '{0}'
						                                                and co_numero = mo_co_numero
						                                                and te_cod_motivo = mo_cod_motivo 
						                                                and te_cod_mot_empresa = mo_cod_mot_empresa 
						                                                and trim(te_cod_submotivo) = trim(mo_cod_submotivo)
						                                                and trim(mo_cod_submotivo) <> ''
						                                            group by data, hora, minuto, codcanal, canal, te_desc_mot_empres, mais48h

						                                            UNION	------------------ FECHADO

						                                            select to_char(cf_fecha_inicio, '%d/%m/%Y') data, to_char(cf_fecha_inicio, '%H') hora, CASE WHEN to_char(cf_fecha_inicio, '%M') <= '30' THEN '00' ELSE '30' END as minuto, cf_cod_medio as codcanal
							                                            , nvl((select trim(tm_desc_medio) from ct_tab_medio where tm_cod_medio = cf_cod_medio), 'Indefinido')
								                                            as CANAL
						                                                , trim(te_desc_mot_empres) AS te_desc_mot_empres
						                                                , (cf_fecha_inicio < (current - NUMTODSINTERVAL(2, 'DAY'))) AS mais48h
						                                                , count(*) as total
						                                            from ct_contacto_final , ct_motivo_final, ct_tab_mot_empresa
						                                            where 1=1
						                                                and date(cf_fecha_inicio) = '{0}'
						                                                and cf_numero = mf_co_numero
						                                                and te_cod_motivo = mf_cod_motivo 
						                                                and te_cod_mot_empresa = mf_cod_mot_empresa 
						                                                and (mf_cod_submotivo is null OR nvl(trim(mf_cod_submotivo), '') = '')
						                                            group by data, hora, minuto, codcanal, canal, te_desc_mot_empres, mais48h

						                                            UNION

						                                            select to_char(cf_fecha_inicio, '%d/%m/%Y') data, to_char(cf_fecha_inicio, '%H') hora, CASE WHEN to_char(cf_fecha_inicio, '%M') <= '30' THEN '00' ELSE '30' END as minuto, cf_cod_medio as codcanal
							                                            , nvl((select trim(tm_desc_medio) from ct_tab_medio where tm_cod_medio = cf_cod_medio), 'Indefinido')
								                                            as CANAL
						                                                , trim(te_desc_mot_empres) AS te_desc_mot_empres
						                                                , (cf_fecha_inicio > current) AS mais48h
						                                                , count(*) as total
						                                            from ct_contacto_final , ct_motivo_final, ct_tab_mot_empresa
						                                            where 1=1
						                                                and date(cf_fecha_inicio) = '{0}'
						                                                and cf_numero = mf_co_numero
						                                                and te_cod_motivo = mf_cod_motivo 
						                                                and te_cod_mot_empresa = mf_cod_mot_empresa 
						                                                and trim(te_cod_submotivo) = trim(mf_cod_submotivo)
						                                                and trim(mf_cod_submotivo) <> ''
						                                            group by data, hora, minuto, codcanal, canal, te_desc_mot_empres, mais48h
	                                            )
                                            group by data, hora, minuto, codcanal, canal, te_desc_mot_empres, mais48h 
                                            order by data, hora, minuto, canal, te_desc_mot_empres", data.ToString("MM/dd/yyyy"));
            try
            {
                return db.RetornaDataTable(sql, null);
            }
            catch
            {
                return null;
            }
        }


        internal List<string> ConsultarClientesNovasLigacoes(DateTime dataInicio, DateTime dataFim, bool comNumeroCaso)
        {
            List<string> result = new List<string>();
            string sql = string.Format(@"select os.numero_cliente
                                           from ordem_servico os, outer sales_geral sg
                                          where data_estado between
                                                    to_date('{0}', '%d/%m/%Y') and
                                                    to_date('{1}', '%d/%m/%Y')    
                                                 and estado = '06'
                                                 and nro_caso is {2} null
                                                 and sg.numero_cliente = os.numero_cliente"
                , dataInicio.ToString("dd/MM/yyyy")
                , dataFim.ToString("dd/MM/yyyy")
                , comNumeroCaso ? "NOT" : string.Empty);
            try
            {
                DataTable dt = db.RetornaDataTable(sql, null);
                foreach(DataRow l in dt.Rows)
                {
                    result.Add(l[0].ToString());
                }
            }
            catch
            {
                return null;
            }
            return result;
        }


        /// <summary>
        /// Retorna toda a tabela ACCOUNT_SF
        /// </summary>
        /// <returns></returns>
        internal Dictionary<string, string> GetContas()
        {
            string sql = "SELECT numero_cliente, id_conta FROM account_sf";

            Dictionary<string, string> dicContas = new Dictionary<string, string>(); 
            DataTable dtContas = db.RetornaDataTable(sql, null);
            
            if(dtContas == null || dtContas.Rows.Count <= 0)
                return dicContas;

            return dtContas.AsEnumerable().ToDictionary(row => row.Field<string>(0),row => row.Field<string>(1));
        }


        public AltaContratacaoDAO GetInstanceAltaContratacaoDAO(TipoCliente tipoCliente)
        {
            if (TipoCliente.GA.Equals(tipoCliente))
                return new AltaContratacaoGADAO();

            if (TipoCliente.GB.Equals(tipoCliente))
                return new AltaContratacaoGBDAO();

            throw new ArgumentException("Parâmetro TipoCliente inválido para esta operação.");
        }


        internal List<ItemAttribute> ObterItemsAttribute(TipoCliente tipoCliente, int lote, Type tipoItem, List<string> clientes)
        {
            AltaContratacaoDAO altaDao = GetInstanceAltaContratacaoDAO(tipoCliente);
            string sql = altaDao.GetConsultaB2Win();
            string strClientes = string.Join(",", clientes.ToArray());
            //merge da consulta com a lista de clientes
            sql = sql.Replace("{0}", strClientes);
            sql = sql.Replace("{1}", lote.ToString());

            List<ItemAttribute> result = new List<ItemAttribute>();
            DataTable dtContas = db.RetornaDataTable(sql, null);

            if (dtContas == null || dtContas.Rows.Count <= 0)
                return result;

            ItemAttribute asset;
            int carga = 15;
            int potencia = 15;
            foreach (DataRow item in dtContas.Rows)
            {
                asset = new ItemAttribute();
                asset = TipoCliente.GB == tipoCliente ? new ItemAttributeGB() : new ItemAttribute();

                asset.ExternalIdAsset = typeof(ItemAttribute).Equals(tipoItem) ? item[19].ToString().Trim() : item[0].ToString().Trim();
                Int32.TryParse(item[1].ToString(), NumberStyles.AllowDecimalPoint, CultureInfo.GetCultureInfo("en-US"), out carga);
                asset.CargaKW = carga < 1 ? 1 : carga;
                asset.ModalidadeTarifaria = item[2].ToString();     //GA
                asset.CategoriaTarifa = item[3].ToString();
                asset.Classe = item[4].ToString().Trim();
                asset.SubClasse = item[5].ToString().Trim();
                asset.DemandaKV = item[6].ToString();       //GA--
                asset.DemandaPonta = item[7].ToString();       //GA
                asset.DemandaForaPonta = item[8].ToString();       //GA
                asset.CapacidadeDisjuntor = item[9].ToString();
                asset.ValorTensao = string.IsNullOrWhiteSpace(item[10].ToString()) ? "380   VOLTS" : item[10].ToString();
                asset.InstalacaoPadrao = item[11].ToString();
                asset.TipoTensao = item[12].ToString();
                Int32.TryParse(item[13].ToString(), out potencia);
                asset.PotenciaKWA = potencia < 1 ? "1" : potencia.ToString();
                asset.NivelTensao = item[14].ToString();
                asset.ExternalIdPod = item[18].ToString().Trim();
                asset.NumeroCliente = item[20].ToString();
                asset.OrderItemId = dtContas.Columns.Count > 21 ? DBNull.Value.Equals(item[21]) ? string.Empty : item[21].ToString() : string.Empty;

                result.Add(asset);
            }

            return result;
        }
    }
}

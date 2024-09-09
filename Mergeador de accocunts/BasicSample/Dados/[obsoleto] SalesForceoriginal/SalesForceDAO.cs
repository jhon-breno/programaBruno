using ExtracaoSalesForce;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IBM.Data.Informix;
using ExtracaoSalesForce.Modelo.InformixBase;
using System.Globalization;
using System.Diagnostics;


namespace ExtracaoSalesForce.Modelo.SalesForce
{
    public class SalesForceDAO
    {
        static DbInformix db = null;
        public SalesForceDAO(string connectionString)
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

        public void criarTabTempClientesModificados(string qtdeDiasAnteriores, IfxTransaction transacao)
        {
            string sql = Properties.Resources.QuerySalesForceModif;
            sql = sql.Replace("parametroDias", qtdeDiasAnteriores);

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


        public string[] consultarClientesSynergiaA(string qtdeDiasAnteriores, IfxTransaction transacao)
        {
            string sql = Properties.Resources.QuerySalesForceA;
            
            if (System.Configuration.ConfigurationSettings.AppSettings["ConnectionStringCoelcePro"].Contains("10.14.32.72") ||
                System.Configuration.ConfigurationSettings.AppSettings["ConnectionStringCoelcePro"].Contains("10.14.45.45"))
            {
                sql = sql.Replace(@"c.ddd_celular||c.celular", "''");
            }

            sql = sql.Replace("parametroDias", qtdeDiasAnteriores);

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

            // DataTable dt = ConsultaSql(sql, dbProviderInformix);

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

        internal Boolean consultarConta(IfxTransaction transacao, string numero_cliente, out string id_conta)
        {
            string sql = "SELECT distinct numero_cliente,id_conta FROM account_sf where numero_cliente=" + numero_cliente;
            id_conta = "";
            DataTable dt = db.RetornaDataTable(sql, transacao);

            Dictionary<long, string> dicConta = new Dictionary<long, string>();

            foreach (DataRow dr in dt.Rows)
            {
                id_conta = dr["id_conta"].ToString().Trim();

                return true;
            }
            return false;
        }
       
    }
}

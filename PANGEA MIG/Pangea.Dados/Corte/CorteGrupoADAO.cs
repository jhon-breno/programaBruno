using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using Pangea.Dados.Base;
using Pangea.Entidades.Enumeracao;
using System.Web.Script.Serialization;
using Pangea.Entidades;

namespace Pangea.Dados.Corte
{
    public abstract class CorteGrupoADAO : BaseDAO
    {

        public CorteGrupoADAO(Empresa empresa)
            : base(empresa)
        {
        }


        #region CLIENTES GRUPO A

        /// <summary>
        /// Método responsável por devolver o motivo do corte
        /// </summary>
        /// <param name="numeroCliente"></param>
        /// <returns>String</returns>
        public string ObterMotivoDoCorteAtualGA(int numeroCliente)
        {

            string sql = gerarSQLObterMotivoDoCorteAtualGA(numeroCliente);
            DataTable dtResultado = ConsultaSql(sql);

            string motivo = "";

            foreach (DataRow item in dtResultado.Rows)
                motivo = TratarString(dtResultado, item, "motivo_corte");

            return motivo;

        }

        public Entidades.SalesForce.Corte MotivoCorteAtualGASalesForce(int numeroCliente)
        {
            List<Entidades.SalesForce.Corte> obj = new List<Entidades.SalesForce.Corte>();

            string sql = gerarSQLObterMotivoDoCorteAtualSalesForceGA(numeroCliente);
            DataTable dtResultado = ConsultaSql(sql);

            string formatado = DataTableToJSON(dtResultado);

            obj = new JavaScriptSerializer().Deserialize<List<Entidades.SalesForce.Corte>>(formatado);

            if (obj.Count == 0)
            {
                Entidades.SalesForce.Corte obj_erro = new Entidades.SalesForce.Corte();

                obj_erro.CodigoError = "ERROR003";
                obj_erro.MensajeError = "Cliente não possui corte ativo.";

                return obj_erro;
            }
            else
            {
                return obj.First();
            }
        }

        public IList<Entidades.CorteGrupoA> VerificarUltimoCorteDesligamentoPedidoGA(int numeroCliente)
        {
            try
            {
                string sql = gerarSQLVerificarUltimoCorteDesligamentoPedidoGA(numeroCliente);
                DataTable dtResultado = ConsultaSql(sql);

                return dtToListObject<Entidades.CorteGrupoA>(dtResultado);

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// Método responsável por criar a query de consulta relacionada aos cortes de desligamento do cliente
        /// </summary>
        /// <param name="numeroCliente"></param>
        /// <returns>String</returns>
        private string gerarSQLVerificarUltimoCorteDesligamentoPedidoGA(int numeroCliente)
        {
            var sql = new StringBuilder();

            sql.Append(" SELECT  ");
            sql.Append(" co.tipo_corte         AS tipo_corte,  ");
            sql.Append(" co.num_ordem_serv_crt AS numero_ordem, ");
            sql.Append(" co.acc_realizada_cor  AS acc_realizada_cor , ");
            sql.Append(" co.numero_ordem AS numero_solicitacao_corte, ");
            sql.Append(" co.motivo_corte AS motivo_corte, ");
            sql.Append(" t.descripcion AS descricao_motivo ");
            sql.Append(" from GRANDES:cliente c, GRANDES:correp co, GRANDES:tabla t ");
            sql.Append(" where c.estado_cliente = 8 ");
            sql.Append(" and c.estado_suministro = 1 ");
            sql.Append(" and c.numero_cliente = co.numero_cliente ");
            sql.Append(" AND t.nomtabla = 'CORMOT' ");
            sql.Append(" AND t.sucursal = '0000' ");
            sql.Append(" AND t.codigo = co.motivo_corte ");
            sql.Append(" and c.corr_corte = ");
            sql.Append(" (select max(corr_corte) ");
            sql.Append(" from GRANDES:correp ");
            sql.Append(" where numero_cliente = c.numero_cliente) ");
            sql.Append(" and c.corr_corte = co.corr_corte");
            sql.Append(" and co.motivo_corte in ('08') ");
            sql.Append(" and co.numero_cliente = " + numeroCliente);

            return sql.ToString();
        }

        public IList<TEntidade> dtToListObject<TEntidade>(System.Data.DataTable dt)
        {
            IList<Entidades.CorteGrupoA> result = new List<Entidades.CorteGrupoA>();

            foreach (DataRow item in dt.Rows)
            {
                Entidades.CorteGrupoA tempCorte = new Entidades.CorteGrupoA();

                tempCorte.NumeroCliente = TratarInt(dt, item, "NUMERO_CLIENTE", -1);
                tempCorte.corr_corte = TratarInt(dt, item, "CORR_CORTE", -1);
                tempCorte.Tipo = TratarString(dt, item, "TIPO_CORTE");
                tempCorte.DataDaExecucao = TratarString(dt, item, "DATA_EXECUCAO");
                tempCorte.Funcionario = TratarString(dt, item, "FUNCIONARIO");
                tempCorte.motivo_corte = TratarString(dt, item, "MOTIVO_CORTE");
                tempCorte.NumeroOrdem = TratarString(dt, item, "NUMERO_ORDEM");
                tempCorte.AcaoRealizada = TratarString(dt, item, "ACC_REALIZADA_COR");


                //tempCorte.DataDeSolicitacao = TratarString(dt, item, "DATA_EMISSAO");
                //tempCorte.Hora = TratarString(dt, item, "HORA_CORTE");
                //tempCorte.Situacao = TratarString(dt, item, "SITUACAO_CORTE");


                result.Add(tempCorte);
            }

            return result as IList<TEntidade>;
        }

        #endregion


        public bool AtualizarCorrelativoCorteGA(int numeroCliente, string ultimoCorrelativoCorte)
        {
            bool result = false;

            StringBuilder sqlCorrep = new StringBuilder();
            sqlCorrep.Append(" Select count(*) as qtd from grandes:correp  ");
            sqlCorrep.Append(" where numero_cliente =" + numeroCliente);
            sqlCorrep.Append(" and corr_corte =" + ultimoCorrelativoCorte);
            sqlCorrep.Append(" and ( fecha_reposicion is not null or fecha_reposicion != '')");

            bool possuiCorrep = false;

            DataTable dtCorrep = ConsultaSql(sqlCorrep.ToString());

            if (dtCorrep.Rows.Count > 0)
            {
                int resultQtdCorrep = 0;
                int.TryParse(dtCorrep.Rows[0]["qtd"].ToString(), out resultQtdCorrep);
                possuiCorrep = resultQtdCorrep > 0;
            }

            if (!possuiCorrep)
            {
                StringBuilder sqlCorsore = new StringBuilder();
                sqlCorsore.Append(" Select count(*) as qtd from grandes:corsore  ");
                sqlCorsore.Append(" where numero_cliente =" + numeroCliente);
                sqlCorsore.Append(" and corr_corte =" + ultimoCorrelativoCorte);

                bool possuiCorsore = false;

                DataTable dtCorsore = ConsultaSql(sqlCorsore.ToString());

                if (dtCorsore.Rows.Count > 0)
                {
                    int resultQtdCorsore = 0;
                    int.TryParse(dtCorsore.Rows[0]["qtd"].ToString(), out resultQtdCorsore);
                    possuiCorsore = resultQtdCorsore > 0;

                    if (possuiCorsore)
                    {
                        DBProviderInformix informix = ObterProviderInformix();
                        informix.BeginTransacion();

                        StringBuilder sqlUpdateCorrelativo = new StringBuilder();
                        sqlUpdateCorrelativo.Append(" update grandes:cliente set corr_corte = corr_corte + 1 Where numero_cliente =" + numeroCliente);

                        result = ExecutarSql(sqlUpdateCorrelativo.ToString(), informix);

                        if (result)
                        {
                            informix.Commit();
                        }
                        else
                        {
                            informix.Rollback();
                        }
                    }
                }
            }

            return result;
        }


        public IList<Entidades.CorteGrupoA> VerificarTipoCorteAcaoRealizadaGA(int numeroCliente)
        {
            string sql = gerarSQLVerificarTipoCorteAcaoRealizadaGA(numeroCliente);
            DataTable dtResultado = ConsultaSql(sql);

            return dtToListObject<Entidades.CorteGrupoA>(dtResultado);
        }

        protected virtual string gerarSQLVerificarTipoCorteAcaoRealizadaGA(int numeroCliente)
        {
            StringBuilder sql = new StringBuilder();
            sql.Append(" SELECT FIRST 1 ");
            sql.Append(" co.numero_cliente AS NUMERO_CLIENTE, ");
            sql.Append(" co.corr_corte AS CORR_CORTE, ");
            sql.Append(" co.tipo_corte AS TIPO_CORTE, ");
            sql.Append(" co.fecha_corte AS DATA_EXECUCAO, ");
            sql.Append(" co.funcionario_corte AS FUNCIONARIO, ");
            sql.Append(" co.motivo_corte AS MOTIVO_CORTE, ");
            sql.Append(" co.acc_realizada_cor ACC_REALIZADA_COR, ");
            sql.Append(" co.numero_ordem AS NUMERO_ORDEM");
            sql.Append(" from ");
            sql.Append(" grandes@clientes:correp co ");
            sql.Append(" where ");
            sql.Append(" co.numero_cliente = '" + numeroCliente + "' ");
            sql.Append(" and (co.fecha_reposicion is null or co.fecha_reposicion = '') ");
            sql.Append(" order by co.fecha_corte desc ");

            return sql.ToString();
        }


        public string DataTableToJSON(DataTable table)
        {
            var list = new List<Dictionary<string, object>>();

            foreach (DataRow row in table.Rows)
            {
                var dict = new Dictionary<string, object>();

                foreach (DataColumn col in table.Columns)
                {
                    dict[col.ColumnName] = (Convert.ToString(row[col]));
                }
                list.Add(dict);
            }

            JavaScriptSerializer serializer = new JavaScriptSerializer();

            serializer.MaxJsonLength = Int32.MaxValue;

            return serializer.Serialize(list);
        }

        /// <summary>
        /// Por opção de arquitetura o corpo do metodo sera implementado conforma a classe(Ampla - Coelce) construida pela fabrica grupo A.
        /// </summary>
        /// <param name="numeroCliente"></param>
        /// <returns></returns>
        public abstract string gerarSQLObterMotivoDoCorteAtualGA(int numeroCliente);
        public abstract string gerarSQLObterMotivoDoCorteAtualSalesForceGA(int numeroCliente);


        public bool VerificaGrupo(int numero_cliente)
        {
            string sql = "select * from grandes@clientes:gc_cliente where numero_cliente = " + numero_cliente;

            DataTable dtResultado = ConsultaSql(sql);

            if(dtResultado.Rows.Count > 0)
                return true;
            else
                return false;
        }
    }



}

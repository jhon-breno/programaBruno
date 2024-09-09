using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using Pangea.Dados.Base;
using Pangea.Entidades;
using Pangea.Entidades.Enumeracao;
using System.Web.Script.Serialization;
using Pangea.Util;
using Pangea.Entidades.DTO;

namespace Pangea.Dados.Corte
{
    public class CorteDAO : BaseDAO
    {
        public CorteDAO(Empresa empresa)
            : base(empresa)
        {
        }

        public bool InserirCorsosoco(string sucursal, int numero_solicitud, string corr_corte, string numero_cliente, string motivo_sol, string tipo_corte, string funiconario, string oficina, string numero_livro, int corr_reaviso, double valor_divida, double divida_total, int qtde_fat_aberta)
        {
            string sql = String.Format(@"insert into corsoco
                                       (sucursal, numero_solicitud, estado, corr_corte, numero_cliente, motivo_sol,
                                       tipo_corte, terminal, funcionario, fecha_solicitud, fecha_generacion,
                                       oficina, numero_livro, corr_reaviso, valor_divida, divida_total, qtde_fat_aberta)
                                       values
                                       ({0},{1}, 'M', {2}, {3}, {4},{5},'00.00.00.000', '{6}', current,current,{0},
                                       {7}, {8}, {9}, {10}, {11});",
              sucursal, numero_solicitud, corr_corte, numero_cliente, motivo_sol, tipo_corte, funiconario, numero_livro, corr_reaviso, valor_divida, divida_total, qtde_fat_aberta);

            DBProviderInformix informix = ObterProviderInformix();
            informix.BeginTransacion();

            ExecutarSql(sql, informix);

            informix.Commit();

            return true;
        }

        public DadosCorsocoDTO RetornaDadosCorsoco(string numero_livro)
        {

            string sql = String.Format(@"SELECT *  
                                         FROM   dados_corsoco
                                         WHERE  numero_livro = {0}", numero_livro);

            var dt = ConsultaSql(sql);
            DadosCorsocoDTO resultado = DataHelper.ConvertDataTableToEntity<DadosCorsocoDTO>(dt);

            return resultado;
        }
        public int RetornaNumeroSolicitude(string sucursal)
        {
            int numero_solicitude;
            string sql = String.Format(@"SELECT valor
                                         FROM tabla
                                         WHERE nomtabla = 'SECUEN'
                                         AND codigo = 'SOLCOR'
                                         AND sucursal = {0}", sucursal);

            var dt = ConsultaSql(sql);

            if (dt.Rows.Count > 0)
            {
                numero_solicitude = Convert.ToInt32(dt.Rows[0][0].ToString());
                numero_solicitude++;
                if (numero_solicitude > 999999)
                {
                    numero_solicitude = 1;
                }

                DBProviderInformix informix = ObterProviderInformix();
                string update = String.Format(@"UPDATE tabla
                                                 SET valor = {0}
                                                 WHERE nomtabla = 'SECUEN'
                                                 AND codigo = 'SOLCOR'
                                                 AND sucursal = {0}", numero_solicitude, sucursal);
                ExecutarSql(update.ToString(), informix);

                return numero_solicitude;
            }


            return 0;
        }
        public bool InserirDadosCorsoco(string numero_livro, string sucursal, int corr_convenio, string ind_natureza, string empresa, string tipo_corte, string funcionario, int corr_reaviso)
        {
            DBProviderInformix informix = ObterProviderInformix();
            string sql = String.Format(@"insert  into dados_corsoco (numero_livro, sucursal, corr_convenio, ind_natureza, empresa, tipo_corte,funcionario,corr_reaviso)
                                       values ({0},{1},{2},'{3}',{4},{5},'{6}',{7})", numero_livro, sucursal, corr_convenio, ind_natureza, empresa, tipo_corte, funcionario, corr_reaviso);

            return ExecutarSql(sql.ToString(), informix);

        }
        
        public string gerarSQLObterMotivoDoCorteAtualSalesForce(int numeroCliente)
        {

            StringBuilder sql = new StringBuilder();
            sql.Append(" SELECT first 1                                 ");
            sql.Append(" c.numero_cliente,                              ");
            sql.Append(" to_char(fecha_corte,'%d/%m/%Y') as fecha,      ");
            sql.Append(" c.motivo_corte as guion,                       ");
            sql.Append(" to_char(hora_exec_corte,'%H:%M:%S') as Hora,   ");
            sql.Append(" c.valor_divida as valor,                       ");
            sql.Append(" cl.Nombre as Nome,                             ");
            sql.Append(" cl.estado_cliente as EstadoCliente,            ");
            sql.Append(" cl.ind_cliente_vital as vital, ");
            sql.Append(" cl.rut as documento1,                          ");
            sql.Append(" cl.dv_rut as dvDoc1,                           ");
            sql.Append(" cl.estado_suministro as estadoFornecimento,    ");
            sql.Append(" cl.estado_facturacion as EstadoFaturamento,    ");
            sql.Append(" cl.sector as loteFaturamento                   ");
            sql.Append(" from                                           ");
            sql.Append(" outer clientes@clientes:correp c, cliente cl   ");
            sql.Append(" where                                          ");
            sql.Append(" cl.numero_cliente = "                          + numeroCliente);
            sql.Append(" AND cl.numero_cliente = c.numero_cliente       ");
            sql.Append(" AND c.corr_corte = cl.corr_corte               ");
            sql.Append(" AND nvl(c.fecha_solic_repo,'') = ''            ");
            sql.Append(" order by fecha desc                            ");
            
            return sql.ToString();
        }

        public Entidades.SalesForce.Corte MotivoCorteAtualSalesForce(int numeroCliente)
        {
            List<Entidades.SalesForce.Corte> obj = new List<Entidades.SalesForce.Corte>();

            string sql = gerarSQLObterMotivoDoCorteAtualSalesForce(numeroCliente);
            DataTable dtResultado = ConsultaSql(sql);

            string formatado = DataTableToJSON(dtResultado);

            obj = new JavaScriptSerializer().Deserialize<List<Entidades.SalesForce.Corte>>(formatado);

            if (obj.Count > 0)
            {
                if (obj[0].EstadoFaturamento != "S")
                {
                    obj[0].EstadoFaturamento = "N";
                }
                if (obj[0].estadoFornecimento.Equals("0"))
                {
                    obj[0].Guion = string.Empty;
                    obj[0].Fecha = string.Empty;
                }
            }

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

        public IList<Entidades.Corte> BuscaCorteCliente(string numeroCliente)
        {
            string sql = gerarSQLBuscaCorteCliente(numeroCliente);
            DataTable dtResultado = ConsultaSql(sql);

            return dtToListObject<Entidades.Corte>(dtResultado);
        }

        protected virtual string gerarSQLBuscaCorteCliente(string numeroCliente)
        {
            StringBuilder sql = new StringBuilder();
            sql.Append(" SELECT ");
            sql.Append(" to_char(data_solic_corte,'%d/%m/%Y') data_emissao, ");
            sql.Append(" to_char(fecha_corte,'%d/%m/%Y') data_execucao, ");
            sql.Append(" tc.descricao_tipo tipo_corte, ");
            sql.Append(" c.tipo_corte as tip_corte, ");
            sql.Append(" to_char(hora_exec_corte,'%H:%M:%S') hora_corte, ");
            sql.Append(" acreco.descripcion situacao_corte, ");
            sql.Append(" '' numero_ordem, ");
            sql.Append(" '' ACC_REALIZADA_COR, ");
            sql.Append(" data_solic_corte data_hora_solicitacao ");
            sql.Append(" from  ");
            sql.Append(" clientes@clientes:correp c,  ");
            sql.Append(" outer clientes@clientes:tabla acreco,  ");
            sql.Append(" outer clientes@clientes:tipo_corte tc ");
            sql.Append(" where  ");
            sql.Append(" numero_cliente = '" + numeroCliente + "' ");
            sql.Append(" AND acreco.nomtabla = 'ACRECO'  ");
            sql.Append(" AND acreco.codigo = c.acc_realizada_cor ");
            sql.Append(" And acreco.sucursal = '0000' ");
            sql.Append(" And ((acreco.fecha_activacion >= acreco.fecha_desactivac And acreco.fecha_desactivac > Today)  Or acreco.fecha_desactivac is null) ");
            sql.Append(" and tc.tipo_corte = c.tipo_corte ");
            sql.Append(" UNION ");
            sql.Append(" SELECT  ");
            sql.Append(" to_char(c.Fecha_solicitud,'%d/%m/%Y') data_emissao, ");
            sql.Append(" '' data_execucao, ");
            sql.Append(" tc.descricao_tipo tipo_corte, ");
            sql.Append(" c.tipo_corte as tip_corte, ");
            sql.Append(" '' hora_corte, ");
            sql.Append(" nvl(estcor.descripcion, c.estado) situacao_corte, ");
            sql.Append(" '' numero_ordem, ");
            sql.Append(" '' ACC_REALIZADA_COR, ");
            sql.Append(" c.Fecha_solicitud data_hora_solicitacao ");
            sql.Append(" from ");
            sql.Append(" clientes@clientes:corsoco c,  ");
            sql.Append(" outer clientes@clientes:tabla estcor,  ");
            sql.Append(" outer clientes@clientes:tipo_corte tc  ");
            sql.Append(" where c.numero_cliente = '" + numeroCliente + "' ");
            sql.Append(" and tc.tipo_corte = c.tipo_corte ");
            sql.Append(" and estcor.codigo = c.estado ");
            sql.Append(" and estcor.nomtabla = 'ESTCOR' ");
            sql.Append(" and estcor.sucursal = '0000' ");
            sql.Append(" and ((estcor.fecha_activacion >= estcor.fecha_desactivac and estcor.fecha_desactivac > Today)  Or estcor.fecha_desactivac is null) ");
            sql.Append(" order by data_hora_solicitacao desc, data_execucao desc ");

            return sql.ToString();
        }

        public IList<TEntidade> dtToListObject<TEntidade>(System.Data.DataTable dt)
        {
            IList<Entidades.Corte> result = new List<Entidades.Corte>();

            foreach (DataRow item in dt.Rows)
            {
                Entidades.Corte tempCorte = new Entidades.Corte();

                tempCorte.DataDeSolicitacao = TratarString(dt, item, "DATA_EMISSAO");
                tempCorte.DataDaExecucao = TratarString(dt, item, "DATA_EXECUCAO");

                tempCorte.Tipo = TratarString(dt, item, "TIPO_CORTE");
                tempCorte.Hora = TratarString(dt, item, "HORA_CORTE");
                tempCorte.Situacao = TratarString(dt, item, "SITUACAO_CORTE");
                tempCorte.NumeroOrdem = TratarString(dt, item, "NUMERO_ORDEM");
                tempCorte.AcaoRealizada = TratarString(dt, item, "ACC_REALIZADA_COR");

                result.Add(tempCorte);
            }

            return result as IList<TEntidade>;
        }

        public IList<Entidades.Corte> VerificarUltimoCorteDesligamentoPedido(string numeroCliente)
        {
            string sql = gerarSQLVerificarUltimoCorteDesligamentoPedido(numeroCliente);
            DataTable dtResultado = ConsultaSql(sql);

            return dtToListObject<Entidades.Corte>(dtResultado);
        }

        protected virtual string gerarSQLVerificarUltimoCorteDesligamentoPedido(string numeroCliente)
        {
            StringBuilder sql = new StringBuilder();
            sql.Append(" SELECT ");
            sql.Append(" '' data_emissao, ");
            sql.Append(" '' data_execucao, ");
            sql.Append(" '' tipo_corte, ");
            sql.Append(" '' hora_corte, ");
            sql.Append(" '' situacao_corte, ");
            sql.Append(" '' ACC_REALIZADA_COR, ");
            sql.Append(" c.numero_livro, ");
            sql.Append(" c.num_ordem_serv_crt numero_ordem ");
            sql.Append(" from  ");
            sql.Append(" clientes@clientes:correp c ");
            sql.Append(" where  ");
            sql.Append(" c.numero_cliente = '" + numeroCliente + "' ");
            sql.Append(" and c.tipo_corte in ('01','02') ");

            return sql.ToString();
        }

        public IList<Entidades.Corte> VerificarTipoCorteAcaoRealizada(int numeroCliente)
        {
            string sql = gerarSQLVerificarTipoCorteAcaoRealizada(numeroCliente);
            DataTable dtResultado = ConsultaSql(sql);

            return dtToListObject<Entidades.Corte>(dtResultado);
        }

        protected virtual string gerarSQLVerificarTipoCorteAcaoRealizada(int numeroCliente)
        {
            StringBuilder sql = new StringBuilder();
            sql.Append(" SELECT FIRST 1 ");
            sql.Append(" '' data_emissao, ");
            sql.Append(" '' data_execucao, ");
            sql.Append(" c.tipo_corte tipo_corte, ");
            sql.Append(" '' hora_corte, ");
            sql.Append(" '' situacao_corte, ");
            sql.Append(" c.acc_realizada_cor ACC_REALIZADA_COR, ");
            sql.Append(" '' numero_livro, ");
            sql.Append(" c.fecha_corte, ");
            sql.Append(" '' numero_ordem ");
            sql.Append(" from  ");
            sql.Append(" clientes@clientes:correp c ");
            sql.Append(" where  ");
            sql.Append(" c.numero_cliente = '" + numeroCliente + "' ");
            sql.Append(" and (c.fecha_reposicion is null or c.fecha_reposicion = '') ");
            sql.Append(" order by c.fecha_corte desc ");

            return sql.ToString();
        }

        public bool AtualizarCorrelativoCorte(int numeroCliente, string ultimoCorrelativoCorte)
        {
            bool result = false;

            StringBuilder sqlCorrep = new StringBuilder();
            sqlCorrep.Append(" Select count(*) as qtd from correp  ");
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
                sqlCorsore.Append(" Select count(*) as qtd from corsore  ");
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
                        sqlUpdateCorrelativo.Append(" update cliente set corr_corte = corr_corte + 1 Where numero_cliente =" + numeroCliente);

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

        protected virtual string gerarSQLObterMotivoDoCorteAtual(int numeroCliente)
        {
            StringBuilder sql = new StringBuilder();
            sql.AppendFormat(@"SELECT FIRST 1 cr.motivo_corte
                                 FROM correp cr
                           INNER JOIN tabla t
                                   ON t.codigo         = cr.acc_realizada_cor
                                  AND t.nomtabla       = 'ACRECO'
                                  AND t.sucursal       = '0000'
                                  AND t.valor_alf[2,2] = '1'
                                WHERE cr.numero_cliente = '{0}'
                                  AND cr.motivo_corte   <> '20'
                                  AND NOT EXISTS (SELECT 1
                                                    FROM correp cr1
                                              INNER JOIN tabla t1
                                                      ON cr1.numero_cliente    = cr.numero_cliente
                                                     AND cr1.acc_realizada_rep = t1.codigo
                                                     AND t1.nomtabla           = 'ACRERE'
                                                     AND t1.sucursal           = '0000'
                                                     AND t1.valor_alf[2,2]     = '1'
                                                   WHERE cr1.fecha_reposicion  > cr.data_atual_corte)
                             ORDER BY cr.data_atual_corte DESC "
                , numeroCliente);

            return sql.ToString();
        }

        public string ObterMotivoDoCorteAtual(int numeroCliente)
        {
            string sql = gerarSQLObterMotivoDoCorteAtual(numeroCliente);
            DataTable dtResultado = ConsultaSql(sql);

            string motivo = "";

            foreach (DataRow item in dtResultado.Rows)
                motivo = TratarString(dtResultado, item, "motivo_corte");

            return motivo;
        }


        /// <summary>
        /// Retorna as últimas solicitações de corte para um cliente na CORSOCO.
        /// </summary>
        /// <param name="numeroCliente"></param>
        /// <param name="qtdRegistros">Mínimo de registros é 1, por padrão.</param>
        /// <returns></returns>
        public DataTable ObterUltimasSolicitacaoCorte(int numeroCliente, int qtdRegistros)
        {
            qtdRegistros = qtdRegistros <= 0 ? 1 : qtdRegistros;

            StringBuilder sql = new StringBuilder(string.Format(
                      @"SELECT FIRST {0} C.tipo_corte,
                                       T.ind_massivo,
                                       C.corr_corte,
                                       date(C.fecha_solicitud) as data_corte,
                                       T.ind_racionamento as ind_racionamento,
                                       T1.valor_alf[1,1] as ind_rel_divida,
                                       ' ' as ind_executado,
                                       T2.valor_alf[4,5] as tipo_religacao,
                                       T3.valor_alf[1,8] as inf_irreg_inst
                          FROM corsoco C, tipo_corte T, tabla T1,
                               OUTER (correp C1, tabla T2, outer tabla T3)
                         WHERE c.numero_cliente = {1}
                           AND C.estado <> 'I'
                           AND T.tipo_corte *1 = C.tipo_corte *1
                           AND T1.nomtabla = 'CORMOT'
                           AND T1.sucursal = '0000'
                           AND C.motivo_sol = T1.codigo
                           AND C1.numero_cliente = C.numero_cliente
                           AND C1.corr_corte = C.corr_corte
                           AND T2.nomtabla = 'ACRECO'
                           AND T2.sucursal = '0000'
                           AND T2.codigo = C1.acc_realizada_cor
                           AND T3.nomtabla = 'IRRINS'
                           AND T3.sucursal = '0000'
                           AND T3.codigo = C1.irreg_instalacao
                         ORDER by 3 desc", qtdRegistros, numeroCliente));
            
            return ConsultaSql(sql.ToString());
        }


        /// <summary>
        /// Retorna os últimos registros do histórico de corte e religação na CORREP.
        /// </summary>
        /// <param name="numeroCliente"></param>
        /// <param name="qtdRegistros">Mínimo de registros é 1, por padrão.</param>
        /// <returns></returns>
        public DataTable ObterHistoricoCortePorCliente(int numeroCliente, int qtdRegistros)
        {
            qtdRegistros = qtdRegistros <= 0 ? 1 : qtdRegistros;

            StringBuilder sql = new StringBuilder(string.Format(
                 @"SELECT FIRST {0} C.tipo_corte,
                              T.ind_massivo,
                              C.corr_corte,
                              C.fecha_corte as data_corte,
                              T.ind_racionamento,
                              T1.valor_alf[1,1] as ind_rel_divida,
                              T2.valor_alf[2,2] as ind_executado,
                              T2.valor_alf[4,5] as tipo_religacao,
                              T3.valor_alf[1,8] as inf_irreg_inst
                 FROM correp C, tipo_corte T, tabla T1, tabla T2, outer tabla T3
                WHERE numero_cliente = {1}
                  AND T.tipo_corte *1 = C.tipo_corte *1
                  AND T1.nomtabla = 'CORMOT'
                  AND T1.sucursal = '0000'
                  AND C.motivo_corte = T1.codigo
                  AND T2.nomtabla = 'ACRECO'
                  AND T2.sucursal = '0000'
                  AND T2.codigo = C.acc_realizada_cor
                  AND T3.nomtabla = 'IRRINS'
                  AND T3.sucursal = '0000'
                  AND T3.codigo = C.irreg_instalacao
                ORDER by 3 desc", qtdRegistros, numeroCliente));

            return ConsultaSql(sql.ToString());
        }


        /// <summary>
        /// Retorna as solicitações de corte por cliente e motivo
        /// </summary>
        /// <param name="numeroCliente"></param>
        /// <param name="codMotivo"></param>
        /// <param name="qtdRegistros"></param>
        /// <returns></returns>
        public DataTable ObterCortesPorMotivo(int numeroCliente, string codMotivo, int qtdRegistros)
        {
            qtdRegistros = qtdRegistros <= 0 ? 1 : qtdRegistros;

            StringBuilder sql = new StringBuilder(string.Format(
                 @"SELECT FIRST {0} fecha_solicitud, numero_cliente, corr_corte
 			         FROM corsoco 
			        WHERE numero_cliente = {1} ", qtdRegistros, numeroCliente));

            if(!string.IsNullOrEmpty(codMotivo))
            {
                sql.AppendFormat(" AND motivo_sol = '{0}'", codMotivo);
            }
            return ConsultaSql(sql.ToString());
        }

        
        public string[] ObterPrevisaoDeCorte(int numeroCliente)
        {
            var sql = string.Format(@"select to_char(data_apto_corte, '%d/%m/%Y') as data_apto_corte
                                        from hisreav 
                                       where numero_cliente = '{0}'
                                         and estado = 'V' "
                                       , numeroCliente);

            DataTable dtResultado = ConsultaSql(sql);
            
            List<string> datasDePrevisaoParaCorte = new List<string>();

            foreach (DataRow item in dtResultado.Rows)
            {
                string dataPrevistaParaCorte = TratarString(dtResultado, item, "data_apto_corte");
                datasDePrevisaoParaCorte.Add(dataPrevistaParaCorte);
            }

            return datasDePrevisaoParaCorte.ToArray();
        }

        //public HistoricoReaviso[] ConsultarHistoricoReaviso(int numeroCliente)
        //{
        //    List<HistoricoReaviso> result = new List<HistoricoReaviso>();

        //    StringBuilder sql = new StringBuilder();
        //    sql.Append(" SELECT ");
        //    sql.Append(" h.corr_reaviso, ");
        //    sql.Append(" h.referencia_fat, ");
        //    sql.Append(" h.data_reaviso, ");
        //    sql.Append(" h.divida_cliente, ");
        //    sql.Append(" h.estado cod_estado, ");
        //    sql.Append(" t.descripcion desc_estado , ");
        //    sql.Append(" h.data_estado, ");
        //    sql.Append(" h.rol_estado, ");
        //    sql.Append(" r2.nombre nome_usrEstado, ");
        //    sql.Append(" h.ind_entrega, ");
        //    sql.Append(" h.rol_atual_entrega ,");
        //    sql.Append(" r.nombre nome_usrEntrega, ");
        //    sql.Append(" h.motivo_corte, ");
        //    sql.Append(" t1.descripcion desc_motivoCorte, ");
        //    sql.Append(" h.numero_protocolo, ");
        //    sql.Append(" h.data_apto_corte ");
        //    sql.Append(" FROM ");
        //    sql.Append(" hisreav h, tabla T1,  outer tabla t, outer rol r, outer rol r2 ");
        //    sql.Append(" WHERE ");
        //    sql.Append(" h.numero_cliente = " + numeroCliente);
        //    sql.Append(" And t.nomtabla = 'ESTREA'  ");
        //    sql.Append(" AND t.sucursal = '0000' ");
        //    sql.Append(" And t.codigo = h.estado  ");
        //    sql.Append(" And T1.nomtabla = 'CORMOT' ");
        //    sql.Append(" AND T1.sucursal = '0000'  ");
        //    sql.Append(" And T1.codigo = h.motivo_corte ");
        //    sql.Append(" And r.rol = h.rol_atual_entrega ");
        //    sql.Append(" And r2.ROL = h.rol_estado");
        //    sql.Append(" and h.estado = 'V' ");
        //    sql.Append(" and h.data_apto_corte is not null ");
        //    sql.Append(" Order by ");
        //    sql.Append(" h.Corr_reaviso Desc ");

        //    DataTable dt = ConsultaSql(sql.ToString());

        //    foreach (DataRow item in dt.Rows)
        //    {
        //        HistoricoReaviso hisreavtemp = new HistoricoReaviso();

        //        hisreavtemp.CorrelativoReaviso = TratarShort(dt, item, "corr_reaviso", 0);
        //        hisreavtemp.DataEstado = TratarDateTime(dt, item, "data_estado");
        //        hisreavtemp.DataAptoCorte = TratarDateTime(dt, item, "data_apto_corte");
        //        hisreavtemp.DataReaviso = TratarDateTime(dt, item, "data_reaviso");
        //        hisreavtemp.DividaCliente = TratarFloat(dt, item, "divida_cliente", 0);
        //        hisreavtemp.EstadoReaviso = TratarString(dt, item, "cod_estado");
        //        hisreavtemp.DividaCliente = TratarChar(dt, item, "ind_entrega");
        //        hisreavtemp.MotivoCorte = TratarString(dt, item, "motivo_corte");
        //        hisreavtemp.NumeroCliente = numeroCliente;
        //        hisreavtemp.Protocolo = TratarInt(dt, item, "numero_protocolo", 0);
        //        hisreavtemp.ReferenciaFatura = TratarInt(dt, item, "referencia_fat", 0);
        //        hisreavtemp.TipoReaviso = hisreavtemp.ReferenciaFatura == 0 ? "Individual" : "Massivo";

        //        result.Add(hisreavtemp);
        //    }

        //    return result.ToArray();
        //}

        public bool IngressarSuspensaoCorte(int numeroCliente, int dias, string canalAtendimento,  DBProviderInformix informix)
        {
            bool result = false;

            StringBuilder sql = new StringBuilder();
            sql.Append(" INSERT ");
            sql.Append(" INTO ");
            sql.Append(" corplazo ");
            sql.Append(" ( ");
            sql.Append(" tipo,  ");
            sql.Append(" sucursal, ");
            sql.Append(" rol, ");
            sql.Append(" motivo, ");
            sql.Append(" dias, ");
            sql.Append(" numero_cliente, ");
            sql.Append(" fecha_solicitud ");
            sql.Append(" ) ");
            sql.Append(" SELECT ");
            sql.Append(" 'D' AS tipo, ");
            sql.Append(" Sucursal, ");
            sql.Append(" '" + canalAtendimento + "' as rol, ");
            sql.Append(" 'COMUNICACAO CONTA PAGA' as Motivo, ");
            sql.Append(" '" + dias + "' as dias, ");
            sql.Append(" Numero_cliente, ");
            sql.Append(" TODAY as fecha_solicitud ");
            sql.Append(" FROM  ");
            sql.Append(" cliente ");
            sql.Append(" WHERE ");
            sql.Append(" numero_cliente = " + numeroCliente);
            result = ExecutarSql(sql.ToString(), informix);

            return result;
        }

        public bool AtualizarDataAptoCorte(DateTime dataAptoCorte, short correlativoReaviso, int numeroCliente, int dias, DBProviderInformix informix)
        {
            bool result = false;

            string dataApto = dataAptoCorte.AddDays(dias).ToString("MM/dd/yyyy");

            StringBuilder sqlHisRea = new StringBuilder();
            sqlHisRea.Append(" UPDATE ");
            sqlHisRea.Append(" hisreav ");
            sqlHisRea.Append(" set data_apto_corte = '" + dataApto + "' ");
            sqlHisRea.Append(" ,estado = 'S' ");
            sqlHisRea.Append(" WHERE ");
            sqlHisRea.Append(" numero_cliente = " + numeroCliente);
            sqlHisRea.Append(" and corr_reaviso = '" + correlativoReaviso + "' ");

            StringBuilder sqlCliente = new StringBuilder();
            sqlCliente.Append(" UPDATE ");
            sqlCliente.Append(" cliente ");
            sqlCliente.Append(" set fecha_a_corte = '" + dataApto + "' ");
            sqlCliente.Append(" WHERE ");
            sqlCliente.Append(" numero_cliente = " + numeroCliente);
            
            result = ExecutarSql(sqlHisRea.ToString(), informix);

            if (result)
            {
                result = ExecutarSql(sqlCliente.ToString(), informix);
            }

            return result;
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

        public bool VerificaCliente(int numero_cliente)
        {
           string sql = "select * from cliente where numero_cliente = " + numero_cliente;

            DataTable dtResultado = ConsultaSql(sql);

            if(dtResultado.Rows.Count > 0)
                return true;
            else
                return false;
        }

        /// <summary>
        /// Ingressa uma ocorrência de corte.
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public int IngressarOcorrenciaCorte(OcorrenciaCorteDTO obj)
        {
            StringBuilder sql = new StringBuilder();
            sql.Append(" INSERT INTO OCORRENCIA_CORTE ( ");
            sql.Append(" NUMERO_CLIENTE,  ");
            sql.Append(" DATA_OCORRENCIA, ");
            sql.Append(" CORR_CORTE, ");
            sql.Append(" TIPO_OCORRENCIA, ");
            sql.Append(" OBSERVACAO ) VALUES ");

            sql.AppendFormat(" {0} ", obj.numero_cliente);
            sql.AppendFormat(" {0} ", obj.data_processamento_s.ToString("yyyy-MM-dd"));
            sql.AppendFormat(" {0} ", obj.corr_corte);
            sql.AppendFormat(" {0} ", obj.tipo_ocorrencia);
            sql.AppendFormat(" {0} ", obj.observacao);
            
            return ExecutarSql(sql.ToString());
        }
    }
}

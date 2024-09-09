using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Pangea.Dados.Base;
using Pangea.Entidades;
using Pangea.Util;
using System.Web.Script.Serialization;
using Pangea.Entidades.DTO;
using Pangea.Entidades.Enumeracao;
using Entidades.DTO;

namespace Pangea.Dados
{
    public class CorteDAO : BaseDAO
    {
        public CorteDAO(Empresa empresa)
            : base(empresa)
        {
            this.empresa = empresa;
        }

        #region ORDEM CORTE

        public SolicitBloqCorteClienteVitalDTO RetornaBloqCorteClienteVital(string numeroCliente)
        {
            StringBuilder sql = new StringBuilder();
            sql.Append(@"select first 1 os.tipo_ordem,
	                                            os.tipo_servico,
	                                            s.des_servico,
	                                            os.numero_cliente,
	                                            os.numero_ordem,
	                                            vo.cod_retorno,
                                                cor.motivo,
		                                        cor.dias,
		                                        cor.fecha_solicitud
                                            from ordem_servico os,servicos s, visita_ordem vo, corplazo cor   
                                            where 1=1
                                                and os.tipo_ordem = s.tipo_ordem
                                                and os.tipo_servico = s.cod_servico
                                                and vo.numero_ordem = os.numero_ordem
                                                and vo.etapa = os.etapa
                                                and vo.corr_visita = os.corr_visita");
            if (empresa.Equals(Empresa.RJ))
            {
                sql.Append(" and os.tipo_ordem='OUT' ");
                sql.Append(" and os.tipo_servico='R02' ");
                sql.Append(" and vo.cod_retorno='0029' ");
            }
            if (empresa.Equals(Empresa.CE))
            {
                sql.Append(" and os.tipo_ordem='OUT' ");
                sql.Append(" and os.tipo_servico='CRT' ");
                sql.Append(" and vo.cod_retorno='0040' ");
                sql.Append(" and os.numero_ordem = cor.numero_ordem ");
            }
            sql.Append(" and cor.numero_cliente = os.numero_cliente ");
            sql.Append(String.Format(" and os.numero_cliente={0}", numeroCliente));

            var dt = ConsultaSql(sql.ToString());

            SolicitBloqCorteClienteVitalDTO resultado = gerarEntidadeSolicitudBloqCorte(dt);

            return resultado;
        }


        public bool RetornaOrdem(int numeroCliente, int corCorte)
        {
            bool resultado;

            String sql = String.Format(@"SELECT cor.num_ordem_serv_crt
                                         FROM correp cor
                                         where cor.numero_cliente={0}
                                         AND cor.corr_corte = {1}", numeroCliente, corCorte);
            var dt = ConsultaSql(sql);

            if (dt.Rows.Count > 0)
            {
                if (!String.IsNullOrEmpty(dt.Rows[0]["num_ordem_serv_crt"].ToString()))
                    resultado = true;
                else
                    resultado = false;
            }
            else
                resultado = false; 

            return resultado;

        }

        public ResultadoCorteReconexaoDTO RetornaCorteOrdem(int numero_cliente, int corr)
        {
            String sql = String.Format(@"SELECT c.num_ordem_serv_crt AS identificador_synergia, 		
                                               c.corr_corte as corr_corte,                                                     
                                               m.numero_medidor ||'-'|| m.marca_medidor as instalacao,			
                                               t.codigo as estado,        
                                               c.tipo_corte,        
                                               c.fecha_corte AS data_corte,                                                   
                                               c.fecha_reposicion AS data_relicacao
                                                                                                   	
                                        from correp c, 
                        	                 tabla t, 
                        	                 medid m
                                        where c.acc_realizada_cor = t.codigo 
                                        AND t.nomtabla = 'ACRECO'
                                        AND t.sucursal='0000'
                                        AND m.numero_cliente = c.numero_cliente
                                        AND m.estado='I'
                                        and c.numero_cliente={0}
                                        and c.corr_corte={1}", numero_cliente, corr);

            var dt = ConsultaSql(sql);

            if (dt != null)
            {
                ResultadoCorteReconexaoDTO resultado = DataHelper.ConvertDataTableToEntity<ResultadoCorteReconexaoDTO>(dt);
                return resultado;
            }
            else return null;
            
        }
        #endregion

        #region LIVRO DE CORTE
        public ResultadoCorteReconexaoDTO RetornaCorteLivro(int numero_cliente, int corr)
        {
            String sql = String.Format(@"SELECT c.numero_livro AS identificador_synergia, 		
                                                c.corr_corte,                                                     
                                                m.numero_medidor ||'-'|| m.marca_medidor as instalacao,			
                                                t.codigo as estado,        
                                                c.tipo_corte,        
                                                c.fecha_corte AS data_corte,                                                   
                                                c.fecha_reposicion AS data_relicacao                        	
                                        from correp c, 
                	                            tabla t, 
                	                            medid m
                                        where c.acc_realizada_cor = t.codigo 
                                        AND t.nomtabla = 'ACRECO'
                                        AND t.sucursal='0000'
                                        AND m.numero_cliente = c.numero_cliente
                                        AND m.estado='I'
                                        and c.numero_cliente={0}
                                        and c.corr_corte={1}", numero_cliente, corr);

            var dt = ConsultaSql(sql);

            if (dt != null)
            {
                ResultadoCorteReconexaoDTO resultado = DataHelper.ConvertDataTableToEntity<ResultadoCorteReconexaoDTO>(dt);
                return resultado;
            }
            else return null;
        }
        #endregion

        public ResultadoCorteDTO gerarEntidadeCorte(DataTable resultDt)
        {

            ResultadoCorteDTO DTO = new ResultadoCorteDTO();
            if (resultDt.Rows.Count > 0)
            {

                DTO.sucursal = TratarString(resultDt, resultDt.Rows[0], "sucursal");
                DTO.numero_livro = TratarString(resultDt, resultDt.Rows[0], "numero_livro");
                DTO.corr_corte = TratarInt(resultDt, resultDt.Rows[0], "numero_livro", -1);
                DTO.estado = TratarString(resultDt, resultDt.Rows[0], "estado");
                DTO.motivo_sol = TratarString(resultDt, resultDt.Rows[0], "motivo_sol");
                DTO.tipo_corte = TratarString(resultDt, resultDt.Rows[0], "tipo_corte");
                DTO.numero_cliente = TratarInt(resultDt, resultDt.Rows[0], "numero_cliente", -1);
                DTO.numero_ordem = TratarString(resultDt, resultDt.Rows[0], "numero_ordem");
                DTO.corr_reaviso = TratarString(resultDt, resultDt.Rows[0], "corr_reaviso");
                DTO.numero_ordem_corte = TratarInt(resultDt, resultDt.Rows[0], "numero_ordem_corte", -1);
                DTO.data_solic_corte = TratarDateTime(resultDt, resultDt.Rows[0], "data_solic_corte");
                DTO.fecha_corte = TratarDateTime(resultDt, resultDt.Rows[0], "fecha_corte");
                DTO.hora_exec_corte = TratarDateTime(resultDt, resultDt.Rows[0], "hora_exec_corte");
                DTO.fase_corte = TratarString(resultDt, resultDt.Rows[0], "fase_corte");
                DTO.motivo_corte = TratarString(resultDt, resultDt.Rows[0], "motivo_corte");
                DTO.acc_realizada_cor = TratarString(resultDt, resultDt.Rows[0], "acc_realizada_cor");
                DTO.irreg_instalacao = TratarString(resultDt, resultDt.Rows[0], "irreg_instalacao");
                DTO.sit_encon_cor = TratarString(resultDt, resultDt.Rows[0], "sit_encon_cor");
                DTO.leitura_corte = TratarInt(resultDt, resultDt.Rows[0], "leitura_corte", -1);
                DTO.fecha_reposicion = TratarDateTime(resultDt, resultDt.Rows[0], "fecha_reposicion");
                DTO.tipo_religacao = TratarString(resultDt, resultDt.Rows[0], "tipo_religacao");
                DTO.acc_realizada_rep = TratarString(resultDt, resultDt.Rows[0], "acc_realizada_rep");
                DTO.sit_encon_rep = TratarString(resultDt, resultDt.Rows[0], "sit_encon_rep");
                DTO.leitura_repo = TratarInt(resultDt, resultDt.Rows[0], "leitura_repo", -1);
                DTO.numero_ordem_repo = TratarInt(resultDt, resultDt.Rows[0], "numero_ordem_repo", -1);
                DTO.fecha_solic_repo = TratarDateTime(resultDt, resultDt.Rows[0], "fecha_solic_repo");
                DTO.data_inicio_cort = TratarDateTime(resultDt, resultDt.Rows[0], "data_inicio_cort");
                DTO.data_inicio_relg = TratarDateTime(resultDt, resultDt.Rows[0], "data_inicio_relg");
                DTO.desc_servico = TratarString(resultDt, resultDt.Rows[0], "des_servico");


                return DTO;
            }
            return new ResultadoCorteDTO();
        }

        public SolicitBloqCorteClienteVitalDTO gerarEntidadeSolicitudBloqCorte(DataTable resultDt)
        {
            SolicitBloqCorteClienteVitalDTO DTO = new SolicitBloqCorteClienteVitalDTO();

            if (resultDt.Rows.Count > 0)
            {
                DTO.codigo_empresa = empresa == Empresa.RJ ? "2005" : "2003"; 
                DTO.numero_cliente = TratarString(resultDt, resultDt.Rows[0], "numero_cliente");
                DTO.motivo = TratarString(resultDt, resultDt.Rows[0], "motivo");
                DTO.dias = TratarInt(resultDt, resultDt.Rows[0], "dias", 0);
                DTO.fecha_solicitud = TratarDateTime(resultDt, resultDt.Rows[0], "fecha_solicitud");

                return DTO;
            }

            return new SolicitBloqCorteClienteVitalDTO();

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
            sql.Append(" cl.numero_cliente = " + numeroCliente);
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

        public  IList<TEntidade> dtToListObject<TEntidade>(System.Data.DataTable dt)
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

        //[Obsolete("Utilizar ClienteDAO para executar esta atualização.")]
        //public bool AtualizarCorrelativoCorte(int numeroCliente, string ultimoCorrelativoCorte)
        //{
        //    bool result = false;

        //    StringBuilder sqlCorrep = new StringBuilder();
        //    sqlCorrep.Append(" Select count(*) as qtd from correp  ");
        //    sqlCorrep.Append(" where numero_cliente =" + numeroCliente);
        //    sqlCorrep.Append(" and corr_corte =" + ultimoCorrelativoCorte);
        //    sqlCorrep.Append(" and ( fecha_reposicion is not null or fecha_reposicion != '')");

        //    bool possuiCorrep = false;

        //    DataTable dtCorrep = ConsultaSql(sqlCorrep.ToString());

        //    if (dtCorrep.Rows.Count > 0)
        //    {
        //        int resultQtdCorrep = 0;
        //        int.TryParse(dtCorrep.Rows[0]["qtd"].ToString(), out resultQtdCorrep);
        //        possuiCorrep = resultQtdCorrep > 0;
        //    }

        //    if (!possuiCorrep)
        //    {
        //        StringBuilder sqlCorsore = new StringBuilder();
        //        sqlCorsore.Append(" Select count(*) as qtd from corsore  ");
        //        sqlCorsore.Append(" where numero_cliente =" + numeroCliente);
        //        sqlCorsore.Append(" and corr_corte =" + ultimoCorrelativoCorte);

        //        bool possuiCorsore = false;

        //        DataTable dtCorsore = ConsultaSql(sqlCorsore.ToString());

        //        if (dtCorsore.Rows.Count > 0)
        //        {
        //            int resultQtdCorsore = 0;
        //            int.TryParse(dtCorsore.Rows[0]["qtd"].ToString(), out resultQtdCorsore);
        //            possuiCorsore = resultQtdCorsore > 0;

        //            if (possuiCorsore)
        //            {
        //                DBProviderInformix informix = ObterProviderInformix();
        //                informix.BeginTransacion();

        //                StringBuilder sqlUpdateCorrelativo = new StringBuilder();
        //                sqlUpdateCorrelativo.Append(" update cliente set corr_corte = corr_corte + 1 Where numero_cliente =" + numeroCliente);

        //                result = ExecutarSql(sqlUpdateCorrelativo.ToString(), informix);

        //                if (result)
        //                {
        //                    informix.Commit();
        //                }
        //                else
        //                {
        //                    informix.Rollback();
        //                }
        //            }
        //        }
        //    }

        //    return result;
        //}

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
                                       date(C.fecha_solicitud),
                                       T.ind_racionamento,
                                       T1.valor_alf[1,1],
                                       ' ',
                                       T2.valor_alf[4,5],
                                       T3.valor_alf[1,8],
                                       C.numero_livro
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
                              C.fecha_corte,
                              T.ind_racionamento,
                              T1.valor_alf[1,1],
                              T2.valor_alf[2,2],
                              T2.valor_alf[4,5],
                              T3.valor_alf[1,8],
                              numero_livro
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
                 @"SELECT FIRST {0} fecha_solicitud
 			         FROM corsoco 
			        WHERE numero_cliente = {1} ", qtdRegistros, numeroCliente));

            if (!string.IsNullOrEmpty(codMotivo))
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

        public bool IngressarSuspensaoCorte(string numeroCliente, int dias, string canalAtendimento, DBProviderInformix conn)
        {
            #region obsoleto
            //bool result = false;

            //StringBuilder sql = new StringBuilder();
            //sql.Append(" INSERT ");
            //sql.Append(" INTO ");
            //sql.Append(" corplazo ");
            //sql.Append(" ( ");
            //sql.Append(" tipo,  ");
            //sql.Append(" sucursal, ");
            //sql.Append(" rol, ");
            //sql.Append(" motivo, ");
            //sql.Append(" dias, ");
            //sql.Append(" numero_cliente, ");
            //sql.Append(" fecha_solicitud ");
            //sql.Append(" ) ");
            //sql.Append(" SELECT ");
            //sql.Append(" 'D' AS tipo, ");
            //sql.Append(" Sucursal, ");
            //sql.Append(" '" + canalAtendimento + "' as rol, ");
            //sql.Append(" 'COMUNICACAO CONTA PAGA' as Motivo, ");
            //sql.Append(" '" + dias + "' as dias, ");
            //sql.Append(" Numero_cliente, ");
            //sql.Append(" TODAY as fecha_solicitud ");
            //sql.Append(" FROM  ");
            //sql.Append(" cliente ");
            //sql.Append(" WHERE ");
            //sql.Append(" numero_cliente = " + numeroCliente);
            //result = ExecutarSql(sql.ToString(), informix);

            //return result;

            #endregion

            CorplazoDTO dtoSuspensao = new CorplazoDTO(rol: canalAtendimento, motivoIngresso: "COMUNICACAO CONTA PAGA", numeroCliente: numeroCliente, dias: dias, oficina: string.Empty, tipo: "D");
            return IngressarSuspensaoCorte(dtoSuspensao, conn);
        }

        /// <summary>
        /// Ingressa uma anulação temporal de corte, referente à tabela CORPLAZO.
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public bool IngressarSuspensaoCorte(CorplazoDTO obj, DBProviderInformix conn)
        {
            StringBuilder sql = new StringBuilder();
            sql.Append(" INSERT INTO corplazo ( ");
            sql.Append(" tipo,  ");
            sql.Append(" sucursal, ");
            sql.Append(" rol, ");
            sql.Append(" motivo, ");
            sql.Append(" dias, ");
            sql.Append(" numero_cliente, ");
            sql.Append(" fecha_solicitud ) ");

            sql.Append(" SELECT ");
            sql.AppendFormat(" '{0}' AS tipo, ", string.IsNullOrEmpty(obj.tipo) ? obj.tipo : string.Empty);
            sql.Append(" sucursal, ");      // fixo o campo CLIENTE.SUCURSAL
            sql.AppendFormat(" '{0}' as rol, ", (string.IsNullOrEmpty(obj.rol)) ? obj.rol : string.Empty);
            sql.AppendFormat(" '{0}' as Motivo, ", (string.IsNullOrEmpty(obj.motivo)) ? obj.motivo : string.Empty);
            sql.AppendFormat(" '{0}' as dias, ", (obj.dias.HasValue) ? obj.dias : null);
            sql.Append(" numero_cliente, ");    //FIXO o campo CLIENTE.NUMERO_CLIENTE
            sql.AppendFormat(" {0} as fecha_solicitud ", (obj.fecha_solicitud.HasValue && DateTime.MinValue > obj.fecha_solicitud) ?
                string.Concat("'", obj.fecha_solicitud.Value.ToString("yyyy-MM-dd HH:mm"), "'") : "extend (current, year to minute)");

            sql.Append(" FROM  ");
            sql.Append(" cliente ");
            sql.Append(" WHERE 1=1 ");

            sql.AppendFormat(" and numero_cliente = {0}", obj.numero_cliente);

            return ExecutarSql(sql.ToString(), conn);
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

            if (dtResultado.Rows.Count > 0)
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

        public TipoCorteDTO RetornaTipoCorte(string cod_tipo)
        {

            string sql = String.Format(@"SELECT descricao_tipo, 
                                         qtd_corte_min, qtd_corte_max,
                                         qtd_autorel_min, qtd_autorel_max,
                                         clientes_to, clientes_caducados, clientes_atraso,clientes_parafuso,
                                         clientes_recorte, clientes_autorel, nvl(clientes_asf,'N') as clientes_asf, meses_corte,
                                         meses_autorel, meses_parcel, qtd_parcel_min, qtd_parcel_max,
                                         clientes_smc, ind_automatico,clientes_debaut,cod_seg,
                                         ind_caixa, ind_natur_corte , parcel_recorte ,
                                         ind_tipo_corte ,clientes_prepago ,cod_operacao  
                                         FROM   TIPO_CORTE
                                         WHERE  tipo_corte = {0}", cod_tipo);

            var dt = ConsultaSql(sql);
            TipoCorteDTO resultado = DataHelper.ConvertDataTableToEntity<TipoCorteDTO>(dt);

            return resultado;
        }

        public CorparDTO RetornaCorpar(string sucursal, string dir_ip)
        {

            string sql = String.Format(@"SELECT  deuda,  antiguedad_deuda,  tarifa,  classe,
            tipo_cliente,  subclasse, atividad_economica, deuda_extra,
            antiguedad_extra, today as fecha_sistema, deuda_max as divida_max, antigued_deuda_max,
            ind_corte_vip,ind_corte_br, ind_local_med, ind_padrao_agrup,
            cod_tipo_caixa
            from   CORPAR
            where  sucursal = {0}
              and  dir_ip = '{1}'", sucursal, dir_ip);

            var dt = ConsultaSql(sql);
            CorparDTO resultado = DataHelper.ConvertDataTableToEntity<CorparDTO>(dt);

            return resultado;
        }

        public CorcliDTO RetornaCorcli(string numero_cliente)
        {
            String sql;
            sql = String.Format(@"select cl.numero_cliente,cl.sucursal,cl.fecha_a_corte,TODAY as dia_hoje, cl.corr_corte,
                                    cl.estado_cliente,cl.tipo_cliente,cl.subclasse,cl.classe,
                                    cl.giro,cl.tarifa,cl.estado_suministro,cl.ind_cliente_vip,
                                    cl.corr_reaviso as corr_reaviso_cli,cl.tiene_notific,cl.cod_seg,cl.tiene_prepago,cl.corr_convenio,te.ind_local_med,te.ind_padrao_agrup, 
                                    te.tipo_caixa_med,nvl(te.fecha_parafuso,'') fecha_parafuso,te.acometida_retirada,cl.fecha_notifica 
                                    from CLIENTE cl ,OUTER TECNI te 
                                     WHERE 1=1
                                     AND te.numero_cliente = cl.numero_cliente 
                                     AND cl.numero_cliente = {0}
                                     group by 1,2,3,4,5,6,7,8,9,10,11,12,13,14,15,17,16,18,19,20,21,22,23,24
                            ", numero_cliente);

            var dt = ConsultaSql(sql);

            CorcliDTO resultado = DataHelper.ConvertDataTableToEntity<CorcliDTO>(dt);

            return resultado;
        }

        public List<CorcliDTO> RetornaCorcli(string lote, string localidade, string zona)
        {
            String sql;
            sql = String.Format(@"select cl.numero_cliente,cl.sucursal,cl.fecha_a_corte,TODAY as dia_hoje,cl.corr_corte,
	                            cl.estado_cliente,cl.tipo_cliente,cl.subclasse,cl.classe,
	                            cl.giro,cl.tarifa,cl.estado_suministro,cl.ind_cliente_vip,
	                            cl.corr_reaviso as corr_reaviso_cli ,cl.corr_convenio,cl.tiene_notific,cl.cod_seg,cl.tiene_prepago,te.ind_local_med,te.ind_padrao_agrup, 
	                            te.tipo_caixa_med,nvl(te.fecha_parafuso,'') fecha_parafuso,te.acometida_retirada,cl.fecha_notifica 
	                             from CLIENTE cl, 
	                             ,OUTER TECNI te 
	                             WHERE 1=1
	                             AND te.numero_cliente = cl.numero_cliente 
	                             AND cl.sector = {0} and cl.localidade = {1} and cl.zona = {2}
	                             group by 1,2,3,4,5,6,7,8,9,10,11,12,13,14,15,17,16,18,19,20,21,22,23,24", lote, localidade, zona);

            var dt = ConsultaSql(sql);
            List<CorcliDTO> lista = new List<CorcliDTO>();
            lista = DataHelper.ConvertDataTableToList<CorcliDTO>(dt);

            return lista;
        }

        public string RetornaCaixa(string codigo)
        {
            String sql;
            if (string.IsNullOrEmpty(codigo))
            {
                sql = String.Format(@"select valor_alf[2]
                                         from  TABLA TA
                                         where ta.sucursal = '0000' and
                                               ta.codigo = {0} and
                                               ta.nomtabla = 'TIPCXA' and
                                               (fecha_desactivac is null or fecha_desactivac > today)", codigo);

            }
            else
            {
                sql = String.Format(@"select valor_alf[2]
                                         into :ind_tipcxa
                                         from  TECNI TE , TABLA TA
                                         where te.numero_cliente = :numero_cliente and
                                               ta.sucursal = '0000' and
                                               ta.codigo = te.tipo_caixa_med and
                                               ta.nomtabla = 'TIPCXA' and
                                               (fecha_desactivac is null or fecha_desactivac > today)");
            }
            var dt = ConsultaSql(sql);


            return !string.IsNullOrEmpty(dt.Rows[0][0].ToString()) ? dt.Rows[0][0].ToString() : "";
        }

        public int RetornaAcaoUltimoRecorte(string numero_cliente)
        {
            String sql;
            sql = String.Format(@"SELECT  correp.acc_realizada_cor
                                      FROM cliente, correp, tipo_corte
                                      WHERE cliente.numero_cliente = correp.numero_cliente
                                      and correp.tipo_corte = tipo_corte.tipo_corte
                                      and (tipo_corte.clientes_asf = 'S'
                                          or tipo_corte.cod_operacao = 'R')
                                      and correp.numero_cliente = {0}
                                      AND correp.corr_corte = cliente.corr_corte", numero_cliente);

            var dt = ConsultaSql(sql);
            return !string.IsNullOrEmpty(dt.Rows[0][0].ToString()) ? Convert.ToInt32(dt.Rows[0][0].ToString()) : 0;
        }

        public int RetornaDadosAutoReligamento(string numero_cliente, int corr_corte)
        {
            String sql;
            sql = String.Format(@"select count(*)
                                  from   CORREP
                                  where numero_cliente = {0}
                                  and corr_corte = {1}
                                  and ind_auto_relig = 'S'", numero_cliente, corr_corte);

            var dt = ConsultaSql(sql);
            return !string.IsNullOrEmpty(dt.Rows[0][0].ToString()) ? Convert.ToInt32(dt.Rows[0][0].ToString()) : 0;
        }

        public int RetornaCNR(string numero_cliente)
        {
            String sql;
            sql = String.Format(@"select count(*) 
                                  from cnr
           		                  where numero_cliente = {0}
           		                  and cod_estado_result in ('51', '53')", numero_cliente);

            var dt = ConsultaSql(sql);
            return !string.IsNullOrEmpty(dt.Rows[0][0].ToString()) ? Convert.ToInt32(dt.Rows[0][0].ToString()) : 0;
        }

        public string ConsultaCorteEfetivo(int int_acao, string sucursal)
        {
            String sql;
            sql = String.Format(@"   $SELECT Valor_Alf[13] INTO :alf_corte_efet
                                     FROM TABLA
                                     WHERE nomtabla = 'ACRECO' AND
                                     codigo   = {0}  AND
                                     sucursal = {1} AND
                                     fecha_desactivac is null;", int_acao, sucursal);

            var dt = ConsultaSql(sql);
            return !string.IsNullOrEmpty(dt.Rows[0][0].ToString()) ? dt.Rows[0][0].ToString() : "";
        }

        public int RetornaQuantidadeCorte(string numero_cliente)
        {
            String sql;
            sql = String.Format(@"select count(*)
                                 FROM corsoco
                                 WHERE numero_cliente = {0} and
                                        estado <> 'I'
                            ", numero_cliente);

            var dt = ConsultaSql(sql);
            return !string.IsNullOrEmpty(dt.Rows[0][0].ToString()) ? Convert.ToInt32(dt.Rows[0][0].ToString()) : 0;
        }

        public int RetornaQuantidadeCorteHistorico(string numero_cliente, DateTime data_corte)
        {
            String sql;
            sql = String.Format(@"select count(*) as qtd_corte_hist from correp, tipo_corte
             where numero_cliente = {0} and
             tipo_corte.tipo_corte = correp.tipo_corte and
             tipo_corte.ind_massivo = 'S' and
             fecha_corte between {1} and today
                            ", numero_cliente, data_corte.ToString("MM/dd/yyyy"));

            var dt = ConsultaSql(sql);
            return !string.IsNullOrEmpty(dt.Rows[0][0].ToString()) ? Convert.ToInt32(dt.Rows[0][0].ToString()) : 0;
        }

        /// <summary>
        /// Recupera as solicitações de Anulação Temporal de Corte
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public List<CorplazoDTO> ObterAnulacaoTemporalCorte(CorplazoDTO obj)
        {
            if (obj == null)
                return new List<CorplazoDTO>();

            if (this.empresa == Empresa.NaoIdentificada)
            {
                //TODO: gerar log antes de lançar erro
                throw new ArgumentException("Parâmetro empresa obrigatório para a consulta de religações.");
            }

            #region Prepara a consulta básica

            StringBuilder sql = new StringBuilder("SELECT * FROM corplazo WHERE 1=1 ");

            if (!string.IsNullOrEmpty(obj.tipo))
                sql.AppendFormat("AND tipo = '{0}' ", obj.tipo);

            if (!string.IsNullOrEmpty(obj.sucursal))
                sql.AppendFormat("AND sucursal = '{0}' ", obj.sucursal);

            if (!string.IsNullOrEmpty(obj.rol))
                sql.AppendFormat("AND rol = '{0}' ", obj.rol);

            if (!string.IsNullOrEmpty(obj.motivo))
                sql.AppendFormat("AND motivo = '{0}' ", obj.motivo);

            if (!string.IsNullOrEmpty(obj.numero_cliente))
                sql.AppendFormat("AND numero_cliente = {0} ", obj.numero_cliente);

            if (obj.dias > 0)
                sql.AppendFormat("AND dias = {0} ", obj.numero_cliente);

            if (!string.IsNullOrEmpty(obj.numero_ordem))
                sql.AppendFormat("AND numero_ordem = '{0}' ", obj.numero_ordem);

            if (!string.IsNullOrEmpty(obj.oficina))
                sql.AppendFormat("AND oficina = '{0} '", obj.oficina);

            if (!string.IsNullOrEmpty(obj.ind_notifica))
                sql.AppendFormat("AND ind_notifica = '{0}' ", obj.ind_notifica);

            //TODO: implementar melhor lógica para consulta de Data, sem que seja somente com comparativo de igualdade
            //if (!string.IsNullOrEmpty(obj.fecha_solicitud))
            //    sql.AppendFormat("AND fecha_solicitud = '{0}' ", obj.fecha_solicitud);

            #endregion

            IList<CorplazoDTO> _lst = new List<CorplazoDTO>();
            _lst = DataHelper.ConvertDataTableToList<CorplazoDTO>(ConsultaSql(sql.ToString()));

            return (List<CorplazoDTO>)_lst;
        }

        /// <summary>
        /// Retorna os dados dos motivos de corte para um cliente informado.
        /// </summary>
        /// <param name="pNumeroCliente">Cliente que se deseja obter os motivos de corte registrados.</param>
        /// <param name="pCorrelativoCorte">O identificador de um motivo espcífico ou -1 para retornar todos os motivos registrados.</param>
        /// <returns></returns>
        public List<TablaDTO> ObterMotivosCorte(decimal pNumeroCliente, int pCorrelativoCorte)
        {
            if (empresa == Empresa.NaoIdentificada)
            {
                //TODO: gerar log antes de lançar erro
                throw new ArgumentException("Parâmetro empresa obrigatório para a consulta do motivo de corte de um cliente.");
            }

            #region Prepara a consulta básica

            StringBuilder sql = new StringBuilder("select * from tabla, hisreav ");
            sql.AppendFormat("where numero_cliente = {0} ");
            sql.Append("and sucursal = '0000' ");
            sql.Append("and nomtabla = 'CORMOT' ");
            sql.Append("and codigo = motivo_corte ");

            if (pCorrelativoCorte == -1)
                sql.AppendFormat("and corr_reaviso = {0} ", pCorrelativoCorte);

            #endregion

            IList<TablaDTO> _lst = new List<TablaDTO>();
            _lst = DataHelper.ConvertDataTableToList<TablaDTO>(ConsultaSql(sql.ToString()));

            return (List<TablaDTO>)_lst;
        }

        /// <summary>
        /// Atualiza uma suspensão temporal de corte com os dados que estiverem preenchidos no objeto instanciado.
        /// </summary>
        /// <param name="obj">As propriedades numero_cliente e fecha_solicitud são chaves do registro a ser atualizado.</param>
        /// <returns></returns>
        public bool AtualizarSuspensaoCorte(CorplazoDTO obj, DBProviderInformix conn)
        {
            const string VIRGULA = ",";
            StringBuilder sql = new StringBuilder("UPDATE CORPLAZO SET ");
            StringBuilder update = new StringBuilder();

            if (obj.dias.HasValue)
                update.AppendFormat("{0}dias = {1} ", update.Length > 0 ? VIRGULA : string.Empty, obj.dias);

            if (string.IsNullOrEmpty(obj.ind_notifica))
                update.AppendFormat("{0}ind_notifica = '{1}' ", update.Length > 0 ? VIRGULA : string.Empty, obj.ind_notifica);

            if (string.IsNullOrEmpty(obj.motivo))
                update.AppendFormat("{0}motivo = '{1}' ", update.Length > 0 ? VIRGULA : string.Empty, obj.motivo);

            if (string.IsNullOrEmpty(obj.numero_ordem))
                update.AppendFormat("{0}numero_ordem = '{1}' ", update.Length > 0 ? VIRGULA : string.Empty, obj.numero_ordem);

            if (string.IsNullOrEmpty(obj.oficina))
                update.AppendFormat("{0}oficina = '{1}' ", update.Length > 0 ? VIRGULA : string.Empty, obj.oficina);

            if (string.IsNullOrEmpty(obj.rol))
                update.AppendFormat("{0}rol = '{1}' ", update.Length > 0 ? VIRGULA : string.Empty, obj.rol);

            if (string.IsNullOrEmpty(obj.sucursal))
                update.AppendFormat("{0}sucursal = '{1}' ", update.Length > 0 ? VIRGULA : string.Empty, obj.sucursal);

            if (string.IsNullOrEmpty(obj.tipo))
                update.AppendFormat("{0}tipo = '{1}' ", update.Length > 0 ? VIRGULA : string.Empty, obj.tipo);

            sql.AppendFormat(" {0} ", update.ToString());

            sql.AppendFormat("WHERE numero_cliente = {0} ", obj.numero_cliente);
            sql.AppendFormat("AND date(fecha_solicitud) = {0} ", obj.fecha_solicitud.Value.ToString("YYYY-MM-dd HH:mm"));

            return ExecutarSql(sql.ToString(), conn);
        }

        
        
		#region PANGEA.SalesForce.Corte-Repo
        public RespuestaCorteRepoDTO ConsultarCorteExecutado(ParamCorteRepoDTO corte)
        {
            RespuestaCorteRepoDTO Respuesta = null;
            StringBuilder sql = new StringBuilder();            

            sql.Append("SELECT 	DISTINCT																		");
            sql.Append("    os.numero_ordem,                                                                    ");
            sql.Append("	num_ordem_serv_crt,                                                                 ");
            sql.Append("    c.numero_cliente, corr_corte,                                                       ");
            sql.Append("    (SELECT sociedad_sie2000 FROM insta) AS empresa,                                    ");
            sql.Append("    (SELECT sociedad_sie2000 FROM insta)||'CORTE'||os.numero_ordem AS id_corte_repo,    ");
            sql.Append("    'C' AS tipo_registro,                                                                ");
            sql.Append("    c.motivo_corte,                                                     ");
            sql.Append("    TRIM(tcormot.codigo) ||'C'||(SELECT sociedad_sie2000 FROM insta) AS motivo,        ");
            sql.Append("    TRIM(os.estado) ||'C'||(SELECT sociedad_sie2000 FROM insta) AS estado_ordem,        ");
            sql.Append("    c.fecha_corte, data_solic_corte,                                                    ");
            sql.Append("    TRIM(acc_realizada_cor) ||'C'||(SELECT sociedad_sie2000 FROM insta) AS acc_realizada_cor, tacreco.descripcion AS acreco_descripcion,                       ");
            sql.Append("    TRIM(c.tipo_corte)||'C'||(SELECT sociedad_sie2000 FROM insta) AS tipo_corte, tc.descricao_tipo                                       ");
            sql.Append("FROM correp c ,  ordem_servico os , OUTER tipo_corte tc, OUTER  tabla tcormot , OUTER  tabla tacreco, OUTER tabla testord ");
            sql.Append("WHERE c.numero_cliente = os.numero_cliente  ");
            sql.Append("AND c.tipo_corte = tc.tipo_corte    ");
            sql.Append("AND c.motivo_corte = tcormot.codigo ");
            sql.Append("AND c.acc_realizada_cor = tacreco.codigo AND c.num_ordem_serv_crt = os.numero_ordem ");
            sql.Append("AND os.estado = testord.codigo  ");
            sql.Append("AND fecha_corte IS NOT NULL                                                           ");
            sql.Append("AND date(tc.data_ativacao) <= today                                                     ");
            sql.Append("AND (date(tc.data_desativacao) >= today OR tc.data_desativacao IS NULL)                 ");
            sql.Append("AND tcormot.NomTabla = 'CORMOT'                                                         ");
            sql.Append("AND tcormot.Sucursal = '0000'                                                           ");
            sql.Append("AND (tcormot.fecha_activacion <= today)                                                 ");
            sql.Append("AND (tcormot.fecha_desactivac >= today OR tcormot.fecha_desactivac IS NULL)             ");
            sql.Append("AND tacreco.NomTabla = 'ACRECO'                                                         ");
            sql.Append("AND tacreco.Sucursal = '0000'                                                           ");
            sql.Append("AND (tacreco.fecha_activacion <= today)                                                 ");
            sql.Append("AND (tacreco.fecha_desactivac >= today OR tacreco.fecha_desactivac IS NULL)             ");
            sql.Append("AND testord.NomTabla = 'ESTORD'                                                         ");
            sql.Append("AND testord.Sucursal = '0000'                                                           ");
            sql.Append("AND (testord.fecha_activacion <= today)                                                 ");
            sql.Append("AND (testord.fecha_desactivac >= today OR testord.fecha_desactivac IS NULL)             ");
            sql.AppendFormat("AND c.numero_cliente = {0} ", corte.NumeroSuministro.ToString());
            //sql.AppendFormat(" AND os.data_ingresso >= '{0}'", corte.FechaInicio.ToString("yyyy-MM-dd HH:mm"));
            //sql.AppendFormat(" AND os.data_ingresso <= '{0}'", corte.FechaFin.ToString("yyyy-MM-dd HH:mm"));

            DataTable result = ConsultaSql(sql.ToString());

            if (result.Rows.Count > 0)
            {
                Respuesta = new RespuestaCorteRepoDTO();                
                Respuesta.NumeroSuministro = Convert.ToInt64(result.Rows[0]["numero_cliente"]);
                Respuesta.CodigoEmpresa = result.Rows[0]["empresa"].ToString();

                foreach(DataRow row in result.Rows)
                {
                    var corteSolicitado = new CorteRepoSolicitadoDTO();
                    corteSolicitado.NumeroOrden = row["numero_ordem"].ToString();
                    corteSolicitado.IDCorteRepo = row["id_corte_repo"].ToString();
                    corteSolicitado.TipoRegistro = row["tipo_registro"].ToString();
                    corteSolicitado.Motivo = row["motivo"].ToString().Trim();
                    corteSolicitado.Estado = row["estado_ordem"].ToString().Trim();
                    //corteSolicitado.FechaEjecucion = String.Format("{0:MM/dd/yyyy }", row["fecha_corte"]).ToString();
                    corteSolicitado.FechaEjecucion = String.Format("{0:s}", row["fecha_corte"]).ToString();
                    corteSolicitado.FechaSolicitud = String.Format("{0:s}", row["data_solic_corte"]).ToString();
                    corteSolicitado.AccionRealizada = row["acc_realizada_cor"].ToString().Trim();
                    corteSolicitado.Tipo = row["tipo_corte"].ToString().Trim();

                    Respuesta.ListaCortesReposSolicitados.Add(corteSolicitado);
                }

                
            }

            return Respuesta;
        }

        public RespuestaCorteRepoDTO ConsultarCorteExecutadoGA(ParamCorteRepoDTO corte)
        {
            RespuestaCorteRepoDTO Respuesta = null;
            StringBuilder sql = new StringBuilder();            

            sql.Append("SELECT DISTINCT  ");
            sql.Append("    os.numero_ordem,                                                                    ");
            sql.Append("	num_ordem_serv_crt,                                                                 ");
            sql.Append("    c.numero_cliente, corr_corte,                                                       ");
            sql.Append("    (SELECT sociedad_sie2000 FROM insta) AS empresa,                                    ");
            sql.Append("    (SELECT sociedad_sie2000 FROM insta)||'CORTE'||os.numero_ordem AS id_corte_repo,    ");
            sql.Append("    'C' AS tipo_registro,                                                                ");
            sql.Append("    c.motivo_corte, TRIM(tcormot.codigo) ||'C'||(SELECT sociedad_sie2000 FROM insta) AS motivo,                                                     ");
            sql.Append("    tcormot.descripcion,                                                      ");
            sql.Append("    testord.codigo ||'C'||(SELECT sociedad_sie2000 FROM insta) AS estado_ordem,                                                ");
            sql.Append("    c.fecha_corte, data_solic_corte,                                                    ");
            sql.Append("    TRIM(acc_realizada_cor)||'C'||(SELECT sociedad_sie2000 FROM insta) AS acc_realizada_cor, tacreco.descripcion AS acreco_descripcion,                       ");
            sql.Append("    TRIM(c.tipo_corte)||'C'||(SELECT sociedad_sie2000 FROM insta) AS tipo_corte, tc.descricao_tipo                                                    ");
            sql.Append("FROM grandes:correp c ,  grandes:ordem_servico os , OUTER clientes:tipo_corte tc, OUTER  grandes:tabla tcormot , OUTER  grandes:tabla tacreco, OUTER grandes:tabla testord ");
            sql.Append("WHERE c.numero_cliente = os.numero_cliente  ");
            sql.Append("AND c.tipo_corte = tc.tipo_corte    ");
            sql.Append("AND c.motivo_corte = tcormot.codigo ");
            sql.Append("AND c.acc_realizada_cor = tacreco.codigo AND c.num_ordem_serv_crt = os.numero_ordem ");
            sql.Append("AND os.estado = testord.codigo  ");
            sql.Append("AND fecha_corte IS NOT NULL                                                           ");
            sql.Append("AND date(tc.data_ativacao) <= today                                                     ");
            sql.Append("AND (date(tc.data_desativacao) >= today OR tc.data_desativacao IS NULL)                 ");
            sql.Append("AND tcormot.NomTabla = 'CORMOT'                                                         ");
            sql.Append("AND tcormot.Sucursal = '0000'                                                           ");
            sql.Append("AND (tcormot.fecha_activacion <= today)                                                 ");
            sql.Append("AND (tcormot.fecha_desactivac >= today OR tcormot.fecha_desactivac IS NULL)             ");
            sql.Append("AND tacreco.NomTabla = 'ACRECO'                                                         ");
            sql.Append("AND tacreco.Sucursal = '0000'                                                           ");
            sql.Append("AND (tacreco.fecha_activacion <= today)                                                 ");
            sql.Append("AND (tacreco.fecha_desactivac >= today OR tacreco.fecha_desactivac IS NULL)             ");
            sql.Append("AND testord.NomTabla = 'ESTORD'                                                         ");
            sql.Append("AND testord.Sucursal = '0000'                                                           ");
            sql.Append("AND (testord.fecha_activacion <= today)                                                 ");
            sql.Append("AND (testord.fecha_desactivac >= today OR testord.fecha_desactivac IS NULL)             ");
            sql.AppendFormat("AND c.numero_cliente = {0} ", corte.NumeroSuministro.ToString());
            //sql.AppendFormat(" AND os.data_ingresso >= {0} ", corte.FechaInicio.ToString("yyyy-MM-dd HH:mm"));
            //sql.AppendFormat(" AND os.data_ingresso <= {0}", corte.FechaFin.ToString("yyyy-MM-dd HH:mm"));

            DataTable result = ConsultaSql(sql.ToString());

            if (result.Rows.Count > 0)
            {
                Respuesta = new RespuestaCorteRepoDTO();
                Respuesta.NumeroSuministro = Convert.ToInt64(result.Rows[0]["numero_cliente"]);
                Respuesta.CodigoEmpresa = result.Rows[0]["empresa"].ToString();

                foreach (DataRow row in result.Rows)
                {
                    var corteSolicitado = new CorteRepoSolicitadoDTO();
                    corteSolicitado.NumeroOrden = row["numero_ordem"].ToString();
                    corteSolicitado.IDCorteRepo = row["id_corte_repo"].ToString();
                    corteSolicitado.TipoRegistro = row["tipo_registro"].ToString();
                    corteSolicitado.Motivo = row["motivo"].ToString().Trim();
                    corteSolicitado.Estado = row["estado_ordem"].ToString().Trim();
                    corteSolicitado.FechaEjecucion = String.Format("{0:s}", row["fecha_corte"]).ToString();
                    corteSolicitado.FechaSolicitud = String.Format("{0:s}", row["data_solic_corte"]).ToString();
                    corteSolicitado.AccionRealizada = row["acc_realizada_cor"].ToString().Trim();
                    corteSolicitado.Tipo = row["tipo_corte"].ToString().Trim();

                    Respuesta.ListaCortesReposSolicitados.Add(corteSolicitado);
                }
            }

            return Respuesta;
        }

        #endregion

       


        /// <summary>
        /// Retorna o histórico de corte e religação de um cliente.
        /// </summary>
        /// <param name="filtroRelig"></param>
        /// <returns></returns>
        public DataTable ObterHistoricoCorteReligacao(CorrepDTO filtroRelig)
        {
            if (filtroRelig == null || string.IsNullOrEmpty(filtroRelig.numero_cliente))
                throw new ArgumentException("Parâmetros insuficientes para consultar o histórico de corte/religação.");

            StringBuilder sql = new StringBuilder("");
            sql.AppendFormat(@" SELECT numero_cliente, tipo_corte, corr_corte, fecha_corte, motivo_corte,
                                corr_reaviso, fecha_reposicion, tipo_religacao, funcionario_repo
                                FROM correp
                                WHERE numero_cliente = {0} ", filtroRelig.numero_cliente);

            if (filtroRelig.corr_corte.HasValue)
                sql.AppendFormat(" AND corr_corte = {0} ", filtroRelig.corr_corte.Value);

            return ConsultaSql(sql.ToString());
        }


        /// <summary>
        /// Retorna os cortes em andamento.
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public DataTable ObterCortesEmAndamento(CorsoreDTO obj)
        {
            if (obj == null || string.IsNullOrEmpty(obj.numero_cliente))
                throw new ArgumentException("Parâmetros insuficientes para consultar o histórico de corte/religação.");

            StringBuilder sql = new StringBuilder("");
            sql.AppendFormat(@" SELECT sucursal, numero_solicitud, tipo_religacao, estado, numero_cliente,
                                        motivo_sol, corr_corte, tipo_corte, funcionario, fecha_solicitud,
                                        observacoes, numero_ordem, solic_smc
                                FROM corsore
                                WHERE numero_cliente = {0} ", obj.numero_cliente);

            if (obj.corr_corte.HasValue)
                sql.AppendFormat(" AND corr_corte = {0} ", obj.corr_corte);

            return ConsultaSql(sql.ToString());
        }

        public bool IngressarSuspensaoCorte(CorsoreDTO obj, DBProviderInformix conn)
        {
            throw new NotImplementedException();
        }



        public bool IngressarCorteAndamento(CorsoreDTO obj, DBProviderInformix conn)
        {
            StringBuilder sql = new StringBuilder("INSERT INTO corsore ");
            sql.Append(@" sucursal,
                          numero_solicitud,
                          tipo_religacao,
                          estado,
                          numero_cliente,
                          motivo_sol,
                          tipo_corte,
                          funcionario,
                          rol_repo,
                          fecha_solicitud,
                          oficina,
                          corr_corte,
                          numero_ordem)
                    VALUES ( ");

            //TODO: corrigir tipagens
            sql.AppendFormat(" {0} ", obj.sucursal);
            sql.AppendFormat(" {0} ", obj.numero_solicitud);
            sql.AppendFormat(" {0} ", obj.tipo_religacao);
            sql.AppendFormat(" {0} ", obj.estado);
            sql.AppendFormat(" {0} ", obj.numero_cliente);
            sql.AppendFormat(" {0} ", obj.motivo_sol);
            sql.AppendFormat(" {0} ", obj.tipo_corte);
            sql.AppendFormat(" {0} ", obj.funcionario);
            sql.AppendFormat(" {0} ", obj.rol_repo);
            sql.AppendFormat(" {0} ", obj.fecha_solicitud);
            sql.AppendFormat(" {0} ", obj.oficina);
            sql.AppendFormat(" {0} ", obj.corr_corte);
            sql.AppendFormat(" {0} ", obj.numero_ordem); 

            return ExecutarSql(sql.ToString(), conn);
        }



        /// <summary>
        /// //TODO:
        /// </summary>
        /// <param name="numeroCliente"></param>
        /// <returns></returns>
        public DataTable ObterCorteAptoEOrder(string numeroCliente)
        {
            StringBuilder sql = new StringBuilder();
            sql.AppendFormat(@"    SELECT C.numero_cliente, C.nombre, C.sector, C.localidade, C.zona,
                                          C.correlativo_ruta, C1.corr_reaviso, C1.corr_corte, nvl(C1.numero_livro,"")
                                     FROM cliente C, corsoco C1
                                    WHERE C.numero-cliente = {0}
                                      AND C.numero_cliente     = A.numero_cliente
                                      AND C.estado_cliente in ('0','3')
                                      AND C.estado_suministro  = '0'
                                      AND C1.numero_cliente = C.numero_cliente
                                      AND C1.estado <> 'I'
                                      AND C1.ind_mail_enviado = 'N'
                                      AND C1.motivo_sol = '01'
				                      AND NOT EXISTS
				                        (select 1
				                           from eorder_transferencia
				                          where numero_cliente = c.numero_cliente
					                        and (numero_ordem = C1.numero_ordem or numero_livro = C1.numero_livro)
					                        and estado in ('12','14','15','16'))
                                 GROUP BY 1,2,3,4,5,6,8,9");

            return ConsultaSql(sql.ToString());
        }


        /// <summary>
        /// Retorna detalhes do Corte.
        /// </summary>
        /// <param name="numeroCliente"></param>
        /// <param name="tipoCorte">I-ndividual (padrão), M-assivo</param>
        /// <returns></returns>
        public DataTable ObterDadosCortePorTipo(string numeroCliente, string tipoCorte = "I")
        {
            StringBuilder sql = new StringBuilder();
            
            //TODO: converter forma de obter consulta pelo Tipo de Corte
            if ("M".Equals(tipoCorte))
            {
                sql.AppendFormat(@"SELECT 'CM',co.numero_cliente, co.numero_livro, co.numero_livro,
                      nvl(ge.numero_servico,'0'), e.razon_social[1,30],
                      trim(e.mail_principal), decode(lc.empreiteira,'30288', '',trim(e.mail_secundario)),
                      co.eletricista, lc.estado, nvl(tr.ind_impressao,'0'),
                      lc.sucursal,1, lc.empreiteira, co.numero_tarefa, 0
                 FROM corsoco co, livro_corte lc, outer geracao_empr ge, cotrat e,
                      OUTER tarefa_remessa tr
                WHERE co.numero_cliente = :cli_scrt_row.numero_cliente
                  AND co.estado = 'M'
                  AND co.corr_reaviso = :cli_scrt_row.corr_reaviso
                  AND lc.sucursal = co.sucursal
                  AND lc.numero_livro = co.numero_livro
				  AND lc.estado in ('01','02','03','07','97','08','09')
                  AND lc.empreiteira = e.contratista
                  AND co.numero_livro = ge.numero_livro
                  AND tr.tipo_remessa = 'CM'
                  AND tr.numero_remessa = co.numero_livro
                  AND tr.corr_tarefa = co.numero_tarefa");
            }
            else
            {
                sql.AppendFormat(@"SELECT 'OS',os.numero_cliente, os.numero_ordem,
                      vo.numero_protocolo, nvl(ge.numero_servico,'0'),
                      e.razon_social[1,30], trim(e.mail_principal),
                      decode(lc.empreiteira,'30288','',trim(e.mail_secundario)), vo.eletricista,
                      nvl(pr.estado,'0'), nvl(tr.ind_impressao,'0'),
                      os.sucursal_destino, nvl(os.corr_visita,'0'),
                      vo.empreiteira, vo.numero_tarefa, nvl(os.nro_gac,'0'),
                      os.estado
                 FROM ordem_servico os, servicos s, OUTER (visita_ordem vo, cotrat e,
                      OUTER protocolo_ordem pr, OUTER  geracao_empr ge,
                      OUTER tarefa_remessa tr)
                WHERE os.numero_cliente =  :cli_scrt_row.numero_cliente
                  AND os.tipo_ordem = s.tipo_ordem
                  AND os.tipo_servico = s.cod_servico
                  AND s.grupo = 1
                  AND os.estado not in ('04','09')
                  AND os.numero_ordem = vo.numero_ordem
                  AND os.corr_visita = vo.corr_visita
                  AND vo.empreiteira = e.contratista
                  AND vo.numero_protocolo = ge.numero_livro
                  AND pr.numero_protocolo = vo.numero_protocolo
                  AND tr.numero_remessa = vo.numero_protocolo
                  AND tr.corr_tarefa = vo.numero_tarefa");
            }

            return ConsultaSql(sql.ToString());
        }
    }
}

using Pangea.Dados.Base;
using Pangea.Dados.Solucoes;
using Pangea.Entidades;
using Pangea.Entidades.Enumeracao;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

namespace Pangea.Dados
{
    public abstract class SolucoesDAO : BaseDAO
    {
        public SolucoesDAO(Empresa empresa)
            : base(empresa)
        {

        }

        private static SolucoesAmplaDAO solucoesAmplaDAO = null;
        private static SolucoesCoelceDAO solucoesCoelceDAO = null;

        public static SolucoesDAO getInstanceSolucoesDAO(string empresa)
        {
            SolucoesDAO solucoesDAO = null;
            switch (int.Parse(empresa))
            {
                case (int)Empresa.RJ:
                    if (solucoesAmplaDAO == null)
                    {
                        solucoesAmplaDAO = new SolucoesAmplaDAO();
                    }

                    solucoesDAO = solucoesAmplaDAO;
                    break;
                case (int)Empresa.CE:
                    if (solucoesCoelceDAO == null)
                    {
                        solucoesCoelceDAO = new SolucoesCoelceDAO();
                    }

                    solucoesDAO = solucoesCoelceDAO;
                    break;
                default:
                    solucoesDAO = null;
                    break;
            }

            return solucoesDAO;
        }

        public List<Produto> buscaProdutoParceira(string loginParceira)
        {
            StringBuilder sql = new StringBuilder();
            Produto produto;
            List<Produto> listaProduto = new List<Produto>();
            
            sql.Append(" select ");
            sql.Append(" sp.empresa_parc ");
            sql.Append(" , trim(t.descripcion) as desc_empresa ");
            sql.Append(" , sp.cod_produto ");
            sql.Append(" , trim(sp.descripcion) as desc_produto ");
            sql.Append(" , sp.tipo_cobranca ");
            sql.Append(" , sp.valor_produto ");
            sql.Append(" , sp.valor_parcela ");
            sql.Append(" , sp.dias_cancel ");
            sql.Append(" , sp.dias_atraso ");
            sql.Append(" , trim(sp.codigo_facilidad) as codigo_facilidad ");
            sql.Append(" , trim(sp.cod_encargo_cob) as cod_encargo_cob ");
            sql.Append(" , trim(sp.cod_encargo_canc) as cod_encargo_canc ");
            sql.Append(" , trim(sp.cod_encargo_ind) as cod_encargo_ind ");
            sql.Append(" from solucoes_produto sp ");
            sql.Append(" , tabla t ");
            sql.Append(" where t.nomtabla = 'EMPPAR' ");
            sql.Append(" and t.sucursal = '0000' ");
            sql.Append(" and t.fecha_desactivac is null ");
            sql.Append(" and t.codigo = sp.empresa_parc ");
            sql.Append(" and sp.fecha_desactivac is null");
            //sql.Append(" and sp.empresa_parc = '" + empresaParc + "'");
            //sql.Append(" and sp.cod_produto = '" + codProduto + "'");
            sql.Append(" AND sp.empresa_parc in (select codigo ");
            sql.Append(" from tabla ");
            sql.Append(" where nomtabla = 'RELEMP' ");
            sql.Append(" and sucursal = '0000' ");
            sql.Append(" and fecha_desactivac is null ");
            sql.Append(" and valor_alf = '" +  loginParceira + "') ");


            DataTable dt = ConsultaSql(sql.ToString());

            if (dt.Rows.Count > 0)
            {

                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    produto = new Produto();

                    produto.empresa.codEmpresaParc = dt.Rows[0]["empresa_parc"].ToString();
                    produto.empresa.descricaoEmpresa = dt.Rows[0]["desc_empresa"].ToString();
                    produto.codProduto = dt.Rows[0]["cod_produto"].ToString();
                    produto.descricaoProduto = dt.Rows[0]["desc_produto"].ToString();
                    produto.tipoCobranca = dt.Rows[0]["tipo_cobranca"].ToString();
                    produto.valorProduto = double.Parse(dt.Rows[0]["valor_produto"].ToString());
                    produto.valorParcela = double.Parse(dt.Rows[0]["valor_parcela"].ToString());
                    produto.diasCancelRecente = int.Parse(dt.Rows[0]["dias_cancel"].ToString());
                    produto.diasCancelAtraso = int.Parse(dt.Rows[0]["dias_atraso"].ToString());
                    produto.codigoFacilidade = dt.Rows[0]["codigo_facilidad"].ToString();
                    produto.encargoCobranca = dt.Rows[0]["cod_encargo_cob"].ToString();
                    produto.encargoCancelamento = dt.Rows[0]["cod_encargo_canc"].ToString();
                    produto.encargoIndenizacao = dt.Rows[0]["cod_encargo_ind"].ToString();

                    listaProduto.Add(produto);
                    produto = null;
                }

            }
            else
            {
                produto = null;
            }

            return listaProduto;
        }

        public Produto buscaProduto(string empresaParc, string codProduto)
        {
            StringBuilder sql = new StringBuilder();
            Produto produto = new Produto();

            sql.Append(" select ");
            sql.Append(" sp.empresa_parc ");
            sql.Append(" , trim(t.descripcion) as desc_empresa ");
            sql.Append(" , sp.cod_produto ");
            sql.Append(" , trim(sp.descripcion) as desc_produto ");
            sql.Append(" , sp.tipo_cobranca ");
            sql.Append(" , sp.valor_produto ");
            sql.Append(" , sp.valor_parcela ");
            sql.Append(" , sp.dias_cancel ");
            sql.Append(" , sp.dias_atraso ");
            sql.Append(" , trim(sp.codigo_facilidad) as codigo_facilidad ");
            sql.Append(" , trim(sp.cod_encargo_cob) as cod_encargo_cob ");
            sql.Append(" , trim(sp.cod_encargo_canc) as cod_encargo_canc ");
            sql.Append(" , trim(sp.cod_encargo_ind) as cod_encargo_ind ");
            sql.Append(" from solucoes_produto sp ");
            sql.Append(" , tabla t ");
            sql.Append(" where t.nomtabla = 'EMPPAR' ");
            sql.Append(" and t.sucursal = '0000' ");
            sql.Append(" and t.fecha_desactivac is null ");
            sql.Append(" and t.codigo = sp.empresa_parc ");
            sql.Append(" and sp.fecha_desactivac is null");
            sql.Append(" and sp.empresa_parc = '" + empresaParc + "'");
            sql.Append(" and sp.cod_produto = '" + codProduto + "'");

            DataTable dt = ConsultaSql(sql.ToString());

            if (dt.Rows.Count > 0)
            {

                produto.empresa.codEmpresaParc = dt.Rows[0]["empresa_parc"].ToString();
                produto.empresa.descricaoEmpresa = dt.Rows[0]["desc_empresa"].ToString();
                produto.codProduto = dt.Rows[0]["cod_produto"].ToString();
                produto.descricaoProduto = dt.Rows[0]["desc_produto"].ToString();
                produto.tipoCobranca = dt.Rows[0]["tipo_cobranca"].ToString();
                produto.valorProduto = double.Parse(dt.Rows[0]["valor_produto"].ToString());
                produto.valorParcela = double.Parse(dt.Rows[0]["valor_parcela"].ToString());
                produto.diasCancelRecente = int.Parse(dt.Rows[0]["dias_cancel"].ToString());
                produto.diasCancelAtraso = int.Parse(dt.Rows[0]["dias_atraso"].ToString());
                produto.codigoFacilidade = dt.Rows[0]["codigo_facilidad"].ToString();
                produto.encargoCobranca = dt.Rows[0]["cod_encargo_cob"].ToString();
                produto.encargoCancelamento = dt.Rows[0]["cod_encargo_canc"].ToString();
                produto.encargoIndenizacao = dt.Rows[0]["cod_encargo_ind"].ToString();
            }
            else
            {
                produto = null;
            }

            return produto;
        }


        public List<ProdutoCliente> buscaAdesoesProdutoCliente(int numeroCliente, string empresaParc, string codProduto)
        {
            return buscaAdesoesProdutoCliente(numeroCliente, empresaParc, codProduto, "");
        }

        public List<ProdutoCliente> buscaAdesoesProdutoCliente(int numeroCliente, string loginParceira)
        {
            return buscaAdesoesProdutoCliente(numeroCliente, "", "", loginParceira);
        }

        public List<ProdutoCliente> buscaAdesoesProdutoCliente(int numeroCliente, string empresaParc, string codProduto, string loginParceira)
        {

            List<ProdutoCliente> result = new List<ProdutoCliente>();
            ProdutoCliente p = new ProdutoCliente();

            StringBuilder sql = new StringBuilder();

            sql.Append(" select ");
            sql.Append(" sp.empresa_parc ");
            sql.Append(" , trim(t.descripcion) as desc_empresa ");
            sql.Append(" , sp.cod_produto ");
            sql.Append(" , trim(sp.descripcion) as desc_produto ");
            sql.Append(" , sp.tipo_cobranca ");
            sql.Append(" , round(sp.valor_produto, 2) as valor_produto ");
            sql.Append(" , round(sp.valor_parcela, 2) as valor_parcela ");
            sql.Append(" , sp.dias_cancel ");
            sql.Append(" , sp.dias_atraso ");
            sql.Append(" , sc.numero_cliente ");
            sql.Append(" , sc.dv_numero_cliente ");
            sql.Append(" , sc.fecha_inclusao ");
            sql.Append(" , sc.fecha_exclusao ");
            sql.Append(" , sc.estado ");
            sql.Append(" , sc.fecha_processo ");
            sql.Append(" , sc.ind_fatura_carona ");
            sql.Append(" , sc.fecha_pagamento ");
            sql.Append(" , sc.fecha_carona ");
            sql.Append(" , sc.fecha_envio ");
            sql.Append(" , sc.fecha_anulacao ");
            sql.Append(" , sc.fecha_certif ");
            sql.Append(" , nvl(sc.numero_sorteio, 0) as numero_sorteio ");
            sql.Append(" , case when exists (select t1.codigo from tabla t1 ");
            sql.Append("  where t1.codigo = sc.canal_venda_emppar ");
            sql.Append("  and t1.nomtabla = 'CANVEN' ");
            sql.Append("  and t1.sucursal = '0000' ");
            sql.Append("  and t1.fecha_desactivac is null) then ");
            sql.Append("  sc.canal_venda_emppar ");
            sql.Append("  else (select sc.canal_venda from tabla t1 ");
            sql.Append("  where t1.codigo = sc.canal_venda ");
            sql.Append("  and t1.nomtabla = 'SOLCAN' ");
            sql.Append("  and t1.sucursal = '0000' ");
            sql.Append("  and t1.fecha_desactivac is null) end as canal_venda ");
            sql.Append(" , trim(sc.rol_inclusao) as rol_inclusao ");
            sql.Append(" , NVL(sc.numero_emp_parc, 0) as numero_emp_parc ");
            sql.Append(" , trim(sc.rol_canc) as rol_canc ");
            sql.Append(" , sc.obs_cancel ");
            sql.Append(" from solucoes_cliente sc ");
            sql.Append(" , solucoes_produto sp ");
            sql.Append(" , tabla t ");
            sql.Append(" where 1=1 ");
            sql.Append(" and t.nomtabla = 'EMPPAR' ");
            sql.Append(" and t.codigo = sc.empresa_parc ");
            sql.Append(" and t.sucursal = '0000' ");
            sql.Append(" and t.fecha_desactivac is null ");
            sql.Append(" and sc.empresa_parc = sp.empresa_parc ");
            sql.Append(" and sc.cod_produto = sp.cod_produto ");
            sql.Append(" and sp.fecha_desactivac is null ");
            sql.Append(" and sc.estado in ('2', '3', '7', '9') ");
            sql.Append(" and sc.numero_cliente = " + numeroCliente);

            if (loginParceira == "")
            {

                sql.Append(" and sc.empresa_parc = '" + empresaParc + "'");
                sql.Append(" and sc.cod_produto = '" + codProduto + "'");
                sql.Append(" order by sc.fecha_inclusao desc; ");
            }
            else if (empresaParc == "" && codProduto == "" && loginParceira != "")
            {
                sql.Append(" and sc.empresa_parc in ");
                adicionaLoginParceira(sql, loginParceira);
                sql.Append(" order by sc.empresa_parc, sc.cod_produto, sc.fecha_inclusao desc; ");
            }
            else
            {
                return null;
            }



            DataTable dt = ConsultaSql(sql.ToString());

            if (dt.Rows.Count > 0)
            {
                DateTime tmpData;
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    //p.produto.empresa.codEmpresaParc = dt.Rows[i]["empresa_parc"].ToString();
                    //p.produto.empresa.descricaoEmpresa = dt.Rows[i]["desc_empresa"].ToString();
                    //p.produto.codProduto = dt.Rows[i]["cod_produto"].ToString();
                    //p.produto.descricaoProduto = dt.Rows[i]["desc_produto"].ToString();
                    //p.produto.tipoCobranca = dt.Rows[i]["tipo_cobranca"].ToString();
                    //p.produto.valorProduto = double.Parse(dt.Rows[i]["valor_produto"].ToString());
                    //p.produto.valorParcela = double.Parse(dt.Rows[i]["valor_parcela"].ToString());
                    //p.produto.diasCancelRecente = int.Parse(dt.Rows[i]["dias_cancel"].ToString());
                    //p.produto.diasCancelAtraso = int.Parse(dt.Rows[i]["dias_atraso"].ToString());
                    p.empresaParc = dt.Rows[i]["empresa_parc"].ToString();
                    p.codProduto = dt.Rows[i]["cod_produto"].ToString();
                    p.numeroCliente = int.Parse(dt.Rows[i]["numero_cliente"].ToString());

                    int tmpDvNumeroCliente;

                    if (int.TryParse(dt.Rows[i]["dv_numero_cliente"].ToString(), out tmpDvNumeroCliente))
                    {
                        p.dvNumeroCliente = tmpDvNumeroCliente;
                    }
                    
                    

                    //p.dvNumeroCliente = int.Parse(dt.Rows[i]["dv_numero_cliente"].ToString());

                    if (DateTime.TryParse(dt.Rows[i]["fecha_inclusao"].ToString(), out tmpData))
                    {
                        p.dataAdesao = tmpData.ToString("MM/dd/yyyy");
                    }

                    if (DateTime.TryParse(dt.Rows[i]["fecha_exclusao"].ToString(), out tmpData))
                    {
                        p.dataCancelamento = tmpData.ToString("MM/dd/yyyy");
                    }


                    p.estado = dt.Rows[i]["estado"].ToString();

                    if (DateTime.TryParse(dt.Rows[i]["fecha_processo"].ToString(), out tmpData))
                    {
                        p.dataProcessamento = tmpData.ToString("MM/dd/yyyy");
                    }


                    p.indicaFaturaCarona = dt.Rows[i]["ind_fatura_carona"].ToString();

                    if (DateTime.TryParse(dt.Rows[i]["fecha_pagamento"].ToString(), out tmpData))
                    {
                        p.dataPagamento = tmpData.ToString("MM/dd/yyyy");
                    }

                    if (DateTime.TryParse(dt.Rows[i]["fecha_carona"].ToString(), out tmpData))
                    {
                        p.dataCriacaoFaturaCarona = tmpData.ToString("MM/dd/yyyy");
                    }

                    if (DateTime.TryParse(dt.Rows[i]["fecha_envio"].ToString(), out tmpData))
                    {
                        p.dataEnvioCadastro = tmpData.ToString("MM/dd/yyyy");
                    }

                    if (DateTime.TryParse(dt.Rows[i]["fecha_anulacao"].ToString(), out tmpData))
                    {
                        p.dataAnulacao = tmpData.ToString("MM/dd/yyyy");
                    }

                    if (DateTime.TryParse(dt.Rows[i]["fecha_certif"].ToString(), out tmpData))
                    {
                        p.dataEnvioCertificado = tmpData.ToString("MM/dd/yyyy");
                    }


                    p.numeroSorteio = int.Parse(dt.Rows[i]["numero_sorteio"].ToString());
                    p.codCanalVenda = dt.Rows[i]["canal_venda"].ToString();
                    p.usuarioAdesao = dt.Rows[i]["rol_inclusao"].ToString();
                    p.numeroClienteEmpresaParceira = int.Parse(dt.Rows[i]["numero_emp_parc"].ToString());
                    p.usuarioCancelamento = dt.Rows[i]["rol_canc"].ToString();
                    p.obsCancelamento = dt.Rows[i]["obs_cancel"].ToString();

                    result.Add(p);
                    p = null;
                    p = new ProdutoCliente();
                }

            }

            return result;
        }



        public bool cancelarProdutoCliente(SolicitacaoTipoD sol, DBProviderInformix informix)
        {
            StringBuilder sql = new StringBuilder();

            sql.Append(" UPDATE solucoes_cliente ");
            sql.Append(" set estado = '7' ");
            sql.Append(" , fecha_exclusao = today ");
            sql.Append(" , rol_canc = 'ONLINE' ");
            sql.Append(" , obs_cancel = '" + sol.codOcorr + "' ");
            sql.Append(" WHERE numero_cliente = " + sol.numeroCliente);
            sql.Append(" AND empresa_parc = '" + sol.empresaParc + "' ");
            sql.Append(" AND cod_produto = '" + sol.codProduto + "' ");
            sql.Append(" AND estado = '2'");

            return ExecutarSql(sql.ToString(), informix);
        }

        public bool cancelarCobrancaValorInformado(SolicitacaoTipoD sol, DBProviderInformix informix)
        {
            StringBuilder sql = new StringBuilder();

            sql.Append(" UPDATE solucoes_encargos ");
            sql.Append(" set estado = 'C' ");
            sql.Append(" , fecha_termino = today ");
            sql.Append(" WHERE numero_cliente = " + sol.numeroCliente);
            sql.Append(" AND empresa_parc = '" + sol.empresaParc + "'");
            sql.Append(" AND cod_produto = '" + sol.codProduto + "'");
            sql.Append(" AND codigo_ocorr = '60' ");
            sql.Append(" AND estado = 'V'");

            return ExecutarSql(sql.ToString(), informix);

        }

        public bool cancelarCobrancaValorPredefinido(SolicitacaoTipoD sol, DBProviderInformix informix)
        {
            StringBuilder sql = new StringBuilder();

            sql.Append(" update fac_cli fc ");

            sql.Append(" set fc.estado = 'C' ");
            sql.Append(" , fc.fecha_termino = today ");
            sql.Append(" , fc.usuario_termino = 'ONLINE' ");

            sql.Append(" where fc.rowid = ");
            sql.Append(" ( ");
            sql.Append(" select ");
            sql.Append(" fc1.rowid ");
            sql.Append(" from fac_cli fc1 ");
            sql.Append(" , solucoes_produto sp ");
            sql.Append(" where 1=1 ");
            sql.Append(" and fc1.codigo_facilidad = sp.codigo_facilidad ");
            sql.Append(" and sp.fecha_desactivac is null ");
            sql.Append(" and fc1.estado = 'V' ");
            sql.Append(" and sp.empresa_parc = + '" + sol.empresaParc + "' ");
            sql.Append(" and sp.cod_produto = '" + sol.codProduto + "' ");
            sql.Append(" and fc1.numero_cliente = " + sol.numeroCliente);
            sql.Append(" ) ");


            return ExecutarSql(sql.ToString(), informix);

        }

        //public bool inserirOcorrencia(int numeroCliente, string empresaParc, string codProduto, string codOcorr, DBProviderInformix informix)
        public bool inserirOcorrencia(RetornoSolicitacaoTipoC ocorr, DBProviderInformix informix)
        {
            StringBuilder sql = new StringBuilder();

            sql.Append(" INSERT INTO solucoes_ocorr ");
            sql.Append(" (numero_cliente, ");
            sql.Append(" empresa_parc, ");
            sql.Append(" cod_produto, ");
            sql.Append(" cod_ocorr, ");
            sql.Append(" fecha_inclusao, ");
            sql.Append(" rol_inclusao) ");
            sql.Append(" VALUES ( " + ocorr.numeroCliente + " ");
            sql.Append(" , '" + ocorr.empresaParc + "' ");
            sql.Append(" , '" + ocorr.codProduto + "' ");
            sql.Append(" , '" + ocorr.codOcorr + "' ");
            sql.Append(" , today ");
            sql.Append(" , 'ONLINE') ");

            return ExecutarSql(sql.ToString(), informix);

        }


        public bool inserirCadastroCliente(SolicitacaoTipoD sol, DBProviderInformix informix)
        {
            StringBuilder sql = new StringBuilder();

            sql.Append(" INSERT INTO solucoes_cliente ");
            sql.Append(" (empresa_parc,  ");
            sql.Append(" cod_produto,  ");
            sql.Append(" numero_cliente,  ");
            sql.Append(" dv_numero_cliente,  ");
            sql.Append(" estado,  ");
            sql.Append(" canal_venda,  ");
            sql.Append(" fecha_processo,  ");
            sql.Append(" fecha_inclusao,  ");
            sql.Append(" numero_emp_parc,  ");
            sql.Append(" rol_inclusao,  ");
            sql.Append(" ag_inclusao,  ");
            sql.Append(" canal_venda_emppar)  ");
            sql.Append(" values ('" + sol.empresaParc + "', ");
            sql.Append(" '" + sol.codProduto + "', ");
            sql.Append(" " + sol.numeroCliente + " , ");
            sql.Append(" " + sol.dvNumeroCliente + " , ");
            sql.Append(" '2', ");
            sql.Append(" '3', ");
            sql.Append(" '" + sol.dataOcorrencia + "', ");
            sql.Append(" today, ");
            sql.Append(" " + sol.numeroClienteEmpPar + " , ");
            sql.Append(" 'ONLINE', ");
            sql.Append(" '', ");
            sql.Append(" '" + sol.codCanalVenda + "') ");

            return ExecutarSql(sql.ToString(), informix);
        }

        public bool insereCobrancaPredefinida(SolicitacaoTipoD sol, Produto p, DBProviderInformix informix)
        {
            StringBuilder sql = new StringBuilder();

            int corr_facilidad = 0;
            int corr_fac_cli = 0;

            if (p.tipoCobranca == "1")
            {
                buscaCorrelativo(sol.numeroCliente, out corr_facilidad, out corr_fac_cli);

                sql.Append(" INSERT INTO fac_cli ");
                sql.Append(" (numero_cliente, ");
                sql.Append(" codigo_facilidad, ");
                sql.Append(" monto_inicial, ");
                sql.Append(" valor_cuota, ");
                sql.Append(" numero_cuotas, ");
                sql.Append(" cuotas_facturadas, ");
                sql.Append(" corr_facilidad, ");
                sql.Append(" corr_fac_cli, ");
                sql.Append(" estado, ");
                sql.Append(" fecha_creacion, ");
                sql.Append(" usuario_creacion) ");
                sql.Append(" VALUES ( " + sol.numeroCliente + ", ");
                sql.Append(" '" + p.codigoFacilidade + "', ");
                sql.Append(" 0, ");
                sql.Append(" " + p.valorParcela + ", ");
                sql.Append(" 9999, ");
                sql.Append(" 0, ");
                sql.Append(" " + corr_facilidad + ", ");
                sql.Append(" " + corr_fac_cli + ", ");
                sql.Append(" 'V', ");
                sql.Append(" today, ");
                sql.Append(" 'ONLINE'); ");

                return ExecutarSql(sql.ToString(), informix);

            }

            return false;

        }

        protected bool buscaCorrelativo(int numeroCliente, out int corr_facilidad, out int corr_fac_cli)
        {
            bool result = false;
            corr_facilidad = 0;
            corr_fac_cli = 0;

            StringBuilder sql = new StringBuilder();

            sql.Append(" SELECT nvl(MAX(corr_facilidad), 0) as corr_facilidad ");
            sql.Append(" , nvl(MAX(corr_fac_cli), 0) as corr_fac_cli ");
            sql.Append(" FROM    fac_cli ");
            sql.Append(" WHERE  numero_cliente = " + numeroCliente + " ");

            DataTable dt = ConsultaSql(sql.ToString());

            if (dt.Rows.Count > 0)
            {
                //p.produto.diasCancelRecente = int.Parse(dt.Rows[i]["dias_cancel"].ToString());
                corr_facilidad = int.Parse(dt.Rows[0]["corr_facilidad"].ToString());
                corr_fac_cli = int.Parse(dt.Rows[0]["corr_fac_cli"].ToString());
                result = true;
            }

            corr_facilidad++;
            corr_fac_cli++;

            return result;

        }

        public bool insereCobrancaValorInformado(SolicitacaoTipoD sol, Produto p, DBProviderInformix informix)
        {
            StringBuilder sql = new StringBuilder();
            string codigo = sol.codOcorr;
            string estado = "V";
            double valor = 0;
            string encargo = "";

            //Se codigo for 60 - Inserir cobranca. Se for 54 ou 58, inserir indenizacao. Se for 56, inserir cancelamento.
            if (sol.codOcorr == "60")
            {
                //sql.Append(" " + p.encargoCobranca + " , ");
                valor = sol.valorParcela;
                encargo = p.encargoCobranca;
            }
            else
            {
                if (sol.codOcorr == "54" || sol.codOcorr == "58")
                {
                    if (p.encargoIndenizacao != "")
                    {
                        // sql.Append(" " + p.encargoIndenizacao + " , ");
                        encargo = p.encargoIndenizacao;
                        if (sol.valorParcela > 0)
                        {
                            valor = sol.valorParcela * (-1);
                        }
                    }
                    else
                    {
                        return false;
                    }
                }
                else
                {
                    if (sol.codOcorr == "56")
                    {
                        if (p.encargoCancelamento != "")
                        {
                            //sql.Append(" " + p.encargoCancelamento + " , ");
                            encargo = p.encargoCancelamento;
                            if (sol.valorParcela > 0)
                            {
                                valor = sol.valorParcela * (-1);
                            }
                        }
                        else
                        {
                            return false;
                        }
                    }
                    else
                    {
                        if (sol.codOcorr == "61")
                        {
                            encargo = p.encargoCobranca;
                            estado = "P";
                            codigo = "60";
                            valor = sol.valorParcela;
                        }
                    }

                }
            }
            sql.Append(" INSERT INTO solucoes_encargos ");
            sql.Append(" (empresa_parc, ");
            sql.Append(" cod_produto, ");
            sql.Append(" numero_cliente, ");
            sql.Append(" codigo_ocorr, ");
            sql.Append(" codigo_cargo, ");
            sql.Append(" quant_parcelas, ");
            sql.Append(" valor_parcela, ");
            sql.Append(" fecha_inclusao, ");
            sql.Append(" parcelas_faturadas, ");
            sql.Append(" estado) ");
            sql.Append(" VALUES ");
            sql.Append(" ('" + sol.empresaParc + "' , ");
            sql.Append(" '" + sol.codProduto + "', ");
            sql.Append(" " + sol.numeroCliente + " , ");
            sql.Append(" '" + codigo + "' , ");
            sql.Append(" '" + encargo + "', ");
            sql.Append(" " + sol.numParcelas + ", ");
            sql.Append(" " + valor + " , ");
            sql.Append(" today, ");
            sql.Append(" 0, ");
            sql.Append(" '" + estado + "'); ");

            return ExecutarSql(sql.ToString(), informix);





        }



        public int buscaCobrancaValorInformado(int numeroCliente, string empresaParc, string codProduto, string codOcorr)
        {
            int result = 0;

            StringBuilder sql = new StringBuilder();

            sql.Append(" SELECT count(*) cont_encargos ");
            sql.Append(" FROM    solucoes_encargos ");
            sql.Append(" WHERE  empresa_parc  = '" + empresaParc + "' ");
            sql.Append(" AND      cod_produto = '" + codProduto + "' ");
            sql.Append(" AND      numero_cliente = " + numeroCliente + " ");

            if (codOcorr == "57")
            {
                sql.Append(" AND codigo_ocorr in ('54', '58', '57') ");
            }
            else
            {
                sql.Append(" AND      codigo_ocorr = '" + codOcorr + "' ");
            }

            sql.Append(" AND      estado = 'V' ");



            DataTable dt = ConsultaSql(sql.ToString());

            if (dt.Rows.Count > 0)
            {
                result = int.Parse(dt.Rows[0]["cont_encargos"].ToString());
            }

            return result;

        }

        public int buscaDebitoValorInformado(int numeroCliente, string empresaParc, string codProduto)
        {
            return buscaCobrancaValorInformado(numeroCliente, empresaParc, codProduto, "60");
        }

        public bool buscaSinistro(int numeroCliente, string empresaParc, string codProduto, string codOcorr)
        {
            StringBuilder sql = new StringBuilder();

            sql.Append(" SELECT numero_cliente ");
            sql.Append(" FROM    solucoes_encargos ");
            sql.Append(" WHERE  empresa_parc  = '" + empresaParc + "' ");
            sql.Append(" AND      cod_produto = '" + codProduto + "' ");
            sql.Append(" AND      numero_cliente = " + numeroCliente + " ");
            sql.Append(" AND      codigo_ocorr = '" + codOcorr + "' ");
            sql.Append(" AND      estado = 'V' ");

            DataTable dt = ConsultaSql(sql.ToString());

            if (dt.Rows.Count > 0)
            {
                return true;
            }

            return false;
        }

        public bool cancelaIndenizacao(SolicitacaoTipoD sol, Produto p, DBProviderInformix informix)
        {
            StringBuilder sql = new StringBuilder();


            sql.Append(" UPDATE solucoes_encargos ");
            sql.Append(" SET estado = 'C',  ");
            sql.Append(" fecha_termino = today ");
            sql.Append(" WHERE empresa_parc = '" + sol.empresaParc + "' ");
            sql.Append(" AND   cod_produto = '" + sol.codProduto + "' ");
            sql.Append(" AND   numero_cliente = " + sol.numeroCliente + " ");

            if (sol.codOcorr == "57")
            {
                sql.Append(" AND codigo_ocorr in ('54', '58', '57') ");

            }
            else
            {
                sql.Append(" AND   codigo_ocorr = '" + sol.codOcorr + "' ");
            }


            sql.Append(" AND   estado = 'V'; ");

            return ExecutarSql(sql.ToString(), informix);


        }

        public bool atualizaEstadoCancelamento(SolicitacaoTipoD sol, Produto p, DBProviderInformix informix)
        {
            StringBuilder sql = new StringBuilder();

            sql.Append(" UPDATE solucoes_cliente SET estado = '9' ");
            sql.Append(" WHERE empresa_parc   = '" + sol.empresaParc + "' ");
            sql.Append(" AND   cod_produto    = '" + sol.codProduto + "' ");
            sql.Append(" AND   numero_cliente = " + sol.numeroCliente + " ");
            sql.Append(" AND   estado = '7'; ");

            return ExecutarSql(sql.ToString(), informix);

        }


        public List<Tabla> buscaDadosTabla(string nomeTabla)
        {

            List<Tabla> result = new List<Tabla>();


            StringBuilder sql = new StringBuilder();

            sql.Append(" select trim(t.codigo) as codigo, ");
            sql.Append(" trim(t.descripcion) as descripcion, ");
            sql.Append(" trim(t.nomtabla) as nomtabla ");
            sql.Append(" from tabla t  ");
            sql.Append(" where t.fecha_desactivac is null ");
            sql.Append(" and t.sucursal = '0000' ");
            sql.Append(" and t.nomtabla = '" + nomeTabla + "' ");


            DataTable dt = ConsultaSql(sql.ToString());

            if (dt.Rows.Count > 0)
            {
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    Tabla t = new Tabla();

                    t.codigo = dt.Rows[i]["codigo"].ToString();
                    t.descricao = dt.Rows[i]["descripcion"].ToString();
                    t.nomeTabla = dt.Rows[i]["nomtabla"].ToString();

                    result.Add(t);
                }

            }

            return result;
        }

        public List<CobrancaValorInformado> buscaListagemCobrancasValorInformado(int numeroCliente, string empresaParc, string codProduto)
        {
            return buscaListagemCobrancasValorInformado(numeroCliente, empresaParc, codProduto, 0);
        }

        public List<CobrancaValorInformadoCliente> buscaListagemHistoricoCobrancasValorInformado(int numeroCliente, string empresaParc, string codProduto)
        {
            List<CobrancaValorInformado> l1;
            List<CobrancaValorInformadoCliente> l2 = new List<CobrancaValorInformadoCliente>();

            l1 = buscaListagemCobrancasValorInformado(numeroCliente, empresaParc, codProduto, 1);

            if (l1.Count > 0)
            {
                for (int i = 0; i < l1.Count; i++)
                {
                    CobrancaValorInformadoCliente f = new CobrancaValorInformadoCliente(l1[i]);
                    
                    l2.Add(f);
                }
            }

            return l2;
        }

        public List<CobrancaValorInformado> buscaListagemCobrancasValorInformado(int numeroCliente, string empresaParc, string codProduto, int tipoConsulta)
        {

            List<CobrancaValorInformado> result = new List<CobrancaValorInformado>();

            StringBuilder sql = new StringBuilder();

            //tipoConsulta = 0: Somente parcelas vigentes de codigo 60 (cobranca).
            //tipoConsulta diferente de zero. Todo o historico


            sql.Append(" select ");
            sql.Append(" se.empresa_parc ");
            sql.Append(" , se.cod_produto ");
            sql.Append(" , se.numero_cliente ");
            sql.Append(" , se.codigo_ocorr ");
            sql.Append(" , se.codigo_cargo ");
            sql.Append(" , se.quant_parcelas ");
            sql.Append(" , se.valor_parcela ");
            sql.Append(" , se.fecha_inclusao ");
            sql.Append(" , se.parcelas_faturadas ");
            sql.Append(" , se.estado ");
            sql.Append(" , se.fecha_termino ");
            sql.Append(" , se.rowid ");

            sql.Append(" from solucoes_encargos se  ");
            sql.Append(" where 1=1  ");
            sql.Append(" and se.numero_cliente = " + numeroCliente + " ");
            sql.Append(" and se.empresa_parc =  '" + empresaParc + "' ");
            sql.Append(" and se.cod_produto =  '" + codProduto + "' ");
            if (tipoConsulta == 0)
            {
                sql.Append(" and se.codigo_ocorr = '60' ");
                sql.Append(" and se.estado = 'V' ");
            }

            sql.Append(" order by se.fecha_inclusao;  ");

            DataTable dt = ConsultaSql(sql.ToString());

            if (dt.Rows.Count > 0)
            {
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    CobrancaValorInformado c = new CobrancaValorInformado();
                    DateTime tmpData;
                    c.empresaParc = dt.Rows[i]["empresa_parc"].ToString();
                    c.codProduto = dt.Rows[i]["cod_produto"].ToString();
                    c.numeroCliente = int.Parse(dt.Rows[i]["numero_cliente"].ToString());
                    c.codOcorr = dt.Rows[i]["codigo_ocorr"].ToString();
                    c.codEncargo = dt.Rows[i]["codigo_cargo"].ToString();
                    c.quantParcelas = int.Parse(dt.Rows[i]["quant_parcelas"].ToString());
                    c.valorParcela = double.Parse(dt.Rows[i]["valor_parcela"].ToString());
                    if (DateTime.TryParse(dt.Rows[i]["fecha_inclusao"].ToString(), out tmpData))
                    {
                        c.dataIngresso = tmpData.ToString("MM/dd/yyyy");
                    }


                    c.parcelasFat = int.Parse(dt.Rows[i]["parcelas_faturadas"].ToString());
                    c.estado = dt.Rows[i]["estado"].ToString();

                    if (DateTime.TryParse(dt.Rows[i]["fecha_termino"].ToString(), out tmpData))
                    {
                        c.dataTermino = tmpData.ToString("MM/dd/yyyy");
                    }


                    c.rowid = long.Parse(dt.Rows[i]["rowid"].ToString());

                    result.Add(c);

                }

            }

            return result;
        }

        public bool cancelaCobrancaVInfEspecifica(CobrancaValorInformado c, DBProviderInformix informix)
        {
            StringBuilder sql = new StringBuilder();

            sql.Append(" UPDATE solucoes_encargos ");
            sql.Append(" set ");
            sql.Append(" (codigo_ocorr, estado, fecha_termino) = ('61', 'C', 'today') ");
            sql.Append(" WHERE numero_cliente = " + c.numeroCliente + " ");
            sql.Append(" AND empresa_parc = '" + c.empresaParc + "' ");
            sql.Append(" AND cod_produto = '" + c.codProduto + "' ");
            sql.Append(" AND rowid = " + c.rowid + " ");

            return ExecutarSql(sql.ToString(), informix);
        }

        public bool insereEncargoFuturoD61(SolicitacaoTipoD sol, CobrancaValorInformado c, int numParcelas, int quantMeses, Produto p, DBProviderInformix informix)
        {
            StringBuilder sql = new StringBuilder();
            //string dataFutura = buscaDataFutura(quantMeses);
            string dataFutura = DateTime.Now.AddMonths(quantMeses).ToString();
            DateTime dataFuturaDT = Convert.ToDateTime(dataFutura);

            if (numParcelas < 0)
            {
                numParcelas = 0;
            }

            sql.Append(" INSERT INTO ");
            sql.Append(" solucoes_encargos ");

            sql.Append(" (empresa_parc,  ");
            sql.Append(" cod_produto,  ");
            sql.Append(" numero_cliente,  ");
            sql.Append(" codigo_ocorr,  ");
            sql.Append(" codigo_cargo,  ");
            sql.Append(" quant_parcelas,  ");
            sql.Append(" parcelas_faturadas, ");
            sql.Append(" valor_parcela,  ");
            sql.Append(" fecha_inclusao,  ");
            sql.Append(" estado) ");

            sql.Append(" values  ");
            sql.Append(" ('" + sol.empresaParc + "', ");
            sql.Append(" '" + sol.codProduto + "' ,");
            sql.Append(" " + sol.numeroCliente + ", ");
            sql.Append(" '60',  ");
            sql.Append(" '" + p.encargoCobranca + "', ");
            sql.Append(" " + numParcelas + ", ");
            sql.Append(" 0,  ");
            sql.Append(" " + c.valorParcela + ", ");
            sql.Append(" '" + dataFuturaDT.ToString("MM/dd/yyyy") + "', ");
            sql.Append(" 'P'); ");


            return ExecutarSql(sql.ToString(), informix);

        }

        public string buscaDataFutura(int quantMeses)
        {
            string result = "";
            StringBuilder sql = new StringBuilder();

            sql.Append(" select date(current + " + quantMeses + " units month) as data ");
            sql.Append(" FROM insta; ");

            DataTable dt = ConsultaSql(sql.ToString());

            if (dt.Rows.Count > 0)
            {
                //c.empresaParc = dt.Rows[i]["empresa_parc"].ToString();
                result = dt.Rows[0]["data"].ToString();
            }

            return result;

        }

        public bool inserirCanalReconquista(SolicitacaoTipoD sol, ProdutoCliente a, DBProviderInformix informix)
        {
            StringBuilder sql = new StringBuilder();
            DateTime tmpData = Convert.ToDateTime(a.dataAdesao);

            sql.Append(" insert into solucoes_reconquista ");
            sql.Append(" (numero_cliente, ");
            sql.Append(" empresa_parc, ");
            sql.Append(" cod_produto,  ");
            sql.Append(" fecha_inclusao, ");
            sql.Append(" data_processo,  ");
            sql.Append(" canal_venda_emppar, ");
            sql.Append(" canal_reconquista, ");
            sql.Append(" cont_reconquista ");
            sql.Append(" ) ");
            sql.Append(" values ");
            sql.Append(" ( ");
            sql.Append(" " + sol.numeroCliente + ", ");
            sql.Append(" '" + sol.empresaParc + "', ");
            sql.Append(" '" + sol.codProduto + "', ");
            sql.Append(" '" + tmpData.ToString("MM/dd/yyyy") + "', ");
            sql.Append(" today, ");
            sql.Append(" '" + a.codCanalVenda + "', ");
            sql.Append(" '" + sol.codCanalVenda + "', ");
            sql.Append(" 1 ");
            sql.Append(" ); ");

            return ExecutarSql(sql.ToString(), informix);

        }

        public bool atualizaCanalVenda(SolicitacaoTipoD sol, ProdutoCliente a, DBProviderInformix informix)
        {
            StringBuilder sql = new StringBuilder();
            DateTime tmpData;
            tmpData = Convert.ToDateTime(a.dataAdesao);

            sql.Append(" update solucoes_cliente sc ");
            sql.Append(" set canal_venda_emppar = '" + sol.codCanalVenda + "' ");
            sql.Append(" where 1=1  ");
            sql.Append(" and sc.empresa_parc = '" + sol.empresaParc + "' ");
            sql.Append(" and sc.cod_produto = '" + sol.codProduto + "' ");
            sql.Append(" and sc.numero_cliente = " + sol.numeroCliente + " ");
            sql.Append(" and sc.fecha_inclusao = '" + tmpData.ToString("MM/dd/yyyy") + "' ");
            sql.Append(" and sc.estado = '2'; ");

            return ExecutarSql(sql.ToString(), informix);

        }

        public long buscaIdCanalVenda(SolicitacaoTipoD sol, ProdutoCliente a, DBProviderInformix informix)
        {
            long result = 0;
            DateTime tmpData;

            tmpData = Convert.ToDateTime(a.dataAdesao);

            StringBuilder sql = new StringBuilder();

            sql.Append(" select first 1 rowid ");
            sql.Append(" from solucoes_reconquista ");
            sql.Append(" where 1=1 ");
            sql.Append(" and numero_cliente = " + a.numeroCliente + " ");
            sql.Append(" and empresa_parc = '" + a.empresaParc + "' ");
            sql.Append(" and cod_produto = '" + a.codProduto + "' ");
            sql.Append(" and fecha_inclusao = '" + tmpData.ToString("MM/dd/yyyy") + "' ");
            sql.Append(" and canal_reconquista = '" + sol.codCanalVenda + "' ");

            DataTable dt = ConsultaSql(sql.ToString());

            if (dt.Rows.Count > 0)
            {

                result = long.Parse(dt.Rows[0]["rowid"].ToString());
            }

            return result;

        }

        public bool incrementaContadorCanalVenda(SolicitacaoTipoD sol, ProdutoCliente a, long rowid, DBProviderInformix informix)
        {
            StringBuilder sql = new StringBuilder();
            DateTime tmpData = Convert.ToDateTime(a.dataAdesao);

            sql.Append(" update solucoes_reconquista ");
            sql.Append(" set cont_reconquista = cont_reconquista + 1  ");
            sql.Append(" where 1=1 ");
            sql.Append(" and numero_cliente = " + sol.numeroCliente + " ");
            sql.Append(" and empresa_parc = '" + sol.empresaParc + "' ");
            sql.Append(" and cod_produto = '" + sol.codProduto + "' ");
            sql.Append(" and fecha_inclusao = '" + tmpData.ToString("MM/dd/yyyy") + "' ");
            sql.Append(" and rowid = " + rowid + "; ");

            return ExecutarSql(sql.ToString(), informix);

        }

        public bool atualizaNumeroClienteEmpresaParceira(SolicitacaoTipoD sol, ProdutoCliente a, DBProviderInformix informix)
        {
            StringBuilder sql = new StringBuilder();

            sql.Append(" UPDATE solucoes_cliente SET ");
            sql.Append(" numero_emp_parc = " + sol.numeroClienteEmpPar + " ");
            sql.Append(" WHERE empresa_parc= '" + sol.empresaParc + "' AND  ");
            sql.Append(" cod_produto       = '" + sol.codProduto + "' AND  ");
            sql.Append(" numero_cliente    = " + sol.numeroCliente + " ");
            sql.Append(" AND estado = '2'; ");

            return ExecutarSql(sql.ToString(), informix);

        }

        public bool insereMovimentoSolucoes(SolicitacaoTipoD sol, string codOcorr, DBProviderInformix informix)
        {
            StringBuilder sql = new StringBuilder();
            string estadoProcesso;
            if (codOcorr == "74")
            {
                estadoProcesso = "F";
            }
            else
            {
                estadoProcesso = "P"; //Consta como processado. Grava registro para constar historico
            }

            /* D
             * {0} numeroCliente 10 char
             * {1} dvNumeroCliente 1 char 
             * {2} codOcorr 2 char 
             * {3} dataOcorrencia 10 char
             * {4} valor 9 char (7 + 2)
             * {5} quantParcelas 2 char
             * {6} numeroClienteEmpPar 8 char
             * {7} codProduto 4 char 
             * {8} empresaParc 2 char
             * {9} canalVenda 2 char
             */

            string registro = String.Format("D{0}{1}{2}{3}{4}{5}{6}{7}{8}{9}",
               sol.numeroCliente.ToString().PadLeft(10, '0'),
               sol.dvNumeroCliente,
               sol.codOcorr,
               sol.dataOcorrencia,
               Convert.ToString(sol.valorParcela * 100).PadLeft(9, '0'),
               Convert.ToString(sol.numParcelas).PadLeft(2, '0'),
               Convert.ToString(sol.numeroClienteEmpPar).PadLeft(8, '0'),
               sol.codProduto.PadLeft(4, '0'),
               sol.empresaParc.PadLeft(2, '0'),
               sol.codCanalVenda
               );

            sql.Append(" INSERT INTO solucoes_mov_receb ");
            sql.Append(" (empresa_parc, ");
            sql.Append(" cod_produto, ");
            sql.Append(" tipo_reg, ");
            sql.Append(" numero_cliente, ");
            sql.Append(" dv_numero_cliente, ");
            sql.Append(" codigo_ocorr, ");
            sql.Append(" fecha_ocorrencia, ");
            sql.Append(" valor, ");
            sql.Append(" num_parcelas, ");
            sql.Append(" num_emp_parc, ");
            sql.Append(" fecha_recebido, ");
            sql.Append(" num_arquivo, ");
            sql.Append(" fecha_processo, ");
            sql.Append(" estado_processo, ");
            sql.Append(" ocorr_receb, ");
            sql.Append(" registro) VALUES ");

            sql.Append(" ('" + sol.empresaParc + "', ");
            sql.Append(" '" + sol.codProduto + "', ");
            sql.Append(" 'D', "); //tipo_reg
            sql.Append(" " + sol.numeroCliente + ", ");
            sql.Append(" " + sol.dvNumeroCliente + ", ");
            sql.Append(" '" + sol.codOcorr + "', ");
            sql.Append(" '" + sol.dataOcorrencia + "', ");
            sql.Append(" " + sol.valorParcela + ", ");
            sql.Append(" " + sol.numParcelas + ", ");
            sql.Append(" " + sol.numeroClienteEmpPar + ", ");
            sql.Append(" today, "); //fecha_recebido
            sql.Append(" 999999, "); //num_arquivo. Sequencial de arquivo nao existe no contexto de Web Service
            sql.Append(" today, "); //fecha_processo
            sql.Append(" '" + estadoProcesso + "', "); //estado_processo
            sql.Append(" '00', "); //ocorr_receb
            sql.Append(" '" + registro + "') ");

            return ExecutarSql(sql.ToString(), informix);

        }

        public ClienteSolucoes buscaCadastroCliente(int numeroCliente)
        {
            ClienteSolucoes c = null;
            List<ClienteSolucoes> l = buscaCadastroCliente(numeroCliente, 0, "", "");
            if (l.Count > 0)
            {
                c = new ClienteSolucoes();
                c = l[0];
            }

            return c;
        }

        public ClienteSolucoes buscaCadastroCliente(int numeroCliente, string loginParceira)
        {
            ClienteSolucoes c = null;
            List<ClienteSolucoes> l = buscaCadastroCliente(numeroCliente, 2, "", loginParceira);

            if (l.Count > 0)
            {
                c = new ClienteSolucoes();
                c = l[0];
            }

            return c;
        }

        public List<ClienteSolucoes> buscaCadastroCliente(int numeroCliente, int tipoConsulta, string dataOcorrencia, string loginParceira)
        {
            return buscaCadastroCliente(numeroCliente, tipoConsulta, dataOcorrencia, null, loginParceira);
        }


        public List<ClienteSolucoes> buscaCadastroCliente(int numeroCliente, int tipoConsulta, string dataOcorrencia, Produto p, string loginParceira)
        {
            StringBuilder sql = new StringBuilder();


            //tipoConsulta
            //0 -> Cliente Synergia
            //1 -> Clientes parceira por data de adesao (Listagem)
            //2 -> Cliente parceira 
            //3 -> Clientes por data e produto

            sql.Append(" SELECT ");
            if (tipoConsulta == 1)
            {
                sql.Append(" distinct ");
            }
            sql.Append(" c.numero_cliente,  ");
            sql.Append(" c.dv_numero_cliente,  ");
            sql.Append(" trim(c.nombre) as nombre,  ");
            sql.Append(" trim(c.direccion) as endereco,  ");
            sql.Append(" trim(b.nome) as bairro,  ");
            sql.Append(" trim(l.descripcion) as municipio, ");
            sql.Append(" trim('RJ') as uf,  ");
            sql.Append(" trim(c.cep) as cep, ");
            sql.Append(" trim(c.ddd) as ddd, ");
            sql.Append(" trim(c.telefono) as telefono, ");
            sql.Append(" trim(c.tipo_ident) as tipo_ident1, ");
            sql.Append(" desc_tabla('TIPIDE', c.tipo_ident, '0000') as desc_tipoDoc1, ");
            sql.Append(" trim(c.rut) as documento1,  ");
            sql.Append(" trim(c.dv_rut) as dv_documento1,  ");
            sql.Append(" trim(c.comp_docu) as orgaoExpDoc1, ");
            sql.Append(" trim(c.tipo_ident2) tipo_ident2,  ");
            sql.Append(" desc_tabla('TIPIDE', c.tipo_ident2, '0000') as desc_tipoDoc2, ");
            sql.Append(" trim(c.documento2) as documento2, ");
            sql.Append(" trim(c.dv_docu2) as dv_docu2, ");
            sql.Append(" ROUND(c.cons_prom_3_actfp,0) as media_consumo, ");
            sql.Append(" c.data_nasc,  ");
            sql.Append(" c.sector, ");
            sql.Append(" c.zona, ");
            sql.Append(" c.fecha_ultima_fact, ");
            sql.Append(" trim(c.tipo_vencimiento) as tipo_vencimiento, ");
            sql.Append(" c.localidade as localidade, ");
            sql.Append(" c.estado_cliente,  ");
            sql.Append(" c.estado_facturacion,  ");
            sql.Append(" c.estado_suministro, ");
            sql.Append(" trim(c.classe) as classe,  ");
            sql.Append(" c.correlativo_ruta, ");
            sql.Append(" c.dv_ruta_lectura, ");
            sql.Append(" c.sector||' '||c.localidade||' '||c.zona||' '||lpad(c.correlativo_ruta, 6, '0')||'-'||dv_ruta_lectura as rota ");
            sql.Append(" FROM  cliente c, localidades l, OUTER barrios b ");
            if (tipoConsulta == 1 || tipoConsulta == 3)
            {
                sql.Append(" , solucoes_cliente s ");
            }
            sql.Append(" WHERE 1=1  ");

            sql.Append(" AND l.localidad = c.localidade  ");
            sql.Append(" AND b.municipio = c.municipio   ");
            sql.Append(" AND c.comuna = b.codigo_barrio ");
            if (tipoConsulta == 0) //Cliente Synergia (nao importa se é da parceira ou nao)
            {
                sql.Append(" and c.numero_cliente = " + numeroCliente + " ");
            }
            else if (tipoConsulta == 1) //Listagem de clientes da parceira.
            {
                DateTime tmpData;
                DateTime.TryParse(dataOcorrencia, out tmpData);
                string data = tmpData.ToString("MM/dd/yyyy");

                sql.Append(" and  s.numero_cliente = c.numero_cliente ");
                sql.Append(" AND s.fecha_inclusao = '" + data + "' ");

                sql.Append(" AND s.empresa_parc in ");
                sql.Append("  (select t_emp.codigo ");
                sql.Append("  from tabla t_emp ");
                sql.Append(" where 1=1 ");
                sql.Append(" and t_emp.fecha_desactivac is null ");
                sql.Append(" and t_emp.sucursal = '0000' ");
                sql.Append(" and t_emp.nomtabla = 'RELEMP' ");
                sql.Append(" and t_emp.valor_alf = '" + loginParceira + "' ");
                sql.Append(" and t_emp.codigo = s.empresa_parc) ");
                //sql.Append(" ) ");



            }
            else if (tipoConsulta == 2) //Cliente da parceira.
            {
                sql.Append(" and c.numero_cliente = " + numeroCliente + " ");
                sql.Append("and c.numero_cliente in ( ");
                sql.Append("select s.numero_cliente ");
                sql.Append("from solucoes_cliente s ");
                sql.Append("where s.numero_cliente = c.numero_cliente ");
                sql.Append(" AND s.empresa_parc in ");
                adicionaLoginParceira(sql, loginParceira);
                sql.Append(" ) ");

            }
            else if (tipoConsulta == 3)
            {
                //3 -> Clientes por data e produto

                DateTime tmpData;
                DateTime.TryParse(dataOcorrencia, out tmpData);
                string data = tmpData.ToString("MM/dd/yyyy");

                sql.Append(" and  s.numero_cliente = c.numero_cliente ");
                sql.Append(" AND s.fecha_inclusao = '" + data + "' ");
                sql.Append(" and s.empresa_parc = '" + p.empresa.codEmpresaParc + "' ");
                sql.Append(" and s.cod_produto = '" + p.codProduto + "' ");

                sql.Append(" AND s.empresa_parc in ");
                sql.Append("  (select t_emp.codigo ");
                sql.Append("  from tabla t_emp ");
                sql.Append(" where 1=1 ");
                sql.Append(" and t_emp.fecha_desactivac is null ");
                sql.Append(" and t_emp.sucursal = '0000' ");
                sql.Append(" and t_emp.nomtabla = 'RELEMP' ");
                sql.Append(" and t_emp.valor_alf = '" + loginParceira + "' ");
                sql.Append(" and t_emp.codigo = s.empresa_parc) ");

            }
            DataTable dt = ConsultaSql(sql.ToString());
            List<ClienteSolucoes> result = new List<ClienteSolucoes>();
            if (dt.Rows.Count > 0)
            {

                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    ClienteSolucoes c = new ClienteSolucoes();

                    c.numeroCliente = int.Parse(dt.Rows[0]["numero_cliente"].ToString());
                    c.dvNumeroCliente = int.Parse(dt.Rows[0]["dv_numero_cliente"].ToString());
                    c.nome = dt.Rows[0]["nombre"].ToString();
                    c.endereco = dt.Rows[0]["endereco"].ToString();
                    c.bairro = dt.Rows[0]["bairro"].ToString();
                    c.municipio = dt.Rows[0]["municipio"].ToString();
                    //result.uf = dt.Rows[0]["uf"].ToString();
                    c.cep = dt.Rows[0]["cep"].ToString();
                    c.ddd = dt.Rows[0]["ddd"].ToString();
                    c.telefone = dt.Rows[0]["telefono"].ToString();
                    c.tipoDoc1 = dt.Rows[0]["tipo_ident1"].ToString();
                    c.documento1 = dt.Rows[0]["documento1"].ToString();
                    c.dvDoc1 = dt.Rows[0]["dv_documento1"].ToString();
                    c.orgaoExpDoc1 = dt.Rows[0]["orgaoExpDoc1"].ToString();
                    c.descTipoDoc2 = dt.Rows[0]["tipo_ident2"].ToString();
                    c.documento2 = dt.Rows[0]["documento2"].ToString();
                    c.dvDoc2 = dt.Rows[0]["dv_docu2"].ToString();

                    double tmpMediaConsumo = 0;

                    if (Double.TryParse(dt.Rows[0]["media_consumo"].ToString(), out tmpMediaConsumo))
                    {
                        c.mediaConsumo = tmpMediaConsumo;
                    }

                    //result.mediaConsumo = double.Parse(dt.Rows[0]["media_consumo"].ToString());

                    DateTime tmpData;
                    if (DateTime.TryParse(dt.Rows[0]["data_nasc"].ToString(), out tmpData))
                    {
                        c.dataNascimento = tmpData.ToString("MM/dd/yyyy");
                    }


                    c.loteFaturamento = int.Parse(dt.Rows[0]["sector"].ToString());
                    c.zona = int.Parse(dt.Rows[0]["zona"].ToString());

                    if (DateTime.TryParse(dt.Rows[0]["fecha_ultima_fact"].ToString(), out tmpData))
                    {
                        c.dataUltimoFaturamento = tmpData.ToString("MM/dd/yyyy");
                    }




                    c.tipoVencimento = dt.Rows[0]["tipo_vencimiento"].ToString();
                    c.codigoLocalidade = dt.Rows[0]["localidade"].ToString();
                    c.estadoCliente = dt.Rows[0]["estado_cliente"].ToString();
                    c.EstadoFaturamento = dt.Rows[0]["estado_facturacion"].ToString();
                    c.estadoFornecimento = dt.Rows[0]["estado_suministro"].ToString();
                    c.Classe = dt.Rows[0]["classe"].ToString();
                    c.descTipoDoc1 = dt.Rows[0]["desc_tipoDoc1"].ToString();
                    c.descTipoDoc2 = dt.Rows[0]["desc_tipoDoc2"].ToString();
                    c.correlativoRota = dt.Rows[0]["correlativo_ruta"].ToString();
                    c.dvCorrelativoRota = dt.Rows[0]["dv_ruta_lectura"].ToString();
                    c.rotaLeitura = dt.Rows[0]["rota"].ToString();

                    result.Add(c);
                }


            }
            return result;
        }

        public bool atualizaEstadoParcelasValorInformado(SolicitacaoTipoD sol, DBProviderInformix informix)
        {
            StringBuilder sql = new StringBuilder();

            sql.Append(" UPDATE solucoes_encargos ");
            sql.Append(" set estado = 'V'  ");
            sql.Append(" where 1=1  ");
            sql.Append(" and numero_cliente = " + sol.numeroCliente + " ");
            sql.Append(" and empresa_parc = '" + sol.empresaParc + "' ");
            sql.Append(" and cod_produto = '" + sol.codProduto + "' ");
            sql.Append(" and estado = 'P'; ");

            return ExecutarSql(sql.ToString(), informix);
        }

        List<String> buscaEmpresasParceira(string loginParceira)
        {
            List<String> empresas = new List<String>();



            return empresas;

        }

        public List<RetornoSolicitacaoTipoC> buscaOcorrencias(string dataOcorrencia, Produto p, string loginParceira)
        {

            return buscaOcorrencias(0, p, dataOcorrencia, loginParceira);

        }

        public List<RetornoSolicitacaoTipoC> buscaOcorrencias(string dataOcorrencia, string loginParceira, string empresa)
        {
            List<Produto> p = buscaProdutoParceira(loginParceira);
            List<RetornoSolicitacaoTipoC> l = new List<RetornoSolicitacaoTipoC>();

            for (int i = 0; i < p.Count; i++)
            {
                Produto produtoTemp = p[i];
                List<RetornoSolicitacaoTipoC> temp = buscaOcorrencias( dataOcorrencia, produtoTemp, loginParceira);
                
                l.AddRange(temp);
            }

            return l;
        }

        public List<RetornoSolicitacaoTipoC> buscaOcorrencias(int numeroCliente, Produto p, string dataOcorrencia, string loginParceira)
        {
            List<RetornoSolicitacaoTipoC> result = new List<RetornoSolicitacaoTipoC>();
            StringBuilder sql = new StringBuilder();

            sql.Append(" SELECT solucoes_ocorr.numero_cliente ");
            sql.Append(" , nvl((SELECT dv_numero_cliente FROM cliente WHERE numero_cliente = solucoes_ocorr.numero_cliente),0) as dv_numero_cliente ");
            sql.Append(" , '' as num_emp_parc  ");

            sql.Append(" , solucoes_ocorr.cod_ocorr ");
            sql.Append(" , solucoes_ocorr.fecha_inclusao ");
            sql.Append(" , nvl((select sector from cliente c1 where c1.numero_cliente = solucoes_ocorr.numero_cliente), 0) as sector  ");
            sql.Append(" , solucoes_ocorr.empresa_parc ");
            sql.Append(" , solucoes_ocorr.cod_produto ");
            sql.Append(" , solucoes_ocorr.referencia ");
            sql.Append(" , solucoes_ocorr.fecha_proceso ");
            sql.Append(" , nvl((select max(sc1.numero_emp_parc)   ");
            sql.Append(" from solucoes_cliente sc1    ");
            sql.Append(" where sc1.numero_cliente = solucoes_ocorr.numero_cliente    ");
            sql.Append(" and sc1.empresa_parc = solucoes_ocorr.empresa_parc ");
            sql.Append(" and sc1.cod_produto = solucoes_ocorr.cod_produto    ");
            sql.Append(" and (sc1.fecha_inclusao <= solucoes_ocorr.fecha_inclusao    ");
            sql.Append(" or (sc1.fecha_exclusao >= solucoes_ocorr.fecha_inclusao  ");
            sql.Append(" and sc1.fecha_inclusao  <= solucoes_ocorr.fecha_inclusao) )), 0) as numero_emp_parc  ");
            sql.Append(" FROM    solucoes_ocorr,       ");
            sql.Append(" solucoes_produto ");
            sql.Append(" WHERE   1=1 ");
            sql.Append(" AND             solucoes_ocorr.empresa_parc = solucoes_produto.empresa_parc ");
            sql.Append(" AND     solucoes_ocorr.cod_produto = solucoes_produto.cod_produto ");
            sql.Append(" AND             solucoes_produto.fecha_desactivac is null ");


            if (p != null && dataOcorrencia == "")
            {
                sql.Append(" AND solucoes_ocorr.empresa_parc = '" + p.empresa.codEmpresaParc + "' ");
                sql.Append(" AND solucoes_ocorr.cod_produto =  '" + p.codProduto + "' ");
                sql.Append(" AND solucoes_ocorr.numero_cliente = " + numeroCliente + " ");
            }
            else
            {
                if (p != null)
                {
                    sql.Append(" AND solucoes_ocorr.empresa_parc = '" + p.empresa.codEmpresaParc + "' ");
                    sql.Append(" AND solucoes_ocorr.cod_produto =  '" + p.codProduto + "' ");
                }


                if (dataOcorrencia != "")
                {
                    sql.Append(" AND solucoes_ocorr.fecha_inclusao = '" + dataOcorrencia + "' ");
                }
            }
            sql.Append(" AND solucoes_ocorr.empresa_parc in ");
            adicionaLoginParceira(sql, loginParceira);

            DataTable dt = ConsultaSqlDinamico(sql.ToString());

            if (dt.Rows.Count > 0)
            {
                List<Tabla> t = buscaDadosTabla("SOLOCO");
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    RetornoSolicitacaoTipoC o = new RetornoSolicitacaoTipoC();
                    o.numeroCliente = int.Parse(dt.Rows[i]["numero_cliente"].ToString());
                    o.dvNumeroCliente = int.Parse(dt.Rows[i]["dv_numero_cliente"].ToString());
                    o.empresaParc = dt.Rows[i]["empresa_parc"].ToString();
                    o.codProduto = dt.Rows[i]["cod_produto"].ToString();
                    o.tipoRetorno = "OCORR";
                    string tmpData = dt.Rows[i]["fecha_inclusao"].ToString();
                    o.dataOcorrencia = Convert.ToDateTime(tmpData).ToString("MM/dd/yyyy");
                    o.codOcorr = dt.Rows[i]["cod_ocorr"].ToString();
                    o.referencia = dt.Rows[i]["referencia"].ToString();
                    o.loteFaturamento = int.Parse(dt.Rows[i]["sector"].ToString());
                    //o.numeroClienteEmpPar = buscaNumeroClienteEmpParc(o.numeroCliente, o.empresaParc, o.codProduto, o.dataOcorrencia);
                    o.numeroClienteEmpPar = int.Parse(dt.Rows[i]["numero_emp_parc"].ToString());
                    result.Add(o);

                    for (int j = 0; j < t.Count; j++)
                    {
                        if (t[j].codigo == o.codOcorr)
                        {
                            o.descricaoRetorno = t[j].descricao;
                        }
                    }

                }
            }

            return result;
        }

        private StringBuilder adicionaLoginParceira(StringBuilder sql, string loginParceira)
        {
            //Restrige consulta para somente os produtos que estao associados ao login da parceira 
            //na tabla RELEMP.

            sql.Append(" (select t_emp.codigo  ");
            sql.Append(" from tabla t_emp ");
            sql.Append(" where 1=1 ");
            sql.Append(" and t_emp.fecha_desactivac is null ");
            sql.Append(" and t_emp.sucursal = '0000' ");
            sql.Append(" and t_emp.nomtabla = 'RELEMP' ");
            sql.Append(" and t_emp.valor_alf = '" + loginParceira + "') ");

            return sql;
        }

        protected int buscaNumeroClienteEmpParc(int numeroCliente, string empresaParc, string codProduto, string dataAdesao)
        {
            int result = 0;
            DateTime tmpData;

            tmpData = Convert.ToDateTime(dataAdesao);

            StringBuilder sql = new StringBuilder();

            sql.Append(" select first 1 nvl(sc.numero_emp_parc, 0) as numero_emp_parc ");

            sql.Append(" from solucoes_cliente sc  ");
            sql.Append(" where sc.numero_cliente = " + numeroCliente + " ");
            sql.Append(" and sc.empresa_parc = '" + empresaParc + "' ");
            sql.Append(" and sc.cod_produto = '" + codProduto + "' ");
            sql.Append(" and sc.fecha_inclusao <= '" + tmpData.ToString("MM/dd/yyyy") + "' ");
            sql.Append(" and sc.fecha_inclusao <= today ");

            sql.Append(" order by sc.fecha_inclusao desc ");

            DataTable dt = ConsultaSql(sql.ToString());

            if (dt.Rows.Count > 0)
            {

                result = int.Parse(dt.Rows[0]["numero_emp_parc"].ToString());
            }

            return result;

        }

        public List<RetornoSolicitacaoTipoC> buscaArrecadacoesData(string dataBaixa, string loginParceira)
        {
            return buscaArrecadacoes(dataBaixa, null, loginParceira);
        }


        public List<RetornoSolicitacaoTipoC> buscaArrecadacoes(string dataBaixa, Produto p, string loginParceira)
        {
            List<RetornoSolicitacaoTipoC> l = new List<RetornoSolicitacaoTipoC>();
            StringBuilder sql = new StringBuilder();

            sql.Append(" select ");
            sql.Append(" s.numero_cliente,  ");
            sql.Append(" s.dv_numero_cliente,  ");
            sql.Append(" s.fecha_ocorrencia, ");
            sql.Append(" s.empresa_parc, ");
            sql.Append(" s.cod_produto, ");
            sql.Append(" s.fecha_pago, ");
            sql.Append(" s.valor as valor_cargo, ");
            sql.Append(" s.referencia, ");
            sql.Append(" nvl((select c.sector from cliente c where c.numero_cliente = s.numero_cliente), 0) as sector, ");
            sql.Append(" s.codigo_cargo, ");
            sql.Append(" s.fecha_actualiza as fecha_proceso, ");
            sql.Append(" s.canal_venda_emppar ");
            sql.Append("  , nvl((select max(sc1.numero_emp_parc) ");
            sql.Append(" from solucoes_cliente sc1  ");
            sql.Append(" where sc1.numero_cliente = s.numero_cliente  ");
            sql.Append(" and sc1.empresa_parc = s.empresa_parc  ");
            sql.Append(" and sc1.cod_produto = s.cod_produto  ");
            sql.Append(" and sc1.fecha_inclusao <= s.fecha_pago  ");
            sql.Append(" and sc1.fecha_exclusao >= s.fecha_pago), 0) as numero_emp_parc ");
            sql.Append(" from sol_arrec_can s   ");
            sql.Append(" where s.fecha_actualiza = '" + dataBaixa + "' ");

            if (p != null)
            {
                sql.Append(" and s.empresa_parc = '" + p.empresa.codEmpresaParc + "' ");
                sql.Append(" and s.cod_produto = '" + p.codProduto + "' ");
            }

            sql.Append(" AND s.empresa_parc in ");
            adicionaLoginParceira(sql, loginParceira);

            DataTable dt = ConsultaSql(sql.ToString());
            if (dt.Rows.Count > 0)
            {
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    RetornoSolicitacaoTipoC a = new RetornoSolicitacaoTipoC();
                    DateTime tmpData;
                    a.numeroCliente = int.Parse(dt.Rows[i]["numero_cliente"].ToString());
                    a.dvNumeroCliente = int.Parse(dt.Rows[i]["dv_numero_cliente"].ToString());

                    tmpData = Convert.ToDateTime(dt.Rows[i]["fecha_ocorrencia"].ToString());
                    a.dataOcorrencia = tmpData.ToString("MM/dd/yyyy");

                    a.empresaParc = dt.Rows[i]["empresa_parc"].ToString();
                    a.codProduto = dt.Rows[i]["cod_produto"].ToString();

                    tmpData = Convert.ToDateTime(dt.Rows[i]["fecha_pago"].ToString());
                    a.dataPagamento = tmpData.ToString("MM/dd/yyyy");

                    a.valor = double.Parse(dt.Rows[i]["valor_cargo"].ToString());
                    a.referencia = dt.Rows[i]["referencia"].ToString();
                    a.loteFaturamento = int.Parse(dt.Rows[i]["sector"].ToString());

                    tmpData = Convert.ToDateTime(dt.Rows[i]["fecha_proceso"].ToString());
                    a.dataBaixaPagamento = tmpData.ToString("MM/dd/yyyy");

                    a.codCanalVenda = dt.Rows[i]["canal_venda_emppar"].ToString();
                    a.tipoRetorno = "ARREC";
                    a.codOcorr = "11"; //Arrecadacao

                    //a.numeroClienteEmpPar = buscaNumeroClienteEmpParc(a.numeroCliente, a.empresaParc, a.codProduto, a.dataOcorrencia);

                    a.numeroClienteEmpPar = int.Parse(dt.Rows[i]["numero_emp_parc"].ToString());

                    //result = int.Parse(dt.Rows[0]["numero_emp_parc"].ToString());
                    l.Add(a);
                }

            }

            return l;

        }

        public List<RetornoSolicitacaoTipoC> buscaFaturamentosData(string dataOcorrencia, string loginParceira)
        {
            return buscaFaturamentos(dataOcorrencia, null, loginParceira);
        }

        public List<RetornoSolicitacaoTipoC> buscaFaturamentosData(string dataOcorrencia, Produto p, string loginParceira)
        {
            List<RetornoSolicitacaoTipoC> l = new List<RetornoSolicitacaoTipoC>();

            l = buscaFaturamentos(dataOcorrencia, p, loginParceira);

            return l;

        }

        public List<RetornoSolicitacaoTipoC> buscaFaturamentos(string dataOcorrencia, Produto p, string loginParceira)
        {
            List<RetornoSolicitacaoTipoC> l = new List<RetornoSolicitacaoTipoC>();
            StringBuilder sql = new StringBuilder();

            sql.Append(" select ");
            sql.Append(" s.numero_cliente,  ");
            sql.Append(" case when s.dv_numero_cliente is null or s.dv_numero_cliente = '' then ");
            sql.Append(" (select c.dv_numero_cliente from cliente c where c.numero_cliente = s.numero_cliente) ");
            sql.Append(" else s.dv_numero_cliente end as dv_numero_cliente, ");

            sql.Append(" s.fecha_ocorrencia, ");

            sql.Append(" s.fecha_vencimiento, ");
            sql.Append(" s.valor as valor_cargo, ");
            sql.Append(" s.referencia, ");

            sql.Append(" nvl((select c.sector from cliente c where c.numero_cliente = s.numero_cliente), 0) as sector, ");
            sql.Append(" s.codigo_cargo, ");
            sql.Append(" s.cod_produto, ");
            sql.Append(" s.empresa_parc, ");
            sql.Append(" s.canal_venda_emppar, ");
            sql.Append(" nvl(s.numero_emp_parc, 0) as numero_emp_parc ");
            sql.Append(" from sol_fat_can s   ");
            sql.Append(" where s.fecha_ocorrencia = '" + dataOcorrencia + "' ");

            if (p != null)
            {
                sql.Append(" and s.empresa_parc = '" + p.empresa.codEmpresaParc + "' ");
                sql.Append(" and s.cod_produto = '" + p.codProduto + "' ");
            }

            sql.Append(" AND s.empresa_parc in ");
            adicionaLoginParceira(sql, loginParceira);

            DataTable dt = ConsultaSql(sql.ToString());
            if (dt.Rows.Count > 0)
            {
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    RetornoSolicitacaoTipoC f = new RetornoSolicitacaoTipoC();
                    DateTime tmpData;
                    f.numeroCliente = int.Parse(dt.Rows[i]["numero_cliente"].ToString());
                    f.dvNumeroCliente = int.Parse(dt.Rows[i]["dv_numero_cliente"].ToString());

                    tmpData = Convert.ToDateTime(dt.Rows[i]["fecha_ocorrencia"].ToString());
                    f.dataOcorrencia = tmpData.ToString("MM/dd/yyyy");

                    f.empresaParc = dt.Rows[i]["empresa_parc"].ToString();
                    f.codProduto = dt.Rows[i]["cod_produto"].ToString();


                    f.valor = double.Parse(dt.Rows[i]["valor_cargo"].ToString());
                    f.referencia = dt.Rows[i]["referencia"].ToString();
                    f.loteFaturamento = int.Parse(dt.Rows[i]["sector"].ToString());

                    tmpData = Convert.ToDateTime(dt.Rows[i]["fecha_vencimiento"].ToString());
                    f.dataVencimento = tmpData.ToString("MM/dd/yyyy");

                    f.codCanalVenda = dt.Rows[i]["canal_venda_emppar"].ToString();
                    f.tipoRetorno = "FAT";
                    f.codOcorr = "12"; //Faturamento

                    f.numeroClienteEmpPar = int.Parse(dt.Rows[i]["numero_emp_parc"].ToString());


                    l.Add(f);
                }

            }

            return l;

        }

        public List<RetornoSolicitacaoTipoCFatura> consultaHistoricoFaturamentoCliente(int numeroCliente, int dvNumeroCliente, Produto p)
        {
            List<RetornoSolicitacaoTipoCFatura> result = new List<RetornoSolicitacaoTipoCFatura>();
            StringBuilder sql = new StringBuilder();

            sql.Append(" SELECT h.fecha_facturacion, ");
            sql.Append(" h.fecha_vencimiento, ");
            sql.Append(" h.ind_saldo,  ");
            sql.Append(" h.referencia,  ");
            sql.Append(" h.fecha_pago,  ");
            sql.Append(" 'Faturamento' as tipo_fat,  ");
            sql.Append(" c.valor_cargo,  ");
            sql.Append(" hp.fecha_actualiza  ");
            sql.Append(" , h.sector ");
            sql.Append(" FROM carfac c ");
            sql.Append(" , hisfac h   ");
            sql.Append(" , outer hispa hp ");
            sql.Append(" WHERE c.corr_facturacion = h.corr_facturacion  ");
            sql.Append(" AND  c.numero_cliente = h.numero_cliente  ");
            sql.Append(" AND  c.codigo_cargo in ('" + p.encargoCobranca + "'" + ",'" + p.encargoCancelamento + "', '" + p.encargoIndenizacao + "' ) ");
            //sql.Append(" AND h.indica_refact = 'N'  ");
            sql.Append(" AND  h.numero_cliente = " + numeroCliente + " ");
            sql.Append(" and h.numero_cliente = hp.numero_cliente  ");
            sql.Append(" and h.corr_facturacion = hp.correlativo ");
            sql.Append(" and hp.ind_corr_afec = 'F'  ");

            sql.Append(" UNION   ");
            sql.Append(" SELECT h.fecha_refacturac, h.fecha_vencimiento, h.ind_saldo,   ");
            sql.Append(" h.referencia_orig as referencia,  ");
            sql.Append(" h.fecha_pago,  ");
            sql.Append(" 'Refaturamento' as tipo_fat,  ");
            sql.Append(" c.valor_cargo , ");
            sql.Append(" hp.fecha_actualiza  ");
            sql.Append(" , h.sector ");
            sql.Append(" FROM drefac c ");
            sql.Append(" , refac h  ");
            sql.Append(" , outer hispa hp  ");
            sql.Append(" WHERE c.corr_refacturacion = h.corr_refacturacion  ");
            sql.Append(" AND c.numero_cliente = h.numero_cliente  ");
            sql.Append(" AND  c.codigo_cargo in ('" + p.encargoCobranca + "'" + ",'" + p.encargoCancelamento + "', '" + p.encargoIndenizacao + "' ) ");
            //sql.Append(" AND h.indica_refact = 'N'  ");
            sql.Append(" AND h.tipo_nota in ('1', 'D')  ");
            sql.Append(" AND  h.numero_cliente = " + numeroCliente + " ");
            sql.Append(" and h.numero_cliente = hp.numero_cliente  ");
            sql.Append(" and h.corr_refacturacion = hp.correlativo ");
            sql.Append(" and hp.ind_corr_afec = 'R'  ");

            sql.Append(" UNION  ");

            sql.Append(" SELECT h.fecha_facturacion, ");
            sql.Append(" h.fecha_vencimiento,  ");
            sql.Append(" h.ind_saldo,  ");
            sql.Append(" h.referencia,  ");
            sql.Append(" h.fecha_pago,  ");
            sql.Append(" 'Faturamento' as tipo_fat,  ");
            sql.Append(" c.valor_cargo  , ");
            sql.Append(" hp.fecha_actualiza  ");
            sql.Append(" , h.sector ");
            sql.Append(" FROM cliente_hist@histor:carfac_hist c,  ");
            sql.Append(" cliente_hist@histor:hisfac_hist h , ");
            sql.Append(" outer cliente_hist@histor:hispa_hist hp  ");

            sql.Append(" WHERE c.corr_facturacion = h.corr_facturacion  ");
            sql.Append(" AND  c.numero_cliente = h.numero_cliente  ");
            sql.Append(" AND  c.codigo_cargo in ('" + p.encargoCobranca + "'" + ",'" + p.encargoCancelamento + "', '" + p.encargoIndenizacao + "' ) ");
            //sql.Append(" AND h.indica_refact = 'N'  ");
            sql.Append(" AND  h.numero_cliente = " + numeroCliente + " ");
            sql.Append(" and h.numero_cliente = hp.numero_cliente  ");
            sql.Append(" and h.corr_facturacion = hp.correlativo ");
            sql.Append(" and hp.ind_corr_afec = 'F'  ");

            sql.Append(" UNION   ");
            sql.Append(" SELECT h.fecha_refacturac, h.fecha_vencimiento, h.ind_saldo,   ");
            sql.Append(" h.referencia_orig as referencia,  ");
            sql.Append(" h.fecha_pago,  ");
            sql.Append(" 'Refaturamento' as tipo_fat,  ");
            sql.Append(" c.valor_cargo , ");
            sql.Append(" hp.fecha_actualiza  ");
            sql.Append(" , h.sector ");
            sql.Append(" FROM drefac c ");
            sql.Append(" , refac h  ");
            sql.Append(" , outer hispa hp  ");
            sql.Append(" , cliente cl ");
            sql.Append(" WHERE c.corr_refacturacion = h.corr_refacturacion  ");
            sql.Append(" AND c.numero_cliente = h.numero_cliente  ");
            sql.Append(" AND  c.codigo_cargo in ('" + p.encargoCobranca + "'" + ",'" + p.encargoCancelamento + "', '" + p.encargoIndenizacao + "' ) ");
            //sql.Append(" AND h.indica_refact = 'N'  ");
            sql.Append(" AND h.tipo_nota in ('1', 'D')  ");
            sql.Append(" AND  h.numero_cliente = cl.numero_cliente ");
            sql.Append(" and h.numero_cliente = hp.numero_cliente  ");
            sql.Append(" and h.corr_refacturacion = hp.correlativo ");
            sql.Append(" and hp.ind_corr_afec = 'R'  ");
            sql.Append(" and cl.cliente_anterior = " + numeroCliente + " ");

            DataTable dt = ConsultaSql(sql.ToString());
            if (dt.Rows.Count > 0)
            {
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    RetornoSolicitacaoTipoCFatura f = new RetornoSolicitacaoTipoCFatura();
                    f.numeroCliente = numeroCliente;
                    f.empresaParc = p.empresa.codEmpresaParc;
                    f.codProduto = p.codProduto;
                    f.dvNumeroCliente = dvNumeroCliente;

                    DateTime tmpDate = Convert.ToDateTime(dt.Rows[i]["fecha_facturacion"].ToString());
                    f.dataOcorrencia = tmpDate.ToString("MM/dd/yyyy");

                    tmpDate = Convert.ToDateTime(dt.Rows[i]["fecha_vencimiento"].ToString());
                    f.dataVencimento = tmpDate.ToString("MM/dd/yyyy");

                    f.referencia = dt.Rows[i]["referencia"].ToString();

                    //tmpDate = Convert.ToDateTime(dt.Rows[i]["fecha_pago"].ToString());
                    if (DateTime.TryParse(dt.Rows[i]["fecha_pago"].ToString(), out tmpDate))
                    {
                        f.dataPagamento = tmpDate.ToString("MM/dd/yyyy");
                    }

                    f.valor = double.Parse(dt.Rows[i]["valor_cargo"].ToString());

                    f.descricaoRetorno = dt.Rows[i]["tipo_fat"].ToString();

                    //tmpDate = Convert.ToDateTime(dt.Rows[i]["fecha_actualiza"].ToString());
                    if (DateTime.TryParse(dt.Rows[i]["fecha_actualiza"].ToString(), out tmpDate))
                    {
                        f.dataBaixaPagamento = tmpDate.ToString("MM/dd/yyyy");
                    }


                    int tmpLote = 0;
                    int.TryParse(dt.Rows[i]["sector"].ToString(), out tmpLote);
                    f.loteFaturamento = tmpLote;

                    f.codOcorr = "";

                    f.estadoDocumento = dt.Rows[i]["ind_saldo"].ToString();

                    switch (f.estadoDocumento)
                    {
                        case "S":
                            f.codOcorr = "11";
                            break;
                        case "N":
                            f.codOcorr = "12";
                            break;
                        case "P":
                            f.codOcorr = "19";
                            break;
                        case "C":
                            break;
                        default:
                            break;
                    }

                    result.Add(f);
                }

            }


            return result;
        }

        public bool validaProdutoEmpresaParceira(string empresaParc, string codProduto, string loginParceira)
        {
            StringBuilder sql = new StringBuilder();
            sql.Append(" select first 1 t.codigo ");
            sql.Append(" from tabla t ");
            sql.Append(" where t.codigo = '" + empresaParc + "' ");
            sql.Append(" and t.codigo in ");
            adicionaLoginParceira(sql, loginParceira);

            DataTable dt = ConsultaSql(sql.ToString());
            if (dt.Rows.Count > 0)
            {
                return true;
            }

            return false;
        }

        public List<MovimentosProdutoCliente> consultaMovimentosSolucoes(int numeroCliente, string codProduto, string empresaParc)
        {
            List<MovimentosProdutoCliente> l = new List<MovimentosProdutoCliente>();
            StringBuilder sql = new StringBuilder();

            sql.Append(" select sm.empresa_parc ");
            sql.Append(" , sm.cod_produto ");
            sql.Append(" , sm.numero_cliente ");
            sql.Append(" , sm.dv_numero_cliente ");
            sql.Append(" , sm.codigo_ocorr ");
            sql.Append(" , sm.fecha_ocorrencia ");
            sql.Append(" , sm.valor ");
            sql.Append(" , sm.num_parcelas ");
            sql.Append(" , sm.fecha_recebido ");
            sql.Append(" , sm.num_arquivo ");
            sql.Append(" , desc_tabla('SOLMOV', sm.codigo_ocorr, '0000') as descMovimento ");
            sql.Append(" from solucoes_mov_receb sm  ");
            sql.Append(" where 1=1 ");
            sql.Append(" and sm.estado_processo = 'P' ");
            sql.Append(" and tipo_reg = 'D' ");
            sql.Append(" and sm.numero_cliente = " + numeroCliente + " ");
            sql.Append(" and sm.empresa_parc = '" + empresaParc + "' ");
            sql.Append(" and sm.cod_produto = '" + codProduto + "' ");

            DataTable dt = ConsultaSql(sql.ToString());
            if (dt.Rows.Count > 0)
            {


                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    MovimentosProdutoCliente m = new MovimentosProdutoCliente();
                    DateTime tmpData;


                    m.empresaParc = dt.Rows[i]["empresa_parc"].ToString();
                    m.codProduto = dt.Rows[i]["cod_produto"].ToString();
                    m.numeroCliente = int.Parse(dt.Rows[i]["numero_cliente"].ToString());
                    m.dvNumeroCliente = int.Parse(dt.Rows[i]["dv_numero_cliente"].ToString());
                    m.codOcorr = dt.Rows[i]["codigo_ocorr"].ToString();

                    if (DateTime.TryParse(dt.Rows[i]["fecha_ocorrencia"].ToString(), out tmpData))
                    {
                        m.dataOcorrencia = tmpData.ToString("MM/dd/yyyy");
                    }


                    m.valorParcela = double.Parse(dt.Rows[i]["valor"].ToString());
                    m.numParcelas = int.Parse(dt.Rows[i]["num_parcelas"].ToString());

                    if (DateTime.TryParse(dt.Rows[i]["fecha_recebido"].ToString(), out tmpData))
                    {
                        m.dataRecebimento = tmpData.ToString("MM/dd/yyyy"); ;
                    }


                    m.numArquivo = int.Parse(dt.Rows[i]["num_arquivo"].ToString());
                    m.descricaoMovimento = dt.Rows[i]["descMovimento"].ToString();

                    l.Add(m);
                }



            }

            return l;
        }

        public ClienteSolucoes consultaClienteSynergia(int numeroCliente)
        {
            StringBuilder sql = new StringBuilder();
            ClienteSolucoes c = new ClienteSolucoes();

            sql.Append(" select c.numero_cliente ");
            sql.Append(" , c.dv_numero_cliente      ");
            sql.Append(" , c.nombre ");
            sql.Append(" , c.rut      ");
            sql.Append(" , c.dv_rut     ");
            sql.Append(" , c.tipo_ident     ");
            sql.Append(" , desc_tabla('TIPIDE', c.tipo_ident, '0000') as desc_tipident1 ");
            sql.Append(" , c.estado_cliente       ");
            sql.Append(" , c.estado_facturacion  ");
            sql.Append(" , c.estado_suministro ");
            sql.Append(" , c.sector ");
            sql.Append(" from cliente c      ");
            sql.Append(" where 1=1  ");
            sql.Append(" and c.numero_cliente = " + numeroCliente);

            DataTable dt = ConsultaSql(sql.ToString());
            if (dt.Rows.Count > 0)
            {
                c.numeroCliente = int.Parse(dt.Rows[0]["numero_cliente"].ToString());
                c.dvNumeroCliente = int.Parse(dt.Rows[0]["dv_numero_cliente"].ToString());
                c.nome = dt.Rows[0]["nombre"].ToString();
                c.documento1 = dt.Rows[0]["rut"].ToString();
                c.dvDoc1 = dt.Rows[0]["dv_rut"].ToString();
                c.descTipoDoc1 = dt.Rows[0]["desc_tipident1"].ToString();
                c.estadoCliente = dt.Rows[0]["estado_cliente"].ToString();
                c.EstadoFaturamento = dt.Rows[0]["estado_facturacion"].ToString();
                c.estadoFornecimento = dt.Rows[0]["estado_suministro"].ToString();
                c.loteFaturamento = int.Parse(dt.Rows[0]["sector"].ToString());

            }

            return c;
        }

        //public override IList<TEntidade> dtToListObject<TEntidade>(DataTable dt)
        //{
        //    throw new NotImplementedException();
        //}

    }
}

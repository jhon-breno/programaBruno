using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Pangea.Dados.Base;
using Pangea.Entidades;
using Pangea.Entidades.Enumeracao;
using Pangea.Entidades.Base;
using Pangea.Util;
using Pangea.Entidades.DTO;

namespace Pangea.Dados
{
    public class ValidacoesDocumentoDesconexionDAO : BaseDAO
    {
        public ValidacoesDocumentoDesconexionDAO(Empresa empresa)
            : base(empresa)
        {

        }

        //public override IList<TEntidade> dtToListObject<TEntidade>(System.Data.DataTable dt)
        //{
        //    throw new NotImplementedException();
        //}

        /// <summary>
        /// Retornar validações para o tipo de corte.
        /// </summary>
        /// <param name="cod_tipo">Codigo do tipo de corte</param>
        /// <returns></returns>
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
	                             group by 1,2,3,4,5,6,7,8,9,10,11,12,13,14,15,17,16,18,19,20,21,22,23,24", lote,localidade,zona);

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

    }
}

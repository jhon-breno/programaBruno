using Pangea.Entidades;
using Pangea.Dados.Base;
using Pangea.Entidades.Base;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Pangea.Util;
using System.Web.Script.Serialization;
using Pangea.Entidades.DTO;
using Pangea.Entidades.Enumeracao;

namespace Pangea.Dados
{
    public class NISDAO : BaseDAO
    {
        private Empresa empresa;
        public NISDAO(Empresa empresa)
            : base(empresa)
        {
            this.empresa = empresa;
        }
        
        public NisDTO RetornaDadosNis(long paramNumeroNIS)
        {
            String sql = String.Format(@"SELECT nvl(a.cod_indigena_fam,'2') as ind_indigena,
                                                nvl(a.ind_quilombola_fam,'2') as ind_quilombola,
                                                (medsal.valor * salmin.valor) as valor_salario,
                                                case when a.dat_atual_fam + 2 units year > today then 1 else 0 end as data_de_vigencia,
                                                case when vlr_renda_media_fam <= (salmin.valor /2) then 1 else 0 end as ind_per_capita, 
                                                nvl(a.vlr_renda_total_fam,0) as valor_medio,
                                                a.cod_familiar as codigo_familiar,
                                                case when (a.dat_atual_fam + 730 units day) > (today + 47 units day) then 1 else 0 END as ind_a_vencer,
                                                TO_CHAR((a.dat_atual_fam + 730 units day),'%d/%m/%Y') as Data_Atual,
                                                a.dat_atual_fam + 730 units day - today   as qtd_dias ,
                                                a.numero_nis                                        
                                           FROM nis_mds a, tabla salmin, tabla medsal
                                          WHERE salmin.sucursal = '0000' 
                                            AND salmin.nomtabla = 'SALMIN'
                                            AND salmin.codigo = '1' 
                                            AND medsal.sucursal = '0000' 
                                            AND medsal.nomtabla = 'MEDSAL' 
                                            AND medsal.codigo = '1' 
                                            AND a.numero_nis = {0}", paramNumeroNIS);

            DataTable dtResultado = ConsultaSql(sql);

            if (dtResultado.Rows.Count > 0)
            {        
                NisDTO nisRetorno =  gerarEntidadeNis(dtResultado);
               return GetDadosClasseNIS(nisRetorno);
            }
            else
                return null;

        }

        public NisDTO gerarEntidadeNis(DataTable resultDt)
        {

            NisDTO entity = new NisDTO();
            if (resultDt.Rows.Count > 0)
            {
                entity.NumeroNis = TratarString(resultDt, resultDt.Rows[0], "numero_nis");
                entity.codigo_familiar = TratarString(resultDt, resultDt.Rows[0], "codigo_familiar");
                entity.data_de_vigencia = TratarString(resultDt, resultDt.Rows[0], "data_de_vigencia");
                entity.ind_indigena = TratarString(resultDt, resultDt.Rows[0], "ind_indigena");
                entity.ind_quilombola = TratarString(resultDt, resultDt.Rows[0], "ind_quilombola");
                entity.valor_salario = TratarString(resultDt, resultDt.Rows[0], "valor_salario");
                entity.valor_medio = TratarString(resultDt, resultDt.Rows[0], "valor_medio");
                entity.ind_per_capita = TratarString(resultDt, resultDt.Rows[0], "ind_per_capita");
                entity.ind_a_vencer = TratarString(resultDt, resultDt.Rows[0], "ind_a_vencer");
                entity.data_atual = TratarString(resultDt, resultDt.Rows[0], "data_atual");
                entity.qtd_dias = TratarString(resultDt, resultDt.Rows[0], "qtd_dias");
            }
            return entity;
        }

        public NisDTO GetDadosClasseNIS(NisDTO paramNis)
        {
            String sql = String.Format(@"SELECT cli.classe || '' || cli.subclasse as classe 
                                         FROM cliente_doc_bxr cli 
                                         where cli.numero_nis = '{0}'", paramNis.NumeroNis);

            DataTable dtResultado = ConsultaSql(sql);

            if (dtResultado.Rows.Count > 0)
            {
                paramNis.classe = TratarString(dtResultado, dtResultado.Rows[0], "classe");
            }

                return paramNis;
        
        }

        public Boolean NisAssociadoOutroCliente(long paramNumeroNis, int paramNumeroCliente)
        {
            String sql = String.Format(@"SELECT numero_cliente 
                                           FROM cliente 
                                          WHERE numero_nis = '{0}' 
                                            AND numero_cliente <> {1}", paramNumeroNis, paramNumeroCliente);
            DataTable dtResultado = ConsultaSql(sql);

            if (dtResultado.Rows.Count > 0)
                return true;
            else
                return false;

        }


        public Boolean NisAssociadoNovoCliente(long paramNumeroNis)
        {
            String sql = String.Format(@"SELECT C.numero_cliente 
                                           FROM ordem_servico O,
                                                cliente_novo C
                                          WHERE O.tipo_ordem = 'NOV'
                                            AND O.estado not in ('06','09')
                                            AND O.numero_ordem = C.numero_ordem 
                                            AND C.numero_NIS = '{0}'
                                            AND O.numero_ordem NOT IN (SELECT o1.numero_ordem 
                                                                         FROM ordem_servico o1,
                                         									  visita_ordem v,
                                         									  retorno_servico r
                                         				                WHERE o1.numero_ordem = O.numero_ordem
                                         				                  AND o1.estado = '04'
                                         				                  AND v.numero_ordem = o1.numero_ordem
                                         				                  AND v.corr_visita = o.corr_visita
                                         				                  AND r.tipo_ordem = o1.tipo_ordem
                                         				                  AND r.tipo_servico = o1.tipo_servico
                                         				                  AND r.etapa = o1.etapa
                                         				                  AND r.codigo_retorno = v.cod_retorno
                                         				                  AND r.ind_encerra_ordem = 'S')", paramNumeroNis);
            DataTable dtResultado = ConsultaSql(sql);

            if (dtResultado.Rows.Count > 0)
                return true;
            else
                return false;
        }    
    }
}

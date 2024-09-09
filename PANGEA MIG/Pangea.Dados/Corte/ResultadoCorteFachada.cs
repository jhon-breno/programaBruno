using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Pangea.Dados.Base;
using Pangea.Entidades;
using Pangea.Entidades.Enumeracao;

namespace Pangea.Dados
{
    public class ResultadoCorteFachada : BaseDAO
    {
        public ResultadoCorteFachada(Empresa empresa)
            : base(empresa)
        {

        }

        public ResultadoCorteEntity RetornaCorte(int numero_cliente, int corr)
        {
            String sql = String.Format(@"select 
		                                corsoco.sucursal,
		                                corsoco.numero_livro, 		
		                                correp.corr_corte,
		                                corsoco.estado,
		                                correp.numero_cliente,
		                                corsoco.motivo_sol,		
		                                corsoco.tipo_corte,
		                                corsoco.numero_ordem,
		                                correp.corr_reaviso,
		                                correp.numero_ordem_corte,
		                                correp.data_solic_corte,		
		                                correp.fecha_corte,
		                                correp.hora_exec_corte,
		                                correp.fase_corte,
		                                correp.motivo_corte,
		                                correp.acc_realizada_cor,
		                                correp.irreg_instalacao,
		                                correp.sit_encon_cor,
		                                correp.leitura_corte,		
		                                correp.fecha_reposicion,
		                                correp.tipo_religacao,
		                                correp.acc_realizada_rep,
		                                correp.sit_encon_rep,
		                                correp.leitura_repo,
		                                correp.numero_ordem_repo,
		                                correp.fecha_solic_repo,
		                                correp.data_inicio_cort,
		                                correp.data_inicio_relg			
                            from correp,corsoco
                            where  correp.numero_cliente = corsoco.numero_cliente
                                    and correp.corr_corte = corsoco.corr_corte
                                    and correp.numero_cliente={0}
                                    and correp.corr_corte={1}", numero_cliente, corr);

            var dt = ConsultaSql(sql);

            ResultadoCorteEntity resultado = gerarEntidadeCorte(dt);

            return resultado;
        }

        public ResultadoCorteEntity gerarEntidadeCorte(DataTable resultDt)
        {

            ResultadoCorteEntity entity = new ResultadoCorteEntity();
            if (resultDt.Rows.Count > 0)
            {

                entity.sucursal = TratarString(resultDt, resultDt.Rows[0], "sucursal");
                entity.numero_livro = TratarString(resultDt, resultDt.Rows[0], "numero_livro");
                entity.corr_corte = TratarInt(resultDt, resultDt.Rows[0], "numero_livro", -1);
                entity.estado = TratarString(resultDt, resultDt.Rows[0], "estado");
                entity.motivo_sol = TratarString(resultDt, resultDt.Rows[0], "motivo_sol");
                entity.tipo_corte = TratarString(resultDt, resultDt.Rows[0], "tipo_corte");
                entity.numero_cliente = TratarInt(resultDt, resultDt.Rows[0], "numero_cliente", -1);
                entity.numero_ordem = TratarString(resultDt, resultDt.Rows[0], "numero_ordem");
                entity.corr_reaviso = TratarString(resultDt, resultDt.Rows[0], "corr_reaviso");
                entity.numero_ordem_corte = TratarInt(resultDt, resultDt.Rows[0], "numero_ordem_corte", -1);
                entity.data_solic_corte = TratarDateTime(resultDt, resultDt.Rows[0], "data_solic_corte");
                entity.fecha_corte = TratarDateTime(resultDt, resultDt.Rows[0], "fecha_corte");
                entity.hora_exec_corte = TratarDateTime(resultDt, resultDt.Rows[0], "hora_exec_corte");
                entity.fase_corte = TratarString(resultDt, resultDt.Rows[0], "fase_corte");
                entity.motivo_corte = TratarString(resultDt, resultDt.Rows[0], "motivo_corte");
                entity.acc_realizada_cor = TratarString(resultDt, resultDt.Rows[0], "acc_realizada_cor");
                entity.irreg_instalacao = TratarString(resultDt, resultDt.Rows[0], "irreg_instalacao");
                entity.sit_encon_cor = TratarString(resultDt, resultDt.Rows[0], "sit_encon_cor");
                entity.leitura_corte = TratarInt(resultDt, resultDt.Rows[0], "leitura_corte", -1);
                entity.fecha_reposicion = TratarDateTime(resultDt, resultDt.Rows[0], "fecha_reposicion");
                entity.tipo_religacao = TratarString(resultDt, resultDt.Rows[0], "tipo_religacao");
                entity.acc_realizada_rep = TratarString(resultDt, resultDt.Rows[0], "acc_realizada_rep");
                entity.sit_encon_rep = TratarString(resultDt, resultDt.Rows[0], "sit_encon_rep");
                entity.leitura_repo = TratarInt(resultDt, resultDt.Rows[0], "leitura_repo", -1);
                entity.numero_ordem_repo = TratarInt(resultDt, resultDt.Rows[0], "numero_ordem_repo", -1);
                entity.fecha_solic_repo = TratarDateTime(resultDt, resultDt.Rows[0], "fecha_solic_repo");
                entity.data_inicio_cort = TratarDateTime(resultDt, resultDt.Rows[0], "data_inicio_cort");
                entity.data_inicio_relg = TratarDateTime(resultDt, resultDt.Rows[0], "data_inicio_relg");


                return entity;
            }
            return new ResultadoCorteEntity();
        }

        //public override IList<TEntidade> dtToListObject<TEntidade>(DataTable dt)
        //{
        //    return new List<TEntidade>();
        //}
    }
}

using Pangea.Util;
using Pangea.Dados.Base;
using Pangea.Entidades;
using Pangea.Entidades.Enumeracao;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Pangea.Entidades.DTO;

namespace Pangea.Dados
{
    public class SuministroDAO : BaseDAO
    {
        public SuministroDAO(Empresa empresa, string database)
            : base(empresa, database)
        {
        }

        public SuministroDTO RetornaSuministro(int numero_cliente)
        {

            string sql = String.Format(@"SELECT numero_cliente, 
                                         'true' as restriccion_convenio, 
                                         'true' as restriccion_cambio_de_corte,
                                         'true' as pago_en_proceso,
                                         estado_suministro as estado_conexion,
                                         estado_cliente as estado_suministro,
                                         estado_facturacion as periodo_de_facturacion,
                                         direccion,fecha_a_corte as fecha_corte
                                         FROM   cliente
                                         WHERE  numero_cliente = {0}", numero_cliente);

            var dt = ConsultaSql(sql);
            
            SuministroDTO resultado = DataHelper.ConvertDataTableToEntity<SuministroDTO>(dt);

            return resultado;
        }
        public SuministroSynergiaDTO ConsultaSuministroSynergia(int numero_cliente, string tipocliente)
        {
            string toi = tipocliente == "AT" ? "'false'" : "CASE WHEN ind_ro = 'S' THEN 'true' ELSE 'false' END";
            SuministroSynergiaDTO resultado = null;
            string sql = String.Format(@"SELECT numero_cliente, 
                                         estado_suministro as estado_conexion,
                                         estado_cliente as estado_suministro, 
                                         {1}
                                         AS toi
                                         FROM   cliente
                                         WHERE  numero_cliente = {0}", numero_cliente, toi);

            var dt = ConsultaSql(sql);
            if ((dt != null) && (dt.Rows.Count > 0))
            {
                resultado = DataHelper.ConvertDataTableToEntity<SuministroSynergiaDTO>(dt);



                string sql2 = String.Format(@"SELECT cor.numero_cliente
                                          FROM correp  cor
                                          WHERE cor.motivo_corte in ('01','19')
                                          AND cor.numero_cliente = {0}", numero_cliente);

                var dt2 = ConsultaSql(sql);

                if(dt2 != null)
                resultado.corte_deuda = dt2.Rows.Count > 0;
            }

            return resultado;
        }


        public bool AtualizacaoSuministro(int numero_cliente, string restriccion_convenio, string direccion, string restriccion_cambio_de_corte)
        {
            DBProviderInformix informix = ObterProviderInformix(); ;

            StringBuilder sql = new StringBuilder();
            sql.Append("update cliente set  ");
            //if (!String.IsNullOrWhiteSpace(restriccion_convenio))
            //{
            //    sql.AppendFormat(@"restriccion_convenio = '{0}', ", restriccion_convenio);
            //}
            //if (!String.IsNullOrWhiteSpace(restriccion_cambio_de_corte))
            //{
            //    sql.AppendFormat(@"restriccion_cambio_de_corte = '{0}', ",restriccion_cambio_de_corte);
            //}
            if (!String.IsNullOrWhiteSpace(direccion))
            {
                sql.AppendFormat(@"direccion = '{0}'",direccion);
            }
            sql.AppendFormat("where numero_cliente = {0}",numero_cliente);

            sql = sql.Replace(", where", " where");

            return ExecutarSql(sql.ToString(), informix);
        }


    }
}

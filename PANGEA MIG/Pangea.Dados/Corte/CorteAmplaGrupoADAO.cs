using Pangea.Entidades.Enumeracao;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Pangea.Dados.Corte
{
    public class CorteAmplaGrupoADAO : CorteGrupoADAO
    {

        public CorteAmplaGrupoADAO()
            : base(Empresa.RJ)
        {
        }



        /// <summary>
        /// Método cria a string de verificação do motivo de corte do cliente.
        /// Método para clientes Grupo A.
        /// </summary>
        /// <param name="numeroCliente"></param>
        /// <returns>String</returns>
        /// <remarks>O campo (fecha_corte - Grupo A) foi mapeado em relacao ao campo (data_atual_corte - Grupo B).</remarks>
        public override string gerarSQLObterMotivoDoCorteAtualGA(int numeroCliente)
        {

            var sql = new StringBuilder();

            sql.Append(" select  distinct  ");
            sql.Append(" c.numero_cliente, ");
            sql.Append(" co.tipo_corte, ");
            sql.Append(" co.motivo_corte, ");
            sql.Append(" co.fecha_reposicion, ");
            sql.Append(" c.dv_numero_cliente ");
            sql.Append(" c.Nombre, ");
            sql.Append(" c.estado_cliente ");

            sql.Append(" from ");
            sql.Append(" GRANDES:cliente c, ");
            sql.Append(" GRANDES:correp co ");
            sql.Append(" where c.estado_cliente = 8 ");
            sql.Append(" and c.estado_suministro = 1 ");
            sql.Append(" and c.numero_cliente = co.numero_cliente ");
            sql.Append(" and co.corr_corte = ");
            sql.Append(" (select max(corr_corte)  ");
            sql.Append(" from GRANDES:correp  ");
            sql.Append(" where numero_cliente = c.numero_cliente) ");
            sql.Append(" and co.corr_corte = c.corr_corte ");            
            sql.Append(" and co.motivo_corte in ('04','05','06','07') ");
            sql.Append(" and co.numero_cliente = " + numeroCliente);

            return sql.ToString();

        }


        public override string gerarSQLObterMotivoDoCorteAtualSalesForceGA(int numeroCliente)
        {

            var sql = new StringBuilder();

            sql.Append(" select  distinct                              ");
            sql.Append(" c.numero_cliente,                    ");
            sql.Append(" co.valor_cobranca as valor,                   ");
            sql.Append(" co.motivo_corte as guion,                    ");
            sql.Append(" to_char(co.fecha_corte,'%d/%m/%Y') as fecha,  ");
            sql.Append(" to_char(co.fecha_corte,'%H:%M') as Hora,      ");
            sql.Append(" c.Nombre as Nome,                             ");
            sql.Append(" c.rut as documento1,                          ");
            sql.Append(" c.dv_rut as dvDoc1,                           ");
            sql.Append(" case when c.estado_cliente = 8 then '0' else c.estado_cliente end as EstadoCliente, ");
            sql.Append(" c.estado_suministro as estadoFornecimento,    ");
            sql.Append(" c.estado_facturacion as EstadoFaturamento,    ");
            sql.Append(" c.sector as loteFaturamento                   ");
            sql.Append(" from                                          ");
            sql.Append(" GRANDES:cliente c,                            ");
            sql.Append(" outer GRANDES:correp co                       ");
            sql.Append(" where 1=1                                     ");
            sql.Append(" AND nvl(co.fecha_reposicion,'') = ''          ");
            sql.Append(" and c.numero_cliente = co.numero_cliente      ");
            sql.Append(" and co.corr_corte =                           ");
            sql.Append(" (select max(corr_corte)                       ");
            sql.Append(" from GRANDES:correp                           ");
            sql.Append(" where numero_cliente = c.numero_cliente)      ");
            sql.Append(" and co.corr_corte = c.corr_corte              ");
            sql.Append(" and co.motivo_corte in ('04','05','06','07')  ");
            sql.Append(" and c.numero_cliente =                       " + numeroCliente);
            sql.Append(" order by fecha desc                            ");

            return sql.ToString();

        }

    }
}

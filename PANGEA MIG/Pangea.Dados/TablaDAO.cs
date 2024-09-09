using Pangea.Dados.Base;
using Pangea.Entidades;
using Pangea.Entidades.Enumeracao;
using Pangea.Util;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pangea.Dados
{
    public class TablaDAO : BaseDAO
    {
        public TablaDAO(Empresa empresa)
            : base(empresa)
        {
        }
        
        public List<Tabla> Consultar(string nomeTabla, string sucursal)
        {
            List<Tabla> result = new List<Tabla>();

            StringBuilder sql = new StringBuilder("select trim(t.codigo) as codigo, ");
            sql.Append(" trim(t.descripcion) as descripcion, ");
            sql.Append(" trim(t.nomtabla) as nomtabla ");
            sql.Append(" from tabla t  ");
            sql.Append(" where t.fecha_desactivac is null ");
            sql.AppendFormat(" and t.sucursal = '{0}' ", sucursal);
            sql.AppendFormat(" and t.nomtabla = '{0}' ", nomeTabla);


            using (DataTable dt = ConsultaSql(sql.ToString()))
            {
                if (dt != null && dt.Rows.Count > 0)
                {
                    foreach(DataRow linha in dt.Rows)
                    {
                        Tabla t = new Tabla();
                        t.codigo = DBNull.Value.Equals(linha["codigo"]) ? string.Empty : linha["codigo"].ToString();
                        t.descricao = DBNull.Value.Equals(linha["descripcion"]) ? string.Empty : linha["descripcion"].ToString();
                        t.nomeTabla = DBNull.Value.Equals(linha["nomtabla"]) ? string.Empty : linha["nomtabla"].ToString();

                        result.Add(t);
                    }
                }
            }
            return result;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="sucursal"></param>
        /// <param name="nomeTabla"></param>
        /// <param name="codigo"></param>
        /// <returns></returns>
        public int ObterSequencial(string sucursal, string nomeTabla, string codigo)
        {
            int sequencial = 0;

            StringBuilder consulta = new StringBuilder();
            StringBuilder update = new StringBuilder();
            StringBuilder condicao = new StringBuilder();
            
            consulta.Append(@"SELECT valor
                                 FROM tabla ");
            
            update.Append("UPDATE tabla set valor = {0} ");

            condicao.AppendFormat(@" WHERE sucursal = '{0}'
                                  AND nomtabla = '{1}'
                                  AND codigo   = '{2}'
                                  AND fecha_activacion <= today
                                  AND (fecha_desactivac > today or fecha_desactivac is null) "
                , sucursal
                , nomeTabla
                , codigo);

            using (DBProviderInformix conn = this.ObterProviderInformix())
            {
                conn.OpenConnection();                
                conn.BeginTransacion(IsolationLevel.ReadCommitted);

                try
                {
                    //consulta o sequencial atual
                    using (DataTable dt = ConsultaSql(string.Concat(consulta.ToString(), condicao.ToString()), conn))
                    {
                        if (dt != null && dt.Rows.Count > 0)
                        {
                            //TODO: testar com resultado nulo para ver se o fluxo segue com sequencial = 0 ou se lança exceção
                            if (Int32.TryParse(dt.Rows[0][1].ToString(), out sequencial))
                            {
                                //incrementa o sequencial
                                sequencial = sequencial > 999999 ? 1 : sequencial++;

                                //atualiza o sequencial
                                if (!ExecutarSql(string.Concat(string.Format(update.ToString(), sequencial), condicao.ToString())
                                                , conn))
                                {
                                    throw new Exception("Erro ao atualizar a sequência.");
                                }

                                conn.Commit();

                                return sequencial;
                            }
                        }

                        throw new Exception("Código sequencial nãoa encontrado.");
                    }
                }
                catch (Exception ex)
                {
                    conn.Rollback();
                    throw ex;
                }
            }
        }
    }
}

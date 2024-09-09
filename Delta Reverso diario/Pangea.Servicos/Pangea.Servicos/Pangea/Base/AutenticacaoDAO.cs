using Pangea.Entidades;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Pangea.Dados.Base
{
    public class AutenticacaoDAO : BaseDAO
    {
        public AutenticacaoDAO(Empresa empresa)
            : base(empresa)
        {
        }



        protected virtual string gerarSqlValidarCanalChave(string canal, string chave)
        {
            StringBuilder sql = new StringBuilder();
            sql.Append(" select first 1 ");
            sql.Append(" codigo ");
            sql.Append(" from tabla ");
            sql.Append(" where nomtabla = 'WSSOLU' ");
            sql.Append(" and sucursal = '0000' ");
            sql.Append(" and fecha_desactivac is null ");
            sql.Append(" and codigo = '" + canal + "' ");
            sql.Append(" and valor_alf = '" + chave + "' ");

            return sql.ToString();
        }

        public bool ValidarCanalChave(string canal, string chave)
        {
            string sql = gerarSqlValidarCanalChave(canal, chave);

            return ConsultaSql(sql).Rows.Count > 0;
        }
    }
}

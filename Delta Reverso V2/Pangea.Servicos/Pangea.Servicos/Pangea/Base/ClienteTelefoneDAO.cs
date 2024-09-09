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


namespace Pangea.Dados
{
    public class ClienteTelefoneDAO : BaseDAO
    {
        private Empresa empresa;
        public ClienteTelefoneDAO(Empresa empresa, string database = "")
            : base(empresa)
        {
            this.empresa = empresa;
        }

        public List<string> GetNumeroCliente(string tipo_doc, string numero_documento, string numero_dv)
        {
            string sql = String.Format(@"SELECT numero_cliente
                                            FROM cliente 
                                              WHERE tipo_ident = '{0}'
	                                            AND rut = '{1}'
	                                            AND dv_rut = '{2}'
                                                AND classe not in ('05','06','07')
                                                UNION 
                                                SELECT numero_cliente
                                            FROM cliente 
                                              WHERE tipo_ident = '{0}'
	                                            AND rut = '{3}'
	                                            AND dv_rut = '{2}'
                                                AND classe not in ('05','06','07')
                                                ", tipo_doc, numero_documento, numero_dv,numero_documento.PadLeft(20,'0'));

            var dt = ConsultaSql(sql.ToString());
            List<String> retorno = new List<String>();
            if (dt.Rows.Count > 0)
            {
                foreach (DataRow dr in dt.Rows)
                {
                    retorno.Add(dr[0].ToString());
                }
                return retorno;
            }
            else
                return retorno;
        }

        public List<ClientesTelefone> ListTelefones(string numero_cliente)
        {
            string sql = String.Format(@"SELECT c.numero_cliente, t.tipo_telefone, TRIM(t.prefixo_ddd) as prefixo_ddd, TRIM(t.numero_telefone) as numero_telefone, c.tipo_ident as tipo_doc, c.rut, c.dv_rut
                                            FROM cliente c
                                            JOIN telefone_cliente t
                                            ON t.numero_cliente = c.numero_cliente
                                              WHERE  c.numero_cliente = '{0}'
                                         UNION
                                         SELECT  c.numero_cliente,'' AS tipo_telefone, c.ddd as prefixo_ddd,  c.telefono as numero_telefone, c.tipo_ident as tipo_doc, c.rut, c.dv_rut
                                             FROM cliente  c
                                              WHERE  c.numero_cliente = '{0}'
	                                            AND (telefono IS NOT NULL OR telefono !='')
                                         UNION 
                                         SELECT  c.numero_cliente,'' AS tipo_telefone, c.ddd2 as prefixo_ddd, c.telefono2 as numero_telefone, c.tipo_ident as tipo_doc, c.rut, c.dv_rut
                                             FROM cliente c
                                              WHERE  c.numero_cliente = '{0}'
	                                            AND (telefono IS NOT NULL OR telefono !='')", numero_cliente);


            var dt = ConsultaSql(sql.ToString());

            if (dt.Rows.Count > 0)
                return DataHelper.ConvertDataTableToList<ClientesTelefone>(dt);
            else
                return new List<ClientesTelefone>();
        }

        public Boolean InsertTelefoneCliente(ClientesTelefone paramCli)
        {
            bool controle = false;
            int i = 0;

            while (!controle)
            {
                try
                {
                    string sql1 = String.Format(@"INSERT INTO telefone_cliente (numero_cliente, tipo_telefone, prefixo_ddd, numero_telefone, ind_contato)
                                             VALUES({0},'{1}','{2}','{3}','N')", paramCli.numero_cliente, paramCli.tipo_telefone, paramCli.prefixo_ddd, paramCli.numero_telefone);

                    using (DBProviderInformix informix = ObterProviderInformix())
                    {
                        ExecutarSql(sql1.ToString(), informix);
                    }

                    sql1 = null;
                    
                    StringBuilder sql = new StringBuilder();

                    sql.Append(" INSERT INTO clientes@clientes:Modif ");
                    sql.Append(" (numero_cliente, ");
                    sql.Append("  Tipo_Orden     , ");
                    sql.Append("  Numero_Orden   , ");
                    sql.Append("  Ficha          , ");
                    sql.Append("  Fecha_Modif    , ");
                    sql.Append("  Tipo_Cliente   , ");
                    sql.Append("  Codigo_Modif   , ");
                    sql.Append("  Dato_Anterior  , ");
                    sql.Append("  Dato_Nuevo     , ");
                    sql.Append("  Proced ) ");
                    sql.Append(" VALUES ( ");
                    sql.Append(paramCli.numero_cliente + ", ");
                    sql.Append("  'MOD',  ");
                    sql.Append("  0, ");
                    sql.Append("  '', ");
                    sql.Append("  Current, ");
                    sql.Append("  'A', ");
                    sql.Append("  '8', ");
                    sql.Append("  '', ");
                    sql.Append("  '" + paramCli.numero_telefone + "', ");
                    sql.Append("  'DELTAREVERSO');\n ");

                    using (DBProviderInformix informix = ObterProviderInformix())
                    {
                        return ExecutarSql(sql.ToString(), informix);
                    }

                    sql = null;
                    controle = true;
                }
                catch
                {
                    paramCli.tipo_telefone = Convert.ToString(Convert.ToInt32(paramCli.tipo_telefone) + 1).PadLeft(2, '0');
                    i++;
                    if (i == 10) return false;
                }
            }

            return true;
        }

        public Boolean UpdateTelefoneCliente(ClientesTelefone paramCli)
        {
            string sql = String.Format(@"UPDATE telefone_cliente
                                         SET numero_telefone = {0},
                                             prefixo_ddd = {1}
                                       WHERE numero_cliente = {2}
                                         AND tipo_telefone = {3}", paramCli.numero_telefone, paramCli.prefixo_ddd, paramCli.numero_cliente, paramCli.tipo_telefone);

            using (DBProviderInformix informix = ObterProviderInformix())
            {
                return ExecutarSql(sql.ToString(), informix);
            }

            sql= null;
        }


        internal string getEmail(string p1)
        {
            string sql = String.Format(@"SELECT c.mail
                                            FROM cliente c
                                            WHERE  c.numero_cliente = '{0}'", p1);


            var dt = ConsultaSql(sql.ToString());

            if (dt.Rows.Count > 0)
                return dt.Rows[0][0].ToString();
            else
                return "";
        }

        internal void InsertGenerico(StringBuilder sql)
        {
            using (DBProviderInformix informix = ObterProviderInformix())
            {
                ExecutarSql(sql.ToString(), informix);
            }
        }
    }
}

using IBM.Data.Informix;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SalesforceExtractor.Dados.InformixBase
{
    public class DbInformix : IDisposable
    {
        public static IfxConnection con = null;

        public DbInformix(string connectionString)
        {
            con = RealizaConexao(connectionString);
        }

        public IfxConnection RealizaConexao(string connectionString)
        {
            IfxConnection con;

            con = new IfxConnection(connectionString);


            if (con.State == ConnectionState.Closed)
            {
                con.Open();
            }

            return con;
        }

        public void FecharConexao()
        {
            if (con != null)
            {

                if (con.State == ConnectionState.Open)
                {
                    con.Close();
                }

                con.Dispose();
            }
        }

        public IfxTransaction AbrirTransacao()
        {
            if (con.State == ConnectionState.Closed)
            {
                con.Open();
            }
            IfxTransaction trans = con.BeginTransaction(IsolationLevel.ReadUncommitted);

            return trans;
        }


        public int EnviarComando(string sql, IfxTransaction trans)
        {
            int totalCmdsSucesso = 0;
            try
            {
                IfxCommand cmd = new IfxCommand("set pdqpriority 80; set isolation to dirty read;" + sql, con);
                cmd.CommandTimeout = 100000;
                cmd.Transaction = trans;
                totalCmdsSucesso = cmd.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                throw ex;
            }

            return totalCmdsSucesso;
        }

        public DataTable RetornaDataTable(string sql, IfxTransaction trans)
        {
            DataTable dt = new DataTable();
            try
            {
                IfxCommand command = new IfxCommand(sql, con, trans);
                command.CommandTimeout = 10000;
                command.CommandText = sql;
                using (IfxDataReader dr = command.ExecuteReader())
                {
                    dt.BeginLoadData();
                    dt.Load(dr);
                    dt.EndLoadData();
                }

            }
            catch (Exception ex)
            {

                // throw ex;
            }
            return dt;
        }

        public void RetornaDataTableStreet(string sql, IfxTransaction trans)
        {
            IfxCommand command = new IfxCommand(sql, con, trans);
            command.CommandTimeout = 10000;

            StringBuilder result = new StringBuilder();
            int contaLinha = 0;
            using (IfxDataReader reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    for (int i = 0; i < reader.FieldCount; i++)
                    {
                        result.Append(reader.GetString(i).Trim());
                        result.Append("|");
                    }
                    result.Append("\n");
                    contaLinha++;
                }
            }
        }


        public void Dispose()
        {
            if (con != null)
            {

                if (con.State == ConnectionState.Open)
                {
                    con.Close();
                }

                con.Dispose();
            }
        }
    }
}

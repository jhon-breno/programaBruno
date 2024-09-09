using Pangea.Dados.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IBM.Data.Informix;
using System.Data;
using Pangea.Entidades.Enumeracao;

namespace Pangea.Swat.Dados
{
    public class ExecutorDB : BaseDAO
    {
        public ExecutorDB(Empresa empresa, string tipoCliente = "BT") : base(empresa, tipoCliente)
        {}


        public DataTable ExecutarDataTable(string comandoSql, bool useDirtyRead = true)
        {            
            DataTable result = null;
            try
            {
                using (DBProviderInformix informix = ObterProviderInformix())
                {
                    informix.PrepareCommand(comandoSql, useDirtyRead);
                    result = informix.ExecuteDataReader();
                }
            }
            catch (Exception e)
            {
                throw e;
            }

            return result;
        }


        public int ExecutarComandoSql(string comandoSql)
        {
            bool useDirtyRead = false;

            try
            {
                using (DBProviderInformix informix = ObterProviderInformix())
                {
                    informix.PrepareCommand(comandoSql, useDirtyRead);
                    return informix.ExecuteNonQuery();
                }
            }
            catch (Exception e)
            {
                throw e;
            }
        }


        public int ExecutarComandosSql(List<string> comandosSql)
        {
            bool useDirtyRead = false;
            int totalExecutado = 0;
            int cont = 0;
            int paginas = 0;
            StringBuilder loteSql = new StringBuilder();
            try
            {
                using (DBProviderInformix informix = ObterProviderInformix())
                {
                    foreach (string sql in comandosSql)
                    {
                        cont++;
                        if (cont == 2000)
                        {
                            informix.PrepareCommand(loteSql.ToString(), useDirtyRead);
                            totalExecutado += informix.ExecuteNonQuery();
                            if (totalExecutado > 0)
                            {
                            }

                            paginas++;
                            cont = 0;
                            loteSql.Clear();
                        }

                        if(loteSql.Length > 0)
                            loteSql.AppendFormat("{0}", Environment.NewLine);

                        loteSql.Append(sql.ToString());
                    }

                    if (cont > 0)
                    {
                        informix.PrepareCommand(loteSql.ToString(), useDirtyRead);
                        totalExecutado += informix.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception e)
            {
                throw e;
            }
            return totalExecutado;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Pangea.Dados.Base;
using Pangea.Entidades;
using Pangea.Entidades.Enum;

namespace Pangea.Pangea.Dados
{
    public class GenericaPangeaFachada : BaseDAO
    {
        public GenericaPangeaFachada(Empresa empresa)
            : base(empresa)
        { 
        
        }
        /// <summary>
        /// FOT
        /// </summary>
        /// <param name="codigoExec">String com codigos de execução Ex: "1,2"</param>
        /// <returns></returns>
        public DataTable RetornoComando(String codigoExec)
        {
          
            String sql = @"select id,cod_exec,       
                               parametros,     
                               status  
                        from generica_pangea
                        where cod_exec in ("+ codigoExec +@")
                        and status ='I'";

            return ConsultaSql(sql);
        }


        public bool AtualizaStatus(char pStatus, int pId, DBProviderInformix pConn)
        {
            bool result = false;

            String sql = String.Format(@"update generica_pangea set status='{0}'
                                         where id={1}", pStatus, pId);

            result = ExecutarSql(sql.ToString(), pConn);

            return result;

        }


        
    }
}

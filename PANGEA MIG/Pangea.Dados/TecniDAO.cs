using Entidades.DTO;
using Pangea.Dados.Base;
using Pangea.Entidades.Enumeracao;
using Pangea.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pangea.Dados
{
    public class TecniDAO  : BaseDAO
    {
        public TecniDAO(Empresa empresa)
            : base(empresa)
        {
        }

        public TecniDTO RetornaInforTrafo(int numero_cliente)
        {
            string sql = string.Format(@"SELECT 
                                            numero_equipo,
                                            tipo_tranformador,    
                                            subestac_transmi,
                                            medida_tecnica,
                                            alimentador,
                                            acometida_retirada
                                        FROM tecni                                        
                                        WHERE numero_cliente ={0}", numero_cliente);

            var dt = ConsultaSql(sql.ToString());
            if (dt.Rows.Count > 0)
            {
                return DataHelper.ConvertDataTableToEntity<TecniDTO>(dt);
            }
            else
                return null;

        }
    }
}

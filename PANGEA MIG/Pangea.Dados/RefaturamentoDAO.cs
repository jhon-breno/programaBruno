using Pangea.Dados.Base;
using Pangea.Entidades;
using Pangea.Entidades.DTO;
using Pangea.Entidades.Enumeracao;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pangea.Dados
{
    public class RefaturamentoDAO : BaseDAO
    {
        public RefaturamentoDAO(Empresa empresa)
            : base(empresa)
        {
        }
        
        public DataTable Consultar(RefaturamentoDTO obj)
        {
            StringBuilder sql = new StringBuilder(@"SELECT * FROM refac WHERE 1=1 ");
            
            if(!string.IsNullOrEmpty(obj.numero_cliente))
            sql.AppendFormat(" numero_cliente = '{0}' ", obj.numero_cliente); 

            if (!string.IsNullOrEmpty(obj.motivo_refacturac))
                sql.AppendFormat(" motivo_refacturac = '{0}' ", obj.motivo_refacturac); 

            if (!string.IsNullOrEmpty(obj.tipo_nota))
                sql.AppendFormat(" tipo_nota in ({0}) ", obj.tipo_nota); 

            if (!string.IsNullOrEmpty(obj.indica_refact))
                sql.AppendFormat(" indica_refact = '{0}' ", obj.indica_refact); 

            //if (obj.fecha_refacturac > DateTime.MinValue)
            //    sql.AppendFormat(" fecha_refacturac = '{0}' ", obj.fecha_refacturac.ToString("MM/dd/yyyy")); 

            return ConsultaSql(sql.ToString());
        }
    }
}

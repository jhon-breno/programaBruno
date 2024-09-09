using Pangea.Entidades;
using Pangea.Dados.Base;
using Pangea.Entidades.Base;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Pangea.Entidades.DTO;
using Pangea.Util;
using Pangea.Entidades.Enumeracao;
using Entidades.DTO;

namespace Pangea.Dados
{
    public class SalesGeralDAO : BaseDAO
    {

        private string _empresa;

        public SalesGeralDAO(Empresa empresa, string tipoCliente = "BT")
            : base(empresa, tipoCliente)
        {
            this._empresa = empresa.ToString();
        }


        public List<SalesGeral> GetSalesgeralByCliente(string numeroCliente)
        {
            StringBuilder sql = new StringBuilder();
            sql.Append(@"SELECT sg.* FROM cliente c, sales_geral sg WHERE c.numero_cliente = sg.numero_cliente ");
            sql.AppendFormat(@" AND c.numero_cliente = '{0}'", numeroCliente);

            var dt = ConsultaSql(sql.ToString());

            return DataHelper.ConvertDataTableToList<SalesGeral>(dt);
        }
    }
}

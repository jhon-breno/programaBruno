using Pangea.Dados.Base;
using Pangea.Entidades.Enumeracao;
using System.Data;
using System.Text;

namespace Pangea.Dados
{
    public class FurtoDAO : BaseDAO
    {
        public FurtoDAO(Empresa empresa)
            : base(empresa)
        {
        }

        public DataTable ConsultaClienteTOI(int numeroCliente)
        {
            //TESTE
            //string sql = String.Format("SELECT * FROM cliente WHERE numero_cliente = {0}", numeroCliente);

            //RELEASE
            StringBuilder sql = new StringBuilder();
            sql.AppendFormat("SELECT * FROM cliente WHERE numero_cliente = {0} AND ind_ro = 'S'", numeroCliente);

            var dt = ConsultaSql(sql.ToString());
            if (dt.Rows.Count > 0)
                //return DataHelper.ConvertDataTableToEntity<ClienteDTO>(dt);
                return dt;
            return null;
        }

        public bool ConsultaPossuiTOI(int numeroCliente)
        {
            //TESTE
            //string sql = String.Format("SELECT numero_cliente, ind_ro FROM cliente WHERE numero_cliente = {0}", numeroCliente);

            //RELEASE
            StringBuilder sql = new StringBuilder();
            sql.AppendFormat("SELECT numero_cliente, ind_ro FROM cliente WHERE numero_cliente = {0} AND ind_ro = 'S'", numeroCliente);

            var dt = ConsultaSql(sql.ToString());

            return (dt.Rows.Count > 0);
        }
    }
}

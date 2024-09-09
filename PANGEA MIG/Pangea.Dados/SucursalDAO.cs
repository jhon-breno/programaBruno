using Pangea.Dados.Base;
//using Pangea.Dados.Solucoes;
using Pangea.Entidades;
using Pangea.Entidades.DTO;
using Pangea.Entidades.Enumeracao;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

namespace Pangea.Dados
{
    public class SucursalDAO : BaseDAO
    {
        private string _empresa;

        public SucursalDAO(Empresa empresa, string DataBase = "BT")
            : base(empresa,DataBase)
        {
            if (empresa != null)
                this._empresa = empresa.ToString();
        }

        public DataTable Consultar(SucursalDTO obj, string pCodigoArea)
        {
            if (obj == null)
                return new DataTable();

            if (string.IsNullOrEmpty(this._empresa))
            {
                //TODO: gerar log antes de lançar erro
                throw new ArgumentException("Parâmetro empresa obrigatório para a consulta de sucursal.");
            }

            #region Prepara a consulta básica 

            StringBuilder sql = new StringBuilder("SELECT ");
            

            //SELECT
            if(string.IsNullOrEmpty(pCodigoArea))
                sql.AppendFormat("{0} AS sucursal, S.descripcion AS sucursalDescricao, S.regional, T.descripcion AS regionalDescricao, '' AS areaDescricao, S.carga_limite_trafo ", obj.sucursal);
            else
                sql.Append("A.sucursal, S.descripcion AS sucursalDescricao, S.regional, T.descripcion AS regionalDescricao, A1.descripcion AS areaDescricao, S.carga_limite_trafo ");
            

            //FROM
            sql.Append("from sucur S, tabla T ");

            if(!string.IsNullOrEmpty(pCodigoArea))
                sql.Append(", sucar A, AREA A1 ");


            //CONDIÇÕES
            if (!string.IsNullOrEmpty(pCodigoArea))
            {
                sql.AppendFormat("where A.area = '{0}' ", pCodigoArea);
                sql.Append("AND S.sucursal = A.sucursal AND A1.area = A.area ");
            }
            else
                sql.AppendFormat("where S.sucursal = '{0}' ", obj.sucursal);


            if (!string.IsNullOrEmpty(obj.tipo_sucursal))
                sql.AppendFormat("AND S.tipo_sucursal = '{0}' ", obj.tipo_sucursal);

            if (!string.IsNullOrEmpty(obj.regional))
                sql.AppendFormat("AND S.regional = '{0}' ", obj.regional);

            sql.Append("AND T.nomtabla = 'REGION ' ");
            sql.Append("AND T.sucursal = '0000' ");
            sql.Append("AND T.codigo = S.regional ");
            #endregion

            return ConsultaSql(sql.ToString());
        }

        public IList<TEntidade> dtToListObject<TEntidade>(DataTable dt)
        {
            throw new NotImplementedException();
        }
    }
}

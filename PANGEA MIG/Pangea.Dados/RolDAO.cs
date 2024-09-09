using Pangea.Entidades.DTO;
using Pangea.Dados.Base;
using Pangea.Dados;
using Pangea.Entidades;
using Pangea.Entidades.Enumeracao;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

namespace Pangea.Dados
{
    public class RolDAO : BaseDAO
    {
        private string _empresa;

        public RolDAO(Empresa empresa)
            : base(empresa)
        {
            if (empresa != null)
                this._empresa = empresa.ToString();
        }

        public RolDAO(Empresa empresa,string DataBase)
            : base(empresa,DataBase)
        {
            if (empresa != null)
                this._empresa = empresa.ToString();
        }


        public DataTable Consultar(RolDTO obj)
        {
            if (obj == null)
                return new DataTable();

            if (string.IsNullOrEmpty(this._empresa))
            {
                //TODO: gerar log antes de lançar erro
                throw new ArgumentException("Parâmetro empresa obrigatório para a consulta de rol.");
            }

            #region Prepara a consulta básica 

            StringBuilder sql = new StringBuilder("select r.ROL, a.AREA as codigo_area, r.nombre, a.descripcion as desc_area, sc.sucursal ");
            sql.Append(" FROM area a, rol r, sucar sc ");
            sql.Append("WHERE a.empresa = r.empresa ");
            sql.Append("  AND a.area = r.area ");
            sql.Append("  AND a.area = sc.area ");
            
            if (!string.IsNullOrEmpty(obj.rol))
                sql.AppendFormat("AND r.rol = '{0}' ", obj.rol);

            if (!string.IsNullOrEmpty(obj.codigo_area))
                sql.AppendFormat("AND a.area = '{0}' ", obj.codigo_area);

            if (!string.IsNullOrEmpty(obj.nombre))
                sql.AppendFormat("AND r.nombre = '{0}' ", obj.nombre);

            if (!string.IsNullOrEmpty(obj.desc_area))
                sql.AppendFormat("AND S.descripcion = '{0}' ", obj.desc_area);

            #endregion

            return ConsultaSql(sql.ToString());
        }

        //public  IList<TEntidade> dtToListObject<TEntidade>(DataTable dt)
        //{
        //    throw new NotImplementedException();
        //}
    }
}

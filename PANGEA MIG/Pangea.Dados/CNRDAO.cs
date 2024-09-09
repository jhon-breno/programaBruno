using Pangea.Dados.Base;
using Pangea.Dados.Solucoes;
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
    //TODO: adaptar classe para o objeto CnrDTO
    public class CNRDAO : BaseDAO
    {
        private string _empresa;

        public CNRDAO(Empresa empresa)
            : base(empresa)
        {
            if (Empresa.NaoIdentificada != empresa)
                this._empresa = empresa.ToString();
        }

        public DataTable Consultar(CnrDTO obj)
        {
            if (obj == null)
                return new DataTable();

            if (string.IsNullOrEmpty(this._empresa))
            {
                //TODO: gerar log antes de lançar erro
                throw new ArgumentException("Parâmetro empresa obrigatório para a consulta de religações.");
            }

            #region Prepara a consulta básica 

            StringBuilder sql = new StringBuilder("SELECT * FROM corsore WHERE 1=1 ");

            if(!string.IsNullOrEmpty(obj.numero_cliente))
                sql.AppendFormat("AND numero_cliente = {0} ", obj.numero_cliente);

            #endregion

            return ConsultaSql(sql.ToString());
        }

        //public override IList<TEntidade> dtToListObject<TEntidade>(DataTable dt)
        //{
        //    throw new NotImplementedException();
        //}
    }
}

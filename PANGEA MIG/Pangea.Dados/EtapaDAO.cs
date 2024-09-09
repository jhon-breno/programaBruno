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
    public class EtapaDAO : BaseDAO
    {
        private string _empresa;

        public EtapaDAO(Empresa empresa)
            : base(empresa)
        {
            if (empresa != null)
                this._empresa = empresa.ToString();
        }

        public EtapaDAO(Empresa empresa, string DataBase)
            : base(empresa, DataBase)
        {
            if (empresa != null)
                _empresa = empresa.ToString();
        }

        public DataTable ConsultarEtapasAtivas(EtapaDTO obj)
        {
            if (obj == null)
                return new DataTable();

            if (string.IsNullOrEmpty(this._empresa))
            {
                //TODO: gerar log antes de lançar erro
                throw new ArgumentException("Parâmetro empresa obrigatório para a consulta de etapa.");
            }

            #region Prepara a consulta básica 

            StringBuilder sql = new StringBuilder("select * from etapa_servico S");
            sql.AppendFormat(" where tipo_ordem = '{0}' ", obj.tipo_ordem);
            sql.AppendFormat(" and tipo_servico = '{0}' ", obj.tipo_servico);
            sql.Append(" and ( data_desativacao >= current or data_desativacao is null) ");
            
            //if (!string.IsNullOrEmpty(obj.tipo_ordem))
            //    sql.AppendFormat("AND S.tipo_ordem = '{0}' ", obj.tipo_ordem);

            //if (!string.IsNullOrEmpty(obj.tipo_servico))
            //    sql.AppendFormat("AND S.tipo_servico = '{0}' ", obj.tipo_servico);

            if (!string.IsNullOrEmpty(obj.cod_etapa))
                sql.AppendFormat("AND S.cod_etapa = '{0}' ", obj.cod_etapa);

            if (!string.IsNullOrEmpty(obj.cod_formulario))
                sql.AppendFormat("AND S.cod_formulario = '{0}' ", obj.cod_formulario);

            if (!string.IsNullOrEmpty(obj.ind_deafea))
                sql.AppendFormat("AND S.ind_deafea = '{0}' ", obj.ind_deafea);

            if (obj.sequencia_etapa.HasValue)
                sql.AppendFormat("AND S.sequencia_etapa = {0} ", obj.sequencia_etapa);

            #endregion

            return ConsultaSql(sql.ToString());
        }

        public IList<TEntidade> dtToListObject<TEntidade>(DataTable dt)
        {
            throw new NotImplementedException();
        }
    }
}

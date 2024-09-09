using Entidades;
using Pangea.Dados.Base;
using Pangea.Entidades.Enumeracao;
using Pangea.Util;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pangea.Dados
{
    public class RetornoServicoDAO : BaseDAO
    {
        private string _empresa;

        public RetornoServicoDAO(Empresa empresa)
            : base(empresa)
        {
            _empresa = empresa.ToString();
        }

        public RetornoServicoDAO(Empresa empresa, string DataBase)
            : base(empresa, DataBase)
        {
            _empresa = empresa.ToString();
        }

        public RetornoServico ObtemRetornoServico(string tipoOrdem, string tipo_servico, string etapa, string codigo_retorno)
        {
            string sql = string.Format(@"SELECT ind_comunica                                           
                                         FROM   retorno_servico
                                         WHERE  tipo_ordem = '{0}'
                                         AND    tipo_servico = '{1}'
                                         AND    etapa = '{2}'
                                         AND    codigo_retorno = '{3}'", tipoOrdem, tipo_servico, etapa, codigo_retorno);

            var dt = ConsultaSql(sql.ToString());

            if (dt.Rows.Count > 0)
                return DataHelper.ConvertDataTableToEntity<RetornoServico>(dt);
            else
                return null;
        }

    }
}

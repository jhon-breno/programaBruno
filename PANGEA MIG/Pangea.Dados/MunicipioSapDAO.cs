using Pangea.Dados.Base;
using Pangea.Entidades;
using Pangea.Entidades.Enumeracao;
using System.Collections.Generic;
using System.Data;

namespace Pangea.Dados
{
    public class MunicipioSapDAO : BaseDAO
    {
        public MunicipioSapDAO(Empresa empresa)
            : base(empresa)
        {
        }

        public Dictionary<string, string> RetornaSiglaEMunicipioSap()
        {
            Dictionary<string, string> dicionario = new Dictionary<string, string>();

            string sql = "select sigla, cod_municipio from municipio_sap";

            var dt = ConsultaSql(sql);

            foreach (DataRow dr in dt.Rows)
            {
                dicionario.Add(dr[0].ToString(), dr[1].ToString());
            }

            return dicionario;
        }
    }
}

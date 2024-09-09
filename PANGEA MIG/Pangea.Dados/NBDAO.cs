using Pangea.Entidades;
using Pangea.Dados.Base;
using Pangea.Entidades.Base;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Pangea.Util;
using System.Web.Script.Serialization;
using Pangea.Entidades.DTO;
using Pangea.Entidades.Enumeracao;

namespace Pangea.Dados
{
    public class NBDAO : BaseDAO
    {
        private Empresa empresa;
        public NBDAO(Empresa empresa)
            : base(empresa)
        {
            this.empresa = empresa;
        }

        public NBDTO RetornaDadosNB(String paramNumeroNb)
        {

            String sql = String.Format(@"SELECT numero_nb,dta_ativacao,dta_desativacao 
                                           FROM nb_suas 
                                          WHERE numero_nb = '{0}'
                                            AND ativo = 'S'", paramNumeroNb);

            DataTable dtResultado = ConsultaSql(sql);


            if (dtResultado.Rows.Count > 0)
            {
                return gerarEntidadeNB(dtResultado);
            }
            else
                return null;
        }

        public NBDTO gerarEntidadeNB(DataTable resultDt)
        {

            NBDTO entity = new NBDTO();
            if (resultDt.Rows.Count > 0)
            {
                entity.NumeroNB = TratarString(resultDt, resultDt.Rows[0], "numero_nb");
                entity.dta_ativacao = TratarString(resultDt, resultDt.Rows[0], "dta_ativacao");
                entity.dta_desativacao = TratarString(resultDt, resultDt.Rows[0], "dta_desativacao");
            }
            return entity;
        }


        public Boolean NBDuplicado(long paramNumeroNb)
        {
            String sql = String.Format(@"SELECT numero_cliente 
                                           FROM cliente_doc_bxr 
                                          WHERE numero_nb = {0} 
                                            AND estado in ('V', 'P', 'F')", paramNumeroNb);
            DataTable dtResultado = ConsultaSql(sql);

            if (dtResultado.Rows.Count > 0)
                return true;
            else
                return false;
        }
    }
}

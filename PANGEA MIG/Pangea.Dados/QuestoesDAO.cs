using Pangea.Entidades.DTO;
using Pangea.Dados.Base;
using Pangea.Entidades;
using Pangea.Entidades.Enumeracao;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

namespace Pangea.Dados
{
    public class QuestoesDAO : BaseDAO
    {
        public QuestoesDAO(Empresa empresa)
            : base(empresa)
        {
        }

        public bool Inserir(string pergunta, string resposta, DBProviderInformix conn)
        {
         
            String sql = String.Format(@"insert into questoes_sap 
                                        (pergunta,
                                         resposta,
                                        data_ingresso) values 
                                        ('{0}',
                                         '{1}',
                                        current)", pergunta, resposta);

            return ExecutarSql(sql.ToString(), conn);
        }
    }
}

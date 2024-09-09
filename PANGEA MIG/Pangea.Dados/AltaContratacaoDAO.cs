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
    public abstract class AltaContratacaoDAO : BaseDAO    
    {
        private string _empresa;

        public AltaContratacaoDAO(Empresa empresa)
            : base(empresa)
        {
        }

        public AltaContratacaoDAO(Empresa empresa, string DataBase)
            : base(empresa, DataBase)
        {
        }

        public abstract string GetConsultaBase();

        public IList<TEntidade> dtToListObject<TEntidade>(DataTable dt)
        {
            throw new NotImplementedException();
        }
    }
}

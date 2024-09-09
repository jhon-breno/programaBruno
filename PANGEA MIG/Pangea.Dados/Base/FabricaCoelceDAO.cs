using Pangea.Dados.Agenda;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Pangea.Dados.Base
{
    public class FabricaCoelceDAO : FabricaDAO
    {
        private static AutenticacaoDAO autenticacaoCoelceDAO = null;
        public override AutenticacaoDAO getInstanceAutenticacaoDAO()
        {
            if (autenticacaoCoelceDAO == null)
            {
                autenticacaoCoelceDAO = new AutenticacaoCoelceDAO();
            }
            return autenticacaoCoelceDAO;
        }



        public override AgendaDAO getInstanceAgendaDAO()
        {
            throw new NotImplementedException();
        }
    }
}

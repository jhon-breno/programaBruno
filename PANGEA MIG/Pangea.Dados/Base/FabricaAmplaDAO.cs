using Pangea.Dados.Agenda;
using Pangea.Entidades;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Pangea.Dados.Base
{
    public class FabricaAmplaDAO : FabricaDAO
    {
        private static AutenticacaoDAO autenticacaoAmplaDAO = null;
        public override AutenticacaoDAO getInstanceAutenticacaoDAO()
        {
            if (autenticacaoAmplaDAO == null)
            {
                autenticacaoAmplaDAO = new AutenticacaoAmplaDAO();
            }
            return autenticacaoAmplaDAO;
        }


        public override AgendaDAO getInstanceAgendaDAO()
        {
            throw new NotImplementedException();
        }
    }
}

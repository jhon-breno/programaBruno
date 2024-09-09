using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Pangea.Dados.Agenda;
using Pangea.Entidades.Enumeracao;

namespace Pangea.Dados.Agenda
{
    public class AgendaCoelceDAO : AgendaDAO
    {
        public AgendaCoelceDAO()
            : base(Empresa.CE)
        {
        }
    }
}

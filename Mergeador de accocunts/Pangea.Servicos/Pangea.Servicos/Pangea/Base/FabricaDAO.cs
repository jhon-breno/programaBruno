using Pangea.Entidades;
using Pangea.Entidades.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Pangea.Dados.Base
{
    public abstract class FabricaDAO
    {
        private static FabricaAmplaDAO fabricaAmplaDAO = null;
        private static FabricaCoelceDAO fabricaCoelceDAO = null;

        public static FabricaDAO getInstanceFabricaDAO(string empresa)
        {
            FabricaDAO fabricaDAO = null;
            switch (int.Parse(empresa))
            {
                case (int)Empresa.Ampla:
                    if (fabricaAmplaDAO == null)
                    {
                        fabricaAmplaDAO = new FabricaAmplaDAO();
                    }

                    fabricaDAO = fabricaAmplaDAO;
                    break;
                case (int)Empresa.Coelce:
                    if (fabricaCoelceDAO == null)
                    {
                        fabricaCoelceDAO = new FabricaCoelceDAO();
                    }

                    fabricaDAO = fabricaCoelceDAO;
                    break;
                default:
                    fabricaDAO = null;
                    break;
            }

            return fabricaDAO;
        }

        public abstract AutenticacaoDAO getInstanceAutenticacaoDAO();
        //public abstract AgendaDAO getInstanceAgendaDAO();
    }
}

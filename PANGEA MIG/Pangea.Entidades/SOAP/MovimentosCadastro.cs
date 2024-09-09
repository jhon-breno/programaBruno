using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Pangea.Entidades.SOAP
{
    public class MovimentosCadastro : Movimentos
    {
        public List<ClienteSolucoes> retornos { get; set; }

        public MovimentosCadastro()
        {
            this.retornos = new List<ClienteSolucoes>();
        }

        public MovimentosCadastro(int total, int sucesso, int erros, string msg, List<ClienteSolucoes> l)
        {
            totalSolicitacoes = total;
            totalSucessos = sucesso;
            totalErros = erros;
            Status = msg;
            retornos = l;
        }

        public MovimentosCadastro(MovimentosSolucoes mov)
        {
            this.totalSolicitacoes = mov.totalSolicitacoes;
            this.totalSucessos = mov.totalSucessos;
            this.totalErros = mov.totalErros;
            this.Status = mov.Status;

            if (mov.retornos != null)
            {
                if (mov.retornos.GetType() == new List<ClienteSolucoes>().GetType())
                {
                    this.retornos = (List<ClienteSolucoes>)mov.retornos;
                }
                else if (mov.retornos.GetType() == new ClienteSolucoes().GetType())
                {
                    ClienteSolucoes c = (ClienteSolucoes)mov.retornos;
                    this.retornos = new List<ClienteSolucoes>();
                    this.retornos.Add(c);
                }
                else
                {
                    this.Status = getMsgObjetoIncorreto();
                }
            }

        }
    }
}

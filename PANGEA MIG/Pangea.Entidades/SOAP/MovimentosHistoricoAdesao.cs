using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Pangea.Entidades.SOAP
{
    public class MovimentosHistoricoAdesao : Movimentos
    {
        public List<ProdutoCliente> retornos { get; set; }

        public MovimentosHistoricoAdesao()
        {
            this.retornos = new List<ProdutoCliente>();
        }

        public MovimentosHistoricoAdesao(MovimentosSolucoes mov)
        {
            this.totalSolicitacoes = mov.totalSolicitacoes;
            this.totalSucessos = mov.totalSucessos;
            this.totalErros = mov.totalErros;
            this.Status = mov.Status;

            if (mov.retornos != null)
            {
                if (mov.retornos.GetType() == new List<ProdutoCliente>().GetType())
                {
                    this.retornos = (List<ProdutoCliente>)mov.retornos;
                }
                else
                {
                    this.Status = getMsgObjetoIncorreto();
                }
                
            }
        }

        public MovimentosHistoricoAdesao(int total, int sucesso, int erros, string msg, List<ProdutoCliente> l)
        {
            totalSolicitacoes = total;
            totalSucessos = sucesso;
            totalErros = erros;
            Status = msg;
            retornos = l;
        }
    }
}

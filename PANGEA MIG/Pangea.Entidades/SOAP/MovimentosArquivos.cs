using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Pangea.Entidades.SOAP
{
    public class MovimentosArquivos : Movimentos
    {
        public List<MovimentosProdutoCliente> retornos { get; set; }

        public MovimentosArquivos()
        {
            this.retornos = new List<MovimentosProdutoCliente>();
        }

        public MovimentosArquivos(int total, int sucesso, int erros, string msg, List<MovimentosProdutoCliente> l)
        {
            totalSolicitacoes = total;
            totalSucessos = sucesso;
            totalErros = erros;
            Status = msg;
            retornos = l;
        }

        public MovimentosArquivos(MovimentosSolucoes mov)
        {
            this.totalSolicitacoes = mov.totalSolicitacoes;
            this.totalSucessos = mov.totalSucessos;
            this.totalErros = mov.totalErros;
            this.Status = mov.Status;

            if (mov.retornos != null)
            {
                if (mov.retornos.GetType() == new List<MovimentosProdutoCliente>().GetType())
                {
                    this.retornos = (List<MovimentosProdutoCliente>)mov.retornos;
                }
                else
                {
                    this.Status = getMsgObjetoIncorreto();
                }
            }

        }
    }
}

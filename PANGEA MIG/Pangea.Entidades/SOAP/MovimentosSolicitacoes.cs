using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Pangea.Entidades.SOAP
{
    public class MovimentosSolicitacoes : Movimentos
    {
        public List<RetornoSolicitacaoTipoC> retornos { get; set; }
        
        public MovimentosSolicitacoes()
        {
            this.retornos = new List<RetornoSolicitacaoTipoC>();
        }

        public MovimentosSolicitacoes(int total, int sucesso, int erros, string msg, List<RetornoSolicitacaoTipoC> l)
        {
            totalSolicitacoes = total;
            totalSucessos = sucesso;
            totalErros = erros;
            Status = msg;
            retornos = l;
        }

        public MovimentosSolicitacoes(MovimentosSolucoes mov)
        {
            this.totalSolicitacoes = mov.totalSolicitacoes;
            this.totalSucessos = mov.totalSucessos;
            this.totalErros = mov.totalErros;
            this.Status = mov.Status;

            if (mov.retornos != null)
            {
                if (mov.retornos.GetType() == new List<RetornoSolicitacaoTipoC>().GetType())
                {
                    this.retornos = (List<RetornoSolicitacaoTipoC>)mov.retornos;
                }
                else
                {
                    this.Status = getMsgObjetoIncorreto();
                }
            }

        }
    }
}

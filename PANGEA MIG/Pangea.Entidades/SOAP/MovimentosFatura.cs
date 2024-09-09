using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Pangea.Entidades.SOAP
{
    public class MovimentosFatura : Movimentos
    {
        public List<RetornoSolicitacaoTipoCFatura> retornos { get; set; }

        public MovimentosFatura()
        {
            retornos = new List<RetornoSolicitacaoTipoCFatura>();
        }

        public MovimentosFatura(int total, int sucesso, int erros, string msg, List<RetornoSolicitacaoTipoCFatura> l)
        {
            totalSolicitacoes = total;
            totalSucessos = sucesso;
            totalErros = erros;
            Status = msg;
            retornos = l;
        }

        public MovimentosFatura(MovimentosSolucoes mov)
        {
            this.totalSolicitacoes = mov.totalSolicitacoes;
            this.totalSucessos = mov.totalSucessos;
            this.totalErros = mov.totalErros;
            this.Status = mov.Status;

            if (mov.retornos != null)
            {
                if (mov.retornos.GetType() == new List<RetornoSolicitacaoTipoCFatura>().GetType())
                {
                    this.retornos = (List<RetornoSolicitacaoTipoCFatura>)mov.retornos;
                }
                else
                {
                    this.Status = getMsgObjetoIncorreto();
                }
            }
        }
    }
}

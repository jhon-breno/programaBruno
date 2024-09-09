using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Pangea.Entidades.SOAP
{
    public class MovimentosParcelasVInf : Movimentos
    {
        public List<CobrancaValorInformadoCliente> retornos { get; set; }

        MovimentosParcelasVInf()
        {
            this.retornos = new List<CobrancaValorInformadoCliente>();
        }

        public MovimentosParcelasVInf(int total, int sucesso, int erros, string msg, List<CobrancaValorInformadoCliente> l)
        {
            totalSolicitacoes = total;
            totalSucessos = sucesso;
            totalErros = erros;
            Status = msg;
            retornos = l;
        }

        public MovimentosParcelasVInf(MovimentosSolucoes mov)
        {
            this.totalSolicitacoes = mov.totalSolicitacoes;
            this.totalSucessos = mov.totalSucessos;
            this.totalErros = mov.totalErros;
            this.Status = mov.Status;

            if (mov.retornos != null)
            {
                if (mov.retornos.GetType() == new List<CobrancaValorInformadoCliente>().GetType())
                {
                    this.retornos = (List<CobrancaValorInformadoCliente>)mov.retornos;
                }
                else
                {
                    this.Status = getMsgObjetoIncorreto();
                }
            }

        }
    }
}

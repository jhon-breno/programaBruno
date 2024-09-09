using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Pangea.Entidades
{
    public class RetornoSolicitacaoTipoC
    {
        public int numeroCliente { get; set; }
        public int dvNumeroCliente { get; set; }
        public string codOcorr { get; set; }
        public string dataOcorrencia { get; set; }
        public double valor { get; set; }
        public int numParcelas { get; set; }
        public string referencia { get; set; }
        public string dataVencimento { get; set; }
        public string dataPagamento { get; set; }
        public int loteFaturamento { get; set; }
        public int numeroClienteEmpPar { get; set; }
        public string codProduto { get; set; }
        public string empresaParc { get; set; }
        public string dataBaixaPagamento { get; set; }
        public string codCanalVenda { get; set; }
        public string tipoRetorno { get; set; } //Tipo sol_ocorr ou tipo mov
        public string descricaoRetorno { get; set; }

        public RetornoSolicitacaoTipoC()
        {
            numeroCliente = 0;
            dvNumeroCliente = 0;
            codOcorr = "00";
            dataOcorrencia = "01/01/1900";
            valor = 0.0;
            referencia = "";
            dataVencimento = "";
            dataPagamento = "";
            loteFaturamento = 0;
            numeroClienteEmpPar = 0;
            codProduto = "00";
            empresaParc = "00";
            dataBaixaPagamento = "";
            codCanalVenda = "";
            tipoRetorno = "";
            descricaoRetorno = "";
            numParcelas = 0;

        }

        protected void defineValores(SolicitacaoTipoD sol)
        {
            this.numeroCliente = sol.numeroCliente;
            this.dvNumeroCliente = sol.dvNumeroCliente;
            this.codOcorr = sol.codOcorr;
            this.dataOcorrencia = sol.dataOcorrencia;
            if (sol.codOcorr == "60" || sol.codOcorr == "54" || sol.codOcorr == "58" || sol.codOcorr == "61")
            {
                this.valor = sol.valorParcela;
                this.numParcelas = sol.numParcelas;
            }
            else
            {
                this.valor = 0.0;
                this.numParcelas = 0;
            }
            this.numeroClienteEmpPar = sol.numeroClienteEmpPar;
            this.codProduto = sol.codProduto;
            this.empresaParc = sol.empresaParc;
            this.codCanalVenda = sol.codCanalVenda;

            referencia = "";
            dataVencimento = "";
            dataPagamento = "";
            dataBaixaPagamento = "";
            tipoRetorno = "";
            descricaoRetorno = "";
            loteFaturamento = 0;

        }

        public RetornoSolicitacaoTipoC(SolicitacaoTipoD sol)
        {
            defineValores(sol);
        }

        public RetornoSolicitacaoTipoC(SolicitacaoTipoD sol, ClienteSolucoes c)
        {

            defineValores(sol);

            loteFaturamento = c.loteFaturamento;

        }
        public RetornoSolicitacaoTipoC(ClienteSolucoes c)
        {

            this.numeroCliente = c.numeroCliente;
            this.dvNumeroCliente = c.dvNumeroCliente;

            loteFaturamento = c.loteFaturamento;

        }


    }
}

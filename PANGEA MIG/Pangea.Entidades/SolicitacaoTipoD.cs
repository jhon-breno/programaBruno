using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Pangea.Entidades
{
    public class SolicitacaoTipoD
    {
        public int numeroCliente { get; set; }
        public int dvNumeroCliente { get; set; }
        public string codOcorr { get; set; }
        public string dataOcorrencia { get; set; }
        public double valorParcela { get; set; }
        public int numParcelas { get; set; }
        public int numeroClienteEmpPar { get; set; }
        public string codProduto { get; set; }
        public string empresaParc { get; set; }
        public string codCanalVenda { get; set; }

        public SolicitacaoTipoD()
        {
            numeroCliente = 0;
            dvNumeroCliente = 0;
            codOcorr = "00";
            dataOcorrencia = "01/01/1900";
            valorParcela = 0.0;
            numParcelas = 0;
            numeroClienteEmpPar = 0;
            codProduto = "00";
            empresaParc = "00";
            codCanalVenda = "";

        }

    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Pangea.Entidades
{
    public class MovimentosProdutoCliente
    {
        public string empresaParc { get; set; }
        public string codProduto { get; set; }
        public int numeroCliente { get; set; }
        public int dvNumeroCliente { get; set; }
        public string codOcorr { get; set; }
        public string dataOcorrencia { get; set; }
        public double valorParcela { get; set; }
        public int numParcelas { get; set; }
        public string dataRecebimento { get; set; }
        public int numArquivo { get; set; }
        public string descricaoMovimento { get; set; }


        public MovimentosProdutoCliente()
        {
            this.empresaParc = "";
            this.codProduto = "";
            this.numeroCliente = 0;
            this.dvNumeroCliente = 0;
            this.codOcorr = "";
            this.dataOcorrencia = "";
            this.valorParcela = 0;
            this.numParcelas = 0;
            this.dataRecebimento = "";
            this.numArquivo = 0;
            this.descricaoMovimento = "";
        }
    }
}

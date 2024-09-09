using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Pangea.Entidades
{
    public class ProdutoCliente
    {
        //public Produto produto { get; set; }
        public string empresaParc { get; set; }
        public string codProduto { get; set; }
        public int numeroCliente { get; set; }
        public int dvNumeroCliente { get; set; }
        public string dataAdesao { get; set; }
        public string dataCancelamento { get; set; }
        public string estado { get; set; }
        public string dataProcessamento { get; set; } //Data em que o registro foi incluido na solucoes_cliente
        public string indicaFaturaCarona { get; set; } //Indica se a adesao foi por fatura carona
        public string dataPagamento { get; set; } //Data de pagamento de fatura carona 
        public string dataCriacaoFaturaCarona { get; set; } //Data em que foi gerada a fatura carona 
        public string dataEnvioCadastro { get; set; } //Data em que o cadastro do cliente foi enviado em arquivo sol_cad
        public string dataAnulacao { get; set; }
        public string dataEnvioCertificado { get; set; }
        public int numeroSorteio { get; set; }
        public string codCanalVenda { get; set; }
        public string usuarioAdesao { get; set; }
        public int numeroClienteEmpresaParceira { get; set; } //Numero do cliente no sistema da empresa parceira 
        public string usuarioCancelamento { get; set; }
        public string obsCancelamento { get; set; }


        public ProdutoCliente()
        {
            //produto = new Produto();
            empresaParc = "";
            codProduto = "";
            numeroCliente = 0;
            dvNumeroCliente = 0;
            dataAdesao = "";
            dataCancelamento = "";
            estado = "";
            dataProcessamento = "";
            indicaFaturaCarona = "";
            dataPagamento = "";
            dataCriacaoFaturaCarona = "";
            dataEnvioCadastro = "";
            dataAnulacao = "";
            dataEnvioCertificado = "";
            numeroSorteio = 0;
            codCanalVenda = "";
            usuarioAdesao = "";
            numeroClienteEmpresaParceira = 0;
            usuarioCancelamento = "";
            obsCancelamento = "";
        }

    }
}

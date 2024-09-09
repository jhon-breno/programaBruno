using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Pangea.Entidades
{
    public class Produto
    {
        public EmpresaParceira empresa { get; set; }
        public string codProduto { get; set; }
        public string descricaoProduto { get; set; }
        public string tipoCobranca { get; set; }
        public double valorProduto { get; set; }
        public double valorParcela { get; set; }
        public int diasCancelRecente { get; set; }
        public int diasCancelAtraso { get; set; }
        public string codigoFacilidade { get; set; }
        public string encargoCobranca { get; set; }
        public string encargoCancelamento { get; set; }
        public string encargoIndenizacao { get; set; }

        public Produto()
        {
            empresa = new EmpresaParceira();
            codProduto = "00";
            descricaoProduto = "";
            tipoCobranca = "";
            valorProduto = 0.0;
            valorParcela = 0.0;
            diasCancelRecente = 0;
            diasCancelAtraso = 0;
            codigoFacilidade = "";
            encargoCobranca = "";
            encargoCancelamento = "";
            encargoIndenizacao = "";

        }

        public Produto(string empresaParc, string codProduto)
        {
            empresa = new EmpresaParceira(empresaParc);
            this.codProduto = codProduto;
        }

    }
}

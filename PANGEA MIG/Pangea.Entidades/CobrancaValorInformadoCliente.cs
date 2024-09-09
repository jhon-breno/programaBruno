using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Pangea.Entidades
{
    public class CobrancaValorInformadoCliente
    {
        public string empresaParc { get; set; }
        public string codProduto { get; set; }
        public int numeroCliente { get; set; }
        public string codOcorr { get; set; }
        public string codEncargo { get; set; }
        public int quantParcelas { get; set; }
        public double valorParcela { get; set; }
        public string dataIngresso { get; set; }
        public int parcelasFat { get; set; }
        public string estado { get; set; }
        public string dataTermino { get; set; }
        
        public CobrancaValorInformadoCliente()
        {
            empresaParc = "";
            codProduto = "";
            numeroCliente = 0;
            codOcorr = "";
            codEncargo = "";
            quantParcelas = 0;
            valorParcela = 0;
            dataIngresso = "";
            parcelasFat = 0;
            estado = "";
            dataTermino = "";
        }

        public CobrancaValorInformadoCliente(CobrancaValorInformado c)
        {
            this.empresaParc = c.empresaParc;
            this.codProduto = c.codProduto;
            this.numeroCliente =  c.numeroCliente;
            this.codOcorr =  c.codOcorr;
            this.codEncargo =  c.codEncargo;
            this.quantParcelas =  c.quantParcelas;
            this.valorParcela =  c.valorParcela;
            this.dataIngresso =  c.dataIngresso;
            this.parcelasFat =  c.parcelasFat;
            this.estado =  c.estado;
            this.dataTermino = c.dataTermino;
        }

    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Pangea.Entidades
{
    public class ClienteSolucoes
    {
        public int numeroCliente { get; set; }
        public int dvNumeroCliente { get; set; }
        public string nome { get; set; }
        public string endereco { get; set; }
        public string bairro { get; set; }
        public string municipio { get; set; }
        //public string uf { get; set; }
        public string cep { get; set; }
        public string ddd { get; set; }
        public string telefone { get; set; }
        public string tipoDoc1 { get; set; }
        public string documento1 { get; set; }
        public string dvDoc1 { get; set; }
        public string orgaoExpDoc1 { get; set; }
        public string tipoDoc2 { get; set; }
        public string documento2 { get; set; }
        public string dvDoc2 { get; set; }
        public double mediaConsumo { get; set; }
        public string dataNascimento { get; set; }
        public int loteFaturamento { get; set; }
        public int zona { get; set; }
        public string dataUltimoFaturamento { get; set; }
        public string tipoVencimento { get; set; }
        public string codigoLocalidade { get; set; }
        public string estadoCliente { get; set; }
        public string EstadoFaturamento { get; set; }
        public string estadoFornecimento { get; set; }
        public string Classe { get; set; }
        public string descTipoDoc1 { get; set; }
        public string descTipoDoc2 { get; set; }
        public string correlativoRota { get; set; }
        public string dvCorrelativoRota { get; set; }
        public string rotaLeitura { get; set; }

        public ClienteSolucoes()
        {
            this.numeroCliente = 0;
            this.dvNumeroCliente = 0;
            this.nome = "";
            this.endereco = "";
            this.bairro = "";
            this.municipio = "";
            //this.uf = "";
            this.cep = "";
            this.ddd = "";
            this.telefone = "";
            this.tipoDoc1 = "";
            this.documento1 = "";
            this.dvDoc1 = "";
            this.orgaoExpDoc1 = "";
            this.tipoDoc2 = "";
            this.documento2 = "";
            this.dvDoc2 = "";
            this.mediaConsumo = 0;
            this.dataNascimento = "";
            this.loteFaturamento = 0;
            this.zona = 0;
            this.dataUltimoFaturamento = "";
            this.tipoVencimento = "";
            this.codigoLocalidade = "";
            this.estadoCliente = "";
            this.EstadoFaturamento = "";
            this.estadoFornecimento = "";
            this.Classe = "";

            this.descTipoDoc1 = "";
            this.descTipoDoc2 = "";
            this.correlativoRota = "";
            this.dvCorrelativoRota = "";
            this.rotaLeitura = "";
        }
    }
}

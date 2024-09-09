using System;
using System.Collections.Generic;

namespace BotaoEmpresa.entidades
{
    [Serializable]
    public class HistoricoOrdem
    {

        public string NumeroCliente { get; set; }
        public string NomeCliente { get; set; }
        public string StatusConexao { get; set; }
        public string NumeroOrdem { get; set; }
        public string TipoOrdem { get; set; }
        public string DescricaoTipoServico { get; set; }
        public DateTime DataIngresso { get; set; }
        public string Estado { get; set; }
        public string Prazo { get; set; }
        public string Etapa { get; set; }
        public HistoricoOrdemDetalhe OrdemDetalhe { get; set; }
        public List<HistoricoOrdemVisitas> HistoricoVisitas { get; set; }
    }
}
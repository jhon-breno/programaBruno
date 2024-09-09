using System;

namespace BotaoEmpresa.entidades
{
    [Serializable]
    public class HistoricoOrdemDetalhe
    {
        public string Tipo { get; set; }
        public string SucursalOrigem { get; set; }
        public string SucursalDestino { get; set; }
        public string Etapa { get; set; }
        public string Estado { get; set; }
        public string Atendimento { get; set; }
        public string Servico { get; set; }
        public string AreaOrigem { get; set; }
        public string AreaDestinoo { get; set; }
        public string DataIngresso { get; set; }
        public string Usuario { get; set; }
        public string FinalzadaPorData { get; set; }
        public string ObsDoAtendimento { get; set; }
        public string ObsDoExecutante { get; set; }
        public string NumeroCliente { get; set; }
        public string DataEstado { get; set; }
        public string DataEtapa { get; set; }
        public string DataFinalizacao { get; set; }
        public string Nome { get; set; }
        public string Endereco { get; set; }
    }
}
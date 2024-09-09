using System;

namespace BotaoEmpresa.entidades
{
    [Serializable]
    public class HistoricoOrdemVisitas
    {
        public string NumeroVisita { get; set; }
        public string Etapa { get; set; }
        public string DataVisita { get; set; }
        public string AreaExecutante { get; set; }
        public string ResponsavelDespacho { get; set; }
        public string DataExec { get; set; }
        public string HoraExec { get; set; }
        public string DescricaoRetorno { get; set; }
        public string DataRetorno { get; set; }
        public string OrdemRelacionada { get; set; }
    }
}
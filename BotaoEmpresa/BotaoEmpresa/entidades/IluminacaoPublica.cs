namespace BotaoEmpresa.entidades
{
    public class IluminacaoPublica
    {
        public string DataVigencia { get; set; }
        public string KwhMin { get; set; }
        public string KwhMax { get; set; }
        public string Tarifa { get; set; }
        public string Cidade { get; set; }
        public string Grupo { get; set; }
        public string Tipo { get; set; }
        public string TipoTarifa { get; set; }
        public string CodTipo { get; set; }
        public string DescricaoTipo { get; set; }
        public string DescricaoGrupo { get; set; }
        public string ValorOrigem { get; set; }
    }
}
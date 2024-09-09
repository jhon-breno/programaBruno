using System.ComponentModel;

namespace Pangea.Entidades.Enumeracao
{
    public enum OrdemServicoEstado
    {

        [Description("00")]
        INGRESSADA = 0,

        [Description("01")]
        APTA_A_DESIGNAR = 1,

        [Description("02")]
        EM_EXECUCAO = 2,

        [Description("03")]
        SERVICO_EXECUTADO = 3,

        [Description("04")]
        FINALIZADA = 4,

        [Description("05")]
        EM_DEFEITO_TECNICO = 5,

        [Description("06")]
        CLIENTE_IMPLANTADO = 6,

        [Description("07")]
        SUSPENSA = 7,

        [Description("08")]
        ENVIADA = 8, 

        [Description("09")]
        CANCELADA = 9,

        [Description("10")]
        EM_INTERGRACAO_EORDER  = 10,

        [Description("11")]
        ENVIADO_EORDER = 11,

        [Description("99")]
        NAO_FINALIZADA = 99
    }
}
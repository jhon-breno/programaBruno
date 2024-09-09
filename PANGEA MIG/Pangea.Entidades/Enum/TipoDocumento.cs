using System.ComponentModel;

namespace Pangea.Entidades.Enumeracao
{
    /// <summary>
    /// Correspondente ao campo TIPO_IDENT da tabela CLIENTE
    /// </summary>
    public enum TipoDocumento
    {
        /// <summary>
        /// Tipo diferente de CPF e CNPJ.
        /// </summary>
        [Description("Não Identificado")]
        NaoIdentificado = -1,

        /// <summary>
        /// CNPJ
        /// </summary>
        [Description("002CNPJ")]
        CNPJ = 1,

        /// <summary>
        /// CPF
        /// </summary>
        [Description("003CPF")]
        CPFGA = 2,

        /// <summary>
        /// CPF
        /// </summary>
        [Description("005CPF")]
        CPFGB = 3,

        /// <summary>
        /// CPF
        /// </summary>
        [Description("006CPF")]
        CPFGB2 = 4,
    }
}

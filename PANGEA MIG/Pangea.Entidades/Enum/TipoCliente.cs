using System.ComponentModel;

namespace Pangea.Entidades.Enumeracao
{
    public enum TipoCliente
    {
        /// <summary>
        /// Nao informada o tipo cliente.
        /// </summary>
        [Description("Não Identificado")]
        NaoIdentificado = -1,

        /// <summary>
        /// Grupo A.
        /// </summary>
        [Description("AT")]
        GA = 1,

        /// <summary>
        /// Grupo B.
        /// </summary>
        [Description("BT")]
        GB = 2
    }
}

using System.ComponentModel;


namespace Pangea.Entidades.Enumeracao
{
    public enum Motivo : int 
    {
        /// <summary>
        /// Alteração de Tarifa.
        /// </summary>
        [Description("Alteração de Tarifa")]
        AlteracaoTarifa = 1,

        /// <summary> 
        /// Acréscimo e Decréscimo de Carga.
        /// </summary>
        [Description("Acréscimo e Decréscimo de Carga")]
        AcrescimoDecrescimoCagra = 2,

        /// <summary> 
        /// Acréscimo e Decréscimo de Demanda.
        /// </summary>
        [Description("Acréscimo e Decréscimo de Demanda")]
        AcrescimoDecrescimoDemanda = 3

    }
}

using System.ComponentModel;
using System.Runtime.Serialization;

namespace Pangea.Entidades.Enumeracao
{
    public enum Submotivo
    {

        /// <summary>
        /// Acréscimo de Carga Mono para Bi. 
        /// </summary>
        [Description("Acréscimo de Carga Mono para Bi - ACM")]
        [DataMember]
        AcrescimoDeCargaMonoParaBi = 21,
        
        /// <summary>
        /// Acréscimo de Carga Bi para Tri.
        /// </summary>
        [Description("Acréscimo de Carga Bi para Tri - ACB")]
        [DataMember]
        AcrescimoDeCargaBiParaTri = 22,

        /// <summary>
        /// Acréscimo de Carga Mono para Tri. 
        /// </summary>
        [Description("Acréscimo de Carga Mono para Tri - ACT")]
        [DataMember]
        AcrescimoDeCargaMonoParaTri = 23,

        /// <summary>
        /// Decréscimo de Carga Bi para Mono.
        /// </summary>
        [Description("Decréscimo de Carga Bi para Mono - DCB")]
        [DataMember]
        DecrescimoDeCargaBiParaMono = 24,

        /// <summary>
        /// Decréscimo de Carga Tri para Bi. 
        /// </summary>
        [Description("Decréscimo de Carga Tri para Bi - DCT")]
        [DataMember]
        DecrescimoDeCargaTriParaBi = 25,

        /// <summary>
        /// Decréscimo de Carga Tri para Mono.
        /// </summary>
        [Description("Decréscimo de Carga Tri para Mono - DCM")]
        [DataMember]
        DecrescimoDeCargaTriParaMono = 26,

        /// <summary>
        ///  
        /// </summary>
        [Description("Acréscimo de Carga sem mudar a fase")]
        [DataMember]
        AcrescimoDeCargaSemMudarAFase = 27
    }
}

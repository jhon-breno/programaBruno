using System;
using System.ComponentModel;
using System.Runtime.Serialization;

namespace Pangea.Entidades.Enumeracao
{
    [Serializable]
    [DataContract]
    public enum Empresa
    {
        /// <summary>
        /// Nao Informada a Empresa.
        /// </summary>
        [Description("Não Identificada")]
        [DataMember]
        NaoIdentificada = -1,

        /// <summary>
        /// Código da empresa Coelce.
        /// </summary>
        [Description("Coelce")]
        [DataMember]
        CE = 2003,

        /// <summary>
        /// Código da empresa Ampla.
        /// </summary>
        [Description("Ampla")]
        [DataMember]
        RJ = 2005
    }
}

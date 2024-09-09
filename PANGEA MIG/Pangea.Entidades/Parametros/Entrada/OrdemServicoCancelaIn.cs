using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Pangea.Entidades.Enumeracao;
using System.ComponentModel.DataAnnotations;
using Pangea.Entidades.DTO;
using System.Runtime.Serialization;

namespace Pangea.Entidades.Parametros.Entrada
{
    /// <summary>
    /// 
    /// </summary>
    /// <remarks>Conforme o Acordo de Interface da Integração, haverá mudanças nas propriedades do parâmetro de entrada.</remarks>
    [Serializable]
    [DataContract]
    public class OrdemServicoCancelaIn : ValidationAttribute
    {
        public OrdemServicoCancelaIn(string Empresa, string NumeroOrdemOriginal, string Solicitante)
        {
            this.CodigoEmpresa = Empresa;
            this.NumeroOrdemOriginal = NumeroOrdemOriginal;
            this.Solicitante = Solicitante;
        }

        [Required(ErrorMessage="Obrigatório informar a Empresa.")]
        [DataMember]
        public string CodigoEmpresa { get; set; }

        [Required(ErrorMessage = "Obrigatório informar o número da ordem de serviço a cancelar.")]
        [DataMember]
        public string NumeroOrdemOriginal { get; set; }

        [Required(ErrorMessage = "Obrigatório informar o Solicitante da ordem de serviço.")]
        [DataMember]
        public string Solicitante { get; set; }
    }
}

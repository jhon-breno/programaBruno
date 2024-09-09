using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Pangea.Entidades.Enumeracao;
using System.ComponentModel.DataAnnotations;
using Pangea.Entidades.DTO;
using System.Runtime.Serialization;
using Pangea.Util;

namespace Pangea.Entidades.Parametros.Saida
{
    /// <summary>
    /// 
    /// </summary>
    /// <remarks>Conforme o Acordo de Interface da Integração, haverá mudanças nas propriedades do parâmetro de entrada.</remarks>
    [DataContract]
    public class OrdemServicoGenericaOut
    {
        public OrdemServicoGenericaOut(string mensagem)
        {
            this.NumeroOrdem = string.Empty;
            this.Descricao = mensagem;
        }


        public OrdemServicoGenericaOut(string numeroOrdem, string tipoOrdem, string mensagem)
        {
            this.NumeroOrdem = numeroOrdem;
            this.Descricao = mensagem;
        }


        public OrdemServicoGenericaOut(Enum enumeracao, object[] parametros = null)
        {
            this.CodigoMensagem = enumeracao.ToString();
            this.Descricao = (parametros != null && parametros.Length > 0) ? string.Format(EnumString.GetStringValue(enumeracao), parametros) : EnumString.GetStringValue(enumeracao);
            this.NumeroOrdem = string.Empty;
        }


        public OrdemServicoGenericaOut(string numeroOrdem, string tipoOrdem, Enum enumeracao, object[] parametros = null)
        {
            this.CodigoMensagem = enumeracao.ToString();
            this.Descricao = (parametros != null && parametros.Length > 0) ? string.Format(EnumString.GetStringValue(enumeracao), parametros) : EnumString.GetStringValue(enumeracao);
            this.NumeroOrdem = numeroOrdem;
        }

        public override string ToString()
        {
            return base.ToString();
        }

        [DataMember]
        public string CodigoMensagem { get; set; }

        [DataMember]
        public string Descricao { get; set; }

        private object objeto { get; set; }

        [DataMember]
        public string NumeroOrdem { get; set; }
    }
}

using Pangea.Util;
using System;
using System.Linq;
using System.Runtime.Serialization;

namespace Pangea.Entidades
{
    [Serializable]
    public class MensagemRetorno : IDisposable
    {
        [DataMember]
        public string CodigoMensagem { get; set; }

        [DataMember]
        public string Descricao { get; set; }

        /// <summary>
        /// Construtor da classe
        /// </summary>
        /// <param name="enumeracao">Código da mensagem de retorno.</param>
        /// <param name="parametros">Parâmetros para a descrição da mensagem de retorno</param>
        public MensagemRetorno(Enum enumeracao, object[] parametros = null)
        {
            #region Antigo
            //StringBuilder sbParam = new StringBuilder();

            //if (parametros != null && parametros.Count() > 0)
            //{
            //    for (int i = 0; i < parametros.Count(); i++)
            //    {
            //        sbParam.Append((sbParam.Length > 0 ? ", " : string.Empty));
            //        sbParam.Append(parametros[i].ToString());
            //    }
            //}

            //this.Descricao = string.Format(EnumString.GetStringValue(enumeracao), sbParam);
            //this.CodigoMensagem = enumeracao.ToString();
            #endregion

            this.CodigoMensagem = enumeracao.ToString();

            if (parametros != null && parametros.Count() > 0)
            {
                if (parametros.Count() > 1)
                    this.Descricao = string.Format(EnumString.GetStringValue(enumeracao), parametros.Select(x => x.ToString()).ToArray());
                else
                    this.Descricao = string.Format(EnumString.GetStringValue(enumeracao), parametros[0]);
            }
            else
                this.Descricao = EnumString.GetStringValue(enumeracao);
        }


        /// <summary>
        /// Construtor da classe
        /// </summary>
        /// <param name="mensagem">Descrição da mensagem de retorno.</param>
        public MensagemRetorno(string mensagem)
        {
            this.Descricao = mensagem;
            this.CodigoMensagem = string.Empty;
        }


        /// <summary>
        /// Construtor
        /// </summary>
        public MensagemRetorno()
        {
        }


        public void Dispose()
        {
            this.CodigoMensagem = null;
            this.Descricao = null;
        }
    }
}

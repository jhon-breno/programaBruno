using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Pangea.Entidades.Enumeracao;
using System.Runtime.Serialization;
using Pangea.Util;
using Pangea.Entidades.DTO;

namespace Pangea.Entidades.Parametros.Saida
{
    [DataContract]
    public class SuministroSynergiaOut
    {
        public SuministroSynergiaOut(Enum enumeracao, object[] parametros = null, List<SuministroSynergiaDTO> lista=null)
        {
            StringBuilder sbParam = new StringBuilder();

            if (parametros != null && parametros.Count() > 0)
            {
                for (int i = 0; i < parametros.Count(); i++)
                {
                    sbParam.Append((sbParam.Length > 0 ? "," : string.Empty));
                    sbParam.Append(parametros[i].ToString());
                }
            }
            this.CodigoMensagem = enumeracao.ToString();
            this.Descricao = string.Format(EnumString.GetStringValue(enumeracao), sbParam);
            this.Retorno = lista;
        }

        [DataMember]
        public string CodigoMensagem { get; set; }
        [DataMember]
        public string Descricao { get; set; }
        [DataMember]     
        public object Retorno { get; set; }
    }
}

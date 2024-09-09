using Pangea.Entidades.Base;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Pangea.Entidades.DTO
{
    public class SuministroSynergiaDTO : EntidadeBase
    {


        [Required(ErrorMessage = "Obrigatório informar o numero cliente.")]
        public int? numero_cliente { get; set; }

        [Required(ErrorMessage = "Obrigatório informar a empresa.")]
        public string empresa { get; set; }
        public string estado_conexion { get; set; }
        public string estado_suministro { get; set; }
        public bool toi { get; set; }
        public bool corte_deuda { get; set; }


        /// <summary>
        /// Converte para string o conteúdo do objeto
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();

            foreach(PropertyInfo prop in this.GetType().GetProperties(BindingFlags.Public))
            {
                sb.Append(sb.Length > 0 ? ", " : string.Empty);
                sb.AppendFormat("{ '{0}':", prop.Name);

                var valor = prop.GetValue(this, null);
                if(valor != null)
                {
                    sb.AppendFormat("{0}", valor.ToString());
                }
            }
            sb.Append(" } ");

            return sb.ToString();
        }
   
    }
}

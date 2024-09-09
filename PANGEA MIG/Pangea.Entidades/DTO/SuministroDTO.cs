using Pangea.Entidades.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Pangea.Entidades.DTO
{
    public class SuministroDTO : EntidadeBase
    {
    
        public int numero_cliente { get; set; }
        public string empresa { get; set; }
        public string direccion { get; set; }
        public string restriccion_convenio { get; set; }
        public string restriccion_cambio_de_corte { get; set; }
        public int? codigo_resultado { get; set; }
        public string descripcion_resultado { get; set; }
        public string fecha_corte { get; set; }
        public string pago_en_proceso { get; set; }
        public string estado_conexion { get; set; }
        public string estado_suministro { get; set; }
        public string periodo_de_facturacion { get; set; }


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

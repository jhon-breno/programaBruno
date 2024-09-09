using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace Pangea.Entidades.SalesForce
{
    public class NotificarCasoReturn
    {
        public NotificarCasoReturn()
        {
            OKKO = string.Empty;
            CodigoError = string.Empty;
            MensajeError = string.Empty;
            
        }

        /// <summary>
        /// Resultado do envio.
        /// </summary>
        public string OKKO { get; set; }
            /// <summary>
        /// Codigo de erro.
        /// </summary>
        public string CodigoError { get; set; }
        /// <summary>
        /// Menssagem de erro.
        /// </summary>
        public string MensajeError { get; set; }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace Pangea.Entidades.SalesForce
{
    public class Corte
    {
        public Corte()
        {
            Guion = string.Empty;
            Monto = string.Empty;
            Fecha = string.Empty;
            Hora = string.Empty;
            Motivo = string.Empty;
            Nome = string.Empty;
            EstadoCliente = string.Empty;
            Vital = string.Empty;
            EstadoFaturamento = string.Empty;
            estadoFornecimento = string.Empty;
            documento1 = string.Empty;
            dvDoc1 = string.Empty;
            loteFaturamento = 0;

            CodigoError = string.Empty;
            MensajeError = string.Empty;
        }

        
        /// <summary>
        /// Data de emissão da ordem de corte do cliente.
        /// </summary>
        public string Guion { get; set; }

        /// <summary>
        /// Data de emissão da ordem de corte do cliente.
        /// </summary>
        public string Monto { get; set; }

        /// <summary>
        /// Data em que o cliente foi cortado.
        /// </summary>
        public string Fecha { get; set; }

        /// <summary>
        /// Tipo de corte realizado no cliente.
        /// </summary>
        public string Hora { get; set; }

        /// <summary>
        /// Tipo de corte realizado no cliente.
        /// </summary>
        public string Motivo { get; set; }

        /// <summary>
        /// Nome do cliente.
        /// </summary>
        public string Nome { get; set; }

        

        public string documento1 { get; set; }
        public string dvDoc1 { get; set; }
        /// <summary>
        /// Retorna o estado do cliente
        /// </summary>
        public string EstadoCliente { get; set; }
        public string EstadoFaturamento { get; set; }
        public string estadoFornecimento { get; set; }

        public int loteFaturamento { get; set; }

        [XmlElementAttribute(IsNullable = true)]
        public string CodigoError { get; set; }

        [XmlElementAttribute(IsNullable = true)]
        public string MensajeError { get; set; }

        public string Vital { get; set; }
    }
}

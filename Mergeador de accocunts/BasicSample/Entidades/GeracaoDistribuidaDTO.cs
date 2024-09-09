using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace SalesforceExtractor.Entidades
{
    /// <summary>
    /// Representação do arquivo de entrada para carga de clientes de Geração Distribuída
    /// </summary>
    [DebuggerDisplay("{ClienteGerador} - {ClienteConsumidor}")]
    public class GeracaoDistribuidaDTO : EqualityComparer<GeracaoDistribuidaDTO>
    {
        public string CodigoEmpresa { get; set; }
        public string TipoCliente { get; set; }
        public string ContratoGerador { get; set; }
        public string ClienteGerador { get; set; }
        public string ContratoConsumidor { get; set; }
        public string ClienteConsumidor { get; set; }
        public string Percentual { get; set; }

        public override bool Equals(GeracaoDistribuidaDTO x, GeracaoDistribuidaDTO y)
        {
            return x.ContratoGerador == y.ContratoGerador &&
                x.ContratoConsumidor == y.ContratoConsumidor &&
                x.ClienteGerador == y.ClienteGerador &&
                x.ClienteConsumidor == y.ClienteConsumidor;
        }

        public override int GetHashCode(GeracaoDistribuidaDTO obj)
        {
            return obj.GetHashCode();
        }
    }
}

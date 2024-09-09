using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Pangea.Entidades
{
    public class Corte : Entidades.Base.EntidadeBase
    {
        public Corte()
        {
            DataDaExecucao = string.Empty;
            DataDeSolicitacao = string.Empty;
            Tipo = string.Empty;
            Hora = string.Empty;
            Situacao = string.Empty;
            NumeroOrdem = string.Empty;
            AcaoRealizada = string.Empty;
        }

        /// <summary>
        /// Data de emissão da ordem de corte do cliente.
        /// </summary>
        public string DataDeSolicitacao { get; set; }

        /// <summary>
        /// Data em que o cliente foi cortado.
        /// </summary>
        public string DataDaExecucao { get; set; }

        /// <summary>
        /// Tipo de corte realizado no cliente.
        /// </summary>
        public string Tipo { get; set; }

        /// <summary>
        /// Tipo de corte realizado no cliente.
        /// </summary>
        public string Codigo_Tipo { get; set; }

        /// <summary>
        /// Horário em que o cliente foi cortado.
        /// </summary>
        public string Hora { get; set; }

        /// <summary>
        /// Situação do corte.
        /// </summary>
        public string Situacao { get; set; }

        public string NumeroOrdem { get; set; }

        public string AcaoRealizada { get; set; }
    }
}

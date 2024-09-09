using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Pangea.Entidades.Base;

namespace Pangea.Entidades
{
    public class CorteGrupoA : EntidadeBase
    {

        public CorteGrupoA()
        {
            NumeroCliente = -1;
            corr_corte = -1;
            motivo_corte = string.Empty;
            DataDaExecucao = string.Empty;
            DataDeSolicitacao = string.Empty;
            Tipo = string.Empty;
            Funcionario = string.Empty;
            Hora = string.Empty;
            Situacao = string.Empty;
            NumeroOrdem = string.Empty;
            AcaoRealizada = string.Empty;
        }

        /// <summary>
        /// Numero do cliente para instancia de corte especifica
        /// </summary>
        public int NumeroCliente { get; set; }

        /// <summary>
        /// correlativo corte
        /// </summary>
        public int corr_corte { get; set; }

        /// <summary>
        /// Motivo do corte
        /// </summary>
        public string motivo_corte { get; set; }

        /// <summary>
        /// Tipo de corte realizado no cliente.
        /// </summary>
        public string Tipo { get; set; }

        /// <summary>
        /// Data de emissão da ordem de corte do cliente.
        /// </summary>
        public string DataDeSolicitacao { get; set; }

        /// <summary>
        /// Data em que o cliente foi cortado.
        /// </summary>
        public string DataDaExecucao { get; set; }

        /// <summary>
        /// Funcionario responsavel 
        /// </summary>
        public string Funcionario { get; set; }

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

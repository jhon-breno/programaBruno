using Pangea.Entidades.Base;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace Pangea.Entidades.DTO
{
    [DisplayName("servicos")]
    public class ServicoDTO : EntidadeBase
    {
        public string tipo_ordem { get; set; }
        public string cod_servico { get; set; }
        public string des_servico { get; set; }
        public string cod_formulario { get; set; }
        public int tempo_max_servico { get; set; }
        public int perc_tempo_maximo { get; set; }
        public string tipo_ordem_relac { get; set; }
        public string tipo_servico_relac { get; set; }
        public DateTime data_ativacao { get; set; }
        public DateTime data_desativacao { get; set; }
        public Decimal valor1 { get; set; }
        public Decimal valor2 { get; set; }
        public string grupo { get; set; }
        public string subgrupo { get; set; }
        public string motivo_cliente { get; set; }
        public string motivo_empresa { get; set; }
        public int prioridade { get; set; }
        public string canc_ordem_assoc { get; set; }
        public int qtd_dias_max_rds { get; set; }
        public int qtd_dias_ingresso { get; set; }
        public string ind_tempo { get; set; }
        public string ind_varios_serv { get; set; }
        public string ind_dias_corridos { get; set; }
        public string ind_cliente_obrig { get; set; }
        public string ind_bloqueio_atend { get; set; }
        public string ind_irregularidade { get; set; }
        public string ind_cliente_desper { get; set; }
        public string ind_agendamento { get; set; }
        public string ind_cliente_divida { get; set; }
        public string ind_retira_corte { get; set; }
        public string ind_anula_corte { get; set; }
        public int ind_verif_deb { get; set; }
        public string ind_zona { get; set; }

        public bool FlAtivo
        {
            get
            {
                return DateTime.MinValue.Equals(this.data_desativacao);
            }
        }
    }
}
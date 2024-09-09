using Pangea.Entidades.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Pangea.Entidades
{
    public class Servico : EntidadeBase
    {
        public string tipo_ordem { get; set; }
        public string cod_servico { get; set; }
        public string des_servico { get; set; }
        public int grupo { get; set; }
        public int estado { get; set; }
        public int ind_dias_verifica_debito { get; set; }
        public string ind_verif_deb { get; set; }
        public string ind_cliente_desper { get; set; }
        public string ind_irregularidade { get; set; }
        public string cod_formulario { get; set; }
        public int tempo_max_servico { get; set; }
        public int perc_tempo_maximo { get; set; }
        public string ind_tempo { get; set; }
        public string ind_varios_serv { get; set; }
        public string tipo_ordem_relac { get; set; }
        public string tipo_servico_relac { get; set; }
        public DateTime data_ativacao { get; set; }
        public DateTime data_desativacao { get; set; }
        public decimal valor1 { get; set; }
        public decimal valor2 { get; set; }
        public string codigo_division { get; set; }
        public string motivo_cliente { get; set; }
        public string motivo_empresa { get; set; }
        public string ind_servico_ponser { get; set; }
        public string ind_dias_corridos { get; set; }
        public string ind_cliente_obrig { get; set; }
        public string ind_bloqueio_atend { get; set; }
        public string ind_cliente_inat { get; set; }
        public string des_fantasia { get; set; }
        public string ind_inspec_aberta { get; set; }
        public string permite_canc { get; set; }
        public int qtd_dias_max_pend { get; set; }
        public int qtd_dia_max_ordem { get; set; }
        public string ind_agendamento { get; set; }
        public string ind_cliente_divida { get; set; }
        public string ind_retira_corte { get; set; }
        public string ind_pda { get; set; }
        public string ind_ro { get; set; }
        public int prioridade { get; set; }
        public string ind_doc_valido { get; set; }
        public string numero_pexvarchar { get; set; }
        public string canc_ordem_assoc { get; set; }
        public int qtd_dias_max_rds { get; set; }
        public int qtd_dias_ingresso { get; set; }
        public string ing_ini { get; set; }
        public string ing_fim { get; set; }
        public string ind_acao_relacion { get; set; }
        public string ind_anula_corte { get; set; }
        public int qtd_dias_anula { get; set; }
        public string ind_obrig_ref { get; set; }
        public string ind_agenda_ordem { get; set; }
        public int tmp_carencia_minag { get; set; }
        public int tmp_carencia_maxag { get; set; }
        public string ignora_restricao { get; set; }
        public string npex { get; set; }
        public string tipo_medidor { get; set; }
        public string ind_comp_medid { get; set; }
        public string ind_iphan { get; set; }
        public string des_servico_clien { get; set; }
        public string exibir_portal { get; set; }
        public string hora_despacho { get; set; }
        public string antecipa_despacho { get; set; }
        public string ind_bloqueio_parci { get; set; }
        public int qtd_per_agend { get; set; }
        public int ind_agend_d_corri { get; set; }
        public string ind_ingr_portal { get; set; }
        public string ind_tmp_corri_ini { get; set; }
        public string ind_tmp_corri_fim { get; set; }
        public string ind_hr_comer_ini { get; set; }
        public string ind_hr_comer_fim { get; set; }
        public int pz_normativo { get; set; }
        public string enc_tempo_atend { get; set; }
        public string enc_tempo_susp { get; set; }
        public string ind_analise_susp { get; set; }
        public string tipo_ordem_relig { get; set; }
        public string tipo_servico_relig { get; set; }
        public string ind_zona { get; set; }
        public int subgrupo { get; set; }
        public string ind_utiliza_caixa { get; set; }
        public string perm_cli_des { get; set; }
        public string cod_derfer_prazo { get; set; }
        public string cod_derfer_retorno { get; set; }
        public string indica_rdschar { get; set; }
        public string arrecadacao { get; set; }
        public Sucursal Sucursal { get; set; }

        public bool fl_cliente_obrig
        {
            get { return "S".Equals(this.ind_cliente_obrig); }
        }
    }
}
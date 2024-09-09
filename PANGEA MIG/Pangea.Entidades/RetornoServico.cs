using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entidades
{
    public class RetornoServico
    {
        public string tipo_ordem { get; set; }
        public string tipo_servico { get; set; }
        public string etapa { get; set; }
        public string codigo_retorno { get; set; }
        public string ind_serv_executado { get; set; }
        public string ind_encerra_ordem  { get; set; }
        public string ind_def_tec_client { get; set; }
        public string ind_def_tec_empres { get; set; }
        public string ind_pendencia { get; set; }
        public string ind_efeito_tempo { get; set; }
        public string ind_movimenta_med { get; set; }
        public string ind_procedente { get; set; }
        public DateTime data_ativacao { get; set; }
        public DateTime data_desativacao { get; set; }
        public string descricao_retorno { get; set; }
        public string proxima_etapa { get; set; }
        public string codigo_acao { get; set; }
        public string tipo_ordem_acao { get; set; }
        public string tipo_servico_acao { get; set; }
        public string etapa_acao { get; set; }
        public string tipo_obra { get; set; }
        public string ind_retorna_corte { get; set; }
        public string ind_msg_fatura { get; set; }
        public string ind_plc { get; set; }
        public string ind_dados_plc { get; set; }
        public string ind_agendamento { get; set; }
        public string encargo_variavel { get; set; }
        public string libercao_gom { get; set; }
        public string aprovacao_gom { get; set; }
        public string utiliza_pda { get; set; }
        public string cod_tela_int_exec { get; set; }
        public string cod_tela_ext_exec { get; set; }
        public string cod_tela_ext_pend { get; set; }
        public string cod_tela_acao_exec { get; set; }
        public string cod_tela_acao_pend { get; set; }
        public string ind_deslig_med { get; set; }
        public string ind_envia_syngb { get; set; }
        public string ing_corte_rest { get; set; }
        public string ret_corte_rest { get; set; }
        public string cod_motivo { get; set; }
        public string cod_restricao { get; set; }
        public string obrig_digit_proc { get; set; }
        public string ind_vital { get; set; }
        public string utiliza_caixa { get; set; }
        public string retorno_acao { get; set; }
        public string cod_derfer_retorno { get; set; }
        public string ind_comunica { get; set; }
    }
}

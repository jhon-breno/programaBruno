using Pangea.Entidades.Base;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace Pangea.Entidades
{
    public class Etapa : EntidadeBase
    {
        public string tipo_ordem { get; set; }
        public string tipo_servico { get; set; }
        public string cod_etapa { get; set; }
        public int sequencia_etapa { get; set; }
        public string descricao_etapa { get; set; }
        public string cod_formulario { get; set; }
        public string indicador_final { get; set; }
        public int tempo_maximo_etapa { get; set; }
        public int perc_tempo_maximo { get; set; }
        public DateTime data_ativacao { get; set; }
        public DateTime data_desativacao { get; set; }
        public string periodo_despacho { get; set; }
        public DateTime data_ult_despacho { get; set; }
        public string codigo_division { get; set; }
        public string ind_atend_int { get; set; }
        public string tipo_remessa { get; set; }
        public string ind_dados_plc { get; set; }
        public string ind_plc { get; set; }
        public string ind_deafea { get; set; }
        public string principal_etapa { get; set; }
        public string trabalho_campo { get; set; }
        public string ind_capt_coord { get; set; }
    }
}
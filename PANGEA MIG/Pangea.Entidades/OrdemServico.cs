using Pangea.Entidades.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pangea.Entidades
{
    public class OrdemServico : EntidadeBase
    {
        public string numero_ordem { get; set; }
        public int numero_cliente { get; set; }
        public string estado { get; set; }
        public DateTime data_ingresso { get; set; }
        public string tipo_ordem { get; set; }
        public string tipo_servico { get; set; }
        public string sucursal_origem { get; set; }
        public string area_origem { get; set; }
        public DateTime data_estado { get; set; }
        public string etapa { get; set; }
        public DateTime data_etapa { get; set; }
        public DateTime data_cont_def_tec { get; set; }
        public int tempo_def_tecnico { get; set; }
        public int tempo_def_tec_etap { get; set; }
        public string sucursal_destino { get; set; }
        public string regional_destino { get; set; }
        public string area_destino { get; set; }
        public int corr_visita { get; set; }
        public int corr_pendencia { get; set; }
        public string observacoes { get; set; }
        public string observacao_exe { get; set; }
        public string telefone_contato { get; set; }
        public string rol_ingresso { get; set; }
        public string rol_alteracao { get; set; }
        public string cod_mot_anulacao { get; set; }
        public string rol_anulacao { get; set; }
        public string ind_cobranca { get; set; }
        public string ind_cliente_obrig { get; set; }
        public string ind_baixarenda_sol { get; set; }
        public int nro_caso { get; set; }
        public string nro_ordem_ponser { get; set; }
        public string ind_enviado_tec { get; set; }
        public string numero_ordem_relac { get; set; }
        public string numero_segen { get; set; }
        public int numero_sda { get; set; }
        public string motivo_ingresso { get; set; }
        public string sucursal_original { get; set; }
    }


}

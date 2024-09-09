using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entidades.DTO
{
    public class VisitaOrdemDTO
    {
        public string numero_ordem { get; set; }
        public string etapa { get; set; }
        public int corr_visita { get; set; }
        public DateTime data_visita { get; set; }
        public DateTime hora_exec_visita { get; set; }
        public string ind_agendado { get; set; }
        public string rol_ret_visita { get; set; }
        public string numero_ordem_filha { get; set; }
        public DateTime hora_ini_prevista { get; set; }
        public DateTime hora_fim_prevista { get; set; }
        public string empreiteira { get; set; }
        public string rol_responsavel { get; set; }
        public string rol_visita { get; set; }
        public string area_destino{ get; set; }
        public DateTime data_ini_etapa{ get; set; }
        public int temp_def_tecnico { get; set; }
	
        public VisitaOrdemDTO()
        {
        }

        public VisitaOrdemDTO(string numero_ordem, int corr_visita = 0)
        {
            this.numero_ordem = numero_ordem;
            this.corr_visita = corr_visita;
        }
        public string estado { get; set; }
        public string descricao_etapa { get; set; }
        public string cod_retorno { get; set; }
    }

}

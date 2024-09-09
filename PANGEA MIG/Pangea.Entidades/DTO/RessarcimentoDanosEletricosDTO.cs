using Pangea.Entidades.Enumeracao;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Entidades;

namespace Pangea.Entidades
{
    [Serializable]
    public class RessarcimentoDanosEletricosDTO
    {
        public string CodigoEmpresa { get; set; }
        public string numero_ordem { get; set; }
        public string numero_caso { get; set; }
        public string cod_retorno { get; set; }
        public string eletricista { get; set; }
        public DateTime data_exec_visita { get; set; }
        public DateTime hora_exec_visita { get; set; }
        public string rol_ret_visita { get; set; }
        public DateTime data_ret_visita { get; set; }
        public DateTime hora_fim_prevista { get; set; }
        public string rol_responsavel { get; set; }
        public DateTime dat_lib_def_tec { get; set; }
        public string rol_lib_def_tec { get; set; }
        public string codigo_cargo { get; set; }
        public float valor_cargo { get; set; }
        public string numero_ordem_filha { get; set; }
        public int numero_form_venda { get; set; }
        public string serie_form_venda { get; set; }
        public DateTime data_despacho { get; set; }
        public byte numero_tarefa { get; set; }
        public DateTime data_inic_visita { get; set; }
        public int periodo_agend { get; set; }
        public DateTime data_agend { get; set; }
        public string turno_agend { get; set; }
        public string numero_cliente { get; set; }
    }

}

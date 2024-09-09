using Pangea.Entidades.Base;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace Pangea.Entidades.DTO
{
    [DisplayName("v2_comunica")]
    public class V2ComunicaDTO : EntidadeBase
    {
        public string numero_cliente { get; set; }
        public int numero_sol { get; set; }
        public string comando { get; set; }
        public string codigo_cp { get; set; }
        public string codigo_cs { get; set; }
        public string codigo_ps { get; set; }
        public string estado { get; set; }
        
        /// <summary>
        /// MinValue = current
        /// </summary>
        public DateTime data_ingresso { get; set; }

        /// <summary>
        /// MinValue = current
        /// </summary>
        public DateTime data_inicio { get; set; }

        /// <summary>
        /// MinValue = current
        /// </summary>
        public DateTime data_executa { get; set; }

        /// <summary>
        /// MinValue = current
        /// </summary>
        public DateTime data_modifica { get; set; }

        public int prioridade { get; set; }
        public int id_orion { get; set; }
        public int total_cliente { get; set; }
    }
}

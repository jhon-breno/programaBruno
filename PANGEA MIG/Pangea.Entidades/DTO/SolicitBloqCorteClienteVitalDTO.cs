using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pangea.Entidades.DTO
{
    public class SolicitBloqCorteClienteVitalDTO
    {
        public string codigo_empresa { get; set; }
        //public int sucursal { get; set; }
        //public string numero_livro { get; set; }
        //public int corr_corte { get; set; }
        //public string estado { get; set; }
        //public string conta_contrato { get; set; }
        public string numero_cliente { get; set; }
        //public string motivo_sol { get; set; }
        //public string tipo_corte { get; set; }        
        //public string numero_ordem { get; set; }
        //public string tipo_ordem { get; set; }
        //public string tipo_servico { get; set; }
        //public DateTime? data_ingresso { get; set; }       
        //public DateTime? data_estado { get; set; }
        //public string etapa { get; set; }
        //public DateTime? data_etapa { get; set; }
        public string motivo { get; set; }
        public int dias { get; set; }
        public DateTime? fecha_solicitud { get; set; }



    }
}

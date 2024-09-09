using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Entidades.DTO
{
    public class CorteRepoSolicitadoDTO
    {
        //[DataMember]
        [MaxLength(10)]
        public string NumeroOrden { get; set; }
        
        [MaxLengthAttribute(20)]
        public string IDCorteRepo { get; set; }

        
        [MaxLengthAttribute(1)]
        public string TipoRegistro { get; set; }

        
        [MaxLengthAttribute(2)]
        public string Motivo { get; set; }

       
        [MaxLengthAttribute(2)]
        public string Estado { get; set; }

         
        //[MaxLengthAttribute(1)]
        public string FechaEjecucion { get; set; }

       
        //[MaxLengthAttribute(1)]
        public string FechaSolicitud { get; set; }

        
        [MaxLengthAttribute(2)]
        public string AccionRealizada { get; set; }

            
        [MaxLengthAttribute(2)]
        public string Tipo { get; set; }
    }
}

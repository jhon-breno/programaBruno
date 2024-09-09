using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pangea.Entidades.DTO
{
    [Serializable]
    public class ActuacionSolicitudDTO
    {
        public string NumeroOrden { get; set; }
        public string Estado { get; set; }
        //public string COD_ETAPA { get; set; }
        //public string DES_ETAPA { get; set; }
        //public string CODIGO_RESULTADO { get; set; }
        //public string DESCRIPCION_RESULTADO { get; set; }
        public string NumeroCaso { get; set; }
        //public string OBS_EXECUCAO { get; set; }
        public string Descripcion { get; set; }
        public string Responsable { get; set; }
        public string Favorabilidad { get; set; }
        public string ROL { get; set; }
        public string DescripcionRetornoOrden { get; set; }
    }
}

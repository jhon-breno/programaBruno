using Entidades;
using System;

namespace Pangea.Entidades.DTO
{
    public class CalendFacturacionDTO : Arquivo
    {
        public string Portion { get; set; }
        public string Rate { get; set; }
        public string Plan { get; set; }
        public string OperativeCenter { get; set; }
        public string ScheduleBillingDate { get; set; }
        public string Municipio { get; set; }
        public string Localidade { get; set; }
        public string Sector { get; set; }
    }
}
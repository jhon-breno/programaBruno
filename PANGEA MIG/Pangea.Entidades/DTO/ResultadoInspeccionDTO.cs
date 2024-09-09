using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Pangea.Entidades.DTO
{
    public class ResultadoInspeccionDTO
    {
        public string CodigoEmpresa { get; set; }
        public int NumeroMedidor { get; set; }
        public int NumeroCliente { get; set; }
        public string Marca_medidor { get; set; }
        public string Modelo { get; set; }
        public int Correlativo { get; set; }
        public float Constante { get; set; }
        public float UltimaLeituraAtiva1 { get; set; }
        public float UltimaLeituraAtiva3 { get; set; }
        public int NumeroMedAnt { get; set; }
        public string MarcaMedAnt { get; set; }

    }
}

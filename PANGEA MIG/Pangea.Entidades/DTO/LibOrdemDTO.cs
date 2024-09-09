using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entidades.DTO
{
    [Serializable]
    public class LibOrdemDTO
    {
        public int CodigoEmpresa { get; set; }
        public int NumeroCaso { get; set; }
        public string TipoOrdem { get; set; }
        public string Rol { get; set; }
        public string ObsPendencia { get; set; }
        public string Observacao { get; set; }
        public string tipoCliente { get; set; }
        public int NumeroCliente { get; set; }

    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pangea.Entidades
{
    [Serializable]
    public class UnidadeLecturaSecDTO
    {
        public int codigo_empresa { get; set; }
        public string unidade_leitura { get; set; }
        public string conjuntoContrato { get; set; }
        public string data_leitura_prevista { get; set; }
        public string data_leitura_atual { get; set; }
        public int intervalo { get; set; }
        public string sucursal { get; set; }
        public int sector { get; set; }
       // public int referencia { get; set; }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExtracaoSalesForce.Modelo.SalesForce
{
    public class Street
    {
        public long  id_rua { get; set; }
        public string nome_rua { get; set; }
        public string tipo_logradouro { get; set; }
        public string cidade { get; set; }
        public string uf { get; set; }
        public string pais { get; set; }
        public string comuna { get; set; }
        public string regiao { get; set; }
        public string calle { get; set; }
        public string localidad { get; set; }
        public string bairro { get; set; }
        public DateTime data_inclusao { get; set; }
    }
}

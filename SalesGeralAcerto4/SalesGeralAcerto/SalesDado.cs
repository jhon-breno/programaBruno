using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SalesGeralAcerto
{
    public class SalesDado
    {
        public string tipo_documento { get; set; }
        public string documento { get; set; }
        public string externalid_conta { get; set; }
        public string id_sales_conta { get; set; }
        public string externalid_contato { get; set; }
        public string id_sales_contato { get; set; }
        public string nome { get; set; }
        public string numero_cliente { get; set; }
        public string id_sales_pod { get; set; }
        public string externalid_pod { get; set; }
        public string empresa { get; set; }
        public string id_sales_asset { get; set; }
        public string externalid_asset { get; set; }
    }
}

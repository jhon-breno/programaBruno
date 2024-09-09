using System;
using System.Collections.Generic;
using System.Text;
using Entidades.BotaoEmpresa.TarifasIp;

namespace Entidades.BotaoEmpresa.TarifaGa
{
    public class Tarifa
    {
        public string CodigoTarifa { get; set; }
        public DescricaoTarifa DescricaoTarifa { get; set; }
        public List<PrecoTarifa> PrecoTarifa { get; set; }
        public List<PrecoTarifaFaixa> PrecoTarifaFaixa { get; set; }
    }
}

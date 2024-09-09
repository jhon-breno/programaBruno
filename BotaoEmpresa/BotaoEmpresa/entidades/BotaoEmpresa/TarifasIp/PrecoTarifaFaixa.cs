using System;
using System.Collections.Generic;
using System.Text;
using Entidades.BotaoEmpresa.TarifaGa;

namespace Entidades.BotaoEmpresa.TarifasIp
{
    public class PrecoTarifaFaixa : PrecoTarifa
    {
        public double? ValorOrigem { get; set; }
        public float? Porcentagem { get; set; }
    }
}

using System;
using System.Collections.Generic;
using System.Text;
using Entidades.BotaoEmpresa.TarifaGa;

namespace Entidades.BotaoEmpresa.TarifasIp
{
    public class TarifaFaixa : Tarifa
    {
        public string LimiteInferior { get; set; }

        public string LimiteSuperior { get; set; }



    }
}

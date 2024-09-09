using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Pangea.Entidades.Base;

namespace Pangea.Entidades
{
    public class Agenda : EntidadeBase
    {
        public Agenda()
        {
            Localidade = 0;
            Zona = 0;
            DataLeitura = string.Empty;
            DataFaturamento = string.Empty;
            DataReparto = string.Empty;
            DataVencimento = string.Empty;
            CodigoVencimento = 0;
            DataVencimento2 = string.Empty;

  

        }

        public int Localidade { get; set; }
        public int Zona { get; set; }
        public string DataLeitura { get; set; }
        public string DataFaturamento { get; set; }
        public string DataReparto { get; set; }
        public string DataVencimento { get; set; }
        public int CodigoVencimento { get; set; }
        public string DataVencimento2 { get; set; }
  

    }
}

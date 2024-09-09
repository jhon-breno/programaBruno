using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Pangea.Entidades.SOAP
{
    public class Movimentos
    {
        public int totalSolicitacoes { get; set; }
        public int totalSucessos { get; set; }
        public int totalErros { get; set; }
        public string Status { get; set; }

        public Movimentos()
        {
            this.totalSolicitacoes = 0;
            this.totalSucessos = 0;
            this.totalErros = 0;
            Status = String.Empty;
        }

        public string getMsgObjetoIncorreto()
        {
            return "Uso de objeto de tipo incorreto";
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Pangea.Entidades
{
    public class Tabla
    {
        public string codigo { get; set; }
        public string descricao { get; set; }
        public string nomeTabla { get; set; }
        public string valorAlf { get; set; }

        public Tabla()
        {
            codigo = string.Empty;
            descricao = string.Empty;
            nomeTabla = string.Empty;
            valorAlf = string.Empty;
        }

    }
}

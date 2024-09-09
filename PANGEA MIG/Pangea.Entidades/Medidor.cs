using Pangea.Entidades.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Pangea.Entidades
{
    public class Medidor : EntidadeBase
    {
        public string numero_cliente { get; set; }
        public string estado { get; set; }
    }
}

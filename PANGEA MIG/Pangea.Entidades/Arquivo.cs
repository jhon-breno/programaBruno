using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entidades
{
    public class Arquivo
    {
        public int NumLinha { get; set; }
        public string StrLinha { get; set; }
        public bool PossuiErro { get; set; }
        public string DescricaoErro { get; set; }
    }
}

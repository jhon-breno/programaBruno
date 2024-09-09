using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Pangea.Entidades
{
    public class EmpresaParceira
    {
        public string codEmpresaParc { get; set; }
        public string descricaoEmpresa { get; set; }

        public EmpresaParceira()
        {
            codEmpresaParc = "";
            descricaoEmpresa = "";
        }

        public EmpresaParceira(string codEmpresaParc)
        {
            this.codEmpresaParc = codEmpresaParc;
        }
    }
}

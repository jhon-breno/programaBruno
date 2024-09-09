﻿using Pangea.Entidades;
using Pangea.Entidades.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Pangea.Dados.Base
{
    public class AutenticacaoCoelceDAO : AutenticacaoDAO
    {
        public AutenticacaoCoelceDAO() : base(Empresa.Coelce) { }
    }
}

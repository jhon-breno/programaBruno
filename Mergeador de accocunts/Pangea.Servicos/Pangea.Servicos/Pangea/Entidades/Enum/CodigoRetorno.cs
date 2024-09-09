using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Pangea.Entidades.Enum
{
    public enum CodigoDeRetorno
    {
        Erro = -1,
        NenhumRegistroEncontrado = 0,
        Sucesso = 1,
        MaisDeUmRegistroFoiEncontrado = 2,
        OperacaoInvalida = 3
    }
}

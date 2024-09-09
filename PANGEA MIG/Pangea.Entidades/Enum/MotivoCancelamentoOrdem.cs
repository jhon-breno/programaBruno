using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace Pangea.Entidades.Enumeracao
{
    public enum MotivoCancelamentoOrdem
    {
        [Description("Não Identificado")]
        NaoIdentificado = -1,

        DesistenciaCliente = 1,
        EnderecoNaoLocalizado = 2,
        ServicoDesacordoPedido = 3,
        ErroPreenchimento = 4,
        PedidoEmDuplicidade = 6,
        DividaPaga = 10,
        AreaPreservacaoAmbiental = 11,
        FaixaServidao = 12,
        DefeitoTecnico = 13,
        CasaAbandonadaDemolida = 14,
        ClienteNormalizado = 15,
        TerrenoVazio = 17,
        RDScomDataVencida = 19,
        ClienteAusente = 20,
        ServicoDesacordo = 21,
        LuzParaTodos = 23,
        CancelamentoDirTecnica = 24,
        AreaRisco = 26,
        SemOrdemExternaRedeAssociada = 27,
        PendenciaLicencaAmbiental = 28,
        CancelamentoViaSalesForce = 29
    }
}

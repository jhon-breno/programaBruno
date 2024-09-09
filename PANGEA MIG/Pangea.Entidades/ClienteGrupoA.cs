using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Pangea.Entidades.Base;

namespace Pangea.Entidades
{
    public class ClienteGrupoA : EntidadeBase
    {

        public ClienteGrupoA()
        {
            NumeroCliente = 0;
            DVNumeroCliente = 0;
            Nome = string.Empty;
            Email = string.Empty;
            DDD = string.Empty;
            Telefone = string.Empty;
            Ramal = string.Empty;
            EstadoFornecimento = 0;
            ClienteDespersonalizado = string.Empty;
            CPF = string.Empty;
            CPFDV = string.Empty;
            QtdMedidoresInstalados = 0;
            Medidor = string.Empty;
            Sucursal = string.Empty;
            Regional = string.Empty;
            CorrelativoCorte = string.Empty;
            EstadoFaturamento = string.Empty;
            CEP = string.Empty;
            EnderecoPostal = string.Empty;
            PontoReferencia = string.Empty;
            IndicadorBaixaRenda = string.Empty;
            Classe = string.Empty;
            Municipio = string.Empty;
            Localidade = string.Empty;
            CorrelativoParcelamento = string.Empty;
            TipoRede = string.Empty;
            Endereco = string.Empty;
            Tarifa = string.Empty;
            Documento = string.Empty;
            DVDocumento = string.Empty;
            TipoDocumento = string.Empty;
            Documento2 = string.Empty;
            DVDocumento2 = string.Empty;
            TipoDocumento2 = string.Empty;
            Agrupado = false;
            NumeroClienteAgenciaVirtual = -1;
            NumeroUsuarioAgenciaVirtual = -1;
            PermiteReligacaoUrgente = false;
            HoraInicioReligacaoUrgente = "00:00";
            HoraFimReligacaoUrgente = "00:00";
            HoraInicioReligacaoUrgenteSabado = "00:00";
            HoraFimReligacaoUrgenteSabado = "00:00";
            Zona = "I";
        }

        public int NumeroCliente { get; set; }
        public int DVNumeroCliente { get; set; }
        public string Nome { get; set; }
        public string Email { get; set; }
        public string DDD { get; set; }
        public string Telefone { get; set; }
        public string Ramal { get; set; }
        public int EstadoFornecimento { get; set; }
        public string ClienteDespersonalizado { get; set; }
        public string CPF { get; set; }
        public string CPFDV { get; set; }
        public int QtdMedidoresInstalados { get; set; }
        public string Medidor { get; set; }
        public string Sucursal { get; set; }
        public string Regional { get; set; }
        public string CorrelativoCorte { get; set; }
        public string IndicadorClienteVital { get; set; }
        public string Estado { get; set; }
        public string EstadoFaturamento { get; set; }
        public int DiaVencimento { get; set; }
        public string CEP { get; set; }
        public string EnderecoPostal { get; set; }
        public string PontoReferencia { get; set; }
        public string IndicadorBaixaRenda { get; set; }
        public string Classe { get; set; }
        public string Municipio { get; set; }
        public string Localidade { get; set; }
        public string CorrelativoParcelamento { get; set; }
        public string TipoRede { get; set; }
        public string Endereco { get; set; }
        public string Tarifa { get; set; }
        public string Documento { get; set; }
        public string DVDocumento { get; set; }
        public string TipoDocumento { get; set; }
        public string Documento2 { get; set; }
        public string DVDocumento2 { get; set; }
        public string TipoDocumento2 { get; set; }
        public bool Agrupado { get; set; }
        public int NumeroClienteAgenciaVirtual { get; set; }
        public int NumeroUsuarioAgenciaVirtual { get; set; }
        public bool PermiteReligacaoUrgente { get; set; }
        public string HoraInicioReligacaoUrgente { get; set; }
        public string HoraFimReligacaoUrgente { get; set; }
        public string HoraInicioReligacaoUrgenteSabado { get; set; }
        public string HoraFimReligacaoUrgenteSabado { get; set; }
        public string Zona { get; set; }

    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Pangea.Entidades.Enumeracao;
//using Entidades.Parametros.Entrada;

namespace Pangea.Entidades.Atendimentos
{
    public class Atendimento : Base.EntidadeBase
    {
        public Atendimento()
        {
            NumeroCliente = 0;
            MotivoEmpresa = string.Empty;
            MotivoCliente = string.Empty;
            SubMotivo = string.Empty;
            Protocolo = "0";
            CanalAtendimento = string.Empty;
            Erro = string.Empty;
            Telefone = string.Empty;
            Observacao = string.Empty;
            isPrimeiro = false;
            agencia = string.Empty;
        }

        //public Atendimento(AtendimentoEntrada atendEnt)
        //{
        //    NumeroCliente = atendEnt.NumeroCliente;
        //    MotivoEmpresa = TratarMotivos(atendEnt.Empresa, atendEnt.MotivoEmpresa, false);
        //    MotivoCliente = TratarMotivos(atendEnt.Empresa, atendEnt.MotivoCliente, true);
        //    SubMotivo = atendEnt.SubMotivo;
        //    Protocolo = ConverterProtocolo(atendEnt.Protocolo).ToString();
        //    Telefone = atendEnt.Telefone;
        //    Observacao = atendEnt.Observacao;
        //    agencia = atendEnt.Agencia;
        //    Erro = string.Empty;
        //}

        //public Atendimento(AtendimentoGAEntrada atendEnt)
        //{
        //    NumeroCliente = atendEnt.NumeroCliente;
        //    MotivoEmpresa = TratarMotivos(atendEnt.Empresa, atendEnt.MotivoEmpresa, false);
        //    MotivoCliente = TratarMotivos(atendEnt.Empresa, atendEnt.MotivoCliente, true);            
        //    Protocolo = ConverterProtocolo(atendEnt.Protocolo).ToString();
        //    Telefone = atendEnt.Telefone;
        //    Observacao = atendEnt.Observacao;
        //    Erro = string.Empty;
        //}

        private int ConverterProtocolo(string protocolo)
        {
            int _protocolo = 0;

            if (string.IsNullOrEmpty(protocolo) || !int.TryParse(protocolo, out _protocolo))
            {
                _protocolo = 0;
            }

            return _protocolo;
        }

        //private string TratarMotivos(string empresa, string motivo, bool motCli)
        //{
        //    string result = motivo;
        //    if (string.IsNullOrEmpty(motivo) || motivo.Length != 4)
        //    {
        //        if (motCli)
        //        {
        //            result = Motivos.getInstance(empresa).MOTIVO_CLIENTE_GENERICO;
        //        }
        //        else
        //        {
        //            result = Motivos.getInstance(empresa).MOTIVO_EMPRESA_GENERICO;
        //        }                
        //    }

        //    return result;
        //}

        public int NumeroCliente { get; set; }
        public string MotivoEmpresa { get; set; }
        public string MotivoCliente { get; set; }
        public string SubMotivo { get; set; }
        public string Protocolo { get; set; }
        public string CanalAtendimento { get; set; }
        public string Erro { get; set; }
        public string Telefone { get; set; }
        public string Observacao { get; set; }
        public bool isPrimeiro { get; set; }
        public string agencia { get; set; }
    }
}

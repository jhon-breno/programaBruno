using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Pangea.Entidades.Atendimentos
{
    public class Atendimento_PrevisaoCorte : Atendimento
    {
        public Atendimento_PrevisaoCorte(string empresa, string canalAtendimento, int numeroCliente, string protocolo)
            : base()
        {
            CanalAtendimento = canalAtendimento;
            NumeroCliente = numeroCliente;
            Protocolo = protocolo;
            MotivoCliente = Motivos.getInstance(empresa).MOTIVO_CLIENTE_PREVISAO_CORTE;
            MotivoEmpresa = Motivos.getInstance(empresa).MOTIVO_EMPRESA_PREVISAO_CORTE;
        }
    }
}

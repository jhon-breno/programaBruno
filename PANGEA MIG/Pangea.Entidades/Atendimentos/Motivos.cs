using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Linq;
using System.Reflection;
using System.Text;
using Pangea.Entidades.Enumeracao;

namespace Pangea.Entidades.Atendimentos
{
    public class Motivos
    {
        private static Motivos motivosAmpla = null;
        private static Motivos motivosCoelce = null;
        public static Motivos getInstance(string empresa)
        {
            Motivos result = null;
            switch (int.Parse(empresa))
            {
                case (int)Empresa.RJ:
                    if (motivosAmpla == null)
                    {
                        motivosAmpla = new Motivos("_AMPLA");
                    }
                    result = motivosAmpla;
                    break;
                case (int)Empresa.CE:
                    if (motivosCoelce == null)
                    {
                        motivosCoelce = new Motivos("_COELCE");
                    }
                    result = motivosCoelce;
                    break;
            }

            return result;
        }

        protected Motivos(string empresa)
        {
            PropertyInfo[] props = typeof(Motivos).GetProperties();
            foreach (PropertyInfo prop in props)
            {
                string tempConfig = ConfigurationManager.AppSettings[prop.Name + empresa];

                if (string.IsNullOrEmpty(tempConfig))
                {
                    tempConfig = "1234";
                }

                prop.SetValue(this, tempConfig, null);
            }
        }

        public string MOTIVO_CLIENTE_CONSULTA_DOCUMENTO { get; private set; }
        public string MOTIVO_EMPRESA_CONSULTA_DOCUMENTO { get; private set; }        
        public string MOTIVO_CLIENTE_CONSULTA_DEBITO { get; private set; }
        public string MOTIVO_EMPRESA_CONSULTA_DEBITO { get; private set; }        
        public string MOTIVO_CLIENTE_GENERICO { get; private set; }
        public string MOTIVO_EMPRESA_GENERICO { get; private set; }
        public string MOTIVO_CLIENTE_SOLICITACAO_EMERGENCIA { get; private set; }
        public string MOTIVO_EMPRESA_SOLICITACAO_EMERGENCIA { get; private set; }
        public string MOTIVO_CLIENTE_CONSULTA_EMERGENCIA { get; private set; }
        public string MOTIVO_EMPRESA_CONSULTA_EMERGENCIA { get; private set; }
        public string MOTIVO_CLIENTE_RECLAMACAO_EMERGENCIA { get; private set; }
        public string MOTIVO_EMPRESA_RECLAMACAO_EMERGENCIA { get; private set; }
        public string MOTIVO_CLIENTE_SOLICITACAO_RELIGACAO_NORMAL_URBANA { get; private set; }
        public string MOTIVO_EMPRESA_SOLICITACAO_RELIGACAO_NORMAL_URBANA { get; private set; }
        public string MOTIVO_CLIENTE_SOLICITACAO_RELIGACAO_URGENTE_URBARNA { get; private set; }
        public string MOTIVO_EMPRESA_SOLICITACAO_RELIGACAO_URGENTE_URBARNA { get; private set; }
        public string MOTIVO_CLIENTE_SOLICITACAO_RELIGACAO_NORMAL_RURAL { get; private set; }
        public string MOTIVO_EMPRESA_SOLICITACAO_RELIGACAO_NORMAL_RURAL { get; private set; }
        public string MOTIVO_CLIENTE_SOLICITACAO_RELIGACAO_URGENTE_RURAL { get; private set; }
        public string MOTIVO_EMPRESA_SOLICITACAO_RELIGACAO_URGENTE_RURAL { get; private set; }
        public string MOTIVO_CLIENTE_SOLICITACAO_RELIGACAO_VITAL { get; private set; }
        public string MOTIVO_EMPRESA_SOLICITACAO_RELIGACAO_VITAL { get; private set; }
        public string MOTIVO_CLIENTE_CONSULTA_RELIGACAO { get; private set; }
        public string MOTIVO_EMPRESA_CONSULTA_RELIGACAO { get; private set; }
        public string MOTIVO_CLIENTE_CONSULTA_RELIGACAO_GA { get; private set; }
        public string MOTIVO_EMPRESA_CONSULTA_RELIGACAO_GA { get; private set; } 
        public string MOTIVO_CLIENTE_CONSULTA_SUSPENSAO_FORNECIMENTO { get; private set; }
        public string MOTIVO_EMPRESA_CONSULTA_SUSPENSAO_FORNECIMENTO { get; private set; }
        public string MOTIVO_CLIENTE_CONSULTA_ORDENS_PRAZOS { get; private set; }
        public string MOTIVO_EMPRESA_CONSULTA_ORDENS_PRAZOS { get; private set; }
        public string MOTIVO_CLIENTE_OUVIDORIA { get; private set; }
        public string MOTIVO_EMPRESA_OUVIDORIA { get; private set; }
        public string MOTIVO_CLIENTE_COMUNICACAO_CONTA_PAGA { get; private set; }
        public string MOTIVO_EMPRESA_COMUNICACAO_CONTA_PAGA { get; private set; }
        public string MOTIVO_CLIENTE_ATUALIZACAO_CADASTRAL { get; private set; }
        public string MOTIVO_EMPRESA_ATUALIZACAO_CADASTRAL { get; private set; }        
        public string MOTIVO_CLIENTE_DATA_VENCIMENTO { get; private set; }
        public string MOTIVO_EMPRESA_DATA_VENCIMENTO { get; private set; }
        public string MOTIVO_CLIENTE_PAGAMENTO_SEM_FATURA { get; private set; }
        public string MOTIVO_EMPRESA_PAGAMENTO_SEM_FATURA { get; private set; }
        public string MOTIVO_CLIENTE_PREVISAO_CORTE { get; private set; }
        public string MOTIVO_EMPRESA_PREVISAO_CORTE { get; private set; }
        public string MOTIVO_CLIENTE_ENVIO_2VIA_EMAIL { get; private set; }
        public string MOTIVO_EMPRESA_ENVIO_2VIA_EMAIL { get; private set; }
        public string MOTIVO_CLIENTE_SOLICITACAO_LEITURA_ESPECIAL { get; private set; }
        public string MOTIVO_EMPRESA_SOLICITACAO_LEITURA_ESPECIAL { get; private set; }
        public string MOTIVO_CLIENTE_INGRESSO_LEITURA_ESPECIAL { get; private set; }
        public string MOTIVO_EMPRESA_INGRESSO_LEITURA_ESPECIAL { get; private set; }
        public string MOTIVO_CLIENTE_CONSULTA_PRAZO_INCIDENCIA { get; private set; }
        public string MOTIVO_EMPRESA_CONSULTA_PRAZO_INCIDENCIA { get; private set; }
        public string MOTIVO_CLIENTE_SOLICITACAO_LOJA_PROXIMA { get; private set; }
        public string MOTIVO_EMPRESA_SOLICITACAO_LOJA_PROXIMA { get; private set; }
        public string MOTIVO_CLIENTE_ENVIO_2VIA_SMS { get; private set; }
        public string MOTIVO_EMPRESA_ENVIO_2VIA_SMS { get; private set; }
        public string MOTIVO_CLIENTE_INGRESSO_ENDERECO_POSTAL { get; private set; }
        public string MOTIVO_EMPRESA_INGRESSO_ENDERECO_POSTAL { get; private set; }
        public string MOTIVO_CLIENTE_CONSULTA_OPCAO_PARCELAMENTO { get; private set; }
        public string MOTIVO_EMPRESA_CONSULTA_OPCAO_PARCELAMENTO { get; private set; }
        public string MOTIVO_CLIENTE_SOLICITACAO_PARCELAMENTO { get; private set; }
        public string MOTIVO_EMPRESA_SOLICITACAO_PARCELAMENTO { get; private set; }
        public string MOTIVO_CLIENTE_SEGUNDA_VIA_PARCELAMENTO { get; private set; }
        public string MOTIVO_EMPRESA_SEGUNDA_VIA_PARCELAMENTO { get; private set; }
        public string MOTIVO_CLIENTE_CONSULTA_ENDERECO_POSTAL { get; private set; }
        public string MOTIVO_EMPRESA_CONSULTA_ENDERECO_POSTAL { get; private set; }
        public string MOTIVO_CLIENTE_EXCLUSAO_ENDERECO_POSTAL { get; private set; }
        public string MOTIVO_EMPRESA_EXCLUSAO_ENDERECO_POSTAL { get; private set; }
        public string MOTIVO_CLIENTE_CONSULTA_DATA_VENCIMENTO { get; private set; }
        public string MOTIVO_EMPRESA_CONSULTA_DATA_VENCIMENTO { get; private set; }
        public string MOTIVO_CLIENTE_INFORMACAO_DESLIGAMENTO_PROGRAMADO { get; private set; }
        public string MOTIVO_EMPRESA_INFORMACAO_DESLIGAMENTO_PROGRAMADO { get; private set; }
        public string TIPO_INGRESSO_EMERGENCIA_INDIVIDUAL { get; private set; }
        public string TIPO_INGRESSO_EMERGENCIA_LOCALIDADE { get; private set; }

        public string MOTIVO_CLIENTE_SOLICITACAO_RELIGACAO_URGENTE_GA { get; private set; }
        public string MOTIVO_EMPRESA_SOLICITACAO_RELIGACAO_URGENTE_GA { get; private set; }

        public string MOTIVO_CLIENTE_SOLICITACAO_RELIGACAO_NORMAL_GA { get; private set; }
        public string MOTIVO_EMPRESA_SOLICITACAO_RELIGACAO_NORMAL_GA { get; private set; }

        public string MOTIVO_CLIENTE_SOLICITACAO_MUDANCA_MEDIDOR_LOCAL { get; private set; }
        public string MOTIVO_EMPRESA_SOLICITACAO_MUDANCA_MEDIDOR_LOCAL { get; private set; }

        public string MOTIVO_CLIENTE_CONSULTA_MUDANCA_MEDIDOR_LOCAL { get; private set; }
        public string MOTIVO_EMPRESA_CONSULTA_MUDANCA_MEDIDOR_LOCAL { get; private set; }
        
        public string MOTIVO_CLIENTE_CONSULTA_PROTOCOLO { get; private set; }
        public string MOTIVO_EMPRESA_CONSULTA_PROTOCOLO { get; private set; }

        public string MOTIVO_CLIENTE_CONSULTA_ORDEM { get; private set; }
        public string MOTIVO_EMPRESA_CONSULTA_ORDEM { get; private set; }

      
        public string MOTIVO_CLIENTE_OUVIDORIA_GA { get; private set; }
        public string MOTIVO_EMPRESA_OUVIDORIA_GA { get; private set; }

        // CARLOS INDRA
        public string MOTIVO_CLIENTE_CONSULTA_HISTORICO_PAGAMENTO { get; private set; }
        public string MOTIVO_EMPRESA_CONSULTA_HISTORICO_PAGAMENTO { get; private set; }

        public string MOTIVO_CLIENTE_SOL_INCLUSAO_EXCLUSAO_DE_DEBITO_AUTOMA { get; private set; }
        public string MOTIVO_EMPRESA_SOL_INCLUSAO_EXCLUSAO_DE_DEBITO_AUTOMA { get; private set; }

        public string INF_CLIENTE_DEBITO_AUTOMATICO { get; private set; }
        public string INF_EMPRESA_DEBITO_AUTOMATICO { get; private set; }

        public string INF_CLIENTE_CONSUMO { get; private set; }
        public string INF_EMPRESA_CONSUMO { get; private set; }


        //Lavor
        //public string MOTIVO_CLIENTE_COMUNICACAO_CONTA_PAGA { get; private set; }
        //public string MOTIVO_EMPRESA_COMUNICACAO_CONTA_PAGA { get; private set; }


    }
}

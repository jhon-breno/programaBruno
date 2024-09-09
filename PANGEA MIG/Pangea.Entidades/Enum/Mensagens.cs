using System.Configuration;
using System.Reflection;

namespace Pangea.Entidades.Enumeracao
{
    public class Mensagens
    {
        private static Mensagens mensagens;
        public static Mensagens getInstance()
        {
            if (mensagens == null)
            {
                mensagens = new Mensagens();
            }

            return mensagens;
        }

        private Mensagens()
        {
            PropertyInfo[] props = typeof(Mensagens).GetProperties();
            foreach (PropertyInfo prop in props)
            {
                string tempConfig = ConfigurationManager.AppSettings[prop.Name];

                if (string.IsNullOrEmpty(tempConfig))
                {
                    tempConfig = "NAO_CONFIGURADO";
                }

                prop.SetValue(this, tempConfig, null);
            }
        }

        #region MSG Operacao Invalida
        public string TIPO_DE_CONSULTA_NAO_DISPONIVEL { get; private set; }
        public string NAO_PODE_CADASTRAR_UM_TELEFONE_COM_TIPO_IGUAL_A_TODOS { get; private set; }
        public string TELEFONE_JA_CADASTRADO { get; private set; }
        public string TIPO_DE_TELEFONE_JA_CADASTRADO { get; private set; }
        public string PARA_CADASTRAR_UM_DOCUMENTO_O_TIPO_NAO_PODE_SER_TODOS { get; private set; }
        public string JA_EXISTE_ESTE_TIPO_DE_DOCUMENTO_CADASTRADO_PARA_ESTE_CLIENTE { get; private set; }
        public string PONTO_DE_REFERENCIA_JA_CADASTRADA { get; private set; }
        public string CEP_JA_CADASTRADO { get; private set; }
        public string A_DATA_DE_VENCIMENTO_INFORMADO_E_A_MESMA_CADASTRADA { get; private set; }
        public string DATA_DE_VENCIMENTO_NAO_DISPONIVEL_PARA_O_CLIENTE { get; private set; }
        public string CLIENTE_POSSUI_PARCELAMENTO_SOLICITADO_COM_DATA_DE_CONTRATACAO_SUPERIOR { get; private set; }
        public string NAO_HA_CONFIRMACAO_DO_PAGAMENTO_DO_QIP { get; private set; }
        public string CLIENTE_POSSUI_MAIS_DE_UMA_ALEGACAO_DE_FALSO_PAGAMENTO { get; private set; }
        public string CLIENTE_DESPERSONALIZADO { get; private set; }
        public string CLIENTE_POSSUI_SOLICITACOES_DE_RELIGACAO_PENDENTES { get; private set; }
        public string CLIENTE_NAO_POSSUI_CPF_CADASTRADO_NO_SISTEMA { get; private set; }
        public string CLIENTE_NAO_POSSUI_MEDIDORES_INSTALADOS { get; private set; }
        public string CLIENTE_POSSUI_CORTE_DE_DESLIGAMENTO_PEDIDO { get; private set; }
        public string CLIENTE_POSSUI_OUTRA_UC_COM_DEBITO { get; private set; }
        public string CLIENTE_NAO_POSSUI_DEBITO_NESTA_UC { get; private set; }
        public string CLIENTE_POSSUI_MARCACAO_DE_FALSO_PAGAMENTO { get; private set; }
        public string CLIENTE_SEM_FORNECIMENTO { get; private set; }
        public string PROTOCOLO_INEXISTENTE_PARA_O_CLIENTE { get; private set; }
        public string CLIENTE_POSSUI_REAVISO { get; private set; }
        public string ESTADO_DO_CLIENTE_DIFERENTE_DE_ATIVO { get; private set; }
        public string CLIENTE_NAO_ESTA_ATIVO { get; private set; }
        public string CLIENTE_COM_FORNECIMENTO_SUSPENSO_POR_DEBITO { get; private set; }
        public string CLIENTE_RETIRADO { get; private set; }
        public string CLIENTE_EM_PROCESSO_DE_FATURAMENTO { get; private set; }
        public string A_RELIGACAO_FOI_FINALIZADA_NO_PRAZO { get; private set; }
        public string CLIENTE_EM_LOCALIDADE_DE_DESLIGAMENTO_PROGRAMADO { get; private set; }
        public string CLIENTE_VITAL { get; private set; }
        public string CLIENTE_COM_ALIMENTADOR_DESARMADO { get; private set; }
        public string CLIENTE_COM_TROCA_DE_TITULARIDADE_EM_ABERTO { get; private set; }
        public string SENHA_JA_CADASTRADA { get; private set; }
        public string CLIENTE_GRUPO_A { get; set; }
        public string CLIENTE_ESTADO_INVALIDO { get; set; }
        public string NAO_FOI_POSSIVEL_INSERIR_AS_FATURAS_DO_CONVENIO { get; set; }
        public string NAO_FOI_POSSIVEL_INSERIR_OS_PERCENTUIAS_DOS_ENCARGOS { get; set; }
        public string NAO_FOI_POSSIVEL_ATUALIZAR_O_CORRELATIVO_DE_PARCELAMENTO_DO_CLIENTE { get; set; }
        public string VALOR_DA_PORCENTAGEM_DOS_ENCARGOS_DIFERENTE_100 { get; set; }
        public string VALOR_DA_DIVIDA_DIFERENTE_DA_SOMA_DE_ENCARGOS { get; set; }
        public string NAO_FOI_POSSIVEL_RECUPERAR_OS_ENCARGOS_DO_PARCELAMENTO { get; set; }
        public string NAO_FOI_POSSIVEL_INSERIR_O_PARCELAMENTO { get; set; }
        public string SEM_OPCOAO_DE_INTERESSE_CADASTRADA { get; set; }
        public string CLIENTE_SEM_PERFIL_DE_PARCELAMENTO { get; set; }
        public string CLIENTE_POSSUI_TOI_OU_RO_PENDENTE { get; set; }
        public string NAO_FOI_POSSIVEL_RETORNAR_DADOS_DO_WS_DA_TECNICA { get; set; }
        public string NAO_FOI_POSSIVEL_RECUPERAR_DADOS_DA_TECNICA { get; set; }
        public string PROTOCOLO_RECLAMADO_INVALIDO { get; set; }
        public string PROTOCOLO_RECLAMADO_NAO_POSSUI_CLIENTE_ASSOCIADO { get; set; }
        public string NAO_PODE_RECUPERAR_DATAS_CLIENTE { get; set; }
        public string NAO_ESTA_NA_DATA_LEITURA { get; set; }
        public string NAO_PODE_INFORMAR_LEITURA_NESTE_CICLO_FATURAMENTO { get; set; }
        public string NAO_PODE_RECUPERAR_INFORMACAO_MEDIDOR_CLIENTE { get; set; }
        public string NUMERO_INFORMADO_INVALIDO_COMO_CPF_NUMEROCLIENTE { get; set; }
        public string CLIENTE_COM_PARCELAMENTO_DE_DEBITO_EM_ESTADO_VIGENTE { get; set; }
        public string CLIENTE_COM_PARCELAMENTO_DE_DEBITO_EM_ESTADO_SOLICITADO { get; set; }
        public string VALOR_DA_DIVIDA_SUPERIOR_AO_MAXIMO_PRE_APROVADO { get; set; }
        public string CLIENTE_NAO_POSSUI_FATURAS_VENCIDAS_COM_DATA_SUPERIOR_PARA_PARCELAMENTO { get; set; }
        public string CLIENTE_POSSUI_MUITAS_SOLICITACOES_DE_PARCELAMENTO_EM_UM_PERIODO { get; set; }
        public string RELIGACAO_HA_MENOS_DE_48_HORAS { get; set; }
        public string APENAS_UM_CLIENTE_COM_ESTE_CPF { get; set; }
        public string NECESSARIO_MOSTRAR_COMPROVANTE { get; set; }
        public string DATA_PAGAMENTO_MAIOR_QUE_DATA_ATUAL { get; set; }
        public string NAO_INFORMADO_TODOS_CORRELATIVOS_REAVISADOS { get; set; }
        public string CLIENTE_COM_FORNECIMENTO { get; set; }
        public string INGRESSO_RELIGACAO_NAO_VALIDADO { get; set; }
        public string CLIENTE_COM_INCIDENCIA_NAO_PROGRAMADA { get; set; }

        public string NAO_E_PERMITIDO_INGRESSAR_MAIS_DE_UMA_ORDEM_POR_CLIENTE { get; set; }
        public string CLIENTE_POSSUI_DEBITOS_VENCIDOS { get; set; }
        public string CLIENTE_POSSUI_ORDEM_ATIVA_PARA_SERVICO_MUDANCA_DE_MEDIDOR_LOCAL { get; set; }
        public string INGRESSO_MUDANCA_MEDIDOR_LOCAL_NAO_VALIDADO { get; set; }

        public string AGENDAMENTO_NAO_VALIDADO_NAO_REALIZADO { get; set; }
        public string ORDEM_SERVICO_INVALIDA_OU_INEXISTENTE { get; set; }
        
        

        #endregion

        #region MSG Sucesso
        public string APELIDO_ATUALIZADO_COM_SUCESSO { get; private set; }
        public string TELEFONE_CADASTRADO_COM_SUCESSO { get; private set; }
        public string REMOVIDO_ASSOCIACOES_COM_SUCESSO { get; private set; }
        public string MODIFICACAO_DO_MARCA_SMS_REALIZADO_COM_SUCESSO { get; private set; }
        public string EMAIL_ATUALIZADO_COM_SUCESSO { get; private set; }
        public string MODIFICACAO_NA_AUTORIZACAO_DE_ENVIAR_EMAIL_ATUALIZADO_COM_SUCESSO { get; private set; }
        public string DOCUMENTO_INSERIDO_COM_SUCESSO { get; private set; }
        public string DOCUMENTO_ATUALIZADO_COM_SUCESSO { get; private set; }
        public string DATA_DE_VENCIMENTO_ATUALIZADO_COM_SUCESSO { get; private set; }
        public string PONTO_DE_REFERENCIA_ATUALIZADO_COM_SUCESSO { get; private set; }
        public string CEP_ATUALIZADO_COM_SUCESSO { get; private set; }
        public string CLIENTE_COM_DEBITOS_VENCIDOS { get; private set; }
        public string SUSPENSAO_DE_CORTE_INSERIDO_COM_SUCESSO { get; private set; }
        public string DATA_DE_CORTE_SUPERIOR { get; private set; }
        public string SEM_AVISO_DE_CORTE_NAO_ESTA_APTO_A_CORTE { get; private set; }
        public string CLIENTE_COM_TOI { get; private set; }
        public string A_NOVA_DATA_DE_VENCIMENTO_SO_ESTARA_DISPONIVEL_NA_FATURA_SUBSEQUENTE { get; private set; }
        public string PROBLEMAS_NA_LOCALIDADE { get; private set; }
        public string SEM_PROBLEMAS_NA_LOCALIDADE { get; private set; }
        public string ENDERECO_POSTAL_INSERIDO_COM_SUCESSO { get; private set; }
        public string ENDERECO_POSTAL_ATUALIZADO_COM_SUCESSO { get; private set; }
        public string ENDERECO_POSTAL_DELETADO_COM_SUCESSO { get; private set; }
        public string CLIENTE_VALIDADO_COM_SUCESSO_CONFIRMAR_RELIGACAO_AUTORIZADA { get; private set; }
        public string RELIGACAO_INSERIDA_COM_SUCESSO { get; private set; }
        public string LEITURA_INSERIDA_PENDENTE_ANALISE { get; set; }
        public string SUCESSO_SOLICITACAO_EMERGENCIA { get; set; }
        public string SUCESSO_RECLAMACAO_EMERGENCIA { get; set; }
        public string SUCESSO_CONSULTA_EMERGENCIA { get; set; }
        public string MARCACAO_NOTICIA_EMAIL_ATUALIZADO_COM_SUCESSO { get; set; }
        public string CLIENTE_COM_AVISO_DENTRO_DO_PRAZO { get; set; }
        public string CLIENTE_COM_AVISO_FORA_DO_PRAZO { get; set; }
        public string SUCESSO_NA_SOLICITACAO_DE_PARCELAMENTO { get; set; }


        public string DIAS_DE_AGENDAMENTO_DISPONIVEIS { get; set; }
        public string AGENDAMENTO_CANCELADO_COM_SUCESSO { get; set; }
        public string PERMITIDO_INGRESSO_DE_ORDEM_PARA_O_CLIENTE { get; set; }
        public string ENDERECO_CROQUI_GERADO_COM_SUCESSO { get; set; }
        public string MUDANCA_DE_MEDIDOR_LOCAL_INSERIDA_COM_SUCESSO { get; set; }
        public string CLIENTE_VALIDADO_COM_SUCESSO_CONFIRMAR_MUDANCA_MEDIDOR_LOCAL_AUTORIZADA { get; set; }
        public string TAXA_OBTIDA_COM_SUCESSO { get; set; }
        public string CON_PROTOCOLO_COM_SUCESSO { get; set; }

        public string SUCESSO_NA_SOLICITACAO_DE_EXCLUSAO_DE_DEBITO_AUTOMATICO { get; set; }
        public string SUCESSO_NA_SOLICITACAO_DE_DEBITO_AUTOMATICO { get; set; }
        public string CON_ORDEM_COM_SUCESSO { get; set; }
        
                
        #endregion

        #region MSG Nenhum Resultado
        public string NUMERO_CLIENTE_NAO_ENCONTRADO { get; private set; }
        public string MODIFICACAO_NA_AUTORIZACAO_DE_ENVIAR_EMAIL_IDENTICA_A_ANTERIOR { get; private set; }
        public string CLIENTE_SEM_DEBITO_VENCIDO { get; private set; }
        public string CLIENTE_NAO_POSSUI_DEBITOS { get; private set; }
        public string NAO_EXISTE_DATAS_DE_VENCIMENTO_PARA_ESTE_CLIENTE { get; private set; }
        public string NENHUMA_LOJA_COM_ENDERECO_DISPONIVEL { get; private set; }
        public string O_CLIENTE_NAO_TEM_ENDERECO_POSTAL_CADASTRADO { get; private set; }
        public string CLIENTE_INVALIDO { get; private set; }
        public string AGENCIA_NAO_ENCONTRADA { get; set; }
        public string CLIENTE_POSSUI_UMA_OU_NENHUMA_FATURA_VENCIDA { get; set; }
        public string OPCAO_NAO_EXISTE_FAVOR_CONSULTAR_NOVAMENTE_AS_OPCOES { get; set; }
        public string NAO_POSSUI_DADOS_DESTE_PARCELAMENTO { get; set; }
        public string NENHUMA_FATURA_ENCONTRADA { get; set; }
        public string CLIENTE_SEM_PROBLEMAS_E_SEM_PRAZO { get; set; }
        public string PROTOCOLO_RECLAMADO_NAO_ENCONTRADO { get; set; }
        public string CLIENTE_NAO_POSSUI_EMAIL_CADASTRADO { get; set; }
        public string NAO_EXISTE_CORRELATIVOS_PARA_REFERENCIAS_INFORMADA { get; set; }
        public string SEM_USUARIO_CADASTRADO { get; set; }
        public string PROTOCOLO_NAO_ENCONTRADO { get; set; }
		public string CLIENTE_NAO_POSSUI_HISTORICO_PAGAMENTO { get; set; } //CARLOS INDRA
        #endregion

        #region MSG Erro
        public string ERRO_AO_TENTAR_INSERIR_NA_MODIF { get; private set; }
        public string ERRO_AO_TENTAR_ATUALIZAR_O_APELIDO_DO_CLIENTE { get; private set; }
        public string ERRO_AO_TENTAR_CADASTRAR_O_TELEFONE_DO_CLIENTE { get; private set; }
        public string ERRO_AO_TENTAR_DELETAR_O_TELEFONE_DO_CLIENTE { get; private set; }
        public string ERRO_AO_TENTAR_MODIFICAR_O_MARCA_SMS { get; private set; }
        public string ERRO_AO_TENTAR_ATUALIZAR_O_EMAIL_DO_CLIENTE { get; private set; }
        public string ERRO_AO_TENTAR_INSERIR_DOCUMENTO_DO_CLIENTE { get; private set; }
        public string ERRO_AO_TENTAR_ATUALIZAR_DOCUMENTO_DO_CLIENTE { get; private set; }
        public string ERRO_AO_TENTAR_ATUALIZAR_A_DATA_DE_VENCIMENTO_DO_CLIENTE { get; private set; }
        public string ERRO_AO_TENTAR_ATUALIZAR_PONTO_DE_REFERENCIA_DO_CLIENTE { get; private set; }
        public string ERRO_AO_TENTAR_ATUALIZAR_CEP_DO_CLIENTE { get; private set; }
        public string NAO_FOI_POSSIVEL_INGRESSAR_COM_A_RESTRICAO_DE_RELIGACAO { get; private set; }
        public string NAO_FOI_POSSIVEL_ATUALIZAR_DATA_DE_CORTE { get; private set; }
        public string NAO_FOI_POSSIVEL_INGRESSAR_A_SUSPENSAO_DE_CORTE { get; private set; }
        public string ERRO_AO_TENTAR_ATUALIZAR_CLIENTE_CAD_POSTAL { get; private set; }
        public string ERRO_AO_TENTAR_INSERIR_NA_CAD_POSTAL { get; private set; }
        public string ERRO_AO_TENTAR_ATUALIZAR_NA_CAD_POSTAL { get; private set; }
        public string ERRO_AO_TENTAR_DELETADO_NA_CAD_POSTAL { get; private set; }
        public string NAO_FOI_POSSIVEL_INSERIR_A_RELIGACAO { get; private set; }
        public string NAO_FOI_POSSIVEL_INSERIR_A_ORDEM_DE_SERVICO { get; private set; }
        public string A_CHAMADA_NAO_PODE_SER_INSERIDA { get; private set; }
        public string ERRO_AO_GERAR_PREATEND_PAINEL { get; private set; }
        public string NAO_FOI_POSSIVEL_CANCELAR_OS_PARCELAMENTOS { get; private set; }
        public string NAO_FOI_POSSIVEL_ATUALIZAR_O_PARCELAMENTO { get; private set; }
        public string LEITURA_NAO_INSERIDA { get; set; }
        public string NAO_FOI_POSSIVEL_ENTREGAR_TODOS_EMAILS { get; set; }
        public string NAO_FOI_POSSIVEL_INGRESSAR_O_MOTIVO { get; set; }
        public string NAO_FOI_POSSIVEL_RECUPERAR_O_ATENDIMENTO { get; set; }
        public string NAO_FOI_POSSIVEL_COMPLETAR_A_OPERACAO_DE_AGRUPAMENTO_COM_SUCESSO { get; set; }
        public string NAO_FOI_POSSIVEL_INSERIR_A_OBSERVACAO_ATENDIMENTO { get; set; }

        public string NAO_FOI_POSSIVEL_ACHAR_DIAS_DISPONIVEIS_PARA_AGENDAMENTO { get; set; }
        public string ERRO_AO_TENTAR_INSERIR_AGENDAMENTO { get; set; }
        public string ERRO_AO_TENTAR_CANCELAR_AGENDAMENTO { get; set; }
        public string ERRO_AO_TENTAR_CRIAR_ENDERECO_CROQUI { get; set; }
        public string TAXA_NAO_CONFIGURADA_PARA_O_SERVICO { get; set; }
        public string ERRO_AO_TENTAR_DESPACHAR_ORDEM { get; set; }
        public string ERRO_AO_TENTAR_SUSPENDER_ORDEM { get; set; }
        public string ERRO_ACESSO_A_BASE_DADOS_INDISPONIVEL { get; set; }
        public string ERRO_NA_VALIDACAO_CLIENTE_GRUPO_A { get; set; }
        public string ERRO_NO_INGRESSO_ATENDIMENTO_EMERGENCIA_GRUPO_A { get; set; }

        public string NAO_FOI_POSSIVEL_EXCLUIR_DEBITO_AUTOMATICO { get; set; }
        public string NAO_FOI_POSSIVEL_INSERIR_OU_ATUALIZAR_DEBITO_AUTOMATICO { get; set; }

        public string ERRO_AO_TENTAR_EXECUTAR_O_PROCEDIMENTO { get; set; }

        public string ERRO_AO_TENTAR_ATUALIZAR_CLIENTE_FATURA_POR_EMAIL { get; set; }
        #endregion

        #region MSG Mais de Um Registro
        public string EXISTE_MAIS_DE_UM_CLIENTE { get; private set; }
        #endregion
    }
}

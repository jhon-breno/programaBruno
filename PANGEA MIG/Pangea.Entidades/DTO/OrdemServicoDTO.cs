using Pangea.Entidades.Base;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;

namespace Pangea.Entidades.DTO
{
    [DisplayName("ordem_servico")]
    public class OrdemServicoDTO : EntidadeBase
    {
        public int CodEmpresa { get; set; }
        public string numero_ordem { get; set; }
        public string tipo_ordem { get; set; }
        public string tipo_servico { get; set; }
        public string numero_cliente { get; set; }
        public DateTime data_ingresso { get; set; }
        public string estado { get; set; }
        public string estadoNot { get; set; }
        public DateTime data_estado { get; set; }
        public string etapa { get; set; }
        public DateTime data_etapa { get; set; }
        public string municipio { get; set; }
        public string sucursal_origem { get; set; }
        public string sucursal_original { get; set; }
        public string area_origem { get; set; }
        public string sucursal_destino { get; set; }
        public string regional_destino { get; set; }
        public string area_destino { get; set; }
        public int corr_visita { get; set; }
        public int corr_pendencia { get; set; }
        public string rol_ingresso { get; set; }
        public string rol_alteracao { get; set; }
        public string cod_mot_anulacao { get; set; }
        public string rol_anulacao { get; set; }
        public string nro_caso { get; set; }
        public int nro_gac { get; set; }
        public string ind_enviado_tec { get; set; }
        public string numero_ordem_relac { get; set; }
        public string numero_segen { get; set; }
        public string motivo_ingresso { get; set; }
        public string observacoes { get; set; }
        public string telefone_contato { get; set; }
        public string referencia { get; set; }
        public int tempo_def_tecnico { get; set; }
        public int tempo_def_tec_etap { get; set; }
        public DateTime data_cont_def_tec { get; set; }
        public string codigo_area { get; set; }


        #region Aparelho ordem

        [Required(ErrorMessage = "Nome do solicitante deve ser informado.")]
        [MaxLength(40, ErrorMessage = "O nome do solicitante deve conter  no máximo{1}.")]
        public string solic_nome { get; set; }

        [Required(ErrorMessage = "CPF/CNPJ do solicitante deve ser informado.")]
        [MaxLength(18, ErrorMessage = "Quantidade de dígitos informado está maior que o obrigatório.")]
        public string solic_cpf { get; set; }

        [Required(ErrorMessage = "RG do solicitante deve ser informado.")]
        [MaxLength(15, ErrorMessage = "Quantidade de dígitos informado está maior que o obrigatório.")]
        public string solic_rg { get; set; }

        public string solic_fax { get; set; }
        public string email { get; set; }
        public string pagto_forma { get; set; }
        public string pagto_banco { get; set; }
        public string pagto_agencia { get; set; }
        public string pagto_conta { get; set; }

        [MaxLength(255, ErrorMessage = "A descrição da ocorrência deve conter no máximo {1} caracteres.")]
        public string ocorrencia_desc { get; set; }

        [Required(ErrorMessage = "Data da ocorrência deve ser informada.")]
        [DataType(DataType.DateTime, ErrorMessage = "Data da Ocorrência inválida.")]
        public DateTime? ocorrencia_data { get; set; }

        [Required(ErrorMessage = "Hora da ocorrência deve ser informada corretamente.")]
        [DataType(DataType.Time, ErrorMessage = "Hora da ocorrência inválida.")]
        public DateTime? ocorrencia_hora { get; set; }

        public string ind_coelceavisada { get; set; }
        public string ind_houveatendimen { get; set; }
        public string ind_outrosclientes { get; set; }
        public string ind_oscilacaotensa { get; set; }

        [Required(ErrorMessage = "Número do aviso deve ser informado.")]
        [MaxLength(12, ErrorMessage = "O número do aviso deve conter no máximo {1} caracteres.")]
        public string nro_aviso { get; set; }

        [Required(ErrorMessage = "Data da vistoria deve ser informada.")]
        [DataType(DataType.Date, ErrorMessage = "Data da vistoria inválida!")]
        public DateTime? data_vistoria { get; set; }

        [Required(ErrorMessage = "Turno da vistoria deve ser informado.")]
        public string turno_vistoria { get; set; }

        [Required(ErrorMessage = "Tipo de documento deve ser informado.")]
        [MaxLength(3, ErrorMessage = "Tipo do documento deve conter no máximo {1} caracteres")]
        public string tipo_doc_apresent { get; set; }

        [Required(ErrorMessage = "Número do documento deve ser informado.")]
        public string nro_doc_apresent { get; set; }

        [Required(ErrorMessage = "Data de emissão deve ser informada.")]
        [DataType(DataType.Date, ErrorMessage = "Data de emissão do documento inválida.")]
        public DateTime? data_doc_apresent { get; set; }

        [Required(ErrorMessage = "Informe o valor coabrado.")]
        [Range(1, double.MaxValue, ErrorMessage = "O valor deve ser maior que 0")]
        public double? valor_cobrado { get; set; }

        //[Required(ErrorMessage = "Hora de agendamento deve ser informado corretamente.")]
        [DataType(DataType.Time, ErrorMessage = "Hora de agendamento inválida.")]
        public DateTime? hora_agendto { get; set; }
        #endregion


        #region Aparelho queimado

        [Required(ErrorMessage = "Correlativo do Aparelho/Bem danificado deve ser informado.")]
        public Int16? corr_aparelho { get; set; }

        [Required(ErrorMessage = "Código do Aparelho/Bem danificado deve ser informado.")]
        [MaxLength(3, ErrorMessage = "O código de aterfato deve conter no máximo {1} caracteres")]
        public string cod_artefacto { get; set; }

        [Required(ErrorMessage = "Marca/Bem danificado deve ser informado.")]
        public string marca { get; set; }

        [Required(ErrorMessage = "Informe Ano de Aquisição com quatro digitos.")]
        public Int16? anofabricacao { get; set; }

        [Required(ErrorMessage = "Digite valor orçado do equipamento.")]
        public double? valororcado { get; set; }

        [Required(ErrorMessage = "Informe a quantidade de equipamentos.")]
        [Range(1, double.MaxValue, ErrorMessage = "A quantidade orçada deve ser maior que 0")]
        public double? qtdorcada { get; set; }

        #endregion


    }
}

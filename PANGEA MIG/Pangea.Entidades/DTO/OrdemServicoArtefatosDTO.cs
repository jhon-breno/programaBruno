using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pangea.Entidades.DTO
{
    public class OrdemServicoArtefatosDTO
    {
        [Required(ErrorMessage = "Informe a empresa.")]
        public int? CodEmpresa { get; set; }


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


        #region Ordem serviço

        [Required(ErrorMessage = "Informe o correlativo visita.")]
        public Int16? corr_visita { get; set; }

        [Required(ErrorMessage = "Informe o destino regional.")]
        [MaxLength(2, ErrorMessage = "Tamanho máximo permitido para o Regional destino {1} caracteres.")]
        public string regional_destino { get; set; }

        [Required(ErrorMessage = "Informe o sucursal de destino.")]
        [MaxLength(4, ErrorMessage = "Tamanho máximo permitido para o sucursal destino {1} caracteres.")]
        public string sucursal_destino { get; set; }

        [Required(ErrorMessage = "Informe a data estado.")]
        [DataType(DataType.DateTime, ErrorMessage = "Data do estado inválida.")]
        public DateTime? data_estado { get; set; }

        [Required(ErrorMessage = "Informe a estado.")]
        [MaxLength(2, ErrorMessage = "Tamanho máximo permitido para o Estado {1} caracteres.")]
        public string estado { get; set; }

        [Required(ErrorMessage = "Informe a data de ingresso.")]
        [DataType(DataType.DateTime, ErrorMessage = "Data do estado inválida.")]
        public DateTime? data_ingresso { get; set; }

        [Required(ErrorMessage = "Informe a data de etapa.")]
        [DataType(DataType.DateTime, ErrorMessage = "Data de etapa inválida.")]
        public DateTime? data_etapa { get; set; }

        [Required(ErrorMessage = "Informe o sucursal de origem.")]
        [MaxLength(4, ErrorMessage = "Tamanho máximo permitido do sucursal de origem {1} caracteres.")]
        public string sucursal_origem { get; set; }

        //[Required(ErrorMessage = "Informe o número da ordem.")]
        //[MaxLength(10, ErrorMessage = "O número do documeto deve conter no máximo {1} caracteres.")]
        public string numero_ordem { get; set; }

        [Required(ErrorMessage = "Informe o tipo da ordem.")]
        [MaxLength(3, ErrorMessage = "Tipo da ordem deve conter no máximo {1} caracteres.")]
        public string tipo_ordem { get; set; }

        [Required(ErrorMessage = "Informe o tipo do serviço.")]
        [MaxLength(3, ErrorMessage = "Tipo serviço deve conter no máximo {1} caracteres.")]
        public string tipo_servico { get; set; }

        [Required(ErrorMessage = "Informe o número do cliente.")]
        public int? numero_cliente { get; set; }

        [Required(ErrorMessage = "Informe o telefone de contato.")]
        [MaxLength(10, ErrorMessage = "O número do telefone deve conter no máximo {1} caracteres.")]
        public string telefone_contato { get; set; }

        public string Rol_Ingresso { get; set; }

        [MaxLength(255, ErrorMessage = "A referência deve conter no máximo {1} caracteres.")]
        public string referencia { get; set; }

        [MaxLength(255, ErrorMessage = "Observações deve conter no máximo {1} caracteres.")]
        public string observacoes { get; set; }

        [Required(ErrorMessage = "Informe o a etapa.")]
        [MaxLength(3, ErrorMessage = "Tamanho máximo permitido para Etapa {0} caracteres.")]
        public string etapa { get; set; }

        [Required(ErrorMessage = "Informe a área de origem.")]
        [MaxLength(4, ErrorMessage = "Tamanho máximo permitido para Área origem {1} caracteres.")]
        public string area_origem { get; set; }

        [Required(ErrorMessage = "Informe o número do Gac.")]
        [MaxLength(10, ErrorMessage = "O número do Gac não pode ser 0")]
        public string nro_gac { get; set; }

        public string Codigo_Area { get; set; }
        public DateTime data_cont_def_tec { get; set; }

        [Required(ErrorMessage = "Área destino deve ser informada.")]
        [MaxLength(4, ErrorMessage = "Tamanho máximo permitido para a área destino {1} caracteres.")]
        public string area_destino { get; set; }

        [MaxLength(4, ErrorMessage = "Tamanho máximo permitido para o sucursal destino {1} caracteres.")]
        public string sucursal_original { get; set; }

        [MaxLength(5, ErrorMessage = "Tamanho máximo permitido para o motivo do ingresso {1} caracteres.")]
        public string motivo_ingresso { get; set; }

        [MaxLength(10, ErrorMessage = "Tamanho máximo permitido para o número de ordem relac {1} caracteres.")]
        public string numero_ordem_relac { get; set; }

        [MaxLength(4, ErrorMessage = "Tamanho máximo permitido para o município {1} caracteres.")]
        public string municipio { get; set; }

        #endregion

    }
}

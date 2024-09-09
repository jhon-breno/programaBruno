using Pangea.Entidades.DTO;
using Pangea.Entidades.Enumeracao;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Pangea.Entidades
{

    public class ContratoDTO
    {
        public ContratoDTO()
        {
            Type t = typeof(ContratoDTO);
            foreach (PropertyInfo pi in t.GetProperties())
            {
                if (pi.PropertyType.Name.ToUpper().Equals("STRING"))
                    pi.SetValue(this, "NULL", null);
            }
            if (!string.IsNullOrWhiteSpace(this.nome))
                this.nombre = this.nome;


        }

        //[EnumDataType(typeof(InterfacesContrato), ErrorMessage = "Deve ser informado um valor válido para {0}.")]
        public InterfacesContrato interface_contrato { get; set; }
        //Dados para Atualizar Telefone 
        public List<TelefoneDTO> telefones { get; set; }
        //Dados para Atualizar Documentos do Cliente 
        public List<DocumentoDTO> documentos { get; set; }

        public List<QuestoesDTO> questoes { get; set; }

        //[EnumDataType(typeof(Empresa), ErrorMessage = "Deve ser informado um valor válido para {0}.")]
        public string codigo_empresa { get; set; }

        public string ddd { get; set; }
        public string ddd1 { get; set; }
        public string ddd2 { get; set; }
        public string nome { get; set; }
        public string cod_doc { get; set; }
        public string num_doc { get; set; }
        public string dv_doc { get; set; }
        public string municipio { get; set; }
        public string bairro { get; set; }
        public string cod_logra { get; set; }
        public string nome_logra { get; set; }
        public string num_imovel { get; set; }
        public string complemento { get; set; }
        public string mail { get; set; }
        public string bairro_postal { get; set; }
        public string municipio_postal { get; set; }
        public string estado { get; set; }
        public string cep_postal { get; set; }
        public string cobro_postal { get; set; }
        public string direccion_postal { get; set; }
        public string tipo_ident2 { get; set; }
        public string tipo_pessoa { get; set; }
        public string ramal { get; set; }
        public string ramal2 { get; set; }
        public string latitudeEndVizinho { get; set; }
        public string longitudeEndVizinho { get; set; }
        public string documento2 { get; set; }
        public string dv_docu2 { get; set; }
        public string cadastro_br { get; set; }
        public string proprio_br { get; set; }
        public string nome_benef_br { get; set; }
        public string grau_parent_br { get; set; }
        public string numero_nis { get; set; }
        public string numero_nb { get; set; }
        public string numero_nit { get; set; }
        public string uf_nasc_br { get; set; }
        public string data_nasc_br { get; set; }
        public string tipo_br { get; set; }
        public string tipo_ident_br { get; set; }
        public string rut_br { get; set; }
        public string dv_rut_br { get; set; }
        public string tipo_ident2_br { get; set; }
        public string rut2_br { get; set; }
        public string dv_rut2_br { get; set; }
        public string giro { get; set; }
        public string tipo_ligacao { get; set; }
        public string classe { get; set; }
        public string subclasse { get; set; }
        public string tarifa { get; set; }
        public string estado_civil { get; set; }
        public string ind_cad_conjuge { get; set; }
        public string nome_conjuge { get; set; }
        public string tipo_doc_conjuge { get; set; }
        public string documento_conjuge { get; set; }
        public string dv_documento_conjuge { get; set; }
        public string ind_aut_email { get; set; }

        //[RegularExpression("^(?!NULL$).*$", ErrorMessage = "{0} deve ser informado.")]
        public string sucursal { get; set; }

        //[RegularExpression("^(?!NULL$).*$", ErrorMessage = "{0} deve ser informado.")]
        public string numero_cliente { get; set; }

        public string numero_cliente_novo { get; set; }
        public string dv_numero_cliente { get; set; }
        public string numero_ordem { get; set; }
        public string tipo_contrato { get; set; }
        public string numero_contrato { get; set; }
        public string fecha_contrato { get; set; }
        public string nombre { get; set; }
        public string nome_mae { get; set; }
        public string tipo_ident { get; set; }
        public string atv_economica { get; set; }
        public string rut { get; set; }
        public string dv_rut { get; set; }
        public string direccion_multiple { get; set; }
        public string tipo_cliente { get; set; }
        public string fecha_creacion { get; set; }
        public string rol_creacion { get; set; }
        public string telefono { get; set; }
        public string dia_vencimento { get; set; }
        public string tiene_postal { get; set; }
        public string tipo_morador { get; set; }
        public string telefono2 { get; set; }
        public string estado_docu { get; set; }
        public string comp_docu { get; set; }
        public string ind_baixarenda { get; set; }
        public string ind_conv_gov { get; set; }
        public string codigo_logra { get; set; }
        public string direccion { get; set; }
        public string numero_casa { get; set; }
        public string sector { get; set; }
        public string zona { get; set; }
        public string localidade { get; set; }
        public string comuna { get; set; }
        public string coordenadas_eura { get; set; }
        public string cep { get; set; }
        public string pot_inst_kw { get; set; }
        public string classe_atend { get; set; }
        public string subclasse_atend { get; set; }
        public string construcao_padrao { get; set; }
        public string tipo_parcelamento { get; set; }
        public string saldo_inicial { get; set; }
        public string tiene_convenio { get; set; }
        public string numero_cuotas { get; set; }
        public string valor_cuota { get; set; }
        public string intereses { get; set; }
        public string Ind_Poder_Publico { get; set; }
        public string bairro_solicitacao { get; set; }
        public string dica_localizacao { get; set; }
        public string cliente_iphan { get; set; }
        public string cliente_zona_urbana { get; set; }
        public string estado_cliente { get; set; }
        public TipoCliente Tipo
        {
            get { return ("8").Equals(this.estado_cliente) ? TipoCliente.GA : TipoCliente.GB; }
        }
        public string ind_bloqueio_norm { get; set; }
        public string estado_suministro { get; set; }
        public string data_ingresso { get; set; }
        public string nome_novo { get; set; }
        public string nome_mae_novo { get; set; }
        public string mail_novo { get; set; }
        public string documento { get; set; }
        
        public string activity { get; set; }
        public string id_unico { get; set; }
        public string user_number { get; set; }
        public string data_contrato { get; set; }
        public string execution_mode { get; set; }

        //Dados especificos
        public string ind_pessoa { get; set; }
        public string religiao { get; set; }
        public string escolaridade { get; set; }
        public string nome_completo { get; set; }
        public string qtd_pessoas_uc { get; set; }
        public string qtd_filhos { get; set; }
        public string domicilio { get; set; }
        public string sexo { get; set; }
        public string profissao { get; set; }
        public string uf_nascimento { get; set; }
        public string data_nasc { get; set; }
        public string tempo_atuacao { get; set; }
        public string qtd_funcionarios { get; set; }
        public string ind_ucbaixarendaant { get; set; }
        public string estado_facturacion { get; set; }
        public string seq_controle { get; set; }

        //Carga
        public string cod_artefacto { get; set; }
        public string cantidad { get; set; }
        public string potencia { get; set; }
        public string horas_uso { get; set; }
        public string ind_cliente_vital { get; set; }
        //Fim carga

        //OrdemServico
        //[RegularExpression("^(?!NULL$).*$", ErrorMessage = "{0} deve ser informado.")]
        public string numero_caso { get; set; }

        //[RegularExpression("^(?!NULL$).*$", ErrorMessage = "{0} deve ser informado.")]
        public string tipo_servico { get; set; }

        public string Observacao { get; set; }
        public string tipo_ordem { get; set; }
        public string UsuarioIngresso { get; set; }
        public string ReferenciaEndereco { get; set; }
        public string grupo { get; set; }

        //>> Ateração contratual
        public string tipo_operacao { get; set; }
        public string tarifa_nova { get; set; }
        public string classe_nova { get; set; }
        public string subclasse_nova { get; set; }
        public string motivo { get; set; }
        public string submotivo { get; set; }
        public string origem_solicitacao { get; set; }
        public string Tipo_liga_nova { get; set; }
        public string DataDoAgendamento { get; set; }
        public string HoraInicio { get; set; }
        public string HoraFim { get; set; }
        //<<

        public string tiene_fornecimento { get; set; }

        //grandes
        public string dem_ctr_hp_seco { get; set; }
        public string dem_ctr_fp_seco { get; set; }
        public string dem_ctr_hr_seco { get; set; }
        public string dem_ctr_hp_umido { get; set; }
        public string dem_ctr_fp_umido { get; set; }
        public string dem_ctr_hr_umido { get; set; }
        public string carga_inst_kva { get; set; }
        public string voltaje_solicitado { get; set; }
        public string data_criacao { get; set; }
        public string contato_cli { get; set; }
        public string unidade_federacao { get; set; }
        public string area_concessao { get; set; }
        public string optante_bt { get; set; }
        public string contato_fone { get; set; }
        public string crea_tecnico { get; set; }
        public string potencia_trafo { get; set; }
        public string hora_func_emp_ini_1 { get; set; }
        public string hora_func_emp_fim_1 { get; set; }
        public string hora_func_emp_ini_2 { get; set; }
        public string hora_func_emp_fim_2 { get; set; }
        public string acteco_com { get; set; }
        public string ejecutivo { get; set; }
        public string insc_estadual { get; set; }
        public string retirada_medidor { get; set; }
        public string ultima_leitura_visita { get; set; }
        public string canal_atendimento { get; set; }

    }

}

using Pangea.Entidades.Base;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;

namespace Pangea.Entidades.DTO
{
    [DisplayName("cliente")]
    public class ClienteDTO : EntidadeBase
    {
       public string codigo_empresa { get; set; }
        public string numero_cliente { get; set; }
        //Dados para Atualizar Telefone 
        public List<TelefoneDTO> telefones { get; set; }
        //Dados para Atualizar Documentos do Cliente 
        public List<DocumentoDTO> documentos { get; set; }
        //Dados para Atualizar Dados do Cliente 
		[MaxLength(1, ErrorMessage = "O tamanho máximo do campo dv_numero_cliente é de {1} caracter.")]
        public string dv_numero_cliente { get; set; }
		
		[MaxLength(40, ErrorMessage = "O tamanho máximo para o campo nombre é de {1} caracteres.")]
        public string nombre { get; set; }
		
        [MaxLength(75, ErrorMessage = "O tamanho máximo para o campo direccion é de {1} caracteres.")]
		public string direccion { get; set; }
		
		[MaxLength(14, ErrorMessage = "O tamanho máximo do campo soundex_dir é de {1} caracteres.")]
        public string soundex_dir { get; set; }
		
		[MaxLength(10, ErrorMessage = "O tamanho máximo do campo soundex_nombre é de {1} de caracteres.")]
        public string soundex_nombre { get; set; }
		
		[MaxLength(5, ErrorMessage = "O tamanho máximo do campo comuna é de {1} caracteres.")]
        public string comuna { get; set; }
		
		[MaxLength(10, ErrorMessage = "O tamanho máximo do campo giro é de {1} caracteres.")]
        public string giro { get; set; }
		
		[MaxLength(10, ErrorMessage = "O tamanho máximo do campo telefono é de {1} caracteres.")]
        public string telefono { get; set; }

        [MaxLength(10, ErrorMessage = "O tamanho máximo do campo telefono é de {1} caracteres.")]
        public string numero_telefone { get; set; }

		[MaxLength(3, ErrorMessage = "O tamanho máximo do campo tipo_ident é de {1} caracteres.")]
        public string tipo_ident { get; set; }
		
        [MaxLength(20, ErrorMessage = "O tamanho máximo do campo rut é de {1} caracteres.")]
        public string rut { get; set; }

        [MaxLength(2, ErrorMessage = "O tamanho máximo do campo dv_rut é de {1} caracteres.")]
        public string dv_rut { get; set; }
		
        [MaxLength(1, ErrorMessage = "O tamanho máximo do campo empresa é de {1} caracter.")]
        public string empresa { get; set; }

        [MaxLength(3, ErrorMessage = "O tamanho máximo de caracteres permitidos {1}.")]
        public string religiao { get; set; }

        [MaxLength(1, ErrorMessage = "O tamanho máximo de caracteres permitidos {1}.")]
        public string escolaridade { get; set; }

        [MaxLength(100, ErrorMessage = "O tamanho máximo do campo nome_completo é de {1} caracteres.")]
        public string nome_completo { get; set; }
        public string qtd_pessoas_uc { get; set; }
		
		[MaxLength(1, ErrorMessage = "O tamanho máximo do campo sexo é de {1} caracter.")]
        public string sexo { get; set; }
        public string tempo_atuacao { get; set; }
        public string qtd_funcionarios { get; set; }
		
		[MaxLength(3, ErrorMessage = "O tamanho máximo do campo é de {1} caracteres.")]
        public string domicilio { get; set; }
        public string qtd_filhos { get; set; }
		
		[MaxLength(1, ErrorMessage = "O tamanho máximo do campo estado_civil é de {1} caracter.")]
        public string estado_civil { get; set; }
		
		[MaxLength(4, ErrorMessage = "O tamanho máximo do campo profissao é de {1} caracteres.")]
        public string profissao { get; set; }
		
		[MaxLength(255, ErrorMessage = "O tamanho máximo do campo dica_localizacao é de {1} caracteres.")]
        public string dica_localizacao { get; set; }
		
		[MaxLength(9, ErrorMessage = "O tamanho máximo do campo celular é de {1} caracteres.")]
        public string celular { get; set; }
		
		
        public string sector { get; set; }
        public string localidade { get; set; }
        public string zona { get; set; }
        public string correlativo_ruta { get; set; }
		
		[MaxLength(1, ErrorMessage = "O tamanho máximo do campo dv_ruta_lectura é de {1} caracter.")]
        public string dv_ruta_lectura { get; set; }
		
		[MaxLength(30, ErrorMessage = "O tamanho máximo do campo complemento é de {1} caracteres.")]
        public string complemento { get; set; }
		
		[MaxLength(4, ErrorMessage = "O tamanho máximo do campo coordenadas_eura é de {1} caracteres.")]
        public string coordenadas_eura { get; set; }
        public string cantidad_medidores { get; set; }
		
		[MaxLength(5, ErrorMessage = "O tamanho máximo do campo info_adic_lectura é de {1} caracteres.")]
        public string info_adic_lectura { get; set; }
		
		[MaxLength(5, ErrorMessage = "O tamanho máximo do campo tarifa é de {1} caracteres.")]
        public string tarifa { get; set; }
		
		[MaxLength(2, ErrorMessage = "O tamanho máximo de caracteres permitidos {1}.")]
        public string tipo_vencimiento { get; set; }
		
		[MaxLength(1, ErrorMessage = "O tamanho máximo do campo tipo_cliente é de {1} caracter.")]
        public string tipo_cliente { get; set; }
        public string cantidad_casas { get; set; }
        public string consumo_30_dias { get; set; }
        public string recargo_malfactor { get; set; }
        public string recargo_tension { get; set; }
        public string potencia_contrato { get; set; }
        public string potencia_inst_hp { get; set; }
        public string potencia_inst_fp { get; set; }
        public string potencia_cont_hp { get; set; }
        public string potencia_cont_fp { get; set; }
		
		[MaxLength(1, ErrorMessage = "O tamanho máximo do campo propiedad_empalme é de {1} caracter.")]
        public string propiedad_empalme { get; set; }
		
		[MaxLength(4, ErrorMessage = "O tamanho máximo do campo tipo_empalme é de {1} caracteres.")]
        public string tipo_empalme { get; set; }


        [MaxLength(1, ErrorMessage = "O tamanho máximo do campo ind contato é de {1} caracter permitido.")]
        public string ind_contato { get; set; }

        [MaxLength(5, ErrorMessage = "O tamanho máximo do campo ramal é de {1} caracteres permitidos.")]
        public string ramal { get; set; }

        public string meses_cerrados { get; set; }
        public string fecha_a_corte { get; set; }
        public string fecha_penult_fact { get; set; }
        public string fecha_ultima_fact { get; set; }
        public string fecha_penult_lect { get; set; }
        public string fecha_ultima_lect { get; set; }
        public string prom_importe_cons { get; set; }
        public string cons_prom_diario { get; set; }
        public string nro_dias_consumo { get; set; }
		
		[MaxLength(1, ErrorMessage = "O tamanho máximo do campo clave_boleta é de {1} caracter.")]
        public string clave_boleta { get; set; }
        public string deuda_convenida { get; set; }
        public string antiguedad_saldo { get; set; }
        public string corr_facturacion { get; set; }
        public string corr_pagos { get; set; }
        public string corr_convenio { get; set; }
        public string corr_corte { get; set; }
        public string corr_refacturacion { get; set; }
		
		[MaxLength(1, ErrorMessage = "O tamanho máximo do campo estado_facturacion é de {1} caracter.")]
        public string estado_facturacion { get; set; }
		
		[MaxLength(1, ErrorMessage = "O tamanho máximo do campo estado suministro é de {1} caracter.")]
        public string estado_suministro { get; set; }
		
		[MaxLength(1, ErrorMessage = "O tamanho máximo do campo estado_cliente é de {1} caracter.")]
        public string estado_cliente { get; set; }
		
		[MaxLength(1, ErrorMessage = "O tamanho máximo do campo tiene_cobro_int é de {1} caracter.")]
        public string tiene_cobro_int { get; set; }
		
		[MaxLength(1, ErrorMessage = "O tamanho máximo do campo tiene_prorrateo é de {1} caracter")]
        public string tiene_prorrateo { get; set; }
		
		[MaxLength(1, ErrorMessage = "O tamanho máximo do campo tiene_cnr é de {1} caracter.")]
        public string tiene_cnr { get; set; }
		
		[MaxLength(1, ErrorMessage = "O tamanho máximo do campo tiene_cambios_rest é de {1} caracter.")]
        public string tiene_cambios_rest { get; set; }
		
		[MaxLength(1, ErrorMessage = "O tamanho máximo do campo tiene_corte_rest é de {1} caracter.")]
        public string tiene_corte_rest { get; set; }
		
		
        [MaxLength(1, ErrorMessage = "O tamanho máximo do campo tiene_convenio é de {1} caracter.")]
        public string tiene_convenio { get; set; }

        [MaxLength(1, ErrorMessage = "O tamanho máximo do campo tiene_refacturac é de {1} caracter.")]
        public string tiene_refacturac { get; set; }

        [MaxLength(1, ErrorMessage = "O tamanho máximo do campo tiene_postal é de {1} caracter.")]
        public string tiene_postal { get; set; }
        [MaxLength(1, ErrorMessage = "O tamanho máximo do campo tiene_calma é de {1} caracter.")]
        public string tiene_calma { get; set; }

        [MaxLength(1, ErrorMessage = "O tamanho máximo do campo tiene_cobro_corte é de {1} caracter.")]
        public string tiene_cobro_corte { get; set; }

        [MaxLength(1, ErrorMessage = "O tamanho máximo do campo tiene_at_med_bt é {1} de caracter.")]
        public string tiene_at_med_bt { get; set; }

        [MaxLength(1, ErrorMessage = "O tamanho máximo do campo tiene_notific é {1} de caracter.")]
        public string tiene_notific { get; set; }
		
		[MaxLength(12, ErrorMessage = "O tamanho máximo do campo cod_proyecto é de {1} caracteres.")]
        public string cod_proyecto { get; set; }
        public string fecha_proyecto { get; set; }
		
        [MaxLength(4, ErrorMessage = "O tamanho máximo do campo municipio é {1} de caracteres.")]
        public string municipio { get; set; }

        [MaxLength(10, ErrorMessage = "O tamanho máximo do campo cep é de {1} caracteres.")]
        public string cep { get; set; }

        [MaxLength(2, ErrorMessage = "O tamanho máximo do campo classe é de {1} caracteres.")]
        public string classe { get; set; }

        [MaxLength(2, ErrorMessage = "O tamanho máximo do campo subclasse é de {1} caracteres.")]
        public string subclasse { get; set; }

        [MaxLength(1, ErrorMessage = "O tamanho máximo do campo cliente_veranista é de {1} caracter.")]
        public string cliente_veranista { get; set; }

        [MaxLength(1, ErrorMessage = "O tamanho máximo do campo ind_baixarenda é de {1} caracter permitido {1}.")]
        public string ind_baixarenda { get; set; }

        [MaxLength(1, ErrorMessage = "O tamanho máximo do campo ind_cliente_vip é de {1} caracter.")]
        public string ind_cliente_vip { get; set; }

        [MaxLength(1, ErrorMessage = "O tamanho do campo máximo ind_ilumpublica é de {1} caracter.")]
        public string ind_ilumpublica { get; set; }

        [MaxLength(4, ErrorMessage = "O tamanho máximo do campo ddd é de {1} caracteres.")]
        public string ddd { get; set; }

        [MaxLength(4, ErrorMessage = "O tamanho máximo do campo ddd é de {1} caracteres.")]
        public string prefixo_ddd { get; set; }
       
        [MaxLength(2, ErrorMessage = "O tamanho máximo do campo tipo telefone é de {1} caracteres permitidos.")]
        public string tipo_telefone { get; set; }

		
		[MaxLength(1, ErrorMessage = "O tamanho máximo do campo tributo é de {1} caracter.")]
        public string tributo { get; set; }
        public string codigo_emp_comp { get; set; }
        public string saldo_afecto { get; set; }
        public string saldo_noafecto { get; set; }
        public string intereses { get; set; }
        public string multas { get; set; }
        public string cliente_anterior { get; set; }
		
		[MaxLength(5, ErrorMessage = "O tamanho máximo do campo codigo_logra é de {1} caracteres.")]
        public string codigo_logra { get; set; }
        [MaxLength(1, ErrorMessage = "O tamanho máximo do campo tipo_ligacao é de {1} caracter.")]
        public string tipo_ligacao { get; set; }

        [MaxLength(12, ErrorMessage = "O tamanho máximo do campo codigo_pee é de {1} caracteres.")]
        public string codigo_pee { get; set; }

        [MaxLength(5, ErrorMessage = "O tamanho máximo do campo numero_casa é de {1} caracteres.")]
        public string numero_casa { get; set; }

        [MaxLength(1, ErrorMessage = "O tamanho máximo do campo ind_calc_media é de {1} caracter.")]
        public string ind_calc_media { get; set; }

        [MaxLength(1, ErrorMessage = "O tamanho máximo do campo ind_conv_gov é de {1} caracter.")]
        public string ind_conv_gov { get; set; }
        public string media_movel { get; set; }
		
		[MaxLength(20, ErrorMessage = "O tamanho máximo do campo numero_nis é de {1} caracteres.")]
        public string numero_nis { get; set; }
        public string refer_lido_br { get; set; }
        public string dia_vencimento { get; set; }
        public string quadra { get; set; }
		
		 [MaxLength(65, ErrorMessage = "O tamanho máximo do campo nome_mae é de {1} caracteres.")]
        public string nome_mae { get; set; }

        [MaxLength(50, ErrorMessage = "O tamanho máximo do campo mail é de {1} caracteres.")]
        public string mail { get; set; }

        [MaxLength(1, ErrorMessage = "O tamanho máximo do campo ind_pessoa é de {1} caracter.")]
        public string ind_pessoa { get; set; }

        [MaxLength(2, ErrorMessage = "O tamanho máximo do campo uf_nascimento é de {1} caracteres.")]
        public string uf_nascimento { get; set; }
        public string data_nasc { get; set; }

        [MaxLength(2, ErrorMessage = "O tamanho máximo do campo subclasse_orig é de {1} caracteres.")]
        public string subclasse_orig { get; set; }

        [MaxLength(1, ErrorMessage = "O tamanho máximo do campo ind_ucbaixarenda é de caracter.")]
        public string ind_ucbaixarenda { get; set; }
        public string codigo_imovel { get; set; }
		
		[MaxLength(4, ErrorMessage = "O tamanho máximo do campo sucursal é de {1} caracteres.")]
        public string sucursal { get; set; }
		
		[MaxLength(1, ErrorMessage = "O tamanho máximo do campo tiene_notific é {1} de caracter.")]
        public string tiene_notif { get; set; }
        public string ddd2 { get; set; }
        public string telefono2 { get; set; }
        public string ramal2 { get; set; }
        public string tipo_ident2 { get; set; }
        public string documento2 { get; set; }
        public string dv_docu2 { get; set; }
		
		[MaxLength(1, ErrorMessage = "O tamanho máximo do campo ind_cli_despersona é de {1} caracter.")]
        public string ind_cli_despersona { get; set; }
        public string ind_ro { get; set; }
		
        //DOCUMENTO

        [MaxLength(3, ErrorMessage = "O tamanho máximo do campo tipo documento é de {1} caracteres permitidos.")]
        public string tipo_documento { get; set; }

        [MaxLength(20, ErrorMessage = "O tamanho máximo do campo número documento é de {1} caracteres permitidos.")]
        public string numero_doc { get; set; }

        [MaxLength(2, ErrorMessage = "O tamanho máximo do campo dv documento é de {1} caracteres permitidos.")]
        public string dv_documento { get; set; }

        [MaxLength(6, ErrorMessage = "O tamanho máximo do campo complemento documento é de {1} caracteres permitidos.")]
        public string compl_documento { get; set; }
        public string data_emissao { get; set; }
        public int sequencial { get; set; }

        [MaxLength(2, ErrorMessage = "O tamanho máximo do campo UF é de {1} caracteres permitidos.")]
        public string uf { get; set; }

        [MaxLength(1, ErrorMessage = "O tamanho máximo do campo tipo documento é de {1} caracter permitido.")]
        public string foto { get; set; }

        [MaxLength(1, ErrorMessage = "O tamanho máximo do campo tipo documento é de {1} caracter permitido.")]
        public string valida_org_emis { get; set; }

        public string ind_cliente_vital { get; set; }

        //public ClienteDTO()
        //{
        //    Type t = typeof(ClienteDTO);
        //    foreach (PropertyInfo pi in t.GetProperties())
        //    {
        //        if (pi.PropertyType.Name.ToUpper().Equals("STRING"))
        //            pi.SetValue(this, "NULL", null);
        //    }
        //}
    }
}

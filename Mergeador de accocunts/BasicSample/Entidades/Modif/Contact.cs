using SalesforceExtractor.Entidades.Enumeracoes;
using SalesforceExtractor.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace SalesforceExtractor.Entidades.Modif
{
    [ModifAttribute(TipoCliente = TipoCliente.GB)]
    public class Contact : ModifBase
    {
        public Contact()
            : base("Contact", 0)
        {
            IdDicionarioIdentificador = (int)DicColuna.col1_7.identificador_conta;
        }

        #region Propriedades Privadas Grupo A

        //private ItemEntidade nomeGA = new ItemEntidade(7, (int)DicColuna.col1_7.nombre, "Nome", "nombre", "NE__Name__c");
        //private ItemEntidade telefone = new ItemEntidade(13, (int)DicColuna.col1_7.telefone1, "Telefone da conta", "telefono", "Phone");
        //private ItemEntidade cepGA = new ItemEntidade(99, (int)DicColuna.col1_7.cep, "cep", "cep", "CNT_Is_Postal_Address__c");
        //private ItemEntidade emailGA = new ItemEntidade(624, (int)DicColuna.col1_7.mail, "EMAIL", "EMAIL", "PrimaryEmail__c");
        //private ItemEntidade nomeFantasia = new ItemEntidade(701, (int)DicColuna.col1_7.nombre, "Nome Fantasia", "nome_fantasia", "NE__Name__c");
        //private ItemEntidade endereco = new ItemEntidade(8, (int)DicColuna.col1_7.direccion, "Endereço de cobrança", "direccion", "BillingAddress");
        //private ItemEntidade documentoGA = new ItemEntidade(14, (int)DicColuna.col1_7.documento_cliente, "IdentityNumber__c", "rur", "SecondaryPhone__c");
        //private ItemEntidade documentoTipoGA = new ItemEntidade(16, (int)DicColuna.col1_7.tipo_identidade, "Tipo do Documento", "tipo_ident", "IdentityType__c");
        //private ItemEntidade atividadeEconomica = new ItemEntidade(30, (int)DicColuna.col1_7.tipo_identidade?????, "Telefone Principal", "TELEFONE", "MainPhone__c");
        //private ItemEntidade documento2 = new ItemEntidade(116, (int)DicColuna.col1_7.?????, "Documento", "rut_2", "CNT_ID_NUM_2__c");
        //private ItemEntidade tipoDocumento2 = new ItemEntidade(118, (int)DicColuna.col1_7.????, "Tipo de Documento", "dv_rut_2", "CNT_ID_Type_2__c");

        #endregion

        #region Propriedades Privadas Grupo B

        private ItemEntidade nome = new ItemEntidade(4, (int)DicColuna.col1_7.nombre, "Nome completo", "NOME", "Name");
        private ItemEntidade cep = new ItemEntidade(20, (int)DicColuna.col1_7.cep, "CEP de correspondência", "CEP", "MailingPostalCode");
        private ItemEntidade telefone1 = new ItemEntidade(8, (int)DicColuna.col1_7.telefone1, "Telefone Principal", "TELEFONE", "Phone");
        private ItemEntidade telefone2 = new ItemEntidade(8, (int)DicColuna.col1_7.telefone2, "Telefone Alternativo", "TELEFONE", "SecondaryPhone__c");
        private ItemEntidade telefone3 = new ItemEntidade(8, (int)DicColuna.col1_7.telefone3, "Outro telefone", "TELEFONE", "OtherPhone");
        private ItemEntidade email = new ItemEntidade(26, (int)DicColuna.col1_7.mail, "Email", "E-MAIL", "Email");
        private ItemEntidade emailSecundario = new ItemEntidade(26, (int)DicColuna.col1_7.mail, "E-mail Alternativo", "E-MAIL", "SecondaryEmail__c");
        private ItemEntidade nascimento = new ItemEntidade(87, (int)DicColuna.col1_7.fecha_nasc, "Data de nascimento", "DATA NASCIMENTO", "Birthdate");

        #endregion



        #region Propriedades Públicas Grupo A

        //[ModifAttribute(TipoCliente = TipoCliente.GA)]
        //public ItemEntidade NomeGA
        //{
        //    get { return nomeGA; }
        //    set { nomeGA = value; }
        //}

        ////[ModifAttribute(TipoCliente = TipoCliente.GA)]
        ////public ItemEntidade Endereco
        ////{
        ////    get { return endereco; }
        ////    set { endereco = value; }
        ////}

        //[ModifAttribute(TipoCliente = TipoCliente.GA)]
        //public ItemEntidade Telefone
        //{
        //    get { return telefone; }
        //    set { telefone = value; }
        //}

        ////[ModifAttribute(TipoCliente = TipoCliente.GA)]
        ////public ItemEntidade DocumentoGA
        ////{
        ////    get { return documentoGA; }
        ////    set { documentoGA = value; }
        ////}

        ////[ModifAttribute(TipoCliente = TipoCliente.GA)]
        ////public ItemEntidade DocumentoTipoGA
        ////{
        ////    get { return documentoTipoGA; }
        ////    set { documentoTipoGA = value; }
        ////}

        //[ModifAttribute(TipoCliente = TipoCliente.GA)]
        //public ItemEntidade CepGA
        //{
        //    get { return cepGA; }
        //    set { cepGA = value; }
        //}

        //[ModifAttribute(TipoCliente = TipoCliente.GA)]
        //public ItemEntidade EmailGA
        //{
        //    get { return emailGA; }
        //    set { emailGA = value; }
        //}

        //[ModifAttribute(TipoCliente = TipoCliente.GA)]
        //public ItemEntidade NomeFantasia
        //{
        //    get { return nomeFantasia; }
        //    set { nomeFantasia = value; }
        //}

        #endregion




        #region Propriedades Públicas Grupo B

        [ModifAttribute(TipoCliente = TipoCliente.GB)]
        public ItemEntidade Nome
        {
            get { return nome; }
            set { nome = value; }
        }

        [ModifAttribute(TipoCliente = TipoCliente.GB)]
        public ItemEntidade Telefone1
        {
            get { return telefone1; }
            set { telefone1 = value; }
        }

        [ModifAttribute(TipoCliente = TipoCliente.GB)]
        public ItemEntidade Telefone2
        {
            get { return telefone2; }
            set { telefone2 = value; }
        }

        [ModifAttribute(TipoCliente = TipoCliente.GB)]
        public ItemEntidade Telefone3
        {
            get { return telefone3; }
            set { telefone3 = value; }
        }

        //[ModifAttribute(TipoCliente = TipoCliente.GB)]
        //public ItemEntidade DocumentoTipo
        //{
        //    get { return documentoTipo; }
        //    set { documentoTipo = value; }
        //}

        //[ModifAttribute(TipoCliente = TipoCliente.GB)]
        //public ItemEntidade Documento
        //{
        //    get { return documento; }
        //    set { documento = value; }
        //}

        [ModifAttribute(TipoCliente = TipoCliente.GB)]
        public ItemEntidade Email
        {
            get { return email; }
            set { email = value; }
        }

        [ModifAttribute(TipoCliente = TipoCliente.GB)]
        public ItemEntidade EmailSecundario
        {
            get { return emailSecundario; }
            set { emailSecundario = value; }
        }

        [ModifAttribute(TipoCliente = TipoCliente.GB)]
        public ItemEntidade Nascimento
        {
            get { return string.IsNullOrWhiteSpace(nascimento.NovoValor) ? null : nascimento; }
            set { nascimento = value; }
        }

        [ModifAttribute(TipoCliente = TipoCliente.GB)]
        public ItemEntidade Cep
        {
            get { return cep; }
            set { cep = value; }
        }

        #endregion
    }
}

using SalesforceExtractor.Entidades.Enumeracoes;
using SalesforceExtractor.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace SalesforceExtractor.Entidades.Modif
{
    [ModifAttribute(TipoCliente = TipoCliente.Ambos)]
    public class PointOfDelivery : ModifBase
    {
        public PointOfDelivery()
            : base("PointofDelivery__c", 54)
        {
            IdDicionarioIdentificador = (int)DicColuna.col1_7.identificador_pod;
        }

        #region Propriedades Privadas para Ambos os Grupos GA e GB
        
        private ItemEntidade rota = new ItemEntidade(2, (int)DicColuna.col1_7.ruta_lectura, "ROTA DE LEITURA", "Rota", "Route__c");
        private ItemEntidade indClienteVital1 = new ItemEntidade(46, (int)DicColuna.col1_7.electrodependiente, "IND EQUIPAMENTO VITAL", "Cliente Vital", "Electrodependant__c");

        #endregion


        #region Propriedades Privadas GA

        //private ItemEntidade lote = new ItemEntidade(3, (int)DicColuna.col1_7.??? "Lote", "Lote Number", "VIP_Customer_Indicator__c");
        //private ItemEntidade zona = new ItemEntidade(4, (int)DicColuna.col1_7.??, "Zona", "Zona", "Zone__c");
        private ItemEntidade municipioGA = new ItemEntidade(11, (int)DicColuna.col1_7.localidad, "Municipio", "Municipio", "Municipality__c");
        private ItemEntidade tarifaGA = new ItemEntidade(24, (int)DicColuna.col1_7.tarifa, "TARIFA", "Tarifa", "Rate__c");
        private ItemEntidade cargaInstalalda = new ItemEntidade(40, (int)DicColuna.col1_7.carga_aforada, "Carga Instalada", "Potency of Power Unit", "Power__c");
        private ItemEntidade alimentadorNumeroGA = new ItemEntidade(246, (int)DicColuna.col1_7.num_alimentador, "Código do Alimentador", "Número do Alimentador", "FeederNumber__c");
        private ItemEntidade potenciaTransformador = new ItemEntidade(404, (int)DicColuna.col1_7.potencia_capacidad_instalada, "Poder", "Tipo de Ligação", "CNT_Potency_of_Power_Unit__c");
        private ItemEntidade subestacaoGA = new ItemEntidade(405, (int)DicColuna.col1_7.subestacion_electrica_conexion, "Subestação", "Nome da Substacao", "ElectricalSubstation__c");
        private ItemEntidade indClienteVital = new ItemEntidade(205, (int)DicColuna.col1_7.electrodependiente, "IND EQUIPAMENTO VITAL", "Cliente Vital", "Electrodependant__c");
        
        #endregion

        
        #region Propriedades Privadas Grupo B

        private ItemEntidade municipio = new ItemEntidade(22, (int)DicColuna.col1_7.ciudad, "MUNICIPIO", "Municipio", "Municipality__c");
        private ItemEntidade subestacao = new ItemEntidade(56, (int)DicColuna.col1_7.subestacion_electrica_conexion, "SUBEST.TRANSMISSÃO", "Subestação", "ElectricalSubstation__c");
        private ItemEntidade alimentadorNumero = new ItemEntidade(58, (int)DicColuna.col1_7.num_alimentador, "ALIMENTADOR", "Número do Alimentador", "FeederNumber__c");
        private ItemEntidade chave = new ItemEntidade(59, (int)DicColuna.col1_7.llave, "CHAVE", "Chave", "Key__c");
        private ItemEntidade tipoRede = new ItemEntidade(60, (int)DicColuna.col1_7.tipo_rede, "TIPO DE REDE", "Tipo de Rede", "Network_Type__c");
        private ItemEntidade tarifa = new ItemEntidade(64, (int)DicColuna.col1_7.tarifa, "TARIFA", "Tarifa", "Rate__c");
        private ItemEntidade tipoLigacao = new ItemEntidade(202, (int)DicColuna.col1_7.tipo_conexion, "TIPO LIGAÇÃO", "Tipo de Ligação", "ConnectionType__c");
        private ItemEntidade tipoMedicao = new ItemEntidade(308, (int)DicColuna.col1_7.tipo_facturacion, "TIPO DE MEDIÇÃO", "Tipo de Faturamento", "MeterType__c");

        #endregion


        #region Propriedades Públicas Grupo A

        [ModifAttribute(TipoCliente = TipoCliente.GA)]
        public ItemEntidade MunicipioGA
        {
            get { return this.municipioGA; }
            set { this.municipioGA = value; }
        }

        [ModifAttribute(TipoCliente = TipoCliente.GA)]
        public ItemEntidade TarifaGA
        {
            get { return this.tarifaGA; }
            set { this.tarifaGA = value; }
        }

        [ModifAttribute(TipoCliente = TipoCliente.GA)]
        public ItemEntidade CargaInstalalda
        {
            get { return this.cargaInstalalda; }
            set { this.cargaInstalalda = value; }
        }

        [ModifAttribute(TipoCliente = TipoCliente.GA)]
        public ItemEntidade AlimentadorNumeroGA
        {
            get { return this.alimentadorNumeroGA; }
            set { this.alimentadorNumeroGA = value; }
        }

        [ModifAttribute(TipoCliente = TipoCliente.GA)]
        public ItemEntidade PotenciaTransformador
        {
            get { return this.potenciaTransformador; }
            set { this.potenciaTransformador = value; }
        }

        [ModifAttribute(TipoCliente = TipoCliente.GA)]
        public ItemEntidade SubestacaoGA
        {
            get { return this.subestacaoGA; }
            set { this.subestacaoGA = value; }
        }

        #endregion

        
        #region Propriedades Públicas Grupo B
        [ModifAttribute(TipoCliente = TipoCliente.Ambos)]
        public ItemEntidade Rota
        {
            get { return this.rota; }
            set { this.rota = value; }
        }

        [ModifAttribute(TipoCliente = TipoCliente.GB)]
        public ItemEntidade Municipio
        {
            get { return this.municipio; }
            set { this.municipio = value; }
        }

        [ModifAttribute(TipoCliente = TipoCliente.GB)]
        public ItemEntidade Subestacao
        {
            get { return this.subestacao; }
            set { this.subestacao = value; }
        }

        [ModifAttribute(TipoCliente = TipoCliente.GB)]
        public ItemEntidade AlimentadorNumero
        {
            get { return this.alimentadorNumero; }
            set { this.alimentadorNumero = value; }
        }

        [ModifAttribute(TipoCliente = TipoCliente.GB)]
        public ItemEntidade Chave
        {
            get { return this.chave; }
            set { this.chave = value; }
        }

        [ModifAttribute(TipoCliente = TipoCliente.GB)]
        public ItemEntidade TipoRede
        {
            get { return this.tipoRede; }
            set { this.tipoRede = value; }
        }

        [ModifAttribute(TipoCliente = TipoCliente.GB)]
        public ItemEntidade Tarifa
        {
            get { return tarifa; }
            set { tarifa = value; }
        }

        [ModifAttribute(TipoCliente = TipoCliente.GB)]
        public ItemEntidade TipoLigacao
        {
            get { return this.tipoLigacao; }
            set { this.tipoLigacao = value; }
        }

        [ModifAttribute(TipoCliente = TipoCliente.GB)]
        public ItemEntidade IndEquipamentoVital
        {
            get 
            {
                if(this.indClienteVital != null && !string.IsNullOrWhiteSpace(this.indClienteVital.NovoValor))
                {
                    this.indClienteVital.NovoValor = ("V".Equals(this.indClienteVital.NovoValor.ToUpper())) ? this.indClienteVital.NovoValor : "N";
                }
                return this.indClienteVital; 
            }
            set { this.indClienteVital = value; }
        }

        [ModifAttribute(TipoCliente = TipoCliente.Ambos)]
        public ItemEntidade IndEquipamentoVital1
        {
            get
            {
                if (this.indClienteVital1 != null && !string.IsNullOrWhiteSpace(this.indClienteVital1.NovoValor))
                {
                    this.indClienteVital1.NovoValor = ("V".Equals(this.indClienteVital1.NovoValor.ToUpper())) ? this.indClienteVital1.NovoValor : "N";
                }
                return this.indClienteVital1;
            }
            set { this.indClienteVital1 = value; }
        }

        [ModifAttribute(TipoCliente = TipoCliente.GB)]
        public ItemEntidade TipoMedicao
        {
            get { return this.tipoMedicao; }
            set { this.tipoMedicao = value; }
        }
        #endregion
    }
}

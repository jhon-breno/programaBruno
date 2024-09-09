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
    public class Device : ModifBase
    {
        #region Propriedades Privadas
        private ItemEntidade numero = new ItemEntidade(501, (int)DicColuna.col1_7.numero_medidor, "Número do medidor", "Numero de Medidor", "MeterNumber__c");
        private ItemEntidade marca = new ItemEntidade(502, (int)DicColuna.col1_7.marca_medidor, "Marca de Medidor", "Marca do medidor", "MeterBrand__c");
        private ItemEntidade modelo = new ItemEntidade(506, (int)DicColuna.col1_7.modelo_medidor, "Modelo do Medidor", "Modelo do medidor", "MeterModel__c");
        private ItemEntidade propriedade = new ItemEntidade(515, (int)DicColuna.col1_7.propiedad_medidor, "Propriedade do Medidor", "Propriedade do Medidor", "MeterProperty__c");
        //private ItemEntidade tipo = new ItemEntidade(503, (int)DicColuna.col1_7.???, "Telefone Principal", "TELEFONE", "MainPhone__c");
        //private ItemEntidade estado = new ItemEntidade(507, (int)DicColuna.col1_7.modelo_medidor???, "Telefone Adicional", "TELEFONE", "AdditionalPhone__c");
        //private ItemEntidade fase = new ItemEntidade(509, (int)DicColuna.col1_7.tipo_conexion???, "Telefone Principal", "TELEFONE", "MainPhone__c");
        #endregion


        //TODO: identificar o Id do Consumo no Dicionario Interno
        public Device()
            : base("Device__c", 0)
        {
        }

        #region Propriedades Públicas
        [ModifAttribute(TipoCliente = TipoCliente.GB)]
        public ItemEntidade Numero
        {
            get { return numero; }
            set { numero = value; }
        }

        [ModifAttribute(TipoCliente = TipoCliente.GB)]
        public ItemEntidade Marca
        {
            get { return marca; }
            set { marca = value; }
        }

        [ModifAttribute(TipoCliente = TipoCliente.GB)]
        public ItemEntidade Modelo
        {
            get { return modelo; }
            set { modelo = value; }
        }

        [ModifAttribute(TipoCliente = TipoCliente.GB)]
        public ItemEntidade Propriedade
        {
            get { return propriedade; }
            set { propriedade = value; }
        }
        #endregion
    }
}

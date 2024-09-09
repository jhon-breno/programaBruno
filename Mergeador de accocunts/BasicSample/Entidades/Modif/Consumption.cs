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
    public class Consumption : ModifBase
    {
        #region Propriedades Privadas
        //private ItemEntidade tipoConsumo = new ItemEntidade(701, "CONSUMO FIXO", "Tipo Consumo", "Consumption_Type__c");
        #endregion


        //TODO: identificar o Id do Consumo no Dicionario Interno
        public Consumption()
            : base("Consumption",0) 
        { }


        #region Propriedades Públicas
        //[ModifAttribute(TipoCliente = TipoCliente.GB)]
        //public ItemEntidade TipoConsumo
        //{
        //    get { return tipoConsumo; }
        //    set { tipoConsumo = value; }
        //}
        #endregion
    }
}

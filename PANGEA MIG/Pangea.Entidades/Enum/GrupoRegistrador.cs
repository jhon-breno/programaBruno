using System.ComponentModel;

namespace Pangea.Negocio
{
    public enum GrupoRegistradorExcluir
    {
        /// <summary>
        /// Ação último recorte
        /// </summary>
        [Description("NNSSNSNNSSNS")]
        GACNV06, //ULTIMO_RECORTE

        /// <summary>
        /// Ação não parametrizada como corte_efetivo
        /// </summary>
        [Description("SSSSSSSSSSSS")]
        GAIRR12,//CORTE_EFETIVO

        /// <summary>
        /// NÃO GERADO - Cliente possui ind_tip_cxa = 'S'
        /// </summary>
        [Description("SNSSNSSNSSNS")]
        GAPDR08, //TIPO_CAIXA

        /// <summary>
        /// Quantidade de corte diferente de zero
        /// </summary>
        [Description("SNSNNNNNNNNN")]
        GBIRR02, //QUANTIDADE _CORTE

        /// <summary>
        /// Quantidade de corte menor que o minimo
        /// </summary>
        [Description("NNSNNNNNNNNN")]
        GBPDR01,//QTD_CORTE_HISTORICO

        /// <summary>
        /// Possui o campo fecha_parafuso com valor diferente de nulo não atendendo a parametrização
        /// </summary>
        [Description("SSSNNNNNNNNN")]
        GBTBR03, //FECHA_PARAFUSO

        /// <summary>
        /// Sucursal divergente da parametrização
        /// </summary>
        [Description("SSSNNNNNNNNNS")]
        GBTBRI04, //SUCURSAL

        /// <summary>
        /// Possui fornecimento e a parametrização impede o corte nesse caso
        /// </summary>
        [Description("SNNNNNNNNNNNNS")]
        GBGER02, //FORNECIMENTO

        /// <summary>
        /// De acordo com a parametrização deveria haver registros na tabela CNR.
        /// </summary>
        [Description("SNNNNNSNNNNN")]
        GBOPT02, //CNR

        /// <summary>
        /// Estado é diferente de ativo
        /// </summary>
        [Description("SNSNNNSNNNNN")]
        GBOPTI03

       

        
    }
}

#region OBSOLETO
//public class StringValueAttribute : Attribute
//{

//    #region Properties

//    /// <summary>
//    /// Holds the stringvalue for a value in an enum.
//    /// </summary>
//    public string StringValue { get; protected set; }

//    #endregion

//    #region Constructor

//    /// <summary>
//    /// Constructor used to init a StringValue Attribute
//    /// </summary>
//    /// <param name="value"></param>
//    public StringValueAttribute(string value)
//    {
//        this.StringValue = value;
//    }

//    #endregion

//}
#endregion

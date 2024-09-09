using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Pangea.Entidades.Enum
{
    public enum ValidacoesSynergia
    {
        [StringValue("SUCESSO. ")]
        Sucesso = 0,

        [StringValue("Ação último recorte ")]
        ULTIMO_RECORTE = 1,

        [StringValue("Ação não parametrizada como corte_efetivo. ")]
        CORTE_EFETIVO = 2,

        [StringValue("NÃO GERADO - Cliente possui ind_tip_cxa = 'S' ")]
        TIPO_CAIXA = 3,

        [StringValue("Quantidade de corte diferente de zero. ")]
        QUANTIDADE_CORTE = 4,

        [StringValue("Quantidade de corte menor que o minimo. ")]
        QUANTIDADE_CORTE_HISTORICO = 5,

        [StringValue("Possui o campo fecha_parafuso com valor diferente de nulo não atendendo a parametrização.")]
        FECHA_PARAFUSO = 6,

        [StringValue("Sucursal divergente da parametrização.")]
        SUCURSAL = 7,

        [StringValue("Possui fornecimento e a parametrização impede o corte nesse caso.")]
        FORNECIMENTO = 8,

        [StringValue("De acordo com a parametrização deveria ter registros na tabela cnr.")]
        CNR = 9,
       
        [StringValue("Estado é diferente de ativo.")]
        CLIENTE_INATIVO = 10,

        [StringValue("Tipo cliente divergente da parametrização.")]
        PARAMETRIZACAO_TIPO_CLIENTE = 11,

        [StringValue("Subclasse divergente da parametrização.")]
        PARAMETRIZACAO_SUBCLASSE = 12,

        [StringValue("Classe divergente da parametrização.")]
        PARAMETRIZACAO_CLASSE = 13,

        [StringValue("Tipo cliente divergente da parametrização.")]
        PARAMETRIZACAO_GIRO = 14,

        [StringValue("Tarifa divergente da parametrização.")]
        PARAMETRIZACAO_TARIFA = 15,

        [StringValue("Ind_local_med divergente da parametrização.")]
        PARAMETRIZACAO_IND_LOCAL_MED = 16,

        [StringValue("Tipo_caixa_med divergente da parametrização.")]
        PARAMETRIZACAO_TIPO_CAIXA_MED = 17

    }

    public class StringValueAttribute : Attribute
    {

        #region Properties

        /// <summary>
        /// Holds the stringvalue for a value in an enum.
        /// </summary>
        public string StringValue { get; protected set; }

        #endregion

        #region Constructor

        /// <summary>
        /// Constructor used to init a StringValue Attribute
        /// </summary>
        /// <param name="value"></param>
        public StringValueAttribute(string value)
        {
            this.StringValue = value;
        }

        #endregion

    }

}

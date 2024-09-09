using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace Pangea.Entidades.DTO
{
    [DataContract]
    public class RetornoAltaContratoDTO
    {
        
        [DataMember] public string MESSAGE { get; set; }

        [DataMember] public string CODE_RETURN { get; set; }
        /// <summary>
        /// Fabrica
        /// </summary>
        [DataMember] public string EQ_HERST { get; set; }
        /// <summary>
        /// Modelo
        /// </summary>
        [DataMember] public string EQ_TYPBZ { get; set; }

        /// <summary>
        /// Unidade de Leitura
        /// </summary>
        [DataMember] public string ABLEINH { get; set; }

        /// <summary>
        /// Sequencia de leitura
        /// </summary>
        [DataMember] public string SEC_LECT { get; set; }

        /// <summary>
        /// Flag telemedido
        /// </summary>
        [DataMember] public string IM_FL_TELEMEDIDO { get; set; }

        #region Valores não utilizados(Passar nulo)
        [DataMember] public string EQ_BAUJJ { get; set; }
        
        [DataMember] public string EQ_EQUNR_I { get; set; }
        
        [DataMember] public string EQ_ACTIVE_RFACT { get; set; }
        
        [DataMember] public string EQ_ACTIVE_TFACT { get; set; }
        
        [DataMember] public string EQ_ACTIVE_EN_QTY { get; set; }
        
        [DataMember] public string EQ_ACTIVE_DEC_QTY { get; set; }
        
        [DataMember] public string EQ_REACTIVE_RFACT { get; set; }
        
        [DataMember] public string EQ_REACTIVE_TFACT { get; set; }
        
        [DataMember] public string EQ_REACTIVE_EN_QTY { get; set; }
        
        [DataMember] public string EQ_REACTIVE_DEC_QTY { get; set; }
        
        [DataMember] public string EQ_DEMANDA_RFACT { get; set; }
        
        [DataMember] public string EQ_DEMANDA_TFACT { get; set; }
        
        [DataMember] public string EQ_DEMANDA_QTY { get; set; }
        
        [DataMember] public string EQ_DEMANDA_DEC_QTY { get; set; }
        
        [DataMember] public string SWERK { get; set; }
        
        [DataMember] public string COUNC { get; set; }
        
        [DataMember] public string STR_SUPPL1 { get; set; }
        
        [DataMember] public string STR_SUPPL2 { get; set; }
        
        [DataMember] public string VBSART { get; set; }
        
        [DataMember] public string ZZ_POD { get; set; }
        
        [DataMember] public string ZWNABR { get; set; }
        
        [DataMember] public string DV_DEVLOC { get; set; }
        
        [DataMember] public string EQ_REGISTER_EA_QTY { get; set; }
        
        [DataMember] public string EQ_REGISTER_ER_QTY { get; set; }
        
        [DataMember] public string EQ_REACTIVE_CFACT { get; set; }
        
        [DataMember] public string EQ_REGISTER_DA_QTY { get; set; }

        [DataMember] public string EQ_DEMANDA_CFACT { get; set; }

        #endregion Valores não utilizados(Passar nulo)

        [DataMember] public string ISTYPE { get; set; }
        /// <summary>
        /// Municipio
        /// </summary>
        [DataMember] public string IM_CITY_CODE { get; set; }
        /// <summary>
        /// Localidade
        /// </summary>
        [DataMember] public string IM_LOCALIZATION { get; set; }
        /// <summary>
        /// Lote
        /// </summary>
        [DataMember] public string IM_LOT { get; set; }
        /// <summary>
        /// Lista
        /// </summary>
        [DataMember] public string IM_LIST { get; set; }
        /// <summary>
        ///Tamnho fachada da residencia     
        /// </summary>
        [DataMember] public string F_TESTADA { get; set; }
        /// <summary>
        /// Complemento do endereço
        /// </summary>

        [DataMember] public string EQ_DATAB { get; set; }
        
        [DataMember] public string EQ_MATNR_I { get; set; }
        
        [DataMember] public string EQ_ZWGRUPPE_I { get; set; }
        
        [DataMember] public string EQ_SERNR { get; set; }
        
        [DataMember] public string EQ_DEVICE_CATEG { get; set; }
        
        [DataMember] public string EQ_ACTIVE_CFACT { get; set; }



        [DataMember] public string EQ_ZWSTANDCE_EAI_1 { get; set; }
        [DataMember] public string EQ_ZWSTANDCE_EAI_2 { get; set; }
        [DataMember] public string EQ_ZWSTANDCE_EAI_3 { get; set; }
        [DataMember] public string EQ_ZWSTANDCE_EAI_4 { get; set; }
        [DataMember] public string EQ_ZWSTANDCE_EAI_5 { get; set; }

        [DataMember] public string EQ_ZWSTANDCE_ERI_1 { get; set; }
        [DataMember] public string EQ_ZWSTANDCE_ERI_2 { get; set; }
        [DataMember] public string EQ_ZWSTANDCE_ERI_3 { get; set; }
        [DataMember] public string EQ_ZWSTANDCE_ERI_4 { get; set; }
        [DataMember] public string EQ_ZWSTANDCE_ERI_5 { get; set; }
        [DataMember] public string EQ_ZWSTANDCE_ERI_6 { get; set; }


        [DataMember] public string EQ_ZWSTANDCE_DAI_1 { get; set; }//registrador 12
        [DataMember] public string EQ_ZWSTANDCE_DAI_2 { get; set; }//registrador 13
        [DataMember] public string EQ_ZWSTANDCE_DAI_3 { get; set; }//registrador 14
        [DataMember] public string EQ_ZWSTANDCE_DAI_4 { get; set; }//registrador 15
        [DataMember] public string EQ_ZWSTANDCE_DAI_5 { get; set; }//registrador 16
        [DataMember] public string EQ_ZWSTANDCE_DAI_6 { get; set; }//registrador 17
        [DataMember] public string EQ_ZWSTANDCE_DAI_7 { get; set; }//registrador 18
        [DataMember] public string EQ_ZWSTANDCE_DAI_8 { get; set; }//registrador 19

        [DataMember] public string EQ_ACTIVE_EN_QTY_EAI_1 { get; set; }
        [DataMember] public string EQ_ACTIVE_DEC_QTY_EAI_1 { get; set; }
        [DataMember] public string EQ_ACTIVE_RFACT_EAI_1 { get; set; }

        [DataMember] public string EQ_ACTIVE_EN_QTY_EAI_2 { get; set; }
        [DataMember] public string EQ_ACTIVE_DEC_QTY_EAI_2 { get; set; }
        [DataMember] public string EQ_ACTIVE_RFACT_EAI_2 { get; set; }

        [DataMember] public string EQ_ACTIVE_EN_QTY_EAI_3 { get; set; }
        [DataMember] public string EQ_ACTIVE_DEC_QTY_EAI_3 { get; set; }
        [DataMember] public string EQ_ACTIVE_RFACT_EAI_3 { get; set; }

        [DataMember] public string EQ_ACTIVE_EN_QTY_EAI_4 { get; set; }
        [DataMember] public string EQ_ACTIVE_DEC_QTY_EAI_4 { get; set; }
        [DataMember] public string EQ_ACTIVE_RFACT_EAI_4 { get; set; }

        [DataMember] public string EQ_ACTIVE_EN_QTY_EAI_5 { get; set; }
        [DataMember] public string EQ_ACTIVE_DEC_QTY_EAI_5 { get; set; }
        [DataMember] public string EQ_ACTIVE_RFACT_EAI_5 { get; set; }

        [DataMember] public string EQ_REACTIVE_EN_QTY_ERI_1 { get; set; }
        [DataMember] public string EQ_REACTIVE_DEC_QTY_ERI_1 { get; set; }
        [DataMember] public string EQ_REACTIVE_RFACT_ERI_1 { get; set; }

        [DataMember] public string EQ_REACTIVE_EN_QTY_ERI_2 { get; set; }
        [DataMember] public string EQ_REACTIVE_DEC_QTY_ERI_2 { get; set; }
        [DataMember] public string EQ_REACTIVE_RFACT_ERI_2 { get; set; }

        [DataMember] public string EQ_REACTIVE_EN_QTY_ERI_3 { get; set; }
        [DataMember] public string EQ_REACTIVE_DEC_QTY_ERI_3 { get; set; }
        [DataMember] public string EQ_REACTIVE_RFACT_ERI_3 { get; set; }

        [DataMember] public string EQ_REACTIVE_EN_QTY_ERI_4 { get; set; }
        [DataMember] public string EQ_REACTIVE_DEC_QTY_ERI_4 { get; set; }
        [DataMember] public string EQ_REACTIVE_RFACT_ERI_4 { get; set; }

        [DataMember] public string EQ_REACTIVE_EN_QTY_ERI_5 { get; set; }
        [DataMember] public string EQ_REACTIVE_DEC_QTY_ERI_5 { get; set; }
        [DataMember] public string EQ_REACTIVE_RFACT_ERI_5 { get; set; }

        [DataMember] public string EQ_REACTIVE_EN_QTY_ERI_6 { get; set; }
        [DataMember] public string EQ_REACTIVE_DEC_QTY_ERI_6 { get; set; }
        [DataMember] public string EQ_REACTIVE_RFACT_ERI_6 { get; set; }

        [DataMember] public string EQ_DEMANDA_QTY_DAI_1 { get; set; }
        [DataMember] public string EQ_DEMANDA_DEC_QTY_DAI_1 { get; set; }
        [DataMember] public string EQ_DEMANDA_RFACT_DAI_1 { get; set; }

        [DataMember] public string EQ_DEMANDA_QTY_DAI_2 { get; set; }
        [DataMember] public string EQ_DEMANDA_DEC_QTY_DAI_2 { get; set; }
        [DataMember] public string EQ_DEMANDA_RFACT_DAI_2 { get; set; }

        [DataMember] public string EQ_DEMANDA_QTY_DAI_3 { get; set; }
        [DataMember] public string EQ_DEMANDA_DEC_QTY_DAI_3 { get; set; }
        [DataMember] public string EQ_DEMANDA_RFACT_DAI_3 { get; set; }

        [DataMember] public string EQ_DEMANDA_QTY_DAI_4 { get; set; }
        [DataMember] public string EQ_DEMANDA_DEC_QTY_DAI_4 { get; set; }
        [DataMember] public string EQ_DEMANDA_RFACT_DAI_4 { get; set; }

        [DataMember] public string EQ_DEMANDA_QTY_DAI_5 { get; set; }
        [DataMember] public string EQ_DEMANDA_DEC_QTY_DAI_5 { get; set; }
        [DataMember] public string EQ_DEMANDA_RFACT_DAI_5 { get; set; }

        [DataMember] public string EQ_DEMANDA_QTY_DAI_6 { get; set; }
        [DataMember] public string EQ_DEMANDA_DEC_QTY_DAI_6 { get; set; }
        [DataMember] public string EQ_DEMANDA_RFACT_DAI_6 { get; set; }

        [DataMember] public string EQ_DEMANDA_QTY_DAI_7 { get; set; }
        [DataMember] public string EQ_DEMANDA_DEC_QTY_DAI_7 { get; set; }
        [DataMember] public string EQ_DEMANDA_RFACT_DAI_7 { get; set; }

        [DataMember] public string EQ_DEMANDA_QTY_DAI_8 { get; set; }
        [DataMember] public string EQ_DEMANDA_DEC_QTY_DAI_8 { get; set; }
        [DataMember] public string EQ_DEMANDA_RFACT_DAI_8 { get; set; }

    }
}

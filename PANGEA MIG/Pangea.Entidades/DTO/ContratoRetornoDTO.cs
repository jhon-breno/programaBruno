using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Entidades.DTO
{
    [DataContract]
    public class ContratoRetornoDTO
    {
        #region PROCESSAR BAJA BR 242
        [DataMember]
        public string CODE_RETURN { get; set; } // Código de Retorno 1) Ok 2) No Ok + Mensaje de Error Dato Faltante

        [DataMember]
        public string MESSAGE { get; set; } // Mensagem de retorno da Solicitud (Ok - No Ok)

        [DataMember]
        public string OPTION_REMOVE_EQ { get; set; } //Retira medidor? S/N

        [DataMember]
        public string EQ_MATNR_I { get; set; } // Material Number actual

        [DataMember]
        public string EQ_SERNR { get; set; } // Device Serial Number

        [DataMember]
        public string OPTION_FIELD_READ { get; set; } // Leitura em campo? S/N

        [DataMember]
        public double EQ_ZWSTANDCE_EAI_1 { get; set; } // GRUPO A: grandes:hislec[lect_terreno_hp]; GRUPO B: hislec[lect_ter_act_hp] OU hislec[lect_fac_act_hp]

        [DataMember]
        public string EQ_ZWSTANDCE_EAI_2 { get; set; } //GRUPO A: grandes:hislec[lectura_activa_hr]; GRUPO B: Vazio

        [DataMember]
        public double EQ_ZWSTANDCE_EAI_3 { get; set; } // GRUPO A: grandes:hislec[lectura_terreno]; GRUPO B: hislec[lectura_terreno] OU hislec[lectura_facturac]

        [DataMember]
        public string EQ_ZWSTANDCE_EAI_4 { get; set; } // VAZIO

        [DataMember]
        public string EQ_ZWSTANDCE_EAI_5 { get; set; } // VAZIO

        [DataMember]
        public double EQ_ZWSTANDCE_ERI_1 { get; set; } // GRUPO A: grandes:hislec[lectura_react_hp]; GRUPO B:Não possui Energia Reativa

        [DataMember]
        public double EQ_ZWSTANDCE_ERI_2 { get; set; } // GRUPO A: grandes:hislec[lectura_reativa_hr]; GRUPO B: Não possui Energia Reativa

        [DataMember]
        public double EQ_ZWSTANDCE_ERI_3 { get; set; } // GRUPO A: grandes:hislec[llectura_react]; GRUPO B: Não possui Energia Reativa

        [DataMember]
        public string EQ_ZWSTANDCE_ERI_4 { get; set; } // VAZIO

        [DataMember]
        public string EQ_ZWSTANDCE_ERI_5 { get; set; } // VAZIO

        [DataMember]
        public string EQ_ZWSTANDCE_ERI_6 { get; set; } // VAZIO

        [DataMember]
        public double EQ_ZWSTANDCE_DAI_1 { get; set; } // GRUPO A: grandes:hisled[DDA_LEID_HP]; GRUPO B:Não possui Energia Demanda

        [DataMember]
        public double EQ_ZWSTANDCE_DAI_2 { get; set; } // GRUPO A: grandes:hisled[DDA_LEID_HR]; GRUPO B:Não possui Energia Demanda

        [DataMember]
        public double EQ_ZWSTANDCE_DAI_3 { get; set; } // GRUPO A: grandes:hisled[DDA_LEID_FP]; GRUPO B:Não possui Energia Demanda

        [DataMember]
        public string EQ_ZWSTANDCE_DAI_4 { get; set; } // VAZIO

        [DataMember]
        public string EQ_ZWSTANDCE_DAI_5 { get; set; } // VAZIO

        [DataMember]
        public string EQ_ZWSTANDCE_DAI_6 { get; set; } // VAZIO

        [DataMember]
        public string EQ_ZWSTANDCE_DAI_7 { get; set; } // VAZIO

        [DataMember]
        public string EQ_ZWSTANDCE_DAI_8 { get; set; } // VAZIO

        #endregion

    }
}

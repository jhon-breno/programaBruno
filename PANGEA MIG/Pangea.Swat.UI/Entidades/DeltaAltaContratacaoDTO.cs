using Newtonsoft.Json;
using Pangea.Entidades.Base;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Text;

namespace Pangea.Swat.UI.Entidades
{
    public class DeltaAltaContratacaoDTO : EntidadeBase
    {
        public DeltaAltaContratacaoDTO()
        {
            foreach (PropertyDescriptor prop in TypeDescriptor.GetProperties(this))
            {
                DefaultValueAttribute attr = (DefaultValueAttribute)prop.Attributes[typeof(DefaultValueAttribute)];
                if (attr != null)
                {
                    prop.SetValue(this, attr.Value);
                }
            }
        }
        [DefaultValue("")]
        public string ZZ_CANALE_STAMPA { get; set; }
        [DefaultValue("")]
        public string MI_VERTRAG { get; set; }
        [DefaultValue("0")]
        public string IM_TRASNF_LOSS { get; set; }
        [DefaultValue("")]
        public string IM_REGION { get; set; }
        [DefaultValue("99999")]
        public string IM_FACTOR_3 { get; set; }
        [DefaultValue("99999")]
        public string IM_FACTOR_2 { get; set; }
        [DefaultValue("")]
        public string IM_FACTOR_1 { get; set; }
        [DefaultValue("")]
        public string BUPLA { get; set; }
        [DefaultValue("9001")]
        public string BPKIND { get; set; }
        [DefaultValue("1")]
        public string AC_VKTYP { get; set; }            //10
        [DefaultValue("1")]
        public string AC_GSBER { get; set; }
        [DefaultValue("")]
        public string AC_EZAWE { get; set; }
        [DefaultValue("")]
        public string AC_ABWRH { get; set; }

        [DefaultValue("")]
        public string ATTIVITA { get; set; }            //14
        [DefaultValue("")]
        public string CLASSE { get; set; }
        [DefaultValue("")]
        public string DATA_VALIDITA { get; set; }
        [DefaultValue("")]
        public string MI_VENDE { get; set; }

        [DefaultValue("10")]
        public string AC_ZAHLKOND { get; set; }         //18
        [DefaultValue("")]
        public string AC_VKONT { get; set; }
        [DefaultValue("")]
        public string AC_KOFIZ_SD { get; set; }
        [DefaultValue("")]
        public string BP_ZZ_CODFISC { get; set; }
        [DefaultValue("")]
        public string TipoIdent { get; set; }
        [DefaultValue("")]
        public string BP_TYPE { get; set; }
        [DefaultValue("")]
        public string BP_TAXTYPE { get; set; }
        [DefaultValue("")]
        public string BP_STREET { get; set; }

        [DefaultValue("")]
        public string BP_SMTP_ADDR { get; set; }        //25
        [DefaultValue("U")]
        public string BP_REGION { get; set; }
        [DefaultValue("60060001")]
        public string BP_POST_CODE1 { get; set; }
        [DefaultValue("")]
        public string BP_PARTNER { get; set; }
        [DefaultValue("")]
        public string BP_EXECUTIVE { get; set; }
        [DefaultValue("")]
        public string BP_OPBUK { get; set; }
        [DefaultValue("")]
        public string BP_NAME_ORG1 { get; set; }
        [DefaultValue("")]
        public string BP_NAME_FIRST { get; set; }
        [DefaultValue("")]
        public string BP_NAME_LAST { get; set; }
        [DefaultValue("")]
        public string BP_HOUSE_NUM1 { get; set; }
        [DefaultValue("CENTRO")]
        public string BP_CITY2 { get; set; }
        [DefaultValue("FORTALEZA")]
        public string BP_CITY1 { get; set; }
        [DefaultValue("")]
        public string BP_BPEXT { get; set; }            //35
        [DefaultValue("")]
        public string BP_ADEXT_ADDR { get; set; }
        [DefaultValue("")]
        public string CO_STREET { get; set; }
        [DefaultValue("")]
        public string CO_REGION { get; set; }
        [DefaultValue("60060002")]
        public string CO_POST_CODE1 { get; set; }
        [DefaultValue("")]
        public string CO_HOUSE_NUM1 { get; set; }
        [DefaultValue("FORTALEZA")]
        public string CO_CITY1 { get; set; }
        [DefaultValue("")]
        public string MODTARIFBR { get; set; }
        [DefaultValue(0)]
        public int IM_ZZ_NUMUTE { get; set; }
        [DefaultValue("")]
        public string IM_TEMP_AREA { get; set; }
        [DefaultValue("")]
        public string IM_TARIFTYP { get; set; }
        [DefaultValue("")]
        public string IM_SPEBENE { get; set; }
        [DefaultValue("")]
        public string IM_GROUP_TENSION { get; set; }
        [DefaultValue("")]
        public string IM_FACTOR_4 { get; set; }
        [DefaultValue("0")]
        public string IM_DI_CONTUL { get; set; }
        [DefaultValue("0")]
        public string IM_DI_CONTRPT { get; set; }       //55
        [DefaultValue("0")]
        public string IM_DI_CONTRFP { get; set; }
        [DefaultValue("0")]
        public string IM_DI_CONTRAT { get; set; }
        [DefaultValue("0")]
        public string IM_DI_CONTPTL { get; set; }
        [DefaultValue("0")]
        public string IM_DI_CONTFPL { get; set; }
        //[DefaultValue("")]
        //public string IM_DI_CONTRGE { get; set; }
        [DefaultValue("")]
        public string IM_CHARGE { get; set; }
        [DefaultValue("")]
        public string IM_BRANCHE { get; set; }
        [DefaultValue("")]
        public string IM_ANLAGE { get; set; }
        [DefaultValue("")]
        public string ID_RICHIESTA_FO { get; set; }
        [DefaultValue("")]
        public string ID_RICHIESTA { get; set; }
        [DefaultValue("")]
        public string ID_CASE_EXTERNAL { get; set; }    //66    

        public DeltaAltaContratacao ToJsonObject()
        {
            DeltaAltaContratacao o = new DeltaAltaContratacao();
            o.Header = new Header();
            o.Body = new Body();

            o.Body.ZZ_CANALE_STAMPA = this.ZZ_CANALE_STAMPA;
            o.Body.MI_VERTRAG = this.MI_VERTRAG;
            o.Body.IM_TRASNF_LOSS = Convert.ToInt32(this.IM_TRASNF_LOSS);
            o.Body.IM_REGION = this.IM_REGION;
            o.Body.IM_FACTOR_3 = this.IM_FACTOR_3;
            o.Body.IM_FACTOR_2 = this.IM_FACTOR_2;
            o.Body.IM_FACTOR_1 = this.IM_FACTOR_1;
            o.Body.BUPLA = this.BUPLA;
            o.Body.BPKIND = this.BPKIND;
            o.Body.AC_VKTYP = this.AC_VKTYP;            //10
            o.Body.AC_GSBER = this.AC_GSBER;
            o.Body.AC_EZAWE = this.AC_EZAWE;
            o.Body.AC_ABWRH = this.AC_ABWRH;

            o.Body.ATTIVITA = this.ATTIVITA;            //14
            o.Body.CLASSE = this.CLASSE;
            o.Body.DATA_VALIDITA = this.DATA_VALIDITA;
            o.Body.MI_VENDE = this.MI_VENDE;

            o.Body.AC_ZAHLKOND = this.AC_ZAHLKOND;         //18
            o.Body.AC_VKONT = this.AC_VKONT;
            o.Body.AC_KOFIZ_SD = this.AC_KOFIZ_SD;
            o.Body.BP_ZZ_CODFISC = this.BP_ZZ_CODFISC;
            o.Body.BP_TYPE = this.BP_TYPE;
            o.Body.BP_TAXTYPE = this.BP_TAXTYPE;
            o.Body.BP_STREET = this.BP_STREET;

            o.Body.BP_SMTP_ADDR = this.BP_SMTP_ADDR;        //25
            o.Body.BP_REGION = this.BP_REGION;
            o.Body.BP_POST_CODE1 = string.IsNullOrEmpty(this.BP_POST_CODE1) ? "60060000" : this.BP_POST_CODE1;
            o.Body.BP_PARTNER = this.BP_PARTNER;
            o.Body.BP_EXECUTIVE = this.BP_EXECUTIVE;
            o.Body.BP_OPBUK = this.BP_OPBUK;
            o.Body.BP_NAME_ORG1 = this.BP_NAME_ORG1;
            o.Body.BP_NAME_FIRST = this.BP_NAME_FIRST;
            o.Body.BP_NAME_LAST = this.BP_NAME_LAST;
            o.Body.BP_HOUSE_NUM1 = this.BP_HOUSE_NUM1;
            o.Body.BP_CITY2 = string.IsNullOrEmpty(this.BP_CITY2) ? "CENTRO" : this.BP_CITY2;
            o.Body.BP_CITY1 = this.BP_CITY1;
            o.Body.BP_BPEXT = this.BP_BPEXT;            //35
            o.Body.BP_ADEXT_ADDR = this.BP_ADEXT_ADDR;
            o.Body.CO_STREET = this.CO_STREET;
            o.Body.CO_REGION = this.CO_REGION;
            o.Body.CO_POST_CODE1 = string.IsNullOrEmpty(this.CO_POST_CODE1) ? "60060000" : this.CO_POST_CODE1;
            o.Body.CO_HOUSE_NUM1 = this.CO_HOUSE_NUM1;
            o.Body.CO_CITY1 = this.CO_CITY1;
            o.Body.MODTARIFBR = this.MODTARIFBR;
            o.Body.IM_ZZ_NUMUTE = this.IM_ZZ_NUMUTE;
            o.Body.IM_TEMP_AREA = this.IM_TEMP_AREA;
            o.Body.IM_TARIFTYP = this.IM_TARIFTYP;
            o.Body.IM_SPEBENE = this.IM_SPEBENE;
            o.Body.IM_GROUP_TENSION = this.IM_GROUP_TENSION;
            o.Body.IM_FACTOR_4 = "monofásica".Equals(this.IM_FACTOR_4.ToLower()) ? "1" : "bifásica".Equals(this.IM_FACTOR_4.ToLower()) ? "2" : "trifásica".Equals(this.IM_FACTOR_4.ToLower()) ? "3" : string.Empty;
            o.Body.IM_DI_CONTUL = Convert.ToInt32(this.IM_DI_CONTUL);
            o.Body.IM_DI_CONTRPT = Convert.ToInt32(this.IM_DI_CONTRPT);       //55
            o.Body.IM_DI_CONTRFP = Convert.ToInt32(this.IM_DI_CONTRFP);
            o.Body.IM_DI_CONTRAT = Convert.ToInt32(this.IM_DI_CONTRAT);
            o.Body.IM_DI_CONTPTL = Convert.ToInt32(this.IM_DI_CONTPTL);
            o.Body.IM_DI_CONTFPL = Convert.ToInt32(this.IM_DI_CONTFPL);
            //o.Body.IM_DI_CONTRGE = this.IM_DI_CONTRGE;
            o.Body.IM_CHARGE = string.IsNullOrWhiteSpace(this.IM_CHARGE) ? -200 : Convert.ToInt32(Convert.ToDecimal(this.IM_CHARGE, CultureInfo.GetCultureInfo("en-US")));
            o.Body.IM_BRANCHE = this.IM_BRANCHE;
            o.Body.IM_ANLAGE = this.IM_ANLAGE;
            o.Body.ID_RICHIESTA_FO = this.ID_RICHIESTA_FO;
            o.Body.ID_RICHIESTA = this.ID_RICHIESTA;
            o.Body.ID_CASE_EXTERNAL = this.ID_CASE_EXTERNAL;    //66   
            o.TipoIdent = this.TipoIdent;
            return o;
        }
    }
}

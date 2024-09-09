using Newtonsoft.Json;
using Pangea.Entidades.Base;
using Pangea.Entidades.Enumeracao;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace Pangea.Swat.UI.Entidades
{
    public class DeltaAltaContratacao : EntidadeBase
    {
        public Header Header { get; set; }

        public Body Body { get; set; }

        [JsonIgnore]
        public string TipoIdent { get; set; }

        [JsonIgnore]
        public TipoDocumento TipoIdentidade
        {
            get
            {
                if ("A".Equals(this.Body.IM_GROUP_TENSION) && ("CPF".Equals(this.TipoIdent) || "C.P.F.".Equals(this.TipoIdent)))
                    return TipoDocumento.CPFGA;

                if ("B".Equals(this.Body.IM_GROUP_TENSION) && ("CPF".Equals(this.TipoIdent) || "C.P.F.".Equals(this.TipoIdent)))
                    return TipoDocumento.CPFGB;

                if (("CNPJ".Equals(this.TipoIdent)))
                    return TipoDocumento.CNPJ;

                return TipoDocumento.NaoIdentificado;
            }
        }

        [JsonIgnore]
        public bool IsPessoaFisica 
        {
            get 
            {
                return (this != null && !string.IsNullOrWhiteSpace(this.Body.BP_ZZ_CODFISC) && 
                    Utils.FormatarDocumento(this.Body.BP_ZZ_CODFISC, this.TipoIdentidade).Length == 11);
            }
        }

        [JsonIgnore]
        public bool IsClienteLivre 
        {
            get 
            {
                return (this != null && !string.IsNullOrWhiteSpace(this.Body.IM_TARIFTYP) && this.Body.IM_TARIFTYP.ToUpper().Contains("LIVRE"));
            }
        }

        /// <summary>
        /// Retorna RJ/CE conforme o campo sociedad_sie2000 da tabela INSTA
        /// </summary>
        [JsonIgnore]
        public string Empresa
        {
            get
            {
                return this != null && !string.IsNullOrEmpty(this.Body.BP_OPBUK) && "2003".Equals(this.Body.BP_OPBUK) ? "CE" : "2005".Equals(this.Body.BP_OPBUK) ? "RJ" : "";
            }
        }
    
    }

    [JsonObject]
    public class Header
    {
        [JsonProperty]
        public string SistemaOrigen { get { return "SFDC"; } }
        [JsonProperty]
        public string Funcionalidad { get { return "Alta"; } }
        [JsonProperty]
        public string FechaHora { get; set; }
        [JsonProperty]
        public string CodSistema { get { return "COESAP"; } }
    }

    [JsonObject]
    public class Body
    {
        public string ZZ_CANALE_STAMPA { get; set; }
        public string MI_VERTRAG { get; set; }
        public int IM_TRASNF_LOSS { get; set; }
        public string IM_REGION { get; set; }
        public string IM_FACTOR_3 { get; set; }
        public string IM_FACTOR_2 { get; set; }
        public string IM_FACTOR_1 { get; set; }
        public string BUPLA { get; set; }
        public string BPKIND { get; set; }
        public string AC_VKTYP { get; set; }            //10
        public string AC_GSBER { get; set; }
        public string AC_EZAWE { get; set; }
        public string AC_ABWRH { get; set; }

        public string ATTIVITA { get; set; }            //14
        public string CLASSE { get; set; }
        public string DATA_VALIDITA { get; set; }
        public string MI_VENDE { get; set; }

        public string AC_ZAHLKOND { get; set; }         //18
        public string AC_VKONT { get; set; }
        public string AC_KOFIZ_SD { get; set; }
        public string BP_ZZ_CODFISC { get; set; }
        public string BP_TYPE { get; set; }
        public string BP_TAXTYPE { get; set; }
        public string BP_STREET { get; set; }

        public string BP_SMTP_ADDR { get; set; }        //25
        public string BP_REGION { get; set; }
        public string BP_POST_CODE1 { get; set; }
        public string BP_PARTNER { get; set; }
        public string BP_EXECUTIVE { get; set; }
        public string BP_OPBUK { get; set; }
        public string BP_NAME_ORG1 { get; set; }
        public string BP_NAME_FIRST { get; set; }
        public string BP_NAME_LAST { get; set; }
        public string BP_HOUSE_NUM1 { get; set; }
        public string BP_CITY2 { get; set; }            //35
        public string BP_CITY1 { get; set; }
        public string BP_BPEXT { get; set; }            
        public string BP_ADEXT_ADDR { get; set; }
        public string CO_STREET { get; set; }
        public string CO_REGION { get; set; }
        public string CO_POST_CODE1 { get; set; }
        public string CO_HOUSE_NUM1 { get; set; }
        public string CO_CITY1 { get; set; }
        [JsonIgnore]
        public string MODTARIFBR { get; set; }
        public int IM_ZZ_NUMUTE { get; set; }        //45
        public string IM_TEMP_AREA { get; set; }
        public string IM_TARIFTYP { get; set; }
        public string IM_SPEBENE { get; set; }
        public string IM_GROUP_TENSION { get; set; }
        public string IM_FACTOR_4 { get; set; }
        public int IM_DI_CONTUL { get; set; }
        public int IM_DI_CONTRPT { get; set; }
        public int IM_DI_CONTRFP { get; set; }
        public int IM_DI_CONTRAT { get; set; }
        public int IM_DI_CONTPTL { get; set; }         //55
        public int IM_DI_CONTFPL { get; set; }
        //public string IM_DI_CONTRGE { get; set; }
        public int IM_CHARGE { get; set; }
        public string IM_BRANCHE { get; set; }
        public string IM_ANLAGE { get; set; }
        public string ID_RICHIESTA_FO { get; set; }
        public string ID_RICHIESTA { get; set; }
        public string ID_CASE_EXTERNAL { get; set; }    //62
    }
}

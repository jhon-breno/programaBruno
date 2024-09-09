using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;

namespace SalesforceExtractor.Entidades
{
    public class ItemAttribute : IDisposable
    {
        public ItemAttribute()
        { 
        
        }

        public string Id { get; set; }

        public string NumeroCliente { get; set; }

        private int cargaKW;

        [DefaultValue("74")]
        [Display(Name = "Carga KW BR")]
        public int CargaKW { get { return this.cargaKW < 1 ? 1 : this.cargaKW; } set { this.cargaKW = value; } }
        
        /// <summary>
        /// ModalidadeTarifaria
        /// </summary>
        [Display(Name = "Modalidade Tarifaria BR", Description = "GA")]
        public string ModalidadeTarifaria { get; set; }
        /// <summary>
        /// CategoriaTarifa
        /// </summary>
        [DefaultValue("B1_RESID - Categoria De Tarifa B1 Residencial")]
        [Display(Name = "Categoria de Tarifa BR")]
        public string CategoriaTarifa { get; set; }
        /// <summary>
        /// Classe
        /// </summary>
        [DefaultValue("10 - Residencial")]
        [Display(Name = "Classe BR")]
        public string Classe { get; set; }
        /// <summary>
        /// SubClasse
        /// </summary>
        [DefaultValue("REPLN - Residencial Pleno")]
        [Display(Name = "SubClasse BR")]
        public string SubClasse { get; set; }
        
        /// <summary>
        /// DemandaKV
        /// </summary>
        [Display(Name = "Demanda KV BR", Description = "GA")]
        public string DemandaKV { get; set; }
        /// <summary>
        /// DemandaPonta
        /// </summary>
        [Display(Name = "Demanda Ponta BR", Description = "GA")]
        public string DemandaPonta { get; set; }
        /// <summary>
        /// DemandaForaPonta
        /// </summary>
        [Display(Name = "Demanda Fora de Ponta BR", Description = "GA")]
        public string DemandaForaPonta { get; set; }

        /// <summary>
        /// CapacidadeDisjuntor.   Grupo B: 0.0 Fixo
        /// </summary>
        [DefaultValue("0")]
        [Display(Name = "Capacidade Disjuntor BR")]
        public virtual string CapacidadeDisjuntor { get; set; }
        /// <summary>
        /// ValorTensao
        /// </summary>
        [DefaultValue("220 V")]
        [Display(Name = "Valor de Tensão BR")]
        public string ValorTensao { get; set; }
        /// <summary>
        /// InstalacaoPadrao
        /// </summary>
        [DefaultValue("Não")]
        [Display(Name = "Instalação Padrão")]
        public string InstalacaoPadrao { get; set; }
        /// <summary>
        /// TipoTensao
        /// </summary>
        [DefaultValue("Monofásica")]
        [Display(Name = "Tipo de Tensão BR")]
        public string TipoTensao { get; set; }
        /// <summary>
        /// PotenciaKWA.  Grupo B:  vazio fixo
        /// </summary>
        [DefaultValue("74")]
        [Display(Name = "Potencia KWA BR")]
        public virtual string PotenciaKWA { get; set; }
        /// <summary>
        /// NivelTensao
        /// </summary>
        [DefaultValue("Baixa Tensão")]
        [Display(Name = "Nivel de Tensão BR")]
        public string NivelTensao { get; set; }

        //[Display(Name = "Nis")]
        //public string Nis { get; set; }

        //[Display(Name = "Nb")]
        //public string Nb { get; set; }

        public string ExternalIdAccount { get; set; }

        public string ExternalIdPod { get; set; }

        public string ExternalIdAsset { get; set; }

        public string ExternalIdContract { get; set; }

        public string OrderItemId { get; set; }

        /// <summary>
        /// NE__DynamicPropertyDefinition__c 
        /// </summary>
        public virtual Dictionary<string, string> dicPropriedadesDinamicas
        {
            get
            {
                return new Dictionary<string,string>()
                {
                    {"Carga KW BR", "a1c1o00000B8SijAAF"},              //74
                    {"Categoria de Tarifa BR", "a1c1o00000B8SivAAF"},   //B1_RESID - Categoria De Tarifa B1 Residencial
                    {"Classe BR", "a1c1o00000B8SiuAAF"},                //10 - Residencial
                    {"SubClasse BR", "a1c1o00000B8SiwAAF"},             //REPLN - Residencial Pleno
                    {"Demanda KV BR", "a1c1o00000B8SikAAF"},
                    {"Demanda Ponta BR", "a1c1o00000B8SisAAF"},
                    {"Demanda Fora de Ponta BR", "a1c1o00000B8SitAAF"},
                    {"Capacidade Disjuntor BR", "a1c1o00000B8SimAAF"},  //0
                    {"Valor de Tensão BR", "a1c1o00000B8SilAAF"},       //220 V
                    {"Instalação Padrão", "a1c1o00000B8SioAAF"},        //Não
                    {"Tipo de Tensão BR", "a1c1o00000B8SiqAAF"},        //Monofásica
                    {"Modalidade Tarifaria BR", "a1c1o00000B8SirAAF"},
                    {"Potencia KWA BR", "a1c1o00000B8SipAAF"},          //74
                    {"Nivel de Tensão BR", "a1c1o00000B8SinAAF"}
                };
            }
        }

        public void Dispose()
        {
            this.NumeroCliente = null;
            this.ModalidadeTarifaria = null;
            this.CategoriaTarifa = null;
            this.Classe = null;
            this.SubClasse = null;
            this.DemandaKV = null;
            this.DemandaPonta = null;
            this.DemandaForaPonta = null;
            this.CapacidadeDisjuntor = null;
            this.ValorTensao = null;
            this.InstalacaoPadrao = null;
            this.TipoTensao = null;
            this.PotenciaKWA = null;
            this.NivelTensao = null;
            this.ExternalIdAccount = null;
            this.ExternalIdPod = null;
            this.ExternalIdAsset = null;
            this.ExternalIdContract = null;
        }
    }
}

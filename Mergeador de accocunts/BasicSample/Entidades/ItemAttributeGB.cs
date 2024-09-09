using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;

namespace SalesforceExtractor.Entidades
{
    public class ItemAttributeGB : ItemAttribute
    {
        public ItemAttributeGB() : base()
        {

        }

        /// <summary>
        /// CapacidadeDisjuntor.   Grupo B: 0.0 Fixo
        /// </summary>
        [Display(Name = "Capacidade Disjuntor BR")]
        public override string CapacidadeDisjuntor { get { return "0";  } }

        /// <summary>
        /// PotenciaKWA.  Grupo B:  vazio fixo
        /// </summary>
        //[Display(Name = "Potencia KWA BR")]
        //public override string PotenciaKWA { get { return this.CargaKW.ToString(); } }

        /// <summary>
        /// NE__DynamicPropertyDefinition__c 
        /// </summary>
        public override Dictionary<string, string> dicPropriedadesDinamicas
        {
            get
            {
                return new Dictionary<string,string>()
                {
                    {"Carga KW BR", "a1c1o00000B8SijAAF"},
                    {"Categoria de Tarifa BR", "a1c1o00000B8SivAAF"},
                    {"Classe BR", "a1c1o00000B8SiuAAF"},
                    {"SubClasse BR", "a1c1o00000B8SiwAAF"},
                    {"Demanda KV BR", "a1c1o00000B8SikAAF"},
                    {"Demanda Ponta BR", "a1c1o00000B8SisAAF"},
                    {"Demanda Fora de Ponta BR", "a1c1o00000B8SitAAF"},
                    {"Capacidade Disjuntor BR", "a1c1o00000B8SimAAF"},
                    {"Valor de Tensão BR", "a1c1o00000B8SilAAF"},
                    {"Instalação Padrão", "a1c1o00000B8SioAAF"},
                    {"Tipo de Tensão BR", "a1c1o00000B8SiqAAF"},
                    {"Modalidade Tarifaria BR", "a1c1o00000B8SirAAF"},
                    {"Potencia KWA BR", "a1c1o00000B8SipAAF"},
                    {"Nivel de Tensão BR", "a1c1o00000B8SinAAF"}
                };
            }
        }
    }
}

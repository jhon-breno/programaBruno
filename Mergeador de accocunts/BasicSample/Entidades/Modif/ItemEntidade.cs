using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace SalesforceExtractor.Entidades.Modif
{
    /// <summary>
    /// Representa os dados da tabela MODIF e sua correspondência no Salesforce.
    /// </summary>
    [DebuggerDisplay("{CodigoModif} - {Label} - {CampoSalesforce}")]  
    public class ItemEntidade
    {
        #region Propriedades Privadas
        private int codigoModif;
        private int dicionarioInternoValor;
        private string campoModif;
        private string campoSalesforce;
        private string label;
        private string novoValor;
        #endregion

        public ItemEntidade(int codigoModif, int dicionarioInternoValor, string labelSalesforce, string campoModif, string campoSalesForce)
        {
            this.codigoModif = codigoModif;
            this.dicionarioInternoValor = dicionarioInternoValor;
            this.campoModif = campoModif;
            this.campoSalesforce = campoSalesForce;
            this.label= labelSalesforce;
        }

        #region Propriedades Públicas
        public string CampoModif { get { return this.campoModif; } }
        public string CampoSalesforce { get { return this.campoSalesforce; } }
        public string Label { get { return this.label; }  }
        public int CodigoModif { get { return this.codigoModif; } }
        public int DicionarioInternoValor { get { return this.dicionarioInternoValor; } }
        public string NovoValor
        {
            get
            {
                return this.novoValor;
            }

            set
            {
                if(this.codigoModif == 46 || this.codigoModif == 47)
                {
                    if (!string.IsNullOrWhiteSpace(value))
                    {
                        this.novoValor = ("V".Equals(value.ToUpper())) ? value : "N";
                    }
                }
                else
                {
                    this.novoValor = value;
                }
            }
        }
        #endregion
    }
}

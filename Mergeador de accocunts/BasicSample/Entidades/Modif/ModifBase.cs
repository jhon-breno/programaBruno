using SalesforceExtractor.Entidades.Enumeracoes;
using SalesforceExtractor.Utils;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;

namespace SalesforceExtractor.Entidades.Modif
{
    [DebuggerDisplay("{EntidadeSalesforce} - Identificado: {Identificador}[{DicionarioInternoId}]")]  
    public class ModifBase
    {
        public string EntidadeSalesforce { get; private set; }
        public int DicionarioInternoId { get; set; }
        public string Identificador { get; set; }
        public int IdDicionarioIdentificador { get; set; }

        //public abstract List<ItemEntidade> GetEntidades(TipoCliente tipoCliente);
        public List<ItemEntidade> ItemsModificados { get; set; }

        public ModifBase(string entidadeSalesForce, int dicionarioInternoId) //, string campoModif, string campoSF, string valor)
        {
            this.EntidadeSalesforce = entidadeSalesForce;
            this.DicionarioInternoId = dicionarioInternoId;
            this.ItemsModificados = new List<ItemEntidade>();
        }


        /// <summary>
        /// Retorna as propriedades das classes ModifBase que possuam o atributo ModifAttribute definido.
        /// </summary>
        /// <param name="grupoCliente">A|B (Padrão)</param>
        /// <returns></returns>
        public List<ItemEntidade> GetEntidades(string grupoCliente)
        {
            TipoCliente tipoCliente = "A".Equals(grupoCliente) ? TipoCliente.GA : TipoCliente.GB;

            List<ItemEntidade> listaEntidades = new List<ItemEntidade>();

            PropertyInfo[] listainfo = this.GetType().GetProperties().Where(propInfo => propInfo.GetCustomAttributes(typeof(ModifAttribute), true).Length > 0).ToArray();
            foreach (PropertyInfo prop in listainfo)
            {
                if (((ModifAttribute)((Attribute)prop.GetCustomAttributes(false)[0])).TipoCliente == TipoCliente.Ambos || 
                    tipoCliente == ((ModifAttribute)((Attribute)prop.GetCustomAttributes(false)[0])).TipoCliente)
                {
                    listaEntidades.Add((ItemEntidade)prop.GetValue(this, null));
                }
            }

            //listaEntidades.Add(this.Nome);
            //listaEntidades.Add(this.Endereco);
            //listaEntidades.Add(this.Telefone1);
            //listaEntidades.Add(this.Telefone2);
            //listaEntidades.Add(this.Telefone3);
            //listaEntidades.Add(this.Cep);
            //listaEntidades.Add(this.OrigemCliente);
            
            return listaEntidades;
        }
    }
}

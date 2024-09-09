using Pangea.Entidades.Base;
using Pangea.Entidades.Enumeracao;
using Pangea.Util;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace Pangea.Entidades
{
    [DebuggerDisplay("{Numero_cliente}/{Cliente_anterior} - Despers.SF:{DespersonaSalesforce}")]  
    public class Cliente : EntidadeBase
    {
        #region Propriedades Relacionadas aos campos da tabela CLIENTE
        public string Empresa { get; set; }
        public string CodigoEmpresa { get { return "RJ".Equals(this.Empresa) ? "2005" : "CE".Equals(this.Empresa) ? "2003" : "EMPRESA"; } }
        public string Numero_cliente { get; set; }
        public string Cliente_anterior { get; set; }
        public string Nombre { get; set; }
        public string Estado_cliente { get; set; }
        public string Ind_cliente_vital { get; set; }
        public string Rut { get; set; }
        public string Dv_rut { get; set; }
        public string Tipo_Ident { get; set; }
        public string Estado_suministro { get; set; }
        public string Estado_facturacion { get; set; }
        public string Sector { get; set; }
        public string Corr_corte { get; set; }
        public int Municipio { get; set; }
        public string Direccion { get; set; }
        public int Localidade { get; set; }
        public string Codigo_logra { get; set; }
        public string Complemento { get; set; }
        public int Numero_casa { get; set; }
        public DateTime fecha_a_corte { get; set; }
        public string sucursal { get; set; }
        public string tiene_notific { get; set; }
        public DateTime fecha_notifica { get; set; }
        public int corr_reaviso { get; set; }
        public string ClientePai { get; set; }

        /// <summary>
        /// Indica se o cliente é Despersonalizado para fins de cadastro no Salesforce.
        /// </summary>
        public bool DespersonaSalesforce
        {
            get { return string.IsNullOrWhiteSpace(this.Nombre) || this.Nombre.Contains("PROCURE A ") || this.Nombre.Contains("CONSUMIDOR ATUALIZE") || this.Nombre.Contains("PROCURAR A ") || this.Nombre.Contains("CLIENTE PROC") || this.Nombre.Contains("CLIENTE NAO "); }
        }

        public TipoCliente Tipo {
            get { return ("8").Equals(this.Estado_cliente) ? TipoCliente.GA : TipoCliente.GB; }
        }
        public TipoDocumento TipoDocumento
        {
            get { return ("002").Equals(this.Tipo_Ident) ? 
                TipoDocumento.CNPJ : 
                (this.Tipo == TipoCliente.GA ? 
                    ("003").Equals(this.Tipo_Ident) ? 
                        TipoDocumento.CPFGA : 
                        TipoDocumento.NaoIdentificado 
                    : ("005").Equals(this.Tipo_Ident) ? 
                        TipoDocumento.CPFGB : 
                        ("006").Equals(this.Tipo_Ident) ? 
                            TipoDocumento.CPFGB2 : 
                            TipoDocumento.NaoIdentificado); }
        }
        #endregion


        public string ind_cliente_vital { get; set; }
        public string Ind_cli_despersona { get; set; }
        /// <summary>
        /// SALES_GERAL.APELIDO
        /// </summary>
        public string Apelido { get; set; }


        #region Propriedades Públicas
        /// <summary>
        /// Retorna o endereço com a rua, número e complemento.
        /// </summary>
        public string EnderecoAgrupado
        {
            get
            {
                return string.Format("{0}, {1}, {2}",
                    string.IsNullOrEmpty(this.Direccion)   ? string.Empty : this.Direccion.Trim(),
                    Numero_casa <= 0                       ? string.Empty : Numero_casa.ToString(),
                    string.IsNullOrEmpty(this.Complemento) ? string.Empty : this.Complemento);
            }
        }


        /// <summary>
        /// Flag que indica se o cliente está configurado como despersonalizado (ind_cli_despersona).
        /// </summary>
        public bool FlDespersonalizado
        {
            get
            {
                return "S".Equals(this.Ind_cli_despersona);
            }
        }

        /// <summary>
        /// Flag que indica se o cliente foi notificado com reaviso (tiene_notific).
        /// </summary>
        public bool FlClienteNotificado
        {
            get
            {
                return "S".Equals(this.tiene_notific);
            }
        }
        #endregion
    }
}

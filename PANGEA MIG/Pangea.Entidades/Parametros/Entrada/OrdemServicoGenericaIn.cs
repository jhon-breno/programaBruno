using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Pangea.Entidades.Enumeracao;
using System.ComponentModel.DataAnnotations;
using Pangea.Entidades.DTO;
using System.Runtime.Serialization;

namespace Pangea.Entidades.Parametros.Entrada
{
    /// <summary>
    /// 
    /// </summary>
    /// <remarks>Conforme o Acordo de Interface da Integração, haverá mudanças nas propriedades do parâmetro de entrada.</remarks>
    [Serializable]
    [DataContract]
    public class OrdemServicoGenericaIn : ValidationAttribute
    {
        private int _auxNumeroCliente = 0;

        public OrdemServicoGenericaIn(string Empresa, string NumeroCliente, string NumeroOrdemOriginal, string Observacao, string Solicitante, string CanalAtendimento, string Telefone = "", string NumeroGac = "", string Estado = "", string Descricao = "", string Motivo = "", string SubMotivo = "", string Endereco = "", string ReferenciaEndereco = "", string Municipio = "", string UsuarioIngresso = "", string NumeroDocumento = "", string TipoCliente = "", string EnderecoSolicitante = "")
        {
            this.CodigoEmpresa = Empresa;
            if(Int32.TryParse(NumeroCliente, out _auxNumeroCliente))
                this.NumeroCliente = _auxNumeroCliente.ToString();
            this.NumeroOrdemOriginal = NumeroOrdemOriginal;
            this.Observacao = Observacao;
            this.Solicitante = Solicitante;
            this.NumeroGac = NumeroGac;
            this.CanalAtendimento = CanalAtendimento;
            this.Motivo = Motivo;
            this.SubMotivo = SubMotivo;
            this.UsuarioIngresso = UsuarioIngresso;
            this.Descricao = Descricao;     //TODO:urgente

            //TODO: aguardar definição sobre como será a distribuição dos dados nessa string;  dados que o SalesForce não tratará como propriedades
            //this.Descricao = new Descricao(); 
            ////this.Descricao.IdServicoIntegracao = IdServicoIntegracao;
            ////this.Descricao.ListaTelefones = Telefones;
            //this.Descricao.Endereco = Endereco;
            //this.Descricao.ReferenciaEndereco = ReferenciaEndereco;
            //this.Descricao.NumeroDocumento = NumeroDocumento;
            //this.Descricao.TipoCliente = TipoCliente;
            //this.Descricao.EnderecoSolicitante = EnderecoSolicitante;
            //this.CanalAtendimento = CanalAtendimento;
            //this.Descricao.Municipio = Municipio;
            //if (string.IsNullOrEmpty(Municipio))
            //    this.Descricao.Municipio = Municipio;
        }

        [Required(ErrorMessage="Obrigatório informar a Empresa.")]
        [DataMember]
        public string CodigoEmpresa { get; set; }
        
        [Required(ErrorMessage = "Obrigatório informar o Solicitante da ordem de serviço.")]
        [DataMember]
        public string Solicitante { get; set; }
        
        [Required]
        [DataMember]
        public string NumeroGac { get; set; }

        [Required(ErrorMessage = "Obrigatório informar o Canal de Atendimento para o ingresso de ordem de serviço.")]
        [DataMember]
        public string CanalAtendimento { get; set; }

        [Required(ErrorMessage = "Obrigatório informar o Motivo da ordem de serviço.")]
        [DataMember]
        public string Motivo { get; set; }

        [Required]
        [DataMember]
        public string SubMotivo { get; set; }
        
        [DataMember]
        public string NumeroCliente { get; set; }

        [DataMember]
        public string NumeroOrdemOriginal { get; set; }

        /// <summary>
        /// Nome, Endereço, Municipio, Telefone, Documento, Tipo documento
        /// </summary>
        [DataMember]
        public string Descricao { get; set; }
        
        //[Required(ErrorMessage = "Obrigatório informar uma observação.")]
        [DataMember]
        public string Observacao { get; set; }
        
        [DataMember]
        public string UsuarioIngresso { get; set; }

        //[Required(ErrorMessage = "Obrigatório informar o Tipo de Ordem.")]

        [DataMember]
        public string CodOrdem { get; set; }

        //[Required(ErrorMessage = "Obrigatório informar o Tipo de Serviço.")]
        [DataMember]
        public string CodServico { get; set; }
    }
}


//[Serializable]
//[DataContract]
//public class Descricao
//{
//    [DataMember]
//    public string Endereco { get; set; }

//    [DataMember]
//    public string NumeroDocumento { get; set; }

//    [DataMember]
//    public string TipoCliente { get; set; }

//    [DataMember]
//    public string EnderecoSolicitante { get; set; }

    //[Required(ErrorMessage = "Obrigatório informar o Identificador do Serviço para a Integração.")]
    //[DataMember]
    //public int IdServicoIntegracao { get; set; }

    //[DataMember]
    //public List<TelefoneDTO> ListaTelefones { get; set; }
//}
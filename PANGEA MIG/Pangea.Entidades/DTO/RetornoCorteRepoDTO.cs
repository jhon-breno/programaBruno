using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using Pangea.Entidades;

namespace Entidades.DTO
{
    [DataContract]
    public class RetornoCorteRepoDTO
    {
        public RetornoCorteRepoDTO()
        {
            ListaRespuesta = new List<RespuestaCorteRepoDTO>();
        }

        public RetornoCorteRepoDTO(string mensagem)
        {
            //MensagemRetorno retorno = new MensagemRetorno(ValidacoesSynergia.P00000x00000);
            //this.CodigoResultado = retorno.CodigoMensagem;
            this.DescripcionResultado = mensagem;
        }


        [DataMember]
        public List<RespuestaCorteRepoDTO> ListaRespuesta { get; set; }
        
        [DataMember]
        [Required]
        [MaxLengthAttribute(12)]
        public string CodigoResultado { get; set; }
        
        
        [DataMember]
        [Required]
        [MaxLengthAttribute(100)]
        public string DescripcionResultado { get; set; }
    }
}

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Pangea.Entidades.DTO
{
    [Serializable]
    [DataContract]
    public class TelefoneDTO
    {
        [DataMember]
        public int numero_cliente { get; set; }

        [DataMember]
        [MaxLength(2, ErrorMessage = "O tamanho máximo do campo tipo telefone é de {1} caracteres.")]
        public string tipo_telefone { get; set; }
        
        [DataMember]
        [MaxLength(2, ErrorMessage = "O tamanho máximo do campo prefixo é de {1} caracteres.")]
        public string prefixo_ddd { get; set; }
        
        [DataMember]
        [MaxLength(10, ErrorMessage = "O tamanho máximo do campo telefone é de {1} caracteres.")]
        public string numero_telefone { get; set; }
        
        [DataMember]
        [MaxLength(1, ErrorMessage = "O tamanho máximo do campo ind contato é de {1} caracter.")]
        public string ind_contato { get; set; }
        
        [DataMember]
        [MaxLength(5, ErrorMessage = "O tamanho máximo do campo ramal é de {1} caracteres.")]
        public string ramal { get; set; }
    }
}

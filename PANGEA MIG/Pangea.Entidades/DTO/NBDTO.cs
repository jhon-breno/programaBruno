using Pangea.Entidades.Enumeracao;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

namespace Pangea.Entidades.DTO
{
    [DataContract]
    public class NBDTO
    {
        [Required(ErrorMessage = "O NumeroNB é obrigatório")]
        [DataMember]
        public string NumeroNB { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "O NumeroCliente é obrigatório")]
        [DataMember]
        public int NumeroCliente { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "O CodigoEmpresa é obrigatório")]
        [DataMember]
        public int CodigoEmpresa { get; set; }

        [DataMember]
        public string dta_ativacao { get; set; }

        [DataMember]
        public string dta_desativacao { get; set; }
    }
}

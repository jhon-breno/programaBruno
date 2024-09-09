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
    public class NisDTO
    {
        [DataMember]
        public string codigo_familiar { get; set; }

        [DataMember]
        public string data_de_vigencia { get; set; }

        [DataMember]
        public string ind_indigena { get; set; }

        [DataMember]
        public string ind_quilombola { get; set; }

        [DataMember]
        public string valor_salario { get; set; }

        [DataMember]
        public string valor_medio { get; set; }

        [DataMember]
        public string ind_per_capita { get; set; }

        [DataMember]
        public string ind_a_vencer { get; set; }

        [DataMember]
        public string data_atual { get; set; }

        [DataMember]
        public string qtd_dias { get; set; }

        [DataMember]
        public string classe { get; set; }

        [DataMember]
        public string dta_ativacao { get; set; }

        [DataMember]
        public string dta_desativacao { get; set; }

        [DataMember]
        public string NumeroNis { get; set; }

        [DataMember]
        public string NumeroNB { get; set; }
        
        [Range(1, int.MaxValue, ErrorMessage = "O CodigoEmpresa é obrigatório")]
        [DataMember]
        public int CodigoEmpresa { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "O NumeroCliente é obrigatório")]
        [DataMember]
        public int NumeroCliente { get; set; }
    }
}

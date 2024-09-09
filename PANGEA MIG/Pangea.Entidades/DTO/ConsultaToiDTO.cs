using Pangea.Entidades.Base;
using Pangea.Entidades.Enumeracao;
using System.ComponentModel.DataAnnotations;

namespace Pangea.Entidades.DTO
{
    public class ConsultaToiDTO : EntidadeBase
    {
        [Required(AllowEmptyStrings = false, ErrorMessage = "{0} deve ser informado.")]
        [EnumDataType(typeof(Empresa), ErrorMessage = "Informe um {0} válido.")]
        public string CodigoEmpresa { get; set; }

        //[Required(ErrorMessage = "{0} deve ser informado.")]
        [Range(1, int.MaxValue, ErrorMessage = "Informe um {0} válido.")]
        public int NumeroCliente { get; set; }

        //public string Resultado { get; set; }
    }
}
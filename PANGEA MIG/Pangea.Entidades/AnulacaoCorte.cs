using Pangea.Entidades.Enumeracao;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pangea.Entidades
{
    [Serializable]
    public class AnulacaoCorte
    {
        [Required(ErrorMessage="Favor informar a empresa.", AllowEmptyStrings=false)]
        public string codigo_empresa { get; private set; }
        public string numero_cliente { get; private set; }
        

        /// <summary>
        /// 
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="numeroCliente"></param>
        public AnulacaoCorte(Empresa empresa, string numeroCliente)
        {
            this.codigo_empresa = ((int)empresa).ToString();
            this.numero_cliente = numeroCliente;
        }
    }

}

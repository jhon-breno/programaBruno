using Pangea.Entidades.Enumeracao;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace Pangea.Entidades.DTO
{
    [Serializable]
    public class DocumentoDesconexionDTO
    {
        [Required(ErrorMessage = "O campo empresa é obrigatório", AllowEmptyStrings = false )]
        public string empresa  { get; private set; }
        
        [Required(ErrorMessage = "O campo numero_livro é obrigatório", AllowEmptyStrings = false)]
        public string numero_livro { get; private set; }

        [Required(ErrorMessage = "O campo sucursal é obrigatório", AllowEmptyStrings = false)]
        public string sucursal { get; set; }

        [Required(ErrorMessage = "O campo tipo_corte é obrigatório", AllowEmptyStrings = false)]
        public string tipo_corte { get; set; }

        [Required(ErrorMessage = "O campo empreiteira é obrigatório", AllowEmptyStrings = false)]
        public string empreiteira { get; set; }

        [Required(ErrorMessage = "O campo opcao é obrigatório", AllowEmptyStrings = false)]
        public int opcao { get; set; }

        [Required(ErrorMessage = "O campo g_rol é obrigatório", AllowEmptyStrings = false)]
        public string g_rol { get; set; }

        [Required(ErrorMessage = "O campo dir_ip é obrigatório", AllowEmptyStrings = false)]
        public string dir_ip { get; set; }

        [Required(ErrorMessage = "O campo rede_dat é obrigatório", AllowEmptyStrings = false)]
        public string rede_dat { get; set; }

        public string charNomArq { get; set; }

        public string lote { get; set; }

        public string localidade { get; set; }
        
        public string zona { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="numeroLivro"></param>
        public DocumentoDesconexionDTO(string empresa, string numeroLivro)
        {
            this.empresa = empresa;
            this.numero_livro = numeroLivro;
        }
        public DocumentoDesconexionDTO(string cod_empresa, string numero_livro, string sucursal, string tipo_corte, string empreiteira, int opcao, string g_rol, string dir_ip, string rede_dat, string charNomArq,string lote, string localidade, string zona) {
            this.empresa = cod_empresa;
            this.numero_livro = numero_livro;
            this.sucursal = sucursal;
            this.tipo_corte = tipo_corte;
            this.empreiteira = empreiteira;
            this.opcao = opcao;
            this.g_rol = g_rol;
            this.dir_ip = dir_ip;
            this.rede_dat = rede_dat;
            this.charNomArq = charNomArq;
            this.lote = lote;
            this.localidade = localidade;
            this.zona = zona;
        }
    }

}

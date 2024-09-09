using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pangea.Entidades.DTO
{
    [Serializable]
    public class DocumentoDTO
    {
        //Dados para Atualizar Documentos do Cliente
        public int    numero_cliente { get; set; }

        //[MaxLength(3, ErrorMessage = "O tamanho máximo do campo tipo documento é de {1} caracteres permitidos.")]
        public string tipo_documento { get; set; }

        //[MaxLength(20, ErrorMessage = "O tamanho máximo do campo número documento é de {1} caracteres permitidos.")]
        public string numero_doc { get; set; }

        //[MaxLength(2, ErrorMessage = "O tamanho máximo do campo dv documento é de {1} caracteres permitidos.")]
        public string dv_documento { get; set; }

        //[MaxLength(6, ErrorMessage = "O tamanho máximo do campo complemento documento é de {1} caracteres permitidos.")]
        public string compl_documento{ get; set; }
        public string data_emissao { get; set; }
        public int    sequencial { get; set; }

        //[MaxLength(2, ErrorMessage = "O tamanho máximo do campo UF é de {1} caracteres permitidos.")]
        public string uf { get; set; }

        //[MaxLength(1, ErrorMessage = "O tamanho máximo do campo tipo documento é de {1} caracter permitido.")]
        public string foto { get; set; }

        //[MaxLength(1, ErrorMessage = "O tamanho máximo do campo tipo documento é de {1} caracter permitido.")]
        public string valida_org_emis { get; set; }

        public DocumentoDTO() 
        {
            numero_cliente = 0;
            tipo_documento = "NULL";
            numero_doc = "NULL";
            dv_documento = "NULL";
            compl_documento= "NULL";
            data_emissao = "NULL";
            sequencial = 0;
            uf = "NULL";
            foto = "NULL";
            valida_org_emis = "NULL";
        }
       
    }
}

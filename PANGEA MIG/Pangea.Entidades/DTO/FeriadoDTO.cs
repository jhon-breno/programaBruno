using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Pangea.Entidades;

namespace Entidades.DTO
{
    public class FeriadoDTO
    {
        /// <summary>
        /// Data do Feriado que será lida do Arquivo.
        /// </summary>
        public string dataDoFeriado { get; set; }
        
        /// <summary>
        /// Código do município que será recuperado do banco de dados a partir da sigla do município.
        /// </summary>
        public string codigoDoMunicipio{ get; set; }

        /// <summary>
        /// Sigla do Feriado que será lida do Arquivo.
        /// </summary>
        public string siglaDoMunicipio{ get; set; }

        /// <summary>
        /// Número da linha lida do Arquivo.
        /// </summary>
        public int numeroDaLinhaNoArquivo { get; set; }

        /// <summary>
        /// Objeto ErroArquivo que representa o erro na transformação da linha do Arquivo para o FeriadoDTO.
        /// </summary>
        public ErroArquivo erro { get; set; }
       
        /// <summary>
        /// Nome do arquivo lido.
        /// </summary>
        public static string nomeDoArquivo { get; set; }
 
        /// <summary>
        /// Construtor.
        /// </summary>
        public FeriadoDTO()
        {
        }

        /// <summary>
        /// Construtor Sobrecarregado.
        /// </summary>
        /// <param name="siglaDoMunicipio"></param>
        /// <param name="codigoDoMunicipio"></param>
        /// <param name="dataDoFeriado"></param>
        public FeriadoDTO(string siglaDoMunicipio, string codigoDoMunicipio, string dataDoFeriado)
        {
            this.codigoDoMunicipio = codigoDoMunicipio;
            this.siglaDoMunicipio = siglaDoMunicipio;
            this.dataDoFeriado = dataDoFeriado;
        }
    }

}

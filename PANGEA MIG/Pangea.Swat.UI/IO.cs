using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;

namespace Pangea.Swat.UI
{
    public class IO
    {
        /// <summary>
        /// Escreve um determinado texto em um arquivo informado.\nNão lança exceção.
        /// </summary>
        /// <param name="nomeArquivo"></param>
        /// <param name="conteudo"></param>
        /// <param name="extensao">[Opcional] Extensão do arquivo a ser gravado.  TXT é o padrão.</param>
        /// <param name="encode">[Opcional] Codificação do conteúdo a ser gravado.  Encoding.ASCII é o padrão.</param>
        /// <returns></returns>
        public static bool EscreverArquivo(string nomeArquivo, string conteudo, Encoding encode, string extensao = "txt")
        {
            if (string.IsNullOrEmpty(nomeArquivo))
                return false;

            if (encode == null)
                encode = Encoding.UTF8;

            try
            {
                using (System.IO.StreamWriter arquivo = new System.IO.StreamWriter(
                    string.Format("{0}.{1}", nomeArquivo.Trim(), extensao)
                    , true
                    , encode)
                ) {
                    arquivo.WriteLine(conteudo);
                    arquivo.Close();
                }
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }
    }
}

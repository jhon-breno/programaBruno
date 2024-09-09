using SalesforceExtractor.Entidades;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;

namespace SalesforceExtractor
{
    public class IO
    {
        public static void EscreverArquivo(string nomeArquivo, string conteudo, string extensao = "txt")
        {
            try
            {
                using (System.IO.StreamWriter file2 = new System.IO.StreamWriter(
                    string.Format("{0}.{1}", nomeArquivo, extensao)
                    , true
                    , Encoding.ASCII)
                )
                {
                    file2.WriteLine(conteudo);
                }
            }
            catch (Exception ex)
            { }
        }


        public static void EscreverArquivo(Arquivo arquivo, string conteudo)
        {
            try
            {
                using (System.IO.StreamWriter file2 = new System.IO.StreamWriter(
                    arquivo.CaminhoCompleto
                    , true
                    , Encoding.UTF8)
                )
                {
                    file2.Write(conteudo);
                }
            }
            catch (Exception ex)
            {
 
            }
        }
    }
}

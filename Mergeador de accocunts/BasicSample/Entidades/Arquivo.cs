
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace SalesforceExtractor.Entidades
{
    public class Arquivo : IDisposable
    {
        private FileInfo _arquivo = null;
        /// <summary>
        /// Diretório onde encontra-se o arquivo de entrada, terminado com '/'
        /// </summary>
        public string Caminho { get; set; }
        /// <summary>
        /// Nome do arquivo de entrada, sem a extensão
        /// </summary>
        public string Nome { get; set; }
        /// <summary>
        /// Extensão do arquivo de entrada, sem o '.'
        /// </summary>
        public string Extensao { get; set; }
        /// <summary>
        /// Concatenação do Nome e Extensão
        /// </summary>
        public string NomeExtensao
        {
            get { return string.Format("{0}.{1}", this.Nome, this.Extensao); }
        }
        public bool TemCabecalho { get; set; }
        /// <summary>
        /// Concatenação do Caminho, Nome e Extensão
        /// </summary>
        public string CaminhoCompleto
        {
            get { return this._arquivo.FullName; }
        }

        public char Separador { get; set; }

        public int QuantidadeLinhas { get; set; }

        
        
        public Arquivo(string fullPath, char separador = ',', bool temCabecalho = true)
        {
            FileInfo arq = new FileInfo(fullPath.Replace("\"", string.Empty));

            if (!arq.Exists)
            {
                try
                {
                    FileStream fs = File.Create(fullPath);
                    fs.Close();
                    this._arquivo = new FileInfo(fullPath);
                }
                catch (Exception ex2)
                { }
            }
            string[] partes = arq.Name.Split('.');

            this.Caminho = arq.DirectoryName;
            this.Nome = arq.Name.Replace(string.Concat(".", partes[partes.Length - 1]), "");
            this.Separador = separador;
            this.Extensao = partes[partes.Length - 1];
            this._arquivo = arq;
            this.TemCabecalho = temCabecalho;
        }


        public Arquivo(string caminho, string nome, string extensao, char separador = ',', bool temCabecalho = true)
        {
            this.Caminho = caminho;
            this.Nome = nome;
            this.Extensao = extensao;
            this.Separador = separador;
            this.TemCabecalho = temCabecalho;

            this._arquivo = new FileInfo(string.Format("{0}\\{1}.{2}", this.Caminho, this.Nome, this.Extensao));
        }

        public void Dispose()
        {
            this._arquivo = null;
            this.Caminho = null;
            this.Nome = null;
            this.Extensao = null;
            this.Separador = new char();
        }
    }
}

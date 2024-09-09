using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SalesforceExtractor.Utils
{
    public class Log
    {
        private List<string> logs = new List<string>();
        private string nomeArquivo;

        public string NomeArquivo
        {
            get { return nomeArquivo; }
            set { nomeArquivo = value; }
        }


        public Log()
        {

        }

        public Log(Log log)
        {
            this.nomeArquivo = log.NomeArquivo;
        }

        public Log(string nomeArquivo)
        {
            this.nomeArquivo = nomeArquivo;
        }


        public int Tamanho { get { return this.logs.Count(); } }


        /// <summary>
        /// Insere na lista interna mensagens a serem exibidas posteriormente.
        /// </summary>
        /// <param name="conteudo"></param>
        public void Add(string conteudo)
        {
            LogFull(conteudo, true, false, false);
        }

        /// <summary>
        /// Insere na lista interna mensagens a serem exibidas posteriormente.
        /// </summary>
        /// <param name="conteudo"></param>
        public void AddPrint(string conteudo)
        {
            LogFull(conteudo, true, true, false);
        }


        public void AddRange(List<string> conteudo)
        {
            LogFull(conteudo, true, false, false);
        }

        /// <summary>
        /// Grava a mensagem no arquivo de saída mas não a imprime no console.
        /// </summary>
        /// <param name="conteudo"></param>
        public void Write(string conteudo)
        {
            LogFull(conteudo, true, false, true);
        }

        /// <summary>
        /// Exibe a mensagem no console mas não a adiciona o item ao Log nem grava o arquivo de saída.
        /// </summary>
        /// <param name="conteudo"></param>
        public void Print(string conteudo)
        {
            LogFull(conteudo, false, true, false);
        }


        public void LogFull(string conteudo)
        {
            LogFull(conteudo, true ,true, true);
        }

        /// <summary>
        /// Armazena a mensagem na lista interna, exibe-a no console e também grava-a no arquivo de saída.
        /// </summary>
        /// <param name="conteudo"></param>
        /// <param name="printMessage"></param>
        /// <param name="escreverArquivo"></param>
        private void LogFull(string conteudo, bool addMessage = true, bool printMessage = true, bool escreverArquivo = true)
        {
            if (addMessage)
                this.logs.Add(conteudo);

            if (printMessage)
                Console.WriteLine(conteudo);

            if (escreverArquivo)
                EscreverArquivo(conteudo);
        }


        public void LogFull(List<string> conteudo)
        {
            LogFull(conteudo, true, true, true);
        }

        private void LogFull(List<string> conteudo, bool addMessage = true, bool printMessage = true, bool escreverArquivo = true)
        {
            if(addMessage)
                this.logs.AddRange(conteudo);

            if (printMessage)
                Console.WriteLine("Funcionalidade a implementar: imprimir msg via thread.");

            if (escreverArquivo)
                EscreverArquivo(conteudo);
        }

        
        /// <summary>
        /// Recupera a lista interna de mensagens inseridas.
        /// </summary>
        /// <returns></returns>
        public List<string> GetLog()
        {
            return this.logs;
        }


        /// <summary>
        /// Retorna a última mensagem inserida.
        /// </summary>
        /// <returns></returns>
        public string GetUltimoLog()
        {
            if (this.logs.Count == 0)
                return string.Empty;

            return this.logs.Last();
        }


        public void EscreverLog()
        {
            if(this.logs.Count > 0)
                EscreverArquivo(string.Join(Environment.NewLine, this.logs.ToArray()).Trim());
        }

        public void EscreverArquivo(string conteudo, bool limparLog = true)
        {
            if (string.IsNullOrWhiteSpace(this.nomeArquivo))
                return;

            try
            {
                using (System.IO.StreamWriter log = new System.IO.StreamWriter(
                    this.nomeArquivo
                    , true
                    , Encoding.UTF8)
                )
                {
                    log.WriteLine(conteudo);
                }
                if (limparLog)
                    this.logs.Clear();
            }
            catch (Exception ex)
            { }
        }

        public void EscreverArquivo(List<string> conteudo, bool limparLog = true)
        {
            if (string.IsNullOrWhiteSpace(this.nomeArquivo))
                return;

            if (conteudo.Count <= 0)
                return;

            try
            {
                using (System.IO.StreamWriter log = new System.IO.StreamWriter(
                    this.nomeArquivo
                    , true
                    , Encoding.UTF8)
                )
                {
                    foreach (string linha in conteudo)
                    {
                        log.WriteLine(linha);
                    }
                }
                if(limparLog)
                    this.logs.Clear();
            }
            catch (Exception ex)
            { }
        }

        public void Limpar()
        {
            this.logs.Clear();
        }
    }
}

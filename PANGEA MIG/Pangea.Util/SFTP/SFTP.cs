using Renci.SshNet;
using Renci.SshNet.Common;
using System;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace Pangea.Util.SFTP
{
    public class SFTP
    {
        // Tratar melhor a dependência de Log.
        public AppSettingsReader ConfigurationAppSettings;
        public EventLog EventLog { get; set;}

        public Conexao Conexao { get; private set; }

        public SFTP(Conexao conexaoSFTP)
        {
            this.ConfigurationAppSettings = new AppSettingsReader();
            this.EventLog = new EventLogFactory(ConfigurationAppSettings).GetEventLog("Source");

            this.Conexao = conexaoSFTP;
        }

        //public void SendFile(string host, string username, string password, string filePath)
        //{
        //    FileStream stream = null;

        //    try
        //    {
        //        using (var sftp = new SftpClient(host, username))
        //        {
        //            sftp.Connect();

        //            stream = new FileStream(filePath, FileMode.Open);

        //            sftp.UploadFile(stream, string.Format("{0}{1}", filePath, Path.GetFileName(filePath)));

        //            sftp.Disconnect();
        //        }
        //    }
        //    finally
        //    {
        //        stream.Close();
        //    }
        //}


        public string ObterArquivo(Conexao conexaoSFTP, string caminhoOrigem, string nomeArquivo, string caminhoDestino)
        {

            string NomeDoArquivo = String.Empty;
                        
            var conexaoInfoSFTP = Conexao.ObterConexaoSFTP();
            using (var cliente = new SftpClient(conexaoInfoSFTP))
            {
                // Conexão do cliente SFTP
                try
                {
                    cliente.Connect();
                }
                catch (SshConnectionException ex) 
                {
                    var mensagemDeErro = string.Format("Não foi possível realizar a conexão SFTP.");
                    throw new Exception(mensagemDeErro, ex);
                }
                catch (Exception ex)
                {
                    var mensagemDeErro = string.Format("Ocorreu um erro. Entre em contato com o administrador");
                    EventLog.WriteEntry(mensagemDeErro);
                    throw new Exception(mensagemDeErro, ex);
                }

                try
                {
                    var arquivos = cliente.ListDirectory(caminhoOrigem);
                    var arquivo = arquivos.Where(f => (!f.IsDirectory) && (f.Name.Equals(nomeArquivo))).ToArray();
                    if (arquivo.Count() == 1)
                    {
                        try
                        {
                            using (var fs = new FileStream(caminhoDestino + arquivo[0].Name, FileMode.Create))
                            {
                                cliente.DownloadFile(arquivo[0].FullName, fs);
                                NomeDoArquivo = caminhoDestino + arquivo[0].Name;
                                fs.Close();
                            }

                        }
                        catch (Exception ex)
                        {
                            var mensagemDeErro = string.Format("Não foi possível realizar o Download do Arquivo {0} no caminho {1}", nomeArquivo, caminhoOrigem);
                            EventLog.WriteEntry(mensagemDeErro);
                            throw new Exception(mensagemDeErro,ex);
                        }

                    }
                    else
                    {
                        NomeDoArquivo = String.Empty;
                        var mensagemDeErro = string.Format("O arquivo {0} não foi encontrado no caminho: {1}", nomeArquivo, caminhoOrigem);
                        EventLog.WriteEntry(mensagemDeErro);
                        throw new Exception(mensagemDeErro);
                    }

                }
                finally
                {
                    cliente.Disconnect();
                }

                return NomeDoArquivo;
            }
        }        
    }
}

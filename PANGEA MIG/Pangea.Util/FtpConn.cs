using Chilkat;
using Synapsis.FtpLib;
using System;
using System.Configuration;

namespace Pangea.Util
{
    public static class FtpConn
    {
        static AppSettingsReader configurationAppSettings = new AppSettingsReader();
        static String FtpServer = ConfigurationSettings.AppSettings["IpServer"];
        static String FtpUser = ConfigurationSettings.AppSettings["UserFtp"];
        static String FtpPass = ConfigurationSettings.AppSettings["PwdFtp"];
        //static String FtpServer = configurationAppSettings.GetValue("IpServer", typeof(String)).ToString();
        //static String FtpUser = configurationAppSettings.GetValue("UserFtp", typeof(String)).ToString();
        //static String FtpPass = configurationAppSettings.GetValue("PwdFtp", typeof(String)).ToString();

        public static void transferirSftp(string PastaLocal, string caminhoSftp)
        {
            bool success = false;

            //caminhoSftp = ConfigurationSettings.AppSettings["CaminhoSftpAmp"];

            SFtp sftp = new SFtp();

            if (!sftp.IsConnected)
            {
                sftp = ConectaSftp(sftp);
            }

            success = sftp.DownloadFileByName(caminhoSftp, PastaLocal);
            if (success != true)
            {
                Console.WriteLine(sftp.LastErrorText);
                throw new Exception(sftp.LastErrorText);
            }

            sftp.Disconnect();
        }
        private static SFtp ConectaSftp(SFtp sftp)
        {
            string host = configurationAppSettings.GetValue("IpServerSftp", typeof(String)).ToString();
            int port = Convert.ToInt32(configurationAppSettings.GetValue("portaSftp", typeof(String)));
            string user = configurationAppSettings.GetValue("UsuarioSftp", typeof(String)).ToString();
            string psw = configurationAppSettings.GetValue("SenhaSftp", typeof(String)).ToString();
            //string host = ConfigurationSettings.AppSettings["IpServerSftp"];
            //int port = Convert.ToInt32(ConfigurationSettings.AppSettings["portaSftp"]);
            //string user = ConfigurationSettings.AppSettings["UsuarioSftp"];
            //string psw = ConfigurationSettings.AppSettings["SenhaSftp"];

            //------------------------------------------//
            bool success = sftp.UnlockComponent("Anything for 30-day trial.");
            if (success != true)
            {
                Console.WriteLine(sftp.LastErrorText);
            }

            //  Connect to an SSH/SFTP server
            success = sftp.Connect(host, port);
            if (success != true)
            {
                Console.WriteLine(sftp.LastErrorText);
            }

            //  Authenticate with the SSH server using a username + private key.
            //  (The private key serves as the password.  The username identifies
            //  the SSH user account on the server.)
            success = sftp.AuthenticatePw(user, psw);

            if (success != true)
            {
                Console.WriteLine(sftp.LastErrorText);
            }

            Console.WriteLine("OK, conexão e autenticação com SSH server feita com sucesso!");

            //  After authenticating, the SFTP subsystem must be initialized:
            success = sftp.InitializeSftp();

            if (success != true)
            {
                Console.WriteLine(sftp.LastErrorText);
            }

            return sftp;
        }

        public static void FtpTransferFile(string PastaServidor, string arquivoServer, string caminhoArquivo)
        {
            FTP ftp = new FTP(FtpServer, FtpUser, FtpPass);
            try
            {
                ftp.PassiveMode = false;
                ftp.timeout = 90000;
                if (!ftp.IsConnected)
                {
                    Console.WriteLine("Abrindo Conexão FTP com servidor: " + FtpServer + ", usuário: " + FtpUser + ", senha: " + FtpPass);
                    ftp.Connect();
                }
                if (ftp.IsConnected)
                {
                    Console.WriteLine("Conexão com o FTP realizada com sucesso!");
                    String WorkDir = ftp.GetWorkingDirectory();
                    //ArrayList Dirs = ftp.ListDirectories();

                    Console.WriteLine("Mudando para o diretório correto..." + PastaServidor);
                    try
                    {
                        ftp.ChangeDir(PastaServidor);
                    }
                    catch
                    {
                        ftp.MakeDir(PastaServidor);
                        ftp.ChangeDir(PastaServidor);
                    }

                    Console.WriteLine("Iniciando Download do arquivo " + arquivoServer + " para o arquivo.");

                    ftp.OpenDownload(arquivoServer, caminhoArquivo, true);
                    while (ftp.DoDownload() > 0) { }

                    Console.WriteLine("Arquivo transferido com sucesso!");
                }
            }
            catch (Exception ex)
            {
                throw (ex);
                //Console.WriteLine("Erro ao enviar arquivo via FTP: " + ex.ToString());
            }
            finally
            {
                if (ftp.IsConnected)
                {
                    Console.WriteLine("Finalizando conexão FTP...");
                    ftp.Disconnect();
                }
            }
        }
    }
}

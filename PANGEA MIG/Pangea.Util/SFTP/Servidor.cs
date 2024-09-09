
namespace Pangea.Util.SFTP
{
    public class Servidor
    {
        public string Ip { get; private set; }
        public int Porta { get; private set; }

        public Servidor(string ip, int port)
        {
            this.Ip = ip; 
            this.Porta = port;
        }
    }
}

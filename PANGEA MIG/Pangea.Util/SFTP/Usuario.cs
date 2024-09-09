
namespace Pangea.Util.SFTP
{
    public class Usuario
    {
        public string Login { get; private set;} 
        public string Senha { get; private set;} 

        public Usuario(string login, string senha)
        {
            this.Login = login;
            this.Senha = senha;
        }
        
    }
}

using Renci.SshNet;
using System.Collections.Generic;

namespace Pangea.Util.SFTP
{
    // Classe de Infra, precisa de Log? Logar erro de infra?
    public class Conexao
    {
        // TODO: Tratar melhor a Dependência Log, caso realmente seja necessário Logar erros de Infra.
        //public AppSettingsReader configurationAppSettings;
        //public EventLog EventLog { get; set;}

        public Servidor Servidor { get; private set;}
        public Usuario Usuario { get; private set;}
        public List<AuthenticationMethod> MetodosDeAutenticacao { get; private set;}

        public Conexao(Servidor servidor, Usuario usuario)
        {
            this.Servidor = servidor;
            this.Usuario = usuario;
            this.MetodosDeAutenticacao = new List<AuthenticationMethod>();

        }

        public Conexao(Servidor servidor, Usuario usuario, List<AuthenticationMethod> metodosDeAutenticacao)
        {
            //this.configurationAppSettings = new AppSettingsReader();
            //this.EventLog = new EventLogFactory(this.configurationAppSettings).GetEventLog("Source");

            // Atributos necessários à conexão SFTP.
            this.Servidor = servidor;
            this.Usuario = usuario;
            this.MetodosDeAutenticacao = metodosDeAutenticacao;
        }

        // TODO: Diminuir reescrita de código. métodos sobrecarregados. 
        public ConnectionInfo ObterConexaoSFTP()
        {
            this.MetodosDeAutenticacao.Add(new PasswordAuthenticationMethod(Usuario.Login, Usuario.Senha));
            return new ConnectionInfo(Servidor.Ip, Servidor.Porta, Usuario.Login, MetodosDeAutenticacao.ToArray());
            // TODO:  Tratar exceção de infra
        }

        public ConnectionInfo ObterConexaoSFTP(PrivateKeyFile[] chavesPrivadas) 
        {
            this.MetodosDeAutenticacao.Add(new PrivateKeyAuthenticationMethod(Usuario.Login, chavesPrivadas));
            return new ConnectionInfo(Servidor.Ip, Servidor.Porta, Usuario.Login, MetodosDeAutenticacao.ToArray());
            // TODO:  Tratar exceção de infra
        }

    }
}

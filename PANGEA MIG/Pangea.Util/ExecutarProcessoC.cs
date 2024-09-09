using System;
using System.Configuration;
using System.Linq;
using System.Text;
using Tamir.SharpSsh;

namespace Pangea.Util
{
    public class ExecutarProcessoC
    {
        public String ExecutaProcessoC(string[] parametros, string EnderecoCompletoExecutavel, string empresa)
        {
            try
            {
                String enderecoCompletoExecutavel = EnderecoCompletoExecutavel;
                StringBuilder comando = new StringBuilder();
                String ipServidor, login, senha;
                comando.AppendFormat("{0} ", enderecoCompletoExecutavel);

                for (int i = 0; i <= parametros.Count() - 1; i++)
                {
                    comando.Append(parametros[i] + " ");
                }

                if (empresa.Equals("Coelce"))
                {
                    ipServidor = ConfigurationSettings.AppSettings["ServidorComercial_Coelce"].ToString();
                    login = ConfigurationSettings.AppSettings["LoginComercial_Coelce"].ToString();
                    senha = ConfigurationSettings.AppSettings["SenhaComercial_Coelce"].ToString();

                }
                else
                {
                    ipServidor = ConfigurationSettings.AppSettings["ServidorComercial_Ampla"].ToString();
                    login = ConfigurationSettings.AppSettings["LoginComercial_Ampla"].ToString();
                    senha = ConfigurationSettings.AppSettings["SenhaComercial_Ampla"].ToString();
                }

                Int32 portaSSH = Convert.ToInt32(ConfigurationSettings.AppSettings["PortaSSH"].ToString());

                string output = String.Empty;
                SshShell ssh = new SshShell(ipServidor, login, senha);
                ssh.Connect(portaSSH);
                ssh.ExpectPattern = ">";
                ssh.RemoveTerminalEmulationCharacters = true;
                ssh.Expect(">");

                if (ssh.ShellOpened)
                {
                    ssh.WriteLine(comando.ToString());
                    output = ssh.Expect(">");
                }
                ssh.Close();

                return output;

            }
            catch (Exception ex)
            {
                return "";
            }

        }
    }
}

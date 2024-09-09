using Pangea.Controler;
using Pangea.Dados;
using Pangea.Entidades;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Web.Script.Serialization;

namespace Pangea
{
    public class Cliente
    {
        public string documento { get; set; }
        public string tipo_documento { get; set; }
        public string empresa { get; set; }
        public List<String> telefone { get; set; }
        public string email { get; set; }
        public string email_secundario { get; set; }
    }


    class Program
    {
        private static AppSettingsReader configurationAppSettings;
        private static EventLog eventLog;

        static void Main(string[] args)
        {
            configurationAppSettings = new AppSettingsReader();
            System.Threading.Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("pt-BR");
            ClientesTelefoneController envio = new ClientesTelefoneController();
            String data = DateTime.Now.ToString("yyyyMMdd");
                           
            try
            {
                List<Cliente> lst_clientes = new List<Cliente>();
                StreamReader sr = new StreamReader(ConfigurationSettings.AppSettings["local_arquivo"].ToString() + "delta_reverso_" + data +".txt", Encoding.GetEncoding("ISO-8859-1"));
                
                string linha = sr.ReadToEnd();

                JavaScriptSerializer conversor = new JavaScriptSerializer();

                conversor.MaxJsonLength = int.MaxValue;

                lst_clientes = conversor.Deserialize<List<Cliente>>(linha);

                ApagarArquivo("COELCE_telefones_" + data);
                ApagarArquivo("AMPLA_telefones_" + data);

                ApagarArquivo("COELCE_emails_" + data);
                ApagarArquivo("AMPLA_emails_" + data);

                EscreverArquivo("COELCE_telefones_" + data, "NUMERO_CLIENTE;DDD;TELEFONE;DATA\r\n");
                EscreverArquivo("AMPLA_telefones_" + data, "NUMERO_CLIENTE;DDD;TELEFONE;DATA\r\n");

                EscreverArquivo("COELCE_emails_" + data, "NUMERO_CLIENTE;EMAIL ANTERIOR;EMAIL NOVO\r\n");
                EscreverArquivo("AMPLA_emails_" + data, "NUMERO_CLIENTE;EMAIL ANTERIOR;EMAIL NOVO\r\n");

                int i= 0;

                foreach (var item in lst_clientes.Where(x => (x.email != null && x.email.Trim() != "") || (x.email_secundario != null && x.email_secundario.Trim() != "")))
                {
                    try
                    {
                        if (item.email.Trim() == "")
                        {
                            item.email = item.email_secundario.Trim();
                        }

                        Console.WriteLine("\nProcessando a conta para email: \n" + item.documento);

                        EscreverArquivo(item.empresa + "_emails_" + data, envio.atualizarEmails(item.documento, item.tipo_documento, item.email, "E", item.empresa));

                        i++;

                        if (i >= 200)
                        {
                            System.Threading.Thread.Sleep(3000);
                            i = 0;
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("\nAconteceu um problema ao pesquisar o telefone: \n"
                           + ex.Message);
                    }
                }

                foreach (var item in lst_clientes.Where(x => x.telefone.Count > 0))
                {
                    try
                    {
                        Console.WriteLine("\nProcessando a conta: \n"+ item.documento);

                        EscreverArquivo(item.empresa + "_telefones_" + data, envio.pesquisaTelefoneBase(item.documento, item.tipo_documento, item.telefone, "E", item.empresa));
                        i += item.telefone.Count;

                        if (i >= 350)
                        {
                            System.Threading.Thread.Sleep(5000);
                            i = 0;
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("\nAconteceu um problema ao pesquisar o telefone: \n"
                           + ex.Message);
                    }
                }

            }
            catch (Exception e)
            {
                Console.WriteLine(e.InnerException);
            }

            Console.WriteLine("Finalizado");

            Console.ReadKey();      
        }

        public static void EscreverArquivo(string nomeArquivo, string conteudo)
        {
            using (System.IO.StreamWriter file2 =
            new System.IO.StreamWriter(ConfigurationSettings.AppSettings["local_arquivo"].ToString() + nomeArquivo + ".csv", true, Encoding.GetEncoding("ISO-8859-1")))
            {
                file2.Write(conteudo);
            }
        }

        public static void ApagarArquivo(string nomeArquivo)
        {
            File.Delete(ConfigurationSettings.AppSettings["local_arquivo"].ToString() + nomeArquivo + ".csv");
        }
    }
}


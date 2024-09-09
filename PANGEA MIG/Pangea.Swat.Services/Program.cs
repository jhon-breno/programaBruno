using Pangea.Entidades.Enumeracao;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pangea.Swat.Services
{
    public class Program
    {
        static void Main(string[] args)
        {
            clienteAnteriorStart();
        }


        private static void clienteAnteriorStart()
        {
            Console.WriteLine("[CLIENTES ANTERIORES]");
            Console.WriteLine(string.Format("[{0}] Processo iniciado", DateTime.Now.ToLongTimeString()));

            try
            {
                Pangea.Swat.UI.Negocio.ClienteBO neg = new Pangea.Swat.UI.Negocio.ClienteBO(Empresa.CE, TipoCliente.GB);
                //Console.WriteLine(neg.GerarRelatorioClienteAnterior("c:\\temp\\clienteAnterior_CE_GB.txt"));
                Console.WriteLine(neg.GerarRelatorioClienteAnteriorTodosClientes("c:\\temp\\clienteAnterior_CE_GB.txt"));
            }
            catch (Exception ex)
            {
                Console.WriteLine(string.Format("[{0}] Erro: {1}{2}", DateTime.Now.ToLongTimeString(), ex.Message, ex.StackTrace));
            }

            Console.WriteLine(string.Format("[{0}] Processo finalizado.", DateTime.Now.ToLongTimeString()));
            Console.ReadKey();
        }
    }
}

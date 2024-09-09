using System.Diagnostics;
using System.Threading;

namespace DisparadorPrograma
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            for (int index = 1; index <= 10; ++index)
            {
                Process process = new Process();
                process.StartInfo.FileName = "C:\\Users\\blawe\\Desktop\\Programas\\Anonimo new\\AnonimoScriptSales.exe";
                string str = string.Format("moveout.cs lista_{0}.txt 0", index);
                //string str = string.Format("troca_definitiva_transferencia.c.ts lista_{0}xt 0", index);
                //string str = string.Format("tarifa_data_decreto.cs lista_{0}.txt 0", index);
                //string str = string.Format("troca_definitiva_moveout.cs lista_{0}.txt 1 troca_in.cs", index);
                //string str = string.Format("C:\\Users\\blawe\\Desktop\\LOAD_CONTRACT_LINE_ITEM_split_9\\LOAD_CONTRACT_LINE_ITEM_{0}.csv", index);
                //string str = string.Format("movein_new.cs lista_{0}.txt 0", index);
                //string str = string.Format("postal_sp.cs lista_postal.txt 2", index);
                //string str = string.Format("postal_parametros.cs lista_cagece.txt 2", index);
                //string str = string.Format("moveout.cs lista.txt 0", index);
                process.StartInfo.Arguments = str;
                process.Start();
                Thread.Sleep(5000);
            }

        }
    }
}

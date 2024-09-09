using System;
using System.IO;
using System.Linq;
using System.Text;

namespace ConsoleApplication4
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            Program.GerarEntrada();
            //GerarInvoice();
            //GerarUnico();
            //GerarBilling();
            //GerarDunningMig();
        }

        private static void GerarBilling()
        {
            StreamReader streamReader = new StreamReader("C:\\Users\\Bruno Werneck\\Desktop\\bills.csv");
            String anterior = "";
            string salvar = "";

            while (!streamReader.EndOfStream)
            {
                string linha = streamReader.ReadLine(); ;
                try
                {
                    if (linha.Split(',')[2].ToString() != linha.Split(',')[3].ToString())
                    {
                        salvar = linha.Split(',')[1].ToString() + "," + linha.Split(',')[2].ToString();
                        Program.EscreverArquivo(salvar + "\n", "saida_bills.txt");
                    }
                }
                catch (Exception)
                {
                    continue;
                }
                
            }

            Program.EscreverArquivo(salvar + "\n", "saida_bills.txt");
        }

        private static void GerarUnico()
        {
            StreamReader streamReader = new StreamReader("C:\\Users\\Bruno Werneck\\Desktop\\docs_acerto.txt");
            String anterior = "";
            string salvar = "";

            while (!streamReader.EndOfStream)
            {
               string linha =  streamReader.ReadLine();;

               if (anterior != "" && anterior != linha.Split('|')[3].ToString())
               {
                   Program.EscreverArquivo(salvar.Substring(0, salvar.Length - 1) + "\n", "merge_saida.txt");
                   salvar = "";
               }

               salvar += linha.Split('|')[0].ToString() + ",";
               anterior = linha.Split('|')[3].ToString();
            }

            Program.EscreverArquivo(salvar.Substring(0, salvar.Length - 1) + "\n", "merge_saida.txt");
        }

        private static void GerarInvoice()
        {
            StreamReader streamReader = new StreamReader("C:\\Users\\Bruno Werneck\\Desktop\\ago_ago.csv");
            int num1 = 0;
            StringBuilder stringBuilder1 = new StringBuilder();
            StringBuilder stringBuilder2 = new StringBuilder();
            int num2 = 0;
            while (!streamReader.EndOfStream)
            {
                ++num2;
                string str = streamReader.ReadLine();
                stringBuilder1.Append(str.Split(';')[10] + "|");
                stringBuilder1.Append(str.Split(';')[13] + "|");
                stringBuilder1.Append(str + "\n");
                if (num1.Equals(100000))
                {
                    Program.EscreverArquivo(stringBuilder1.ToString(), "invoice_saida.txt");
                    num1 = 0;
                    stringBuilder1.Clear();
                }
                ++num1;
            }
            Program.EscreverArquivo(stringBuilder1.ToString(), "invoice_saida.txt");
        }

        private static void GerarPagamento()
        {
            StreamReader streamReader = new StreamReader("C:\\Users\\Bruno Werneck\\Desktop\\Pagamentos\\Payments\\output.txt");
            int num1 = 0;
            StringBuilder stringBuilder1 = new StringBuilder();
            StringBuilder stringBuilder2 = new StringBuilder();
            int num2 = 0;
            while (!streamReader.EndOfStream)
            {
                ++num2;
                string str = streamReader.ReadLine();
                stringBuilder1.Append(str.Split(';')[11] + "|");
                stringBuilder1.Append(str.Split(';')[10] + "|");
                stringBuilder1.Append(str + "\n");
                if (num1.Equals(100000))
                {
                    Program.EscreverArquivo(stringBuilder1.ToString(), "pago_saida.txt");
                    num1 = 0;
                    stringBuilder1.Clear();
                }
                ++num1;
            }
            Program.EscreverArquivo(stringBuilder1.ToString(), "pago_saida.txt");
        }

        private static void GerarDunningMig()
        {
            StreamReader streamReader = new StreamReader("C:\\Users\\HomeWork\\Downloads\\mig_rj.csv");
            int num1 = 0;
            StringBuilder _linha = new StringBuilder();
            StringBuilder stringBuilder2 = new StringBuilder();
            int i = 0;
            string separador = "|";
            string[] recebido;
            
            while (!streamReader.EndOfStream)
            {
                recebido = streamReader.ReadLine().ToString().Replace("\"", "").Split(',');

                try
                {

                
                _linha.Append(recebido[0].ToString().Trim().Substring(4, (recebido[0].Length - 4)) + separador);
                _linha.Append(recebido[0].ToString().Trim().Substring(0, 4) + separador);
                
                if(recebido[0].ToString().Trim().Contains("CPF") || recebido[0].ToString().Trim().Contains("INVALIDO"))
                    _linha.Append(recebido[3].ToString().Trim() + separador);
                else
                    _linha.Append("" + separador);

                if (recebido[0].ToString().Trim().Contains("CNPJ"))
                    _linha.Append(recebido[3].ToString().Trim() + separador);
                else
                    _linha.Append("" + separador);               

                _linha.Append(recebido[1].ToString().Trim() + separador);

                if (recebido[2].ToString().Trim().Contains("005") || recebido[2].ToString().Trim().Contains("5"))
                {
                    _linha.Append("CPF" + separador);
                }
                else if (recebido[2].ToString().Trim().Contains("002") || recebido[2].ToString().Trim().Contains("2"))
                {
                    _linha.Append("CNPJ" + separador);
                }
                else
                {
                    _linha.Append("" + separador);
                }
                
                
                _linha.Append(recebido[4].ToString().Trim() + separador);

                //string tip = "";

                /*_linha.Append(ValidaTelefone(recebido[5].ToString().Trim(), out tip) + separador);
                _linha.Append(tip + separador);

                _linha.Append(ValidaTelefone(recebido[6].ToString().Trim(), out tip) + separador);
                _linha.Append(tip + separador);

                _linha.Append(ValidaTelefone(recebido[7].ToString().Trim(), out tip) + separador);
                _linha.Append(tip + separador);

                _linha.Append(ValidaTelefone(recebido[8].ToString().Trim(), out tip) + separador);
                _linha.Append(tip + separador);*/


                _linha.Append(ValidaTelefone(recebido[5].ToString().Trim()) + separador);
                
                _linha.Append(ValidaTelefone(recebido[6].ToString().Trim()) + separador);
                
                _linha.Append(ValidaTelefone(recebido[7].ToString().Trim()) + separador);
                
                _linha.Append(ValidaTelefone(recebido[8].ToString().Trim()) + separador);



                _linha.Append(recebido[9].ToString().Trim() + separador);
                _linha.Append(recebido[13].ToString().Trim() + separador);
                _linha.Append(ValidaTelefone(recebido[11].ToString().Trim()) + separador);
                //_linha.Append(tip + separador);

                _linha.Append(recebido[11].ToString().Trim() + "\n");


                if (i == 20000)
                {
                    Program.EscreverArquivo(_linha.ToString(), "mig_rj_11012023.txt");
                    i= 0;
                    _linha.Clear();
                }
                i++;
                }
                catch 
                {
                    continue;
                }


            }


            Program.EscreverArquivo(_linha.ToString(), "mig_rj_11012023.txt");
        }

        private static void GerarEntrada()
        {
            StreamReader streamReader = new StreamReader("C:\\Users\\Homework\\Desktop\\entrada.txt");
            int num1 = 0;
            StringBuilder stringBuilder1 = new StringBuilder();
            StringBuilder stringBuilder2 = new StringBuilder();
            int num2 = 0;
            while (!streamReader.EndOfStream)
            {
                ++num2;
                if (num2 == 1)
                    stringBuilder1.Append("'" + streamReader.ReadLine() + "'");
                else if (num2 == 7)
                {
                    stringBuilder1.Append(",'" + streamReader.ReadLine() + "'\n");
                    num2 = 0;
                }
                else
                    stringBuilder1.Append(",'" + streamReader.ReadLine() + "'");
                if (num1.Equals(100000))
                {
                    Program.EscreverArquivo(stringBuilder1.ToString(), "lista.txt");
                    num1 = 0;
                    stringBuilder1.Clear();
                }
                ++num1;
            }
            Program.EscreverArquivo(stringBuilder1.ToString(), "lista.txt");
        }

        private static void TirarDuplicidade()
        {
            StreamReader streamReader = new StreamReader("C:\\Users\\Bruno Werneck\\Desktop\\fat_novo.txt");
            int num = 0;
            StringBuilder stringBuilder1 = new StringBuilder();
            StringBuilder stringBuilder2 = new StringBuilder();
            string str = "";
            while (!streamReader.EndOfStream)
            {
                string[] strArray = streamReader.ReadLine().Replace("\"", "").Trim().Split('|');
                if (!(str == Convert.ToInt32(strArray[0]).ToString() + strArray[1]))
                {
                    stringBuilder1.Append(Convert.ToInt32(strArray[0]).ToString() + "|" + strArray[1] + "\n");
                    if (num.Equals(100000))
                    {
                        Program.EscreverArquivo(stringBuilder1.ToString(), "comparativo_new.txt");
                        num = 0;
                        stringBuilder1.Clear();
                    }
                    str = Convert.ToInt32(strArray[0]).ToString() + strArray[1];
                    ++num;
                }
            }
        }

        public static void EscreverArquivo(string conteudo, string nome)
        {
            using (StreamWriter streamWriter = new StreamWriter("C:\\Users\\HomeWork\\Desktop\\" + nome, true, Encoding.GetEncoding("UTF-8")))
                streamWriter.Write(conteudo);
        }

        public static string ValidaTelefone(string telefone, out string tipoTel)
        {
            tipoTel = "";

            try
            {
                telefone = telefone.Trim().Replace("(", "").Replace(")", "").Replace(".", "").Replace("-", "").Replace("/", "").Replace(@"\", "").Replace(" ", "").Replace(",", "");

                //Se entrar no if abaixo, significa que o telefone é um número inválido e contém letras.
                if (telefone.All(char.IsNumber))
                {
                    if (Convert.ToInt32(telefone.Substring(3,1)) >= 6)
                    {
                        tipoTel = "M";
                    }
                    else
                    {
                        tipoTel = "F";
                    }
                }
                else
                {
                    telefone = string.Empty;
                    tipoTel = string.Empty;
                }
            }
            catch (Exception ex)
            {
                telefone = string.Empty;
                tipoTel = string.Empty;
            }

            return telefone;
        }


        public static string ValidaTelefone(string telefone)
        {
            string tipoTel = "";

            try
            {
                //telefone = telefone.Trim().Replace("(", "").Replace(")", "").Replace(".", "").Replace("-", "").Replace("/", "").Replace(@"\", "").Replace(" ", "").Replace(",", "");

                //Se entrar no if abaixo, significa que o telefone é um número inválido e contém letras.
                if (telefone.Equals(""))
                    return "|";

                if (IsDigitsOnly(telefone))
                {
                    if (Convert.ToInt32(telefone.Substring(3, 1)) >= 6)
                    {
                        tipoTel = "M";
                    }
                    else
                    {
                        tipoTel = "F";
                    }
                }
                else
                {
                    telefone = string.Empty;
                    tipoTel = string.Empty;
                }
            }
            catch (Exception ex)
            {
                telefone = string.Empty;
                tipoTel = string.Empty;
            }

            return telefone +"|" + tipoTel;
        }

        public static bool IsDigitsOnly(string str)
        {
            foreach (char c in str)
            {
                if (c < '0' || c > '9')
                    return false;
            }

            return true;
        }
    }

}

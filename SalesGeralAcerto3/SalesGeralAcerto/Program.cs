using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SalesGeralAcerto
{
    class Program
    {
        public static Dictionary<string, string> DicNome = new Dictionary<string, string>();

        static void Main(string[] args)
        {
            StreamReader sr = new StreamReader(ConfigurationSettings.AppSettings["local_arquivo_desp"].ToString(), Encoding.GetEncoding("ISO-8859-1"));

            //StreamReader sr2 = new StreamReader(ConfigurationSettings.AppSettings["nome_clientes"].ToString(), Encoding.GetEncoding("ISO-8859-1"));

            //StreamReader sr3 = new StreamReader(ConfigurationSettings.AppSettings["local_arquivo_pai"].ToString(), Encoding.GetEncoding("ISO-8859-1"));

            List<SalesDado> lista_desp = new List<SalesDado>();           

            /*while (!sr2.EndOfStream)
            {
                string[] nome = sr2.ReadLine().Trim().Split(';');

                DicNome.Add(nome[0], nome[1]);
            }*/

            StringBuilder formato = new StringBuilder();
            int i = 0;

            while (!sr.EndOfStream)
            {
                string[] linha = sr.ReadLine().Trim().Split('|');
                SalesDado obj = new SalesDado();

                if (linha.Length == 1)
                    continue;

                /*if (linha[15].ToString().Trim().Equals("COMANDO DA AERONAUTICA"))
                { 
                
                }*/

                obj.externalid_conta = linha[2];
                obj.id_sales_conta = "";
                obj.externalid_contato = "";
                obj.id_sales_contato = "";
                obj.nome = linha[3].Trim() == "" ? "CONSUMIDOR PROCURE A ENEL" : linha[3].Trim();
                obj.numero_cliente = linha[0];
                obj.id_sales_pod = "";
                obj.externalid_pod = linha[0] + "BRAAMAB2C";
                obj.empresa = "AMPLA";
                obj.id_sales_asset = "";
                obj.externalid_asset = linha[0] + "0113" + (linha[2].Contains("CNPJ") ? "BRAAMAB2B" : "BRAAMAB2C");
                obj.documento = linha[5].Replace("CPF", "").Replace("CNPJ", "");
                obj.apelido = linha[4];
                //obj.tipo_documento = linha[11];
                //obj.documento =linha[2].;

                //lista_desp.Add(obj);
            
            obj.nome = BuscarNome(lista_desp, obj);

            i++;


            formato.Append(obj.externalid_conta); formato.Append("|");
            formato.Append(obj.id_sales_conta); formato.Append("|");
            formato.Append(obj.externalid_contato); formato.Append("|");
            formato.Append(obj.id_sales_contato); formato.Append("|");
            formato.Append(obj.nome); formato.Append("|");
            formato.Append(obj.numero_cliente); formato.Append("|");
            formato.Append(obj.id_sales_pod); formato.Append("|");
            formato.Append(obj.externalid_pod); formato.Append("|");
            formato.Append(obj.empresa); formato.Append("|");
            formato.Append(obj.id_sales_asset); formato.Append("|");
            formato.Append(obj.externalid_asset); formato.Append("|");
            formato.Append("|");
            formato.Append(obj.documento); formato.Append("|");
            formato.Append(obj.apelido); formato.Append("||\n");

                //formato.Append(item.tipo_documento); formato.Append("|"); 
                //formato.Append(item.documento); formato.Append("\n");

                if (i >= 10000)
                {
                    EscreverArquivo(ConfigurationSettings.AppSettings["arquivo_sales_geral"].ToString(), formato.ToString());
                    formato = new StringBuilder();
                    i = 0;
                }
            }

            EscreverArquivo(ConfigurationSettings.AppSettings["arquivo_sales_geral"].ToString(), formato.ToString());
            
            formato = new StringBuilder();
            i = 0;

            
            /*while (!sr3.EndOfStream)
            {
                string[] linha = sr3.ReadLine().Trim().Split('|');
                SalesDado obj = new SalesDado();

                /*if (linha[15].ToString().Trim().Equals("COMANDO DA AERONAUTICA"))
                { 
                
                }

                obj.externalid_conta = "";
                obj.id_sales_conta = "";
                obj.externalid_contato = "";
                obj.id_sales_contato = "";
                obj.nome = linha[4].Trim().EndsWith(" ENEL") ? "CONSUMIDOR PROCURE A ENEL" : linha[4].Trim();
                obj.numero_cliente = linha[3];
                obj.id_sales_pod = "";
                obj.externalid_pod = linha[3] + (linha[0].Contains("002") ? "BRACOEB2B" : "BRACOEB2C");
                obj.empresa = "COELCE";
                obj.id_sales_asset = "";
                obj.externalid_asset = linha[3] + "0113" + (linha[0].Contains("002") ? "BRACOEB2B" : "BRACOEB2C");
                obj.tipo_documento = linha[0];
                obj.documento = linha[1].Trim() + linha[2].Trim();

                //lista_desp.Add(obj);
            

            

            
                string externalId = gerarIdConta(obj);

                /*if (externalId != item.externalid_conta)
                {
                    EscreverArquivo(ConfigurationSettings.AppSettings["arquivo_delta"].ToString(), item.numero_cliente + "\n");

                    if (item.externalid_asset != "")
                    {
                        EscreverArquivo(ConfigurationSettings.AppSettings["arquivo_atualizacao"].ToString(), item.externalid_asset + "|" + item.externalid_contato + "\n");
                    }

                    item.externalid_conta = externalId;
                    item.externalid_contato = externalId;
                    item.externalid_pod = item.numero_cliente + "BRACOEB2C";
                    item.externalid_asset = item.numero_cliente + "0113BRACOEB2C";
                    
                }

                obj.externalid_conta = externalId;
                obj.externalid_contato = externalId;

                obj.nome = BuscarNome(lista_desp, obj);

                i++;


                formato.Append(obj.externalid_conta); formato.Append("|");
                formato.Append(obj.id_sales_conta); formato.Append("|");
                formato.Append(obj.externalid_contato); formato.Append("|");
                formato.Append(obj.id_sales_contato); formato.Append("|");
                formato.Append(obj.nome); formato.Append("|");
                formato.Append(obj.numero_cliente); formato.Append("|");
                formato.Append(obj.id_sales_pod); formato.Append("|");
                formato.Append(obj.externalid_pod); formato.Append("|");
                formato.Append(obj.empresa); formato.Append("|");
                formato.Append(obj.id_sales_asset); formato.Append("|");
                formato.Append(obj.externalid_asset); formato.Append("|\n");
                //formato.Append(item.tipo_documento); formato.Append("|"); 
                //formato.Append(item.documento); formato.Append("\n");

                if (i >= 10000)
                {
                    EscreverArquivo(ConfigurationSettings.AppSettings["arquivo_sales_geral"].ToString(), formato.ToString());
                    formato = new StringBuilder();
                    i = 0;
                }
            }

            EscreverArquivo(ConfigurationSettings.AppSettings["arquivo_sales_geral"].ToString(), formato.ToString());
            formato = new StringBuilder();
            i = 0;*/
        
        }



        private static string BuscarNome(List<SalesDado> lista, SalesDado item)
        {
            String nome = "";

            if (item.nome == "CONSUMIDOR PROCURE A ENEL")
                return item.nome;

            if (!DicNome.TryGetValue(item.externalid_conta.Substring(4, item.externalid_conta.Length - 4).Replace("CPF", "").Replace("CNPJ", ""), out nome))
            {
                DicNome.Add(item.externalid_conta.Substring(4, item.externalid_conta.Length - 4).Replace("CPF", "").Replace("CNPJ", ""), item.nome);
            }
            else
            {
                if (nome != item.nome)
                {
                    EscreverArquivo(ConfigurationSettings.AppSettings["arquivo_nome_diferente"].ToString(), item.externalid_conta +"|"+ item.nome +"|"+ nome + "\n");
                }
            }

            if (nome == null || nome.Equals(""))
                nome = item.nome;
            
            return nome;
        }     

        public static void EscreverArquivo(string Arquivo, string conteudo)
        {
            using (System.IO.StreamWriter file2 =

            new System.IO.StreamWriter(Arquivo, true, Encoding.GetEncoding("ISO-8859-1")))
            {
                file2.Write(conteudo);
            }
        }

        public static string gerarIdConta(SalesDado obj)
        {
           bool FlagCpf = false;
           String id_retorno = "";

           String empresa = "2003";

            //ID e Documento precisam ser padronizados
            if (!string.IsNullOrEmpty(obj.documento.Trim())
                && obj.tipo_documento.Equals("005") ||
                obj.tipo_documento.Equals("002") ||
                obj.tipo_documento.Equals("006"))
            {
                if (obj.tipo_documento.Equals("005") || obj.tipo_documento.Equals("006"))
                {
                    //Sinaliza que é CPF
                    FlagCpf = true;
                    obj.documento = obj.documento.Length > 11 ? obj.documento.Substring((obj.documento.Length) - 11, 11).ToString() : obj.documento;
                }
                else
                {
                    obj.documento = obj.documento.Length > 14 ? obj.documento.Substring((obj.documento.Length) - 14, 14).ToString() : obj.documento;
                }
            }
            //Cliente sem documento, gera ID a partir do numero_cliente
            else
            {


                if (obj.nome.EndsWith(" ENEL"))
                {
                    id_retorno = empresa + "D" + obj.numero_cliente;
                }
                else
                {
                    id_retorno = empresa + obj.numero_cliente + "INVALIDO";
                }

                return id_retorno;
            }

            
            #region Validação CPF e CNPJ
          
            if (FlagCpf)
            {
                    if (!ValidaCPF(obj.documento))
                    {
                        id_retorno = empresa + obj.numero_cliente + "INVALIDO";
                      
                    }
                    else
                    {
                        id_retorno = empresa + obj.documento + "CPF";
                    }
             }
             else
             {
                    if (!ValidaCNPJ(obj.documento))
                    {
                        id_retorno = empresa + obj.numero_cliente + "INVALIDO";
                    }
                    else
                    {
                        id_retorno = empresa + obj.documento + "CNPJ";
                    }              
             }          

            #endregion
           
            return id_retorno;
        }

        public static bool ValidaCPF(string cpf)
        {
            int[] multiplicador1 = new int[9] { 10, 9, 8, 7, 6, 5, 4, 3, 2 };
            int[] multiplicador2 = new int[10] { 11, 10, 9, 8, 7, 6, 5, 4, 3, 2 };
            string tempCpf;
            string digito;
            int soma;
            int resto;

            cpf = cpf.Trim();
            cpf = cpf.Replace(".", "").Replace("-", "").Replace(" ", "");

            if (!IsNumeric(cpf) || cpf.Length != 11 || cpf == "00000000000" || cpf == "11111111111" || cpf == "22222222222" ||
                cpf == "33333333333" || cpf == "44444444444" || cpf == "55555555555" || cpf == "66666666666" ||
                cpf == "77777777777" || cpf == "88888888888" || cpf == "99999999999" || VerificaRepetidas(cpf.Substring(0, 9)))
                return false;

            tempCpf = cpf.Substring(0, 9);
            soma = 0;

            for (int i = 0; i < 9; i++)
                soma += int.Parse(tempCpf[i].ToString()) * multiplicador1[i];

            resto = soma % 11;
            if (resto < 2)
                resto = 0;
            else
                resto = 11 - resto;

            digito = resto.ToString();

            tempCpf = tempCpf + digito;

            soma = 0;
            for (int i = 0; i < 10; i++)
                soma += int.Parse(tempCpf[i].ToString()) * multiplicador2[i];

            resto = soma % 11;
            if (resto < 2)
                resto = 0;
            else
                resto = 11 - resto;

            digito = digito + resto.ToString();

            return cpf.EndsWith(digito);
        }

        public static bool ValidaCNPJ(string cnpj)
        {
            int[] multiplicador1 = new int[12] { 5, 4, 3, 2, 9, 8, 7, 6, 5, 4, 3, 2 };
            int[] multiplicador2 = new int[13] { 6, 5, 4, 3, 2, 9, 8, 7, 6, 5, 4, 3, 2 };
            int soma;
            int resto;
            string digito;
            string tempCnpj;

            cnpj = cnpj.Trim();
            cnpj = cnpj.Replace(".", "").Replace("-", "").Replace("/", "").Replace(" ", "");

            if (cnpj.Length != 14 || !IsNumeric(cnpj) || cnpj == "00000000000191" || cnpj == "00000000000000" || cnpj == "11111111111111" || cnpj == "22222222222222" ||
                cnpj == "33333333333333" || cnpj == "44444444444444" || cnpj == "55555555555555" || cnpj == "66666666666666" ||
                cnpj == "77777777777777" || cnpj == "88888888888888" || cnpj == "99999999999999" || VerificaRepetidas(cnpj.Substring(0, 12)))
                return false;

            tempCnpj = cnpj.Substring(0, 12);

            soma = 0;
            for (int i = 0; i < 12; i++)
                soma += int.Parse(tempCnpj[i].ToString()) * multiplicador1[i];

            resto = (soma % 11);
            if (resto < 2)
                resto = 0;
            else
                resto = 11 - resto;

            digito = resto.ToString();

            tempCnpj = tempCnpj + digito;
            soma = 0;
            for (int i = 0; i < 13; i++)
                soma += int.Parse(tempCnpj[i].ToString()) * multiplicador2[i];

            resto = (soma % 11);
            if (resto < 2)
                resto = 0;
            else
                resto = 11 - resto;

            digito = digito + resto.ToString();

            return cnpj.EndsWith(digito);
        }

        private static bool VerificaRepetidas(string documento)
        {
            return documento.Length == 0 || documento.All(ch => ch == documento[0]);
        }

        public static Boolean IsNumeric(String value)
        {
            foreach (Char c in value.ToCharArray())
            {
                if (Char.IsNumber(c))
                {
                    continue;
                }
                else
                {
                    return false;
                }
            }
            return true;
        }

    }
}

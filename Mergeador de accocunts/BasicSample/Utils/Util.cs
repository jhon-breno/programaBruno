using SalesforceExtractor.Entidades;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace SalesforceExtractor.Utils
{
    public class Util
    {
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

            if (!IsNumeric(cpf) || cpf.Length != 11 || cpf == "00000000191" || cpf == "00000000000" || cpf == "11111111111" || cpf == "22222222222" ||
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

        /// <summary>
        /// Romulo Silva - 25/01/2017
        /// </summary>
        /// <param name="telefone"></param>
        /// <returns></returns>
        //public static string ValidaTelefone(string telefone, string ddd)
        //{
        //    try
        //    {
        //        telefone = telefone.Trim().Replace("(", "").Replace(")", "").Replace(".", "").Replace("-", "").Replace("/", "").Replace(@"\", "").Replace(" ", "").Replace(",", "");

        //        //Se entrar no if abaixo, significa que o telefone é um número inválido e contém letras.
        //        if (!telefone.All(char.IsNumber) || telefone.Length < 8 || string.IsNullOrEmpty(ddd.Trim()))
        //        {
        //            telefone = string.Empty;
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        throw ex;
        //    }
        //    return telefone;
        //}

        /// <summary>
        /// Romulo Silva - 25/01/2017
        /// </summary>
        /// <param name="telefone"></param>
        /// <returns></returns>
        public static string ValidaTelefone(string telefone, string municipio)
        {
            try
            {
                telefone = telefone.Trim().Replace("(", "").Replace(")", "").Replace(".", "").Replace("-", "").Replace("/", "").Replace(@"\", "").Replace(" ", "").Replace(",", "");

                //Se entrar no if abaixo, significa que o telefone é um número inválido e contém letras.
                if (telefone.All(char.IsNumber))
                {
                    telefone = descobreDDD(telefone, municipio);
                }
                else
                {
                    telefone = string.Empty;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return telefone;
        }

        /// <summary>
        /// Padronização de escrita de arquivo
        /// </summary>
        /// <param name="nomeArquivo"></param>
        /// <param name="conteudo"></param>
        public static void EscreverArquivo(string nomeArquivo, string conteudo)
        {
            using (System.IO.StreamWriter file2 =
            new System.IO.StreamWriter(@"" + ExtracaoSalesForce.outputArquivo + nomeArquivo + ".csv", true, Encoding.GetEncoding("UTF-8")))
            {
                file2.Write(conteudo);
            }
        }

        public static void EscreverArquivo(string nomeArquivo, string conteudo, string formato)
        {
            using (System.IO.StreamWriter file2 =
            new System.IO.StreamWriter(@"" + ExtracaoSalesForce.outputArquivo + nomeArquivo + formato, true, Encoding.GetEncoding("ISO-8859-1")))
            {
                file2.Write(conteudo);
            }
        }

        /// <summary>
        /// Padronização do tipo de rua
        /// </summary>
        /// <param name="tipoEnd"></param>
        /// <returns></returns>
        public static string descobreTipoCalle(string tipoEnd, out bool nome)
        {
            string end = string.Empty;
            //int comecoEnd = 0;

            nome = false;

            tipoEnd = tipoEnd.ToUpper();
            //tipoEnd = tipoEnd.Trim().Replace(",", "").Replace(".", "").Replace("-", "").Replace("/", "").Replace(" ", "").ToUpper();

            #region Verifica Tipo
            if (tipoEnd.Equals("E")
                || tipoEnd.Equals("EST")
                || tipoEnd.Equals("ESTRADA")
                || tipoEnd.Equals("ESTR")
                || tipoEnd.Equals("ESTS")
                || tipoEnd.Equals("ESTV")
                || tipoEnd.Equals("ESTRS")
                || tipoEnd.Equals("EESTR")
                || tipoEnd.Equals("ESTTR")
                || tipoEnd.Equals("ET")
                || tipoEnd.Equals("ESTRDA")
                || tipoEnd.Equals("ESTG")
                || tipoEnd.Equals("ESTRA")
                || tipoEnd.Equals("ESTRADO")
                || tipoEnd.Equals("ESTTRA")
                || tipoEnd.Equals("ESTSDA")
                || tipoEnd.Equals("ESTT")
                || tipoEnd.Equals("ESTRAD")
                || tipoEnd.Equals("ESTADA")
                || tipoEnd.Equals("ESTARDA")
                || tipoEnd.Equals("AESTRADA")
                || tipoEnd.Equals("ESTRADAA")
                || tipoEnd.Equals("SEST")
                || tipoEnd.Equals("ESTRADADE")
                || tipoEnd.Equals("ESTA")
                || tipoEnd.Equals("ESTRC")
                || tipoEnd.Equals("ESTE")
                || tipoEnd.Equals("ESTSAS")
                || tipoEnd.Equals("RESTR")
                || tipoEnd.Equals("ESTRADAS")
                || tipoEnd.Equals("ESTRD")
                || tipoEnd.Equals("RESTADA")
                || tipoEnd.Equals("ESTREDA")
                || tipoEnd.Equals("ESTRADRA")
                || tipoEnd.Equals("ESTRDO")
                || tipoEnd.Equals("ESTRA&A")
                || tipoEnd.Equals("ESTRATA")
                || tipoEnd.Equals("ESTRAA")
                || tipoEnd.Equals("REST")
                || tipoEnd.Equals("ESTDE")
                || tipoEnd.Equals("ESTRRADA")
                || tipoEnd.Equals("EEST")
                 || tipoEnd.Equals("DEST")
                || tipoEnd.Equals("ESTRSDAS")
                || tipoEnd.Equals("EST0")
                || tipoEnd.Equals("ESTDA")
                || tipoEnd.Equals("ESTSA")
                || tipoEnd.Equals("ESTD"))
            {
                tipoEnd = "27";
            }
            else if (tipoEnd.Equals("RUA")
                || tipoEnd.Equals("R")
                || tipoEnd.Equals("R.")
                || tipoEnd.Equals("RAU")
                || tipoEnd.Equals("RU"))
            {
                tipoEnd = "58";
            }
            else if (tipoEnd.Equals("AC")
                || tipoEnd.Equals("ACESSO"))
            {
                tipoEnd = "1";
            }
            else if (tipoEnd.Equals("AD") || tipoEnd.Equals("ADRO"))
            {
                tipoEnd = "2";
            }
            else if (tipoEnd.Equals("ALM")
                || tipoEnd.Equals("AL")
                || tipoEnd.Equals("ALAMEDA")
                || tipoEnd.Equals("ALAM"))
            {
                tipoEnd = "3";
            }
            else if (tipoEnd.Equals("AT")
                || tipoEnd.Equals("ALTO"))
            {
                tipoEnd = "4";
            }
            else if (tipoEnd.Equals("ART")
                || tipoEnd.Equals("ARTERIA"))
            {
                tipoEnd = "5";
            }
            else if (tipoEnd.Equals("AVE")
               || tipoEnd.Equals("AV")
               || tipoEnd.Equals("A")
               || tipoEnd.Equals("AVENIDA")
                || tipoEnd.Equals("?V"))
            {
                tipoEnd = "6";
            }
            else if (tipoEnd.Equals("BAI")
            || tipoEnd.Equals("BAIRRO"))
            {
                tipoEnd = "7";
            }
            else if (tipoEnd.Equals("BX")
                || tipoEnd.Equals("BAIXA"))
            {
                tipoEnd = "8";
            }
            else if (tipoEnd.Equals("BAL") || tipoEnd.Equals("BALNEARIO"))
            {
                tipoEnd = "9";
            }
            else if (tipoEnd.Equals("BARRA"))
            {
                tipoEnd = "10";
            }
            else if (tipoEnd.Equals("BECO")
                || tipoEnd.Equals("BCO")
                || tipoEnd.Equals("BC"))
            {
                tipoEnd = "11";
            }
            else if (tipoEnd.Equals("BL") || tipoEnd.Equals("BLOCO"))
            {
                tipoEnd = "12";
            }
            else if (tipoEnd.Equals("BVD") || tipoEnd.Equals("BOULEVARD"))
            {
                tipoEnd = "13";
            }
            else if (tipoEnd.Equals("CALC") || tipoEnd.Equals("CALCADA") || tipoEnd.Equals("CALÇADA"))
            {
                tipoEnd = "14";
            }
            else if (tipoEnd.Equals("CAM") || tipoEnd.Equals("CAMINHO"))
            {
                tipoEnd = "15";
            }
            else if (tipoEnd.Equals("CAMPO")
                || tipoEnd.Equals("CPO"))
            {
                tipoEnd = "16";
            }
            else if (tipoEnd.Equals("CHACARA"))
            {
                tipoEnd = "17";
            }
            else if (tipoEnd.Equals("CIRC") || tipoEnd.Equals("CIRCULAR"))
            {
                tipoEnd = "18";
            }
            else if (tipoEnd.Equals("COL") || tipoEnd.Equals("COLONIA"))
            {
                tipoEnd = "19";
            }
            else if (tipoEnd.Equals("COND")
                || tipoEnd.Equals("CON")
                || tipoEnd.Equals("COM") || tipoEnd.Equals("CONDOMINIO"))
            {
                tipoEnd = "20";
            }
            else if (tipoEnd.Equals("CONJ") || tipoEnd.Equals("CONJUNTO"))
            {
                tipoEnd = "21";
            }
            else if (tipoEnd.Equals("CORONEL"))
            {
                tipoEnd = "22";
            }
            else if (tipoEnd.Equals("COR") || tipoEnd.Equals("CORREDOR"))
            {
                tipoEnd = "23";
            }
            else if (tipoEnd.Equals("EDIFICIO") || tipoEnd.Equals("EDI"))
            {
                tipoEnd = "24";
            }
            else if (tipoEnd.Equals("ESC") || tipoEnd.Equals("ESCADA"))
            {
                tipoEnd = "25";
            }
            else if (tipoEnd.Equals("ESP") || tipoEnd.Equals("ESPLANADA"))
            {
                tipoEnd = "26";
            }
            else if (tipoEnd.Equals("FRA") || tipoEnd.Equals("FEIRA"))
            {
                tipoEnd = "30";
            }
            else if (tipoEnd.Equals("FAV") || tipoEnd.Equals("FAVELA"))
            {
                tipoEnd = "28";
            }
            else if (tipoEnd.Equals("FAZ")
                || tipoEnd.Equals("FAZENDA"))
            {
                tipoEnd = "29";
            }

            else if (tipoEnd.Equals("FERROVIA")
                || tipoEnd.Equals("FER"))
            {
                tipoEnd = "31";
            }
            else if (tipoEnd.Equals("FTE") || tipoEnd.Equals("FORTE"))
            {
                tipoEnd = "32";
            }
            else if (tipoEnd.Equals("GAL") || tipoEnd.Equals("GALERIA"))
            {
                tipoEnd = "33";
            }
            else if (tipoEnd.Equals("GJA") || tipoEnd.Equals("GRANJA"))
            {
                tipoEnd = "34";
            }
            else if (tipoEnd.Equals("ILHA"))
            {
                tipoEnd = "35";
            }
            else if (tipoEnd.Equals("J")
                || tipoEnd.Equals("JARDIM")
                || tipoEnd.Equals("JD"))
            {
                tipoEnd = "36";
            }
            else if (tipoEnd.Equals("LADEIRA")
                || tipoEnd.Equals("LAD")
                || tipoEnd.Equals("LD"))
            {
                tipoEnd = "37";
            }
            else if (tipoEnd.Equals("LG") || tipoEnd.Equals("LAGO"))
            {
                tipoEnd = "38";
            }
            else if (tipoEnd.Equals("LGR") || tipoEnd.Equals("LARGO"))
            {
                tipoEnd = "39";
            }
            else if (tipoEnd.Equals("LOTEAMENTO")
                || tipoEnd.Equals("LOT")
                || tipoEnd.Equals("L")
                || tipoEnd.Equals("LT")
                || tipoEnd.Equals("LOTE")
                || tipoEnd.Equals("LOTEAM")
                || tipoEnd.Equals("LOTEMANTO")
                || tipoEnd.Equals("LOTES")
                || tipoEnd.Equals("ALOTE")
                || tipoEnd.Equals("LOTEAMNETO"))
            {
                tipoEnd = "40";
            }
            else if (tipoEnd.Equals("MORRO")
                || tipoEnd.Equals("MRR")
                || tipoEnd.Equals("MRO"))
            {
                tipoEnd = "41";
            }
            else if (tipoEnd.Equals("NUC") || tipoEnd.Equals("NUCLEO"))
            {
                tipoEnd = "42";
            }
            else if (tipoEnd.Equals("PARQUE")
               || tipoEnd.Equals("PRQ")
               || tipoEnd.Equals("PR?"))
            {
                tipoEnd = "43";
            }
            else if (tipoEnd.Equals("PSA") || tipoEnd.Equals("PASSARELA"))
            {
                tipoEnd = "44";
            }
            else if (tipoEnd.Equals("PAS") || tipoEnd.Equals("PASSEIO"))
            {
                tipoEnd = "45";
            }
            else if (tipoEnd.Equals("PAT") || tipoEnd.Equals("PATIO"))
            {
                tipoEnd = "46";
            }
            else if (tipoEnd.Equals("PONTA"))
            {
                tipoEnd = "47";
            }
            else if (tipoEnd.Equals("PTE") || tipoEnd.Equals("PONTE"))
            {
                tipoEnd = "48";
            }
            else if (tipoEnd.Equals("PRAÇA")
               || tipoEnd.Equals("PRACA")
               || tipoEnd.Equals("PCA")
                || tipoEnd.Equals("P")
                || tipoEnd.Equals("PRA")
                || tipoEnd.Equals("PC")
                || tipoEnd.Equals("P?"))
            {
                tipoEnd = "49";
            }
            else if (tipoEnd.Equals("PRAIA")
                || tipoEnd.Equals("PR")
                || tipoEnd.Equals("PRA?A"))
            {
                tipoEnd = "50";
            }
            else if (tipoEnd.Equals("QD")
                || tipoEnd.Equals("Q")
                || tipoEnd.Equals("QUADRA"))
            {
                tipoEnd = "51";
            }
            else if (tipoEnd.Equals("QTA") || tipoEnd.Equals("QUINTA"))
            {
                tipoEnd = "52";
            }
            else if (tipoEnd.Equals("REC")
                || tipoEnd.Equals("RECANTO"))
            {
                tipoEnd = "53";
            }
            else if (tipoEnd.Equals("RES") || tipoEnd.Equals("RESIDENCIAL"))
            {
                tipoEnd = "54";
            }
            else if (tipoEnd.Equals("RET") || tipoEnd.Equals("RETA"))
            {
                tipoEnd = "55";
            }
            else if (tipoEnd.Equals("ROD")
                || tipoEnd.Equals("RODOVIA"))
            {
                tipoEnd = "56";
            }
            else if (tipoEnd.Equals("ROT") || tipoEnd.Equals("ROTULA"))
            {
                tipoEnd = "57";
            }
            else if (tipoEnd.Equals("SRV") || tipoEnd.Equals("SERVIDAO"))
            {
                tipoEnd = "59";
            }
            else if (tipoEnd.Equals("ST") || tipoEnd.Equals("SETOR"))
            {
                tipoEnd = "60";
            }
            else if (tipoEnd.Equals("SIT")
                || tipoEnd.Equals("SITIO"))
            {
                tipoEnd = "61";
            }
            else if (tipoEnd.Equals("SUB") || tipoEnd.Equals("SUBIDA"))
            {
                tipoEnd = "62";
            }
            else if (tipoEnd.Equals("TER") || tipoEnd.Equals("TERMINAL"))
            {
                tipoEnd = "63";
            }
            else if (tipoEnd.Equals("TRV")
              || tipoEnd.Equals("TV")
              || tipoEnd.Equals("TRAV")
               || tipoEnd.Equals("TR")
               || tipoEnd.Equals("TRAVESSA")
               || tipoEnd.Equals("T")
               || tipoEnd.Equals("TR")
                || tipoEnd.Equals("T?"))
            {
                tipoEnd = "64";
            }
            else if (tipoEnd.Equals("VAL") || tipoEnd.Equals("VALA"))
            {
                tipoEnd = "65";
            }
            else if (tipoEnd.Equals("VLE")
                || tipoEnd.Equals("VALE"))
            {
                tipoEnd = "66";
            }
            else if (tipoEnd.Equals("VER") || tipoEnd.Equals("VEREDA"))
            {
                tipoEnd = "67";
            }
            else if (tipoEnd.Equals("VILA")
             || tipoEnd.Equals("VLA")
             || tipoEnd.Equals("V")
              || tipoEnd.Equals("VL"))
            {
                tipoEnd = "68";
            }
            //else if (tipoEnd.Equals("BOSQUE"))
            //{
            //    tipoEnd = "19";
            //}
            else
            {
                tipoEnd = "";
                nome = true;
            }

            #endregion


            return tipoEnd;
        }

        /// <summary>
        /// Romulo Silva - 23/01/2017
        /// Verifica se existem caracteres repetidos, dado um valor e a posição inicial e final da variável a verificar
        /// </summary>
        /// <param name="documento">variável que contém o valor a verificar</param>
        /// <returns></returns>
        private static bool VerificaRepetidas(string documento)
        {
            return documento.Length == 0 || documento.All(ch => ch == documento[0]);
        }

        static Dictionary<string, int> dicDDDTelefone = new Dictionary<string, int>();
        static Dictionary<string, string> dicTipoIdentidade = new Dictionary<string, string>();
        static Dictionary<string, string> dicClasse = new Dictionary<string, string>();
        static Dictionary<string, string> dicMunicipio = new Dictionary<string, string>();
        static Dictionary<string, string> dicElectrodependiente = new Dictionary<string, string>();
        static Dictionary<string, string> dicEstadoPod = new Dictionary<string, string>();
        static Dictionary<string, string> dicPropriedadMedidor = new Dictionary<string, string>();
        static Dictionary<string, string> dicTarifa = new Dictionary<string, string>();
        static Dictionary<string, string> dicTipoMedida = new Dictionary<string, string>();
        static Dictionary<string, string> dicTipoRede = new Dictionary<string, string>();
        
        public static Dictionary<string, string> dicModifGB = new Dictionary<string, string>();
        public static Dictionary<string, string> dicModifGA = new Dictionary<string, string>();

        
        
        public static void preencheDicionarios()
        {

            if (dicModifGB.Count > 0)
            {
                return;
            }

            #region Dicionário de POD 
            //Dicionários GB
            dicModifGB.Add("3", "3");
            dicModifGB.Add("5", "5");
            dicModifGB.Add("2", "2");
            dicModifGB.Add("22","22");
            dicModifGB.Add("35","35");
            dicModifGB.Add("36", "36");
            dicModifGB.Add("48","48");
            dicModifGB.Add("46", "46");
            dicModifGB.Add("47", "47");
            dicModifGB.Add("53","53");
            dicModifGB.Add("54", "54");
            dicModifGB.Add("56","56");
            dicModifGB.Add("57","57");
            dicModifGB.Add("58","58");
            dicModifGB.Add("59","59");
            dicModifGB.Add("60","60");
            dicModifGB.Add("64","64");
            dicModifGB.Add("87","87");
            dicModifGB.Add("202","202");
            dicModifGB.Add("205","205");
            dicModifGB.Add("308","308");
            dicModifGB.Add("M", "M");

            //Dicionários GA
            dicModifGA.Add("2", "2");
            dicModifGA.Add("3", "3");
            dicModifGA.Add("4", "4");
            dicModifGA.Add("5", "5");
            dicModifGA.Add("6", "6");
            dicModifGA.Add("8", "8");
            dicModifGA.Add("11", "11");
            dicModifGA.Add("12", "12");
            dicModifGA.Add("16", "16");
            dicModifGA.Add("24", "24");
            dicModifGA.Add("25", "25");
            dicModifGA.Add("40", "40");
            dicModifGA.Add("45", "45");
            dicModifGA.Add("118", "118");
            dicModifGA.Add("130", "130");
            dicModifGA.Add("213", "213");
            dicModifGA.Add("214", "214");
            dicModifGA.Add("246", "246");
            dicModifGA.Add("252", "252");
            dicModifGA.Add("314", "314");
            dicModifGA.Add("404", "404");
            dicModifGA.Add("405", "405");
            dicModifGA.Add("501", "501");
            dicModifGA.Add("502", "502");
            dicModifGA.Add("503", "503");
            dicModifGA.Add("506", "506");
            dicModifGA.Add("507", "507");
            dicModifGA.Add("509", "509");
            dicModifGA.Add("515", "515");

            #endregion



            #region Dicionário Telefone
            dicDDDTelefone.Add("ANGRA DOS REIS", 24);
            dicDDDTelefone.Add("APERIBE", 22);
            dicDDDTelefone.Add("ARARUAMA", 22);
            dicDDDTelefone.Add("AREAL", 24);
            dicDDDTelefone.Add("ARMACAO DOS BUZIOS", 22);
            dicDDDTelefone.Add("ARRAIAL DO CABO", 22);
            dicDDDTelefone.Add("BARRA DO PIRAI", 24);
            dicDDDTelefone.Add("BARRA MANSA", 24);
            dicDDDTelefone.Add("BELFORD ROXO", 21);
            dicDDDTelefone.Add("BOM JARDIM", 22);
            dicDDDTelefone.Add("BOM JESUS DO ITABAPOANA", 22);
            dicDDDTelefone.Add("CABO FRIO", 22);
            dicDDDTelefone.Add("CACHOEIRAS DE MACACU", 21);
            dicDDDTelefone.Add("CAMBUCI", 22);
            dicDDDTelefone.Add("CAMPOS DOS GOYTACAZES", 22);
            dicDDDTelefone.Add("CANTAGALO", 22);
            dicDDDTelefone.Add("CARAPEBUS", 22);
            dicDDDTelefone.Add("CARDOSO MOREIRA", 22);
            dicDDDTelefone.Add("CARMO", 22);
            dicDDDTelefone.Add("CASIMIRO DE ABREU", 22);
            dicDDDTelefone.Add("COMENDADOR LEVY GASPARIAN", 24);
            dicDDDTelefone.Add("CONCEICAO DE MACABU", 22);
            dicDDDTelefone.Add("CORDEIRO", 22);
            dicDDDTelefone.Add("DUAS BARRAS", 22);
            dicDDDTelefone.Add("DUQUE DE CAXIAS", 21);
            dicDDDTelefone.Add("ENGENHEIRO PAULO DE FRONTIN", 24);
            dicDDDTelefone.Add("GUAPIMIRIM", 21);
            dicDDDTelefone.Add("IGUABA GRANDE", 22);
            dicDDDTelefone.Add("ITABORAI", 21);
            dicDDDTelefone.Add("ITAGUAI", 21);
            dicDDDTelefone.Add("ITALVA", 22);
            dicDDDTelefone.Add("ITAOCARA", 22);
            dicDDDTelefone.Add("ITAPERUNA", 22);
            dicDDDTelefone.Add("ITATIAIA", 24);
            dicDDDTelefone.Add("JAPERI", 21);
            dicDDDTelefone.Add("LAJE DO MURIAE", 22);
            dicDDDTelefone.Add("MACAE", 22);
            dicDDDTelefone.Add("MACUCO", 22);
            dicDDDTelefone.Add("MAGE", 21);
            dicDDDTelefone.Add("MANGARATIBA", 21);
            dicDDDTelefone.Add("MARICA", 21);
            dicDDDTelefone.Add("MENDES", 24);
            dicDDDTelefone.Add("MESQUITA", 21);
            dicDDDTelefone.Add("MIGUEL PEREIRA", 24);
            dicDDDTelefone.Add("MIRACEMA", 22);
            dicDDDTelefone.Add("NATIVIDADE", 22);
            dicDDDTelefone.Add("NILOPOLIS", 21);
            dicDDDTelefone.Add("NITEROI", 21);
            dicDDDTelefone.Add("NOVA FRIBURGO", 22);
            dicDDDTelefone.Add("NOVA IGUACU", 21);
            dicDDDTelefone.Add("PARACAMBI", 21);
            dicDDDTelefone.Add("PARAIBA DO SUL", 24);
            dicDDDTelefone.Add("PARATI", 24);
            dicDDDTelefone.Add("PATY DO ALFERES", 24);
            dicDDDTelefone.Add("PETROPOLIS", 24);
            dicDDDTelefone.Add("PINHEIRAL", 24);
            dicDDDTelefone.Add("PIRAI", 24);
            dicDDDTelefone.Add("PORCIUNCULA", 22);
            dicDDDTelefone.Add("PORTO REAL", 24);
            dicDDDTelefone.Add("QUATIS", 24);
            dicDDDTelefone.Add("QUEIMADOS", 21);
            dicDDDTelefone.Add("QUISSAMA", 22);
            dicDDDTelefone.Add("RESENDE", 24);
            dicDDDTelefone.Add("RIO BONITO", 21);
            dicDDDTelefone.Add("RIO CLARO", 24);
            dicDDDTelefone.Add("RIO DAS FLORES", 24);
            dicDDDTelefone.Add("RIO DAS OSTRAS", 22);
            dicDDDTelefone.Add("RIO DE JANEIRO", 21);
            dicDDDTelefone.Add("SANTA MARIA MADALENA", 22);
            dicDDDTelefone.Add("SANTO ANTONIO DE PADUA", 22);
            dicDDDTelefone.Add("SAO FIDELIS", 22);
            dicDDDTelefone.Add("SAO FRANCISCO DE ITABAPOANA", 22);
            dicDDDTelefone.Add("SAO GONCALO", 21);
            dicDDDTelefone.Add("SAO JOAO DA BARRA", 22);
            dicDDDTelefone.Add("SAO JOAO DE MERITI", 21);
            dicDDDTelefone.Add("SAO JOSE DE UBA", 22);
            dicDDDTelefone.Add("SAO JOSE DO VALE DO RIO PRETO", 24);
            dicDDDTelefone.Add("SAO PEDRO DA ALDEIA", 22);
            dicDDDTelefone.Add("SAO SEBASTIAO DO ALTO", 22);
            dicDDDTelefone.Add("SAPUCAIA", 24);
            dicDDDTelefone.Add("SAQUAREMA", 22);
            dicDDDTelefone.Add("SEROPEDICA", 21);
            dicDDDTelefone.Add("SILVA JARDIM", 22);
            dicDDDTelefone.Add("SUMIDOURO", 22);
            dicDDDTelefone.Add("TANGUA", 21);
            dicDDDTelefone.Add("TERESOPOLIS", 21);
            dicDDDTelefone.Add("TRAJANO DE MORAIS", 22);
            dicDDDTelefone.Add("TRES RIOS", 24);
            dicDDDTelefone.Add("VALENCA", 24);
            dicDDDTelefone.Add("VARRE-SAI", 22);
            dicDDDTelefone.Add("VASSOURAS", 24);
            dicDDDTelefone.Add("VOLTA REDONDA", 24);
            //COELCE
            dicDDDTelefone.Add("ABAIARA", 88);
            dicDDDTelefone.Add("ACARAPE", 85);
            dicDDDTelefone.Add("ACARAU", 88);
            dicDDDTelefone.Add("ACOPIARA", 88);
            dicDDDTelefone.Add("AIUABA", 88);
            dicDDDTelefone.Add("ALCANTARAS", 88);
            dicDDDTelefone.Add("ALTANEIRA", 88);
            dicDDDTelefone.Add("ALTO SANTO", 88);
            dicDDDTelefone.Add("AMONTADA", 88);
            dicDDDTelefone.Add("ANTONINA DO NORTE", 88);
            dicDDDTelefone.Add("APUIARES", 85);
            dicDDDTelefone.Add("AQUIRAZ", 85);
            dicDDDTelefone.Add("ARACATI", 88);
            dicDDDTelefone.Add("ARACOIABA", 85);
            dicDDDTelefone.Add("ARARENDA", 88);
            dicDDDTelefone.Add("ARARIPE", 88);
            dicDDDTelefone.Add("ARATUBA", 85);
            dicDDDTelefone.Add("ARNEIROZ", 88);
            dicDDDTelefone.Add("ASSARE", 88);
            dicDDDTelefone.Add("AURORA", 88);
            dicDDDTelefone.Add("BAIXIO", 88);
            dicDDDTelefone.Add("BANABUIU", 88);
            dicDDDTelefone.Add("BARBALHA", 88);
            dicDDDTelefone.Add("BARREIRA", 85);
            dicDDDTelefone.Add("BARRO", 88);
            dicDDDTelefone.Add("BARROQUINHA", 88);
            dicDDDTelefone.Add("BATURITE", 85);
            dicDDDTelefone.Add("BEBERIBE", 85);
            dicDDDTelefone.Add("BELA CRUZ", 88);
            dicDDDTelefone.Add("BOA VIAGEM", 88);
            dicDDDTelefone.Add("BREJO SANTO", 88);
            dicDDDTelefone.Add("CAMOCIM", 88);
            dicDDDTelefone.Add("CAMPOS SALES", 88);
            dicDDDTelefone.Add("CANINDE", 85);
            dicDDDTelefone.Add("CAPISTRANO", 85);
            dicDDDTelefone.Add("CARIDADE", 85);
            dicDDDTelefone.Add("CARIRE", 88);
            dicDDDTelefone.Add("CARIRIACU", 88);
            dicDDDTelefone.Add("CARIUS", 88);
            dicDDDTelefone.Add("CARNAUBAL", 88);
            dicDDDTelefone.Add("CASCAVEL", 85);
            dicDDDTelefone.Add("CATARINA", 88);
            dicDDDTelefone.Add("CATUNDA", 88);
            dicDDDTelefone.Add("CAUCAIA", 85);
            dicDDDTelefone.Add("CEDRO", 88);
            dicDDDTelefone.Add("CHAVAL", 88);
            dicDDDTelefone.Add("CHORO", 88);
            dicDDDTelefone.Add("CHOROZINHO", 85);
            dicDDDTelefone.Add("COREAU", 88);
            dicDDDTelefone.Add("CRATEUS", 88);
            dicDDDTelefone.Add("CRATO", 88);
            dicDDDTelefone.Add("CROATA", 88);
            dicDDDTelefone.Add("CRUZ", 88);
            dicDDDTelefone.Add("DEPUTADO IRAPUAN PINHEIRO", 88);
            dicDDDTelefone.Add("ERERE", 88);
            dicDDDTelefone.Add("EUSEBIO", 85);
            dicDDDTelefone.Add("FARIAS BRITO", 88);
            dicDDDTelefone.Add("FORQUILHA", 88);
            dicDDDTelefone.Add("FORTALEZA", 85);
            dicDDDTelefone.Add("FORTIM", 88);
            dicDDDTelefone.Add("FRECHEIRINHA", 88);
            dicDDDTelefone.Add("GENERAL SAMPAIO", 85);
            dicDDDTelefone.Add("GRACA", 88);
            dicDDDTelefone.Add("GRANJA", 88);
            dicDDDTelefone.Add("GRANJEIRO", 88);
            dicDDDTelefone.Add("GROAIRAS", 88);
            dicDDDTelefone.Add("GUAIUBA", 85);
            dicDDDTelefone.Add("GUARACIABA DO NORTE", 88);
            dicDDDTelefone.Add("GUARAMIRANGA", 85);
            dicDDDTelefone.Add("HIDROLANDIA", 88);
            dicDDDTelefone.Add("HORIZONTE", 85);
            dicDDDTelefone.Add("IBARETAMA", 88);
            dicDDDTelefone.Add("IBIAPINA", 88);
            dicDDDTelefone.Add("IBICUITINGA", 88);
            dicDDDTelefone.Add("ICAPUI", 88);
            dicDDDTelefone.Add("ICO", 88);
            dicDDDTelefone.Add("IGUATU", 88);
            dicDDDTelefone.Add("INDEPENDENCIA", 88);
            dicDDDTelefone.Add("IPAPORANGA", 88);
            dicDDDTelefone.Add("IPAUMIRIM", 88);
            dicDDDTelefone.Add("IPU", 88);
            dicDDDTelefone.Add("IPUEIRAS", 88);
            dicDDDTelefone.Add("IRACEMA", 88);
            dicDDDTelefone.Add("IRAUCUBA", 88);
            dicDDDTelefone.Add("ITAICABA", 88);
            dicDDDTelefone.Add("ITAITINGA", 85);
            dicDDDTelefone.Add("ITAPAGE", 85);
            dicDDDTelefone.Add("ITAPIPOCA", 88);
            dicDDDTelefone.Add("ITAPIUNA", 88);
            dicDDDTelefone.Add("ITAREMA", 88);
            dicDDDTelefone.Add("ITATIRA", 88);
            dicDDDTelefone.Add("JAGUARETAMA", 88);
            dicDDDTelefone.Add("JAGUARIBARA", 88);
            dicDDDTelefone.Add("JAGUARIBE", 88);
            dicDDDTelefone.Add("JAGUARUANA", 88);
            dicDDDTelefone.Add("JARDIM", 88);
            dicDDDTelefone.Add("JATI", 88);
            dicDDDTelefone.Add("JIJOCA DE JERICOACOARA", 88);
            dicDDDTelefone.Add("JUAZEIRO DO NORTE", 88);
            dicDDDTelefone.Add("JUCAS", 88);
            dicDDDTelefone.Add("LAVRAS DA MANGABEIRA", 88);
            dicDDDTelefone.Add("LIMOEIRO DO NORTE", 88);
            dicDDDTelefone.Add("MADALENA", 88);
            dicDDDTelefone.Add("MARACANAU", 85);
            dicDDDTelefone.Add("MARANGUAPE", 85);
            dicDDDTelefone.Add("MARCO", 88);
            dicDDDTelefone.Add("MARTINOPOLE", 88);
            dicDDDTelefone.Add("MASSAPE", 88);
            dicDDDTelefone.Add("MAURITI", 88);
            dicDDDTelefone.Add("MERUOCA", 88);
            dicDDDTelefone.Add("MILAGRES", 88);
            dicDDDTelefone.Add("MILHA", 88);
            dicDDDTelefone.Add("MIRAIMA", 88);
            dicDDDTelefone.Add("MISSAO VELHA", 88);
            dicDDDTelefone.Add("MOMBACA", 88);
            dicDDDTelefone.Add("MONSENHOR TABOSA", 88);
            dicDDDTelefone.Add("MORADA NOVA", 88);
            dicDDDTelefone.Add("MORAUJO", 88);
            dicDDDTelefone.Add("MORRINHOS", 88);
            dicDDDTelefone.Add("MUCAMBO", 88);
            dicDDDTelefone.Add("MULUNGU", 85);
            dicDDDTelefone.Add("NOVA OLINDA", 88);
            dicDDDTelefone.Add("NOVA RUSSAS", 88);
            dicDDDTelefone.Add("NOVO ORIENTE", 88);
            dicDDDTelefone.Add("OCARA", 85);
            dicDDDTelefone.Add("OROS", 88);
            dicDDDTelefone.Add("PACAJUS", 85);
            dicDDDTelefone.Add("PACATUBA", 85);
            dicDDDTelefone.Add("PACOTI", 85);
            dicDDDTelefone.Add("PACUJA", 88);
            dicDDDTelefone.Add("PALHANO", 88);
            dicDDDTelefone.Add("PALMACIA", 85);
            dicDDDTelefone.Add("PARACURU", 85);
            dicDDDTelefone.Add("PARAIPABA", 85);
            dicDDDTelefone.Add("PARAMBU", 88);
            dicDDDTelefone.Add("PARAMOTI", 85);
            dicDDDTelefone.Add("PEDRA BRANCA", 88);
            dicDDDTelefone.Add("PENAFORTE", 88);
            dicDDDTelefone.Add("PENTECOSTE", 85);
            dicDDDTelefone.Add("PEREIRO", 88);
            dicDDDTelefone.Add("PINDORETAMA", 85);
            dicDDDTelefone.Add("PIQUET CARNEIRO", 88);
            dicDDDTelefone.Add("PIRES FERREIRA", 88);
            dicDDDTelefone.Add("PORANGA", 88);
            dicDDDTelefone.Add("PORTEIRAS", 88);
            dicDDDTelefone.Add("POTENGI", 88);
            dicDDDTelefone.Add("POTIRETAMA", 88);
            dicDDDTelefone.Add("QUITERIANOPOLIS", 88);
            dicDDDTelefone.Add("QUIXADA", 88);
            dicDDDTelefone.Add("QUIXELO", 88);
            dicDDDTelefone.Add("QUIXERAMOBIM", 88);
            dicDDDTelefone.Add("QUIXERE", 88);
            dicDDDTelefone.Add("REDENCAO", 85);
            dicDDDTelefone.Add("RERIUTABA", 88);
            dicDDDTelefone.Add("RUSSAS", 88);
            dicDDDTelefone.Add("SABOEIRO", 88);
            dicDDDTelefone.Add("SALITRE", 88);
            dicDDDTelefone.Add("SANTA QUITERIA", 88);
            dicDDDTelefone.Add("SANTANA DO ACARAU", 88);
            dicDDDTelefone.Add("SANTANA DO CARIRI", 88);
            dicDDDTelefone.Add("SAO BENEDITO", 88);
            dicDDDTelefone.Add("SAO GONCALO DO AMARANTE", 85);
            dicDDDTelefone.Add("SAO JOAO DO JAGUARIBE", 88);
            dicDDDTelefone.Add("SAO LUIS DO CURU", 85);
            dicDDDTelefone.Add("SENADOR POMPEU", 88);
            dicDDDTelefone.Add("SENADOR SA", 88);
            dicDDDTelefone.Add("SOBRAL", 88);
            dicDDDTelefone.Add("SOLONOPOLE", 88);
            dicDDDTelefone.Add("TABULEIRO DO NORTE", 88);
            dicDDDTelefone.Add("TAMBORIL", 88);
            dicDDDTelefone.Add("TARRAFAS", 88);
            dicDDDTelefone.Add("TAUA", 88);
            dicDDDTelefone.Add("TEJUCUOCA", 85);
            dicDDDTelefone.Add("TIANGUA", 88);
            dicDDDTelefone.Add("TRAIRI", 85);
            dicDDDTelefone.Add("TURURU", 85);
            dicDDDTelefone.Add("UBAJARA", 88);
            dicDDDTelefone.Add("UMARI", 88);
            dicDDDTelefone.Add("UMIRIM", 85);
            dicDDDTelefone.Add("URUBURETAMA", 85);
            dicDDDTelefone.Add("URUOCA", 88);
            dicDDDTelefone.Add("VARJOTA", 88);
            dicDDDTelefone.Add("VARZEA ALEGRE", 88);
            dicDDDTelefone.Add("VICOSA DO CEARA", 88);
            #endregion

            #region Dicionário Tipo Identidade
            dicTipoIdentidade.Add("001", "001");
            dicTipoIdentidade.Add("002", "002");
            dicTipoIdentidade.Add("003", "003");
            dicTipoIdentidade.Add("005", "005");
            dicTipoIdentidade.Add("006", "006");
            dicTipoIdentidade.Add("010", "010");
            dicTipoIdentidade.Add("014", "014");
            dicTipoIdentidade.Add("015", "015");
            dicTipoIdentidade.Add("016", "016");
            dicTipoIdentidade.Add("017", "017");
            dicTipoIdentidade.Add("018", "018");
            dicTipoIdentidade.Add("777", "777");
            #endregion

            #region Classe
            dicClasse.Add("01", "01");
            dicClasse.Add("02", "02");
            dicClasse.Add("03", "03");
            dicClasse.Add("04", "04");
            dicClasse.Add("05", "05");
            dicClasse.Add("06", "06");
            dicClasse.Add("07", "07");
            dicClasse.Add("08", "08");
            dicClasse.Add("09", "09");
            #endregion

            #region Municipio
            dicMunicipio.Add("20051117", "20051117");
            dicMunicipio.Add("20051119", "20051119");
            dicMunicipio.Add("20051120", "20051120");
            dicMunicipio.Add("20051231", "20051231");
            dicMunicipio.Add("20051238", "20051238");
            dicMunicipio.Add("20051239", "20051239");
            dicMunicipio.Add("20051324", "20051324");
            dicMunicipio.Add("20051329", "20051329");
            dicMunicipio.Add("20051341", "20051341");
            dicMunicipio.Add("20051342", "20051342");
            dicMunicipio.Add("20051401", "20051401");
            dicMunicipio.Add("20051407", "20051407");
            dicMunicipio.Add("20052102", "20052102");
            dicMunicipio.Add("20052106", "20052106");
            dicMunicipio.Add("20052108", "20052108");
            dicMunicipio.Add("20052109", "20052109");
            dicMunicipio.Add("20052111", "20052111");
            dicMunicipio.Add("20052221", "20052221");
            dicMunicipio.Add("20052226", "20052226");
            dicMunicipio.Add("20052227", "20052227");
            dicMunicipio.Add("20052233", "20052233");
            dicMunicipio.Add("20053103", "20053103");
            dicMunicipio.Add("20053104", "20053104");
            dicMunicipio.Add("20053105", "20053105");
            dicMunicipio.Add("20053110", "20053110");
            dicMunicipio.Add("20053166", "20053166");
            dicMunicipio.Add("20053228", "20053228");
            dicMunicipio.Add("20053234", "20053234");
            dicMunicipio.Add("20053236", "20053236");
            dicMunicipio.Add("20053315", "20053315");
            dicMunicipio.Add("20053316", "20053316");
            dicMunicipio.Add("20053318", "20053318");
            dicMunicipio.Add("20053367", "20053367");
            dicMunicipio.Add("20054130", "20054130");
            dicMunicipio.Add("20054157", "20054157");
            dicMunicipio.Add("20054158", "20054158");
            dicMunicipio.Add("20054169", "20054169");
            dicMunicipio.Add("20054225", "20054225");
            dicMunicipio.Add("20054240", "20054240");
            dicMunicipio.Add("20054243", "20054243");
            dicMunicipio.Add("20054248", "20054248");
            dicMunicipio.Add("20054249", "20054249");
            dicMunicipio.Add("20054250", "20054250");
            dicMunicipio.Add("20054251", "20054251");
            dicMunicipio.Add("20054268", "20054268");
            dicMunicipio.Add("20054322", "20054322");
            dicMunicipio.Add("20054359", "20054359");
            dicMunicipio.Add("20054360", "20054360");
            dicMunicipio.Add("20054361", "20054361");
            dicMunicipio.Add("20054362", "20054362");
            dicMunicipio.Add("20054363", "20054363");
            dicMunicipio.Add("20054364", "20054364");
            dicMunicipio.Add("20054365", "20054365");
            dicMunicipio.Add("20054432", "20054432");
            dicMunicipio.Add("20054452", "20054452");
            dicMunicipio.Add("20054453", "20054453");
            dicMunicipio.Add("20054454", "20054454");
            dicMunicipio.Add("20054455", "20054455");
            dicMunicipio.Add("20054456", "20054456");
            dicMunicipio.Add("20054523", "20054523");
            dicMunicipio.Add("20054535", "20054535");
            dicMunicipio.Add("20054537", "20054537");
            dicMunicipio.Add("20054544", "20054544");
            dicMunicipio.Add("20054545", "20054545");
            dicMunicipio.Add("20054546", "20054546");
            dicMunicipio.Add("20054547", "20054547");
            dicMunicipio.Add("20031501", "20031501");
            dicMunicipio.Add("20032711", "20032711");
            dicMunicipio.Add("20033502", "20033502");
            dicMunicipio.Add("20031201", "20031201");
            dicMunicipio.Add("20031601", "20031601");
            dicMunicipio.Add("20033203", "20033203");
            dicMunicipio.Add("20031111", "20031111");
            dicMunicipio.Add("20032501", "20032501");
            dicMunicipio.Add("20033503", "20033503");
            dicMunicipio.Add("20031602", "20031602");
            dicMunicipio.Add("20033401", "20033401");
            dicMunicipio.Add("20032000", "20032000");
            dicMunicipio.Add("20032400", "20032400");
            dicMunicipio.Add("20032701", "20032701");
            dicMunicipio.Add("20033914", "20033914");
            dicMunicipio.Add("20031603", "20031603");
            dicMunicipio.Add("20032702", "20032702");
            dicMunicipio.Add("20033702", "20033702");
            dicMunicipio.Add("20031114", "20031114");
            dicMunicipio.Add("20031502", "20031502");
            dicMunicipio.Add("20031801", "20031801");
            dicMunicipio.Add("20032804", "20032804");
            dicMunicipio.Add("20031301", "20031301");
            dicMunicipio.Add("20032712", "20032712");
            dicMunicipio.Add("20031503", "20031503");
            dicMunicipio.Add("20033304", "20033304");
            dicMunicipio.Add("20032700", "20032700");
            dicMunicipio.Add("20032001", "20032001");
            dicMunicipio.Add("20033504", "20033504");
            dicMunicipio.Add("20033604", "20033604");
            dicMunicipio.Add("20031504", "20031504");
            dicMunicipio.Add("20033300", "20033300");
            dicMunicipio.Add("20031600", "20031600");
            dicMunicipio.Add("20033600", "20033600");
            dicMunicipio.Add("20032703", "20032703");
            dicMunicipio.Add("20033601", "20033601");
            dicMunicipio.Add("20033206", "20033206");
            dicMunicipio.Add("20031302", "20031302");
            dicMunicipio.Add("20031204", "20031204");
            dicMunicipio.Add("20033101", "20033101");
            dicMunicipio.Add("20032002", "20032002");
            dicMunicipio.Add("20031219", "20031219");
            dicMunicipio.Add("20033915", "20033915");
            dicMunicipio.Add("20032200", "20032200");
            dicMunicipio.Add("20031205", "20031205");
            dicMunicipio.Add("20033301", "20033301");
            dicMunicipio.Add("20032806", "20032806");
            dicMunicipio.Add("20032006", "20032006");
            dicMunicipio.Add("20033208", "20033208");
            dicMunicipio.Add("20033800", "20033800");
            dicMunicipio.Add("20031100", "20031100");
            dicMunicipio.Add("20033121", "20033121");
            dicMunicipio.Add("20033505", "20033505");
            dicMunicipio.Add("20030934", "20030934");
            dicMunicipio.Add("20030994", "20030994");
            dicMunicipio.Add("20030993", "20030993");
            dicMunicipio.Add("20030991", "20030991");
            dicMunicipio.Add("20031407", "20031407");
            dicMunicipio.Add("20030992", "20030992");
            dicMunicipio.Add("20031706", "20031706");
            dicMunicipio.Add("20032004", "20032004");
            dicMunicipio.Add("20031116", "20031116");
            dicMunicipio.Add("20033220", "20033220");
            dicMunicipio.Add("20030000", "20030000");
            dicMunicipio.Add("20032401", "20032401");
            dicMunicipio.Add("20033102", "20033102");
            dicMunicipio.Add("20033402", "20033402");
            dicMunicipio.Add("20033120", "20033120");
            dicMunicipio.Add("20033302", "20033302");
            dicMunicipio.Add("20031208", "20031208");
            dicMunicipio.Add("20031305", "20031305");
            dicMunicipio.Add("20033210", "20033210");
            dicMunicipio.Add("20032603", "20032603");
            dicMunicipio.Add("20033103", "20033103");
            dicMunicipio.Add("20032704", "20032704");
            dicMunicipio.Add("20033902", "20033902");
            dicMunicipio.Add("20032007", "20032007");
            dicMunicipio.Add("20032805", "20032805");
            dicMunicipio.Add("20033105", "20033105");
            dicMunicipio.Add("20032513", "20032513");
            dicMunicipio.Add("20032402", "20032402");
            dicMunicipio.Add("20031800", "20031800");
            dicMunicipio.Add("20031200", "20031200");
            dicMunicipio.Add("20033802", "20033802");
            dicMunicipio.Add("20033804", "20033804");
            dicMunicipio.Add("20033912", "20033912");
            dicMunicipio.Add("20031802", "20031802");
            dicMunicipio.Add("20033903", "20033903");
            dicMunicipio.Add("20033904", "20033904");
            dicMunicipio.Add("20031701", "20031701");
            dicMunicipio.Add("20033403", "20033403");
            dicMunicipio.Add("20032403", "20032403");
            dicMunicipio.Add("20032008", "20032008");
            dicMunicipio.Add("20033400", "20033400");
            dicMunicipio.Add("20033500", "20033500");
            dicMunicipio.Add("20032705", "20032705");
            dicMunicipio.Add("20033506", "20033506");
            dicMunicipio.Add("20033602", "20033602");
            dicMunicipio.Add("20033512", "20033512");
            dicMunicipio.Add("20031702", "20031702");
            dicMunicipio.Add("20031703", "20031703");
            dicMunicipio.Add("20031700", "20031700");
            dicMunicipio.Add("20032404", "20032404");
            dicMunicipio.Add("20031306", "20031306");
            dicMunicipio.Add("20031505", "20031505");
            dicMunicipio.Add("20031300", "20031300");
            dicMunicipio.Add("20031212", "20031212");
            dicMunicipio.Add("20031807", "20031807");
            dicMunicipio.Add("20032506", "20032506");
            dicMunicipio.Add("20033605", "20033605");
            dicMunicipio.Add("20032600", "20032600");
            dicMunicipio.Add("20032602", "20032602");
            dicMunicipio.Add("20033507", "20033507");
            dicMunicipio.Add("20033303", "20033303");
            dicMunicipio.Add("20033213", "20033213");
            dicMunicipio.Add("20031506", "20031506");
            dicMunicipio.Add("20033214", "20033214");
            dicMunicipio.Add("20031500", "20031500");
            dicMunicipio.Add("20031406", "20031406");
            dicMunicipio.Add("20033510", "20033510");
            dicMunicipio.Add("20031507", "20031507");
            dicMunicipio.Add("20031402", "20031402");
            dicMunicipio.Add("20033905", "20033905");
            dicMunicipio.Add("20032507", "20032507");
            dicMunicipio.Add("20033215", "20033215");
            dicMunicipio.Add("20033508", "20033508");
            dicMunicipio.Add("20033107", "20033107");
            dicMunicipio.Add("20032706", "20032706");
            dicMunicipio.Add("20031117", "20031117");
            dicMunicipio.Add("20033900", "20033900");
            dicMunicipio.Add("20033803", "20033803");
            dicMunicipio.Add("20032009", "20032009");
            dicMunicipio.Add("20032710", "20032710");
            dicMunicipio.Add("20031808", "20031808");
            dicMunicipio.Add("20032003", "20032003");
            dicMunicipio.Add("20032601", "20032601");
            dicMunicipio.Add("20032707", "20032707");
            dicMunicipio.Add("20033108", "20033108");
            dicMunicipio.Add("20032508", "20032508");
            dicMunicipio.Add("20032708", "20032708");
            dicMunicipio.Add("20033404", "20033404");
            dicMunicipio.Add("20033405", "20033405");
            dicMunicipio.Add("20033703", "20033703");
            dicMunicipio.Add("20033603", "20033603");
            dicMunicipio.Add("20031403", "20031403");
            dicMunicipio.Add("20031508", "20031508");
            dicMunicipio.Add("20033406", "20033406");
            dicMunicipio.Add("20031704", "20031704");
            dicMunicipio.Add("20032005", "20032005");
            dicMunicipio.Add("20031404", "20031404");
            dicMunicipio.Add("20033913", "20033913");
            dicMunicipio.Add("20033906", "20033906");
            dicMunicipio.Add("20031509", "20031509");
            dicMunicipio.Add("20031604", "20031604");
            dicMunicipio.Add("20031705", "20031705");
            dicMunicipio.Add("20033704", "20033704");
            dicMunicipio.Add("20032800", "20032800");
            dicMunicipio.Add("20031218", "20031218");
            dicMunicipio.Add("20032803", "20032803");
            dicMunicipio.Add("20032509", "20032509");
            dicMunicipio.Add("20032709", "20032709");
            dicMunicipio.Add("20033907", "20033907");
            dicMunicipio.Add("20032500", "20032500");
            dicMunicipio.Add("20032202", "20032202");
            dicMunicipio.Add("20032510", "20032510");
            dicMunicipio.Add("20033407", "20033407");
            dicMunicipio.Add("20031605", "20031605");
            dicMunicipio.Add("20031510", "20031510");
            dicMunicipio.Add("20031606", "20031606");
            dicMunicipio.Add("20033217", "20033217");
            dicMunicipio.Add("20031119", "20031119");
            dicMunicipio.Add("20033100", "20033100");
            dicMunicipio.Add("20031400", "20031400");
            dicMunicipio.Add("20033218", "20033218");
            dicMunicipio.Add("20033200", "20033200");
            dicMunicipio.Add("20031405", "20031405");
            dicMunicipio.Add("20033908", "20033908");
            dicMunicipio.Add("20032511", "20032511");
            dicMunicipio.Add("20033910", "20033910");
            dicMunicipio.Add("20031120", "20031120");
            dicMunicipio.Add("20033700", "20033700");
            dicMunicipio.Add("20033410", "20033410");
            dicMunicipio.Add("20033112", "20033112");
            dicMunicipio.Add("20033408", "20033408");
            dicMunicipio.Add("20033511", "20033511");
            dicMunicipio.Add("20033113", "20033113");
            dicMunicipio.Add("20031810", "20031810");
            dicMunicipio.Add("20033409", "20033409");
            dicMunicipio.Add("20033509", "20033509");
            dicMunicipio.Add("20033219", "20033219");
            dicMunicipio.Add("20033911", "20033911");
            dicMunicipio.Add("20031217", "20031217");
            dicMunicipio.Add("20033114", "20033114");
            #endregion

            #region Electrodependiente
            dicElectrodependiente.Add("N", "N");
            dicElectrodependiente.Add("P", "P");
            dicElectrodependiente.Add("R", "R");
            dicElectrodependiente.Add("V", "V");
            dicElectrodependiente.Add("T", "T");
            dicElectrodependiente.Add("E", "E");
            #endregion

            #region Estado Pod
            dicEstadoPod.Add("0", "0");
            dicEstadoPod.Add("2", "2");
            dicEstadoPod.Add("3", "3");
            dicEstadoPod.Add("4", "4");
            dicEstadoPod.Add("7", "7");
            dicEstadoPod.Add("8", "8");
            #endregion

            #region Propriedad Medidor
            dicPropriedadMedidor.Add("C", "C");
            dicPropriedadMedidor.Add("P", "P");
            #endregion

            #region Tarifa
            dicTarifa.Add("93", "93");
            dicTarifa.Add("97", "97");
            dicTarifa.Add("94", "94");
            dicTarifa.Add("95", "95");
            dicTarifa.Add("96", "96");
            dicTarifa.Add("90", "90");
            dicTarifa.Add("92", "92");

            #endregion

            #region Tipo de medida / Condicao Instalação
            dicTipoMedida.Add("01", "01");
            dicTipoMedida.Add("02", "02");
            dicTipoMedida.Add("03", "03");
            dicTipoMedida.Add("04", "04");
            dicTipoMedida.Add("05", "05");
            dicTipoMedida.Add("06", "06");
            #endregion

            #region Tipo de Rede
            dicTipoRede.Add("1", "1");
            dicTipoRede.Add("2", "2");
            dicTipoRede.Add("3", "3");
            dicTipoRede.Add("4", "4");
            dicTipoRede.Add("5", "5");
            dicTipoRede.Add("6", "6");
            dicTipoRede.Add("7", "7");
            dicTipoRede.Add("8", "8");
            dicTipoRede.Add("9", "9");
            dicTipoRede.Add("A", "A");
            dicTipoRede.Add("B", "B");
            dicTipoRede.Add("C", "C");
            dicTipoRede.Add("D", "D");
            dicTipoRede.Add("E", "E");
            dicTipoRede.Add("F", "F");
            dicTipoRede.Add("G", "G");
            dicTipoRede.Add("H", "H");
            dicTipoRede.Add("I", "I");
            dicTipoRede.Add("J", "J");
            dicTipoRede.Add("K", "K");
            dicTipoRede.Add("L", "L");
            dicTipoRede.Add("M", "M");
            dicTipoRede.Add("N", "N");
            dicTipoRede.Add("O", "O");
            dicTipoRede.Add("P", "P");
            dicTipoRede.Add("Q", "Q");
            dicTipoRede.Add("R", "R");
            dicTipoRede.Add("S", "S");
            dicTipoRede.Add("T", "T");
            dicTipoRede.Add("U", "U");
            dicTipoRede.Add("V", "V");
            dicTipoRede.Add("W", "W");
            dicTipoRede.Add("X", "X");
            dicTipoRede.Add("Y", "Y");
            dicTipoRede.Add("Z", "Z");
            #endregion

        }

        /// <summary>
        /// Romulo Silva - 14/03/2017
        /// Responsável por descobrir o DDD do cliente de acordo com o telefone
        /// </summary>
        /// <param name="telefone"></param>
        /// <returns></returns>
        private static string descobreDDD(string telefone, string municipio)
        {
            string prefixoTel = string.Empty;
            int ddd = 0;

            if (telefone.Length.Equals(10) || telefone.Length.Equals(11))
            {
                prefixoTel = telefone.Substring(0, 2);

                if (dicDDDTelefone.TryGetValue(municipio, out ddd))
                {
                    if (!prefixoTel.Equals(ddd))
                    {
                        telefone = ddd + telefone.Substring(2, (telefone.Length - 2));
                    }
                }
            }
            else if (telefone.Length.Equals(8) || telefone.Length.Equals(9))
            {
                if (dicDDDTelefone.TryGetValue(municipio, out ddd))
                {
                    telefone = ddd + telefone;
                }
                else
                {
                    telefone = string.Empty;
                }
            }
            else
            {
                telefone = string.Empty;
            }

            return telefone;
        }

        #region Validação Picklist
        public static void picklistTipoIdentidade(string tipoIdent, string numeroCliente)
        {
            string tipoIdentDic = string.Empty;
            if (!dicTipoIdentidade.TryGetValue(tipoIdent, out tipoIdentDic))
            {
                escreveLogErro("\"" + numeroCliente + "\";\"Ausente Picklist Tipo Identidade\";\"" + tipoIdent + "\"");
            }
        }

        public static void picklistClasse(string classe, string numeroCliente)
        {
            string tipoIdentDic = string.Empty;
            if (!dicClasse.TryGetValue(classe, out tipoIdentDic))
            {
                escreveLogErro("\"" + numeroCliente + "\";\"Ausente Picklist Classe\";\"" + classe + "\"");
            }
        }
        public static void picklistMunicipio(string municipio, string numeroCliente)
        {
            string tipoIdentDic = string.Empty;
            if (!dicMunicipio.TryGetValue(municipio, out tipoIdentDic))
            {
                escreveLogErro("\"" + numeroCliente + "\";\"Ausente Picklist Municipio\";\"" + municipio + "\"");
            }
        }

        public static void picklistElectrodependiente(string electrodependiente, string numeroCliente)
        {
            string tipoIdentDic = string.Empty;
            if (!dicElectrodependiente.TryGetValue(electrodependiente, out tipoIdentDic))
            {
                escreveLogErro("\"" + numeroCliente + "\";\"Ausente Picklist Electrodepiendente\";\"" + electrodependiente + "\"");
            }
        }

        public static void picklistEstadoConexion(string estadoConexion, string numeroCliente)
        {
            string tipoIdentDic = string.Empty;
            if (!estadoConexion.Equals("0") && !estadoConexion.Equals("1"))
            {
                escreveLogErro("\"" + numeroCliente + "\";\"Ausente Picklist Estado Conexion\";\"" + estadoConexion + "\"");
            }
        }

        public static void picklistEstadoPod(string estadoPod, string numeroCliente)
        {
            string tipoIdentDic = string.Empty;
            if (!dicEstadoPod.TryGetValue(estadoPod, out tipoIdentDic))
            {
                escreveLogErro("\"" + numeroCliente + "\";\"Ausente Picklist Estado Pod\";\"" + estadoPod + "\"");
            }
        }

        public static void picklistPropriedadMedidor(string propriedadMedidor, string numeroCliente)
        {
            string tipoIdentDic = string.Empty;
            if (!dicPropriedadMedidor.TryGetValue(propriedadMedidor, out tipoIdentDic))
            {
                escreveLogErro("\"" + numeroCliente + "\";\"Ausente Picklist Propriedad Medidor\";\"" + propriedadMedidor + "\"");
            }
        }

        public static void picklistRegion(string region, string numeroCliente)
        {
            string tipoIdentDic = string.Empty;
            if (!region.Equals("23") && !region.Equals("33"))
            {
                escreveLogErro("\"" + numeroCliente + "\";\"Ausente Picklist Region\";\"" + region + "\"");
            }
        }

        public static void picklistSubClasse(string subClasse, string numeroCliente)
        {
            string tipoIdentDic = string.Empty;
            if (!subClasse.Equals("A") && !subClasse.Equals("B"))
            {
                escreveLogErro("\"" + numeroCliente + "\";\"Ausente Picklist subClasse\";\"" + subClasse + "\"");
            }
        }

        public static void picklistTipoMedida(string tipoMedida, string numeroCliente)
        {
            string tipoIdentDic = string.Empty;
            if (!string.IsNullOrEmpty(tipoMedida) && !dicTipoMedida.TryGetValue(tipoMedida, out tipoIdentDic))
            {
                escreveLogErro("\"" + numeroCliente + "\";\"Ausente Picklist Tipo Medida\";\"" + tipoMedida + "\"");
            }
        }

        public static void picklistTipoRede(string tipoRede, string numeroCliente)
        {
            string tipoIdentDic = string.Empty;
            if (!dicTipoRede.TryGetValue(tipoRede, out tipoIdentDic))
            {
                escreveLogErro("\"" + numeroCliente + "\";\"Ausente Picklist Tipo Rede\";\"" + tipoRede + "\"");
            }
        }
        #endregion

        public static void escreveLogErro(string conteudo)
        {
            using (System.IO.StreamWriter file2 =
                new System.IO.StreamWriter(ExtracaoSalesForce.outputArquivoLog, true, Encoding.GetEncoding("ISO-8859-1")))
            {
                file2.WriteLine(conteudo);
            }
        }

        internal static void ApagarArquivo(string nomeArquivo)
        {
            File.Delete(@"" + ExtracaoSalesForce.outputArquivo + nomeArquivo + ".csv");
        }

        public static List<T> ConvertDataTableToList<T>(DataTable resultDt)
        {
            List<T> lista = new List<T>();

            if (resultDt != null && resultDt.Rows.Count > 0)
            {
                Type type = typeof(T);
                foreach (DataRow dr in resultDt.Rows)
                {
                    object entity = (T)Activator.CreateInstance(typeof(T));
                    foreach (PropertyInfo pi in type.GetProperties())
                    {
                        if (!resultDt.Columns.Contains(pi.Name)) continue;
                        if (DBNull.Value.Equals(dr[pi.Name]) || string.IsNullOrWhiteSpace(dr[pi.Name].ToString())) continue;

                        object valor = dr[pi.Name].GetType() == typeof(TimeSpan) ? dr[pi.Name].ToString() : dr[pi.Name];
                        object valorTipado = null;
                        if (pi.PropertyType.IsGenericType && pi.PropertyType.GetGenericTypeDefinition().Equals(typeof(Nullable<>)))
                        {
                            if (valor == null)
                            {
                                valorTipado = default(T);
                            }

                            type = Nullable.GetUnderlyingType(pi.PropertyType);
                            valorTipado = Convert.ChangeType(valor, type);
                        }
                        else
                            valorTipado = Convert.ChangeType(valor, pi.PropertyType);

                        //Tratamento de Espaços no final
                        if (valorTipado.GetType() == typeof(string))
                            valorTipado = valor.ToString().Trim();


                        pi.SetValue(entity, valorTipado, null);
                    }

                    lista.Add((T)entity);
                }
            }

            return lista;
        }
    }
}

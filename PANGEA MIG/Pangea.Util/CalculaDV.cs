using System;

namespace Pangea.Util
{
    public class CalculaDV
    {
        public int CalculaDvCli(string numCli)
        {
            int tam = numCli.Length - 1;
            int soma = 0;
            int fator = 2;
            int digito = 0;

            for (int count = tam; count > 0; count--)
            {
                digito = Convert.ToInt32(numCli.Substring(count, 1));
                soma += Convert.ToInt32(digito) * fator;
                if (fator == 30)
                    fator = 2;
                else
                    fator++;
            }

            digito = 11 - (soma - (soma / 11) * 11);
            if (digito == 11)
                digito = 0;
            else if (digito == 10)
                digito = 0;

            return digito;
        }

        #region VALIDA E CALCULA CGC
        public static bool ValidaDvCGC(string numDoc, string dvDoc, string tipoDoc)
        {
            string numReal = GeraNumDocReal(numDoc, tipoDoc);
            if (!(numReal.Length > 4 && numReal.Length < 13))
                return false;
            
            string result = CalculaDvCGC(numDoc, tipoDoc);
            return result.Equals(dvDoc);
        }

        public static string CalculaDvCGC(string numDoc, string tipoDoc)
        {
            numDoc = GeraNumDocReal(numDoc, tipoDoc);

            int digito1, digito2;
            int soma = 0;
            int fator = 2;

            for (int i = numDoc.Length; i > 0; i--)
            {
                digito1 = Convert.ToInt32(numDoc.Substring(i - 1, 1));
                soma += Convert.ToInt32(digito1) * fator;
                if (fator == 9) fator = 2;
                else fator++;
            }

            digito1 = 11 - (soma % 11);
            if (digito1 == 11 || digito1 == 10)
                digito1 = 0;

            numDoc += digito1.ToString();
            soma = 0;
            fator = 2;

            for (int i = numDoc.Length; i > 0; i--)
            {
                digito2 = Convert.ToInt32(numDoc.Substring(i - 1, 1));
                soma += Convert.ToInt32(digito2) * fator;
                if (fator == 9) fator = 2;
                else fator++;
            }

            digito2 = 11 - (soma % 11);
            if (digito2 == 11 || digito2 == 10)
                digito2 = 0;

            return digito1.ToString() + digito2.ToString();
        }
        #endregion VALIDA E CALCULA CGC

        #region VALIDA E CALCULA CPF
        public static bool ValidaDvCPF(string numDoc, string dvDoc)
        {
            string tipoDoc = "005";
            string numReal = GeraNumDocReal(numDoc,tipoDoc);
            if (!(numReal.Length > 4 && numReal.Length < 13))
                return false;

            string result = CalculaDvCPF(numDoc);
            return result.Equals(dvDoc);
        }

        public static string CalculaDvCPF(string numDoc)
        {
            string tipoDoc = "005";
            numDoc = GeraNumDocReal(numDoc, tipoDoc);

            int digito1, digito2, digitoAtual;
            int soma = 0;
            int somaDigito = 0;
            int fator = 2;

            for (int i = numDoc.Length; i > 0; i--)
            {
                digitoAtual = Convert.ToInt32(numDoc.Substring(i - 1, 1));
                soma += Convert.ToInt32(digitoAtual) * fator;
                somaDigito += digitoAtual;
                fator++;
            }

            digito1 = 11 - (soma % 11);
            if (digito1 == 11 || digito1 == 10)
                digito1 = 0;

            soma += (digito1 * 2) + somaDigito;
            digito2 = 11 - (soma % 11);
            if (digito2 == 11 || digito2 == 10)
                digito2 = 0;

            return digito1.ToString() + digito2.ToString();
        }
        #endregion VALIDA E CALCULA CPF

        private static string GeraNumDocReal(string numDoc, string tipoDoc)
        {
            try
            {
                switch (tipoDoc)
                {
                    case "002":     //CNPJ
                        return numDoc.Substring(8);
                    case "005":     //CPF
                        return numDoc.Substring(11);
                    default:
                        return numDoc.Substring(9);
                }
            }
            catch
            { return numDoc; }
        }
    }
}
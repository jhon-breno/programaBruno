using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Linq;

namespace Pangea.Util
{
    public class Validacao
    {
        public static StringBuilder getValidationErros(object obj)
        {
            StringBuilder sb =  new StringBuilder();
            var resultadoValidacao = new List<ValidationResult>();
            var contexto = new ValidationContext(obj, null, null);
            Validator.TryValidateObject(obj, contexto, resultadoValidacao, true);

            foreach (var error in resultadoValidacao)
            {
                sb.AppendLine(error.ErrorMessage);
            }

            return sb;
        }

        public static bool ValidarCPF(ref string doc)
        {
            string cpf = doc;
            cpf = cpf.Replace(" ", "").PadLeft(22, '0').Substring(22 - 11, 11);

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
                cpf == "77777777777" || cpf == "88888888888" || cpf == "99999999999" || verificaRepetidas(cpf.Substring(0, 9)))
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

            doc = cpf.EndsWith(digito) ? cpf : doc;
            return cpf.EndsWith(digito);
        }

        public static bool ValidarCNPJ(ref string doc)
        {
            string cnpj = doc;
            cnpj = cnpj.Replace(" ", "").PadLeft(22, '0').Substring(22 - 14, 14);

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
                cnpj == "77777777777777" || cnpj == "88888888888888" || cnpj == "99999999999999" || verificaRepetidas(cnpj.Substring(0, 12)))
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

            doc = cnpj.EndsWith(digito) ? cnpj : doc;
            return cnpj.EndsWith(digito);
        }

        public static bool IsNumeric(string value)
        {
            foreach (char c in value.ToCharArray())
            {
                if (char.IsNumber(c))
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
        /// Romulo Silva - 23/01/2017
        /// Verifica se existem caracteres repetidos, dado um valor e a posição inicial e final da variável a verificar
        /// </summary>
        /// <param name="documento">variável que contém o valor a verificar</param>
        /// <returns></returns>
        private static bool verificaRepetidas(string documento)
        {
            return documento.Length == 0 || documento.All(ch => ch == documento[0]);
        }
    }
}

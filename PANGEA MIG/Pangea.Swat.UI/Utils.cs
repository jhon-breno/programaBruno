using Pangea.Entidades.Enumeracao;
using Pangea.Swat.Dados;
using Pangea.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Pangea.Swat.UI
{
    public class Utils
    {
        internal static ExecutorDB GetExecutor(Empresa empresa, TipoCliente tipoCliente)
        {
            return new ExecutorDB(empresa, EnumString.GetStringValue(tipoCliente));
        }


        /// <summary>
        /// Formata o documento informado diferenciando o formato de CNPJ de todos os demais.
        /// </summary>
        /// <param name="documento"></param>
        /// <param name="tipoDocumento"></param>
        /// <returns></returns>
        public static string FormatarDocumento(string documento, TipoDocumento tipoDocumento)
        {
            if (string.IsNullOrWhiteSpace(documento))
                throw new ArgumentException("Documento não infomado.", "documento");

            if (TipoDocumento.NaoIdentificado == tipoDocumento)
                return documento.Length > 21 ? documento.Trim().Substring(0, 21) : documento.Trim();

            try
            {
                if (tipoDocumento == TipoDocumento.CNPJ)
                    return Regex.Replace(documento.Trim(), " ", "").PadLeft(22, '0').Substring(22 - 14, 14);

                return Regex.Replace(documento.Trim(), " ", "").PadLeft(22, '0').Substring(22 - 11, 11);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}

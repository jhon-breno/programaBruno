using Newtonsoft.Json;
using SalesforceExtractor.Entidades;
using SalesforceExtractor.Entidades.Enumeracoes;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;

namespace SalesforceExtractor.Dados
{
    public static class ExtratorFactory
    {
        /// <summary>
        /// Retorna classes extratoras para cada tipo de cliente: [SalesforceExtractor.Dados] ExtratorGA ou ExtratorGB
        /// </summary>
        /// <param name="tipoCliente"></param>
        /// <returns></returns>
        public static Extrator GetExtrator(string tipoCliente)
        {
            if("gb".Equals(tipoCliente.ToLower()))
                return new ExtratorGB();
            
            return new ExtratorGA();
        }
    }
}

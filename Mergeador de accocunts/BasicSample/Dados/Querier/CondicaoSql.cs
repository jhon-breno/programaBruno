using Newtonsoft.Json;
using SalesforceExtractor.Entidades;
using SalesforceExtractor.Entidades.Enumeracoes;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;

namespace SalesforceExtractor.Dados.Querier
{
    public class CondicaoSql
    {
        public bool Fixo{ get; set; }
    }

    public class CondicaoSimplesSql : CondicaoSql
    {
        public string Campo { get; set; }
        public List<string> Valor { get; set; }
    }

    public class CondicaoMultiplaSql : CondicaoSql
    {
        public Dictionary<string, List<string>> Grupo { get; set; }
    }
}

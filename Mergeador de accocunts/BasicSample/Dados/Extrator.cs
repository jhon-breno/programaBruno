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
    public abstract class Extrator
    {
        public abstract List<ItemAttribute> GetItemAttributes(Arquivo arquivo, Type tipoItem);
    }
}

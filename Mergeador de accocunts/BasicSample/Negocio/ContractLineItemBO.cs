using IBM.Data.Informix;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Solus.Controle;
using System.Data;
using System.Configuration;
using System.IO;
//using Synapsis.FtpLib;
//using ExtracaoSalesForce.Modelo.SalesForce;
//using Chilkat;
using System.Diagnostics;
using SalesforceExtractor.Utils;
using SalesforceExtractor.Entidades;
using SalesforceExtractor.Dados.SalesForce;
using SalesforceExtractor.Entidades.Modif;
using SalesforceExtractor.Entidades.Enumeracoes;
using System.Threading;
using SalesforceExtractor.Dados;
using SalesforceExtractor.apex;
using basicSample_cs_p;

namespace BasicSample.Negocio
{
    public class ContractLineItemBO : NegocioBase
    {
        public ContractLineItemBO(string ambiente, string codigoEmpresa, SforceService binding)
            : base(ambiente, binding, codigoEmpresa)
        {
        }


        public ContractLineItemBO(string ambiente, SforceService binding)
            : base(ambiente, binding, null)
        {
        }
    }
}

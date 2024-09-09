using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace SalesforceExtractor.Entidades
{
    /// <summary>
    /// Arquivo-base de extração do Synergia
    /// </summary>
    [DebuggerDisplay("ExtId:{AssetExternalId} Classe:{ClasseSAP} Pot:{Potencia}")]
    public class B2WinDTO : ItemAttribute
    {
        public string Status { get; set; }
        public string Erro { get; set; }
        public string Id { get; set; }

        public override string ToString()
        {
            return string.Concat(this.Id, "\t", this.ExternalIdAccount, "\t", this.ExternalIdAsset, "\t", this.ExternalIdPod, "\t", this.ExternalIdContract, "\t", this.Id, "\t", this.Status, "\t", this.Erro);
        }
    }
}

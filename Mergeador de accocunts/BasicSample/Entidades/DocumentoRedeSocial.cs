using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SalesforceExtractor.Entidades
{
    public class DocumentoRedeSocial
    {
        /// <summary>
        /// 2005=RJ, 2003=CE
        /// </summary>
        public string codigoEmpresa;
        /// <summary>
        /// Modelo de Entidade no SF relacionado ao objeto manipulado
        /// </summary>
        public string entidadeSF;
        /// <summary>
        /// Número do documento do Contato, usado como Id para identificação do contato
        /// </summary>
        public string documentoNumero;
        /// <summary>
        /// Dado da Rede Social do Contato
        /// </summary>
        public string redeSocialId;
        /// <summary>
        /// Campo no SF referente ao dado inserido no campo RedeSocialId
        /// Facebook=sf4twitter__Fcbk_Username__c;  Twitter=sf4twitter__Twitter_Username__c
        /// </summary>
        public string camposRedeSocial; 
    }
}

using Pangea.Entidades.Enumeracao;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Pangea.Entidades
{
    public class Log
    {
        /// <summary>
        /// Código interno Ayesa relativo especificamente a cada Integração.
        /// </summary>
        public string codigoPangea { get; set; }

        /// <summary>
        /// Data do registro no log.
        /// </summary>
        public bool dataIngresso { get; private set; }

        /// <summary>
        /// Indica se é um log de uma operação de sucesso ou erro.
        /// </summary>
        public bool sucesso { get; set; }

        /// <summary>
        /// Qualquer informação que permita reproduzir o cenário envolvido no log. 
        /// Ex.: parâmetros, primary key, etc.
        /// </summary>
        public string dadosOrigem { get; set; }

        /// <summary>
        /// Observações relevantes a serem registradas.
        /// </summary>
        public string descricao { get; set; }

        /// <summary>
        /// Nome completo do fonte, como: classe, processo C, 
        /// </summary>
        public string origem { get; set; }

        /// <summary>
        /// Quantidade de registros afetados.
        /// Por conceito, as operações de sucesso devem registrar a quantidade total de registros afetados.
        /// Em caso de erro, a quantidade será sempre 1, para efeito de estatística.
        /// </summary>
        public int quantidade { get { return this.quantidade; } set { value = !this.sucesso ? 1 : value; } }

        /// <summary>
        /// Define a base de dados onde o log será ingressado.
        /// </summary>
        public Empresa empresa { get; set; }
    }
}

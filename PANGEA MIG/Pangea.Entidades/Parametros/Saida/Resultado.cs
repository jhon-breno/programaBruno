using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Pangea.Entidades.Enumeracao;

namespace Pangea.Entidades.Parametros.Saida
{
    public class Resultado
    {
        //Metodo era para funcionar como padrao fabrica, porem teve que ser modificado pois estava retornando dados invalidos nas consultas.
        private static Resultado resultadoVazio = null;
        public static Resultado GetResultadoVazio()
        {
            resultadoVazio = new Resultado();

            return resultadoVazio;
        }

        public Resultado()
        {
            this.Codigo = (int)CodigoDeRetorno.OperacaoInvalida;
            this.Mensagem = string.Empty;
            this.Retorno = null;
        }

        public int Codigo { get; set; }

        public string Mensagem { get; set; }        

        public object Retorno { get; set; }
    }
}

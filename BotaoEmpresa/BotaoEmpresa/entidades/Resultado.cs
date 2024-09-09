namespace BotaoEmpresa.entidades
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
            this.Codigo = 3;
            this.Mensagem = string.Empty;
            this.Retorno = null;
        }

        public int Codigo { get; set; }

        public string Mensagem { get; set; }

        public object Retorno { get; set; }
    } 
}
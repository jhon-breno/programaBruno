using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Pangea.Entidades
{
    public class MovimentosSolucoes
    {
        public int totalSolicitacoes { get; set; }
        public int totalSucessos { get; set; }
        public int totalErros { get; set; }
        //public List<RetornoSolicitacaoTipoC> retornos { get; set; }
        public string Status { get; set; }
        public Object retornos { get; set; }

         //Eliminando construtores sem mensagem de status
         
        public MovimentosSolucoes()
        {
            totalSolicitacoes = 0;
            totalSucessos = 0;
            totalErros = 0;
            
        }
        /*
        public MovimentosSolucoes(int total, int sucesso, int erro, List<RetornoSolicitacaoTipoC> r)
        {
            this.totalSolicitacoes = total;
            this.totalSucessos = sucesso;
            this.totalErros = erro;
            this.Status = "";
            this.retornos = r;
        }
        
        public MovimentosSolucoes(int total, int sucesso, int erro, Object o)
        {
            this.totalSolicitacoes = total;
            this.totalSucessos = sucesso;
            this.totalErros = erro;
            this.Status = "";
            this.retornos = o;
        }
         */
        public MovimentosSolucoes(int total, int sucesso, int erro, string msg, Object o)
        {
            this.totalSolicitacoes = total;
            this.totalSucessos = sucesso;
            this.totalErros = erro;
            this.Status = msg;
            if (total == 0)
            {
                this.retornos = null;
            }
            else
            {
                this.retornos = o;
            }
        }

        public MovimentosSolucoes(int total, string msg)
        {
            this.totalSolicitacoes = total;
            this.totalSucessos = 0;
            this.totalErros = total;
            this.Status = msg;
            this.retornos = null;
        }

        public MovimentosSolucoes(int total, string msg, Object o)
        {
            this.totalSolicitacoes = total;
            this.totalSucessos = total;
            this.totalErros = 0;
            this.Status = msg;

            if (total == 0)
            {
                this.retornos = null;
            }
            else
            {
                this.retornos = o;
            }
        }

    }
}

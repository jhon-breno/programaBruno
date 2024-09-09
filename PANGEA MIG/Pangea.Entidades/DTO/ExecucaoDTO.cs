using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;


namespace Pangea.Entidades.DTO
{
    public class ExecucaoDTO
    {
       // public ExecucaoDTO(DataRow dr) {
            //this.id_pangea_config = TratarInt;//(resultDt, dr[0], "numero_livro", -1); 
        //    this.classeExecutante = dr[1].ToString();
            //this.periodicidade = dr[2].ToString();
            //this.unidade = Convert.ToInt32(dr[3]);
        //    this.periodo = gerarPeriodo(dr[2].ToString(),Convert.ToInt32(dr[3]));
        //    this.ativo = true;//Convert.ToBoolean(dr[4]);
        //    this.ultimaExecucao = Convert.ToDateTime(dr[5]);
       // }

        public string cod_pangea_integracao { get; set; }
        public string classeExecutante { get; set; }
        public TimeSpan periodo { get; set; }
        public DateTime ultimaExecucao { get; set; }
        public bool ativo { get; set; }

        

    }
}

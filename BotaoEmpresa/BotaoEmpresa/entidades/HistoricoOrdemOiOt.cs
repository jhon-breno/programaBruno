using System;
using System.Collections.Generic;

namespace BotaoEmpresa.entidades
{
    public class HistoricoOrdemOiOt
    {
        public string NumTdc { get; set; }
        public string Situacao { get; set; }
        public string Executante { get; set; }
        public string Nome { get; set; }
        public string Endereco { get; set; }
        public string CadNumCoelce { get; set; }
        public string CadNumFabrica { get; set; }
        public string CadAlg { get; set; }
        public string CadFabricante { get; set; }
        public string CadDescricao { get; set; }
        public string CadTipo { get; set; }
        public string CadModelo { get; set; }
        public string CadUltimaLeitura { get; set; }
        public string CadUltLeituraHp { get; set; }
        public string CadUltLeituraReat { get; set; }
        public DateTime CadUltimaInstalacao { get; set; }
        public string EncNumCoelce { get; set; }
        public string EncNumFabrica { get; set; }
        public string EncAlg { get; set; }
        public string EncFabricante { get; set; }
        public string EncDescricao { get; set; }
        public string EncTipo { get; set; }
        public string EncModelo { get; set; }
        public string EncUltimaLeitura { get; set; }
        public string EncUltLeituraHp { get; set; }
        public string EncUltLeituraReat { get; set; }
        public List<string> Anormalidades { get; set; }
        public string AnorTotal { get; set; }
        public List<string> Ngs { get; set; }
        public string NgTotal { get; set; }
        public string Observacoes { get; set; }
        public DateTime DataExec { get; set; }
        public DateTime HoraIniExec { get; set; }
        public DateTime HoraFimExec { get; set; }
        public List<string> RespNgs { get; set; }
    }
}
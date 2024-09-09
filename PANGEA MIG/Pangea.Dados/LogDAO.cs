using Pangea.Entidades.DTO;
using Pangea.Dados.Base;
using Pangea.Entidades;
using Pangea.Entidades.Enumeracao;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

namespace Pangea.Dados
{
    public class LogDAO : BaseDAO
    {
        public LogDAO(Empresa empresa)
            : base(empresa)
        {
        }

        public bool Inserir(string codigo, string sucesso, string dadosOrigem, string descricao, string origem, int quantidade)
        {
            DBProviderInformix conn = ObterProviderInformix();

            String sql = String.Format(@"insert into pangea_log_servico 
                                        (cod_pangea_integracao,
                                        data_ingresso,
                                        sucesso,
                                        dado_origem,
                                        descricao,
                                        origem,
                                        quantidade) values 
                                        ('{0}',
                                        current,
                                        '{1}',
                                        '{2}',
                                        '{3}',
                                        '{4}',
                                        {5})", codigo, sucesso, dadosOrigem, descricao, origem, quantidade);

            return ExecutarSql(sql.ToString(), conn);
        }

        public bool Inserir(string codigo, string sucesso, string dadosOrigem, string descricao, string origem, int quantidade, DBProviderInformix conn)
        {
            String sql = String.Format(@"insert into pangea_log_servico 
                                        (cod_pangea_integracao,
                                        data_ingresso,
                                        sucesso,
                                        dado_origem,
                                        descricao,
                                        origem,
                                        quantidade) values 
                                        ('{0}',
                                        current,
                                        '{1}',
                                        '{2}',
                                        '{3}',
                                        '{4}',
                                        {5})", codigo, sucesso, dadosOrigem, descricao, origem, quantidade);

            return ExecutarSql(sql.ToString(), conn);
        }

        public bool InsertJsonEntrada(string codigoPangea, string json, string descricao)
        {
            string sql = string.Format(@"INSERT INTO pangea_log_servico (cod_pangea_integracao,data_ingresso,descricao,origem) 
						               VALUES ('{0}',CURRENT,'{1}','{2}')", codigoPangea, json, descricao);

            DBProviderInformix informix = ObterProviderInformix();
            return ExecutarSql(sql, informix);
        }
    }
}

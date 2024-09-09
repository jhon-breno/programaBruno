using Pangea.Entidades.DTO;
using Pangea.Dados.Base;
using Pangea.Dados;
using Pangea.Entidades;
using Pangea.Entidades.Enumeracao;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

namespace Pangea.Dados
{
    public class RegistradorDAO : BaseDAO
    {
        private string _empresa;

        public RegistradorDAO(Empresa empresa)
            : base(empresa)
        {
            if (empresa != null)
                this._empresa = empresa.ToString();
        }

        public RegistradorDAO(Empresa empresa, string DataBase)
            : base(empresa, DataBase)
        {
            if (empresa != null)
                this._empresa = empresa.ToString();
        }


        public RegistradorDTO Consultar(RegistradorDTO registrador)
        {
            //if (obj == null)
            //    return new DataTable();

            if (string.IsNullOrEmpty(this._empresa))
            {
                //TODO: gerar log antes de lançar erro
                throw new ArgumentException("Parâmetro empresa obrigatório para a consultar dados do registrador");
            }

            #region Prepara a consulta básica
            //Campo Valor
            String sql = String.Format("select first 1 {0} from {1} where numero_cliente = {2} order by fecha_evento desc", registrador.campoValor, registrador.tabelaValor, registrador.numero_cliente);

            if (ConsultaSql(sql).Rows.Count > 0)
                registrador.valor = ConsultaSql(sql).Rows[0][0].ToString();

            //Campo inteiro
            if (!String.IsNullOrWhiteSpace(registrador.tabelaInteiro))
            {
                sql = String.Format("select {0} from {1} where numero_cliente = {2}", registrador.campoInteiro, registrador.tabelaInteiro, registrador.numero_cliente);
                if (ConsultaSql(sql).Rows.Count > 0)
                    registrador.valorInteiro = ConsultaSql(sql).Rows[0][0].ToString();
            }


            //Campo decimal
            if (!String.IsNullOrWhiteSpace(registrador.tabelaDecimal))
            {
                sql = String.Format("select {0} from {1} where numero_cliente = {2}", registrador.campoDecimal, registrador.tabelaDecimal, registrador.numero_cliente);
                if (ConsultaSql(sql).Rows.Count > 0)
                    registrador.valorDecimal = ConsultaSql(sql).Rows[0][0].ToString();

            }
            return registrador;



            #endregion

            
        }

        //public  IList<TEntidade> dtToListObject<TEntidade>(DataTable dt)
        //{
        //    throw new NotImplementedException();
        //}
    }
}

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
using System.Globalization;

namespace Pangea.Dados
{
    public class V2ComunicaDAO : BaseDAO
    {
        private string _empresa;

        public V2ComunicaDAO(Empresa empresa)
            : base(empresa)
        {
            if (empresa.Equals(Empresa.NaoIdentificada))
                this._empresa = empresa.ToString();
        }

        public DataTable Consultar(V2ComunicaDTO obj)
        {
            if (obj == null)
                return new DataTable();

            if (string.IsNullOrEmpty(this._empresa))
            {
                //TODO: gerar log antes de lançar erro
                throw new ArgumentException("Parâmetro empresa obrigatório para a consulta na V2_Comunica.");
            }

            #region Prepara a consulta básica 

            StringBuilder sql = new StringBuilder(@"Select first 1 v.id_orion
                                                          , v.codigo_cp
                                                          , v.codigo_cs
                                                          , v.codigo_ps
                                                       from v2_comunica v
                                                          , cliente_smc c
                                                      where c.codigo_cp      = v.codigo_cp
                                                        and c.codigo_cs      = v.codigo_cs
                                                        and c.codigo_ps      = v.codigo_ps");
            
            if (!string.IsNullOrEmpty(obj.numero_cliente))
                sql.AppendFormat(" AND c.numero_cliente = {0} ", obj.numero_cliente);

            if (!string.IsNullOrEmpty(obj.comando))
                sql.AppendFormat(" AND v.comando in ({0}) ", obj.comando);

            if (!string.IsNullOrEmpty(obj.estado))
                sql.AppendFormat(" AND v.estado = '{0}' ", obj.estado);

            #endregion

            return ConsultaSql(sql.ToString());
        }


        /// <summary>
        /// Ingressa um comando a ser enviado para o Synegra, via a tabela V2_COMUNICA
        /// </summary>
        /// <param name="cliente"></param>
        /// <param name="conn"></param>
        public bool Ingressar(V2ComunicaDTO v2, DBProviderInformix conn)
        {
            if (v2 == null)
                return false;

            #region Prepara a instrução

            StringBuilder sql = new StringBuilder("insert into v2_comunica (");
            sql.Append("numero_sol, comando, codigo_cp, codigo_cs, codigo_ps, ");
            sql.Append("estado, data_ingresso, data_inicio, data_executa, data_modifica, ");
            sql.Append("prioridade, id_orion, total_cliente ) values (");

            sql.AppendFormat("   {0}, '{1}', {2},   {3},   {4}", v2.numero_sol, v2.comando, v2.codigo_cp, v2.codigo_cs, v2.codigo_ps);  //1..5
            sql.AppendFormat(", '{0}' ", v2.estado);
            sql.AppendFormat(",  {0} ", v2.data_ingresso == DateTime.MinValue ? "current" : v2.data_ingresso.ToString("yyyy-MM-dd HH:mm:ss"));
            sql.AppendFormat(",  {0} ", v2.data_inicio == DateTime.MinValue ? "current" : v2.data_inicio.ToString("yyyy-MM-dd HH:mm:ss"));
            sql.AppendFormat(",  {0} ", v2.data_executa == DateTime.MinValue ? "current" : v2.data_executa.ToString("yyyy-MM-dd HH:mm:ss"));
            sql.AppendFormat(",  {0} ", v2.data_modifica == DateTime.MinValue ? "current" : v2.data_modifica.ToString("yyyy-MM-dd HH:mm:ss")); 
            sql.AppendFormat(",  {0},  {1},  {2} )", v2.prioridade, v2.id_orion, v2.total_cliente);                                     //11..13
            
            #endregion

            return ExecutarSql(sql.ToString(), conn);
        }
    }
}

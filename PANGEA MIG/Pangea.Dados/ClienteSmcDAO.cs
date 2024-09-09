using Entidades.DTO;
using Pangea.Dados.Base;
using Pangea.Entidades;
using Pangea.Entidades.DTO;
using Pangea.Entidades.Enumeracao;
using Pangea.Util;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Pangea.Dados
{
    public class ClienteSmcDAO : BaseDAO
    {
        private Empresa empresa;
        public ClienteSmcDAO(Empresa empresa)
            : base(empresa)
        {
            this.empresa = empresa;
        }

        public ClienteSmcDTO RetornaClienteSMC(string numero_cliente)
        {
            string sql = string.Format(@"select numero_cliente,
                                                  codigo_cp,
                                                  codigo_cs,
                                                  codigo_ps,
                                                  shunt1,
                                                  shunt2,
                                                  shunt3,
                                                  tipo_ligacao,
                                                  ultima_lectura,
                                                  fecha_activacion,
                                                  fecha_modificacion,
                                                  fecha_desactivac,
                                                  tipo_med,
                                                  fact_smc,
                                                  fator,
  			                                     modulo_controle
                                             from cliente_smc
  	                                      where numero_cliente = {0}
  	                                      and fecha_desactivac is null",numero_cliente);

            var dt = ConsultaSql(sql.ToString());
            if (dt.Rows.Count > 0)
            {
                ClienteSmcDTO resultado = DataHelper.ConvertDataTableToEntity<ClienteSmcDTO>(dt);

                return (ClienteSmcDTO)resultado;
            }
            else
                return null;
        }

        public string RetornaTipoMed(string numero_cliiente)
        {
            string retorno = string.Empty;

            string sql = string.Format(@"select tipo_med from cliente_smc
		                                   where numero_cliente = {0}
		                                   and nvl(fecha_desactivac,'') = ''",numero_cliiente);

             var dt = ConsultaSql(sql.ToString());
             if (dt.Rows.Count > 0)
             {
                 retorno = dt.Rows[0]["tipo_med"].ToString();
             }
             else
                 retorno = null;

             return retorno;

        }

        public int VerificaCliSmcCpAtivo(ContratoDTO contrato)
        {
            int resultado = 0;
            string sql = string.Format(@"SELECT COUNT(*)                                            
                                            FROM cliente_smc 
                                            WHERE numero_cliente = {0}
                                            AND fecha_desactivac IS NULL;",contrato.numero_cliente);

            DataTable dt = ConsultaSql(sql.ToString());

            if (dt.Rows.Count > 0)
            {
                resultado = Convert.ToInt32(dt.Rows[0][0]);
                return resultado;
            }

            else
                return resultado;

        }

        public bool AtualizaClienteSMC (ClienteSmcDTO cliente_smc)
        {
            StringBuilder sql = new StringBuilder();
            sql.Append(" update cliente_smc set ");
            sql.AppendFormat(!String.IsNullOrWhiteSpace(cliente_smc.codigo_cp) ? "codigo_cp = '{0}' ," : "", cliente_smc.codigo_cp);
            sql.AppendFormat(!String.IsNullOrWhiteSpace(cliente_smc.codigo_cs) ? "codigo_cs = '{0}' ," : "", cliente_smc.codigo_cs);
            sql.AppendFormat(!String.IsNullOrWhiteSpace(cliente_smc.codigo_ps) ? "codigo_ps = '{0}' ," : "", cliente_smc.codigo_ps);
            sql.AppendFormat(!String.IsNullOrWhiteSpace(cliente_smc.shunt1) ? "shunt1 = '{0}' ," : "", cliente_smc.shunt1);
            sql.AppendFormat(!String.IsNullOrWhiteSpace(cliente_smc.shunt2) ? "shunt2 = '{0}' ," : "", cliente_smc.shunt2);
            sql.AppendFormat(!String.IsNullOrWhiteSpace(cliente_smc.shunt3) ? "shunt3 = '{0}' ," : "", cliente_smc.shunt3);
            sql.AppendFormat(!String.IsNullOrWhiteSpace(cliente_smc.tipo_ligacao) ? "tipo_ligacao = '{0}' ," : "", cliente_smc.tipo_ligacao);
            sql.AppendFormat(!String.IsNullOrWhiteSpace(cliente_smc.ultima_lectura) ? "ultima_lectura = '{0}' ," : "", cliente_smc.ultima_lectura);
            sql.AppendFormat(!String.IsNullOrWhiteSpace(cliente_smc.fecha_activacion) ? "fecha_activacion = '{0}' ," : "", cliente_smc.fecha_activacion);
            sql.AppendFormat(!String.IsNullOrWhiteSpace(cliente_smc.fecha_modificacion) ? "fecha_modificacion = '{0}' ," : "", cliente_smc.fecha_modificacion);
            sql.AppendFormat(!String.IsNullOrWhiteSpace(cliente_smc.fecha_desactivac) ? "fecha_desactivac = '{0}' ," : "", cliente_smc.fecha_desactivac);
            sql.AppendFormat(!String.IsNullOrWhiteSpace(cliente_smc.tipo_med) ? "tipo_med = '{0}' ," : "", cliente_smc.tipo_med);
            sql.AppendFormat(!String.IsNullOrWhiteSpace(cliente_smc.fact_smc) ? "fact_smc = '{0}' ," : "", cliente_smc.fact_smc);
            sql.AppendFormat(!String.IsNullOrWhiteSpace(cliente_smc.fator) ? "fator = '{0}' ," : "", cliente_smc.fator);
            sql.AppendFormat(!String.IsNullOrWhiteSpace(cliente_smc.modulo_controle) ? "modulo_controle = '{0}' ," : "", cliente_smc.modulo_controle);

            sql.Remove(sql.Length - 1, 1);

            sql.AppendFormat("where numero_cliente = {0} ", cliente_smc.numero_cliente);

            DBProviderInformix informix = ObterProviderInformix();
            return ExecutarSql(sql.ToString(), informix);

        }

        public bool InsereClienteSMC(ClienteSmcDTO cliente_smc, DBProviderInformix informix)
        {

            Type t = typeof(ClienteDTO);
            foreach (PropertyInfo pi in t.GetProperties())
            {
                if (pi.PropertyType.Name.ToUpper().Equals("STRING") && Nullable.Equals(pi.GetValue(cliente_smc), null))

                    pi.SetValue(cliente_smc, "NULL", null);
            }

            string sql = string.Format(@"insert into cliente_smc
		                                           (numero_cliente,
			                                        codigo_cp,
			                                        codigo_cs,
			                                        codigo_ps,
			                                        shunt1,
			                                        shunt2,
			                                        shunt3,
			                                        tipo_ligacao,
			                                        ultima_lectura,
			                                        fecha_activacion,
			                                        fecha_modificacion,
			                                        fecha_desactivac,
			                                        tipo_med,
			                                        fact_smc,
			                                        fator,
				                                    modulo_controle)
		                                           values ({0},'{1}','{2}','{3}','{4}','{5}','{6}','{7}','{8}','{9}','{10}','{11}','{12}','{13}','{14}','{15}')"
                                                    ,cliente_smc.numero_cliente
                                                    ,cliente_smc.codigo_cp
                                                    ,cliente_smc.codigo_cs
                                                    ,cliente_smc.codigo_ps
                                                    ,cliente_smc.shunt1
                                                    ,cliente_smc.shunt2
                                                    ,cliente_smc.shunt3
                                                    ,cliente_smc.tipo_ligacao
                                                    ,cliente_smc.ultima_lectura
                                                    ,cliente_smc.fecha_activacion
                                                    ,cliente_smc.fecha_modificacion
                                                    ,cliente_smc.fecha_desactivac
                                                    ,cliente_smc.tipo_med
                                                    ,cliente_smc.fact_smc
                                                    ,cliente_smc.fator
                                                    ,cliente_smc.modulo_controle);

            sql = sql.Replace("'NULL'", "NULL");
            try
            {
                return ExecutarSql(sql.ToString(), informix);
            }
            catch (Exception ex)
            {
                return false;
            }
        }


    }
}

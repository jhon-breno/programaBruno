using Pangea.Entidades;
using Pangea.Dados.Base;
using Pangea.Entidades.Base;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Pangea.Entidades.DTO;
using Pangea.Util;
using Pangea.Entidades.Enumeracao;
using Entidades.DTO;

namespace Pangea.Dados
{
	public class MedidorDAO : BaseDAO
	{

		private string _empresa;

		public MedidorDAO(Empresa empresa)
			: base(empresa)
		{
			this._empresa = empresa.ToString();
		}

		public DataTable Consultar(Medidor obj)
		{
			if (obj == null)
				return new DataTable();

			if (string.IsNullOrEmpty(this._empresa))
			{
				//TODO: gerar log antes de lançar erro
				throw new ArgumentException("Parâmetro empresa obrigatório para a consulta de religações.");
			}

			#region Prepara a consulta básica

			StringBuilder sql = new StringBuilder("SELECT * FROM corsore WHERE 1=1 ");

			if (!string.IsNullOrEmpty(obj.estado))
				sql.AppendFormat("AND estado IN '{0}' ", obj.estado);

			if (!string.IsNullOrEmpty(obj.numero_cliente))
				sql.AppendFormat("AND numero_cliente = {0} ", obj.numero_cliente);

			#endregion

			return ConsultaSql(sql.ToString());
		}
        public ResultadoInspeccionDTO RetornaInfMudMed(int numero_cliente)
        {
            String sql = String.Format(@"SELECT m.numero_medidor as NumeroMedidor,
	                                           m.numero_cliente as NumeroCliente,
	                                           m.marca_medidor as Marca_medidor,
	                                           m.modelo as Modelo,
	                                           m.correlativo as Correlativo,
	                                           m.constante as Constante,
	                                           m.ultima_lect_activa AS UltimaLeituraAtiva1,
	                                           m.ultima_lect_act_hp AS UltimaLeituraAtiva3,
	                                           m.numero_med_ant as NumeroMedAnt,
	                                           m.marca_med_ant as MarcaMedAnt
                                        FROM medid m
                                        WHERE m.numero_cliente={0}", numero_cliente);

            var dt = ConsultaSql(sql.ToString());

            return DataHelper.ConvertDataTableToEntity<ResultadoInspeccionDTO>(dt);
        }

        public DataTable RetornaMarcaModeloMedidor(string numero_cliente)
        {
            string sql = string.Format(@"select t.descripcion as fabrica, m.modelo from 
                                        medid m, tabla t
                                        where nomtabla = 'MAMED'
                                        and m.numero_cliente = {0}
                                        and t.codigo = m.marca_medidor
                                        and estado = 'I'
                                        and t.sucursal = '0000'", numero_cliente);

            return ConsultaSql(sql); ;
        }

		public ResultadoInspeccionDTO RetornaInfMudMedGA(int numero_cliente)
		{
			String sql = String.Format(@"SELECT m.numero_medidor as NumeroMedidor,
											   m.numero_cliente as NumeroCliente,
											   m.marca_medidor as Marca_medidor,
											   m.modelo as Modelo,
											   m.correlativo as Correlativo,
											   m.constante as Constante,
											   m.ultima_lect_activa AS UltimaLeituraAtiva1,
											   '0' AS UltimaLeituraAtiva3,
											   '0' as NumeroMedAnt,
											   '0' as MarcaMedAnt
										FROM medid m
										WHERE m.numero_cliente={0}", numero_cliente);

			var dt = ConsultaSql(sql.ToString());

			return DataHelper.ConvertDataTableToEntity<ResultadoInspeccionDTO>(dt);
		}

		public ResultadoInspeccionDTO RetornaInfMudMedGB(int numero_cliente)
		{
			String sql = String.Format(@"SELECT m.numero_medidor as NumeroMedidor,
											   m.numero_cliente as NumeroCliente,
											   m.marca_medidor as Marca_medidor,
											   m.modelo as Modelo,
											   m.correlativo as Correlativo,
											   m.constante as Constante,
											   m.ultima_lect_activa AS UltimaLeituraAtiva1,
											   m.ultima_lect_act_hp AS UltimaLeituraAtiva3,
											   m.numero_med_ant as NumeroMedAnt,
											   m.marca_med_ant as MarcaMedAnt
										FROM medid m
										WHERE m.numero_cliente={0}", numero_cliente);

			var dt = ConsultaSql(sql.ToString());

			return DataHelper.ConvertDataTableToEntity<ResultadoInspeccionDTO>(dt);
		}

		public int RetornaNumeroMedidor(int numero_cliente)
		{
			int retorno = 0;

			String sql = String.Format(@"select numero_medidor from medid where numero_cliente = {0}
										 and estado='I'",numero_cliente);


			var dt = ConsultaSql(sql.ToString());

			if (dt.Rows.Count > 0)
				retorno = Convert.ToInt32(dt.Rows[0]["numero_medidor"]);
			else return retorno;

			return retorno;
		}

		public int RetornaNumeroMedidorAnterior(int numero_cliente)
		{
			int retorno = 0;

			String sql = String.Format(@"select numero_medidor from medid where numero_cliente = {0}
										 and estado='R'", numero_cliente);


			var dt = ConsultaSql(sql.ToString());

			if (dt.Rows.Count > 0)
				retorno = Convert.ToInt32(dt.Rows[0]["numero_medidor"]);
			else return retorno;

			return retorno;
		}
		
		public List<MedidDTO> RetornaMedidor(ContratoDTO contrato)
		{
			string sql = string.Format(@"select numero_medidor,
												 marca_medidor,
												 constante,
												 constante_react_hp,
												 ultima_lect_activa,
												 ultima_lect_react,
												 ultima_lect_act_hp                                                                                             
											from medid
										   where numero_cliente = {0}
											 and estado in ('I','Z')",contrato.numero_cliente);

			var dt = ConsultaSql(sql.ToString());

			if (dt.Rows.Count > 0)
				return DataHelper.ConvertDataTableToList<MedidDTO>(dt);
			else
				return null;


		}

		public bool AtualizaMedid(ContratoDTO contrato,  DBProviderInformix informix)
		{
			string sql = string.Format(@"update medid set numero_cliente = {0}
											where numero_cliente = {1}
											  and estado in ('I','Z');",contrato.numero_cliente_novo,contrato.numero_cliente);
			
			return ExecutarSql(sql.ToString(), informix);
		}

		public int RetornaContMedidores(ClienteDTO cliente)
        {
            int retorno = 0;

            string sql = string.Format(@"select count(*) as total 
                                           from medid
                                          where numero_cliente = {0}
                                            and estado = 'I'", cliente.numero_cliente);

            var dt = ConsultaSql(sql.ToString());
            
            if(dt.Rows.Count >= 0)
            {
                retorno = Convert.ToInt32(dt.Rows[0]["total"]);  
            }

            return retorno;
        }
        public MedidDTO RetornaInforMedidor(int numero_cliente)
        {
            String sql = String.Format(@"SELECT 
                                               m.numero_medidor AS numero_medidor,
											   m.marca_medidor AS marca_medidor,
                                               m.propiedad_medidor AS propriedade_medidor,
											   m.modelo AS modelo_medidor
										FROM medid m
										WHERE m.numero_cliente={0}", numero_cliente);

            var dt = ConsultaSql(sql.ToString());

            if (dt.Rows.Count > 0)
                return DataHelper.ConvertDataTableToEntity<MedidDTO>(dt);
            else
                return null;
        }
		}
}

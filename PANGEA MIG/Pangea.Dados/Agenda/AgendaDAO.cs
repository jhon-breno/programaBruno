using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using Pangea.Dados.Base;
using Pangea.Entidades.Enumeracao;

namespace Pangea.Dados.Agenda
{
     public class AgendaDAO : BaseDAO
    {
        public AgendaDAO(Empresa empresa)
            : base(empresa)
        {
        }
  

        public IList<Entidades.Agenda> ObterAgenda(string sucursal, string dataProcesso, int localidade, int setor, int zona, bool agendaFutura)
        {
            var agenda = new List<string>();
            var sqlFeriados = new StringBuilder();
            sqlFeriados.AppendFormat(@" Select A.localidade,A.zona,A.Fecha_Lectura,A.Fecha_Factura,
                                        A.fecha_reparto,V.Fecha_Vcto,V.codigo_venc,V.Fecha_Vcto2 ");
            if (agendaFutura)
            {
                sqlFeriados.AppendFormat(@"from Agenda1 A, Venage1 V 
                                        Where A.Sucursal ='{0}'  And A.Fecha_Proceso = '{1}'"
                                               , sucursal, dataProcesso);
            }
            else {
                sqlFeriados.AppendFormat(@"from Agenda A, Venage V 
                                        Where A.Sucursal ='{0}'  And A.Fecha_Proceso = '{1}'"
                                                  , sucursal, dataProcesso);
            }
            if (localidade != 0) {
                sqlFeriados.AppendFormat("And A.Localidade = {0} ", localidade);
            }
            if (zona != 0)
            {
                sqlFeriados.AppendFormat("And A.zona = {0} ", zona);
            }
            sqlFeriados.AppendFormat(@"And A.sector = {0} --and a.municipio = v.municipio 
                                       And A.Fecha_proceso = V.Fecha_proceso
                                       And A.Sucursal = V.Sucursal
                                       And A.Sector = V.Sector
                                       And A.Zona = V.Zona
                                       And A.Localidade = V.Localidade
                                       And A.Tipo_Reparto = V.Tipo_Reparto
                                       And V.tipo_vcto = 03
                                       Order By A.Sector,A.localidade,A.Zona ", setor);


            var dt = ConsultaSql(sqlFeriados.ToString());


            IList<Entidades.Agenda> agendas = dtToListObject<Entidades.Agenda>(dt);

            return agendas;
        }

        public IList<TEntidade> dtToListObject<TEntidade>(System.Data.DataTable dt)
        {
            IList<Entidades.Agenda> resultados = new List<Entidades.Agenda>();

            foreach (DataRow item in dt.Rows)
            {
                Entidades.Agenda temp = new Entidades.Agenda();
                temp.Localidade = TratarInt(dt, item, "localidade", 0);
                temp.Zona = TratarInt(dt, item, "zona", 0);
                temp.DataLeitura = TratarString(dt, item, "Fecha_Lectura");
                temp.DataFaturamento = TratarString(dt, item, "Fecha_Factura");
                temp.DataReparto = TratarString(dt, item, "fecha_reparto");
                temp.DataVencimento = TratarString(dt, item, "Fecha_Vcto");
                temp.CodigoVencimento = TratarInt(dt, item, "codigo_venc",-1);
                temp.DataVencimento2 = TratarString(dt, item, "Fecha_Vcto2");
        

                resultados.Add(temp);
            }

            return resultados as IList<TEntidade>;
        }
        public DataTable retornaRotas(string lote, string zona, string localidade )
        {
            StringBuilder sql = new StringBuilder();
            sql.Append(String.Format(@"SELECT sucursal, sector, zona, localidade, municipio,"));
            if (Convert.ToInt32(lote) >= 22)
            {
                sql.Append("'A'");
            }
            else
            {
                sql.Append(" ind_zona ");
            }
            sql.Append(String.Format(@" AS grupotensao FROM susec WHERE sucursal <>'0000'
                                        and sector = {0} and zona = {1} and localidade = {2}  ", lote, zona, localidade));

            var dt = ConsultaSql(sql.ToString());
            return dt;
        }
        public DataTable retornaRotasPorCliente(string numero_cliente)
        {
            StringBuilder sql = new StringBuilder();
            sql.Append(String.Format(@"SELECT c.sucursal, c.sector, c.zona, c.localidade, c.municipio,s.ind_zona"));
             
            sql.Append(String.Format(@" AS grupotensao FROM cliente c,susec s WHERE numero_cliente = {0} and c.sucursal <>'0000'
                                        and c.sector = s.sector and c.zona = s.zona and c.localidade = s.localidade  ", numero_cliente));

            var dt = ConsultaSql(sql.ToString());
            return dt;
        }
        public DataTable retornaRotas(string lote, string zona = null, string localidade = null, string municipio = null)
        {
            StringBuilder sql = new StringBuilder();
           sql.Append("select zona,localidade,municipio,");
            if (Convert.ToInt32(lote) >= 22)
            {
                sql.Append("'A' ");
            }
            else
            {
                sql.Append(" ind_zona ");
            }
            sql.Append(@"  AS grupotensao from susec where sector = " + lote);
            if(String.IsNullOrEmpty(zona) || zona.Equals(0)) 
            sql.Append("and zona = " + zona);
            if(String.IsNullOrEmpty(localidade) || localidade.Equals(0)) 
            sql.Append("and localidade = " + localidade);
            if(String.IsNullOrEmpty(municipio) || municipio.Equals(0))
            sql.Append("and municipio = " + municipio);
            

            var dt = ConsultaSql(sql.ToString());
            return dt;
        }

        public string retornaSiglaMunicipio(string cod_municipio)
        {
            String sql = @"select sigla from municipio_sap where cod_municipio = " + cod_municipio;

            var dt = ConsultaSql(sql);

            return dt.Rows[0][0].ToString();
        }

        public DataTable retornaAgenda()
        {
            String sql = @"Select A.localidade,A.Sucursal,A.zona,A.Fecha_Lectura,A.Fecha_Factura,A.Fecha_Proceso,
                                       A.fecha_reparto,V.Fecha_Vcto,V.codigo_venc,
	                                   V.Fecha_Vcto2,A.sector
                                from Agenda A, Venage V 
                                Where A.Sucursal ='0000'  
                                And A.Fecha_Proceso = '01/01/2015'
                                And A.Fecha_proceso = V.Fecha_proceso
                                And A.Sucursal = V.Sucursal
                                And A.Sector = V.Sector
                                And A.Zona = V.Zona
                                And A.Localidade = V.Localidade
                                And A.Tipo_Reparto = V.Tipo_Reparto
                                    And V.tipo_vcto = 03
                                And A.Localidade = 21025
                                And A.zona = 43
                                And A.sector = 1";

            var dt = ConsultaSql(sql);

            return dt;
        }
        public DataTable retornaIntervalo(string sector, string localidade, string zona)
        {
            String sql = String.Format(@"SELECT 
                                             MAX(A0.fecha_lectura) as dataUltimaLeitura,
                                             MIN(A1.fecha_lectura) as dataProximaLeitura
                                        FROM agenda A0
                                        JOIN agenda1 A1
                                        ON A0.sector = A1.sector
                                        WHERE A0.sector = {0}
                                        AND A0.localidade = {1}
                                        AND A0.zona = {2}
                                        AND A1.FECHA_LECTURA > (SELECT max(fecha_lectura) 
                                        FROM agenda WHERE sector = {0} and localidade={1} and zona =  {2})", sector, localidade, zona);

            var dt = ConsultaSql(sql);

            return dt;
        }
    }
}

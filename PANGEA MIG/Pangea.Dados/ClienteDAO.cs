using Entidades.DTO;
using Pangea.Dados.Base;
using Pangea.Entidades;
using Pangea.Entidades.DTO;
using Pangea.Entidades.Enumeracao;
using Pangea.Util;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Web.Script.Serialization;

namespace Pangea.Dados
{
    public class ClienteDAO : BaseDAO
    {

        public ClienteDAO(Empresa empresa)
            : base(empresa)
        {
        }		
		

        public DataTable Consultar(string sql)
        {
            return ConsultaSql(sql, true);
        }

        public ClienteDAO(Empresa empresa, string database = "")
            : base(empresa, database)
        {
            this.empresa = empresa;
        }

        public DataTable RetornaRotaCliente(string numeroCliente)
        {
            StringBuilder sql = new StringBuilder();

            sql.Append(@" SELECT
                                c.sector,
                                c.localidade,
                                c.zona,
                                c.correlativo_ruta,
                                c.dv_ruta_lectura,
                                c.sucursal,
                                c.municipio,
                                c.quadra,
                                    CASE WHEN
	                                    c.estado_cliente = 8 THEN 'A'
                                    ELSE
	                                    s.ind_zona
                                    END AS grupotensao
                        FROM cliente c
                        JOIN susec s
                        ON s.sector = c.sector
                        AND s.localidade = c.localidade
                        AND s.zona = c.zona
                        WHERE numero_cliente = " + numeroCliente);

            var dt = ConsultaSql(sql.ToString());

            //return new RetornoOrdemDTO();
            return dt;
        }

        /// <summary>
        /// Retorna numero de cliente por ordem servico.
        /// </summary>
        /// <param name="numeroOrdem"></param>
        /// <returns></returns>
        public int retornaNumeroCliente(string numeroOrdem)
        {
            int numero_cliente = 0;

            String sql = String.Format(@"select numero_cliente from ordem_servico 
					   where numero_ordem ='{0}'", numeroOrdem);

            var dt = ConsultaSql(sql.ToString());

            if (dt.Rows.Count > 0)
            {
                numero_cliente = (int)dt.Rows[0]["numero_cliente"];
            }

            return numero_cliente;

        }

        public bool ClienteTelemedido (string numero_cliente)
        {
            bool retorno = false;
            
            string sql = string.Format(@"select count(*) from cliente_smc where numero_cliente = {0} and fecha_desactivac is null",numero_cliente);
            if (ConsultaSql(sql).Rows.Count > 0)
                retorno = true;

            return retorno;
        }

        public List<AcquisizioneLavoroLetturaDTO> listLeitura(int numero_cliente, int numero_medidor, int numero_med_ant)
        {
            StringBuilder sql = new StringBuilder();

            sql.Append(@"select ");
            sql.Append("  case  ");
            sql.Append("    when( ");
            sql.Append("        S.numero_cliente IS NOT NULL");
            sql.Append("   AND    m.tipo_medidor in ('05')) THEN 'CALCULATED' ");
            sql.Append("   ELSE 'VISUAL'  ");
            sql.Append("   END AS sourcetype, ");

            //Letture
            sql.Append("  '0' as anomalyCode,");
            sql.Append("   case when cla.descripcion is null then '0' else cla.descripcion end AS anomalyReasonCode, ");
            sql.Append("  '0' as enAttImmF1, ");
            sql.Append("  '0' as enAttImmF2, ");
            sql.Append("  '0' as enAttImmF3,");
            sql.Append("  '0' as enAttImmTot,");
            sql.Append("  case when ((select count(*) from tabla where nomtabla = 'CRIEST' and codigo = 0 ) > 0) then ' '  ");
            sql.Append("  else '0' end AS enAttPrelF1 ,  "); //EnergiaAttivaPrelevata
            sql.Append("  '0' as enAttPrelF2,"); //EnergiaAttivaPrelevata
            sql.Append(" case when c.estado_suministro = 1  then to_char(Round(nvl(m.ultima_lect_act_hp,0))) else   ");
            sql.Append(" to_char(Round(nvl(m.ultima_lect_activa,0))) end AS enAttPrelF3, "); //EnergiaAttivaPrelevata 
            sql.Append("  '0' as enAttPrelTot,");
            sql.Append("  case when ((select count(*) from tabla where nomtabla = 'CRIEST' and codigo = 0 ) > 0) then ' '  ");
            sql.Append("  else '0' end AS enReatF1,  "); //Energia Reattiva 
            sql.Append("  case when ((select count(*) from tabla where nomtabla = 'CRIEST' and codigo = 0 ) > 0) then ' '  ");
            sql.Append("  else '0' end AS enReatF2, "); //Energia Reattiva 
            sql.Append("  case when ((select count(*) from tabla where nomtabla = 'CRIEST' and codigo = 0 ) > 0) then ' '  ");
            sql.Append("  else '0' end AS enReatF3, "); //Energia Reattiva 
            sql.Append("  '0' as enReatQ1Tot,");
            sql.Append("  '0' as enReatQ2F1,");
            sql.Append("  '0' as enReatQ2F2,");
            sql.Append("  '0' as enReatQ2F3,");
            sql.Append("  '0' as enReatQ2Tot,");
            sql.Append("  '0' as enReatQ3F1,"); // Picco di Energia Reattiva
            sql.Append("  '0' as enReatQ3F2,"); // Picco di Energia Reattiva
            sql.Append("  '0' as enReatQ3F3,"); // Picco di Energia Reattiva
            sql.Append("  '0' as enReatQ3Tot,");
            sql.Append("  '0' as enReatQ4Tot,");
            sql.Append("  case when  ( ( c.estado_suministro = 1 and m.tipo_medidor in ('04','05') and m.marca_medidor not like 'COM%') or (c.estado_suministro=1 and m.tipo_medidor='02' and m.marca_medidor='DIS')) then ");
            sql.Append(" ' '  ");
            sql.Append("  else ( ");
            sql.Append("  case when m.marca_medidor is null or m.marca_medidor = '' then ");
            sql.Append(" 'MED' ");
            sql.Append(" else ");
            sql.Append("  rpad(trim(m.marca_medidor),3,0) end  ");
            sql.Append("  ) end AS manufacturer,  ");
            //sql.Append("   case ");
            //sql.Append("        when ( ");
            //sql.Append("  S.numero_cliente IS NOT NULL ");
            //sql.Append("   AND   m.tipo_medidor in ('05') and lec.membro <> 4) THEN 'SYNTEGRA GB'  ");
            //sql.Append("  WHEN col.codigo_instalador IS NULL THEN 'OPER NULL'   ");
            //sql.Append("  ELSE col.codigo_instalador  ");
            //sql.Append("  END AS matrLetturista,  ");
            sql.Append("  '0'  AS matrLetturista,  ");
            sql.Append(" case when  ( ( c.estado_suministro = 1 and m.tipo_medidor in ('04','05') and m.marca_medidor not like 'COM%') or (c.estado_suministro=1 and m.tipo_medidor='02' and m.marca_medidor='DIS')) then ");
            sql.Append("  ' '  ");
            sql.Append(" else ");
            sql.Append(" ( ");
            sql.Append("  case when M.modelo is null or m.modelo = '' then  ");
            sql.Append("  'NUL' ");
            sql.Append(" else  ");
            sql.Append(" rPAD(trim(m.modelo),3,0) end  ");
            sql.Append("  ) end  AS model,  ");
            sql.Append("   round(nvl(c.consumo_30_dias,0),0) AS Note, ");
            sql.Append("  '0' as piccoEnReatF1,");
            sql.Append("  '0' as piccoEnReatF2,");
            sql.Append("  '0' as piccoEnReatF3,");
            if (empresa.Equals(Empresa.RJ))
                sql.Append("  'BR101E'||LPAD(c.numero_cliente ,8,0) as podId,  ");
            else
                sql.Append("  'BR102E'||LPAD(c.numero_cliente ,8,0) as podId,  ");
            //sql.Append("   SUBSTR(c.fecha_ultima_lect,4,2)||'/' ||SUBSTR(c.fecha_ultima_lect,1,2)|| '/' ||SUBSTR(c.fecha_ultima_lect,7,4) AS readingdateSistema,  ");
            //sql.Append("   SUBSTR(c.fecha_ultima_lect,4,2)||'/' ||SUBSTR(c.fecha_ultima_lect,1,2)|| '/' ||SUBSTR(c.fecha_ultima_lect,7,4) AS readingdateContatore, ");
            sql.Append("   SUBSTR(c.fecha_ultima_lect,7,4) || SUBSTR(c.fecha_ultima_lect,1,2) || SUBSTR(c.fecha_ultima_lect,4,2) || '000000' AS readingdateSistema,  ");
            sql.Append("   SUBSTR(c.fecha_ultima_lect,7,4) || SUBSTR(c.fecha_ultima_lect,1,2) || SUBSTR(c.fecha_ultima_lect,4,2) || '000000' AS readingdateContatore, ");
            sql.Append("  case when ((select count(*) from tabla where nomtabla = 'CRIEST' and codigo = 0 ) > 0) then ' '  ");
            sql.Append("  else '0' end AS potAttImmF1, "); // Potenza Attiva Prelevata
            sql.Append("  case when ((select count(*) from tabla where nomtabla = 'CRIEST' and codigo = 0 ) > 0) then ' '  ");
            sql.Append("  else '0' end AS potAttImmF2, "); // Potenza Attiva Prelevata
            sql.Append("  case when ((select count(*) from tabla where nomtabla = 'CRIEST' and codigo = 0 ) > 0) then ' '  ");
            sql.Append("  else '0' end AS potAttImmF3, "); // Potenza Attiva Prelevata
            sql.Append("  '0' as potAttPrelF1,");
            sql.Append("  '0' as potAttPrelF2,");
            sql.Append("  '0' as potAtt_prelF3,");
            //serialnumber
            sql.Append("  case when  ( ( c.estado_suministro = 1 and m.tipo_medidor in ('04','05') and m.marca_medidor not like 'COM%') or (c.estado_suministro=1 and m.tipo_medidor='02' and m.marca_medidor='DIS')) then  ");
            sql.Append("  case when m.numero_medidor = 0 or m.numero_medidor is null or m.numero_medidor = '' then  ");
            sql.Append("  case when  ( ( c.estado_suministro = 1 and m.tipo_medidor in ('04','05') and m.marca_medidor not like 'COM%') or (c.estado_suministro=1 and m.tipo_medidor='02' and m.marca_medidor='DIS')) then  ");
            sql.Append("  '003' ");
            sql.Append("  else ");
            sql.Append("  Case when (S.numero_cliente is not null and m.tipo_medidor in ('05')) then ");
            sql.Append("  '001' ");
            sql.Append("  else ");
            sql.Append("  '002' end end || 'MEDNUL' ");
            sql.Append("  else ");
            sql.Append("  ( ");
            sql.Append("  case when  ( ( c.estado_suministro = 1 and m.tipo_medidor in ('04','05') and m.marca_medidor not like 'COM%') or (c.estado_suministro=1 and m.tipo_medidor='02' and m.marca_medidor='DIS')) then  ");
            sql.Append("  '003' ");
            sql.Append("  else ");
            sql.Append("  Case when (S.numero_cliente is not null and m.tipo_medidor in ('05')) then  ");
            sql.Append("  '001' ");
            sql.Append("  else ");
            sql.Append("  '002' end end  ");
            sql.Append("  || case when m.marca_medidor is null or m.marca_medidor = '' then   ");
            sql.Append("  'MED' ");
            sql.Append("  else ");
            sql.Append("  rpad(trim(m.marca_medidor),3,0) end  ");
            sql.Append("  || case when M.modelo is null or m.modelo = '' then  ");
            sql.Append("  'NUL'  ");
            sql.Append("  else  ");
            sql.Append("  rPAD(trim(m.modelo),3,0) end  ");
            sql.Append("  ) end ");
            sql.Append("  || ");
            if (empresa.Equals(Empresa.RJ))
                sql.Append(" LPAD( m.numero_medidor,8,0) ");
            else//Coelce
                sql.Append(" case when ((select count(*) from medid where medid.numero_medidor = m.numero_medidor and estado = 'I') > 1) then 'D' || lpad(to_char(m.numero_cliente),7,0) else lpad(to_char(m.numero_medidor),8,0) end  ");
            sql.Append("  else  ");
            sql.Append("  (  ");
            sql.Append("   case when m.numero_medidor = 0 or m.numero_medidor is null or m.numero_medidor = ''  then  ");
            sql.Append("    'N'||LPAD(c.numero_cliente,7,0)  ");
            sql.Append("   else ");
            if (empresa.Equals(Empresa.RJ))
                sql.Append("    LPAD( m.numero_medidor,8,0) end ");
            else
                sql.Append("  case when ((select count(*) from medid where medid.numero_medidor = m.numero_medidor and estado = 'I') > 1) then 'D' || lpad(to_char(m.numero_cliente),7,0) else lpad(to_char(m.numero_medidor),8,0) end end   ");
            sql.Append("  ) end as serialNumber,  ");
            sql.Append("  'T' as sourceCode ");

            sql.Append(" FROM   cliente c,");
            //sql.Append("        tabla est,");
            sql.Append("        outer tabla cla,");
            //sql.Append("        tecni tec,");
            sql.Append("        outer medid m,");
            //sql.Append("        OUTER(coletor_assoc col, inscor ins, cotrat con),");
            sql.Append("        OUTER CLIENTE_SMC S");
            // sql.Append(" WHERE ");            
            //sql.Append("    AND est.sucursal = '0000'   ");
            //sql.Append("    AND est.nomtabla = 'LECEST'  ");                        
            //sql.Append("    AND ins.area              = col.area    ");
            //sql.Append("    AND ins.codigo_instalador = col.codigo_instalador");
            //sql.Append("    AND con.contratista       = ins.contratista  ");
            sql.Append("    Where c.numero_cliente = m.numero_cliente  ");
            sql.Append("    And m.estado = 'I'  ");
            sql.Append("    AND cla.sucursal = '0000'    ");
            sql.Append("    AND cla.nomtabla = 'CLALEC'   ");
            sql.Append("    AND cla.codigo =  c.cod_consumo_incl  ");
            sql.Append("    AND cla.codigo = c.clave_lectura_ant ");
            sql.Append("    AND c.numero_cliente =   S.numero_cliente  ");
            sql.Append("    And s.fecha_desactivac is null  ");
            sql.Append("    AND cla.fecha_desactivac is null    ");
            //sql.Append("    AND tec.numero_cliente = c.numero_cliente  ");
            sql.Append(String.Format("    and c.numero_cliente ={0}", numero_cliente));
            if (numero_medidor != 0 && numero_med_ant != 0)
            {
                sql.Append(String.Format("    and m.numero_medidor in ({0},{0})", numero_medidor, numero_med_ant));
            }
            if (numero_medidor != 0 && numero_med_ant == 0)
            {
                sql.Append(String.Format("    and m.numero_medidor = {0}", numero_medidor));
            }

            var dt = ConsultaSql(sql.ToString());
            if (dt.Rows.Count > 0)
            {
                List<AcquisizioneLavoroLetturaDTO> lista = new List<AcquisizioneLavoroLetturaDTO>();
                lista = DataHelper.ConvertDataTableToList<AcquisizioneLavoroLetturaDTO>(dt);

                return lista;
            }
            else
                return null;
        }

        public AnagraficaDTO retornaClienteMedidor(int numeroCliente, string numero_ordem, int numero_medidor)
        {
            StringBuilder sql = new StringBuilder();

            sql.Append(@"select first 1 ");
            if (base.empresa.Equals(Empresa.RJ))
            {
                sql.Append(" 'AB'||LPAD(age.sector,2,0) AS codiceZona ");
                sql.Append(" ,SUBSTR(age.localidade,2,3) AS cft, ");
                sql.Append(" LPAD(age.sector || age.localidade || age.zona ,13,0) as codicePresa,");
                sql.Append(" LPAD(c.numero_cliente ,9,0) AS eneltel, ");
                sql.Append(" Case when S.numero_cliente is null then 'N'  else 'S' end as flagTG, ");
                sql.Append(" age.sector as codiceGruppo,");
                sql.Append(" age.sector as lotto,");
            }
            else
            {
                sql.Append(" CASE WHEN age.sector = 39 THEN 'CB96' ELSE 'CB'||LPAD(age.sector,2,0) END AS codiceZona ");
                sql.Append(" ,lpad(round(left(c.municipio,1) * 3 + right(c.municipio,3) + left(c.municipio,2) + left(c.municipio,1),0), 3, '0') AS CFT,");
                sql.Append(" CASE WHEN age.sector = 39 THEN LPAD('96' || age.localidade || age.zona ,13,0) ELSE LPAD(age.sector || age.localidade || age.zona ,13,0) END as codicePresa,");
                sql.Append(" 'C'||LPAD(c.numero_cliente ,8,0) AS eneltel,");
                sql.Append(" Case when S.numero_cliente is null then 'N'  else 'S' end as flagTG,");
                sql.Append(" CASE WHEN age.sector = 39 THEN 96 ELSE age.sector END as codiceGruppo,");
                sql.Append(" age.sector as lotto,");
            }
            sql.Append(" Case WHEN c.classe ||c.subclasse = '0806' THEN 'XX' ");
            sql.Append(" when c.classe = '06' THEN 'IP' ");
            sql.Append(" when S.numero_cliente is not null and s.fact_smc = 'S' then 'S' || tec.acometida_retirada ");
            sql.Append(" when c.consumo_30_dias > 0  then 'FX' ");
            sql.Append(" else tec.acometida_retirada end as tariffario,");
            sql.Append(" m.constante*5 as rapp1, ");///
            sql.Append(" '5' as rapp2,");
            sql.Append(" Case when c.estado_suministro = 0 then  'A' else 'C' end as statoPresa,");
            sql.Append(" Case when c.estado_suministro = 0 then  'A' else 'C' end as sottostatoPresa,");
            sql.Append(" case when m.numero_medidor = 0 or m.numero_medidor is null or m.numero_medidor = '' then ");
            sql.Append(" case when  ( ( c.estado_suministro = 1 and m.tipo_medidor in ('04','05') and m.marca_medidor not like 'COM%') or (c.estado_suministro=1 and m.tipo_medidor='02' and m.marca_medidor='DIS')) then ");
            sql.Append(" '003' ");
            sql.Append(" else ");
            sql.Append(" case when (S.numero_cliente is not null and m.tipo_medidor in ('05') and lec.membro <> 4) then  ");
            sql.Append(" '001' ");
            sql.Append(" else ");
            sql.Append("    '002' end end || 'MEDNUL'  ");
            sql.Append(" else ");
            sql.Append(" ( ");
            sql.Append(" case when  ( ( c.estado_suministro = 1 and m.tipo_medidor in ('04','05') and m.marca_medidor not like 'COM%') or (c.estado_suministro=1 and m.tipo_medidor='02' and m.marca_medidor='DIS')) then ");
            sql.Append(" '003' ");
            sql.Append(" else ");
            sql.Append(" Case when (S.numero_cliente is not null and m.tipo_medidor in ('05') and lec.membro <> 4) then  ");
            sql.Append(" '001' ");
            sql.Append(" else ");
            sql.Append(" '002' end end ");
            sql.Append(" || case when m.marca_medidor is null or m.marca_medidor = '' then ");
            sql.Append(" 'MED' ");
            sql.Append(" else ");
            sql.Append(" rpad(trim(m.marca_medidor),3,0) end  ");
            sql.Append(" || case when M.modelo is null or m.modelo = '' then ");
            sql.Append(" 'NUL' ");
            sql.Append(" else ");
            sql.Append(" rPAD(trim(m.modelo),3,0) end  ");
            sql.Append(" ) end as codiceMis, ");
            if (base.empresa.Equals(Empresa.RJ))
                sql.Append(" case when m.numero_medidor = 0 or m.numero_medidor is null or m.numero_medidor = ''  then 'N'||LPAD(c.numero_cliente,7,0) else LPAD( m.numero_medidor,8,0) end as matricolaMis,");
            else
                sql.Append(" case when ((select count(*) from medid where medid.numero_medidor = m.numero_medidor and estado = 'I') > 1) then 'D' || lpad(to_char(m.numero_cliente),7,0) else case when m.numero_medidor = 0 or m.numero_medidor is null or m.numero_medidor = '' then 'N'||LPAD(c.numero_cliente,7,0) else LPAD( m.numero_medidor,8,0) end end as matricolaMis,");
            sql.Append(" c.nombre as nominativo,");
            sql.Append(" ' ' as prefisso,");
            sql.Append(" replace(replace(SUBSTR(c.direccion,0,60),chr(10),''),chr(13),'') as via, ");
            sql.Append(" ' ' as numCiv,");
            sql.Append(" ' ' as cdClMerceologica ");
            sql.Append(" ,'100' as potDisp ");
            sql.Append(" ,'0' as somPot,");
            sql.Append(" m.constante as costE ");
            sql.Append(" ,'1' as costP,");

            sql.Append(" case when (m.fecha_prim_insta > c.fecha_ultima_lect) or (m.fecha_ult_insta > c.fecha_ultima_lect) or (m.numero_medidor = 0) or (m.numero_medidor is null) or (m.numero_medidor = '') ");
            sql.Append("    then ");
            sql.Append("        SUBSTR(c.fecha_ultima_lect,7,4)||SUBSTR(c.fecha_ultima_lect,1,2)||SUBSTR(c.fecha_ultima_lect,4,2) || '000000' ");
            sql.Append(" else ");
            sql.Append(" ( ");
            sql.Append("    Case when m.fecha_prim_insta is not null ");
            sql.Append("    then ");
            sql.Append("        case when ((select count(*) from medid where medid.numero_medidor = m.numero_medidor and estado = 'I') = 1) then ");
            sql.Append("            SUBSTR(m.fecha_prim_insta,7,4) || SUBSTR(m.fecha_prim_insta,1,2) || SUBSTR(m.fecha_prim_insta,4,2) || '000000' ");
            sql.Append("        when m.fecha_prim_insta is null and m.fecha_ult_insta is not null ");
            sql.Append("            then ");
            sql.Append("                SUBSTR(m.fecha_ult_insta,7,4) || SUBSTR(m.fecha_ult_insta,1,2) || SUBSTR(m.fecha_ult_insta,4,2) || '000000'  ");
            sql.Append("        When (select max(m2.fecha_prim_insta)  from clientes:medid m2 where m2.numero_cliente = c.numero_cliente ) is not null");
            sql.Append("            then ");
            sql.Append("                (select SUBSTR(max(m2.fecha_prim_insta),7,4)  from clientes:medid m2 where m2.numero_cliente = c.numero_cliente ) || ");
            sql.Append("                (select SUBSTR(max(m2.fecha_prim_insta),1,2)  from clientes:medid m2 where m2.numero_cliente = c.numero_cliente ) || ");
            sql.Append("                (select SUBSTR(max(m2.fecha_prim_insta),4,2)  from clientes:medid m2 where m2.numero_cliente = c.numero_cliente ) || '000000' ");
            sql.Append("        When  (select max(m2.fecha_orden_xn)  from clientes:medid m2 where m2.numero_cliente = c.numero_cliente )  is not null ");
            sql.Append("            then ");
            sql.Append("                (select SUBSTR(max(m2.fecha_orden_xn),7,4)  from clientes:medid m2 where m2.numero_cliente = c.numero_cliente ) || ");
            sql.Append("                (select SUBSTR(max(m2.fecha_orden_xn),1,2)  from clientes:medid m2 where m2.numero_cliente = c.numero_cliente ) || ");
            sql.Append("                (select SUBSTR(max(m2.fecha_orden_xn),4,2)  from clientes:medid m2 where m2.numero_cliente = c.numero_cliente ) || '000000' ");
            sql.Append(" else ");
            sql.Append("    (SUBSTR(c.fecha_ultima_lect,7,4) || SUBSTR(c.fecha_ultima_lect,1,2) || SUBSTR(c.fecha_ultima_lect,4,2) || '000000') end ");
            sql.Append(" else ");
            sql.Append(" '20000101' end) ");
            sql.Append(" end as dtContratto, ");

            sql.Append(" case when (m.fecha_prim_insta > c.fecha_ultima_lect) or (m.fecha_ult_insta > c.fecha_ultima_lect) or (m.numero_medidor = 0) or (m.numero_medidor is null) or (m.numero_medidor = '') ");
            sql.Append(" then ");
            sql.Append("    SUBSTR(c.fecha_ultima_lect,7,4)||SUBSTR(c.fecha_ultima_lect,1,2)||SUBSTR(c.fecha_ultima_lect,4,2) || '000000' ");
            sql.Append(" else ");
            sql.Append(" (Case when m.fecha_prim_insta is not null ");
            sql.Append("    then ");
            sql.Append("    case when ((select count(*) from medid where medid.numero_medidor = m.numero_medidor and estado = 'I') = 1) then ");
            sql.Append("        SUBSTR(m.fecha_prim_insta,7,4) || SUBSTR(m.fecha_prim_insta,1,2) || SUBSTR(m.fecha_prim_insta,4,2) || '000000' ");
            sql.Append(" when m.fecha_prim_insta is null and  m.fecha_ult_insta is not null ");
            sql.Append("    then ");
            sql.Append("        SUBSTR(m.fecha_ult_insta,7,4) || SUBSTR(m.fecha_ult_insta,1,2) || SUBSTR(m.fecha_ult_insta,4,2) || '000000' ");
            sql.Append(" When (select max(m2.fecha_prim_insta)  from clientes:medid m2 where m2.numero_cliente = c.numero_cliente )  is not null");
            sql.Append("    then ");
            sql.Append(" (select SUBSTR(max(m2.fecha_prim_insta),7,4)  from clientes:medid m2 where m2.numero_cliente = c.numero_cliente ) || ");
            sql.Append(" (select SUBSTR(max(m2.fecha_prim_insta),1,2)  from clientes:medid m2 where m2.numero_cliente = c.numero_cliente ) || ");
            sql.Append(" (select SUBSTR(max(m2.fecha_prim_insta),4,2)  from clientes:medid m2 where m2.numero_cliente = c.numero_cliente ) || '000000' ");
            sql.Append(" When  (select max(m2.fecha_orden_xn)  from clientes:medid m2 where m2.numero_cliente = c.numero_cliente )  is not null ");
            sql.Append(" then ");
            sql.Append("    (select SUBSTR(max(m2.fecha_orden_xn),7,4)  from clientes:medid m2 where m2.numero_cliente = c.numero_cliente ) || ");
            sql.Append("    (select SUBSTR(max(m2.fecha_orden_xn),1,2)  from clientes:medid m2 where m2.numero_cliente = c.numero_cliente ) || ");
            sql.Append("    (select SUBSTR(max(m2.fecha_orden_xn),4,2)  from clientes:medid m2 where m2.numero_cliente = c.numero_cliente ) || '000000' ");
            sql.Append(" else ");
            sql.Append(" (SUBSTR(c.fecha_ultima_lect,7,4) || SUBSTR(c.fecha_ultima_lect,1,2) || SUBSTR(c.fecha_ultima_lect,4,2) || '000000')  end ");
            sql.Append(" else ");
            sql.Append(" '20000101' end)  ");
            sql.Append(" end as  dtAllacciamento, ");

            sql.Append(" '0' as flagMisPot,");
            sql.Append(" '110' as tensione,");
            sql.Append(" '110' as tensFor,");
            sql.Append(" ' ' as fondoScala,");
            sql.Append(" ' ' as matr1ta,");
            sql.Append(" ' ' as matr2ta,");
            sql.Append(" ' ' as matr3ta,");
            sql.Append(" ' ' as matr4ta,");
            if (empresa.Equals(Empresa.RJ))
                sql.Append(" 'BR101E'||LPAD(c.numero_cliente ,8,0) as pod,");
            else
                sql.Append(" 'BR102E'||LPAD(c.numero_cliente ,8,0) as pod,");
            sql.Append(" c.tiene_postal as scala,");
            sql.Append(" ' ' as piano,");
            sql.Append(" ' ' as interno,");
            sql.Append(" ' ' as CAP,");
            sql.Append(" (select b.nome from barrios b where b.codigo_barrio = c.comuna AND b.municipio = c.municipio) as localita,");
            sql.Append(" c.telefono as numTel1,");
            sql.Append(" (select max(nro_servicio) from sepro where nro_cliente_sec = c.numero_cliente)  as noteAccesso,");
            sql.Append(" c.rut || c.dv_rut as codiceFiscale,");
            sql.Append(" c.rut || c.dv_rut as partitaIVA,");
            sql.Append(" '1' as codU,");
            sql.Append(" case when  m.enteros is null or m.enteros =0  then 6 else m.enteros end as cifreAtt,");
            sql.Append(" ' ' as dataScadenzaStr,");
            sql.Append(" '100' as potenzaFranchigia,");
            sql.Append(" ' ' as situazForn,");
            sql.Append(" '100' as potImpegnata,");
            sql.Append(" ' ' as codContributo,");
            sql.Append(" ' ' as cognome,");
            sql.Append(" c.nombre as nome,");
            sql.Append(" ' ' as ragioneSociale,");
            sql.Append(" '20' as determinazContiContratti,");
            sql.Append(" ' ' as codEsazione,");
            sql.Append(" c.cep as capEsaz,");
            sql.Append(" loca.descripcion as comuneEsaz,");
            sql.Append("    m.constante AS constEnrgAttivaFrF3,");
            sql.Append("    1 AS constEnrgUferHpReattivaF1,");
            sql.Append("    1 AS constEnrgDncrHpReattivaF2,");
            sql.Append("    1 AS constEnrgUferFpReattivaF3,");
            sql.Append("    1 as constEnrgDncrReattivaFpF4,");
            sql.Append("    1 as constEnrgUferReattivaHrF5,");
            sql.Append("    1 as constEnrgDncrReattivaHrF6,");
            sql.Append("    1 as constEnrgPtnzAttivaHrF1,");
            sql.Append("    1 AS constEnrgPtnzAttivaHpF2,");
            sql.Append("    1 AS constEnrgPtnzAttivaFpF3,");
            sql.Append("    loca.localidad AS centroOperativo,");

            sql.Append(" replace(replace(SUBSTR(c.direccion,0,24),chr(10),''),chr(13),'') as viaEsaz,");
            sql.Append(" ' ' as componenteAuc,");
            sql.Append(" ' ' as energiaAdF3Ass,");
            sql.Append(" ' ' as energiaAdF3Ael,");
            sql.Append(" ' ' as enerAnnuaCott,");
            sql.Append(" ' ' as scontoPotF3,");
            sql.Append(" ' ' as potAggDetrazF3,");
            sql.Append(" ' ' as potAggDetrazF3_2,");
            sql.Append(" ' ' as percMaggiorazione,");
            sql.Append(" ' ' as potContrCottimo,");
            sql.Append(" ' ' as dtFirmaContrCommittStr,");
            sql.Append(" ' ' as dtFirmaContrContraeStr,");
            sql.Append(" Case when c.consumo_30_dias > 0 then '04'  else '00' end as idTipoFornit,");
            sql.Append(" ' ' as oreUtilizzo,");
            sql.Append(" '0' as cifrePotenza,");
            sql.Append(" '0' as cifreReattiva,");
            sql.Append(" '0' as codMisuratorePot,");
            sql.Append(" '0' as codMisuratoreRea,");
            sql.Append(" '39' as codInserzionePotenza,");
            sql.Append(" '0' as matrMisuratorePot,");
            sql.Append(" '0' as matrMisuratoreRea,");
            sql.Append(" 'A' as frequenzaLettura,");
            sql.Append(" ' ' as idFase,");
            sql.Append(" '1' as tipoRiprogr,");
            sql.Append(" 'N' as nonDisalimentabile,");
            sql.Append(" 'ENEL' AS gestoreRete,");
            sql.Append(" ' ' AS tipoOpzione,");
            sql.Append(" ' ' AS flagSwitchCessato,");
            sql.Append(" ' ' AS provincia,");
            sql.Append(" ' ' AS provinciaEsaz,");

            sql.Append(" '1' as flagMandatoConnessione,");
            sql.Append(" 'DP0426' AS idDispacc,");
            sql.Append(" 'BR' as nazioneEsaz,");
            sql.Append(" case when (m.fecha_prim_insta > c.fecha_ultima_lect) or (m.fecha_ult_insta > c.fecha_ultima_lect) or (m.numero_medidor = 0) or (m.numero_medidor is null) or (m.numero_medidor = '') ");
            sql.Append("    then SUBSTR(c.fecha_ultima_lect,7,4)||SUBSTR(c.fecha_ultima_lect,1,2)||SUBSTR(c.fecha_ultima_lect,4,2) || '000000' ");
            sql.Append("    else ( ");
            sql.Append("        Case when m.fecha_prim_insta is not null then SUBSTR(m.fecha_prim_insta,7,4)||SUBSTR(m.fecha_prim_insta,1,2)||SUBSTR(m.fecha_prim_insta,4,2) || '000000' ");
            sql.Append("            when m.fecha_prim_insta is null and  m.fecha_ult_insta is not null then SUBSTR(m.fecha_ult_insta,7,4)||SUBSTR(m.fecha_ult_insta,1,2)||SUBSTR(m.fecha_ult_insta,4,2) || '000000' ");
            sql.Append("            When  (select max(m2.fecha_prim_insta)  from clientes:medid m2 where m2.numero_cliente = c.numero_cliente )  is not null then ");
            sql.Append("            (select SUBSTR(max(m2.fecha_prim_insta),7,4)  from clientes:medid m2 where m2.numero_cliente = c.numero_cliente ) || (select SUBSTR(max(m2.fecha_prim_insta),1,2)  from clientes:medid m2 where m2.numero_cliente = c.numero_cliente ) ||  (select SUBSTR(max(m2.fecha_prim_insta),4,2) || '000000'");
            sql.Append("            from clientes:medid m2 where m2.numero_cliente = c.numero_cliente ) ");
            sql.Append("            When  (select max(m2.fecha_orden_xn)  from clientes:medid m2 where m2.numero_cliente = c.numero_cliente )  is not null then ");
            sql.Append("            (select SUBSTR(max(m2.fecha_orden_xn),7,4)  from clientes:medid m2 where m2.numero_cliente = c.numero_cliente ) || (select SUBSTR(max(m2.fecha_orden_xn),1,2)  from clientes:medid m2 where m2.numero_cliente = c.numero_cliente ) ||  (select SUBSTR(max(m2.fecha_orden_xn),4,2) || '000000'");
            sql.Append("            from clientes:medid m2 where m2.numero_cliente = c.numero_cliente ) ");
            sql.Append("            else '20000101' end)");
            sql.Append("    end as dataDecorrenzaMis,");
            sql.Append(" case when (c.classe || c.subclasse = '0806') or (S.numero_cliente is not null and s.fact_smc = 'S' and c.estado_suministro != 0) ");
            sql.Append(" then (");
            sql.Append("    case when(");
            sql.Append("        case when ((select count(*) from medid where medid.numero_medidor = m.numero_medidor and estado = 'I') = 1) then ");
            sql.Append("            Case when m.fecha_prim_insta is not null ");
            sql.Append("                then ");
            sql.Append("                    SUBSTR(m.fecha_prim_insta,7,4) || ");
            sql.Append("                    SUBSTR(m.fecha_prim_insta,1,2) || ");
            sql.Append("                    SUBSTR(m.fecha_prim_insta,4,2) ");
            sql.Append("                when m.fecha_prim_insta is null and  m.fecha_ult_insta is not null ");
            sql.Append("            then ");
            sql.Append("                SUBSTR(m.fecha_ult_insta,7,4) || ");
            sql.Append("                SUBSTR(m.fecha_ult_insta,1,2) || ");
            sql.Append("                SUBSTR(m.fecha_ult_insta,4,2) ");
            sql.Append("            When  (select max(m2.fecha_prim_insta)  from clientes:medid m2 where m2.numero_cliente = c.numero_cliente )  is not null ");
            sql.Append("            then ");
            sql.Append("                (select SUBSTR(max(m2.fecha_prim_insta),7,4) from clientes:medid m2 where m2.numero_cliente = c.numero_cliente ) || ");
            sql.Append("                (select SUBSTR(max(m2.fecha_prim_insta),1,2)  from clientes:medid m2 where m2.numero_cliente = c.numero_cliente ) || ");
            sql.Append("                (select SUBSTR(max(m2.fecha_prim_insta),4,2)  from clientes:medid m2 where m2.numero_cliente = c.numero_cliente ) ");
            sql.Append("            else ");
            sql.Append("                (select SUBSTR(max(m2.fecha_orden_xn),7,4)  from clientes:medid m2 where m2.numero_cliente = c.numero_cliente ) || ");
            sql.Append("                (select SUBSTR(max(m2.fecha_orden_xn),1,2)  from clientes:medid m2 where m2.numero_cliente = c.numero_cliente )|| ");
            sql.Append("                (select SUBSTR(max(m2.fecha_orden_xn),4,2)  from clientes:medid m2 where m2.numero_cliente = c.numero_cliente ) end ");
            sql.Append("        else ");
            sql.Append("         (SUBSTR(c.fecha_ultima_lect,7,4) || SUBSTR(c.fecha_ultima_lect,1,2) || SUBSTR(c.fecha_ultima_lect,4,2)) end ");
            sql.Append("        ) ");
            sql.Append("        > ");
            sql.Append("        (SUBSTR(c.fecha_ultima_lect,7,4) || SUBSTR(c.fecha_ultima_lect,1,2) || SUBSTR(c.fecha_ultima_lect,4,2)) ");
            sql.Append("        then ( ");
            sql.Append("                case when ((select count(*) from medid where medid.numero_medidor = m.numero_medidor and estado = 'I') = 1) then ");
            sql.Append("                    Case when m.fecha_prim_insta is not null");
            sql.Append("                        then ");
            sql.Append("                            SUBSTR(m.fecha_prim_insta,4,2) || ");
            sql.Append("                            SUBSTR(m.fecha_prim_insta,1,2) || ");
            sql.Append("                            SUBSTR(m.fecha_prim_insta,7,4) ");
            sql.Append("                    when m.fecha_prim_insta is null and  m.fecha_ult_insta is not null ");
            sql.Append("                    then ");
            sql.Append("                        SUBSTR(m.fecha_ult_insta,4,2) || ");
            sql.Append("                        SUBSTR(m.fecha_ult_insta,1,2) || ");
            sql.Append("                        SUBSTR(m.fecha_ult_insta,7,4) ");
            sql.Append("                    When  (select max(m2.fecha_prim_insta)  from clientes:medid m2 where m2.numero_cliente = c.numero_cliente )  is not null ");
            sql.Append("                    then ");
            sql.Append("                        (select SUBSTR(max(m2.fecha_prim_insta),4,2)  from clientes:medid m2 where m2.numero_cliente = c.numero_cliente ) || ");
            sql.Append("                        (select SUBSTR(max(m2.fecha_prim_insta),1,2)  from clientes:medid m2 where m2.numero_cliente = c.numero_cliente ) || ");
            sql.Append("                        (select SUBSTR(max(m2.fecha_prim_insta),7,4) ");
            sql.Append("                    from clientes:medid m2 where m2.numero_cliente = c.numero_cliente ) ");
            sql.Append("                    else ");
            sql.Append("                        (select SUBSTR(max(m2.fecha_orden_xn),4,2)  from clientes:medid m2 where m2.numero_cliente = c.numero_cliente ) || ");
            sql.Append("                        (select SUBSTR(max(m2.fecha_orden_xn),1,2)  from clientes:medid m2 where m2.numero_cliente = c.numero_cliente ) || ");
            sql.Append("                        (select SUBSTR(max(m2.fecha_orden_xn),7,4) ");
            sql.Append("                    from clientes:medid m2 where m2.numero_cliente = c.numero_cliente ) end ");
            sql.Append("            else ");
            sql.Append("            (SUBSTR(c.fecha_ultima_lect,4,2) || SUBSTR(c.fecha_ultima_lect,1,2) || SUBSTR(c.fecha_ultima_lect,7,4)) end) ");
            sql.Append(" else ");
            sql.Append("    (SUBSTR(c.fecha_ultima_lect,4,2) || ");
            sql.Append("     SUBSTR(c.fecha_ultima_lect,1,2) || ");
            sql.Append("     SUBSTR(c.fecha_ultima_lect,7,4)) end )");
            sql.Append(" else ");
            sql.Append("    SUBSTR(c.fecha_ultima_lect,7,4) || SUBSTR(c.fecha_ultima_lect,1,2) ||  SUBSTR(c.fecha_ultima_lect,4,2) || '000000' end  as dataEfficacia, ");
            sql.Append("    case when (select max(1) from clire where numero_cliente  = c.cliente_anterior ");
            sql.Append("    and fecha_retiro >= c.fecha_ultima_lect and fecha_retiro >= today -30) = 1 ");
            sql.Append("    then  LPAD(c.cliente_anterior,9,0) ");
            sql.Append("    when c.corr_facturacion = 0 then LPAD(nvl(c.cliente_anterior,0),9,0) else '0' end as eneltelPrecedente, ");
            sql.Append("    case when tec.acometida_retirada = 'K' then '1' else '0' end  as puntoProduttore,");
            sql.Append(" '100' as potProduzione,");
            sql.Append(" Case when c.estado_suministro = 0 then  ' ' else (");
            sql.Append(" select case when max(cor.fecha_corte) is not null then SUBSTR(max(cor.fecha_corte),4,2)||SUBSTR(max(cor.fecha_corte),1,2)||SUBSTR(max(cor.fecha_corte),7,4) ");
            sql.Append("    else ");
            sql.Append(" SUBSTR((c.fecha_ultima_lect),4,2)||SUBSTR((c.fecha_ultima_lect),1,2)||SUBSTR((c.fecha_ultima_lect),7,4) ");
            sql.Append(" end ");
            sql.Append("    from correp cor ");
            sql.Append("    Where cor.numero_cliente = c.numero_cliente ");
            sql.Append("    ) end AS dataCessazione,");
            sql.Append("    0 as saldo,");
            sql.Append("    'OUTROS' tipoTariffa,");
            sql.Append("    'OUTROS' as tipoMisuratore,");
            sql.Append("    c.estado_cliente as statoCliente,");
            sql.Append("    'OUTROS' tipoCliente,");
            sql.Append("    1 as constEnrgAttivaHrF1,");
            sql.Append("    1 AS constEnrgAttivaHpF2,");
            sql.Append("    ' ' dataFatturazioneFissa,");
            sql.Append("    ' ' dataInizioProva,");
            sql.Append("    ' ' dataFineProva,");
            sql.Append("    ' ' inicial,");
            sql.Append("    c.classe CLASSE,");
            sql.Append("    c.subclasse classeESub,");
            sql.Append("    ' ' valorePerc,");
            //sql.Append("    c.numero_cliente, c.estado_suministro, m.numero_medidor, m.marca_medidor, m.tipo_medidor, ");                                            
            sql.Append("  case  ");
            sql.Append("    when( ");
            sql.Append("        S.numero_cliente IS NOT NULL");
            sql.Append("   AND    m.tipo_medidor in ('05') and lec.membro <> 4) THEN 'CALCULATED' ");
            sql.Append("   ELSE 'VISUAL'  ");
            sql.Append("   END AS sourcetype, ");

            sql.Append(" case when  m.enteros is null or m.enteros =0  then 6 else m.enteros end as cifreDecimaliAttiva,     ");
            sql.Append(" m.enteros as cifreDecimaliReattiva,  ");
            sql.Append(" m.enteros as cifreDecimaliPotenza,  ");
            sql.Append(" tec.coord_lat_gps_lida as latitudine,  ");
            sql.Append(" ' ' AS letturaAttF1,");
            sql.Append(" ' ' AS letturaAttF2,");
            sql.Append(" ' ' AS letturaAttF3,");
            sql.Append(" ' ' AS letturaAttF4,");
            sql.Append(" ' ' AS letturaReaF1,");
            sql.Append(" ' ' AS letturaReaF2,");
            sql.Append(" ' ' AS letturaReaF3,");
            sql.Append(" ' ' AS letturaReaF4,");
            sql.Append(" ' ' AS letturaPotF1,");
            sql.Append(" ' ' AS letturaPotF2,");
            sql.Append(" ' ' AS letturaPotF3,");
            sql.Append(" ' ' AS letturaPotF4,");

            sql.Append(" tec.coord_lon_gps_lida as longitudine,   ");
            sql.Append("  'N' as flagMisuratoreInterno,  ");
            sql.Append("  'N' as clienteSpeciale,  ");
            sql.Append("  ' ' as interseccion,  ");
            sql.Append(" c.TELEFONO as telefono,   ");
            sql.Append(" ' ' as codigoNroConfig,   ");
            sql.Append(" ' ' as precintoT1,  ");
            sql.Append(" ' ' as precintoT2T3N1, ");
            sql.Append(" ' ' as precintoT2T3N2,  ");
            sql.Append(" ' ' as precintoT2T3N3,   ");
            sql.Append(" ' ' as precintoT2T3N4,   ");
            sql.Append(" ' ' as codigoPartido,   ");
            sql.Append("  '0' as recorrido, ");
            sql.Append("  '0' as advlecturista, ");
            if (base.empresa.Equals(Empresa.RJ))
                sql.Append("  'AA01' as advlecturista, ");
            else
                sql.Append("  'CC01' as advlecturista, ");
            sql.Append("  'B' as gruppo, ");
            if (empresa.Equals(Empresa.RJ))
                sql.Append("  'AA01' as codSocieta, ");
            else
                sql.Append("  'CC01' as codSocieta, ");

            if (!String.IsNullOrEmpty(numero_ordem))
                sql.Append(" " + "'" + numero_ordem + "'" + " as idOrdineLavoro ");
            else
                sql.Append(" '00000' as idOrdineLavoro ");


            sql.Append(" FROM   cliente c,");
            sql.Append("        agenda age,");
            sql.Append("        localidades loca,");
            sql.Append("        lectu_agrup lec,");
            sql.Append("        tabla est,");
            sql.Append("        outer tabla cla,");
            sql.Append("        tecni tec,");
            sql.Append("        outer medid m,");
            sql.Append("        OUTER(coletor_assoc col, inscor ins, cotrat con),");
            sql.Append("        OUTER CLIENTE_SMC S,");
            sql.Append("        lectu_diversos ld");
            sql.Append(" WHERE ");
            sql.Append("    lec.sector = age.sector ");
            sql.Append("    AND lec.localidade = age.localidade ");
            sql.Append("    AND lec.zona = age.zona ");
            sql.Append("    AND loca.localidad         = lec.localidade    ");
            sql.Append("    AND col.referencia        = lec.referencia    ");
            sql.Append("    AND col.sector            = lec.sector     ");
            sql.Append("    AND col.polo              = loca.polo    ");
            sql.Append("    AND col.cod_colet         = lec.cod_colet  ");
            sql.Append("    AND est.sucursal = '0000'   ");
            sql.Append("    AND est.nomtabla = 'LECEST'  ");
            sql.Append("    AND est.codigo = lec.estado   ");
            sql.Append("    AND lec.membro = ld.membro  ");
            sql.Append("    AND ld.numero_cliente = c.numero_cliente ");
            sql.Append("    AND ld.sector = lec.sector ");
            sql.Append("    AND ld.zona = lec.zona ");
            sql.Append("    AND c.numero_cliente = c.numero_cliente  ");
            sql.Append("    AND col.referencia        = lec.referencia   ");
            sql.Append("    AND col.sector            = lec.sector   ");
            sql.Append("    AND col.cod_colet         = lec.cod_colet  ");
            sql.Append("    AND ins.area              = col.area    ");
            sql.Append("    AND ins.codigo_instalador = col.codigo_instalador");
            sql.Append("    AND con.contratista       = ins.contratista  ");
            sql.Append("    And c.numero_cliente = m.numero_cliente  ");
            sql.Append("    And m.estado = 'I'  ");
            sql.Append("    AND cla.sucursal = '0000'    ");
            sql.Append("    AND cla.nomtabla = 'CLALEC'   ");
            sql.Append("    AND cla.codigo =  c.cod_consumo_incl  ");
            sql.Append("    AND cla.codigo = c.clave_lectura_ant ");
            sql.Append("    AND c.numero_cliente =   S.numero_cliente  ");
            sql.Append("    And s.fecha_desactivac is null  ");
            sql.Append("    AND cla.fecha_desactivac is null    ");
            sql.Append("    AND est.fecha_desactivac is null   ");
            sql.Append("    AND tec.numero_cliente = c.numero_cliente  ");
            sql.Append(String.Format("    and c.numero_cliente ={0}", numeroCliente));
            if (numero_medidor != 0)
                sql.Append(String.Format("    and m.numero_medidor ={0}", numero_medidor));
            sql.Append(" order by age.fecha_proceso desc ");

            //var dt = ConsultaSql(sql.ToString());
            var dt = ConsultaSql(sql.ToString());
            if (dt.Rows.Count > 0)
            {
                //Type tipo = new ClienteModMedidor().GetType();
                //EntidadeBase eb = new EntidadeBase();
                //object resultado = eb.gerarEntidadeGenerica(dt, tipo);
                //ClienteModMedidor resultado = DataHelper.ConvertDataTableToDTO<ClienteModMedidor>(dt);

                //return resultado;                
                //Type tipo = new AnagraficaDTO().GetType();
                //EntidadeBase eb = new EntidadeBase();
                AnagraficaDTO resultado = DataHelper.ConvertDataTableToEntity<AnagraficaDTO>(dt);
                //object resultado = eb.gerarEntidadeGenerica(dt, tipo);

                return (AnagraficaDTO)resultado;
            }
            else
                return null;
        }

        public string gerarSQLObterMotivoDoCorteAtualSalesForce(int numeroCliente)
        {
            StringBuilder sql = new StringBuilder();
            sql.Append(" SELECT first 1                                 ");
            sql.Append(" c.numero_cliente,                              ");
            sql.Append(" to_char(fecha_corte,'%d/%m/%Y') as fecha,      ");
            sql.Append(" c.motivo_corte as guion,                       ");
            sql.Append(" to_char(hora_exec_corte,'%H:%M:%S') as Hora,   ");
            sql.Append(" c.valor_divida as valor,                       ");
            sql.Append(" cl.Nombre as Nome,                             ");
            sql.Append(" cl.estado_cliente as EstadoCliente,            ");
            sql.Append(" cl.ind_cliente_vital as vital, ");
            sql.Append(" cl.rut as documento1,                          ");
            sql.Append(" cl.dv_rut as dvDoc1,                           ");
            sql.Append(" cl.estado_suministro as estadoFornecimento,    ");
            sql.Append(" cl.estado_facturacion as EstadoFaturamento,    ");
            sql.Append(" cl.sector as loteFaturamento                   ");
            sql.Append(" from                                           ");
            sql.Append(" outer clientes@clientes:correp c, cliente cl   ");
            sql.Append(" where                                          ");
            sql.Append(" cl.numero_cliente = " + numeroCliente);
            sql.Append(" AND cl.numero_cliente = c.numero_cliente       ");
            sql.Append(" AND c.corr_corte = cl.corr_corte               ");
            sql.Append(" AND nvl(c.fecha_solic_repo,'') = ''            ");
            sql.Append(" order by fecha desc                            ");

            return sql.ToString();
        }

        public Entidades.SalesForce.Corte MotivoCorteAtualSalesForce(int numeroCliente)
        {
            List<Entidades.SalesForce.Corte> obj = new List<Entidades.SalesForce.Corte>();

            string sql = gerarSQLObterMotivoDoCorteAtualSalesForce(numeroCliente);
            DataTable dtResultado = ConsultaSql(sql);

            string formatado = DataTableToJSON(dtResultado);

            obj = new JavaScriptSerializer().Deserialize<List<Entidades.SalesForce.Corte>>(formatado);

            if (obj.Count > 0)
            {
                if (obj[0].EstadoFaturamento != "S")
                {
                    obj[0].EstadoFaturamento = "N";
                }
                if (obj[0].estadoFornecimento.Equals("0"))
                {
                    obj[0].Guion = string.Empty;
                    obj[0].Fecha = string.Empty;
                }
            }

            if (obj.Count == 0)
            {
                Entidades.SalesForce.Corte obj_erro = new Entidades.SalesForce.Corte();
                obj_erro.CodigoError = "ERROR003";
                obj_erro.MensajeError = "Cliente não possui corte ativo.";

                return obj_erro;
            }
            else
            {
                return obj.First();
            }
        }

        public IList<Entidades.Corte> BuscaCorteCliente(string numeroCliente)
        {
            string sql = gerarSQLBuscaCorteCliente(numeroCliente);
            DataTable dtResultado = ConsultaSql(sql);

            return dtToListObject<Entidades.Corte>(dtResultado);
        }

        protected virtual string gerarSQLBuscaCorteCliente(string numeroCliente)
        {
            StringBuilder sql = new StringBuilder();
            sql.Append(" SELECT ");
            sql.Append(" to_char(data_solic_corte,'%d/%m/%Y') data_emissao, ");
            sql.Append(" to_char(fecha_corte,'%d/%m/%Y') data_execucao, ");
            sql.Append(" tc.descricao_tipo tipo_corte, ");
            sql.Append(" c.tipo_corte as tip_corte, ");
            sql.Append(" to_char(hora_exec_corte,'%H:%M:%S') hora_corte, ");
            sql.Append(" acreco.descripcion situacao_corte, ");
            sql.Append(" '' numero_ordem, ");
            sql.Append(" '' ACC_REALIZADA_COR, ");
            sql.Append(" data_solic_corte data_hora_solicitacao ");
            sql.Append(" from  ");
            sql.Append(" clientes@clientes:correp c,  ");
            sql.Append(" outer clientes@clientes:tabla acreco,  ");
            sql.Append(" outer clientes@clientes:tipo_corte tc ");
            sql.Append(" where  ");
            sql.Append(" numero_cliente = '" + numeroCliente + "' ");
            sql.Append(" AND acreco.nomtabla = 'ACRECO'  ");
            sql.Append(" AND acreco.codigo = c.acc_realizada_cor ");
            sql.Append(" And acreco.sucursal = '0000' ");
            sql.Append(" And ((acreco.fecha_activacion >= acreco.fecha_desactivac And acreco.fecha_desactivac > Today)  Or acreco.fecha_desactivac is null) ");
            sql.Append(" and tc.tipo_corte = c.tipo_corte ");
            sql.Append(" UNION ");
            sql.Append(" SELECT  ");
            sql.Append(" to_char(c.Fecha_solicitud,'%d/%m/%Y') data_emissao, ");
            sql.Append(" '' data_execucao, ");
            sql.Append(" tc.descricao_tipo tipo_corte, ");
            sql.Append(" c.tipo_corte as tip_corte, ");
            sql.Append(" '' hora_corte, ");
            sql.Append(" nvl(estcor.descripcion, c.estado) situacao_corte, ");
            sql.Append(" '' numero_ordem, ");
            sql.Append(" '' ACC_REALIZADA_COR, ");
            sql.Append(" c.Fecha_solicitud data_hora_solicitacao ");
            sql.Append(" from ");
            sql.Append(" clientes@clientes:corsoco c,  ");
            sql.Append(" outer clientes@clientes:tabla estcor,  ");
            sql.Append(" outer clientes@clientes:tipo_corte tc  ");
            sql.Append(" where c.numero_cliente = '" + numeroCliente + "' ");
            sql.Append(" and tc.tipo_corte = c.tipo_corte ");
            sql.Append(" and estcor.codigo = c.estado ");
            sql.Append(" and estcor.nomtabla = 'ESTCOR' ");
            sql.Append(" and estcor.sucursal = '0000' ");
            sql.Append(" and ((estcor.fecha_activacion >= estcor.fecha_desactivac and estcor.fecha_desactivac > Today)  Or estcor.fecha_desactivac is null) ");
            sql.Append(" order by data_hora_solicitacao desc, data_execucao desc ");

            return sql.ToString();
        }

        public IList<TEntidade> dtToListObject<TEntidade>(System.Data.DataTable dt)
        {
            IList<Entidades.Corte> result = new List<Entidades.Corte>();

            foreach (DataRow item in dt.Rows)
            {
                Entidades.Corte tempCorte = new Entidades.Corte();

                tempCorte.DataDeSolicitacao = TratarString(dt, item, "DATA_EMISSAO");
                tempCorte.DataDaExecucao = TratarString(dt, item, "DATA_EXECUCAO");

                tempCorte.Tipo = TratarString(dt, item, "TIPO_CORTE");
                tempCorte.Hora = TratarString(dt, item, "HORA_CORTE");
                tempCorte.Situacao = TratarString(dt, item, "SITUACAO_CORTE");
                tempCorte.NumeroOrdem = TratarString(dt, item, "NUMERO_ORDEM");
                tempCorte.AcaoRealizada = TratarString(dt, item, "ACC_REALIZADA_COR");

                result.Add(tempCorte);
            }

            return result as IList<TEntidade>;
        }

        public IList<Entidades.Corte> VerificarUltimoCorteDesligamentoPedido(string numeroCliente)
        {
            string sql = gerarSQLVerificarUltimoCorteDesligamentoPedido(numeroCliente);
            DataTable dtResultado = ConsultaSql(sql);

            return dtToListObject<Entidades.Corte>(dtResultado);
        }

        protected virtual string gerarSQLVerificarUltimoCorteDesligamentoPedido(string numeroCliente)
        {
            StringBuilder sql = new StringBuilder();
            sql.Append(" SELECT ");
            sql.Append(" '' data_emissao, ");
            sql.Append(" '' data_execucao, ");
            sql.Append(" '' tipo_corte, ");
            sql.Append(" '' hora_corte, ");
            sql.Append(" '' situacao_corte, ");
            sql.Append(" '' ACC_REALIZADA_COR, ");
            sql.Append(" c.numero_livro, ");
            sql.Append(" c.num_ordem_serv_crt numero_ordem ");
            sql.Append(" from  ");
            sql.Append(" clientes@clientes:correp c ");
            sql.Append(" where  ");
            sql.Append(" c.numero_cliente = '" + numeroCliente + "' ");
            sql.Append(" and c.tipo_corte in ('01','02') ");

            return sql.ToString();
        }

        public IList<Entidades.Corte> VerificarTipoCorteAcaoRealizada(int numeroCliente)
        {
            string sql = gerarSQLVerificarTipoCorteAcaoRealizada(numeroCliente);
            DataTable dtResultado = ConsultaSql(sql);

            return dtToListObject<Entidades.Corte>(dtResultado);
        }

        protected virtual string gerarSQLVerificarTipoCorteAcaoRealizada(int numeroCliente)
        {
            StringBuilder sql = new StringBuilder();
            sql.Append(" SELECT FIRST 1 ");
            sql.Append(" '' data_emissao, ");
            sql.Append(" '' data_execucao, ");
            sql.Append(" c.tipo_corte tipo_corte, ");
            sql.Append(" '' hora_corte, ");
            sql.Append(" '' situacao_corte, ");
            sql.Append(" c.acc_realizada_cor ACC_REALIZADA_COR, ");
            sql.Append(" '' numero_livro, ");
            sql.Append(" c.fecha_corte, ");
            sql.Append(" '' numero_ordem ");
            sql.Append(" from  ");
            sql.Append(" clientes@clientes:correp c ");
            sql.Append(" where  ");
            sql.Append(" c.numero_cliente = '" + numeroCliente + "' ");
            sql.Append(" and (c.fecha_reposicion is null or c.fecha_reposicion = '') ");
            sql.Append(" order by c.fecha_corte desc ");

            return sql.ToString();
        }

        /// <summary>
        /// Atualiza o correlativo de Corte do cliente, incrementando o correlativo informado no parâmetro.
        /// </summary>
        /// <param name="numeroCliente"></param>
        /// <param name="ultimoCorrelativoCorte">Último correlativo registrado.  O novo valor será o incremento deste parâmetro.</param>
        /// <param name="conn"></param>
        /// <returns></returns>
        public bool AtualizarCorrelativoCorte(int numeroCliente, int ultimoCorrelativoCorte, DBProviderInformix conn)
        {
            bool result = false;

            StringBuilder sqlUpdateCorrelativo = new StringBuilder();
            sqlUpdateCorrelativo.AppendFormat(" update cliente set corr_corte = {0} Where numero_cliente = {1} ", ultimoCorrelativoCorte+1, numeroCliente);

            return ExecutarSql(sqlUpdateCorrelativo.ToString(), conn);

            #region Obsolete
            StringBuilder sqlCorrep = new StringBuilder();
            sqlCorrep.Append(" Select count(*) as qtd from correp  ");
            sqlCorrep.AppendFormat(" where numero_cliente = {0} ", numeroCliente);
            sqlCorrep.AppendFormat(" and corr_corte = {0}", ultimoCorrelativoCorte);
            sqlCorrep.Append(" and ( fecha_reposicion is not null or fecha_reposicion != '')");

            bool possuiCorrep = false;

            DataTable dtCorrep = ConsultaSql(sqlCorrep.ToString());

            if (dtCorrep.Rows.Count > 0)
            {
                int resultQtdCorrep = 0;
                int.TryParse(dtCorrep.Rows[0]["qtd"].ToString(), out resultQtdCorrep);
                possuiCorrep = resultQtdCorrep > 0;
            }

            if (!possuiCorrep)
            {
                StringBuilder sqlCorsore = new StringBuilder();
                sqlCorsore.Append(" Select count(*) as qtd from corsore  ");
                sqlCorsore.AppendFormat(" where numero_cliente = {0} ", numeroCliente);
                sqlCorsore.AppendFormat(" and corr_corte = {0} ", ultimoCorrelativoCorte);

                bool possuiCorsore = false;

                DataTable dtCorsore = ConsultaSql(sqlCorsore.ToString());

                if (dtCorsore.Rows.Count > 0)
                {
                    int resultQtdCorsore = 0;
                    int.TryParse(dtCorsore.Rows[0]["qtd"].ToString(), out resultQtdCorsore);
                    possuiCorsore = resultQtdCorsore > 0;

                    if (possuiCorsore)
                    {
                        StringBuilder sqlUpdateCorrelativo2 = new StringBuilder();
                        sqlUpdateCorrelativo2.AppendFormat(" update cliente set corr_corte = corr_corte + 1 Where numero_cliente = {0} ", numeroCliente);

                        return ExecutarSql(sqlUpdateCorrelativo2.ToString(), conn);
                    }
                }
            }

            return result;
            #endregion Obsolete
        }

        protected virtual string gerarSQLObterMotivoDoCorteAtual(int numeroCliente)
        {
            StringBuilder sql = new StringBuilder();
            sql.AppendFormat(@"SELECT FIRST 1 cr.motivo_corte
								 FROM correp cr
						   INNER JOIN tabla t
								   ON t.codigo         = cr.acc_realizada_cor
								  AND t.nomtabla       = 'ACRECO'
								  AND t.sucursal       = '0000'
								  AND t.valor_alf[2,2] = '1'
								WHERE cr.numero_cliente = '{0}'
								  AND cr.motivo_corte   <> '20'
								  AND NOT EXISTS (SELECT 1
													FROM correp cr1
											  INNER JOIN tabla t1
													  ON cr1.numero_cliente    = cr.numero_cliente
													 AND cr1.acc_realizada_rep = t1.codigo
													 AND t1.nomtabla           = 'ACRERE'
													 AND t1.sucursal           = '0000'
													 AND t1.valor_alf[2,2]     = '1'
												   WHERE cr1.fecha_reposicion  > cr.data_atual_corte)
							 ORDER BY cr.data_atual_corte DESC "
                , numeroCliente);

            return sql.ToString();
        }

        public string ObterMotivoDoCorteAtual(int numeroCliente)
        {
            string sql = gerarSQLObterMotivoDoCorteAtual(numeroCliente);
            DataTable dtResultado = ConsultaSql(sql);

            string motivo = "";

            foreach (DataRow item in dtResultado.Rows)
                motivo = TratarString(dtResultado, item, "motivo_corte");

            return motivo;
        }

        public string[] ObterPrevisaoDeCorte(int numeroCliente)
        {
            var sql = string.Format(@"select to_char(data_apto_corte, '%d/%m/%Y') as data_apto_corte
										from hisreav 
									   where numero_cliente = '{0}'
										 and estado = 'V' "
                                       , numeroCliente);

            DataTable dtResultado = ConsultaSql(sql);

            List<string> datasDePrevisaoParaCorte = new List<string>();

            foreach (DataRow item in dtResultado.Rows)
            {
                string dataPrevistaParaCorte = TratarString(dtResultado, item, "data_apto_corte");
                datasDePrevisaoParaCorte.Add(dataPrevistaParaCorte);
            }

            return datasDePrevisaoParaCorte.ToArray();
        }

        //public HistoricoReaviso[] ConsultarHistoricoReaviso(int numeroCliente)
        //{
        //    List<HistoricoReaviso> result = new List<HistoricoReaviso>();

        //    StringBuilder sql = new StringBuilder();
        //    sql.Append(" SELECT ");
        //    sql.Append(" h.corr_reaviso, ");
        //    sql.Append(" h.referencia_fat, ");
        //    sql.Append(" h.data_reaviso, ");
        //    sql.Append(" h.divida_cliente, ");
        //    sql.Append(" h.estado cod_estado, ");
        //    sql.Append(" t.descripcion desc_estado , ");
        //    sql.Append(" h.data_estado, ");
        //    sql.Append(" h.rol_estado, ");
        //    sql.Append(" r2.nombre nome_usrEstado, ");
        //    sql.Append(" h.ind_entrega, ");
        //    sql.Append(" h.rol_atual_entrega ,");
        //    sql.Append(" r.nombre nome_usrEntrega, ");
        //    sql.Append(" h.motivo_corte, ");
        //    sql.Append(" t1.descripcion desc_motivoCorte, ");
        //    sql.Append(" h.numero_protocolo, ");
        //    sql.Append(" h.data_apto_corte ");
        //    sql.Append(" FROM ");
        //    sql.Append(" hisreav h, tabla T1,  outer tabla t, outer rol r, outer rol r2 ");
        //    sql.Append(" WHERE ");
        //    sql.Append(" h.numero_cliente = " + numeroCliente);
        //    sql.Append(" And t.nomtabla = 'ESTREA'  ");
        //    sql.Append(" AND t.sucursal = '0000' ");
        //    sql.Append(" And t.codigo = h.estado  ");
        //    sql.Append(" And T1.nomtabla = 'CORMOT' ");
        //    sql.Append(" AND T1.sucursal = '0000'  ");
        //    sql.Append(" And T1.codigo = h.motivo_corte ");
        //    sql.Append(" And r.rol = h.rol_atual_entrega ");
        //    sql.Append(" And r2.ROL = h.rol_estado");
        //    sql.Append(" and h.estado = 'V' ");
        //    sql.Append(" and h.data_apto_corte is not null ");
        //    sql.Append(" Order by ");
        //    sql.Append(" h.Corr_reaviso Desc ");

        //    DataTable dt = ConsultaSql(sql.ToString());

        //    foreach (DataRow item in dt.Rows)
        //    {
        //        HistoricoReaviso hisreavtemp = new HistoricoReaviso();

        //        hisreavtemp.CorrelativoReaviso = TratarShort(dt, item, "corr_reaviso", 0);
        //        hisreavtemp.DataEstado = TratarDateTime(dt, item, "data_estado");
        //        hisreavtemp.DataAptoCorte = TratarDateTime(dt, item, "data_apto_corte");
        //        hisreavtemp.DataReaviso = TratarDateTime(dt, item, "data_reaviso");
        //        hisreavtemp.DividaCliente = TratarFloat(dt, item, "divida_cliente", 0);
        //        hisreavtemp.EstadoReaviso = TratarString(dt, item, "cod_estado");
        //        hisreavtemp.DividaCliente = TratarChar(dt, item, "ind_entrega");
        //        hisreavtemp.MotivoCorte = TratarString(dt, item, "motivo_corte");
        //        hisreavtemp.NumeroCliente = numeroCliente;
        //        hisreavtemp.Protocolo = TratarInt(dt, item, "numero_protocolo", 0);
        //        hisreavtemp.ReferenciaFatura = TratarInt(dt, item, "referencia_fat", 0);
        //        hisreavtemp.TipoReaviso = hisreavtemp.ReferenciaFatura == 0 ? "Individual" : "Massivo";

        //        result.Add(hisreavtemp);
        //    }

        //    return result.ToArray();
        //}

        public bool IngressarSuspensaoCorte(int numeroCliente, int dias, string canalAtendimento, DBProviderInformix informix)
        {
            bool result = false;

            StringBuilder sql = new StringBuilder();
            sql.Append(" INSERT ");
            sql.Append(" INTO ");
            sql.Append(" corplazo ");
            sql.Append(" ( ");
            sql.Append(" tipo,  ");
            sql.Append(" sucursal, ");
            sql.Append(" rol, ");
            sql.Append(" motivo, ");
            sql.Append(" dias, ");
            sql.Append(" numero_cliente, ");
            sql.Append(" fecha_solicitud ");
            sql.Append(" ) ");
            sql.Append(" SELECT ");
            sql.Append(" 'D' AS tipo, ");
            sql.Append(" Sucursal, ");
            sql.AppendFormat(" '{0}' as rol ", canalAtendimento);
            sql.Append(" 'COMUNICACAO CONTA PAGA' as Motivo, ");
            sql.AppendFormat(" '{0}' as dias ", dias);
            sql.Append(" Numero_cliente, ");
            sql.Append(" TODAY as fecha_solicitud ");
            sql.Append(" FROM  ");
            sql.Append(" cliente ");
            sql.Append(" WHERE ");
            sql.AppendFormat(" numero_cliente = {0}", numeroCliente);
            result = ExecutarSql(sql.ToString(), informix);

            return result;
        }


        /// <summary>
        /// Ingressa um registro na tabela CLIENTE_A_RELIGAR
        /// </summary>
        /// <param name="cliente"></param>
        /// <returns></returns>
        public bool Ingressar(ClienteAReligar cliente, DBProviderInformix conn)
        {
            StringBuilder sql = new StringBuilder();
            sql.AppendFormat(" INSERT INTO cliente_a_religar (numero_cliente, data_processamento, data_emissao) VALUES ({0}, '{1}', '{2}')"
                , cliente.numero_cliente
                , cliente.data_processamento.Value.ToString("yyyy-MM-dd hh:mm")
                , cliente.data_emissao.Value.ToString("yyyy-MM-dd hh:mm"));

            return ExecutarSql(sql.ToString(), conn);
        }

        public bool Atualizar(ClienteDTO pCliente)
        {
            return Atualizar(pCliente, null);
        }

        public bool Atualizar(ClienteDTO pCliente, DBProviderInformix informix)
        {
            StringBuilder sql = new StringBuilder();
            sql.Append(" update cliente set ");
            //sql.AppendFormat(!String.IsNullOrWhiteSpace(pCliente.dv_numero_cliente) ? "dv_numero_cliente = '{0}' ," : "", pCliente.dv_numero_cliente);
            sql.AppendFormat(!String.IsNullOrWhiteSpace(pCliente.nombre)? "nombre = '{0}' ," : "", pCliente.nombre);
            sql.AppendFormat(!String.IsNullOrWhiteSpace(pCliente.direccion) ? "direccion = '{0}' ," : "", pCliente.direccion);
            sql.AppendFormat(!String.IsNullOrWhiteSpace(pCliente.soundex_dir) ? "soundex_dir = '{0}' ," : "", pCliente.soundex_dir);
            sql.AppendFormat(!String.IsNullOrWhiteSpace(pCliente.soundex_nombre) ? "soundex_nombre = '{0}' ," : "", pCliente.soundex_nombre);
            sql.AppendFormat(!String.IsNullOrWhiteSpace(pCliente.comuna) ? "comuna = '{0}' ," : "", pCliente.comuna);
            sql.AppendFormat(!String.IsNullOrWhiteSpace(pCliente.giro) ? "giro = '{0}' ," : "", pCliente.giro);
            sql.AppendFormat(!String.IsNullOrWhiteSpace(pCliente.telefono) ? "telefono = '{0}' ," : "", pCliente.telefono);
            sql.AppendFormat(!String.IsNullOrWhiteSpace(pCliente.tipo_ident) ? "tipo_ident = '{0}' ," : "", pCliente.tipo_ident);
            sql.AppendFormat(!String.IsNullOrWhiteSpace(pCliente.rut) ? "rut = '{0}' ," : "", pCliente.rut);
            sql.AppendFormat(!String.IsNullOrWhiteSpace(pCliente.dv_rut) ? "dv_rut = '{0}' ," : "", pCliente.dv_rut);
            sql.AppendFormat(!String.IsNullOrWhiteSpace(pCliente.empresa) ? "empresa = '{0}' ," : "", pCliente.empresa);
            sql.AppendFormat(!String.IsNullOrWhiteSpace(pCliente.religiao) ? "religiao = '{0}' ," : "", pCliente.religiao);
            sql.AppendFormat(!String.IsNullOrWhiteSpace(pCliente.escolaridade) ? "escolaridade = '{0}' ," : "", pCliente.escolaridade);
            sql.AppendFormat(!String.IsNullOrWhiteSpace(pCliente.nome_completo) ? "nome_completo = '{0}' ," : "", pCliente.nome_completo);
            sql.AppendFormat(!String.IsNullOrWhiteSpace(pCliente.qtd_pessoas_uc) ? "qtd_pessoas_uc = '{0}' ," : "", pCliente.qtd_pessoas_uc);
            sql.AppendFormat(!String.IsNullOrWhiteSpace(pCliente.sexo) ? "sexo = '{0}' ," : "", pCliente.sexo);
            sql.AppendFormat(!String.IsNullOrWhiteSpace(pCliente.tempo_atuacao) ? "tempo_atuacao = '{0}' ," : "", pCliente.tempo_atuacao);
            sql.AppendFormat(!String.IsNullOrWhiteSpace(pCliente.qtd_funcionarios) ? "qtd_funcionarios = '{0}' ," : "", pCliente.qtd_funcionarios);
            sql.AppendFormat(!String.IsNullOrWhiteSpace(pCliente.domicilio) ? "domicilio = '{0}' ," : "", pCliente.domicilio);
            sql.AppendFormat(!String.IsNullOrWhiteSpace(pCliente.qtd_filhos) ? "qtd_filhos = '{0}' ," : "", pCliente.qtd_filhos);
            sql.AppendFormat(!String.IsNullOrWhiteSpace(pCliente.estado_civil) ? "estado_civil = '{0}' ," : "", pCliente.estado_civil);
            sql.AppendFormat(!String.IsNullOrWhiteSpace(pCliente.profissao) ? "profissao = '{0}' ," : "", pCliente.profissao);
            sql.AppendFormat(!String.IsNullOrWhiteSpace(pCliente.dica_localizacao) ? "dica_localizacao = '{0}' ," : "", pCliente.dica_localizacao);
            sql.AppendFormat(!String.IsNullOrWhiteSpace(pCliente.celular) ? "celular = '{0}' ," : "", pCliente.celular);
            sql.AppendFormat(!String.IsNullOrWhiteSpace(pCliente.estado_suministro) ? "estado_suministro = '{0}' ," : "", pCliente.estado_suministro);
            sql.AppendFormat(!String.IsNullOrWhiteSpace(pCliente.estado_cliente) ? "estado_cliente = '{0}' ," : "", pCliente.estado_cliente);
            sql.AppendFormat(!String.IsNullOrWhiteSpace(pCliente.estado_facturacion) ? "estado_facturacion = '{0}' ," : "", pCliente.estado_facturacion);
            sql.AppendFormat(!String.IsNullOrWhiteSpace(pCliente.ind_baixarenda) ? "ind_baixarenda = '{0}' ," : "", pCliente.ind_baixarenda);
            sql.AppendFormat(!String.IsNullOrWhiteSpace(pCliente.ind_ucbaixarenda) ? "ind_ucbaixarenda = '{0}' ," : "", pCliente.ind_ucbaixarenda);
            sql.AppendFormat(!String.IsNullOrWhiteSpace(pCliente.ind_ilumpublica) ? "ind_ilumpublica = '{0}' ," : "", pCliente.ind_ilumpublica);
            sql.AppendFormat(!String.IsNullOrWhiteSpace(pCliente.numero_nis) ? "numero_nis = '{0}' ," : "", pCliente.numero_nis);
            sql.AppendFormat(!String.IsNullOrWhiteSpace(pCliente.ind_conv_gov) ? "ind_conv_gov = '{0}' ," : "", pCliente.ind_conv_gov);
            sql.AppendFormat(!String.IsNullOrWhiteSpace(pCliente.tiene_postal) ? "tiene_postal = '{0}' ," : "", pCliente.tiene_postal);
            sql.AppendFormat(!String.IsNullOrWhiteSpace(pCliente.cantidad_medidores) ? "cantidad_medidores = '{0}' ," : "", pCliente.cantidad_medidores);
            sql.AppendFormat(!String.IsNullOrWhiteSpace(pCliente.tiene_notif) ? "tiene_notif = '{0}' ," : "", pCliente.tiene_notif);
            sql.AppendFormat(!String.IsNullOrWhiteSpace(pCliente.ddd) ? "ddd = '{0}' ," : "", pCliente.ddd);
            sql.AppendFormat(!String.IsNullOrWhiteSpace(pCliente.ddd2) ? "ddd2 = '{0}' ," : "", pCliente.ddd2);
            sql.AppendFormat(!String.IsNullOrWhiteSpace(pCliente.telefono2) ? "telefono2 = '{0}' ," : "", pCliente.telefono2);
            sql.AppendFormat(!String.IsNullOrWhiteSpace(pCliente.ramal) ? "ramal = '{0}' ," : "", pCliente.ramal);
            sql.AppendFormat(!String.IsNullOrWhiteSpace(pCliente.ramal2) ? "ramal2 = '{0}' ," : "", pCliente.ramal2);
            sql.AppendFormat(!String.IsNullOrWhiteSpace(pCliente.tipo_ident2) ? "tipo_ident2 = '{0}' ," : "", pCliente.tipo_ident2);
            sql.AppendFormat(!String.IsNullOrWhiteSpace(pCliente.documento2) ? "documento2 = '{0}' ," : "", pCliente.documento2);
            sql.AppendFormat(!String.IsNullOrWhiteSpace(pCliente.dv_docu2) ? "dv_docu2 = '{0}' ," : "", pCliente.dv_docu2);

            sql.Remove(sql.Length - 1, 1);

            sql.AppendFormat("where numero_cliente = {0} ", pCliente.numero_cliente);

            if (informix == null)
                return ExecutarSql(sql.ToString()) > 0;
            else
                return ExecutarSql(sql.ToString(), informix);
        }
        public bool AtualizarGrandesClientes(ClienteDTO pCliente, DBProviderInformix informix)
        {
            StringBuilder sql = new StringBuilder();
            sql.Append(" update cliente set ");
            //sql.AppendFormat(!String.IsNullOrWhiteSpace(pCliente.dv_numero_cliente) ? "dv_numero_cliente = '{0}' ," : "", pCliente.dv_numero_cliente);
            sql.AppendFormat(!String.IsNullOrWhiteSpace(pCliente.nombre) ? "nombre = '{0}' ," : "", pCliente.nombre);
            sql.AppendFormat(!String.IsNullOrWhiteSpace(pCliente.direccion) ? "direccion = '{0}' ," : "", pCliente.direccion);
            sql.AppendFormat(!String.IsNullOrWhiteSpace(pCliente.comuna) ? "comuna = '{0}' ," : "", pCliente.comuna);
            sql.AppendFormat(!String.IsNullOrWhiteSpace(pCliente.giro) ? "giro = '{0}' ," : "", pCliente.giro);
            sql.AppendFormat(!String.IsNullOrWhiteSpace(pCliente.telefono) ? "telefono = '{0}' ," : "", pCliente.telefono);
            sql.AppendFormat(!String.IsNullOrWhiteSpace(pCliente.empresa) ? "empresa = '{0}' ," : "", pCliente.empresa);
            sql.AppendFormat(!String.IsNullOrWhiteSpace(pCliente.celular) ? "celular = '{0}' ," : "", pCliente.celular);
            
            sql.Remove(sql.Length - 1, 1);

            sql.AppendFormat("where numero_cliente = {0} ", pCliente.numero_cliente);

            if (informix == null)
                return ExecutarSql(sql.ToString()) > 0;
            else
                return ExecutarSql(sql.ToString(), informix);
        }

        public bool ExcluirDocumentoPorCliente(string pNumeroCliente)
        {
            StringBuilder sql = new StringBuilder();
            sql.AppendFormat(" DELETE FROM DOCUMENTO_CLIENTE WHERE NUMERO_CLIENTE = {0} ", pNumeroCliente);

            return ExecutarSql(sql.ToString()) > 0;
        }

        public bool ExcluirDocumentoPorClienteETipo(string pNumeroCliente, string pTipoDocumento)
        {
            String sql = string.Empty;
            sql = String.Format(@"DELETE FROM DOCUMENTO_CLIENTE WHERE NUMERO_CLIENTE = {0} 
                            AND TIPO_DOCUMENTO = {1}", pNumeroCliente, pTipoDocumento);

            DBProviderInformix informix = ObterProviderInformix();
            return ExecutarSql(sql.ToString(), informix);
        }

        public bool ExcluirTelefonePorCliente(string pNumeroCliente)
        {
            StringBuilder sql = new StringBuilder();
            sql.AppendFormat(" DELETE FROM TELEFONE_CLIENTE WHERE NUMERO_CLIENTE = {0} ", pNumeroCliente);

            DBProviderInformix informix = ObterProviderInformix();
            return ExecutarSql(sql.ToString(), informix);
        }

        public bool ExcluirTelefoneContatoPorCliente(string pNumeroCliente)
        {
            StringBuilder sql = new StringBuilder();
            sql.Append(" DELETE FROM TELEFONE_CLIENTE WHERE NUMERO_CLIENTE = ");
            sql.AppendFormat(" {0} and ind_contato = 'S'", pNumeroCliente);

            DBProviderInformix informix = ObterProviderInformix();
            return ExecutarSql(sql.ToString(), informix);
        }

        public bool InserirTelefonePorCliente(TelefoneDTO telefone)
        {
            StringBuilder sql = new StringBuilder();
            sql.Append(" INSERT INTO TELEFONE_CLIENTE  VALUES ( ");
            sql.AppendFormat("'{0}', '{1}', '{2}', '{3}', '{4}', '{5}')", telefone.numero_cliente, "1", telefone.prefixo_ddd, telefone.numero_telefone, telefone.ind_contato, telefone.ramal);

            sql = sql.Replace("'NULL'", "NULL");
            DBProviderInformix informix = ObterProviderInformix();
            return ExecutarSql(sql.ToString(), informix);
        }
        public bool InserirTelefonePorCliente(TelefoneDTO telefone, DBProviderInformix informix)
        {
            StringBuilder sql = new StringBuilder();
            sql.Append(" INSERT INTO TELEFONE_CLIENTE  VALUES ( ");
            sql.AppendFormat("'{0}', '{1}', '{2}', '{3}', '{4}', '{5}')", telefone.numero_cliente, telefone.tipo_telefone, telefone.prefixo_ddd, telefone.numero_telefone, telefone.ind_contato, telefone.ramal);

            sql = sql.Replace("'NULL'", "NULL");
            return ExecutarSql(sql.ToString(), informix);
        }


        public bool InserirDocumentoPorCliente(DocumentoDTO documento, DBProviderInformix informix)
        {
            StringBuilder sql = new StringBuilder();
            sql.Append(" INSERT INTO DOCUMENTO_CLIENTE VALUES ( ");
            sql.AppendFormat(" '{0}', '{1}', '{2}', '{3}', '{4}', '{5}', '{6}', '{7}', '{8}', '{9}')", documento.numero_cliente, documento.tipo_documento, documento.numero_doc, documento.dv_documento, documento.compl_documento, documento.data_emissao, documento.sequencial, documento.uf, documento.foto, documento.valida_org_emis);

            sql = sql.Replace("'NULL'", "NULL");

            
            return ExecutarSql(sql.ToString(), informix);
        }

        public bool InserirDocumentoPorCliente(DocumentoDTO documento)
        {
            StringBuilder sql = new StringBuilder();
            sql.Append(" INSERT INTO DOCUMENTO_CLIENTE VALUES ( ");
            sql.AppendFormat(" '{0}', '{1}', '{2}', '{3}', '{4}', '{5}', '{6}', '{7}', '{8}', '{9}')", documento.numero_cliente, documento.tipo_documento, documento.numero_doc, documento.dv_documento, documento.compl_documento, documento.data_emissao, documento.sequencial, documento.uf, documento.foto, documento.valida_org_emis);

            sql = sql.Replace("'NULL'", "NULL");
            DBProviderInformix informix = ObterProviderInformix();

            return ExecutarSql(sql.ToString(), informix);
        }


        public bool AtualizarDataAptoCorte(DateTime dataAptoCorte, short correlativoReaviso, int numeroCliente, int dias, DBProviderInformix informix)
        {
            bool result = false;

            string dataApto = dataAptoCorte.AddDays(dias).ToString("MM/dd/yyyy");

            StringBuilder sqlHisRea = new StringBuilder();
            sqlHisRea.Append(" UPDATE hisreav ");
            sqlHisRea.AppendFormat(" set data_apto_corte = '{0}', estado = '{1}' ", dataApto, "S");
            sqlHisRea.Append(" WHERE ");
            sqlHisRea.AppendFormat(" numero_cliente = {0} ", numeroCliente);
            sqlHisRea.AppendFormat(" and corr_reaviso = {0} ", correlativoReaviso);

            StringBuilder sqlCliente = new StringBuilder();
            sqlCliente.Append(" UPDATE cliente ");
            sqlCliente.AppendFormat(" set fecha_a_corte = '{0}' ", dataApto);
            sqlCliente.AppendFormat(" WHERE numero_cliente = {0} ", numeroCliente);

            result = ExecutarSql(sqlHisRea.ToString(), informix);

            if (result)
            {
                result = ExecutarSql(sqlCliente.ToString(), informix);
            }

            return result;
        }

        public string DataTableToJSON(DataTable table)
        {
            var list = new List<Dictionary<string, object>>();

            foreach (DataRow row in table.Rows)
            {
                var dict = new Dictionary<string, object>();

                foreach (DataColumn col in table.Columns)
                {
                    dict[col.ColumnName] = (Convert.ToString(row[col]));
                }
                list.Add(dict);
            }

            JavaScriptSerializer serializer = new JavaScriptSerializer();

            serializer.MaxJsonLength = Int32.MaxValue;

            return serializer.Serialize(list);
        }

        public bool VerificaCliente(int numero_cliente)
        {
            string sql = string.Format("select * from cliente where numero_cliente = {0} ", numero_cliente);

            DataTable dtResultado = ConsultaSql(sql);

            if (dtResultado.Rows.Count > 0)
                return true;
            else
                return false;
        }
        public int VerificaEstadoCliente(int numero_cliente)
        {
            string sql = string.Format(@"select first 1 estado_cliente from cliente where numero_cliente = {0} ", numero_cliente);


            DataTable dtResultado = ConsultaSql(sql);

            int estado_cliente = 0;

            if (dtResultado.Rows.Count > 0)
                estado_cliente = TratarInt(dtResultado, dtResultado.Rows[0], "estado_cliente", 0);


            return estado_cliente;
        }
        public DataTable Consultar(Cliente pCliente)
        {
            if (pCliente == null)
                return new DataTable();

            bool filtroAtivado = false;

            StringBuilder sql = new StringBuilder(string.Format("select * from cliente where 1=1 "));

            if (!string.IsNullOrEmpty(pCliente.Numero_cliente))
            {
                sql.AppendFormat("AND numero_cliente = {0} ", pCliente.Numero_cliente);
                filtroAtivado = true;
            }

            if (!string.IsNullOrEmpty(pCliente.Cliente_anterior))
            { 
                sql.AppendFormat("AND cliente_anterior = {0} ", pCliente.Cliente_anterior);
                filtroAtivado = true;
            }

            if (!string.IsNullOrEmpty(pCliente.Estado_cliente))
            { 
                sql.AppendFormat("AND estado_cliente in ({0}) ", pCliente.Estado_cliente);
                filtroAtivado = true;
            }

            if (!string.IsNullOrEmpty(pCliente.Estado_suministro))
            { 
                sql.AppendFormat("AND estado_suministro = {0} ", pCliente.Estado_suministro);
                filtroAtivado = true;
            }

            return filtroAtivado ? ConsultaSql(sql.ToString()) : new DataTable();
        }

        /// <summary>
        /// Retorna um DataTable contendo clientes aptos à religação, configurados fora do período válido para o serviço de religação.
        /// </summary>
        /// <param name="pCliente"></param>
        /// <returns></returns>
        public DataTable Consultar(ClienteAReligar pCliente)
        {
            if (pCliente == null)
                return new DataTable();

            StringBuilder sql = new StringBuilder(string.Format("select * from cliente_a_religar where 1=1 "));

            if (!string.IsNullOrEmpty(pCliente.numero_cliente))
                sql.AppendFormat("AND numero_cliente = {0} ", pCliente.numero_cliente);

            if (!string.IsNullOrEmpty(pCliente.data_emissao_operador))
                sql.AppendFormat("AND data_emissao {0} {1} ", pCliente.data_emissao_operador, pCliente.data_emissao.HasValue ? string.Format("'{0}'", pCliente.data_emissao.Value.ToString("yyyy-MM-dd HH:mm:ss")) : "null");

            if (pCliente.data_emissao_min.HasValue && pCliente.data_emissao_min.Value > DateTime.MinValue)
                sql.AppendFormat("AND data_emissao > '{0}' ", pCliente.data_emissao_min.Value.ToString("yyyy-MM-dd HH:mm:ss"));

            if (pCliente.data_emissao_max.HasValue && pCliente.data_emissao_max.Value < DateTime.MaxValue)
                sql.AppendFormat("AND data_emissao < '{0}' ", pCliente.data_emissao_max.Value.ToString("yyyy-MM-dd HH:mm:ss"));

            return ConsultaSql(sql.ToString());
        }

        public ContratoDTO BuscarDadosCliente(ContratoDTO contrato)
        {
            String sql = string.Format(@"	SELECT  cliente.numero_cliente,	      
													t.DESCRIPCION, 
													cliente.nombre,
													cliente.dv_rut,
													cliente.rut,             
													cliente.tipo_ident,         
													cliente.correlativo_ruta,                
													cliente.dv_ruta_lectura, 
													cliente.giro,               
													cliente.nome_mae,           
													cliente.mail,               
													cliente.escolaridade,       
													cliente.profissao,          
													cliente.sexo,               
													cliente.estado_civil,       
													cliente.religiao,           
													cliente.nome_completo,      
													cliente.qtd_pessoas_uc,     
													cliente.qtd_funcionarios,   
													cliente.tempo_atuacao,      
													cliente.uf_nascimento,      
													cliente.data_nasc,          
													cliente.subclasse_orig,     
													cliente.ind_ucbaixarenda,   
													cliente.ind_baixarenda,     
													cliente.estado_cliente,
													cliente.ind_bloqueio_norm,
                                                    cliente.sucursal                
											   FROM cliente, 
													tabla t          
											   WHERE cliente.numero_cliente= {0}
												  AND t.nomtabla = 'DESPER'
												  AND t.codigo = '01'
												  AND t.sucursal = '0000'", contrato.numero_cliente);
            var dt = ConsultaSql(sql.ToString());
            if (dt.Rows.Count > 0)
            {
                ContratoDTO resultado = DataHelper.ConvertDataTableToEntity<ContratoDTO>(dt);

                return (ContratoDTO)resultado;
            }
            else
                return null;
        }

        public bool AtualizarDespersonalizaCliente(ContratoDTO contrato, DBProviderInformix informix)
        {
            string sql = string.Format(@"UPDATE CLIENTE 
											 SET Nombre = '{0}' 
											  , Tipo_Ident  =  ''
											  , Tipo_Ident2 =  ''
											  , Rut         =  ''
											  , Documento2  =  ''
											  , dv_rut      =  ''
											  , dv_docu2    =  ''
											  , estado_docu =  ''
											  , comp_docu   =  ''
											  , dia_vencimento  =  '0'
											  , ind_cli_despersona = 'S' 
											  , tiene_postal        ='N' 
											  , Telefono = ''
											  , ddd   = ''
											  , ramal = ''
											  , telefono2 = ''
											  , ddd2 = ''
											  , ramal2 = ''
											  , data_nasc = null 
											  , ind_pessoa = null 
											  , religiao = null 
											  , escolaridade = null 
											  , nome_completo = null 
											  , qtd_pessoas_uc = null 
											  , qtd_filhos = null 
											  , domicilio = null 
											  , sexo = null 
											  , estado_civil = null 
											  , profissao = null 
											  , uf_nascimento = null 
											  , data_nasc = null 
											  , tempo_atuacao = null 
											  , qtd_funcionarios = null 
											  , giro = null 
											  , nome_mae = null 
											 ,ind_calc_media = 'N' 
											 ,ind_conv_gov = 'N' 
											 ,media_movel = 0 
											 ,subclasse = '01' 
											 ,ind_baixarenda = 'N' 
											 ,refer_lido_br = year(today)||to_char(today, '%m')
											 ,ind_ucbaixarenda = null 
											 ,numero_nis = null 
											 ,estado_cliente = 0 
											WHERE numero_cliente = {1}", contrato.nombre, contrato.numero_cliente);

            return ExecutarSql(sql.ToString(), informix);

        }

        public bool delete_documento_cliente(ContratoDTO contrato, DBProviderInformix informix)
        {
            string sql = string.Format(@"delete  from documento_cliente where numero_cliente = {0}", contrato.numero_cliente);

            return ExecutarSql(sql.ToString(), informix);
        }

        public bool delete_telefone_cliente(ContratoDTO contrato, DBProviderInformix informix)
        {
            string sql = string.Format(@"delete  from telefone_cliente where numero_cliente = {0}", contrato.numero_cliente);

            return ExecutarSql(sql.ToString(), informix);
        }

        public bool delete_cliente_post(ContratoDTO contrato, DBProviderInformix informix)
        {
            string sql = string.Format(@"delete  from cliente_post where numero_cliente = {0}", contrato.numero_cliente);

            return ExecutarSql(sql.ToString(), informix);
        }

        public int selectcountMaeaut(ContratoDTO contrato)
        {
            int resultado = 0;
            string sql = string.Format(@"select count(*) as total from maeaut WHERE data_exclusion is null and numero_cliente = {0}", contrato.numero_cliente);

            var dt = ConsultaSql(sql.ToString());
            if (dt.Rows.Count > 0)
            {
                resultado = Convert.ToInt32(dt.Rows[0]["total"]);
            }

            return resultado;
        }

        public bool AtualizaMaeaut(ContratoDTO contrato, DBProviderInformix informix)
        {
            string sql = string.Format(@"Update maeaut  Set
										data_exclusion = today , estado = 'E', motivo_exclusion = '98' 
										WHERE numero_cliente = {0}
										and data_exclusion is null;", contrato.numero_cliente);

            return ExecutarSql(sql.ToString(), informix);
        }

        public bool InsertSeguroOcorr(int numero_cliente)
        {
            DBProviderInformix informix = ObterProviderInformix();
            string sql = string.Format(@"INSERT INTO seguro_ocorr(Numero_Cliente,Codigo_Ocorr,data_inclusao) VALUES ({0},'01',today)", numero_cliente);

            return ExecutarSql(sql.ToString(), informix);
        }

        #region CLIENTE NOVO AMPLA
        public bool IngressarclienteNovoAmpla(ContratoDTO clienteNovo, DBProviderInformix informix)
        {

            StringBuilder sql = new StringBuilder();
            sql.Append(@" insert into clientes_prevenda (numero_ordem,
                          nome,
                          ddd,
                          telefone,
                          ddd2,
                          telefone2,
                          cod_doc,
                          num_doc,
                          dv_doc,
                          municipio,
                          bairro,
                          cod_logra,
                          nome_logra,
                          num_imovel,
                          complemento,
                          mail,
                          bairro_postal,
                          municipio_postal,
                          estado,
                          cep_postal,
                          cobro_postal,
                          direccion_postal,
                          tipo_ident2,
                          tipo_pessoa,
                          ramal,
                          ramal2,
                          latitudeEndVizinho,
                          longitudeEndVizinho,
                          documento2,
                          dv_docu2,
                          cadastro_br,
                          proprio_br,
                          nome_benef_br,
                          grau_parent_br,
                          numero_nis,
                          numero_nb,
                          numero_nit,
                          uf_nasc_br,
                          data_nasc_br,
                          tipo_br,
                          tipo_ident_br,
                          rut_br,
                          dv_rut_br,
                          tipo_ident2_br,
                          rut2_br,
                          dv_rut2_br,
                          giro,
                          tipo_ligacao,
                          classe,
                          subclasse,
                          tarifa,
                          estado_civil,
                          ind_cad_conjuge,
                          nome_conjuge,
                          tipo_doc_conjuge,
                          documento_conjuge,
                          dv_documento_conjuge,
                          ind_aut_email,
                          numero_cliente,
                          pot_inst_kw
                          )values ( ");
            sql.AppendFormat(" '{0}','{1}','{2}','{3}','{4}','{5}','{6}','{7}','{8}','{9}','{10}','{11}','{12}','{13}','{14}','{15}','{16}','{17}','{18}','{19}','{20}','{21}','{22}','{23}','{24}','{25}','{26}','{27}','{28}','{29}','{30}','{31}','{32}','{33}','{34}','{35}','{36}','{37}','{38}','{39}','{40}','{41}','{42}','{43}','{44}','{45}','{46}','{47}','{48}','{49}','{50}','{51}','{52}','{53}','{54}','{55}','{56}','{57}','{58}','{59}'); ", clienteNovo.numero_ordem,
clienteNovo.nome,
clienteNovo.ddd,
clienteNovo.telefono,
clienteNovo.ddd2,
clienteNovo.telefono2,
clienteNovo.cod_doc,
clienteNovo.num_doc,
clienteNovo.dv_doc,
clienteNovo.municipio,
clienteNovo.bairro,
clienteNovo.cod_logra,
clienteNovo.nome_logra,
clienteNovo.num_imovel,
clienteNovo.complemento,
clienteNovo.mail,
clienteNovo.bairro_postal,
clienteNovo.municipio_postal,
clienteNovo.estado,
clienteNovo.cep_postal,
clienteNovo.cobro_postal,
clienteNovo.direccion_postal,
clienteNovo.tipo_ident2,
clienteNovo.tipo_pessoa,
clienteNovo.ramal,
clienteNovo.ramal2,
clienteNovo.latitudeEndVizinho,
clienteNovo.longitudeEndVizinho,
clienteNovo.documento2,
clienteNovo.dv_docu2,
clienteNovo.cadastro_br,
clienteNovo.proprio_br,
clienteNovo.nome_benef_br,
clienteNovo.grau_parent_br,
clienteNovo.numero_nis,
clienteNovo.numero_nb,
clienteNovo.numero_nit,
clienteNovo.uf_nasc_br,
clienteNovo.data_nasc_br,
clienteNovo.tipo_br,
clienteNovo.tipo_ident_br,
clienteNovo.rut_br,
clienteNovo.dv_rut_br,
clienteNovo.tipo_ident2_br,
clienteNovo.rut2_br,
clienteNovo.dv_rut2_br,
clienteNovo.giro,
clienteNovo.tipo_ligacao,
clienteNovo.classe,
clienteNovo.subclasse,
clienteNovo.tarifa,
clienteNovo.estado_civil,
clienteNovo.ind_cad_conjuge,
clienteNovo.nome_conjuge,
clienteNovo.tipo_doc_conjuge,
clienteNovo.documento_conjuge,
clienteNovo.dv_documento_conjuge,
clienteNovo.ind_aut_email,
clienteNovo.numero_cliente,clienteNovo.potencia);

            sql.Replace("'NULL'", "NULL");


            return ExecutarSql(sql.ToString(), informix);
        }

        #endregion

        #region CLIENTE NOVO COELCE
        public bool IngressarClienteNovoCoelce(ContratoDTO clienteNovo, DBProviderInformix informix)
        {

            StringBuilder sql = new StringBuilder();
            sql.Append(@" insert into CLIENTE_NOVO(numero_cliente,
                            dv_numero_cliente,
                            numero_ordem  ,
                            sucursal      ,
                            tipo_contrato ,
                            numero_contrato,
                            fecha_contrato,
                            nombre        ,
                            nome_mae      ,       
                            tipo_ident    ,
                            rut           ,
                            dv_rut        ,
                            direccion_multiple,
                            tipo_cliente  ,
                            fecha_creacion,
                            rol_creacion  ,
                            telefono      ,
                            dia_vencimento,    
                            giro          ,
                            tiene_postal  ,
                            tipo_morador  ,
                            ddd           ,
                            ramal         ,
                            municipio     ,
                            Classe        ,
                            ddd2          ,
                            telefono2     ,
                            estado_docu   ,
                            comp_docu     ,
                            ind_baixarenda,
                            ind_conv_gov  ,
                            numero_NIS    ,
                            tipo_ident2   ,
                            documento2    ,
                            dv_docu2      ,
                            codigo_logra  ,
                            direccion     ,
                            numero_casa   ,
                            complemento   ,
                            sector        ,
                            zona          ,
                             localidade    ,
                            comuna        ,
                            coordenadas_eura,
                            cep           ,
                            pot_inst_kw   ,
                            tipo_ligacao  ,
                            subclasse     ,
                            classe_atend  ,
                            subclasse_atend,
                            construcao_padrao,
                            tipo_parcelamento,
                            saldo_inicial ,
                            tiene_convenio,
                            numero_cuotas ,
                            valor_cuota   ,
                            intereses     ,
                            Ind_Poder_Publico,
                            bairro_solicitacao,
                            dica_localizacao,
                            mail          ,
                            cliente_iphan ,
                             cliente_zona_urbana,
                            cadastro_br,
                            proprio_br,
                            nome_benef_br,
                            grau_parent_br,
                            numero_nis,
                            numero_nb,
                            numero_nit,
                            uf_nasc_br,
                            data_nasc_br,
                            tipo_br,
                            tipo_ident_br,
                            rut_br,
                            dv_rut_br,
                            tipo_ident2_br,
                            rut2_br,
                            dv_rut2_br,
                            estado_civil, 
                             nome_conjuge, 
                             tipo_doc_conjuge, 
                             documento_conjuge, 
                             dv_documento_conjuge, 
                             ind_aut_email,
                             ind_cad_conjuge,
                             ind_pessoa,
                             religiao,
                             escolaridade,
                             nome_completo,
                             qtd_pessoas_uc,
                             qtd_filhos,
                             domicilio,
                             sexo,
                             profissao,
                             uf_nascimento,
                             data_nasc,
                             tempo_atuacao,
                             qtd_funcionarios
                            ) values (");
            sql.AppendFormat(" '{0}','{1}','{2}','{3}','{4}','{5}','{6}','{7}','{8}','{9}','{10}','{11}','{12}','{13}','{14}','{15}','{16}','{17}','{18}','{19}','{20}','{21}','{22}','{23}','{24}','{25}','{26}','{27}','{28}','{29}','{30}','{31}','{32}','{33}','{34}','{35}','{36}','{37}','{38}','{39}','{40}','{41}','{42}','{43}','{44}','{45}','{46}','{47}','{48}','{49}','{50}','{51}','{52}','{53}','{54}','{55}','{56}','{57}','{58}','{59}','{60}','{61}','{62}','{63}','{64}','{65}','{66}','{67}','{68}','{69}','{70}','{71}','{72}','{73}','{74}','{75}','{76}','{77}','{78}','{79}','{80}','{81}','{82}','{83}','{84}','{85}','{86}','{87}','{88}','{89}','{90}','{91}','{92}','{93}','{94}','{95}','{96}','{97}','{98}'", clienteNovo.numero_cliente,
                    clienteNovo.numero_ordem,
                    clienteNovo.sucursal,
                    clienteNovo.tipo_contrato,
                    clienteNovo.numero_contrato,
                    clienteNovo.fecha_contrato,
                    clienteNovo.nombre,
                    clienteNovo.nome_mae,
                    clienteNovo.tipo_ident,
                    clienteNovo.rut,
                    clienteNovo.dv_rut,
                    clienteNovo.direccion_multiple,
                        clienteNovo.tipo_cliente,
                    clienteNovo.fecha_creacion,
                    clienteNovo.rol_creacion,
                    clienteNovo.telefono,
                    clienteNovo.dia_vencimento,
                    clienteNovo.giro,
                    clienteNovo.tiene_postal,
                    clienteNovo.tipo_morador,
                    clienteNovo.ddd,
                    clienteNovo.ramal,
                    clienteNovo.municipio,
                    clienteNovo.classe,
                    clienteNovo.ddd2,
                    clienteNovo.telefono2,
                    clienteNovo.estado_docu,
                    clienteNovo.comp_docu,
                    clienteNovo.ind_baixarenda,
                    clienteNovo.ind_conv_gov,
                    clienteNovo.numero_nis,
                    clienteNovo.tipo_ident2,
                    clienteNovo.documento2,
                    clienteNovo.dv_docu2,
                    clienteNovo.codigo_logra,
                    clienteNovo.direccion,
                    clienteNovo.numero_casa,
                    clienteNovo.complemento,
                    clienteNovo.sector,
                    clienteNovo.zona,
                    clienteNovo.localidade,
                    clienteNovo.comuna,
                    clienteNovo.coordenadas_eura,
                    clienteNovo.cep,
                    clienteNovo.potencia,
                    clienteNovo.tipo_ligacao,
                    clienteNovo.subclasse,
                    clienteNovo.classe_atend,
                    clienteNovo.subclasse_atend,
                    clienteNovo.construcao_padrao,
                    clienteNovo.tipo_parcelamento,
                    clienteNovo.saldo_inicial,
                    clienteNovo.tiene_convenio,
                    clienteNovo.numero_cuotas,
                    clienteNovo.valor_cuota,
                    clienteNovo.intereses,
                    clienteNovo.Ind_Poder_Publico,
                    clienteNovo.bairro_solicitacao,
                    clienteNovo.dica_localizacao,
                    clienteNovo.mail,
                    clienteNovo.cliente_iphan,
                    clienteNovo.cliente_zona_urbana,
                    clienteNovo.cadastro_br,
                    clienteNovo.proprio_br,
                    clienteNovo.nome_benef_br,
                    clienteNovo.grau_parent_br,
                    clienteNovo.numero_nis,
                    clienteNovo.numero_nb,
                    clienteNovo.numero_nit,
                    clienteNovo.uf_nasc_br,
                    clienteNovo.data_nasc_br,
                    clienteNovo.tipo_br,
                    clienteNovo.tipo_ident_br,
                    clienteNovo.rut_br,
                    clienteNovo.dv_rut_br,
                    clienteNovo.tipo_ident2_br,
                    clienteNovo.rut2_br,
                    clienteNovo.dv_rut2_br,
                    clienteNovo.estado_civil,
                    clienteNovo.nome_conjuge,
                    clienteNovo.tipo_doc_conjuge,
                    clienteNovo.documento_conjuge,
                    clienteNovo.dv_documento_conjuge,
                    clienteNovo.ind_aut_email,
                    clienteNovo.ind_cad_conjuge,
                    clienteNovo.ind_pessoa,
                    clienteNovo.religiao,
                    clienteNovo.escolaridade,
                    clienteNovo.nome_completo,
                    clienteNovo.qtd_pessoas_uc,
                    clienteNovo.qtd_filhos,
                    clienteNovo.domicilio,
                    clienteNovo.sexo,
                    clienteNovo.profissao,
                    clienteNovo.uf_nascimento,
                    clienteNovo.data_nasc,
                    clienteNovo.tempo_atuacao,
                    clienteNovo.qtd_funcionarios
                    );

            sql = sql.Replace("'NULL'", "NULL");

            return ExecutarSql(sql.ToString(), informix);
        }

        #endregion 


        public bool IngressarClienteNovoGrupoA(ContratoDTO clienteNovo, DBProviderInformix informix)
        {

            StringBuilder sql = new StringBuilder();
            sql.Append(@"insert into cliente_novo( 
              numero_cliente,
              dv_numero_cliente,
              sucursal,
              numero_ordem,
              nombre,
              tipo_ident,
              rut,
              dv_rut,
              atv_economica,
              direccion,
              telefono,
              dem_ctr_hp_seco,
              dem_ctr_fp_seco,
              dem_ctr_hr_seco ,
              dem_ctr_hp_umido,
              dem_ctr_fp_umido,
              dem_ctr_hr_umido,
              carga_inst_kva,
              voltaje_solicitado,
              data_criacao,
              tiene_postal,
              rol_creacion,
              municipio,
              dddfone,
              classe,
              subclasse,
              fax,
              dddfax,
              contato_cli,
              tipo_ident2,
              documento2,
              dv_docu2,
              numero_casa,
              direccion_postal,
              bairro_postal,
              municipio_postal,
              unidade_federacao,
              area_concessao,
              cep_postal,
              cobro_postal,
              optante_bt,
              contato_fone,
              localidade,
              tipo_ligacao,
              cep,
              dddcontato,
              crea_tecnico,
              potencia_trafo,
              hora_func_emp_ini_1,
              hora_func_emp_fim_1,
              hora_func_emp_ini_2,
              hora_func_emp_fim_2,
             acteco_com,
             ejecutivo,
             insc_estadual,
             cliente_zona_urbana ) values (");
            sql.AppendFormat(" '{0}','{1}','{2}','{3}','{4}','{5}','{6}','{7}','{8}','{9}','{10}','{11}','{12}','{13}','{14}','{15}','{16}','{17}','{18}','{19}','{20}','{21}','{22}','{23}','{24}','{25}','{26}','{27}','{28}','{29}','{30}','{31}','{32}','{33}','{34}','{35}','{36}','{37}','{38}','{39}','{40}','{41}','{42}','{43}','{44}','{45}','{46}','{47}','{48}','{49}','{50}','{51}','{52}','{53}','{54}','{55}','{56}','{57}'); ", clienteNovo.numero_cliente,
            clienteNovo.dv_numero_cliente,
            clienteNovo.sucursal,
            clienteNovo.numero_ordem,
            clienteNovo.nombre,
            clienteNovo.tipo_ident,
            clienteNovo.rut,
            clienteNovo.dv_rut,
            clienteNovo.atv_economica,
            clienteNovo.direccion,
            clienteNovo.telefono,
            clienteNovo.dem_ctr_hp_seco,
            clienteNovo.dem_ctr_fp_seco,
            clienteNovo.dem_ctr_hr_seco,
            clienteNovo.dem_ctr_hp_umido,
            clienteNovo.dem_ctr_fp_umido,
            clienteNovo.dem_ctr_hr_umido,
            clienteNovo.carga_inst_kva,
            clienteNovo.voltaje_solicitado,
            clienteNovo.data_criacao,
            clienteNovo.tiene_postal,
            clienteNovo.rol_creacion,
            clienteNovo.municipio,
            clienteNovo.ddd,
            clienteNovo.classe,
            clienteNovo.subclasse,
            clienteNovo.telefono2,
            clienteNovo.ddd2,
            clienteNovo.contato_cli,
            clienteNovo.tipo_ident2,
            clienteNovo.documento2,
            clienteNovo.dv_docu2,
            clienteNovo.numero_casa,
            clienteNovo.direccion_postal,
            clienteNovo.bairro_postal,
            clienteNovo.municipio_postal,
            clienteNovo.unidade_federacao,
            clienteNovo.area_concessao,
            clienteNovo.cep_postal,
            clienteNovo.cobro_postal,
            clienteNovo.optante_bt,
            clienteNovo.contato_fone,
            clienteNovo.localidade,
            clienteNovo.tipo_ligacao,
            clienteNovo.cep,
            clienteNovo.ddd,
            clienteNovo.crea_tecnico,
            clienteNovo.potencia_trafo,
            clienteNovo.hora_func_emp_ini_1,
            clienteNovo.hora_func_emp_fim_1,
            clienteNovo.hora_func_emp_ini_2,
            clienteNovo.hora_func_emp_fim_2,
            clienteNovo.acteco_com,
            clienteNovo.ejecutivo,
            clienteNovo.insc_estadual,
            clienteNovo.cliente_zona_urbana);
            sql = sql.Replace("'NULL'", "NULL");

            
            return ExecutarSql(sql.ToString(), informix);
        }
        public string RetornaEstadoFaturacao(ContratoDTO contrato)
        {
            string resultado = string.Empty;
            string sql = string.Format(@"select estado_facturacion from cliente where numero_cliente ={0}", contrato.numero_cliente);

            var dt = ConsultaSql(sql.ToString());
            if (dt.Rows.Count > 0)
            {
                resultado = dt.Rows[0]["estado_facturacion"].ToString();
            }

            return resultado;
        }

        public ClienteDTO RetornaCliente(ContratoDTO contrato)
        {
            string sql = string.Format(@"select * from cliente                                           
										  where numero_cliente = {0}", contrato.numero_cliente);

            var dt = ConsultaSql(sql.ToString());
            if (dt.Rows.Count > 0)
            {
                ClienteDTO resultado = DataHelper.ConvertDataTableToEntity<ClienteDTO>(dt);
                return (ClienteDTO)resultado;
            }
            else
                return null;
        }
    
        #region INGRESSA CLIENTE
        public bool IngressaCliente(ClienteDTO cliente, DBProviderInformix informix)
        {
            Type t = typeof(ClienteDTO);
            foreach (PropertyInfo pi in t.GetProperties())
            {
                if (pi.PropertyType.Name.ToUpper().Equals("STRING") && Nullable.Equals(pi.GetValue(cliente),null))

                    pi.SetValue(cliente, "NULL", null);
            }

            string sql = string.Format(@"insert into cliente
                                                       (numero_cliente,
                                                        dv_numero_cliente,
                                                        sector,
                                                        localidade,
                                                        zona,
                                                        correlativo_ruta,
                                                        dv_ruta_lectura,
                                                        nombre,
                                                        direccion,
		                                                complemento, 
                                                        soundex_dir,
                                                        soundex_nombre,
                                                        comuna,
                                                        giro,
                                                        telefono,
                                                        tipo_ident,
                                                        rut,
                                                        dv_rut,
                                                        empresa,
                                                        sucursal,
                                                        coordenadas_eura,
                                                        cantidad_medidores,
                                                        info_adic_lectura,
                                                        tarifa,
                                                        tipo_vencimiento,
                                                        tipo_cliente,
                                                        cantidad_casas,
                                                        consumo_30_dias,
                                                        recargo_malfactor,
                                                        recargo_tension,
                                                        potencia_contrato,
                                                        potencia_inst_hp,
                                                        potencia_inst_fp,
                                                        potencia_cont_hp,
                                                        potencia_cont_fp,
                                                        propiedad_empalme,
                                                        tipo_empalme,
                                                        meses_cerrados,
                                                        fecha_a_corte,
                                                        fecha_penult_fact,
                                                        fecha_ultima_fact,
                                                        fecha_penult_lect,
                                                        fecha_ultima_lect,
                                                        prom_importe_cons,
                                                        cons_prom_diario,
                                                        nro_dias_consumo,
                                                        clave_boleta,
                                                        deuda_convenida,
                                                        antiguedad_saldo,
                                                        corr_facturacion,
                                                        corr_pagos,
                                                        corr_convenio,
                                                        corr_corte,
                                                        corr_refacturacion,
                                                        estado_facturacion,
                                                        estado_suministro,
                                                        estado_cliente,
                                                        tiene_cobro_int,
                                                        tiene_prorrateo,
                                                        tiene_cnr,
                                                        tiene_cambios_rest,
                                                        tiene_corte_rest,
                                                        tiene_convenio,
                                                        tiene_refacturac,
                                                        tiene_postal,
                                                        tiene_calma,
                                                        tiene_cobro_corte,
                                                        tiene_at_med_bt,
                                                        tiene_notific,
                                                        cod_proyecto,
                                                        fecha_proyecto,
                                                        municipio,
                                                        cep,
                                                        classe,
                                                        subclasse,
                                                        cliente_veranista,
                                                        ind_baixarenda,
                                                        ind_cliente_vip,
                                                        ind_ilumpublica,
                                                        ddd,
                                                        ramal,
                                                        tributo,
                                                        codigo_emp_comp,
                                                        saldo_afecto,
                                                        saldo_noafecto,
                                                        intereses,
                                                        multas,
                                                        cliente_anterior,
                                                        codigo_logra,
                                                        tipo_ligacao,
                                                        codigo_pee,
                                                        numero_casa,
                                                        ind_calc_media,
                                                        ind_conv_gov,
                                                        media_movel,
                                                        numero_nis,
                                                        refer_lido_br,
                                                        dia_vencimento,
                                                        quadra,
                                                        nome_mae,                   
                                                        mail,
                                                        escolaridade,
                                                        profissao,
                                                        sexo,
                                                        estado_civil,
                                                        religiao,
                                                        nome_completo,
                                                        qtd_pessoas_uc,
                                                        ind_pessoa,
                                                        qtd_funcionarios,
                                                        tempo_atuacao,
                                                        uf_nascimento,
                                                        data_nasc,    
                                                        subclasse_orig,
                                                        ind_ucbaixarenda, 
                                                        codigo_imovel) 
                                                        values('{0}',
                                                                '{1}',
                                                                '{2}',
                                                                '{3}',
                                                                '{4}',
                                                                '{5}',
                                                                '{6}',
                                                                '{7}',
                                                                '{8}',
                                                                '{9}',
                                                                '{10}',
                                                                '{11}',
                                                                '{12}',
                                                                '{13}',
                                                                '{14}',
                                                                '{15}',
                                                                '{16}',
                                                                '{17}',
                                                                '{18}',
                                                                '{19}',
                                                                '{20}',
                                                                '{21}',
                                                                '{22}',
                                                                '{23}',
                                                                '{24}',
                                                                '{25}',
                                                                '{26}',
                                                                '{27}',
                                                                '{28}',
                                                                '{29}',
                                                                '{30}',
                                                                '{31}',
                                                                '{32}',
                                                                '{33}',
                                                                '{34}',
                                                                '{35}',
                                                                '{36}',
                                                                '{37}',
                                                                '{38}',
                                                                '{39}',
                                                                '{40}',
                                                                '{41}',
                                                                '{42}',
                                                                '{43}',
                                                                '{44}',
                                                                '{45}',
                                                                '{46}',
                                                                '{47}',
                                                                '{48}',
                                                                '{49}',
                                                                '{50}',
                                                                '{51}',
                                                                '{52}',
                                                                '{53}',
                                                                '{54}',
                                                                '{55}',
                                                                '{56}',
                                                                '{57}',
                                                                '{58}',
                                                                '{59}',
                                                                '{60}',
                                                                '{61}',
                                                                '{62}',
                                                                '{63}',
                                                                '{64}',
                                                                '{65}',
                                                                '{66}',
                                                                '{67}',
                                                                '{68}',
                                                                '{69}',
                                                                '{70}',
                                                                '{71}',
                                                                '{72}',
                                                                '{73}',
                                                                '{74}',
                                                                '{75}',
                                                                '{76}',
                                                                '{77}',
                                                                '{78}',
                                                                '{79}',
                                                                '{80}',
                                                                '{81}',
                                                                '{82}',
                                                                '{83}',
                                                                '{84}',
                                                                '{85}',
                                                                '{86}',
                                                                '{87}',
                                                                '{88}',
                                                                '{89}',
                                                                '{90}',
                                                                '{91}',
                                                                '{92}',
                                                                '{93}',
                                                                '{94}',
                                                                '{95}',
                                                                '{96}',
                                                                '{97}',
                                                                '{98}',
                                                                '{99}',
                                                                '{100}',
                                                                '{101}',
                                                                '{102}',
                                                                '{103}',
                                                                '{104}',
                                                                '{105}',
                                                                '{106}',
                                                                '{107}',
                                                                '{108}',
                                                                '{109}',
                                                                '{110}',
                                                                '{111}',
                                                                '{112}',
                                                                '{113}',
                                                                '{114}',
                                                                '{115}')", cliente.numero_cliente
                                                                         , cliente.dv_numero_cliente
                                                                         , cliente.sector
                                                                         , cliente.localidade
                                                                         , cliente.zona
                                                                         , cliente.correlativo_ruta
                                                                         , cliente.dv_ruta_lectura
                                                                         , cliente.nombre
                                                                         , cliente.direccion
                                                                         , cliente.complemento
                                                                         , cliente.soundex_dir
                                                                         , cliente.soundex_nombre
                                                                         , cliente.comuna
                                                                         , cliente.giro
                                                                         , cliente.telefono
                                                                         , cliente.tipo_ident
                                                                         , cliente.rut
                                                                         , cliente.dv_rut
                                                                         , cliente.empresa
                                                                         , cliente.sucursal
                                                                         , cliente.coordenadas_eura
                                                                         , cliente.cantidad_medidores
                                                                         , cliente.info_adic_lectura
                                                                         , cliente.tarifa
                                                                         , cliente.tipo_vencimiento
                                                                         , cliente.tipo_cliente
                                                                         , cliente.cantidad_casas
                                                                         , cliente.consumo_30_dias
                                                                         , cliente.recargo_malfactor
                                                                         , cliente.recargo_tension
                                                                         , cliente.potencia_contrato
                                                                         , cliente.potencia_inst_hp
                                                                         , cliente.potencia_inst_fp
                                                                         , cliente.potencia_cont_hp
                                                                         , cliente.potencia_cont_fp
                                                                         , cliente.propiedad_empalme
                                                                         , cliente.tipo_empalme
                                                                         , cliente.meses_cerrados
                                                                         , cliente.fecha_a_corte
                                                                         , cliente.fecha_penult_fact
                                                                         , cliente.fecha_ultima_fact
                                                                         , cliente.fecha_penult_lect
                                                                         , cliente.fecha_ultima_lect
                                                                         , cliente.prom_importe_cons
                                                                         , cliente.cons_prom_diario
                                                                         , cliente.nro_dias_consumo
                                                                         , cliente.clave_boleta
                                                                         , cliente.deuda_convenida
                                                                         , cliente.antiguedad_saldo
                                                                         , cliente.corr_facturacion
                                                                         , cliente.corr_pagos
                                                                         , cliente.corr_convenio
                                                                         , cliente.corr_corte
                                                                         , cliente.corr_refacturacion
                                                                         , cliente.estado_facturacion
                                                                         , cliente.estado_suministro
                                                                         , cliente.estado_cliente
                                                                         , cliente.tiene_cobro_int
                                                                         , cliente.tiene_prorrateo
                                                                         , cliente.tiene_cnr
                                                                         , cliente.tiene_cambios_rest
                                                                         , cliente.tiene_corte_rest
                                                                         , cliente.tiene_convenio
                                                                         , cliente.tiene_refacturac
                                                                         , cliente.tiene_postal
                                                                         , cliente.tiene_calma
                                                                         , cliente.tiene_cobro_corte
                                                                         , cliente.cliente_veranista
                                                                         , cliente.tiene_notific
                                                                         , cliente.cod_proyecto
                                                                         , cliente.fecha_proyecto
                                                                         , cliente.municipio
                                                                         , cliente.cep
                                                                         , cliente.classe
                                                                         , cliente.subclasse
                                                                         , cliente.cliente_veranista
                                                                         , cliente.ind_baixarenda
                                                                         , cliente.ind_cliente_vip
                                                                         , cliente.ind_ilumpublica
                                                                         , cliente.ddd
                                                                         , cliente.ramal
                                                                         , cliente.tributo
                                                                         , cliente.codigo_emp_comp
                                                                         , cliente.saldo_afecto
                                                                         , cliente.saldo_noafecto
                                                                         , cliente.intereses
                                                                         , cliente.multas
                                                                         , cliente.cliente_anterior
                                                                         , cliente.codigo_logra
                                                                         , cliente.tipo_ligacao
                                                                         , cliente.codigo_pee
                                                                         , cliente.numero_casa
                                                                         , cliente.ind_calc_media
                                                                         , cliente.ind_conv_gov
                                                                         , cliente.media_movel
                                                                         , cliente.numero_nis
                                                                         , cliente.refer_lido_br
                                                                         , cliente.dia_vencimento
                                                                         , cliente.quadra
                                                                         , cliente.nome_mae
                                                                         , cliente.mail
                                                                         , cliente.escolaridade
                                                                         , cliente.profissao
                                                                         , cliente.sexo
                                                                         , cliente.estado_civil
                                                                         , cliente.religiao
                                                                         , cliente.nome_completo
                                                                         , cliente.qtd_pessoas_uc
                                                                         , cliente.ind_pessoa
                                                                         , cliente.qtd_funcionarios
                                                                         , cliente.tempo_atuacao
                                                                         , cliente.uf_nascimento
                                                                         , cliente.data_nasc
                                                                         , cliente.subclasse_orig
                                                                         , cliente.ind_ucbaixarenda
                                                                         , cliente.codigo_imovel);

            //DBProviderInformix informix = ObterProviderInformix();
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
        #endregion

        public bool IngressaNucli(int numero_cliente, string dv_cliente)
        {
            string sql = string.Format(@"insert into nucli (
                                                    numero_cliente,
                                                    dv_numero_cliente,
                                                    dv_ss,
                                                    cliente_veranista,
                                                    ind_baixarenda,
                                                    ind_ilumpublica)
                                                  values( {0}, {1}, '{2}', '{3}', '{4}', '{5}')"
                , numero_cliente
                , dv_cliente
                , "0"
                , "N"
                , "N"
                , "N");

            return ExecutarSql(sql.ToString()) > 0;
        }

        public bool IngressaClienteCadAtend(ClienteCadAtendDTO clienteCadAtend)
        {
            string sql = string.Format(@"insert into cliente_cad_atend
                                                    (
                                                        numero_cliente,
                                                        ind_aut_email,
                                                        rol,
                                                        nome_conjuge,
                                                        tipo_doc_conjuge,
                                                        documento_conjuge,
                                                        dv_documento_conjuge,
                                                        ind_cad_conjuge
                                                    )    
                                                    values
                                                    ({0},{1},'{2},'{3}','{4}','{5},'{6}',{7})"
                                                    , clienteCadAtend.numero_cliente
                                                    , clienteCadAtend.ind_aut_email
                                                    , clienteCadAtend.rol
                                                    , clienteCadAtend.nome_conjuge
                                                    , clienteCadAtend.tipo_doc_conjuge
                                                    , clienteCadAtend.documento_conjuge
                                                    , clienteCadAtend.dv_documento_conjuge
                                                    , clienteCadAtend.ind_cad_conjuge);

            return ExecutarSql(sql.ToString()) > 0;
        }
      
		public bool ExcluirTelefonePorOrdem(string pNumeroOrdem)
        {
            StringBuilder sql = new StringBuilder();
            sql.AppendFormat(" DELETE FROM TELEFONE_CLIENTE WHERE NUMERO_ORDEM = {0} ", pNumeroOrdem);

            return ExecutarSql(sql.ToString()) > 0;
        }

        public double ConsultaDivida(ClienteDTO cliente)
        {
            double resultado = 0;

            StringBuilder sql = new StringBuilder("SELECT ( ");
            sql.Append("SELECT nvl_mig(sum(round(hfa.total_facturado,2)), 0) ");
            sql.Append("FROM hisfac hfa ");
            sql.AppendFormat("WHERE hfa.numero_cliente = '{0}' ", cliente.numero_cliente);
            sql.Append("  and hfa.ind_saldo = 'N' ");
            sql.Append("  and hfa.indica_refact = 'N' ");
            sql.Append("  and hfa.fecha_vencimiento < today ");
            sql.Append("  and hfa.corr_facturacion not in ( SELECT corr_fatura ");
            sql.Append("  FROM arrec_parcial A WHERE A.numero_cliente = hfa.numero_cliente ");
            sql.Append("  and A.corr_fatura = hfa.corr_facturacion and A.ind_fatura = 'F' ");
            sql.Append("  and A.estado = 'I' ) ");
            sql.Append(") as dividaFat, ( ");
            sql.Append("SELECT nvl_mig(sum(round(r.total_refacturado,2)),0) ");
            sql.Append(" FROM refac r ");
            sql.AppendFormat(" WHERE r.numero_cliente = '{0}' ", cliente.numero_cliente);
            sql.Append(" and r.tipo_nota in ('D','1') ");
            sql.Append(" and r.ind_saldo = 'N' ");
            sql.Append(" and r.indica_refact = 'N' ");
            sql.Append(" and r.fecha_vencimiento < today ");
            sql.Append(" and r.corr_refacturacion not in ( SELECT corr_fatura ");
            sql.Append("  FROM arrec_parcial A WHERE A.numero_cliente = r.numero_cliente ");
            sql.Append("  and A.corr_fatura = r.corr_refacturacion and A.ind_fatura = 'F' ");
            sql.Append("  and A.estado = 'I' ) ");
            sql.Append(") as dividaRefat ");
            sql.Append("FROM DUAL ");

            DataTable dtResultado = ConsultaSql(sql.ToString());

            if (dtResultado != null && dtResultado.Rows.Count > 0 &&
                !DBNull.Value.Equals(dtResultado.Rows[0]["dividaFat"]) &&
                !DBNull.Value.Equals(dtResultado.Rows[0]["dividaRefat"]))
            {
                resultado = Convert.ToDouble(dtResultado.Rows[0]["dividaFat"]) +
                    Convert.ToDouble(dtResultado.Rows[0]["dividaRefat"]);
            }

            return resultado;
        }

        public List<DocumentoDTO> ListaDocumentos(string numeroCliente)
        {
            StringBuilder sql = new StringBuilder("SELECT tipo_documento, numero_doc, dv_documento ");
            sql.Append("FROM  documento_cliente r");
            sql.AppendFormat(" WHERE r.numero_cliente = {0} ", numeroCliente);

            DataTable dtResultado = ConsultaSql(sql.ToString());

            if (dtResultado != null && dtResultado.Rows.Count > 0)
                return DataHelper.ConvertDataTableToList<DocumentoDTO>(dtResultado);

            return null;
        }

        public RepresentanteLegalDTO BuscaRepresentanteLegal(string numeroCliente)
        {
            StringBuilder sql = new StringBuilder("SELECT * FROM  representante_legal ");
            sql.AppendFormat(" WHERE numero_cliente = {0} ", numeroCliente);

            DataTable dtResultado = ConsultaSql(sql.ToString());

            if (dtResultado != null && dtResultado.Rows.Count > 0)
                return DataHelper.ConvertDataTableToEntity<RepresentanteLegalDTO>(dtResultado);

            return null;
        }

        public bool Atualizar(ClienteAReligar cliente, DBProviderInformix informix)
        {
            string sql = string.Format(@"UPDATE cliente_a_religar
                                            SET data_emissao = {0}
                                          WHERE numero_cliente = {1} "
                , cliente.data_emissao.HasValue ? string.Format("'{0}'", cliente.data_emissao.Value.ToString("yyyy-MM-dd HH:mm")) : "null"
                , cliente.numero_cliente);

            return ExecutarSql(sql.ToString(), informix);
        }


        /// <summary>
        /// Indica se o cliente informado possui Troca de Titularidade registrada a partir de N dias informados.
        /// </summary>
        /// <param name="cliente"></param>
        /// <param name="limiteDias">Período a considerar para o registro de troca de titularidade.</param>
        /// <returns></returns>
        public bool TemTrocaTitularidade(string cliente, int limiteDias)
        {
            StringBuilder sql = new StringBuilder(@"select first 1 1 from modif ");
            sql.AppendFormat(@"WHERE fecha_modif >= today - {0}
                                 and codigo_modif in ('720', '721', '722','723')
                                 and numero_cliente = {1}", limiteDias, cliente.Trim());

            return ConsultaSql(sql.ToString()).Rows.Count > 0;
        }
        public ClienteDTO RetornarInforCliente(int numero_cliente)
        {
            StringBuilder sql = new StringBuilder();

            sql.Append(@" SELECT
                                c.tipo_ligacao,
                                c.estado_cliente,
                                c.classe,
                                c.subclasse,
                                c.sector,
                                c.localidade,
                                c.zona,
                                c.correlativo_ruta,
                                c.dv_ruta_lectura,
                                c.sucursal,
                                c.municipio,
                                c.quadra,
                                c.ind_cliente_vital,
                                c.codigo_pee,
                                c.potencia_contrato,
                                   CASE WHEN
	                                    c.estado_cliente = 8 THEN 'A'
                                    ELSE
	                                    'B'
                                    END AS tipo_cliente
                        FROM cliente c
                        JOIN susec s
                        ON s.sector = c.sector
                        AND s.localidade = c.localidade
                        AND s.zona = c.zona
                        WHERE numero_cliente = " + numero_cliente);

            var dt = ConsultaSql(sql.ToString());
            if (dt.Rows.Count > 0)
            {
                return DataHelper.ConvertDataTableToEntity<ClienteDTO>(dt);
            }
            else
                return null;
        }
		
		#region PANGEA.SalesForce.Corte-Repo
        public RespuestaCorteRepoDTO ConsultarCorteExecutado(ParamCorteRepoDTO corte)
        {
            RespuestaCorteRepoDTO Respuesta = null;
            StringBuilder sql = new StringBuilder();            

            sql.Append("SELECT 	DISTINCT																		");
            sql.Append("    os.numero_ordem,                                                                    ");
            sql.Append("	num_ordem_serv_crt,                                                                 ");
            sql.Append("    c.numero_cliente, corr_corte,                                                       ");
            sql.Append("    (SELECT sociedad_sie2000 FROM insta) AS empresa,                                    ");
            sql.Append("    (SELECT sociedad_sie2000 FROM insta)||'CORTE'||os.numero_ordem AS id_corte_repo,    ");
            sql.Append("    'C' AS tipo_registro,                                                                ");
            sql.Append("    c.motivo_corte, tcormot.codigo,                                                     ");
            sql.Append("    tcormot.codigo AS motivo,                                                      ");
            sql.Append("    os.estado AS estado_ordem,                                                ");
            sql.Append("    c.fecha_corte, data_solic_corte,                                                    ");
            sql.Append("    acc_realizada_cor, tacreco.descripcion AS acreco_descripcion,                       ");
            sql.Append("    c.tipo_corte, tc.descricao_tipo AS tipo_corte                                       ");
            sql.Append("FROM correp c ,  ordem_servico os , OUTER tipo_corte tc, OUTER  tabla tcormot , OUTER  tabla tacreco, OUTER tabla testord ");
            sql.Append("WHERE c.numero_cliente = os.numero_cliente  ");
            sql.Append("AND c.tipo_corte = tc.tipo_corte    ");
            sql.Append("AND c.motivo_corte = tcormot.codigo ");
            sql.Append("AND c.acc_realizada_cor = tacreco.codigo AND c.num_ordem_serv_crt = os.numero_ordem ");
            sql.Append("AND os.estado = testord.codigo  ");
            sql.Append("AND fecha_corte IS NOT NULL                                                           ");
            sql.Append("AND date(tc.data_ativacao) <= today                                                     ");
            sql.Append("AND (date(tc.data_desativacao) >= today OR tc.data_desativacao IS NULL)                 ");
            sql.Append("AND tcormot.NomTabla = 'CORMOT'                                                         ");
            sql.Append("AND tcormot.Sucursal = '0000'                                                           ");
            sql.Append("AND (tcormot.fecha_activacion <= today)                                                 ");
            sql.Append("AND (tcormot.fecha_desactivac >= today OR tcormot.fecha_desactivac IS NULL)             ");
            sql.Append("AND tacreco.NomTabla = 'ACRECO'                                                         ");
            sql.Append("AND tacreco.Sucursal = '0000'                                                           ");
            sql.Append("AND (tacreco.fecha_activacion <= today)                                                 ");
            sql.Append("AND (tacreco.fecha_desactivac >= today OR tacreco.fecha_desactivac IS NULL)             ");
            sql.Append("AND testord.NomTabla = 'ESTORD'                                                         ");
            sql.Append("AND testord.Sucursal = '0000'                                                           ");
            sql.Append("AND (testord.fecha_activacion <= today)                                                 ");
            sql.Append("AND (testord.fecha_desactivac >= today OR testord.fecha_desactivac IS NULL)             ");
            sql.AppendFormat("AND c.numero_cliente = {0} ", corte.NumeroSuministro.ToString());
            //sql.AppendFormat(" AND os.data_ingresso >= '{0}'", corte.FechaInicio.ToString("yyyy-MM-dd HH:mm"));
            //sql.AppendFormat(" AND os.data_ingresso <= '{0}'", corte.FechaFin.ToString("yyyy-MM-dd HH:mm"));

            DataTable result = ConsultaSql(sql.ToString());

            if (result.Rows.Count > 0)
            {
                Respuesta = new RespuestaCorteRepoDTO();                
                Respuesta.NumeroSuministro = Convert.ToInt64(result.Rows[0]["numero_cliente"]);
                Respuesta.CodigoEmpresa = result.Rows[0]["empresa"].ToString();

                foreach(DataRow row in result.Rows)
                {
                    var corteSolicitado = new CorteRepoSolicitadoDTO();
                    corteSolicitado.NumeroOrden = row["numero_ordem"].ToString();
                    corteSolicitado.IDCorteRepo = row["id_corte_repo"].ToString();
                    corteSolicitado.TipoRegistro = row["tipo_registro"].ToString();
                    corteSolicitado.Motivo = row["motivo"].ToString().Trim();
                    corteSolicitado.Estado = row["estado_ordem"].ToString().Trim();
                    //corteSolicitado.FechaEjecucion = String.Format("{0:MM/dd/yyyy }", row["fecha_corte"]).ToString();
                    corteSolicitado.FechaEjecucion = String.Format("{0:s}", row["fecha_corte"]).ToString();
                    corteSolicitado.FechaSolicitud = String.Format("{0:s}", row["data_solic_corte"]).ToString();
                    corteSolicitado.AccionRealizada = row["acc_realizada_cor"].ToString().Trim();
                    corteSolicitado.Tipo = row["tipo_corte"].ToString().Trim();

                    Respuesta.ListaCortesReposSolicitados.Add(corteSolicitado);
                }

                
            }

            return Respuesta;
        }

        public RespuestaCorteRepoDTO ConsultarCorteExecutadoGA(ParamCorteRepoDTO corte)
        {
            RespuestaCorteRepoDTO Respuesta = null;
            StringBuilder sql = new StringBuilder();            

            sql.Append("SELECT DISTINCT  ");
            sql.Append("    os.numero_ordem,                                                                    ");
            sql.Append("	num_ordem_serv_crt,                                                                 ");
            sql.Append("    c.numero_cliente, corr_corte,                                                       ");
            sql.Append("    (SELECT sociedad_sie2000 FROM insta) AS empresa,                                    ");
            sql.Append("    (SELECT sociedad_sie2000 FROM insta)||'CORTE'||os.numero_ordem AS id_corte_repo,    ");
            sql.Append("    '' AS tipo_registro,                                                                ");
            sql.Append("    c.motivo_corte, tcormot.codigo,                                                     ");
            sql.Append("    tcormot.descripcion AS motivo,                                                      ");
            sql.Append("    testord.descripcion AS estado_ordem,                                                ");
            sql.Append("    c.fecha_corte, data_solic_corte,                                                    ");
            sql.Append("    acc_realizada_cor, tacreco.descripcion AS acreco_descripcion,                       ");
            sql.Append("    c.tipo_corte, tc.descricao_tipo AS tipo_corte                                                    ");
            sql.Append("FROM grandes:correp c ,  grandes:ordem_servico os , OUTER clientes:tipo_corte tc, OUTER  grandes:tabla tcormot , OUTER  grandes:tabla tacreco, OUTER grandes:tabla testord ");
            sql.Append("WHERE c.numero_cliente = os.numero_cliente  ");
            sql.Append("AND c.tipo_corte = tc.tipo_corte    ");
            sql.Append("AND c.motivo_corte = tcormot.codigo ");
            sql.Append("AND c.acc_realizada_cor = tacreco.codigo AND c.num_ordem_serv_crt = os.numero_ordem ");
            sql.Append("AND os.estado = testord.codigo  ");
            sql.Append("AND fecha_corte IS NOT NULL                                                           ");
            sql.Append("AND date(tc.data_ativacao) <= today                                                     ");
            sql.Append("AND (date(tc.data_desativacao) >= today OR tc.data_desativacao IS NULL)                 ");
            sql.Append("AND tcormot.NomTabla = 'CORMOT'                                                         ");
            sql.Append("AND tcormot.Sucursal = '0000'                                                           ");
            sql.Append("AND (tcormot.fecha_activacion <= today)                                                 ");
            sql.Append("AND (tcormot.fecha_desactivac >= today OR tcormot.fecha_desactivac IS NULL)             ");
            sql.Append("AND tacreco.NomTabla = 'ACRECO'                                                         ");
            sql.Append("AND tacreco.Sucursal = '0000'                                                           ");
            sql.Append("AND (tacreco.fecha_activacion <= today)                                                 ");
            sql.Append("AND (tacreco.fecha_desactivac >= today OR tacreco.fecha_desactivac IS NULL)             ");
            sql.Append("AND testord.NomTabla = 'ESTORD'                                                         ");
            sql.Append("AND testord.Sucursal = '0000'                                                           ");
            sql.Append("AND (testord.fecha_activacion <= today)                                                 ");
            sql.Append("AND (testord.fecha_desactivac >= today OR testord.fecha_desactivac IS NULL)             ");
            sql.AppendFormat("AND c.numero_cliente = {0} ", corte.NumeroSuministro.ToString());
            //sql.AppendFormat(" AND os.data_ingresso >= {0} ", corte.FechaInicio.ToString("yyyy-MM-dd HH:mm"));
            //sql.AppendFormat(" AND os.data_ingresso <= {0}", corte.FechaFin.ToString("yyyy-MM-dd HH:mm"));

            DataTable result = ConsultaSql(sql.ToString());

            if (result.Rows.Count > 0)
            {
                Respuesta = new RespuestaCorteRepoDTO();
                Respuesta.NumeroSuministro = Convert.ToInt64(result.Rows[0]["numero_cliente"]);
                Respuesta.CodigoEmpresa = result.Rows[0]["empresa"].ToString();

                foreach (DataRow row in result.Rows)
                {
                    var corteSolicitado = new CorteRepoSolicitadoDTO();
                    corteSolicitado.NumeroOrden = row["numero_ordem"].ToString();
                    corteSolicitado.IDCorteRepo = row["id_corte_repo"].ToString();
                    corteSolicitado.TipoRegistro = row["tipo_registro"].ToString();
                    corteSolicitado.Motivo = row["motivo"].ToString().Trim();
                    corteSolicitado.Estado = row["estado_ordem"].ToString().Trim();
                    corteSolicitado.FechaEjecucion = String.Format("{0:s}", row["fecha_corte"]).ToString();
                    corteSolicitado.FechaSolicitud = String.Format("{0:s}", row["data_solic_corte"]).ToString();
                    corteSolicitado.AccionRealizada = row["acc_realizada_cor"].ToString().Trim();
                    corteSolicitado.Tipo = row["tipo_corte"].ToString().Trim();

                    Respuesta.ListaCortesReposSolicitados.Add(corteSolicitado);
                }
            }

            return Respuesta;
        }

        #endregion
    }
}

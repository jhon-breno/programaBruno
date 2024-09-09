using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

namespace SalesforceExtractor.Dados
{
    public class AltaContratacaoGBDAO : AltaContratacaoDAO
    {
        public override string GetConsultaBase()
        {
            return @"SELECT   (select sociedad_sie2000 from insta)    AS BP_OPBUK,
                                    tecni.fecha_conexion                            AS DATA_VALIDITA,
                                    cliente.numero_cliente                          AS IM_ZZ_NUMUTE,
                                    cliente.nombre                                  AS BP_NAME_ORG1,
                                    --sg.externalid_asset                           AS BP_BPEXT, 
                                    TRIM(cliente.rut) || TRIM(cliente.dv_rut)       AS BP_BPEXT,
                                    TRIM(cliente.rut) || TRIM(cliente.dv_rut)       AS BP_ZZ_CODFISC,
                                    cliente.direccion                   AS BP_STREET,
                                    cliente.numero_casa                  AS BP_HOUSE_NUM1,      --*
                                    cliente.giro,
                                    cliente.cep                         AS BP_POST_CODE1,
                                    ''                                  AS BP_CITY2,        --*
                                    (select l.descripcion from localidades l where l.localidad = c.municipio * 10)		    AS BP_CITY1,            --*
                                    cliente.mail                        AS BP_SMTP_ADDR,    --*
                                    (select descripcion from giros where codigo_giros = trim(cliente.giro))          AS ATTIVITA,   --* 
                                    ''                                  AS BP_EXECUTIVE,    --*
                                    su.ind_zona                         AS IM_REGION,       --*
                                    ''                                  AS IM_CHARGE,    --IM_DI_CONTRGE,      --CargaKWBR          --* ??
                                    ''                                  AS IM_BRANCHE,   --as CNAE,
                                    CASE WHEN TARSAP.valor_alf LIKE '%verde%' THEN 'Horosazonal Verde' ELSE  
                                    CASE WHEN TARSAP.valor_alf LIKE '%azul%' THEN 'Horosazonal Azul' ELSE  
                                    CASE WHEN TARSAP.valor_alf LIKE '%horo azul%' THEN 'HORO AZUL' ELSE 'OPTANTE' END END END       AS ModTarifBR, 
                                    TARSAP.valor_alf                    AS IM_TARIFTYP,        -- Categoria_Tarie_BR, 
                                    cliente.dia_vencimento AS AC_ZAHLKOND,
                                    TARSYN.descripcion                  AS AC_KOFIZ_SD,        --Classe_BR, 
                                    TARSAP.descripcion                  AS IM_TEMP_AREA,       --SubClasse_BR, 
                                    cliente.potencia_cont_hp            AS IM_DI_CONTRAT  ,    --Demanda_KVABR, 
                                    cliente.potencia_cont_hp            AS IM_DI_CONTRPT,      --Demanda_Ponta_BR,   
                                    cliente.potencia_cont_fp            AS IM_DI_CONTRFP,      --Demanda_FP_BR, 
                                    '0'                                 AS Capacidade_DisjuntorBR, 
                                    NVL(desc_tabla('VOLTA', codigo_voltaje, '0000'), '') AS ValorTensaoBR, 
                                    CASE WHEN SBP.valor_alf = '12' THEN '04'
	                                        ELSE CASE WHEN SBP.valor_alf = '22' THEN '06'
	                                        ELSE CASE WHEN SBP.valor_alf = '38' THEN '10'  
	                                        ELSE '06' END END END       AS IM_SPEBENE,
                                    'Não'                               AS instalacao_padrao, 
                                    CASE WHEN UPPER(TRIM(m.clave_montri)) = 'T' THEN 'Trifásica' 
	                                ELSE CASE WHEN UPPER(TRIM(m.clave_montri)) = 'B' THEN 'Bifásica' 
		                            ELSE 'Monofásica' END END                                           AS Tipo_TensaoBR,       --*
                                    ''                                  AS Potencia_KWBR,           --*  ??
                                    CASE WHEN UPPER(TRIM(m.clave_montri)) = 'T' THEN 'Trifásica' 
	                                ELSE CASE WHEN UPPER(TRIM(m.clave_montri)) = 'B' THEN 'Bifásica' 
		                            ELSE 'Monofásica' END END                                           AS tipo_tensao ,
                                    (select cj.cod_conjunto_aneel
                                        from cliente c
                                            inner join hispostes hp on hp.cod_poste = c.codigo_pee
                                            inner join hisal ha on ha.cod_alimentador = hp.cod_alimentador 
                                            inner join conjuntos cj on cj.cod_conjunto = ha.cod_conjunto
                                        where 
                                            c.numero_cliente = sg.numero_cliente
                                        group by cj.cod_conjunto_aneel, cj.nome) AS IM_FACTOR_2,
                                    sg.ExternalId_Pod,
                                    sg.ExternalId_Asset
                            FROM    clientes@clientes:Sales_geral sg 
                                    JOIN cliente 
                                        ON sg.numero_cliente = cliente.numero_cliente 
                                    JOIN medid m 
	                                    ON m.numero_cliente = cliente.numero_cliente  
                                    JOIN tecni 
                                        ON tecni.numero_cliente = cliente.numero_cliente 
                                    LEFT JOIN gc_cliente                        --*   ?????????????????????????
                                        ON cliente.numero_cliente = gc_cliente.numero_cliente 
                                        LEFT JOIN tabla TARSAP 
                                        ON cliente.giro = LEFT(TARSAP.codigo,2) 
                                        AND gc_cliente.sub_clase = SUBSTR(TARSAP.codigo,3,2) 
                                        AND cliente.tarifa = RIGHT(TARSAP.codigo,2) 
                                        AND TARSAP.nomtabla='TARSAP' 
                                        LEFT JOIN tabla TARSYN 
                                        ON TARSYN.codigo =TARSAP.codigo 
                                        AND TARSYN.nomtabla='TARSYN' 
                                        AND TARSYN.sucursal='0000' 
                                    LEFT JOIN tabla SBP
                                        ON SBP.codigo = tecni.codigo_voltaje
                                        and SBP.nomtabla = 'VOLTA'
                                        and SBP.sucursal = '0000'
                                    LEFT JOIN susec su
                                        ON cliente.sucursal = su.sucursal
                                        and cliente.zona = su.zona
                                        and cliente.sector = su.sector
                                        and cliente.localidade = su.localidade
                            WHERE   cliente.numero_cliente in ({0})
                              --AND   cliente.estado_cliente <> 8
                            GROUP BY 1,2,3,4,5,6,7,8,9,10,11,12,13,14,15,16,17,18,19,20,21,22,23,24,25,26,27,28,29,30,31,32,33,34,35
                            ORDER BY 1";
        }


        public override string GetConsultaB2Win()
        {
            string sql = @"SELECT 
	                            sg.externalid_asset AS ExternalId_c , 
	                            --CASE WHEN nvl(cliente.potencia_inst_fp,0) = 0 THEN '15.0' ELSE to_char(ROUND(cliente.potencia_inst_fp/1000),2) END AS CargaKWBR,
	                            '74' AS CargaKWBR,
                                '' AS modtarifa, 
	                            CASE WHEN nvl(TARSAP.valor_alf,'') = '' THEN 'B1_RESID - Categoria de tarifa B1 residencial' ELSE
	                              TARSAP.valor_alf END AS Categoria_Tarifa_BR,  
	                            CASE WHEN nvl(TARSYN.descripcion,'') = '' THEN '10 - Residencial' ELSE
	                              TARSYN.descripcion END AS Classe_BR,  
	                            CASE WHEN nvl(TARSAP.descripcion,'') = '' THEN 'REPLN - Residencial Pleno' ELSE
	                              TARSAP.descripcion END AS SubClasse_BR,  
	                            '0' AS Demanda_KVABR,  
	                            '0' AS Demanda_Ponta_BR,  
	                            '0' AS Demanda_FP_BR,  
	                            '0' AS Capacidade_DisjuntorBR,  
                                CASE WHEN tecni.codigo_voltaje = '02' THEN '380 V' ELSE                                                               
			                                CASE WHEN tecni.codigo_voltaje = '04' THEN '127 V' ELSE '220 V' END END as ValorTensaoBR, 
	                            'Não' AS instalacao_padrao,  
	                            CASE WHEN UPPER(TRIM(cliente.tipo_ligacao)) = 'T' THEN 'Trifásica' ELSE   
	                                CASE WHEN UPPER(TRIM(cliente.tipo_ligacao)) = 'B' THEN 'Bifásica' ELSE 'Monofásica' END END AS tipo_tensaobr,   
	                            --CASE WHEN nvl(cliente.potencia_inst_fp,0) = 0 THEN '15.0' ELSE to_char(ROUND(cliente.potencia_inst_fp/1000),2) END  AS Potencia_KWBR,
                                '74' AS Potencia_KWBR,
	                            'Baixa Tensão',
	                            tecni.codigo_voltaje,
	                            cliente.numero_nis AS Nis, 
	                            brx.numero_nb AS Nb,
	                            sg.ExternalId_Pod,
	                            sg.ExternalId_Asset,
                                cliente.numero_cliente
                            FROM clientes@clientes:Sales_geral sg  
                            JOIN cliente 
                            ON sg.numero_cliente = cliente.numero_cliente 
                            --JOIN medid m 
                            --ON m.numero_cliente = cliente.numero_cliente  
                            JOIN tecni 
                            ON tecni.numero_cliente = cliente.numero_cliente 
                            LEFT JOIN grandes@clientes:tabla TARSAP 
                            ON cliente.classe = LEFT(TARSAP.codigo,2) 
                            AND cliente.subclasse = SUBSTR(TRIM(TARSAP.codigo),3,2)  
                            AND cliente.tarifa =  SUBSTR(TRIM(TARSAP.codigo),5,2) 
                            AND TARSAP.nomtabla='TARSAP' 
                            AND TARSAP.sucursal='0000' 
                            LEFT JOIN grandes@clientes:tabla TARSYN 
                            ON TARSYN.codigo =TARSAP.codigo 
                            AND TARSYN.nomtabla='TARSYN' 
                            AND TARSYN.sucursal='0000' 
                            LEFT JOIN cliente_doc_bxr brx 
                            ON brx.numero_cliente = cliente.numero_cliente 
                            WHERE cliente.numero_cliente in ({0})";

            return sql;
        }
    }
}

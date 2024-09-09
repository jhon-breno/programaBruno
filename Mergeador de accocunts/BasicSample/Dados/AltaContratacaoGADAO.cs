using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

namespace SalesforceExtractor.Dados
{
    public class AltaContratacaoGADAO : AltaContratacaoDAO
    {
       public override string GetConsultaBase()
        {
            return @"SELECT   (select sociedad_sie2000 from insta)    AS BP_OPBUK,
                                        case when tecni.fecha_conexion < tecni.fecha_ult_contrato  then TO_CHAR(tecni.fecha_ult_contrato,'%Y-%m-%d') else TO_CHAR(tecni.fecha_conexion,'%Y-%m-%d') end   AS DATA_VALIDITA,
                                        cliente.numero_cliente                          AS IM_ZZ_NUMUTE,
                                        trim(cliente.nombre)                            AS BP_NAME_ORG1,
                                        --sg.externalid_asset                           AS BP_BPEXT, 
                                        TRIM(cliente.rut) || TRIM(cliente.dv_rut)       AS BP_BPEXT,
                                        TRIM(cliente.rut) || TRIM(cliente.dv_rut)       AS BP_ZZ_CODFISC,
                                        trim(cliente.direccion)             AS BP_STREET,
                                        cliente.end_numero                  AS BP_HOUSE_NUM1,
                                        trim(cliente.giro)                  AS GIRO,
                                        cliente.cep                         AS BP_POST_CODE1,
                                        cliente.barrio_postal               AS BP_CITY2,
                                        (select trim(l.descripcion) from localidades l where l.localidad = c.municipio * 10)		    AS BP_CITY1,
                                        trim(gc_cliente.email)              AS BP_SMTP_ADDR,
                                        cliente.actividad_economic          AS ATTIVITA, 
                                        gc_cliente.ejecutivo                AS BP_EXECUTIVE,
                                        gc_cliente.localizacao              AS IM_REGION,
                                        NVL(ggc_resumen.pot_conectada,0)    AS IM_CHARGE,    --IM_DI_CONTRGE,      --CargaKWBR  
                                        substr(trim(oko.valor_alf),(charindex('|',trim(oko.valor_alf)) + 1),len(oko.valor_alf))                     AS IM_BRANCHE,   --as CNAE,
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
                                        --NVL(desc_tabla('VOLTA', codigo_voltaje, '0000'), '') AS ValorTensaoBR, 
                                        SBP.valor_alf AS IM_SPEBENE,
                                        'Não'                               AS instalacao_padrao, 
                                        'Trifásica'                         AS Tipo_TensaoBR, 
                                        okl.transformador                   AS Potencia_KWBR, 
                                        cliente.tipo_tensao ,
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
                                        JOIN gc_tecni okl
                                        ON okl.numero_cliente = sg.numero_cliente
                                        JOIN cliente 
                                        ON sg.numero_cliente = cliente.numero_cliente 
                                        JOIN tecni 
                                        ON tecni.numero_cliente = cliente.numero_cliente 
                                        JOIN gc_cliente
                                        ON cliente.numero_cliente = gc_cliente.numero_cliente 
                                        JOIN ggc_resumen 
                                        ON ggc_resumen.numero_cliente = cliente.numero_cliente 
                                        LEFT JOIN tabla TARSAP 
                                        ON cliente.giro = LEFT(TARSAP.codigo,2) 
                                        AND gc_cliente.sub_clase = SUBSTR(TARSAP.codigo,3,2) 
                                        AND cliente.tarifa = RIGHT(TARSAP.codigo,2) 
                                        AND TARSAP.nomtabla='TARSAP' 
                                        LEFT JOIN tabla TARSYN 
                                        ON TARSYN.codigo =TARSAP.codigo 
                                        AND TARSYN.nomtabla='TARSYN' 
                                        AND TARSYN.sucursal='0000' 
                                        LEFT JOIN tabla oko 
                                        ON substr(oko.valor_alf,0,(charindex('|',oko.valor_alf)-1)) = cliente.actividad_economic
                                        AND oko.nomtabla='ATVECO' 
                                        AND oko.sucursal='0000'
                                        LEFT JOIN tabla SBP
                                        ON SBP.codigo = tecni.codigo_voltaje
                                        and SBP.nomtabla = 'VOLTA'
                                        and SBP.sucursal = '0000'
                                WHERE   cliente.numero_cliente in ({0})
                                GROUP BY 1,2,3,4,5,6,7,8,9,10,11,12,13,14,15,16,17,18,19,20,21,22,23,24,25,26,27,28,29,30,31,32,33,34,35
                                ORDER BY 1";
        }

       public override string GetConsultaB2Win()
       {
           string sql = @"select    '' as ExternalId_Asset,
                                    nvl(trim(ggc_resumen.pot_conectada),0)    as im_charge,    --im_di_contrge,      --cargakwbr
                                    case when tarsap.valor_alf like '%verde%' then 'horosazonal verde' else
                                        case when tarsap.valor_alf like '%azul%' then 'horosazonal azul' else
                                        case when tarsap.valor_alf like '%horo azul%' then 'horo azul' else 'optante' end end end       as modtarifbr,
                                    trim(tarsap.valor_alf)                    as im_tariftyp_TARIFA,        -- categoria_tarie_br,
                                    trim(tarsyn.descripcion)                  as ac_kofiz_sd,        --classe_br,
                                    trim(tarsap.descripcion)                  as im_temp_area,       --subclasse_br,
                                    cliente.potencia_cont_hp            as im_di_contrat  ,    --demanda_kvabr,
                                    cliente.potencia_cont_hp            as im_di_contrpt,      --demanda_ponta_br,
                                    cliente.potencia_cont_fp            as im_di_contrfp,      --demanda_fp_br,
                                    '0'                                 as capacidade_disjuntorbr,
                                    nvl(trim(desc_tabla('VOLTA', codigo_voltaje, '0000')), '') as valortensaobr,
                                    'não'                               as instalacao_padrao,
                                    'trifásica'                         as tipo_tensaobr,
                                    okl.transformador                   as potencia_kwbr,
                                    cliente.tipo_tensao,
                                    '' AS codigo_voltaje,
                                    '' AS Nis,
                                    '' AS Nb,
                                    '' as ExternalId_POD,
	                                '' as ExternalId_Asset,
                                    cliente.numero_cliente
                                from  gc_tecni okl
                                    join cliente
                                     on okl.numero_cliente = cliente.numero_cliente
                                    join tecni
                                    on tecni.numero_cliente = cliente.numero_cliente
                                    join gc_cliente
                                     on cliente.numero_cliente = gc_cliente.numero_cliente
                                    join ggc_resumen
                                     on ggc_resumen.numero_cliente = cliente.numero_cliente
                                    left join tabla tarsap
                                     on cliente.giro = left(tarsap.codigo,2)
                                       and gc_cliente.sub_clase = substr(tarsap.codigo,3,2)
                                       and cliente.tarifa = right(tarsap.codigo,2)
                                       and tarsap.nomtabla='TARSAP'
                                    left join tabla tarsyn
                                     on tarsyn.codigo =tarsap.codigo
                                       and tarsyn.nomtabla='TARSYN'
                                       and tarsyn.sucursal='0000'
                                where   cliente.numero_cliente in ({0})
                                  --AND   cliente.estado_cliente = 8
                                        --AND  cliente.sector = {1}
                                group by 1,2,3,4,5,6,7,8,9,10,11,12,13,14,15,16,17,18,19,20,21";

           return sql;
       }

    }
}

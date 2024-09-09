using Pangea.Dados.Base;
//using Pangea.Dados.Solucoes;
using Pangea.Entidades;
using Pangea.Entidades.DTO;
using Pangea.Entidades.Enumeracao;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

namespace Pangea.Dados
{
    public class AltaContratacaoGBDAO : AltaContratacaoDAO
    {
        public AltaContratacaoGBDAO(Empresa empresa) : base(empresa)
        {

        }

        public override string GetConsultaBase()
        {
            return @"SELECT
                        (select sociedad_sie2000 from insta)          AS BP_OPBUK,
                        TO_CHAR(tecni.fecha_conexion,'%Y-%m-%d')                                            AS DATA_VALIDITA,
                        cliente.numero_cliente              AS IM_ZZ_NUMUTE,
                        cliente.nombre                      AS BP_NAME_ORG1,
                        --sg.externalid_asset               AS BP_BPEXT, 


                        case when nvl(d.tipo_documento,'') = '' THEN
       	                    nvl(trim(cliente.rut),'') || nvl(trim(cliente.dv_rut),'')
                        else
                            nvl(trim(d.numero_doc),'') || nvl(trim(d.dv_documento),'')
                        end         AS BP_BPEXT,

                        case when nvl(d.tipo_documento,'') = '' THEN
       	                    nvl(trim(cliente.rut),'') || nvl(trim(cliente.dv_rut),'')
                        else
                            nvl(trim(d.numero_doc),'') || nvl(trim(d.dv_documento),'')
                        end          AS BP_ZZ_CODFISC,


                        case when nvl(d.tipo_documento,'') = '' THEN
       	                    (select descripcion from tabla t 
                                    where t.nomtabla = 'TIPIDE' 
                                    and t.sucursal = '0000' 
                                    and t.fecha_desactivac is null 
                                    and t.codigo = cliente.tipo_ident)
                        else
       	                    (select descripcion from tabla t 
                                    where t.nomtabla = 'TIPIDE' 
                                    and t.sucursal = '0000' 
                                    and t.fecha_desactivac is null 
                                    and t.codigo = d.tipo_documento)
                        end  AS TipoIdent,

                        --(select descripcion from tabla t where t.nomtabla = 'TIPIDE' and t.sucursal = '0000' and t.fecha_desactivac is null 
                        --        and t.codigo = cliente.tipo_ident)                                          AS TipoIdent,


                        cliente.direccion                   AS BP_STREET,
                        cliente.numero_casa                 AS BP_HOUSE_NUM1,      --*
                        cliente.giro                        AS GIRO,
                        cliente.cep                         AS BP_POST_CODE1,
                        (select nome from barrios b 
                                where b.codigo_barrio = cliente.comuna 
                                    and b.municipio = cliente.municipio)  AS BP_CITY2,        --*
                        (select l.descripcion from localidades l where l.localidad = cliente.municipio * 10)	AS BP_CITY1,            --*
                        cliente.mail                        AS BP_SMTP_ADDR,    --*
                        (select descripcion from giros where codigo_giros = trim(cliente.giro))             AS ATTIVITA,   --* 
                        ''                                  AS BP_EXECUTIVE,    --*
                        nvl(su.ind_zona,'U')                         AS IM_REGION,       --*
                        CASE WHEN nvl(cliente.potencia_inst_fp,0) = 0 THEN '15.0' 
			                ELSE TO_CHAR(ROUND(cliente.potencia_inst_fp/1000,2)) END                        AS IM_CHARGE,    --IM_DI_CONTRGE,      --CargaKWBR          --* ??
                        'Z0101000'                          AS IM_BRANCHE,   --as CNAE,
                        ''                                  AS ModTarifBR, 
                        TARSAP.valor_alf                    AS IM_TARIFTYP,        -- Categoria_Tarie_BR, 
                        cliente.dia_vencimento AS AC_ZAHLKOND,
                        TARSYN.descripcion                  AS AC_KOFIZ_SD,        --Classe_BR, 
                        TARSAP.descripcion                  AS IM_TEMP_AREA,       --SubClasse_BR, 
                        '0'                                 AS IM_DI_CONTRAT  ,    --Demanda_KVABR, 
                        '0'                                 AS IM_DI_CONTRPT,      --Demanda_Ponta_BR,   
                        '0'                                 AS IM_DI_CONTRFP,      --Demanda_FP_BR, 
                        '0'                                 AS Capacidade_DisjuntorBR, 
                        nvl(trim(desc_tabla('VOLTA', codigo_voltaje, '0000')), '220 V') AS ValorTensaoBR,
                        'B'                                 AS IM_GROUP_TENSION,
                        CASE WHEN SBP.valor_alf = '12' THEN '04'
	                        ELSE CASE WHEN SBP.valor_alf = '22' THEN '06'
	                        ELSE CASE WHEN SBP.valor_alf = '38' THEN '10'  
	                        ELSE '06' END END END            AS IM_SPEBENE,
                        'Não'                               AS instalacao_padrao, 

                        CASE WHEN NVL(m.clave_montri, '') = '' THEN	
		                    CASE WHEN UPPER(TRIM(cliente.tipo_ligacao)) = 'T' THEN 'Trifásica' 
			                ELSE CASE WHEN UPPER(TRIM(cliente.tipo_ligacao)) = 'B' THEN 'Bifásica' 
			                    ELSE CASE WHEN UPPER(TRIM(cliente.tipo_ligacao)) = 'M' THEN 'Monofásica' 
                                    ELSE ''
                                    END
			                    END 
		                    END
                        ELSE CASE WHEN UPPER(TRIM(m.clave_montri)) = 'T' THEN 'Trifásica' 
                            ELSE CASE WHEN UPPER(TRIM(m.clave_montri)) = 'B' THEN 'Bifásica' 
			                    ELSE 'Monofásica' 
			                    END 
		                    END
                        END                                         AS IM_FACTOR_4,         --Tipo_TensaoBR,
                                        
                        CASE WHEN cliente.potencia_inst_fp = 0 THEN '15.0' 
			                ELSE TO_CHAR(ROUND(cliente.potencia_inst_fp/1000, 2)) END       AS Potencia_KWBR,           --*  ??
                        'Baixa Tensão'                      AS tipo_tensao ,
                        (select cj.cod_conjunto_aneel
                            from cliente c
                                inner join hispostes hp on hp.cod_poste = c.codigo_pee
                                inner join hisal ha on ha.cod_alimentador = hp.cod_alimentador 
                                inner join conjuntos cj on cj.cod_conjunto = ha.cod_conjunto
                            where 
                                c.numero_cliente = cliente.numero_cliente
                            group by cj.cod_conjunto_aneel, cj.nome)                        AS IM_FACTOR_2

                FROM    cliente 
                        LEFT JOIN documento_cliente d
                            ON cliente.numero_cliente = d.numero_cliente
                            AND d.tipo_documento in ('002', '005', '006')
                        LEFT JOIN medid m 
	                        ON m.numero_cliente = cliente.numero_cliente  
                        JOIN tecni 
                            ON tecni.numero_cliente = cliente.numero_cliente 
                        JOIN grandes@clientes:tabla TARSAP
                            ON cliente.classe = LEFT(TARSAP.codigo,2) 
                            AND cliente.subclasse = SUBSTR(TRIM(TARSAP.codigo),3,2)  
                            AND cliente.tarifa =  SUBSTR(TRIM(TARSAP.codigo),5,2) 
                            AND TARSAP.nomtabla='TARSAP' 
                            AND TARSAP.sucursal='0000' 
                        JOIN grandes@clientes:tabla TARSYN 
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
                GROUP BY 1,2,3,4,5,6,7,8,9,10,11,12,13,14,15,16,17,18,19,20,21,22,23,24,25,26,27,28,29,30,31,32,33,34,35,36
                ORDER BY 1";
        }
    }
}

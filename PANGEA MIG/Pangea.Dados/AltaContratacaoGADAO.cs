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
    public class AltaContratacaoGADAO : AltaContratacaoDAO
    {
        public AltaContratacaoGADAO(Empresa empresa) : base(empresa)
        {

        }

       public override string GetConsultaBase()
        {
            return @"SELECT   (select sociedad_sie2000 from insta)          AS BP_OPBUK,
                                        case when tecni.fecha_conexion < tecni.fecha_ult_contrato  then TO_CHAR(tecni.fecha_ult_contrato,'%Y-%m-%d') else TO_CHAR(tecni.fecha_conexion,'%Y-%m-%d') end   AS DATA_VALIDITA,
                                        cliente.numero_cliente              AS IM_ZZ_NUMUTE,
                                        trim(cliente.nombre)                AS BP_NAME_ORG1,
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
                                        --        and t.codigo = cliente.tipo_ident)   AS TipoIdent,

                                        trim(cliente.direccion)             AS BP_STREET,
                                        cliente.end_numero                  AS BP_HOUSE_NUM1,
                                        trim(cliente.giro)                  AS GIRO,
                                        cliente.cep                         AS BP_POST_CODE1,
                                        '' AS BP_CITY2,
                                        nvl(case when nvl((select lo.descripcion from localidades lo where cliente.comuna = lo.municipio  
                                            and lo.localidad = cliente.comuna * 10), '') <> '' then (select trim(lo.descripcion) from localidades lo where cliente.comuna = lo.municipio  
                                            and lo.localidad = cliente.comuna * 10) 
                                            else trim(l.descripcion) end, '')                AS BP_CITY1,
                                        trim(gc_cliente.email)              AS BP_SMTP_ADDR,
                                        cliente.actividad_economic          AS ATTIVITA, 
                                        gc_cliente.ejecutivo                AS BP_EXECUTIVE,
                                        gc_cliente.localizacao              AS IM_REGION,
                                        NVL(ggc.pot_conectada,15)    AS IM_CHARGE,    --IM_DI_CONTRGE,      --CargaKWBR  
                                        substr(trim(oko.valor_alf),(charindex('|',trim(oko.valor_alf)) + 1),len(oko.valor_alf))                     AS IM_BRANCHE,   --as CNAE,
                                        CASE WHEN TARSAP.valor_alf LIKE '%verde%' THEN 'Horosazonal Verde' ELSE  
                                        CASE WHEN TARSAP.valor_alf LIKE '%azul%' THEN 'Horosazonal Azul' ELSE  
                                        CASE WHEN TARSAP.valor_alf LIKE '%horo azul%' THEN 'HORO AZUL' ELSE 'OPTANTE' END END END       AS ModTarifBR, 
                                        TARSAP.valor_alf                    AS IM_TARIFTYP,        -- Categoria_Tarie_BR, 
                                        cliente.dia_vencimento              AS AC_ZAHLKOND,
                                        TARSYN.descripcion                  AS AC_KOFIZ_SD,        --Classe_BR, 
                                        TARSAP.descripcion                  AS IM_TEMP_AREA,       --SubClasse_BR, 
                                        cliente.potencia_cont_hp            AS IM_DI_CONTRAT  ,    --Demanda_KVABR, 
                                        cliente.potencia_cont_hp            AS IM_DI_CONTRPT,      --Demanda_Ponta_BR,   
                                        cliente.potencia_cont_fp            AS IM_DI_CONTRFP,      --Demanda_FP_BR, 
                                        '0'                                 AS Capacidade_DisjuntorBR, 
                                        --NVL(desc_tabla('VOLTA', codigo_voltaje, '0000'), '') AS ValorTensaoBR, 
                                        'A'                                 AS IM_GROUP_TENSION,
                                        SBP.valor_alf                       AS IM_SPEBENE,
                                        'Não'                               AS instalacao_padrao, 
                                        'Trifásica'                         AS IM_FACTOR_4,         --Tipo_TensaoBR, 
                                        okl.transformador                   AS Potencia_KWBR, 
                                        cliente.tipo_tensao,
                                        (select cj.cod_conjunto_aneel
                                            from cliente c2
                                                inner join hispostes hp on hp.cod_poste = c2.codigo_pee
                                                inner join hisal ha on ha.cod_alimentador = hp.cod_alimentador 
                                                inner join conjuntos cj on cj.cod_conjunto = ha.cod_conjunto
                                            where 
                                                c2.numero_cliente = cliente.numero_cliente
                                            group by cj.cod_conjunto_aneel, cj.nome) AS IM_FACTOR_2

                                FROM    grandes@clientes:cliente
                                        LEFT JOIN clientes@clientes:documento_cliente d
                                            ON cliente.numero_cliente = d.numero_cliente
                                           AND d.tipo_documento in ('002', '005', '006')
                                        JOIN grandes@clientes:gc_tecni okl
                                            ON okl.numero_cliente = cliente.numero_cliente
                                        JOIN tecni 
                                            ON tecni.numero_cliente = cliente.numero_cliente 
                                        JOIN gc_cliente
                                            ON cliente.numero_cliente = gc_cliente.numero_cliente 
                                        JOIN ggc_resumen AS ggc
                                            ON ggc.numero_cliente = cliente.numero_cliente 
                                        JOIN tabla TARSAP 
                                            ON cliente.giro = LEFT(TARSAP.codigo,2) 
                                            AND gc_cliente.sub_clase = SUBSTR(TARSAP.codigo,3,2) 
                                            AND cliente.tarifa = RIGHT(TARSAP.codigo,2) 
                                            AND TARSAP.nomtabla='TARSAP' 
                                        JOIN tabla TARSYN 
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
                                        LEFT JOIN grandes@clientes:localidades l
                                            ON l.localidad = cliente.localidad 
                                WHERE   cliente.numero_cliente in ({0})
                                GROUP BY 1,2,3,4,5,6,7,8,9,10,11,12,13,14,15,16,17,18,19,20,21,22,23,24,25,26,27,28,29,30,31,32,33,34,35
                                ORDER BY 1";
        }
    }
}

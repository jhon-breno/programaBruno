select '' identificador_conta,
c.nombre,
c.tipo_ident,
nvl(trim(c.rut),'') || nvl(trim(c.dv_rut),'') documento_cliente,
case when gc.email not like '%@%' or gc.email is null then '' else replace(replace(replace(trim(gc.email),chr(13),''),chr(10), ''),'|','')  end as mail,
trim(c.telefono) as telefone1,
trim(gc.tel_contato) as telefono2,
'' as telefono3,
'BRL' moneda,
nvl(desc_tabla('TIPIDE',(case 
    when c.tipo_ident = '002' then '002'
	when c.tipo_ident = '003' then '005'
	else
	'001' 
 end), '0000'), '') tipo_registro,
'' fecha_nasc,--grandes não tem
c.numero_cliente cuenta_principal,
'' Apellido_materno,
nvl((replace(replace(trim(c.direccion),chr(13),''),chr(10), '')), '') direccion,
'' ejecutivo,
nvl(desc_tabla('GIROS', c.giro, '0000'), '') giro,
nvl(desc_tabla('GIROS', c.giro, '0000'), '') clase_servicio,
i.sociedad_sie2000 id_empresa,
case when c.tipo_ident in ('003','002')
then c.nombre else '' end razao_social,
--+++++FIM CONTA++++--
--+++++INICIO CONTATO++++--
'' identificador_contacto,
c.nombre nombre_contacto,
'' apellido,
'' saludo,
'' identificador_conta_contato,
'' estado_civil_contato, -- gc nao tem
'' genero_contato, --gc nao tem
'' tipo_identidade_contato,
'' numero_identidade_contato,
'' fase_ciclo_vida, --gc nao tem
'' estrato,
'' nivel_educacion, --gc nao tem
'' permite_uso_datos_personales,
'' no_llamar, --gc nao tem
'' no_recibir_correo_eletronico, --gc nao tem
'' profesion,
'' ocupacion,
'' fecha_nascimento,
'' canal_preferente_contato,
'' correo_eletronico_secundario,
''telefono1_contato,
'' telefono2_contato,
'BRL' moneda_contato,
''apellido_materno_contato,
'' tipo_acreditacion,
nvl((replace(replace(trim(c.direccion),chr(13),''),chr(10), '')), '')  direccion_contato,
'' nombre_usuario_twitter,
'' recuento_seguidores_twitter,
'' influencia,
'' tipo_influencia,
'' biografia_twitter,
'' id_usuario_twitter,
'' nombre_usuario_facebook,
'' id_usuario_facebook,
'' id_empresa_contato,
---+++++FIM CONTATO+++++---
---+++++INICIO POD++++++---
''   identificador_pod,
c.numero_cliente numero_pod,
'BRL' moneda_pod,
c.dv_numero_cliente digito_verificador_pod,
nvl((replace(replace(trim(c.direccion),chr(13),''),chr(10), '')), '') direccion_pod,
c.estado_cliente estado_pod,
'BRASIL' pais,
i.sociedad_sie2000||c.comuna comuna,
'A' tipo_segmento,
'' medida_disciplina,
'' id_empresa_pod,
gc.ind_corte_vital electrodependiente,
nvl(desc_tabla('CODTAR', c.tarifa, '0000'), '') tarifa,
--c.tarifa tarifa,
'' tipo_agrupacion,
'' full_electric,
'' nombre_boleta,
c.sector||c.localidad||c.zona||c.correlativo_ruta||c.dv_ruta_lectura ruta,
replace(replace(c.direccion,chr(13),''),chr(10), '') direccion_reparto,
b.nome comuna_reparto,

m.propiedad_medidor propiedad_medidor,
m.modelo modelo_medidor,
m.marca_medidor marca_medidor,
m.numero_medidor numero_medidor,

'' num_transformador,--t.cod_transformador num_transformador,
'' tipo_transformador,--t.tipo_tranformador tipo_transformador,

m.clave_montri tipo_conexion,

'' estrato_socioeconomico,
te.nro_subestacion subestacion_electrica_conexion,

(select tipo_medidor from modmed where marca_medidor = m.marca_medidor and modelo = m.modelo) tipo_medida,

t.alimentador num_alimentador,--43
'' tipo_lectura,--44
'' bloque,--45
'' horario_racionamiento,--46

c.estado_suministro estado_conexao,

to_char(c.fecha_a_corte, '%Y-%m-%d') fecha_corte,

case when (c.codigo_pee is null or c.codigo_pee = '') then        
                'UC' || c.numero_cliente  
else        
                c.codigo_pee     
END codigo_PCR,

'' SED,
'' set_cliente,

'' llave,
'' potencia_capacidad_instalada,
'' cliente_singular,

c.giro clase_servicio_pod,
'A' subclase_servicio,
'' ruta_lectura,
'' tipo_facturacion,
'' mercado,
'' carga_aforada,
m.cr_ano ano_fabricacion,
'' centro_poblado,
---+++++FIM POD+++++---
---+++++INICIO ASSET+++++---
'' identificador_asset,

'' Nombre_del_activo,
'' identificador_conta_asset,

'' identificador_contato_asset,
'' suministro,
'' descripcion,
'' producto,--codigo solucoes_cliente
'' Estado, 
'B2B' contrato,
---+++++FIM ASSET+++++---
---+++++INICIO ADDRESS+++++---
'BRL' Moneda_address,
--(select dica from temp_clientes_localizacao where numero_cliente = c.numero_cliente) esquina_via_sec,
'' esquina_via_sec,
c.end_numero numero,

c.end_compl complemento,
replace(replace(c.cep,chr(13),''),chr(10), '') cep,
'' identificador_address,

'' identificador_street_address,
b.nome barrio,
'' tipo_numeracion,--1
'' direccion_concatenada,--2
c.sector bloque_direccion,--3
--clientes_coord.lng coord_x,--4 --Coordenadas estão invertidas no bd
--clientes_coord.lat coord_y,--5
'' coord_x,
'' coord_y,
'' nombre_agrupacion,--6
'' tipo_agrupacion_address,--7
'' tipo_interior,--8
'' direccion_larga,--9
'' lote_manzana,--10
'' tipo_sector,--11
---+++++FIM ADDRESS+++++---
---+++++INICIO STREET+++++---
--i.sociedad_sie2000||logr.municipio||logr.codigo_logra identificador_street,
'' identificador_street,

nvl((replace(replace(trim(c.direccion),chr(13),''),chr(10), '')), '') nombre_calle,

'' tipo_calle,--picklist de tipo logradouro

nvl(case when nvl((select lo.descripcion from localidades lo where c.comuna = lo.municipio  
and lo.localidad = c.comuna * 10), '') <> '' then (select trim(lo.descripcion) from localidades lo where c.comuna = lo.municipio  
and lo.localidad = c.comuna * 10) 
else trim(l.descripcion) end, '') ciudad,

case when i.sociedad_sie2000 = '2005' then 'RJ' else 'CE' end uf,

'BRASIL' pais_street,

i.sociedad_sie2000||c.comuna comuna_street,

case when i.sociedad_sie2000 = '2005' then
                '33'
else
                '23'
end region,
'' calle,
i.sociedad_sie2000||c.localidad localidad,
b.nome barrio_street,
---+++++FIM STREET+++++---
---+++++INICIO BILLING+++++---
'' identificador_conta_billing,

'STATEMENT' Tipo,

'' identificador_address_billing,
'' apellido_paterno,

te.acometida_retirada tipo_rede,
b.codigo_barrio,
to_char(te.fecha_conexion,'%Y-%m-%d'),
nvl(xpto.string1,'')
from 
grandes@clientes:cliente c, 
outer grandes@clientes:medid m,  
grandes@clientes:gc_tecni t,
grandes@clientes:tecni te,   
grandes@clientes:insta i,  
 grandes@clientes:gc_cliente gc,
outer grandes@clientes:localidades l,
outer grandes@clientes:tabla giros,
outer clientes@clientes:barrios b,
outer clientes@clientes:acda xpto
where 1=1
and c.numero_cliente = m.numero_cliente 
--and te.fecha_conexion = today - parametroDias
and c.giro = giros.codigo
and giros.fecha_desactivac is null
and giros.nomtabla = 'GIROS'
and c.sucursal = giros.sucursal
and c.numero_cliente = t.numero_cliente
and m.numero_cliente = t.numero_cliente
and c.numero_cliente = te.numero_cliente
and m.numero_cliente = te.numero_cliente
and c.numero_cliente = gc.numero_cliente
and l.localidad = c.localidad 
and c.comuna = b.municipio 
and gc.bairro = b.codigo_barrio
and m.estado = 'I'
and m.clave_demanda != 'R'
and xpto.numero_cliente = c.numero_cliente;
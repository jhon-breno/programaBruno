SELECT
'' identificador_conta,
c.nombre,
case when nvl(d.tipo_documento,'') = '' then trim(c.tipo_ident)
else trim(d.tipo_documento) end tipo_identidade,
case when nvl(d.tipo_documento,'') = '' THEN
       	nvl(trim(c.rut),'') || nvl(trim(c.dv_rut),'')
 else
     nvl(trim(d.numero_doc),'') || nvl(trim(d.dv_documento),'')
end documento_cliente,

case when c.mail not like '%@%' or c.mail is null then '' else replace(replace(c.mail,chr(13),''),chr(10), '')  end as mail,

case when tf.tipo_telefone = '01' and nvl(tf.numero_telefone, '') <> ''
and tf.ind_contato = 'S' then tf.prefixo_ddd||tf.numero_telefone
when nvl(c.telefono, '') <> '' then c.ddd||c.telefono
else '' end as telefone1,
case when nvl(c.telefono2, '') <> '' then c.ddd2||c.telefono2
when tf.tipo_telefone = '01' and nvl(tf.numero_telefone, '') <> ''
and tf.ind_contato = 'N' then tf.prefixo_ddd||tf.numero_telefone
else '' end as telefone2,
case when cel.tipo_telefone = '03' and nvl(cel.numero_telefone, '') <> ''
then cel.prefixo_ddd||cel.numero_telefone
else '' end as telefone3,
'BRL' Moneda,
nvl(desc_tabla('TIPIDE', (case when trim(d.tipo_documento) in ('002', '005', '006') then trim(d.tipo_documento)
when trim(c.tipo_ident) in ('002', '005', '006') then c.tipo_ident
when trim(c.tipo_ident2) in ('002', '005', '006') then trim(c.tipo_ident2) end), '0000'), '') as tipo_registro,
case when nvl(c.data_nasc, '') <> '' then to_char(c.data_nasc, '%Y-%m-%d')
else '' end as fecha_nasc,
c.numero_cliente cuenta_principal,
nome_mae Apellido_materno,
nvl((replace(replace(trim(c.direccion),chr(13),''),chr(10), '')), '') direccion,
'' ejecutivo,
g.descripcion giro,
nvl(desc_tabla('CLASSE', c.classe, '0000'), '') clase_servicio,
i.sociedad_sie2000 id_empresa,
case when c.tipo_ident = '002'
then c.nombre else '' end razao_social,
'' identificador_contacto,
c.nombre nombre_contacto,
'' apellido,
'' saludo,
''identificador_conta_contato,
desc_tabla('ESTCIV', trim(c.estado_civil), '0000')estado_civil_contato,
nvl(trim(case when nvl(desc_tabla('SEXO', c.sexo, '0000'), '') is not null then
desc_tabla('SEXO', c.sexo, '0000')
when nvl(desc_tabla('GRRE2A', c.sexo, '0000'), '') is not null then
desc_tabla('GRRE2A', c.sexo, '0000') end), '')  genero_contato,
'' tipo_identidade_contato,
'' numero_identidade_contato,
'' fase_ciclo_vida,
'' estrato,
trim(desc_tabla('ESCOLA', c.escolaridade, '0000')) nivel_educacion,
'' permite_uso_datos_personales,
Case when NVL(tf.numero_telefone,'') <> '' then
tf.ind_contato
Else
'S'
End no_llamar,
cca.ind_aut_email no_recibir_correo_eletronico,
trim(desc_tabla('PROFIS', c.profissao, '0000'))profesion,
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
''   identificador_pod,
c.numero_cliente numero_pod,
'BRL' moneda_pod,
c.dv_numero_cliente digito_verificador_pod,
nvl((replace(replace(trim(c.direccion),chr(13),''),chr(10), '')), '') direccion_pod,
c.estado_cliente estado_pod,
'BRASIL' pais,
i.sociedad_sie2000||c.municipio comuna,
case when c.estado_cliente = 8
then 'A'
else
'B' end tipo_segmento,
'' medida_disciplina,
'' id_empresa_pod,
c.ind_cliente_vital electrodependiente,
	tar.descripcion tarifa,
	'' tipo_agrupacion,
	'' full_electric,
	'' nombre_boleta,
	c.sector||c.localidade||c.zona||c.correlativo_ruta||c.dv_ruta_lectura ruta,
	replace(replace(c.direccion,chr(13),''),chr(10), '') direccion_reparto,
b.nome comuna_reparto,
m.propiedad_medidor propiedad_medidor,
m.modelo modelo_medidor,
m.marca_medidor marca_medidor,
m.numero_medidor numero_medidor,
t.cod_transformador num_transformador,
t.tipo_tranformador tipo_transformador,
c.tipo_ligacao tipo_conexion,
'' estrato_socioeconomico,
t.nro_subestacion subestacion_electrica_conexion,--41
(select tipo_medidor from modmed where marca_medidor = m.marca_medidor and modelo = m.modelo) tipo_medida,
t.alimentador num_alimentador,--43
'' tipo_lectura,--44
'' bloque,--45
'' horario_racionamiento,--46
c.estado_suministro estado_conexao,
nvl(c.fecha_a_corte, '')fecha_corte,
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
c.classe clase_servicio_pod,
case when c.estado_cliente != 8 then 'B' else 'A' end subclase_servicio,
'' ruta_lectura,
'' tipo_facturacion,
'' mercado,
'' carga_aforada,
m.cr_ano ano_fabricacion,
'' centro_poblado,
'' identificador_asset,
'' Nombre_del_activo,--nome solucoes_produto
'' identificador_conta_asset,
'' identificador_contato_asset,
'' suministro,
'' descripcion,
'' producto,--codigo solucoes_cliente
'' Estado,
case
when c.tipo_ident = '002' then 'B2B'
when c.tipo_ident = '005' then 'B2C'
else 'B2C' end contrato,
'BRL' Moneda_address,
'' esquina_via_sec,
c.numero_casa numero,
c.complemento complemento,
replace(replace(c.cep,chr(13),''),chr(10), '') cep,
'' identificador_address,
'' identificador_street_address,
b.nome barrio,
'' tipo_numeracion,--1
'' direccion_concatenada,--2
c.sector bloque_direccion,--3
clientes_coord.lng coord_x,--4
clientes_coord.lat coord_y,--5
'' nombre_agrupacion,--6
'' tipo_agrupacion_address,--7
'' tipo_interior,--8
'' direccion_larga,--9
'' lote_manzana,--10
'' tipo_sector,--11
'' identificador_street,
nvl((replace(replace(trim(c.direccion),chr(13),''),chr(10), '')), '') nombre_calle,
'' tipo_calle,
nvl(case when nvl((select lo.descripcion from localidades lo where c.municipio = lo.municipio
and lo.localidad = c.municipio * 10), '') <> '' then (select trim(lo.descripcion) from localidades lo where c.municipio = lo.municipio
and lo.localidad = c.municipio * 10)
else trim(l.descripcion) end, '') ciudad,
case when i.sociedad_sie2000 = '2005' then 'RJ' else 'CE' end uf,

'BRASIL' pais_street,

i.sociedad_sie2000||c.municipio comuna_street,

case when i.sociedad_sie2000 = '2005' then
                '33'
else
                '23'
end region,
'' calle,
i.sociedad_sie2000||c.localidade localidad,
b.nome barrio_street,
'' identificador_conta_billing,
'STATEMENT' Tipo,
'' identificador_address_billing,
'' apellido_paterno,
t.acometida_retirada tipo_rede,
tp.codigo_modif,
tp.grupo
FROM
CLIENTE C,
temp_clientes_modif tp,
outer documento_cliente d,
outer telefone_cliente tf,
outer telefone_cliente cel,
outer tabla l,
outer medid m,
outer tecni t,
outer barrios b,
insta i,
outer giros g,
outer clientes_coord,
outer cliente_cad_atend cca,
outer tarifa tar
WHERE
c.numero_cliente = d.numero_cliente
and tp.numero_cliente = c.numero_cliente
and tp.grupo = 'B'
--and te.fecha_conexion < parametroDias
and c.numero_cliente = tf.numero_cliente
and c.numero_cliente = cel.numero_cliente
and c.municipio = l.codigo  --tabla munici
and l.sucursal = c.sucursal  --tabla munici
and l.fecha_desactivac is null  --tabla munici
and m.numero_cliente = c.numero_cliente
and t.numero_cliente = c.numero_cliente
and c.municipio = b.municipio
and c.comuna = b.codigo_barrio
and c.codigo_pee = clientes_coord.codigo_pee
and c.giro = g.codigo_giros
and c.numero_cliente = cca.numero_cliente
and c.tarifa = tar.tarifa
and l.nomtabla = 'MUNICI'
and tf.tipo_telefone = '01'
and cel.tipo_telefone = '03'
and m.estado = 'I'
and c.estado_cliente != 8
and d.tipo_documento in ('002', '005', '006');
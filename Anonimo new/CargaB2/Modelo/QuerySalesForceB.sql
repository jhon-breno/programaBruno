SELECT 
    '' AS identificador_conta,--1
c.nombre,--2

case when nvl(d.tipo_documento,'') = ''  then trim(c.tipo_ident)
else trim(d.tipo_documento) end tipo_identidade,--3

case when nvl(d.tipo_documento,'') = ''  THEN
       nvl(trim(c.rut),'') || nvl(trim(c.dv_rut),'')
else
     nvl(trim(d.numero_doc),'') || nvl(trim(d.dv_documento),'')
end documento_cliente,  --4

case when c.mail not like '%@%' or c.mail is null then ''  else replace(replace(c.mail,chr(13),''),chr(10), '')  end as mail, --5

case when tf.tipo_telefone = '01' and nvl(tf.numero_telefone, '') <> ''
and tf.ind_contato = 'S' then tf.prefixo_ddd||tf.numero_telefone
when nvl(c.telefono, '') <> ''  then c.ddd||c.telefono
else ''  end as telefone1,--6

case when nvl(c.telefono2, '') <> ''  then c.ddd2||c.telefono2
when tf.tipo_telefone = '01' and nvl(tf.numero_telefone, '') <> ''
and tf.ind_contato = 'N' then tf.prefixo_ddd||tf.numero_telefone
else ''  end as telefone2,    --7

case when cel.tipo_telefone = '03' and nvl(cel.numero_telefone, '') <> ''
then cel.prefixo_ddd||cel.numero_telefone
else ''  end as telefone3,--8

'BRL' Moneda,  --9
nvl(desc_tabla('TIPIDE', (case when trim(d.tipo_documento) in ('002', '005', '006') then trim(d.tipo_documento)
when trim(c.tipo_ident) in ('002', '005', '006') then c.tipo_ident
when trim(c.tipo_ident2) in ('002', '005', '006') then trim(c.tipo_ident2) end), '0000'), '') as tipo_registro, --10

case when nvl(c.data_nasc, '') <> ''  then to_char(c.data_nasc, '%Y-%m-%d')
else ''  end as fecha_nasc, --11

c.numero_cliente cuenta_principal,       --12
''  Apellido_materno,     --13
nvl((replace(replace(trim(c.direccion),chr(13),''),chr(10), '')), '') direccion,  --14
''  ejecutivo,        --15
g.descripcion giro,--16
nvl(desc_tabla('CLASSE', c.classe, '0000'), '') clase_servicio,--17
i.sociedad_sie2000 id_empresa,   --18

case when c.tipo_ident = '002'
then c.nombre else ''  end razao_social, --19

'' AS identificador_contacto, --20
c.nombre nombre_contacto,    --21
''  apellido,--22
''  saludo,  --23
''  identificador_conta_contato, --24
''   estado_civil_contato,  --25

''   genero_contato, --26
''  tipo_identidade_contato,--27
''  numero_identidade_contato,--28
''  fase_ciclo_vida,--29
''  estrato, --30
'' nivel_educacion,       --31
''  permite_uso_datos_personales,--32

Case when NVL(tf.numero_telefone,'') <> ''  then
tf.ind_contato
Else
'S'
End no_llamar, --33

cca.ind_aut_email no_recibir_correo_eletronico, --34
''  profesion,   --35
''  ocupacion,      --36
''  fecha_nascimento,     --37
''  canal_preferente_contato,     --38
''  correo_eletronico_secundario,  --39
''  telefono1_contato,     --40
''  telefono2_contato,    --41
'BRL' moneda_contato,--42
''apellido_materno_contato,     --43
''  tipo_acreditacion,       --44
nvl((replace(replace(trim(c.direccion),chr(13),''),chr(10), '')), '')  direccion_contato,  --45
''  nombre_usuario_twitter,        --46
''  recuento_seguidores_twitter,--47
''  influencia,       --48
''  tipo_influencia, --49
''  biografia_twitter,        --50
''  id_usuario_twitter,    --51
''  nombre_usuario_facebook,   --52
''  id_usuario_facebook,--53
''  id_empresa_contato,--54
'' AS identificador_pod,      --55
c.numero_cliente numero_pod,   --56
'BRL' moneda_pod,        --57
c.dv_numero_cliente digito_verificador_pod,   --58
nvl((replace(replace(trim(c.direccion),chr(13),''),chr(10), '')), '') direccion_pod,  --59
c.estado_cliente estado_pod,   --60
'BRASIL' pais,     --61
i.sociedad_sie2000||c.municipio comuna, --62
'B' tipo_segmento,--63
''  medida_disciplina,      --64
''  id_empresa_pod,        --65
c.ind_cliente_vital electrodependiente,--66
tar.descripcion tarifa,    --67
''  tipo_agrupacion,--68
''  full_electric,   --69
''  nombre_boleta,--70
c.sector||c.localidade||c.zona||c.correlativo_ruta||c.dv_ruta_lectura ruta,     --71
replace(replace(c.direccion,chr(13),''),chr(10), '') direccion_reparto,       --72
b.nome comuna_reparto,--73
m.propiedad_medidor propiedad_medidor,      --74
m.modelo modelo_medidor,    --75
m.marca_medidor marca_medidor,      --76
m.numero_medidor numero_medidor,--77
t.cod_transformador num_transformador,--78
t.tipo_tranformador tipo_transformador,--79
c.tipo_ligacao tipo_conexion,   --80
''  estrato_socioeconomico,        --81
t.nro_subestacion subestacion_electrica_conexion,       --82
''  tipo_medida,--83
t.alimentador num_alimentador, --84
''  tipo_lectura,  --85
''  bloque,  --86
''  horario_racionamiento,--87
c.estado_suministro estado_conexao,  --88
to_char(h.data_apto_corte,'%Y-%m-%d'),--89

case when (c.codigo_pee is null or c.codigo_pee = '') then 'UC' || c.numero_cliente
else c.codigo_pee END codigo_PCR, --90

''  SED,   --91
''  set_cliente,    --92
t.llave,  --93

'' AS potencia_capacidad_instalada,--CargaKWBR
--''  AS potencia_capacidad_instalada,  --94

''  cliente_singular, --95
c.classe clase_servicio_pod,       --96
case when c.estado_cliente != 8 then 'B' else 'A' end subclase_servicio,--97
c.sector||c.localidade||c.zona||c.correlativo_ruta||c.dv_ruta_lectura ruta,     --98

m.clave_montri AS tipo_facturacion,--99
''  mercado,--100
''  carga_aforada,  --101
m.cr_ano ano_fabricacion,--102
''  centro_poblado, --103
'' AS identificador_asset,   --104
''  Nombre_del_activo,  --105
''  identificador_conta_asset,     --106
''  identificador_contato_asset, --107
''  suministro,     --108
''  descripcion,    --109
''  producto,--codigo solucoes_cliente,        --110
''  Estado,  --111
case
when c.tipo_ident = '002' then 'B2B'
when c.tipo_ident = '005' then 'B2C'
else 'B2C' end contrato,--112
'BRL' Moneda_address,--113
replace(replace(c.dica_localizacao,chr(13),''),chr(10), '') esquina_via_sec,--114
c.numero_casa numero,  --115
c.complemento complemento, --116
replace(replace(c.cep,chr(13),''),chr(10), '') cep,--117

''  identificador_address,   --118
''  identificador_street_address,--119
b.nome barrio, --120
''  tipo_numeracion,       --121
c.direccion direccion_concatenada,--122
c.sector bloque_direccion, --123
clientes_coord.lng coord_x,       --124
clientes_coord.lat coord_y,        --125
''  nombre_agrupacion, --126
''  tipo_agrupacion_address,       --127
''  tipo_interior, --128
''  direccion_larga, --129
''  lote_manzana,  --130
''  tipo_sector,   --131
''  identificador_street,  --132
nvl((replace(replace(trim(c.direccion),chr(13),''),chr(10), '')), '') nombre_calle,   --133
''  tipo_calle,       --134
case when i.sociedad_sie2000 = '2005' then 'RJ' else 'CE' end uf,   --135
'BRASIL' pais_street,      --136
i.sociedad_sie2000||c.municipio comuna_street,--137

nvl(case 
   when nvl((select lo.descripcion 
 from localidades lo 
where c.municipio = lo.municipio
        and lo.localidad = c.municipio * 10), '') <> ''  then 
  (select trim(lo.descripcion) 
     from localidades lo 
    where c.municipio = lo.municipio
      and lo.localidad = c.municipio * 10)
else 
   trim(l.descripcion) end, '')  as ciudad,

case when i.sociedad_sie2000 = '2005' then '33' else '23' end region,        --138

''  calle,  --139
i.sociedad_sie2000||c.localidade localidad,        --140
b.nome barrio_street,  --141
''  identificador_conta_billing,    --142
'STATEMENT' Tipo,--143
''  identificador_address_billing,--144
''  apellido_paterno,       --145
t.acometida_retirada tipo_rede,  --146
b.codigo_barrio,   --147
to_char(t.fecha_conexion,'%Y-%m-%d') AS data_instalacao,        --148
       s2.ind_zona  AS localizacao, --149 -- 149
       ''  AS conta_cliente,--nvl(xpto.string1,''),        --150      -- 150
       ''  AS nomeresponsavel, -- 151
       ''  AS emailresponsavel,-- 152
       ''  AS tipoDocumentoResponsavel,-- 153
       ''  AS documentoresponsavel,-- 154
       ''  AS cargoresponsavel,-- 155
       ''  AS telefone1Responsavel,-- 156
       ''  AS telefone2Responsavel,-- 157
       ''  AS documento_conta,-- 158
       ''  AS inscricao_estadual,-- 159
       ''  AS inscricao_municipal,-- 160
       'B',-- TIPO_CLIENTE161
       to_char(h.data_reaviso,'%Y-%m-%d'), --162
       ''  AS ejecutivo,   --163
       a.cod_entidad  AS cod_entidad, --164
       '', --165
       a.conta_corrente || a.dv_cc  AS cuenta_corriente,-- 166
       c.dia_vencimento,      -- 167
       '',--168
       '',--169
       '',--170
       ''  AS recordType,--171
       case when (nvl(lec.sector, '') != '') then
       'Bymonthly'
       else
        'Monthly'
       end 
FROM
CLIENTE C,
temp_clientes_modif sg,
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
outer hisreav h,
outer tarifa tar,
outer susec s2,
outer leitura_especial_config lec,
outer maeaut a
WHERE 1=1
and c.numero_cliente = sg.numero_cliente
and c.numero_cliente = d.numero_cliente
and d.tipo_documento in ('002', '005', '006')
and c.numero_cliente = tf.numero_cliente
and c.numero_cliente = cel.numero_cliente
and h.numero_cliente= c.numero_cliente
and h.corr_reaviso = c.corr_reaviso
and h.estado = 'V'
and c.sector = s2.sector 
and c.zona = s2.zona 
and c.localidade = s2.localidade
and c.municipio = s2.municipio
and s2.sucursal = c.sucursal
and l.nomtabla = 'MUNICI'
and l.sucursal = '0000'  
and l.fecha_desactivac is null  
and l.codigo = c.municipio
and m.numero_cliente = c.numero_cliente
and t.numero_cliente = c.numero_cliente
and c.municipio = b.municipio
and c.comuna = b.codigo_barrio
and c.codigo_pee = clientes_coord.codigo_pee
and c.giro = g.codigo_giros
and c.numero_cliente = cca.numero_cliente
and c.tarifa = tar.tarifa
and tf.tipo_telefone = '01'
and cel.tipo_telefone = '03'
and m.estado = 'I'
and c.estado_cliente != 8
and not exists (select 1 from grandes:cliente gc where gc.numero_cliente = c.numero_cliente)
and lec.sector = c.sector
and lec.localidade_inicial = c.localidade
and lec.zona_inicial = c.localidade
and nvl(lec.fim, '') = ''
and a.numero_cliente = c.numero_cliente
and a.estado = 'A'
and nvl(a.data_exclusion,'') = ''

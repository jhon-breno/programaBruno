SELECT 	--'' identificador_conta,	
		'' identificador_conta,						-- 0
		c.nombre,															-- 1
		CASE
		WHEN c.tipo_ident = '002' THEN '002'
		WHEN c.tipo_ident = '003' THEN '005' end tipo_identidade,										-- 2
		NVL(TRIM(c.rut),'') || NVL(TRIM(c.dv_rut),'') documento_cliente,	-- 3
		--CASE 
		--	WHEN gc.email NOT LIKE '%@%' OR gc.email IS NULL THEN '' ELSE REPLACE(REPLACE(REPLACE(TRIM(gc.email),CHR(13),''),CHR(10), ''),'|','')  END AS mail,
		gc.email mail,																			-- 4
		NVL(ddd_contato,'')||' '||TRIM(c.telefono) AS telefone1,								-- 5
		NVL(ddd_contato,'')||' '||TRIM(gc.tel_contato) AS telefono2,							-- 6
		'' AS telefono3,																		-- 7
		'BRL' moneda,																			-- 8
		NVL(desc_tabla('TIPIDE',(	CASE
		WHEN c.tipo_ident = '002' THEN '002'
		WHEN c.tipo_ident = '003' THEN '005'
		ELSE
			'001'
		END), '0000'), '') tipo_registro,														-- 9
		'' fecha_nasc,--grandes não tem															-- 10
		c.numero_cliente cuenta_principal,														-- 11
		'' Apellido_materno, 																	-- 12
		c.direccion direccion,																	-- 13
		--ex.nombre AS ejecutivo, -- corrigido(Bruno Giminiani)									-- 14
		ex.codigo AS ejecutivo,                                                                 -- 14
		NVL(desc_tabla('GIROS', c.giro, '0000'), '') giro,										-- 15		
		'clase_servicio' clase_servicio,														-- 16
		--NVL((REPLACE(REPLACE(TRIM(c.direccion),CHR(13),''),CHR(10), '')), '') direccion,      								
		i.sociedad_sie2000 id_empresa,															-- 17
		CASE WHEN c.tipo_ident IN ('003','002')								                    
		THEN c.nombre ELSE '' END razao_social,													-- 18
		gc.contacto identificador_contacto,-- (Bruno/Léo/Rafa)									-- 19
		c.nombre nombre_contacto,																-- 20
	--+++++FIM CONTA++++--	                                                                    
	--+++++INICIO CONTATO++++--                                                                 
		--'' identificador_contacto,	                                                        
		'' apellido,																			-- 21
		'' saludo,																				-- 22
		'' identificador_conta_contato,															-- 23				(Bruno/Léo/Rafa - verificar com o Werneck)		
		--representante.no_nome,                                                                	
		--representante.cargo,                                                                  
		--GROUP BY c.nombre,tb.no_nome, cargo, profissao                                        
		--HAVING MAX(tb.no_nome) ) t) x,                                                        
		--desc_tabla('TIPIDE' , cd_tipo_doc_1 , '0000'), --(Rafael/Léo/Bruno)                   
		'' estado_civil_contato, -- gc nao tem													-- 24
		'' genero_contato, --gc nao tem															-- 25
		'' tipo_identidade_contato,																-- 26
		-- desc_tabla('TIPIDE' , c.cd_tipo_doc_1 , '0000') tipo_identidade_contato,             
		'' numero_identidade_contato,															-- 27
		'' fase_ciclo_vida, --gc nao tem														-- 28
		'' estrato,																				-- 29
		'' nivel_educacion, --gc nao tem														-- 30
		'' permite_uso_datos_personales,														-- 31
		'' no_llamar, --gc nao tem																-- 32
		'' no_recibir_correo_eletronico, --gc nao tem											-- 33
		'' profesion,																			-- 34
		'' ocupacion,																			-- 35
		'' fecha_nascimento,																	-- 36
		'' canal_preferente_contato,															-- 37
		'' correo_eletronico_secundario,														-- 38
		'' telefono1_contato,																	-- 39
		'' telefono2_contato,																	-- 40
		'BRL' moneda_contato,																	-- 41
		''apellido_materno_contato,																-- 42
		'' tipo_acreditacion,																	-- 43
		--NVL((REPLACE(REPLACE(TRIM(c.direccion),CHR(13),''),CHR(10), '')), '')  direccion_contato
		c.direccion direccion_contato,    														-- 44
		'' nombre_usuario_twitter,																-- 45
		'' recuento_seguidores_twitter,                                     					-- 46
		'' influencia,                                                      					-- 47
		'' tipo_influencia,                                                 					-- 48
		'' biografia_twitter,                                               					-- 49
		'' id_usuario_twitter,                                              					-- 50
		'' nombre_usuario_facebook,                                         					-- 51
		'' id_usuario_facebook,                                             					-- 52
		'' id_empresa_contato,                                              					-- 53
	---+++++FIM CONTATO+++++---                                             					
	---+++++INICIO POD++++++---                                             					
		'' identificador_pod,	                                            					-- 54
		c.numero_cliente numero_pod,	                                    					-- 55
		'BRL' moneda_pod,	                                                					-- 56
		c.dv_numero_cliente digito_verificador_pod,												-- 57
		--NVL((REPLACE(REPLACE(TRIM(c.direccion),CHR(13),''),CHR(10), '')), '') direccion_pod,
		c.direccion direccion_pod,																-- 58
		c.estado_cliente estado_pod,															-- 59
		'BRASIL' pais,	                                                    					-- 60
		i.sociedad_sie2000||c.comuna,                                                           -- 61
		'A' tipo_segmento,                                                  					-- 62
		'' medida_disciplina,                                               					-- 63
		'' id_empresa_pod,	                                                					-- 64
		gc.ind_corte_vital electrodependiente,	                            					-- 65
		NVL(TARSAP.valor_alf, '') tarifa,             					-- 66
		'' tipo_agrupacion,                                                 					-- 67
		'' full_electric,                                                   					-- 68
		'' nombre_boleta,                                                   					-- 69
		c.sector || ' ' || c.zona || ' ' || c.correlativo_ruta || ' ' || c.dv_ruta_lectura ruta,				-- 70
		replace(replace(c.direccion,chr(13),''),chr(10), '') direccion_reparto,					-- 71
		b.nome comuna_reparto,	                                                                -- 72
		m.propiedad_medidor propiedad_medidor,	                                                -- 73
		m.modelo modelo_medidor,                                                                -- 74
		m.marca_medidor marca_medidor,                                                          -- 75
		m.numero_medidor numero_medidor,                                                        -- 76
		gc.numero_poste num_transformador,--t.cod_transformador num_transformador,                           -- 77
		'' tipo_transformador,--t.tipo_tranformador tipo_transformador,                         -- 78
		m.clave_montri tipo_conexion,                                                           -- 79
		'' estrato_socioeconomico,                                                              -- 80
		te.nro_subestacion subestacion_electrica_conexion,										-- 81
		(select tipo_medidor from modmed where marca_medidor = m.marca_medidor and modelo = m.modelo) tipo_medida,	-- 82
		gc.cod_alimentador num_alimentador,															-- 83	
		'' tipo_lectura,                                                                        -- 84
		'' bloque,                                                                              -- 85
		'' horario_racionamiento,                                                               -- 86
		c.estado_suministro estado_conexao,	                                                    -- 87
		--to_char(c.fecha_a_corte, '%Y-%m-%d') fecha_corte,	                                    
		n.data_apto_corte,                                                                        -- 88
		case when (c.codigo_pee is null or c.codigo_pee = '') then       
						'UC' || c.numero_cliente 
		else       
						c.codigo_pee    
		END codigo_PCR,																			-- 89
		'' sed,																					-- 90
		'' set_cliente,                                                                         -- 91
		'' llave,                                                                               -- 92
		'' potencia_capacidad_instalada,                                                        -- 93
		'' cliente_singular,	                                                                -- 94
		c.giro clase_servicio_pod,                                                             -- 95
		--'A' subclase_servicio,                                                                -- 96
		gc.sub_clase subclase_servicio,                                                           -- 96
		'' ruta_lectura,                                                                        -- 97
		'MENSAL' tipo_facturacion,                                                              -- 98
		'' mercado,                                                                             -- 99
		'' carga_aforada,                                                                       -- 100
		m.cr_ano ano_fabricacion,																-- 101
		'' centro_poblado,																		-- 102
		---+++++FIM POD+++++---	
		---+++++INICIO ASSET+++++---
		'' identificador_asset,																	-- 103
		'' Nombre_del_activo,                                                                   -- 104
		'' identificador_conta_asset,                                                           -- 105
		'' identificador_contato_asset,                                                         -- 106
		'' suministro,                                                                          -- 107
		'' descripcion,                                                                         -- 108
		'' producto,                                                                            -- 109
		desc_tabla('ESTSUM', c.tarifa, '0000') estado,											-- 110
		'B2B' contrato,																			-- 111
		---+++++FIM ASSET+++++---	
		---+++++INICIO ADDRESS+++++---
		'BRL' moneda_address,																	-- 112
		--(select dica from temp_clientes_localizacao where numero_cliente = c.numero_cliente) esquina_via_sec,
		'' esquina_via_sec,																		-- 113
		c.end_numero numero,																	-- 114
		c.end_compl complemento,                                                                -- 115
		replace(replace(c.cep,chr(13),''),chr(10), '') cep,                                     -- 116
		'' identificador_address,                                                               -- 117
		'' identificador_street_address,                                                        -- 118
		--b.nome barrio,                                                                        
		gc.bairro,																				-- 119
		'' tipo_numeracion,																		-- 120
		c.direccion direccion_concatenada,                                                               -- 121
		c.sector bloque_direccion,                                                              -- 122
		----------------------
		te.gps_x coord_x,																		-- 123
		te.gps_y coord_y,																		-- 124
		----------------------
		--clientes_coord.lng coord_x,--4 --Coordenadas estão invertidas no bd
		--clientes_coord.lat coord_y,--5
		--'' coord_x,
		--'' coord_y,
		'' nombre_agrupacion,																	-- 125
		'' tipo_agrupacion_address,                                                             -- 126
		'' tipo_interior,                                                                       -- 127
		'' direccion_larga,                                                                     -- 128
		'' lote_manzana,                                                                        -- 129
		'' tipo_sector,                                                                         -- 130
		---+++++FIM ADDRESS+++++---	
		---+++++INICIO STREET+++++---
		--i.sociedad_sie2000||logr.municipio||logr.codigo_logra identificador_street,
		'' identificador_street,																	-- 131	
		'' nombre_calle,	    	-- 132
		'x' tipo_calle,--picklist de tipo logradouro												-- 133
		nvl(case when nvl((select lo.descripcion from localidades lo where c.comuna = lo.municipio 			
		and lo.localidad = c.comuna * 10), '') <> '' then (select trim(lo.descripcion) from localidades lo where c.comuna = lo.municipio 
		and lo.localidad = c.comuna * 10)
		else trim(l.descripcion) end, '') ciudad,													-- 134
		case when i.sociedad_sie2000 = '2005' then 'RJ' else 'CE' end uf,							-- 135
		'BRASIL' pais_street,																		-- 136
		i.sociedad_sie2000||c.comuna comuna_street,													-- 137
		
		case when i.sociedad_sie2000 = '2005' then
						'33'
		else
						'23'
		end region,																					-- 138
		
		'' calle,																					-- 139
		i.sociedad_sie2000||c.localidad localidad,													-- 140
		b.nome barrio_street,																		-- 141
		---+++++FIM STREET+++++---	
		---+++++INICIO BILLING+++++---
		'' identificador_conta_billing,																-- 142
		'STATEMENT' tipo,																			-- 143
		'' identificador_address_billing,															-- 144
		'' apellido_paterno,																		-- 145
		te.acometida_retirada tipo_rede,															-- 146
		b.codigo_barrio codigo_bairro,																-- 147
		to_char(te.fecha_conexion,'%Y-%m-%d') fecha_conexion,										-- 148
		gc.localizacao AS localizacao,																-- 149
		'',--nvl(xpto.string1,''),																		-- 150
		'', -- 151
		'',-- 152
		'',-- 153
		'',-- 154
		'',-- 155
		'',-- 156
		'',-- 157
		'',-- 158
		'',-- 159
		'',-- 160
		'',-- 161
		n.data_reaviso, --162
		gc.ejecutivo,   --163
		ma.cod_entidad,																				-- 164
        ma.agencia,																					-- 165
        ma.cuenta_corriente,																		-- 166
        c.dia_vencimento,																			-- 167
        '',
        '',
        '',
        '',
        'Monthly'
from 
grandes@clientes:cliente c, 
outer grandes@clientes:medid m,  
grandes@clientes:gc_tecni t,
grandes@clientes:tecni te,   
grandes@clientes:insta i,  
grandes@clientes:gc_cliente gc,
outer grandes@clientes:tabla TARSAP,
outer (grandes@clientes:localidades l, grandes@clientes:barrios b),
outer grandes@clientes:tabla giros,
outer grandes@clientes:executivo ex,
outer grandes@clientes:hisreav n,
outer grandes@clientes:maeaut ma,
temp_clientes_modif poc
where 1=1
--and c.numero_cliente = 9005385
and c.numero_cliente = m.numero_cliente 
and gc.ejecutivo = ex.codigo	
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
and c.localidad = b.localidad 
AND b.localidad = l.localidad  
AND gc.bairro = b.nome
AND m.estado = 'I'
and m.clave_demanda != 'R'
and n.numero_cliente = c.numero_cliente
and n.estado = 'V'
and ma.numero_cliente = c.numero_cliente
and ma.estado = 'A'
and n.corr_reaviso = c.corr_reaviso
and poc.numero_cliente = c.numero_cliente                                                                                   
AND LEFT(TARSAP.codigo,2) = c.giro                                                                 
AND SUBSTR(TARSAP.codigo,3,2) = gc.sub_clase                                                      
AND RIGHT(TARSAP.codigo,2)  = c.tarifa
AND TARSAP.nomtabla='TARSAP'
AND TARSAP.sucursal='0000'                                                       
order by c.rut;
select distinct numero_cliente, desc_tabla('MODIF', codigo_modif, '0000') descricao, codigo_modif, 'B' grupo from modif
where codigo_modif 
in(2,3,4,5,7,8,20,22,26,35,36,53,46,47,48,54,55,56,57,58,59,60,64,87,147,202,205,308)
and date(fecha_modif) >= today - parametroDias
union
select distinct numero_cliente, 'TROCA DE MEDIDOR' descricao, 'M' codigo_modif,'B' grupo from cli_med_processo
where data_atualizacao >= today - parametroDias
union 
select distinct numero_cliente, grandes@clientes:desc_tabla('MODIF', codigo_modif, '0000') || ' GA' descricao, codigo_modif, 'A' grupo 
from grandes@clientes:modif 
where codigo_modif 
in(2 ,3 ,4 ,5,6, 7,8,11,12,13,14,15,16,24,25,30,40,45,99,116,117,118,130,131,132,213,214,246,252,314,404,405,501,502,503,506,507,509,515,624,700,701,702)
and date(fecha_modif) >= today - parametroDias
into temp temp_clientes_modif with no log;


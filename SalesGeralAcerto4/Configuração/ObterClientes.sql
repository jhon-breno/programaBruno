create temp table cliente_temp_nao(numero_cliente integer);

load from temp_clientes.txt insert into cliente_temp_nao;

unload to /synergia/archivos/tmp/sales_geral_new.txt
select c.tipo_ident, c.rut, c.dv_rut, c.numero_cliente, c.nombre
from cliente c where           
c.numero_cliente not in ( select numero_cliente from cliente_temp_nao) 

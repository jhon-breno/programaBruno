--select c.numero_cliente, 'CLIENTES NOVOS' descricao from cliente c, tecni t
--where t.fecha_conexion = today - parametroDias 
--and c.numero_cliente = t.numero_cliente
--and nvl(c.cliente_anterior, 0) = 0
--union
--select c.numero_cliente, 'TROCA DE TITULARIDADE' descricao from cliente c, tecni t
--where t.fecha_conexion = today - parametroDias
--and c.numero_cliente = t.numero_cliente
--and nvl(c.cliente_anterior, 0) != 0

select c.numero_cliente, 'CLIENTES NOVOS' descricao from cliente c
where 1=1
{0} 
into temp temp_clientes_modif with no log;
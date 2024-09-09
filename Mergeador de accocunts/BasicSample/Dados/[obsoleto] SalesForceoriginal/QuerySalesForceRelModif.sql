SELECT count(numero_cliente), descricao, grupo
FROM temp_clientes_modif
GROUP BY descricao,grupo
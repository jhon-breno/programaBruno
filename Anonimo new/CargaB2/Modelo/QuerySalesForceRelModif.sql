SELECT count(numero_cliente), descricao
FROM temp_clientes_modif
GROUP BY descricao
import math

# Lê o arquivo casos.txt e armazena cada linha em uma lista
with open('lista.txt', 'r') as f:
    casos = f.read().splitlines()

# Calcula o número de casos que serão adicionados em cada arquivo
num_casos_por_arquivo = math.ceil(len(casos) / 15)

# Divide a lista de casos em 10 sub-listas
sublistas_de_casos = [casos[i:i+num_casos_por_arquivo] for i in range(0, len(casos), num_casos_por_arquivo)]

# Escreve cada sub-lista em um arquivo
for i, sublista in enumerate(sublistas_de_casos):
    nome_arquivo = f"lista_{i+1}.txt"
    with open(nome_arquivo, 'w') as f:
        for j in range(0, len(sublista), 7):
            casos_concatenados = ",".join([f"'{caso}'" for caso in sublista[j:j+7]])
            f.write(casos_concatenados + "\n")

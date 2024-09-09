using Newtonsoft.Json;
using SalesforceExtractor.Entidades;
using SalesforceExtractor.Entidades.Enumeracoes;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;

namespace SalesforceExtractor.Dados
{
    public class ExtratorGB : Extrator
    {
        public override List<ItemAttribute> GetItemAttributes(Arquivo arquivo, Type tipoItem)
        {
            StreamReader stream = new StreamReader(arquivo.CaminhoCompleto, Encoding.GetEncoding("iso-8859-1"));
            ItemAttribute asset = null;
            List<ItemAttribute> lista = new List<ItemAttribute>();
            var linha = string.Empty;

            if (arquivo.TemCabecalho)
                linha = stream.ReadLine();

            while (true)
            {
                try
                {
                    linha = stream.ReadLine();

                    if (string.IsNullOrEmpty(linha))
                        return lista;
                    
                    int carga = 15;
                    int potencia = 15;
                    try
                    {
                        string[] item = linha.Replace("\"", "").Split(arquivo.Separador);
                        asset = new ItemAttributeGB();

                        asset.ExternalIdAsset = typeof(ItemAttribute).Equals(tipoItem) ? item[18] : typeof(B2WinDTO).Equals(tipoItem) ? item[0] : string.Empty;
                        asset.NumeroCliente = typeof(ItemAttribute).Equals(tipoItem) ? item[0] : string.Empty;
                        //asset.ExternalIdAccount = item[1];
                        //asset.ExternalIdContract = item[2];
                        //asset.ExternalIdPod = item[3];
                        Int32.TryParse(item[1], NumberStyles.AllowDecimalPoint, CultureInfo.GetCultureInfo("en-US"), out carga);
                        asset.CargaKW = 74;
                        asset.ModalidadeTarifaria = item[2];        //GA
                        asset.CategoriaTarifa = item[3];
                        asset.Classe = item[4];
                        asset.SubClasse = item[5];
                        //GA--asset.DemandaKV = item[6];
                        //GA--asset.DemandaPonta = item[7];
                        //GA--asset.DemandaForaPonta = item[8];
                        asset.CapacidadeDisjuntor = item[9];
                        asset.ValorTensao = item[10];
                        asset.InstalacaoPadrao = item[11];
                        asset.TipoTensao = item[12];
                        Int32.TryParse(item[13], out potencia);
                        asset.PotenciaKWA = "74";
                        asset.NivelTensao = item[14];
                        //asset.Nis = item[15];
                        //asset.Nb = item[16];
                        asset.OrderItemId = DBNull.Value.Equals(item[21]) ? string.Empty : item[21];
                        lista.Add(asset);
                    }
                    catch (Exception ex)
                    {
                        //TODO: log
                        Console.WriteLine(string.Format("ERRO ao carregar o objeto do arquivo de entrada: {0}", linha));
                    }
                }
                catch (Exception ex)
                {
                    //TODO: log
                    Console.WriteLine(string.Format("ERRO inexperado: {0}", ex.Message));
                }
            }

            return lista;
        }
    }
}

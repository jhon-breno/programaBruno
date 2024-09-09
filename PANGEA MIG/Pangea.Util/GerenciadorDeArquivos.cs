using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;

namespace Pangea.Util
{
    public class GerenciadorDeArquivos
    {
        //private static AppSettingsReader configurationAppSettings;

        public static bool GerarArquivoTexto(StringBuilder sb, string nomeArquivo)
        {
            try
            {
                //Verificar existencia do arquivo caso exista excluir
                if (File.Exists(nomeArquivo))
                    File.Delete(nomeArquivo);

                FileInfo t = new FileInfo(nomeArquivo);

                using (StreamWriter arquivo = t.CreateText())
                {

                    arquivo.WriteLine(sb);
                    arquivo.Write(arquivo.NewLine);

                }

                return true;
            }
            //catch(Exception ex){
            catch { return false; }
        }

        public static List<string> LerArquivoTexto(string caminhoDoArquivo)
        {
            List<string> listaRetorno = new List<string>();
            string linha = string.Empty;

            try
            {
                using (var reader = new StreamReader(caminhoDoArquivo))
                {
                    while ((linha = reader.ReadLine()) != null)
                    {
                        if (string.IsNullOrEmpty(linha))
                            continue;

                        listaRetorno.Add(linha);
                    }
                    reader.Close();
                }
                return listaRetorno;
            }
            catch (Exception ex) { throw ex; }
        }

        //Considera a primeira linha como cabeçalho
        public static List<T> ConverterArquivoParaListaDeEntidade<T>(string caminhoDoArquivo, char separador)
        {
            List<T> listaResultado = new List<T>();
            List<string> listaDeLinhasDoArquivo = LerArquivoTexto(caminhoDoArquivo);

            if (listaDeLinhasDoArquivo != null && listaDeLinhasDoArquivo.Count > 0)
            {
                List<string> listaPropriedades = new List<string>();
                listaPropriedades.AddRange(listaDeLinhasDoArquivo[0].Split(separador));
                listaDeLinhasDoArquivo.Remove(listaDeLinhasDoArquivo[0]);
                Type type = typeof(T);

                foreach (string str in listaDeLinhasDoArquivo)
                {
                    string[] linha = str.Split(separador);
                    object entidadeDTO = (T)Activator.CreateInstance(typeof(T));
                    foreach (PropertyInfo pi in type.GetProperties())
                    {
                        if (!listaPropriedades.Contains(pi.Name)) continue;
                        int index = listaPropriedades.IndexOf(pi.Name);
                        if (string.IsNullOrWhiteSpace(linha[index])) continue;

                        object valor = linha[index];
                        object valorTipado = null;

                        if (pi.PropertyType.IsGenericType && pi.PropertyType.GetGenericTypeDefinition().Equals(typeof(Nullable<>)))
                            valorTipado = Convert.ChangeType(valor, Nullable.GetUnderlyingType(pi.PropertyType));
                        else
                            valorTipado = Convert.ChangeType(valor, pi.PropertyType);

                        if (valorTipado.GetType() == typeof(string))
                            valorTipado = valor.ToString().Trim();

                        pi.SetValue(entidadeDTO, valorTipado, null);
                    }
                    listaResultado.Add((T)entidadeDTO);
                }
            }
            return listaResultado;
        }
    }
}
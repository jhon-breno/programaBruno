using Newtonsoft.Json;
using SalesforceExtractor.Entidades;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;

namespace SalesforceExtractor.Dados
{
    public static class ArquivoLoader
    {
        /// <summary>
        /// Retorna uma simulação de objetos que contém os dados para atualização de Redes Sociais de um Contato
        /// </summary>
        /// <returns></returns>
        public static List<DocumentoRedeSocial> GetList()
        {
            List<DocumentoRedeSocial> _lst = new List<DocumentoRedeSocial>();
            _lst.Add(new DocumentoRedeSocial
            {
                codigoEmpresa = "2005",
                entidadeSF = "Contact",
                documentoNumero = "07208709769",
                redeSocialId = "www.facebook.com/adriel.mt",
                camposRedeSocial = "sf4twitter__Fcbk_Username__c"
            });

            _lst.Add(new DocumentoRedeSocial
            {
                codigoEmpresa = "2005",
                entidadeSF = "Contact",
                documentoNumero = "11883153743",
                redeSocialId = "www.twitter.com/blaw_peq",
                camposRedeSocial = "sf4twitter__Twitter_Username__c"
            });

            return _lst;
        }



        /// <summary>
        /// 
        /// </summary>
        /// <param name="arquivo">Arquivo no formato:  COD_EMPRESA|NUMERO_DOCUMENTO|TIPO_REDE_SOCIAL{twitter/facebook}|DADO_REDE_SOCIAL</param>
        /// <returns></returns>
        public static List<DocumentoRedeSocial> GetListDocRedesSociais(Arquivo arquivo)
        {
            #region validação de parâmetros
            if (arquivo == null || string.IsNullOrEmpty(arquivo.Caminho))
                throw new ArgumentException(string.Format("Caminho do arquivo inválido: {0}.", arquivo.CaminhoCompleto));

            if (string.IsNullOrEmpty(arquivo.Nome) || string.IsNullOrEmpty(arquivo.NomeExtensao))
                throw new ArgumentException(string.Format("Nome do arquivo de entrada inválido.: '{0}'", arquivo.NomeExtensao));
            #endregion

            StreamReader sr = new StreamReader(arquivo.CaminhoCompleto, Encoding.GetEncoding("iso-8859-1"));
            List<DocumentoRedeSocial> lista = new List<DocumentoRedeSocial>();
            DocumentoRedeSocial doc = null;
            string tipoRedeSocial = null;

            while (true)
            {
                try
                {
                    var linha = sr.ReadLine();
                    if (string.IsNullOrEmpty(linha))
                        break;

                    tipoRedeSocial = ("twitter".Equals(linha.Split(arquivo.Separador)[2].ToLower())) ?
                        "sf4twitter__Twitter_Username__c" : ("facebook".Equals(linha.Split(arquivo.Separador)[2].ToLower())) ?
                            "sf4twitter__Fcbk_Username__c" : "";

                    if (string.IsNullOrEmpty(tipoRedeSocial))
                        throw new ArgumentException("Erro ao recuperar o tipo de rede social do arquivo de entrada.");

                    try
                    {
                        doc = new DocumentoRedeSocial()
                        {
                            codigoEmpresa = linha.Split(arquivo.Separador)[0],
                            entidadeSF = "Contact",
                            documentoNumero = linha.Split(arquivo.Separador)[1],
                            redeSocialId = linha.Split(arquivo.Separador)[3],
                            camposRedeSocial = tipoRedeSocial
                        };
                        lista.Add(doc);
                    }
                    catch (Exception ex)
                    {
                        //TODO: log
                        Console.WriteLine(string.Format("ERRO ao carregar o objeto do arquivo de entrada: {0}", linha));
                    }
                }
                catch(Exception ex)
                {
                    //TODO: log
                    Console.WriteLine(string.Format("ERRO inexperado: {0}", ex.Message));
                }
            }

            return lista;
        }



        /// <summary>
        /// Carrega dados de responsáveis a partir de um arquivo texto com um formato pré-definido.
        /// </summary>
        /// <param name="arquivo">Arquivo no formato:  TIPO CLIENTE|EMAIL|NRO DOC|TIPO DOC|NOME|TELEFONE</param>
        /// <returns></returns>
        public static List<ClienteSalesforce> GetResponsaveis(Arquivo arquivo)
        {
            #region validação de parâmetros
            if (arquivo == null || string.IsNullOrEmpty(arquivo.Caminho))
                throw new ArgumentException(string.Format("Caminho do arquivo inválido: {0}.", arquivo.CaminhoCompleto));

            if (string.IsNullOrEmpty(arquivo.Nome) || string.IsNullOrEmpty(arquivo.NomeExtensao))
                throw new ArgumentException(string.Format("Nome do arquivo de entrada inválido.: '{0}'", arquivo.NomeExtensao));
            #endregion

            StreamReader sr = new StreamReader(arquivo.CaminhoCompleto, Encoding.GetEncoding("iso-8859-1"));
            List<ClienteSalesforce> lista = new List<ClienteSalesforce>();
            ClienteSalesforce cli = null;

            while (true)
            {
                try
                {
                    var linha = sr.ReadLine();
                    if (string.IsNullOrEmpty(linha))
                        break;

                    try
                    {
                        cli = new ClienteSalesforce()
                        {
                            ExternalId = linha.Split(arquivo.Separador)[0],
                            TipoRegistroId = linha.Split(arquivo.Separador)[1],
                            ResponsavelEmail = linha.Split(arquivo.Separador)[2],
                            ResponsavelDocumento = linha.Split(arquivo.Separador)[3],
                            ResponsavelTipoDocumento = linha.Split(arquivo.Separador)[4],
                            ResponsavelNome = linha.Split(arquivo.Separador)[5],
                            ResponsavelTelefone = linha.Split(arquivo.Separador)[6]
                        };
                        lista.Add(cli);
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



        /// <summary>
        /// 
        /// </summary>
        /// <param name="arquivo"></param>
        /// <returns></returns>
        public static List<AssetDTOCorretivo> GetAssetsCorretivo(Arquivo arquivo)
        {
            #region validação de parâmetros
            if (arquivo == null || string.IsNullOrEmpty(arquivo.Caminho))
                throw new ArgumentException(string.Format("Caminho do arquivo inválido: {0}.", arquivo.CaminhoCompleto));

            if (string.IsNullOrEmpty(arquivo.Nome) || string.IsNullOrEmpty(arquivo.NomeExtensao))
                throw new ArgumentException(string.Format("Nome do arquivo de entrada inválido.: '{0}'", arquivo.NomeExtensao));
            #endregion

            StreamReader sr = new StreamReader(arquivo.CaminhoCompleto, Encoding.GetEncoding("iso-8859-1"));
            List<AssetDTOCorretivo> lista = new List<AssetDTOCorretivo>();
            AssetDTOCorretivo asset = null;

            while (true)
            {
                try
                {
                    var linha = sr.ReadLine();
                    if (string.IsNullOrEmpty(linha))
                        break;

                    try
                    {
                        asset = new AssetDTOCorretivo()
                        {
                            AccountId = linha.Split(arquivo.Separador)[0],
                            Identidade = linha.Split(arquivo.Separador)[1]
                        };
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



        /// <summary>
        /// Extrai os dados do relatório Base_Atends_RJ e Base_Atends_CE, com os atendimento da URA que foram ingressados erradamente em duplicidade pelo WsUra.
        /// Filtros utilizados no SAlesforce (exemplo RJ):
        /// ((1 OR ((5 AND 8) OR (5 AND 7) OR 9)) AND (2 or (15 and 13) or (14 and 15)) AND 6 AND (3 OR (4 AND 5)) AND (10 AND 11 AND 12 and 16))
        ///1. Status não contém Em Criação,Cancelado,Validado 
        ///2. Casos: Ponto de Fornecimento: Companhia_2 igual a ENEL Distribuição RIO 
        ///3. Erro de Envio (ST) igual a Falso 
        ///4. Erro de Envio (ST) igual a Verdadeiro 
        ///5. Número do aviso (ST) não igual a  
        ///6. Tipo de registro do caso igual a EmergencyBrasil 
        ///7. Status igual a Fechado,Cancelado 
        ///8. Status igual a Cancelado 
        ///9. Motivo (Emerg) igual a INF - Aviso emergencial,INF - Desligamento Programado 
        ///10. Assunto não igual a REC AVISO EMERGENCIAL 
        ///11. Assunto não igual a Email Brasil Caso 
        ///12. Assunto não contém Atención solucionada 
        ///13. Endereço: Logradouro: Companhia igual a AMPLA 
        ///14. Endereço: Logradouro: Companhia igual a 2005 
        ///15. Número do Ponto de Fornecimento igual a  
        ///16. Assunto não contém 2018- 
        /// </summary>
        /// <param name="arquivo"></param>
        /// <returns></returns>
        public static List<AtendimentoURA> GetAtendimentosURA(Arquivo arquivo)
        {
            #region validação de parâmetros
            if (arquivo == null || string.IsNullOrEmpty(arquivo.Caminho))
                throw new ArgumentException(string.Format("Caminho do arquivo inválido: {0}.", arquivo.CaminhoCompleto));

            if (string.IsNullOrEmpty(arquivo.Nome) || string.IsNullOrEmpty(arquivo.NomeExtensao))
                throw new ArgumentException(string.Format("Nome do arquivo de entrada inválido.: '{0}'", arquivo.NomeExtensao));
            #endregion

            StreamReader sr = new StreamReader(arquivo.CaminhoCompleto, Encoding.GetEncoding("iso-8859-1"));
            List<AtendimentoURA> lista = new List<AtendimentoURA>();
            AtendimentoURA asset = null;

            while (true)
            {
                try
                {
                    var linha = sr.ReadLine();
                    if (string.IsNullOrEmpty(linha))
                        break;

                    try
                    {
                        asset = new AtendimentoURA()
                        {
                            IdAtividade = linha.Split(arquivo.Separador)[3],
                            NumeroCaso = linha.Split(arquivo.Separador)[8],
                            DataHora = linha.Split(arquivo.Separador)[17],
                            NumeroAtividade = linha.Split(arquivo.Separador)[13]
                        };
                        
                        if (string.IsNullOrWhiteSpace(asset.NumeroAtividade))
                            continue;

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



        public static List<CaseSalesforce> GetCasos(Arquivo arquivo)
        {
            #region validação de parâmetros
            if (arquivo == null || string.IsNullOrEmpty(arquivo.Caminho))
                throw new ArgumentException(string.Format("Caminho do arquivo inválido: {0}.", arquivo.CaminhoCompleto));

            if (string.IsNullOrEmpty(arquivo.Nome) || string.IsNullOrEmpty(arquivo.NomeExtensao))
                throw new ArgumentException(string.Format("Nome do arquivo de entrada inválido.: '{0}'", arquivo.NomeExtensao));
            #endregion

            StreamReader sr = new StreamReader(arquivo.CaminhoCompleto, Encoding.GetEncoding("iso-8859-1"));
            List<CaseSalesforce> lista = new List<CaseSalesforce>();
            CaseSalesforce obj = null;

            while (true)
            {
                try
                {
                    var linha = sr.ReadLine();
                    if (string.IsNullOrEmpty(linha))
                        break;

                    try
                    {
                        obj = new CaseSalesforce()
                        {
                            NumeroAviso = linha.Split(arquivo.Separador)[0],
                            NumeroCaso = linha.Split(arquivo.Separador)[1]
                        };

                        lista.Add(obj);
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



        public static List<ClienteSalesforce> GetRotas(Arquivo arquivo)
        {
            #region validação de parâmetros
            if (arquivo == null || string.IsNullOrEmpty(arquivo.Caminho))
                throw new ArgumentException(string.Format("Caminho do arquivo inválido: {0}.", arquivo.CaminhoCompleto));

            if (string.IsNullOrEmpty(arquivo.Nome) || string.IsNullOrEmpty(arquivo.NomeExtensao))
                throw new ArgumentException(string.Format("Nome do arquivo de entrada inválido.: '{0}'", arquivo.NomeExtensao));
            #endregion

            StreamReader sr = new StreamReader(arquivo.CaminhoCompleto, Encoding.GetEncoding("iso-8859-1"));
            List<ClienteSalesforce> lista = new List<ClienteSalesforce>();
            ClienteSalesforce obj = null;

            string linha;

            if(arquivo.TemCabecalho)
                linha = sr.ReadLine();
            
            while (true)
            {
                try
                {
                    linha = sr.ReadLine();

                    if (string.IsNullOrEmpty(linha))
                        break;

                    try
                    {
                        obj = new ClienteSalesforce()
                        {
                            Id = linha.Split(arquivo.Separador)[0].Replace("-", string.Empty).Replace(" ", string.Empty).Replace(".", string.Empty).Replace("//", string.Empty).Replace("\"", string.Empty),
                            NumeroCliente = linha.Split(arquivo.Separador)[1].Replace("-", string.Empty).Replace(" ", string.Empty).Replace(".", string.Empty).Replace("//", string.Empty).Replace("\"", string.Empty),
                            Rota = linha.Split(arquivo.Separador)[2].Replace("-", string.Empty).Replace(" ", string.Empty).Replace(".", string.Empty).Replace("//", string.Empty).Replace("\"", string.Empty),
                            Lote = linha.Split(arquivo.Separador)[5].Replace("-", string.Empty).Replace(" ", string.Empty).Replace(".", string.Empty).Replace("//", string.Empty).Replace("\"", string.Empty)
                        };

                        lista.Add(obj);
                    }
                    catch (Exception ex)
                    {
                        //TODO: log
                        Console.WriteLine(string.Format("ERRO ao carregar o objeto do arquivo de entrada: {0} {1}", linha, ex.Message));
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



        /// <summary>
        /// 
        /// </summary>
        /// <param name="arquivo"></param>
        /// <returns></returns>
        public static List<AssetDTO> GetAssets(Arquivo arquivo)
        {
            #region validação de parâmetros
            if (arquivo == null || string.IsNullOrEmpty(arquivo.Caminho))
                throw new ArgumentException(string.Format("Caminho do arquivo inválido: {0}.", arquivo.CaminhoCompleto));

            if (string.IsNullOrEmpty(arquivo.Nome) || string.IsNullOrEmpty(arquivo.NomeExtensao))
                throw new ArgumentException(string.Format("Nome do arquivo de entrada inválido.: '{0}'", arquivo.NomeExtensao));
            #endregion

            StreamReader sr = new StreamReader(arquivo.CaminhoCompleto, Encoding.GetEncoding("iso-8859-1"));
            List<AssetDTO> lista = new List<AssetDTO>();
            AssetDTO asset = null;

            while (true)
            {
                try
                {
                    var linha = sr.ReadLine();
                    if (string.IsNullOrEmpty(linha))
                        break;

                    try
                    {
                        asset = new AssetDTO()
                        {
                            Identidade = linha.Split(arquivo.Separador)[0],
                            TipoIdentidade = linha.Split(arquivo.Separador)[1],
                            AccountId = linha.Split(arquivo.Separador)[2],
                            NomeCliente = linha.Split(arquivo.Separador)[3],
                            AccountExternalId = linha.Split(arquivo.Separador)[4]
                        };
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


        public static List<EnelxAsset> GetAssetsEnelx(Arquivo arquivo)
        {
            #region validação de parâmetros
            if (arquivo == null || string.IsNullOrEmpty(arquivo.Caminho))
                throw new ArgumentException(string.Format("Caminho do arquivo inválido: {0}.", arquivo.CaminhoCompleto));

            if (string.IsNullOrEmpty(arquivo.Nome) || string.IsNullOrEmpty(arquivo.NomeExtensao))
                throw new ArgumentException(string.Format("Nome do arquivo de entrada inválido.: '{0}'", arquivo.NomeExtensao));
            #endregion

            StreamReader sr = new StreamReader(arquivo.CaminhoCompleto, Encoding.GetEncoding("iso-8859-1"));
            List<EnelxAsset> lista = new List<EnelxAsset>();
            EnelxAsset asset = null;

            var linha = string.Empty;

            if (arquivo.TemCabecalho)
                linha = sr.ReadLine();

            while (true)
            {
                try
                {
                    linha = sr.ReadLine();

                    if (string.IsNullOrEmpty(linha))
                        break;

                    try
                    {
                        string[] item = linha.Replace("\"", "").Split(arquivo.Separador);
                        asset = new EnelxAsset()
                        {
                            ExternalId = item[0],
                            Name = item[1],
                            AccountExternalId = item[2],
                            PointofDeliveryExternalId = item[4],
                            Description = item[5],
                            NE__Zip_Code__c = item[6],
                            NE__Description__c = item[7],
                            Status = item[8],
                            SerialNumber = item[10],
                            Company__c = item[11]
                        };
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



        /// <summary>
        /// Recupera os dados de Asset a partir do arquivo resultante do Ingresso de Orders na carga do B2Win.
        /// </summary>
        /// <param name="arquivo"></param>
        /// <returns></returns>
        public static List<AssetDTO> ExtractAssetsFromOrders(Arquivo arquivo)
        {
            #region validação de parâmetros
            if (arquivo == null || string.IsNullOrEmpty(arquivo.Caminho))
                throw new ArgumentException(string.Format("Caminho do arquivo inválido: {0}.", arquivo.CaminhoCompleto));

            if (string.IsNullOrEmpty(arquivo.Nome) || string.IsNullOrEmpty(arquivo.NomeExtensao))
                throw new ArgumentException(string.Format("Nome do arquivo de entrada inválido.: '{0}'", arquivo.NomeExtensao));
            #endregion

            StreamReader sr = new StreamReader(arquivo.CaminhoCompleto, Encoding.GetEncoding("iso-8859-1"));
            List<AssetDTO> lista = new List<AssetDTO>();
            AssetDTO asset = null;
            //pula o cabecalho
            var linha = sr.ReadLine();

            while (true)
            {
                try
                {
                    linha = sr.ReadLine();

                    if (string.IsNullOrEmpty(linha))
                        break;

                    string[] item = linha.Replace("\"", "").Split(arquivo.Separador);
                    if ("Failed".Equals(item[13]))
                        continue;

                    try
                    {
                        asset = new AssetDTO()
                        {
                            AccountExternalId = item[0],
                            NumeroCliente = string.IsNullOrWhiteSpace(item[10]) ? " " : item[10].Split(new string[] {"BRA"}, StringSplitOptions.RemoveEmptyEntries)[0],
                            PointofDeliveryExternalId = string.IsNullOrWhiteSpace(item[10]) ? " " : item[10],
                            ExternalId = item[11],
                            ContractExternalId = item[12],
                            OrderId = item[14]
                        };
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



        public static List<B2WinDTO> ExtractResultadoB2WinOrders(Arquivo arquivo)
        {
            #region validação de parâmetros
            if (arquivo == null || string.IsNullOrEmpty(arquivo.Caminho))
                throw new ArgumentException(string.Format("Caminho do arquivo inválido: {0}.", arquivo.CaminhoCompleto));

            if (string.IsNullOrEmpty(arquivo.Nome) || string.IsNullOrEmpty(arquivo.NomeExtensao))
                throw new ArgumentException(string.Format("Nome do arquivo de entrada inválido.: '{0}'", arquivo.NomeExtensao));
            #endregion

            StreamReader sr = new StreamReader(arquivo.CaminhoCompleto, Encoding.GetEncoding("iso-8859-1"));
            List<B2WinDTO> lista = new List<B2WinDTO>();
            B2WinDTO obj = null;

            //pula o cabecalho
            var linha = sr.ReadLine();
            //linha = sr.ReadLine();

            while (true)
            {
                try
                {

                    if (string.IsNullOrEmpty(linha))
                        break;
                    linha = sr.ReadLine();
                    string[] item = linha.Replace("\"", "").Split(arquivo.Separador);
                    try
                    {
                        obj = new B2WinDTO()
                        {
                            ExternalIdAccount = item[0],
                            NumeroCliente = item[10].Split(new string[] { "BRA" }, StringSplitOptions.RemoveEmptyEntries)[0],
                            ExternalIdPod = item[10],
                            ExternalIdAsset = item[11],
                            ExternalIdContract = item[12],
                            Id = item[14].Trim(),
                            Status = item[13].Trim(),
                            Erro = item[16].Trim()
                        };
                        lista.Add(obj);
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



        public static List<B2WinDTO> ExtractIdOrders(Arquivo arquivo)
        {
            #region validação de parâmetros
            if (arquivo == null || string.IsNullOrEmpty(arquivo.Caminho))
                throw new ArgumentException(string.Format("Caminho do arquivo inválido: {0}.", arquivo.CaminhoCompleto));

            if (string.IsNullOrEmpty(arquivo.Nome) || string.IsNullOrEmpty(arquivo.NomeExtensao))
                throw new ArgumentException(string.Format("Nome do arquivo de entrada inválido.: '{0}'", arquivo.NomeExtensao));
            #endregion

            StreamReader sr = new StreamReader(arquivo.CaminhoCompleto, Encoding.GetEncoding("iso-8859-1"));
            List<B2WinDTO> lista = new List<B2WinDTO>();
            B2WinDTO obj = null;

            //pula o cabecalho
            var linha = sr.ReadLine();
            //linha = sr.ReadLine();

            while (true)
            {
                try
                {

                    if (string.IsNullOrEmpty(linha))
                        break;
                    linha = sr.ReadLine();
                    string[] item = linha.Replace("\"", "").Split(arquivo.Separador);
                    try
                    {
                        obj = new B2WinDTO()
                        {
                            Id = item[0].Trim(),
                        };
                        lista.Add(obj);
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



        public static List<AssetDTO> GetAssetIds(Arquivo arquivo)
        {
            #region validação de parâmetros
            if (arquivo == null || string.IsNullOrEmpty(arquivo.Caminho))
                throw new ArgumentException(string.Format("Caminho do arquivo inválido: {0}.", arquivo.CaminhoCompleto));

            if (string.IsNullOrEmpty(arquivo.Nome) || string.IsNullOrEmpty(arquivo.NomeExtensao))
                throw new ArgumentException(string.Format("Nome do arquivo de entrada inválido.: '{0}'", arquivo.NomeExtensao));
            #endregion

            StreamReader sr = new StreamReader(arquivo.CaminhoCompleto, Encoding.GetEncoding("iso-8859-1"));
            List<AssetDTO> lista = new List<AssetDTO>();
            AssetDTO asset = null;

            var linha = string.Empty;

            if (arquivo.TemCabecalho)
                sr.ReadLine();

            while (true)
            {
                linha = sr.ReadLine();
                try
                {
                    if (string.IsNullOrEmpty(linha))
                        break;

                    try
                    {
                        asset = new AssetDTO()
                        {
                            Id = linha.Split(arquivo.Separador)[0]
                        };
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



        public static List<AssetDTO> GetAccountIds(Arquivo arquivo)
        {
            #region validação de parâmetros
            if (arquivo == null || string.IsNullOrEmpty(arquivo.Caminho))
                throw new ArgumentException(string.Format("Caminho do arquivo inválido: {0}.", arquivo.CaminhoCompleto));

            if (string.IsNullOrEmpty(arquivo.Nome) || string.IsNullOrEmpty(arquivo.NomeExtensao))
                throw new ArgumentException(string.Format("Nome do arquivo de entrada inválido.: '{0}'", arquivo.NomeExtensao));
            #endregion

            StreamReader sr = new StreamReader(arquivo.CaminhoCompleto, Encoding.GetEncoding("iso-8859-1"));
            List<AssetDTO> lista = new List<AssetDTO>();
            AssetDTO asset = null;

            var linha = string.Empty;

            if (arquivo.TemCabecalho)
                sr.ReadLine();

            while (true)
            {
                linha = sr.ReadLine();
                try
                {
                    if (string.IsNullOrEmpty(linha))
                        break;

                    try
                    {
                        asset = new AssetDTO()
                        {
                            AccountId = linha.Split(arquivo.Separador)[0]
                        };
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



        public static List<string> GetAccountExternalIds(Arquivo arquivo)
        {
            #region validação de parâmetros
            if (arquivo == null || string.IsNullOrEmpty(arquivo.Caminho))
                throw new ArgumentException(string.Format("Caminho do arquivo inválido: {0}.", arquivo.CaminhoCompleto));

            if (string.IsNullOrEmpty(arquivo.Nome) || string.IsNullOrEmpty(arquivo.NomeExtensao))
                throw new ArgumentException(string.Format("Nome do arquivo de entrada inválido.: '{0}'", arquivo.NomeExtensao));
            #endregion

            StreamReader sr = new StreamReader(arquivo.CaminhoCompleto, Encoding.GetEncoding("iso-8859-1"));
            List<string> lista = new List<string>();

            while (true)
            {
                var linha = sr.ReadLine();
                try
                {
                    if (string.IsNullOrEmpty(linha))
                        break;

                    try
                    {
                        lista.Add(linha.Split(arquivo.Separador)[0]);
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



        /// <summary>
        /// 
        /// </summary>
        /// <param name="arquivo"></param>
        /// <returns></returns>
        public static List<ClienteSalesforce> GetAccounts(Arquivo arquivo)
        {
            #region validação de parâmetros
            if (arquivo == null || string.IsNullOrEmpty(arquivo.Caminho))
                throw new ArgumentException(string.Format("Caminho do arquivo inválido: {0}.", arquivo.CaminhoCompleto));

            if (string.IsNullOrEmpty(arquivo.Nome) || string.IsNullOrEmpty(arquivo.NomeExtensao))
                throw new ArgumentException(string.Format("Nome do arquivo de entrada inválido.: '{0}'", arquivo.NomeExtensao));
            #endregion

            StreamReader sr = new StreamReader(arquivo.CaminhoCompleto, Encoding.GetEncoding("iso-8859-1"));
            List<ClienteSalesforce> lista = new List<ClienteSalesforce>();
            ClienteSalesforce asset = null;

            while (true)
            {
                try
                {
                    var linha = sr.ReadLine();
                    if (string.IsNullOrEmpty(linha))
                        break;

                    try
                    {
                        asset = new ClienteSalesforce()
                        {
                            Documento = linha.Split(arquivo.Separador)[0],
                            TipoDocumento = linha.Split(arquivo.Separador)[1],
                            Nome = linha.Split(arquivo.Separador)[2],
                            TipoRegistroId = linha.Split(arquivo.Separador)[3].Contains("B2B") ? "01236000000yI8mAAE" : linha.Split(arquivo.Separador)[3].Contains("B2C") ? "01236000000yI8nAAE" : linha.Split(arquivo.Separador)[3].Contains("B2G") ? "0121o000000oWhZAAU" : linha.Split(arquivo.Separador)[3],
                            ExternalId = linha.Split(arquivo.Separador)[4],
                            TipoCliente = linha.Split(arquivo.Separador)[5],
                            IdConta = linha.Split(arquivo.Separador)[6]
                        };
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



        public static List<ClienteSalesforce> GetAccountsParaMerge(Arquivo arquivo)
        {
            #region validação de parâmetros
            if (arquivo == null || string.IsNullOrEmpty(arquivo.Caminho))
                throw new ArgumentException(string.Format("Caminho do arquivo inválido: {0}.", arquivo.CaminhoCompleto));

            if (string.IsNullOrEmpty(arquivo.Nome) || string.IsNullOrEmpty(arquivo.NomeExtensao))
                throw new ArgumentException(string.Format("Nome do arquivo de entrada inválido.: '{0}'", arquivo.NomeExtensao));
            #endregion

            StreamReader sr = new StreamReader(arquivo.CaminhoCompleto, Encoding.GetEncoding("iso-8859-1"));
            List<ClienteSalesforce> lista = new List<ClienteSalesforce>();
            ClienteSalesforce asset = null;

            var linha = string.Empty;
            if (arquivo.TemCabecalho)
                linha = sr.ReadLine();

            while (true)
            {
                try
                {
                    linha = sr.ReadLine();

                    if (string.IsNullOrEmpty(linha))
                        break;

                    try
                    {
                        if (linha.Split(arquivo.Separador).Count() == 5)
                        {
                            asset = new ClienteSalesforce()
                            {
                                IdConta = linha.Split(arquivo.Separador)[0],
                                ExternalId = linha.Split(arquivo.Separador)[1],
                                Nome = linha.Split(arquivo.Separador)[2],
                                Documento = linha.Split(arquivo.Separador)[3],
                                TipoDocumento = linha.Split(arquivo.Separador)[4]
                            };
                        }

                        if (linha.Split(arquivo.Separador).Count() == 1)
                        {
                            asset = new ClienteSalesforce()
                            {
                                IdConta = linha.Split(arquivo.Separador)[0],
                            };
                        }
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


        /// <summary>
        /// 
        /// </summary>
        /// <param name="arquivo"></param>
        /// <returns></returns>
        public static List<ItemAttribute> GetItemsAttributes(Arquivo arquivo, Type tipoItem, string tipoCliente = "GB")
        {
            #region validação de parâmetros
            if (arquivo == null || string.IsNullOrEmpty(arquivo.Caminho))
                throw new ArgumentException(string.Format("Caminho do arquivo inválido: {0}.", arquivo.CaminhoCompleto));

            if (string.IsNullOrEmpty(arquivo.Nome) || string.IsNullOrEmpty(arquivo.NomeExtensao))
                throw new ArgumentException(string.Format("Nome do arquivo de entrada inválido.: '{0}'", arquivo.NomeExtensao));
            #endregion

            Extrator ext = ExtratorFactory.GetExtrator(tipoCliente);
            return ext.GetItemAttributes(arquivo, tipoItem);
        }


        public static List<OrderSalesforce> GetOrders(Arquivo arquivo)
        {
            #region validação de parâmetros
            if (arquivo == null || string.IsNullOrEmpty(arquivo.Caminho))
                throw new ArgumentException(string.Format("Caminho do arquivo inválido: {0}.", arquivo.CaminhoCompleto));

            if (string.IsNullOrEmpty(arquivo.Nome) || string.IsNullOrEmpty(arquivo.NomeExtensao))
                throw new ArgumentException(string.Format("Nome do arquivo de entrada inválido.: '{0}'", arquivo.NomeExtensao));
            #endregion

            StreamReader sr = new StreamReader(arquivo.CaminhoCompleto, Encoding.GetEncoding("iso-8859-1"));
            List<OrderSalesforce> lista = new List<OrderSalesforce>();
            OrderSalesforce order = null;

            var linha = sr.ReadLine();

            while (true)
            {
                try
                {

                    //pula o cabecalho
                    linha = sr.ReadLine();
                    if (string.IsNullOrEmpty(linha))
                        break;
                    try
                    {
                        order = new OrderSalesforce();
                        string[] item = linha.Replace("\"","").Split(arquivo.Separador);
                        if ("Failed".Equals(item[13]))
                            continue;
                        order.AccountId = item[0];
                        order.ExternalId = item[11];
                        order.Id = item[14];

                        lista.Add(order);
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


        public static List<string[]> GetEnelX(Arquivo arquivo)
        {
            #region validação de parâmetros
            if (arquivo == null || string.IsNullOrEmpty(arquivo.Caminho))
                throw new ArgumentException(string.Format("Caminho do arquivo inválido: {0}.", arquivo.CaminhoCompleto));

            if (string.IsNullOrEmpty(arquivo.Nome) || string.IsNullOrEmpty(arquivo.NomeExtensao))
                throw new ArgumentException(string.Format("Nome do arquivo de entrada inválido.: '{0}'", arquivo.NomeExtensao));
            #endregion

            StreamReader sr = new StreamReader(arquivo.CaminhoCompleto, Encoding.GetEncoding("iso-8859-1"));
            List<string[]> lista = new List<string[]>();

            var linha = string.Empty;

            while (true)
            {
                try
                {
                    //pula o cabecalho
                    linha = sr.ReadLine();
                    if (string.IsNullOrEmpty(linha))
                        break;
                    try
                    {
                        string[] item = linha.Replace("\"", "").Split(arquivo.Separador);
                        lista.Add(item);
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


        /// <summary>
        /// Processo temporário para carregar os orders manualmente em um formato diferente do projeto inicialmente.
        /// </summary>
        /// <param name="arquivo"></param>
        /// <returns></returns>
        public static List<OrderSalesforce> GetOrdersTemp(Arquivo arquivo)
        {
            #region validação de parâmetros
            if (arquivo == null || string.IsNullOrEmpty(arquivo.Caminho))
                throw new ArgumentException(string.Format("Caminho do arquivo inválido: {0}.", arquivo.CaminhoCompleto));

            if (string.IsNullOrEmpty(arquivo.Nome) || string.IsNullOrEmpty(arquivo.NomeExtensao))
                throw new ArgumentException(string.Format("Nome do arquivo de entrada inválido.: '{0}'", arquivo.NomeExtensao));
            #endregion

            StreamReader sr = new StreamReader(arquivo.CaminhoCompleto, Encoding.GetEncoding("iso-8859-1"));
            List<OrderSalesforce> lista = new List<OrderSalesforce>();
            OrderSalesforce order = null;

            var linha = sr.ReadLine();

            while (true)
            {
                try
                {

                    //pula o cabecalho
                    linha = sr.ReadLine();
                    if (string.IsNullOrEmpty(linha))
                        break;
                    try
                    {
                        order = new OrderSalesforce();
                        string[] item = linha.Replace("\"","").Split(arquivo.Separador);
                        if ("Failed".Equals(item[13]))
                            continue;
                        order.AccountId = item[20];
                        order.ExternalId = item[19];
                        order.Id = item[21];

                        lista.Add(order);
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


        public static List<AccountSalesforce> GetOrgaosControladores(Arquivo arquivo)
        {
            #region validação de parâmetros
            if (arquivo == null || string.IsNullOrEmpty(arquivo.Caminho))
                throw new ArgumentException(string.Format("Caminho do arquivo inválido: {0}.", arquivo.CaminhoCompleto));

            if (string.IsNullOrEmpty(arquivo.Nome) || string.IsNullOrEmpty(arquivo.NomeExtensao))
                throw new ArgumentException(string.Format("Nome do arquivo de entrada inválido.: '{0}'", arquivo.NomeExtensao));
            #endregion

            StreamReader sr = new StreamReader(arquivo.CaminhoCompleto, Encoding.GetEncoding("iso-8859-1"));
            List<AccountSalesforce> lista = new List<AccountSalesforce>();
            List<string> codigosControladores = new List<string>();
            AccountSalesforce obj = null;

            string linha;
            
            if(arquivo.TemCabecalho)
                linha = sr.ReadLine();

            while (true)
            {
                try
                {
                    linha = sr.ReadLine();
                    if (string.IsNullOrEmpty(linha))
                        break;
                    try
                    {
                        obj = new AccountSalesforce();
                        string[] item = linha.Replace("\"", "").Split(arquivo.Separador);
                        obj.ExternalId = item[1];
                        obj.CodigoOrgaoControlador = item[2];
                        //obj.ParentId = item[0];

                        if (codigosControladores.Contains(obj.ExternalId))
                            continue;

                        codigosControladores.Add(obj.ExternalId);
                        lista.Add(obj);
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


        public static List<ContratoColetivoEntrada> GetContratosColetivos(Arquivo arquivo)
        {
            #region validação de parâmetros
            if (arquivo == null || string.IsNullOrEmpty(arquivo.Caminho))
                throw new ArgumentException(string.Format("Caminho do arquivo inválido: {0}.", arquivo.CaminhoCompleto));

            if (string.IsNullOrEmpty(arquivo.Nome) || string.IsNullOrEmpty(arquivo.NomeExtensao))
                throw new ArgumentException(string.Format("Nome do arquivo de entrada inválido.: '{0}'", arquivo.NomeExtensao));
            #endregion

            StreamReader sr = new StreamReader(arquivo.CaminhoCompleto, Encoding.GetEncoding("iso-8859-1"));
            List<ContratoColetivoEntrada> lista = new List<ContratoColetivoEntrada>();
            ContratoColetivoEntrada obj = null;

            string linha;

            if (arquivo.TemCabecalho)
                linha = sr.ReadLine();

            while (true)
            {
                try
                {
                    //pula o cabecalho
                    linha = sr.ReadLine();
                    if (string.IsNullOrEmpty(linha))
                        break;
                    try
                    {
                        obj = new ContratoColetivoEntrada();
                        string[] item = linha.Replace("\"", "").Split(arquivo.Separador);
                        obj.CodigoContrato = item[0];
                        obj.AccountExternalId = item[1];
                        obj.Nome = item[3];
                        obj.DataVencimento = item[4];
                        obj.TipoImpressao = item[5];
                        obj.Lote = item[6];
                        lista.Add(obj);
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


        public static List<EnelxContract> GetContratos(Arquivo arquivo)
        {
            #region validação de parâmetros
            if (arquivo == null || string.IsNullOrEmpty(arquivo.Caminho))
                throw new ArgumentException(string.Format("Caminho do arquivo inválido: {0}.", arquivo.CaminhoCompleto));

            if (string.IsNullOrEmpty(arquivo.Nome) || string.IsNullOrEmpty(arquivo.NomeExtensao))
                throw new ArgumentException(string.Format("Nome do arquivo de entrada inválido.: '{0}'", arquivo.NomeExtensao));
            #endregion

            StreamReader sr = new StreamReader(arquivo.CaminhoCompleto, Encoding.GetEncoding("iso-8859-1"));
            List<EnelxContract> lista = new List<EnelxContract>();
            EnelxContract obj = null;

            string linha;

            if (arquivo.TemCabecalho)
                linha = sr.ReadLine();

            while (true)
            {
                try
                {
                    //pula o cabecalho
                    linha = sr.ReadLine();
                    if (string.IsNullOrEmpty(linha))
                        break;
                    try
                    {
                        string[] item = linha.Replace("\"", "").Split(arquivo.Separador);
                        obj = new EnelxContract()
                        {
                            ExternalID__c = item[0],
                            Name = item[0],
                            AccountExternalId = item[0],
                            Description = item[0],
                            Status = item[0],
                            StartDate = item[0],
                            Contract_Type__c = item[0]
                        };
                        lista.Add(obj);
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


        public static List<EnelxContract> GetContratosEnelx(Arquivo arquivo)
        {
            #region validação de parâmetros
            if (arquivo == null || string.IsNullOrEmpty(arquivo.Caminho))
                throw new ArgumentException(string.Format("Caminho do arquivo inválido: {0}.", arquivo.CaminhoCompleto));

            if (string.IsNullOrEmpty(arquivo.Nome) || string.IsNullOrEmpty(arquivo.NomeExtensao))
                throw new ArgumentException(string.Format("Nome do arquivo de entrada inválido.: '{0}'", arquivo.NomeExtensao));
            #endregion

            StreamReader sr = new StreamReader(arquivo.CaminhoCompleto, Encoding.GetEncoding("iso-8859-1"));
            List<EnelxContract> lista = new List<EnelxContract>();
            EnelxContract obj = null;

            string linha = string.Empty;

            if (arquivo.TemCabecalho)
                linha = sr.ReadLine();

            while (true)
            {
                try
                {
                    linha = sr.ReadLine();

                    if (string.IsNullOrEmpty(linha))
                        break;

                    try
                    {
                        string[] item = linha.Replace("\"", "").Split(arquivo.Separador);
                        obj = new EnelxContract()
                        {
                            ExternalID__c = item[0],
                            Name = item[1],
                            AccountExternalId = item[2],
                            Description = item[4],
                            Status = item[5],
                            StartDate = item[6],
                            Contract_Type__c = item[9]
                        };
                        lista.Add(obj);
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


        public static List<EnelxContractLine> GetContractLinesEnelx(Arquivo arquivo)
        {
            #region validação de parâmetros
            if (arquivo == null || string.IsNullOrEmpty(arquivo.Caminho))
                throw new ArgumentException(string.Format("Caminho do arquivo inválido: {0}.", arquivo.CaminhoCompleto));

            if (string.IsNullOrEmpty(arquivo.Nome) || string.IsNullOrEmpty(arquivo.NomeExtensao))
                throw new ArgumentException(string.Format("Nome do arquivo de entrada inválido.: '{0}'", arquivo.NomeExtensao));
            #endregion

            StreamReader sr = new StreamReader(arquivo.CaminhoCompleto, Encoding.GetEncoding("iso-8859-1"));
            List<EnelxContractLine> lista = new List<EnelxContractLine>();
            EnelxContractLine obj = null;

            string linha;

            if (arquivo.TemCabecalho)
                linha = sr.ReadLine();

            while (true)
            {
                try
                {
                    linha = sr.ReadLine();
                    if (string.IsNullOrEmpty(linha))
                        break;
                    try
                    {
                        string[] item = linha.Replace("\"", "").Split(arquivo.Separador);
                        obj = new EnelxContractLine()
                        {
                            ExternalId = item[0],
                            ContractExternalId = item[2],
                            AssetExternalId = item[3],
                            BillingExternalId = item[4]
                        };
                        lista.Add(obj);
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


        public static Dictionary<string, string> GetContratosColetivosEContas(Arquivo arquivo)
        {
            #region validação de parâmetros
            if (arquivo == null || string.IsNullOrEmpty(arquivo.Caminho))
                throw new ArgumentException(string.Format("Caminho do arquivo inválido: {0}.", arquivo.CaminhoCompleto));

            if (string.IsNullOrEmpty(arquivo.Nome) || string.IsNullOrEmpty(arquivo.NomeExtensao))
                throw new ArgumentException(string.Format("Nome do arquivo de entrada inválido.: '{0}'", arquivo.NomeExtensao));
            #endregion

            StreamReader sr = new StreamReader(arquivo.CaminhoCompleto, Encoding.GetEncoding("iso-8859-1"));
            Dictionary<string, string> lista = new Dictionary<string, string>();
            string linha;

            if (arquivo.TemCabecalho)
                linha = sr.ReadLine();

            while (true)
            {
                try
                {
                    linha = sr.ReadLine();
                    if (string.IsNullOrEmpty(linha))
                        break;
                    try
                    {
                        string[] item = linha.Replace("\"", "").Split(arquivo.Separador);
                        lista.Add(item[1], item[0]);
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

        
        
        public static List<OrderItemSalesforce> GetOrderItems(Arquivo arquivo)
        {
            #region validação de parâmetros
            if (arquivo == null || string.IsNullOrEmpty(arquivo.Caminho))
                throw new ArgumentException(string.Format("Caminho do arquivo inválido: {0}.", arquivo.CaminhoCompleto));

            if (string.IsNullOrEmpty(arquivo.Nome) || string.IsNullOrEmpty(arquivo.NomeExtensao))
                throw new ArgumentException(string.Format("Nome do arquivo de entrada inválido.: '{0}'", arquivo.NomeExtensao));
            #endregion

            StreamReader sr = new StreamReader(arquivo.CaminhoCompleto, Encoding.GetEncoding("iso-8859-1"));
            List<OrderItemSalesforce> lista = new List<OrderItemSalesforce>();
            OrderItemSalesforce obj = null;

            var linha = string.Empty;

            if(arquivo.TemCabecalho)
                linha = sr.ReadLine();

            while (true)
            {
                try
                {
                    linha = sr.ReadLine();

                    if (string.IsNullOrEmpty(linha))
                        break;
                    try
                    {
                        obj = new OrderItemSalesforce();
                        string[] item = linha.Replace("\"","").Split(arquivo.Separador);
                        if ("failed".Equals(item[18].ToLower()))
                            continue;
                        obj.AssetItemEnterpriseId = item[10];
                        obj.Id = item[19];

                        lista.Add(obj);
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


        public static List<OrderItemSalesforce> GetOrderItemsManual(Arquivo arquivo)
        {
            #region validação de parâmetros
            if (arquivo == null || string.IsNullOrEmpty(arquivo.Caminho))
                throw new ArgumentException(string.Format("Caminho do arquivo inválido: {0}.", arquivo.CaminhoCompleto));

            if (string.IsNullOrEmpty(arquivo.Nome) || string.IsNullOrEmpty(arquivo.NomeExtensao))
                throw new ArgumentException(string.Format("Nome do arquivo de entrada inválido.: '{0}'", arquivo.NomeExtensao));
            #endregion

            StreamReader sr = new StreamReader(arquivo.CaminhoCompleto, Encoding.GetEncoding("iso-8859-1"));
            List<OrderItemSalesforce> lista = new List<OrderItemSalesforce>();
            OrderItemSalesforce obj = null;

            var linha = sr.ReadLine();
            linha = sr.ReadLine();
            while (true)
            {
                try
                {
                    linha = sr.ReadLine();

                    if (string.IsNullOrEmpty(linha))
                        break;
                    try
                    {
                        obj = new OrderItemSalesforce();
                        string[] item = linha.Replace("\"", "").Split(arquivo.Separador);
                        obj.Id = item[21];

                        lista.Add(obj);
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


        public static List<string> GetNumeroClientes(string caminhoCompleto, bool delimitado = false)
        {
            Arquivo arquivo = new Arquivo(caminhoCompleto);
            return GetNumeroClientes(arquivo, delimitado);
        }


        /// <summary>
        /// Obtem a lista de clientes de um arquivo texto
        /// </summary>
        /// <param name="arquivo"></param>
        /// <returns></returns>
        public static List<string> GetNumeroClientes(Arquivo arquivo, bool delimitado = false)
        {
            #region validação de parâmetros
            if (arquivo == null || string.IsNullOrEmpty(arquivo.Caminho))
                throw new ArgumentException(string.Format("Caminho do arquivo inválido: {0}.", arquivo.CaminhoCompleto));

            if (string.IsNullOrEmpty(arquivo.Nome) || string.IsNullOrEmpty(arquivo.NomeExtensao))
                throw new ArgumentException(string.Format("Nome do arquivo de entrada inválido.: '{0}'", arquivo.NomeExtensao));
            #endregion

            StreamReader sr = new StreamReader(arquivo.CaminhoCompleto, Encoding.GetEncoding("iso-8859-1"));
            List<string> lista = new List<string>();

            while (!sr.EndOfStream)
            {
                try
                {
                    var linha = sr.ReadLine();
                    if (string.IsNullOrEmpty(linha))
                        continue;

                    try
                    {
                        if(delimitado)
                            lista.Add(string.Concat("'", linha, "'"));
                        else
                            lista.Add(linha);
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



        public static Dictionary<string,string> GetTiposTensao(Arquivo arquivo, bool delimitado = false)
        {
            #region validação de parâmetros
            if (arquivo == null || string.IsNullOrEmpty(arquivo.Caminho))
                throw new ArgumentException(string.Format("Caminho do arquivo inválido: {0}.", arquivo.CaminhoCompleto));

            if (string.IsNullOrEmpty(arquivo.Nome) || string.IsNullOrEmpty(arquivo.NomeExtensao))
                throw new ArgumentException(string.Format("Nome do arquivo de entrada inválido.: '{0}'", arquivo.NomeExtensao));
            #endregion

            StreamReader sr = new StreamReader(arquivo.CaminhoCompleto, Encoding.GetEncoding("iso-8859-1"));
            Dictionary<string, string> lista = new Dictionary<string, string>();

            var linha = sr.ReadLine();

            if(arquivo.TemCabecalho)
                linha = sr.ReadLine();

            while (!sr.EndOfStream)
            {
                try
                {
                    linha = sr.ReadLine();
                    if (string.IsNullOrEmpty(linha))
                        continue;

                    try
                    {
                        string[] dados = linha.Split(new char[] {','});
                        if (delimitado)
                            lista.Add(string.Concat("'", dados[0], "'"), string.Concat("'", dados[1], "'"));
                        else
                            lista.Add(dados[0], dados[1]);
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



        /// <summary>
        /// 
        /// </summary>
        /// <param name="arquivo"></param>
        /// <param name="tipoCliente"></param>
        /// <returns></returns>
        public static List<GeracaoDistribuidaDTO> GetClientesGeracaoDistribuida(Arquivo arquivo, string tipoCliente = "GB")
        {
            #region validação de parâmetros
            if (arquivo == null || string.IsNullOrEmpty(arquivo.Caminho))
                throw new ArgumentException(string.Format("Caminho do arquivo inválido: {0}.", arquivo.CaminhoCompleto));

            if (string.IsNullOrEmpty(arquivo.Nome) || string.IsNullOrEmpty(arquivo.NomeExtensao))
                throw new ArgumentException(string.Format("Nome do arquivo de entrada inválido.: '{0}'", arquivo.NomeExtensao));
            #endregion

            StreamReader sr = new StreamReader(arquivo.CaminhoCompleto, Encoding.GetEncoding("iso-8859-1"));
            List<GeracaoDistribuidaDTO> lista = new List<GeracaoDistribuidaDTO>();
            GeracaoDistribuidaDTO obj = null;

            while (true)
            {
                try
                {
                    var linha = sr.ReadLine();
                    if (string.IsNullOrEmpty(linha))
                        break;

                    try
                    {
                        string[] item = linha.Split(arquivo.Separador);
                        obj = new GeracaoDistribuidaDTO()
                        {
                            TipoCliente = tipoCliente,
                            ContratoGerador = item[0],
                            ClienteGerador = item[1],
                            ContratoConsumidor = item[2],
                            ClienteConsumidor = item[3],
                            Percentual = item[4]
                        };

                        lista.Add(obj);
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


        public static List<ItemAttribute> GetOrderItemsAttribute(Arquivo arquivo)
        {
            #region validação de parâmetros
            if (arquivo == null || string.IsNullOrEmpty(arquivo.Caminho))
                throw new ArgumentException(string.Format("Caminho do arquivo inválido: {0}.", arquivo.CaminhoCompleto));

            if (string.IsNullOrEmpty(arquivo.Nome) || string.IsNullOrEmpty(arquivo.NomeExtensao))
                throw new ArgumentException(string.Format("Nome do arquivo de entrada inválido.: '{0}'", arquivo.NomeExtensao));
            #endregion

            StreamReader sr = new StreamReader(arquivo.CaminhoCompleto, Encoding.GetEncoding("iso-8859-1"));
            List<ItemAttribute> lista = new List<ItemAttribute>();
            ItemAttribute obj = null;

            while (true)
            {
                try
                {
                    var linha = sr.ReadLine();
                    if (string.IsNullOrEmpty(linha))
                        break;

                    try
                    {
                        obj = new OrderItemAttributeSalesforce()
                        {
                            NumeroCliente = linha.Split(arquivo.Separador)[0],
                            CargaKW = Convert.ToInt32(Convert.ToDecimal(linha.Split(arquivo.Separador)[1], CultureInfo.GetCultureInfo("en-US"))),
                            //ModalidadeTarifaria = linha.Split(arquivo.Separador)[2],
                            CategoriaTarifa = linha.Split(arquivo.Separador)[3],
                            Classe = linha.Split(arquivo.Separador)[4],
                            SubClasse = linha.Split(arquivo.Separador)[5],
                            //DemandaKV = Convert.ToDecimal(linha.Split(arquivo.Separador)[6], CultureInfo.GetCultureInfo("en-US")).ToString(),
                            //DemandaPonta = Convert.ToDecimal(linha.Split(arquivo.Separador)[7], CultureInfo.GetCultureInfo("en-US")).ToString(),
                            //DemandaForaPonta = Convert.ToDecimal(linha.Split(arquivo.Separador)[8], CultureInfo.GetCultureInfo("en-US")).ToString(),
                            CapacidadeDisjuntor = linha.Split(arquivo.Separador)[9],
                            ValorTensao = linha.Split(arquivo.Separador)[10],
                            InstalacaoPadrao = linha.Split(arquivo.Separador)[11],
                            TipoTensao = linha.Split(arquivo.Separador)[12],
                            //PotenciaKWA = Convert.ToDecimal(linha.Split(arquivo.Separador)[13], CultureInfo.GetCultureInfo("en-US")).ToString(),
                            NivelTensao = linha.Split(arquivo.Separador)[14],
                            ExternalIdPod = linha.Split(arquivo.Separador)[15]
                        };
                        lista.Add(obj);
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



        public static List<BillingSalesforce> GetDebitosAutomaticos(Arquivo arquivo)
        {
            #region validação de parâmetros
            if (arquivo == null || string.IsNullOrEmpty(arquivo.Caminho))
                throw new ArgumentException(string.Format("Caminho do arquivo inválido: {0}.", arquivo.CaminhoCompleto));

            if (string.IsNullOrEmpty(arquivo.Nome) || string.IsNullOrEmpty(arquivo.NomeExtensao))
                throw new ArgumentException(string.Format("Nome do arquivo de entrada inválido.: '{0}'", arquivo.NomeExtensao));
            #endregion

            StreamReader sr = new StreamReader(arquivo.CaminhoCompleto, Encoding.GetEncoding("iso-8859-1"));
            List<BillingSalesforce> lista = new List<BillingSalesforce>();
            BillingSalesforce obj = null;

            while (true)
            {
                try
                {
                    var linha = sr.ReadLine();
                    if (string.IsNullOrEmpty(linha))
                        break;

                    try
                    {
                        obj = new BillingSalesforce()
                        {
                            NumeroCliente = linha.Split(arquivo.Separador)[8],
                        };
                        lista.Add(obj);
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

        
        
        
        /// <summary>
        /// TODO:
        /// </summary>
        /// <param name="arquivo"></param>
        /// <returns></returns>
        public static List<DeviceSalesforce> GetDevices(Arquivo arquivo)
        {
            #region validação de parâmetros
            if (arquivo == null || string.IsNullOrEmpty(arquivo.Caminho))
                throw new ArgumentException(string.Format("Caminho do arquivo inválido: {0}.", arquivo.CaminhoCompleto));

            if (string.IsNullOrEmpty(arquivo.Nome) || string.IsNullOrEmpty(arquivo.NomeExtensao))
                throw new ArgumentException(string.Format("Nome do arquivo de entrada inválido.: '{0}'", arquivo.NomeExtensao));
            #endregion

            StreamReader sr = new StreamReader(arquivo.CaminhoCompleto, Encoding.GetEncoding("iso-8859-1"));
            List<DeviceSalesforce> lista = new List<DeviceSalesforce>();
            DeviceSalesforce obj = null;

            while (true)
            {
                try
                {
                    var linha = sr.ReadLine();
                    if (string.IsNullOrEmpty(linha))
                        break;

                    try
                    {
                        obj = new DeviceSalesforce()
                        {
                            Marca = linha.Split(arquivo.Separador)[0],
                            Modelo = linha.Split(arquivo.Separador)[1],
                            Numero = linha.Split(arquivo.Separador)[2],
                            Propriedade = linha.Split(arquivo.Separador)[3],
                            Tipo = linha.Split(arquivo.Separador)[4],
                            PointOfDeliveryId = linha.Split(arquivo.Separador)[6],
                            ExternalId = linha.Split(arquivo.Separador)[5],
                            DataInstalacao = linha.Split(arquivo.Separador)[7],
                            TipoMedicao = linha.Split(arquivo.Separador)[8],
                            //Constante = linha.Split(arquivo.Separador)[10],
                            Cubiculo = linha.Split(arquivo.Separador)[11],
                            //DataRetirada = linha.Split(arquivo.Separador)[12],
                            Estado = linha.Split(arquivo.Separador)[13],
                            Constante1 = linha.Split(arquivo.Separador)[14],
                            Constante2 = linha.Split(arquivo.Separador)[15],
                            Constante3 = linha.Split(arquivo.Separador)[16],
                            Constante4 = linha.Split(arquivo.Separador)[17],
                            Constante5 = linha.Split(arquivo.Separador)[18],
                            DataFabricacao = string.IsNullOrWhiteSpace(linha.Split(arquivo.Separador)[19]) ? string.Empty : linha.Split(arquivo.Separador)[19],
                            Empresa = "2003"
                        };

                        lista.Add(obj);
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
                    Console.WriteLine(string.Format("ERRO inexperado: {0} {1}", ex.Message, ex.StackTrace));
                }
            }

            return lista;
        }


        /// <summary>
        /// TODO:
        /// </summary>
        /// <param name="arquivo"></param>
        /// <returns></returns>
        public static List<TalendSalesforce> GetTalends(Arquivo arquivo)
        {
            #region validação de parâmetros
            if (arquivo == null || string.IsNullOrEmpty(arquivo.Caminho))
                throw new ArgumentException(string.Format("Caminho do arquivo inválido: {0}.", arquivo.CaminhoCompleto));

            if (string.IsNullOrEmpty(arquivo.Nome) || string.IsNullOrEmpty(arquivo.NomeExtensao))
                throw new ArgumentException(string.Format("Nome do arquivo de entrada inválido.: '{0}'", arquivo.NomeExtensao));
            #endregion

            StreamReader sr = new StreamReader(arquivo.CaminhoCompleto, Encoding.GetEncoding("UTF-8"));
            List<TalendSalesforce> lista = new List<TalendSalesforce>();
            TalendSalesforce obj = null;

            while (true)
            {
                try
                {
                    var linha = sr.ReadLine();
                    if (string.IsNullOrWhiteSpace(linha))
                        break;

                    try
                    {
                        obj = new TalendSalesforce()
                        {
                            ExternalId = linha.Split(arquivo.Separador)[0],
                            Functionality = linha.Split(arquivo.Separador)[1],
                            Body = JsonConvert.DeserializeObject(linha.Split(arquivo.Separador)[2]).ToString()
                        };

                        lista.Add(obj);
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
                    Console.WriteLine(string.Format("ERRO inexperado: {0} {1}", ex.Message, ex.StackTrace));
                }
            }
            sr.Close();
            return lista;
        }


        /// <summary>
        /// TODO:
        /// </summary>
        /// <param name="arquivo"></param>
        /// <returns></returns>
        public static List<BillingSalesforce> GetBillingsFromFile(Arquivo arquivo)
        {
            #region validação de parâmetros
            if (arquivo == null || string.IsNullOrEmpty(arquivo.Caminho))
                throw new ArgumentException(string.Format("Caminho do arquivo inválido: {0}.", arquivo.CaminhoCompleto));

            if (string.IsNullOrEmpty(arquivo.Nome) || string.IsNullOrEmpty(arquivo.NomeExtensao))
                throw new ArgumentException(string.Format("Nome do arquivo de entrada inválido.: '{0}'", arquivo.NomeExtensao));
            #endregion

            StreamReader sr = new StreamReader(arquivo.CaminhoCompleto, Encoding.GetEncoding("iso-8859-1"));
            List<BillingSalesforce> lista = new List<BillingSalesforce>();
            BillingSalesforce obj ;

            while (true)
            {
                try
                {
                    var linha = sr.ReadLine();
                    if (string.IsNullOrWhiteSpace(linha))
                        break;

                    try
                    {
                        obj = new BillingSalesforce()
                        {
                            AccountSF = linha.Split(arquivo.Separador)[0],
                            Type__c = linha.Split(arquivo.Separador)[1],
                            BallotName__c = linha.Split(arquivo.Separador)[3],
                            Bank__c = linha.Split(arquivo.Separador)[4],
                            BillingAddress__c = linha.Split(arquivo.Separador)[5],
                            CurrentAccountNum__c = linha.Split(arquivo.Separador)[12],
                            CurrentAccountNumber__c = linha.Split(arquivo.Separador)[13],
                            ExternalID__c = linha.Split(arquivo.Separador)[19],
                            PoDSF = linha.Split(arquivo.Separador)[20],
                            AccountContract__c = linha.Split(arquivo.Separador)[21],
                            DeliveryType__c = linha.Split(arquivo.Separador)[22],
                            CNT_Braile__c = linha.Split(arquivo.Separador)[26],
                            CNT_Due_Date__c = linha.Split(arquivo.Separador)[35],
                            Company__c = linha.Split(arquivo.Separador)[36]
                        };

                        lista.Add(obj);
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
                    Console.WriteLine(string.Format("ERRO inexperado: {0} {1}", ex.Message, ex.StackTrace));
                }
            }

            return lista;
        }


        public static List<AccountSalesforce> GetInscricaoMunicipal(Arquivo arquivo)
        {
            #region validação de parâmetros
            if (arquivo == null || string.IsNullOrEmpty(arquivo.Caminho))
                throw new ArgumentException(string.Format("Caminho do arquivo inválido: {0}.", arquivo.CaminhoCompleto));

            if (string.IsNullOrEmpty(arquivo.Nome) || string.IsNullOrEmpty(arquivo.NomeExtensao))
                throw new ArgumentException(string.Format("Nome do arquivo de entrada inválido.: '{0}'", arquivo.NomeExtensao));
            #endregion


            StreamReader sr = new StreamReader(arquivo.CaminhoCompleto, Encoding.GetEncoding("iso-8859-1"));
            List<AccountSalesforce> lista = new List<AccountSalesforce>();
            AccountSalesforce obj;

            while (true)
            {
                try
                {
                    var linha = sr.ReadLine();
                    if (string.IsNullOrWhiteSpace(linha))
                        break;

                    try
                    {
                        obj = new AccountSalesforce()
                        {
                            NumeroCliente = linha.Split(arquivo.Separador)[0],
                            NumeroDocumento = linha.Split(arquivo.Separador)[1],
                            Nome = linha.Split(arquivo.Separador)[2],
                            InscricaoMunicipal = linha.Split(arquivo.Separador)[3],
                        };

                        lista.Add(obj);
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
                    Console.WriteLine(string.Format("ERRO inexperado: {0} {1}", ex.Message, ex.StackTrace));
                }
            }

            return lista;
        }


        public static List<AddressSalesforce> GetAddresses(Arquivo arquivo)
        {
            #region validação de parâmetros
            if (arquivo == null || string.IsNullOrEmpty(arquivo.Caminho))
                throw new ArgumentException(string.Format("Caminho do arquivo inválido: {0}.", arquivo.CaminhoCompleto));

            if (string.IsNullOrEmpty(arquivo.Nome) || string.IsNullOrEmpty(arquivo.NomeExtensao))
                throw new ArgumentException(string.Format("Nome do arquivo de entrada inválido.: '{0}'", arquivo.NomeExtensao));
            #endregion


            StreamReader sr = new StreamReader(arquivo.CaminhoCompleto, Encoding.GetEncoding("iso-8859-1"));
            List<AddressSalesforce> lista = new List<AddressSalesforce>();
            AddressSalesforce obj;

            while (true)
            {
                try
                {
                    var linha = sr.ReadLine();
                    if (string.IsNullOrWhiteSpace(linha))
                        break;

                    try
                    {
                        obj = new AddressSalesforce()
                        {
                            Id = linha.Split(arquivo.Separador)[0],
                            Region = linha.Split(arquivo.Separador)[1],
                        };

                        lista.Add(obj);
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
                    Console.WriteLine(string.Format("ERRO inexperado: {0} {1}", ex.Message, ex.StackTrace));
                }
            }

            return lista;
        }

        public static List<Atividade> GetAtividades(Arquivo arquivo)
        {
            #region validação de parâmetros
            if (arquivo == null || string.IsNullOrEmpty(arquivo.Caminho))
                throw new ArgumentException(string.Format("Caminho do arquivo inválido: {0}.", arquivo.CaminhoCompleto));

            if (string.IsNullOrEmpty(arquivo.Nome) || string.IsNullOrEmpty(arquivo.NomeExtensao))
                throw new ArgumentException(string.Format("Nome do arquivo de entrada inválido.: '{0}'", arquivo.NomeExtensao));
            #endregion


            StreamReader sr = new StreamReader(arquivo.CaminhoCompleto, Encoding.GetEncoding("iso-8859-1"));
            List<Atividade> lista = new List<Atividade>();
            Atividade obj;

            while (true)
            {
                try
                {
                    var linha = sr.ReadLine();
                    if (string.IsNullOrWhiteSpace(linha))
                        break;

                    try
                    {
                        obj = new Atividade()
                        {
                            Id = linha.Split(arquivo.Separador)[0],
                            CasoId = linha.Split(arquivo.Separador)[1]
                        };

                        lista.Add(obj);
                    }
                    catch (Exception ex)
                    {
                        //TODO: log
                        Console.WriteLine(string.Format("ERRO ao carregar o objeto do arquivo de entrada: {0} {1}", linha, ex.StackTrace));
                    }
                }
                catch (Exception ex)
                {
                    //TODO: log
                    Console.WriteLine(string.Format("ERRO inexperado: {0} {1}", ex.Message, ex.StackTrace));
                }
            }

            return lista;
        }



        public static List<ClienteSalesforce> GetEmailsPorCliente(Arquivo arquivo)
        {
            #region validação de parâmetros
            if (arquivo == null || string.IsNullOrEmpty(arquivo.Caminho))
                throw new ArgumentException(string.Format("Caminho do arquivo inválido: {0}.", arquivo.CaminhoCompleto));

            if (string.IsNullOrEmpty(arquivo.Nome) || string.IsNullOrEmpty(arquivo.NomeExtensao))
                throw new ArgumentException(string.Format("Nome do arquivo de entrada inválido.: '{0}'", arquivo.NomeExtensao));
            #endregion


            StreamReader sr = new StreamReader(arquivo.CaminhoCompleto, Encoding.GetEncoding("iso-8859-1"));
            List<ClienteSalesforce> lista = new List<ClienteSalesforce>();
            ClienteSalesforce obj;

            while (true)
            {
                try
                {
                    var linha = sr.ReadLine();
                    if (string.IsNullOrWhiteSpace(linha))
                        break;

                    try
                    {
                        obj = new ClienteSalesforce()
                        {
                            NumeroCliente = linha.Split(arquivo.Separador)[0],
                            Email = linha.Split(arquivo.Separador)[1]
                        };

                        lista.Add(obj);
                    }
                    catch (Exception ex)
                    {
                        //TODO: log
                        Console.WriteLine(string.Format("ERRO ao carregar o objeto do arquivo de entrada: {0} {1}", linha, ex.StackTrace));
                    }
                }
                catch (Exception ex)
                {
                    //TODO: log
                    Console.WriteLine(string.Format("ERRO inexperado: {0} {1}", ex.Message, ex.StackTrace));
                }
            }

            return lista;
        }


        /// <summary>
        /// Recupera todas as linhas de um arquivo texto para uma lista.
        /// </summary>
        /// <param name="arquivo"></param>
        /// <param name="delimitado"></param>
        /// <returns></returns>
        public static List<string> GetTextosToList(Arquivo arquivo, bool delimitado = false)
        {
            #region validação de parâmetros
            if (arquivo == null || string.IsNullOrEmpty(arquivo.Caminho))
                throw new ArgumentException(string.Format("Caminho do arquivo inválido: {0}.", arquivo.CaminhoCompleto));

            if (string.IsNullOrEmpty(arquivo.Nome) || string.IsNullOrEmpty(arquivo.NomeExtensao))
                throw new ArgumentException(string.Format("Nome do arquivo de entrada inválido.: '{0}'", arquivo.NomeExtensao));
            #endregion

            List<string> lista = new List<string>();
            using (StreamReader sr = new StreamReader(arquivo.CaminhoCompleto, Encoding.GetEncoding("UTF-8")))
            {
                while (!sr.EndOfStream)
                {
                    try
                    {
                        var linha = sr.ReadLine();
                        if (string.IsNullOrEmpty(linha))
                            continue;

                        try
                        {
                            if (delimitado)
                                lista.Add(string.Concat("'", linha, "'"));
                            else
                                lista.Add(linha);
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
            }
            return lista;
        }
    }
}

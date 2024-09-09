using basicSample_cs_p;
using IBM.Data.Informix;
using NLog;
//using Pangea.Util.SFTP;
using SalesforceExtractor.apex;
using SalesforceExtractor.Dados;
using SalesforceExtractor.Dados.SalesForce;
using SalesforceExtractor.Entidades;
using SalesforceExtractor.Entidades.Enumeracoes;
//using SalesforceExtractor.Entidades.Enumeracoes;
using SalesforceExtractor.Entidades.Modif;
using SalesforceExtractor.Utils;
using Solus.Controle.SalesForce;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.Authentication;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Xml;

namespace SalesforceExtractor
{
    public class Negocio : IDisposable
    {
        private SforceService binding = null;
        SFDCSchemeBuild schema = new SFDCSchemeBuild();
        private bool loggedIn = false;
        private const string EXTENSAO = "TXT";
        private string codigoEmpresa = string.Empty;
        private string ambiente = string.Empty;
        private List<string> lstLog = new List<string>();
        public Log log = new Log();

        public Negocio(string ambiente, SforceService binding, string codigoEmpresa)
        {
            this.ambiente = ambiente;
            this.codigoEmpresa = codigoEmpresa;
            this.binding = binding;
            this.loggedIn = (this.binding != null && this.binding.SessionHeaderValue != null && !string.IsNullOrWhiteSpace(this.binding.SessionHeaderValue.sessionId));
        }


        public void Dispose()
        {
            this.binding = null;
            this.schema = null;
            this.loggedIn = false;
            this.codigoEmpresa = null;
        }


        [ComVisible(true)]
        public void AtualizarModif(string empresa, int qtdDias)
        {
            Console.WriteLine(string.Format("Autenticando em {0} ...", this.ambiente));
            if (!loggedIn)
            {
                Autenticacao auth = new Autenticacao(this.ambiente);
                if (!auth.ValidarLogin(ref this.binding))
                    return;
                this.loggedIn = true;
            }
            SalesforceDAO.AtualizarModif(empresa, qtdDias, TipoCliente.GB, ref binding);
        }


        /// <summary>
        /// Extrai do Salesforce os Id referente ao número de cliente informado.
        /// </summary>
        [ComVisible(true)]
        internal void GerarRelatorioIdContatoSalesforce()
        {
            int contadorTotal = 0;
            string pEmpresa = ConfigurationManager.AppSettings.Get("codigoEmpresa");

            string empresa = "2003".Equals(pEmpresa) ? "CE" :
                 "2005".Equals(pEmpresa) ? "RJ" :
                 "2018".Equals(pEmpresa) ? "GO" : string.Empty;

            //Login no serviço
            if (!loggedIn)
            {
                Autenticacao auth = new Autenticacao(this.ambiente);
                if (!auth.ValidarLogin(ref this.binding))
                    return;

                this.loggedIn = true;
            }

            String data = DateTime.Now.ToString("yyyyMMdd");

            QueryResult qr = null;
            binding.QueryOptionsValue = new SalesforceExtractor.apex.QueryOptions();
            binding.QueryOptionsValue.batchSize = 3;
            binding.QueryOptionsValue.batchSizeSpecified = true;

            StringBuilder sbNomeArquivo = new StringBuilder();

            try
            {
                sbNomeArquivo.Append(string.Format("{0}{1}{2}{3}",
                    ConfigurationManager.AppSettings.Get("local_arquivo")
                    , ConfigurationManager.AppSettings.Get("nomeArquivo")
                    , "_RESULTADO_", empresa));

                if (string.IsNullOrEmpty(empresa))
                {
                    throw new Exception("Código de Empresa inválido.  Configure a aplicação para '2003' para CE, '2005' para RJ ou '2018' para Goiás.");
                }

                Console.WriteLine(string.Format("\nPreparando gravação do arquivo '{0}' ...", sbNomeArquivo.ToString()));
                Console.WriteLine(string.Format("(se o arquivo já existir, os dados extraídos serão adicionados ao conteúdo atual)\n"));

                string cliente = "";
                StreamReader sr = new StreamReader(string.Format("{0}{1}.{2}",
                    ConfigurationManager.AppSettings.Get("local_arquivo")
                    , ConfigurationManager.AppSettings.Get("nomeArquivo")
                    , EXTENSAO)
                    , Encoding.GetEncoding("ISO-8859-1")
                    );

                Console.WriteLine(string.Format("Extraindo clientes da base no {0} ...\n", empresa));

                while (!sr.EndOfStream)
                {
                    int contadorAux = 0;
                    for (int i = 1; i <= 50; i++)
                    {
                        contadorAux = i;
                        try
                        {
                            string registro = sr.ReadLine();
                            if (cliente.Length > 0 && !string.IsNullOrEmpty(registro))
                                cliente += ", ";

                            cliente += "'" + registro.Replace('|', ' ').Trim() + "'";
                        }
                        catch
                        {
                            contadorTotal--;
                        }
                    }

                    contadorTotal += contadorAux;

                    //cliente = sr.ReadLine().Replace('|', ' ').Trim() ;

                    String sql = string.Format(@"SELECT contact__r.account.externalid__c,
                    contact__r.account.IdentityNumber__c,
                    contact__r.account.IdentityType__c,
                    contact__r.accountid,
                    contact__r.externalid__c,
                    contact__r.id,
                    contact__r.name,
                    asset__r.PointofDelivery__r.PointofDeliveryNumber__c,
                    asset__r.PointofDelivery__r.id,
                    asset__r.PointofDelivery__r.externalid__c,
                    asset__r.id,
                    asset__r.externalid__c,
                    asset__r.PointofDelivery__r.company__c 
                    FROM ServiceProduct__c WHERE contact__r.externalid__c != ''
                    AND asset__r.PointofDelivery__r.companyid__c='{0}'
                    AND Asset__r.PointofDelivery__r.PointofDeliveryNumber__c in ({1})", pEmpresa, cliente);

                    bool bContinue = false;
                    //int loopCounter = 0;

                    try
                    {
                        qr = binding.query(sql);

                        bContinue = true;
                        //loopCounter = 0;
                    }
                    catch (Exception ex)
                    {
                        Debugger.Break();
                        throw ex;
                    }

                    cliente = "";

                    while (bContinue)
                    {

                        if (qr.records == null)
                        {
                            //EscreverArquivo("clientes.txt", cliente + "\n");
                            bContinue = false;
                            continue;
                        }

                        for (int i = 0; i < qr.records.Length; i++)
                        {
                            SalesforceExtractor.apex.sObject con = qr.records[i];


                            String id_conta = "";
                            String id_externo_conta = "";

                            String id_contato = "";
                            String id_externo_contato = "";
                            String nome = "";

                            String id_suministro = "";
                            String id_externo_suministro = "";
                            String numero_cliente = "";

                            String codEmpresa = "";

                            String id_sales_asset = "";
                            String id_externo_asset = "";

                            String documento = "";
                            String tipo_documento = "";

                            id_conta = schema.getFieldValueMore("contact__r", "", "", "Accountid", con.Any);
                            id_externo_conta = schema.getFieldValueMore("contact__r", "account", "", "externalid__c", con.Any);

                            id_contato = schema.getFieldValueMore("contact__r", "", "", "id", con.Any);
                            id_externo_contato = schema.getFieldValueMore("contact__r", "", "", "externalid__c", con.Any);
                            nome = schema.getFieldValueMore("contact__r", "", "", "name", con.Any);

                            id_suministro = schema.getFieldValueMore("asset__r", "PointofDelivery__r", "", "id", con.Any);
                            id_externo_suministro = schema.getFieldValueMore("asset__r", "PointofDelivery__r", "", "externalid__c", con.Any);
                            numero_cliente = schema.getFieldValueMore("asset__r", "PointofDelivery__r", "", "PointofDeliveryNumber__c", con.Any);
                            codEmpresa = schema.getFieldValueMore("asset__r", "PointofDelivery__r", "", "company__c", con.Any);

                            id_sales_asset = schema.getFieldValueMore("asset__r", "", "", "id", con.Any); ;
                            id_externo_asset = schema.getFieldValueMore("asset__r", "", "", "externalid__c", con.Any);

                            documento = schema.getFieldValueMore("contact__r", "", "", "IdentityNumber__c", con.Any);
                            tipo_documento = schema.getFieldValueMore("contact__r", "", "", "IdentityType__c", con.Any);

                            String reg = "";

                            reg += id_externo_conta + "|";
                            reg += id_conta + "|";
                            reg += id_externo_contato + "|";
                            reg += id_contato + "|";
                            reg += nome + "|";
                            reg += numero_cliente + "|";
                            reg += id_suministro + "|";
                            reg += id_externo_suministro + "|";
                            reg += codEmpresa + "|";
                            reg += id_sales_asset + "|";
                            reg += id_externo_asset + "|";
                            reg += tipo_documento + "|";
                            reg += documento + "|";


                            IO.EscreverArquivo(sbNomeArquivo.ToString(), string.Concat(reg, "\n"));

                        }

                        //handle the loop + 1 problem by checking to see if the most recent queryResult
                        if (qr.done)
                        {
                            bContinue = false;
                            //Console.WriteLine("Registrou: " + loopCounter);
                        }
                        else
                            qr = binding.queryMore(qr.queryLocator);
                    }

                    int resto = 0;
                    Math.DivRem(contadorTotal, 5000, out resto);

                    if (resto == 0)
                        Console.WriteLine(string.Concat(contadorTotal, " às ", DateTime.Now.ToShortTimeString()));
                }

                Console.WriteLine(string.Format("\nTotal clientes extraídos: {0}", contadorTotal));
                Console.WriteLine(string.Format("\nArquivo gerado: {0}", sbNomeArquivo.ToString()));
            }
            catch (Exception ex)
            {
                if (sbNomeArquivo == null || sbNomeArquivo.Length <= 0)
                    sbNomeArquivo.Append(string.Format("{0}_RESULTADO.txt", DateTime.Now.ToString("yyyyMMdd HH:mm:ss")));

                IO.EscreverArquivo(sbNomeArquivo.ToString(), string.Concat(ex.Message, ex.StackTrace) + "\n");
                Console.WriteLine(string.Format("\nProblema para executar a query: {0}\n", ex.Message));
            }
        }



        
        /// <summary>
        /// Percorre os ExternalId's de um determinado arquivo e verifica o cadastro correspondente no Salesforce.
        /// </summary>
        /// <param name="codigoEmpresa">Empresa a consultar: 2005=RJ,  2003=CE</param>
        /// <param name="arquivo">Instância de Arquivo, com os dados do arquivo de entrada.</param>
        /// <param name="arquivoSaida">Instância de Arquivo com dados para gravação do resultado do processamento.</param>
        [ComVisible(true)]
        internal void GerarRelatorioContatosDuplicados(Arquivo arquivo, Arquivo arquivoSaida)
        {
            #region validação de parâmetros
            if (string.IsNullOrEmpty(this.codigoEmpresa))
                throw new ArgumentException("Código de Empresa não especificado.");

            if (arquivo == null || string.IsNullOrEmpty(arquivo.Caminho))
                throw new ArgumentException(string.Format("Caminho do arquivo inválido: {0}.", arquivo.CaminhoCompleto));

            if (string.IsNullOrEmpty(arquivo.Nome) || string.IsNullOrEmpty(arquivo.NomeExtensao))
                throw new ArgumentException(string.Format("Nome do arquivo de entrada inválido.: '{0}'", arquivo.NomeExtensao));

            if (arquivoSaida == null || string.IsNullOrEmpty(arquivoSaida.Caminho))
                throw new ArgumentException(string.Format("Caminho do arquivo de saída inválido: {0}.", arquivoSaida.CaminhoCompleto));

            if (string.IsNullOrEmpty(arquivoSaida.Nome) || string.IsNullOrEmpty(arquivoSaida.NomeExtensao))
                throw new ArgumentException(string.Format("Nome do arquivo de saída inválido.: '{0}'", arquivoSaida.NomeExtensao));

            if (string.IsNullOrEmpty(codigoEmpresa))
                throw new ArgumentException("Informe o código da Empresa de onde os dados serão consultados.");
            #endregion

            #region autenticação SF
            if (!loggedIn)
            {
                Autenticacao auth = new Autenticacao(this.ambiente);
                if (!auth.ValidarLogin(ref this.binding))
                    return;
                this.loggedIn = true;
            }
            #endregion


            #region navegação no arquivo de entrada
            if(!File.Exists(arquivo.CaminhoCompleto))
                throw new Exception(string.Format("Arquivo '{0}' não encontrado.", arquivo.CaminhoCompleto));

            StreamReader sr = new StreamReader(arquivo.CaminhoCompleto, Encoding.ASCII);
            List<ClienteSalesforce> cli = null;
            string id = null;

            int cont = 0;
            while (true)
            {
                try
                {
                    var linha = sr.ReadLine();
                    if (string.IsNullOrEmpty(linha))
                        break;

                    //consultar ExternalId no SF
                    try
                    {
                        id = linha.Split(arquivo.Separador)[0];
                        cli = SalesforceDAO.GetContatosPorExternalId(this.codigoEmpresa, id, ref binding);
                        if (cli == null || cli.Count == 0)
                            throw new NullReferenceException(id);

                        if (cli.Count > 1)
                        {
                            Console.WriteLine(string.Format("{0}|{1}", cli.Count, id));
                            IO.EscreverArquivo(string.Concat(arquivoSaida.Caminho, arquivoSaida.Nome),
                                string.Format("{0}|{1}", cli.Count(), cli[0].ExternalId)
                                );
                        }
                        else
                        {
                            //Console.WriteLine(string.Format("1|{0}", id));
                            //IO.EscreverArquivo(string.Concat(arquivoSaida.Caminho, arquivoSaida.Nome),
                            //    string.Format("{0}|{1}", "1", cli[0].ExternalId)
                            //    );
                        }
                    }
                    catch (NullReferenceException ex)
                    {
                        Console.WriteLine(string.Format("0|{0}", id));
                        IO.EscreverArquivo(string.Concat(arquivoSaida.Caminho, arquivoSaida.Nome),
                            string.Format("{0}|{1}", "0", id)
                            );
                    }
                    catch (Exception ex)
                    {
                        throw ex;
                    }

                    var arrCampos = linha.Trim().Split(new char[] { Convert.ToChar(arquivo.Separador) });

                    cont++;
                    int resto = 0;
                    Math.DivRem(cont, 500, out resto);

                    if (resto == 0)
                        Console.WriteLine(string.Concat(cont, " às ", DateTime.Now.ToShortTimeString()));
                }
                catch (Exception ex)
                {
                    IO.EscreverArquivo(string.Concat(arquivoSaida.Caminho, arquivoSaida.Nome),
                        string.Format("{0}: {1}", "ERRO", ex.Message)
                        );
                    // throw ex;
                }
            }
            #endregion
        }


        /// <summary>
        /// Atualiza dados de Redes Sociais a partir de uma lista pré-determinada.
        /// </summary>
        [ComVisible(true)]
        internal void AtualizarRedeSocial(Arquivo arquivo)
        {
            if (!loggedIn)
            {
                Autenticacao auth = new Autenticacao(this.ambiente);
                if (!auth.ValidarLogin(ref this.binding))
                    return;

                this.loggedIn = true;
            }

            int i = 0;
            sObject update = null;
            List<sObject> lista = new List<sObject>();

            try
            {
                List<DocumentoRedeSocial> listaDocumentos = ArquivoLoader.GetListDocRedesSociais(arquivo);

                using (Arquivo arqSaida = new Arquivo(arquivo.Caminho, string.Concat(arquivo.Nome, "_SAIDA"), arquivo.Extensao))
                {
                    ClienteSalesforce cli = null;
                    //realizo o while para ler o conteudo da linha, Adriel aqui você tem que aplicar a sua lógica
                    //Que pode ser de um arquivo de texto de uma lista que seja a preferência é sua!
                    foreach (DocumentoRedeSocial rs in listaDocumentos)
                    {
                        cli = null;
                        //TODO: validar parametros
                        for (int t = 0; t < 3; t++)
                        {
                            if (cli != null)
                                continue;

                            int inicio = t == 0 ? 0 : t == 1 ? 8 : 11;
                            cli = SalesforceDAO.GetContatoPorDocumento(rs.codigoEmpresa, rs.documentoNumero.PadLeft(22, '0').Substring(inicio), ref binding);

                            if (cli == null && t == 2)
                            {
                                Console.WriteLine(string.Format("ERRO: Documento não localizado: {0}-{1}", rs.codigoEmpresa, rs.documentoNumero));
                                IO.EscreverArquivo(arqSaida, string.Format("ERRO: Documento não localizado: {0}-{1}", rs.codigoEmpresa, rs.documentoNumero));
                                continue;
                            }
                        }
                        if (cli == null)
                            continue;

                        i++;

                        //tipo de objeto do SalesForce
                        update = new sObject();
                        update.type = rs.entidadeSF;

                        //Aqui será o id do objeto recuperado
                        update.Id = cli.IdContato;
                        update.Any = new System.Xml.XmlElement[] { SFDCSchemeBuild.GetNewXmlElement(rs.camposRedeSocial, rs.redeSocialId) };

                        lista.Add(update);
                        Console.WriteLine(string.Format("Documento OK: {0} :: {1}-{2}", lista.Count, rs.codigoEmpresa, rs.documentoNumero));
                        IO.EscreverArquivo(arqSaida, string.Format("Documento OK: {0} :: {1}-{2}", lista.Count, rs.codigoEmpresa, rs.documentoNumero));

                        //Update a cada 200 registros, por limitação do Sales ce
                        if (i == 199)
                        {
                            SaveResult[] saveResults = binding.update(lista.ToArray());
                            Console.WriteLine(string.Format("Registros atualizados: {0}", saveResults.Count()));
                            IO.EscreverArquivo(arqSaida, string.Format("Registros atualizados: {0}", saveResults.Count()));
                            i = 0;
                            lista.Clear();
                        }

                    }  // fim foreach

                    //Update do remanescente da lista
                    if (i < 200)
                    {
                        SaveResult[] saveResults = binding.update(lista.ToArray());
                        Console.WriteLine(string.Format("Registros atualizados: {0}", saveResults.Count()));
                        IO.EscreverArquivo(arqSaida, string.Format("Registros atualizados: {0}", saveResults.Count()));
                        i = 0;
                        lista.Clear();
                    }

                }  //fim arquivoSaida

            }
            catch (Exception ex)
            {
                Console.WriteLine("\nErro ao atualizar o registro: \n" + ex.Message);
            }
        }


        /// <summary>
        /// Atualiza dados de Redes Sociais a partir de uma lista pré-determinada.
        /// </summary>
        [ComVisible(true)]
        internal void AtualizarResponsavelPorContato(Arquivo arquivo, string codigoEmpresa)
        {
            if (string.IsNullOrWhiteSpace(codigoEmpresa))
                return;

            this.codigoEmpresa = codigoEmpresa;

            if (!loggedIn)
            {
                Autenticacao auth = new Autenticacao(this.ambiente);
                if (!auth.ValidarLogin(ref this.binding))
                    return;

                this.loggedIn = true;
            }

            int i = 0;
            int totalAtualizado = 0;
            int contCliente = 0;
            sObject update = null;
            List<sObject> listaUpdate = new List<sObject>();

            try
            {
                List<ClienteSalesforce> listaArq = ArquivoLoader.GetResponsaveis(arquivo);
                StringBuilder msgLog = new StringBuilder();
                int sucesso = 0;
                int falha = 0;

                using (Arquivo arqSaida = new Arquivo(arquivo.Caminho, string.Concat(arquivo.Nome, "_SAIDA"), arquivo.Extensao))
                {
                    foreach (ClienteSalesforce rs in listaArq)
                    {
                        sucesso = 0;
                        falha = 0;
                        msgLog.Clear();
                        contCliente++;

                        //validação de documentos
                        if (string.IsNullOrEmpty(rs.ResponsavelTipoDocumento) || "1".Equals(rs.ResponsavelTipoDocumento))
                        {
                            msgLog.AppendLine(string.Format("Document Invalido: {0} - {1}", rs.ExternalId, rs.ResponsavelNome));
                            Console.WriteLine(msgLog.ToString());
                            IO.EscreverArquivo(arqSaida, msgLog.ToString()); 
                            continue;
                        }

                        //TODO: TRATAR POSSIVEIS CPF COM ZERO À ESQUERDA, CONVERTIDOS EM NUMERICO NO CSV
                        if ((string.IsNullOrEmpty(rs.ResponsavelTipoDocumento) || "5".Equals(rs.ResponsavelTipoDocumento)) &&
                            rs.ResponsavelDocumento.Length != 11)
                        {
                            msgLog.AppendLine(string.Format("CPF Incorreto: {0} - {1}", rs.ExternalId, rs.ResponsavelDocumento));
                            Console.WriteLine(msgLog.ToString());
                            IO.EscreverArquivo(arqSaida, msgLog.ToString());
                            continue;
                        }

                        if ((string.IsNullOrEmpty(rs.ResponsavelTipoDocumento) || "3".Equals(rs.ResponsavelTipoDocumento)) &&
                            rs.ResponsavelDocumento.Length != 14)
                        {
                            msgLog.AppendLine(string.Format("CNPJ Incorreto: {0} - {1}", rs.ExternalId, rs.ResponsavelDocumento));
                            Console.WriteLine(msgLog.ToString());
                            IO.EscreverArquivo(arqSaida, msgLog.ToString());
                            continue;
                        }

                        List<ClienteSalesforce> lstCli = SalesforceDAO.GetContatosPorExternalId(this.codigoEmpresa, rs.ExternalId, ref binding);

                        if (lstCli == null || lstCli.Count == 0)
                        {
                            msgLog.AppendLine(string.Format("ERRO: Registro não localizado: {0}", rs.ExternalId));
                            Console.WriteLine(msgLog.ToString());
                            IO.EscreverArquivo(arqSaida, msgLog.ToString());
                            continue;
                        }
                        i++;

                        //tipo de objeto do SalesForce
                        update = new sObject();
                        update.type = "Account";

                        //Aqui será o id do objeto recuperado
                        update.Id = lstCli.First().IdConta;
                        update.Any = new System.Xml.XmlElement[] {
                            SFDCSchemeBuild.GetNewXmlElement("CNT_Responsible_Name__c", rs.ResponsavelNome),
                            SFDCSchemeBuild.GetNewXmlElement("CNT_Resp_ID_Number__c", rs.ResponsavelDocumento) ,
                            SFDCSchemeBuild.GetNewXmlElement("CNT_Resp_ID_Type__c", rs.ResponsavelDescTipoDocumento) ,
                            SFDCSchemeBuild.GetNewXmlElement("CNT_Responsible_Phone__c", rs.ResponsavelTelefone) ,
                            SFDCSchemeBuild.GetNewXmlElement("CNT_Responsible_Email__c", rs.ResponsavelEmail) 
                        };

                        listaUpdate.Add(update);
                        totalAtualizado++;

                        msgLog.AppendLine(string.Format("Registro OK: {0} :: {1} - {2}", contCliente, rs.ExternalId, rs.ResponsavelNome));
                        Console.WriteLine(msgLog.ToString());
                        IO.EscreverArquivo(arqSaida, msgLog.ToString());

                        //Update a cada 200 registros, por limitação do Sales ce
                        if (listaUpdate.Count > 0 && i == 199)
                        {
                            SaveResult[] saveResults = binding.update(listaUpdate.ToArray());
                            sucesso = saveResults.Where(x => x.success = true).Count();
                            falha = saveResults.Where(x => x.success = false).Count();
                            msgLog.AppendLine(string.Format("Totais:  Sucesso {0},  Erro {1}", sucesso, falha));

                            Console.WriteLine(msgLog.ToString());
                            IO.EscreverArquivo(arqSaida, msgLog.ToString());
                            i = 0;
                            listaUpdate.Clear();
                        }
                    }  // fim foreach


                    //Update do remanescente da lista
                    if (i < 200)
                    {
                        totalAtualizado += i;
                        SaveResult[] saveResults = binding.update(listaUpdate.ToArray());
                        sucesso = saveResults.Where(x => x.success = true).Count();
                        falha = saveResults.Where(x => x.success = false).Count();
                        msgLog.AppendLine(string.Format("Últimos Totais:  Sucesso {0},  Erro {1}", sucesso, falha));

                        Console.WriteLine(msgLog.ToString());
                        IO.EscreverArquivo(arqSaida, msgLog.ToString());
                        i = 0;
                        listaUpdate.Clear();
                    }

                    msgLog.AppendLine(string.Format("Total clientes: {0},  Total Atualizado {1}", contCliente, totalAtualizado));
                    Console.WriteLine(msgLog.ToString());
                    IO.EscreverArquivo(arqSaida, msgLog.ToString());

                }  //fim arquivoSaida

                Console.ReadKey();
            }
            catch (Exception ex)
            {
                Console.WriteLine("\nErro ao atualizar o registro: \n" + ex.Message);
            }
        }



        /// <summary>
        /// Atualiza email no Salesforce a partir de uma lista de clientes pré-determinada.
        /// </summary>
        [ComVisible(true)]
        internal void AtualizarEmails(Arquivo arquivo, string codigoEmpresa)
        {
            if (string.IsNullOrWhiteSpace(codigoEmpresa))
                return;

            this.codigoEmpresa = codigoEmpresa;

            if (!loggedIn)
            {
                Autenticacao auth = new Autenticacao(this.ambiente);
                if (!auth.ValidarLogin(ref this.binding))
                    return;

                this.loggedIn = true;
            }

            int i = 0;
            int totalAtualizado = 0;
            int contCliente = 0;
            sObject update = null;
            List<sObject> listaUpdate = new List<sObject>();

            try
            {
                List<ClienteSalesforce> listaArq = complementarClientes(ArquivoLoader.GetEmailsPorCliente(arquivo));
                StringBuilder msgLog = new StringBuilder();

                using (Arquivo arqSaida = new Arquivo(arquivo.Caminho, string.Concat(arquivo.Nome, "_SAIDA"), arquivo.Extensao))
                {
                    foreach (ClienteSalesforce rs in listaArq)
                    {
                        msgLog.Clear();
                        contCliente++;

                        //validação de documentos
                        if (string.IsNullOrWhiteSpace(rs.Id) || string.IsNullOrWhiteSpace(rs.Email))
                        {
                            msgLog.Append(string.Format("{0} [ERRO] Cliente inválido ou não encontrado\t{1}", DateTime.Now.ToLongTimeString(), rs.NumeroCliente));
                            Console.WriteLine(msgLog.ToString());
                            IO.EscreverArquivo(arqSaida, msgLog.ToString());
                            continue;
                        }

                        i++;

                        //tipo de objeto do SalesForce
                        update = new sObject();
                        update.type = "Account";

                        //Aqui será o id do objeto recuperado
                        update.Id = rs.Id;
                        update.Any = new System.Xml.XmlElement[] {
                            SFDCSchemeBuild.GetNewXmlElement("PrimaryEmail__c", rs.Email)
                        };

                        listaUpdate.Add(update);
                        totalAtualizado++;

                        msgLog.Append(string.Format("{0} [OK] {1}\t{2} [{3}]\t{4}", DateTime.Now.ToLongTimeString(), contCliente, rs.Id, rs.NumeroCliente, rs.Email));
                        Console.WriteLine(msgLog.ToString());
                        IO.EscreverArquivo(arqSaida, msgLog.ToString());

                        //Update a cada 200 registros, por limitação do Sales ce
                        if (listaUpdate.Count > 0 && i == 199)
                        {
                            Console.WriteLine("Gravando...");
                            SaveResult[] saveRecordType = binding.update(listaUpdate.ToArray());

                            string idsUpdate = string.Join(",", listaUpdate.Select(t => t.Id).ToArray());
                            List<Error[]> arrErros = saveRecordType.Where(e => e.errors != null).Select(t => t.errors).ToList();
                            try
                            {
                                if (arrErros != null && arrErros.Count > 0)
                                {
                                    foreach (Error[] err in arrErros)
                                    {
                                        Console.WriteLine(string.Format("{0}\t[ERRO] {1} {2}", DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss"), idsUpdate, err[0].message));
                                        IO.EscreverArquivo(arqSaida, string.Format("{0}\t[ERRO] {1} {2}", DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss"), idsUpdate, err[0].message));
                                    }
                                }
                                else
                                {
                                    //Console.WriteLine(string.Format("{0}\t[OK] {1}", DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss"), idsUpdate));
                                    //IO.EscreverArquivo(arqSaida, string.Format("{0}\t[OK] {1}", DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss"), idsUpdate));
                                }

                                i = 0;
                            }
                            catch(Exception ex)
                            {
                                Console.WriteLine(string.Format("{0}\t[EX] {1} {2}", DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss"), idsUpdate, ex.Message));
                                IO.EscreverArquivo(arqSaida, string.Format("{0}\t[EX] {1} {2}", DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss"), idsUpdate, ex.Message));
                            }

                            listaUpdate.Clear();
                        }
                    }  // fim foreach


                    //Update do remanescente da lista
                    if (i > 0 && i < 200)
                    {
                        SaveResult[] saveRecordType = binding.update(listaUpdate.ToArray());
                        string idsUpdate = string.Join(",", listaUpdate.Select(t => t.Id).ToArray());
                        List<Error[]> arrErros = saveRecordType.Where(e => e.errors != null).Select(t => t.errors).ToList();

                        try
                        {
                            if (arrErros != null && arrErros.Count > 0)
                            {
                                foreach (Error[] err in arrErros)
                                {
                                    Console.WriteLine(string.Format("{0}\t[ERRO] {1} {2}", DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss"), idsUpdate, err[0].message));
                                    IO.EscreverArquivo(arqSaida, string.Format("{0}\t[ERRO] {1} {2}", DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss"), idsUpdate, err[0].message));
                                }
                            }
                            else
                            {
                                //Console.WriteLine(string.Format("{0}\t[OK] {1}", DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss"), idsUpdate));
                                //IO.EscreverArquivo(arqSaida, string.Format("{0}\t[OK] {1}", DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss"), idsUpdate));
                            }

                            i = 0;
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(string.Format("{0}\t[EX] {1} {2}", DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss"), idsUpdate, ex.Message));
                            IO.EscreverArquivo(arqSaida, string.Format("{0}\t[EX] {1} {2}", DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss"), idsUpdate, ex.Message));
                        }
                    }

                    msgLog.AppendLine(string.Format("Total clientes: {0},  Total Atualizado {1}", contCliente, totalAtualizado));
                    Console.WriteLine(msgLog.ToString());
                    IO.EscreverArquivo(arqSaida, msgLog.ToString());

                }  //fim arquivoSaida
                Debugger.Break();
                Console.ReadKey();
            }
            catch (Exception ex)
            {
                Console.WriteLine("\nErro ao atualizar o registro: \n" + ex.Message);
            }
        }



        /// <summary>
        /// Busca massivamente o Id de Account dado um número de cliente.
        /// Método exclusivo de uso do processo 'AtualizarEmails'.
        /// </summary>
        /// <param name="list"></param>
        /// <returns></returns>
        private List<ClienteSalesforce> complementarClientes(List<ClienteSalesforce> list)
        {
            Console.Write("Buscando Ids Salesforce .");
            int i = 0;
            List<string> lstClientes = new List<string>(); 
            List<ClienteSalesforce> lstCli = new List<ClienteSalesforce>();
            foreach(ClienteSalesforce cli in list)
            {
                if (i == 0 || i % 500 != 0)
                {
                    lstClientes.Add(string.Concat("'", cli.NumeroCliente, "'"));
                    i++;
                }
                else
                {
                    Console.Write(".");
                    lstCli.AddRange(SalesforceDAO.GetContasPorNumeroCliente(this.codigoEmpresa, string.Join(",", lstClientes.ToArray()), ref binding));
                    lstClientes.Clear();
                    i = 0;
                }
            }

            lstCli.AddRange(SalesforceDAO.GetContasPorNumeroCliente(this.codigoEmpresa, string.Join(",", lstClientes.ToArray()), ref binding));

            foreach (ClienteSalesforce cli in lstCli)
            {
                cli.Email = list.Where(c => c.NumeroCliente == cli.NumeroCliente).FirstOrDefault().Email;
            }
            return lstCli;
        }




        /// <summary>
        /// Processamento necessário enquanto a alta de contração não está completa, ingressando/atualizando todos os dados.
        /// <remarks>
        /// Obrigatório rodar no Synergia a consulta para obter os dados usados na atualização.
        /// </remarks>
        /// </summary>
        internal void AtualizarItemAttributes(List<ItemAttribute> listaArq, Arquivo arquivo)
        {
            AtualizarItemAttributes(listaArq, null, arquivo, true);
        }



        /// <summary>
        /// Configura o agrupamento de contas e contratos com base nos arquivos de carga disponibilizados.
        /// </summary>
        /// <param name="arquivo"></param>
        internal void CarregarAgrupamento(Arquivo arqOrgaosControladores, Arquivo arqContratosColetivos, bool atualizarPoDs = true)
        {
            autenticar();

            Log log = new Log(string.Format(@"C:\temp\Carga\190724_Agrupamento\{0}_SAIDA_{1}.txt", arqContratosColetivos.Nome, DateTime.Now.ToString("yyyyMMdd_hhMMss")));

            Dictionary<string, AccountSalesforce> dicOrgaosControladores = new Dictionary<string, AccountSalesforce>();
            Dictionary<string, AccountSalesforce> dicContasBase = new Dictionary<string, AccountSalesforce>();

            #region Órgão Controlador

            log.LogFull(string.Format("Carregando o arquivo {0}..", arqOrgaosControladores.CaminhoCompleto));

            //carrega carga de Id de Accounts que serao os Orgaos Controladores
            List<AccountSalesforce> lstContasOriginais = ArquivoLoader.GetOrgaosControladores(arqOrgaosControladores);

            log.LogFull(string.Format("Carregando o arquivo {0}..", arqContratosColetivos.CaminhoCompleto));

            Dictionary<string, string> contasContratos = ArquivoLoader.GetContratosColetivosEContas(arqContratosColetivos);

            log.LogFull("Buscando Órgãos Controladores previamente criados..");
            SaveResult[] saveResults = null;


            List<string> agrupadorasExistentes = new List<string>();
            List<AccountSalesforce> auxContas = new List<AccountSalesforce>();
            foreach(AccountSalesforce conta in lstContasOriginais)
            {
                auxContas.Add(conta);
                if (auxContas.Count == 200)
                {
                    agrupadorasExistentes.AddRange(SalesforceDAO.GetContasAgrupadorasCorrespondente(auxContas, ref this.binding));
                    auxContas.Clear();
                }
            }
            if(auxContas.Count > 0)
                agrupadorasExistentes.AddRange(SalesforceDAO.GetContasAgrupadorasCorrespondente(auxContas, ref this.binding));

            //recupera as contas-base (contas-mãe)
            List<string> externalIdsApagados = new List<string>();
            foreach (string externalId in agrupadorasExistentes)
            {
                string _externalId = externalId.Split(new char[] { '|' })[1];

                if(externalIdsApagados.Contains(_externalId))
                    continue;
                
                externalIdsApagados.Add(_externalId);

                AccountSalesforce contaBase = SalesforceDAO.GetContasPorExternalId(_externalId, ref this.binding).FirstOrDefault();
                contaBase.ParentId = externalId.Split(new char[] { '|' })[0];

                if (contaBase == null)
                {
                    log.LogFull(string.Format("[ERRO ACCOUNT EXISTENTE] External Id não encontrado: {0}"
                    , externalId));

                    continue;
                }

                dicContasBase.Add(_externalId, contaBase);
                lstContasOriginais.RemoveAll(x => x.ExternalId == _externalId);
            }

            log.LogFull(string.Format("Controladoras existentes: {0}", agrupadorasExistentes.Count));
            log.LogFull(string.Format("Controladoras a criar: {0}", lstContasOriginais.Count));

            //ingressa as Controladoras para as contas originais que ainda não tiveram a Controladora correspondente ingressada
            foreach (AccountSalesforce conta in lstContasOriginais)
            {
                try
                {
                    AccountSalesforce contaBase = SalesforceDAO.GetContasPorExternalId(conta.ExternalId, ref this.binding).FirstOrDefault();

                    if(contaBase == null)
                    {
                        log.LogFull(string.Format("[ERRO ACCOUNT] External Id não encontrado: {0}"
                        , conta.ExternalId));

                        continue;
                    }

                    AccountSalesforce contaControladora = contaBase.Clone();
                    dicContasBase.Add(conta.ExternalId, contaBase);

                    contaControladora.ExternalId = string.Concat(contaBase.ExternalId, "OC");
                    contaControladora.TipoCondominio = "1";
                    //contaControladora.ParentId = contaBase.Id;
                    contaControladora.CondominiumRUT__c = conta.CodigoOrgaoControlador;
                    contaControladora.Nome = string.Concat(contaBase.Nome, " [OC]");

                    //======================================
                    //------- INGRESSAR CONTROLADORA -------
                    //======================================
                    contaControladora.Id = IngressarContaControladora(contaControladora);
                    contaBase.ParentId = contaControladora.Id;
                    dicOrgaosControladores.Add(conta.ExternalId, contaControladora);

                    log.LogFull(string.Format("ExternalId {0}\tControlador {1}", conta.ExternalId, contaControladora.Id));

                    #region Criando Contract

                    //consulta contrato de agrupamento existente

                    List<ContractSalesforce> _contratos = SalesforceDAO.GetContratoAgrupamentoPorExternalId(contaControladora.ExternalId, conta.CodigoOrgaoControlador, ref binding);
                    ContractSalesforce novoContrato = _contratos.FirstOrDefault();
                    if (_contratos != null && _contratos.Count > 0 && !string.IsNullOrWhiteSpace(novoContrato.Id))
                    {
                        //TODO: apagar contador de _contratos
                        if (_contratos.Count > 1)
                            Debugger.Break();

                        continue;
                    }
                    novoContrato = new ContractSalesforce();
                    novoContrato.Account = contaControladora.Id;
                    novoContrato.Name = contaControladora.Nome;
                    novoContrato.DataInicio = DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss.000+0000");
                    //newContract.CNT_Case__c = thisCase.Id;
                    novoContrato.TipoAgrupamento = "CNT002";
                    novoContrato.AreaAgrupamento = "4";
                    novoContrato.SegmentoAgrupamento = " ";
                    novoContrato.NumeroAgrupamento = conta.CodigoOrgaoControlador;
                    novoContrato.ExternalId = contaControladora.ExternalId;

                    //===================================================
                    //--------- INGRESSAR CONTRATO CONTROLADORA ---------
                    //===================================================
                    novoContrato.Id = IngressarContratoControladora(novoContrato);
                    log.LogFull(string.Format("Novo Contrato Controladora: {0}", novoContrato.Id));

                    #endregion


                    #region Criar ContractLine

                    ContractLineItemSalesforce novoCline = new ContractLineItemSalesforce();
                    novoCline.ContractId = novoContrato.Id;
                    novoCline.CNT_Status__c = "Active";

                    //===================================================
                    //------ INGRESSAR CONTRACT LINE CONTROLADORA -------
                    //===================================================
                    novoCline.Id = IngressarContractLineControladora(ref novoCline);
                    log.LogFull(string.Format("Novo CLine Controladora: {0}", novoCline.Id));

                    #endregion
                }
                catch(Exception ex)
                {
                    log.LogFull(string.Format("[ERRO CATASTROFICO CONTROLADOR] Account ExternalId: {0} {1} {2}"
                        , conta.ExternalId
                        , ex.Message
                        , ex.StackTrace));
                }
            }
            #endregion

            log.LogFull("Criando Contratos Coletivos..");

            List<string> contasAtualizadas = new List<string>();

            #region Contrato Coletivo
            try
            {
                List<ContratoColetivoEntrada> contratosColetivosOrigem = ArquivoLoader.GetContratosColetivos(arqOrgaosControladores);
                foreach (ContratoColetivoEntrada contrato in contratosColetivosOrigem)
                {
                    //identifica os clientes no arquivo de Contratos Coletivos
                    List<string> clientesContrato = contasContratos.Where(x => x.Value == contrato.CodigoContrato).Select(c => { return c.Key; }).ToList();
                    
                    //pula o contrato caso nao encontre clientes para ele no arquivo referenciado
                    if (clientesContrato.Count == 0)
                    {
                        log.LogFull(string.Format("Nenhum cliente encontrado para o contrato {0} no arquivo {1}", contrato.CodigoContrato, arqContratosColetivos.NomeExtensao));
                        continue;
                    }

                    if(!dicContasBase.ContainsKey(contrato.AccountExternalId))
                    {
                        log.LogFull(string.Format("[ERRO] Account nao encontrado na lista de Contas-base: {0}", contrato.AccountExternalId));
                        continue;
                    }

                    log.Print("-------------------------------");
                    log.LogFull(string.Format("[CONTA-BASE]: Codigo-Contrato: {0} ExternalId: {1}", contrato.CodigoContrato, contrato.AccountExternalId));

                    AccountSalesforce contaBase = dicContasBase[contrato.AccountExternalId];
                    List<sObject> listaUpdate = new List<sObject>();
                    sObject update = new sObject();

                    #region Atualizando Account

                    if (!contasAtualizadas.Contains(contaBase.ExternalId))
                    {
                        update.type = "Account";
                        update.Id = contaBase.Id;
                        update.Any = new System.Xml.XmlElement[] {
                        //SFDCSchemeBuild.GetNewXmlElement("CondominiumType__c", "2"),      //pedido do bruno
                        SFDCSchemeBuild.GetNewXmlElement("CNT_GroupAssociate__c", contaBase.ParentId)
                            //SFDCSchemeBuild.GetNewXmlElement("ParentId", contaBase.ParentId)      //pedido do bruno
                        };

                        //======================================
                        //-------- ATUALIZAR CONTA MAE ---------
                        //======================================
                        listaUpdate.Add(update);
                        saveResults = SalesforceDAO.Atualizar(listaUpdate, 29, ref binding);
                        List<Error[]> erros = saveResults.Where(sr => sr.errors != null).Select(s => s.errors).ToList();

                        if (erros == null || erros.Count == 0)
                            contasAtualizadas.Add(contaBase.ExternalId);

                        foreach (Error[] err in erros)
                        {
                            //msgLog.Append(string.Format("[ERRO ITEM ATTRIBUTES] {0}", string.Join(", ", err.Select(e => e.message))));
                            //lstLog.Add(msgLog.ToString());
                            log.LogFull(string.Format("[ERRO ACCOUNT] {0}", string.Join(", ", err.Select(e => e.message))));
                        }
                    }
                    #endregion


                    #region Criando Contract

                    List<ContractSalesforce> _contratos = SalesforceDAO.GetContratoAgrupamentoPorExternalId(contaBase.ExternalId, contrato.CodigoContrato, ref binding);
                    ContractSalesforce novoContrato = _contratos.FirstOrDefault();
                    if (novoContrato != null && !string.IsNullOrWhiteSpace(novoContrato.Id))
                    {
                        listaUpdate.Clear();
                        log.LogFull(string.Format("Contrato Coletivo existente: {0}", novoContrato.Id));
                        for(int i=1; i <= _contratos.Count-1; i++)
                        {
                            update = new sObject();
                            update.type = "Contract";
                            update.Id  = _contratos[i].Id;
                            listaUpdate.Add(update);

                            log.LogFull(string.Format("[DELETE] Contrato Coletivo: {0}", _contratos[i].Id));
                        }
                        SalesforceDAO.Apagar(listaUpdate, ref binding);
                    }
                    else
                    {
                        novoContrato = new ContractSalesforce();
                        novoContrato.Account = contaBase.Id;
                        novoContrato.DataInicio = DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss.000+0000");
                        novoContrato.TipoAgrupamento = "CNT002";
                        novoContrato.AreaAgrupamento = "4";
                        novoContrato.SegmentoAgrupamento = "2";
                        novoContrato.NumeroAgrupamento = contrato.CodigoContrato;
                        novoContrato.Status = "Draft";
                        novoContrato.ExternalId = contaBase.ExternalId;

                        //======================================
                        //--------- INGRESSAR CONTRATO ---------
                        //--------- E ATUALIZAR STATUS ---------
                        //======================================
                        novoContrato.Id = IngressarContratoControladora(novoContrato);

                        //carregar o campo Contract Number (automatico) gerado ao criar contrato
                        ContractSalesforce aux = SalesforceDAO.GetContratoPorId(novoContrato.Id, ref binding);
                        novoContrato.ContractNumber = aux.ContractNumber;

                        log.LogFull(string.Format("Novo Contrato Coletivo: {0}", novoContrato.Id));

                        listaUpdate.Clear();
                        update.type = "Contract";
                        update.Id = novoContrato.Id;
                        update.Any = new System.Xml.XmlElement[] {
                            SFDCSchemeBuild.GetNewXmlElement("Status", "Activated")
                        };

                        //======================================
                        //--------- ATUALIZAR CONTRACT ---------
                        //======================================
                        listaUpdate.Add(update);
                        saveResults = SalesforceDAO.Atualizar(listaUpdate, 29, ref binding);
                    }

                    #endregion


                    List<BillingSalesforce> _billlings = SalesforceDAO.GetBillingsPorNumeroContrato(string.Empty, string.Empty, novoContrato.ContractNumber, ref binding);
                    BillingSalesforce novoBilling = _billlings.FirstOrDefault();
                    if (novoBilling != null && !string.IsNullOrWhiteSpace(novoBilling.Id))
                    {
                        log.LogFull(string.Format("Billing existente: {0}", novoBilling.Id));
                        listaUpdate.Clear();
                        for (int i = 1; i <= _billlings.Count - 1; i++)
                        {
                            update = new sObject();
                            update.type = "Billing_Profile__c";
                            update.Id = _billlings[i].Id;
                            listaUpdate.Add(update);

                            log.LogFull(string.Format("[DELETE] Billing Profile: {0}", _billlings[i].Id));
                        }
                        SalesforceDAO.Apagar(listaUpdate, ref binding);

                        listaUpdate.Clear();
                        update = new sObject();
                        update.type = "Billing_Profile__c";
                        update.Id = novoBilling.Id;

                        List<XmlElement> _auxXml = new List<XmlElement>();
                        _auxXml.Add(SFDCSchemeBuild.GetNewXmlElement("AccountContract__c", contrato.CodigoContrato));
                        _auxXml.Add(SFDCSchemeBuild.GetNewXmlElement("CNT_Contract__c", novoContrato.ContractNumber));

                        //atualiza o External Id se estiver diferente
                        if (!novoBilling.ExternalID__c.Equals(string.Concat(contaBase.ExternalId, novoContrato.ContractNumber)))
                        {
                            _auxXml.Add(SFDCSchemeBuild.GetNewXmlElement("ExternalID__c", string.Concat(contaBase.ExternalId, novoContrato.ContractNumber)));
                        }

                        update.Any = _auxXml.ToArray();
                        listaUpdate.Add(update);
                        saveResults = SalesforceDAO.Atualizar(listaUpdate, 29, ref binding);
                        List<Error[]> erros = saveResults.Where(sr => sr.errors != null).Select(s => s.errors).ToList();

                        if (erros == null || erros.Count == 0)
                        {
                            log.LogFull(string.Format("Billing atualizado para o agrupamento {0}"
                                , contrato.CodigoContrato));
                        }
                        foreach (Error[] err in erros)
                        {
                            log.LogFull(string.Format("[ERRO BILLING UPD] {0}", string.Join(", ", err.Select(e => e.message))));
                        }
                    }
                    else
                    {
                        #region Criando Billing Profile
                        novoBilling = new BillingSalesforce();
                        novoBilling.CNT_GroupPayType__c = contrato.TipoImpressao;
                        novoBilling.CNT_Lot__c = contrato.Lote;
                        //novoBilling.CNT_GroupClass__c = "";   //TODO:
                        novoBilling.CNT_Due_Date__c = contrato.DataVencimento.Replace("CP", string.Empty).Replace("cp", string.Empty);
                        novoBilling.Type__c = "Statement";
                        //novoBilling.Address__c = contaColetivaPai.Address__c; //TODO:
                        //novoBilling.BillingAddress__c = contaColetivaPai.PaymentAddress__c;   //TODO:
                        novoBilling.Account__c = contaBase.Id;
                        novoBilling.AccountContract__c = contrato.CodigoContrato;   //codigo do contrato coletivo conforme orientação do Bruno Werneck
                        novoBilling.RecordTypeId = "01236000000On9B";
                        novoBilling.CNT_Contract__c = novoContrato.ContractNumber;
                        novoBilling.Company__c = this.codigoEmpresa;    //"2003".Equals(this.codigoEmpresa) ? "COELCE" : "2005".Equals(this.codigoEmpresa) ? "AMPLA" : "2018".Equals(this.codigoEmpresa) ? "CELG" : string.Empty;
                        novoBilling.ExternalID__c = string.Concat(contaBase.ExternalId, novoContrato.ContractNumber);

                        //======================================
                        //------ INGRESSAR BILLING PROFILE -----
                        //======================================
                        try
                        {
                            novoBilling.Id = IngressarBillingProfileControladora(novoBilling);
                            log.LogFull(string.Format("Novo Billing: {0}", novoBilling.Id));
                        }
                        catch (Exception ex)
                        {
                            log.LogFull(string.Format("[ERRO Billing] {0} {1}", ex.Message, ex.StackTrace));
                            continue;
                        }

                        #endregion

                        #region Criar ContractLine

                        //ingressa um novo ContractLine somente se o Billing for novo, com as propriedades preenchidas.  
                        //Ignora se for um já existente, que a consulta 'GetBillingsPorNumeroCliente' não preenche o campo ExternalID__c
                        if (!string.IsNullOrWhiteSpace(novoBilling.ExternalID__c))
                        {
                            ContractLineItemSalesforce novoCline = new ContractLineItemSalesforce();
                            novoCline.ContractId = novoContrato.Id;
                            novoCline.CNT_Status__c = "Active";
                            novoCline.BillingProfile__c = novoBilling.Id;

                            //======================================
                            //------ INGRESSAR CONTRACT LINE -------
                            //======================================
                            try
                            {
                                novoCline.Id = IngressarContractLineControladora(ref novoCline);
                                log.LogFull(string.Format("Novo CLine: {0}", novoCline.Id));
                            }
                            catch (Exception ex)
                            {
                                log.LogFull(string.Format("[ERRO CLine] {0} {1}", ex.Message, ex.StackTrace));
                                continue;
                            }
                        }

                    }
                    #endregion

                    #region Associando PODs

                    if (!atualizarPoDs)
                        continue;

                    log.LogFull(string.Format("Associando {0} PODs..", clientesContrato.Count));
                    foreach (string cliente in clientesContrato)
                    {
                        try
                        {
                            ContractLineItemSalesforce conta = SalesforceDAO.GetContractLineByNumeroCliente("'2003','COELCE'", cliente, ref this.binding);

                            if (conta == null || string.IsNullOrWhiteSpace(conta.Id))
                            {
                                log.LogFull(string.Format("[ERRO CONTRACT_LINE] Cliente não encontrado {0}", cliente));
                                continue;
                            }

                            listaUpdate.Clear();
                            update.type = "Contract_Line_Item__c";
                            update.Id = conta.Id;
                            update.Any = new System.Xml.XmlElement[] {
                                SFDCSchemeBuild.GetNewXmlElement("Company__c", this.codigoEmpresa),
                                SFDCSchemeBuild.GetNewXmlElement("GroupAccountContract__c", novoBilling.Id)
                            };

                            //======================================
                            //------ ATUALIZAR CONTRACT LINE -------
                            //======================================
                            listaUpdate.Add(update);
                            saveResults = SalesforceDAO.Atualizar(listaUpdate, 29, ref binding);
                            List<Error[]> erros = saveResults.Where(sr => sr.errors != null).Select(s => s.errors).ToList();

                            if (erros == null || erros.Count == 0)
                            {
                                contasAtualizadas.Add(contaBase.ExternalId);
                                log.LogFull(string.Format("Cliente {0}\tContract Line atual {1} associado ao novo Billing Profile {2}"
                                    , cliente
                                    , conta.Id
                                    , novoBilling.Id));
                            }
                            foreach (Error[] err in erros)
                            {
                                log.LogFull(string.Format("[ERRO CONTRAC_LINE] {0}", string.Join(", ", err.Select(e => e.message))));
                            }
                        }
                        catch (Exception ex)
                        {
                            log.LogFull(string.Format("[ERRO CATASTRÓFICO] {0}{1}", ex.Message, ex.StackTrace));
                        }
                    }

                    #endregion  Associando PODs
                }
            }
            catch (Exception ex)
            {
                log.LogFull(string.Format("[ERRO CONTRATO COLETIVO] {0} {1}"
                    , ex.Message
                    , ex.StackTrace));
            }
            #endregion

            log.LogFull(string.Format("{0} Processo finalizado.", DateTime.Now.ToString("dd-MM-yyyy HH:mm:ss")));
            Console.ReadKey();
            #region Atualizar Contratos-filhos
            //try
            //{
            //    //consultar contractline item
            //    foreach (string cliente in contasContratos.Keys)
            //    {
            //        ContractLineItemSalesforce conta = SalesforceDAO.GetContractLineItemsByExternalId("'2003','COELCE'", cliente, ref this.binding);
            //        conta.GroupAccountContract__c = dicContasBase[conta.AccountExternalId].Id;

            //        //recupera o Billing Profile gerado a partir da conta consultada (conta-base), original/existente


            //        //====================================================
            //        //-------- ATUALIZAR CONTRACT LINE EXISTENTE ---------
            //        //====================================================
            //        //listaUpdate.Add(update);
            //        //SaveResult[] saveResults = SalesforceDAO.Atualizar(listaUpdate, 29, ref binding);
            //        //List<Error[]> erros = saveResults.Where(sr => sr.errors != null).Select(s => s.errors).ToList();

            //        //if (erros == null || erros.Count == 0)
            //        //    contasAtualizadas.Add(contaBase.ExternalId);

            //        //foreach (Error[] err in erros)
            //        //{
            //        //    //msgLog.Append(string.Format("[ERRO ITEM ATTRIBUTES] {0}", string.Join(", ", err.Select(e => e.message))));
            //        //    //lstLog.Add(msgLog.ToString());
            //        //    lstLog.Add(string.Format("[ERRO ACCOUNT] {0}", string.Join(", ", err.Select(e => e.message))));
            //        //    Console.WriteLine(lstLog.Last().Trim());
            //        //}
            //    }
            //}
            //catch (Exception ex)
            //{

            //}
            #endregion
        }


        /// <summary>
        /// Método de autenticação interna da classe quando for preciso acessar dados no Salesforce.
        /// </summary>
        private void autenticar()
        {
            if (!loggedIn)
            {
                Autenticacao auth = new Autenticacao(this.ambiente);
                if (!auth.ValidarLogin(ref this.binding))
                    throw new AuthenticationException(string.Format("Não foi possível autenticar o login em {0}", this.ambiente));

                this.loggedIn = true;
            }
        }
        



        [ComVisible(true)]
        internal List<sObject> AtualizarItemAttributes(List<ItemAttribute> listaArq, string orderId, Arquivo arquivo, bool somenteUpdate)
        {
            List<sObject> listaUpdate = new List<sObject>();

            #region Consulta-base para rodar este processo
            /*
            select  sg.numero_cliente as numerocliente,
                nvl(trim(ggc_resumen.pot_conectada),0)    as im_charge,    --im_di_contrge,      --cargakwbr
                case when tarsap.valor_alf like '%verde%' then 'horosazonal verde' else
                case when tarsap.valor_alf like '%azul%' then 'horosazonal azul' else
                case when tarsap.valor_alf like '%horo azul%' then 'horo azul' else 'optante' end end end       as modtarifbr,
                trim(tarsap.valor_alf)                    as im_tariftyp_TARIFA,        -- categoria_tarie_br,
                trim(tarsyn.descripcion)                  as ac_kofiz_sd,        --classe_br,
                trim(tarsap.descripcion)                  as im_temp_area,       --subclasse_br,
                cliente.potencia_cont_hp            as im_di_contrat  ,    --demanda_kvabr,
                cliente.potencia_cont_hp            as im_di_contrpt,      --demanda_ponta_br,
                cliente.potencia_cont_fp            as im_di_contrfp,      --demanda_fp_br,
                '0'                                 as capacidade_disjuntorbr,
                nvl(trim(desc_tabla('VOLTA', codigo_voltaje, '0000')), '220 V') as valortensaobr,
                'não'                               as instalacao_padrao,
                'trifásica'                         as tipo_tensaobr,
                okl.transformador                   as potencia_kwbr,
                cliente.tipo_tensao,
                ExternalId_POD,
                ExternalId_Asset
            from    clientes@clientes:sales_geral sg
                join gc_tecni okl
                 on okl.numero_cliente = sg.numero_cliente
                join cliente
                 on sg.numero_cliente = cliente.numero_cliente
                join tecni
                on tecni.numero_cliente = cliente.numero_cliente
                join gc_cliente
                 on cliente.numero_cliente = gc_cliente.numero_cliente
                join ggc_resumen
                 on ggc_resumen.numero_cliente = cliente.numero_cliente
                left join tabla tarsap
                 on cliente.giro = left(tarsap.codigo,2)
                   and gc_cliente.sub_clase = substr(tarsap.codigo,3,2)
                   and cliente.tarifa = right(tarsap.codigo,2)
                   and tarsap.nomtabla='TARSAP'
                left join tabla tarsyn
                 on tarsyn.codigo =tarsap.codigo
                   and tarsyn.nomtabla='TARSYN'
                   and tarsyn.sucursal='0000'
            where   cliente.numero_cliente in ()
            group by 1,2,3,4,5,6,7,8,9,10,11,12,13,14,15,16,17
            */
            #endregion

            autenticar();

            int totalAtualizado = 0;
            int contCliente = 0;
            sObject update = null;
            //List<sObject> listaUpdatePod = new List<sObject>();

            try
            {
                StringBuilder msgLog = new StringBuilder();

                bool? _modalidadeVerde = null; //NULL = OPTANTE
                string idPod = null;
                string tarifaPod = null;

                using (Arquivo arqSaida = new Arquivo(arquivo.Caminho, string.Concat(arquivo.Nome, "_SAIDA"), arquivo.Extensao))
                {
                    foreach (ItemAttribute rs in listaArq)
                    {
                        //Console.WriteLine("ATUALIZAR ITEM ATTRIBUTE ...");
                        msgLog.Clear();
                        contCliente++;
                        tarifaPod = string.Empty;

                        _modalidadeVerde = null;

                        //Console.WriteLine(string.Format("{0}\tconsultar PoD Id", DateTime.Now.ToLongTimeString()));

                        List<ClienteSalesforce> clientes = SalesforceDAO.GetPodsPorExternalId(rs.ExternalIdPod, ref binding);
                        AssetDTO asset = null;
                        idPod = clientes == null || clientes.Count == 0 ? string.Empty : clientes.First().IdPod;

                        foreach (string entidade in new List<string> { "NE__AssetItemAttribute__c", "NE__Order_Item_Attribute__c" })
                        {
                            //Console.WriteLine(string.Format("{0}\tconsultar items attribute para {1}", DateTime.Now.ToLongTimeString(), entidade));

                            Dictionary<string, string> itemsBase = (entidade.Equals("NE__AssetItemAttribute__c")) ?
                                SalesforceDAO.GetAssetItemsAttributePorAsset(rs.ExternalIdAsset, ref binding) :
                                SalesforceDAO.GetOrderItemsAttributePorItem(orderId, ref binding);

                            if (itemsBase == null || itemsBase.Count == 0)
                            {
                                List<AssetDTO> assets = SalesforceDAO.GetAssetsPorExternalId(rs.ExternalIdAsset, ref binding);
                                if (assets == null || assets.Count == 0)
                                {
                                    lstLog.Add(string.Format("[ERRO]\tNão encontrou atributos a atualizar para o obj {0} e Asset ExternalId {1}", entidade, rs.ExternalIdAsset));
                                    Console.WriteLine(lstLog.Last().Trim());
                                    continue;
                                }
                                asset = assets.First();
                                orderId = asset.OrderId;
                                if (string.IsNullOrWhiteSpace(orderId))
                                {
                                    lstLog.Add(string.Format("[ERRO]\tNão encontrou em {0} o Order Id para o Asset ExternalId {1}", entidade, rs.ExternalIdAsset));
                                    Console.WriteLine(lstLog.Last().Trim());
                                    continue;
                                }

                                itemsBase = (entidade.Equals("NE__AssetItemAttribute__c")) ?
                                SalesforceDAO.GetAssetItemsAttributePorAsset(rs.ExternalIdAsset, ref binding) :
                                SalesforceDAO.GetOrderItemsAttributePorItem(orderId, ref binding);
                            }

                            if (itemsBase.Count() > rs.GetType().GetProperties().Count())
                            {
                                throw new InvalidDataException(string.Concat("{0} possui mais atributos que o permitido."));
                            }

                            //tipo de objeto do SalesForce
                            foreach (PropertyInfo prop in rs.GetType().GetProperties())
                            {
                                #region AssetItemAttribute
                                foreach (DisplayAttribute idAttr in ((DisplayAttribute[])(prop.GetCustomAttributes(typeof(DisplayAttribute), true)))) //[0])).Name;
                                {
                                    update = new sObject();
                                    update.type = entidade;

                                    if (string.IsNullOrWhiteSpace(rs.ExternalIdAsset))
                                        Debugger.Break();

                                    try
                                    {
                                        if (itemsBase.Count == 0)
                                            throw new Exception();

                                        if (entidade.Equals("NE__AssetItemAttribute__c"))
                                            update.Id = itemsBase[idAttr.Name];

                                        if (entidade.Equals("NE__Order_Item_Attribute__c"))
                                            update.Id = itemsBase[idAttr.Name];
                                    }
                                    catch(Exception ex)
                                    {
                                        lstLog.Add(string.Format("[ERRO]\tNão encontrou atributo {0} em {1} para {2}", idAttr.Name, entidade, orderId));
                                        Console.WriteLine(lstLog.Last().Trim());

                                        if(!somenteUpdate)
                                            throw new InvalidDataException();

                                        continue;
                                    }

                                    if (string.IsNullOrWhiteSpace(update.Id))
                                    {
                                        lstLog.Add(string.Format("[ERRO]\tId Atributo vazio para {0} em {1}", idAttr.Name, entidade));
                                        Console.WriteLine(lstLog.Last().Trim());
                                        continue;
                                    }

                                    var valor = prop.GetValue(rs, null).ToString();

                                    if ("Categoria de Tarifa BR".Equals(idAttr.Name))
                                    {
                                        if (!string.IsNullOrWhiteSpace(idPod))
                                            tarifaPod = valor;
                                    }

                                    if ("Modalidade Tarifaria BR".Equals(idAttr.Name) && valor.ToUpper().Contains("VERDE"))
                                        _modalidadeVerde = true;

                                    else if ("Modalidade Tarifaria BR".Equals(idAttr.Name) && valor.ToUpper().Contains("AZUL"))
                                        _modalidadeVerde = false;


                                    valor = (("DemandaPonta".Equals(prop.Name) || "DemandaForaPonta".Equals(prop.Name) || "DemandaKV".Equals(prop.Name)) && !_modalidadeVerde.HasValue) ? string.Empty : valor;
                                    valor = (("DemandaPonta".Equals(prop.Name) || "DemandaForaPonta".Equals(prop.Name)) && _modalidadeVerde.HasValue && _modalidadeVerde.Value) ? string.Empty : valor;
                                    valor = ("DemandaKV".Equals(prop.Name) && _modalidadeVerde.HasValue && !_modalidadeVerde.Value) ? string.Empty : valor;

                                    //TODO: levar este trecho para a propria classe ItemAttribute
                                    if (string.IsNullOrWhiteSpace(valor))
                                    {
                                        DefaultValueAttribute[] defaulvalue = ((DefaultValueAttribute[])prop.GetCustomAttributes(typeof(DefaultValueAttribute), true));
                                        if (defaulvalue != null && defaulvalue.Count() > 0)
                                        {
                                            valor = defaulvalue[0].Value.ToString();
                                        }
                                    }

                                    var valorConvertido = string.IsNullOrWhiteSpace(valor) ? " " : Convert.ChangeType(valor, prop.PropertyType);

                                    update.Any = new System.Xml.XmlElement[] {
                                        SFDCSchemeBuild.GetNewXmlElement("NE__Value__c", CultureInfo.CurrentCulture.TextInfo.ToTitleCase(valorConvertido.ToString())),
                                        SFDCSchemeBuild.GetNewXmlElement("NE__Old_Value__c", CultureInfo.CurrentCulture.TextInfo.ToTitleCase(valorConvertido.ToString()))
                                    };

                                    listaUpdate.Add(update);
                                    totalAtualizado++;
                                }
                                #endregion
                            }
                            //Console.WriteLine(string.Format("[ITEM_ATTRIBUTE]\tAtualizou a entidade {0}", entidade));
                        }  // fim foreach de Entidades

                        #region PointOfDelivery
                        if (string.IsNullOrWhiteSpace(idPod))
                        {
                            msgLog.Clear();
                            msgLog.Append(string.Format("[ERRO] Cliente {0} nao localizado na SALES_GERAL.", rs.NumeroCliente));
                            IO.EscreverArquivo(arqSaida, msgLog.ToString());
                            lstLog.Add(msgLog.ToString());
                            Console.WriteLine(lstLog.Last().Trim());

                            if (somenteUpdate)
                                IO.EscreverArquivo(arqSaida, msgLog.ToString());
                        }
                        else
                        {
                            update.type = "PointOfDelivery__c";
                            update.Id = idPod;
                            update.Any = new System.Xml.XmlElement[] {
                                SFDCSchemeBuild.GetNewXmlElement("Rate__c", tarifaPod)
                            };

                            listaUpdate.Add(update);
                        }
                        #endregion

                        //msgLog.AppendLine(string.Format("Asset {0} atualizado.", rs.ExternalIdAsset));
                        //lstLog.Add(msgLog.ToString().Trim());
                        //Console.WriteLine(lstLog.Last());

                        if (somenteUpdate)
                            IO.EscreverArquivo(arqSaida, msgLog.ToString());

                    }// fim for each Cliente/Itens de Asset


                    if (somenteUpdate)
                    {
                        SaveResult[] saveResults = SalesforceDAO.Atualizar(listaUpdate, 29, ref binding);
                        List<Error[]> erros = saveResults.Where(sr => sr.errors != null).Select(s => s.errors).ToList();
                        foreach (Error[] err in erros)
                        {
                            msgLog.Append(string.Format("[ERRO ITEM ATTRIBUTES] {0}", string.Join(", ", err.Select(e => e.message))));
                            lstLog.Add(msgLog.ToString());
                            Console.WriteLine(lstLog.Last().Trim());
                        }

                        if (msgLog.Length > 0)
                        {
                            IO.EscreverArquivo(arqSaida, msgLog.ToString());
                        }
                        listaUpdate.Clear();
                    }

                }  //fim arquivoSaida
            }
            catch(InvalidDataException ex)
            {
                 throw ex;
            }
            catch (Exception ex)
            {
                lstLog.Add(string.Concat("\nErro ao atualizar o registro: \n",ex.Message, ex.StackTrace));
                Console.WriteLine(lstLog.Last());
            }

            return listaUpdate;
        }



        [ComVisible(true)]
        internal List<sObject> AtualizarItemAttributes(Dictionary<AssetDTO, List<ItemAttribute>> lista)
        {
            autenticar();

            List<sObject> listaUpdate = new List<sObject>();
            int totalAtualizado = 0;
            int contCliente = 0;
            sObject update = null;

            try
            {
                StringBuilder msgLog = new StringBuilder();

                bool? _modalidadeVerde = null; //NULL = OPTANTE
                string idPod = null;
                string tarifaPod = null;

                foreach (AssetDTO asset in lista.Keys)
                {
                    //Console.WriteLine(contCliente + "\tAsset " + asset.Id + " Cliente " + asset.NumeroCliente + " Contrato " + asset.ContractNumber);
                    foreach (ItemAttribute rs in lista[asset])
                    {
                        //Console.WriteLine("ATUALIZAR ITEM ATTRIBUTE ...");
                        msgLog.Clear();
                        contCliente++;
                        tarifaPod = string.Empty;

                        _modalidadeVerde = null;

                        //Console.WriteLine(string.Format("{0}\tconsultar PoD Id", DateTime.Now.ToLongTimeString()));

                        foreach (string entidade in new List<string> { "NE__AssetItemAttribute__c", "NE__Order_Item_Attribute__c" })
                        {
                            Dictionary<string, string> itemsBase = (entidade.Equals("NE__AssetItemAttribute__c")) ?
                                SalesforceDAO.GetAssetItemsAttributePorAsset(rs.ExternalIdAsset, ref binding) :
                                SalesforceDAO.GetOrderItemsAttributePorItem(asset.OrderId, ref binding);

                            if (itemsBase == null || itemsBase.Count == 0)
                            {
                                if (string.IsNullOrWhiteSpace(asset.OrderId))
                                {
                                    lstLog.Add(string.Format("[ERRO]\tNão encontrou em {0} o Order Id para o Asset ExternalId {1}", entidade, rs.ExternalIdAsset));
                                    Console.WriteLine(lstLog.Last().Trim());
                                    continue;
                                }

                                itemsBase = (entidade.Equals("NE__AssetItemAttribute__c")) ?
                                SalesforceDAO.GetAssetItemsAttributePorAsset(rs.ExternalIdAsset, ref binding) :
                                SalesforceDAO.GetOrderItemsAttributePorItem(asset.OrderId, ref binding);
                            }
                            //tipo de objeto do SalesForce
                            foreach (PropertyInfo prop in rs.GetType().GetProperties())
                            {
                                #region AssetItemAttribute
                                foreach (DisplayAttribute idAttr in ((DisplayAttribute[])(prop.GetCustomAttributes(typeof(DisplayAttribute), true)))) //[0])).Name;
                                {
                                    update = new sObject();
                                    update.type = entidade;

                                    if (string.IsNullOrWhiteSpace(rs.ExternalIdAsset))
                                        Debugger.Break();

                                    try
                                    {
                                        if (itemsBase.Count == 0)
                                            throw new Exception();

                                        if (entidade.Equals("NE__AssetItemAttribute__c"))
                                            update.Id = itemsBase[idAttr.Name];

                                        if (entidade.Equals("NE__Order_Item_Attribute__c"))
                                            update.Id = itemsBase[idAttr.Name];
                                    }
                                    catch (Exception ex)
                                    {
                                        lstLog.Add(string.Format("[ERRO]\tNão encontrou atributo {0} em {1} para {2}", idAttr.Name, entidade, asset.OrderId));
                                        Console.WriteLine(lstLog.Last().Trim());

                                        continue;
                                    }

                                    if (string.IsNullOrWhiteSpace(update.Id))
                                    {
                                        lstLog.Add(string.Format("[ERRO]\tId Atributo vazio para {0} em {1}", idAttr.Name, entidade));
                                        Console.WriteLine(lstLog.Last().Trim());
                                        continue;
                                    }

                                    var valor = prop.GetValue(rs, null).ToString();

                                    if ("Categoria de Tarifa BR".Equals(idAttr.Name))
                                    {
                                        if (!string.IsNullOrWhiteSpace(idPod))
                                            tarifaPod = valor;
                                    }

                                    if ("Modalidade Tarifaria BR".Equals(idAttr.Name) && valor.ToUpper().Contains("VERDE"))
                                        _modalidadeVerde = true;

                                    else if ("Modalidade Tarifaria BR".Equals(idAttr.Name) && valor.ToUpper().Contains("AZUL"))
                                        _modalidadeVerde = false;


                                    valor = (("DemandaPonta".Equals(prop.Name) || "DemandaForaPonta".Equals(prop.Name) || "DemandaKV".Equals(prop.Name)) && !_modalidadeVerde.HasValue) ? string.Empty : valor;
                                    valor = (("DemandaPonta".Equals(prop.Name) || "DemandaForaPonta".Equals(prop.Name)) && _modalidadeVerde.HasValue && _modalidadeVerde.Value) ? string.Empty : valor;
                                    valor = ("DemandaKV".Equals(prop.Name) && _modalidadeVerde.HasValue && !_modalidadeVerde.Value) ? string.Empty : valor;

                                    //TODO: levar este trecho para a propria classe ItemAttribute
                                    if (string.IsNullOrWhiteSpace(valor))
                                    {
                                        DefaultValueAttribute[] defaulvalue = ((DefaultValueAttribute[])prop.GetCustomAttributes(typeof(DefaultValueAttribute), true));
                                        if (defaulvalue != null && defaulvalue.Count() > 0)
                                        {
                                            valor = defaulvalue[0].Value.ToString();
                                        }
                                    }

                                    var valorConvertido = string.IsNullOrWhiteSpace(valor) ? " " : Convert.ChangeType(valor, prop.PropertyType);

                                    update.Any = new System.Xml.XmlElement[] {
                                    SFDCSchemeBuild.GetNewXmlElement("NE__Value__c", CultureInfo.CurrentCulture.TextInfo.ToTitleCase(valorConvertido.ToString())),
                                    SFDCSchemeBuild.GetNewXmlElement("NE__Old_Value__c", CultureInfo.CurrentCulture.TextInfo.ToTitleCase(valorConvertido.ToString()))
                                };

                                    listaUpdate.Add(update);
                                    totalAtualizado++;
                                }
                                #endregion
                            }
                            //Console.WriteLine(string.Format("[ITEM_ATTRIBUTE]\tAtualizou a entidade {0}", entidade));
                        }  // fim foreach de Entidades

                        #region PointOfDelivery
                        if (string.IsNullOrWhiteSpace(idPod))
                        {
                            msgLog.Clear();
                            msgLog.Append(string.Format("[ERRO] Cliente {0} nao localizado na SALES_GERAL.", rs.NumeroCliente));
                            //IO.EscreverArquivo(arqSaida, msgLog.ToString());
                            lstLog.Add(msgLog.ToString());
                            Console.WriteLine(lstLog.Last().Trim());
                        }
                        else
                        {
                            update.type = "PointOfDelivery__c";
                            update.Id = idPod;
                            update.Any = new System.Xml.XmlElement[] {
                            SFDCSchemeBuild.GetNewXmlElement("Rate__c", tarifaPod)
                        };

                            listaUpdate.Add(update);
                        }
                        #endregion

                    }// fim for each Cliente/Itens de Asset
                }

                if (listaUpdate.Count > 0)
                {
                    SaveResult[] saveResults = SalesforceDAO.Atualizar(listaUpdate, 29, ref binding);
                    List<Error[]> erros = saveResults.Where(sr => sr.errors != null).Select(s => s.errors).ToList();
                    foreach (Error[] err in erros)
                    {
                        msgLog.Append(string.Format("[ERRO ITEM ATTRIBUTES] {0}", string.Join(", ", err.Select(e => e.message))));
                        lstLog.Add(msgLog.ToString());
                        Console.WriteLine(lstLog.Last().Trim());
                    }

                    if (msgLog.Length > 0)
                    {
                        //IO.EscreverArquivo(arqSaida, msgLog.ToString());
                    }
                    listaUpdate.Clear();
                }
            }
            catch (InvalidDataException ex)
            {
                throw ex;
            }
            catch (Exception ex)
            {
                lstLog.Add(string.Concat("\nErro ao atualizar o registro: \n", ex.Message, ex.StackTrace));
                Console.WriteLine(lstLog.Last());
            }

            return listaUpdate;
        }




        
        /// <summary>
        /// TODO:
        /// </summary>
        /// <param name="arquivo"></param>
        [ComVisible(true)]
        internal void AtualizarInscricaoMunicipal(Arquivo arquivo)
        {
            if (!loggedIn)
            {
                Autenticacao auth = new Autenticacao(this.ambiente);
                if (!auth.ValidarLogin(ref this.binding))
                    return;

                this.loggedIn = true;
            }

            int i = 0;
            int totalAtualizado = 0;
            int contCliente = 0;
            sObject update = null;
            List<sObject> listaUpdate = new List<sObject>();

            try
            {
                List<AccountSalesforce> listaArq = ArquivoLoader.GetInscricaoMunicipal(arquivo);
                StringBuilder msgLog = new StringBuilder();
                int sucesso = 0;
                int falha = 0;

                using (Arquivo arqSaida = new Arquivo(arquivo.Caminho, string.Concat(arquivo.Nome, "_SAIDA"), arquivo.Extensao))
                {
                    foreach (AccountSalesforce rs in listaArq)
                    {
                        sucesso = 0;
                        falha = 0;
                        msgLog.Clear();
                        contCliente++;

                        //validação de documentos
                        if (string.IsNullOrWhiteSpace(rs.NumeroDocumento) || "0".Equals(rs.NumeroDocumento))
                        {
                            msgLog.AppendLine(string.Format("Sem Documento: {0}", rs.NumeroCliente));
                            Console.WriteLine(msgLog.ToString());
                            IO.EscreverArquivo(arqSaida, msgLog.ToString());
                            continue;
                        }
                        if (string.IsNullOrWhiteSpace(rs.InscricaoMunicipal) || "0".Equals(rs.InscricaoMunicipal))
                        {
                            msgLog.AppendLine(string.Format("Sem Inscricao: {0} - {1}", rs.NumeroCliente, rs.NumeroDocumento));
                            Console.WriteLine(msgLog.ToString());
                            IO.EscreverArquivo(arqSaida, msgLog.ToString());
                            continue;
                        }

                        List<ClienteSalesforce> lstCli = SalesforceDAO.GetContasPorDocumento(this.codigoEmpresa, rs.NumeroDocumento, ref binding);

                        if (lstCli == null || lstCli.Count <= 0)
                        {
                            lstCli = SalesforceDAO.GetContasPorDocumento("2003", rs.NumeroDocumento.PadLeft(14,'0'), ref binding);

                            if (lstCli == null || lstCli.Count <= 0)
                            {
                                lstCli = SalesforceDAO.GetContasPorDocumento("2003", rs.NumeroDocumento.PadLeft(20, '0'), ref binding);

                                if (lstCli == null || lstCli.Count <= 0)
                                {
                                    msgLog.AppendLine(string.Format("ERRO: Doc não localizado [nivel 2]: {0}", rs.NumeroDocumento));
                                    Console.WriteLine(msgLog.ToString());
                                    IO.EscreverArquivo(arqSaida, msgLog.ToString());
                                    continue;
                                }
                            }
                            else
                            {
                                continue;
                            }
                        }
                        else
                        {
                            //TODO: Temporário, para rodar uma 2a vez, somente para atualizar os que antes foram descartados como DOC NAO LOCALIZADO
                            //desfazer este ELSE imediatamente
                            continue;
                        }
                        
                        foreach (ClienteSalesforce _cli in lstCli)
                        {
                            if (_cli.ExternalId.ToUpper().Contains("INVALIDO"))
                                continue;

                            //tipo de objeto do SalesForce
                            update = new sObject();
                            update.type = "Account";

                            //Aqui será o id do objeto recuperado
                            update.Id = _cli.IdConta;
                            update.Any = new System.Xml.XmlElement[] {
                            SFDCSchemeBuild.GetNewXmlElement("CNT_Municipality_Inscription__c", rs.InscricaoMunicipal) 
                            };

                            listaUpdate.Add(update);
                            i++;

                            totalAtualizado++;
                        }

                        msgLog.AppendLine(string.Format("Registro OK: {0} :: {1} - {2}", contCliente, rs.NumeroCliente, rs.NumeroDocumento));
                        Console.WriteLine(msgLog.ToString());
                        IO.EscreverArquivo(arqSaida, msgLog.ToString());

                        //Update a cada 200 registros, por limitação do Sales ce
                        if (listaUpdate.Count > 0 && i == 199)
                        {
                            SaveResult[] saveResults = binding.update(listaUpdate.ToArray());
                            sucesso = saveResults.Where(x => x.success = true).Count();
                            falha = saveResults.Where(x => x.success = false).Count();
                            msgLog.AppendLine(string.Format("Totais:  Sucesso {0},  Erro {1}", sucesso, falha));

                            Console.WriteLine(msgLog.ToString());
                            IO.EscreverArquivo(arqSaida, msgLog.ToString());
                            i = 0;
                            listaUpdate.Clear();
                        }
                    }  // fim foreach de clientes


                    //Update do remanescente da lista
                    if (i < 200)
                    {
                        totalAtualizado += i;
                        SaveResult[] saveResults = binding.update(listaUpdate.ToArray());
                        sucesso = saveResults.Where(x => x.success = true).Count();
                        falha = saveResults.Where(x => x.success = false).Count();
                        msgLog.AppendLine(string.Format("Últimos Totais:  Sucesso {0},  Erro {1}", sucesso, falha));

                        Console.WriteLine(msgLog.ToString());
                        IO.EscreverArquivo(arqSaida, msgLog.ToString());
                        i = 0;
                        listaUpdate.Clear();
                    }

                    msgLog.AppendLine(string.Format("Total clientes: {0},  Total Atualizado {1}", contCliente, totalAtualizado));
                    Console.WriteLine(msgLog.ToString());
                    IO.EscreverArquivo(arqSaida, msgLog.ToString());

                }  //fim arquivoSaida

                Console.ReadKey();
            }
            catch (Exception ex)
            {
                Console.WriteLine(string.Concat("\nErro ao atualizar o registro: \n",ex.Message, ex.StackTrace));
            }
        }



        /// <summary>
        /// TODO:
        /// </summary>
        /// <param name="arquivo"></param>
        [ComVisible(true)]
        public void AtualizarCasos(string numeroCaso)
        {
            autenticar();

            sObject update = null;
            List<sObject> listaUpdate = new List<sObject>();

            try
            {
                StringBuilder msgLog = new StringBuilder();

                msgLog.Clear();

                //tipo de objeto do SalesForce
                update = new sObject();
                update.type = "Case";

                update.Id = "5001o00002N3HLiAAN";

                //ALTERAR PARA INGRESSADO
                update.Any = new System.Xml.XmlElement[] {
                SFDCSchemeBuild.GetNewXmlElement("Status", "ESTA001")
                };

                listaUpdate.Add(update);
                SaveResult[] saveResults = binding.update(listaUpdate.ToArray());
                listaUpdate.Clear();


                //RESTAURAR PARA FECHADO
                update.Any = new System.Xml.XmlElement[] {
                SFDCSchemeBuild.GetNewXmlElement("Status", "ESTA007")
                };

                listaUpdate.Add(update);
                saveResults = binding.update(listaUpdate.ToArray());
                listaUpdate.Clear();

                Console.ReadKey();
            }
            catch (Exception ex)
            {
                Console.WriteLine(string.Concat("\nErro ao atualizar o registro: \n", ex.Message, ex.StackTrace));
            }
        }


        /// <summary>
        /// Processo para ajustar no Salesforce os documentos que estao fora do padrao: CPF = 11 caracteres, CNPJ = 14 caracteres
        /// </summary>
        /// <param name="arquivo"></param>
        [ComVisible(true)]
        internal void HigienizarDocumentos(Arquivo arquivo)
        {
            if (!loggedIn)
            {
                Autenticacao auth = new Autenticacao(this.ambiente);
                if (!auth.ValidarLogin(ref this.binding))
                    return;

                this.loggedIn = true;
            }

            int i = 0;
            int totalAtualizado = 0;
            int contCliente = 0;
            sObject update = null;
            
            //lista de objetos Salesforce a ser enviada ao ws para atualização
            List<sObject> listaUpdate = new List<sObject>();

            //lista de RecordTypes a ser utilizada para atualização massiva dos RecordTypes que serão atualizados para B2G no inicio do processo
            Dictionary<string, string> dicRecordType = new Dictionary<string, string>();
            StringBuilder auxDocAntigo = new StringBuilder();

            try
            {
                List<ClienteSalesforce> listaArq = ArquivoLoader.GetAccounts(arquivo);
                StringBuilder msgLog = new StringBuilder();
                StringBuilder auxRegion = new StringBuilder();

                using (Arquivo arqSaida = new Arquivo(arquivo.Caminho, string.Concat(arquivo.Nome, "_SAIDA"), arquivo.Extensao))
                {
                    Console.WriteLine(string.Format("{0}\tArquivo de entrada carregado",DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss")));
                    IO.EscreverArquivo(arqSaida, string.Format("{0}\tArquivo de entrada carregado",DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss")));

                    foreach (ClienteSalesforce rs in listaArq)
                    {
                        msgLog.Clear();
                        contCliente++;
                        auxRegion.Clear();
                        auxDocAntigo.Clear();

                        //validação de documentos
                        if (string.IsNullOrWhiteSpace(rs.IdConta) ||string.IsNullOrWhiteSpace(rs.Documento))
                        {
                            msgLog.Append(string.Format("{0}\tParâmetros inválidos: Id: {0} ExternalId: {1} Doc: {2}", rs.IdConta, rs.ExternalId, rs.Documento));
                            Console.WriteLine(msgLog.ToString());
                            IO.EscreverArquivo(arqSaida, msgLog.ToString());
                            continue;
                        }

                        //tipo de objeto do SalesForce
                        update = new sObject();
                        update.type = "Account";
                        update.Id = rs.IdConta;

                        if(string.IsNullOrWhiteSpace(rs.TipoRegistroId))
                        {
                            Console.WriteLine(string.Format("{0}\t[ERRO] RecordType Invalido {1} {2}", DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss"), rs.IdConta, rs.TipoRegistroId));
                            IO.EscreverArquivo(arqSaida, string.Format("{0}\t[ERRO] RecordType Invalido {1} {2}", DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss"), rs.IdConta, rs.TipoRegistroId));
                        }

                        dicRecordType.Add(rs.IdConta, rs.TipoRegistroId);
                        auxDocAntigo.Append(rs.Documento);

                        if (rs.Documento.Length > 16)
                        {
                            Console.WriteLine(string.Format("{0}\t[CORRECAO] Doc > 16 {1} {2}", DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss"), rs.IdConta, rs.Documento));
                            IO.EscreverArquivo(arqSaida, string.Format("{0}\t[CORRECAO] Doc > 16 {1} {2}", DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss"), rs.IdConta, rs.Documento));

                            rs.Documento = "2003".Equals(rs.Documento.Substring(0, 4)) ? 
                                rs.Documento.Substring(4, (rs.Documento.Length - (rs.Documento.Length - 16))) : 
                                rs.Documento.Substring((rs.Documento.Length - 16), (rs.Documento.Length - (rs.Documento.Length - 16)));
                        }

                        try
                        {
                            if ("005".Equals(rs.TipoDocumento) || "5".Equals(rs.TipoDocumento))
                            {
                                rs.Documento = rs.Documento.PadLeft(11, '0');
                                rs.Documento = rs.Documento.Length == 11 ? rs.Documento : rs.Documento.Substring(rs.Documento.Length - 11, 11);
                            }

                            if ("002".Equals(rs.TipoDocumento))
                            {
                                rs.Documento = rs.Documento.PadLeft(14, '0');
                                rs.Documento = rs.Documento.Length == 14 ? rs.Documento : rs.Documento.Substring(rs.Documento.Length - 14, 14);
                            }
                        }
                        catch(Exception ex)
                        {
                            Console.WriteLine(string.Format("{0}\t[ERRO] {1} {2}", DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss"), rs.IdConta, ex.Message));
                            IO.EscreverArquivo(arqSaida, string.Format("{0}\t[ERRO] {1} {2}", DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss"), rs.IdConta, ex.Message));
                            continue;
                        }

                        update.Any = new System.Xml.XmlElement[] {
                            SFDCSchemeBuild.GetNewXmlElement("RecordTypeId", "0121o000000oWhZAAU"),
                            SFDCSchemeBuild.GetNewXmlElement("IdentityNumber__c", rs.Documento)
                        };

                        listaUpdate.Add(update);
                        i++;
                        totalAtualizado++;

                        msgLog.Append(string.Format("{0}\tId: {1} Doc Antigo: {2}  Doc Novo: {3}", DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss"), rs.IdConta, auxDocAntigo.ToString(), rs.Documento));
                        Console.WriteLine(msgLog.ToString());
                        IO.EscreverArquivo(arqSaida, msgLog.ToString());

                        if (listaUpdate.Count > 0 && i == 199)
                        {
                            SaveResult[] saveResults = binding.update(listaUpdate.ToArray());
                            if (saveResults[0].errors != null)
                            {
                                string idsUpdate = string.Join(",", listaUpdate.Select(t => t.Id).ToArray());
                                foreach (Error err in saveResults[0].errors)
                                {
                                    Console.WriteLine(string.Format("{0}\t[ERRO] {1} {2}", DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss"), idsUpdate, err.message));
                                    IO.EscreverArquivo(arqSaida, string.Format("{0}\t[ERRO] {1} {2}", DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss"), idsUpdate, err.message));
                                }
                            }

                            i = 0;
                            listaUpdate.Clear();
                        }
                    }  // fim foreach de clientes

                    //atualiza os Docs remanescentes
                    if (listaUpdate.Count <= 199)
                    {
                        SaveResult[] saveRecordType = binding.update(listaUpdate.ToArray());
                        if (saveRecordType[0].errors != null)
                        {
                            string idsUpdate = string.Join(",", listaUpdate.Select(t => t.Id).ToArray());
                            foreach (Error err in saveRecordType[0].errors)
                            {
                                Console.WriteLine(string.Format("{0}\t[ERRO] {1} {2}", DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss"), idsUpdate, err.message));
                                IO.EscreverArquivo(arqSaida, string.Format("{0}\t[ERRO] {1} {2}", DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss"), idsUpdate, err.message));
                            }
                        }
                        listaUpdate.Clear();
                    }


                    //Restaura os RecordTypes alterados no inicio do processo
                    listaUpdate.Clear();
                    int cont = 0;
                    Console.WriteLine(string.Format("{0}\tRestaurando os RecordTypes ...", DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss")));
                    IO.EscreverArquivo(arqSaida, string.Format("{0}\tRestaurando os RecordTypes ...", DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss")));

                    foreach (string recType in dicRecordType.Keys)
                    {
                        #region Restaurar RecordTypeId
                        update = new sObject();
                        update.type = "Account"; 
                        update.Id = recType;
                        update.Any = new System.Xml.XmlElement[] {
                                SFDCSchemeBuild.GetNewXmlElement("RecordTypeId", dicRecordType[recType])
                            };
                        listaUpdate.Add(update);
                        cont++;

                        if (cont == 199)
                        {
                            SaveResult[] saveRecordType = binding.update(listaUpdate.ToArray());
                            Console.Write(".");

                            List<Error[]> arrErros = saveRecordType.Where(e => e.errors != null).Select(t => t.errors).ToList();
                            if (arrErros != null && arrErros.Count > 0)
                            {
                                string idsUpdate = string.Join(",", listaUpdate.Select(t => t.Id).ToArray());
                                foreach (Error[] err1 in arrErros)
                                {
                                    foreach (Error err2 in err1)
                                    {
                                        Console.WriteLine(string.Format("{0}\t[ERRO] {1} {2}", DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss"), idsUpdate, err2.message));
                                        IO.EscreverArquivo(arqSaida, string.Format("{0}\t[ERRO] {1} {2}", DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss"), idsUpdate, err2.message));
                                    }
                                }
                            }

                            cont = 0;
                            listaUpdate.Clear();
                        }
                        #endregion
                    }


                    //atualiza os RecordTypes remanescentes
                    if (cont <= 199)
                    {
                        SaveResult[] saveRecordType = binding.update(listaUpdate.ToArray());

                        List<Error[]> arrErros = saveRecordType.Where(e => e.errors != null).Select(t => t.errors).ToList();
                        if (arrErros != null && arrErros.Count > 0)
                        {
                            string idsUpdate = string.Join(",", listaUpdate.Select(t => t.Id).ToArray());
                            foreach (Error[] err1 in arrErros)
                            {
                                foreach (Error err2 in err1)
                                {
                                    Console.WriteLine(string.Format("{0}\t[ERRO] {1} {2}", DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss"), idsUpdate, err2.message));
                                    IO.EscreverArquivo(arqSaida, string.Format("{0}\t[ERRO] {1} {2}", DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss"), idsUpdate, err2.message));
                                }
                            }
                        }
                    }

                    msgLog.AppendLine("\nProcesso finalizado");
                    Console.WriteLine(msgLog.ToString());
                    IO.EscreverArquivo(arqSaida, msgLog.ToString());
                }  //fim arquivoSaida

                Console.ReadKey();
            }
            catch (Exception ex)
            {
                Console.WriteLine(string.Concat("\nErro ao atualizar o registro: \n", ex.Message, ex.StackTrace));
            }
        }




        /// <summary>
        /// Atualiza o Id dos Casos nas Atividades que não possuem o campo ReleatedCase devidamente preenchido.
        /// </summary>
        /// <remarks>
        /// Processo solicitado por Juan Sobrinho em 19/02/2019 por email.
        /// </remarks>
        /// <param name="arquivo"></param>
        [ComVisible(true)]
        internal void AtualizarAtividadesIdCaso(Arquivo arquivo)
        {
            if (!loggedIn)
            {
                Autenticacao auth = new Autenticacao(this.ambiente);
                if (!auth.ValidarLogin(ref this.binding))
                    return;

                this.loggedIn = true;
            }

            int i = 0;
            int totalAtualizado = 0;
            int contCliente = 0;
            sObject update = null;

            //lista de objetos Salesforce a ser enviada ao ws para atualização
            List<sObject> listaUpdate = new List<sObject>();

            try
            {
                List<Atividade> listaArq = ArquivoLoader.GetAtividades(arquivo);
                ParAtividadeSalesforce paramTask = new ParAtividadeSalesforce();
                paramTask.CasoRelacionado = "null";
                paramTask.DataCriacaoInicio = new DateTime(2019, 1, 1);
                paramTask.DataCriacaoFim = new DateTime(2019, 2, 28);
                //paramTask.Assunto = "REC Aviso Emergencial não atend' or Subject = 'Con Aviso Emergencial' or Subject = 'Sol Aviso Emergencial Urgente";
                paramTask.Pais = "BRASIL";

                listaArq = SalesforceDAO.GetAtividadesPorPeriodo(paramTask, ref binding);
                listaArq = listaArq.Where(a => a.Assunto.ToLower().Contains("aviso emergencial")).ToList();

                StringBuilder msgLog = new StringBuilder();
                StringBuilder auxRegion = new StringBuilder();
                
                using (Arquivo arqSaida = new Arquivo(arquivo.Caminho, string.Concat(arquivo.Nome, "_SAIDA"), arquivo.Extensao))
                {
                    Console.WriteLine(string.Format("{0}\tArquivo de entrada carregado", DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss")));
                    IO.EscreverArquivo(arqSaida, string.Format("{0}\tArquivo de entrada carregado", DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss")));

                    foreach (Atividade rs in listaArq)
                    {
                        msgLog.Clear();
                        contCliente++;
                        auxRegion.Clear();

                        //tipo de objeto do SalesForce
                        update = new sObject();
                        update.type = "Task";
                        update.Id = rs.Id;

                        update.Any = new System.Xml.XmlElement[] {
                            SFDCSchemeBuild.GetNewXmlElement("RelatedCase__c", rs.CasoId)
                        };

                        listaUpdate.Add(update);
                        i++;
                        totalAtualizado++;

                        msgLog.Append(string.Format("{0}\tTask {1}\tCaso {2}\t Criado em {3}", DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss"), rs.Id, rs.CasoId, rs.DataCriacao));
                        Console.WriteLine(msgLog.ToString());
                        IO.EscreverArquivo(arqSaida, msgLog.ToString());

                        if (listaUpdate.Count > 0 && i == 199)
                        {
                            SaveResult[] saveResults = binding.update(listaUpdate.ToArray());
                            if (saveResults != null && saveResults.Count() > 0 && saveResults[0].errors != null)
                            {
                                string idsUpdate = string.Join(",", listaUpdate.Select(t => t.Id).ToArray());
                                foreach (Error err in saveResults[0].errors)
                                {
                                    Console.WriteLine(string.Format("{0}\t[ERRO] {1} {2}", DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss"), idsUpdate, err.message));
                                    IO.EscreverArquivo(arqSaida, string.Format("{0}\t[ERRO] {1} {2}", DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss"), idsUpdate, err.message));
                                }
                            }

                            i = 0;
                            listaUpdate.Clear();
                        }
                    }  // fim foreach de clientes

                    //atualiza os Docs remanescentes
                    if (listaUpdate.Count <= 199)
                    {
                        SaveResult[] saveRecordType = binding.update(listaUpdate.ToArray());
                        if (saveRecordType != null &&saveRecordType.Count() > 0 && saveRecordType[0].errors != null)
                        {
                            string idsUpdate = string.Join(",", listaUpdate.Select(t => t.Id).ToArray());
                            foreach (Error err in saveRecordType[0].errors)
                            {
                                Console.WriteLine(string.Format("{0}\t[ERRO] {1} {2}", DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss"), idsUpdate, err.message));
                                IO.EscreverArquivo(arqSaida, string.Format("{0}\t[ERRO] {1} {2}", DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss"), idsUpdate, err.message));
                            }
                        }
                        listaUpdate.Clear();
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(string.Concat("\nErro ao atualizar o registro: \n", ex.Message, ex.StackTrace));
            }

            Console.WriteLine(string.Format("{0}\tProcesso terminado.", DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss")));
            Console.ReadKey();
        }



        /// <summary>
        /// TODO:
        /// </summary>
        /// <param name="arquivo"></param>
        [ComVisible(true)]
        internal void AtualizarRegions(Arquivo arquivo)
        {
            if (!loggedIn)
            {
                Autenticacao auth = new Autenticacao(this.ambiente);
                if (!auth.ValidarLogin(ref this.binding))
                    return;

                this.loggedIn = true;
            }

            int i = 0;
            int totalAtualizado = 0;
            int contCliente = 0;
            sObject update = null;
            List<sObject> listaUpdate = new List<sObject>();

            try
            {
                List<AddressSalesforce> listaArq = ArquivoLoader.GetAddresses(arquivo);
                StringBuilder msgLog = new StringBuilder();
                StringBuilder auxRegion = new StringBuilder();

                using (Arquivo arqSaida = new Arquivo(arquivo.Caminho, string.Concat(arquivo.Nome, "_SAIDA"), arquivo.Extensao))
                {
                    foreach (AddressSalesforce rs in listaArq)
                    {
                        msgLog.Clear();
                        contCliente++;
                        auxRegion.Clear();

                        //validação de documentos
                        if (string.IsNullOrWhiteSpace(rs.Id) || string.IsNullOrWhiteSpace(rs.Region))
                        {
                            msgLog.Append(string.Format("{0}\tParâmetros inválidos.", contCliente));
                            Console.WriteLine(msgLog.ToString());
                            IO.EscreverArquivo(arqSaida, msgLog.ToString());
                            continue;
                        }

                        auxRegion.Append("Ceará".Equals(rs.Region) ? "CE" :  "Rio de Janeiro".Equals(rs.Region) ? "RJ" : string.Empty);
                        if(string.IsNullOrWhiteSpace(auxRegion.ToString()))
                        {
                            msgLog.Append(string.Format("{0}\tRegion inválido para AddressId: {1} Region: {2}", contCliente, rs.Id, rs.Region));
                            Console.WriteLine(msgLog.ToString());
                            IO.EscreverArquivo(arqSaida, msgLog.ToString());
                            continue;
                        }
                        
                        //tipo de objeto do SalesForce
                        update = new sObject();
                        update.type = "Street";

                        #region NOVA ALTERAÇÂO <<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<

                        //SE region__c IN (23,33,43)
                        // ATUALIZAR Literal_region__c = "RJ" "CE" "GO"

                        #endregion


                        //Aqui será o id do objeto recuperado
                        update.Id = rs.Id;
                        update.Any = new System.Xml.XmlElement[] {
                        SFDCSchemeBuild.GetNewXmlElement("Region__c", auxRegion.ToString())
                        };

                        listaUpdate.Add(update);
                        i++;

                        totalAtualizado++;

                        msgLog.Append(string.Format("{0}\tRegion atualizado para AddressId: {1} Region: {2}", contCliente, rs.Id, auxRegion.ToString()));
                        Console.WriteLine(msgLog.ToString());
                        IO.EscreverArquivo(arqSaida, msgLog.ToString());

                        //Update a cada 200 registros, por limitação do Sales ce
                        if (listaUpdate.Count > 0 && i == 199)
                        {
                            SaveResult[] saveResults = binding.update(listaUpdate.ToArray());
                            //TODO: incluir o traetamento mais atual de erros <<<<<<<<<<<<<<<<<<<<<<<<<----------------------------------------------
                            //msgLog.AppendLine(string.Format("Totais:  Sucesso {0},  Erro {1}", sucesso, falha));

                            Console.WriteLine(msgLog.ToString());
                            IO.EscreverArquivo(arqSaida, msgLog.ToString());
                            i = 0;
                            listaUpdate.Clear();
                        }
                    }  // fim foreach de clientes


                    //Update do remanescente da lista
                    if (i < 200)
                    {
                        totalAtualizado += i;
                        SaveResult[] saveResults = binding.update(listaUpdate.ToArray());
                        //TODO: incluir o traetamento mais atual de erros <<<<<<<<<<<<<<<<<<<<<<<<<----------------------------------------------
                        //msgLog.AppendLine(string.Format("Últimos Totais:  Sucesso {0},  Erro {1}", sucesso, falha));

                        Console.WriteLine(msgLog.ToString());
                        IO.EscreverArquivo(arqSaida, msgLog.ToString());
                        i = 0;
                        listaUpdate.Clear();
                    }

                    msgLog.AppendLine(string.Format("Total clientes: {0},  Total Atualizado {1}", contCliente, totalAtualizado));
                    Console.WriteLine(msgLog.ToString());
                    IO.EscreverArquivo(arqSaida, msgLog.ToString());

                }  //fim arquivoSaida

                Console.ReadKey();
            }
            catch (Exception ex)
            {
                Console.WriteLine(string.Concat("\nErro ao atualizar o registro: \n", ex.Message, ex.StackTrace));
            }
        }


        /// <summary>
        /// Ingressa dados de medidores.  Parte do processo manual de alta de contratação do Salesforce para o SAP, para casos antigos.
        /// </summary>
        /// <param name="arquivo"></param>
        [ComVisible(true)]
        internal void IngressarDevices(Arquivo arquivo)
        {
            if (!loggedIn)
            {
                Autenticacao auth = new Autenticacao(this.ambiente);
                if (!auth.ValidarLogin(ref this.binding))
                    return;

                this.loggedIn = true;
            }

            int i = 0;
            int totalAtualizado = 0;
            int contCliente = 0;
            sObject sfObj = null;
            List<sObject> lstObjetos = new List<sObject>();

            try
            {
                List<DeviceSalesforce> listaArq = ArquivoLoader.GetDevices(arquivo);
                StringBuilder msgLog = new StringBuilder();
                int sucesso = 0;
                int falha = 0;

                using (Arquivo arqSaida = new Arquivo(arquivo.Caminho, string.Concat(arquivo.Nome, "_SAIDA"), arquivo.Extensao))
                {
                    foreach (DeviceSalesforce rs in listaArq)
                    {
                        sucesso = 0;
                        falha = 0;
                        msgLog.Clear();
                        contCliente++;

                        i++;

                        //tipo de objeto do SalesForce
                        sfObj = new sObject();
                        sfObj.type = "Device__c";

                        ClienteSalesforce cli = SalesforceDAO.GetPodsPorExternalId(rs.ExternalId, ref binding).FirstOrDefault();
                        if (cli == null || string.IsNullOrWhiteSpace(cli.Id))
                        {
                            continue;
                            //TODO: log
                            //throw new InvalidOperationException(string.Format("Cliente {0} não localizado.", rs.PointOfDeliveryId));
                        }

                        sfObj.Any = new System.Xml.XmlElement[] {
                            SFDCSchemeBuild.GetNewXmlElement("MeterNumber__c", rs.Numero),
                            SFDCSchemeBuild.GetNewXmlElement("MeterBrand__c", rs.Marca),
                            SFDCSchemeBuild.GetNewXmlElement("MeterModel__c", rs.Modelo),
                            SFDCSchemeBuild.GetNewXmlElement("MeterProperty__c", rs.Propriedade),
                            SFDCSchemeBuild.GetNewXmlElement("MeterType__c", rs.TipoMedicao),
                            SFDCSchemeBuild.GetNewXmlElement("ExternalID__c", rs.ExternalId),
                            SFDCSchemeBuild.GetNewXmlElement("Instalation_date__c", rs.DataInstalacao),
                            SFDCSchemeBuild.GetNewXmlElement("MeasureType__c", rs.TipoMedicao),
                            //SFDCSchemeBuild.GetNewXmlElement("Constant__c", rs.Constante),
                            //SFDCSchemeBuild.GetNewXmlElement("Retirement_date__c", rs.DataRetirada),
                            SFDCSchemeBuild.GetNewXmlElement("Status__c", rs.Estado),
                            SFDCSchemeBuild.GetNewXmlElement("PointofDelivery__c", cli.Id) ,
                            SFDCSchemeBuild.GetNewXmlElement("ConstanteDEM__c", rs.Constante1) ,
                            SFDCSchemeBuild.GetNewXmlElement("ConstantePRODIA__c", rs.Constante2) ,
                            SFDCSchemeBuild.GetNewXmlElement("ConstantePROANT__c", rs.Constante3) ,
                            SFDCSchemeBuild.GetNewXmlElement("ConstanteATIVAHP__c", rs.Constante4) ,
                            SFDCSchemeBuild.GetNewXmlElement("ConstanteDMCRHP__c", rs.Constante5) ,
                            //SFDCSchemeBuild.GetNewXmlElement("CreatedYear__c", rs.DataFabricacao.ToString()) ,
                            //SFDCSchemeBuild.GetNewXmlElement("Cubicle__c", rs.Cubiculo) ,
                            SFDCSchemeBuild.GetNewXmlElement("CurrencyIsoCode", rs.CurrencyIsoCode) ,
                            SFDCSchemeBuild.GetNewXmlElement("RecordTypeId", rs.RecordTypeId)
                        };
                        
                        lstObjetos.Add(sfObj);
                        totalAtualizado++;

                        msgLog.AppendLine(string.Format("Registro OK: {0} :: {1} - {2}", contCliente, rs.Numero, rs.PointOfDeliveryId));
                        Console.WriteLine(msgLog.ToString());
                        IO.EscreverArquivo(arqSaida, msgLog.ToString());

                        //Update a cada 200 registros, por limitação do Sales ce
                        if (lstObjetos.Count > 0 && i == 199)
                        {
                            SaveResult[] saveResults = SalesforceDAO.InserirObjetosSF(lstObjetos, ref binding);
                            sucesso = saveResults.Where(x => x.success = true).Count();
                            falha = saveResults.Where(x => x.success = false).Count();
                            msgLog.AppendLine(string.Format("Totais:  Sucesso {0},  Erro {1}", sucesso, falha));

                            Console.WriteLine(msgLog.ToString());
                            IO.EscreverArquivo(arqSaida, msgLog.ToString());
                            i = 0;
                            lstObjetos.Clear();
                        }
                    }  // fim foreach


                    //Update do remanescente da lista
                    if (i < 200)
                    {
                        totalAtualizado += i;
                        SaveResult[] saveResults = SalesforceDAO.InserirObjetosSF(lstObjetos, ref binding);
                        sucesso = saveResults.Where(x => x.success = true).Count();
                        falha = saveResults.Where(x => x.success = false).Count();
                        msgLog.AppendLine(string.Format("Últimos Totais:  Sucesso {0},  Erro {1}", sucesso, falha));

                        Console.WriteLine(msgLog.ToString());
                        IO.EscreverArquivo(arqSaida, msgLog.ToString());
                        i = 0;
                        lstObjetos.Clear();
                    }

                    msgLog.AppendLine(string.Format("Total clientes: {0},  Total Atualizado {1}", contCliente, totalAtualizado));
                    Console.WriteLine(msgLog.ToString());
                    IO.EscreverArquivo(arqSaida, msgLog.ToString());

                }  //fim arquivoSaida

                Console.ReadKey();
            }
            catch (Exception ex)
            {
                Console.WriteLine("\nErro ao atualizar o registro: \n" + ex.Message);
            }
        }



        /// <summary>
        /// 
        /// </summary>
        /// <param name="empresa"></param>
        /// <param name="asset"></param>
        [ComVisible(true)]
        internal OrderSalesforce IngressarOrder(AssetDTO asset, ItemAttribute items, string caseId = "")
        {
            sObject sfObj = new sObject();
            List<sObject> lstObjetos = new List<sObject>();

            #region Criar ORDER
            OrderSalesforce novoOrder = new OrderSalesforce();
            try
            {
                novoOrder.AccountId = asset.AccountId;
                novoOrder.ExternalId = string.Format("{0}ORD", asset.AccountExternalId);
                novoOrder.CNT_Case__c = caseId;

                sfObj.type = "NE__Order__c";
                sfObj.Any = new System.Xml.XmlElement[] {
                    //SFDCSchemeBuild.GetNewXmlElement("ExternalId", novoOrder.ExternalId),             //TODO: descobrir o que fazer
                    SFDCSchemeBuild.GetNewXmlElement("NE__AccountId__c", novoOrder.AccountId),
                    SFDCSchemeBuild.GetNewXmlElement("NE__Type__c", "InOrder"),
                    SFDCSchemeBuild.GetNewXmlElement("NE__CatalogId__c", "a101o00000EBAXWAA5"),         //a101o00000EBAXWAA5    TODO: descobrir os Ids
                    SFDCSchemeBuild.GetNewXmlElement("NE__COMMERCIALMODELID__C", "a141o00000GhwG3AAJ"), //TODO: descobrir os Ids
                    SFDCSchemeBuild.GetNewXmlElement("NE__BILLACCID__C", novoOrder.AccountId),
                    SFDCSchemeBuild.GetNewXmlElement("NE__SERVACCID__C", novoOrder.AccountId),
                    SFDCSchemeBuild.GetNewXmlElement("NE__OrderStatus__c", "Pending"),
                    //SFDCSchemeBuild.GetNewXmlElement("Country__c", "BRASIL"),
                    SFDCSchemeBuild.GetNewXmlElement("CurrencyIsoCode", "BRL"),
                    SFDCSchemeBuild.GetNewXmlElement("CNT_Case__c", novoOrder.CNT_Case__c),
                    SFDCSchemeBuild.GetNewXmlElement("NE__ConfigurationStatus__c", "Valid")
                };

                lstObjetos.Add(sfObj);
                SaveResult[] saveResults = SalesforceDAO.InserirObjetosSF(lstObjetos, ref binding);
                SaveResult resultado = saveResults.First();
                if (resultado != null && resultado.errors != null && resultado.errors.Count() > 0)
                {
                    throw new Exception(string.Join(";", resultado.errors.Select(e => e.message).ToArray()));
                }
                novoOrder.Id = saveResults[0].id;
            }
            catch (Exception ex)
            {
                Debugger.Break();

                this.lstLog.Add(string.Format("\nErro ao ingressar Order para o Asset: {0}: {1} {2}\n", asset.ExternalId, ex.Message, ex.StackTrace));
                Console.WriteLine(this.lstLog.Last());
            }
            finally
            {
                lstObjetos.Clear();
            }
            #endregion


            #region Criar ORDER ITEM
            OrderItemSalesforce orderItem = new OrderItemSalesforce();
            try
            {
                orderItem.OrderId = novoOrder.Id;
                orderItem.Country = "BRASIL";
                orderItem.CurrencyIsoCode = "BRL";
                orderItem.ExternalId = novoOrder.ExternalId;
                orderItem.AccountId = novoOrder.AccountId;
                //orderItem.AssetItemEnterpriseId = asset.ExternalId;
                orderItem.BillingAccountId = novoOrder.AccountId;
                orderItem.Catalog = "a101o00000EBAXWAA5";                   //TODO: migrar para uma classe especifica por Empresa
                orderItem.CatalogItem = "A".Equals(asset.TipoCliente) ? "a0z1o000003z2FNAAY" : "a0z1o000003z2FOAAY";    //TODO: migrar para uma classe especifica por Empresa
                orderItem.ProductId = "A".Equals(asset.TipoCliente) ? "a1f1o00000bsF6sAAE" : "a1f1o00000bsF6tAAE";      //TODO: migrar para uma classe especifica por Empresa
                orderItem.ServiceAccountId = novoOrder.AccountId;
                sfObj = new sObject();
                sfObj.type = "NE__OrderItem__c";
                sfObj.Any = new System.Xml.XmlElement[] {
                    //SFDCSchemeBuild.GetNewXmlElement("ExternalId", novoOrder.ExternalId),
                    SFDCSchemeBuild.GetNewXmlElement("NE__OrderId__c", orderItem.OrderId),
                    SFDCSchemeBuild.GetNewXmlElement("NE__Action__c", orderItem.Action),
                    SFDCSchemeBuild.GetNewXmlElement("NE__Qty__c", orderItem.Qty),
                    SFDCSchemeBuild.GetNewXmlElement("NE__CatalogItem__c", orderItem.CatalogItem),
                    SFDCSchemeBuild.GetNewXmlElement("NE__ProdId__c", orderItem.ProductId),
                    SFDCSchemeBuild.GetNewXmlElement("NE__ACCOUNT__C", orderItem.AccountId),
                    SFDCSchemeBuild.GetNewXmlElement("NE__BILLING_ACCOUNT__C", orderItem.BillingAccountId),
                    SFDCSchemeBuild.GetNewXmlElement("NE__CATALOG__C", orderItem.Catalog),
                    SFDCSchemeBuild.GetNewXmlElement("NE__SERVICE_ACCOUNT__C", orderItem.ServiceAccountId),
                    SFDCSchemeBuild.GetNewXmlElement("NE__STATUS__C", orderItem.Status),
                    SFDCSchemeBuild.GetNewXmlElement("NE__ASSETITEMENTERPRISEID__C", orderItem.ExternalId),
                    SFDCSchemeBuild.GetNewXmlElement("NE__BASEONETIMEFEE__C", orderItem.BaseOneTimeFee),
                    SFDCSchemeBuild.GetNewXmlElement("NE__BASERECURRINGCHARGE__C", orderItem.BaseRecurringCharge),
                    SFDCSchemeBuild.GetNewXmlElement("NE__ONETIMEFEEOV__C", orderItem.OneTimeFeeOv),
                    SFDCSchemeBuild.GetNewXmlElement("NE__RECURRINGCHARGEFREQUENCY__C", orderItem.RecurringChargeFrequency),
                    SFDCSchemeBuild.GetNewXmlElement("NE__RECURRINGCHARGEOV__C", orderItem.RecurringChargeOv),
                    SFDCSchemeBuild.GetNewXmlElement("NE__Country__c", orderItem.Country),
                    SFDCSchemeBuild.GetNewXmlElement("CurrencyIsoCode", orderItem.CurrencyIsoCode)
                };

                lstObjetos.Add(sfObj);
                SaveResult[] saveResults = SalesforceDAO.InserirObjetosSF(lstObjetos, ref binding);
                SaveResult resultado = saveResults.First();
                if (resultado != null && resultado.errors != null && resultado.errors.Count() > 0)
                {
                    throw new Exception(string.Join(";", resultado.errors.Select(e => e.message).ToArray()));
                }
                orderItem.Id = saveResults[0].id;
                novoOrder.OrderItem = orderItem;
            }
            catch(Exception ex)
            {
                this.lstLog.Add(string.Format("\nErro ao ingressar Item Order para o Order {0} e Asset: {1} {2} {3}\n", novoOrder.Id, asset.ExternalId, ex.Message, ex.StackTrace));
                Console.WriteLine(this.lstLog.Last());
                Debugger.Break();
                //TODO: tratar exceção
                //throw new Exception(string.Format("Erro ao criar o Order Item para o Order {0}: {1} {2}", novoOrder.Id, ex.Message, ex.StackTrace));
            }
            finally
            {
                lstObjetos.Clear();
            }
            #endregion

            try
            {
                bool resultadoDelete = false;
                //ApagarAssetItemAttributes(asset.Id, ref resultadoDelete);
                IngressarItemAttributes(asset, ref novoOrder, items);
            }
            catch
            {
                #region Criar ORDER/ASSET ITEM ATTRIBUTEs [OBSOLETO]
                //foreach (string attributeType in new string[] { "NE__AssetItemAttribute__c", "NE__Order_Item_Attribute__c" })
                //{
                //    string parentIdCampo = "NE__AssetItemAttribute__c".Equals(attributeType) ? "NE__Asset__c" : "NE__Order_Item__c";
                //    ItemAttribute orderItemAttr = new ItemAttribute();
                //    orderItem.OrderItemAttributes = new List<ItemAttribute>();
                //    try
                //    {
                //        bool? _modalidadeVerde = null; //NULL = OPTANTE
                //        string tarifaPod = null;

                //        foreach (PropertyInfo prop in typeof(ItemAttribute).GetProperties())
                //        {
                //            tarifaPod = string.Empty;
                //            _modalidadeVerde = null;

                //            foreach (DisplayAttribute idAttr in ((DisplayAttribute[])(prop.GetCustomAttributes(typeof(DisplayAttribute), true))))
                //            {
                //                var valor = prop.GetValue(items, null).ToString();

                //                if ("Categoria de Tarifa BR".Equals(idAttr.Name))
                //                {
                //                    if (!string.IsNullOrWhiteSpace(asset.PointofDeliveryId))
                //                        tarifaPod = valor;
                //                }

                //                if ("Modalidade Tarifaria BR".Equals(idAttr.Name) && valor.ToUpper().Contains("VERDE"))
                //                    _modalidadeVerde = true;

                //                else if ("Modalidade Tarifaria BR".Equals(idAttr.Name) && valor.ToUpper().Contains("AZUL"))
                //                    _modalidadeVerde = false;

                //                valor = (("DemandaPonta".Equals(prop.Name) || "DemandaForaPonta".Equals(prop.Name) || "DemandaKV".Equals(prop.Name)) && !_modalidadeVerde.HasValue) ? string.Empty : valor;
                //                valor = (("DemandaPonta".Equals(prop.Name) || "DemandaForaPonta".Equals(prop.Name)) && _modalidadeVerde.HasValue && _modalidadeVerde.Value) ? string.Empty : valor;
                //                valor = ("DemandaKV".Equals(prop.Name) && _modalidadeVerde.HasValue && !_modalidadeVerde.Value) ? string.Empty : valor;

                //                var valorConvertido = string.IsNullOrWhiteSpace(valor) ? " " : Convert.ChangeType(valor, prop.PropertyType);

                //                prop.SetValue(orderItemAttr, valorConvertido, null);

                //                sfObj = new sObject();
                //                sfObj.type = attributeType;
                //                sfObj.Any = new System.Xml.XmlElement[] {
                //            //SFDCSchemeBuild.GetNewXmlElement("Externalid_asset", novoOrder.ExternalId),
                //            SFDCSchemeBuild.GetNewXmlElement(parentIdCampo, "NE__AssetItemAttribute__c".Equals(attributeType) ? asset.Id : orderItem.Id),
                //            SFDCSchemeBuild.GetNewXmlElement("Name", idAttr.Name),
                //            SFDCSchemeBuild.GetNewXmlElement("NE__Value__c", CultureInfo.CurrentCulture.TextInfo.ToTitleCase(valorConvertido.ToString())),
                //            SFDCSchemeBuild.GetNewXmlElement("NE__Old_Value__c", CultureInfo.CurrentCulture.TextInfo.ToTitleCase(valorConvertido.ToString())),
                //            SFDCSchemeBuild.GetNewXmlElement("NE__FamPropId__c", orderItemAttr.dicPropriedadesDinamicas[idAttr.Name]),
                //            SFDCSchemeBuild.GetNewXmlElement("NE__FAMPROPEXTID__C", string.Concat("Atributos CE:", idAttr.Name))
                //            };

                //                lstObjetos.Add(sfObj);

                //                if ("NE__Order_Item_Attribute__c".Equals(attributeType))
                //                {
                //                    orderItem.OrderItemAttributes.Add(orderItemAttr);
                //                    novoOrder.OrderItem = orderItem;
                //                }
                //            }
                //        }

                //        if (lstObjetos.Count > 0)
                //        {
                //            SaveResult[] saveResults = SalesforceDAO.InserirObjetosSF(lstObjetos, ref binding);
                //            SaveResult resultado = saveResults.First();
                //            if (resultado != null && resultado.errors != null && resultado.errors.Count() > 0)
                //            {
                //                throw new Exception(string.Join(";", resultado.errors.Select(e => e.message).ToArray()));
                //            }
                //        }
                //    }
                //    catch (Exception ex)
                //    {
                //        Console.WriteLine(string.Format("\nErro ao ingressar {0} Item Attribute para o Item Order: {1}: {2} {3}\n"
                //            , attributeType, orderItem.Id, ex.Message, ex.StackTrace));

                //        Debugger.Break();
                //    }
                //    finally
                //    {
                //        lstObjetos.Clear();
                //    }
                //}

                #endregion
            }
                
            return novoOrder;
        }



        /// <summary>
        /// Novo método B2Win
        /// </summary>
        /// <param name="asset"></param>
        /// <param name="idOrder"></param>
        /// <param name="items"></param>
        /// <returns></returns>
        private void IngressarItemAttributes(AssetDTO asset, ref OrderSalesforce novoOrder, ItemAttribute items)
        {
            List<sObject> lstObjetos = new List<sObject>();
            sObject sfObj;
            if (novoOrder != null && novoOrder.OrderItem != null && novoOrder.OrderItem.OrderItemAttributes == null)
                novoOrder.OrderItem.OrderItemAttributes = new List<ItemAttribute>();

            //NOVO NOVO NOVO NOVO NOVO
            bool apagado = false;
            //this.lstLog.AddRange(ApagarAssetItemAttributes(asset, ref apagado));
            this.lstLog.AddRange(ApagarOrderEDerivados(asset, ref apagado));

            #region Criar ORDER/ASSET ITEM ATTRIBUTEs
            foreach (string attributeType in new string[] { "NE__AssetItemAttribute__c", "NE__Order_Item_Attribute__c" })
            {
                string parentIdCampo = "NE__AssetItemAttribute__c".Equals(attributeType) ? "NE__Asset__c" : "NE__Order_Item__c";
                ItemAttribute orderItemAttr = new ItemAttribute();
                List<ItemAttribute> orderItemAttributes = new List<ItemAttribute>();
                try
                {
                    bool? _modalidadeVerde = null; //NULL = OPTANTE
                    string tarifaPod = null;

                    foreach (PropertyInfo prop in typeof(ItemAttribute).GetProperties())
                    {
                        tarifaPod = string.Empty;
                        _modalidadeVerde = null;

                        foreach (DisplayAttribute idAttr in ((DisplayAttribute[])(prop.GetCustomAttributes(typeof(DisplayAttribute), true))))
                        {
                            try
                            {
                                var valor = prop.GetValue(items, null).ToString();
                                if ("Categoria de Tarifa BR".Equals(idAttr.Name))
                                {
                                    if (!string.IsNullOrWhiteSpace(asset.PointofDeliveryId))
                                        tarifaPod = valor;
                                }

                                if ("Modalidade Tarifaria BR".Equals(idAttr.Name) && valor.ToUpper().Contains("VERDE"))
                                    _modalidadeVerde = true;

                                else if ("Modalidade Tarifaria BR".Equals(idAttr.Name) && valor.ToUpper().Contains("AZUL"))
                                    _modalidadeVerde = false;

                                valor = (("DemandaPonta".Equals(prop.Name) || "DemandaForaPonta".Equals(prop.Name) || "DemandaKV".Equals(prop.Name)) && !_modalidadeVerde.HasValue) ? string.Empty : valor;
                                valor = (("DemandaPonta".Equals(prop.Name) || "DemandaForaPonta".Equals(prop.Name)) && _modalidadeVerde.HasValue && _modalidadeVerde.Value) ? string.Empty : valor;
                                valor = ("DemandaKV".Equals(prop.Name) && _modalidadeVerde.HasValue && !_modalidadeVerde.Value) ? string.Empty : valor;

                                //TODO: levar este trecho para a propria classe ItemAttribute
                                if(string.IsNullOrWhiteSpace(valor))
                                {
                                    DefaultValueAttribute[] defaulvalue = ((DefaultValueAttribute[])prop.GetCustomAttributes(typeof(DefaultValueAttribute), true));
                                    if(defaulvalue != null && defaulvalue.Count() > 0)
                                    {
                                        valor = defaulvalue[0].Value.ToString();
                                    }
                                }

                                var valorConvertido = string.IsNullOrWhiteSpace(valor) ? " " : Convert.ChangeType(valor, prop.PropertyType);

                                prop.SetValue(orderItemAttr, valorConvertido, null);

                                sfObj = new sObject();
                                sfObj.type = attributeType;
                                sfObj.Any = new System.Xml.XmlElement[] {
                                    //SFDCSchemeBuild.GetNewXmlElement("Externalid_asset", novoOrder.ExternalId),
                                    SFDCSchemeBuild.GetNewXmlElement(parentIdCampo, "NE__AssetItemAttribute__c".Equals(attributeType) ? asset.Id : novoOrder.OrderItem.Id),
                                    SFDCSchemeBuild.GetNewXmlElement("Name", idAttr.Name),
                                    SFDCSchemeBuild.GetNewXmlElement("NE__Value__c", CultureInfo.CurrentCulture.TextInfo.ToTitleCase(valorConvertido.ToString())),
                                    SFDCSchemeBuild.GetNewXmlElement("NE__Old_Value__c", CultureInfo.CurrentCulture.TextInfo.ToTitleCase(valorConvertido.ToString())),
                                    SFDCSchemeBuild.GetNewXmlElement("NE__FamPropId__c", orderItemAttr.dicPropriedadesDinamicas[idAttr.Name]),
                                    SFDCSchemeBuild.GetNewXmlElement("NE__FAMPROPEXTID__C", string.Concat("Atributos CE:", idAttr.Name))
                                };
                            }
                            catch (NullReferenceException)
                            {
                                continue;
                            }
                            lstObjetos.Add(sfObj);

                            if ("NE__Order_Item_Attribute__c".Equals(attributeType))
                            {
                                novoOrder.OrderItem.OrderItemAttributes.Add(orderItemAttr);
                            }
                        }
                    }
                    novoOrder.OrderItem = novoOrder.OrderItem;

                    if (lstObjetos.Count > 0)
                    {
                        SaveResult[] saveResults = SalesforceDAO.InserirObjetosSF(lstObjetos, ref binding);
                        SaveResult resultado = saveResults.First();
                        if (resultado != null && resultado.errors != null && resultado.errors.Count() > 0)
                        {
                            string erros = string.Join(";", resultado.errors.Select(e => e.message).ToArray());
                            this.lstLog.Add(erros);
                            throw new Exception(erros);
                        }
                    }
                }
                catch (Exception ex)
                {
                    this.lstLog.Add(string.Format("\nErro ao ingressar {0} Item Attribute para o Item Order: {1}: {2} {3}\n"
                        , attributeType, novoOrder.OrderItem.Id, ex.Message, ex.StackTrace));
                    throw new Exception();
                }
                finally
                {
                    lstObjetos.Clear();
                }
            }
            #endregion
        }



        [ComVisible(true)] //---------------OK
        public string IngressarStreet(string[] rows)
        {
            autenticar();

            List<sObject> lstObjetos = new List<sObject>();
            sObject sfObj = null;

            //StreetExternalId__c	Name	Street_Type__c	_Municipality__	_Region__	Country__c	Municipality__c	Region__c	_Calle	LocationCode__c	Neighbourhood__c    NeighbourhoodCode__c	Company__c

            sfObj = new sObject();
            sfObj.type = "Street__c";

            sfObj.Any = new System.Xml.XmlElement[] {
            SFDCSchemeBuild.GetNewXmlElement("StreetExternalId__c", rows[(int)DicColuna.col1_7.identificador_street]),
            SFDCSchemeBuild.GetNewXmlElement("Name", rows[(int)DicColuna.col1_7.nombre_calle]),
            SFDCSchemeBuild.GetNewXmlElement("Street_Type__c", string.IsNullOrWhiteSpace(rows[(int)DicColuna.col1_7.tipo_calle]) ? " " : rows[(int)DicColuna.col1_7.tipo_calle]),
            //SFDCSchemeBuild.GetNewXmlElement("_Municipality__", rows[(int)DicColuna.col1_7.ciudad]),
            //SFDCSchemeBuild.GetNewXmlElement("_Region__", rows[(int)DicColuna.col1_7.uf]),
            SFDCSchemeBuild.GetNewXmlElement("Country__c", rows[(int)DicColuna.col1_7.pais_street]),
            SFDCSchemeBuild.GetNewXmlElement("Municipality__c", rows[(int)DicColuna.col1_7.comuna_street]),
            SFDCSchemeBuild.GetNewXmlElement("Region__c", rows[(int)DicColuna.col1_7.region]),
            //SFDCSchemeBuild.GetNewXmlElement("_Calle", rows[(int)DicColuna.col1_7.calle]),
            SFDCSchemeBuild.GetNewXmlElement("LocationCode__c", rows[(int)DicColuna.col1_7.localidad]),
            SFDCSchemeBuild.GetNewXmlElement("Neighbourhood__c", rows[(int)DicColuna.col1_7.barrio_street]),
            //SFDCSchemeBuild.GetNewXmlElement("NeighbourhoodCode__c", rows[(int)DicColuna.col1_7.codigo_bairro]),      ??????????????????????????????????
            SFDCSchemeBuild.GetNewXmlElement("Company__c", rows[(int)DicColuna.col1_7.id_empresa].Equals("2003") ? "COELCE" : "AMPLA")
            };

            lstObjetos.Add(sfObj);
            UpsertResult[] saveResults = SalesforceDAO.Upsert("StreetExternalId__c", lstObjetos, ref binding);
            UpsertResult resultado = saveResults.First();
            if (resultado != null && resultado.errors != null && resultado.errors.Count() > 0)
            {
                throw new Exception(string.Join(";", resultado.errors.Select(e => e.message).ToArray()));
            }

            return resultado.id;
        }


        [ComVisible(true)]
        public string IngressarAddress(string[] rows)
        {
            autenticar();

            List<sObject> lstObjetos = new List<sObject>();
            sObject sfObj = null;

            sfObj = new sObject();
            sfObj.type = "Address__c";
            #region Campos de Address
            //address.Append("\""); address.Append(rows[(int)DicColuna.col1_7.moneda_address]); address.Append("\";");
            //address.Append("\""); address.Append(rows[(int)DicColuna.col1_7.complemento]); address.Append("\";");
            //address.Append("\""); address.Append(rows[(int)DicColuna.col1_7.numero]); address.Append("\";");
            //address.Append("\""); address.Append(rows[(int)DicColuna.col1_7.esquina_via_sec].Replace(Environment.NewLine, " ").Replace("\n", " ")); address.Append("\";");
            //address.Append("\""); address.Append(rows[(int)DicColuna.col1_7.cep]); address.Append("\";");
            //address.Append("\""); address.Append(rows[(int)DicColuna.col1_7.identificador_address]); address.Append("\";");
            //address.Append("\""); address.Append(rows[(int)DicColuna.col1_7.identificador_street_address]); address.Append("\";");
            //address.Append("\""); address.Append(rows[(int)DicColuna.col1_7.barrio]); address.Append("\";");
            //// + "\"" + rows[(int)DicColuna.col1_7.calle] + "\";"                                                                     
            //address.Append("\""); address.Append(rows[(int)DicColuna.col1_7.tipo_numeracion]); address.Append("\";");
            //address.Append("\""); address.Append(rows[(int)DicColuna.col1_7.direccion_concatenada]); address.Append("\";");
            //address.Append("\""); address.Append(rows[(int)DicColuna.col1_7.bloque_direccion]); address.Append("\";");
            //address.Append("\""); address.Append(rows[(int)DicColuna.col1_7.coord_x]); address.Append("\";");
            //address.Append("\""); address.Append(rows[(int)DicColuna.col1_7.coord_y]); address.Append("\";");
            //address.Append("\""); address.Append(rows[(int)DicColuna.col1_7.nombre_agrupacion]); address.Append("\";");
            //address.Append("\""); address.Append(rows[(int)DicColuna.col1_7.tipo_agrupacion_address]); address.Append("\";");
            //address.Append("\""); address.Append(rows[(int)DicColuna.col1_7.tipo_interior]); address.Append("\";");
            //address.Append("\""); address.Append(rows[(int)DicColuna.col1_7.direccion_larga]); address.Append("\";");
            //address.Append("\""); address.Append(rows[(int)DicColuna.col1_7.lote_manzana]); address.Append("\";");
            //address.Append("\""); address.Append(rows[(int)DicColuna.col1_7.tipo_sector]); address.Append("\";");
            //address.Append("\""); address.Append(rows[(int)DicColuna.col1_7.id_empresa]); address.Append("\";");
            #endregion

            try
            {
                sfObj.Any = new System.Xml.XmlElement[] 
                {
                    SFDCSchemeBuild.GetNewXmlElement("CurrencyIsoCode", rows[(int)DicColuna.col1_7.moneda_address]),
                    SFDCSchemeBuild.GetNewXmlElement("Corner__c", rows[(int)DicColuna.col1_7.complemento]),
                    SFDCSchemeBuild.GetNewXmlElement("Number__c", rows[(int)DicColuna.col1_7.numero]),
                    SFDCSchemeBuild.GetNewXmlElement("Reference__c", rows[(int)DicColuna.col1_7.esquina_via_sec].Replace(Environment.NewLine, " ").Replace("\n", " ")),
                    SFDCSchemeBuild.GetNewXmlElement("Postal_Code__c", rows[(int)DicColuna.col1_7.cep]),
                    SFDCSchemeBuild.GetNewXmlElement("ExternalId__c", rows[(int)DicColuna.col1_7.identificador_address]),
                    SFDCSchemeBuild.GetNewXmlElement("StreetMD__r","Street__c:ExternalId__c", rows[(int)DicColuna.col1_7.identificador_street_address]),
                    SFDCSchemeBuild.GetNewXmlElement("Block__c", rows[(int)DicColuna.col1_7.barrio]),
                    SFDCSchemeBuild.GetNewXmlElement("LongAddress__c", rows[(int)DicColuna.col1_7.direccion_concatenada]),
                    //SFDCSchemeBuild.GetNewXmlElement("CoordinateX__c", rows[(int)DicColuna.col1_7.coord_x]),
                    //SFDCSchemeBuild.GetNewXmlElement("CoordinateY__c", rows[(int)DicColuna.col1_7.coord_y]),
                    //SFDCSchemeBuild.GetNewXmlElement("Department__c", rows[(int)DicColuna.col1_7.tipo_sector]),
                    SFDCSchemeBuild.GetNewXmlElement("CompanyID__c", rows[(int)DicColuna.col1_7.id_empresa].Equals("2003") ? "COELCE" : "AMPLA"),
                };
            }
            catch { }

            lstObjetos.Add(sfObj);
            UpsertResult[] saveResults = SalesforceDAO.Upsert("ExternalId__c", lstObjetos, ref binding);
            UpsertResult resultado = saveResults.First();
            if (resultado != null && resultado.errors != null && resultado.errors.Count() > 0)
            {
                throw new Exception(string.Join(";", resultado.errors.Select(e => e.message).ToArray()));
            }

            return resultado.id;
        }
    

        [ComVisible(true)]
        public string IngressarPointOfDelivery(string[] rows)
        {
            if (!loggedIn)
            {
                Autenticacao auth = new Autenticacao(this.ambiente);
                if (!auth.ValidarLogin(ref this.binding))
                    return string.Empty;

                this.loggedIn = true;
            }

            List<sObject> lstObjetos = new List<sObject>();
            sObject sfObj = null;

            sfObj = new sObject();
            sfObj.type = "PointofDelivery__c";

            try
            {
                sfObj.Any = new System.Xml.XmlElement[] 
                {
                    SFDCSchemeBuild.GetNewXmlElement("ExternalId__c", rows[(int)DicColuna.col1_7.identificador_pod]),   //1
                    SFDCSchemeBuild.GetNewXmlElement("Name", rows[(int)DicColuna.col1_7.numero_pod]),   //2             
                    SFDCSchemeBuild.GetNewXmlElement("CurrencyIsoCode", rows[(int)DicColuna.col1_7.moneda_pod]),   //3            
                    SFDCSchemeBuild.GetNewXmlElement("_DV Number Point of Delivery", rows[(int)DicColuna.col1_7.digito_verificador_pod]),   //4       <<<<<<<<<<<<<<<<<<<<<<
                    SFDCSchemeBuild.GetNewXmlElement("_ID Direccion", rows[(int)DicColuna.col1_7.identificador_address]),   //5             <<<<<<<<<<<<<<<<<<<<<<
                    SFDCSchemeBuild.GetNewXmlElement("_Point of Delivery Status", rows[(int)DicColuna.col1_7.estado_pod]),   //6             <<<<<<<<<<<<<<<<<<<<<<
                    SFDCSchemeBuild.GetNewXmlElement("Country__c", rows[(int)DicColuna.col1_7.pais]),   //7             
                    SFDCSchemeBuild.GetNewXmlElement("_Comuna", rows[(int)DicColuna.col1_7.comuna]),   //8             <<<<<<<<<<<<<<<<<<<<<<
                    SFDCSchemeBuild.GetNewXmlElement("SegmentType__c", rows[(int)DicColuna.col1_7.tipo_segmento]),   //9             
                    SFDCSchemeBuild.GetNewXmlElement("__Medida de disciplina", rows[(int)DicColuna.col1_7.medida_disciplina]),   //10             <<<<<<<<<<<<<<<<<<<<<<
                    SFDCSchemeBuild.GetNewXmlElement("CompanyID__c", rows[(int)DicColuna.col1_7.id_empresa_pod]),   //11             
                    SFDCSchemeBuild.GetNewXmlElement("Electrodependant__c", rows[(int)DicColuna.col1_7.electrodependiente]),   //12             
                    SFDCSchemeBuild.GetNewXmlElement("Rate__c", rows[(int)DicColuna.col1_7.tarifa]),   //13             
                    SFDCSchemeBuild.GetNewXmlElement("_Tipo de agrupación", rows[(int)DicColuna.col1_7.tipo_agrupacion]),   //14             <<<<<<<<<<<<<<<<<<<<<<
                    SFDCSchemeBuild.GetNewXmlElement("_Full Electric", rows[(int)DicColuna.col1_7.full_electric]),   //15             <<<<<<<<<<<<<<<<<<<<<<
                    SFDCSchemeBuild.GetNewXmlElement("Route__c", rows[(int)DicColuna.col1_7.ruta]),   //16             
                    SFDCSchemeBuild.GetNewXmlElement("_Direccion de reparto", rows[(int)DicColuna.col1_7.direccion_reparto]),   //17             <<<<<<<<<<<<<<<<<<<<<<
                    SFDCSchemeBuild.GetNewXmlElement("_Distrito de reparto", rows[(int)DicColuna.col1_7.comuna_reparto]),   //18             <<<<<<<<<<<<<<<<<<<<<<
                    SFDCSchemeBuild.GetNewXmlElement("TransformerNumber__c", rows[(int)DicColuna.col1_7.num_transformador]),   //19             
                    SFDCSchemeBuild.GetNewXmlElement("TransformerType__c", rows[(int)DicColuna.col1_7.tipo_transformador]),   //20             
                    SFDCSchemeBuild.GetNewXmlElement("ConnectionType__c", rows[(int)DicColuna.col1_7.tipo_conexion]),   //21             
                    SFDCSchemeBuild.GetNewXmlElement("__Clasificacion Cliente / Estrato socioeconomico", rows[(int)DicColuna.col1_7.estrato_socioeconomico]),   //22   <<<<<<<<<<<<<<<<<<<<<<
                    SFDCSchemeBuild.GetNewXmlElement("ElectricalSubstationConnection__c", rows[(int)DicColuna.col1_7.subestacion_electrica_conexion]),   //23             
                    SFDCSchemeBuild.GetNewXmlElement("__Tipo de medida", rows[(int)DicColuna.col1_7.tipo_medida]),   //24             <<<<<<<<<<<<<<<<<<<<<<
                    SFDCSchemeBuild.GetNewXmlElement("FeederNumber__c", rows[(int)DicColuna.col1_7.num_alimentador]),   //25             
                    SFDCSchemeBuild.GetNewXmlElement("__Tipo de lectura", rows[(int)DicColuna.col1_7.tipo_lectura]),   //26             <<<<<<<<<<<<<<<<<<<<<<
                    SFDCSchemeBuild.GetNewXmlElement("__Bloque", rows[(int)DicColuna.col1_7.bloque]),   //27             <<<<<<<<<<<<<<<<<<<<<<
                    SFDCSchemeBuild.GetNewXmlElement("__Horario de Racionamiento", rows[(int)DicColuna.col1_7.horario_racionamiento]),   //28             <<<<<<<<<<<<<<<<<<<<<<
                    SFDCSchemeBuild.GetNewXmlElement("ConnectionStatus__c", rows[(int)DicColuna.col1_7.estado_conexao]),   //29             
                    SFDCSchemeBuild.GetNewXmlElement("CutoffDate__c", rows[(int)DicColuna.col1_7.fecha_corte]),   //30             
                    SFDCSchemeBuild.GetNewXmlElement("_Codigo PCR", rows[(int)DicColuna.col1_7.codigo_pcr]),   //31             <<<<<<<<<<<<<<<<<<<<<<
                    SFDCSchemeBuild.GetNewXmlElement("__SED", rows[(int)DicColuna.col1_7.sed]),   //32             <<<<<<<<<<<<<<<<<<<<<<
                    SFDCSchemeBuild.GetNewXmlElement("__SET", rows[(int)DicColuna.col1_7.set_cliente]),   //33             <<<<<<<<<<<<<<<<<<<<<<
                    SFDCSchemeBuild.GetNewXmlElement("Key__c", rows[(int)DicColuna.col1_7.llave]),   //34             
                    SFDCSchemeBuild.GetNewXmlElement("_Potencia de empalme", rows[(int)DicColuna.col1_7.potencia_capacidad_instalada]),   //35             <<<<<<<<<<<<<<<<<<<<<<
                    SFDCSchemeBuild.GetNewXmlElement("__VIP", rows[(int)DicColuna.col1_7.cliente_singular]),   //36             <<<<<<<<<<<<<<<<<<<<<<
                    SFDCSchemeBuild.GetNewXmlElement("_Clase de servicio", rows[(int)DicColuna.col1_7.clase_servicio_pod]),   //37             <<<<<<<<<<<<<<<<<<<<<<
                    SFDCSchemeBuild.GetNewXmlElement("_Subclase del servicio", rows[(int)DicColuna.col1_7.subclase_servicio]),   //38             <<<<<<<<<<<<<<<<<<<<<<
                    SFDCSchemeBuild.GetNewXmlElement("Route__c", rows[(int)DicColuna.col1_7.ruta_lectura]),   //39             
                    SFDCSchemeBuild.GetNewXmlElement("_Tipo de liquidacion", "Monthly"),   //40             <<<<<<<<<<<<<<<<<<<<<<
                    SFDCSchemeBuild.GetNewXmlElement("_Mercado", rows[(int)DicColuna.col1_7.mercado]),   //41             
                    SFDCSchemeBuild.GetNewXmlElement("_Carga Aforada", rows[(int)DicColuna.col1_7.carga_aforada]),   //42       <<<<<<<<<<<<<<<<<<<<<<      
                    SFDCSchemeBuild.GetNewXmlElement("Manufacturing_Date__c", rows[(int)DicColuna.col1_7.ano_fabricacion]),   //43             
                    SFDCSchemeBuild.GetNewXmlElement("_Centro poblado", rows[(int)DicColuna.col1_7.centro_poblado]),   //44     <<<<<<<<<<<<<<<<<<<<<<        
                    SFDCSchemeBuild.GetNewXmlElement("Network_Type__c", rows[(int)DicColuna.col1_7.tipo_rede]),   //45             
                    //SFDCSchemeBuild.GetNewXmlElement("ReadingProcess__c", " "),   //46             
                    //SFDCSchemeBuild.GetNewXmlElement("ServiceType__c", " "),   //47             
                    //SFDCSchemeBuild.GetNewXmlElement("DangerZone__c", " "),   //48             
                    //SFDCSchemeBuild.GetNewXmlElement("_Tipo de vivienda", " "),   //49             
                    SFDCSchemeBuild.GetNewXmlElement("Company__c", (rows[(int)DicColuna.col1_7.id_empresa].Equals("2003") ? "COELCE" : "AMPLA")),   //50             
                    //SFDCSchemeBuild.GetNewXmlElement("__Limite de invierno", rows[(int)DicColuna.col1_7.]),   //51             
                    //SFDCSchemeBuild.GetNewXmlElement("__Capacidad empalme", rows[(int)DicColuna.col1_7.]),   //52             
                    //SFDCSchemeBuild.GetNewXmlElement("__Tipo de empalme", rows[(int)DicColuna.col1_7.]),   //53             
                    //SFDCSchemeBuild.GetNewXmlElement("__Restriccion creacion suspension", rows[(int)DicColuna.col1_7.]),   //54             
                    //SFDCSchemeBuild.GetNewXmlElement("__Restriccion creacion convenio", rows[(int)DicColuna.col1_7.]),   //55             
                    //SFDCSchemeBuild.GetNewXmlElement("__Motivo restriccion creacion convenio", rows[(int)DicColuna.col1_7.]),   //56             
                    //SFDCSchemeBuild.GetNewXmlElement("__Fecha restriccion creacion convenio", rows[(int)DicColuna.col1_7.]),   //57          
                    SFDCSchemeBuild.GetNewXmlElement("_Zona", rows[(int)DicColuna.col1_7.localizacao_uc] == "R" ? "RURAL" : "URBAN"),   //58    <<<<<<<<<<<<<<<<<<<<<<
                    //SFDCSchemeBuild.GetNewXmlElement("_Bloqueo de corte", " "),   //59             
                    //SFDCSchemeBuild.GetNewXmlElement("_Cantidad de Personas", " "),   //60             
                    //SFDCSchemeBuild.GetNewXmlElement("_CNR", " "),   //61             
                    //SFDCSchemeBuild.GetNewXmlElement("_CNT_UniqueAddress", " "),   //62             
                    //SFDCSchemeBuild.GetNewXmlElement("_Continuous Operation Power Unit", " "),   //63             
                    //SFDCSchemeBuild.GetNewXmlElement("_Debt", " "),   //64             
                    //SFDCSchemeBuild.GetNewXmlElement("_Electrodependant Origin", " "),   //65             
                    //SFDCSchemeBuild.GetNewXmlElement("_Operating Range of Power Unit", " "),   //66             
                    //SFDCSchemeBuild.GetNewXmlElement("_Physical Space for Power Unit", " "),   //67             
                    //SFDCSchemeBuild.GetNewXmlElement("_Posibility of Relocation", " "),   //68             
                    //SFDCSchemeBuild.GetNewXmlElement("_Potency of Power Unit", " "),   //69             
                    //SFDCSchemeBuild.GetNewXmlElement("CNT_Sensitive_Customer__c", " "),   //70
                    //SFDCSchemeBuild.GetNewXmlElement("_Tipo de Eletrodependiente", " "),   //71
                    SFDCSchemeBuild.GetNewXmlElement("_notification_date", rows[(int)DicColuna.col1_7.data_notificacao])   //72
                };
            }
            catch { }

            lstObjetos.Add(sfObj);
            UpsertResult[] saveResults = SalesforceDAO.Upsert("ExternalId__c", lstObjetos, ref binding);
            UpsertResult resultado = saveResults.First();
            if (resultado != null && resultado.errors != null && resultado.errors.Count() > 0)
            {
                throw new Exception(string.Join(";", resultado.errors.Select(e => e.message).ToArray()));
            }

            return resultado.id;
        }


        [ComVisible(true)]
        public string IngressarAccount(string[] rows)
        {
            if (!loggedIn)
            {
                Autenticacao auth = new Autenticacao(this.ambiente);
                if (!auth.ValidarLogin(ref this.binding))
                    return string.Empty;

                this.loggedIn = true;
            }

            List<sObject> lstObjetos = new List<sObject>();
            sObject sfObj = null;

            sfObj = new sObject();
            sfObj.type = "Account";

            try
            {
                sfObj.Any = new System.Xml.XmlElement[] 
                {
                    SFDCSchemeBuild.GetNewXmlElement("ExternalId__c", rows[(int)DicColuna.col1_7.identificador_conta]),   //1
                    SFDCSchemeBuild.GetNewXmlElement("Name", rows[(int)DicColuna.col1_7.nombre]),   //2             
                    SFDCSchemeBuild.GetNewXmlElement("IdentityType__c", rows[(int)DicColuna.col1_7.tipo_identidade]),   //3            
                    SFDCSchemeBuild.GetNewXmlElement("IdentityNumber__c", rows[(int)DicColuna.col1_7.documento_cliente]),   //4       <<<<<<<<<<<<<<<<<<<<<<
                    SFDCSchemeBuild.GetNewXmlElement("PrimaryEmail__c", rows[(int)DicColuna.col1_7.mail]),   //5             <<<<<<<<<<<<<<<<<<<<<<
                    SFDCSchemeBuild.GetNewXmlElement("SecondaryEmail__c", rows[(int)DicColuna.col1_7.mail]),   //6             <<<<<<<<<<<<<<<<<<<<<<
                    SFDCSchemeBuild.GetNewXmlElement("MainPhone__c", rows[(int)DicColuna.col1_7.telefone1]),   //7             
                    SFDCSchemeBuild.GetNewXmlElement("SecondaryPhone__c", rows[(int)DicColuna.col1_7.telefone2]),   //8             <<<<<<<<<<<<<<<<<<<<<<
                    SFDCSchemeBuild.GetNewXmlElement("AdditionalPhone__c", rows[(int)DicColuna.col1_7.telefone3]),   //9             
                    SFDCSchemeBuild.GetNewXmlElement("CurrencyIsoCode", rows[(int)DicColuna.col1_7.moneda]),   //10             <<<<<<<<<<<<<<<<<<<<<<
                    SFDCSchemeBuild.GetNewXmlElement("RecordTypeId", rows[(int)DicColuna.col1_7.recordType]),   //11             
                    SFDCSchemeBuild.GetNewXmlElement("BirthDate__c", rows[(int)DicColuna.col1_7.fecha_nasc]),   //12             
                    SFDCSchemeBuild.GetNewXmlElement("AccountNumber", rows[(int)DicColuna.col1_7.cuenta_principal]),   //13             
                    SFDCSchemeBuild.GetNewXmlElement("MothersLastName__c", rows[(int)DicColuna.col1_7.apellido_materno]),   //14             <<<<<<<<<<<<<<<<<<<<<<
                    SFDCSchemeBuild.GetNewXmlElement("DireccinDeFacturacion__c", rows[(int)DicColuna.col1_7.id_empresa]+rows[(int)DicColuna.col1_7.cuenta_principal]),   //15             <<<<<<<<<<<<<<<<<<<<<<
                    SFDCSchemeBuild.GetNewXmlElement("Executive__c", rows[(int)DicColuna.col1_7.ejecutivo]),   //16             
                    SFDCSchemeBuild.GetNewXmlElement("Sector__c", rows[(int)DicColuna.col1_7.giro]),   //17             <<<<<<<<<<<<<<<<<<<<<<
                    //SFDCSchemeBuild.GetNewXmlElement("_Clase de servicio", rows[(int)DicColuna.col1_7.clase_servicio_pod]),   //18             <<<<<<<<<<<<<<<<<<<<<<
                    SFDCSchemeBuild.GetNewXmlElement("CompanyID__c>>", rows[(int)DicColuna.col1_7.id_empresa]),   //19             <<<<<<<<<<<<<<<<<<<<<<
                    SFDCSchemeBuild.GetNewXmlElement("CNT_Fantasy_Name__c", rows[(int)DicColuna.col1_7.razao_social]),   //20             
                    SFDCSchemeBuild.GetNewXmlElement("FathersLastName__c", rows[(int)DicColuna.col1_7.apellido_paterno]),   //21        
                    //SFDCSchemeBuild.GetNewXmlElement("_CompanyCategory__c", " "),   //22             
                    //SFDCSchemeBuild.GetNewXmlElement("IsCorporate", " "),   //23   <<<<<<<<<<<<<<<<<<<<<<
                    SFDCSchemeBuild.GetNewXmlElement("Telefono", " "),   //24             
                    SFDCSchemeBuild.GetNewXmlElement("Company__c", (rows[(int)DicColuna.col1_7.id_empresa].Equals("2003") ? "COELCE" : "AMPLA")),   //25             <<<<<<<<<<<<<<<<<<<<<<
                    SFDCSchemeBuild.GetNewXmlElement("Country__c", "BRASIL"),   //26             
                    //SFDCSchemeBuild.GetNewXmlElement("_>>Centro de operación", " "),   //27             <<<<<<<<<<<<<<<<<<<<<<
                    //SFDCSchemeBuild.GetNewXmlElement("_>>Tipo de administrador", " "),   //28             <<<<<<<<<<<<<<<<<<<<<<
                    //SFDCSchemeBuild.GetNewXmlElement("_>>Registration Date", " "),   //29             <<<<<<<<<<<<<<<<<<<<<<
                    //SFDCSchemeBuild.GetNewXmlElement("_>>Regulation of Co-ownership", " "),   //30             
                    //SFDCSchemeBuild.GetNewXmlElement("Industry", " "),   //31             
                    //SFDCSchemeBuild.GetNewXmlElement("_>>IsCondominium?", " "),   //32             <<<<<<<<<<<<<<<<<<<<<<
                    //SFDCSchemeBuild.GetNewXmlElement("_>>Is migrated?", " "),   //33             <<<<<<<<<<<<<<<<<<<<<<
                    //SFDCSchemeBuild.GetNewXmlElement("_>>Fax", " "),   //34             <<<<<<<<<<<<<<<<<<<<<<
                    //SFDCSchemeBuild.GetNewXmlElement("_>>Directory Number", " "),   //35             
                    //SFDCSchemeBuild.GetNewXmlElement("_>>UserTypeCompany", " "),   //36             <<<<<<<<<<<<<<<<<<<<<<
                    SFDCSchemeBuild.GetNewXmlElement("AccountClientNumber__c", rows[(int)DicColuna.col1_7.contacliente]),   //37             <<<<<<<<<<<<<<<<<<<<<<
                    SFDCSchemeBuild.GetNewXmlElement("Executive__c", rows[(int)DicColuna.col1_7.cod_ejecutivo]),   //38             <<<<<<<<<<<<<<<<<<<<<<
                    //SFDCSchemeBuild.GetNewXmlElement("_>>BR Municipality Inscription", " "),   //39             <<<<<<<<<<<<<<<<<<<<<<
                    //SFDCSchemeBuild.GetNewXmlElement("_>>BR State Inscription", " "),   //40             
                    SFDCSchemeBuild.GetNewXmlElement("CNT_Client_Type__c", rows[(int)DicColuna.col1_7.tipo_cliente]),   //41             <<<<<<<<<<<<<<<<<<<<<<
                    //SFDCSchemeBuild.GetNewXmlElement("_>>CNT_Test_Usuario", " "),   //42             
                    //SFDCSchemeBuild.GetNewXmlElement("_>>Fantasy Name", " "),   //43       <<<<<<<<<<<<<<<<<<<<<<      
                    //SFDCSchemeBuild.GetNewXmlElement("CNT_Fathers_Full_Name__c", " "),   //44             
                    //SFDCSchemeBuild.GetNewXmlElement("_>>Identity Number 2", " "),   //45     <<<<<<<<<<<<<<<<<<<<<<        
                    //SFDCSchemeBuild.GetNewXmlElement("_Identity Type 2", " "),   //46             
                    //SFDCSchemeBuild.GetNewXmlElement("CNT_Mothers_Full_Name__c", " "),   //47             
                    SFDCSchemeBuild.GetNewXmlElement("CNT_Responsible_Email__c", rows[(int)DicColuna.col1_7.emailresponsavel].Split('|')[0]),   //48             
                    SFDCSchemeBuild.GetNewXmlElement("_Responsible Identity Number", rows[(int)DicColuna.col1_7.documentoresponsavel]),   //49             
                    SFDCSchemeBuild.GetNewXmlElement("_>>Responsible Identity Type", rows[(int)DicColuna.col1_7.tipoDocumentoResponsavel]),   //50    <<<<<<<<<<<         
                    SFDCSchemeBuild.GetNewXmlElement("CNT_Responsible_Name__c", rows[(int)DicColuna.col1_7.nomeresponsavel]),   //51             
                    SFDCSchemeBuild.GetNewXmlElement("CNT_Responsible_Phone__c", rows[(int)DicColuna.col1_7.telefone1Responsavel])   //52             
                    //SFDCSchemeBuild.GetNewXmlElement("CNT_Account_Type__c", " ")   //53
                };
            }
            catch { }

            lstObjetos.Add(sfObj);
            UpsertResult[] saveResults = SalesforceDAO.Upsert("ExternalId__c", lstObjetos, ref binding);
            UpsertResult resultado = saveResults.First();
            if (resultado != null && resultado.errors != null && resultado.errors.Count() > 0)
            {
                throw new Exception(string.Join(";", resultado.errors.Select(e => e.message).ToArray()));
            }

            return resultado.id;
        }


        [ComVisible(true)]
        public string IngressarAsset(string[] rows)
        {
            if (!loggedIn)
            {
                Autenticacao auth = new Autenticacao(this.ambiente);
                if (!auth.ValidarLogin(ref this.binding))
                    return string.Empty;

                this.loggedIn = true;
            }

            List<sObject> lstObjetos = new List<sObject>();
            sObject sfObj = null;

            //ExternalId__c; Name; AccountId; ContactId; PointofDelivery__c; Description; Product2Id; Status; CurrencyIsoCode; InstallDate; Company__c; Country__c; PurchaseDate; RecordTypeId; salesforce_id; salesforce_created

            sfObj = new sObject();
            sfObj.type = "Asset";

            sfObj.Any = new System.Xml.XmlElement[] {
                SFDCSchemeBuild.GetNewXmlElement("ExternalId__c", rows[(int)DicColuna.col1_7.identificador_asset]),
                SFDCSchemeBuild.GetNewXmlElement("Name", rows[(int)DicColuna.col1_7.nombre_del_activo]),
                SFDCSchemeBuild.GetNewXmlElement("AccountId", rows[(int)DicColuna.col1_7.identificador_conta_asset]),
                SFDCSchemeBuild.GetNewXmlElement("ContactId", rows[(int)DicColuna.col1_7.identificador_contacto]),
                SFDCSchemeBuild.GetNewXmlElement("PointofDelivery__c", rows[(int)DicColuna.col1_7.suministro]),
                SFDCSchemeBuild.GetNewXmlElement("Description", rows[(int)DicColuna.col1_7.descripcion]),
                SFDCSchemeBuild.GetNewXmlElement("Product2Id", rows[(int)DicColuna.col1_7.producto]),
                SFDCSchemeBuild.GetNewXmlElement("Status", rows[(int)DicColuna.col1_7.estado]),
                //SFDCSchemeBuild.GetNewXmlElement("_Tipo Contrato", rows[(int)DicColuna.col1_7.contrato]),
                //SFDCSchemeBuild.GetNewXmlElement("CurrencyIsoCode", " "),
                //SFDCSchemeBuild.GetNewXmlElement("_Familia de productos", rows[(int)DicColuna.col1_7.moneda]),
                SFDCSchemeBuild.GetNewXmlElement("InstallDate", rows[(int)DicColuna.col1_7.fecha_conexion]),
                SFDCSchemeBuild.GetNewXmlElement("Company__c", rows[(int)DicColuna.col1_7.id_empresa].Equals("2003") ? "COELCE" : "AMPLA"),
                SFDCSchemeBuild.GetNewXmlElement("Country__c", "BRASIL")
                //SFDCSchemeBuild.GetNewXmlElement("PurchaseDate", " "),
                //SFDCSchemeBuild.GetNewXmlElement("RecordTypeId", " ")
            };

            lstObjetos.Add(sfObj);
            SaveResult[] saveResults = SalesforceDAO.InserirObjetosSF(lstObjetos, ref binding);
            SaveResult resultado = saveResults.First();
            if (resultado != null && resultado.errors != null && resultado.errors.Count() > 0)
            {
                throw new Exception(string.Join(";", resultado.errors.Select(e => e.message).ToArray()));
            }

            return resultado.id;
        }


        [ComVisible(true)]
        public string IngressarBilling(string[] rows)
        {
            return "";
            //if (!loggedIn)
            //{
            //    Autenticacao auth = new Autenticacao(this.ambiente);
            //    if (!auth.ValidarLogin(ref this.binding))
            //        return string.Empty;

            //    this.loggedIn = true;
            //}

            //List<sObject> lstObjetos = new List<sObject>();
            //sObject sfObj = null;

            ////"Cuenta"	"Tipo de Billing Profile"	"Agrees to Terms"	"Ballot Name"	"Bank"	"BillingAddress"	"Change Invoice Address Restrict. Reason"	"Change Invoice Address Restriction"	"Change Invoice Address Restriction Date"	"Change Name Restriction"	"Change Name Restriction Date"	"Change Name Restriction Reason"	"Current Account"	"Current Account Number"	"Document Type"	"Document Type Change Restriction"	"Document Type Change Restriction Date"	"Document Type Change Restriction Reason"	"EDE Enrolment"	"External ID"	"External ID Suministro"	"CuentaContrato"	"Delivery Type"	"Account Number"	"Account Type"	"Bank"	"Braile?"	"Brand"	"Credit Card"	"Credit Card Due Date"	"Credit Card Name"	"Credit Card Number"	"Credit Card Security Code"	"Digital Billing"	"Distribution Type"	"Due Date"	"Company"
            ////Account__r:Account:ExternalId__c	Type__c	_Agrees to Terms	Account__r:Account:ExternalId__c	BankBR__c	Address__r:Address__c:ExternalId__c	_Change Invoice Address Restrict. Reason	_Change Invoice Address Restriction	_Change Invoice Address Restriction Date	_Change Name Restriction	_Change Name Restriction Date	_Change Name Restriction Reason	CurrentAccountNumber__c	CurrentAccountNum__c	IdentityType__c	_Document Type Change Restriction	_Document Type Change Restriction Date	_Document Type Change Restriction Reason	_EDE Enrolment	ExternalID__c	PointofDelivery__r:PointofDelivery__c:ExternalId__c	AccountContract__c	_DeliveryType__c ????????????	CNT_Account_Number__c	_Account Type	BankBR__c	_CNT_Braile__c	_CNT_Brand__c	_Credit Card	_Credit Card Due Date	_Credit Card Name	_Credit Card Number	_Credit Card Security Code	_Digital Billing	CNT_Distribution_Type__c	CNT_Due_Date__c	Company__c	__Status	__Id	__Action	__Errors
            ////Account__c;Type__c;AgreestoTerms__c;BallotName__c;Bank__c;BillingAddress__c;
            ////ChangeInvoiceAddressRestrictReason__c;ChangeInvoiceAddressRestriction__c;
            ////ChangeInvoiceAddressRestrictionDate__c;ChangeNameRestriction__c;ChangeNameRestrictionDate__c;
            ////ChangeNameRestrictionReason__c;CurrentAccount__c;CurrentAccountNum__c;DocumentType__c;
            ////DocumentTypeChangeRestriction__c;DocumentTypeChangeRestrictionDate__c;DocumentTypeChangeRestrictionReason__c;
            ////EDEEnrolment__c;ExternalId__c;PointofDelivery__c;AccountContract__c;Delivery_Type__c;CNT_Account_Number__c;
            ////CNT_Account_Type__c;CNT_Bank__c;CNT_Braile__c;CNT_Brand__c;CNT_Credit_Card__c;CNT_Credit_Card_Due_Date__c;CNT_Credit_Card_Name__c;
            ////CNT_Credit_Card_Number__c;CNT_Credit_Card_Security_Code__c;CNT_Digital_Billing__c;CNT_Distribution_Type__c;CNT_Due_Date__c;Company__c;RecordTypeId;


            //sfObj = new sObject();
            //sfObj.type = "Billing_Profile__c";

            //sfObj.Any = new System.Xml.XmlElement[] {
            //SFDCSchemeBuild.GetNewXmlElement("Account__r:Account:ExternalId__c", rows[(int)DicColuna.col1_7.identificador_asset]),
            //SFDCSchemeBuild.GetNewXmlElement("Type__c", rows[(int)DicColuna.col1_7.nombre_del_activo]),
            ////SFDCSchemeBuild.GetNewXmlElement("AgreestoTerms__c", rows[(int)DicColuna.col1_7.identificador_conta_asset]),
            //SFDCSchemeBuild.GetNewXmlElement("BallotName__c", rows[(int)DicColuna.col1_7.identificador_contacto]),
            //SFDCSchemeBuild.GetNewXmlElement("Bank__c", rows[(int)DicColuna.col1_7.suministro]),
            //SFDCSchemeBuild.GetNewXmlElement("BillingAddress__c", rows[(int)DicColuna.col1_7.descripcion]),
            ////SFDCSchemeBuild.GetNewXmlElement("ChangeInvoiceAddressRestrictReason__c", rows[(int)DicColuna.col1_7.producto]),
            ////SFDCSchemeBuild.GetNewXmlElement("ChangeInvoiceAddressRestriction__c", rows[(int)DicColuna.col1_7.estado]),
            ////SFDCSchemeBuild.GetNewXmlElement("ChangeInvoiceAddressRestrictionDate__c", rows[(int)DicColuna.col1_7.contrato]),
            ////SFDCSchemeBuild.GetNewXmlElement("ChangeNameRestriction__c", " "),
            ////SFDCSchemeBuild.GetNewXmlElement("ChangeNameRestrictionDate__c", rows[(int)DicColuna.col1_7.moneda]),
            ////SFDCSchemeBuild.GetNewXmlElement("ChangeNameRestrictionReason__c", rows[(int)DicColuna.col1_7.fecha_conexion]),
            //SFDCSchemeBuild.GetNewXmlElement("CurrentAccount__c", rows[(int)DicColuna.col1_7.id_empresa].Equals("2003") ? "COELCE" : "AMPLA"),
            //SFDCSchemeBuild.GetNewXmlElement("CurrentAccountNum__c", "BRASIL"),
            //SFDCSchemeBuild.GetNewXmlElement("DocumentType__c", " "),
            ////SFDCSchemeBuild.GetNewXmlElement("DocumentTypeChangeRestriction__c", " "),
            ////SFDCSchemeBuild.GetNewXmlElement("DocumentTypeChangeRestrictionDate__c", rows[(int)DicColuna.col1_7.identificador_asset]),
            ////SFDCSchemeBuild.GetNewXmlElement("DocumentType__c", rows[(int)DicColuna.col1_7.nombre_del_activo]),
            ////SFDCSchemeBuild.GetNewXmlElement("DocumentTypeChangeRestrictionReason__c", rows[(int)DicColuna.col1_7.identificador_conta_asset]),
            ////SFDCSchemeBuild.GetNewXmlElement("EDEEnrolment__c", rows[(int)DicColuna.col1_7.identificador_contacto]),
            //SFDCSchemeBuild.GetNewXmlElement("ExternalID__c", rows[(int)DicColuna.col1_7.suministro]),
            //SFDCSchemeBuild.GetNewXmlElement("PointofDelivery__c", rows[(int)DicColuna.col1_7.descripcion]),
            //SFDCSchemeBuild.GetNewXmlElement("AccountContract__c", rows[(int)DicColuna.col1_7.producto]),
            //SFDCSchemeBuild.GetNewXmlElement("Delivery_Type__c", rows[(int)DicColuna.col1_7.estado]),   // ????????????
            //SFDCSchemeBuild.GetNewXmlElement("CNT_Account_Number__c", rows[(int)DicColuna.col1_7.contrato]),
            //SFDCSchemeBuild.GetNewXmlElement("CNT_Account_Type__c", rows[(int)DicColuna.col1_7.estado]),   // ????????????
            ////SFDCSchemeBuild.GetNewXmlElement("CNT_Bank__c", " "),
            ////SFDCSchemeBuild.GetNewXmlElement("CNT_Braile__c", rows[(int)DicColuna.col1_7.moneda]),
            ////SFDCSchemeBuild.GetNewXmlElement("CNT_Brand__c", rows[(int)DicColuna.col1_7.fecha_conexion]),
            ////SFDCSchemeBuild.GetNewXmlElement("CNT_Credit_Card__c", rows[(int)DicColuna.col1_7.id_empresa].Equals("2003") ? "COELCE" : "AMPLA"),
            ////SFDCSchemeBuild.GetNewXmlElement("CNT_Credit_Card_Due_Date__c", "BRASIL"),
            ////SFDCSchemeBuild.GetNewXmlElement("CNT_Credit_Card_Name__c", " "),
            ////SFDCSchemeBuild.GetNewXmlElement("CNT_Credit_Card_Number__c", " "),
            ////SFDCSchemeBuild.GetNewXmlElement("CNT_Credit_Card_Security_Code__c", rows[(int)DicColuna.col1_7.fecha_conexion]),
            ////SFDCSchemeBuild.GetNewXmlElement("CNT_Digital_Billing__c", ,
            ////SFDCSchemeBuild.GetNewXmlElement("CNT_Distribution_Type__c", " "),
            ////SFDCSchemeBuild.GetNewXmlElement("CNT_Due_Date__c", " "),
            //SFDCSchemeBuild.GetNewXmlElement("Company__c", rows[(int)DicColuna.col1_7.id_empresa].Equals("2003") ? "COELCE" : "AMPLA")
            ////SFDCSchemeBuild.GetNewXmlElement("RecordTypeId", " ")
            //};

            //lstObjetos.Add(sfObj);
            //SaveResult[] saveResults = SalesforceDAO.InserirObjetosSF(lstObjetos, ref binding);
            //SaveResult resultado = saveResults.First();
            //if (resultado != null && resultado.errors != null && resultado.errors.Count() > 0)
            //{
            //    throw new Exception(string.Join(";", resultado.errors.Select(e => e.message).ToArray()));
            //}

            //return resultado.id;
        }


        [ComVisible(true)]
        public string IngressarContract(string[] rows)
        {
            autenticar();

            List<sObject> lstObjetos = new List<sObject>();
            sObject sfObj = null;

            sfObj = new sObject();
            sfObj.type = "Contract";

            sfObj.Any = new System.Xml.XmlElement[] {
                SFDCSchemeBuild.GetNewXmlElement("ExternalID__c", rows[(int)DicColuna.col1_7.identificador_conta]),
                SFDCSchemeBuild.GetNewXmlElement("Name", rows[(int)DicColuna.col1_7.cuenta_principal]),
                SFDCSchemeBuild.GetNewXmlElement("AccountId", rows[(int)DicColuna.col1_7.identificador_conta]),
                //SFDCSchemeBuild.GetNewXmlElement("CustomerSignedId", " "),
                SFDCSchemeBuild.GetNewXmlElement("Description", "Carga Massiva"),
                SFDCSchemeBuild.GetNewXmlElement("Status", "Draft"),
                SFDCSchemeBuild.GetNewXmlElement("StartDate", rows[(int)DicColuna.col1_7.fecha_conexion]),
                SFDCSchemeBuild.GetNewXmlElement("Contract_Type__c", rows[(int)DicColuna.col1_7.contrato]),
                SFDCSchemeBuild.GetNewXmlElement("CurrencyIsoCode", rows[(int)DicColuna.col1_7.moneda]),
                //SFDCSchemeBuild.GetNewXmlElement("ContractTerm", " "),
                SFDCSchemeBuild.GetNewXmlElement("Company_ID__c", rows[(int)DicColuna.col1_7.id_empresa].Equals("2003") ? "COELCE" : "AMPLA"),
                SFDCSchemeBuild.GetNewXmlElement("RecordTypeId", "01236000000yFs3AAE")
            };

            lstObjetos.Add(sfObj);
            SaveResult[] saveResults = SalesforceDAO.InserirObjetosSF(lstObjetos, ref binding);
            SaveResult resultado = saveResults.First();
            if (resultado != null && resultado.errors != null && resultado.errors.Count() > 0)
            {
                throw new Exception(string.Join(";", resultado.errors.Select(e => e.message).ToArray()));
            }

            return resultado.id;
        }


        [ComVisible(true)]
        public string IngressarContract(ref ContractSalesforce contrato)
        {
            autenticar();

            List<sObject> lstObjetos = new List<sObject>();
            sObject sfObj = null;

            sfObj = new sObject();
            sfObj.type = "Contract";

            sfObj.Any = new System.Xml.XmlElement[] {
                SFDCSchemeBuild.GetNewXmlElement("ExternalID__c", contrato.ExternalId),
                //SFDCSchemeBuild.GetNewXmlElement("Name", contrato.Name),
                SFDCSchemeBuild.GetNewXmlElement("AccountId", contrato.Account),
                //SFDCSchemeBuild.GetNewXmlElement("CustomerSignedId", " "),
                //SFDCSchemeBuild.GetNewXmlElement("Description", "Carga Massiva"),
                SFDCSchemeBuild.GetNewXmlElement("Status", "Draft"),
                SFDCSchemeBuild.GetNewXmlElement("StartDate", contrato.DataInicio),
                SFDCSchemeBuild.GetNewXmlElement("Contract_Type__c", contrato.ContractType),
                //SFDCSchemeBuild.GetNewXmlElement("CurrencyIsoCode", contrato.),
                //SFDCSchemeBuild.GetNewXmlElement("ContractTerm", " "),
                SFDCSchemeBuild.GetNewXmlElement("Company_ID__c", this.codigoEmpresa.Equals("2003") ? "COELCE" : "AMPLA"),
                //SFDCSchemeBuild.GetNewXmlElement("RecordTypeId", contrato.RecordTypeId),
                SFDCSchemeBuild.GetNewXmlElement("CNT_Case__c", contrato.CNT_Case__c),
                SFDCSchemeBuild.GetNewXmlElement("CNT_Economical_Activity__c", contrato.CNAE),                  //ok
                SFDCSchemeBuild.GetNewXmlElement("CNT_Quote__c", contrato.CNT_Quote__c)                  //ok
            };

            lstObjetos.Add(sfObj);
            SaveResult[] saveResults = SalesforceDAO.InserirObjetosSF(lstObjetos, ref binding);
            SaveResult resultado = saveResults.First();
            if (resultado != null && resultado.errors != null && resultado.errors.Count() > 0)
            {
                throw new Exception(string.Join(";", resultado.errors.Select(e => e.message).ToArray()));
            }
            contrato.Id = resultado.id;
            return resultado.id;
        }


        public string IngressarContaControladora(AccountSalesforce rows)
        {
            if (!loggedIn)
            {
                Autenticacao auth = new Autenticacao(this.ambiente);
                if (!auth.ValidarLogin(ref this.binding))
                    return string.Empty;

                this.loggedIn = true;
            }

            List<sObject> lstObjetos = new List<sObject>();
            StringBuilder auxRecordType = new StringBuilder();
            sObject sfObj = new sObject();
            sfObj.type = "Account";

            sfObj.Any = new System.Xml.XmlElement[] {
                SFDCSchemeBuild.GetNewXmlElement("RecordTypeId", "0121o000000oWhZAAU"),
                SFDCSchemeBuild.GetNewXmlElement("ExternalID__c", rows.ExternalId),
                SFDCSchemeBuild.GetNewXmlElement("CondominiumType__c", rows.TipoCondominio),
                SFDCSchemeBuild.GetNewXmlElement("ParentId", rows.ParentId),
                SFDCSchemeBuild.GetNewXmlElement("CondominiumRUT__c", rows.CondominiumRUT__c),
                SFDCSchemeBuild.GetNewXmlElement("Name", rows.Nome)
            };

            lstObjetos.Add(sfObj);
            SaveResult[] saveResults = SalesforceDAO.InserirObjetosSF(lstObjetos, ref binding);
            SaveResult resultado = saveResults.First();
            if (resultado != null && resultado.errors != null && resultado.errors.Count() > 0)
            {
                throw new Exception(string.Join(";", resultado.errors.Select(e => e.message).ToArray()));
            }
            return resultado.id;
        }


        public string IngressarContratoControladora(ContractSalesforce p)
        {
            autenticar();

            List<sObject> lstObjetos = new List<sObject>();
            StringBuilder auxRecordType = new StringBuilder();
            sObject sfObj = new sObject();
            sfObj.type = "Contract";

            sfObj.Any = new System.Xml.XmlElement[] {
                SFDCSchemeBuild.GetNewXmlElement("AccountId", p.Account),
                SFDCSchemeBuild.GetNewXmlElement("StartDate", p.DataInicio),
                SFDCSchemeBuild.GetNewXmlElement("CNT_GroupTypeContract__c", p.TipoAgrupamento),
                SFDCSchemeBuild.GetNewXmlElement("CNT_GroupSegment__c", p.SegmentoAgrupamento),
                SFDCSchemeBuild.GetNewXmlElement("CNT_GroupNumerCntr__c", p.NumeroAgrupamento),
                SFDCSchemeBuild.GetNewXmlElement("CNT_GroupArea__c", p.AreaAgrupamento),
                SFDCSchemeBuild.GetNewXmlElement("ExternalId__c", p.ExternalId),
                SFDCSchemeBuild.GetNewXmlElement("Status", p.Status)
            };

            lstObjetos.Add(sfObj);
            SaveResult[] saveResults = SalesforceDAO.InserirObjetosSF(lstObjetos, ref binding);
            SaveResult resultado = saveResults.First();
            if (resultado != null && resultado.errors != null && resultado.errors.Count() > 0)
            {
                throw new Exception(string.Join(";", resultado.errors.Select(e => e.message).ToArray()));
            }
            return resultado.id;
        }


        public string IngressarBillingProfileControladora(BillingSalesforce p)
        {
            if (!loggedIn)
            {
                Autenticacao auth = new Autenticacao(this.ambiente);
                if (!auth.ValidarLogin(ref this.binding))
                    return string.Empty;

                this.loggedIn = true;
            }

            List<sObject> lstObjetos = new List<sObject>();
            StringBuilder auxRecordType = new StringBuilder();
            sObject sfObj = new sObject();
            sfObj.type = "Billing_Profile__c";

            sfObj.Any = new System.Xml.XmlElement[] {
                SFDCSchemeBuild.GetNewXmlElement("CNT_GroupPayType__c", p.CNT_GroupPayType__c),     //ok
                SFDCSchemeBuild.GetNewXmlElement("CNT_Lot__c", p.CNT_Lot__c),                       //ok
                SFDCSchemeBuild.GetNewXmlElement("CNT_GroupClass__c", p.CNT_GroupClass__c),         //ok
                SFDCSchemeBuild.GetNewXmlElement("CNT_Due_Date__c", p.CNT_Due_Date__c),             //ok
                SFDCSchemeBuild.GetNewXmlElement("Type__c", p.Type__c),                             //ok
                SFDCSchemeBuild.GetNewXmlElement("Address__c", p.Address__c),
                SFDCSchemeBuild.GetNewXmlElement("BillingAddress__c", p.BillingAddress__c),
                SFDCSchemeBuild.GetNewXmlElement("Account__c", p.Account__c),                       //ok
                SFDCSchemeBuild.GetNewXmlElement("RecordTypeId", p.RecordTypeId),                   //ok
                SFDCSchemeBuild.GetNewXmlElement("CNT_Contract__c", p.CNT_Contract__c),             //ok
                SFDCSchemeBuild.GetNewXmlElement("AccountContract__c", p.AccountContract__c),
                SFDCSchemeBuild.GetNewXmlElement("Company__c", p.Company__c),                       //ok
                SFDCSchemeBuild.GetNewXmlElement("ExternalID__c", p.ExternalID__c),                  //ok
                SFDCSchemeBuild.GetNewXmlElement("PointofDelivery__c", p.PoDSF)                  //ok
            };

            lstObjetos.Add(sfObj);
            SaveResult[] saveResults = SalesforceDAO.InserirObjetosSF(lstObjetos, ref binding);
            SaveResult resultado = saveResults.First();
            if (resultado != null && resultado.errors != null && resultado.errors.Count() > 0)
            {
                throw new Exception(string.Join(";", resultado.errors.Select(e => e.message).ToArray()));
            }
            return resultado.id;
        }


        public string IngressarContractLineControladora(ref ContractLineItemSalesforce p)
        {
            if (!loggedIn)
            {
                Autenticacao auth = new Autenticacao(this.ambiente);
                if (!auth.ValidarLogin(ref this.binding))
                    return string.Empty;

                this.loggedIn = true;
            }

            List<sObject> lstObjetos = new List<sObject>();
            StringBuilder auxRecordType = new StringBuilder();
            sObject sfObj = new sObject();
            sfObj.type = "Contract_Line_Item__c";

            sfObj.Any = new System.Xml.XmlElement[] {
                SFDCSchemeBuild.GetNewXmlElement("Company__c", this.codigoEmpresa),
                SFDCSchemeBuild.GetNewXmlElement("Contract__c", p.ContractId),
                SFDCSchemeBuild.GetNewXmlElement("CNT_Status__c", p.CNT_Status__c),
                SFDCSchemeBuild.GetNewXmlElement("Billing_Profile__c", p.BillingProfile__c)
            };

            lstObjetos.Add(sfObj);
            SaveResult[] saveResults = SalesforceDAO.InserirObjetosSF(lstObjetos, ref binding);
            SaveResult resultado = saveResults.First();
            if (resultado != null && resultado.errors != null && resultado.errors.Count() > 0)
            {
                throw new Exception(string.Join(";", resultado.errors.Select(e => e.message).ToArray()));
            }
            return resultado.id;
        }



        [ComVisible(true)]
        public string IngressarContractLine(string[] rows)
        {
            if (!loggedIn)
            {
                Autenticacao auth = new Autenticacao(this.ambiente);
                if (!auth.ValidarLogin(ref this.binding))
                    return string.Empty;

                this.loggedIn = true;
            }

            List<sObject> lstObjetos = new List<sObject>();
            sObject sfObj = null;

            sfObj = new sObject();
            sfObj.type = "Billing_Profile__c";

            string emp = rows[(int)DicColuna.col1_7.id_empresa] == "2005" ? "AMA" : "COE";
            string identificadorAssetEnergia = rows[(int)DicColuna.col1_7.cuenta_principal] +
                        rows[(int)DicColuna.col1_7.producto] + "BRA" + emp + rows[(int)DicColuna.col1_7.contrato];

            //ExternalID__c;Contract__c;Asset__c;Billing_Profile__c;Quantity__c;CurrencyIsoCode;Status__c;HasApportionment__c;
            //EDERestriction__c;EDERestrictionDate__c;EDERestrictionReason__c;AccountContract__c;CNT_Status__c;Company__c;RecordTypeId;

            sfObj.Any = new System.Xml.XmlElement[] {
                SFDCSchemeBuild.GetNewXmlElement("ExternalID__c", rows[(int)DicColuna.col1_7.identificador_conta] + identificadorAssetEnergia),
                //SFDCSchemeBuild.GetNewXmlElement("Nombre del contracto", ),
                SFDCSchemeBuild.GetNewXmlElement("Contract__c", rows[(int)DicColuna.col1_7.identificador_conta] + rows[(int)DicColuna.col1_7.cuenta_principal]),
                SFDCSchemeBuild.GetNewXmlElement("Asset__c", identificadorAssetEnergia),
                SFDCSchemeBuild.GetNewXmlElement("Billing_Profile__c", rows[(int)DicColuna.col1_7.identificador_conta] + rows[(int)DicColuna.col1_7.cuenta_principal]),
                //SFDCSchemeBuild.GetNewXmlElement("Descripcion", " "),
                SFDCSchemeBuild.GetNewXmlElement("Quantity__c", " "),
                SFDCSchemeBuild.GetNewXmlElement("CurrencyIsoCode", rows[(int)DicColuna.col1_7.moneda]),
                SFDCSchemeBuild.GetNewXmlElement("Status__c", " "),
                SFDCSchemeBuild.GetNewXmlElement("HasApportionment__c", " "),
                //SFDCSchemeBuild.GetNewXmlElement("", rows[(int)DicColuna.col1_7.]),
                //SFDCSchemeBuild.GetNewXmlElement("", rows[(int)DicColuna.col1_7.]),
                //SFDCSchemeBuild.GetNewXmlElement("", ),
                //SFDCSchemeBuild.GetNewXmlElement("", ),
                SFDCSchemeBuild.GetNewXmlElement("EDERestrictionReason__c", " "),
                SFDCSchemeBuild.GetNewXmlElement("EDERestrictionDate__c", " "),
                SFDCSchemeBuild.GetNewXmlElement("EDE Restriction Reason", " "),
                SFDCSchemeBuild.GetNewXmlElement("AccountContract", rows[(int)DicColuna.col1_7.cuenta_principal]),
                //SFDCSchemeBuild.GetNewXmlElement("Configuration Item", rows[(int)DicColuna.col1_7.]),
                //SFDCSchemeBuild.GetNewXmlElement("Product", " "),
                SFDCSchemeBuild.GetNewXmlElement("Status", " "),
                SFDCSchemeBuild.GetNewXmlElement("Company", rows[(int)DicColuna.col1_7.id_empresa_pod])
            };

            lstObjetos.Add(sfObj);
            SaveResult[] saveResults = SalesforceDAO.InserirObjetosSF(lstObjetos, ref binding);
            SaveResult resultado = saveResults.First();
            if (resultado != null && resultado.errors != null && resultado.errors.Count() > 0)
            {
                throw new Exception(string.Join(";", resultado.errors.Select(e => e.message).ToArray()));
            }

            return resultado.id;
        }


        [ComVisible(true)]
        public string IngressarDevice(string[] rows)
        {
            if (!loggedIn)
            {
                Autenticacao auth = new Autenticacao(this.ambiente);
                if (!auth.ValidarLogin(ref this.binding))
                    return string.Empty;

                this.loggedIn = true;
            }

            List<sObject> lstObjetos = new List<sObject>();
            sObject sfObj = null;

            sfObj = new sObject();
            sfObj.type = "Device__c";

            string emp = rows[(int)DicColuna.col1_7.id_empresa] == "2005" ? "AMA" : "COE";
            string identificadorAssetEnergia = rows[(int)DicColuna.col1_7.cuenta_principal] +
                        rows[(int)DicColuna.col1_7.producto] + "BRA" + emp + rows[(int)DicColuna.col1_7.contrato];

            //MeterBrand__c;MeterModel__c;MeterNumber__c;MeterProperty__c;MeterType__c;
            //PointofDelivery__c;ExternalID__c;Instalation_date__c;MeasureType__c;Retirement_date__c;
            //Status__c;ConstanteDEM__c;ConstantePRODIA__c;ConstantePROANT__c;ConstanteATIVAHP__c;
            //ConstanteDMCRHP__c;CreatedYear__c;

            sfObj.Any = new System.Xml.XmlElement[] {
                SFDCSchemeBuild.GetNewXmlElement("ExternalID__c", rows[(int)DicColuna.col1_7.identificador_conta] + identificadorAssetEnergia),
                //SFDCSchemeBuild.GetNewXmlElement("Nombre del contracto", ),
                SFDCSchemeBuild.GetNewXmlElement("Contract__c", rows[(int)DicColuna.col1_7.identificador_conta] + rows[(int)DicColuna.col1_7.cuenta_principal]),
                SFDCSchemeBuild.GetNewXmlElement("Asset__c", identificadorAssetEnergia),
                SFDCSchemeBuild.GetNewXmlElement("Billing_Profile__c", rows[(int)DicColuna.col1_7.identificador_conta] + rows[(int)DicColuna.col1_7.cuenta_principal]),
                //SFDCSchemeBuild.GetNewXmlElement("Descripcion", " "),
                SFDCSchemeBuild.GetNewXmlElement("Quantity__c", " "),
                SFDCSchemeBuild.GetNewXmlElement("CurrencyIsoCode", rows[(int)DicColuna.col1_7.moneda]),
                SFDCSchemeBuild.GetNewXmlElement("Status__c", " "),
                SFDCSchemeBuild.GetNewXmlElement("HasApportionment__c", " "),
                //SFDCSchemeBuild.GetNewXmlElement("", rows[(int)DicColuna.col1_7.]),
                //SFDCSchemeBuild.GetNewXmlElement("", rows[(int)DicColuna.col1_7.]),
                //SFDCSchemeBuild.GetNewXmlElement("", ),
                //SFDCSchemeBuild.GetNewXmlElement("", ),
                SFDCSchemeBuild.GetNewXmlElement("EDERestrictionReason__c", " "),
                SFDCSchemeBuild.GetNewXmlElement("EDERestrictionDate__c", " "),
                SFDCSchemeBuild.GetNewXmlElement("EDE Restriction Reason", " "),
                SFDCSchemeBuild.GetNewXmlElement("AccountContract", rows[(int)DicColuna.col1_7.cuenta_principal]),
                //SFDCSchemeBuild.GetNewXmlElement("Configuration Item", rows[(int)DicColuna.col1_7.]),
                //SFDCSchemeBuild.GetNewXmlElement("Product", " "),
                SFDCSchemeBuild.GetNewXmlElement("Status", " "),
                SFDCSchemeBuild.GetNewXmlElement("Company", rows[(int)DicColuna.col1_7.id_empresa_pod])
            };

            lstObjetos.Add(sfObj);
            SaveResult[] saveResults = SalesforceDAO.InserirObjetosSF(lstObjetos, ref binding);
            SaveResult resultado = saveResults.First();
            if (resultado != null && resultado.errors != null && resultado.errors.Count() > 0)
            {
                throw new Exception(string.Join(";", resultado.errors.Select(e => e.message).ToArray()));
            }

            return resultado.id;
        }

        
        /// <summary>
        /// OBJETO:     NE__Order__c
        /// CEBECALHO:  NE__AccountId__r:Account:ExternalId__c  NE__Type__c NE__CatalogId__c    NE__COMMERCIALMODELID__C    NE__BillAccId__r:Account:ExternalId__c  NE__ServAccId__r:Account:ExternalId__c  NE__OrderStatus__c  Country__c  CurrencyIsoCode NE__ConfigurationStatus__c  ExternalId_Pod  ExternalId_Asset    ExternalId_Contract
        /// </summary>
        /// <param name="asset"></param>
        /// <param name="conteudoArquivo"></param>
        [ComVisible(true)]
        public void ProcessarIngressoOrder(ItemAttribute asset, ref StringBuilder conteudoArquivo)
        {
            OrderSalesforce novoOrder = new OrderSalesforce();
            try
            {
                conteudoArquivo.AppendLine(string.Format("{0}\t{1}\t{2}\t{3}\t{4}\t{5}\t{6}\t{7}\t{8}\t{9}\t{10}\t{11}\t{12}"
                    , asset.ExternalIdAccount
                    , "InOrder"
                    , "a101o00000EBAXWAA5"
                    , "a141o00000GhwG3AAJ"
                    , asset.ExternalIdAccount
                    , asset.ExternalIdAccount
                    , "Pending"
                    , "BRASIL"
                    , "BRL"
                    , "Valid"
                    , asset.ExternalIdPod
                    , asset.ExternalIdAsset
                    , asset.ExternalIdContract));
            }
            catch (Exception ex)
            {
                Debugger.Break();
                //TODO: tratar exceção
                Console.WriteLine(string.Format("\nErro ao ingressar Order para o Asset: {0}: {1} {2}\n", asset.ExternalIdAccount, ex.Message, ex.StackTrace));
                //throw ex;
            }
        }

        /// <summary>
        /// CABECALHO:  NE__OrderId__c	NE__Action__c	NE__Qty__c	NE__CatalogItem__c	NE__ProdId__c	NE__Account__r:Account:ExternalId__c	NE__Billing_Account__r:Account:ExternalId__c	NE__CATALOG__C	NE__Service_Account__r:Account:ExternalId__c	NE__STATUS__C	NE__ASSETITEMENTERPRISEID__C	NE__BASEONETIMEFEE__C	NE__BASERECURRINGCHARGE__C	NE__ONETIMEFEEOV__C	NE__RECURRINGCHARGEFREQUENCY__C	NE__RECURRINGCHARGEOV__C	NE__Country__c	CurrencyIsoCode
        /// </summary>
        /// <param name="order"></param>
        /// <param name="conteudoArquivo"></param>
        [ComVisible(true)]
        public void ProcessarIngressoOrderItem(OrderSalesforce order, ref StringBuilder conteudoArquivo)
        {
            OrderItemSalesforce orderItem = new OrderItemSalesforce();
            try
            {
                //

                conteudoArquivo.AppendLine(string.Format("{0}\t{1}\t{2}\t{3}\t{4}\t{5}\t{6}\t{7}\t{8}\t{9}\t{10}\t{11}\t{12}\t{13}\t{14}\t{15}\t{16}\t{17}"
                    , order.Id
                    , orderItem.Action
                    , orderItem.Qty
                    , "a0z1o000003z2FOAAY"
                    , "a1f1o00000bsF6tAAE"
                    , order.AccountId
                    , order.AccountId
                    , "a101o00000EBAXWAA5"
                    , order.AccountId
                    , orderItem.Status
                    , order.ExternalId
                    , orderItem.BaseOneTimeFee
                    , orderItem.BaseRecurringCharge
                    , orderItem.OneTimeFeeOv
                    , orderItem.RecurringChargeFrequency
                    , orderItem.RecurringChargeOv
                    , "BRASIL"
                    , "BRL"));
            }
            catch (Exception ex)
            {
                Console.WriteLine(string.Format("\nErro ao processar Item Order para o Order {0} e Asset: {1} {2} {3}\n", order.Id, ex.Message, ex.StackTrace));
                Debugger.Break();
                //TODO: tratar exceção
                //throw new Exception(string.Format("Erro ao criar o Order Item para o Order {0}: {1} {2}", novoOrder.Id, ex.Message, ex.StackTrace));
            }
        }


        /// <summary>
        /// Arquivo de entrada:  mesmo do Ingressar Order
        /// </summary>
        /// CABECALHO:  NE__Asset__r:Asset:ExternalId__c	Name	NE__Value__c	NE__Old_Value__c	NE__FamPropId__c	NE__FAMPROPEXTID__C
        /// <param name="item"></param>
        /// <param name="conteudoArquivo"></param>
        [ComVisible(true)]
        public void ProcessarIngressoAssetItemAttribute(ItemAttribute item, ref StringBuilder conteudoArquivo)
        {
                foreach (string typeAttribute in new string[] { "NE__AssetItemAttribute__c" })
                {
                    ItemAttribute orderItemAttr = new ItemAttribute();

                    try
                    {
                        bool? _modalidadeVerde = null; //NULL = OPTANTE
                        string tarifaPod = null;

                        foreach (PropertyInfo prop in typeof(ItemAttribute).GetProperties())
                        {
                            tarifaPod = string.Empty;
                            _modalidadeVerde = null;

                            foreach (DisplayAttribute idAttr in ((DisplayAttribute[])(prop.GetCustomAttributes(typeof(DisplayAttribute), true))))
                            {
                                var valor = prop.GetValue(item, null).ToString();

                                if ("Categoria de Tarifa BR".Equals(idAttr.Name))
                                    tarifaPod = valor;

                                if ("Modalidade Tarifaria BR".Equals(idAttr.Name) && valor.ToUpper().Contains("VERDE"))
                                    _modalidadeVerde = true;

                                else if ("Modalidade Tarifaria BR".Equals(idAttr.Name) && valor.ToUpper().Contains("AZUL"))
                                    _modalidadeVerde = false;

                                valor = (("DemandaPonta".Equals(prop.Name) || "DemandaForaPonta".Equals(prop.Name) || "DemandaKV".Equals(prop.Name)) && !_modalidadeVerde.HasValue) ? string.Empty : valor;
                                valor = (("DemandaPonta".Equals(prop.Name) || "DemandaForaPonta".Equals(prop.Name)) && _modalidadeVerde.HasValue && _modalidadeVerde.Value) ? string.Empty : valor;
                                valor = ("DemandaKV".Equals(prop.Name) && _modalidadeVerde.HasValue && !_modalidadeVerde.Value) ? string.Empty : valor;

                                var valorConvertido = string.IsNullOrWhiteSpace(valor) ? " " : Convert.ChangeType(valor, prop.PropertyType);

                                prop.SetValue(orderItemAttr, valorConvertido, null);

                                //Objeto: typeAttribute
                                //NE__Asset__r:Asset:ExternalID__c Name NE__Value__c NE__Old_Value__c NE__FamPropId__c NE__FAMPROPEXTID__C
                                conteudoArquivo.AppendLine(string.Format("{0}\t{1}\t{2}\t{3}\t{4}\t{5}"
                                    , item.ExternalIdAsset
                                    , idAttr.Name
                                    , CultureInfo.CurrentCulture.TextInfo.ToTitleCase(valorConvertido.ToString())
                                    , CultureInfo.CurrentCulture.TextInfo.ToTitleCase(valorConvertido.ToString())
                                    , orderItemAttr.dicPropriedadesDinamicas[idAttr.Name]
                                    , string.Concat("Atributos CE:", idAttr.Name)
                                    )
                                );
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(string.Format("\nErro ao ingressar {0} Item Attribute para o Item Order: {1}: {2} {3}\n"
                            , typeAttribute, item.Id, ex.Message, ex.StackTrace));

                        Debugger.Break();
                    }


            }
        }

        /// <summary>
        /// OBJETO SF:  NE__Order_Item_Attribute__c
        /// CABECALHO:  NE__Order_Item__c	Name	NE__Value__c	NE__Old_Value__c	NE__FamPropId__c	NE__FAMPROPEXTID__C
        /// </summary>
        /// <param name="orderItems"></param>
        /// <param name="item"></param>
        /// <param name="conteudoArquivo"></param>
        [ComVisible(true)]
        public void ProcessarIngressoOrderItemAttribute(Dictionary<string, OrderItemSalesforce> orderItems, ItemAttribute item, ref StringBuilder conteudoArquivo)
        {
            if (!orderItems.ContainsKey(item.ExternalIdAsset))
                //throw new Exception();
                return;

            OrderItemSalesforce orderItem = orderItems[item.ExternalIdAsset];

            foreach (string typeAttribute in new string[] { "NE__Order_Item_Attribute__c" })
            {
                ItemAttribute orderItemAttr = new ItemAttribute();

                try
                {
                    bool? _modalidadeVerde = null; //NULL = OPTANTE
                    string tarifaPod = null;

                    foreach (PropertyInfo prop in typeof(ItemAttribute).GetProperties())
                    {
                        tarifaPod = string.Empty;
                        _modalidadeVerde = null;

                        foreach (DisplayAttribute idAttr in ((DisplayAttribute[])(prop.GetCustomAttributes(typeof(DisplayAttribute), true))))
                        {
                            var valor = prop.GetValue(item, null).ToString();

                            if ("Categoria de Tarifa BR".Equals(idAttr.Name))
                                tarifaPod = valor;

                            if ("Modalidade Tarifaria BR".Equals(idAttr.Name) && valor.ToUpper().Contains("VERDE"))
                                _modalidadeVerde = true;

                            else if ("Modalidade Tarifaria BR".Equals(idAttr.Name) && valor.ToUpper().Contains("AZUL"))
                                _modalidadeVerde = false;

                            valor = (("DemandaPonta".Equals(prop.Name) || "DemandaForaPonta".Equals(prop.Name) || "DemandaKV".Equals(prop.Name)) && !_modalidadeVerde.HasValue) ? string.Empty : valor;
                            valor = (("DemandaPonta".Equals(prop.Name) || "DemandaForaPonta".Equals(prop.Name)) && _modalidadeVerde.HasValue && _modalidadeVerde.Value) ? string.Empty : valor;
                            valor = ("DemandaKV".Equals(prop.Name) && _modalidadeVerde.HasValue && !_modalidadeVerde.Value) ? string.Empty : valor;

                            var valorConvertido = string.IsNullOrWhiteSpace(valor) ? " " : Convert.ChangeType(valor, prop.PropertyType);

                            prop.SetValue(orderItemAttr, valorConvertido, null);

                            //Objeto: typeAttribute
                            //NE__Order_Item__c Name NE__Value__c NE__Old_Value__c NE__FamPropId__c NE__FAMPROPEXTID__C"
                            conteudoArquivo.AppendLine(string.Format("{0}\t{1}\t{2}\t{3}\t{4}\t{5}"
                                , orderItem.Id
                                , idAttr.Name
                                , CultureInfo.CurrentCulture.TextInfo.ToTitleCase(valorConvertido.ToString())
                                , CultureInfo.CurrentCulture.TextInfo.ToTitleCase(valorConvertido.ToString())
                                , orderItemAttr.dicPropriedadesDinamicas[idAttr.Name]
                                , string.Concat("Atributos CE:", idAttr.Name)
                                )
                            );
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(string.Format("\nErro ao ingressar {0} Item Attribute para o Item Order: {1}: {2} {3}\n"
                        , typeAttribute, orderItem.Id, ex.Message, ex.StackTrace));

                    Debugger.Break();
                }
            }
        }


        [ComVisible(true)]
        public void ProcessarIngressoOrderItemAttributeManual(Dictionary<string, OrderItemSalesforce> orderItems, ItemAttribute item, ref StringBuilder conteudoArquivo)
        {
            OrderItemSalesforce orderItem = orderItems[item.OrderItemId];

            foreach (string typeAttribute in new string[] { "NE__Order_Item_Attribute__c" })
            {
                ItemAttribute orderItemAttr = new ItemAttribute();

                try
                {
                    bool? _modalidadeVerde = null; //NULL = OPTANTE
                    string tarifaPod = null;

                    foreach (PropertyInfo prop in typeof(ItemAttribute).GetProperties())
                    {
                        tarifaPod = string.Empty;
                        _modalidadeVerde = null;

                        foreach (DisplayAttribute idAttr in ((DisplayAttribute[])(prop.GetCustomAttributes(typeof(DisplayAttribute), true))))
                        {
                            var valor = prop.GetValue(item, null).ToString();

                            if ("Categoria de Tarifa BR".Equals(idAttr.Name))
                                tarifaPod = valor;

                            if ("Modalidade Tarifaria BR".Equals(idAttr.Name) && valor.ToUpper().Contains("VERDE"))
                                _modalidadeVerde = true;

                            else if ("Modalidade Tarifaria BR".Equals(idAttr.Name) && valor.ToUpper().Contains("AZUL"))
                                _modalidadeVerde = false;

                            valor = (("DemandaPonta".Equals(prop.Name) || "DemandaForaPonta".Equals(prop.Name) || "DemandaKV".Equals(prop.Name)) && !_modalidadeVerde.HasValue) ? string.Empty : valor;
                            valor = (("DemandaPonta".Equals(prop.Name) || "DemandaForaPonta".Equals(prop.Name)) && _modalidadeVerde.HasValue && _modalidadeVerde.Value) ? string.Empty : valor;
                            valor = ("DemandaKV".Equals(prop.Name) && _modalidadeVerde.HasValue && !_modalidadeVerde.Value) ? string.Empty : valor;

                            var valorConvertido = string.IsNullOrWhiteSpace(valor) ? " " : Convert.ChangeType(valor, prop.PropertyType);

                            prop.SetValue(orderItemAttr, valorConvertido, null);

                            //Objeto: typeAttribute
                            //NE__Order_Item__c Name NE__Value__c NE__Old_Value__c NE__FamPropId__c NE__FAMPROPEXTID__C"
                            conteudoArquivo.AppendLine(string.Format("{0}\t{1}\t{2}\t{3}\t{4}\t{5}"
                                , orderItem.Id
                                , idAttr.Name
                                , CultureInfo.CurrentCulture.TextInfo.ToTitleCase(valorConvertido.ToString())
                                , CultureInfo.CurrentCulture.TextInfo.ToTitleCase(valorConvertido.ToString())
                                , orderItemAttr.dicPropriedadesDinamicas[idAttr.Name]
                                , string.Concat("Atributos CE:", idAttr.Name)
                                )
                            );
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(string.Format("\nErro ao ingressar {0} Item Attribute para o Item Order: {1}: {2} {3}\n"
                        , typeAttribute, orderItem.Id, ex.Message, ex.StackTrace));

                    Debugger.Break();
                }
            }
        }
        
        
        
        [ComVisible(true)]
        public void ProcessarAtualizacaoAsset(AssetDTO asset, ref StringBuilder conteudoArquivo)
        {
            #region Update original do metodo CarregarB2Win
            //NE__Status__c", "Active"),
            //NE__Action__c", "Add"),
            //Quantity", "1"),
            //NE__BASEONETIMEFEE__C", "0.0"),
            //NE__BASERECURRINGCHARGE__C", "0.0"),
            //NE__ONETIMEFEEOV__C", "0.0"),
            //NE__RECURRINGCHARGEOV__C", "0.0"),
            //NE__RECURRINGCHARGEFREQUENCY__C", "Monthly"),
            //CurrencyIsoCode", "BRL"),
            //Status", "Active"),
            //Name", "Grupo A"),
            //PointofDelivery__c", asset.PointofDeliveryId),
            //AccountId", asset.AccountId),
            //NE__ASSETITEMENTERPRISEID__C", novoOrder.Id),
            //NE__SERVICE_ACCOUNT__C", asset.AccountId),
            //NE__BILLING_ACCOUNT__C",asset.AccountId ),
            //NE__CatalogItem__c", "a0z1o000003z2FOAAY"),
            //NE__ProdId__c", "a1f1o00000bsF6tAAE"),
            //NE__Order_Config__c", novoOrder.Id),
            //Contract__c", lstContractLineItem.FirstOrDefault().ContractId),
            //Company__c", "2005".Equals(codigoEmpresa) ? "AMPLA" : "2003".Equals(codigoEmpresa) ? "COELCE" : "CELG"),
            //Country__c", "BRASIL")
            #endregion

            conteudoArquivo.AppendLine(string.Format("{0}\t{1}\t{2}\t{3}\t{4}\t{5}\t{6}\t{7}\t{8}\t{9}\t{10}\t{11}\t{12}\t{13}\t{14}\t{15}\t{16}\t{17}\t{18}\t{19}\t{20}\t{21}"
                , asset.ExternalId
                , "Active"
                , "Add"
                , "1"
                , "0.0"
                , "0.0"
                , "0.0"
                , "0.0"
                , "Monthly"
                , "BRL"
                , "Active"
                , asset.PointofDeliveryExternalId
                , asset.AccountExternalId
                , asset.OrderId
                , asset.AccountExternalId
                , asset.AccountExternalId
                , "a0z1o000003z2FOAAY"
                , "a1f1o00000bsF6tAAE"
                , asset.OrderId
                , asset.ContractExternalId
                , "2005".Equals(codigoEmpresa) ? "AMPLA" : "2003".Equals(codigoEmpresa) ? "COELCE" : "CELG"
                , "BRASIL"));
        }

        [ComVisible(true)]
        public void ProcessarAtualizacaoPointOfDelivery(AssetDTO asset, ref StringBuilder conteudoArquivo)
        {
            #region Update original do metodo CarregarB2Win
            //SFDCSchemeBuild.GetNewXmlElement("CNT_Contract__c", lstContractLineItem.FirstOrDefault().ContractId)
            #endregion

            conteudoArquivo.AppendLine(string.Format("{0}\t{1}"
                , asset.PointofDeliveryExternalId
                , asset.ContractExternalId));
        }

        [ComVisible(true)]
        public void ProcessarAtualizacaoContract(AssetDTO asset, ref StringBuilder conteudoArquivo)
        {
            #region Update original do metodo CarregarB2Win
            //SFDCSchemeBuild.GetNewXmlElement("CNT_Quote__c", idOrder.ToString()),
            //SFDCSchemeBuild.GetNewXmlElement("CNT_ExternalContract_ID_2__c", lstContractLineItem.FirstOrDefault().NumeroCliente),
            //SFDCSchemeBuild.GetNewXmlElement("CompanyID__c", codigoEmpresa),
            //SFDCSchemeBuild.GetNewXmlElement("Status", "Activated"),
            //SFDCSchemeBuild.GetNewXmlElement("ShippingCountry", "BRASIL")
            #endregion

            conteudoArquivo.AppendLine(string.Format("{0}\t{1}\t{2}\t{3}\t{4}\t{5}"
                , asset.ContractExternalId
                , asset.OrderId
                , asset.NumeroCliente
                , codigoEmpresa
                , "Activated"
                , "BRASIL"));
        }


        /// <summary>
        /// TODO:
        /// </summary>
        /// <param name="arquivo"></param>
        [ComVisible(true)]
        internal void IngressarBillings(Arquivo arquivo)
        {
            if (!loggedIn)
            {
                Autenticacao auth = new Autenticacao(this.ambiente);
                if (!auth.ValidarLogin(ref this.binding))
                    return;

                this.loggedIn = true;
            }

            int i = 0;
            int agrupamento = 5;
            int totalAtualizado = 0;
            int contCliente = 0;
            sObject sfObj = null;
            List<sObject> lstObjetos = new List<sObject>();

            try
            {
                List<BillingSalesforce> listaArq = ArquivoLoader.GetBillingsFromFile(arquivo);
                StringBuilder msgLog = new StringBuilder();
                int sucesso = 0;
                int falha = 0;

                using (Arquivo arqSaida = new Arquivo(arquivo.Caminho, string.Concat(arquivo.Nome, "_SAIDA"), arquivo.Extensao))
                {
                    foreach (BillingSalesforce rs in listaArq)
                    {
                        sucesso = 0;
                        falha = 0;
                        msgLog.Clear();
                        contCliente++;

                        //tipo de objeto do SalesForce
                        sfObj = new sObject();
                        sfObj.type = "Billing_Profile__c";

                        ClienteSalesforce _cli = SalesforceDAO.GetPodsPorExternalId(rs.PoDSF, ref binding).FirstOrDefault();
                        if (_cli == null || string.IsNullOrWhiteSpace(_cli.Id))
                        {
                            continue;
                        }

                        ClienteSalesforce _account = SalesforceDAO.GetContasPorExternalId(rs.Company__c, string.Concat("'", rs.AccountSF, "'"), ref binding).FirstOrDefault();
                        if (_account == null || string.IsNullOrWhiteSpace(_account.Id))
                        {
                            continue;
                        }

                        ClienteSalesforce _contact = SalesforceDAO.GetContatosPorExternalId(rs.Company__c, rs.BallotName__c, ref binding).FirstOrDefault();
                        if (_contact == null || string.IsNullOrWhiteSpace(_contact.IdContato))
                        {
                            continue;
                        }

                        i++;

                        sfObj.Any = new System.Xml.XmlElement[] {
                            SFDCSchemeBuild.GetNewXmlElement("Account__c", _account.Id),
                            SFDCSchemeBuild.GetNewXmlElement("Type__c", rs.Type__c),
                            SFDCSchemeBuild.GetNewXmlElement("BallotName__c", _contact.IdContato),
                            SFDCSchemeBuild.GetNewXmlElement("Bank__c", rs.Bank__c),
                            SFDCSchemeBuild.GetNewXmlElement("BillingAddress__c", _cli.DetailAddress__c),
                            SFDCSchemeBuild.GetNewXmlElement("Address__c", "N".Equals(rs.DeliveryType__c) ? _cli.DetailAddress__c : string.Empty) ,
                            SFDCSchemeBuild.GetNewXmlElement("CurrentAccountNum__c", rs.CurrentAccountNum__c),
                            SFDCSchemeBuild.GetNewXmlElement("CurrentAccountNumber__c", "0"),
                            SFDCSchemeBuild.GetNewXmlElement("ExternalID__c", rs.ExternalID__c),
                            SFDCSchemeBuild.GetNewXmlElement("PointofDelivery__c", _cli.Id),
                            SFDCSchemeBuild.GetNewXmlElement("AccountContract__c", rs.AccountContract__c) ,
                            SFDCSchemeBuild.GetNewXmlElement("DeliveryType__c", rs.DeliveryType__c) ,
                            SFDCSchemeBuild.GetNewXmlElement("CNT_Braile__c", "S".Equals(rs.CNT_Braile__c) ? "1" : "0") ,
                            SFDCSchemeBuild.GetNewXmlElement("CNT_Due_Date__c", rs.CNT_Due_Date__c) ,
                            SFDCSchemeBuild.GetNewXmlElement("Company__c", rs.Company__c)
                        };

                        lstObjetos.Add(sfObj);
                        totalAtualizado++;

                        msgLog.AppendLine(string.Format("Registro OK: {0} :: Billing.External Id {1} - Account_Id {2}", contCliente, rs.ExternalID__c, _account.Id));
                        Console.WriteLine(msgLog.ToString());
                        IO.EscreverArquivo(arqSaida, msgLog.ToString());

                        //Update a cada 200 registros, por limitação do Sales ce
                        if (lstObjetos.Count > 0 && i == (agrupamento -1))
                        {
                            SaveResult[] saveResults = SalesforceDAO.InserirObjetosSF(lstObjetos, ref binding);
                            SaveResult resultado = saveResults.First();

                            if (resultado.errors != null)
                            {
                                foreach (Error err in resultado.errors)
                                {
                                    Console.WriteLine(string.Concat("[ERRO] ", err.message));
                                    IO.EscreverArquivo(arqSaida, string.Concat("[ERRO] ", err.message));
                                }
                            }
                            msgLog.AppendLine(string.Format("[Id Resultado] {0}", resultado.id));

                            sucesso = saveResults.Where(x => x.success = true).Count();
                            falha = saveResults.Where(x => x.success = false).Count();
                            msgLog.AppendLine(string.Format("Totais:  Sucesso {0},  Erro {1}", sucesso, falha));

                            Console.WriteLine(msgLog.ToString());
                            IO.EscreverArquivo(arqSaida, msgLog.ToString());
                            
                            i = 0;
                            lstObjetos.Clear();
                        }
                    }  // fim foreach


                    //Update do remanescente da lista
                    if (i < agrupamento)
                    {
                        totalAtualizado += i;
                        SaveResult[] saveResults = SalesforceDAO.InserirObjetosSF(lstObjetos, ref binding);
                        sucesso = saveResults.Where(x => x.success = true).Count();
                        falha = saveResults.Where(x => x.success = false).Count();
                        msgLog.AppendLine(string.Format("Últimos Totais:  Sucesso {0},  Erro {1}", sucesso, falha));

                        Console.WriteLine(msgLog.ToString());
                        IO.EscreverArquivo(arqSaida, msgLog.ToString());
                        i = 0;
                        lstObjetos.Clear();
                    }

                    msgLog.AppendLine(string.Format("Total clientes: {0},  Total Atualizado {1}", contCliente, totalAtualizado));
                    Console.WriteLine(msgLog.ToString());
                    IO.EscreverArquivo(arqSaida, msgLog.ToString());

                }  //fim arquivoSaida

                Console.ReadKey();
            }
            catch (Exception ex)
            {
                //TODO:   incrementar o log
                Console.WriteLine("\nErro ao atualizar o registro: \n" + ex.Message + ex.StackTrace);
            }
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="arquivo"></param>
        [ComVisible(true)]
        internal void IngressarTalends(Arquivo arquivo)
        {
            if (!loggedIn)
            {
                Autenticacao auth = new Autenticacao(this.ambiente);
                if (!auth.ValidarLogin(ref this.binding))
                    return;

                this.loggedIn = true;
            }

            int i = 0;
            int totalAtualizado = 0;
            int contCliente = 0;
            sObject sfObj = null;
            List<sObject> lstObjetos = new List<sObject>();

            try
            {
                List<TalendSalesforce> listaArq = ArquivoLoader.GetTalends(arquivo);
                StringBuilder msgLog = new StringBuilder();
                int sucesso = 0;
                int falha = 0;

                using (Arquivo arqSaida = new Arquivo(arquivo.Caminho, string.Concat(arquivo.Nome, "_SAIDA"), arquivo.Extensao))
                {
                    foreach (TalendSalesforce rs in listaArq)
                    {
                        sucesso = 0;
                        falha = 0;
                        msgLog.Clear();
                        contCliente++;

                        i++;

                        //tipo de objeto do SalesForce
                        sfObj = new sObject();
                        sfObj.type = "CNT_Talend_File__c";

                        sfObj.Any = new System.Xml.XmlElement[] {
                            SFDCSchemeBuild.GetNewXmlElement("CNT_Body__c", rs.Body),
                            SFDCSchemeBuild.GetNewXmlElement("CNT_ExternalId__c", rs.ExternalId),
                            SFDCSchemeBuild.GetNewXmlElement("CNT_Functionality__c", rs.Functionality)
                        };

                        lstObjetos.Add(sfObj);
                        totalAtualizado++;

                        msgLog.AppendLine(string.Format("Registro OK: {0} :: {1} - {2}", contCliente, rs.ExternalId, rs.Body));
                        Console.WriteLine(msgLog.ToString());
                        IO.EscreverArquivo(arqSaida, msgLog.ToString());

                        //Update a cada 200 registros, por limitação do Sales ce
                        if (lstObjetos.Count > 0 && i == 199)
                        {
                            SaveResult[] saveResults = SalesforceDAO.InserirObjetosSF(lstObjetos, ref binding);
                            sucesso = saveResults.Where(x => x.success = true).Count();
                            falha = saveResults.Where(x => x.success = false).Count();
                            msgLog.AppendLine(string.Format("Totais:  Sucesso {0},  Erro {1}", sucesso, falha));

                            Console.WriteLine(msgLog.ToString());
                            IO.EscreverArquivo(arqSaida, msgLog.ToString());
                            i = 0;
                            lstObjetos.Clear();
                        }
                    }  // fim foreach


                    //Update do remanescente da lista
                    if (i < 200)
                    {
                        totalAtualizado += i;
                        SaveResult[] saveResults = SalesforceDAO.InserirObjetosSF(lstObjetos, ref binding);
                        sucesso = saveResults.Where(x => x.success = true).Count();
                        falha = saveResults.Where(x => x.success = false).Count();
                        msgLog.AppendLine(string.Format("Últimos Totais:  Sucesso {0},  Erro {1}", sucesso, falha));

                        Console.WriteLine(msgLog.ToString());
                        IO.EscreverArquivo(arqSaida, msgLog.ToString());
                        i = 0;
                        lstObjetos.Clear();
                    }

                    msgLog.AppendLine(string.Format("Total clientes: {0},  Total Atualizado {1}", contCliente, totalAtualizado));
                    Console.WriteLine(msgLog.ToString());
                    IO.EscreverArquivo(arqSaida, msgLog.ToString());

                }  //fim arquivoSaida

                Console.ReadKey();
            }
            catch (Exception ex)
            {
                Console.WriteLine("\nErro ao atualizar o registro: \n" + ex.Message);
            }
        }

        
        public void UpsertSample()
        {
            // TODO Auto-generated method stub
            // Verify that we are already authenticated, if not
            // call the login function to do so
            if (!loggedIn)
            {
                Autenticacao auth = new Autenticacao(this.ambiente);
                if (!auth.ValidarLogin(ref this.binding))
                    return;

                this.loggedIn = true;
            }

            try
            {

                //DescribeSObjectResult dsr = binding.describeSObject("Account");
                //System.Collections.Hashtable fieldMap = this.makeFieldMap(dsr.fields);
                //if (!fieldMap.ContainsKey("External_Id__c"))
                //{
                //    Console.WriteLine("\n\nNOTICE: To run the upsert sample you need \nto add a custom field named \nExternal_Id to the account object.  \nSet the size of the field to 8 characters \nand be sure to check the 'External Id' checkbox.");
                //}
                //else
                //{
                    //First, we need to make sure the test accounts do not exist.
                    QueryResult qr = binding.query("Select Id From Account Where External_Id__c = '11111111' or External_Id__c = '22222222'");
                    if (qr.size > 0)
                    {
                        sObject[] accounts = (sObject[])qr.records;
                        //Get the ids
                        String[] ids = new String[accounts.Length];
                        for (int i = 0; i < ids.Length; i++)
                        {
                            ids[i] = accounts[i].Id;
                        }
                        //Delete the accounts
                        binding.delete(ids);
                    }

                    //Create a new account using create, we wil use this to update via upsert
                    //We will set the external id to be ones so that we can use that value for the upsert
                    sObject newAccount = new sObject();
                    newAccount.type = "Account";
                    newAccount.Any = new System.Xml.XmlElement[] { GetNewXmlElement("Name", "Account to update"), 
                        GetNewXmlElement("External_Id__c", "11111111") };
                    binding.create(new sObject[] { newAccount });

                    //Now we will create an account that should be updated on insert based
                    //on the external id field.
                    sObject updateAccount = new sObject();
                    updateAccount.type = "Account";
                    updateAccount.Any = new System.Xml.XmlElement[] { GetNewXmlElement("Website", "http://www.website.com"), 
                        GetNewXmlElement("External_Id__c", "11111111") };

                    // This account is meant to be new
                    sObject createAccount = new sObject();
                    createAccount.type = "Account";
                    createAccount.Any = new System.Xml.XmlElement[] { GetNewXmlElement("Name", "My Company, Inc"), 
                        GetNewXmlElement("External_Id__c", "22222222") };

                    //We have our two accounts, one should be new, the other should be updated.
                    try
                    {
                        // Invoke the upsert call and save the results.
                        // Use External_Id custom field for matching records
                        UpsertResult[] upsertResults = binding.upsert("External_Id__c", new sObject[] { createAccount, updateAccount });
                        for (int i = 0; i < upsertResults.Length; i++)
                        {
                            UpsertResult result = upsertResults[i];
                            if (result.success)
                            {
                                Console.WriteLine("\nUpsert succeeded.");
                                Console.WriteLine((result.created ? "Inserted" : "Updated") + " account, id is " + result.id);
                            }
                            else
                            {
                                Console.WriteLine("The Upsert failed because: " + result.errors[0].message);
                            }
                        }
                    }
                    catch (System.Web.Services.Protocols.SoapException ex)
                    {
                        Console.WriteLine("Error from web service: " + ex.Message);
                    }
                //}
                Console.WriteLine("\nPress the RETURN key to continue...");
                Console.ReadLine();
            }
            catch (System.Web.Services.Protocols.SoapException ex)
            {
                Console.WriteLine("Error merging account: " + ex.Message + "\nHit return to continue...");
                Console.ReadLine();
            }
        }


        #region Métodos Privados

        private System.Xml.XmlElement GetNewXmlElement(string Name, string nodeValue)
        {
            System.Xml.XmlDocument doc = new System.Xml.XmlDocument();
            System.Xml.XmlElement xmlel = doc.CreateElement(Name);
            xmlel.InnerText = nodeValue;
            return xmlel;
        }

        private System.Collections.Hashtable makeFieldMap(Field[] fields)
        {
            System.Collections.Hashtable fieldMap = new System.Collections.Hashtable();
            for (int i = 0; i < fields.Length; i++)
            {
                Field field = fields[i];
                fieldMap.Add(field.name, field);
            }
            return fieldMap;
        }

        #endregion Métodos Privados





        /// <summary>
        /// Extrai registros originais enviados ao Delta e que não foram carregados pelo ETL (Rechazos).
        ///
        /// <remarks>190319 Contacts: Rechazo ocorreu porque havia uma validação no ETL de carga que impediu a carga de contatos com mesmos nomes.</remarks>
        /// </summary>
        internal void ExtrairRechazos()
        {
            #region Log de Execuções

            //190319 -------------------------------------
            //string arqRechazos = @"C:\!adl\Ayesa\EPT_Suporte\180604 Salesforce\190225 Desfazedor de Merda\180319 Contacts Nao Carregados\CONTACTS\rechazos.txt";  //CONTACT
            //Arquivo arqSaida = new Arquivo(@"C:\!adl\Ayesa\EPT_Suporte\180604 Salesforce\190225 Desfazedor de Merda\180319 Contacts Nao Carregados\CONTACTS\rechazos_FINAL_04.txt");

            //190320 -------------------------------------
            //string arqRechazos = @"C:\!adl\Ayesa\EPT_Suporte\180604 Salesforce\190225 Desfazedor de Merda\180319 Contacts Nao Carregados\POD\rechazos.txt";   // POD
            //Arquivo arqSaida = new Arquivo(@"C:\!adl\Ayesa\EPT_Suporte\180604 Salesforce\190225 Desfazedor de Merda\180319 Contacts Nao Carregados\POD\PointOfDelivery_Rechazos_Reenvio.txt");

            //string arqRechazos = @"C:\!adl\Ayesa\EPT_Suporte\180604 Salesforce\190225 Desfazedor de Merda\180319 Contacts Nao Carregados\POD\rechazos_servicetype.txt";
            //Arquivo arqSaida = new Arquivo(@"C:\!adl\Ayesa\EPT_Suporte\180604 Salesforce\190225 Desfazedor de Merda\180319 Contacts Nao Carregados\POD\PointOfDelivery_Rechazos_servicetype.txt");

            //string arqRechazos = @"C:\!adl\Ayesa\EPT_Suporte\180604 Salesforce\190225 Desfazedor de Merda\180319 Contacts Nao Carregados\POD\rechazos_duplicate.txt";
            //Arquivo arqSaida = new Arquivo(@"C:\!adl\Ayesa\EPT_Suporte\180604 Salesforce\190225 Desfazedor de Merda\180319 Contacts Nao Carregados\POD\PointOfDelivery_Rechazos_duplicate.txt");
            //O QUE FAZER COM OS DUPLICADOS??

            //string arqRechazos = @"C:\!adl\Ayesa\EPT_Suporte\180604 Salesforce\190225 Desfazedor de Merda\180319 Contacts Nao Carregados\CONTACTS\190320\rechazos_contatos_novos.txt";
            //Arquivo arqSaida = new Arquivo(@"C:\!adl\Ayesa\EPT_Suporte\180604 Salesforce\190225 Desfazedor de Merda\180319 Contacts Nao Carregados\CONTACTS\190320\Contact_rechazos_contatos_novos.txt");
            //C:\!adl\Ayesa\EPT_Suporte\180604 Salesforce\190225 Desfazedor de Merda\180319 Contacts Nao Carregados\CONTACTS\190320-2\novos_clientes_2003.txt

            //190321
            //string arqRechazos = @"C:\!adl\Ayesa\EPT_Suporte\180604 Salesforce\190225 Desfazedor de Merda\180319 Contacts Nao Carregados\CONTACTS\190320\rechazos_contatos_novos.txt";  //CONTACT
            //Arquivo arqSaida = new Arquivo(@"C:\!adl\Ayesa\EPT_Suporte\180604 Salesforce\190225 Desfazedor de Merda\180319 Contacts Nao Carregados\CONTACTS\190320\rechazos_FINAL_02.txt");

            //string arqRechazos = @"C:\!adl\Ayesa\EPT_Suporte\180604 Salesforce\190225 Desfazedor de Merda\180319 Contacts Nao Carregados\ACCOUNT\ExternalIds.txt";  //ACCOUNT
            //Arquivo arqSaida = new Arquivo(@"C:\!adl\Ayesa\EPT_Suporte\180604 Salesforce\190225 Desfazedor de Merda\180319 Contacts Nao Carregados\ACCOUNT\Account_Rechazos_Final.txt");

            //190402
            //string arqRechazos = @"C:\!adl\Ayesa\EPT_Suporte\180604 Salesforce\190225 Desfazedor de Merda\180319 Contacts Nao Carregados\CONTRACT\190402\ExternalIds.txt";  //CONTACT
            //Arquivo arqSaida = new Arquivo(@"C:\!adl\Ayesa\EPT_Suporte\180604 Salesforce\190225 Desfazedor de Merda\180319 Contacts Nao Carregados\CONTRACT\190402\CONTRACTS.txt");

            //190404
            //string arqRechazos = @"C:\!adl\Ayesa\EPT_Suporte\180604 Salesforce\190225 Desfazedor de Merda\180319 Contacts Nao Carregados\BILLING\ExternalIds_Contact.txt";  //CONTACT
            //Arquivo arqSaida = new Arquivo(@"C:\!adl\Ayesa\EPT_Suporte\180604 Salesforce\190225 Desfazedor de Merda\180319 Contacts Nao Carregados\BILLING\ExternalIds_Contact_SAIDA.txt");

            //string arqRechazos = @"C:\!adl\Ayesa\EPT_Suporte\180604 Salesforce\190225 Desfazedor de Merda\180319 Contacts Nao Carregados\BILLING\ExternalIds_Contact.txt";  //CONTACT
            //Arquivo arqSaida = new Arquivo(@"C:\!adl\Ayesa\EPT_Suporte\180604 Salesforce\190225 Desfazedor de Merda\180319 Contacts Nao Carregados\BILLING\ExternalIds_Contact_Billing_SAIDA.txt");

            //100409
            string arqRechazos = @"C:\!adl\Ayesa\EPT_Suporte\180604 Salesforce\190225 Desfazedor de Merda\180319 Contacts Nao Carregados\CONTRACT_LINE\ExtenalID_BILLING_RECHAZOS.txt";
            Arquivo arqSaida = new Arquivo(@"C:\!adl\Ayesa\EPT_Suporte\180604 Salesforce\190225 Desfazedor de Merda\180319 Contacts Nao Carregados\CONTRACT_LINE\ExternalIds_ContractLine_Billing_SAIDA.txt");
            #endregion


            List<string> externalIdContact = new List<string>();
            List<string> linhasFinal = new List<string>();
            Dictionary<string, string> dicDelta = new Dictionary<string, string>();

            //foreach (string arqDelta in new string[] { @"C:\!adl\Ayesa\EPT_Suporte\180604 Salesforce\190225 Desfazedor de Merda\180319 Contacts Nao Carregados\POD\PointOfDelivery_1.csv", @"C:\!adl\Ayesa\EPT_Suporte\180604 Salesforce\190225 Desfazedor de Merda\180319 Contacts Nao Carregados\POD\PointOfDelivery_2.csv" })
            foreach (string arqDelta in new string[] { @"C:\!adl\Ayesa\EPT_Suporte\180604 Salesforce\190225 Desfazedor de Merda\180319 Contacts Nao Carregados\BILLING\BILLING-1.csv", @"C:\!adl\Ayesa\EPT_Suporte\180604 Salesforce\190225 Desfazedor de Merda\180319 Contacts Nao Carregados\BILLING\BILLING-2.csv" })
            //foreach (string arqDelta in new string[] { @"C:\!adl\Ayesa\EPT_Suporte\180604 Salesforce\190225 Desfazedor de Merda\180319 Contacts Nao Carregados\CONTACTS\190320\Contact_rechazos_contatos_novos.csv" })
            //foreach (string arqDelta in new string[] { @"C:\!adl\Ayesa\EPT_Suporte\180604 Salesforce\190225 Desfazedor de Merda\180319 Contacts Nao Carregados\ACCOUNT\ACCOUNT_ORIGINAL.csv" })
            //foreach (string arqDelta in new string[] { @"C:\!adl\Ayesa\EPT_Suporte\180604 Salesforce\190225 Desfazedor de Merda\180319 Contacts Nao Carregados\CONTRACT\190402\CONTRACT-1.csv", @"C:\!adl\Ayesa\EPT_Suporte\180604 Salesforce\190225 Desfazedor de Merda\180319 Contacts Nao Carregados\CONTRACT\190402\CONTRACT-2.csv" })
            {
                using (StreamReader sr2 = new StreamReader(arqDelta, Encoding.UTF8)) 
                {
                    while (!sr2.EndOfStream) 
                    {
                        string linha = sr2.ReadLine();
                        string extIdDelta = linha.Split(';')[0].ToUpper().Replace("\"", "");

                        try {
                            if (!dicDelta.ContainsKey(extIdDelta))
                                dicDelta.Add(extIdDelta, linha);
                        }
                        catch {
                            Debugger.Break();
                        }
                    }
                }

                using (StreamReader sr = new StreamReader(arqRechazos, Encoding.UTF8)) 
                {
                    while (!sr.EndOfStream) 
                    {
                        if (linhasFinal.Count > 0 && linhasFinal.Count % 10000 == 0) {
                            IO.EscreverArquivo(arqSaida, string.Join(Environment.NewLine, linhasFinal.ToArray()));
                            linhasFinal.Clear();
                        }

                        string externalId = sr.ReadLine();
                        try 
                        {
                            if (dicDelta.ContainsKey(externalId.ToUpper())) {
                                linhasFinal.Add(dicDelta[externalId.ToUpper()]);
                                continue;
                            }
                        }
                        catch(Exception ex){
                            //Debugger.Break();
                            continue;
                        }
                    }
                }

                if (linhasFinal.Count > 0)
                    IO.EscreverArquivo(arqSaida, string.Join(Environment.NewLine, linhasFinal.ToArray()));

                Debugger.Break();
                linhasFinal.Clear();
                dicDelta.Clear();
                GC.Collect();
            }
            
            if (linhasFinal.Count > 0)
                IO.EscreverArquivo(arqSaida, string.Join(Environment.NewLine, linhasFinal.ToArray()));
            
            linhasFinal.Clear();
            arqSaida.Dispose();            
        }


        
        
        /// <summary>
        /// EPT_Suporte\180604 Salesforce\180813 Delta Grupo B\CE GB 181009 Higienização Assets Accounts
        /// </summary>
        [ComVisible(true)]
        internal void CorrigirAssets()
        {
            // 1--------------------------------------
            // carregar OFFLINE a lista (1) de Assets do Salesforce com
            // NOME, DOC, TIPO DOC, EXTERNAL ID, NUMERO CLIENTE, ID ASSET, DATA CREATE ASSET

            StringBuilder arquivo = new StringBuilder("c:\\temp\\AssetsCEGB.txt");
            Dictionary<string, AssetDTO> dicAssetSF = new Dictionary<string, AssetDTO>();

            //lista auxiliar para linkar cliente e asset mais rapidamente
            Dictionary<string, string> dicAssetCli = new Dictionary<string, string>();

            using (StreamReader sr = new StreamReader(arquivo.ToString(), Encoding.GetEncoding("iso-8859-1")))
            {
                while (!sr.EndOfStream)
                {
                    string linha = sr.ReadLine();
                    string[] campos = linha.Split('|');
                    AssetDTO asset = new AssetDTO();

                    //Id|ExternalId__c|Account.Id|Account.ExternalId__c|Account.IdentityNumber__c|Account.IdentityType__c|Name
                    try
                    {
                        asset.Id = campos[0];
                        asset.ExternalId = campos[1];
                        asset.AccountId = campos[2];
                        asset.AccountExternalId = campos[3];
                        asset.Identidade = campos[4];
                        asset.TipoIdentidade = campos[5];
                        asset.NumeroCliente = campos[6];

                        dicAssetSF.Add(asset.Id, asset);
                        dicAssetCli.Add(asset.Id, asset.Identidade);
                    }
                    catch (Exception ex)
                    {
                        //TODO: log?
                        continue;
                    }
                    finally
                    {
                        asset = null;
                        linha = null;
                        campos = null;
                    }
                }
            }

            //var grupo2 = dicAssetSF.GroupBy(x => x.Value.AccountId);
            //Debugger.Break();


            // 2--------------------------------------
            //// carregar OFFLINE a lista (2) de todos os clientes e external id da tabela SALES_GERAL
            //arquivo = new StringBuilder("c:\\temp\\salesgeral_CE_GB.txt");
            
            // carregar OFFLINE a lista (2) dos 55k clientes a ingressar no SF em UAT
            arquivo = new StringBuilder("c:\\temp\\55k_CE_GB.txt"); 
            
            Dictionary<string, SalesGeralDTO> dic55k = new Dictionary<string, SalesGeralDTO>();

            using (StreamReader sr = new StreamReader(arquivo.ToString(), Encoding.GetEncoding("iso-8859-1")))
            {
                while (!sr.EndOfStream)
                {
                    string linha = sr.ReadLine();
                    string[] campos = linha.Split(';');

                    SalesGeralDTO salesg = new SalesGeralDTO();
                    salesg.NumeroCliente = campos[12].Replace("\"", string.Empty);
                    salesg.AccountExternalId = campos[0].Replace("\"", string.Empty);
                    salesg.Documento = campos[3].Replace("\"", string.Empty);
                    salesg.TipoDoc = campos[2].Replace("\"", string.Empty);
                    salesg.Invalido = salesg.AccountExternalId.ToUpper().Contains("INVALIDO") ? 1 : 0;      //Convert.ToInt32(campos[2]);
                    //salesg.AssetExternalId = campos[3];
                    
                    try
                    {
                        dic55k.Add(salesg.NumeroCliente, salesg);
                    }
                    catch (Exception ex)
                    {
                        //TODO: log?
                        continue;
                    }
                    finally
                    {
                        linha = null;
                        campos = null;
                    }
                }
            }


            string dataArq = DateTime.Now.ToString("yyyyMMdd_HHmm");
            List<string> lstDocs = new List<string>();

            foreach (SalesGeralDTO cli55 in dic55k.Values)
            {
                if (cli55.Invalido == 1 || cli55.Despersonalizado)
                    continue;

                if (lstDocs.Contains(cli55.Documento))
                    continue;

                var cli = dicAssetCli.Where(x => x.Value == cli55.Documento);
                if (cli.Count() <= 1)
                    continue;

                lstDocs.Add(cli55.Documento);

                try
                {
                    foreach (KeyValuePair<string,string> cli2 in cli)
                    {
                        if (string.IsNullOrWhiteSpace(cli55.Documento) || "00000000000000000000".Equals(cli55.Documento))
                        {
                            IO.EscreverArquivo("c:\\temp\\arqFinal_" + dataArq, string.Concat(dicAssetSF[cli2.Key].Id, "|", dicAssetSF[cli2.Key].ExternalId, "|", dicAssetSF[cli2.Key].AccountId, "|", cli55.AccountExternalId, "|", cli55.Documento, "|", cli55.TipoDoc, "|", dicAssetSF[cli2.Key].NumeroCliente, "|", cli2.Key));
                        }
                        else
                        {
                            IO.EscreverArquivo("c:\\temp\\arqFinal_" + dataArq, string.Concat(dicAssetSF[cli2.Key].Id, "|", dicAssetSF[cli2.Key].ExternalId, "|", dicAssetSF[cli2.Key].AccountId, "|", dicAssetSF[cli2.Key].AccountExternalId, "|", dicAssetSF[cli2.Key].Identidade, "|", dicAssetSF[cli2.Key].TipoIdentidade, "|", dicAssetSF[cli2.Key].NumeroCliente, "|", cli2.Key));
                        }
                    }
                }
                catch (Exception ex)
                {
                }


           
                // 3.2 ExternalId despersonalizado:
                // seguir para o próximo ASSET

                // 4---------------------------------------
                // pesquisar em (1) a qtd de ACCOUNTS relacionados ao ASSET e validar:

                // 4.1-
                // qtd == 2
                // Identificar a ACCOUNT com o ExternalId ASSET inválido;
                // Atualizar o External Id desta ACCOUNT pelo Id válido
                // Despersonalizar esta ACCOUNT

                // 4.2-
                // qtd > 2
                // Identificar o External Id válido da ACCOUNT mais nova 
                // Atualizar todas as outras ACCOUNTs para este External Id
            }

            Debugger.Break();
        }


        [ComVisible(true)]
        internal void GetTotalEntidade(string nomeEntidade)
        {
            if(string.IsNullOrWhiteSpace(nomeEntidade))
                return;

            if (!loggedIn)
            {
                Autenticacao auth = new Autenticacao(this.ambiente);
                if (!auth.ValidarLogin(ref this.binding))
                    return;

                this.loggedIn = true;
            }

            try
            {
                QueryResult qr = binding.query(string.Format("Select ExternalID__c from {0}  WHERE Company__c = '{1}'", nomeEntidade, this.codigoEmpresa));

                int total = qr == null ? 0 : qr.size;

                Console.WriteLine(string.Format("\nTotal de registros em '{0}': {1}", nomeEntidade.ToUpper(), total));
            }
            catch(Exception ex)
            {
            }
        }


        /// <summary>
        /// Identifica as Contas que possuem os mesmos documentos e associa a um única conta, despersonalizando as demais.   
        /// Inclui a marcação "DESPERSONA" ao nome das Contas despersonalizadas.
        /// </summary>
        /// <param name="arquivo">Arquivo com a lista-base de clientes para os quais se deseja parametrizar as demais contas.   
        /// Deve conter os clientes cujos documentos estão cadastrados em mais de 1 conta.
        /// O objetivo do processo é unificar em 1 conta todos os demais objetos do Salesforce.</param>
        [ComVisible(true)]
        internal void HigienizarContas(Arquivo arquivo)
        {
            #region Exemplo com NLog
            //var logger = LogManager.GetLogger("HigienizarContasLog");
            //logger.Info("Hello World");
            //logger.Error("nada");

            ////com erro
            //var mail = LogManager.GetLogger("HigienizarContasMail");
            //mail.Error("teste");
            #endregion

            const string DESPERSONA = "*DESPERSONA";
            if (!loggedIn)
            {
                Autenticacao auth = new Autenticacao(this.ambiente);
                if (!auth.ValidarLogin(ref this.binding))
                    return;

                this.loggedIn = true;
            }

            int totalAtualizado = 0;
            int contCliente = 0;
            sObject sfObj = null;
            List<sObject> lstObjetos = new List<sObject>();
            StringBuilder auxRecordType = new StringBuilder();
            StringBuilder auxNome = new StringBuilder();

            try
            {
                List<string> docsConsultados = new List<string>();
                List<AssetDTO> listaArq = ArquivoLoader.GetAssets(arquivo);
                List<ClienteSalesforce> baseFull = ArquivoLoader.GetAccounts(new Arquivo("C:\\!adl\\Ayesa\\EPT_Suporte\\180604 Salesforce\\181120 Higienizacao Account CE\\190213 PROD\\accounts_ce.txt", '|'));
                StringBuilder msgLog = new StringBuilder();

                using (Arquivo arqSaida = new Arquivo(arquivo.Caminho, string.Concat(arquivo.Nome, "_SAIDA"), arquivo.Extensao))
                {
                    using (Arquivo arqInconsistExternalId = new Arquivo(arquivo.Caminho, string.Concat(arquivo.Nome, "_INCONSISTENCIA_EXTERNALID"), arquivo.Extensao))
                    {
                        msgLog.AppendLine(string.Format("Arquivo carregado em {0}", DateTime.Now.ToLongTimeString()));
                        Console.WriteLine(msgLog.ToString());
                        IO.EscreverArquivo(arqSaida, msgLog.ToString());
                        Console.WriteLine("");


                        foreach (AssetDTO rs in listaArq)
                        {
                            msgLog.Clear();
                            contCliente++;

                            if (docsConsultados.Contains(rs.Identidade))
                                continue;

                            if (string.IsNullOrWhiteSpace(rs.Identidade))
                                continue;

                            if (rs.AccountExternalId.ToUpper().Contains("INVALIDO") || rs.NomeCliente.ToUpper().Contains("*DESPERSONA") || rs.AccountExternalId.ToUpper().Contains("2003D"))
                            {
                                msgLog.Append(string.Format("{0}\tINVALID\t{1}|{2}|{3}|{4}|{5}", contCliente.ToString().PadLeft(10, '0'), rs.AccountId, rs.AccountExternalId, rs.Identidade, rs.TipoIdentidade, rs.NomeCliente));
                                Console.WriteLine(msgLog);
                                IO.EscreverArquivo(arqInconsistExternalId, msgLog.ToString());
                                continue;
                            }

                            if (!rs.AccountExternalId.Contains(rs.Identidade))
                            {
                                msgLog.Append(string.Format("{0}\tEXT_ID\t{1}|{2}|{3}|{4}|{5}", contCliente.ToString().PadLeft(10, '0'), rs.AccountId, rs.AccountExternalId, rs.Identidade, rs.TipoIdentidade, rs.NomeCliente));
                                Console.WriteLine(msgLog);
                                IO.EscreverArquivo(arqInconsistExternalId, msgLog.ToString());
                                continue;
                            }

                            //continue;
                            #region ACCOUNT && ASSET ------------------------------

                            //List<ClienteSalesforce> lstCli = SalesforceDAO.GetContasPorDocumento(this.codigoEmpresa, rs.Identidade, ref binding);
                            List<ClienteSalesforce> lstCli = baseFull.Where(c => c.Documento == rs.Identidade).OrderBy(z => z.IdConta).ToList();
                            //List<ClienteSalesforce> lstCli = baseFull.Where(c => c.ExternalId == rs.AccountExternalId).ToList();

                            docsConsultados.Add(rs.Identidade);

                            if (lstCli == null || lstCli.Count <= 1)
                            {
                                msgLog.Append(string.Format("{0}\tACC\tConta unica ou inexistente. AccountId: {1} Doc: {2}", contCliente.ToString().PadLeft(10, '0'), rs.AccountId, rs.Identidade));
                                Console.WriteLine(msgLog.ToString());
                                IO.EscreverArquivo(arqSaida, msgLog.ToString());
                                continue;
                            }

                            sfObj = new sObject();

                            foreach (ClienteSalesforce cli in lstCli)
                            {
                                List<AssetDTO> lstAssets = SalesforceDAO.GetAssetsPorAccountId(this.codigoEmpresa, cli.IdConta, ref binding);

                                if (cli.IdConta.Equals(rs.AccountId))
                                    continue;

                                if(string.IsNullOrWhiteSpace(cli.TipoCliente) && string.IsNullOrWhiteSpace(rs.TipoCliente))
                                { }

                                if (string.IsNullOrWhiteSpace(cli.TipoRegistroId))
                                {
                                    msgLog.Append(string.Format("{0}\tREC_ID\t{1}|{2}", contCliente.ToString().PadLeft(10, '0'), cli.IdConta, cli.TipoRegistroId));
                                    Console.WriteLine(msgLog);
                                    IO.EscreverArquivo(arqInconsistExternalId, msgLog.ToString());
                                    continue;
                                }

                                sfObj.type = "Account";

                                msgLog.Clear();
                                msgLog.Append(string.Format("{0}\tACC\tOLD: {1} Documento: {2} NEW: {3}", contCliente.ToString().PadLeft(10, '0'), cli.IdConta, cli.Documento, rs.AccountId));
                                Console.WriteLine(msgLog.ToString());
                                IO.EscreverArquivo(arqSaida, msgLog.ToString());

                                sfObj.Id = cli.IdConta;

                                #region Alterar RecordTypeId para B2G
                                auxRecordType.Clear();
                                auxRecordType.Append(cli.TipoRegistroId);
                                sfObj.Any = new System.Xml.XmlElement[] {
                                SFDCSchemeBuild.GetNewXmlElement("Name", (cli.Nome.Length > 40) ? cli.Nome.Substring(0, 40-1) : cli.Nome),
                                SFDCSchemeBuild.GetNewXmlElement("RecordTypeId", "0121o000000oWhZAAU")
                            };

                                lstObjetos.Add(sfObj);
                                SaveResult[] saveRecordType = binding.update(lstObjetos.ToArray());
                                #endregion

                                lstObjetos.Clear();
                                auxNome.Clear();
                                auxNome.AppendFormat("{0}{1}", (cli.Nome.Length > 29 ? cli.Nome.Replace(DESPERSONA, "").Substring(0, 29 - 1) : cli.Nome), DESPERSONA);

                                sfObj.Any = new System.Xml.XmlElement[] {
                                SFDCSchemeBuild.GetNewXmlElement("Name", cli.Nome.Contains(DESPERSONA) ? cli.Nome : auxNome.ToString()),
                                SFDCSchemeBuild.GetNewXmlElement("ExternalId__c", string.Concat(cli.ExternalId.Replace(DESPERSONA,""), DESPERSONA)),
                                SFDCSchemeBuild.GetNewXmlElement("NoWishtoGiveMail__c", "TRUE"),
                                SFDCSchemeBuild.GetNewXmlElement("IdentityNumber__c", " "),
                                SFDCSchemeBuild.GetNewXmlElement("IdentityType__c", " "),
                                SFDCSchemeBuild.GetNewXmlElement("CNT_Resp_ID_Number__c", " "),
                                SFDCSchemeBuild.GetNewXmlElement("CNT_Client_Type__c", string.IsNullOrWhiteSpace(cli.TipoCliente) ? rs.TipoCliente : cli.TipoCliente)
                                //, SFDCSchemeBuild.GetNewXmlElement("CNT_State_Inscription_Exemption__c", "TRUE")
                            };

                                lstObjetos.Add(sfObj);
                                totalAtualizado++;

                                SaveResult[] saveResults = binding.update(lstObjetos.ToArray());
                                SaveResult resultado = saveResults.First();
                                if (resultado.errors != null)
                                {
                                    foreach (Error err in resultado.errors)
                                    {
                                        Console.WriteLine(string.Format("[ERRO 01] AccountId: {0} Documento: {1} {2}", cli.IdConta, cli.Documento, err.message));
                                        IO.EscreverArquivo(arqSaida, string.Format("[ERRO 01] AccountId: {0} Documento: {1} {2}", cli.IdConta, cli.Documento, err.message));
                                    }
                                }

                                lstObjetos.Clear();

                                #region Restaurar RecordTypeId
                                sfObj.Any = new System.Xml.XmlElement[] {
                                SFDCSchemeBuild.GetNewXmlElement("RecordTypeId", auxRecordType.ToString())
                            };
                                lstObjetos.Add(sfObj);
                                saveRecordType = binding.update(lstObjetos.ToArray());
                                #endregion


                                #region ASSET --------------------------------
                                saveResults = null;
                                lstObjetos.Clear();
                                msgLog.Clear();

                                if (lstAssets == null || lstAssets.Count <= 0)
                                {
                                    msgLog.Append(string.Format("{0}\tASSET\tNao encontrado para o Account {1}", contCliente.ToString().PadLeft(10, '0'), cli.IdConta));
                                    Console.WriteLine(msgLog.ToString());
                                    IO.EscreverArquivo(arqSaida, msgLog.ToString());
                                }
                                else
                                {
                                    foreach (AssetDTO asset in lstAssets)
                                    {
                                        msgLog.Clear();
                                        lstObjetos.Clear();

                                        if (asset.AccountId.Equals(rs.AccountId))
                                        {
                                            msgLog.Append(string.Format("{0}\tASSET\tId: {1} - Account Iguais: {2}", contCliente.ToString().PadLeft(10, '0'), asset.Id, rs.AccountId));
                                            Console.WriteLine(msgLog.ToString());
                                            IO.EscreverArquivo(arqSaida, msgLog.ToString());
                                        }
                                        else
                                        {
                                            sfObj.type = "Asset";
                                            sfObj.Id = asset.Id;

                                            msgLog.Append(string.Format("{0}\tASSET\tId: {1} OLD: {2} - NEW: {3}", contCliente.ToString().PadLeft(10, '0'), asset.Id, asset.AccountId, rs.AccountId));
                                            Console.WriteLine(msgLog.ToString());
                                            IO.EscreverArquivo(arqSaida, msgLog.ToString());

                                            sfObj.Any = new System.Xml.XmlElement[] {
                                            SFDCSchemeBuild.GetNewXmlElement("AccountId", rs.AccountId)
                                        };

                                            lstObjetos.Add(sfObj);

                                            saveResults = binding.update(lstObjetos.ToArray());
                                            resultado = saveResults.First();
                                            if (resultado.errors != null)
                                            {
                                                foreach (Error err in resultado.errors)
                                                {
                                                    Console.WriteLine(string.Format("[ERRO 02] AssetId: {0} AccountId: {1} {2}", asset.Id, rs.AccountId, err.message));
                                                    IO.EscreverArquivo(arqSaida, string.Format("[ERRO 02] AssetId: {0} AccountId: {1} {2}", asset.Id, rs.AccountId, err.message));
                                                }
                                            }
                                        }
                                    }

                                    lstObjetos.Clear();
                                }
                                #endregion ASSET


                                #region BILLING PROFILE -----------------------------------
                                saveResults = null;
                                lstObjetos.Clear();
                                msgLog.Clear();

                                List<BillingSalesforce> bills = SalesforceDAO.GetPerfisFaturamentoByAccoundId(this.codigoEmpresa, cli.IdConta, ref binding);

                                if (bills == null || bills.Count <= 0)
                                {
                                    msgLog.Append(string.Format("{0}\tBILL\tNao encontrado para o Account {1}", contCliente.ToString().PadLeft(10, '0'), cli.IdConta));
                                    Console.WriteLine(msgLog.ToString());
                                    IO.EscreverArquivo(arqSaida, msgLog.ToString());
                                }
                                else
                                {
                                    foreach (BillingSalesforce bill in bills)
                                    {
                                        msgLog.Clear();
                                        lstObjetos.Clear();

                                        sfObj.type = "Billing_Profile__c";
                                        sfObj.Id = bill.Id;

                                        msgLog.Append(string.Format("{0}\tBILL\tId: {1} Cliente: {2} OLD: {3} - NEW: {4}", contCliente.ToString().PadLeft(10, '0'), bill.Id, bill.AccountContract__c, bill.AccountSF, rs.AccountId));
                                        Console.WriteLine(msgLog.ToString());
                                        IO.EscreverArquivo(arqSaida, msgLog.ToString());

                                        sfObj.Any = new System.Xml.XmlElement[] {
                                        SFDCSchemeBuild.GetNewXmlElement("Company__c", this.codigoEmpresa),
                                        SFDCSchemeBuild.GetNewXmlElement("Account__c", rs.AccountId)
                                    };

                                        lstObjetos.Add(sfObj);
                                        //string idsUpdate = string.Join(",", lstObjetos.Select(x => new { x.Id }).ToArray());

                                        try
                                        {
                                            saveResults = binding.update(lstObjetos.ToArray());

                                            resultado = saveResults.First();
                                            if (resultado.errors != null)
                                            {
                                                foreach (Error err in resultado.errors)
                                                {
                                                    Console.WriteLine(string.Format("[ERRO 03.1] BillingId: {0} AccountId: {1} {2}", bill.Id, bill.AccountSF, err.message));
                                                    IO.EscreverArquivo(arqSaida, string.Format("[ERRO 03.1] BillingId: {0} AccountId: {1} {2}", bill.Id, bill.AccountSF, err.message));
                                                }
                                            }
                                        }
                                        catch (Exception ex)
                                        {
                                            Console.WriteLine(string.Format("[ERRO 03.2] BillingId: {0} AccountId: {1} {2} {3}", bill.Id, bill.AccountSF, ex.Message, ex.StackTrace));
                                            IO.EscreverArquivo(arqSaida, string.Format("[ERRO 03.2] BillingId: {0} AccountId: {1} {2} {3}", bill.Id, bill.AccountSF, ex.Message, ex.StackTrace));
                                        }
                                    }
                                }
                                #endregion BILLING PROFILE


                                #region CASES -----------------------------------
                                lstObjetos.Clear();
                                msgLog.Clear();

                                List<CaseSalesforce> cases = SalesforceDAO.GetCasesByAccountId(this.codigoEmpresa, cli.IdConta, ref binding);

                                if (cases == null || cases.Count <= 0)
                                {
                                    msgLog.Append(string.Format("{0}\tCASE\tNao encontrado para o Account {1}", contCliente.ToString().PadLeft(10, '0'), cli.IdConta));
                                    Console.WriteLine(msgLog.ToString());
                                    IO.EscreverArquivo(arqSaida, msgLog.ToString());
                                }
                                else
                                {
                                    foreach (CaseSalesforce case1 in cases)
                                    {
                                        msgLog.Clear();
                                        lstObjetos.Clear();
                                        sfObj.type = "Case";

                                        if (case1.IsClosed)
                                        {
                                            msgLog.Append(string.Format("{0}\tCASE\tCaso fechado nao pode ser atualizado. Id: {1}", contCliente.ToString().PadLeft(10, '0'), case1.Id));
                                            Console.WriteLine(msgLog.ToString());
                                            IO.EscreverArquivo(arqSaida, msgLog.ToString());
                                            continue;
                                        }

                                        sfObj.Id = case1.Id;

                                        msgLog.Append(string.Format("{0}\tCASE\tId: {1} OLD: {2} - NEW: {3}", contCliente.ToString().PadLeft(10, '0'), case1.Id, case1.AccountId, rs.AccountId));
                                        Console.WriteLine(msgLog.ToString());
                                        IO.EscreverArquivo(arqSaida, msgLog.ToString());

                                        sfObj.Any = new System.Xml.XmlElement[] {
                                        SFDCSchemeBuild.GetNewXmlElement("AccountId", rs.AccountId)
                                    };

                                        lstObjetos.Add(sfObj);

                                        saveResults = binding.update(lstObjetos.ToArray());
                                        resultado = saveResults.First();
                                        if (resultado.errors != null)
                                        {
                                            foreach (Error err in resultado.errors)
                                            {
                                                msgLog.Clear();
                                                msgLog.Append(string.Format("[ERRO 04] Id: {0} {1}", case1.Id, err.message));
                                                Console.WriteLine(msgLog.ToString());
                                                IO.EscreverArquivo(arqSaida, msgLog.ToString());
                                            }
                                        }
                                    }
                                }
                                #endregion CASES


                            }   // fim foreach ClienteSalesforce

                            #endregion ACCOUNT
                        }

                        msgLog.AppendLine(string.Format("Total clientes: {0},  Total Atualizado {1}", contCliente, totalAtualizado));
                        Console.WriteLine(msgLog.ToString());
                        IO.EscreverArquivo(arqSaida, msgLog.ToString());

                    }  //fim arquivoSaida

                    Console.ReadKey();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("\nErro ao atualizar o registro: \n" + ex.Message);
                Debugger.Break();
            }
        }



        [ComVisible(true)]
        internal void HigienizarContasAssets(Arquivo arquivo)
        {
            if (!loggedIn)
            {
                Autenticacao auth = new Autenticacao(this.ambiente);
                if (!auth.ValidarLogin(ref this.binding))
                    return;

                this.loggedIn = true;
            }

            int totalAtualizado = 0;
            int contCliente = 0;
            List<string> lstObjetos = new List<string>();
            StringBuilder auxRecordType = new StringBuilder();
            StringBuilder auxNome = new StringBuilder();

            try
            {
                List<AssetDTO> listaArq = ArquivoLoader.GetAccountIds(arquivo);
                StringBuilder msgLog = new StringBuilder();

                using (Arquivo arqSaida = new Arquivo(arquivo.Caminho, string.Concat("assets_mantidos_03"), arquivo.Extensao))
                {
                    using (Arquivo arqInconsistExternalId = new Arquivo(arquivo.Caminho, string.Concat("assets_apagados_03"), arquivo.Extensao))
                    {
                        msgLog.AppendLine(string.Format("Arquivo carregado em {0}", DateTime.Now.ToLongTimeString()));
                        Console.WriteLine(msgLog.ToString());
                        IO.EscreverArquivo(arqSaida, msgLog.ToString());
                        Console.WriteLine("");


                        foreach (AssetDTO rs in listaArq)
                        {
                            msgLog.Clear();
                            contCliente++;

                            List<AssetDTO> lstAssets = SalesforceDAO.GetAssetsPorAccountId(this.codigoEmpresa, rs.AccountId, ref binding);

                            if (lstAssets != null && lstAssets.Count > 0)
                            {
                                msgLog.Append(rs.AccountId);
                                Console.WriteLine(msgLog);
                                IO.EscreverArquivo(arqSaida, msgLog.ToString().Trim());
                                continue;
                            }
                            else
                            {
                                sObject sfObj = new sObject();
                                msgLog.Clear();
                                lstObjetos.Clear();
                                sfObj.type = "Account";
                                lstObjetos.Add(rs.AccountId);

                                DeleteResult[] saveResults = binding.delete(lstObjetos.ToArray());
                                DeleteResult resultado = saveResults.First();
                                if (resultado.errors != null)
                                {
                                    foreach (Error err in resultado.errors)
                                    {
                                        msgLog.Clear();
                                        msgLog.Append(string.Format("[ERRO] Id: {0} {1}", rs.AccountId, err.message.Trim()));
                                        Console.WriteLine(msgLog.ToString());
                                        IO.EscreverArquivo(arqSaida, msgLog.ToString().Trim());
                                    }
                                }
                                else
                                {
                                    msgLog.Append(string.Format("Account: {0}\tData Criação: {1}\tPoD: {2}", rs.AccountId, lstAssets == null ? "nulo" : lstAssets.FirstOrDefault().DataCriacao, lstAssets == null ? "nulo" : lstAssets.FirstOrDefault().PointofDeliveryId));
                                    Console.WriteLine(msgLog);
                                    IO.EscreverArquivo(arqInconsistExternalId, msgLog.ToString().Trim());
                                }
                                continue;
                            }

                            msgLog.Clear();
                        }   // fim foreach ClienteSalesforce
                    }

                    msgLog.AppendLine(string.Format("Processo terminado em {0}", DateTime.Now.ToLongTimeString()));
                    Console.WriteLine(msgLog.ToString());
                    IO.EscreverArquivo(arqSaida, msgLog.ToString());

                }  //fim arquivoSaida

                Console.ReadKey();
            }
            catch (Exception ex)
            {
                Console.WriteLine("\nErro ao atualizar o registro: \n" + ex.Message);
                Debugger.Break();
            }
        }



        /// <summary>
        /// Atualiza casos de Emergência que tenham ficado sem o número de Aviso do sistema técnico.
        /// </summary>
        /// <remarks>
        /// Fluxo:
        /// - para cada registro de Aviso, no arquivo de entrada enviado pela Técnica
        /// - localizar o Caso no SF
        /// - [EXISTE] 
        ///     consultar número de aviso
        ///     - [NÃO] atualizar o número do aviso no Caso
        ///     - [SIM] próximo Caso
        /// - [NÃO EXISTE]  
        ///     - logar erro pois cenario inválido  
        /// </remarks>
        /// <param name="arquivo"></param>
        internal void AtualizarCasosEmergenciaSemAviso(Arquivo arquivo)
        {
            #region Exemplo com NLog
            //var logger = LogManager.GetLogger("HigienizarContasLog");
            //logger.Info("Hello World");
            //logger.Error("nada");

            //com erro
            //var mail = LogManager.GetLogger("HigienizarContasMail");
            //mail.Error("teste");
            #endregion

            #region Autenticação
            if (!loggedIn)
            {
                Autenticacao auth = new Autenticacao(this.ambiente);
                if (!auth.ValidarLogin(ref this.binding))
                    return;

                this.loggedIn = true;
            }
            #endregion

            int totalAtualizado = 0;
            int contCliente = 0;

            try
            {
                sObject sfObj = null;
                List<sObject> lstObjetos = new List<sObject>();
                List<string> docsConsultados = new List<string>();
                StringBuilder auxRecordType = new StringBuilder();
                StringBuilder auxNome = new StringBuilder();
                StringBuilder msgLog = new StringBuilder();

                using (Arquivo arqSaida = new Arquivo(arquivo.Caminho, string.Concat(arquivo.Nome, "_SAIDA"), arquivo.Extensao))
                {
                    msgLog.Clear();
                    msgLog.AppendLine(string.Format("{0}\tCarregar arquivo de entrada.", DateTime.Now.ToLongTimeString()));
                    Console.WriteLine(msgLog.ToString());
                    IO.EscreverArquivo(arqSaida, msgLog.ToString());

                    List<CaseSalesforce> listaArq = ArquivoLoader.GetCasos(arquivo);

                    msgLog.Clear();
                    msgLog.AppendLine(string.Format("{0}\t{1} registros carregados.", DateTime.Now.ToLongTimeString(), listaArq.Count()));
                    Console.WriteLine(msgLog.ToString());
                    IO.EscreverArquivo(arqSaida, msgLog.ToString());
                    Console.WriteLine("");


                    msgLog.Clear();
                    msgLog.AppendLine(string.Format("{0}\tIdentificando Casos sem Aviso (pode levar alguns minutos)...", DateTime.Now.ToLongTimeString()));
                    Console.WriteLine(msgLog.ToString());
                    IO.EscreverArquivo(arqSaida, msgLog.ToString());
                    List<string> lstNumerosCaso = new List<string>();
                    foreach (CaseSalesforce rs in listaArq)
                    {
                        contCliente++;

                        if(lstNumerosCaso.Count < 200 )
                        {
                            if(!string.IsNullOrWhiteSpace(rs.NumeroCaso))
                                lstNumerosCaso.Add(string.Format("'{0}'", rs.NumeroCaso));

                            if(contCliente != listaArq.Count())
                                continue;
                        }

                        if (lstNumerosCaso.Count == 0)
                        {
                            Debugger.Break();
                            continue;
                        }

                        List<CaseSalesforce> cases = SalesforceDAO.GetCasosByNumeros(lstNumerosCaso, ref binding);
                        lstNumerosCaso.Clear();

                        //TODO: melhorar esse controle de casos pesquisados x retornados
                        // comparar a qtd e, se for menor, listar os que não forem encontrados
                        if (cases == null || cases.Count <= 0)
                        {
                            msgLog.Append(string.Format("{0}\tCASE\tNao encontrado para o Id {1}", totalAtualizado.ToString().PadLeft(10, '0'), rs.NumeroCaso));
                            Console.WriteLine(msgLog.ToString());
                            IO.EscreverArquivo(arqSaida, msgLog.ToString());
                            continue;
                        }

                        foreach (CaseSalesforce casoAtual in cases)
                        {
                            totalAtualizado++;
                            msgLog.Clear();

                            if (!string.IsNullOrWhiteSpace(casoAtual.NumeroAviso))
                            {
                                //msgLog.Append(string.Format("{0}\t[EXISTENTE]\tCASE: {1}\tAVISO: {2}", totalAtualizado.ToString().PadLeft(10, '0'), casoAtual.NumeroCaso, casoAtual.NumeroAviso));
                                //Console.WriteLine(msgLog.ToString());
                                //IO.EscreverArquivo(arqSaida, msgLog.ToString());
                                continue;
                            }

                            if (!string.IsNullOrWhiteSpace(casoAtual.NumeroAvisoIluminacaoPublica))
                            {
                                //msgLog.Append(string.Format("{0}\t[EXISTENTE]\tCASE: {1}\tAVISO IP: {2}", totalAtualizado.ToString().PadLeft(10, '0'), casoAtual.NumeroCaso, casoAtual.NumeroAvisoIluminacaoPublica));
                                //Console.WriteLine(msgLog.ToString());
                                //IO.EscreverArquivo(arqSaida, msgLog.ToString());
                                continue;
                            }

                            string avisoCorreto = listaArq.Where(x => x.NumeroCaso.Equals(casoAtual.NumeroCaso)).FirstOrDefault().NumeroAviso;

                            sfObj = new sObject();
                            //msgLog.Clear();
                            //lstObjetos.Clear();
                            sfObj.type = "Case";
                            sfObj.Id = casoAtual.Id;

                            sfObj.Any = new System.Xml.XmlElement[] {
                                SFDCSchemeBuild.GetNewXmlElement("InserviceNumber__c", avisoCorreto),
                                SFDCSchemeBuild.GetNewXmlElement("InserviceSentError__c", "false")
                            };

                            lstObjetos.Add(sfObj);
                        }
                    }  //fim arquivoSaida

                    msgLog.Clear();
                    msgLog.AppendLine(string.Format("{0}\tAtualizando o Número do Aviso em {1} Casos identificados.", DateTime.Now.ToLongTimeString(), lstObjetos.Count()));
                    Console.WriteLine(msgLog.ToString());
                    IO.EscreverArquivo(arqSaida, msgLog.ToString());

                    SaveResult[] saveResults = SalesforceDAO.Atualizar(lstObjetos, 200, ref binding);
                    SaveResult resultado = saveResults.First();
                    if (resultado.errors != null)
                    {
                        foreach (Error err in resultado.errors)
                        {
                            msgLog.Clear();
                            msgLog.Append(string.Format("[ERRO] Id: {0} {1}", err.fields[0], err.message));
                            Console.WriteLine(msgLog.ToString());
                            IO.EscreverArquivo(arqSaida, msgLog.ToString());
                        }
                    }
                    else
                    {
                        msgLog.Clear();
                        msgLog.Append(string.Format("{0}\tAtualização massiva finalizada.", totalAtualizado.ToString().PadLeft(10, '0')));
                        Console.WriteLine(msgLog.ToString());
                        IO.EscreverArquivo(arqSaida, msgLog.ToString());
                    }



                    msgLog.Clear();
                    msgLog.AppendLine(Environment.NewLine);
                    Console.WriteLine(string.Format("{0}\tProcesso terminado.", DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss")));
                    Console.ReadKey();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("\nErro ao atualizar o registro: \n" + ex.Message);
                Debugger.Break();
            }
        }



        /// <summary>
        /// Atualiza o Billing Profile do cliente com os dados de débito automático cadastrados no Synergia.
        /// </summary>
        /// <remarks>
        /// Atualização pedida por Cleber e Noguti, em 14/02/2019
        /// \EPT_Suporte\180604 Salesforce\190214 Atualizar Debito Automatico
        /// </remarks>
        /// <param name="arquivo"></param>
        internal void AtualizarDebitoAutomatico()
        {
            #region Autenticação
            if (!loggedIn)
            {
                Autenticacao auth = new Autenticacao(this.ambiente);
                if (!auth.ValidarLogin(ref this.binding))
                    return;

                this.loggedIn = true;
            }
            #endregion

            sObject sfObj = null;
            List<sObject> lstObjetos = new List<sObject>();
            StringBuilder auxRecordType = new StringBuilder();
            StringBuilder auxNome = new StringBuilder();

            try
            {
                List<BillingSalesforce> listaArq = ArquivoLoader.GetDebitosAutomaticos(new Arquivo(@"C:\!adl\Ayesa\EPT_Suporte\180604 Salesforce\190214 Atualizar Debito Automatico\DebitosAutomaticos.txt", '|'));
                List<string> docsConsultados = new List<string>();
                StringBuilder msgLog = new StringBuilder();

                using (Arquivo arqSaida = new Arquivo(@"C:\!adl\Ayesa\EPT_Suporte\180604 Salesforce\190214 Atualizar Debito Automatico\DebitosAutomaticos_SAIDA.txt"))
                {
                        msgLog.AppendLine(string.Format("Arquivo carregado em {0}", DateTime.Now.ToLongTimeString()));
                        Console.WriteLine(msgLog.ToString());
                        IO.EscreverArquivo(arqSaida, msgLog.ToString());
                        Console.WriteLine("");

                        foreach (BillingSalesforce rs in listaArq)
                        {
                            msgLog.Clear();
                            lstObjetos.Clear();
                            
                            int numeroClienteAux = 0;
                            if (!Int32.TryParse(rs.NumeroCliente, out numeroClienteAux))
                                continue;

                            //TODO: buscar Id do Billing Profile para o cliente
                            //saber se ha mais de 1 billing por cliente  e, se tiver,  definir o principal
                            List<BillingSalesforce> bills = SalesforceDAO.GetBillingsPorNumeroCliente("BRASIL", this.codigoEmpresa, rs.NumeroCliente, ref binding);
                            DebitoAutomatico debitoAutom = new ConsultarSynergia("COELCE", TipoCliente.GB).ObterDadosBancariosDebitoAutomatico(numeroClienteAux.ToString());

                            if (bills == null || bills.Count <= 0 || debitoAutom == null)
                            {
                                msgLog.Append(string.Format("Debito automatico nao encontrado para o cliente {0}", rs.NumeroCliente));
                                Console.WriteLine(msgLog.ToString());
                                IO.EscreverArquivo(arqSaida, msgLog.ToString());
                            }
                            else
                            {
                                //Deve vir apenas 1 Billing
                                foreach (BillingSalesforce item in bills)
                                {
                                    msgLog.Clear();
                                    lstObjetos.Clear();

                                    sfObj = new sObject();
                                    sfObj.type = "Billing_Profile__c";
                                    sfObj.Id = item.Id;

                                    sfObj.Any = new System.Xml.XmlElement[] {
                                    SFDCSchemeBuild.GetNewXmlElement("Type__c", "Empty")
                                    };
                                    lstObjetos.Add(sfObj);
                                    SaveResult[] saveResults = binding.update(lstObjetos.ToArray());
                                    lstObjetos.Clear();


                                    msgLog.Append(string.Format("{0};{1};{2};{3};{4}", rs.NumeroCliente, bills.FirstOrDefault().Id, debitoAutom.CodigoBanco, debitoAutom.Agencia, debitoAutom.ContaCorrente));
                                    Console.WriteLine(msgLog.ToString());
                                    IO.EscreverArquivo(arqSaida, msgLog.ToString());

                                    sfObj.Any = new System.Xml.XmlElement[] {
                                    SFDCSchemeBuild.GetNewXmlElement("BankBR__c", debitoAutom.CodigoBanco),
                                    SFDCSchemeBuild.GetNewXmlElement("Agency__c", debitoAutom.Agencia),
                                    SFDCSchemeBuild.GetNewXmlElement("CurrentAccountNumber__c", debitoAutom.ContaCorrente),
                                    SFDCSchemeBuild.GetNewXmlElement("BankDocumentOwner__c", debitoAutom.Documento)
                                    };

                                    lstObjetos.Add(sfObj);

                                    saveResults = binding.update(lstObjetos.ToArray());
                                    SaveResult resultado = saveResults.First();
                                    if (resultado.errors != null)
                                    {
                                        foreach (Error err in resultado.errors)
                                        {
                                            msgLog.Clear();
                                            msgLog.Append(string.Format("[ERRO] Id: {0} {1}", item.Id, err.message));
                                            Console.WriteLine(msgLog.ToString());
                                            IO.EscreverArquivo(arqSaida, msgLog.ToString());
                                        }
                                    }


                                    lstObjetos.Clear();
                                    sfObj.Any = new System.Xml.XmlElement[] {
                                    SFDCSchemeBuild.GetNewXmlElement("Type__c", "Automatic debit")
                                    };
                                    lstObjetos.Add(sfObj);
                                    saveResults = binding.update(lstObjetos.ToArray());
                                    lstObjetos.Clear();
                                }
                            }
                        }   // fim foreach ClienteSalesforce

                        Console.WriteLine("Processo Finalizado");
                        Debugger.Break();

                    }  //fim arquivoSaida

                    Console.ReadKey();
            }
            catch (Exception ex)
            {
                Console.WriteLine("\nErro ao atualizar o registro: \n" + ex.Message);
                Debugger.Break();
            }
        }




        [Obsolete]
        /// <summary>
        /// Processo temporário para corrigir External Ids de contas que foram achadas como duplicadas.
        /// Após baixar as contas atuais no Salesforce e procurar por duplicidades ao carregá-las em uma tabela temporaria no informix, 
        /// chega-se a uma lista de contas que possuem o mesmo doc cadastrado mas que foram criadas pelo Delta como contas separadas.
        /// O processo percorre a lista, considera a 1a conta como a principal, corrige os External Ids e despersonaliza as contas que serao unificadas à principal.
        /// </summary>
        /// <param name="arquivo"></param>
        [ComVisible(true)]
        internal void HigienizarContasCorretivo3(Arquivo arquivo)
        {
            const string DESPERSONA = "*DESPERSONA";
            if (!loggedIn)
            {
                Autenticacao auth = new Autenticacao(this.ambiente);
                if (!auth.ValidarLogin(ref this.binding))
                    return;

                this.loggedIn = true;
            }

            int totalAtualizado = 0;
            int contCliente = 0;
            sObject sfObj = null;
            List<sObject> lstObjetos = new List<sObject>();
            StringBuilder auxRecordType = new StringBuilder();
            StringBuilder auxNome = new StringBuilder();

            try
            {
                List<string> docsConsultados = new List<string>();
                List<AssetDTO> listaArq = ArquivoLoader.GetAssets(arquivo);
                List<ClienteSalesforce> baseFull = ArquivoLoader.GetAccounts(new Arquivo("C:\\!adl\\Ayesa\\EPT_Suporte\\180604 Salesforce\\181120 Higienizacao Account CE\\190205 PROD\\accounts_ce.txt", '|'));
                StringBuilder msgLog = new StringBuilder();
                string idAccountPai = string.Empty;

                using (Arquivo arqSaida = new Arquivo(arquivo.Caminho, string.Concat(arquivo.Nome, "_SAIDA"), arquivo.Extensao))
                {
                    using (Arquivo arqInconsistExternalId = new Arquivo(arquivo.Caminho, string.Concat(arquivo.Nome, "_INCONSISTENCIA_EXTERNALID"), arquivo.Extensao))
                    {
                        msgLog.AppendLine(string.Format("Arquivo carregado em {0}", DateTime.Now.ToLongTimeString()));
                        Console.WriteLine(msgLog.ToString());
                        IO.EscreverArquivo(arqSaida, msgLog.ToString());
                        Console.WriteLine("");


                        foreach (AssetDTO rs in listaArq)
                        {
                            if(!docsConsultados.Contains(rs.Identidade))
                                idAccountPai = rs.AccountId;

                            msgLog.Clear();
                            contCliente++;
                            if (contCliente == 10)
                                Debugger.Break();
                            if (string.IsNullOrWhiteSpace(rs.Identidade))
                                continue;

                            if (!rs.AccountExternalId.Contains(rs.Identidade))
                            {
                                //não corrigir o external id para clientes B2B (pessoa jurídica)  :: segundo Bruno, nao convem alterar IDs que ja foram para o SAP
                                if ("002".Equals(rs.TipoIdentidade))
                                {
                                    msgLog.Append(string.Format("{0}\tCNPJ\t{1}|{2}|{3}|{4}|{5}", contCliente.ToString().PadLeft(10, '0'), rs.AccountId, rs.AccountExternalId, rs.Identidade, rs.TipoIdentidade, rs.NomeCliente));
                                    Console.WriteLine(msgLog);
                                    IO.EscreverArquivo(arqInconsistExternalId, msgLog.ToString());
                                }

                                else if (!"005".Equals(rs.TipoIdentidade))
                                {
                                    msgLog.Append(string.Format("{0}\tTIPDOC\t{1}|{2}|{3}|{4}|{5}", contCliente.ToString().PadLeft(10, '0'), rs.AccountId, rs.AccountExternalId, rs.Identidade, rs.TipoIdentidade, rs.NomeCliente));
                                    Console.WriteLine(msgLog);
                                    IO.EscreverArquivo(arqInconsistExternalId, msgLog.ToString());
                                }

                                else if (!Util.ValidaCNPJ(rs.Identidade) && !Util.ValidaCPF(rs.Identidade))
                                {
                                    msgLog.Append(string.Format("{0}\tINVALID\t{1}|{2}|{3}|{4}|{5}", contCliente.ToString().PadLeft(10, '0'), rs.AccountId, rs.AccountExternalId, rs.Identidade, rs.TipoIdentidade, rs.NomeCliente));
                                    Console.WriteLine(msgLog);
                                    IO.EscreverArquivo(arqInconsistExternalId, msgLog.ToString());
                                }

                                else
                                {
                                    //formatar documento para o External Id
                                    string auxDoc = string.Empty;
                                    string auxExternalId = string.Empty;
                                    if ("002".Equals(rs.TipoIdentidade))
                                        auxDoc = Regex.Replace(rs.Identidade, " ", "").PadLeft(22, '0').Substring(22 - 14, 14);
                                    else
                                        auxDoc = Regex.Replace(rs.Identidade, " ", "").PadLeft(22, '0').Substring(22 - 11, 11);

                                    auxExternalId = string.Concat(this.codigoEmpresa, auxDoc, "005".Equals(rs.TipoIdentidade) ? "CPF" : "CNPJ");

                                    msgLog.Append(string.Format("{0}\tEXT_ID\t{1}|{2}|{3}|{4}|{5}|{6}", contCliente.ToString().PadLeft(10, '0'), rs.AccountId, rs.AccountExternalId, rs.Identidade, rs.TipoIdentidade, rs.NomeCliente, auxExternalId));
                                    Console.WriteLine(msgLog);
                                    IO.EscreverArquivo(arqInconsistExternalId, msgLog.ToString());

                                    sfObj = new sObject();
                                    sfObj.type = "Account";
                                    sfObj.Id = rs.AccountId;

                                    #region Corrigir External ID antes de prosseguir
                                    sfObj.Any = new System.Xml.XmlElement[] {
                                    SFDCSchemeBuild.GetNewXmlElement("ExternalID__c", auxExternalId)
                                    };

                                    lstObjetos.Add(sfObj);
                                    SaveResult[] saveRecordType = binding.update(lstObjetos.ToArray());
                                    SaveResult resultado = saveRecordType.First();
                                    if (resultado.errors != null)
                                    {
                                        foreach (Error err in resultado.errors)
                                        {
                                            Console.WriteLine(string.Format("[ERRO 01] AccountId: {0} Documento: {1} {2}", rs.Id, rs.Identidade, err.message));
                                            IO.EscreverArquivo(arqSaida, string.Format("[ERRO 01] AccountId: {0} Documento: {1} {2}", rs.Id, rs.Identidade, err.message));
                                        }
                                    }

                                    #endregion

                                    lstObjetos.Clear();
                                }
                            }

                            //continue;
                            #region ACCOUNT && ASSET ------------------------------



                            //List<ClienteSalesforce> lstCli = SalesforceDAO.GetContasPorDocumento(this.codigoEmpresa, rs.Identidade, ref binding);
                            List<ClienteSalesforce> lstCli = baseFull.Where(c => c.Documento == rs.Identidade).OrderBy(z => z.IdConta).ToList();
                            //List<ClienteSalesforce> lstCli = baseFull.Where(c => c.ExternalId == rs.AccountExternalId).ToList();

                            if (lstCli == null || lstCli.Count <= 1)
                            {
                                msgLog.Append(string.Format("{0}\tACC\tConta unica ou inexistente. AccountId: {1} Doc: {2}", contCliente.ToString().PadLeft(10, '0'), rs.AccountId, rs.Identidade));
                                Console.WriteLine(msgLog.ToString());
                                IO.EscreverArquivo(arqSaida, msgLog.ToString());
                                continue;
                            }

                            sfObj = new sObject();

                            foreach (ClienteSalesforce cli in lstCli)
                            {
                                if (idAccountPai.Equals(cli.IdConta))
                                    continue;

                                if (string.IsNullOrWhiteSpace(cli.TipoCliente) && string.IsNullOrWhiteSpace(rs.TipoCliente))
                                { }

                                if (string.IsNullOrWhiteSpace(cli.TipoRegistroId))
                                {
                                    msgLog.Append(string.Format("{0}\tREC_ID\t{1}|{2}", contCliente.ToString().PadLeft(10, '0'), cli.IdConta, cli.TipoRegistroId));
                                    Console.WriteLine(msgLog);
                                    IO.EscreverArquivo(arqInconsistExternalId, msgLog.ToString());
                                    continue;
                                }

                                sfObj.type = "Account";

                                msgLog.Clear();
                                msgLog.Append(string.Format("{0}\tACC\tOLD: {1} Documento: {2} NEW: {3}", contCliente.ToString().PadLeft(10, '0'), cli.IdConta, cli.Documento, rs.AccountId));
                                Console.WriteLine(msgLog.ToString());
                                IO.EscreverArquivo(arqSaida, msgLog.ToString());

                                sfObj.Id = cli.IdConta;

                                #region Alterar RecordTypeId para B2G
                                auxRecordType.Clear();
                                auxRecordType.Append(cli.TipoRegistroId);
                                sfObj.Any = new System.Xml.XmlElement[] {
                                SFDCSchemeBuild.GetNewXmlElement("Name", (cli.Nome.Length > 40) ? cli.Nome.Substring(0, 40-1) : cli.Nome),
                                SFDCSchemeBuild.GetNewXmlElement("RecordTypeId", "0121o000000oWhZAAU")
                            };

                                lstObjetos.Add(sfObj);
                                SaveResult[] saveRecordType = binding.update(lstObjetos.ToArray());
                                #endregion

                                lstObjetos.Clear();
                                auxNome.Clear();
                                auxNome.AppendFormat("{0}{1}", (cli.Nome.Length > 29 ? cli.Nome.Replace(DESPERSONA, "").Substring(0, 29 - 1) : cli.Nome), DESPERSONA);

                                sfObj.Any = new System.Xml.XmlElement[] {
                                SFDCSchemeBuild.GetNewXmlElement("Name", cli.Nome.Contains(DESPERSONA) ? cli.Nome : auxNome.ToString()),
                                SFDCSchemeBuild.GetNewXmlElement("ExternalId__c", string.Concat(cli.ExternalId.Replace(DESPERSONA,""), DESPERSONA)),
                                SFDCSchemeBuild.GetNewXmlElement("NoWishtoGiveMail__c", "TRUE"),
                                SFDCSchemeBuild.GetNewXmlElement("IdentityNumber__c", " "),
                                SFDCSchemeBuild.GetNewXmlElement("IdentityType__c", " "),
                                SFDCSchemeBuild.GetNewXmlElement("CNT_Resp_ID_Number__c", " "),
                                SFDCSchemeBuild.GetNewXmlElement("CNT_Client_Type__c", string.IsNullOrWhiteSpace(cli.TipoCliente) ? string.IsNullOrWhiteSpace(rs.TipoCliente) ? " " : rs.TipoCliente : cli.TipoCliente)
                                //, SFDCSchemeBuild.GetNewXmlElement("CNT_State_Inscription_Exemption__c", "TRUE")
                            };

                                lstObjetos.Add(sfObj);
                                totalAtualizado++;

                                SaveResult[] saveResults = binding.update(lstObjetos.ToArray());
                                SaveResult resultado = saveResults.First();
                                if (resultado.errors != null)
                                {
                                    foreach (Error err in resultado.errors)
                                    {
                                        Console.WriteLine(string.Format("[ERRO 01] AccountId: {0} Documento: {1} {2}", cli.IdConta, cli.Documento, err.message));
                                        IO.EscreverArquivo(arqSaida, string.Format("[ERRO 01] AccountId: {0} Documento: {1} {2}", cli.IdConta, cli.Documento, err.message));
                                    }
                                }
                                docsConsultados.Add(rs.Identidade);

                                lstObjetos.Clear();

                                #region Restaurar RecordTypeId
                                sfObj.Any = new System.Xml.XmlElement[] {
                                SFDCSchemeBuild.GetNewXmlElement("RecordTypeId", auxRecordType.ToString())
                            };
                                lstObjetos.Add(sfObj);
                                saveRecordType = binding.update(lstObjetos.ToArray());
                                #endregion


                                #region ASSET --------------------------------
                                saveResults = null;
                                lstObjetos.Clear();
                                msgLog.Clear();

                                List<AssetDTO> lstAssets = SalesforceDAO.GetAssetsPorAccountId(this.codigoEmpresa, cli.IdConta, ref binding);
                                if (lstAssets == null || lstAssets.Count <= 0)
                                {
                                    msgLog.Append(string.Format("{0}\tASSET\tNao encontrado para o Account {1}", contCliente.ToString().PadLeft(10, '0'), cli.IdConta));
                                    Console.WriteLine(msgLog.ToString());
                                    IO.EscreverArquivo(arqSaida, msgLog.ToString());
                                }
                                else
                                {
                                    foreach (AssetDTO asset in lstAssets)
                                    {
                                        msgLog.Clear();
                                        lstObjetos.Clear();

                                        if (asset.AccountId.Equals(rs.AccountId))
                                        {
                                            msgLog.Append(string.Format("{0}\tASSET\tId: {1} - Account Iguais: {2}", contCliente.ToString().PadLeft(10, '0'), asset.Id, rs.AccountId));
                                            Console.WriteLine(msgLog.ToString());
                                            IO.EscreverArquivo(arqSaida, msgLog.ToString());
                                        }
                                        else
                                        {
                                            sfObj.type = "Asset";
                                            sfObj.Id = asset.Id;

                                            msgLog.Append(string.Format("{0}\tASSET\tId: {1} OLD: {2} - NEW: {3}", contCliente.ToString().PadLeft(10, '0'), asset.Id, asset.AccountId, rs.AccountId));
                                            Console.WriteLine(msgLog.ToString());
                                            IO.EscreverArquivo(arqSaida, msgLog.ToString());

                                            sfObj.Any = new System.Xml.XmlElement[] {
                                            SFDCSchemeBuild.GetNewXmlElement("AccountId", idAccountPai)
                                        };

                                            lstObjetos.Add(sfObj);

                                            saveResults = binding.update(lstObjetos.ToArray());
                                            resultado = saveResults.First();
                                            if (resultado.errors != null)
                                            {
                                                foreach (Error err in resultado.errors)
                                                {
                                                    Console.WriteLine(string.Format("[ERRO 02] AssetId: {0} AccountId: {1} {2}", asset.Id, rs.AccountId, err.message));
                                                    IO.EscreverArquivo(arqSaida, string.Format("[ERRO 02] AssetId: {0} AccountId: {1} {2}", asset.Id, rs.AccountId, err.message));
                                                }
                                            }
                                        }
                                    }

                                    lstObjetos.Clear();
                                }
                                #endregion ASSET

                                #region BILLING PROFILE -----------------------------------
                                saveResults = null;
                                lstObjetos.Clear();
                                msgLog.Clear();

                                List<BillingSalesforce> bills = SalesforceDAO.GetPerfisFaturamentoByAccoundId(this.codigoEmpresa, cli.IdConta, ref binding);

                                if (bills == null || bills.Count <= 0)
                                {
                                    msgLog.Append(string.Format("{0}\tBILL\tNao encontrado para o Account {1}", contCliente.ToString().PadLeft(10, '0'), cli.IdConta));
                                    Console.WriteLine(msgLog.ToString());
                                    IO.EscreverArquivo(arqSaida, msgLog.ToString());
                                }
                                else
                                {
                                    foreach (BillingSalesforce bill in bills)
                                    {
                                        msgLog.Clear();
                                        lstObjetos.Clear();

                                        sfObj.type = "Billing_Profile__c";
                                        sfObj.Id = bill.Id;

                                        msgLog.Append(string.Format("{0}\tBILL\tId: {1} Cliente: {2} OLD: {3} - NEW: {4}", contCliente.ToString().PadLeft(10, '0'), bill.Id, bill.AccountContract__c, bill.AccountSF, rs.AccountId));
                                        Console.WriteLine(msgLog.ToString());
                                        IO.EscreverArquivo(arqSaida, msgLog.ToString());

                                        sfObj.Any = new System.Xml.XmlElement[] {
                                        SFDCSchemeBuild.GetNewXmlElement("Company__c", this.codigoEmpresa),
                                        SFDCSchemeBuild.GetNewXmlElement("Account__c", idAccountPai)
                                    };

                                        lstObjetos.Add(sfObj);
                                        //string idsUpdate = string.Join(",", lstObjetos.Select(x => new { x.Id }).ToArray());

                                        try
                                        {
                                            saveResults = binding.update(lstObjetos.ToArray());

                                            resultado = saveResults.First();
                                            if (resultado.errors != null)
                                            {
                                                foreach (Error err in resultado.errors)
                                                {
                                                    Console.WriteLine(string.Format("[ERRO 03.1] BillingId: {0} AccountId: {1} {2}", bill.Id, bill.AccountSF, err.message));
                                                    IO.EscreverArquivo(arqSaida, string.Format("[ERRO 03.1] BillingId: {0} AccountId: {1} {2}", bill.Id, bill.AccountSF, err.message));
                                                }
                                            }
                                        }
                                        catch (Exception ex)
                                        {
                                            Console.WriteLine(string.Format("[ERRO 03.2] BillingId: {0} AccountId: {1} {2} {3}", bill.Id, bill.AccountSF, ex.Message, ex.StackTrace));
                                            IO.EscreverArquivo(arqSaida, string.Format("[ERRO 03.2] BillingId: {0} AccountId: {1} {2} {3}", bill.Id, bill.AccountSF, ex.Message, ex.StackTrace));
                                        }
                                    }
                                }
                                #endregion BILLING PROFILE


                                #region CASES -----------------------------------
                                lstObjetos.Clear();
                                msgLog.Clear();

                                List<CaseSalesforce> cases = SalesforceDAO.GetCasesByAccountId(this.codigoEmpresa, cli.IdConta, ref binding);

                                if (cases == null || cases.Count <= 0)
                                {
                                    msgLog.Append(string.Format("{0}\tCASE\tNao encontrado para o Account {1}", contCliente.ToString().PadLeft(10, '0'), cli.IdConta));
                                    Console.WriteLine(msgLog.ToString());
                                    IO.EscreverArquivo(arqSaida, msgLog.ToString());
                                }
                                else
                                {
                                    foreach (CaseSalesforce case1 in cases)
                                    {
                                        msgLog.Clear();
                                        lstObjetos.Clear();
                                        sfObj.type = "Case";

                                        if (case1.IsClosed)
                                        {
                                            msgLog.Append(string.Format("{0}\tCASE\tCaso fechado nao pode ser atualizado. Id: {1}", contCliente.ToString().PadLeft(10, '0'), case1.Id));
                                            Console.WriteLine(msgLog.ToString());
                                            IO.EscreverArquivo(arqSaida, msgLog.ToString());
                                            continue;
                                        }

                                        sfObj.Id = case1.Id;

                                        msgLog.Append(string.Format("{0}\tCASE\tId: {1} OLD: {2} - NEW: {3}", contCliente.ToString().PadLeft(10, '0'), case1.Id, case1.AccountId, rs.AccountId));
                                        Console.WriteLine(msgLog.ToString());
                                        IO.EscreverArquivo(arqSaida, msgLog.ToString());

                                        sfObj.Any = new System.Xml.XmlElement[] {
                                        SFDCSchemeBuild.GetNewXmlElement("AccountId", rs.AccountId)
                                    };

                                        lstObjetos.Add(sfObj);

                                        saveResults = binding.update(lstObjetos.ToArray());
                                        resultado = saveResults.First();
                                        if (resultado.errors != null)
                                        {
                                            foreach (Error err in resultado.errors)
                                            {
                                                msgLog.Clear();
                                                msgLog.Append(string.Format("[ERRO 04] Id: {0} {1}", case1.Id, err.message));
                                                Console.WriteLine(msgLog.ToString());
                                                IO.EscreverArquivo(arqSaida, msgLog.ToString());
                                            }
                                        }
                                    }
                                }
                                #endregion CASES


                            }   // fim foreach ClienteSalesforce

                            #endregion ACCOUNT
                        }

                        msgLog.AppendLine(string.Format("Total clientes: {0},  Total Atualizado {1}", contCliente, totalAtualizado));
                        Console.WriteLine(msgLog.ToString());
                        IO.EscreverArquivo(arqSaida, msgLog.ToString());

                    }  //fim arquivoSaida

                    Console.ReadKey();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("\nErro ao atualizar o registro: \n" + ex.Message);
                Debugger.Break();
            }
        }



        [Obsolete]
        /// <summary>
        /// Processo para refazer a higienização de ACCOUNTs que foram alteradas em parte pelo método original @HigienizarContas, nas suas primeiras execuções.   
        /// Por falha no update, as primeiras Accounts foram despersonalizadas mas os Ativos e outros objetos permaneceram vinculados à Account original.
        /// 1. ler a Account e a nova Account
        /// 2. procurar por ASSET/CASE/BILLING com o account
        /// 3. associar o Asset à nova Account Id
        /// </summary>
        /// <param name="arquivo">ACCOUNT ANTIGO, DOC  :: Arquivo contendo a lista-base de clientes para os quais se deseja parametrizar as demais contas.</param>
        internal void HigienizarContasCorretivo2(Arquivo arquivo)
        {
            const string DESPERSONA = "*DESPERSONA";
            if (!loggedIn)
            {
                Autenticacao auth = new Autenticacao(this.ambiente);
                if (!auth.ValidarLogin(ref this.binding))
                    return;

                this.loggedIn = true;
            }

            int totalAtualizado = 0;
            int contCliente = 0;
            sObject sfObj = null;
            List<sObject> lstObjetos = new List<sObject>();
            StringBuilder auxRecordType = new StringBuilder();
            StringBuilder auxNome = new StringBuilder();

            try
            {
                List<string> docsConsultados = new List<string>();
                List<AssetDTO> listaArq = ArquivoLoader.GetAssets(arquivo);
                List<ClienteSalesforce> baseFull = ArquivoLoader.GetAccounts(new Arquivo("C:\\!adl\\Ayesa\\EPT_Suporte\\180604 Salesforce\\190114 Higienizacao ExternalID\\PROD 190123\\accounts_ce.txt", '|'));
                StringBuilder msgLog = new StringBuilder();

                using (Arquivo arqSaida = new Arquivo(arquivo.Caminho, string.Concat(arquivo.Nome, "_SAIDA"), arquivo.Extensao))
                {
                    using (Arquivo arqInconsistExternalId = new Arquivo(arquivo.Caminho, string.Concat(arquivo.Nome, "_INCONSISTENCIA_EXTERNALID"), arquivo.Extensao))
                    {
                        msgLog.AppendLine(string.Format("Arquivo carregado em {0}", DateTime.Now.ToLongTimeString()));
                        Console.WriteLine(msgLog.ToString());
                        IO.EscreverArquivo(arqSaida, msgLog.ToString());
                        Console.WriteLine("");


                        foreach (AssetDTO rs in listaArq)
                        {
                            msgLog.Clear();
                            contCliente++;

                            //continue;
                            #region ACCOUNT && ASSET ------------------------------

                            //List<ClienteSalesforce> lstCli = SalesforceDAO.GetContasPorDocumento(this.codigoEmpresa, rs.Identidade, ref binding);
                            //List<ClienteSalesforce> lstCli = baseFull.Where(c => c.Documento == rs.Identidade).OrderBy(z => z.IdConta).ToList();
                            //List<ClienteSalesforce> lstCli = baseFull.Where(c => c.ExternalId == rs.AccountExternalId).ToList();
                            //List<ClienteSalesforce> lstCli = baseFull.Where(c => c.IdConta == rs.AccountId).OrderBy(z => z.IdConta).ToList();
                            List<ClienteSalesforce> lstCli = SalesforceDAO.GetContasPorId(this.codigoEmpresa, rs.AccountId, ref binding);

                            sfObj = new sObject();
                            SaveResult[] saveResults = null;
                            SaveResult resultado = null;

                            foreach (ClienteSalesforce cli in lstCli)
                            {
                                List<AssetDTO> lstAssets = SalesforceDAO.GetAssetsPorAccountId(this.codigoEmpresa, rs.AccountId, ref binding);

                                #region ASSET --------------------------------
                                if (lstAssets == null || lstAssets.Count <= 0)
                                {
                                    msgLog.Append(string.Format("{0}\tASSET\tNao encontrado para o Account {1}", contCliente.ToString().PadLeft(10, '0'), rs.AccountId));
                                    Console.WriteLine(msgLog.ToString());
                                    IO.EscreverArquivo(arqSaida, msgLog.ToString());
                                }
                                else
                                {
                                    foreach (AssetDTO asset in lstAssets)
                                    {
                                        msgLog.Clear();
                                        lstObjetos.Clear();

                                            sfObj.type = "Asset";
                                            sfObj.Id = asset.Id;

                                            msgLog.Append(string.Format("{0}\tASSET\tId: {1} OLD: {2} - NEW: {3}", contCliente.ToString().PadLeft(10, '0'), asset.Id, asset.AccountId, rs.Id));
                                            Console.WriteLine(msgLog.ToString());
                                            IO.EscreverArquivo(arqSaida, msgLog.ToString());

                                            sfObj.Any = new System.Xml.XmlElement[] {
                                            SFDCSchemeBuild.GetNewXmlElement("AccountId", rs.Id)
                                        };

                                            lstObjetos.Add(sfObj);

                                            saveResults = binding.update(lstObjetos.ToArray());
                                            resultado = saveResults.First();
                                            if (resultado.errors != null)
                                            {
                                                foreach (Error err in resultado.errors)
                                                {
                                                    Console.WriteLine(string.Format("[ERRO 02] AssetId: {0} AccountId: {1} {2}", asset.Id, rs.AccountId, err.message));
                                                    IO.EscreverArquivo(arqSaida, string.Format("[ERRO 02] AssetId: {0} AccountId: {1} {2}", asset.Id, rs.AccountId, err.message));
                                                }
                                            }
                                    }

                                    lstObjetos.Clear();
                                }
                                #endregion ASSET


                                #region BILLING PROFILE -----------------------------------
                                saveResults = null;
                                lstObjetos.Clear();
                                msgLog.Clear();

                                List<BillingSalesforce> bills = SalesforceDAO.GetPerfisFaturamentoByAccoundId(this.codigoEmpresa, rs.AccountId, ref binding);

                                if (bills == null || bills.Count <= 0)
                                {
                                    msgLog.Append(string.Format("{0}\tBILL\tNao encontrado para o Account {1}", contCliente.ToString().PadLeft(10, '0'), rs.AccountId));
                                    Console.WriteLine(msgLog.ToString());
                                    IO.EscreverArquivo(arqSaida, msgLog.ToString());
                                }
                                else
                                {
                                    foreach (BillingSalesforce bill in bills)
                                    {
                                        msgLog.Clear();
                                        lstObjetos.Clear();

                                        sfObj.type = "Billing_Profile__c";
                                        sfObj.Id = bill.Id;

                                        msgLog.Append(string.Format("{0}\tBILL\tId: {1} Cliente: {2} OLD: {3} - NEW: {4}", contCliente.ToString().PadLeft(10, '0'), bill.Id, bill.AccountContract__c, bill.AccountSF, rs.Id));
                                        Console.WriteLine(msgLog.ToString());
                                        IO.EscreverArquivo(arqSaida, msgLog.ToString());

                                        sfObj.Any = new System.Xml.XmlElement[] {
                                        SFDCSchemeBuild.GetNewXmlElement("Company__c", this.codigoEmpresa),
                                        SFDCSchemeBuild.GetNewXmlElement("Account__c", rs.Id)
                                    };

                                        lstObjetos.Add(sfObj);
                                        //string idsUpdate = string.Join(",", lstObjetos.Select(x => new { x.Id }).ToArray());

                                        try
                                        {
                                            saveResults = binding.update(lstObjetos.ToArray());

                                            resultado = saveResults.First();
                                            if (resultado.errors != null)
                                            {
                                                foreach (Error err in resultado.errors)
                                                {
                                                    Console.WriteLine(string.Format("[ERRO 03.1] BillingId: {0} AccountId: {1} {2}", bill.Id, bill.AccountSF, err.message));
                                                    IO.EscreverArquivo(arqSaida, string.Format("[ERRO 03.1] BillingId: {0} AccountId: {1} {2}", bill.Id, bill.AccountSF, err.message));
                                                }
                                            }
                                        }
                                        catch (Exception ex)
                                        {
                                            Console.WriteLine(string.Format("[ERRO 03.2] BillingId: {0} AccountId: {1} {2} {3}", bill.Id, bill.AccountSF, ex.Message, ex.StackTrace));
                                            IO.EscreverArquivo(arqSaida, string.Format("[ERRO 03.2] BillingId: {0} AccountId: {1} {2} {3}", bill.Id, bill.AccountSF, ex.Message, ex.StackTrace));
                                        }
                                    }
                                }
                                #endregion BILLING PROFILE


                                #region CASES -----------------------------------
                                lstObjetos.Clear();
                                msgLog.Clear();

                                List<CaseSalesforce> cases = SalesforceDAO.GetCasesByAccountId(this.codigoEmpresa, rs.AccountId, ref binding);

                                if (cases == null || cases.Count <= 0)
                                {
                                    msgLog.Append(string.Format("{0}\tCASE\tNao encontrado para o Account {1}", contCliente.ToString().PadLeft(10, '0'), rs.AccountId));
                                    Console.WriteLine(msgLog.ToString());
                                    IO.EscreverArquivo(arqSaida, msgLog.ToString());
                                }
                                else
                                {
                                    foreach (CaseSalesforce case1 in cases)
                                    {
                                        msgLog.Clear();
                                        lstObjetos.Clear();
                                        sfObj.type = "Case";

                                        if (case1.IsClosed)
                                        {
                                            msgLog.Append(string.Format("{0}\tCASE\tCaso fechado nao pode ser atualizado. Id: {1}", contCliente.ToString().PadLeft(10, '0'), case1.Id));
                                            Console.WriteLine(msgLog.ToString());
                                            IO.EscreverArquivo(arqSaida, msgLog.ToString());
                                            continue;
                                        }

                                        sfObj.Id = case1.Id;

                                        msgLog.Append(string.Format("{0}\tCASE\tId: {1} OLD: {2} - NEW: {3}", contCliente.ToString().PadLeft(10, '0'), case1.Id, case1.AccountId, rs.Id));
                                        Console.WriteLine(msgLog.ToString());
                                        IO.EscreverArquivo(arqSaida, msgLog.ToString());

                                        sfObj.Any = new System.Xml.XmlElement[] {
                                        SFDCSchemeBuild.GetNewXmlElement("AccountId", rs.Id)
                                    };

                                        lstObjetos.Add(sfObj);

                                        saveResults = binding.update(lstObjetos.ToArray());
                                        resultado = saveResults.First();
                                        if (resultado.errors != null)
                                        {
                                            foreach (Error err in resultado.errors)
                                            {
                                                msgLog.Clear();
                                                msgLog.Append(string.Format("[ERRO 04] Id: {0} {1}", case1.Id, err.message));
                                                Console.WriteLine(msgLog.ToString());
                                                IO.EscreverArquivo(arqSaida, msgLog.ToString());
                                            }
                                        }
                                    }
                                }
                                #endregion CASES


                            }   // fim foreach ClienteSalesforce

                            #endregion ACCOUNT
                        }

                        msgLog.AppendLine(string.Format("Total clientes: {0},  Total Atualizado {1}", contCliente, totalAtualizado));
                        Console.WriteLine(msgLog.ToString());
                        IO.EscreverArquivo(arqSaida, msgLog.ToString());

                    }  //fim arquivoSaida

                    Console.ReadKey();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("\nErro ao atualizar o registro: \n" + ex.Message);
                Debugger.Break();
            }
        }



        [Obsolete]
        /// <summary>
        /// Processo para refazer a higienização de ASSETs de Accounts que tiveram sua busca afetada por err no método original @HigienizarContas.   
        /// Ficou um código de Account fixo no OSQL ('00136000012TV5PAAW').
        /// 1. procurar ASSET com o account
        /// 2. se existir Asset, buscar o novo Account ID para o DOC
        /// 3. associar o Asset ao novo Account Id
        /// </summary>
        /// <param name="arquivo">ACCOUNT ANTIGO, DOC  :: Arquivo contendo a lista-base de clientes para os quais se deseja parametrizar as demais contas.</param>
        internal void HigienizarContasCorretivo(Arquivo arquivo)
        {
            const string DESPERSONA = "*DESPERSONA";
            if (!loggedIn)
            {
                Autenticacao auth = new Autenticacao(this.ambiente);
                if (!auth.ValidarLogin(ref this.binding))
                    return;

                this.loggedIn = true;
            }

            int totalAtualizado = 0;
            int contCliente = 0;
            sObject sfObj = null;
            List<sObject> lstObjetos = new List<sObject>();
            StringBuilder auxRecordType = new StringBuilder();
            StringBuilder auxNome = new StringBuilder();

            try
            {
                List<AssetDTOCorretivo> listaArq = ArquivoLoader.GetAssetsCorretivo(arquivo);
                StringBuilder msgLog = new StringBuilder();

                using (Arquivo arqSaida = new Arquivo(arquivo.Caminho, string.Concat(arquivo.Nome, "_SAIDA_CORRETIVO"), arquivo.Extensao))
                {
                    msgLog.AppendLine(string.Format("Arquivo carregado em {0}", DateTime.Now.ToLongDateString()));
                    Console.WriteLine(msgLog.ToString());
                    IO.EscreverArquivo(arqSaida, msgLog.ToString());

                    foreach (AssetDTOCorretivo rs in listaArq)
                    {
                        if (string.IsNullOrWhiteSpace(rs.Identidade))
                            continue;

                        msgLog.Clear();
                        contCliente++;

                        List<AssetDTO> lstAssetsRuins = SalesforceDAO.GetAssetsPorAccountId(this.codigoEmpresa, rs.AccountId, ref binding);
                        if (lstAssetsRuins == null || lstAssetsRuins.Count <= 0)
                        {
                            msgLog.Append(string.Format("{0}\tACC\tAssets nao encontrados. AccountId: {1} Doc: {2}", contCliente.ToString().PadLeft(6, '0'), rs.AccountId, rs.Identidade));
                            Console.WriteLine(msgLog.ToString());
                            IO.EscreverArquivo(arqSaida, msgLog.ToString());
                            continue;   //<<<<---- ERRO SE CAIR AQUI
                        }

                        #region ACCOUNT && ASSET ------------------------------

                        List<ClienteSalesforce> lstCli = SalesforceDAO.GetContasPorDocumento(this.codigoEmpresa, rs.Identidade, ref binding);
                        if (lstCli == null || lstCli.Count <= 0)
                        {
                            //<<<<<<<<<<<<< ------------ ERRO
                            msgLog.Append(string.Format("{0}\tACC\tConta unica ou inexistente. AccountId: {1} Doc: {2}", contCliente.ToString().PadLeft(6, '0'), rs.AccountId, rs.Identidade));
                            Console.WriteLine(msgLog.ToString());
                            IO.EscreverArquivo(arqSaida, msgLog.ToString());
                            continue;
                        }

                        if(lstCli.Count > 1)
                        {

                        }

                        sfObj = new sObject();
                        string accountPai = string.Empty;
                        foreach (ClienteSalesforce cliPai in lstCli)
                        {
                            if (!cliPai.IdConta.Equals(rs.AccountId))
                                accountPai = cliPai.IdConta;
                        }

                        if(string.IsNullOrWhiteSpace(accountPai))
                        {
                            msgLog.Append(string.Format("{0}\tASSET\tAccountId Principal nao localizado.   AccountId OLD: {1}  Doc: {2+++}", contCliente.ToString().PadLeft(6, '0'), rs.AccountId, rs.Identidade));
                            Console.WriteLine(msgLog.ToString());
                            IO.EscreverArquivo(arqSaida, msgLog.ToString());
                            continue;
                        }


                            #region ASSET --------------------------------
                            lstObjetos.Clear();
                            msgLog.Clear();

                            //buscar os assets com AccountId errado, antigo, ruim
                            List<AssetDTO> lstAssets = SalesforceDAO.GetAssetsPorAccountId(this.codigoEmpresa, rs.AccountId, ref binding);

                            if (lstAssets == null || lstAssets.Count <= 0)
                            {
                                msgLog.Append(string.Format("{0}\tASSET\tNao encontrado para o Account {1}", contCliente.ToString().PadLeft(6, '0'), accountPai));
                                Console.WriteLine(msgLog.ToString());
                                IO.EscreverArquivo(arqSaida, msgLog.ToString());
                            }
                            else
                            {
                                foreach (AssetDTO asset in lstAssets)
                                {
                                    msgLog.Clear();
                                    lstObjetos.Clear();

                                    sfObj.type = "Asset";
                                    sfObj.Id = asset.Id;

                                    msgLog.Append(string.Format("{0}\tASSET\tId: {1} OLD: {2} - NEW: {3}", contCliente.ToString().PadLeft(6, '0'), asset.Id, asset.AccountId, accountPai));
                                    Console.WriteLine(msgLog.ToString());
                                    IO.EscreverArquivo(arqSaida, msgLog.ToString());

                                    sfObj.Any = new System.Xml.XmlElement[] {
                                        SFDCSchemeBuild.GetNewXmlElement("AccountId", accountPai)
                                    };

                                    lstObjetos.Add(sfObj);

                                    SaveResult[] saveResults = binding.update(lstObjetos.ToArray());
                                    SaveResult resultado = saveResults.First();
                                    saveResults = binding.update(lstObjetos.ToArray());
                                    resultado = saveResults.First();
                                    if (resultado.errors != null)
                                    {
                                        foreach (Error err in resultado.errors)
                                        {
                                            Console.WriteLine(string.Format("[ERRO 02] AssetId: {0} AccountId: {1} {2}", asset.Id, rs.AccountId, err.message));
                                            IO.EscreverArquivo(arqSaida, string.Format("[ERRO 02] AssetId: {0} AccountId: {1} {2}", asset.Id, rs.AccountId, err.message));
                                        }
                                    }
                                }

                                lstObjetos.Clear();
                            }
                            #endregion ASSET

                        #endregion ACCOUNT
                    }

                    msgLog.AppendLine("\nProcesso finalizado");
                    Console.WriteLine(msgLog.ToString());
                    IO.EscreverArquivo(arqSaida, msgLog.ToString());

                }  //fim arquivoSaida

                Console.ReadKey();
            }
            catch (Exception ex)
            {
                Console.WriteLine("\nErro ao atualizar o registro: \n" + ex.Message);
            }
        }



        internal void EliminarDuplicidadesSalesforceUra(Arquivo arquivo)
        {
            if (!loggedIn)
            {
                Autenticacao auth = new Autenticacao(this.ambiente);
                if (!auth.ValidarLogin(ref this.binding))
                    return;

                this.loggedIn = true;
            }

            int i = 0;
            int totalAtualizado = 0;
            int totalCasos = 0;
            //sObject sfObj = null;
            //Dictionary<string, string> dicTask = new Dictionary<string, string>();
            List<string> lstCasos = new List<string>();
            List<string> lstIdAux = new List<string>();
            List<string> lstIds = new List<string>();

            //bool flOk = false;

            try
            {
                List<AtendimentoURA> listaArq = ArquivoLoader.GetAtendimentosURA(arquivo);
                StringBuilder msgLog = new StringBuilder();

                using (Arquivo arqSaida = new Arquivo(arquivo.Caminho, string.Concat(arquivo.Nome, "_SAIDA"), arquivo.Extensao))
                {
                    foreach (AtendimentoURA rs in listaArq)
                    {
                        //flOk = false;
                        msgLog.Clear();
                        totalCasos++;

                        //if (listaArq
                        //    .Where(x => x.NumeroCaso == rs.NumeroCaso)
                        //    .Where(x => x.NumeroAtividade.Contains("/0")).Count() > 0
                        //    && !rs.NumeroAtividade.Contains("/0"))
                        //{
                        //    //possui "/0"
                        //    lstCasos.Add(rs.NumeroCaso);
                        //    dicTask.Add(rs.IdAtividade, rs.NumeroCaso);
                        //    flOk = true;
                        //}
                        

                        var grupos = listaArq.Where(c => c.NumeroCaso == rs.NumeroCaso).GroupBy(d => new { d.DataHora }).ToArray();
                        
                        if (lstCasos.Contains(rs.NumeroCaso))
                            continue;
                        lstCasos.Add(rs.NumeroCaso);

                        var menorIdAtividade = string.Empty;
                        foreach(var lista in grupos)
                        {
                            //var data = grupos.Key;
                            if (lista.Count() > 1)
                                // Debugger.Break();

                            foreach(var item in lista)
                            {
                                lstIds.Add(item.IdAtividade);
                            }

                            menorIdAtividade = lista.Where(c => c.NumeroAtividade == lista.Min(x => x.NumeroAtividade)).FirstOrDefault().IdAtividade;
                            lstIds.Remove(menorIdAtividade);
                            
                            //if (grupos.Count() > 1)
                            //    Debugger.Break();

                            //if (lstIds.Count > 1)
                            //    Debugger.Break();
                        }

                        msgLog.Append(string.Format("Caso: {0}\t [{1}]\t {2}\t Manter Id: {3}", rs.NumeroCaso, grupos.Count(), String.Join(",", grupos.Select(l => new { l.Key.DataHora })), menorIdAtividade));
                        Console.WriteLine(msgLog.ToString());
                        IO.EscreverArquivo(arqSaida, msgLog.ToString());

                        //totalAtualizado++;

                        //if (flOk)
                        //{
                        //    msgLog.AppendLine(string.Format("{0} :: {1} - {2}", totalAtualizado, rs.IdAtividade, rs.NumeroAtividade));
                        //    Console.Write(msgLog.ToString());
                        //    IO.EscreverArquivo(arqSaida, msgLog.ToString());
                        //}
                    }


                    //foreach (string id in dicTask.Keys)
                    foreach (string id in lstIds)
                    {
                        i++;
                        lstIdAux.Add(id);

                        if (i == 199)
                        {
                            string[] listaTemp = lstIdAux.ToArray();
                            string idsUpdate = string.Join(",", lstIdAux.ToArray());
                            try
                            {
                                DeleteResult[] saveResults = binding.delete(listaTemp);
                                List<Error[]> arrErros = saveResults.Where(e => e.errors != null).Select(t => t.errors).ToList();
                                if (arrErros != null && arrErros.Count > 0)
                                {
                                    foreach (Error[] err1 in arrErros)
                                    {
                                        foreach (Error err2 in err1)
                                        {
                                        Console.WriteLine(string.Format("{0}\t[ERRO] {1} {2}", DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss"), idsUpdate, err2.message));
                                        IO.EscreverArquivo(arqSaida, string.Format("{0}\t[ERRO] {1} {2}", DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss"), idsUpdate, err2.message));
                                        }
                                    }
                                }
                                else
                                {
                                    Console.WriteLine(string.Format("{0}\t[EXCLUIDO] {1}", DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss"), idsUpdate));
                                    IO.EscreverArquivo(arqSaida, string.Format("{0}\t[OK] {1}", DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss"), idsUpdate));
                                }
                                i = 0;
                                lstIdAux.Clear();
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine(string.Format("{0}\t[EX] {1} {2}", DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss"), idsUpdate, ex.Message));
                                IO.EscreverArquivo(arqSaida, string.Format("{0}\t[EX] {1} {2}", DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss"), idsUpdate, ex.Message));
                            }
                        }
                    }

                    if (i < 199)
                    {
                        string[] listaTemp = lstIdAux.ToArray();
                        string idsUpdate = string.Join(",", lstIdAux.ToArray());
                        try
                        {
                            DeleteResult[] saveResults = binding.delete(listaTemp);
                            List<Error[]> arrErros = saveResults.Where(e => e.errors != null).Select(t => t.errors).ToList();
                            if (arrErros != null && arrErros.Count > 0)
                            {
                                foreach (Error[] err1 in arrErros)
                                {
                                    foreach (Error err2 in err1)
                                    {
                                        Console.WriteLine(string.Format("{0}\t[ERRO] {1} {2}", DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss"), idsUpdate, err2.message));
                                        IO.EscreverArquivo(arqSaida, string.Format("{0}\t[ERRO] {1} {2}", DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss"), idsUpdate, err2.message));
                                    }
                                }
                            }
                            else
                            {
                                Console.WriteLine(string.Format("{0}\t[EXCLUIDO] {1}", DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss"), idsUpdate));
                                IO.EscreverArquivo(arqSaida, string.Format("{0}\t[OK] {1}", DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss"), idsUpdate));
                            }

                            i = 0;
                            lstIdAux.Clear();
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(string.Format("{0}\t[EX] {1} {2}", DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss"), idsUpdate, ex.Message));
                            IO.EscreverArquivo(arqSaida, string.Format("{0}\t[EX] {1} {2}", DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss"), idsUpdate, ex.Message));
                        }
                    }

                    msgLog.AppendLine(string.Format("\nTotal clientes: {0}", totalCasos));
                    Console.WriteLine(msgLog.ToString());
                    IO.EscreverArquivo(arqSaida, msgLog.ToString());
                }

                //Console.ReadKey();
            }
            catch (Exception ex)
            {
                Console.WriteLine("\nErro ao atualizar o registro: \n" + ex.Message);
            }
        }



        [ComVisible(true)]
        internal void ConsultarAccountsPorExternalId()
        {
            int contadorTotal = 0;
            //Login no serviço
            if (!loggedIn)
            {
                Autenticacao auth = new Autenticacao(this.ambiente);
                if (!auth.ValidarLogin(ref this.binding))
                    return;

                this.loggedIn = true;
            }

            QueryResult qr = null;
            binding.QueryOptionsValue = new SalesforceExtractor.apex.QueryOptions();
            binding.QueryOptionsValue.batchSize = 3;
            binding.QueryOptionsValue.batchSizeSpecified = true;

            StringBuilder arquivoSaida = new StringBuilder(@"C:\!adl\Ayesa\EPT_Suporte\180604 Salesforce\190225 Desfazedor de Merda\180319 Contacts Nao Carregados\CONTACTS\190320-2\AccountExternalIds.txt");

            try
            {
                string cliente = "";

                StreamReader sr = new StreamReader(@"C:\!adl\Ayesa\EPT_Suporte\180604 Salesforce\190225 Desfazedor de Merda\180319 Contacts Nao Carregados\CONTACTS\190320-2\ExternalIds.txt");
                while (!sr.EndOfStream)
                {
                    #region obsoleto
                    //int contadorAux = 0;
                    //for (int i = 1; i <= 50; i++)
                    //{
                    //    contadorAux = i;
                    //    try
                    //    {
                    //        string registro = sr.ReadLine();
                    //        if (cliente.Length > 0 && !string.IsNullOrEmpty(registro))
                    //            cliente += ", ";

                    //        cliente += "'" + registro.Trim().Replace("\"", "") + "'";
                    //    }
                    //    catch
                    //    {
                    //        contadorTotal--;
                    //    }
                    //}

                    //contadorTotal += contadorAux;

                    ////cliente = sr.ReadLine().Replace('|', ' ').Trim() ;
                    #endregion

                    string registro = sr.ReadLine();

                    String sql = string.Format(@"select externalId__c
                                                   from account
                                                  where externalid__c = '{0}'", registro);
                    bool bContinue = false;
                    try
                    {
                        qr = binding.query(sql);
                        bContinue = true;
                    }
                    catch (Exception ex)
                    {
                        Debugger.Break();
                        throw ex;
                    }

                    cliente = "";

                    while (bContinue)
                    {

                        if (qr.records == null)
                        {
                            IO.EscreverArquivo(arquivoSaida.ToString(), string.Concat("[NAO CADASTRADO]\t", registro, "\n"));
                            bContinue = false;
                            continue;
                        }

                        for (int i = 0; i < qr.records.Length; i++)
                        {
                            SalesforceExtractor.apex.sObject con = qr.records[i];

                            String id_externo_conta = "";
                            id_externo_conta = schema.getFieldValue("externalid__c", con.Any);

                            String reg = "";
                            reg += id_externo_conta ;

                            IO.EscreverArquivo(arquivoSaida.ToString(), string.Concat("[OK]\t\t", reg, "\n"));
                        }

                        //handle the loop + 1 problem by checking to see if the most recent queryResult
                        if (qr.done)
                        {
                            bContinue = false;
                            //Console.WriteLine("Registrou: " + loopCounter);
                        }
                        else
                            qr = binding.queryMore(qr.queryLocator);
                    }

                    int resto = 0;
                    Math.DivRem(contadorTotal, 50, out resto);

                    if (resto == 0)
                        Console.WriteLine(string.Concat(contadorTotal, " às ", DateTime.Now.ToShortTimeString()));
                }

                Console.WriteLine(string.Format("\nTotal clientes extraídos: {0}", contadorTotal));
            }
            catch (Exception ex)
            {
                IO.EscreverArquivo(arquivoSaida.ToString(), string.Concat(ex.Message, ex.StackTrace) + "\n");
                Console.WriteLine(string.Format("\nProblema para executar a query: {0}\n", ex.Message));
            }
        }



        /// <summary>
        /// Dada uma lista de clientes, carrega os dados de B2Win obtidos via consulta no Synergia, atualizando Order, Item Order e Asset/Order Attributes
        /// <origem>C:\!adl\Ayesa\EPT_Suporte\180604 Salesforce\190325 Carga Bit2Win</origem>
        /// </summary>
        /// <param name="empresa"></param>
        /// <param name="nomeArquivo"></param>
        /// <param name="numeroCliente"></param>
        /// <param name="lote"></param>
        /// <param name="tipoCliente"></param>
        /// <param name="temCabecalho"></param>
        [ComVisible(true)]
        public void CarregarB2WinPorCliente(string empresa, string nomeArquivo, string numeroCliente, int lote, TipoCliente tipoCliente, bool temCabecalho)
        {
            #region Histório de execução
            //Arquivo arquivo = new Arquivo(@"C:\temp\CargaB2Win\190327\b2win.txt", ';');
            //Arquivo arquivo = new Arquivo(@"C:\temp\CargaB2Win\190327\b2win02.txt", ';');
            //Arquivo arquivo = new Arquivo(@"C:\temp\CargaB2Win\190327\6340029.txt", '|');
            //Arquivo arquivo = new Arquivo(string.Format(@"C:\temp\CargaB2Win\190327\b2win03.txt", '|');
            //Arquivo arquivo = new Arquivo(string.Format(@"C:\temp\CargaB2Win\190327\{0}", nomeArquivo), '|');
            #endregion
            Arquivo arqSaida = new Arquivo(string.Format("{0}_{1}.txt", nomeArquivo.ToLower().Replace(".txt", ""), DateTime.Now.ToString("yyyyMMdd_mmss")));

            List<string> lstClientes = numeroCliente.Split(',').ToList();

            List<ItemAttribute> lstB2Win = new ConsultarSynergia(empresa, tipoCliente).ObterItemsAttribute(lstClientes, lote, typeof(B2WinDTO));
            if (lstB2Win.Count == 0)
            {
                string msgLogs = string.Format("{0}\tDados não encontrados para os clientes informados.", DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss"));
                Console.WriteLine(msgLogs);
                IO.EscreverArquivo(arqSaida, string.Join(Environment.NewLine, msgLogs));
                return;
            }

            List<string> lstClientesEncontrados = lstB2Win.Select(x => { return x.NumeroCliente; }).ToList();
            List<string> lstClientesNaoEncontrados = lstClientes.Except(lstClientesEncontrados).ToList();

            if (lstClientesNaoEncontrados != null && lstClientesNaoEncontrados.Count > 0)
            {
                string msgLogs = string.Format("[ERRO] Clientes não encontrados: {0}", string.Join(",", lstClientesNaoEncontrados.ToArray()));
                Console.WriteLine(msgLogs);
                IO.EscreverArquivo(arqSaida, string.Join(Environment.NewLine, msgLogs));
            }

            
            CarregarB2Win(empresa, lstB2Win, lote, tipoCliente, arqSaida);
        }


        /// <summary>
        /// Dada uma lista de OrderItemId's, consulta no Salesforce respectivos clientes e, então, carrega os dados de B2Win obtidos via consulta no Synergia, atualizando Order, Item Order e Asset/Order Attributes
        /// </summary>
        /// <param name="empresa"></param>
        /// <param name="nomeArquivo"></param>
        /// <param name="entrada"></param>
        /// <param name="tipoCliente"></param>
        /// <param name="temCabecalho"></param>
        [ComVisible(true)]
        public void CarregarB2WinPorOrder(string empresa, string nomeArquivo, string entrada, TipoCliente tipoCliente, bool temCabecalho)
        {
            if (!loggedIn)
            {
                Autenticacao auth = new Autenticacao(this.ambiente);
                if (!auth.ValidarLogin(ref this.binding))
                    return;

                this.loggedIn = true;
            }

            #region Histório de execução
            //Arquivo arquivo = new Arquivo(@"C:\temp\CargaB2Win\190327\b2win.txt", ';');
            //Arquivo arquivo = new Arquivo(@"C:\temp\CargaB2Win\190327\b2win02.txt", ';');
            //Arquivo arquivo = new Arquivo(@"C:\temp\CargaB2Win\190327\6340029.txt", '|');
            //Arquivo arquivo = new Arquivo(string.Format(@"C:\temp\CargaB2Win\190327\b2win03.txt", '|');
            //Arquivo arquivo = new Arquivo(string.Format(@"C:\temp\CargaB2Win\190327\{0}", nomeArquivo), '|');
            #endregion

            Arquivo arqSaida = new Arquivo(string.Format("{0}.txt", nomeArquivo.ToLower().Replace(".txt", "")));

            //List<string> lstOrderNumbers = entrada.Split(new char[] { ',' }).ToList();
            ////===============================================================
            ////---------------------- BUSCAR ORDERS --------------------------
            ////===============================================================
            ////List<ItemAttribute> listaArq = new ConsultarSynergia(empresa, tipoCliente).ObterItemsAttributePorOrder(lstOrders, typeof(B2WinDTO));
            //List<string> lstOrders = SalesforceDAO.GetOrdersPorOrderItemId(string.Concat("'", string.Join("','", lstOrderNumbers.ToArray()), "'"), ref this.binding);
            //if (lstOrders.Count == 0)
            //{
            //    Console.WriteLine("[CONSULTA ORDERS] Dados não encontrados para os Orders informados.");
            //    return;
            //}

            #region Orders não encontrados
            //List<string> lstOrderNaoEncontrados = lstOrderNumbers.Except(lstOrders).ToList();

            //if (lstOrderNaoEncontrados != null && lstOrderNaoEncontrados.Count > 0)
            //{
            //    string msgLogs = string.Format("Clientes não encontrados ao buscar o B2Win: {0}", string.Join(",", lstOrderNaoEncontrados.ToArray()));
            //    Console.WriteLine(msgLogs);
            //    IO.EscreverArquivo(arqSaida, string.Join(Environment.NewLine, msgLogs));
            //}
            #endregion


            //List<string> lstOrders = entrada.Split(new char[] { ',' }).ToList();
            ////=================================================================
            ////----------------------- BUSCAR CLIENTES -------------------------
            ////=================================================================
            ////List<ItemAttribute> listaArq = new ConsultarSynergia(empresa, tipoCliente).ObterItemsAttributePorOrder(lstOrders, typeof(B2WinDTO));
            //List<AssetDTO> lstAssets = SalesforceDAO.GetAssetsPorOrderId(string.Concat("'", string.Join("','", lstOrders.ToArray()), "'"), ref this.binding);
            //if (lstAssets.Count == 0)
            //{
            //    Console.WriteLine("[CONSULTA CLIENTES] Dados não encontrados para os Orders informados.");
            //    return;
            //}


            List<string> lstClientes = entrada.Split(new char[] { ',' }).ToList();
            //===============================================================
            //----------------------- BUSCAR B2WIN --------------------------
            //===============================================================
            //List<string> lstClientes = lstAssets.Where(c => !string.IsNullOrWhiteSpace(c.NumeroCliente)).Select(x => { return x.NumeroCliente; }).ToList();
            List<ItemAttribute> listaArq = new ConsultarSynergia(empresa, tipoCliente).ObterItemsAttribute(lstClientes, 0, typeof(B2WinDTO));
            if(listaArq == null || listaArq.Count == 0)
            {
                Console.WriteLine(string.Format("[ERRO] Dados não encontrados na CONSULTA B2WIN para os clientes informados: {0}", string.Join(",", lstClientes.ToArray())));
                return;
            }
            CarregarB2Win(empresa, listaArq, 0, tipoCliente, arqSaida);
        }



        [ComVisible(true)]
        public void CarregarB2WinPorOrderItem(string empresa, string nomeArquivo, TipoCliente tipoCliente, bool temCabecalho)
        {
            //if (!loggedIn)
            //{
            //    Autenticacao auth = new Autenticacao(this.ambiente);
            //    if (!auth.ValidarLogin(ref this.binding))
            //        return;

            //    this.loggedIn = true;
            //}

            #region Histório de execução
            //Arquivo arquivo = new Arquivo(@"C:\temp\CargaB2Win\190327\b2win.txt", ';');
            //Arquivo arquivo = new Arquivo(@"C:\temp\CargaB2Win\190327\b2win02.txt", ';');
            //Arquivo arquivo = new Arquivo(@"C:\temp\CargaB2Win\190327\6340029.txt", '|');
            //Arquivo arquivo = new Arquivo(string.Format(@"C:\temp\CargaB2Win\190327\b2win03.txt", '|');
            //Arquivo arquivo = new Arquivo(string.Format(@"C:\temp\CargaB2Win\190327\{0}", nomeArquivo), '|');
            #endregion

            Arquivo arqSaida = new Arquivo(string.Format("{0}.txt", nomeArquivo.ToLower().Replace(".txt", "")));

            #region Buscar Orders
            //List<string> lstOrderNumbers = entrada.Split(new char[] { ',' }).ToList();
            ////===============================================================
            ////---------------------- BUSCAR ORDERS --------------------------
            ////===============================================================
            ////List<ItemAttribute> listaArq = new ConsultarSynergia(empresa, tipoCliente).ObterItemsAttributePorOrder(lstOrders, typeof(B2WinDTO));
            //List<string> lstOrders = SalesforceDAO.GetOrdersPorOrderItemId(string.Concat("'", string.Join("','", lstOrderNumbers.ToArray()), "'"), ref this.binding);
            //if (lstOrders.Count == 0)
            //{
            //    Console.WriteLine("[CONSULTA ORDERS] Dados não encontrados para os Orders informados.");
            //    return;
            //}
            #endregion

            #region Orders não encontrados
            //List<string> lstOrderNaoEncontrados = lstOrderNumbers.Except(lstOrders).ToList();

            //if (lstOrderNaoEncontrados != null && lstOrderNaoEncontrados.Count > 0)
            //{
            //    string msgLogs = string.Format("Clientes não encontrados ao buscar o B2Win: {0}", string.Join(",", lstOrderNaoEncontrados.ToArray()));
            //    Console.WriteLine(msgLogs);
            //    IO.EscreverArquivo(arqSaida, string.Join(Environment.NewLine, msgLogs));
            //}
            #endregion

            #region Buscar clientes
            //List<string> lstOrders = entrada.Split(new char[] { ',' }).ToList();
            ////=================================================================
            ////----------------------- BUSCAR CLIENTES -------------------------
            ////=================================================================
            ////List<ItemAttribute> listaArq = new ConsultarSynergia(empresa, tipoCliente).ObterItemsAttributePorOrder(lstOrders, typeof(B2WinDTO));
            //List<AssetDTO> lstAssets = SalesforceDAO.GetAssetsPorOrderId(string.Concat("'", string.Join("','", lstOrders.ToArray()), "'"), ref this.binding);
            //if (lstAssets.Count == 0)
            //{
            //    Console.WriteLine("[CONSULTA CLIENTES] Dados não encontrados para os Orders informados.");
            //    return;
            //}
            #endregion

            #region Buscar B2Win
            //===============================================================
            //----------------------- BUSCAR B2WIN --------------------------
            //===============================================================
            //List<string> lstClientes = lstAssets.Where(c => !string.IsNullOrWhiteSpace(c.NumeroCliente)).Select(x => { return x.NumeroCliente; }).ToList();
            //List<string> lstClientes = entrada.Split(new char[] { ',' }).ToList();

            //List<ItemAttribute> listaArq = new ConsultarSynergia(empresa, tipoCliente).ObterItemsAttribute(lstClientes, 0, typeof(B2WinDTO));
            //if (listaArq == null || listaArq.Count == 0)
            //{
            //    Console.WriteLine("[CONSULTA B2WIN] Dados não encontrados para os Clientes informados.");
            //    return;
            //}
            #endregion


            ProcessarB2WinOrderItemAttributesManual("0", arqSaida.CaminhoCompleto);

            //Arquivo arq = new Arquivo(nomeArquivo);
            //List<ItemAttribute> listaArq = ArquivoLoader.GetItemsAttributes(arq, typeof(B2WinDTO), "GB");

            //CarregarB2Win(empresa, listaArq, 0, tipoCliente, arqSaida);
            //orderItem.OrderId = novoOrder.Id;
            //orderItem.Country = "BRASIL";
            //orderItem.CurrencyIsoCode = "BRL";
            //orderItem.ExternalId = novoOrder.ExternalId;
            //orderItem.AccountId = novoOrder.AccountId;
            ////orderItem.AssetItemEnterpriseId = asset.ExternalId;
            //orderItem.BillingAccountId = novoOrder.AccountId;
            //orderItem.Catalog = "a101o00000EBAXWAA5";                   //TODO: migrar para uma classe especifica por Empresa
            //orderItem.CatalogItem = "A".Equals(asset.TipoCliente) ? "a0z1o000003z2FNAAY" : "a0z1o000003z2FOAAY";    //TODO: migrar para uma classe especifica por Empresa
            //orderItem.ProductId = "A".Equals(asset.TipoCliente) ? "a1f1o00000bsF6sAAE" : "a1f1o00000bsF6tAAE";      //TODO: migrar para uma classe especifica por Empresa
            //orderItem.ServiceAccountId = novoOrder.AccountId;
            //sfObj = new sObject();
            //sfObj.type = "NE__OrderItem__c";
            //sfObj.Any = new System.Xml.XmlElement[] {
            //        //SFDCSchemeBuild.GetNewXmlElement("ExternalId", novoOrder.ExternalId),
            //        SFDCSchemeBuild.GetNewXmlElement("NE__OrderId__c", orderItem.OrderId),
            //        SFDCSchemeBuild.GetNewXmlElement("NE__Action__c", orderItem.Action),
            //        SFDCSchemeBuild.GetNewXmlElement("NE__Qty__c", orderItem.Qty),
            //        SFDCSchemeBuild.GetNewXmlElement("NE__CatalogItem__c", orderItem.CatalogItem),
            //        SFDCSchemeBuild.GetNewXmlElement("NE__ProdId__c", orderItem.ProductId),
            //        SFDCSchemeBuild.GetNewXmlElement("NE__ACCOUNT__C", orderItem.AccountId),
            //        SFDCSchemeBuild.GetNewXmlElement("NE__BILLING_ACCOUNT__C", orderItem.BillingAccountId),
            //        SFDCSchemeBuild.GetNewXmlElement("NE__CATALOG__C", orderItem.Catalog),
            //        SFDCSchemeBuild.GetNewXmlElement("NE__SERVICE_ACCOUNT__C", orderItem.ServiceAccountId),
            //        SFDCSchemeBuild.GetNewXmlElement("NE__STATUS__C", orderItem.Status),
            //        SFDCSchemeBuild.GetNewXmlElement("NE__ASSETITEMENTERPRISEID__C", orderItem.ExternalId),
            //        SFDCSchemeBuild.GetNewXmlElement("NE__BASEONETIMEFEE__C", orderItem.BaseOneTimeFee),
            //        SFDCSchemeBuild.GetNewXmlElement("NE__BASERECURRINGCHARGE__C", orderItem.BaseRecurringCharge),
            //        SFDCSchemeBuild.GetNewXmlElement("NE__ONETIMEFEEOV__C", orderItem.OneTimeFeeOv),
            //        SFDCSchemeBuild.GetNewXmlElement("NE__RECURRINGCHARGEFREQUENCY__C", orderItem.RecurringChargeFrequency),
            //        SFDCSchemeBuild.GetNewXmlElement("NE__RECURRINGCHARGEOV__C", orderItem.RecurringChargeOv),
            //        SFDCSchemeBuild.GetNewXmlElement("NE__Country__c", orderItem.Country),
            //        SFDCSchemeBuild.GetNewXmlElement("CurrencyIsoCode", orderItem.CurrencyIsoCode)
            //    };
        }



        [ComVisible(true)]
        public void CarregarB2WinPorAsset(string empresa, string nomeArquivo, Dictionary<string,string> listaAssets, TipoCliente tipoCliente, bool temCabecalho)
        {
            if (!loggedIn)
            {
                Autenticacao auth = new Autenticacao(this.ambiente);
                if (!auth.ValidarLogin(ref this.binding))
                    return;

                this.loggedIn = true;
            }

            #region Histório de execução
            //Arquivo arquivo = new Arquivo(@"C:\temp\CargaB2Win\190327\b2win.txt", ';');
            //Arquivo arquivo = new Arquivo(@"C:\temp\CargaB2Win\190327\b2win02.txt", ';');
            //Arquivo arquivo = new Arquivo(@"C:\temp\CargaB2Win\190327\6340029.txt", '|');
            //Arquivo arquivo = new Arquivo(string.Format(@"C:\temp\CargaB2Win\190327\b2win03.txt", '|');
            //Arquivo arquivo = new Arquivo(string.Format(@"C:\temp\CargaB2Win\190327\{0}", nomeArquivo), '|');
            #endregion

            Arquivo arqSaida = new Arquivo(string.Format("{0}_{1}.txt"
                , nomeArquivo.ToLower().Replace(".txt", "")
                , DateTime.Now.ToString("yyyyMMdd_HHmmss")));

            #region Buscar Orders
            //List<string> lstOrderNumbers = entrada.Split(new char[] { ',' }).ToList();
            ////===============================================================
            ////---------------------- BUSCAR ORDERS --------------------------
            ////===============================================================
            ////List<ItemAttribute> listaArq = new ConsultarSynergia(empresa, tipoCliente).ObterItemsAttributePorOrder(lstOrders, typeof(B2WinDTO));
            //List<string> lstOrders = SalesforceDAO.GetOrdersPorOrderItemId(string.Concat("'", string.Join("','", lstOrderNumbers.ToArray()), "'"), ref this.binding);
            //if (lstOrders.Count == 0)
            //{
            //    Console.WriteLine("[CONSULTA ORDERS] Dados não encontrados para os Orders informados.");
            //    return;
            //}
            #endregion

            #region Orders não encontrados
            //List<string> lstOrderNaoEncontrados = lstOrderNumbers.Except(lstOrders).ToList();

            //if (lstOrderNaoEncontrados != null && lstOrderNaoEncontrados.Count > 0)
            //{
            //    string msgLogs = string.Format("Clientes não encontrados ao buscar o B2Win: {0}", string.Join(",", lstOrderNaoEncontrados.ToArray()));
            //    Console.WriteLine(msgLogs);
            //    IO.EscreverArquivo(arqSaida, string.Join(Environment.NewLine, msgLogs));
            //}
            #endregion

            #region Buscar clientes
            //=================================================================
            //----------------------- BUSCAR CLIENTES -------------------------
            //=================================================================
            //List<ItemAttribute> listaArq = new ConsultarSynergia(empresa, tipoCliente).ObterItemsAttributePorOrder(lstOrders, typeof(B2WinDTO));
            //List<AssetDTO> lstAssets = SalesforceDAO.GetAssetsPorId(string.Concat("'", string.Join("','", listaAssets.ToArray()), "'"), ref this.binding);
            List<ContractLineItemSalesforce> lstAssets = SalesforceDAO.GetContractLineItemsByAssetId(string.Concat("'", string.Join("','", listaAssets.Keys.ToArray()), "'"), ref this.binding);
            
            if (lstAssets == null || lstAssets.Count == 0)
            {
                Console.WriteLine("[CONSULTA CLIENTES] Dados não encontrados para os Orders informados.");
                return;
            }
            #endregion

            //===============================================================
            //----------------------- BUSCAR B2WIN --------------------------
            //===============================================================
            Dictionary<string,string> dicAssets = lstAssets.Where(c => !string.IsNullOrWhiteSpace(c.NumeroCliente)).ToDictionary(x => x.AssetId, x => x.NumeroCliente);
            //List<string> lstClientes = entrada.Split(new char[] { ',' }).ToList();

            foreach (string idAsset in dicAssets.Keys)
            {
                string orderId = listaAssets[idAsset];
                List<ItemAttribute> listaArq = new ConsultarSynergia(empresa, tipoCliente).ObterItemsAttribute(new List<string> { dicAssets[idAsset] }, 0, typeof(B2WinDTO));
                AtualizarItemAttributes(listaArq, orderId, arqSaida, true);
            }
            ////ProcessarB2WinOrderItemAttributesManual("0", arqSaida.CaminhoCompleto);

            ////Arquivo arq = new Arquivo(nomeArquivo);
            ////List<ItemAttribute> listaArq = ArquivoLoader.GetItemsAttributes(arq, typeof(B2WinDTO), "GB");

            //CarregarB2Win(empresa, lstAssets, listaArq, 0, tipoCliente, arqSaida);
        }


        
        /// <summary>
        /// 
        /// <origem>C:\!adl\Ayesa\EPT_Suporte\180604 Salesforce\190325 Carga Bit2Win</origem>
        /// </summary>
        /// <param name="empresa"></param>
        [ComVisible(true)]
        public void CarregarB2Win(string empresa, string caminhoArquivo, int lote, TipoCliente tipoCliente, bool temCabecalho)
        {
            #region Histório de execução
            //Arquivo arquivo = new Arquivo(@"C:\temp\CargaB2Win\190327\b2win.txt", ';');
            //Arquivo arquivo = new Arquivo(@"C:\temp\CargaB2Win\190327\b2win02.txt", ';');
            //Arquivo arquivo = new Arquivo(@"C:\temp\CargaB2Win\190327\6340029.txt", '|');
            //Arquivo arquivo = new Arquivo(string.Format(@"C:\temp\CargaB2Win\190327\b2win03.txt", '|');
            //Arquivo arquivo = new Arquivo(string.Format(@"C:\temp\CargaB2Win\190327\{0}", nomeArquivo), '|');
            #endregion

            Arquivo arquivo = new Arquivo(caminhoArquivo, '|', false);
            List<ItemAttribute> listaArq = ArquivoLoader.GetItemsAttributes(arquivo, typeof(B2WinDTO), tipoCliente.ToString());
            CarregarB2Win(empresa, listaArq, lote, tipoCliente, arquivo);
        }


        [ComVisible(true)]
        
        public void CarregarB2Win(string empresa, List<ItemAttribute> lstB2Win, int lote, TipoCliente tipoCliente, Arquivo arqSaida, bool flRecursivo = false)
        {
            #region Validar empresa
            
            string nomeEmpresa = "2003".Equals(empresa) ? "COELCE" : "2005".Equals(empresa) ? "AMPLA" : "2018".Equals(empresa) ? "CELG" : string.Empty;
            if(string.IsNullOrWhiteSpace(empresa))
            {
                throw new ArgumentException(string.Format("Empresa '{0}' não informada ou inválida.", empresa));
            }

            #endregion

            #region Autenticação
            if (!loggedIn)
            {
                Autenticacao auth = new Autenticacao(this.ambiente);
                if (!auth.ValidarLogin(ref this.binding))
                    return;

                this.loggedIn = true;
            }
            #endregion

            int i = 0;
            int contCliente = 0;
            sObject sfObj = null;
            List<sObject> lstObjetos = new List<sObject>();
            List<sObject> lstObjetosAssetItemAttr = new List<sObject>();
            List<string> msgLogs = new List<string>();
            StringBuilder idOrder = new StringBuilder();
            TimeSpan dtInicioProcesso = new TimeSpan(DateTime.Now.Ticks);

            try
            {
                if (!flRecursivo)
                {
                    msgLogs.Add(string.Format("{0} [ARQUIVO]\t{1}", DateTime.Now.ToLongTimeString(), arqSaida.CaminhoCompleto));
                    Console.WriteLine(msgLogs.Last());
                }

                    foreach (ItemAttribute itemAttribute in lstB2Win)
                    {
                        if (msgLogs.Count > 0)
                            IO.EscreverArquivo(arqSaida, string.Join(Environment.NewLine, msgLogs));

                        msgLogs.Clear();
                        idOrder.Clear();
                        contCliente++;
                        i++;

                        List<ContractLineItemSalesforce> lstContractLineItem = null;
                        if (string.IsNullOrWhiteSpace(itemAttribute.ExternalIdAsset))
                        {
                            lstContractLineItem = SalesforceDAO.GetContractLinesByNumeroCliente(string.Concat("'",this.codigoEmpresa,"'"), itemAttribute.NumeroCliente, ref binding);

                            if (lstContractLineItem == null || lstContractLineItem.Where(c => !string.IsNullOrWhiteSpace(c.AssetExternalId)).Count() == 0 || string.IsNullOrWhiteSpace(lstContractLineItem.Where(c => !string.IsNullOrWhiteSpace(c.AssetExternalId)).FirstOrDefault().AssetExternalId))
                            {
                                msgLogs.Add(string.Format("{0} [ERRO]\tSem Asset ExternalId para o cliente {1}"
                                                                , DateTime.Now.ToLongTimeString()
                                                                , itemAttribute.NumeroCliente));
                                Console.WriteLine(msgLogs.Last());
                                continue;
                            }
                            itemAttribute.ExternalIdAsset = lstContractLineItem.Where(c => !string.IsNullOrWhiteSpace(c.AssetExternalId)).FirstOrDefault().AssetExternalId;
                        }
                        msgLogs.Add(string.Format("\n\r{0} [B2WIN]\t{1} {2}", DateTime.Now.ToLongTimeString(), itemAttribute.ExternalIdAsset, flRecursivo ? "[recursivo]" : string.Empty));
                        Console.WriteLine(msgLogs.Last());

                        try
                        {
                            if (lstContractLineItem == null || lstContractLineItem.Count == 0)
                                lstContractLineItem = SalesforceDAO.GetContractLinesByNumeroCliente(string.Concat("'",this.codigoEmpresa,"'"), itemAttribute.NumeroCliente, ref binding);
                        }
                        catch(System.Net.WebException ex)
                        {
                            msgLogs.Add(string.Format("{0} [ERRO]\tFalha de conexão: {1}{2}{3}{4}"
                                , DateTime.Now.ToLongTimeString()
                                , itemAttribute.ExternalIdAsset
                                , ex.GetType().ToString()
                                , ex.Message
                                , ex.StackTrace));
                            Console.WriteLine(msgLogs.Last());
                            continue;
                        }
                        catch(Exception ex)
                        {
                            msgLogs.Add(string.Format("{0} [ERRO]\tFalha ao recuperar o Contract Line Item: {1}{2}{3}{4}"
                                , DateTime.Now.ToLongTimeString()
                                , itemAttribute.ExternalIdAsset
                                , ex.GetType().ToString()
                                , ex.Message
                                , ex.StackTrace));
                            Console.WriteLine(msgLogs.Last());
                            continue;
                        }

                        if (lstContractLineItem == null || lstContractLineItem.Count == 0)
                        {
                            //TODO: gravar em log
                            msgLogs.Add(string.Format("{0} [ERRO]\tContractLine/Asset não encontrado {1}", DateTime.Now.ToLongTimeString(), itemAttribute.ExternalIdAsset));
                            Console.WriteLine(msgLogs.Last());
                            continue;
                        }
                        
                        msgLogs.Add(string.Format("{0} [CLIENTE] {1}", DateTime.Now.ToLongTimeString(), lstContractLineItem.FirstOrDefault().NumeroCliente));
                        Console.WriteLine(msgLogs.Last().Trim());

                        List<AssetDTO> assets = null;

                        try
                        {
                            assets = SalesforceDAO.GetAssetsPorExternalId(itemAttribute.ExternalIdAsset, ref binding);
                        }
                        catch(Exception ex)
                        {
                            msgLogs.Add(string.Format("{0} [ERRO]\tFalha ao recuperar Asset por ExternalId {1}. {2}"
                                , DateTime.Now.ToLongTimeString()
                                , itemAttribute.ExternalIdAsset
                                , ex.Message));
                            Console.WriteLine(msgLogs.Last());
                            continue;
                        }

                        #region Asset não encontrado
                        if (assets == null || assets.Count() == 0)
                        {
                            msgLogs.Add(string.Format("{0}\t[ERRO]\tAsset não encontrado com ExternalId {1} ...", DateTime.Now.ToLongTimeString(), itemAttribute.ExternalIdAsset));
                            Console.WriteLine(msgLogs.Last());
                            continue;
                        }
                        #endregion

                        #region Asset encontrado
                        AssetDTO asset = assets.OrderByDescending(x => x.DataCriacao).First();
                        idOrder.Append(asset.OrderId);
                        if (idOrder.Length == 0)
                            //Debugger.Break();

                        if (string.IsNullOrWhiteSpace(lstContractLineItem.FirstOrDefault().ContractId))
                        {
                            msgLogs.Add(string.Format("{0} [ERRO]\tContract Id não encontrado para o Asset {1} ...", DateTime.Now.ToLongTimeString(), lstContractLineItem.FirstOrDefault().AssetId));
                            Console.WriteLine(msgLogs.Last());
                            continue;
                        }

                        itemAttribute.NumeroCliente = string.IsNullOrWhiteSpace(itemAttribute.NumeroCliente) ? string.IsNullOrWhiteSpace(asset.NumeroCliente) ? itemAttribute.NumeroCliente : asset.NumeroCliente : itemAttribute.NumeroCliente;
                        itemAttribute.ExternalIdPod = string.IsNullOrWhiteSpace(itemAttribute.ExternalIdPod) ? string.IsNullOrWhiteSpace(asset.PointofDeliveryExternalId) ? itemAttribute.ExternalIdPod : asset.PointofDeliveryExternalId : itemAttribute.ExternalIdPod;

                        if (asset != null && string.IsNullOrWhiteSpace(asset.OrderId))
                        {
                            #region Asset sem Order
                            msgLogs.Add(string.Format("{0}\tCriando Order para o Asset {1} {2} ...", DateTime.Now.ToLongTimeString(), asset.Id, asset.ExternalId));
                            Console.WriteLine(msgLogs.Last());

                            if(string.IsNullOrWhiteSpace(asset.AccountId))
                            {
                                msgLogs.Add(string.Format("{0} [ERRO]\tAsset {1} {2} sem Account. Cliente: {3}."
                                    , DateTime.Now.ToLongTimeString()
                                    , asset.Id
                                    , asset.ExternalId
                                    , itemAttribute.NumeroCliente));
                                Console.WriteLine(msgLogs.Last());
                                continue;
                            }
                            #region Ingressar Order ------------------------------------------------------------------------
                            OrderSalesforce novoOrder = IngressarOrder(asset, itemAttribute);

                            msgLogs.Add(string.Format("{0}\tOrder criado {1} ...", DateTime.Now.ToLongTimeString(), novoOrder.Id));
                            Console.WriteLine(msgLogs.Last());

                            msgLogs.Add(string.Format("{0}\tCarregando Asset ...", DateTime.Now.ToLongTimeString()));
                            Console.WriteLine(msgLogs.Last());

                            #region Atualizar Asset
                            if (string.IsNullOrWhiteSpace(asset.Id))
                                Debugger.Break();

                            sfObj = new sObject();
                            sfObj.type = "Asset";
                            sfObj.Id = asset.Id;

                            sfObj.Any = new System.Xml.XmlElement[] {
                            SFDCSchemeBuild.GetNewXmlElement("NE__Status__c", "Active"),
                            SFDCSchemeBuild.GetNewXmlElement("Status", "Active"),
                            SFDCSchemeBuild.GetNewXmlElement("NE__Action__c", "Add"),
                            SFDCSchemeBuild.GetNewXmlElement("Quantity", "1"),
                            SFDCSchemeBuild.GetNewXmlElement("NE__BASEONETIMEFEE__C", "0.0"),
                            SFDCSchemeBuild.GetNewXmlElement("NE__BASERECURRINGCHARGE__C", "0.0"),
                            SFDCSchemeBuild.GetNewXmlElement("NE__ONETIMEFEEOV__C", "0.0"),
                            SFDCSchemeBuild.GetNewXmlElement("NE__RECURRINGCHARGEOV__C", "0.0"),
                            SFDCSchemeBuild.GetNewXmlElement("NE__RECURRINGCHARGEFREQUENCY__C", "Monthly"),
                            SFDCSchemeBuild.GetNewXmlElement("CurrencyIsoCode", "BRL"),
                            SFDCSchemeBuild.GetNewXmlElement("NE__StartDate__c", DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss.000+0000")),
                            //SFDCSchemeBuild.GetNewXmlElement("Name", "Grupo A"),
                            SFDCSchemeBuild.GetNewXmlElement("PointofDelivery__c", asset.PointofDeliveryId),
                            SFDCSchemeBuild.GetNewXmlElement("AccountId", asset.AccountId),
                            SFDCSchemeBuild.GetNewXmlElement("NE__ASSETITEMENTERPRISEID__C", novoOrder.Id),
                            SFDCSchemeBuild.GetNewXmlElement("NE__SERVICE_ACCOUNT__C", asset.AccountId),
                            SFDCSchemeBuild.GetNewXmlElement("NE__BILLING_ACCOUNT__C",asset.AccountId ),
                            SFDCSchemeBuild.GetNewXmlElement("NE__CatalogItem__c", tipoCliente == TipoCliente.GA ? "a0z1o000003z2FNAAY" : tipoCliente == TipoCliente.GB ? "a0z1o000003z2FOAAY" : ""),
                            SFDCSchemeBuild.GetNewXmlElement("NE__ProdId__c", tipoCliente == TipoCliente.GA ? "a1f1o00000bsF6sAAE" : tipoCliente == TipoCliente.GB ? "a1f1o00000bsF6tAAE" : ""),
                            SFDCSchemeBuild.GetNewXmlElement("NE__Order_Config__c", novoOrder.Id),
                            SFDCSchemeBuild.GetNewXmlElement("Contract__c", lstContractLineItem.FirstOrDefault().ContractId),
                            SFDCSchemeBuild.GetNewXmlElement("Company__c", "2005".Equals(codigoEmpresa) ? "AMPLA" : "2003".Equals(codigoEmpresa) ? "COELCE" : "CELG"),
                            SFDCSchemeBuild.GetNewXmlElement("Country__c", "BRASIL")
                            };

                            lstObjetos.Add(sfObj);
                            #endregion

                            //Console.WriteLine(msgLogs.Last());
                            #region Atualizar Contract
                            if (string.IsNullOrWhiteSpace(lstContractLineItem.FirstOrDefault().ContractId))
                            {
                                msgLogs.Add(string.Format("{0}\t[CONTRACT]\t Contrato não encontrado para o Asset {1} ...", DateTime.Now.ToLongTimeString(), itemAttribute.ExternalIdAsset));
                                Console.WriteLine(msgLogs.Last());
                            }
                            else
                            {
                                msgLogs.Add(string.Format("{0} Carregando Contract {1} ...", DateTime.Now.ToLongTimeString(), asset.ContractId));

                                sfObj = new sObject();
                                sfObj.type = "Contract";
                                sfObj.Id = lstContractLineItem.FirstOrDefault().ContractId;

                                sfObj.Any = new System.Xml.XmlElement[] {
                                    SFDCSchemeBuild.GetNewXmlElement("CNT_Quote__c", novoOrder.Id),
                                    SFDCSchemeBuild.GetNewXmlElement("CNT_ExternalContract_ID_2__c", asset.NumeroCliente),
                                    SFDCSchemeBuild.GetNewXmlElement("CompanyID__c", codigoEmpresa),
                                    SFDCSchemeBuild.GetNewXmlElement("Status", "Activated"),
                                    SFDCSchemeBuild.GetNewXmlElement("ShippingCountry", "BRASIL")
                                };

                                lstObjetos.Add(sfObj);
                            }
                            #endregion

                            msgLogs.Add(string.Format("{0} Carregando PoD {1} ...", DateTime.Now.ToLongTimeString(), lstContractLineItem.FirstOrDefault().PointOfDelivery));
                            Console.WriteLine(msgLogs.Last());

                            #region Atualizar Point of Delivery
                            if (string.IsNullOrWhiteSpace(lstContractLineItem.FirstOrDefault().PointOfDelivery))
                            {
                                msgLogs.Add(string.Format("{0}\t[PoD]\t PoD não associado ao Contract Line {1} ...", DateTime.Now.ToLongTimeString(), lstContractLineItem[0].ContractId));
                                Console.WriteLine(msgLogs.Last());
                            }
                            else
                            {
                                sfObj = new sObject();
                                sfObj.type = "PointofDelivery__c";
                                sfObj.Id = lstContractLineItem.FirstOrDefault().PointOfDelivery;

                                sfObj.Any = new System.Xml.XmlElement[] {
                                SFDCSchemeBuild.GetNewXmlElement("CNT_Contract__c", lstContractLineItem.FirstOrDefault().ContractId)
                            };

                                lstObjetos.Add(sfObj);
                            }
                            #endregion

                            #endregion

                            #endregion
                        }
                        else
                        {
                            #region Asset com Order

                            msgLogs.Add(string.Format("{0} [ASSET]\tOrder encontrado {1} para o Asset {2} ..."
                                , DateTime.Now.ToLongTimeString()
                                , asset.OrderId
                                , asset.Id));
                            Console.WriteLine(msgLogs.Last());

                            #region Atualizar Order_item_attribute e Asset_item_attibute
                            msgLogs.Add(string.Format("{0} Carregando Attribute Items do Asset e Order ...", DateTime.Now.ToLongTimeString()));
                            Console.WriteLine(msgLogs.Last());

                            try
                            {
                                lstObjetosAssetItemAttr.AddRange(AtualizarItemAttributes(new ItemAttribute[] { itemAttribute }.ToList(), asset.OrderId, arqSaida, false));
                            }
                            catch(InvalidDataException ex)
                            {
                                bool apagado = false;

                                //Apagar Order e respectivos dados de Order Item e ItemAttributes (para Asset e OrderItem)
                                msgLogs.AddRange(ApagarOrderEDerivados(asset, ref apagado));
                                if (apagado)
                                {
                                    try
                                    {
                                        CarregarB2Win(empresa, new List<ItemAttribute> { itemAttribute }, lote, tipoCliente, arqSaida, true);
                                        lstContractLineItem = SalesforceDAO.GetContractLineItemsByExternalId(itemAttribute.ExternalIdAsset, ref binding);
                                        assets = SalesforceDAO.GetAssetsPorExternalId(itemAttribute.ExternalIdAsset, ref binding);
                                        idOrder.Clear();
                                        idOrder.Append(assets.FirstOrDefault().OrderId);
                                    }
                                    catch
                                    {
                                        throw new Exception(string.Format("{0} Falha ao recarregar o Asset {1} / Order {2}"
                                            , DateTime.Now.ToLongTimeString()
                                            , asset.Id
                                            , asset.OrderId));
                                    }
                                }
                            }
                            #endregion

                            if (lstContractLineItem.Count == 0)
                            {
                                msgLogs.Add(string.Format("{0} [ERRO]\tContract Line não encontrado para o Asset {1}. Cliente: {2}."
                                    , DateTime.Now.ToLongTimeString()
                                    , asset.ExternalId
                                    , itemAttribute.NumeroCliente));
                                Console.WriteLine(msgLogs.Last());
                                continue;
                            }
                            
                            if (lstContractLineItem.Count > 0 && !lstContractLineItem.FirstOrDefault().ContractType.Contains("B2"))
                            {
                                msgLogs.Add(string.Format("{0} [INFO]\tContract Type {1} não é B2B/B2C: {2}. Cliente: {3}."
                                    , DateTime.Now.ToLongTimeString()
                                    , asset.ContractId
                                    , lstContractLineItem.FirstOrDefault().ContractType
                                    , itemAttribute.NumeroCliente));
                                Console.WriteLine(msgLogs.Last());
                            }

                            msgLogs.Add(string.Format("{0} Carregando Asset {1} ...", DateTime.Now.ToLongTimeString(), lstContractLineItem.FirstOrDefault().AssetId));
                            Console.WriteLine(msgLogs.Last());

                            #region Atualizar Asset
                            if (string.IsNullOrWhiteSpace(asset.Id))
                                Debugger.Break();

                            sfObj = new sObject();
                            sfObj.type = "Asset";
                            sfObj.Id = lstContractLineItem.FirstOrDefault().AssetId;

                           
                            sfObj.Any = new System.Xml.XmlElement[] {
                            SFDCSchemeBuild.GetNewXmlElement("NE__Action__c", "Add"),  /* NOVO */
                            SFDCSchemeBuild.GetNewXmlElement("NE__Status__c", "Active"),  /* NOVO */
                            SFDCSchemeBuild.GetNewXmlElement("Status", "Active"),  /* NOVO */
                            SFDCSchemeBuild.GetNewXmlElement("NE__BASEONETIMEFEE__C", "0.0"),  /* NOVO */
                            SFDCSchemeBuild.GetNewXmlElement("NE__BASERECURRINGCHARGE__C", "0.0"),  /* NOVO */
                            SFDCSchemeBuild.GetNewXmlElement("NE__ONETIMEFEEOV__C", "0.0"),  /* NOVO */
                            SFDCSchemeBuild.GetNewXmlElement("NE__RECURRINGCHARGEOV__C", "0.0"),  /* NOVO */
                            SFDCSchemeBuild.GetNewXmlElement("NE__RECURRINGCHARGEFREQUENCY__C", "Monthly"),  /* NOVO */
                            SFDCSchemeBuild.GetNewXmlElement("CurrencyIsoCode", "BRL"),  /* NOVO */
                            
                            SFDCSchemeBuild.GetNewXmlElement("NE__CatalogItem__c", tipoCliente == TipoCliente.GA ? "a0z1o000003z2FNAAY" : tipoCliente == TipoCliente.GB ? "a0z1o000003z2FOAAY" : ""),
                            SFDCSchemeBuild.GetNewXmlElement("NE__ProdId__c", tipoCliente == TipoCliente.GA ? "a1f1o00000bsF6sAAE" : tipoCliente == TipoCliente.GB ? "a1f1o00000bsF6tAAE" : ""),

                            SFDCSchemeBuild.GetNewXmlElement("Quantity", "1"),  /* NOVO */
                            SFDCSchemeBuild.GetNewXmlElement("NE__StartDate__c", DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss.000+0000")),
                            SFDCSchemeBuild.GetNewXmlElement("PointofDelivery__c", lstContractLineItem.FirstOrDefault().PointOfDelivery),
                            SFDCSchemeBuild.GetNewXmlElement("AccountId", lstContractLineItem.FirstOrDefault().AccountIdAsset),
                            SFDCSchemeBuild.GetNewXmlElement("NE__SERVICE_ACCOUNT__C", lstContractLineItem.FirstOrDefault().AccountIdAsset),
                            SFDCSchemeBuild.GetNewXmlElement("NE__BILLING_ACCOUNT__C", lstContractLineItem.FirstOrDefault().AccountIdAsset),
                            SFDCSchemeBuild.GetNewXmlElement("Contract__c", lstContractLineItem.FirstOrDefault().ContractId),
                            SFDCSchemeBuild.GetNewXmlElement("Company__c", "2005".Equals(codigoEmpresa) ? "AMPLA" : "2003".Equals(codigoEmpresa) ? "COELCE" : "CELG"),
                            SFDCSchemeBuild.GetNewXmlElement("Country__c", "BRASIL")
                            };

                            lstObjetos.Add(sfObj);
                            //SaveResult[] saveRecordType = binding.update(lstObjetos.ToArray());
                            //SaveResult resultado = saveRecordType.First();
                            //if (resultado != null && resultado.errors != null)
                            //{
                            //    foreach (Error err in resultado.errors)
                            //    {
                            //        msgLogs.Add(string.Format("[ERRO] {0}", err.message));
                            //        Console.WriteLine(msgLogs.Last());
                            //    }
                            //}
                            //lstObjetos.Clear();
                            #endregion


                            msgLogs.Add(string.Format("{0} Carregando Contract {1} ...", DateTime.Now.ToLongTimeString(), asset.ContractId));
                            Console.WriteLine(msgLogs.Last());

                            #region Atualizar Contract
                            //Contrato com  order id,       número do cliente,              companyid__c e  country: 
                            //              CNT_Quote__c,   CNT_ExternalContract_ID_2__c,   CompanyID__c,   ShippingCountry 
                            sfObj = new sObject();
                            sfObj.type = "Contract";
                            sfObj.Id = lstContractLineItem.FirstOrDefault().ContractId;
                            if (string.IsNullOrWhiteSpace(lstContractLineItem.FirstOrDefault().ContractId))
                            {
                                //TODO: log
                                msgLogs.Add(string.Format("{0} [ERRO]\tContrato External ID vazio para o Asset {1}"
                                    , DateTime.Now.ToLongTimeString()
                                    , asset.ExternalId));
                                Console.WriteLine(msgLogs.Last());
                                continue;
                            }

                            sfObj.Any = new System.Xml.XmlElement[] {
                            SFDCSchemeBuild.GetNewXmlElement("CNT_Quote__c", idOrder.ToString()),
                            SFDCSchemeBuild.GetNewXmlElement("CNT_ExternalContract_ID_2__c", lstContractLineItem.FirstOrDefault().NumeroCliente),
                            SFDCSchemeBuild.GetNewXmlElement("CompanyID__c", codigoEmpresa),
                            SFDCSchemeBuild.GetNewXmlElement("Status", "Activated"),
                            SFDCSchemeBuild.GetNewXmlElement("ShippingCountry", "BRASIL")
                            };

                            lstObjetos.Add(sfObj);
                            //saveRecordType = binding.update(lstObjetos.ToArray());
                            //resultado = saveRecordType.First();
                            //if (resultado != null && resultado.errors != null)
                            //{
                            //    foreach (Error err in resultado.errors)
                            //    {
                            //        msgLogs.Add(string.Format("[ERRO] {0}", err.message));
                            //        Console.WriteLine(msgLogs.Last());
                            //    }
                            //}
                            //lstObjetos.Clear();
                            #endregion

                            msgLogs.Add(string.Format("{0} Carregando PoD {1} ...", DateTime.Now.ToLongTimeString(), lstContractLineItem.FirstOrDefault().PointOfDelivery));
                            Console.WriteLine(msgLogs.Last());

                            #region Atualizar Point of Delivery
                            if (string.IsNullOrWhiteSpace(lstContractLineItem.FirstOrDefault().PointOfDelivery))
                            {
                                msgLogs.Add(string.Format("{0}\t[PoD]\t PoD não associado ao Contract Line {1} ...", DateTime.Now.ToLongTimeString(), lstContractLineItem[0].ContractId));
                                Console.WriteLine(msgLogs.Last());
                            }
                            else
                            {
                                sfObj = new sObject();
                                sfObj.type = "PointofDelivery__c";
                                sfObj.Id = lstContractLineItem.FirstOrDefault().PointOfDelivery;

                                sfObj.Any = new System.Xml.XmlElement[] {
                                    SFDCSchemeBuild.GetNewXmlElement("CNT_Contract__c", lstContractLineItem.FirstOrDefault().ContractId)
                                };

                                lstObjetos.Add(sfObj);
                                //saveRecordType = binding.update(lstObjetos.ToArray());
                                //resultado = saveRecordType.First();
                                //if (resultado != null && resultado.errors != null)
                                //{
                                //    foreach (Error err in resultado.errors)
                                //    {
                                //        msgLogs.Add(string.Format("[ERRO] {0}", err.message));
                                //        Console.WriteLine(msgLogs.Last());
                                //    }
                                //}
                                //lstObjetos.Clear();
                            }
                            #endregion

                            #endregion
                        }

                        #endregion


                        if (lstObjetos.Count > 200)
                        {
                            msgLogs.Add(string.Format("\n\n---------------------------\nAtualizando lote de dados.   Tempo total do processo: {0}\n---------------------------", new TimeSpan(DateTime.Now.Ticks).Add(-dtInicioProcesso).ToString(@"hh\:mm\:ss")));
                            Console.WriteLine(msgLogs.Last().Trim());

                            SaveResult[] saveResults = SalesforceDAO.Atualizar(lstObjetos, 10, ref binding);
                            List<Error[]> erros = saveResults.Where(sr => sr.errors != null).Select(s => s.errors).ToList();
                            foreach (Error[] err in erros)
                            {
                                msgLogs.Add(string.Format("{0} [ERRO B2WIN] {1}", DateTime.Now.ToLongTimeString(), string.Join(", ", err.Select(e => e.message))));
                                Console.WriteLine(msgLogs.Last().Trim());
                            }

                            if (lstObjetosAssetItemAttr.Count > 0)
                            {
                                saveResults = SalesforceDAO.Atualizar(lstObjetosAssetItemAttr, 29, ref binding);
                                erros = saveResults.Where(sr => sr.errors != null).Select(s => s.errors).ToList();
                                foreach (Error[] err in erros)
                                {
                                    msgLogs.Add(string.Format("{0} [ERRO B2WIN] {1}", DateTime.Now.ToLongTimeString(), string.Join(", ", err.Select(e => e.message))));
                                    Console.WriteLine(msgLogs.Last().Trim());
                                }

                                lstObjetosAssetItemAttr.Clear();
                            }

                            lstObjetos.Clear();
                        }

                    }

                    if (lstObjetos.Count > 0)
                    {
                        SaveResult[] saveResultsFinal = SalesforceDAO.Atualizar(lstObjetos, 10, ref binding);
                        List<Error[]> errosFinal = saveResultsFinal.Where(sr => sr.errors != null).Select(s => s.errors).ToList();
                        foreach (Error[] err in errosFinal)
                        {
                            msgLogs.Add(string.Format("{0} [ERRO B2WIN] {1}", DateTime.Now.ToLongTimeString(), string.Join(", ", err.Select(e => e.message))));
                            Console.WriteLine(msgLogs.Last().Trim());
                        }

                        if (lstObjetosAssetItemAttr.Count > 0)
                        {
                            saveResultsFinal = SalesforceDAO.Atualizar(lstObjetosAssetItemAttr, 29, ref binding);
                            errosFinal = saveResultsFinal.Where(sr => sr.errors != null).Select(s => s.errors).ToList();
                            foreach (Error[] err in errosFinal)
                            {
                                msgLogs.Add(string.Format("{0} [ERRO B2WIN] {1}", DateTime.Now.ToLongTimeString(), string.Join(", ", err.Select(e => e.message))));
                                Console.WriteLine(msgLogs.Last().Trim());
                            }
                            lstObjetosAssetItemAttr.Clear();
                        }

                        lstObjetos.Clear();
                    }

                    if (!flRecursivo)
                    {
                        msgLogs.Add(string.Format("Processo finalizado."));
                        Console.WriteLine(msgLogs.Last());
                        IO.EscreverArquivo(arqSaida, string.Join(Environment.NewLine, msgLogs));
                    }

                //Console.ReadKey();
            }
            catch (Exception ex)
            {
                Console.WriteLine("\nErro ao atualizar o registro: \n" + ex.Message + (ex.InnerException != null ? ex.InnerException.ToString() : "") + ex.StackTrace);
                //ToDO: log em arquivo
            }
        }



        /// <summary>
        /// Extrair dados de clientes Governo Grupo A, a partir de uma lista parametrizada.
        /// </summary>
        public void GerarRelatorioGovernoGA(string codEmpresa, Arquivo arquivo)
        {
            autenticar();
            this.log.NomeArquivo = string.Format("{0}\\{1}_{2}_SAIDA.txt"
                , arquivo.Caminho
                , arquivo.Nome
                , DateTime.Now.ToString("yyyymmdd_hhmmss"));
            
            string _naoSeAplica = "-";
            string _cpf = "CPF";
            string _cnpj = "CNPJ";

            try
            {
                List<string> lstCliente = ArquivoLoader.GetNumeroClientes(arquivo);

                List<ContractLineItemGoverno> contratos = SalesforceDAO.GetContractLineGovernosByListaClientes(this.codigoEmpresa, lstCliente, ref binding);
                //contratos = SalesforceDAO.GetContractLineGovernosByListaContratos(this.codigoEmpresa, lstCliente, ref binding);

                foreach (string perdido in lstCliente.Where(x => contratos.Where(c => c.NumeroCliente == x.Replace("'", "")).ToArray().Count() == 0))
                {
                    this.log.LogFull(string.Format("[ERRO]\t{0} Contrato nao encontrado "
                        , perdido.Replace("'", "")));
                }

                foreach(ContractLineItemGoverno contrato in contratos)
                {
                    if (string.IsNullOrWhiteSpace(contrato.AssetExternalId))
                    {
                        this.log.LogFull(string.Format("[ERRO]\t{0} Contrato sem ExternalId de Asset"
                            , contrato.ContractNumber));
                        continue;
                    }
                    Dictionary<string,string> dadosTecnicos = SalesforceDAO.GetAssetItemsAttributeValuesPorAsset(contrato.AssetExternalId, ref binding);
                    
                    ContractSalesforce contratoAgrupamento = SalesforceDAO.GetContratoAgrupamentoPorContrato(contrato, ref this.binding);
                    ClienteSalesforce contaControladora = SalesforceDAO.GetContasPorId(this.codigoEmpresa, contrato.ContaAgrupamentoParentId, ref binding).FirstOrDefault();

                    string classe = dadosTecnicos == null ? _naoSeAplica : dadosTecnicos.Where(x => "Classe BR".Equals(x.Key)).Select(y => y.Value).ToList().FirstOrDefault();
                    string subclasse = dadosTecnicos == null ? _naoSeAplica : dadosTecnicos.Where(x => "SubClasse BR".Equals(x.Key)).Select(y => y.Value).ToList().FirstOrDefault();

                    Console.WriteLine(contrato.NumeroCliente);
                    this.log.LogFull(string.Format("{0}\t{1}\t{2}\t{3}\t{4}\t{5}\t{6}\t{7}\t{8}\t{9}\t{10}\t{11}\t{12}\t{13}\t{14}\t{15}"
                        , contrato.NumeroCliente
                        , contrato.EnderecoCompleto
                        , contrato.MunicipioPoD
                        , contrato.Tarifa
                        , contrato.AssetAccountName
                        , contrato.Identidade
                        , contrato.TipoIdentidade.Length == 11 ? _cpf : contrato.TipoIdentidade.Length == 14 ? _cnpj : _naoSeAplica
                        , contrato.AccountExternalId
                        , contrato.ContractNumber
                        , string.IsNullOrWhiteSpace(classe) ? _naoSeAplica : classe
                        , string.IsNullOrWhiteSpace(subclasse) ? _naoSeAplica : subclasse
                        , contratoAgrupamento == null ? "-" : contrato.ContratoAgrupamento
                        , contrato.Executivo
                        , contaControladora == null ? "-" : contaControladora.Nome
                        , contratoAgrupamento == null ? "-" : contratoAgrupamento.SegmentoAgrupamento
                        , contratoAgrupamento == null ? "-" : contratoAgrupamento.AreaAgrupamento));
                }

                this.log.LogFull("Finalizado");
            }
            catch (Exception ex)
            {
                this.log.LogFull(string.Format("{0} [ERRO CATASTROFICO] {1}{2}"
                    , DateTime.Now.ToString("dd-MM-yyyy HH:mm:ss")
                    , ex.Message
                    , ex.StackTrace));

                Console.ReadKey();
            }
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="arquivo"></param>
        public void GetContratoStatus(Arquivo arquivo)
        {
            autenticar();

            this.log.NomeArquivo = @"C:\!adl\Ayesa\EPT_Suporte\180604 Salesforce\191024 Relatorio Governo CE GA\191111\casos.txt";
            
            try
            {
                List<CaseStatus> contratos = SalesforceDAO.GetStatusCasos(ref binding);

                this.log.LogFull(contratos.Select(c => string.Format("{0}\t{1}\t{2}\t{3}\t{4}\t{5}\t{6}\t{7}\t{8}\t{9}",
                        c.Id,
                        c.CaseNumber,
                        c.Status,
                        c.Type,
                        c.PoDName,
                        c.ContractNumber,
                        c.ContractStatus,
                        c.ContractExternalId,
                        c.CreatedDate,
                        c.AccountName)).ToList());
            }
            catch (Exception ex)
            {
                this.log.LogFull(string.Format("{0} [ERRO CATASTROFICO] {1}{2}"
                    , DateTime.Now.ToString("dd-MM-yyyy HH:mm:ss")
                    , ex.Message
                    , ex.StackTrace));
            }
            this.log = null;
        }


        [ComVisible(true)]
        public void CarregarB2Win(string empresa, List<ContractLineItemSalesforce> lstContractLineItem, List<ItemAttribute> listaArq, int lote, TipoCliente tipoCliente, Arquivo arquivo, bool flRecursivo = false)
        {
            #region Validar empresa

            string nomeEmpresa = "2003".Equals(empresa) ? "COELCE" : "2005".Equals(empresa) ? "AMPLA" : "2018".Equals(empresa) ? "CELG" : string.Empty;
            if (string.IsNullOrWhiteSpace(empresa))
            {
                throw new ArgumentException(string.Format("Empresa '{0}' não informada ou inválida.", empresa));
            }

            #endregion

            #region Autenticação
            if (!loggedIn)
            {
                Autenticacao auth = new Autenticacao(this.ambiente);
                if (!auth.ValidarLogin(ref this.binding))
                    return;

                this.loggedIn = true;
            }
            #endregion

            int i = 0;
            int contCliente = 0;
            sObject sfObj = null;
            List<sObject> lstObjetos = new List<sObject>();
            List<sObject> lstObjetosAssetItemAttr = new List<sObject>();
            List<string> msgLogs = new List<string>();
            StringBuilder idOrder = new StringBuilder();
            TimeSpan dtInicioProcesso = new TimeSpan(DateTime.Now.Ticks);

            try
            {
                if (!flRecursivo)
                {
                    msgLogs.Add(string.Format("{0} [ARQUIVO]\t{1}", DateTime.Now.ToLongTimeString(), arquivo.CaminhoCompleto));
                    Console.WriteLine(msgLogs.Last());
                }

                using (Arquivo arqSaida = new Arquivo(arquivo.Caminho, string.Concat(arquivo.Nome, "_", DateTime.Now.ToString("yyyyMMdd_HHmm"), "_SAIDA"), arquivo.Extensao))
                {
                    foreach (ItemAttribute itemAttribute in listaArq)
                    {
                        if (msgLogs.Count > 0)
                            IO.EscreverArquivo(arqSaida, string.Join(Environment.NewLine, msgLogs));

                        msgLogs.Clear();
                        idOrder.Clear();
                        contCliente++;
                        i++;

                        if (string.IsNullOrWhiteSpace(itemAttribute.ExternalIdAsset))
                            continue;

                        msgLogs.Add(string.Format("\n\r{0} [B2WIN]\t{1} {2}", DateTime.Now.ToLongTimeString(), itemAttribute.ExternalIdAsset, flRecursivo ? "[recursivo]" : string.Empty));
                        Console.WriteLine(msgLogs.Last());

                        if (lstContractLineItem == null || lstContractLineItem.Count == 0)
                        {
                            //TODO: gravar em log
                            msgLogs.Add(string.Format("{0} [CONTRACT LINE ITEM]\t Asset não encontrado {1}", DateTime.Now.ToLongTimeString(), itemAttribute.ExternalIdAsset));
                            Console.WriteLine(msgLogs.Last());
                            continue;
                        }

                        List<ContractLineItemSalesforce> clientes = lstContractLineItem.Where(x => x.AssetExternalId == itemAttribute.ExternalIdAsset && x.NumeroCliente == itemAttribute.NumeroCliente).ToList();
                        
                        if (clientes.Count > 1)
                            Debugger.Break();

                        ContractLineItemSalesforce clienteAtual = clientes.First(); 

                        msgLogs.Add(string.Format("{0} [CLIENTE] {1}", DateTime.Now.ToLongTimeString(), clienteAtual.NumeroCliente));
                        Console.WriteLine(msgLogs.Last().Trim());

                        List<AssetDTO> assets = null;

                        try
                        {
                            assets = SalesforceDAO.GetAssetsPorExternalId(itemAttribute.ExternalIdAsset, ref binding);
                        }
                        catch (Exception ex)
                        {
                            msgLogs.Add(string.Format("{0} [ERRO]\tFalha ao recuperar Asset por ExternalId {1}. {2}"
                                , DateTime.Now.ToLongTimeString()
                                , itemAttribute.ExternalIdAsset
                                , ex.Message));
                            Console.WriteLine(msgLogs.Last());
                            continue;
                        }

                        #region Asset não encontrado
                        if (assets == null || assets.Count() == 0)
                        {
                            msgLogs.Add(string.Format("{0}\tAsset não encontrado com ExternalId {1} ...", DateTime.Now.ToLongTimeString(), itemAttribute.ExternalIdAsset));
                            Console.WriteLine(msgLogs.Last());
                            continue;
                        }
                        #endregion

                        #region Asset encontrado
                        AssetDTO asset = assets.First();
                        idOrder.Append(asset.OrderId);

                        if (string.IsNullOrWhiteSpace(clienteAtual.ContractId))
                        {
                            msgLogs.Add(string.Format("{0} [ERRO CONTRACT_ID] Contract Id não encontrado para o Asset {1} ...", DateTime.Now.ToLongTimeString(), lstContractLineItem.FirstOrDefault().AssetId));
                            Console.WriteLine(msgLogs.Last());
                            continue;
                        }

                        itemAttribute.NumeroCliente = string.IsNullOrWhiteSpace(itemAttribute.NumeroCliente) ? string.IsNullOrWhiteSpace(asset.NumeroCliente) ? itemAttribute.NumeroCliente : asset.NumeroCliente : itemAttribute.NumeroCliente;
                        itemAttribute.ExternalIdPod = string.IsNullOrWhiteSpace(itemAttribute.ExternalIdPod) ? string.IsNullOrWhiteSpace(asset.PointofDeliveryExternalId) ? itemAttribute.ExternalIdPod : asset.PointofDeliveryExternalId : itemAttribute.ExternalIdPod;

                        if (asset != null && string.IsNullOrWhiteSpace(asset.OrderId))
                        {
                            msgLogs.Add(string.Format("{0} [ASSET]\tOrder não encontrado para o Asset {1} ..."
                                , DateTime.Now.ToLongTimeString()
                                , asset.Id));
                            Console.WriteLine(msgLogs.Last());
                            Debugger.Break();
                        }
                        else
                        {
                            #region Asset com Order

                            msgLogs.Add(string.Format("{0} [ASSET]\tOrder encontrado {1} para o Asset {2} ..."
                                , DateTime.Now.ToLongTimeString()
                                , asset.OrderId
                                , asset.Id));
                            Console.WriteLine(msgLogs.Last());

                            #region Atualizar Order_item_attribute e Asset_item_attibute
                            msgLogs.Add(string.Format("{0} Carregando Attribute Items do Asset e Order ...", DateTime.Now.ToLongTimeString()));
                            Console.WriteLine(msgLogs.Last());

                            try
                            {
                                lstObjetosAssetItemAttr.AddRange(AtualizarItemAttributes(new ItemAttribute[] { itemAttribute }.ToList(), asset.OrderId, arqSaida, false));
                            }
                            catch (InvalidDataException ex)
                            {
                                bool apagado = false;

                                //Apagar Order e respectivos dados de Order Item e ItemAttributes (para Asset e OrderItem)
                                msgLogs.AddRange(ApagarOrderEDerivados(asset, ref apagado));
                                if (apagado)
                                {
                                    try
                                    {
                                        CarregarB2Win(empresa, new List<ContractLineItemSalesforce> { clienteAtual }, new List<ItemAttribute> { itemAttribute }, lote, tipoCliente, arquivo, true);
                                        lstContractLineItem = SalesforceDAO.GetContractLineItemsByExternalId(itemAttribute.ExternalIdAsset, ref binding);
                                        assets = SalesforceDAO.GetAssetsPorExternalId(itemAttribute.ExternalIdAsset, ref binding);
                                        idOrder.Clear();
                                        idOrder.Append(assets.FirstOrDefault().OrderId);
                                    }
                                    catch
                                    {
                                        throw new Exception(string.Format("{0} Falha ao recarregar o Asset {1} / Order {2}"
                                            , DateTime.Now.ToLongTimeString()
                                            , asset.Id
                                            , asset.OrderId));
                                    }
                                }
                            }
                            #endregion

                            if (lstContractLineItem.Count == 0)
                            {
                                msgLogs.Add(string.Format("{0} [ERRO]\tContract Line não encontrado para o Asset {1}. Cliente: {2}."
                                    , DateTime.Now.ToLongTimeString()
                                    , asset.ExternalId
                                    , itemAttribute.NumeroCliente));
                                Console.WriteLine(msgLogs.Last());
                                continue;
                            }

                            if (lstContractLineItem.Count > 0 && !lstContractLineItem.FirstOrDefault().ContractType.Contains("B2"))
                            {
                                msgLogs.Add(string.Format("{0} [INFO]\tContract Type {1} não é B2B/B2C: {2}. Cliente: {3}."
                                    , DateTime.Now.ToLongTimeString()
                                    , asset.ContractId
                                    , lstContractLineItem.FirstOrDefault().ContractType
                                    , itemAttribute.NumeroCliente));
                                Console.WriteLine(msgLogs.Last());
                            }

                            msgLogs.Add(string.Format("{0} Carregando Asset {1} ...", DateTime.Now.ToLongTimeString(), lstContractLineItem.FirstOrDefault().AssetId));
                            Console.WriteLine(msgLogs.Last());

                            #region Atualizar Asset
                            if (string.IsNullOrWhiteSpace(asset.Id))
                                Debugger.Break();

                            sfObj = new sObject();
                            sfObj.type = "Asset";
                            sfObj.Id = lstContractLineItem.FirstOrDefault().AssetId;


                            sfObj.Any = new System.Xml.XmlElement[] {
                            SFDCSchemeBuild.GetNewXmlElement("NE__Action__c", "Add"),  /* NOVO */
                            SFDCSchemeBuild.GetNewXmlElement("NE__Status__c", "Active"),  /* NOVO */
                            SFDCSchemeBuild.GetNewXmlElement("Status", "Active"),  /* NOVO */
                            SFDCSchemeBuild.GetNewXmlElement("NE__BASEONETIMEFEE__C", "0.0"),  /* NOVO */
                            SFDCSchemeBuild.GetNewXmlElement("NE__BASERECURRINGCHARGE__C", "0.0"),  /* NOVO */
                            SFDCSchemeBuild.GetNewXmlElement("NE__ONETIMEFEEOV__C", "0.0"),  /* NOVO */
                            SFDCSchemeBuild.GetNewXmlElement("NE__RECURRINGCHARGEOV__C", "0.0"),  /* NOVO */
                            SFDCSchemeBuild.GetNewXmlElement("NE__RECURRINGCHARGEFREQUENCY__C", "Monthly"),  /* NOVO */
                            SFDCSchemeBuild.GetNewXmlElement("CurrencyIsoCode", "BRL"),  /* NOVO */
                            SFDCSchemeBuild.GetNewXmlElement("NE__CatalogItem__c", "a0z1o000003z2FOAAY"),  /* NOVO */
                            SFDCSchemeBuild.GetNewXmlElement("NE__ProdId__c", "a1f1o00000bsF6tAAE"),  /* NOVO */
                            SFDCSchemeBuild.GetNewXmlElement("Quantity", "1"),  /* NOVO */
                            SFDCSchemeBuild.GetNewXmlElement("PointofDelivery__c", lstContractLineItem.FirstOrDefault().PointOfDelivery),
                            SFDCSchemeBuild.GetNewXmlElement("AccountId", lstContractLineItem.FirstOrDefault().AccountIdAsset),
                            SFDCSchemeBuild.GetNewXmlElement("NE__SERVICE_ACCOUNT__C", lstContractLineItem.FirstOrDefault().AccountIdAsset),
                            SFDCSchemeBuild.GetNewXmlElement("NE__BILLING_ACCOUNT__C", lstContractLineItem.FirstOrDefault().AccountIdAsset),
                            SFDCSchemeBuild.GetNewXmlElement("Contract__c", lstContractLineItem.FirstOrDefault().ContractId),
                            SFDCSchemeBuild.GetNewXmlElement("Company__c", "2005".Equals(codigoEmpresa) ? "AMPLA" : "2003".Equals(codigoEmpresa) ? "COELCE" : "CELG"),
                            SFDCSchemeBuild.GetNewXmlElement("Country__c", "BRASIL")
                            };

                            lstObjetos.Add(sfObj);
                            //SaveResult[] saveRecordType = binding.update(lstObjetos.ToArray());
                            //SaveResult resultado = saveRecordType.First();
                            //if (resultado != null && resultado.errors != null)
                            //{
                            //    foreach (Error err in resultado.errors)
                            //    {
                            //        msgLogs.Add(string.Format("[ERRO] {0}", err.message));
                            //        Console.WriteLine(msgLogs.Last());
                            //    }
                            //}
                            //lstObjetos.Clear();
                            #endregion


                            msgLogs.Add(string.Format("{0} Carregando Contract {1} ...", DateTime.Now.ToLongTimeString(), asset.ContractId));
                            Console.WriteLine(msgLogs.Last());

                            #region Atualizar Contract
                            //Contrato com  order id,       número do cliente,              companyid__c e  country: 
                            //              CNT_Quote__c,   CNT_ExternalContract_ID_2__c,   CompanyID__c,   ShippingCountry 
                            sfObj = new sObject();
                            sfObj.type = "Contract";
                            sfObj.Id = lstContractLineItem.FirstOrDefault().ContractId;
                            if (string.IsNullOrWhiteSpace(lstContractLineItem.FirstOrDefault().ContractId))
                            {
                                //TODO: log
                                msgLogs.Add(string.Format("{0} Contrato External ID vazio para o Asset {1}"
                                    , DateTime.Now.ToLongTimeString()
                                    , asset.ExternalId));
                                Console.WriteLine(msgLogs.Last());
                                continue;
                            }

                            sfObj.Any = new System.Xml.XmlElement[] {
                            SFDCSchemeBuild.GetNewXmlElement("CNT_Quote__c", idOrder.ToString()),
                            SFDCSchemeBuild.GetNewXmlElement("CNT_ExternalContract_ID_2__c", lstContractLineItem.FirstOrDefault().NumeroCliente),
                            SFDCSchemeBuild.GetNewXmlElement("CompanyID__c", codigoEmpresa),
                            SFDCSchemeBuild.GetNewXmlElement("Status", "Activated"),
                            SFDCSchemeBuild.GetNewXmlElement("ShippingCountry", "BRASIL")
                            };

                            lstObjetos.Add(sfObj);
                            //saveRecordType = binding.update(lstObjetos.ToArray());
                            //resultado = saveRecordType.First();
                            //if (resultado != null && resultado.errors != null)
                            //{
                            //    foreach (Error err in resultado.errors)
                            //    {
                            //        msgLogs.Add(string.Format("[ERRO] {0}", err.message));
                            //        Console.WriteLine(msgLogs.Last());
                            //    }
                            //}
                            //lstObjetos.Clear();
                            #endregion

                            msgLogs.Add(string.Format("{0} Carregando PoD {1} ...", DateTime.Now.ToLongTimeString(), lstContractLineItem.FirstOrDefault().PointOfDelivery));
                            Console.WriteLine(msgLogs.Last());

                            #region Atualizar Point of Delivery
                            if (string.IsNullOrWhiteSpace(lstContractLineItem.FirstOrDefault().PointOfDelivery))
                            {
                                msgLogs.Add(string.Format("{0}\t[PoD]\t PoD não associado ao Contract Line {1} ...", DateTime.Now.ToLongTimeString(), lstContractLineItem[0].ContractId));
                                Console.WriteLine(msgLogs.Last());
                            }
                            else
                            {
                                sfObj = new sObject();
                                sfObj.type = "PointofDelivery__c";
                                sfObj.Id = lstContractLineItem.FirstOrDefault().PointOfDelivery;

                                sfObj.Any = new System.Xml.XmlElement[] {
                                    SFDCSchemeBuild.GetNewXmlElement("CNT_Contract__c", lstContractLineItem.FirstOrDefault().ContractId)
                                };

                                lstObjetos.Add(sfObj);
                                //saveRecordType = binding.update(lstObjetos.ToArray());
                                //resultado = saveRecordType.First();
                                //if (resultado != null && resultado.errors != null)
                                //{
                                //    foreach (Error err in resultado.errors)
                                //    {
                                //        msgLogs.Add(string.Format("[ERRO] {0}", err.message));
                                //        Console.WriteLine(msgLogs.Last());
                                //    }
                                //}
                                //lstObjetos.Clear();
                            }
                            #endregion

                            #endregion
                        }

                        #endregion


                        if (lstObjetos.Count > 200)
                        {
                            msgLogs.Add(string.Format("\n\n---------------------------\nAtualizando lote de dados.   Tempo total do processo: {0}\n---------------------------", new TimeSpan(DateTime.Now.Ticks).Add(-dtInicioProcesso).ToString(@"hh\:mm\:ss")));
                            Console.WriteLine(msgLogs.Last().Trim());

                            SaveResult[] saveResults = SalesforceDAO.Atualizar(lstObjetos, 10, ref binding);
                            List<Error[]> erros = saveResults.Where(sr => sr.errors != null).Select(s => s.errors).ToList();
                            foreach (Error[] err in erros)
                            {
                                msgLogs.Add(string.Format("{0} [ERRO B2WIN] {1}", DateTime.Now.ToLongTimeString(), string.Join(", ", err.Select(e => e.message))));
                                Console.WriteLine(msgLogs.Last().Trim());
                            }

                            if (lstObjetosAssetItemAttr.Count > 0)
                            {
                                saveResults = SalesforceDAO.Atualizar(lstObjetosAssetItemAttr, 29, ref binding);
                                erros = saveResults.Where(sr => sr.errors != null).Select(s => s.errors).ToList();
                                foreach (Error[] err in erros)
                                {
                                    msgLogs.Add(string.Format("{0} [ERRO B2WIN] {1}", DateTime.Now.ToLongTimeString(), string.Join(", ", err.Select(e => e.message))));
                                    Console.WriteLine(msgLogs.Last().Trim());
                                }

                                lstObjetosAssetItemAttr.Clear();
                            }

                            lstObjetos.Clear();
                        }

                    }

                    if (lstObjetos.Count > 0)
                    {
                        SaveResult[] saveResultsFinal = SalesforceDAO.Atualizar(lstObjetos, 10, ref binding);
                        List<Error[]> errosFinal = saveResultsFinal.Where(sr => sr.errors != null).Select(s => s.errors).ToList();
                        foreach (Error[] err in errosFinal)
                        {
                            msgLogs.Add(string.Format("{0} [ERRO B2WIN] {1}", DateTime.Now.ToLongTimeString(), string.Join(", ", err.Select(e => e.message))));
                            Console.WriteLine(msgLogs.Last().Trim());
                        }

                        if (lstObjetosAssetItemAttr.Count > 0)
                        {
                            saveResultsFinal = SalesforceDAO.Atualizar(lstObjetosAssetItemAttr, 29, ref binding);
                            errosFinal = saveResultsFinal.Where(sr => sr.errors != null).Select(s => s.errors).ToList();
                            foreach (Error[] err in errosFinal)
                            {
                                msgLogs.Add(string.Format("{0} [ERRO B2WIN] {1}", DateTime.Now.ToLongTimeString(), string.Join(", ", err.Select(e => e.message))));
                                Console.WriteLine(msgLogs.Last().Trim());
                            }
                            lstObjetosAssetItemAttr.Clear();
                        }

                        lstObjetos.Clear();
                    }

                    if (!flRecursivo)
                    {
                        msgLogs.Add(string.Format("Processo finalizado."));
                        Console.WriteLine(msgLogs.Last());
                        IO.EscreverArquivo(arqSaida, string.Join(Environment.NewLine, msgLogs));
                    }
                }  //fim arquivoSaida

                //Console.ReadKey();
            }
            catch (Exception ex)
            {
                Console.WriteLine("\nErro ao atualizar o registro: \n" + ex.Message + ex.InnerException.ToString() + ex.StackTrace);
                //TODO: log em arquivo
            }
        }


        private List<string> ApagarAssetItemAttributes(AssetDTO asset, ref bool sucesso)
        {
            sucesso = false;

            if (string.IsNullOrWhiteSpace(asset.Id))
                return new List<string>();

            StringBuilder msgLog = new StringBuilder();
            List<string> lstLog = new List<string>();
            if (!loggedIn)
            {
                Autenticacao auth = new Autenticacao(this.ambiente);
                if (!auth.ValidarLogin(ref this.binding))
                    return lstLog;

                this.loggedIn = true;
            }
            try
            {
                DeleteResult[] saveResults;
                List<Error[]> erros;

                //consultar AssetItemAttributes
                List<string> lstAssets = SalesforceDAO.GetAssetItemAttributesPorIdAsset(asset.Id, ref this.binding);
                foreach (string idAsset in lstAssets)
                {
                    msgLog.Clear();
                    saveResults = binding.delete(new string[] { idAsset });
                    erros = saveResults.Where(sr => sr.errors != null).Select(s => s.errors).ToList();
                    if (erros == null || erros.Count == 0)
                    {
                        sucesso = true;
                        msgLog.Append(string.Format("[ASSET ATTRIBUTE APAGADO] {0}", idAsset));
                        lstLog.Add(msgLog.ToString());
                        Console.WriteLine(lstLog.Last().Trim());
                    }
                    foreach (Error[] err in erros)
                    {
                        msgLog.Append(string.Format("[ERRO DELETE ASSET ATTRIBUTE] {0}", string.Join(", ", err.Select(e => e.message))));
                        lstLog.Add(msgLog.ToString());
                        Console.WriteLine(lstLog.Last().Trim());
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(string.Concat("\nErro ao apagar o registro: \n", ex.Message, ex.StackTrace));
            }
            return lstLog;
        }



        private List<string> ApagarOrderEDerivados(AssetDTO asset, ref bool sucesso)
        {
            sucesso = false;
            if (string.IsNullOrWhiteSpace(asset.OrderId))
                return new List<string>();

            StringBuilder msgLog = new StringBuilder();
            List<string> lstLog = new List<string>();
            if (!loggedIn)
            {
                Autenticacao auth = new Autenticacao(this.ambiente);
                if (!auth.ValidarLogin(ref this.binding))
                    return lstLog;

                this.loggedIn = true;
            }
            try
            {
                //TODO: consultar Id do OrderItem para apagá-lo tambem,
                //TODO: a partir do OrderItem.Id, apagar os AssetItems e OrderItems
                DeleteResult[] saveResults = binding.delete(new string[] { asset.OrderId });
                List<Error[]> erros = saveResults.Where(sr => sr.errors != null).Select(s => s.errors).ToList();
                if(erros == null || erros.Count == 0)
                {
                    sucesso = true;
                    msgLog.Append(string.Format("[ORDER APAGADO] {0}", asset.OrderId));
                    lstLog.Add(msgLog.ToString());
                    Console.WriteLine(lstLog.Last().Trim());

                    //consultar AssetItemAttributes
                    List<string> lstAssets = SalesforceDAO.GetAssetItemAttributesPorIdAsset(asset.Id, ref this.binding);
                    foreach(string idAsset in lstAssets)
                    {
                        msgLog.Clear();
                        saveResults = binding.delete(new string[] { idAsset });
                        erros = saveResults.Where(sr => sr.errors != null).Select(s => s.errors).ToList();
                        if (erros == null || erros.Count == 0)
                        {
                            sucesso = true;
                            msgLog.Append(string.Format("[ASSET ATTRIBUTE APAGADO] {0}", idAsset));
                            lstLog.Add(msgLog.ToString());
                            Console.WriteLine(lstLog.Last().Trim());
                        }
                        foreach (Error[] err in erros)
                        {
                            msgLog.Append(string.Format("[ERRO DELETE ASSET ATTRIBUTE] {0}", string.Join(", ", err.Select(e => e.message))));
                            lstLog.Add(msgLog.ToString());
                            Console.WriteLine(lstLog.Last().Trim());
                        }
                    }
                }
                foreach (Error[] err in erros)
                {
                    msgLog.Append(string.Format("[ERRO DELETE ORDER] {0}", string.Join(", ", err.Select(e => e.message))));
                    lstLog.Add(msgLog.ToString());
                    Console.WriteLine(lstLog.Last().Trim());
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(string.Concat("\nErro ao apagar o registro: \n", ex.Message, ex.StackTrace));
            }
            return lstLog;
        }



        /// <summary>
        /// Processa arquivos de faturamento e arrecadação para produtos de terceiros da EnelX
        /// </summary>
        /// <param name="caminhoArquivo">Origem do arquivo de carga</param>
        /// <param name="caminhoArquivoSeq">Sequenciais da tabela sequenciais_solucoes</param>
        /// <param name="tipoArquivo">arrec/fat</param>
        public void ProcessarEnelX(string caminhoArquivo, string caminhoArquivoSeq, string tipoArquivo)
        {
            #region Inicialização
            int i = 0;
            List<sObject> lstObjetos = new List<sObject>();
            List<sObject> lstObjetosAssetItemAttr = new List<sObject>();
            List<string> msgLogs = new List<string>();
            StringBuilder idOrder = new StringBuilder();
            StringBuilder sbLoader = new StringBuilder();
            StringBuilder sbErros = new StringBuilder();
            TimeSpan dtInicioProcesso = new TimeSpan(DateTime.Now.Ticks);
            #endregion

            Arquivo arquivo = new Arquivo(caminhoArquivo, '\t');
            Arquivo arquivoSeq = new Arquivo(caminhoArquivoSeq, '|');
            Arquivo arqErro = new Arquivo(string.Format("{0}{1}_{2}_{3}{4}"
                , caminhoArquivo
                , "\\sol"
                , tipoArquivo
                , "ERRO"
                , ".txt")
            , '|');
            Arquivo arquivoSeqNovo = new Arquivo(string.Format("{0}{1}_{2}{3}"
                , arquivo.Caminho
                , "\\sequenciaisNovo"
                , tipoArquivo
                , ".txt")
            , '|');

            try
            {
                List<string[]> listaArq = ArquivoLoader.GetEnelX(arquivo);
                if(listaArq == null || listaArq.Count == 0)
                {
                    Console.WriteLine("Arquivo não carregado: " + arquivo.CaminhoCompleto);
                    IO.EscreverArquivo(arqErro, string.Format("[ERRO] {0}", "Arquivo não carregado: " + arquivo.CaminhoCompleto));
                    return;
                }

                List<string[]> listaSeq = ArquivoLoader.GetEnelX(arquivoSeq);
                if (listaSeq == null || listaSeq.Count == 0)
                {
                    Console.WriteLine("Arquivo não carregado: " + arquivoSeq.CaminhoCompleto);
                    IO.EscreverArquivo(arqErro, string.Format("[ERRO] {0}", "Arquivo não carregado: " + arquivoSeq.CaminhoCompleto));
                    return;
                }
                
                List<string> listaSeqNovo = new List<string>();

                List<string> empresas = listaArq.Select(c => { return string.Concat(c[11], "|", c[16]); }).Distinct().ToList();

                foreach(string comb in empresas)
                {
                    Console.Write(comb + " ");

                    string codEmpresa = comb.Split(new char[] { '|' })[0];
                    string codProduto = comb.Split(new char[] { '|' })[1];

                    decimal soma = 0;
                    string seqAtual = string.Empty;
                    int seqIndex = "arrec".Equals(tipoArquivo) ? 4 : 3;

                    try
                    {
                        seqAtual = listaSeq.Where(x => x[0] == codEmpresa)
                            .Where(x => x[1] == codProduto)
                            .Select(d => { return d[seqIndex]; }).First();
                    }
                    catch(Exception ex)
                    {
                        IO.EscreverArquivo(arqErro, string.Format("[SEQ] {0} {1}", ex.Message, comb));
                        continue;
                    }

                    int seqProxima = Int32.Parse(seqAtual);
                    //seqProxima++;

                    listaSeqNovo.Add(string.Format("{0}|{1}|{2}|{3}", tipoArquivo, codEmpresa, codProduto, (seqProxima+1).ToString()));
                    
                    //---------------------------------------------------
                    #region Cabeçalho
                    Arquivo arqSaida = new Arquivo(string.Format("{0}{1}_{2}_{3}{4}{5}{6}{7}"
                        , arquivo.Caminho
                        , "\\sol"
                        , tipoArquivo
                        , codEmpresa
                        , codProduto
                        , "_"
                        , seqProxima.ToString().PadLeft(6, '0')
                        , ".amp")
                    , '|');

                    
                    sbLoader.AppendFormat("A1{0}{1}{2}{3}{4}{5}"
                        , codProduto
                        , "COELCE".PadRight(20, ' ')
                        , DateTime.Now.ToString("yyyyMMdd")
                        , seqProxima.ToString().PadLeft(6, '0')
                        , string.Empty.PadLeft(42,' ')
                        , Environment.NewLine);

                    List<string[]> listao = listaArq.Where(x => x[11] == codEmpresa)
                        .Where(x => x[16] == codProduto).ToList();

                    try
                    {
                        IO.EscreverArquivo(arqSaida, sbLoader.ToString());
                        sbLoader.Clear();
                    }
                    catch (Exception ex)
                    {
                        IO.EscreverArquivo(arqErro, string.Format("[CABECALHO] {0} {1}", ex.Message, comb));
                        continue;
                    }

                    #endregion
                    //---------------------------------------------------

                    //---------------------------------------------------
                    #region Corpo
                    foreach (string[] linha in listao)
                    {
                        try
                        {
                            sbLoader.AppendFormat("{0}{1}{2}{3}{4}{5}{6}{7}{8}{9}{10}{11}{12}{13}{14}{15}{16}"
                                , "C"
                                , linha[1]
                                , linha[2]
                                , linha[3]
                                , linha[4]
                                , linha[5]
                                , linha[6]
                                , linha[7]
                                , linha[8]
                                , linha[9]
                                , linha[10]
                                , linha[11]
                                , tipoArquivo.Equals("fat") ? linha[12].Trim().PadLeft(10, ' ') : linha[12]
                                , linha[13]
                                , linha[14]
                                , linha[15]
                                , Environment.NewLine);

                            soma += Decimal.Parse(linha[18].Trim(), CultureInfo.GetCultureInfo("pt-BR"));
                        }
                        catch (Exception ex)
                        {
                            IO.EscreverArquivo(arqErro, string.Format("[CORPO] {0} {1}", ex.Message, string.Join("|", linha)));
                        }
                    }

                    IO.EscreverArquivo(arqSaida, sbLoader.ToString().Trim());
                    sbLoader.Clear();

                    #endregion
                    //---------------------------------------------------

                    //---------------------------------------------------
                    #region Rodapé

                    sbLoader.AppendFormat("\n{0}{1}{2}{3}"
                    , "Z"
                    , listao.Count.ToString().PadLeft(6,'0')
                    , (soma * 100).ToString("F0").PadLeft(9, '0')
                    , string.Empty.PadLeft(64,' '));

                    try
                    {
                        IO.EscreverArquivo(arqSaida, sbLoader.ToString());
                        sbLoader.Clear();
                    }
                    catch (Exception ex)
                    {

                    }
                    #endregion
                    //---------------------------------------------------
                }
                IO.EscreverArquivo(arquivoSeqNovo, string.Join(Environment.NewLine, listaSeqNovo));
            }
            catch (Exception ex)
            {
                Console.WriteLine("\nErro ao atualizar o registro: \n" + ex.Message + ex.StackTrace);
                IO.EscreverArquivo(arqErro, string.Format("[ERRO DESCONHECIDO] {0} {1}", ex.Message, ex.StackTrace));
            }
        }


        /// <summary>
        /// Identifica, em uma dada lista de clientes, aqueles não carregados no Salesforce.
        /// </summary>
        /// <param name="lstClientesSynergia"></param>
        /// <param name="arq"></param>
        /// <returns>Lista de clientes não existentes no Salesforce.</returns>
        public List<string> ValidarClientesSalesforce(List<string> lstClientesSynergia)
        {
            this.autenticar();
            SforceService binding = this.binding;

            List<ClienteSalesforce> lstPoDs = SalesforceDAO.GetContasByListaClientes(this.codigoEmpresa, lstClientesSynergia, ref binding);
            List<String> lstClientesSF = lstPoDs.Select(c => c.NumeroCliente).ToList();
            lstPoDs = null;

            if (lstClientesSF.Count == lstClientesSynergia.Count)
                return new List<string>();

            return lstClientesSynergia.Except(lstClientesSF).ToList();
        }




        /// <summary>
        /// Processo Enelx para identificar o canal de venda de uma lista de cliente/empresa/produto
        /// </summary>
        /// <param name="caminhoArquivo"></param>
        /// <param name="caminhoArquivoSeq"></param>
        /// <param name="tipoArquivo"></param>
        public void IdentificarCanalVenda()
        {
            #region Inicialização
            int i = 0;
            List<sObject> lstObjetos = new List<sObject>();
            List<sObject> lstObjetosAssetItemAttr = new List<sObject>();
            List<string> msgLogs = new List<string>();
            StringBuilder idOrder = new StringBuilder();
            StringBuilder sbLoader = new StringBuilder();
            StringBuilder sbErros = new StringBuilder();
            TimeSpan dtInicioProcesso = new TimeSpan(DateTime.Now.Ticks);
            #endregion

            Arquivo arqBase = new Arquivo(@"C:\temp\EnelX\consulta2.txt", '|');
            Arquivo arqCanais = new Arquivo(@"C:\temp\EnelX\cliente_produto_canal_setor2.txt", '|');
            Arquivo arqErro = new Arquivo(string.Format("{0}{1}{2}"
                , @"C:\temp\EnelX\Enelx_ERRO_"
                , DateTime.Now.ToString("yyyyMMdd_HHmmss")
                , ".txt")
            , '|');
            Arquivo arqFinal = new Arquivo(string.Format("{0}{1}{2}"
                , @"C:\temp\EnelX\Enelx_Canais_"
                , DateTime.Now.ToString("yyyyMMdd_HHmmss")
                , ".txt")
            , '|');


            try
            {
                List<string[]> listaBase = ArquivoLoader.GetEnelX(arqBase);
                if (listaBase == null || listaBase.Count == 0)
                {
                    Console.WriteLine("Arquivo não carregado: " + arqBase.CaminhoCompleto);
                    IO.EscreverArquivo(arqErro, string.Format("[ERRO] {0}", "Arquivo não carregado: " + arqBase.CaminhoCompleto));
                    return;
                }

                List<string[]> listaCanais = ArquivoLoader.GetEnelX(arqCanais);
                if (listaCanais == null || listaCanais.Count == 0)
                {
                    Console.WriteLine("Arquivo não carregado: " + arqCanais.CaminhoCompleto);
                    IO.EscreverArquivo(arqErro, string.Format("[ERRO] {0}", "Arquivo não carregado: " + arqCanais.CaminhoCompleto));
                    return;
                }

                int cont = 0;
                foreach(string[] linha in listaBase)
                {
                    string[] aa = listaCanais.Where(x => x[0] == linha[0] && x[1] == linha[1] && x[2] == linha[2]).FirstOrDefault();

                    string canal = aa == null || string.IsNullOrWhiteSpace(aa[3]) ? "0" : aa[3];

                    cont++;
                    
                    if (cont % 5000 == 0)
                        Console.WriteLine(cont);

                    IO.EscreverArquivo(arqFinal, string.Format("{0}|{1}|{2}|{3}|{4}", linha[0], linha[1], linha[2], canal, Environment.NewLine));
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine("\nErro ao atualizar o registro: \n" + ex.Message + ex.StackTrace);
                IO.EscreverArquivo(arqErro, string.Format("[ERRO DESCONHECIDO] {0} {1}", ex.Message, ex.StackTrace));
            }
        }


        #region Carga B2Win via Import Loader
        
        /// <summary>
        /// Transforma os dados de B2Win do Synergia no formato de carga para o "NE__Order__c" do Salesforce.
        /// </summary>
        /// <param name="empresa"></param>
        /// <param name="lote"></param>
        /// <param name="caminhoArquivo"></param>
        /// <param name="tipoCliente"></param>
        public void ProcessarB2WinOrder(string empresa, string lote, string caminhoArquivo, TipoCliente tipoCliente)
        {
            #region Validar empresa

            string nomeEmpresa = "2003".Equals(empresa) ? "COELCE" : "2005".Equals(empresa) ? "AMPLA" : "2018".Equals(empresa) ? "CELG" : string.Empty;
            if (string.IsNullOrWhiteSpace(empresa))
            {
                throw new ArgumentException(string.Format("Empresa '{0}' não informada ou inválida.", empresa));
            }

            #endregion

            #region Inicialização
            int i = 0;
            int contCliente = 0;
            List<sObject> lstObjetos = new List<sObject>();
            List<sObject> lstObjetosAssetItemAttr = new List<sObject>();
            List<string> msgLogs = new List<string>();
            StringBuilder idOrder = new StringBuilder();
            StringBuilder sbLoader = new StringBuilder();
            TimeSpan dtInicioProcesso = new TimeSpan(DateTime.Now.Ticks);
            #endregion

            try
            {
                #region Histório de execução
                //Arquivo arquivo = new Arquivo(@"C:\temp\CargaB2Win\190327\b2win.txt", ';');
                //Arquivo arquivo = new Arquivo(@"C:\temp\CargaB2Win\190327\b2win02.txt", ';');
                //Arquivo arquivo = new Arquivo(@"C:\temp\CargaB2Win\190327\6340029.txt", '|');
                //Arquivo arquivo = new Arquivo(string.Format(@"C:\temp\CargaB2Win\190327\b2win03.txt", '|');
                //Arquivo arquivo = new Arquivo(string.Format(@"C:\temp\CargaB2Win\190327\{0}", nomeArquivo), '|');
                #endregion

                string nomenclatura = string.Concat("Lote", lote.PadLeft(2,'0'), "_0_Order_", DateTime.Now.ToString("yyyyMMdd_HHmm"), "_LOADERSF");
                Arquivo arquivo = new Arquivo(caminhoArquivo, ';');

                #region Selecionar modo de entrada ----------------------------------------------------
                //PROCESSO POR ARQUIVO DE ENTRADA
                List<ItemAttribute> listaArq = ArquivoLoader.GetItemsAttributes(arquivo, typeof(B2WinDTO), tipoCliente.ToString());
                #endregion

                msgLogs.Add(string.Format("{0}[ARQUIVO]\t{1}", (DateTime.Now.AddHours(1) - DateTime.Now), arquivo.CaminhoCompleto));
                Console.WriteLine(msgLogs.Last());

                Arquivo arqLoaderSF = new Arquivo(arquivo.Caminho, nomenclatura, arquivo.Extensao);
                IO.EscreverArquivo(arqLoaderSF, "NE__AccountId__r:Account:ExternalId__c	NE__Type__c	NE__CatalogId__c	NE__COMMERCIALMODELID__C	NE__BillAccId__r:Account:ExternalId__c	NE__ServAccId__r:Account:ExternalId__c	NE__OrderStatus__c	Country__c	CurrencyIsoCode	NE__ConfigurationStatus__c	ExternalId_Pod	ExternalId_Asset	ExternalId_Contract");
                
                foreach (ItemAttribute itemAttribute in listaArq)
                {
                    msgLogs.Clear();
                    idOrder.Clear();
                    contCliente++;
                    i++;

                    if (string.IsNullOrWhiteSpace(itemAttribute.ExternalIdAsset))
                        continue;

                    ProcessarIngressoOrder(itemAttribute, ref sbLoader);
                }

                if(sbLoader.Length > 0)
                    IO.EscreverArquivo(arqLoaderSF, sbLoader.ToString().Trim());
    
                arqLoaderSF.Dispose();

                Console.WriteLine("Processo finalizado.");
                Console.ReadKey();
            }
            catch (Exception ex)
            {
                Console.WriteLine("\nErro ao atualizar o registro: \n" + ex.Message + ex.StackTrace);
                Console.ReadKey();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="caminhoArquivo"></param>
        public void ProcessarB2WinOrderItem(string lote, string caminhoArquivo)
        {
            #region Inicialização
            int i = 0;
            List<sObject> lstObjetos = new List<sObject>();
            List<sObject> lstObjetosAssetItemAttr = new List<sObject>();
            List<string> msgLogs = new List<string>();
            StringBuilder idOrder = new StringBuilder();
            StringBuilder sbLoader = new StringBuilder();
            TimeSpan dtInicioProcesso = new TimeSpan(DateTime.Now.Ticks);
            #endregion

            try
            {
                #region Histório de execução
                //Arquivo arquivo = new Arquivo(@"C:\temp\CargaB2Win\190327\b2win.txt", ';');
                //Arquivo arquivo = new Arquivo(@"C:\temp\CargaB2Win\190327\b2win02.txt", ';');
                //Arquivo arquivo = new Arquivo(@"C:\temp\CargaB2Win\190327\6340029.txt", '|');
                //Arquivo arquivo = new Arquivo(string.Format(@"C:\temp\CargaB2Win\190327\b2win03.txt", '|');
                //Arquivo arquivo = new Arquivo(string.Format(@"C:\temp\CargaB2Win\190327\{0}", nomeArquivo), '|');
                #endregion

                Arquivo arquivo = new Arquivo(caminhoArquivo, '\t');

                #region Selecionar modo de entrada ----------------------------------------------------
                //PROCESSO POR ARQUIVO DE ENTRADA
                List<OrderSalesforce> listaArq = ArquivoLoader.GetOrders(arquivo);
                #endregion

                msgLogs.Add(string.Format("{0}[ARQUIVO]\t{1}", (DateTime.Now.AddHours(1) - DateTime.Now), arquivo.CaminhoCompleto));
                Console.WriteLine(msgLogs.Last());

                string nomenclatura = string.Concat("Lote", lote.PadLeft(2, '0'), "_2_OrderItems_", DateTime.Now.ToString("yyyyMMdd_HHmm"), "_LOADERSF");
                
                Arquivo arqLoaderSF = new Arquivo(arquivo.Caminho, nomenclatura, arquivo.Extensao);

                //cabecalho
                IO.EscreverArquivo(arqLoaderSF, "NE__OrderId__c\tNE__Action__c\tNE__Qty__c\tNE__CatalogItem__c\tNE__ProdId__c\tNE__Account__r:Account:ExternalId__c\tNE__Billing_Account__r:Account:ExternalId__c\tNE__CATALOG__C\tNE__Service_Account__r:Account:ExternalId__c\tNE__STATUS__C\tNE__ASSETITEMENTERPRISEID__C\tNE__BASEONETIMEFEE__C\tNE__BASERECURRINGCHARGE__C\tNE__ONETIMEFEEOV__C\tNE__RECURRINGCHARGEFREQUENCY__C\tNE__RECURRINGCHARGEOV__C\tNE__Country__c\tCurrencyIsoCode");

                foreach (OrderSalesforce order in listaArq)
                {
                    ProcessarIngressoOrderItem(order, ref sbLoader);
                }

                if (sbLoader.Length > 0)
                    IO.EscreverArquivo(arqLoaderSF, sbLoader.ToString().Trim());

                arqLoaderSF.Dispose();

                Console.WriteLine("Processo finalizado.");
                Console.ReadKey();
            }
            catch (Exception ex)
            {
                Console.WriteLine("\nErro ao atualizar o registro: \n" + ex.Message + ex.StackTrace);
                Console.ReadKey();
            }
        }



        public void ProcessarB2WinOrderTemp(string lote, string caminhoArquivo)
        {
            #region Inicialização
            int i = 0;
            List<sObject> lstObjetos = new List<sObject>();
            List<sObject> lstObjetosAssetItemAttr = new List<sObject>();
            List<string> msgLogs = new List<string>();
            StringBuilder idOrder = new StringBuilder();
            StringBuilder sbLoader = new StringBuilder();
            TimeSpan dtInicioProcesso = new TimeSpan(DateTime.Now.Ticks);
            #endregion

            try
            {
                #region Histório de execução
                //Arquivo arquivo = new Arquivo(@"C:\temp\CargaB2Win\190327\b2win.txt", ';');
                //Arquivo arquivo = new Arquivo(@"C:\temp\CargaB2Win\190327\b2win02.txt", ';');
                //Arquivo arquivo = new Arquivo(@"C:\temp\CargaB2Win\190327\6340029.txt", '|');
                //Arquivo arquivo = new Arquivo(string.Format(@"C:\temp\CargaB2Win\190327\b2win03.txt", '|');
                //Arquivo arquivo = new Arquivo(string.Format(@"C:\temp\CargaB2Win\190327\{0}", nomeArquivo), '|');
                #endregion

                Arquivo arquivo = new Arquivo(caminhoArquivo, ';');

                #region Selecionar modo de entrada ----------------------------------------------------
                //PROCESSO POR ARQUIVO DE ENTRADA
                List<OrderSalesforce> listaArq = ArquivoLoader.GetOrdersTemp(arquivo);
                #endregion

                msgLogs.Add(string.Format("{0}[ARQUIVO]\t{1}", (DateTime.Now.AddHours(1) - DateTime.Now), arquivo.CaminhoCompleto));
                Console.WriteLine(msgLogs.Last());

                string nomenclatura = string.Concat("Lote", lote.PadLeft(2, '0'), "_2_OrderItems_", DateTime.Now.ToString("yyyyMMdd_HHmm"), "_LOADERSF");

                Arquivo arqLoaderSF = new Arquivo(arquivo.Caminho, nomenclatura, arquivo.Extensao);

                //cabecalho
                IO.EscreverArquivo(arqLoaderSF, "NE__OrderId__c\tNE__Action__c\tNE__Qty__c\tNE__CatalogItem__c\tNE__ProdId__c\tNE__Account__r:Account:ExternalId__c\tNE__Billing_Account__r:Account:ExternalId__c\tNE__CATALOG__C\tNE__Service_Account__r:Account:ExternalId__c\tNE__STATUS__C\tNE__ASSETITEMENTERPRISEID__C\tNE__BASEONETIMEFEE__C\tNE__BASERECURRINGCHARGE__C\tNE__ONETIMEFEEOV__C\tNE__RECURRINGCHARGEFREQUENCY__C\tNE__RECURRINGCHARGEOV__C\tNE__Country__c\tCurrencyIsoCode");

                foreach (OrderSalesforce order in listaArq)
                {
                    ProcessarIngressoOrderItem(order, ref sbLoader);
                }

                if (sbLoader.Length > 0)
                    IO.EscreverArquivo(arqLoaderSF, sbLoader.ToString().Trim());

                arqLoaderSF.Dispose();

                Console.WriteLine("Processo finalizado.");
                Console.ReadKey();
            }
            catch (Exception ex)
            {
                Console.WriteLine("\nErro ao atualizar o registro: \n" + ex.Message + ex.StackTrace);
                Console.ReadKey();
            }
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="lote"></param>
        /// <param name="caminhoArquivo"></param>
        public void ProcessarB2WinAssetItemAttributes(string lote, string caminhoArquivo)
        {
            #region Inicialização
            int i = 0;
            int indexArq = 0;
            bool arqCriado = false;
            List<sObject> lstObjetos = new List<sObject>();
            List<sObject> lstObjetosAssetItemAttr = new List<sObject>();
            List<string> msgLogs = new List<string>();
            StringBuilder idOrder = new StringBuilder();
            StringBuilder sbLoader = new StringBuilder();
            TimeSpan dtInicioProcesso = new TimeSpan(DateTime.Now.Ticks);
            #endregion

            try
            {
                #region Histório de execução
                //Arquivo arquivo = new Arquivo(@"C:\temp\CargaB2Win\190327\b2win.txt", ';');
                //Arquivo arquivo = new Arquivo(@"C:\temp\CargaB2Win\190327\b2win02.txt", ';');
                //Arquivo arquivo = new Arquivo(@"C:\temp\CargaB2Win\190327\6340029.txt", '|');
                //Arquivo arquivo = new Arquivo(string.Format(@"C:\temp\CargaB2Win\190327\b2win03.txt", '|');
                //Arquivo arquivo = new Arquivo(string.Format(@"C:\temp\CargaB2Win\190327\{0}", nomeArquivo), '|');
                #endregion

                Arquivo arquivo = new Arquivo(caminhoArquivo, '|', false);

                #region Selecionar modo de entrada ----------------------------------------------------
                //PROCESSO POR ARQUIVO DE ENTRADA
                List<ItemAttribute> listaArq = ArquivoLoader.GetItemsAttributes(arquivo, typeof(B2WinDTO), TipoCliente.GB.ToString());
                //List<OrderItemSalesforce> listaArq = ArquivoLoader.GetOrderItems(arquivo);
                #endregion

                msgLogs.Add(string.Format("{0}[ARQUIVO]\t{1}", (DateTime.Now.AddHours(1) - DateTime.Now), arquivo.CaminhoCompleto));
                Console.WriteLine(msgLogs.Last());

                string nomenclatura = string.Concat("Lote", lote.PadLeft(2,'0'), "_3_AssetItemAttribute_{0}_", DateTime.Now.ToString("yyyyMMdd_HHmm"), "_LOADERSF");
                Arquivo arqLoaderSF = new Arquivo(arquivo.Caminho, string.Format(nomenclatura, ++indexArq), arquivo.Extensao);
                IO.EscreverArquivo(arqLoaderSF, "NE__Asset__r:Asset:ExternalId__c	Name	NE__Value__c	NE__Old_Value__c	NE__FamPropId__c	NE__FAMPROPEXTID__C");
                arqCriado = true;

                foreach (ItemAttribute item in listaArq)
                {
                    ProcessarIngressoAssetItemAttribute(item, ref sbLoader);

                    if (i > 100000 && !arqCriado)
                    {
                        arqLoaderSF = new Arquivo(arquivo.Caminho, string.Format(nomenclatura, ++indexArq), arquivo.Extensao);
                        IO.EscreverArquivo(arqLoaderSF, "NE__Asset__r:Asset:ExternalId__c	Name	NE__Value__c	NE__Old_Value__c	NE__FamPropId__c	NE__FAMPROPEXTID__C");
                        arqCriado = true;
                    }
                    //divide o limite de linhas desejado pela quantidade de ItemAtributes disponíveis para carga
                    if (i > (600000/9))
                    {
                        IO.EscreverArquivo(arqLoaderSF, sbLoader.ToString().Trim());
                        arqCriado = false;
                        sbLoader.Clear();
                        i = 0;
                    }
                    i++;
                }
                
                if(sbLoader.Length > 0)
                    IO.EscreverArquivo(arqLoaderSF, sbLoader.ToString().Trim());
    
                arqLoaderSF.Dispose();

                Console.WriteLine("Processo finalizado.");
                Console.ReadKey();
            }
            catch (Exception ex)
            {
                Console.WriteLine("\nErro ao atualizar o registro: \n" + ex.Message + ex.StackTrace);
                Console.ReadKey();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="lote"></param>
        /// <param name="arquivoOrderItems">Arquivo contendo External Id do Asset e o Id Order Item criado anteriormente.</param>
        /// <param name="arquivoB2Win">Arquivo original/inicial da carga do B2Win</param>
        public void ProcessarB2WinOrderItemAttributes(string lote, string arquivoOrderItems, string arquivoB2Win)
        {
            #region Inicialização
            int i = 0;
            int indexArq = 1;
            List<sObject> lstObjetos = new List<sObject>();
            List<sObject> lstObjetosAssetItemAttr = new List<sObject>();
            List<string> msgLogs = new List<string>();
            StringBuilder idOrder = new StringBuilder();
            StringBuilder sbLoader = new StringBuilder();
            TimeSpan dtInicioProcesso = new TimeSpan(DateTime.Now.Ticks);
            #endregion

            try
            {
                Arquivo arqOrders = new Arquivo(arquivoOrderItems, '\t');
                Arquivo arqB2Win = new Arquivo(arquivoB2Win, ';', false);
                string nomenclatura = string.Concat("Lote", lote.PadLeft(2,'0'), "_5_OrderItemAttributes_{0}_", DateTime.Now.ToString("yyyyMMdd_HHmm"), "_LOADERSF");

                #region Selecionar modo de entrada ----------------------------------------------------
                //PROCESSO POR ARQUIVOS DE ENTRADA
                List<OrderItemSalesforce> lstOrders = ArquivoLoader.GetOrderItems(arqOrders);

                Dictionary<string,OrderItemSalesforce> dicOrderItem = new Dictionary<string,OrderItemSalesforce>();
                foreach(OrderItemSalesforce orderitem in lstOrders)
                {
                    if (!dicOrderItem.ContainsKey(orderitem.AssetItemEnterpriseId))
                        dicOrderItem.Add(orderitem.AssetItemEnterpriseId, orderitem);
                    else
                        Debugger.Break();
                }

                List<ItemAttribute> lstB2Win = ArquivoLoader.GetItemsAttributes(arqB2Win, typeof(B2WinDTO), TipoCliente.GB.ToString());
                #endregion

                msgLogs.Add(string.Format("{0}[ARQUIVO]\t{1}", (DateTime.Now.AddHours(1) - DateTime.Now), arqOrders.CaminhoCompleto));
                Console.WriteLine(msgLogs.Last());

                Arquivo arqLoaderSF = new Arquivo(arqOrders.Caminho, string.Format(nomenclatura, indexArq), arqOrders.Extensao);
                //Arquivo arqSaida = new Arquivo(arqOrders.Caminho, string.Format(nomenclatura, "ERROS"), arqOrders.Extensao);
                IO.EscreverArquivo(arqLoaderSF, "NE__Order_Item__c	Name	NE__Value__c	NE__Old_Value__c	NE__FamPropId__c	NE__FAMPROPEXTID__C");

                foreach (ItemAttribute item in lstB2Win)
                {
                    i++;
                    try
                    {
                        ProcessarIngressoOrderItemAttribute(dicOrderItem, item, ref sbLoader);
                    }
                    catch(Exception ex)
                    {
                        //IO.EscreverArquivo(arqSaida, string.Format("[ERRO] Asset ExternalId {0} não encontrado no resultado de INSERT de OrderItem.", item.ExternalIdAsset));
                        continue;
                    }
                    if (i > 50000)
                    {
                        indexArq++;
                        IO.EscreverArquivo(arqLoaderSF, sbLoader.ToString().Trim());
                        sbLoader.Clear();
                        arqLoaderSF = new Arquivo(arqOrders.Caminho, string.Format(nomenclatura, indexArq), arqOrders.Extensao);
                        IO.EscreverArquivo(arqLoaderSF, "NE__Order_Item__c	Name	NE__Value__c	NE__Old_Value__c	NE__FamPropId__c	NE__FAMPROPEXTID__C");
                        i = 0;
                    }
                }
                
                if (sbLoader.Length > 0)
                    IO.EscreverArquivo(arqLoaderSF, sbLoader.ToString().Trim());
                
                arqLoaderSF.Dispose();
            }
            catch (Exception ex)
            {
                Console.WriteLine("\nErro ao atualizar o registro: \n" + ex.Message + ex.StackTrace);
                Console.ReadKey();
            }
        }



        //public void ProcessarB2WinOrderItemAttributesTemp(string lote, string arquivoB2WinAux)
        //{
        //    #region Inicialização
        //    int i = 0;
        //    int indexArq = 1;
        //    List<sObject> lstObjetos = new List<sObject>();
        //    List<sObject> lstObjetosAssetItemAttr = new List<sObject>();
        //    List<string> msgLogs = new List<string>();
        //    StringBuilder idOrder = new StringBuilder();
        //    StringBuilder sbLoader = new StringBuilder();
        //    TimeSpan dtInicioProcesso = new TimeSpan(DateTime.Now.Ticks);
        //    #endregion

        //    try
        //    {
        //        //MORREU--Arquivo arqOrders = new Arquivo(arquivoOrderItems, '\t');
        //        Arquivo arqB2Win = new Arquivo(arquivoB2WinAux, ';');
        //        string nomenclatura = string.Concat("Lote", lote.PadLeft(2, '0'), "_5_OrderItemAttributes_{0}_", DateTime.Now.ToString("yyyyMMdd_HHmm"), "_LOADERSF");

        //        #region Selecionar modo de entrada ----------------------------------------------------
        //        //PROCESSO POR ARQUIVOS DE ENTRADA
        //        List<OrderItemSalesforce> lstOrders = ArquivoLoader.GetOrdersTemp(arqB2Win);

        //        Dictionary<string, OrderItemSalesforce> dicOrderItem = new Dictionary<string, OrderItemSalesforce>();
        //        foreach (OrderItemSalesforce orderitem in lstOrders)
        //        {
        //            if (!dicOrderItem.ContainsKey(orderitem.AssetItemEnterpriseId))
        //                dicOrderItem.Add(orderitem.AssetItemEnterpriseId, orderitem);
        //            else
        //                Debugger.Break();
        //        }

        //        List<ItemAttribute> lstB2Win = ArquivoLoader.GetItemsAttributes(arqB2Win, typeof(B2WinDTO), TipoCliente.GB.ToString());
        //        #endregion

        //        msgLogs.Add(string.Format("{0}[ARQUIVO]\t{1}", (DateTime.Now.AddHours(1) - DateTime.Now), arqB2Win.CaminhoCompleto));
        //        Console.WriteLine(msgLogs.Last());

        //        Arquivo arqLoaderSF = new Arquivo(arqB2Win.Caminho, string.Format(nomenclatura, indexArq), arqB2Win.Extensao);
        //        //Arquivo arqSaida = new Arquivo(arqOrders.Caminho, string.Format(nomenclatura, "ERROS"), arqOrders.Extensao);
        //        IO.EscreverArquivo(arqLoaderSF, "NE__Order_Item__c	Name	NE__Value__c	NE__Old_Value__c	NE__FamPropId__c	NE__FAMPROPEXTID__C");

        //        foreach (ItemAttribute item in lstB2Win)
        //        {
        //            i++;
        //            try
        //            {
        //                ProcessarIngressoOrderItemAttribute(dicOrderItem, item, ref sbLoader);
        //            }
        //            catch (Exception ex)
        //            {
        //                //IO.EscreverArquivo(arqSaida, string.Format("[ERRO] Asset ExternalId {0} não encontrado no resultado de INSERT de OrderItem.", item.ExternalIdAsset));
        //                continue;
        //            }
        //            if (i > 50000)
        //            {
        //                indexArq++;
        //                IO.EscreverArquivo(arqLoaderSF, sbLoader.ToString().Trim());
        //                sbLoader.Clear();
        //                arqLoaderSF = new Arquivo(arqB2Win.Caminho, string.Format(nomenclatura, indexArq), arqB2Win.Extensao);
        //                IO.EscreverArquivo(arqLoaderSF, "NE__Order_Item__c	Name	NE__Value__c	NE__Old_Value__c	NE__FamPropId__c	NE__FAMPROPEXTID__C");
        //                i = 0;
        //            }
        //        }

        //        if (sbLoader.Length > 0)
        //            IO.EscreverArquivo(arqLoaderSF, sbLoader.ToString().Trim());

        //        arqLoaderSF.Dispose();
        //    }
        //    catch (Exception ex)
        //    {
        //        Console.WriteLine("\nErro ao atualizar o registro: \n" + ex.Message + ex.StackTrace);
        //        Console.ReadKey();
        //    }
        //}


        
        public void ProcessarB2WinOrderItemAttributesManual(string lote, string arquivoB2Win)
        {
            #region Inicialização
            int i = 0;
            int indexArq = 1;
            List<sObject> lstObjetos = new List<sObject>();
            List<sObject> lstObjetosAssetItemAttr = new List<sObject>();
            List<string> msgLogs = new List<string>();
            StringBuilder idOrder = new StringBuilder();
            StringBuilder sbLoader = new StringBuilder();
            TimeSpan dtInicioProcesso = new TimeSpan(DateTime.Now.Ticks);
            #endregion

            try
            {
                Arquivo arqB2Win = new Arquivo(arquivoB2Win, '|');
                string nomenclatura = string.Concat("Lote", lote.PadLeft(2, '0'), "_5_OrderItemAttributes_{0}_", DateTime.Now.ToString("yyyyMMdd_HHmm"), "_LOADERSF");

                #region Selecionar modo de entrada ----------------------------------------------------
                //PROCESSO POR ARQUIVOS DE ENTRADA
                List<OrderItemSalesforce> lstOrders = ArquivoLoader.GetOrderItemsManual(arqB2Win);

                Dictionary<string, OrderItemSalesforce> dicOrderItem = new Dictionary<string, OrderItemSalesforce>();
                foreach (OrderItemSalesforce orderitem in lstOrders)
                {
                    if (!dicOrderItem.ContainsKey(orderitem.Id))
                        dicOrderItem.Add(orderitem.Id, orderitem);
                    else
                        Debugger.Break();
                }

                List<ItemAttribute> lstB2Win = ArquivoLoader.GetItemsAttributes(arqB2Win, typeof(B2WinDTO), TipoCliente.GB.ToString());
                #endregion

                msgLogs.Add(string.Format("{0}[ARQUIVO]\t{1}", (DateTime.Now.AddHours(1) - DateTime.Now), arqB2Win.CaminhoCompleto));
                Console.WriteLine(msgLogs.Last());

                Arquivo arqLoaderSF = new Arquivo(arqB2Win.Caminho, string.Format(nomenclatura, indexArq), arqB2Win.Extensao);
                //Arquivo arqSaida = new Arquivo(arqOrders.Caminho, string.Format(nomenclatura, "ERROS"), arqOrders.Extensao);
                IO.EscreverArquivo(arqLoaderSF, "NE__Order_Item__c	Name	NE__Value__c	NE__Old_Value__c	NE__FamPropId__c	NE__FAMPROPEXTID__C");

                foreach (ItemAttribute item in lstB2Win)
                {
                    i++;
                    try
                    {
                        ProcessarIngressoOrderItemAttributeManual(dicOrderItem, item, ref sbLoader);
                    }
                    catch (Exception ex)
                    {
                        //IO.EscreverArquivo(arqSaida, string.Format("[ERRO] Asset ExternalId {0} não encontrado no resultado de INSERT de OrderItem.", item.ExternalIdAsset));
                        continue;
                    }
                    if (i > 50000)
                    {
                        indexArq++;
                        IO.EscreverArquivo(arqLoaderSF, sbLoader.ToString().Trim());
                        sbLoader.Clear();
                        arqLoaderSF = new Arquivo(arqB2Win.Caminho, string.Format(nomenclatura, indexArq), arqB2Win.Extensao);
                        IO.EscreverArquivo(arqLoaderSF, "NE__Order_Item__c	Name	NE__Value__c	NE__Old_Value__c	NE__FamPropId__c	NE__FAMPROPEXTID__C");
                        i = 0;
                    }
                }

                if (sbLoader.Length > 0)
                    IO.EscreverArquivo(arqLoaderSF, sbLoader.ToString().Trim());

                arqLoaderSF.Dispose();
            }
            catch (Exception ex)
            {
                Console.WriteLine("\nErro ao atualizar o registro: \n" + ex.Message + ex.StackTrace);
                Console.ReadKey();
            }
        }

        /// <summary>
        /// Prepara o arquivo de atualização de Assets do processo global de carga de B2Wins.
        /// </summary>
        /// <param name="lote">Lote do cliente (campo SECTOR da tabela CLIENTES, no Synergia)</param>
        /// <param name="arquivoOrders">Arquivo resultante do ingresso de Orders via "Import Loader".  Ex.: Lote25_0_Order_20190518_1848_LOADERSF_RESULTADO.txt</param>
        public void ProcessarB2WinAsset(string lote, string arquivoOrders)
        {
            StringBuilder sbLoader = new StringBuilder();
            List<string> msgLogs = new List<string>();
            
            Arquivo arquivo = new Arquivo(arquivoOrders, '\t', true);
            List<AssetDTO> lstAssets = ArquivoLoader.ExtractAssetsFromOrders(arquivo);

            int i = 0;
            int contCliente = 0;

            msgLogs.Add(string.Format("{0}[ARQUIVO]\t{1}", (DateTime.Now.AddHours(1) - DateTime.Now), arquivo.CaminhoCompleto));
            Console.WriteLine(msgLogs.Last());

            string nomenclatura = string.Concat("Lote", lote.PadLeft(2, '0'), "_6_Assets_", DateTime.Now.ToString("yyyyMMdd_HHmm"), "_LOADERSF");
            
            Arquivo arqLoaderSF = new Arquivo(arquivo.Caminho, nomenclatura, arquivo.Extensao);
            IO.EscreverArquivo(arqLoaderSF, "ExternalId__c	NE__Status__c	NE__Action__c	Quantity	NE__BASEONETIMEFEE__C	NE__BASERECURRINGCHARGE__C	NE__ONETIMEFEEOV__C	NE__RECURRINGCHARGEOV__C	NE__RECURRINGCHARGEFREQUENCY__C	CurrencyIsoCode	Status	PointofDelivery__r:PointofDelivery__c:ExternalId__c	Account:Account:ExternalId__c	NE__ASSETITEMENTERPRISEID__C	NE__Service_Account__r:Account:ExternalId__c	NE__Billing_Account_Asset_Item__r:Account:ExternalId__c	NE__CatalogItem__c	NE__ProdId__c	NE__Order_Config__c	Contract__r:Contract:ExternalID__c	Company__c	Country__c");

            foreach (AssetDTO asset in lstAssets)
            {
                msgLogs.Clear();
                contCliente++;
                i++;

                if (string.IsNullOrWhiteSpace(asset.ExternalId))
                    continue;

                ProcessarAtualizacaoAsset(asset, ref sbLoader);
            }

            if (sbLoader.Length > 0)
                IO.EscreverArquivo(arqLoaderSF, sbLoader.ToString().Trim());

            arqLoaderSF.Dispose();

            Console.WriteLine("Processo finalizado.");
        }

        /// <summary>
        /// Prepara o arquivo de atualização de Point of Delivery's do processo global de carga de B2Wins.
        /// </summary>
        /// <param name="lote">Lote do cliente (campo SECTOR da tabela CLIENTES, no Synergia)</param>
        /// <param name="arquivoOrders">Arquivo resultante do ingresso de Orders via "Import Loader".  Ex.: Lote25_0_Order_20190518_1848_LOADERSF_RESULTADO.txt</param>
        public void ProcessarB2WinPointOfDelivery(string lote, string arquivoOrders)
        {
            StringBuilder sbLoader = new StringBuilder();
            List<string> msgLogs = new List<string>();

            Arquivo arquivo = new Arquivo(arquivoOrders, '\t', true);
            List<AssetDTO> lstAssets = ArquivoLoader.ExtractAssetsFromOrders(arquivo);

            int i = 0;
            int contCliente = 0;

            msgLogs.Add(string.Format("{0}[ARQUIVO]\t{1}", (DateTime.Now.AddHours(1) - DateTime.Now), arquivo.CaminhoCompleto));
            Console.WriteLine(msgLogs.Last());

            string nomenclatura = string.Concat("Lote", lote.PadLeft(2, '0'), "_7_PoDs_", DateTime.Now.ToString("yyyyMMdd_HHmm"), "_LOADERSF");

            Arquivo arqLoaderSF = new Arquivo(arquivo.Caminho, nomenclatura, arquivo.Extensao);
            IO.EscreverArquivo(arqLoaderSF, "ExternalId__c	CNT_Contract__r:Contract:ExternalID__c");

            foreach (AssetDTO asset in lstAssets)
            {
                msgLogs.Clear();
                contCliente++;
                i++;

                if (string.IsNullOrWhiteSpace(asset.ExternalId))
                    continue;

                ProcessarAtualizacaoPointOfDelivery(asset, ref sbLoader);
            }

            if (sbLoader.Length > 0)
                IO.EscreverArquivo(arqLoaderSF, sbLoader.ToString().Trim());

            arqLoaderSF.Dispose();

            Console.WriteLine("Processo finalizado.");
        }

        /// <summary>
        /// Prepara o arquivo de atualização de Contracts do processo global de carga de B2Wins.
        /// </summary>
        /// <param name="lote">Lote do cliente (campo SECTOR da tabela CLIENTES, no Synergia)</param>
        /// <param name="arquivoOrders">Arquivo resultante do ingresso de Orders via "Import Loader".  Ex.: Lote25_0_Order_20190518_1848_LOADERSF_RESULTADO.txt</param>
        public void ProcessarB2WinContract(string lote, string arquivoOrders)
        {
            StringBuilder sbLoader = new StringBuilder();
            List<string> msgLogs = new List<string>();

            Arquivo arquivo = new Arquivo(arquivoOrders, '\t', true);
            List<AssetDTO> lstAssets = ArquivoLoader.ExtractAssetsFromOrders(arquivo);

            int i = 0;
            int contCliente = 0;

            msgLogs.Add(string.Format("{0}[ARQUIVO]\t{1}", (DateTime.Now.AddHours(1) - DateTime.Now), arquivo.CaminhoCompleto));
            Console.WriteLine(msgLogs.Last());

            string nomenclatura = string.Concat("Lote", lote.PadLeft(2, '0'), "_8_Contracts_", DateTime.Now.ToString("yyyyMMdd_HHmm"), "_LOADERSF");

            Arquivo arqLoaderSF = new Arquivo(arquivo.Caminho, nomenclatura, arquivo.Extensao);
            IO.EscreverArquivo(arqLoaderSF, "ExternalId__c	CNT_Quote__c	CNT_ExternalContract_ID_2__c	CompanyID__c	Status	ShippingCountry");

            foreach (AssetDTO asset in lstAssets)
            {
                msgLogs.Clear();
                contCliente++;
                i++;

                if (string.IsNullOrWhiteSpace(asset.ExternalId))
                    continue;

                ProcessarAtualizacaoContract(asset, ref sbLoader);
            }

            if (sbLoader.Length > 0)
                IO.EscreverArquivo(arqLoaderSF, sbLoader.ToString().Trim());

            arqLoaderSF.Dispose();

            Console.WriteLine("Processo finalizado.");
        }


        /// <summary>
        /// Analisa o resultado do Ingresso de Orders, identificando os registros que precisam ser recarregados via Delta ou ter os dados de Account atualizados.
        /// </summary>
        /// <param name="lote"></param>
        /// <param name="root"></param>
        public void TratarRechazosOrders(string root)
        {
            #region Validar empresa

            string nomeEmpresa = "2003".Equals(codigoEmpresa) ? "COELCE" : "2005".Equals(codigoEmpresa) ? "AMPLA" : "2018".Equals(codigoEmpresa) ? "CELG" : string.Empty;
            if (string.IsNullOrWhiteSpace(codigoEmpresa))
            {
                throw new ArgumentException(string.Format("Empresa '{0}' não informada ou inválida.", codigoEmpresa));
            }

            #endregion

            #region Autenticação
            if (!loggedIn)
            {
                Autenticacao auth = new Autenticacao(this.ambiente);
                if (!auth.ValidarLogin(ref this.binding))
                    return;

                this.loggedIn = true;
            }
            #endregion

            StringBuilder sbLoader = new StringBuilder();
            List<string> msgLogs = new List<string>();
            List<string> lstDelta = new List<string>();
            List<string> lstMerge = new List<string>();
            List<string> lstUpdt = new List<string>();
            List<string> lstErros = new List<string>();
            List<string> lstExtEncontrados = new List<string>();

            string[] diretorios = Directory.GetDirectories(root);
            foreach(string dir in  diretorios)
            {
                string[] arquivos = Directory.GetFiles(dir, "*0_Order_*RESULTADO*", SearchOption.TopDirectoryOnly);
                if(arquivos == null || arquivos.Length == 0)
                {
                    //TODO: log
                    continue;
                }

                foreach (string arq in arquivos)
                {
                    if (!arq.Contains("_RESULTADO"))
                        continue;

                    Arquivo arqEntrada = new Arquivo(arq, '\t', true);
                    List<B2WinDTO> lstAssets = ArquivoLoader.ExtractResultadoB2WinOrders(arqEntrada).Where(r => r.Status == "Failed").ToList();

                    string lote = dir.Substring(dir.Length - 2, 2);
                    string dataHora = DateTime.Now.ToString("yyyyMMdd_HHmmss");

                    Dictionary<string, string> dicExtIdClientes = new Dictionary<string, string>();
                    List<string> lstExtId = new List<string>();
                    List<string> lstClientes = new List<string>();
                    int cont = 1;
                    foreach (B2WinDTO linha in lstAssets)
                    {
                        if(cont < 200 && cont < lstAssets.Count)
                        {
                            lstExtId.Add(linha.NumeroCliente);
                            //dicExtIdClientes.Add(string.Concat(linha.ExternalIdAccount,"|", linha.NumeroCliente), linha.NumeroCliente);
                            cont++;
                            continue ;
                        }
                        cont = 1;

                        List<ClienteSalesforce> lstAccounts = SalesforceDAO.GetContasPorNumeroCliente("2003", string.Concat("'", string.Join("','", lstExtId.ToArray()), "'"), ref this.binding);

                        //identifica os External Ids existentes no SF
                        List<string> _auxExtEncontrados = lstExtId.Intersect(lstAccounts.Select(a => { return a.NumeroCliente; })).ToList();
                        lstExtEncontrados.AddRange(_auxExtEncontrados);

                        //Remove os External Ids encontrados e envia para a lista de Delta somente os não encontrados no SF
                        if (_auxExtEncontrados != null && _auxExtEncontrados.Count > 0)
                            lstExtId.RemoveAll(c => _auxExtEncontrados.Contains(c));

                        lstDelta.AddRange(lstExtId);
                        lstExtId.Clear();
                    }

                    Arquivo arqRechazos = new Arquivo(root, string.Concat("B2WIN_RECHAZO_", lote, "_", dataHora), "txt");
                    Arquivo arqDelta = new Arquivo(AppDomain.CurrentDomain.BaseDirectory, string.Format("DeltaClientes_{0}_{1}", lote, dataHora), "txt");

                    //foreach (string externalId in lstExtEncontrados)
                    //{
                    //    List<B2WinDTO> linhas = lstAssets.Where(e => e.ExternalIdAccount == externalId).ToList();
                    //    if (linhas != null && linhas.Count > 0)
                    //    {
                    //        foreach (B2WinDTO l in linhas)
                    //        {
                    //            lstMerge.Add(string.Concat("[MERGE]\t", l.NumeroCliente, "\t", l));
                    //        }
                    //        continue;
                    //    }
                    //    B2WinDTO linha = lstAssets.Where(e => e.ExternalIdAccount == externalId).First();

                    //    if (linha.Erro.ToLower().Contains("externalid__c in entity account"))
                    //    {
                    //        if (linha.ExternalIdContract.ToLower().Contains("invalido"))
                    //            continue;

                    //        List<ClienteSalesforce> lstAccounts = SalesforceDAO.GetContasPorDocumento(this.codigoEmpresa, linha.ExternalIdContract.Substring(3, linha.ExternalIdContract.Length - 1).Replace("D", "").Replace("CPF", "").Replace("CNPJ", ""), ref binding);
                    //        if (lstAccounts != null && lstAccounts.Count > 0)
                    //        {
                    //            lstMerge.Add(string.Concat("[MERGE]\t", linha.NumeroCliente, "\t", linha));
                    //            continue;
                    //        }

                    //        #region Atualizar Account
                    //        if (lstAccounts.First().ExternalId.ToLower().Contains("invalido") && !lstAccounts.First().ExternalId.Equals(linha.ExternalIdAccount))
                    //        {
                    //            lstUpdt.Add(string.Concat("[UPDATE]\t", lstAccounts.First().Id, "\t", linha));

                    //            List<sObject> lstObjetos = new List<sObject>();
                    //            sObject sfObj = new sObject();
                    //            sfObj.type = "Account";
                    //            sfObj.Id = lstAccounts.First().Id;

                    //            sfObj.Any = new System.Xml.XmlElement[] {
                    //            SFDCSchemeBuild.GetNewXmlElement("ExternalId__c", linha.ExternalIdAccount),
                    //            };

                    //            lstObjetos.Add(sfObj);
                    //            SaveResult[] saveRecordType = binding.update(lstObjetos.ToArray());
                    //            SaveResult resultado = saveRecordType.First();
                    //            if (resultado != null && resultado.errors != null)
                    //            {
                    //                foreach (Error err in resultado.errors)
                    //                {
                    //                    msgLogs.Add(string.Format("[ERRO] {0}", err.message));
                    //                    lstErros.Add(string.Concat(msgLogs.Last()));
                    //                    Console.WriteLine(msgLogs.Last());
                    //                }
                    //            }
                    //            lstObjetos.Clear();
                    //        }
                    //        #endregion
                    //    }
                    //}

                    #region Executar o delta
                    //TODO: considerar mais de 1 conta?
                    ProcessStartInfo startInfo = new ProcessStartInfo();
                    startInfo.CreateNoWindow = false;
                    startInfo.UseShellExecute = true;
                    startInfo.FileName = @"C:\Projetos\Salesforce\CargaEmergenciaNew\CargaEmergenciaNew\Solus\Solus\bin\Debug\Solus.exe";
                    //startInfo.WindowStyle = ProcessWindowStyle.Hidden;
                    startInfo.Arguments = string.Concat("\"Sales Force\" 1 ", nomeEmpresa, " ", arqDelta.NomeExtensao);

                    try
                    {
                        if (lstDelta.Count > 0)
                        {
                            IO.EscreverArquivo(arqRechazos, string.Join(",\r", lstDelta.Select(d => { return string.Concat("[DELTA]\t", d);}).ToArray()));
                            IO.EscreverArquivo(arqDelta, string.Join(",", lstDelta.ToArray()));
                            lstDelta.Clear();
                        }
                        if (lstMerge.Count > 0)
                        {
                            IO.EscreverArquivo(arqRechazos, string.Join(",\r", lstMerge.ToArray()));
                            lstMerge.Clear();
                        }
                        if (lstUpdt.Count > 0)
                        {
                            IO.EscreverArquivo(arqRechazos, string.Join(",\r", lstUpdt.ToArray()));
                            lstUpdt.Clear();
                        }
                    }
                    catch(Exception ex)
                    {
                        //TODO: log
                        continue;
                    }

                    try
                    {
                        // Start the process with the info we specified.
                        // Call WaitForExit and then the using statement will close.
                        using (Process exeProcess = Process.Start(startInfo))
                        {
                            exeProcess.WaitForExit();
                        }
                    }
                    catch
                    {
                        //TODO: Log error.
                    }
                    #endregion
                    
                }
            }

            int i = 0;
            //int contCliente = 0;

            //msgLogs.Add(string.Format("{0}[ARQUIVO]\t{1}", (DateTime.Now.AddHours(1) - DateTime.Now), arquivo.CaminhoCompleto));
            //Console.WriteLine(msgLogs.Last());

            //string nomenclatura = string.Concat("Lote", lote.PadLeft(2, '0'), "_8_Contracts_", DateTime.Now.ToString("yyyyMMdd_HHmm"), "_LOADERSF");

            //Arquivo arqLoaderSF = new Arquivo(arquivo.Caminho, nomenclatura, arquivo.Extensao);
            //IO.EscreverArquivo(arqLoaderSF, "ExternalId__c	CNT_Quote__c	CNT_ExternalContract_ID_2__c	CompanyID__c	Status	ShippingCountry");

            //foreach (AssetDTO asset in lstAssets)
            //{
            //    msgLogs.Clear();
            //    contCliente++;
            //    i++;

            //    if (string.IsNullOrWhiteSpace(asset.ExternalId))
            //        continue;

            //    ProcessarAtualizacaoContract(asset, ref sbLoader);
            //}

            //if (sbLoader.Length > 0)
            //    IO.EscreverArquivo(arqLoaderSF, sbLoader.ToString().Trim());

            //arqLoaderSF.Dispose();

            //Console.WriteLine("Processo finalizado.");
        }


        /// <summary>
        /// Recarrega os dados de B2Win para Orders que foram identificados sem OrderItems associados.
        /// O processo deve buscar o Cliente, via Asset, e então recuperar os dados de B2Win para prosseguir com a carga.
        /// </summary>
        /// <param name="root"></param>
        public void RecarregarOrdersPorOrderId(string root)
        {
            #region Validar empresa

            string nomeEmpresa = "2003".Equals(codigoEmpresa) ? "COELCE" : "2005".Equals(codigoEmpresa) ? "AMPLA" : "2018".Equals(codigoEmpresa) ? "CELG" : string.Empty;
            if (string.IsNullOrWhiteSpace(codigoEmpresa))
            {
                throw new ArgumentException(string.Format("Empresa '{0}' não informada ou inválida.", codigoEmpresa));
            }

            #endregion

            #region Autenticação
            if (!loggedIn)
            {
                Autenticacao auth = new Autenticacao(this.ambiente);
                if (!auth.ValidarLogin(ref this.binding))
                    return;

                this.loggedIn = true;
            }
            #endregion

            StringBuilder sbLoader = new StringBuilder();
            List<string> msgLogs = new List<string>();
            List<string> lstDelta = new List<string>();
            List<string> lstMerge = new List<string>();
            List<string> lstUpdt = new List<string>();
            List<string> lstErros = new List<string>();
            List<string> lstExtEncontrados = new List<string>();

            //string[] diretorios = Directory.GetDirectories(root);
            //foreach (string dir in diretorios)
            //{
            //    string[] arquivos = Directory.GetFiles(dir, "*0_Order_*RESULTADO*", SearchOption.TopDirectoryOnly);
            //    if (arquivos == null || arquivos.Length == 0)
            //    {
            //        //TODO: log
            //        continue;
            //    }

            //    foreach (string arq in arquivos)
            //    {
            //        if (!arq.Contains("_RESULTADO"))
            //            continue;

                    Arquivo arqEntrada = new Arquivo(root, '\t', true);
                    List<B2WinDTO> lstB2Win = ArquivoLoader.ExtractIdOrders(arqEntrada);

                    //string lote = dir.Substring(dir.Length - 2, 2);
                    string dataHora = DateTime.Now.ToString("yyyyMMdd_HHmmss");

                    Dictionary<string, string> dicExtIdClientes = new Dictionary<string, string>();
                    List<string> lstExtId = new List<string>();
                    List<string> lstClientes = new List<string>();
                    int cont = 1;
                    foreach (B2WinDTO linha in lstB2Win)
                    {
                        if (cont < 200 && cont < lstB2Win.Count)
                        {
                            lstExtId.Add(linha.NumeroCliente);
                            //dicExtIdClientes.Add(string.Concat(linha.ExternalIdAccount,"|", linha.NumeroCliente), linha.NumeroCliente);
                            cont++;
                            continue;
                        }
                        cont = 1;

                        List<ClienteSalesforce> lstAccounts = SalesforceDAO.GetContasPorNumeroCliente("2003", string.Concat("'", string.Join("','", lstExtId.ToArray()), "'"), ref this.binding);

                        //identifica os External Ids existentes no SF
                        List<string> _auxExtEncontrados = lstExtId.Intersect(lstAccounts.Select(a => { return a.NumeroCliente; })).ToList();
                        lstExtEncontrados.AddRange(_auxExtEncontrados);

                        //Remove os External Ids encontrados e envia para a lista de Delta somente os não encontrados no SF
                        if (_auxExtEncontrados != null && _auxExtEncontrados.Count > 0)
                            lstExtId.RemoveAll(c => _auxExtEncontrados.Contains(c));

                        lstDelta.AddRange(lstExtId);
                        lstExtId.Clear();
                    }

                    Arquivo arqRechazos = new Arquivo(root, string.Concat("B2WIN_RECHAZO_", dataHora), "txt");
                    Arquivo arqDelta = new Arquivo(AppDomain.CurrentDomain.BaseDirectory, string.Format("DeltaClientes_{0}_{1}", dataHora), "txt");

                    //foreach (string externalId in lstExtEncontrados)
                    //{
                    //    List<B2WinDTO> linhas = lstAssets.Where(e => e.ExternalIdAccount == externalId).ToList();
                    //    if (linhas != null && linhas.Count > 0)
                    //    {
                    //        foreach (B2WinDTO l in linhas)
                    //        {
                    //            lstMerge.Add(string.Concat("[MERGE]\t", l.NumeroCliente, "\t", l));
                    //        }
                    //        continue;
                    //    }
                    //    B2WinDTO linha = lstAssets.Where(e => e.ExternalIdAccount == externalId).First();

                    //    if (linha.Erro.ToLower().Contains("externalid__c in entity account"))
                    //    {
                    //        if (linha.ExternalIdContract.ToLower().Contains("invalido"))
                    //            continue;

                    //        List<ClienteSalesforce> lstAccounts = SalesforceDAO.GetContasPorDocumento(this.codigoEmpresa, linha.ExternalIdContract.Substring(3, linha.ExternalIdContract.Length - 1).Replace("D", "").Replace("CPF", "").Replace("CNPJ", ""), ref binding);
                    //        if (lstAccounts != null && lstAccounts.Count > 0)
                    //        {
                    //            lstMerge.Add(string.Concat("[MERGE]\t", linha.NumeroCliente, "\t", linha));
                    //            continue;
                    //        }

                    //        #region Atualizar Account
                    //        if (lstAccounts.First().ExternalId.ToLower().Contains("invalido") && !lstAccounts.First().ExternalId.Equals(linha.ExternalIdAccount))
                    //        {
                    //            lstUpdt.Add(string.Concat("[UPDATE]\t", lstAccounts.First().Id, "\t", linha));

                    //            List<sObject> lstObjetos = new List<sObject>();
                    //            sObject sfObj = new sObject();
                    //            sfObj.type = "Account";
                    //            sfObj.Id = lstAccounts.First().Id;

                    //            sfObj.Any = new System.Xml.XmlElement[] {
                    //            SFDCSchemeBuild.GetNewXmlElement("ExternalId__c", linha.ExternalIdAccount),
                    //            };

                    //            lstObjetos.Add(sfObj);
                    //            SaveResult[] saveRecordType = binding.update(lstObjetos.ToArray());
                    //            SaveResult resultado = saveRecordType.First();
                    //            if (resultado != null && resultado.errors != null)
                    //            {
                    //                foreach (Error err in resultado.errors)
                    //                {
                    //                    msgLogs.Add(string.Format("[ERRO] {0}", err.message));
                    //                    lstErros.Add(string.Concat(msgLogs.Last()));
                    //                    Console.WriteLine(msgLogs.Last());
                    //                }
                    //            }
                    //            lstObjetos.Clear();
                    //        }
                    //        #endregion
                    //    }
                    //}

                    #region Executar o delta
                    //TODO: considerar mais de 1 conta?
                    ProcessStartInfo startInfo = new ProcessStartInfo();
                    startInfo.CreateNoWindow = false;
                    startInfo.UseShellExecute = true;
                    startInfo.FileName = @"C:\Projetos\Salesforce\CargaEmergenciaNew\CargaEmergenciaNew\Solus\Solus\bin\Debug\Solus.exe";
                    //startInfo.WindowStyle = ProcessWindowStyle.Hidden;
                    startInfo.Arguments = string.Concat("\"Sales Force\" 1 ", nomeEmpresa, " ", arqDelta.NomeExtensao);

                    try
                    {
                        if (lstDelta.Count > 0)
                        {
                            IO.EscreverArquivo(arqRechazos, string.Join(",\r", lstDelta.Select(d => { return string.Concat("[DELTA]\t", d); }).ToArray()));
                            IO.EscreverArquivo(arqDelta, string.Join(",", lstDelta.ToArray()));
                            lstDelta.Clear();
                        }
                        if (lstMerge.Count > 0)
                        {
                            IO.EscreverArquivo(arqRechazos, string.Join(",\r", lstMerge.ToArray()));
                            lstMerge.Clear();
                        }
                        if (lstUpdt.Count > 0)
                        {
                            IO.EscreverArquivo(arqRechazos, string.Join(",\r", lstUpdt.ToArray()));
                            lstUpdt.Clear();
                        }
                    }
                    catch (Exception ex)
                    {
                        //TODO: log
                        //continue;
                    }

                    try
                    {
                        // Start the process with the info we specified.
                        // Call WaitForExit and then the using statement will close.
                        using (Process exeProcess = Process.Start(startInfo))
                        {
                            exeProcess.WaitForExit();
                        }
                    }
                    catch
                    {
                        //TODO: Log error.
                    }
                    #endregion

                //}
            //}

            int i = 0;
            //int contCliente = 0;

            //msgLogs.Add(string.Format("{0}[ARQUIVO]\t{1}", (DateTime.Now.AddHours(1) - DateTime.Now), arquivo.CaminhoCompleto));
            //Console.WriteLine(msgLogs.Last());

            //string nomenclatura = string.Concat("Lote", lote.PadLeft(2, '0'), "_8_Contracts_", DateTime.Now.ToString("yyyyMMdd_HHmm"), "_LOADERSF");

            //Arquivo arqLoaderSF = new Arquivo(arquivo.Caminho, nomenclatura, arquivo.Extensao);
            //IO.EscreverArquivo(arqLoaderSF, "ExternalId__c	CNT_Quote__c	CNT_ExternalContract_ID_2__c	CompanyID__c	Status	ShippingCountry");

            //foreach (AssetDTO asset in lstAssets)
            //{
            //    msgLogs.Clear();
            //    contCliente++;
            //    i++;

            //    if (string.IsNullOrWhiteSpace(asset.ExternalId))
            //        continue;

            //    ProcessarAtualizacaoContract(asset, ref sbLoader);
            //}

            //if (sbLoader.Length > 0)
            //    IO.EscreverArquivo(arqLoaderSF, sbLoader.ToString().Trim());

            //arqLoaderSF.Dispose();

            //Console.WriteLine("Processo finalizado.");
        }

        #endregion


        [ComVisible(true)]
        internal void CarregarGeracaoDistribuida(string empresa, string caminhoArquivo, string tipoCliente = "GB")
        {
            #region Autenticação
            if (!loggedIn)
            {
                Autenticacao auth = new Autenticacao(this.ambiente);
                if (!auth.ValidarLogin(ref this.binding))
                    return;

                this.loggedIn = true;
            }
            #endregion

            int i = 0;
            int contCliente = 0;
            sObject sfObj = null;
            List<sObject> lstObjetos = new List<sObject>();
            List<sObject> lstUpdate = new List<sObject>();
            List<string> msgLogs = new List<string>();
            StringBuilder idOrder = new StringBuilder();

            try
            {
                #region Histório de execução
                #endregion

                Arquivo arquivo = new Arquivo(caminhoArquivo, '|');
                List<GeracaoDistribuidaDTO> listaArq = ArquivoLoader.GetClientesGeracaoDistribuida(arquivo, tipoCliente);
                listaArq = listaArq.Select(x => { x.CodigoEmpresa = codigoEmpresa; return x; }).ToList();

                msgLogs.Add(string.Format("{0}[ARQUIVO]\t{1}", DateTime.Now.ToLongTimeString(), arquivo.CaminhoCompleto));
                Console.WriteLine(msgLogs.Last());

                using (Arquivo arqSaida = new Arquivo(arquivo.Caminho, string.Concat(arquivo.Nome, "_SAIDA"), arquivo.Extensao))
                {
                    List<string> lstClienteGerador = new List<string>();
                    List<ContractLineItemSalesforce> lstContratos = null;
                    SaveResult[] saveRecordType = null;
                    SaveResult resultado = null;

                    foreach (GeracaoDistribuidaDTO item in listaArq)
                    {
                        if (msgLogs.Count > 0)
                            IO.EscreverArquivo(arqSaida, string.Join(Environment.NewLine, msgLogs));

                        msgLogs.Clear();
                        idOrder.Clear();
                        contCliente++;
                        i++;

                        #region Buscar Contratos
                        if (string.IsNullOrWhiteSpace(item.ContratoGerador))
                            continue;

                        if (lstClienteGerador.Count < 200)
                        {
                            if (!lstClienteGerador.Contains(item.ClienteGerador))
                                lstClienteGerador.Add(item.ClienteGerador);

                            if (!lstClienteGerador.Contains(item.ClienteConsumidor))
                                lstClienteGerador.Add(item.ClienteConsumidor);

                            if (contCliente < listaArq.Count())
                                continue;
                        }

                        msgLogs.Add(string.Format("{0} Buscando contratos...", DateTime.Now.ToLongTimeString(), arquivo.CaminhoCompleto));
                        Console.WriteLine(msgLogs.Last());

                        lstContratos = SalesforceDAO.GetContratosPorCliente(
                            this.codigoEmpresa, 
                            "'B2B','B2C'", 
                            string.Join(",", lstClienteGerador.Select( c => string.Concat("'", c, "'")).ToArray()), 
                            ref binding);
                        #endregion

                        #region Atualizações
                        foreach (ContractLineItemSalesforce contrato in lstContratos)
                        {
                            if(string.IsNullOrWhiteSpace(contrato.ContractId))
                            {
                                msgLogs.Add(string.Format("{0}\t[ERRO]\tID Contrato vazio para {1}", DateTime.Now.ToLongTimeString(), contrato.ContractExternalId));
                                Console.WriteLine(msgLogs.Last());
                                continue;
                            }

                            //identifica os beneficiarios de cada cliente/contrato
                            List<GeracaoDistribuidaDTO> lista1 = listaArq.Where(c => c.ClienteGerador == contrato.NumeroCliente).Distinct().ToList();
                            Dictionary<string, string> listaArq2 = null;
                            try
                            {
                                listaArq2 = lista1.ToDictionary(x => x.ClienteConsumidor, x => x.Percentual);
                            }
                            catch { }
                            contrato.Beneficiarios = listaArq2;
                            
                            //contrato de um cliente Consumidor e não Gerador
                            if (contrato.Beneficiarios.Count == 0)
                                continue;

                            #region Atualizar Contratos
                            sfObj = new sObject();
                            sfObj.type = "Contract";
                            sfObj.Id = contrato.ContractId;

                            sfObj.Any = new System.Xml.XmlElement[] {
                                    //SFDCSchemeBuild.GetNewXmlElement("CNT_Distributed_Generation_Eval__c", "true"),
                                    SFDCSchemeBuild.GetNewXmlElement("CNT_Generation_Capability__c", "10"),
                                    SFDCSchemeBuild.GetNewXmlElement("CNT_Generation_Sources__c", "Solar"),
                                    SFDCSchemeBuild.GetNewXmlElement("CNT_Generation_Type__c", contrato.GenerationType)
                                };
                            lstObjetos.Add(sfObj);

                            try
                            {
                                saveRecordType = binding.update(lstObjetos.ToArray());
                                resultado = saveRecordType.First();
                                if (resultado != null && resultado.errors != null)
                                {
                                    foreach (Error err in resultado.errors)
                                    {
                                        msgLogs.Add(string.Format("[ERRO] {0}", err.message));
                                        Console.WriteLine(msgLogs.Last());
                                    }
                                }
                            }
                            catch(Exception ex)
                            {
                                msgLogs.Add(string.Format("[ERRO] Contrato {0} Tipo {1} - {2}", contrato.ContractId, contrato.GenerationType, string.Concat(ex.Message, ex.StackTrace, ex.InnerException != null ? ex.InnerException.Message : string.Empty)));
                                Console.WriteLine(msgLogs.Last());
                                continue;
                            }

                            lstObjetos.Clear();
                            #endregion


                            #region Atualizar PoD Gerador -------------------------------------------

                            sfObj = new sObject();
                            sfObj.type = "PointofDelivery__c";
                            sfObj.Id = contrato.PointOfDelivery;

                            sfObj.Any = new System.Xml.XmlElement[] {
                                    SFDCSchemeBuild.GetNewXmlElement("CNT_IsGD__c", "1")
                                };
                            lstUpdate.Add(sfObj);

                            try
                            {
                                saveRecordType = binding.update(lstUpdate.ToArray());
                                resultado = saveRecordType.First();
                                if (resultado != null && resultado.errors != null)
                                {
                                    foreach (Error err in resultado.errors)
                                    {
                                        msgLogs.Add(string.Format("[ERRO] {0}", err.message));
                                        Console.WriteLine(msgLogs.Last());
                                    }
                                }
                            }
                            catch (Exception ex)
                            {
                                msgLogs.Add(string.Format("[ERRO] Contrato {0} Tipo {1} - {2}", contrato.ContractId, contrato.GenerationType, string.Concat(ex.Message, ex.StackTrace, ex.InnerException != null ? ex.InnerException.Message : string.Empty)));
                                Console.WriteLine(msgLogs.Last());
                                continue;
                            }

                            lstUpdate.Clear();

                            #endregion



                            #region Deletar todos os registros de Geraçao Distribuida para o cliente gerador


                            #endregion

                            #region Ingressar Geracao Distribuida por Cliente
                            List<string> beneficiarios = new List<string>();
                            foreach (string cliente in contrato.Beneficiarios.Keys)
                            {
                                sfObj = new sObject();
                                sfObj.type = "CNT_Distributed_Generation__c";

                                beneficiarios = lstContratos.Where(c => c.NumeroCliente == cliente).Select( cnt => cnt.PointOfDelivery).ToList();
                                if(beneficiarios.Count == 0)
                                {
                                    //Debugger.Break();
                                    continue;
                                }
                                if (contrato.Beneficiarios.Count > 3)
                                {
                                }
                                sfObj.Any = new System.Xml.XmlElement[] {
                                    //SFDCSchemeBuild.GetNewXmlElement("CNT_Distributed_Generation_Eval__c", "true"),
                                    SFDCSchemeBuild.GetNewXmlElement("CNT_Donator__c", contrato.ContractId),
                                    SFDCSchemeBuild.GetNewXmlElement("CNT_Percentage__c",  contrato.Beneficiarios[cliente].Replace(',','.')),
                                    SFDCSchemeBuild.GetNewXmlElement("CNT_Benefited__c", beneficiarios.First())
                                };

                                lstObjetos.Add(sfObj);

                                #region Atualizar PoD Beneficiarios -------------------------------------------

                                sfObj = new sObject();
                                sfObj.type = "PointofDelivery__c";
                                sfObj.Id = beneficiarios.First();

                                sfObj.Any = new System.Xml.XmlElement[] {
                                    SFDCSchemeBuild.GetNewXmlElement("CNT_IsGD__c", contrato.NumeroCliente.Equals(cliente) ? "1" : "2")
                                };
                                lstUpdate.Add(sfObj);

                                try
                                {
                                    saveRecordType = binding.update(lstUpdate.ToArray());
                                    resultado = saveRecordType.First();
                                    if (resultado != null && resultado.errors != null)
                                    {
                                        foreach (Error err in resultado.errors)
                                        {
                                            msgLogs.Add(string.Format("[ERRO] {0}", err.message));
                                            Console.WriteLine(msgLogs.Last());
                                        }
                                    }
                                }
                                catch (Exception ex)
                                {
                                    msgLogs.Add(string.Format("[ERRO] Contrato {0} Tipo {1} - {2}", contrato.ContractId, contrato.GenerationType, string.Concat(ex.Message, ex.StackTrace, ex.InnerException != null ? ex.InnerException.Message : string.Empty)));
                                    Console.WriteLine(msgLogs.Last());
                                    continue;
                                }

                                lstUpdate.Clear();

                                #endregion
                            }


                            saveRecordType = SalesforceDAO.InserirObjetosSF(lstObjetos, ref binding);
                            resultado = saveRecordType.First();
                            if (resultado != null && resultado.errors != null)
                            {
                                foreach (Error err in resultado.errors)
                                {
                                    msgLogs.Add(string.Format("[ERRO] {0}", err.message));
                                    Console.WriteLine(msgLogs.Last());
                                }
                            }
                            else
                            {
                                msgLogs.Add(string.Format("{0} [GD NOVO]\t{1}\tCNT {2}\tPODs {3}", DateTime.Now.ToLongTimeString(), string.Join(", ", saveRecordType.Select(s => s.id).ToArray()), contrato.ContractId, string.Join(", ", contrato.Beneficiarios.Select(c => c.Key).ToArray())));
                                Console.WriteLine(msgLogs.Last());
                            }
                            lstObjetos.Clear();
                            #endregion
                        }
                        #endregion

                        lstClienteGerador.Clear();
                    }

                    msgLogs.Add(string.Format("Processo finalizado."));
                    Console.WriteLine(msgLogs.Last());
                    IO.EscreverArquivo(arqSaida, string.Join(Environment.NewLine, msgLogs));

                }  //fim arquivoSaida

                Console.ReadKey();
            }
            catch (Exception ex)
            {
                Console.WriteLine("\nErro ao atualizar o registro: \n" + ex.Message + ex.StackTrace);
                Console.ReadKey();
            }
        }



        /// <summary>
        /// Corrige os External Ids de Accounts que estejam com Company divergentes.   
        /// Para Ids não encontrados, identifica-os para futuro Delta.
        /// </summary>
        /// <remarks>Processo para ser rodado pontualmente sobre alguns ExternalId's retirados de rechazos de produção.</remarks>
        /// <chamada>29 prod 2003 C:\temp\externalid.txt</chamada>
        /// <param name="empresa"></param>
        /// <param name="caminhoArquivo"></param>
        internal void AjustarExternalIdContas(string empresa, string caminhoArquivo)
        {
            autenticar();

            int i = 0;
            int contCliente = 0;
            sObject sfObj = null;
            List<sObject> lstObjetos = new List<sObject>();
            List<sObject> lstUpdate = new List<sObject>();
            List<string> msgLogs = new List<string>();
            StringBuilder idOrder = new StringBuilder();

            try
            {
                Arquivo arquivo = new Arquivo(caminhoArquivo, '|');
                List<string> listaArq = ArquivoLoader.GetAccountExternalIds(arquivo);
                
                msgLogs.Add(string.Format("{0}[ARQUIVO]\t{1}", DateTime.Now.ToLongTimeString(), arquivo.CaminhoCompleto));
                Console.WriteLine(msgLogs.Last());

                using (Arquivo arqSaida = new Arquivo(arquivo.Caminho, string.Concat(arquivo.Nome, "_SAIDA"), arquivo.Extensao))
                {
                    SaveResult[] saveRecordType = null;
                    SaveResult resultado = null;

                    foreach (string externalId in listaArq)
                    {
                        if (externalId.ToLower().Contains("invalido") || externalId.ToLower().Contains("cnpj"))
                        {
                            msgLogs.Add(string.Format("{0} [ACCOUNT]\t Remigrar {1}", DateTime.Now.ToLongTimeString(), externalId));
                            Console.WriteLine(msgLogs.Last());
                            continue;
                        }

                        string auxExternalId = externalId.Replace("2003", "2005");

                        if (msgLogs.Count > 0)
                            IO.EscreverArquivo(arqSaida, string.Join(Environment.NewLine, msgLogs));

                        msgLogs.Clear();
                        idOrder.Clear();
                        contCliente++;
                        i++;

                        List<AccountSalesforce> lstAccounts = null;
                        try
                        {
                            lstAccounts = SalesforceDAO.GetContasPorExternalId(auxExternalId, ref binding);
                        }
                        catch (System.Net.WebException ex)
                        {
                            msgLogs.Add(string.Format("{0} [ERRO]\tFalha de conexão: {1}{2}{3}{4}"
                                , DateTime.Now.ToLongTimeString()
                                , externalId
                                , ex.GetType().ToString()
                                , ex.Message
                                , ex.StackTrace));
                            Console.WriteLine(msgLogs.Last());
                            continue;
                        }
                        catch (Exception ex)
                        {
                            msgLogs.Add(string.Format("{0} [ERRO]\tConsulta GetContasPorExternalId - External Id: {1}{2}{3}{4}"
                                , DateTime.Now.ToLongTimeString()
                                , auxExternalId
                                , ex.GetType().ToString()
                                , ex.Message
                                , ex.StackTrace));
                            Console.WriteLine(msgLogs.Last());
                            continue;
                        }

                        if (lstAccounts == null || lstAccounts.Count == 0)
                        {
                            //TODO: gravar em log
                            msgLogs.Add(string.Format("{0} [ACCOUNT]\t Remigrar {1}", DateTime.Now.ToLongTimeString(), externalId));
                            Console.WriteLine(msgLogs.Last());
                            continue;
                        }

                        msgLogs.Add(string.Format("{0} [CLIENTE] {1}", DateTime.Now.ToLongTimeString(), lstAccounts.First().NumeroCliente));
                        Console.WriteLine(msgLogs.Last().Trim());


                        #region Atualizar Account -------------------------------------------

                        sfObj = new sObject();
                        sfObj.type = "Account";
                        sfObj.Id = lstAccounts.First().Id;

                        sfObj.Any = new System.Xml.XmlElement[] {
                                    SFDCSchemeBuild.GetNewXmlElement("ExternalId__c", externalId)
                                };
                        lstUpdate.Add(sfObj);

                        try
                        {
                            saveRecordType = binding.update(lstUpdate.ToArray());
                            resultado = saveRecordType.First();
                            if (resultado != null && resultado.errors != null)
                            {
                                foreach (Error err in resultado.errors)
                                {
                                    msgLogs.Add(string.Format("[ERRO] {0}", err.message));
                                    Console.WriteLine(msgLogs.Last());
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            msgLogs.Add(string.Format("[ERRO] Account {0} ExternalId {1} - {2}", lstAccounts.FirstOrDefault().Id, auxExternalId, string.Concat(ex.Message, ex.StackTrace, ex.InnerException != null ? ex.InnerException.Message : string.Empty)));
                            Console.WriteLine(msgLogs.Last());
                            continue;
                        }

                        lstUpdate.Clear();

                        #endregion
                    }

                    msgLogs.Add(string.Format("Processo finalizado."));
                    Console.WriteLine(msgLogs.Last());
                    IO.EscreverArquivo(arqSaida, string.Join(Environment.NewLine, msgLogs));

                }  //fim arquivoSaida

                Console.ReadKey();
            }
            catch (Exception ex)
            {
                Console.WriteLine("\nErro ao atualizar o registro: \n" + ex.Message + ex.StackTrace);
                Console.ReadKey();
            }

        }



        public void AtualizarFaturas(Arquivo arquivoSaida, List<Invoice> lstFaturas)
        {
            autenticar();
            sObject sfObj = null;
            List<sObject> lstUpdate = new List<sObject>();
            List<string> msgLogs = new List<string>();

            try
            {
                this.log.LogFull(string.Format("{0} [ARQUIVO]\t{1}", DateTime.Now.ToLongTimeString(), arquivoSaida.CaminhoCompleto));
                
                msgLogs.Clear();
                UpsertResult[] sfResult = null;

                foreach (Invoice invoice in lstFaturas)
                {
                    sfObj = new sObject();
                    sfObj.type = "Invoice__c";
                    sfObj.Id = invoice.Rut__c;
                        
                    sfObj.Any = new System.Xml.XmlElement[] {
                        SFDCSchemeBuild.GetNewXmlElement("Rut__c", invoice.Rut__c),
                        SFDCSchemeBuild.GetNewXmlElement("Status__c", invoice.Status__c)
                    };
                    this.log.LogFull((string.Format("{0} {1}", DateTime.Now.ToLongTimeString(), invoice.ToString())));

                    lstUpdate.Add(sfObj);
                }

                try
                {
                    sfResult = SalesforceDAO.Upsert("Rut__c", lstUpdate, ref binding);
                    List<Error[]> erros = sfResult.Where(sr => sr.errors != null).Select(s => s.errors).ToList();
                    foreach (Error[] err in erros)
                    {
                        this.log.LogFull((string.Format("{0} [ERRO] {1}", DateTime.Now.ToLongTimeString(), string.Join(", ", err.Select(e => e.message)))));
                    }
                    lstUpdate.Clear();
                }
                catch (Exception ex)
                {
                    this.log.LogFull((string.Format("{0} [ERRO UPSERT] {1}{2}{3}"
                        , DateTime.Now.ToLongTimeString()
                        , ex.GetType().ToString()
                        , ex.Message
                        , ex.StackTrace)));
                }
            }
            catch (Exception ex)
            {
                this.log.LogFull((string.Format("{0} [ERRO DESCONHECIDO] {1}{2}"
                    , DateTime.Now.ToLongTimeString()
                    , ex.Message
                    , ex.StackTrace)));
            }

        }



        //public string AtualizarExternalIdPorDocumento(string documento)
        //{
        //    autenticar();
        //    sObject sfObj = null;
        //    List<sObject> lstUpdate = new List<sObject>();
        //    List<string> msgLogs = new List<string>();

        //    List<ClienteSalesforce> clis = SalesforceDAO.GetContasPorDocumento(this.codigoEmpresa, documento, ref this.binding);
            
        //    if(clis == null || clis.Count == 0)
        //        return string.Format("Nenhum PoD encontrado com o documento {0}", documento);

        //    ExtracaoSalesForce dao = new ExtracaoSalesForce(TipoCliente.GB, this.codigoEmpresa);
        //    List<SalesGeral> sg = dao.GetSalesgeralByDocumento(documento);

        //    return "ok";

        //    //try
        //    //{
        //    //    UpsertResult[] sfResult = null;


        //    //        sfObj = new sObject();
        //    //        sfObj.type = "Invoice__c";
        //    //        sfObj.Id = invoice.Rut__c;
                        
        //    //        sfObj.Any = new System.Xml.XmlElement[] {
        //    //            SFDCSchemeBuild.GetNewXmlElement("Rut__c", invoice.Rut__c),
        //    //            SFDCSchemeBuild.GetNewXmlElement("Status__c", invoice.Status__c)
        //    //        };
        //    //        this.log.LogFull((string.Format("{0} {1}", DateTime.Now.ToLongTimeString(), invoice.ToString())));

        //    //        lstUpdate.Add(sfObj);
        //    //    }

        //    //    try
        //    //    {
        //    //        sfResult = SalesforceDAO.Upsert("Rut__c", lstUpdate, ref binding);
        //    //        List<Error[]> erros = sfResult.Where(sr => sr.errors != null).Select(s => s.errors).ToList();
        //    //        foreach (Error[] err in erros)
        //    //        {
        //    //            this.log.LogFull((string.Format("{0} [ERRO] {1}", DateTime.Now.ToLongTimeString(), string.Join(", ", err.Select(e => e.message)))));
        //    //        }
        //    //        lstUpdate.Clear();
        //    //    }
        //    //    catch (Exception ex)
        //    //    {
        //    //        this.log.LogFull((string.Format("{0} [ERRO UPSERT] {1}{2}{3}"
        //    //            , DateTime.Now.ToLongTimeString()
        //    //            , ex.GetType().ToString()
        //    //            , ex.Message
        //    //            , ex.StackTrace)));
        //    //    }
        //    //}
        //    //catch (Exception ex)
        //    //{
        //    //    this.log.LogFull((string.Format("{0} [ERRO DESCONHECIDO] {1}{2}"
        //    //        , DateTime.Now.ToLongTimeString()
        //    //        , ex.Message
        //    //        , ex.StackTrace)));
        //    //}

        //}
        

        
        /// <summary>
        /// Identifica os PoD's que possuem o campo CompanyID__c vazios e preenche conforme o parâmetro passado.
        /// </summary>
        /// <remarks>Processo idealizado para contornar um problema temporário durante os testes em UAT do GoLive GB em FOR.</remarks>
        /// <param name="codigoEmpresa">Código que será atualizado nos registros encontrados.</param>
        [ComVisible(true)]
        public void NormalizarEmpresa(string codigoEmpresa)
        {
            if (!loggedIn)
            {
                Autenticacao auth = new Autenticacao(this.ambiente);
                if (!auth.ValidarLogin(ref this.binding))
                    return;

                this.loggedIn = true;
            }

            int i = 0;
            int totalAtualizado = 0;
            int contCliente = 0;
            sObject update = null;

            //lista de objetos Salesforce a ser enviada ao ws para atualização
            List<sObject> listaUpdate = new List<sObject>();

            try
            {
                StringBuilder msgLog = new StringBuilder();
                msgLog.Append(string.Format("{0}\tConsultando PoDs para '{1}'...", DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss"), codigoEmpresa));
                Console.WriteLine(msgLog.ToString());

                List<ClienteSalesforce> lstPoDs = SalesforceDAO.GetPoDsSemEmpresa(codigoEmpresa, ref binding);

                StringBuilder auxRegion = new StringBuilder();

                msgLog.Clear();
                msgLog.Append(string.Format("{0}\t{1} PoDs localizados", DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss"), lstPoDs.Count()));
                Console.WriteLine(msgLog.ToString());

                foreach (ClienteSalesforce pod in lstPoDs)
                {
                    msgLog.Clear();
                    contCliente++;
                    auxRegion.Clear();

                    //tipo de objeto do SalesForce
                    update = new sObject();
                    update.type = "PointofDelivery__c";
                    update.Id = pod.Id;

                    update.Any = new System.Xml.XmlElement[] {
                        SFDCSchemeBuild.GetNewXmlElement("CompanyId__c", codigoEmpresa)
                    };

                    listaUpdate.Add(update);
                    i++;
                    totalAtualizado++;

                    msgLog.Clear();
                    msgLog.Append(string.Format("{0}\tPoD {1} atualizado", DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss"), pod.Id));
                    Console.WriteLine(msgLog.ToString());

                    SaveResult[] saveResults = binding.update(listaUpdate.ToArray());
                    if (saveResults != null && saveResults.Count() > 0 && saveResults[0].errors != null)
                    {
                        string idsUpdate = string.Join(",", listaUpdate.Select(t => t.Id).ToArray());
                        foreach (Error err in saveResults[0].errors)
                        {
                            Console.WriteLine(string.Format("{0}\t[ERRO] {1} {2}", DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss"), idsUpdate, err.message));
                        }
                    }

                    i = 0;
                    listaUpdate.Clear();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(string.Concat("\nErro ao atualizar o registro: \n", ex.Message, ex.StackTrace));
            }

            Console.WriteLine(string.Format("{0}\tProcesso terminado.\n", DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss")));
        }


        /// <summary>
        /// Identificar casos de endereços com caracteres inválidos nos campos Numero, CEP e complemento.
        /// <remarks>A saída é um arquivo para cada campo a ser atualizado no SF e um outro arquivo com exceções nao tratatas.</remarks>
        /// </summary>
        public void AtualizarAddress()
        {
            #region Consulta-base

            //select id, CNT_fPointofDelivery_Number__c, Name
            //    , DetailAddress__c,  DetailAddress__r.Number__c, DetailAddress__r.Corner__c, DetailAddress__r.Postal_Code__c 
            //from PointofDelivery__c 
            //where CompanyID__c in ('2003','COELCE')
            //    and SegmentType__c = 'A'
            //    --and Name in ('54093','30591','581972','581134','579809','430072','912837','38272439','9162854','54093','30591','581972','581134','579809','430072','912837','38272439','9162854')

            #endregion

            List<Arquivo> lstarq = new List<Arquivo>();
            Arquivo arq1 = new Arquivo(@"C:\Users\adm\Downloads\190924 CE GB 01.csv", ',', true);
            Arquivo arq2 = new Arquivo(@"C:\Users\adm\Downloads\190924 CE GB 02.csv", ',', true);

            lstarq.Add(arq1);
            lstarq.Add(arq2);

            int contLinhas = 0;

            using (Negocio exec = new Negocio("prod", new SforceService(), "2003"))
            {
                foreach (Arquivo arqEntrada in lstarq)
                {
                    Arquivo loaderNumero = new Arquivo(string.Concat(arqEntrada.Caminho, "\\", arqEntrada.Nome, "_STREET_NUM", DateTime.Now.ToString("yyyyMMddHHmmss"), ".txt"));
                    Arquivo loaderCep = new Arquivo(string.Concat(arqEntrada.Caminho, "\\", arqEntrada.Nome, "_CEP", DateTime.Now.ToString("yyyyMMddHHmmss"), ".txt"));
                    Arquivo loaderOutros = new Arquivo(string.Concat(arqEntrada.Caminho, "\\", arqEntrada.Nome, "_OUTROS", DateTime.Now.ToString("yyyyMMddHHmmss"), ".txt"));
                    Arquivo loaderComplementos = new Arquivo(string.Concat(arqEntrada.Caminho, "\\", arqEntrada.Nome, "CORNER", DateTime.Now.ToString("yyyyMMddHHmmss"), ".txt"));

                    Log logNum = new Log(loaderNumero.CaminhoCompleto);
                    Log logCep = new Log(loaderCep.CaminhoCompleto);
                    Log logComplementos = new Log(loaderComplementos.CaminhoCompleto);
                    Log logOutros = new Log(loaderOutros.CaminhoCompleto);

                    StreamReader sr = new StreamReader(arqEntrada.CaminhoCompleto, Encoding.GetEncoding("iso-8859-1"));
                    StringBuilder _aux2 = new StringBuilder();

                    var linha = string.Empty;
                    if (arqEntrada.TemCabecalho)
                        linha = sr.ReadLine();

                    while (true)
                    {
                        try
                        {
                            contLinhas++;
                            linha = sr.ReadLine();

                            if (string.IsNullOrEmpty(linha)) break;

                            string[] _aux = linha.Split(new char[] { arqEntrada.Separador });


                            #region Campo Número
                            if (!Regex.IsMatch(_aux[4].Replace("\"", ""), @"^[0-9]"))
                            {
                                if (_aux[4].Split(new char[] { ' ' }).Count() > 1)
                                {
                                    logOutros.LogFull(string.Concat(_aux[3], "\t", _aux[2], "\t", _aux[4]));
                                }
                                else
                                {
                                    string regExp = Regex.Replace(_aux[4], @"[^0-9]", string.Empty);
                                    if (string.IsNullOrWhiteSpace(regExp))
                                    {
                                        regExp = "0";
                                    }

                                    logNum.LogFull(string.Concat(_aux[3], "\t", _aux[2], "\t", _aux[4], "\t", "\"", regExp, "\""));
                                }
                            }
                            #endregion


                            #region Campo Complemento
                            if (string.IsNullOrWhiteSpace(_aux[5].Replace("\"", "")))
                            {
                                logComplementos.LogFull(string.Concat(_aux[3], "\t", _aux[2], "\t", _aux[5], "\t", "\"s/n\""));
                            }
                            #endregion


                            #region Campo CEP
                            if (Regex.IsMatch(_aux[6].Replace("\"", ""), @"[^0-9]"))
                            {
                                string regExp2 = Regex.Replace(_aux[6], @"[^0-9]", string.Empty);
                                if (string.IsNullOrWhiteSpace(regExp2))
                                {
                                    regExp2 = "00000000";
                                }
                                regExp2 = Regex.Replace(_aux[6], @"[^\w\d]", string.Empty);

                                logCep.LogFull(string.Concat(_aux[3], "\t", _aux[2], "\t", _aux[6], "\t", "\"", regExp2, "\""));
                            }
                            #endregion

                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(string.Format("ERRO inexperado: {0}", ex.Message));
                        }

                    }
                }
            }
        }



        /// <summary>
        /// Identifica, a partir de uma lista de Accounts, os que não possuem um Contract Line Item associado.   
        /// Problema originado em Julho/2020 no Delta CE.
        /// </summary>
        /// <remarks>2020-07-16</remarks>
        /// <param name="arquivoEntrada"></param>
        /// <param name="temCabecalho"></param>
        internal void ExcluirAssetsDuplicadosPorId(string arquivoEntrada, bool temCabecalho)
        {
            Arquivo arq = new Arquivo(arquivoEntrada, ',', temCabecalho);
            List<AssetDTO> lstAssets = ArquivoLoader.GetAssetIds(arq);
            List<string> lstExclusao = new List<string>();

            autenticar();
            foreach(AssetDTO asset in lstAssets)
            {
                ContractLineItemSalesforce cl = SalesforceDAO.GetContractLineByAssetsId(asset.Id, ref this.binding);

                if (cl == null || string.IsNullOrWhiteSpace(cl.ContractId))
                {
                    if (!lstExclusao.Contains(asset.Id))
                        lstExclusao.Add(asset.Id);
                }
            }

            StringBuilder sb = new StringBuilder();
            foreach(string assetId in lstExclusao)
            {
                if (sb.Length > 0)
                    sb.Append(",");

                sb.AppendFormat("'{0}'", assetId);
            }
            Console.WriteLine(sb.ToString());
            Console.Read();
        }

    }
}

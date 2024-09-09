using IBM.Data.Informix;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Solus.Controle;
using System.Data;
using System.Configuration;
using System.IO;
//using Synapsis.FtpLib;
//using ExtracaoSalesForce.Modelo.SalesForce;
//using Chilkat;
using System.Diagnostics;
using SalesforceExtractor.Utils;
using SalesforceExtractor.Entidades;
using SalesforceExtractor.Dados.SalesForce;
using SalesforceExtractor.Entidades.Modif;
using SalesforceExtractor.Entidades.Enumeracoes;
using System.Threading;
using SalesforceExtractor.Dados;

namespace Solus.Controle.SalesForce
{
    public class ConsultarSynergia
    {
        TipoCliente tipoCliente;

        public ConsultarSynergia(string Empresa, TipoCliente tipoCliente)
        {
            this.tipoCliente = tipoCliente;

            if (Empresa.Trim().ToUpper().Equals("AMPLA") || Empresa.Trim().ToUpper().Equals("2005"))
            {
                if(tipoCliente == TipoCliente.GB)
                    connectionString = ConfigurationSettings.AppSettings["ConnectionStringAmplaPro"];

                if (tipoCliente == TipoCliente.GA)
                    connectionString = ConfigurationSettings.AppSettings["ConnectionStringAmplaProGA"];
            }
            else if (Empresa.Trim().ToUpper().Equals("COELCE") || Empresa.Trim().ToUpper().Equals("2003"))
            {
                if (tipoCliente == TipoCliente.GA)
                    connectionString = ConfigurationSettings.AppSettings["ConnectionStringCoelceProGA"];

                if (tipoCliente == TipoCliente.GB)
                    connectionString = ConfigurationSettings.AppSettings["ConnectionStringCoelcePro"];
            }

            extracaoSalesForce = new ExtracaoSalesForce(tipoCliente, Empresa);
            synergiaDAOPro = new SynergiaDAO(connectionString);
            empresa = Empresa.Trim().ToUpper();
        }

        static string empresa = string.Empty;
        static string connectionString = string.Empty;

        SynergiaDAO synergiaDAOPro = null;

        IfxTransaction transacao = null;

        ExtracaoSalesForce extracaoSalesForce;

        private static Dictionary<string, int> dicRelatorioModif = new Dictionary<string, int>();

        string[] dadosProcessar = null;


        /// <summary>
        /// Romulo Silva - 22/03/2017
        /// Responsável por consultar bd Synergia e trazer os clientes que sofreram alguma alteração cadastral.
        /// Após subir para memória, é criada tabela temporária com essa lista de clientes
        /// </summary>
        internal List<ModifBase> criarTabTempClientesModificados(int qtdeDiasAnteriores)
        {            
            try
            {
                transacao = synergiaDAOPro.AbrirTransacao();

                //RelatorioFinalSF relatorioFinalSF = new RelatorioFinalSF();
                //relatorioFinalSF.DataInicio = DateTime.Now;

                if (!"TRUE".Equals(ConfigurationManager.AppSettings.Get("arquivo")))
                {
                    //IniciarSolus.logGeral.EscreverLog("Consultando clientes com alterações cadastrais no Synergia e criando tabela temporária");

                    synergiaDAOPro.criarTabTempClientesModificados(qtdeDiasAnteriores, transacao);

                    //IniciarSolus.logGeral.EscreverLog("Tabela temporária criada com sucesso!");
                    Console.WriteLine(string.Format("{0}\tTabela temporária criada.", DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss")));

                    //IniciarSolus.logGeral.EscreverLog("Consultando clientes grupo B modificados no modelo Sales Force...");
                    dadosProcessar = synergiaDAOPro.consultarClientesSynergiaB(transacao);

                    //Levanta dados da modif temporária para fazer relatório...
                    dicRelatorioModif = synergiaDAOPro.consultaRelatorioModif(transacao);
                    Console.WriteLine(string.Format("{0}\tDados da MODIF identificados.", DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss")));

                    //int tamanhoGrupoB = dadosProcessar.Length;
                    //IniciarSolus.logGeral.EscreverLog("Encontrados " + tamanhoGrupoB + " registros Grupo B para processar!");

                    //IniciarSolus.logGeral.EscreverLog("Consultando clientes grupo A no modelo Sales Force...");

                    //dadosProcessar = new string[1];
                    dadosProcessar = dadosProcessar.Concat(synergiaDAOPro.consultarClientesSynergiaA(qtdeDiasAnteriores, transacao)).ToArray();
                    //IniciarSolus.logGeral.EscreverLog("Encontrados " + (dadosProcessar.Length - tamanhoGrupoB) + " registros Grupo A para processar!");

                    //IniciarSolus.logGeral.EscreverLog("Total registros a processar: " + dadosProcessar.Length);
                    if(transacao != null && transacao.Connection.State == ConnectionState.Open)
                        transacao.Commit();
                    synergiaDAOPro.FecharConexao();
                }
                else
                {
                    string localArquivoDados = ConfigurationManager.AppSettings.Get("localArquivoDados");
                    StreamReader sr = new StreamReader(localArquivoDados, Encoding.GetEncoding("ISO-8859-1"));
                    List<string> myCollection = new List<string>();
                    
                    while (!sr.EndOfStream)
                    {
                        string linha = sr.ReadLine();
                        myCollection.Add(linha);
                        
                    }

                    dadosProcessar = myCollection.ToArray();
                }

                //IniciarSolus.logGeral.EscreverLog("Iniciando processamento...");
                return enviarCargaProcessamento();

                //ValidaExtracao valida = new ValidaExtracao();
                //valida.iniciarValidacao();

                //relatorioFinalSF.DataFim = DateTime.Now;
                //relatorioFinalSF.TempoExecucao = relatorioFinalSF.DataFim.Subtract(relatorioFinalSF.DataInicio).Duration();
                //relatorioFinalSF.qtdeContaDuplicada = 0;
                
                //IniciarSolus.logGeral.EscreverLog("Disparando e-mail com log...");
                //EnviarEmail enviarEmail = new EnviarEmail();
                //enviarEmail.enviarEmailSalesForce(dicRelatorioModif, relatorioFinalSF, empresa);
            }
            catch (Exception ex)
            {
                if (transacao != null && transacao.Connection != null &&
                    transacao.Connection.State != (ConnectionState.Broken | ConnectionState.Broken))
                    transacao.Rollback();

                Console.Write("ERRO " + ex.Message + ex.StackTrace);
                //Console.ReadKey();
                //Debugger.Break();
                //IniciarSolus.logGeral.EscreverLog("Erro ao consultar cliente com alterações cadastrais: " + ex.ToString());
            }
            finally
            {
                transacao.Dispose();
            }

            return enviarCargaProcessamento();
        }

        internal List<ModifBase> criarTabTempClientesModificados(string data_inicial, string data_final)
        {
            try
            {
                transacao = synergiaDAOPro.AbrirTransacao();

                //RelatorioFinalSF relatorioFinalSF = new RelatorioFinalSF();
                //relatorioFinalSF.DataInicio = DateTime.Now;

                if (!"TRUE".Equals(ConfigurationManager.AppSettings.Get("arquivo")))
                {
                    //IniciarSolus.logGeral.EscreverLog("Consultando clientes com alterações cadastrais no Synergia e criando tabela temporária");

                    synergiaDAOPro.criarTabTempClientesModificados(data_inicial, data_final, transacao);

                    //IniciarSolus.logGeral.EscreverLog("Tabela temporária criada com sucesso!");

                    //IniciarSolus.logGeral.EscreverLog("Consultando clientes grupo B modificados no modelo Sales Force...");
                    dadosProcessar = synergiaDAOPro.consultarClientesSynergiaB(transacao);

                    //Levanta dados da modif temporária para fazer relatório...
                    dicRelatorioModif = synergiaDAOPro.consultaRelatorioModif(transacao);

                    int tamanhoGrupoB = dadosProcessar.Length;
                    //IniciarSolus.logGeral.EscreverLog("Encontrados " + tamanhoGrupoB + " registros Grupo B para processar!");

                    //IniciarSolus.logGeral.EscreverLog("Consultando clientes grupo A no modelo Sales Force...");

                    dadosProcessar = dadosProcessar.Concat(synergiaDAOPro.consultarClientesSynergiaA(data_inicial, data_final, transacao)).ToArray();
                    //IniciarSolus.logGeral.EscreverLog("Encontrados " + (dadosProcessar.Length - tamanhoGrupoB) + " registros Grupo A para processar!");

                    //IniciarSolus.logGeral.EscreverLog("Total registros a processar: " + dadosProcessar.Length);
                    transacao.Commit();
                    transacao.Dispose();
                    synergiaDAOPro.FecharConexao();
                }
                else
                {
                    string localArquivoDados = ConfigurationManager.AppSettings.Get("localArquivoDados");
                    StreamReader sr = new StreamReader(localArquivoDados, Encoding.GetEncoding("ISO-8859-1"));
                    List<string> myCollection = new List<string>();

                    while (!sr.EndOfStream)
                    {
                        string linha = sr.ReadLine();
                        myCollection.Add(linha);

                    }

                    dadosProcessar = myCollection.ToArray();
                }


                //IniciarSolus.logGeral.EscreverLog("Iniciando processamento...");

                /*ValidaExtracao valida = new ValidaExtracao();
                valida.iniciarValidacao();*/

                //relatorioFinalSF.DataFim = DateTime.Now;
                //relatorioFinalSF.TempoExecucao = relatorioFinalSF.DataFim.Subtract(relatorioFinalSF.DataInicio).Duration();
                //relatorioFinalSF.qtdeContaDuplicada = 0;

                //IniciarSolus.logGeral.EscreverLog("Preparando arquivos para FTP...");
                //TratarFtpArquivos();

                //IniciarSolus.logGeral.EscreverLog("Disparando e-mail com log...");
                //EnviarEmail enviarEmail = new EnviarEmail();
                //enviarEmail.enviarEmailSalesForce(dicRelatorioModif, relatorioFinalSF, empresa);
            }
            catch (Exception ex)
            {
                Debugger.Break();
                //IniciarSolus.logGeral.EscreverLog("Erro ao consultar cliente com alterações cadastrais: " + ex.ToString());
            }

            return enviarCargaProcessamento();
        }

        public List<ModifBase> enviarCargaProcessamento()
        {
            if (dadosProcessar.Length <= 0)
                return new List<ModifBase>();

            //IniciarSolus.logGeral.EscreverLog("Criando arquivo com os dados extraídos para backup futuro...");

            string nomeArquivo = DateTime.Now.ToString("yyyyMMdd").Replace("/", "-") + "_SalesForceXSynergia";

            //IniciarSolus.logGeral.EscreverLog("Arquivo criado em " + ConfigurationSettings.AppSettings["outputArquivo"] + nomeArquivo);
            
            Util.preencheDicionarios();
            
            //IniciarSolus.logGeral.EscreverLog("Criando entidades e gerando cabeçalhos");
            
            extracaoSalesForce.GravarCabecalho();
            extracaoSalesForce.AbrirConexaoDes();
            int contaLinha = 0;
            
            //Carrega toda a tabela ACCOUNT_SF, que contem os ExternalIds
            Console.WriteLine(string.Format("{0}\tCarregando External Ids de Contas em D:\\Salesforce\\Modif\\Account_Sf.txt ...", DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss")));
            //extracaoSalesForce.GetContas(empresa);

            Console.WriteLine(string.Format("{0}\tTotal de dados a processar: {1}", DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss"), dadosProcessar.Count()));
            Console.Write(string.Format("{0}\tPreparando dados.", DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss")));

            for (int i = 0; i < dadosProcessar.Length; i++)
            {
                var tt = dadosProcessar.Where(x => x.Contains("890205")).ToList();
                contaLinha++;
                if (i % 500 == 0)
                    Console.Write(".");
                try
                {
                    extracaoSalesForce.processaDadoInformix(dadosProcessar[i]);
                    //ParameterizedThreadStart t = new ParameterizedThreadStart(extracaoSalesForce.processaDadoInformix);
                    //t.Invoke(dadosProcessar[i]);
                }
                catch (Exception ex)
                {
                    //string cliente = dadosProcessar[i].Replace(";", "").Replace("\"", "").Replace("\\", "").Split('|').Select(r => r.Trim()).ToArray()[11];
                    //string erro = contaLinha + "|" + ex.Message;
                    
                    //Util.EscreverArquivo("logErro" + DateTime.Now.ToString("yyyyMMdd"), erro, ".txt");

                    Console.WriteLine("\nErro no processamento do arquivo (linha " + contaLinha + ") :: " + ex.Message + " - " + ex.StackTrace);

                    extracaoSalesForce.FecharConexaoDes();
                    extracaoSalesForce.AbrirConexaoDes();
                }

                if (contaLinha % Convert.ToInt64(ConfigurationManager.AppSettings.Get("CommitArquivo")) == 0)
                {
                    Console.WriteLine("\nForam processadas: " + contaLinha + " linhas");
                }
            }

            return extracaoSalesForce.PacoteSFModif;
        }

        public void TratarFtpArquivos()
        {
            string nomeArquivoNovo = string.Empty;

            //Pasta raiz
            DirectoryInfo dirArquivos = new DirectoryInfo(ConfigurationSettings.AppSettings["outputArquivo"]);
            DirectoryInfo dirArquivoDia;
            
            //Pasta que será criada para armazenar os arquivos diários
            //if (empresa.Equals("AMPLA"))
            //{
            //     //File.Delete(ConfigurationSettings.AppSettings["outputArquivo"] + @"\AMPLA\\" + DateTime.Now.ToString("yyyyMMdd"));
            //     dirArquivoDia = new DirectoryInfo(ConfigurationSettings.AppSettings["outputArquivo"] + @"\AMPLA\\ATUALIZACAO\\" + DateTime.Now.ToString("yyyyMMdd"));
            //}
            //else
            //{
            //     //File.Delete(ConfigurationSettings.AppSettings["outputArquivo"] + @"\COELCE\\" + DateTime.Now.ToString("yyyyMMdd"));
            //    dirArquivoDia = new DirectoryInfo(ConfigurationSettings.AppSettings["outputArquivo"] + @"\COELCE\\ATUALIZACAO\\" + DateTime.Now.ToString("yyyyMMdd"));
            //}

            dirArquivoDia = new DirectoryInfo(ConfigurationSettings.AppSettings["outputArquivo"] + @"\ATUALIZACAO\\" + DateTime.Now.ToString("yyyyMMdd"));

            List<string> nomeArquivosAtuais = new List<string>();
            nomeArquivosAtuais.Add("ADDRESS.csv");
            nomeArquivosAtuais.Add("ASSET.csv");
            nomeArquivosAtuais.Add("BILLING.csv");
            nomeArquivosAtuais.Add("ACCOUNT.csv");
            nomeArquivosAtuais.Add("CONTACT.csv");
            nomeArquivosAtuais.Add("POINTOFDELIVERY.csv");
            nomeArquivosAtuais.Add("ACCOUNT.csv");
            nomeArquivosAtuais.Add("SERVICEPRODUCT.csv");
            nomeArquivosAtuais.Add("STREET.csv");
            nomeArquivosAtuais.Add(DateTime.Now.ToString("yyyyMMdd") + "ExtracaoLogErro.txt");
            nomeArquivosAtuais.Add("sql.txt");
            nomeArquivosAtuais.Add("ClientesGeral.txt");
            nomeArquivosAtuais.Add(DateTime.Now.ToShortDateString().Replace("/", "-") + "SalesForceXSynergia.txt");
            nomeArquivosAtuais.Add(DateTime.Now.ToShortDateString().Replace("/", "-") + "SalesForceXSynergia.log.txt");
            nomeArquivosAtuais.Add(DateTime.Now.Day + "_" +
                                          DateTime.Now.Month + "_" +
                                          DateTime.Now.Year + "_" +
                                          DateTime.Now.DayOfYear + "." +
                                          ConfigurationSettings.AppSettings["NomeArquivoLogLocal"] + ".log.txt");

            //Cria pasta para o dia...
            if (!dirArquivoDia.Exists)
            {
                dirArquivoDia.Create();
            }

            string sftpArquivos = string.Empty;

            //Percorre arquivos na raiz e renomeia
            foreach (FileInfo files in dirArquivos.GetFiles())
            {
                    try
                    {
                        if (files.FullName.EndsWith(".csv"))
                        {
                            nomeArquivoNovo = string.Empty;
                            nomeArquivoNovo = dirArquivoDia.FullName + "\\" + files.Name.ToString();
                           
                            //Renomeia
                            File.Delete(nomeArquivoNovo);
                            File.Move(files.FullName, nomeArquivoNovo);                       
                            //transferirFtp(nomeArquivoNovo, files.Length);
                        }
                    }
                    catch (Exception ex)
                    {
                        int contador = 0;
                        //IniciarSolus.logGeral.EscreverLog("Falha ao mover arquivo local " + files.FullName + ": " + ex.ToString());

                        while (contador <= 10)
                        {
                            try
                            {
                                //transferirFtp(nomeArquivoNovo, files.Length);
                                break;
                            }
                            catch
                            {
                                contador++;
                            }
                        }
                    }            
            }
            // transferirSftp(sftpArquivos.Remove(sftpArquivos.Length - 1).Split('|'));
        }


        public DebitoAutomatico ObterDadosBancariosDebitoAutomatico(string numeroCliente)
        {
            return synergiaDAOPro.ObterDadosBancariosDebitoAutomatico(numeroCliente);
        }


        public DataTable ConsultarAtendimentosComercial(DateTime data)
        {
            return synergiaDAOPro.ConsultarAtendimentosComercial(data);
        }


        public List<string> ConsultarClientesNovasLigacoes(DateTime dataInicio, DateTime dataFim, bool comNumeroCaso, ref Arquivo arq)
        {
            return synergiaDAOPro.ConsultarClientesNovasLigacoes(dataInicio, dataFim, comNumeroCaso);
        }


        //[Obsolete]
        //private bool transferirFtp(string nomeArquivoNovo, long tamanhoArquivo)
        //{
        //    bool retorno = false;
        //    long tamanhoArquivoUpload = 0;
        //    string pastaFtp = string.Empty;

        //    if (empresa.Trim().Equals("AMPLA"))
        //        pastaFtp = ConfigurationSettings.AppSettings["PastaFtpAmp"];
        //    else
        //        pastaFtp = ConfigurationSettings.AppSettings["PastaFtpCoe"];
            
        //    FTP ftp = new FTP();

        //    while (!ftp.IsConnected)
        //    {
        //        ftp = ConectaFTP(ftp);
        //        IniciarSolus.logGeral.EscreverLog("FTP Conectado!");
        //        break;
        //    }

        //    ftp.OpenUpload(nomeArquivoNovo, pastaFtp + "/" + nomeArquivoNovo.Replace('\\', '/').Split('/').Last());

        //    IniciarSolus.logGeral.EscreverLog("Iniciando upload do arquivo...");

        //    IniciarSolus.logGeral.EscreverLog("Progresso:");
            
        //    while (tamanhoArquivoUpload < tamanhoArquivo)
        //    {
        //        tamanhoArquivoUpload += ftp.DoUpload();
        //        IniciarSolus.logGeral.EscreverLog(tamanhoArquivoUpload + "KB de " + tamanhoArquivo + "KB concluído. (" + tamanhoArquivoUpload * 100 / tamanhoArquivo + "%)");
        //    }

        //    tamanhoArquivoUpload += ftp.DoUpload();
        //    IniciarSolus.logGeral.EscreverLog("Upload do arquivo " + nomeArquivoNovo + " concluído com sucesso!");
        //    retorno = true;

        //    ftp.Disconnect();

        //    return retorno;
        //}

        //[Obsolete]
        //private FTP ConectaFTP(FTP ftp)
        //{
        //    try
        //    {
        //        string FtpServer = ConfigurationSettings.AppSettings["IpServerFtp"];

        //        string FtpUsuario = ConfigurationSettings.AppSettings["UsuarioFtp"];
        //        string FtpSenha = ConfigurationSettings.AppSettings["SenhaFtp"];
        //        int FtpTimeOut = Convert.ToInt32(ConfigurationSettings.AppSettings["TimeOutFtp"]);
        //        int Tentativas = Convert.ToInt32(ConfigurationSettings.AppSettings["TentativasFtp"]);

        //        if (ftp.IsConnected)
        //        {
        //            return ftp;
        //        }
        //        else
        //        {
        //            for (int i = 1; i <= Tentativas; i++)
        //            {
        //                IniciarSolus.logGeral.EscreverLog("Conectado ao servidor " + FtpServer + " com o usuário " + FtpUsuario + ". Tentativa " + i + " de " + Tentativas);
        //                ftp = new FTP(FtpServer, FtpUsuario, FtpSenha);

        //                ftp.PassiveMode = false;
        //                ftp.timeout = FtpTimeOut;
        //                if (!ftp.IsConnected)
        //                {
        //                    ftp.Connect();
        //                }

        //                if (ftp.IsConnected)
        //                {
        //                    IniciarSolus.logGeral.EscreverLog("Conexão efetuada com sucesso!");
        //                    break;
        //                }
        //            }
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        IniciarSolus.logGeral.EscreverLog("Erro ao conectar-se com o servidor FTP: " + ex.ToString());
        //    }
        //    return ftp;
        //}

        //[Obsolete]
        //#region SFTP
        //private SFtp ConectaSftp(SFtp sftp)
        //{
        //    string host = ConfigurationSettings.AppSettings["IpServerSftp"];
        //    int port = Convert.ToInt32(ConfigurationSettings.AppSettings["portaSftp"]);
        //    string user = ConfigurationSettings.AppSettings["UsuarioSftp"];


        //    string localKey = System.Reflection.Assembly.GetEntryAssembly().Location;
        //    localKey = localKey.Substring(0, localKey.LastIndexOf("\\"));


        //    string privateKey = localKey + @"\Sftp\privado.ppk";


        //    //------------------------------------------//
        //    bool success = sftp.UnlockComponent("Anything for 30-day trial.");
        //    if (success != true)
        //    {
        //        Console.WriteLine(sftp.LastErrorText);
        //    }

        //    //  Load a .ppk PuTTY private key.
        //    Chilkat.SshKey puttyKey = new Chilkat.SshKey();
        //    string ppkText = puttyKey.LoadText(privateKey);


        //    puttyKey.Password = "salesforce123";

        //    success = puttyKey.FromPuttyPrivateKey(ppkText);
        //    if (success != true)
        //    {
        //        Console.WriteLine(puttyKey.LastErrorText);
        //    }

        //    //  Connect to an SSH/SFTP server
        //    success = sftp.Connect(host, port);
        //    if (success != true)
        //    {
        //        Console.WriteLine(sftp.LastErrorText);
        //    }

        //    //  Authenticate with the SSH server using a username + private key.
        //    //  (The private key serves as the password.  The username identifies
        //    //  the SSH user account on the server.)
        //    success = sftp.AuthenticatePk(user, puttyKey);
            
        //    if (success != true)
        //    {
        //        Console.WriteLine(sftp.LastErrorText);
        //    }

        //    Console.WriteLine("OK, conexão e autenticação com SSH server feita com sucesso!");

        //    //  After authenticating, the SFTP subsystem must be initialized:
        //    success = sftp.InitializeSftp();
            
        //    if (success != true)
        //    {
        //        Console.WriteLine(sftp.LastErrorText);
        //    }

        //    return sftp;
        //}

        //private void transferirSftp(string nomeArquivoNovo)
        //{
        //    bool success = false;
        //    string caminhoSftp = string.Empty;

        //    if (empresa.Trim().Equals("AMPLA"))
        //        caminhoSftp = ConfigurationSettings.AppSettings["CaminhoSftpAmp"];
        //    else
        //        caminhoSftp = ConfigurationSettings.AppSettings["CaminhoSftpCoe"];

        //    SFtp sftp = new SFtp();

        //    if (!sftp.IsConnected)
        //    {
        //        sftp = ConectaSftp(sftp);
        //    }

        //    success = sftp.UploadFileByName(caminhoSftp + nomeArquivoNovo.Split('\\').Last(), nomeArquivoNovo);
        //    if (success != true)
        //    {
        //        Console.WriteLine(sftp.LastErrorText);
        //        return;
        //    }

        //    sftp.Disconnect();
        //}
        //#endregion   


        internal List<ItemAttribute> ObterItemsAttribute(List<string> numeroClientes, int lote, Type tipoItem)
        {
            List<ItemAttribute> result = new List<ItemAttribute>();
            List<string> clausulaIn = new List<string>();
            int cont = 0;

            foreach(string cli in numeroClientes)
            {
                if(cont == 200)
                {
                    result.AddRange(synergiaDAOPro.ObterItemsAttribute(this.tipoCliente, lote, tipoItem, clausulaIn));
                    cont = 0;
                    clausulaIn.Clear();
                }
                clausulaIn.Add(cli);
                cont++;
            }
            result.AddRange(synergiaDAOPro.ObterItemsAttribute(this.tipoCliente, lote, tipoItem, clausulaIn));

            return result;
        }


        internal Dictionary<string,string> GetZonasPorCliente(Arquivo arqEntrada)
        {
            Log log = new Log(string.Concat(arqEntrada.Caminho, "\\", arqEntrada.Nome, DateTime.Now.ToString("_HHmmss"), "_LOG.", arqEntrada.Extensao));
            Console.WriteLine("Log: " + log.NomeArquivo);
            log.LogFull("Cliente, Zona, Classe, Classe_BR, Subclasse, SubClasse_BR");
            List<string> clientes = ArquivoLoader.GetTextosToList(arqEntrada);

            DataSet dtSyn = new DataSet();
            List<string> clausulaIn = new List<string>();
            int cont = 0;
            const string SQL = @"SELECT numero_cliente, ind_zona, classe, TARSYN.descripcion Classe_BR, subclasse, TARSAP.descripcion AS SubClasse_BR
                                    from cliente
                                    LEFT JOIN clientes:tabla TARSAP
                                        ON cliente.classe = LEFT(TARSAP.codigo,2) 
                                        AND cliente.subclasse = SUBSTR(TRIM(TARSAP.codigo),3,2)  
                                        AND cliente.tarifa =  SUBSTR(TRIM(TARSAP.codigo),5,2) 
                                        AND TARSAP.nomtabla='TARSAP' 
                                        AND TARSAP.sucursal='0000' 
                                    LEFT JOIN clientes:tabla TARSYN 
                                        ON TARSYN.codigo =TARSAP.codigo 
                                        AND TARSYN.nomtabla='TARSYN' 
                                        AND TARSYN.sucursal='0000' 
                                    LEFT JOIN susec su
                                        ON cliente.sucursal = su.sucursal
                                        and cliente.zona = su.zona
                                        and cliente.sector = su.sector
                                        and cliente.localidade = su.localidade
                                    WHERE numero_cliente in ({0});";
            
            StringBuilder sql = new StringBuilder(SQL);
            List<string> buffer = new List<string>();
            
            foreach (string cli in clientes)
            {
                if (cont == 500)
                {
                    using (DataTable dt = synergiaDAOPro.ExecutarConsultaSynergia(sql.ToString().Replace("{0}", string.Join(",", clausulaIn.ToArray())), null))
                    {
                        dtSyn.Tables.Add(dt);
                        flush(dt, ref buffer);
                    }

                    log.LogFull(buffer);
                    buffer.Clear();
                    cont = 0;
                    clausulaIn.Clear();
                }
                clausulaIn.Add(cli);
                cont++;
            }
 
            sql.Clear();
            sql.AppendFormat(SQL, string.Join(",", clausulaIn.ToArray()));
            using (DataTable dt = synergiaDAOPro.ExecutarConsultaSynergia(sql.ToString(), null))
            {
                dtSyn.Tables.Add(dt);
                flush(dt, ref buffer);
                log.LogFull(buffer);
            }

            Dictionary<string,string> result = new Dictionary<string,string>();
            StringBuilder d = new StringBuilder();
            foreach(DataTable dt in dtSyn.Tables)
            {
                foreach (DataRow dr in dt.Rows)
                {
                    if (DBNull.Value == dr[1])
                        continue;
                    result.Add(dr[0].ToString(), dr[1].ToString());
                }
            }
            return result;
        }



        /// <summary>
        /// Retorna dados de atendimento de emergência para relatório emergencial de Ouvidoria.
        /// 2020-08-25
        /// </summary>
        /// <param name="arqEntrada">Lista de protocolos</param>
        /// <returns></returns>
        internal Dictionary<string, string> GetAtendimentosEmergencia(Arquivo arqEntrada)
        {
            Log log = new Log(string.Concat(arqEntrada.Caminho, "\\", arqEntrada.Nome, DateTime.Now.ToString("_HHmmss"), "_LOG.", arqEntrada.Extensao));
            Console.WriteLine("Log: " + log.NomeArquivo);
            log.LogFull("Protocolo, Cliente, DataInicio, Hora Inicio, Data Fim, Hora Fim, Motivo Empresa, Observação");
            List<string> protocolos = ArquivoLoader.GetTextosToList(arqEntrada);

            DataSet dtSyn = new DataSet();
            List<string> clausulaIn = new List<string>();
            int cont = 0;
            const string SQL = @"select * from (
                                    select  mo_co_numero gac
                                            , co_numero_cliente
                                            , TO_CHAR(mo_fecha_inicio, '%d/%m/%Y') data_inicio
		                            , TO_CHAR(mo_fecha_inicio, '%H:%M') hora_inicio
                                            , TO_CHAR(mo_fecha_cerrado, '%d/%m/%Y') data_fim
		                            , TO_CHAR(mo_fecha_cerrado, '%H:%M') hora_fim
		                            , mo_cod_mot_empresa
		                            , ''--mo_cod_submotivo
                                            , (select trim(te_desc_mot_empres) from ct_tab_mot_empresa
                                                where te_cod_motivo = mo_cod_motivo
                                                    and te_cod_mot_empresa = mo_cod_mot_empresa) desc_mot_empresa
		                            , replace(replace(ob_descrip,chr(10),''),chr(13),'')
                                    , ob_pagina
                                    from ct_contacto co, ct_motivo m, outer ct_observ
                                    where mo_co_numero in ({0})
                                            and m.mo_co_numero = co_numero
		                            and mo_co_numero = ob_co_numero
                            UNION

                                    select  mf_co_numero gac
                                            , cf_numero_cliente
                                            , TO_CHAR(mf_fecha_inicio, '%d/%m/%Y') data_inicio
		                            , TO_CHAR(mf_fecha_inicio, '%H:%M') hora_inicio
                                            , TO_CHAR(mf_fecha_cerrado, '%d/%m/%Y') data_fim
		                            , TO_CHAR(mf_fecha_cerrado, '%H:%M') hora_fim
		                            , mf_cod_mot_empresa
		                            , ''--mf_cod_submotivo
                                            , (select trim(te_desc_mot_empres) from ct_tab_mot_empresa
                                                where te_cod_motivo = mf_cod_motivo
                                                    and te_cod_mot_empresa = mf_cod_mot_empresa) desc_mot_empresa
		                            , replace(replace(ob_descrip,chr(10),''),chr(13),'')
                                    , ob_pagina
                                    from ct_contacto_final cf, ct_motivo_final mf, outer ct_observ
                                    where mf_co_numero in ({0})
                                            and mf.mf_co_numero = cf_numero
		                            and mf_co_numero = ob_co_numero
                            )
                            order by 1,11;";

            StringBuilder sql = new StringBuilder(SQL);
            List<string> buffer = new List<string>();

            foreach (string gac in protocolos)
            {
                if (cont == 500)
                {
                    using (DataTable dt = synergiaDAOPro.ExecutarConsultaSynergia(sql.ToString().Replace("{0}", string.Join(",", clausulaIn.ToArray())), null))
                    {
                        
                        dtSyn.Tables.Add(dt);
                        flush(dt, ref buffer);
                    }

                    log.LogFull(buffer);
                    buffer.Clear();
                    cont = 0;
                    clausulaIn.Clear();
                }
                clausulaIn.Add(gac);
                cont++;
            }

            sql.Clear();
            sql.AppendFormat(SQL, string.Join(",", clausulaIn.ToArray()));
            using (DataTable dt = synergiaDAOPro.ExecutarConsultaSynergia(sql.ToString(), null))
            {
                dtSyn.Tables.Add(dt);
                flush(dt, ref buffer);
                log.LogFull(buffer);
            }

            Dictionary<string, string> result = new Dictionary<string, string>();
            StringBuilder d = new StringBuilder();

            List<AtendimentoEmergencia> lstAtend = new List<AtendimentoEmergencia>();
            AtendimentoEmergencia atend = new AtendimentoEmergencia();
            string protocoloTemp = string.Empty;
            string paginaTemp = string.Empty;
            foreach (DataTable dt in dtSyn.Tables)
            {
                foreach (DataRow dr in dt.Rows)
                {
                    if (protocoloTemp.Equals(dr[0].ToString()))
                    {
                        if (paginaTemp.Equals(dr[10].ToString()))
                            continue;

                        atend.Observacao = string.Concat(atend.Observacao, dr[9].ToString());
                        atend.Pagina = Int32.Parse(dr[10].ToString());

                        paginaTemp = atend.Pagina.ToString();
                        continue;
                    }
                    atend = new AtendimentoEmergencia();
                    atend.Protocolo = dr[0].ToString();
                    atend.NumeroCliente = dr[1].ToString();
                    atend.DataInicio = dr[2].ToString();
                    atend.HoraInicio = dr[3].ToString();
                    atend.MotivoEmpresa = dr[8].ToString();
                    atend.Observacao = dr[9].ToString();

                    lstAtend.Add(atend);
                    protocoloTemp = atend.Protocolo;
                }
            }

            foreach(AtendimentoEmergencia a in lstAtend)
            {
                log.LogFull(
                    string.Format("{0}\t{1}\t{2}\t{3}\t{4}\t{5}"
                    , a.Protocolo
                    , a.NumeroCliente
                    , a.DataInicio
                    , a.HoraInicio
                    , a.MotivoEmpresa
                    , a.Observacao
                    ));
            }

            return result;
        }


        /// <summary>
        /// Retorna e grava o Estado Cliente e Classe dos clientes solicitados.
        /// </summary>
        /// <param name="arqEntrada">Lista de clientes</param>
        /// <returns></returns>
        internal Dictionary<string, string> GetClassesSynergia(Arquivo arqEntrada)
        {
            Log log = new Log(string.Concat(arqEntrada.Caminho, "\\", arqEntrada.Nome, DateTime.Now.ToString("_HHmmss"), "_LOG.", arqEntrada.Extensao));
            Console.WriteLine("Log: " + log.NomeArquivo);
            log.LogFull("Cliente, Estado, Classe");
            List<string> protocolos = ArquivoLoader.GetTextosToList(arqEntrada);

            DataSet dtSyn = new DataSet();
            List<string> clausulaIn = new List<string>();
            int cont = 0;
            const string SQL = @"select  c.numero_cliente, c.estado_cliente
                                        , '' classe
                                    from cliente c
                                    where numero_cliente in ({0})";

            StringBuilder sql = new StringBuilder(SQL);
            List<string> buffer = new List<string>();

            foreach (string gac in protocolos)
            {
                if (cont == 500)
                {
                    using (DataTable dt = synergiaDAOPro.ExecutarConsultaSynergia(sql.ToString().Replace("{0}", string.Join(",", clausulaIn.ToArray())), null))
                    {

                        dtSyn.Tables.Add(dt);
                        //flush(dt, ref buffer);
                    }

                    log.LogFull(buffer);
                    buffer.Clear();
                    cont = 0;
                    clausulaIn.Clear();
                }
                clausulaIn.Add(gac);
                cont++;
            }

            sql.Clear();
            sql.AppendFormat(SQL, string.Join(",", clausulaIn.ToArray()));
            using (DataTable dt = synergiaDAOPro.ExecutarConsultaSynergia(sql.ToString(), null))
            {
                dtSyn.Tables.Add(dt);
                //flush(dt, ref buffer);
                log.LogFull(buffer);
            }

            Dictionary<string, string> result = new Dictionary<string, string>();
            StringBuilder d = new StringBuilder();

            List<AtendimentoEmergencia> lstAtend = new List<AtendimentoEmergencia>();
            AtendimentoEmergencia atend = new AtendimentoEmergencia();
            string protocoloTemp = string.Empty;
            string paginaTemp = string.Empty;
            foreach (DataTable dt in dtSyn.Tables)
            {
                foreach (DataRow dr in dt.Rows)
                {
                    atend = new AtendimentoEmergencia();
                    atend.NumeroCliente = dr[0].ToString();
                    atend.EstadoCliente = dr[1].ToString();
                    atend.ClasseCliente = dr[2].ToString();

                    lstAtend.Add(atend);
                    protocoloTemp = atend.Protocolo;
                }
            }

            foreach (AtendimentoEmergencia a in lstAtend)
            {
                log.LogFull(
                    string.Format("{0}\t{1}\t{2}"
                    , a.NumeroCliente
                    , a.EstadoCliente
                    , a.ClasseCliente
                    ));
            }

            return result;
        }


        /// <summary>
        /// Grava um arquivo contendo o menor aviso existente para cada cliente.
        /// </summary>
        /// <param name="arqEntrada"></param>
        /// <returns></returns>
        internal Dictionary<string, string> ExtrairMenorAviso(Arquivo arqEntrada)
        {
            Log log = new Log(string.Concat(arqEntrada.Caminho, "\\", arqEntrada.Nome, DateTime.Now.ToString("_HHmmss"), "_LOG.", arqEntrada.Extensao));
            Console.WriteLine("Log: " + log.NomeArquivo);
            log.LogFull("UC, DATA_SOLICITACAO_OUV, HoraInicio, AVISO, Cod_Interrup, Data_Solic, Hora_Solic, Data_Concl, Hora_Concl, teste, dif_dt");

            Dictionary<int, string> result = new Dictionary<int, string>();
            List<string> atendimentos = ArquivoLoader.GetTextosToList(arqEntrada);

            atendimentos = atendimentos.OrderBy(x => x).ToList();
            AtendimentoEmergencia obj = new AtendimentoEmergencia();
            int tempAviso = 0;
            string tempCliente = string.Empty;

            AtendimentoEmergencia atendTemp = new AtendimentoEmergencia();
            
            foreach (string atend in atendimentos)
            {
                string[] dr = atend.Split(',');

                long teste = 0;
                Int64.TryParse(dr[9].ToString(), out teste);

                int difData = 0;
                Int32.TryParse(dr[10].ToString(), out difData);

                if (teste > 0)
                    continue;

                obj = new AtendimentoEmergencia();
                obj.NumeroCliente = dr[0].ToString();
                obj.Aviso = Int32.Parse(dr[3].ToString());

                if (tempCliente.Equals(obj.NumeroCliente))
                {
                    if (tempAviso > obj.Aviso)
                    {
                        tempAviso = obj.Aviso;
                        atendTemp = obj;
                    }
                }
                else
                {
                    atendTemp = new AtendimentoEmergencia();
                    atendTemp = obj;
                    tempCliente = obj.NumeroCliente;
                    tempAviso = obj.Aviso;
                }

                if (difData < 0)
                    continue;

                if (result.ContainsKey(atendTemp.Aviso))
                    continue;
                result.Add(atendTemp.Aviso, atend);
            }

            StringBuilder d = new StringBuilder();

            
            string protocoloTemp = string.Empty;
            string paginaTemp = string.Empty;
            foreach (int a in result.Keys)
            {
                log.LogFull(
                    string.Format("{0}", result[a])
                    );
            }

            return new Dictionary<string, string>() ;
        }




        /// <summary>
        /// Com base nos arquivos de entrada, realiza o cruzamento do numero de cliente entre as bases do Synergia e da Técnica, para recuperar a interrupção correspondente ao atendimento da Ouvidoria, no Synergia.
        /// </summary>
        /// <param name="arqProtocolos">Lista da Ouvidoria, Nível 2.</param>
        /// <param name="arqTecnica">Lista com atendimentos/interrupções da Técnica</param>
        /// <returns></returns>
        internal Dictionary<string, string> ExtrairInterrupcaoIdeal(Arquivo arqProtocolos, Arquivo arqTecnica)
        {
            Log log = new Log(string.Concat(arqProtocolos.Caminho, "\\Nivel2_", DateTime.Now.ToString("_HHmmss"), "_LOG.", arqProtocolos.Extensao));

            Console.WriteLine("Log: " + log.NomeArquivo);
            
            Dictionary<int, string> result = new Dictionary<int, string>();
            List<string> syn = ArquivoLoader.GetTextosToList(arqProtocolos);
            List<string> tec = ArquivoLoader.GetTextosToList(arqTecnica);

            syn = syn.OrderBy(x => x).ToList();

            List<AtendimentoEmergencia> lstOuvidoria = new List<AtendimentoEmergencia>();
            List<AtendimentoTecnica> lstTecnica = new List<AtendimentoTecnica>();

            Dictionary<AtendimentoEmergencia, AtendimentoTecnica> dicNivel2 =  new Dictionary<AtendimentoEmergencia,AtendimentoTecnica>();

            #region Carregar Synergia
            foreach(string s in syn)
            {
                AtendimentoEmergencia t = new AtendimentoEmergencia();
                DateTime dt = new DateTime();
                t.Protocolo = s.Split(',')[0];
                t.NumeroCliente = s.Split(',')[1].Trim();
                
                DateTime.TryParse(s.Split(',')[2], out dt);
                
                if(dt > DateTime.MinValue)
                    t.DataInicio = dt.ToString("dd/MM/yyyy HH:mm:ss");

                lstOuvidoria.Add(t);
            }
            #endregion


            #region Carregar Tecnica
            bool isCabecalho = true;
            foreach(string t in tec)
            {
                if(arqTecnica.TemCabecalho && isCabecalho)
                {
                    isCabecalho = false;
                    continue;
                }
                AtendimentoTecnica tc = new AtendimentoTecnica();
                tc.Cliente = t.Split(',')[0].Trim();

                tc.Aviso = Int32.Parse(t.Split(',')[1].Trim());

                tc.NumeroInterrupcao = t.Split(',')[2].Trim();

                DateTime dt = new DateTime();
                DateTime.TryParse(t.Split(',')[3], out dt);                
                if(dt > DateTime.MinValue)
                    tc.InterrupcaoInicio = dt;

                if (!tc.InterrupcaoInicio.HasValue)
                    Debugger.Break();

                dt = new DateTime();
                DateTime.TryParse(t.Split(',')[4], out dt);                
                if(dt > DateTime.MinValue)
                    tc.InterrupcaoFim = dt;

                lstTecnica.Add(tc);
            }
            #endregion


            List<AtendimentoEmergencia> errSemOcorrencias = new List<AtendimentoEmergencia>();
            List<AtendimentoTecnica> errOcorrenciasAposIngresso = new List<AtendimentoTecnica>();

            foreach(AtendimentoEmergencia o in lstOuvidoria)
            {
                //procurar cliente da ouvid na base da tecnica
                List<AtendimentoTecnica> avisosUc = lstTecnica.Where(t => t.Cliente.Trim().Equals(o.NumeroCliente.Trim())).ToList();
                
                if(avisosUc.Count() == 0)
                {
                    avisosUc = lstTecnica.Where(t => t.Aviso.Equals(o.Aviso)).ToList();

                    if (avisosUc.Count() == 0)
                    {
                        errSemOcorrencias.Add(o);
                        continue;
                    }
                    else
                    {

                    }
                }

                DateTime dt = DateTime.Parse(o.DataInicio);
                avisosUc =  avisosUc.OrderBy(oo => oo.InterrupcaoInicio).ToList();

                AtendimentoTecnica tecnicaIdeal = avisosUc.Where(t => t.InterrupcaoInicio <= dt).LastOrDefault();
                if(tecnicaIdeal == null || string.IsNullOrWhiteSpace(tecnicaIdeal.NumeroInterrupcao))
                {
                    if (avisosUc.Count > 0)
                    {

                        List<AtendimentoTecnica> uu = dicNivel2.Values.Where( b => b.Cliente.Equals(o.NumeroCliente)).ToList();
                        if (uu.Count() < 2)
                        {
                            avisosUc = avisosUc.OrderBy(y => y.InterrupcaoInicio).ToList();
                            tecnicaIdeal = avisosUc.FirstOrDefault();
                        }
                        else
                        {
                            List<AtendimentoTecnica> lstAvisosRegistrados = dicNivel2.Values.Where(b => b.Cliente.Equals(o.NumeroCliente)).ToList();
                            avisosUc.ForEach(aviso => tecnicaIdeal = lstAvisosRegistrados.Where(t => t.InterrupcaoInicio.Value != aviso.InterrupcaoInicio.Value).ToList().FirstOrDefault());
                        }

                        if(tecnicaIdeal == null)
                        {
                            //errOcorrenciasAposIngresso
                        }
                    }
                    else
                    {
                        //nao deve passar por aqui
                        avisosUc.ForEach(x => errOcorrenciasAposIngresso.Add(x));
                        continue;
                    }
                }

                dicNivel2.Add(o, tecnicaIdeal);
            }


            log.LogFull("Numero Cliente\tProtocolo\tDt Inicio Ouvidoria\tNum Aviso\tNum Cliente Tecnica\tNum.Interrupcao\tDt Inicio Interrupcao\tDt Fim Interrupcao");
            foreach(AtendimentoEmergencia o in dicNivel2.Keys)
            {
                log.LogFull(
                    string.Format("{0}\t{1}\t{2}\t{3}\t{4}\t{5}\t{6}\t{7}"
                    , o.NumeroCliente
                    , o.Protocolo
                    , o.DataInicio
                    , dicNivel2[o].Aviso
                    , dicNivel2[o].Cliente
                    , dicNivel2[o].NumeroInterrupcao
                    , dicNivel2[o].InterrupcaoInicio
                    , dicNivel2[o].InterrupcaoFim)
                );
            }

            Log logErros = new Log(string.Concat(arqProtocolos.Caminho, "\\Nivel2_ERROS_Sem_Ocorrencias_", DateTime.Now.ToString("_HHmmss"), "_LOG.", arqProtocolos.Extensao));
            foreach(AtendimentoEmergencia erros in errSemOcorrencias)
            {
                logErros.LogFull(
                    string.Format("{0}\t{1}\t{2}"
                        , erros.NumeroCliente, erros.Protocolo, erros.DataInicio)
                    );
            }

            return new Dictionary<string, string>();
        }



        private void flush(DataTable dt, ref List<string> buffer)
        {
            foreach (DataRow dr in dt.Rows)
            {
                buffer.Add(string.Format("{0},{1},{2},{3},{4},{5}"
                    , DBNull.Value.Equals(dr[0]) ? string.Empty : dr[0].ToString()
                    , DBNull.Value.Equals(dr[1]) ? string.Empty : dr[1].ToString()
                    , DBNull.Value.Equals(dr[2]) ? string.Empty : dr[2].ToString()
                    , DBNull.Value.Equals(dr[3]) ? string.Empty : dr[3].ToString()
                    , DBNull.Value.Equals(dr[4]) ? string.Empty : dr[4].ToString()
                    , DBNull.Value.Equals(dr[5]) ? string.Empty : dr[5].ToString()
                    )
                );
            }
        }
    }

    public class AtendimentoEmergencia
    {
        public string Protocolo { get; set; }
        public string NumeroCliente { get; set; }
        public string DataInicio { get; set; }
        public string HoraInicio { get; set; }
        public string MotivoEmpresa { get; set; }
        public string Observacao { get; set; }
        public int Pagina { get; set; }
        public string EstadoCliente { get; set; }
        public string ClasseCliente { get; set; }
        public int Aviso { get; set; }
    }

    public class AtendimentoTecnica
    {
        public string Cliente { get; set; }
        public DateTime? InterrupcaoInicio { get; set; }
        public DateTime? InterrupcaoFim { get; set; }
        public int Aviso { get; set; }
        public string NumeroInterrupcao { get; set; }
    }

}


using IBM.Data.Informix;
using SalesforceExtractor.Dados.SalesForce;
using SalesforceExtractor.Entidades.Enumeracoes;
using SalesforceExtractor.Entidades.Modif;
using SalesforceExtractor.Utils;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;

namespace SalesforceExtractor.Entidades
{
    public class ExtracaoSalesForce
    {
        List<ModifBase> pacoteSFModif = new List<ModifBase>();
        Dictionary<string, string> _accountSf;
        ModifBase _modif;   

        public List<ModifBase> PacoteSFModif
        {
            get { return pacoteSFModif; }
            private set { pacoteSFModif = value; }
        }

        private string _empresa = null;
        private TipoCliente _tipoCliente;

        public ExtracaoSalesForce(TipoCliente tipoCliente, string Empresa = null)
        {
            this._empresa = Empresa;
            this._tipoCliente = tipoCliente;

            if (Empresa != null)
            {
                if (Empresa.Trim().ToUpper().Equals("AMPLA") || Empresa.Trim().ToUpper().Equals("2005"))
                {
                    connectionString = ConfigurationSettings.AppSettings["ConnectionStringAmplaPro"];
                }
                else if (Empresa.Trim().ToUpper().Equals("COELCE") || Empresa.Trim().ToUpper().Equals("2003"))
                {
                    connectionString = ConfigurationSettings.AppSettings["ConnectionStringCoelcePro"];
                }
            }

            salesForceDaoDes = new SynergiaDAO(connectionString);
            if (string.IsNullOrWhiteSpace(tipoExtracao))
            {

            }
            else if ("DELTA".Equals(tipoExtracao.ToUpper().Trim()))
            {
                //brsyfz001des
                transacao = salesForceDaoDes.AbrirTransacao();
                Console.WriteLine("Inserindo dados de street e conta em dicionário... essa operação pode levar um tempo");
            }
            else if ("ATENDIMENTO".Equals(tipoExtracao.ToUpper().Trim()))
            {
                //
            }
            else
            {
                carregarDadosStreet();
            }
        }

        public void AbrirConexaoDes()
        {
            salesForceDaoDes = new SynergiaDAO(connectionString);
        }


        public void FecharConexaoDes()
        {
            salesForceDaoDes.FecharConexao();
        }

        string connectionString = string.Empty;

        public static string localArquivoDados = ConfigurationSettings.AppSettings["localArquivoDados"];
        public static string outputArquivo = ConfigurationSettings.AppSettings["outputArquivo"];
        public static string outputArquivoLog = ConfigurationSettings.AppSettings["outputArquivoLog"] + DateTime.Now.ToString("yyyyMMdd") + "ExtracaoLogErro.txt";
        public static string tipoExtracao = ConfigurationSettings.AppSettings["tipoExtracao"];
        public static string cabecalho = DateTime.Now.ToString("yyyy-MM-ddT000000.000Z");

        string idAnterior = string.Empty, numeroclienteAnterior = string.Empty, idContaAnterior = string.Empty, tipoIdentidadeAnterior = string.Empty;

        Dictionary<string, string> dicStreet = new Dictionary<string, string>();//key= cobinação de nome da rua, tipo rua, comuna e localidade, código street já  gerado
        public Dictionary<long, Street> dicStreetBd = new Dictionary<long, Street>();//Guarda street recuperada do bd
        public Dictionary<long, string> dicContaBd = new Dictionary<long, string>();//Guarda street recuperada do bd
        
        //CONTA
        public Dictionary<string, string> dicContaModifNome = new Dictionary<string, string>();
        public Dictionary<string, string> dicContaModifEmail = new Dictionary<string, string>();
        public Dictionary<string, string> dicContaModifDocumento = new Dictionary<string, string>();
        public Dictionary<string, string> dicContaModifTelefone = new Dictionary<string, string>();
        public Dictionary<string, string> dicContaModifNascimento = new Dictionary<string, string>();
        public Dictionary<string, string> dicContaModifMae = new Dictionary<string, string>();
        public Dictionary<string, string> dicContaModifGiro = new Dictionary<string, string>();
        public Dictionary<string, string> dicContaModifClasse = new Dictionary<string, string>();
        
        //CONTATO
        public Dictionary<string, string> dicContatoModifNome = new Dictionary<string, string>();
        public Dictionary<string, string> dicContatoModifEmail = new Dictionary<string, string>();
        public Dictionary<string, string> dicContatoModifDocumento = new Dictionary<string, string>();
        public Dictionary<string, string> dicContatoModifTelefone = new Dictionary<string, string>();
        public Dictionary<string, string> dicContatoModifNascimento = new Dictionary<string, string>();
        public Dictionary<string, string> dicContatoModifMae = new Dictionary<string, string>();

        static int qtdeClienteDespersonalido = 0;
        bool clienteDespersonalizado = false;
        bool salvaStreet = false;
        int contaLinha = 0;

        // StringBuilder sql = new StringBuilder();

        //Dados de Endereço estão na base de Des... Usado para recuperar os ID's de conta e street em DES
        SynergiaDAO salesForceDaoDes = null;

        public IfxTransaction transacao;

        string nomeArquivo = DateTime.Now.Day + "_" +
                                          DateTime.Now.Month + "_" +
                                          DateTime.Now.Year + "_" +
                                          DateTime.Now.DayOfYear + "." +
                                          ConfigurationSettings.AppSettings["NomeArquivoLogLocal"];

        internal void iniciarExtracao()
        {
            Util.preencheDicionarios();

            //preencheClientePratil();

            StreamReader sr = new StreamReader(localArquivoDados, Encoding.GetEncoding("ISO-8859-1"));
            StringBuilder texto = new StringBuilder();
            //List<string> ListaNumeroClientes = new List<string>();
            //HashSet<string> ListaNumeroClientes = new HashSet<string>();

            GravarCabecalho();

            while (!sr.EndOfStream)
            {
                string linha = sr.ReadLine();
                
                try
                {
                    processaDadoInformix(linha);
                }
                catch (Exception ex)
                {
                    //string cliente = linha.Replace(";", "").Replace("\"", "").Replace("\\", "").Split('|').Select(r => r.Trim()).ToArray()[11];
                    string erro = +contaLinha + "|" + ex.Message;

                    Util.EscreverArquivo("logErro" + DateTime.Now.ToString("yyyyMMdd"), erro, ".txt");

                    Console.WriteLine(string.Concat("Erro no processamento do registro '", linha , "' (linha ", contaLinha, "): ", ex.Message));
                }

                //string[] rows = sr.ReadLine().Replace(";", "").Replace("\"", "").Replace("\\", "").Split('|');

                contaLinha++;

                if (contaLinha % Convert.ToInt64(ConfigurationSettings.AppSettings["CommitArquivo"]) == 0)
                {
                    GravaArquivo();
                }


                //if (sql.Length % 100 == 0)
                //{
                //    Util.EscreverArquivo("sql", sql.ToString(), ".sql");
                //    Console.WriteLine("Commitando transação...");
                //    salesForceDaoDes.executarComando(sql.ToString(), transacao);
                //    Console.WriteLine("Commit OK... Linhas processadas: " + contaLinha);
                //    sql.Clear();
                //}

            }
            //transacao.Commit();
            GravaArquivo();

            Console.WriteLine("Linhas Processadas: " + contaLinha);
        }

        Dictionary<string, string> clientesPratilCoelce = new Dictionary<string, string>();

        Dictionary<string, string> dicConta = new Dictionary<string, string>();
        Dictionary<string, string> dicPod = new Dictionary<string, string>();

        public void processaDadoInformix(object dadoInformix)
        {
            string[] rows = dadoInformix.ToString().Replace(";", "").Replace("\"", "").Replace("\\", "").Split('|').Select(r => r.Trim()).ToArray();

            //Console.WriteLine(string.Format("{0}\tCliente:{1} Doc:{2} CodModif:{3}", DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss"), rows[(int)DicColuna.col1_7.numero_pod], rows[(int)DicColuna.col1_7.documento_cliente], rows[(int)DicColuna.col1_7.codigo_modif]));
            Console.Write(".");

            string podExistente = string.Empty;           

            if (rows[(int)DicColuna.col1_7.tipo_rede].Equals("0"))
            {
                rows[(int)DicColuna.col1_7.tipo_rede] = string.Empty;
            }

            /*string cliPratil = string.Empty;
            if (clientesPratilCoelce.TryGetValue(rows[(int)DicColuna.col1_7.cuenta_principal], out cliPratil))
            {
                return;
            }*/

            //Romulo Silva - 31/01/2017 - Solicitado que quando o transformador for XXX, colocar vazio
            #region Transformador
            if (rows[(int)DicColuna.col1_7.tipo_transformador].Equals("XXX"))
            {
                rows[(int)DicColuna.col1_7.tipo_transformador] = string.Empty;
            }
            #endregion

            //Romulo Silva - 25/01/2017 - Solicitação Ronan - Caso o telefone não seja um nº válido, colocar vazio
            #region Validação de Telefone e Celular
            rows[(int)DicColuna.col1_7.telefone1] = Util.ValidaTelefone(rows[(int)DicColuna.col1_7.telefone1], rows[(int)DicColuna.col1_7.ciudad]);
            rows[(int)DicColuna.col1_7.telefone2] = Util.ValidaTelefone(rows[(int)DicColuna.col1_7.telefone2], rows[(int)DicColuna.col1_7.ciudad]);
            rows[(int)DicColuna.col1_7.telefone3] = Util.ValidaTelefone(rows[(int)DicColuna.col1_7.telefone3], rows[(int)DicColuna.col1_7.ciudad]);
            #endregion

            //Monta Suministro
            string emp = rows[(int)DicColuna.col1_7.id_empresa] == "2005" ? "AMA" : "COE";
            rows[(int)DicColuna.col1_7.suministro] = rows[(int)DicColuna.col1_7.cuenta_principal];
            rows[(int)DicColuna.col1_7.suministro] += "BRA";
            rows[(int)DicColuna.col1_7.suministro] += emp;
            rows[(int)DicColuna.col1_7.suministro] += rows[(int)DicColuna.col1_7.contrato];

            rows[(int)DicColuna.col1_7.identificador_asset] = rows[(int)DicColuna.col1_7.cuenta_principal] +
                rows[(int)DicColuna.col1_7.producto] + "BRA" + emp + rows[(int)DicColuna.col1_7.contrato];


            #region Valida Data de Nascimento
            if (!string.IsNullOrEmpty(rows[(int)DicColuna.col1_7.fecha_nasc].Trim()))
            {
                int anoNasc = Convert.ToInt32(rows[(int)DicColuna.col1_7.fecha_nasc].Substring(0, 4));
                if (anoNasc <= 1900 || anoNasc > DateTime.Now.Year)
                {
                    rows[(int)DicColuna.col1_7.fecha_nasc] = string.Empty;
                    rows[(int)DicColuna.col1_7.fecha_nascimento] = string.Empty;
                }
                else
                {
                    rows[(int)DicColuna.col1_7.fecha_nascimento] = rows[(int)DicColuna.col1_7.fecha_nasc];
                }
            }
            #endregion

            #region Padronização de Endereços

            //rows = analisaNormalizacaoEndereco(rows);

            #endregion

            #region Valida Email
            if (!string.IsNullOrEmpty(rows[(int)DicColuna.col1_7.mail]) && !Regex.IsMatch(rows[(int)DicColuna.col1_7.mail], @"\A(?:[a-z0-9!#$%&'*+/=?^_`{|}~-]+(?:\.[a-z0-9!#$%&'*+/=?^_`{|}~-]+)*@(?:[a-z0-9](?:[a-z0-9-]*[a-z0-9])?\.)+[a-z0-9](?:[a-z0-9-]*[a-z0-9])?)\Z", RegexOptions.IgnoreCase))
            {
                rows[(int)DicColuna.col1_7.mail] = string.Empty;
            }
            #endregion

            #region Chaves de Street
            rows[(int)DicColuna.col1_7.identificador_street] = rows[(int)DicColuna.col1_7.id_empresa] + rows[(int)DicColuna.col1_7.cuenta_principal];
            rows[(int)DicColuna.col1_7.pais_street] = rows[(int)DicColuna.col1_7.pais];
            rows[(int)DicColuna.col1_7.comuna_street] = rows[(int)DicColuna.col1_7.comuna];
            rows[(int)DicColuna.col1_7.barrio_street] = rows[(int)DicColuna.col1_7.barrio];
            #endregion

            #region Validação Identificador Conta - Delta

            rows = gerarIdConta(rows);

            rows = buscarIdentificadorConta(rows);
            //ler ID conta de um arquivo external, gerado de tempos em tempos
            //rows[(int)DicColuna.col1_7.identificador_conta] = this._accountSf.Where(c => c.Key == rows[(int)DicColuna.col1_7.cuenta_principal]).FirstOrDefault().Value;
            if (string.IsNullOrWhiteSpace(rows[(int)DicColuna.col1_7.identificador_conta]))
            {
                Console.WriteLine("\nCliente não encontrado na account_sf: " + rows[(int)DicColuna.col1_7.cuenta_principal]);
                return;
            }
            #endregion

            if (rows[(int)DicColuna.col1_7.identificador_conta].Equals(rows[(int)DicColuna.col1_7.id_empresa] + "0000000000000000000000INVALIDO"))
            {
                clienteDespersonalizado = true;
                return;
            }


            #region Sincronizar MODIF :: preparação dos dados

            //Percorre a solução em busca das classes que implementam a ModifBase
            foreach (ModifBase inst in typeof(ModifBase)
            .Assembly.GetTypes()
            .Where(t => t.IsSubclassOf(typeof(ModifBase)) && !t.IsAbstract)
            .Select(t => (ModifBase)Activator.CreateInstance(t)))
            {
                foreach (ItemEntidade item in inst.GetEntidades(rows[(int)DicColuna.col1_7.grupo]))
                {
                    try
                    {
                        if (item.CodigoModif == Convert.ToInt32(rows[(int)DicColuna.col1_7.codigo_modif]))
                        {
                            //não atualizar MODIF para CE
                            if ("CE".Equals(rows[135].ToUpper()) && "A".Equals(rows[(int)DicColuna.col1_7.grupo]))
                                continue;

                            if (rows[inst.DicionarioInternoId] != null && rows[inst.DicionarioInternoId].ToUpper().Contains("INVALIDO"))
                                continue;

                            inst.Identificador = rows[inst.IdDicionarioIdentificador];
                            item.NovoValor = string.IsNullOrWhiteSpace(rows[item.DicionarioInternoValor]) ? " " : rows[item.DicionarioInternoValor];
                            inst.ItemsModificados.Add(item);

                            if (!pacoteSFModif.Contains(inst))
                                pacoteSFModif.Add(inst);
                        }
                    }
                    catch (Exception ex)
                    { }
                }
            }

            #region PointOfDelivery   OBSOLETE OBSOLETE  OBSOLETE  OBSOLETE
            //_modif = new PointOfDelivery();
            //Debugger.Break();

            //foreach (ItemEntidade item in _modif.GetEntidades(rows[(int)DicColuna.col1_7.grupo]))
            //{
            //    if (item.CodigoModif == Convert.ToInt32(rows[(int)DicColuna.col1_7.codigo_modif]))
            //    {
            //        _modif.ExternalId = rows[(int)DicColuna.col1_7.identificador_conta];
            //        item.NovoValor = rows[item.CodigoDicionarioInterno];

            //        if (!string.IsNullOrEmpty(item.NovoValor))
            //            _modif.ItemsModificados.Add(item);
            //    }

            //    if (!string.IsNullOrEmpty(item.NovoValor) && !pacoteSFModif.Contains(_modif))
            //        pacoteSFModif.Add(_modif);
            //}

            //#endregion



            //#region Account

            //_modif = new Account();
            //Debugger.Break();

            //foreach (ItemEntidade item in _modif.GetEntidades(rows[(int)DicColuna.col1_7.grupo].ToString()))
            //{
            //    if (item.CodigoModif == Convert.ToInt32(rows[(int)DicColuna.col1_7.codigo_modif]))
            //    {
            //        _modif.ExternalId = rows[(int)DicColuna.col1_7.identificador_conta];
            //        item.NovoValor = rows[item.CodigoDicionarioInterno];

            //        if (!string.IsNullOrEmpty(item.NovoValor))
            //            _modif.ItemsModificados.Add(item);
            //    }

            //    if (!string.IsNullOrEmpty(item.NovoValor) && !pacoteSFModif.Contains(_modif))
            //        pacoteSFModif.Add(_modif);
            //}

            #endregion

            #endregion   //FIM Sincronizar MODIF :: preparação dos dados


            //Gravação de atualização para o POD
            //comentado por a atualizaçao da modif vai descontinuar o uso dos arquivos gerados
            //GravaEntidade(true, rows);
        }

        public string[] gerarIdConta(string[] rows)
        {
            //Controle de Tipo de Documento
            bool FlagCpf = false;

            //Variáveis de controle para cliente despersonalizado.
            string consumidorDespersonalizadoId = string.Empty;
            string consumidorDespersonalizadoNome = "CONSUMIDOR PROCURE A ENEL";

            #region Padronização Documentos
            rows[(int)DicColuna.col1_7.identificador_conta] = rows[(int)DicColuna.col1_7.identificador_conta].Replace("-", "").Replace("/", "").Replace(" ", "");

            rows[(int)DicColuna.col1_7.documento_cliente] = rows[(int)DicColuna.col1_7.documento_cliente].Replace(".", "").Replace("-", "").Replace("/", "").Replace(" ", "");

            //ID e Documento precisam ser padronizados
            if (!string.IsNullOrEmpty(rows[(int)DicColuna.col1_7.documento_cliente].Trim())
                && rows[(int)DicColuna.col1_7.tipo_identidade].Equals("005") ||
                rows[(int)DicColuna.col1_7.tipo_identidade].Equals("002") ||
                rows[(int)DicColuna.col1_7.tipo_identidade].Equals("006"))
            {
                if (rows[(int)DicColuna.col1_7.tipo_identidade].Equals("005") || rows[(int)DicColuna.col1_7.tipo_identidade].Equals("006"))
                {
                    //Sinaliza que é CPF
                    FlagCpf = true;

                    //Acerta pra pegar os últimos 11 digitos do documento, no caso, CPF
                    rows[(int)DicColuna.col1_7.identificador_conta] = rows[(int)DicColuna.col1_7.documento_cliente].Length > 11 ? rows[(int)DicColuna.col1_7.id_empresa] + rows[(int)DicColuna.col1_7.documento_cliente].Substring((rows[(int)DicColuna.col1_7.documento_cliente].Length) - 11, 11).ToString() : rows[(int)DicColuna.col1_7.id_empresa] + rows[(int)DicColuna.col1_7.documento_cliente];
                    rows[(int)DicColuna.col1_7.documento_cliente] = rows[(int)DicColuna.col1_7.documento_cliente].Length > 11 ? rows[(int)DicColuna.col1_7.documento_cliente].Substring((rows[(int)DicColuna.col1_7.documento_cliente].Length) - 11, 11).ToString() : rows[(int)DicColuna.col1_7.documento_cliente];
                }
                else
                {
                    //Acerta pra pegar os últimos 11 digitos do documento, no caso, CNPJ
                    rows[(int)DicColuna.col1_7.identificador_conta] = rows[(int)DicColuna.col1_7.identificador_conta].Length > 14 ? rows[(int)DicColuna.col1_7.id_empresa] + rows[(int)DicColuna.col1_7.documento_cliente].Substring((rows[(int)DicColuna.col1_7.documento_cliente].Length) - 14, 14).ToString() : rows[(int)DicColuna.col1_7.id_empresa] + rows[(int)DicColuna.col1_7.documento_cliente];
                    rows[(int)DicColuna.col1_7.documento_cliente] = rows[(int)DicColuna.col1_7.documento_cliente].Length > 14 ? rows[(int)DicColuna.col1_7.documento_cliente].Substring((rows[(int)DicColuna.col1_7.documento_cliente].Length) - 14, 14).ToString() : rows[(int)DicColuna.col1_7.documento_cliente];
                }
            }
            //Cliente sem documento, gera ID a partir do numero_cliente
            else
            {
                rows[(int)DicColuna.col1_7.identificador_conta] = rows[(int)DicColuna.col1_7.id_empresa] + rows[(int)DicColuna.col1_7.cuenta_principal] + "INVALIDO";

                rows[(int)DicColuna.col1_7.tipo_identidade] = rows[(int)DicColuna.col1_7.tipo_identidade_contato] = "001";
            }

            #endregion

            #region Controle de Clientes despersonalizados

            consumidorDespersonalizadoId = rows[(int)DicColuna.col1_7.id_empresa] + "0000000000000000000000INVALIDO";


            if (rows[(int)DicColuna.col1_7.nombre].StartsWith("CONSUMIDOR PRO") ||
                rows[(int)DicColuna.col1_7.nombre].StartsWith("CLIENTE PROCURE") ||
                rows[(int)DicColuna.col1_7.nombre].StartsWith("CONSUMIDOR ATUALIZE") ||
                rows[(int)DicColuna.col1_7.nombre].EndsWith("CONSUMIDORA DESATIVADA") ||
                rows[(int)DicColuna.col1_7.nombre].Contains("DESPERSONALIZAD"))
            {
                //Limpa posição sempre...
                //Array.Clear(rows, 0, rows.Length);

                rows[(int)DicColuna.col1_7.identificador_conta] = consumidorDespersonalizadoId;
                //id = consumidorDespersonalizadoId;

                rows[(int)DicColuna.col1_7.nombre] = consumidorDespersonalizadoNome;
                rows[(int)DicColuna.col1_7.tipo_identidade] = rows[(int)DicColuna.col1_7.tipo_identidade_contato] = "001";

                qtdeClienteDespersonalido++;
                clienteDespersonalizado = true;
            }
            #endregion

            #region Validação CPF e CNPJ
            //System.Threading.ThreadPool.QueueUserWorkItem(o => GravaEntidade());


            //if (!clienteDespersonalizado && //idAnterior.Trim().Equals(rows[(int)DicColuna.col1_7.documento_cliente]) && tipoIdentidadeAnterior.Equals(rows[(int)DicColuna.col1_7.tipo_identidade]))
            //{
            //    rows[(int)DicColuna.col1_7.identificador_conta] = idContaAnterior;
            //}
            //else 
            if (!clienteDespersonalizado) //!dicId.TryGetValue(rows[(int)DicColuna.col1_7.documento_cliente], out ListaNumeroClientes))//!id.Trim().Equals(rows[(int)DicColuna.col1_7.documento_cliente]))
            {
                if (FlagCpf)
                {
                    if (!Util.ValidaCPF(rows[(int)DicColuna.col1_7.documento_cliente]))
                    {
                        rows[(int)DicColuna.col1_7.identificador_conta] = rows[(int)DicColuna.col1_7.id_empresa] + rows[(int)DicColuna.col1_7.cuenta_principal] + "INVALIDO";
                        rows[(int)DicColuna.col1_7.tipo_identidade] = rows[(int)DicColuna.col1_7.tipo_identidade_contato] = "001";

                    }
                    else
                    {
                        rows[(int)DicColuna.col1_7.identificador_conta] = rows[(int)DicColuna.col1_7.id_empresa] + rows[(int)DicColuna.col1_7.documento_cliente] + "CPF";
                        //rows[(int)DicColuna.col1_7.tipo_identidade_contato] = "CPF";
                    }
                }
                else
                {
                    if (!Util.ValidaCNPJ(rows[(int)DicColuna.col1_7.documento_cliente]))
                    {
                        rows[(int)DicColuna.col1_7.identificador_conta] = rows[(int)DicColuna.col1_7.id_empresa] + rows[(int)DicColuna.col1_7.cuenta_principal] + "INVALIDO";
                        rows[(int)DicColuna.col1_7.tipo_identidade] = rows[(int)DicColuna.col1_7.tipo_identidade_contato] = "001";

                    }
                    else
                    {
                        rows[(int)DicColuna.col1_7.identificador_conta] = rows[(int)DicColuna.col1_7.id_empresa] + rows[(int)DicColuna.col1_7.documento_cliente] + "CNPJ";
                        //rows[(int)DicColuna.col1_7.tipo_identidade_contato] = "CNPJ";
                    }
                }
            }
            //else
            //{
            //    rows[(int)DicColuna.col1_7.identificador_conta] = rows[(int)DicColuna.col1_7.id_empresa] + rows[(int)DicColuna.col1_7.cuenta_principal] + "INVALIDO";
            //    rows[(int)DicColuna.col1_7.tipo_identidade] = rows[(int)DicColuna.col1_7.tipo_identidade_contato] = "001";
            //}

            #endregion
            //Console.WriteLine("Conta gerada: " + rows[(int)DicColuna.col1_7.identificador_conta]);
            return rows;
        }

        public static Boolean IsNumeric(String value)
        {
            foreach (Char c in value.ToCharArray())
            {
                if (Char.IsNumber(c))
                {
                    return true;
                }
            }
            return false;
        }

        private static StringBuilder conta = new StringBuilder();
        private static StringBuilder contato = new StringBuilder();
        private static StringBuilder asset = new StringBuilder();
        private static StringBuilder street = new StringBuilder();
        private static StringBuilder address = new StringBuilder();
        private static StringBuilder pod = new StringBuilder();
        private static StringBuilder billing = new StringBuilder();
        private static StringBuilder serviceProduct = new StringBuilder();
        private static StringBuilder eletricityService = new StringBuilder();

        #region Métodos de Organizar e Gravar em Arquivo
        private void montaEGravaConta(string[] rows)
        {
            //string data = _empresa + "_" + cabecalho;

            string data = cabecalho;

            conta.Clear();

            if (rows[(int)DicColuna.col1_7.apellido_materno].Trim().Equals(""))
            {
                rows[(int)DicColuna.col1_7.apellido_materno] = ".";
            }
            else 
            {
                try
                {
                    rows[(int)DicColuna.col1_7.apellido_materno] = rows[(int)DicColuna.col1_7.apellido_materno].Substring(rows[(int)DicColuna.col1_7.apellido_materno].LastIndexOf(" ").Equals(-1) ? 0 : rows[(int)DicColuna.col1_7.apellido_materno].LastIndexOf(" "));
                }                                                  
                catch
                {
                    rows[(int)DicColuna.col1_7.apellido_materno] = ".";
                }                
            }

            rows[(int)DicColuna.col1_7.apellido_paterno] = ".";

            if (rows[(int)DicColuna.col1_7.grupo].Equals("B"))
            {
                //NOME
                if (rows[(int)DicColuna.col1_7.codigo_modif].Equals("4"))
                {
                    string nome = string.Empty;

                    if(!dicContaModifNome.TryGetValue(rows[(int)DicColuna.col1_7.identificador_conta].ToString(),out nome))
                    {
                        if ((rows[(int)DicColuna.col1_7.identificador_conta].Contains("CPF") || rows[(int)DicColuna.col1_7.identificador_conta].Contains("CNPJ")) && rows[(int)DicColuna.col1_7.nombre].Contains("CONSUMIDOR"))
                        {
                            dicContaModifNome.Add(rows[(int)DicColuna.col1_7.identificador_conta], rows[(int)DicColuna.col1_7.identificador_conta]);
                            return;
                        }

                        conta.Append("\""); conta.Append(rows[(int)DicColuna.col1_7.identificador_conta]); conta.Append("\";");
                        conta.Append("\""); conta.Append(rows[(int)DicColuna.col1_7.nombre]); conta.Append("\"\r\n");
                        
                        Util.EscreverArquivo("ACCOUNT_nombre_" + data, conta.ToString());

                        dicContaModifNome.Add(rows[(int)DicColuna.col1_7.identificador_conta], rows[(int)DicColuna.col1_7.identificador_conta]);
                    }
                }

                //if documento
                if (rows[(int)DicColuna.col1_7.codigo_modif].Equals("7"))
                {
                    string documento = string.Empty;

                    if (!dicContaModifDocumento.TryGetValue(rows[(int)DicColuna.col1_7.identificador_conta].ToString(), out documento))
                    {

                        if ((rows[(int)DicColuna.col1_7.identificador_conta].Contains("CPF") || rows[(int)DicColuna.col1_7.identificador_conta].Contains("CNPJ")) && rows[(int)DicColuna.col1_7.tipo_identidade].Trim() == "001")
                        {
                            dicContaModifDocumento.Add(rows[(int)DicColuna.col1_7.identificador_conta], rows[(int)DicColuna.col1_7.identificador_conta]);
                            return;
                        }

                        conta.Append("\""); conta.Append(rows[(int)DicColuna.col1_7.identificador_conta]); conta.Append("\";");
                        conta.Append("\""); conta.Append(rows[(int)DicColuna.col1_7.tipo_identidade]); conta.Append("\";");
                        conta.Append("\""); conta.Append(rows[(int)DicColuna.col1_7.documento_cliente]); conta.Append("\"\r\n");
                        
                        Util.EscreverArquivo("ACCOUNT_documento_" + data, conta.ToString());

                        dicContaModifDocumento.Add(rows[(int)DicColuna.col1_7.identificador_conta], rows[(int)DicColuna.col1_7.identificador_conta]);
                    }
                }

                //if email
                if (rows[(int)DicColuna.col1_7.codigo_modif].Equals("26"))
                {
                    string email = string.Empty;

                    if (!dicContaModifEmail.TryGetValue(rows[(int)DicColuna.col1_7.identificador_conta].ToString(), out email))
                    {
                        conta.Append("\""); conta.Append(rows[(int)DicColuna.col1_7.identificador_conta]); conta.Append("\";");
                        conta.Append("\""); conta.Append(rows[(int)DicColuna.col1_7.mail]); conta.Append("\";");
                        conta.Append("\""); conta.Append(rows[(int)DicColuna.col1_7.mail]); conta.Append("\"\r\n");
                        
                        Util.EscreverArquivo("ACCOUNT_email_" + data, conta.ToString());

                        dicContaModifEmail.Add(rows[(int)DicColuna.col1_7.identificador_conta], rows[(int)DicColuna.col1_7.identificador_conta]);
                    }
                }

                //if telefone
                if (rows[(int)DicColuna.col1_7.codigo_modif].Equals("8"))
                {
                    string telefone = string.Empty;

                    if (!dicContaModifTelefone.TryGetValue(rows[(int)DicColuna.col1_7.identificador_conta].ToString(), out telefone))
                    {
                        if (rows[(int)DicColuna.col1_7.telefone1].Trim() == "")
                        {
                            if (rows[(int)DicColuna.col1_7.telefone2].Trim() == "")
                            {
                                if (rows[(int)DicColuna.col1_7.telefone3].Trim() == "")
                                {
                                    return;
                                }
                                else
                                {
                                    rows[(int)DicColuna.col1_7.telefone1] = rows[(int)DicColuna.col1_7.telefone3].Trim();
                                }
                            }
                            else
                            {
                                rows[(int)DicColuna.col1_7.telefone1] = rows[(int)DicColuna.col1_7.telefone2].Trim();
                                rows[(int)DicColuna.col1_7.telefone2] = "";
                            }
                        }

                        conta.Append("\""); conta.Append(rows[(int)DicColuna.col1_7.identificador_conta]); conta.Append("\";");
                        conta.Append("\""); conta.Append(rows[(int)DicColuna.col1_7.telefone1]); conta.Append("\";");
                        conta.Append("\""); conta.Append(rows[(int)DicColuna.col1_7.telefone2]); conta.Append("\";");
                        conta.Append("\""); conta.Append(rows[(int)DicColuna.col1_7.telefone3]); conta.Append("\"\r\n");
                        
                        Util.EscreverArquivo("ACCOUNT_telefone_" + data, conta.ToString());

                        dicContaModifTelefone.Add(rows[(int)DicColuna.col1_7.identificador_conta], rows[(int)DicColuna.col1_7.identificador_conta]);
                    }
                }

                //if nascimento
                if (rows[(int)DicColuna.col1_7.codigo_modif].Equals("87"))
                {
                     string nascimento = string.Empty;

                     if (!dicContaModifNascimento.TryGetValue(rows[(int)DicColuna.col1_7.identificador_conta].ToString(), out nascimento))
                     {
                         conta.Append("\""); conta.Append(rows[(int)DicColuna.col1_7.identificador_conta]); conta.Append("\";");
                         conta.Append("\""); conta.Append(rows[(int)DicColuna.col1_7.fecha_nasc]); conta.Append("\"\r\n");
                         
                         Util.EscreverArquivo("ACCOUNT_nascimento_" + data, conta.ToString());

                         dicContaModifNascimento.Add(rows[(int)DicColuna.col1_7.identificador_conta], rows[(int)DicColuna.col1_7.identificador_conta]);
                     }
                }

                //if mae
                if (rows[(int)DicColuna.col1_7.codigo_modif].Equals("147"))
                {
                    string mae = string.Empty;

                    if (!dicContaModifMae.TryGetValue(rows[(int)DicColuna.col1_7.identificador_conta].ToString(), out mae))
                    {
                        conta.Append("\""); conta.Append(rows[(int)DicColuna.col1_7.identificador_conta]); conta.Append("\";");
                        conta.Append("\""); conta.Append(rows[(int)DicColuna.col1_7.apellido_materno]); conta.Append("\"\r\n");
                        Util.EscreverArquivo("ACCOUNT_mae_" + data, conta.ToString());


                        dicContaModifMae.Add(rows[(int)DicColuna.col1_7.identificador_conta], rows[(int)DicColuna.col1_7.identificador_conta]);
                    }
                }

                if (rows[(int)DicColuna.col1_7.codigo_modif].Equals("55"))
                {
                    string giro = string.Empty;

                    if (!dicContaModifGiro.TryGetValue(rows[(int)DicColuna.col1_7.identificador_conta].ToString(), out giro))
                    {
                        conta.Append("\""); conta.Append(rows[(int)DicColuna.col1_7.identificador_conta]); conta.Append("\";");
                        conta.Append("\""); conta.Append(rows[(int)DicColuna.col1_7.giro]); conta.Append("\"\r\n");
                        Util.EscreverArquivo("ACCOUNT_giro_" + data, conta.ToString());

                        dicContaModifGiro.Add(rows[(int)DicColuna.col1_7.identificador_conta], rows[(int)DicColuna.col1_7.identificador_conta]);
                    }
                }

                /*if (rows[(int)DicColuna.col1_7.codigo_modif].Equals("54"))
                {
                    string classe = string.Empty;

                    if (!dicContaModifClasse.TryGetValue(rows[(int)DicColuna.col1_7.identificador_conta].ToString(), out classe))
                    {
                        conta.Append("\""); conta.Append(rows[(int)DicColuna.col1_7.identificador_conta]); conta.Append("\";");
                        conta.Append("\""); conta.Append(rows[(int)DicColuna.col1_7.clase_servicio]); conta.Append("\";\r\n");
                        Util.EscreverArquivo("ACCOUNT_classe_" + data, conta.ToString());

                        dicContaModifClasse.Add(rows[(int)DicColuna.col1_7.identificador_conta], rows[(int)DicColuna.col1_7.identificador_conta]);
                    }
                }*/
            }
            else // GRUPO A
            {
                if (rows[(int)DicColuna.col1_7.codigo_modif].Equals("7"))
                {
                    string nome = string.Empty;

                    if (!dicContaModifNome.TryGetValue(rows[(int)DicColuna.col1_7.identificador_conta].ToString(), out nome))
                    {
                        if ((rows[(int)DicColuna.col1_7.identificador_conta].Contains("CPF") || rows[(int)DicColuna.col1_7.identificador_conta].Contains("CNPJ")) && rows[(int)DicColuna.col1_7.nombre].Contains("CONSUMIDOR"))
                        {
                            dicContaModifNome.Add(rows[(int)DicColuna.col1_7.identificador_conta], rows[(int)DicColuna.col1_7.identificador_conta]);
                            return;
                        }

                        conta.Append("\""); conta.Append(rows[(int)DicColuna.col1_7.identificador_conta]); conta.Append("\";");
                        conta.Append("\""); conta.Append(rows[(int)DicColuna.col1_7.nombre]); conta.Append("\"\r\n");

                        Util.EscreverArquivo("ACCOUNT_nombre_" + data, conta.ToString());

                        dicContaModifNome.Add(rows[(int)DicColuna.col1_7.identificador_conta], rows[(int)DicColuna.col1_7.identificador_conta]);
                    }
                }

                //if documento
                if (rows[(int)DicColuna.col1_7.codigo_modif].Equals("14"))
                {
                    string documento = string.Empty;

                    if (!dicContaModifDocumento.TryGetValue(rows[(int)DicColuna.col1_7.identificador_conta].ToString(), out documento))
                    {
                        if ((rows[(int)DicColuna.col1_7.identificador_conta].Contains("CPF") || rows[(int)DicColuna.col1_7.identificador_conta].Contains("CNPJ")) && rows[(int)DicColuna.col1_7.tipo_identidade].Trim() == "001")
                        {
                            dicContaModifDocumento.Add(rows[(int)DicColuna.col1_7.identificador_conta], rows[(int)DicColuna.col1_7.identificador_conta]);
                            return;
                        }

                        conta.Append("\""); conta.Append(rows[(int)DicColuna.col1_7.identificador_conta]); conta.Append("\";");
                        conta.Append("\""); conta.Append(rows[(int)DicColuna.col1_7.tipo_identidade]); conta.Append("\";");
                        conta.Append("\""); conta.Append(rows[(int)DicColuna.col1_7.documento_cliente]); conta.Append("\"\r\n");

                        Util.EscreverArquivo("ACCOUNT_documento_" + data, conta.ToString());

                        dicContaModifDocumento.Add(rows[(int)DicColuna.col1_7.identificador_conta], rows[(int)DicColuna.col1_7.identificador_conta]);
                    }
                }

                //if email
                if (rows[(int)DicColuna.col1_7.codigo_modif].Equals("624"))
                {
                    string email = string.Empty;

                    if (!dicContaModifEmail.TryGetValue(rows[(int)DicColuna.col1_7.identificador_conta].ToString(), out email))
                    {
                        conta.Append("\""); conta.Append(rows[(int)DicColuna.col1_7.identificador_conta]); conta.Append("\";");
                        conta.Append("\""); conta.Append(rows[(int)DicColuna.col1_7.mail]); conta.Append("\";");
                        conta.Append("\""); conta.Append(rows[(int)DicColuna.col1_7.mail]); conta.Append("\"\r\n");

                        Util.EscreverArquivo("ACCOUNT_email_" + data, conta.ToString());

                        dicContaModifEmail.Add(rows[(int)DicColuna.col1_7.identificador_conta], rows[(int)DicColuna.col1_7.identificador_conta]);
                    }
                }

                //if telefone
                if (rows[(int)DicColuna.col1_7.codigo_modif].Equals("13"))
                {
                    string telefone = string.Empty;

                    if (!dicContaModifTelefone.TryGetValue(rows[(int)DicColuna.col1_7.identificador_conta].ToString(), out telefone))
                    {

                        if (rows[(int)DicColuna.col1_7.telefone1].Trim() == "")
                        {
                            if (rows[(int)DicColuna.col1_7.telefone2].Trim() == "")
                            {
                                if (rows[(int)DicColuna.col1_7.telefone3].Trim() == "")
                                {
                                    return;
                                }
                                else
                                {
                                    rows[(int)DicColuna.col1_7.telefone1] = rows[(int)DicColuna.col1_7.telefone3].Trim();
                                }
                            }
                            else
                            {
                                rows[(int)DicColuna.col1_7.telefone1] = rows[(int)DicColuna.col1_7.telefone2].Trim();
                                rows[(int)DicColuna.col1_7.telefone2] = "";
                            }
                        }

                        conta.Append("\""); conta.Append(rows[(int)DicColuna.col1_7.identificador_conta]); conta.Append("\";");
                        conta.Append("\""); conta.Append(rows[(int)DicColuna.col1_7.telefone1]); conta.Append("\";");
                        conta.Append("\""); conta.Append(rows[(int)DicColuna.col1_7.telefone2]); conta.Append("\";");
                        conta.Append("\""); conta.Append(rows[(int)DicColuna.col1_7.telefone3]); conta.Append("\"\r\n");

                        Util.EscreverArquivo("ACCOUNT_telefone_" + data, conta.ToString());

                        dicContaModifTelefone.Add(rows[(int)DicColuna.col1_7.identificador_conta], rows[(int)DicColuna.col1_7.identificador_conta]);
                    }
                }
                             
                if (rows[(int)DicColuna.col1_7.codigo_modif].Equals("30"))
                {
                    string giro = string.Empty;

                    if (!dicContaModifGiro.TryGetValue(rows[(int)DicColuna.col1_7.identificador_conta].ToString(), out giro))
                    {
                        conta.Append("\""); conta.Append(rows[(int)DicColuna.col1_7.identificador_conta]); conta.Append("\";");
                        conta.Append("\""); conta.Append(rows[(int)DicColuna.col1_7.giro]); conta.Append("\"\r\n");
                        Util.EscreverArquivo("ACCOUNT_giro_" + data, conta.ToString());

                        dicContaModifGiro.Add(rows[(int)DicColuna.col1_7.identificador_conta], rows[(int)DicColuna.col1_7.identificador_conta]);
                    }
                }

                /*if (rows[(int)DicColuna.col1_7.codigo_modif].Equals("25"))
                {
                    string classe = string.Empty;

                    if (!dicContaModifClasse.TryGetValue(rows[(int)DicColuna.col1_7.identificador_conta].ToString(), out classe))
                    {
                        conta.Append("\""); conta.Append(rows[(int)DicColuna.col1_7.identificador_conta]); conta.Append("\";");
                        conta.Append("\""); conta.Append(rows[(int)DicColuna.col1_7.clase_servicio]); conta.Append("\";\r\n");
                        Util.EscreverArquivo("ACCOUNT_classe_" + data, conta.ToString());

                        dicContaModifClasse.Add(rows[(int)DicColuna.col1_7.identificador_conta], rows[(int)DicColuna.col1_7.identificador_conta]);
                    }
                }*/
            }
           
        }

        private void montaEGravaContato(string[] rows)
        {
            //string data = _empresa + "_" + cabecalho;

            string data = cabecalho;

            contato.Clear();
            rows[(int)DicColuna.col1_7.identificador_contacto] = rows[(int)DicColuna.col1_7.identificador_conta];
            rows[(int)DicColuna.col1_7.identificador_conta_contato] = rows[(int)DicColuna.col1_7.identificador_conta];
            rows[(int)DicColuna.col1_7.id_empresa_contato] = rows[(int)DicColuna.col1_7.id_empresa];

            rows[(int)DicColuna.col1_7.apellido] = rows[(int)DicColuna.col1_7.nombre].Substring(rows[(int)DicColuna.col1_7.nombre].LastIndexOf(" ").Equals(-1) ? 0 : rows[(int)DicColuna.col1_7.nombre].LastIndexOf(" "));
            rows[(int)DicColuna.col1_7.nombre_contacto] = rows[(int)DicColuna.col1_7.nombre] = rows[(int)DicColuna.col1_7.nombre].Substring(0, rows[(int)DicColuna.col1_7.nombre].LastIndexOf(" ").Equals(-1) ? rows[(int)DicColuna.col1_7.nombre].Length : rows[(int)DicColuna.col1_7.nombre].LastIndexOf(" "));


            rows[(int)DicColuna.col1_7.telefono1_contato] = retornaMaiorString(rows[(int)DicColuna.col1_7.telefone1], rows[(int)DicColuna.col1_7.telefone2]);
            rows[(int)DicColuna.col1_7.telefono2_contato] = rows[(int)DicColuna.col1_7.telefone3];

            rows[(int)DicColuna.col1_7.moneda_contato] = rows[(int)DicColuna.col1_7.moneda];

            rows[(int)DicColuna.col1_7.id_empresa_contato] = rows[(int)DicColuna.col1_7.id_empresa];

            rows[(int)DicColuna.col1_7.numero_identidade_contato] = rows[(int)DicColuna.col1_7.documento_cliente];

            rows[(int)DicColuna.col1_7.apellido_materno_contato] = rows[(int)DicColuna.col1_7.apellido_materno];

            rows[(int)DicColuna.col1_7.tipo_identidade_contato] = rows[(int)DicColuna.col1_7.tipo_identidade];

            if (rows[(int)DicColuna.col1_7.grupo].Equals("B"))
            {
                //NOME
                if (rows[(int)DicColuna.col1_7.codigo_modif].Equals("4"))
                {
                    string nome = string.Empty;

                    if (!dicContatoModifNome.TryGetValue(rows[(int)DicColuna.col1_7.identificador_conta].ToString(), out nome))
                    {
                        if ((rows[(int)DicColuna.col1_7.identificador_contacto].Contains("CPF") || rows[(int)DicColuna.col1_7.identificador_contacto].Contains("CNPJ")) && rows[(int)DicColuna.col1_7.nombre].Contains("CONSUMIDOR"))
                        {
                            dicContatoModifNome.Add(rows[(int)DicColuna.col1_7.identificador_conta], rows[(int)DicColuna.col1_7.identificador_conta]);
                            return;
                        }

                        contato.Append("\""); contato.Append(rows[(int)DicColuna.col1_7.identificador_contacto]); contato.Append("\";");
                        contato.Append("\""); contato.Append(rows[(int)DicColuna.col1_7.nombre_contacto]); contato.Append("\";");
                        contato.Append("\""); contato.Append(rows[(int)DicColuna.col1_7.apellido]); contato.Append("\"\r\n");
                        
                        Util.EscreverArquivo("CONTACT_nombre_" + data, contato.ToString());

                        dicContatoModifNome.Add(rows[(int)DicColuna.col1_7.identificador_conta], rows[(int)DicColuna.col1_7.identificador_conta]);
                    }
                   
                }

                //if documento
                if (rows[(int)DicColuna.col1_7.codigo_modif].Equals("7"))
                {
                    string documento = string.Empty;

                    if (!dicContatoModifDocumento.TryGetValue(rows[(int)DicColuna.col1_7.identificador_conta].ToString(), out documento))
                    {

                        if ((rows[(int)DicColuna.col1_7.identificador_contacto].Contains("CPF") || rows[(int)DicColuna.col1_7.identificador_contacto].Contains("CNPJ")) && rows[(int)DicColuna.col1_7.tipo_identidade_contato].Trim().Equals("001"))
                        {
                            dicContatoModifDocumento.Add(rows[(int)DicColuna.col1_7.identificador_conta], rows[(int)DicColuna.col1_7.identificador_conta]);
                            return;
                        }

                        contato.Append("\""); contato.Append(rows[(int)DicColuna.col1_7.identificador_contacto]); contato.Append("\";");
                        contato.Append("\""); contato.Append(rows[(int)DicColuna.col1_7.tipo_identidade_contato]); contato.Append("\";");
                        contato.Append("\""); contato.Append(rows[(int)DicColuna.col1_7.numero_identidade_contato]); contato.Append("\"\r\n");
                        Util.EscreverArquivo("CONTACT_documento_" + data, contato.ToString());

                        dicContatoModifDocumento.Add(rows[(int)DicColuna.col1_7.identificador_conta], rows[(int)DicColuna.col1_7.identificador_conta]);
                    }

                }

                //if nascimento
                if (rows[(int)DicColuna.col1_7.codigo_modif].Equals("87"))
                {
                    string nasc = string.Empty;

                    if (!dicContatoModifNascimento.TryGetValue(rows[(int)DicColuna.col1_7.identificador_conta].ToString(), out nasc))
                    {
                        contato.Append("\""); contato.Append(rows[(int)DicColuna.col1_7.identificador_contacto]); contato.Append("\";");
                        contato.Append("\""); contato.Append(rows[(int)DicColuna.col1_7.fecha_nascimento]); contato.Append("\"\r\n");
                        Util.EscreverArquivo("CONTACT_nascimento_" + data, contato.ToString());

                        dicContatoModifNascimento.Add(rows[(int)DicColuna.col1_7.identificador_conta], rows[(int)DicColuna.col1_7.identificador_conta]);
                    }
                }

                //if email
                if (rows[(int)DicColuna.col1_7.codigo_modif].Equals("26"))
                {
                    string email = string.Empty;

                    if (!dicContatoModifEmail.TryGetValue(rows[(int)DicColuna.col1_7.identificador_conta].ToString(), out email))
                    {
                        contato.Append("\""); contato.Append(rows[(int)DicColuna.col1_7.identificador_contacto]); contato.Append("\";");
                        contato.Append("\""); contato.Append(rows[(int)DicColuna.col1_7.mail]); contato.Append("\";");
                        contato.Append("\""); contato.Append(rows[(int)DicColuna.col1_7.mail]); contato.Append("\"\r\n");
                        Util.EscreverArquivo("CONTACT_email_" + data, contato.ToString());

                        dicContatoModifEmail.Add(rows[(int)DicColuna.col1_7.identificador_conta], rows[(int)DicColuna.col1_7.identificador_conta]);
                    }
                }

                //if telefone
                if (rows[(int)DicColuna.col1_7.codigo_modif].Equals("8"))
                {
                    string telefone = string.Empty;

                    if (!dicContatoModifTelefone.TryGetValue(rows[(int)DicColuna.col1_7.identificador_conta].ToString(), out telefone))
                    {
                        if (rows[(int)DicColuna.col1_7.telefone1].Trim() == "")
                        {
                            if (rows[(int)DicColuna.col1_7.telefone2].Trim() == "")
                            {
                                if (rows[(int)DicColuna.col1_7.telefone3].Trim() == "")
                                {
                                    return;
                                }
                                else
                                {
                                    rows[(int)DicColuna.col1_7.telefone1] = rows[(int)DicColuna.col1_7.telefone3].Trim();
                                }
                            }
                            else
                            {
                                rows[(int)DicColuna.col1_7.telefone1] = rows[(int)DicColuna.col1_7.telefone2].Trim();
                                rows[(int)DicColuna.col1_7.telefone2] = "";
                            }
                        }

                        contato.Append("\""); contato.Append(rows[(int)DicColuna.col1_7.identificador_contacto]); contato.Append("\";");
                        contato.Append("\""); contato.Append(rows[(int)DicColuna.col1_7.telefone1]); contato.Append("\";");
                        contato.Append("\""); contato.Append(rows[(int)DicColuna.col1_7.telefone2]); contato.Append("\";");
                        contato.Append("\""); contato.Append(rows[(int)DicColuna.col1_7.telefone3]); contato.Append("\"\r\n");
                        Util.EscreverArquivo("CONTACT_telefone_" + data, contato.ToString());

                        dicContatoModifTelefone.Add(rows[(int)DicColuna.col1_7.identificador_conta], rows[(int)DicColuna.col1_7.identificador_conta]);
                    }
                }
                //if Mae
                if (rows[(int)DicColuna.col1_7.codigo_modif].Equals("147"))
                {
                    string mae = string.Empty;

                    if (!dicContatoModifMae.TryGetValue(rows[(int)DicColuna.col1_7.identificador_conta].ToString(), out mae))
                    {
                        contato.Append("\""); contato.Append(rows[(int)DicColuna.col1_7.identificador_contacto]); contato.Append("\";");
                        contato.Append("\""); contato.Append(rows[(int)DicColuna.col1_7.apellido_materno_contato]); contato.Append("\"\r\n");
                        Util.EscreverArquivo("CONTACT_mae_" + data, contato.ToString());

                        dicContatoModifMae.Add(rows[(int)DicColuna.col1_7.identificador_conta], rows[(int)DicColuna.col1_7.identificador_conta]);
                    }
                }
            }
            else //ELSE GRUPO A
            {
                if (rows[(int)DicColuna.col1_7.codigo_modif].Equals("7"))
                {
                    string nome = string.Empty;

                    if (!dicContatoModifNome.TryGetValue(rows[(int)DicColuna.col1_7.identificador_conta].ToString(), out nome))
                    {
                        if ((rows[(int)DicColuna.col1_7.identificador_contacto].Contains("CPF") || rows[(int)DicColuna.col1_7.identificador_contacto].Contains("CNPJ")) && rows[(int)DicColuna.col1_7.nombre].Contains("CONSUMIDOR"))
                        {
                            dicContatoModifNome.Add(rows[(int)DicColuna.col1_7.identificador_conta], rows[(int)DicColuna.col1_7.identificador_conta]);
                            return;
                        }

                        contato.Append("\""); contato.Append(rows[(int)DicColuna.col1_7.identificador_contacto]); contato.Append("\";");
                        contato.Append("\""); contato.Append(rows[(int)DicColuna.col1_7.nombre_contacto]); contato.Append("\";");
                        contato.Append("\""); contato.Append(rows[(int)DicColuna.col1_7.apellido]); contato.Append("\"\r\n");

                        Util.EscreverArquivo("CONTACT_nombre_" + data, contato.ToString());

                        dicContatoModifNome.Add(rows[(int)DicColuna.col1_7.identificador_conta], rows[(int)DicColuna.col1_7.identificador_conta]);
                    }
                }

                //if documento
                if (rows[(int)DicColuna.col1_7.codigo_modif].Equals("14"))
                {
                    string documento = string.Empty;

                    if (!dicContatoModifDocumento.TryGetValue(rows[(int)DicColuna.col1_7.identificador_conta].ToString(), out documento))
                    {

                        if ((rows[(int)DicColuna.col1_7.identificador_contacto].Contains("CPF") || rows[(int)DicColuna.col1_7.identificador_contacto].Contains("CNPJ")) && rows[(int)DicColuna.col1_7.tipo_identidade_contato].Trim().Equals("001"))
                        {
                            dicContatoModifDocumento.Add(rows[(int)DicColuna.col1_7.identificador_conta], rows[(int)DicColuna.col1_7.identificador_conta]);
                            return;
                        }

                        contato.Append("\""); contato.Append(rows[(int)DicColuna.col1_7.identificador_contacto]); contato.Append("\";");
                        contato.Append("\""); contato.Append(rows[(int)DicColuna.col1_7.tipo_identidade_contato]); contato.Append("\";");
                        contato.Append("\""); contato.Append(rows[(int)DicColuna.col1_7.numero_identidade_contato]); contato.Append("\"\r\n");
                        Util.EscreverArquivo("CONTACT_documento_" + data, contato.ToString());

                        dicContatoModifDocumento.Add(rows[(int)DicColuna.col1_7.identificador_conta], rows[(int)DicColuna.col1_7.identificador_conta]);
                    }
                }

                //if email
                if (rows[(int)DicColuna.col1_7.codigo_modif].Equals("624"))
                {
                    string email = string.Empty;

                    if (!dicContatoModifEmail.TryGetValue(rows[(int)DicColuna.col1_7.identificador_conta].ToString(), out email))
                    {
                        contato.Append("\""); contato.Append(rows[(int)DicColuna.col1_7.identificador_contacto]); contato.Append("\";");
                        contato.Append("\""); contato.Append(rows[(int)DicColuna.col1_7.mail]); contato.Append("\";");
                        contato.Append("\""); contato.Append(rows[(int)DicColuna.col1_7.mail]); contato.Append("\"\r\n");
                        Util.EscreverArquivo("CONTACT_email_" + data, contato.ToString());

                        dicContatoModifEmail.Add(rows[(int)DicColuna.col1_7.identificador_conta], rows[(int)DicColuna.col1_7.identificador_conta]);
                    }
                }

                //if telefone
                if (rows[(int)DicColuna.col1_7.codigo_modif].Equals("13"))
                {
                    string telefone = string.Empty;

                    if (!dicContatoModifTelefone.TryGetValue(rows[(int)DicColuna.col1_7.identificador_conta].ToString(), out telefone))
                    {

                        if (rows[(int)DicColuna.col1_7.telefone1].Trim() == "")
                        {
                            if (rows[(int)DicColuna.col1_7.telefone2].Trim() == "")
                            {
                                if (rows[(int)DicColuna.col1_7.telefone3].Trim() == "")
                                {
                                    return;
                                }
                                else
                                {
                                    rows[(int)DicColuna.col1_7.telefone1] = rows[(int)DicColuna.col1_7.telefone3].Trim();
                                }
                            }
                            else
                            {
                                rows[(int)DicColuna.col1_7.telefone1] = rows[(int)DicColuna.col1_7.telefone2].Trim();
                                rows[(int)DicColuna.col1_7.telefone2] = "";
                            }
                        }

                        contato.Append("\""); contato.Append(rows[(int)DicColuna.col1_7.identificador_contacto]); contato.Append("\";");
                        contato.Append("\""); contato.Append(rows[(int)DicColuna.col1_7.telefone1]); contato.Append("\";");
                        contato.Append("\""); contato.Append(rows[(int)DicColuna.col1_7.telefone2]); contato.Append("\";");
                        contato.Append("\""); contato.Append(rows[(int)DicColuna.col1_7.telefone3]); contato.Append("\"\r\n");
                        Util.EscreverArquivo("CONTACT_telefone_" + data, contato.ToString());

                        dicContatoModifTelefone.Add(rows[(int)DicColuna.col1_7.identificador_conta], rows[(int)DicColuna.col1_7.identificador_conta]);
                    }
                }             
            }

        }
        
        private void montaEGravaAsset(string[] rows)
        {
            //Se tiver produto, grava asset
            if (!string.IsNullOrEmpty(rows[(int)DicColuna.col1_7.producto].Trim()))
            {
                rows[(int)DicColuna.col1_7.identificador_conta_asset] = rows[(int)DicColuna.col1_7.identificador_contato_asset] = rows[(int)DicColuna.col1_7.identificador_conta];

                asset.Append("\""); asset.Append(rows[(int)DicColuna.col1_7.identificador_asset]); asset.Append("\";");
                asset.Append("\""); asset.Append(rows[(int)DicColuna.col1_7.nombre_del_activo]); asset.Append("\";");
                asset.Append("\""); asset.Append(rows[(int)DicColuna.col1_7.identificador_conta_asset]); asset.Append("\";");
                asset.Append("\""); asset.Append(rows[(int)DicColuna.col1_7.identificador_contato_asset]); asset.Append("\";");
                asset.Append("\""); asset.Append(rows[(int)DicColuna.col1_7.suministro]); asset.Append("\";");
                asset.Append("\""); asset.Append(rows[(int)DicColuna.col1_7.descripcion]); asset.Append("\";");
                asset.Append("\""); asset.Append(rows[(int)DicColuna.col1_7.producto]); asset.Append("\";");
                asset.Append("\""); asset.Append(rows[(int)DicColuna.col1_7.estado]); asset.Append("\";");
                asset.Append("\""); asset.Append(rows[(int)DicColuna.col1_7.contrato]); asset.Append("\"\r\n");
            }
        }

        private void montaEGravaStreet(string[] rows)
        {
            //Console.WriteLine("Metodo Street: " + salvaStreet);
            //if (!salvaStreet)
            //{
            //    return;
            //}

            street.Append("\""); street.Append(rows[(int)DicColuna.col1_7.identificador_street]); street.Append("\";");
            street.Append("\""); street.Append(rows[(int)DicColuna.col1_7.nombre_calle]); street.Append("\";");
            street.Append("\""); street.Append(rows[(int)DicColuna.col1_7.tipo_calle]); street.Append("\";");
            street.Append("\""); street.Append(rows[(int)DicColuna.col1_7.ciudad]); street.Append("\";");
            street.Append("\""); street.Append(rows[(int)DicColuna.col1_7.uf]); street.Append("\";");
            street.Append("\""); street.Append(rows[(int)DicColuna.col1_7.pais_street]); street.Append("\";");
            street.Append("\""); street.Append(rows[(int)DicColuna.col1_7.comuna_street]); street.Append("\";");
            street.Append("\""); street.Append(rows[(int)DicColuna.col1_7.region]); street.Append("\";");
            street.Append("\""); street.Append(rows[(int)DicColuna.col1_7.calle]); street.Append("\";");
            street.Append("\""); street.Append(rows[(int)DicColuna.col1_7.localidad]); street.Append("\";");
            street.Append("\""); street.Append(rows[(int)DicColuna.col1_7.barrio_street]); street.Append("\"\r\n");

            dicStreet.Add(rows[(int)DicColuna.col1_7.nombre_calle].ToLower().Replace(" ", "") + rows[(int)DicColuna.col1_7.tipo_calle].Trim() + rows[(int)DicColuna.col1_7.comuna_street].Trim() + rows[(int)DicColuna.col1_7.localidad].Trim(), rows[(int)DicColuna.col1_7.identificador_street].Trim());
        }

        private void montaEGravaAddress(string[] rows, bool original)
        {
            String FormatoDirecao = string.Empty;

            rows[(int)DicColuna.col1_7.identificador_address] = rows[(int)DicColuna.col1_7.id_empresa] + rows[(int)DicColuna.col1_7.cuenta_principal];
            rows[(int)DicColuna.col1_7.identificador_street_address] = rows[(int)DicColuna.col1_7.identificador_street];
            rows[(int)DicColuna.col1_7.moneda_address] = rows[(int)DicColuna.col1_7.moneda];

            address.Append("\""); address.Append(rows[(int)DicColuna.col1_7.moneda_address]); address.Append("\";");
            address.Append("\""); address.Append(rows[(int)DicColuna.col1_7.complemento]); address.Append("\";");
            address.Append("\""); address.Append(rows[(int)DicColuna.col1_7.numero]); address.Append("\";");
            address.Append("\""); address.Append(rows[(int)DicColuna.col1_7.esquina_via_sec].Replace(Environment.NewLine, " ").Replace("\n", " ")); address.Append("\";");
            address.Append("\""); address.Append(rows[(int)DicColuna.col1_7.cep]); address.Append("\";");
            address.Append("\""); address.Append(rows[(int)DicColuna.col1_7.identificador_address]); address.Append("\";");
            address.Append("\""); address.Append(rows[(int)DicColuna.col1_7.identificador_street_address]); address.Append("\";");
            address.Append("\""); address.Append(rows[(int)DicColuna.col1_7.barrio]); address.Append("\";");
            // + "\"" + rows[(int)DicColuna.col1_7.calle] + "\";"                                                                     
            address.Append("\""); address.Append(rows[(int)DicColuna.col1_7.tipo_numeracion]); address.Append("\";");
            address.Append("\""); address.Append(rows[(int)DicColuna.col1_7.direccion_concatenada]); address.Append("\";");
            address.Append("\""); address.Append(rows[(int)DicColuna.col1_7.bloque_direccion]); address.Append("\";");
            address.Append("\""); address.Append(rows[(int)DicColuna.col1_7.coord_x]); address.Append("\";");
            address.Append("\""); address.Append(rows[(int)DicColuna.col1_7.coord_y]); address.Append("\";");
            address.Append("\""); address.Append(rows[(int)DicColuna.col1_7.nombre_agrupacion]); address.Append("\";");
            address.Append("\""); address.Append(rows[(int)DicColuna.col1_7.tipo_agrupacion_address]); address.Append("\";");
            address.Append("\""); address.Append(rows[(int)DicColuna.col1_7.tipo_interior]); address.Append("\";");
            address.Append("\""); address.Append(rows[(int)DicColuna.col1_7.direccion_larga]); address.Append("\";");
            address.Append("\""); address.Append(rows[(int)DicColuna.col1_7.lote_manzana]); address.Append("\";");
            address.Append("\""); address.Append(rows[(int)DicColuna.col1_7.tipo_sector]); address.Append("\"\r\n");
        }
       
        private void montaEGravaPod(string[] rows)
        {
            pod.Clear();
            //string data = _empresa + "_" + cabecalho;

            string data = cabecalho;

            rows[(int)DicColuna.col1_7.id_empresa_pod] = rows[(int)DicColuna.col1_7.id_empresa];

            rows[(int)DicColuna.col1_7.direccion_pod] = rows[(int)DicColuna.col1_7.identificador_address];

            rows[(int)DicColuna.col1_7.numero_pod] = rows[(int)DicColuna.col1_7.cuenta_principal];

            rows[(int)DicColuna.col1_7.moneda_pod] = rows[(int)DicColuna.col1_7.moneda];

            rows[(int)DicColuna.col1_7.direccion_pod] = rows[(int)DicColuna.col1_7.direccion];

            rows[(int)DicColuna.col1_7.id_empresa_pod] = rows[(int)DicColuna.col1_7.id_empresa];


            rows[(int)DicColuna.col1_7.identificador_pod] = rows[(int)DicColuna.col1_7.suministro];

            rows[(int)DicColuna.col1_7.fecha_corte] = !string.IsNullOrEmpty(rows[(int)DicColuna.col1_7.fecha_corte].Trim()) ? DateTime.Parse(rows[(int)DicColuna.col1_7.fecha_corte], CultureInfo.GetCultureInfo("en-US")).ToString("yyyy-MM-dd") + "T00:00:00.000Z" : string.Empty;

            rows[(int)DicColuna.col1_7.electrodependiente] = string.IsNullOrEmpty(rows[(int)DicColuna.col1_7.electrodependiente].Trim()) ? "N" : rows[(int)DicColuna.col1_7.electrodependiente].Trim();

            if (!string.IsNullOrEmpty(rows[(int)DicColuna.col1_7.numero_medidor].Trim()))
            {
                rows[(int)DicColuna.col1_7.propiedad_medidor] = string.IsNullOrEmpty(rows[(int)DicColuna.col1_7.propiedad_medidor].Trim()) ? "C" : rows[(int)DicColuna.col1_7.propiedad_medidor].Trim();
            }

            pod.Append("\""); pod.Append(rows[(int)DicColuna.col1_7.identificador_pod]); pod.Append("\";");
            pod.Append("\""); pod.Append(rows[(int)DicColuna.col1_7.numero_pod]); pod.Append("\";");
            pod.Append("\""); pod.Append(rows[(int)DicColuna.col1_7.moneda_pod]); pod.Append("\";");
            pod.Append("\""); pod.Append(rows[(int)DicColuna.col1_7.digito_verificador_pod]); pod.Append("\";");
            pod.Append("\""); pod.Append(rows[(int)DicColuna.col1_7.identificador_address]); pod.Append("\";");
            pod.Append("\""); pod.Append(rows[(int)DicColuna.col1_7.estado_pod]); pod.Append("\";");
            pod.Append("\""); pod.Append(rows[(int)DicColuna.col1_7.pais]); pod.Append("\";");
            pod.Append("\""); pod.Append(rows[(int)DicColuna.col1_7.comuna]); pod.Append("\";");
            pod.Append("\""); pod.Append(rows[(int)DicColuna.col1_7.tipo_segmento]); pod.Append("\";");
            pod.Append("\""); pod.Append(rows[(int)DicColuna.col1_7.medida_disciplina]); pod.Append("\";");
            pod.Append("\""); pod.Append(rows[(int)DicColuna.col1_7.id_empresa_pod]); pod.Append("\";");
            pod.Append("\""); pod.Append(rows[(int)DicColuna.col1_7.electrodependiente]); pod.Append("\";");
            pod.Append("\""); pod.Append(rows[(int)DicColuna.col1_7.tarifa]); pod.Append("\";");
            pod.Append("\""); pod.Append(rows[(int)DicColuna.col1_7.tipo_agrupacion]); pod.Append("\";");
            pod.Append("\""); pod.Append(rows[(int)DicColuna.col1_7.full_electric]); pod.Append("\";");
            pod.Append("\""); pod.Append(rows[(int)DicColuna.col1_7.nombre_boleta]); pod.Append("\";");
            pod.Append("\""); pod.Append(rows[(int)DicColuna.col1_7.ruta]); pod.Append("\";");
            pod.Append("\""); pod.Append(rows[(int)DicColuna.col1_7.direccion_reparto]); pod.Append("\";");
            pod.Append("\""); pod.Append(rows[(int)DicColuna.col1_7.comuna_reparto]); pod.Append("\";");
            pod.Append("\""); pod.Append(rows[(int)DicColuna.col1_7.propiedad_medidor]); pod.Append("\";");
            pod.Append("\""); pod.Append(rows[(int)DicColuna.col1_7.modelo_medidor]); pod.Append("\";");
            pod.Append("\""); pod.Append(rows[(int)DicColuna.col1_7.marca_medidor]); pod.Append("\";");
            pod.Append("\""); pod.Append(rows[(int)DicColuna.col1_7.numero_medidor]); pod.Append("\";");
            pod.Append("\""); pod.Append(rows[(int)DicColuna.col1_7.num_transformador]); pod.Append("\";");
            pod.Append("\""); pod.Append(rows[(int)DicColuna.col1_7.tipo_transformador]); pod.Append("\";");
            pod.Append("\""); pod.Append(rows[(int)DicColuna.col1_7.tipo_conexion]); pod.Append("\";");
            pod.Append("\""); pod.Append(rows[(int)DicColuna.col1_7.estrato_socioeconomico]); pod.Append("\";");
            pod.Append("\""); pod.Append(rows[(int)DicColuna.col1_7.subestacion_electrica_conexion]); pod.Append("\";");
            pod.Append("\""); pod.Append(rows[(int)DicColuna.col1_7.tipo_medida]); pod.Append("\";");
            pod.Append("\""); pod.Append(rows[(int)DicColuna.col1_7.num_alimentador]); pod.Append("\";");
            pod.Append("\""); pod.Append(rows[(int)DicColuna.col1_7.tipo_lectura]); pod.Append("\";");
            pod.Append("\""); pod.Append(rows[(int)DicColuna.col1_7.bloque]); pod.Append("\";");
            pod.Append("\""); pod.Append(rows[(int)DicColuna.col1_7.horario_racionamiento]); pod.Append("\";");
            pod.Append("\""); pod.Append(rows[(int)DicColuna.col1_7.estado_conexao]); pod.Append("\";");
            pod.Append("\""); pod.Append(rows[(int)DicColuna.col1_7.fecha_corte]); pod.Append("\";");
            pod.Append("\""); pod.Append(rows[(int)DicColuna.col1_7.codigo_pcr]); pod.Append("\";");
            pod.Append("\""); pod.Append(rows[(int)DicColuna.col1_7.sed]); pod.Append("\";");
            pod.Append("\""); pod.Append(rows[(int)DicColuna.col1_7.set_cliente]); pod.Append("\";");
            pod.Append("\""); pod.Append(rows[(int)DicColuna.col1_7.llave]); pod.Append("\";");
            pod.Append("\""); pod.Append(rows[(int)DicColuna.col1_7.potencia_capacidad_instalada]); pod.Append("\";");
            pod.Append("\""); pod.Append(rows[(int)DicColuna.col1_7.cliente_singular]); pod.Append("\";");
            pod.Append("\""); pod.Append(rows[(int)DicColuna.col1_7.clase_servicio_pod]); pod.Append("\";");
            pod.Append("\""); pod.Append(rows[(int)DicColuna.col1_7.subclase_servicio]); pod.Append("\";");
            pod.Append("\""); pod.Append(rows[(int)DicColuna.col1_7.ruta_lectura]); pod.Append("\";");
            pod.Append("\""); pod.Append(rows[(int)DicColuna.col1_7.tipo_facturacion]); pod.Append("\";");
            pod.Append("\""); pod.Append(rows[(int)DicColuna.col1_7.mercado]); pod.Append("\";");
            pod.Append("\""); pod.Append(rows[(int)DicColuna.col1_7.carga_aforada]); pod.Append("\";");
            pod.Append("\""); pod.Append(rows[(int)DicColuna.col1_7.ano_fabricacion]); pod.Append("\";");
            pod.Append("\""); pod.Append(rows[(int)DicColuna.col1_7.centro_poblado]); pod.Append("\";");
            pod.Append("\""); pod.Append(rows[(int)DicColuna.col1_7.tipo_rede]); pod.Append("\"\r\n");

            Util.EscreverArquivo("PointOfDelivery_" + data, pod.ToString());

        }
        
        private void montaEGravaBilling(string[] rows)
        {

            rows[(int)DicColuna.col1_7.identificador_conta_billing] = rows[(int)DicColuna.col1_7.identificador_conta];
            rows[(int)DicColuna.col1_7.identificador_address_billing] = rows[(int)DicColuna.col1_7.identificador_address];


            billing.Append("\""); billing.Append(rows[(int)DicColuna.col1_7.identificador_conta_billing]); billing.Append("\";");
            billing.Append("\""); billing.Append(rows[(int)DicColuna.col1_7.tipo]); billing.Append("\";");
            billing.Append("\""); billing.Append(rows[(int)DicColuna.col1_7.identificador_address_billing]); billing.Append("\"\r\n");


        }
        
        private void montaEGravaServiceProduct(string[] rows)
        {
            serviceProduct.Append("\""); serviceProduct.Append(rows[(int)DicColuna.col1_7.identificador_conta]); serviceProduct.Append("\";");
            serviceProduct.Append("\""); serviceProduct.Append(rows[(int)DicColuna.col1_7.identificador_contacto]); serviceProduct.Append("\";");
            serviceProduct.Append("\""); serviceProduct.Append(rows[(int)DicColuna.col1_7.identificador_conta]); serviceProduct.Append("\";");
            serviceProduct.Append("\""); serviceProduct.Append(rows[(int)DicColuna.col1_7.moneda]); serviceProduct.Append("\";");
            serviceProduct.Append("\""); serviceProduct.Append(rows[(int)DicColuna.col1_7.id_empresa] == "2005" ? "AMPLA\"\r\n" : "COELCE\"\r\n");
        }

        //Gera produto 
        private void gerarEletricityService(string[] rows)
        {
            string emp = rows[(int)DicColuna.col1_7.id_empresa] == "2005" ? "AMA" : "COE";

            rows[(int)DicColuna.col1_7.producto] = "0113";

            string identificadorAssetEnergia = rows[(int)DicColuna.col1_7.cuenta_principal] +
                        rows[(int)DicColuna.col1_7.producto] + "BRA" + emp + rows[(int)DicColuna.col1_7.contrato];

            eletricityService.Append("\""); eletricityService.Append(identificadorAssetEnergia); eletricityService.Append("\";");
            eletricityService.Append("\""); eletricityService.Append("Eletricity Service"); eletricityService.Append("\";");
            eletricityService.Append("\""); eletricityService.Append(rows[(int)DicColuna.col1_7.identificador_conta]); eletricityService.Append("\";");
            eletricityService.Append("\""); eletricityService.Append(rows[(int)DicColuna.col1_7.identificador_conta]); eletricityService.Append("\";");
            eletricityService.Append("\""); eletricityService.Append(rows[(int)DicColuna.col1_7.suministro]); eletricityService.Append("\";");
            eletricityService.Append("\""); eletricityService.Append(rows[(int)DicColuna.col1_7.descripcion]); eletricityService.Append("\";");
            eletricityService.Append("\""); eletricityService.Append(rows[(int)DicColuna.col1_7.producto]); eletricityService.Append("\";");
            eletricityService.Append("\""); eletricityService.Append("2"); eletricityService.Append("\";");
            eletricityService.Append("\""); eletricityService.Append(rows[(int)DicColuna.col1_7.contrato]); eletricityService.Append("\"\r\n");


        }
        #endregion

        #region Método de Chamada dos Organizadoes e Gravadores em Arquivo
        private void GravaEntidade(bool gravarCompleto, string[] rows)
        {
            string identificador_street = string.Empty;
            string podExistente = "";

            if (gravarCompleto)
            {
                montaEGravaConta(rows);
                montaEGravaContato(rows);
              
                //montaEGravaAddress(rows, true);
                

                if (!dicPod.TryGetValue(rows[(int)DicColuna.col1_7.cuenta_principal], out podExistente))
                {        
                    String saida= "";

                    if (rows[(int)DicColuna.col1_7.grupo].Equals("B"))
                    {
                        if (Util.dicModifGB.TryGetValue(rows[(int)DicColuna.col1_7.codigo_modif].ToString(), out saida))
                        {
                            montaEGravaPod(rows);
                            dicPod.Add(rows[(int)DicColuna.col1_7.cuenta_principal], rows[(int)DicColuna.col1_7.cuenta_principal]);
                        }
                    }
                    else
                    {
                        if (Util.dicModifGA.TryGetValue(rows[(int)DicColuna.col1_7.codigo_modif].ToString(), out saida))
                        {
                            montaEGravaPod(rows);
                            dicPod.Add(rows[(int)DicColuna.col1_7.cuenta_principal], rows[(int)DicColuna.col1_7.cuenta_principal]);
                        }
                    }                    
                }
            }
            else
            {
                montaEGravaAsset(rows);

                //Street já gerada
                if (dicStreet.TryGetValue(rows[(int)DicColuna.col1_7.nombre_calle].ToLower().Replace(" ", "") + rows[(int)DicColuna.col1_7.tipo_calle].Trim() + rows[(int)DicColuna.col1_7.comuna_street].Trim() + rows[(int)DicColuna.col1_7.localidad].Trim(), out identificador_street))
                {
                    rows[(int)DicColuna.col1_7.identificador_street] = identificador_street;
                }
                else
                {
                    montaEGravaStreet(rows);
                    // Util.picklistRegion(rows[(int)DicColuna.col1_7.region], rows[(int)DicColuna.col1_7.cuenta_principal]);
                }

                montaEGravaAddress(rows, true);
                montaEGravaPod(rows);
                gerarEletricityService(rows);
                montaEGravaBilling(rows);
            }

        }

        public void GravaArquivo()
        {
            Util.EscreverArquivo("ACCOUNT", conta.ToString());

            Util.EscreverArquivo("CONTACT", contato.ToString());

            Util.EscreverArquivo("ASSET", asset.ToString());

            Util.EscreverArquivo("STREET", street.ToString());

            Util.EscreverArquivo("ADDRESS", address.ToString());

            Util.EscreverArquivo("PointOfDelivery", pod.ToString());

            Util.EscreverArquivo("BILLING", billing.ToString());

            Util.EscreverArquivo("SERVICEPRODUCT", serviceProduct.ToString());

            Util.EscreverArquivo("ASSET", eletricityService.ToString());
            //Util.EscreverArquivo("sql", sql.ToString(), ".sql");


            conta = new StringBuilder();
            contato = new StringBuilder();
            asset = new StringBuilder();
            street = new StringBuilder();
            address = new StringBuilder();
            pod = new StringBuilder();
            billing = new StringBuilder();
            serviceProduct = new StringBuilder();
            eletricityService = new StringBuilder();
            //sql = new StringBuilder();
        }
        #endregion

        private string retornaMaiorString(string primeira, string segunda)
        {
            primeira = primeira == null ? string.Empty : primeira;
            segunda = segunda == null ? string.Empty : segunda;

            if (primeira.Trim().Length > segunda.Trim().Length)
            {
                return primeira;
            }
            else
            {
                return segunda;
            }
        }

        public void GravarCabecalho()
        {
            //string data = _empresa + "_" + cabecalho;
            string data = cabecalho;

            if (File.Exists(ExtracaoSalesForce.outputArquivo + "PointOfDelivery_" + data + ".csv"))
            {
                return;
            }

            string cabPod = "\"Identificador PoD\";\"Número PoD\";\"Moneda\";\"Digito verificador PoD\";\"Id Dirección\";\"Estado PoD\";\"País\";\"Comuna\";\"Tipo de Segmento Esquema de Facturación\";\"Medida de Disciplina\";\"Id Empresa\";\"Electrodependiente\";\"Tarifa\";\"Tipo de agrupación\";\"Full electric\";\"Nombre Boleta\";\"Ruta\";\"Dirección de reparto\";\"Comuna de reparto\";\"Propiedad Medidor\";\"Modelo Medidor\";\"Marca Medidor\";\"Número Medidor\";\"Número de Transformador\";\"Tipo de Transformador\";\"Tipo de Conexión\";\"Estrato Socioeconómico\";\"Subestación eléctrica conexión\";\"Tipo de medida Condición de Instalación\";\"Número de Alimentador\";\"Tipo de Lectura\";\"Bloque\";\"Horario de racionamiento\";\"Estado de Conexión\";\"Fecha de Corte\";\"Codigo PCR\";\"SED\";\"SET\";\"Llave\";\"Potencia Capacidad Instalada\";\"Cliente Singular\";\"Clase de servicio\";\"Subclase del servicio\";\"Ruta de lectura\";\"Tipo de Liquidación\";\"Mercado\";\"Carga aforada\";\"Año de fabricación\";\"Centro Probado\";\"Tipo Rede\"\r\n";
            Util.ApagarArquivo("PointOfDelivery_" + data);
            Util.EscreverArquivo("PointOfDelivery_" + data, cabPod);

            string conta = "\"Identificador unico de la cuenta\";\"nombre\"\r\n";
            Util.ApagarArquivo("ACCOUNT_nombre_" + data);
            Util.EscreverArquivo("ACCOUNT_nombre_" + data,conta);

            conta = "\"Identificador unico de la cuenta\";\"Email principal\";\"Email secundario\"\r\n";
            Util.ApagarArquivo("ACCOUNT_email_" + data);
            Util.EscreverArquivo("ACCOUNT_email_" + data,conta);

            conta = "\"Identificador unico de la cuenta\";\"Tipo de identidad\";\"Número de documento\"\r\n";
            Util.ApagarArquivo("ACCOUNT_documento_" + data);
            Util.EscreverArquivo("ACCOUNT_documento_" + data, conta);

            conta = "\"Identificador unico de la cuenta\";\"Teléfono principal\";\"Telefono secundario\";\"Telefono adicional\"\r\n";
            Util.ApagarArquivo("ACCOUNT_telefone_" + data);
            Util.EscreverArquivo("ACCOUNT_telefone_" + data, conta);
            
            conta = "\"Identificador unico de la cuenta\";\"Fecha de nacimiento\"\r\n";
            Util.ApagarArquivo("ACCOUNT_nascimento_" + data);
            Util.EscreverArquivo("ACCOUNT_nascimento_" + data, conta);

            conta = "\"Identificador unico de la cuenta\";\"Apellido materno\"\r\n";
            Util.ApagarArquivo("ACCOUNT_mae_" + data);
            Util.EscreverArquivo("ACCOUNT_mae_" + data, conta);

            conta = "\"Identificador unico de la cuenta\";\"GIRO\"\r\n";
            Util.ApagarArquivo("ACCOUNT_giro_" + data);
            Util.EscreverArquivo("ACCOUNT_giro_" + data, conta);

            string contato = "\"Identificador Unico de Contacto\";\"Nombre\";\"Apellido\"\r\n";
            Util.ApagarArquivo("CONTACT_nombre_" + data);
            Util.EscreverArquivo("CONTACT_nombre_" + data, contato);

            contato = "\"Identificador Unico de Contacto\";\"Correo electrónico\";\"Correo electrónico secundario\"\r\n";
            Util.ApagarArquivo("CONTACT_email_" + data);
            Util.EscreverArquivo("CONTACT_email_" + data, contato);

            contato = "\"Identificador Unico de Contacto\";\"Tipo de identificación\";\"Número de identidad\"\r\n";
            Util.ApagarArquivo("CONTACT_documento_" + data);
            Util.EscreverArquivo("CONTACT_documento_" + data, contato);

            contato = "\"Identificador Unico de Contacto\";\"Teléfono\";\"Teléfono secundario\";\"Teléfono móvil\"\r\n";
            Util.ApagarArquivo("CONTACT_telefone_" + data);
            Util.EscreverArquivo("CONTACT_telefone_" + data, contato);

            contato = "\"Identificador Unico de Contacto\";\"Fecha de nacimiento\"\r\n";
            Util.ApagarArquivo("CONTACT_nascimento_" + data);
            Util.EscreverArquivo("CONTACT_nascimento_" + data, contato);

            contato = "\"Identificador unico de Contacto\";\"Apellido materno\"\r\n";
            Util.ApagarArquivo("CONTACT_mae_" + data);
            Util.EscreverArquivo("CONTACT_mae_" + data, contato);

        }

        /// <summary>
        /// Gera endereço padronizado
        /// </summary>
        /// <param name="endereco">Endereço</param>
        /// <returns>Endereço</returns>
        public string[] normalizarEndereco(string endereco)
        {
            string[] retorno = { "", "", "" };

            String[] _split = endereco.Trim().Replace(".", " ").Replace("  ", " ").Split(' ');

            bool nome = false;
            bool complemento = false;

            for (int i = 0; i < _split.Length; i++)
            {
                if (i == 0)
                {
                    retorno[0] = Util.descobreTipoCalle(_split[i].ToString(), out nome);

                    if (nome)
                        retorno[1] += _split[i].Trim() + " ";

                }
                else if (IsNumeric(_split[i]) && i == 1)
                {
                    retorno[1] = _split[i].Trim() + " ";
                }
                else if (IsNumeric(_split[i]) && i > 1 && string.IsNullOrWhiteSpace(retorno[1]))
                {
                    retorno[1] = _split[i].Trim() + " ";
                    complemento = true;
                }
                else if (IsNumeric(_split[i]) || complemento)
                {
                    retorno[2] += _split[i].Trim() + " ";
                    complemento = true;
                }
                else if ((_split[i].Equals("N") || _split[i].Equals("NUM") || _split[i].Equals("CASA") || _split[i].Equals("CS") || _split[i].Equals("LOTE")
                    || _split[i].Equals("LT") || _split[i].Equals("N°") || _split[i].Equals("AP") || _split[i].Equals("BL") || _split[i].Equals("BLOCO")
                    || _split[i].Equals("QUADRA") || _split[i].Equals("QD") || _split[i].Equals("L")) && i > 1)
                {
                    retorno[2] += _split[i].Trim() + " ";
                    complemento = true;
                }
                else
                {
                    retorno[1] += _split[i].Trim() + " ";
                }
            }

            return retorno;

        }

        public void carregarDadosStreet()
        {
            if (!string.IsNullOrWhiteSpace(tipoExtracao) && tipoExtracao.ToUpper().Trim().Equals("DELTA"))
            {
                dicStreetBd = salesForceDaoDes.consultarStreet(transacao);
            }
            else
            {
                StreamReader sr = new StreamReader(ConfigurationSettings.AppSettings["localDicStreet"], Encoding.GetEncoding("ISO-8859-1"));

                while (!sr.EndOfStream)
                {
                    string[] streetArq = sr.ReadLine().Replace(";", "").Replace("\"", "").Replace("\\", "").Split('|').Select(r => r.Trim()).ToArray();

                    Street street = new Street();

                    street.id_rua = Convert.ToInt64(streetArq[0]);
                    street.nome_rua = streetArq[1];
                    street.tipo_logradouro = streetArq[2];
                    street.cidade = streetArq[3];
                    street.uf = streetArq[4];
                    street.pais = streetArq[5];
                    street.comuna = streetArq[6];
                    street.regiao = streetArq[7];
                    street.calle = streetArq[8];
                    street.localidad = streetArq[9];
                    street.bairro = streetArq[10];
                    //street.data_inclusao = Convert.ToDateTime(streetArq[11]);

                    dicStreetBd.Add(street.id_rua, street);
                }
            }
        }
        public void carregarDadosConta()
        {
            if (tipoExtracao.ToUpper().Trim().Equals("DELTA"))
            {
                //dicContaBd = salesForceDaoDes.consultarConta(transacao);
            }
            else
            {
                //StreamReader sr = new StreamReader(ConfigurationSettings.AppSettings["localDicConta"], Encoding.GetEncoding("ISO-8859-1"));

                //while (!sr.EndOfStream)
                //{
                //    string[] contaArq = sr.ReadLine().Replace(";", "").Replace("\"", "").Replace("\\", "").Split('|').Select(r => r.Trim()).ToArray();
                //    string c = string.Empty;
                //    if (dicContaBd.TryGetValue(Convert.ToInt64(contaArq[0]), out c))
                //    {
                //        continue;
                //    }
                //    dicContaBd.Add(Convert.ToInt64(contaArq[0]), contaArq[1]);
                //}
            }
        }


        //[Obsolete("Consulta individualizada no Synergia foi substituída no processo por uma única carga completa da tabela ACCOUNT_SF, para melhoria de performance.")]
        public string[] buscarIdentificadorConta(string[] rows)
        {
            string identificadorConta = string.Empty;
            string identificadorPoD = string.Empty;

            IfxTransaction trans;

            try
            {
                trans = salesForceDaoDes.AbrirTransacao();
            }
            catch (Exception ex)
            {
                throw ex;
            }

            if (tipoExtracao.ToUpper().Trim().Equals("DELTA"))
            {
                try
                {
                    //!dicContaBd.TryGetValue(Convert.ToInt64(rows[(int)DicColuna.col1_7.cuenta_principal]), out identificadorContaDelta))
                    //if (!salesForceDaoDes.consultarConta(trans, rows[(int)DicColuna.col1_7.cuenta_principal].ToString().Trim(), out identificadorContaDelta))
                    if (!salesForceDaoDes.GetExternalIds(trans, rows[(int)DicColuna.col1_7.cuenta_principal].ToString().Trim(), out identificadorConta, out identificadorPoD))
                    {
                        //Console.WriteLine("Conta do cliente" + rows[(int)DicColuna.col1_7.cuenta_principal] + " ainda não gerada");
                        //Gera Id de Conta
                        //rows = gerarIdConta(rows);
                        rows[(int)DicColuna.col1_7.identificador_conta] = string.Empty;
                        rows[(int)DicColuna.col1_7.identificador_pod] = string.Empty;
                    }
                    else
                    {
                        rows = gerarIdConta(rows);
                        rows[(int)DicColuna.col1_7.identificador_conta] = identificadorConta.Trim();
                        rows[(int)DicColuna.col1_7.identificador_pod] = identificadorPoD.Trim();
                    }

                    trans.Commit();
                }
                catch
                {
                    trans.Rollback();
                }
                finally
                {
                    trans.Dispose();
                }
            }
            else
            {
                rows = gerarIdConta(rows);
            }

            return rows;
        }
        public string[] analisaNormalizacaoEndereco(string[] rows)
        {
            string[] padronizacao = normalizarEndereco(rows[(int)DicColuna.col1_7.direccion]);

            if (padronizacao.Length > 0)
            {
                rows[(int)DicColuna.col1_7.nombre_calle] = padronizacao[1].Trim();
                rows[(int)DicColuna.col1_7.tipo_calle] = padronizacao[0].Trim();
                rows[(int)DicColuna.col1_7.complemento] = padronizacao[2].Trim();


                if (tipoExtracao.ToUpper().Trim().Equals("DELTA"))
                {
                    //if (_empresa.Trim().ToUpper().Equals("COELCE"))
                    //{
                    //    long idStreet = dicStreetBd.FirstOrDefault(x => x.Value.nome_rua.Equals(rows[(int)DicColuna.col1_7.nombre_calle]) &&
                    //        x.Value.tipo_logradouro.Equals(rows[(int)DicColuna.col1_7.tipo_calle]) &&
                    //        x.Value.cidade.Equals(rows[(int)DicColuna.col1_7.ciudad]) &&
                    //        x.Value.uf.Equals(rows[(int)DicColuna.col1_7.uf]) &&
                    //        x.Value.comuna.Equals(rows[(int)DicColuna.col1_7.comuna]) &&
                    //        x.Value.regiao.Equals(rows[(int)DicColuna.col1_7.region]) &&
                    //        x.Value.localidad.Equals(rows[(int)DicColuna.col1_7.localidad])).Key;
                    //    //&&x.Value.bairro.Equals(rows[(int)DicColuna.col1_7.barrio])).Key;


                    //    if (!idStreet.Equals(0))
                    //    {
                    //        rows[(int)DicColuna.col1_7.identificador_street] = idStreet.ToString();
                    //        //salvaStreet = false;
                    //    }
                    //    else
                    //    {
                    //        Util.EscreverArquivo(nomeArquivo, "Street " + rows[(int)DicColuna.col1_7.nombre_calle] + ", cidade " + rows[(int)DicColuna.col1_7.ciudad] + ", bairro " + rows[(int)DicColuna.col1_7.barrio] + ", localidad " + rows[(int)DicColuna.col1_7.localidad] + " não encontrada!", ".txt");

                    //        Util.EscreverArquivo(nomeArquivo, "Gerando ID de Street para documento " + rows[(int)DicColuna.col1_7.documento_cliente] + ", cliente " + rows[(int)DicColuna.col1_7.cuenta_principal], ".txt");

                    //        #region Chave de Street
                    //        rows[(int)DicColuna.col1_7.identificador_street] = rows[(int)DicColuna.col1_7.id_empresa] + rows[(int)DicColuna.col1_7.cuenta_principal];
                    //        idStreet = Convert.ToInt64(rows[(int)DicColuna.col1_7.identificador_street]);

                    //        #endregion
                    //        Util.EscreverArquivo(nomeArquivo, "ID gerada: " + rows[(int)DicColuna.col1_7.identificador_street], ".txt");
                    //        Util.EscreverArquivo(nomeArquivo, "Consultando chave de Street gerada no Synergia...", ".txt");

                    //        Street streetDic = null;
                    //        if (dicStreetBd.TryGetValue(Convert.ToInt64(idStreet), out streetDic))
                    //        {
                    //            idStreet = dicStreetBd.Max(x => x.Key) + 1;
                    //            rows[(int)DicColuna.col1_7.identificador_street] = idStreet.ToString();
                    //            //Console.WriteLine("Chave gerada já existe! Será utilizado o ID " + idStreet);
                    //        }

                    //        streetDic = new Street();

                    //        streetDic.id_rua = idStreet;
                    //        streetDic.nome_rua = rows[(int)DicColuna.col1_7.nombre_calle];
                    //        streetDic.tipo_logradouro = rows[(int)DicColuna.col1_7.tipo_calle];
                    //        streetDic.cidade = rows[(int)DicColuna.col1_7.ciudad];
                    //        streetDic.uf = rows[(int)DicColuna.col1_7.uf];
                    //        streetDic.pais = rows[(int)DicColuna.col1_7.pais];
                    //        streetDic.comuna = rows[(int)DicColuna.col1_7.comuna];
                    //        streetDic.regiao = rows[(int)DicColuna.col1_7.region];
                    //        streetDic.calle = rows[(int)DicColuna.col1_7.calle];
                    //        streetDic.localidad = rows[(int)DicColuna.col1_7.localidad];
                    //        streetDic.bairro = rows[(int)DicColuna.col1_7.barrio];

                    //        try
                    //        {
                    //            dicStreetBd.Add(idStreet, streetDic);
                    //            IfxTransaction trans = salesForceDaoDes.AbrirTransacao();
                    //            try
                    //            {
                    //                Util.EscreverArquivo(nomeArquivo, "Inserindo street no banco de dados - Chave: " + rows[(int)DicColuna.col1_7.identificador_street] + " Rua: " + rows[(int)DicColuna.col1_7.nombre_calle] + " Cliente: " + rows[(int)DicColuna.col1_7.cuenta_principal], ".txt");

                    //                salesForceDaoDes.inserirIdStreet(rows, trans);
                    //                trans.Commit();
                    //                trans.Dispose();
                    //            }
                    //            catch (Exception ex)
                    //            {
                    //                Util.EscreverArquivo(nomeArquivo, "Erro ao inserir ID de Street. Dados: " + string.Join("|", rows) + " Erro: " + ex.ToString(), ".txt");
                    //                trans.Rollback();
                    //                trans.Dispose();
                    //            }
                    //        }
                    //        catch (Exception ex)
                    //        {
                    //            Util.EscreverArquivo(nomeArquivo, "Erro ao inserir chave street no dicionário: " + ex.ToString() + " - Dados - Chave: " + rows[(int)DicColuna.col1_7.identificador_street] + " Rua: " + rows[(int)DicColuna.col1_7.nombre_calle] + " Cliente: " + rows[(int)DicColuna.col1_7.cuenta_principal], ".txt");
                    //        }
                    //    }
                    //}
                    //else //AMPLA
                    //{
                        rows[(int)DicColuna.col1_7.identificador_street] = rows[(int)DicColuna.col1_7.id_empresa] + rows[(int)DicColuna.col1_7.cuenta_principal];
                        IfxTransaction trans = salesForceDaoDes.AbrirTransacao();
                        rows[(int)DicColuna.col1_7.identificador_street] = salesForceDaoDes.consultarIdStreet(rows, trans);
                        trans.Commit();
                        trans.Dispose();
                    //}
                }
                else
                {
                    rows[(int)DicColuna.col1_7.identificador_street] = rows[(int)DicColuna.col1_7.id_empresa] + rows[(int)DicColuna.col1_7.cuenta_principal];
                    //salvaStreet = true;
                }

                rows[(int)DicColuna.col1_7.pais_street] = rows[(int)DicColuna.col1_7.pais];
                rows[(int)DicColuna.col1_7.comuna_street] = rows[(int)DicColuna.col1_7.comuna];
                rows[(int)DicColuna.col1_7.barrio_street] = rows[(int)DicColuna.col1_7.barrio];
            }
            else
            {
                rows[(int)DicColuna.col1_7.nombre_calle] = rows[(int)DicColuna.col1_7.direccion];
                salvaStreet = true;
            }
            return rows;
        }

        internal void GetContas(string empresa)
        {
            if (this._accountSf == null)
                this._accountSf = new Dictionary<string, string>();

            string[] linha;
            
            string pathAccountSf = string.Empty;
            
            try
            {
                pathAccountSf = string.Format(ConfigurationManager.AppSettings.Get("pathAccountSf"), empresa);
                //pathAccountSf = string.Format(@"c:\temp\Modif\account_sf_{0}.txt", empresa);
            }
            catch(Exception ex)
            {
                throw new ArgumentException(string.Concat("Falha ao recuperar o arquivo da ACCOUNT_SF.", ex.StackTrace));
            }

            if(!File.Exists(pathAccountSf))
            {
                throw new ArgumentException(string.Format("Arquivo da AccountSF não encontrado em '{0}'", pathAccountSf));
            }

            using (StreamReader f = new StreamReader(pathAccountSf))
            {
                if (f == null)
                    throw new Exception("Arquivo da MODIF não encontrado.");

                while (!f.EndOfStream)
                {
                    try
                    {
                        linha = f.ReadLine().Split('|');
                        if (!this._accountSf.ContainsKey(linha[0]))
                            this._accountSf.Add(linha[0], linha[1]);
                    }
                    catch (Exception ex)
                    { }
                }
            }
            //this._accountSf = salesForceDaoDes.GetContas();
        }
    }
}

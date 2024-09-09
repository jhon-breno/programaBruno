using System;
using System.Threading;
using System.Text;
using System.Xml;
using System.Web.Services;
using System.Web.Services.Protocols;
using System.Collections.Generic;
using System.Net;
using System.Web.Script.Serialization;
using System.Configuration;
using System.IO;
using System.Collections;
using System.Linq;
using SalesforceExtractor.apex;
using SalesforceExtractor.apexMetadata;
using SalesforceExtractor;
using SalesforceExtractor.Entidades;
using SalesforceExtractor.Entidades.Enumeracoes;
using SalesforceExtractor.Dados.SalesForce;
using SalesforceExtractor.Dados;
using System.Globalization;
using Solus.Controle.SalesForce;
using System.Runtime.InteropServices;
using System.Reflection;
using NLog;
using System.Diagnostics;
using SalesforceExtractor.Utils;
using BasicSample.Negocio;

namespace basicSample_cs_p
{
    [ComVisible(true)]
    public class SFDCSchemeBuild
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        public static void Main(string[] args)
        {
            #region trecho para merge de Accounts
            SforceService binding = new SforceService();
            SFDCSchemeBuild schema = new SFDCSchemeBuild();
            TrocaTitularidadeBO neg = new TrocaTitularidadeBO("prod", binding, "2005");
            neg.Autenticar();
            QueryResult qr = null;
            binding.QueryOptionsValue = new SalesforceExtractor.apex.QueryOptions();
            binding.QueryOptionsValue.batchSize = 3;
            binding.QueryOptionsValue.batchSizeSpecified = true;
            List<MergeResult[]> r = new List<MergeResult[]>();

            StreamReader arq = new StreamReader("C:\\Users\\br0118831537\\Desktop\\cnpj.txt");

            while(!arq.EndOfStream)
            {
                String conta = arq.ReadLine();
                String sql = string.Format(@"select id, IdentityNumber__c,externalid__c,name from account where CompanyID__c ='2005' 
                                            and IdentityNumber__c in ('{0}')"
                   , conta);

                Console.WriteLine("Processando documento: " + conta);

                bool bContinue = false;

                try
                {
                    qr = binding.query(sql);

                    bContinue = true;
                }
                catch { }

                string pai = "";
                List<string> lista= new List<string>();

                while (bContinue)
                {
                    if (qr.records == null)
                    {
                        bContinue = false;
                        continue;
                    }

                    if (qr.records.Length > 0)
                    {
                        for (int i = 0; i < qr.records.Length; i++)
                        {
                            SalesforceExtractor.apex.sObject con = qr.records[i];
                            AssetDTO obj = new AssetDTO();

                            
                            if (pai.Equals(string.Empty))
                            {
                                pai = schema.getFieldValue("Id", con.Any);
                            }
                            else
                            {
                                lista.Add(schema.getFieldValue("Id", con.Any));
                            }
                           
                            
                            if (qr.done)
                            {
                                bContinue = false;
                                //Console.WriteLine("Registrou: " + loopCounter);
                            }
                            else
                                qr = binding.queryMore(qr.queryLocator);
                        }
                    }                    
                }

                if (lista.Count > 0)
                {
                    IO.EscreverArquivo("C:\\temp\\quemnao.txt", conta + "\n");
                    r.Add(SalesforceDAO.MergeContas(pai, lista, ref binding));
                }
            }
            




    /*try
    {
        List<MergeResult[]> r = new List<MergeResult[]>();
        r.Add(SalesforceDAO.MergeContas("0017Z00001F0G3iQAF", new List<string> { "00136000014HRApAAO" }, ref bind));
        r.Add(SalesforceDAO.MergeContas("00136000014HDtVAAW", new List<string> { "00136000014HIdRAAW", "00136000014cRSaAAM", "00136000014HDzhAAG" }, ref bind));
        r.Add(SalesforceDAO.MergeContas("00136000014UKIwAAO", new List<string> { "0013600001215ZSAAY" }, ref bind));
        r.Add(SalesforceDAO.MergeContas("00136000014atqbAAA", new List<string> { "00136000014HDuUAAW" }, ref bind));
        r.Add(SalesforceDAO.MergeContas("00136000014HEEFAA4", new List<string> { "0017Z00001F0EvKQAV" }, ref bind));
        r.Add(SalesforceDAO.MergeContas("00136000014HIB2AAO", new List<string> { "0017Z00001F0EvkQAF" }, ref bind));
        r.Add(SalesforceDAO.MergeContas("00136000012DFR0AAO", new List<string> { "00136000014aNbFAAU" }, ref bind));
        r.Add(SalesforceDAO.MergeContas("00136000014HLbUAAW", new List<string> { "00136000014VBBoAAO" }, ref bind));
        r.Add(SalesforceDAO.MergeContas("0011o00001hL5w4AAC", new List<string> { "0011o00001hL5x6AAC" }, ref bind));
        r.Add(SalesforceDAO.MergeContas("00136000014HFVbAAO", new List<string> { "00136000014dnb2AAA" }, ref bind));
        r.Add(SalesforceDAO.MergeContas("0011o00001hL62XAAS", new List<string> { "0011o00001hL61hAAC" }, ref bind));
        r.Add(SalesforceDAO.MergeContas("00136000014HGyMAAW", new List<string> { "00136000014fjiTAAQ" }, ref bind));
        r.Add(SalesforceDAO.MergeContas("00136000014gpqxAAA", new List<string> { "0011o00001jti5pAAA" }, ref bind));
        r.Add(SalesforceDAO.MergeContas("0017Z00001F0G3gQAF", new List<string> { "00136000014HECyAAO", "0017Z00001F0G3xQAF", "0017Z00001F0G30QAF" }, ref bind));
        r.Add(SalesforceDAO.MergeContas("00136000014HKnLAAW", new List<string> { "0017Z00001F0Ex6QAF" }, ref bind));
        r.Add(SalesforceDAO.MergeContas("00136000014c986AAA", new List<string> { "0017Z00001F0HLTQA3", "0017Z00001F0G1wQAF", "00136000014HDWcAAO" }, ref bind));
        r.Add(SalesforceDAO.MergeContas("0017Z00001F0HKoQAN", new List<string> { "00136000014HEYBAA4", "0017Z00001F0HLwQAN" }, ref bind));
        r.Add(SalesforceDAO.MergeContas("00136000014HEX1AAO", new List<string> { "00136000014ZwKyAAK" }, ref bind));
        r.Add(SalesforceDAO.MergeContas("00136000014frUJAAY", new List<string> { "00136000014HS9gAAG" }, ref bind));
        r.Add(SalesforceDAO.MergeContas("00136000014cjrkAAA", new List<string> { "00136000014HDYQAA4" }, ref bind));
        r.Add(SalesforceDAO.MergeContas("0017Z00001F0GdQQAV", new List<string> { "00136000014HF2uAAG" }, ref bind));
        r.Add(SalesforceDAO.MergeContas("00136000014HEFKAA4", new List<string> { "00136000014a57cAAA" }, ref bind));
        r.Add(SalesforceDAO.MergeContas("00136000014HR9YAAW", new List<string> { "00136000014TERuAAO" }, ref bind));
        r.Add(SalesforceDAO.MergeContas("00136000014HHjzAAG", new List<string> { "0017Z00001F0G3AQAV" }, ref bind));
        r.Add(SalesforceDAO.MergeContas("00136000014UxmCAAS", new List<string> { "00136000014HHkzAAG" }, ref bind));
        r.Add(SalesforceDAO.MergeContas("00136000014HOhwAAG", new List<string> { "00136000014eK93AAE" }, ref bind));
        r.Add(SalesforceDAO.MergeContas("00136000014HIdoAAG", new List<string> { "0017Z00001F0EvSQAV" }, ref bind));
        r.Add(SalesforceDAO.MergeContas("0013600001209WVAAY", new List<string> { "00136000014TwZEAA0" }, ref bind));
        r.Add(SalesforceDAO.MergeContas("00136000014UJPVAA4", new List<string> { "001360000120q1UAAQ" }, ref bind));
        r.Add(SalesforceDAO.MergeContas("0017Z00001F0GcFQAV", new List<string> { "00136000014HDh2AAG" }, ref bind));
        r.Add(SalesforceDAO.MergeContas("00136000014HXabAAG", new List<string> { "00136000014VNZwAAO" }, ref bind));
        r.Add(SalesforceDAO.MergeContas("00136000014b3vhAAA", new List<string> { "00136000014HSySAAW" }, ref bind));
        r.Add(SalesforceDAO.MergeContas("00136000014HEXuAAO", new List<string> { "00136000014fpHdAAI" }, ref bind));
        r.Add(SalesforceDAO.MergeContas("00136000014HGG0AAO", new List<string> { "00136000014Uc5CAAS" }, ref bind));
        r.Add(SalesforceDAO.MergeContas("00136000014HFgNAAW", new List<string> { "00136000014bxteAAA" }, ref bind));
        r.Add(SalesforceDAO.MergeContas("0017Z00001F0IMcQAN", new List<string> { "0017Z00001F0IMnQAN", "0017Z00001F0IMmQAN", "00136000014HDY6AAO" }, ref bind));
        r.Add(SalesforceDAO.MergeContas("0017Z00001F0IN7QAN", new List<string> { "0017Z00001F0HLkQAN", "00136000014HG9UAAW" }, ref bind));
        r.Add(SalesforceDAO.MergeContas("00136000014HJwTAAW", new List<string> { "0017Z00001F0HM3QAN" }, ref bind));
        r.Add(SalesforceDAO.MergeContas("0017Z00001F0ILlQAN", new List<string> { "0017Z00001F0ILkQAN", "0017Z00001F0ILiQAN", "0017Z00001F0ILjQAN" }, ref bind));
        r.Add(SalesforceDAO.MergeContas("0017Z00001F0GcqQAF", new List<string> { "0017Z00001F0G3IQAV" }, ref bind));
        r.Add(SalesforceDAO.MergeContas("0011o00001hL60HAAS", new List<string> { "0011o00001hL5zJAAS" }, ref bind));
        r.Add(SalesforceDAO.MergeContas("00136000014HMdRAAW", new List<string> { "00136000014ZbPUAA0" }, ref bind));
        r.Add(SalesforceDAO.MergeContas("00136000014dhR3AAI", new List<string> { "0011o00001ULGqJAAX" }, ref bind));
        r.Add(SalesforceDAO.MergeContas("00136000014HR92AAG", new List<string> { "00136000014UUAOAA4" }, ref bind));
        r.Add(SalesforceDAO.MergeContas("00136000014HFfbAAG", new List<string> { "00136000014bWxwAAE" }, ref bind));
        r.Add(SalesforceDAO.MergeContas("00136000014HPuwAAG", new List<string> { "00136000014YxYyAAK" }, ref bind));
        r.Add(SalesforceDAO.MergeContas("0011o00001hL5tjAAC", new List<string> { "0011o00001hL5ujAAC" }, ref bind));
        r.Add(SalesforceDAO.MergeContas("00136000014HDaFAAW", new List<string> { "00136000014HLcDAAW" }, ref bind));
        r.Add(SalesforceDAO.MergeContas("00136000014Zfn8AAC", new List<string> { "00136000014HFhRAAW" }, ref bind));
        r.Add(SalesforceDAO.MergeContas("0017Z00001F0EwNQAV", new List<string> { "00136000014HHimAAG" }, ref bind));
        r.Add(SalesforceDAO.MergeContas("0011o00001T83nQAAR", new List<string> { "00136000014asVjAAI" }, ref bind));
        r.Add(SalesforceDAO.MergeContas("00136000014YyEvAAK", new List<string> { "001360000129qREAAY" }, ref bind));
        r.Add(SalesforceDAO.MergeContas("0011o00001hL61eAAC", new List<string> { "0011o00001hL62UAAS" }, ref bind));
        r.Add(SalesforceDAO.MergeContas("0011o00001hL5uuAAC", new List<string> { "0011o00001hL5tuAAC" }, ref bind));
        r.Add(SalesforceDAO.MergeContas("0011o00001hL5veAAC", new List<string> { "0011o00001hL5wgAAC" }, ref bind));
        r.Add(SalesforceDAO.MergeContas("0011o00001hL5z1AAC", new List<string> { "0011o00001hL5yAAAS" }, ref bind));
        r.Add(SalesforceDAO.MergeContas("0011o00001hL5xlAAC", new List<string> { "0011o00001hL5ycAAC" }, ref bind));
        r.Add(SalesforceDAO.MergeContas("0011o00001hL5xCAAS", new List<string> { "0011o00001hL5wAAAS" }, ref bind));
        r.Add(SalesforceDAO.MergeContas("0011o00001hL5ziAAC", new List<string> { "0011o00001hL60gAAC" }, ref bind));
        r.Add(SalesforceDAO.MergeContas("0011o00001hL5utAAC", new List<string> { "0011o00001hL5ttAAC" }, ref bind));
        r.Add(SalesforceDAO.MergeContas("0011o00001hL5xJAAS", new List<string> { "0011o00001hL5wHAAS" }, ref bind));
        r.Add(SalesforceDAO.MergeContas("0011o00001hL5z5AAC", new List<string> { "0011o00001hL5yEAAS" }, ref bind));
        r.Add(SalesforceDAO.MergeContas("0011o00001hL5vFAAS", new List<string> { "0011o00001hL5uFAAS" }, ref bind));
        r.Add(SalesforceDAO.MergeContas("0011o00001hL61VAAS", new List<string> { "0011o00001hL62LAAS" }, ref bind));
        r.Add(SalesforceDAO.MergeContas("0011o00001hL613AAC", new List<string> { "0011o00001hL605AAC" }, ref bind));
        r.Add(SalesforceDAO.MergeContas("0011o00001hL5vTAAS", new List<string> { "0011o00001hL5wVAAS" }, ref bind));
        r.Add(SalesforceDAO.MergeContas("00136000014HEZRAA4", new List<string> { "0017Z00001F0G26QAF" }, ref bind));
        r.Add(SalesforceDAO.MergeContas("0011o00001hL5tZAAS", new List<string> { "0011o00001hL5uZAAS" }, ref bind));
        r.Add(SalesforceDAO.MergeContas("0011o00001hL5vmAAC", new List<string> { "0011o00001hL5woAAC" }, ref bind));
        r.Add(SalesforceDAO.MergeContas("0011o00001hL5tbAAC", new List<string> { "0011o00001hL5ubAAC" }, ref bind));
        r.Add(SalesforceDAO.MergeContas("0011o00001hL5wqAAC", new List<string> { "0011o00001hL5voAAC" }, ref bind));
        r.Add(SalesforceDAO.MergeContas("00136000014HDWeAAO", new List<string> { "0017Z00001F0ILtQAN", "0017Z00001F0ILrQAN", "0017Z00001F0ILcQAN" }, ref bind));
        r.Add(SalesforceDAO.MergeContas("00136000014HDz9AAG", new List<string> { "0017Z00001F0Ev8QAF" }, ref bind));
        r.Add(SalesforceDAO.MergeContas("00136000014VlK6AAK", new List<string> { "001360000128DbgAAE" }, ref bind));
        r.Add(SalesforceDAO.MergeContas("00136000014HEH3AAO", new List<string> { "00136000014ZUB6AAO" }, ref bind));
        r.Add(SalesforceDAO.MergeContas("00136000014cNFuAAM", new List<string> { "00136000012RGdiAAG" }, ref bind));
        r.Add(SalesforceDAO.MergeContas("00136000014dia9AAA", new List<string> { "001360000127w3sAAA" }, ref bind));
        r.Add(SalesforceDAO.MergeContas("00136000014HDZnAAO", new List<string> { "0017Z00001F0HN8QAN" }, ref bind));
        r.Add(SalesforceDAO.MergeContas("00136000014HXaaAAG", new List<string> { "00136000014aMB5AAM" }, ref bind));

        int cont = 0;
        foreach(MergeResult[] result in r)
        {
            cont++;
            if (result.FirstOrDefault().success)
                continue;

            Console.WriteLine(cont.ToString() + "\t" + result.FirstOrDefault().id);
        }
    }
    catch (Exception ex)
    { }*/

    Console.ReadLine();
            return;
            #endregion

            //SalesforceExtractor.WsCorte.SalesForceEmergenciaSoapClient ws = new SalesforceExtractor.WsCorte.SalesForceEmergenciaSoapClient();
            //SalesforceExtractor.WsCorte.Corte aaa = ws.GetCorteEnergia2("1666875", "8", "2003", "");
            //return;

            Dictionary<string, ParameterInfo[]> lstMetodos = new Dictionary<string, ParameterInfo[]>();
            Thread.CurrentThread.CurrentCulture = new CultureInfo("pt-BR");
            //SforceService binding = new SforceService();

            Type T = typeof(SFDCSchemeBuild);
            //Type T = typeof(Negocio);
            lstMetodos = informarOpcoesDisponiveis(T);

            bool validado = false;
            int opcaoInt = 0;
            string opcao = string.Empty;
            while (!validado)
            {
                opcao = Console.ReadLine();
                if (string.IsNullOrWhiteSpace(opcao))
                    return;

                validado = Int32.TryParse(opcao.Split(' ')[0], out opcaoInt) && opcaoInt > 0 && opcaoInt <= lstMetodos.Count;
                if (!validado)
                    Console.WriteLine("Opção inválida.");
            }
            
            //Recupera os parâmetros
            List<object> lstParam = new List<object>();
            StringBuilder auxParam = new StringBuilder();
            for (int i = 0; i < opcao.Split(' ').Count(); i++ )
            {
                if (i == 0) continue;
                lstParam.Add(opcao.Split(' ')[i]);
            }

            lstParam.Add(binding);

            try
            {
                var obj = Activator.CreateInstance(typeof(Negocio), new object[] { lstParam[0], new SforceService(), "2003" });
                var result = (MethodInfo)T
                .GetMethod(lstMetodos.ElementAt(opcaoInt - 1).Key, BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public);
                var sss = result.Invoke(obj, lstParam.ToArray());
            }
            catch (Exception ex)
            {
                Console.Write(string.Format("ERRO: {0} {1}", ex.Message, ex.StackTrace));
            }

            Console.WriteLine(string.Format("\n----------- Processo Finalizado em {0} -----------", DateTime.Now.ToShortTimeString()));
            Console.WriteLine("Tecle para sair.");

            if (args != null && args.Where(a => a == "r").Count() > 0)
                Console.ReadKey();
        }


        private static Dictionary<string, ParameterInfo[]> informarOpcoesDisponiveis(Type T)
        {
            Dictionary<string, ParameterInfo[]> result = new Dictionary<string, ParameterInfo[]>();
            var metodosExpostos = T.GetMethods(BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public);
            StringBuilder sbParam = new StringBuilder();

            foreach (MethodInfo m in metodosExpostos)
            {
                if (m.GetCustomAttributes(true).Where(t => t.GetType().Name.Equals("ComVisibleAttribute")).Count() > 0)
                    result.Add(m.Name, m.GetParameters());
            }

            Console.WriteLine("Escolha dentre as opções disponíveis ou 'Enter' para sair.\n");

            int cont = 1;
            foreach (string k in result.Keys)
            {
                sbParam.Clear();
                Console.Write(string.Format("{0} - {1}", (cont++).ToString().PadLeft(3, ' '), k));

                Console.Write(result[k].Length > 0 ? " (" : string.Empty);
                foreach (ParameterInfo p in result[k])
                {
                    sbParam.Append(sbParam.Length > 0 ? ", " : string.Empty);
                    sbParam.AppendFormat("{0} [{1}]", p.Name, p.ParameterType.Name);
                }
                Console.Write(string.Format((sbParam.Length > 0 ? sbParam.ToString() : string.Empty)));
                Console.Write(result[k].Length > 0 ? ")" : string.Empty);

                Console.WriteLine();
            }

            return result;
        }


        [ComVisible(true)]
        internal static void higienizarContas(string ambiente, string arquivo, SforceService binding)
        {
            using (Negocio exec = new Negocio(ambiente, binding, "2003"))
            {
                //string dir = "C:\\!adl\\Ayesa\\EPT_Suporte\\180604 Salesforce\\181120 Higienizacao Account CE\\";
                
                //toda a base de clientes válidos
                //arquivo = "C:\\!adl\\Ayesa\\EPT_Suporte\\180604 Salesforce\\181120 Higienizacao Account CE\\Full\\Carga00.txt";

                //somente contas que possuam documentos cadastrados a mais de 1 conta
                //arquivo = "C:\\!adl\\Ayesa\\EPT_Suporte\\180604 Salesforce\\181120 Higienizacao Account CE\\190111 UAT\\Base_clientes_doc_duplicados_uat.txt";
                arquivo = ConfigurationManager.AppSettings["local_arquivo"];

                using (Arquivo arq = new Arquivo(arquivo, '|'))
                {
                    exec.HigienizarContas(arq);
                    //exec.HigienizarContasCorretivo3(arq);
                    //exec.HigienizarContasCorretivo2(arq);
                    //exec.HigienizarContasCorretivo(arq);
                }
            }
        }



        /// <summary>
        /// Identificar as Accounts que estão cadastradas mas não estão associadas a nenhum Account/Asset.
        /// </summary>
        /// <param name="arquivo"></param>
        /// <param name="binding"></param>
        [ComVisible(true)]
        internal static void HigienizarContasAssets(string ambiente, string arquivo, SforceService binding)
        {
            using (Negocio exec = new Negocio(ambiente, binding, "2003"))
            {
                //lista de AccountIds que estão causando erro de duplicidade pelo ETL da Delloite
                arquivo = @"C:\!adl\Ayesa\EPT_Suporte\180604 Salesforce\181120 Higienizacao Account CE\190213 PROD\Rechazos\account_ids.txt";

                using (Arquivo arq = new Arquivo(arquivo, '|'))
                {
                    exec.HigienizarContasAssets(arq);
                }
            }
        }


        /// <summary>
        /// Retorna o total de registros de uma Entidade do Salesforce parametrizada.
        /// </summary>
        /// <param name="entidades">Nome dos objetos no Salesforce, que se deseja obter o total de registros, separados por PIPE.</param>
        /// <param name="binding">Instância do serviço Salesforce a ser instanciado</param>
        [ComVisible(true)]
        public static void GetTotalEntidade(string ambiente, string entidades, SforceService binding)
        {
            using (Negocio exec = new Negocio(ambiente, binding, "2003"))
            {
                foreach (string entidade in entidades.Split('|'))
                {
                    exec.GetTotalEntidade(entidade);
                }
            }
        }


        /// <summary>
        /// Corrige o cadastro de Assest, eliminando as contas duplicadas e/ou com ExternalId Invalidos
        /// </summary>
        /// <param name="binding"></param>
        [ComVisible(true)]
        public static void corrigirAssets(string ambiente, SforceService binding)
        {
            Negocio exec = new Negocio(ambiente, binding, null);
            exec.CorrigirAssets();
        }




        /// <summary>
        /// Processo para ajustar no Salesforce os documentos que estao fora do padrao: CPF = 11 caracteres, CNPJ = 14 caracteres
        /// </summary>
        /// <param name="binding"></param>
        [ComVisible(true)]
        public static void higienizarDocumento(string ambiente, string arquivo, SforceService binding)
        {
            //arquivo = @"C:\!adl\Ayesa\EPT_Suporte\180604 Salesforce\190219 Higienizar Docs 15+\docs_CE_17+.txt";
            //arquivo = @"C:\!adl\Ayesa\EPT_Suporte\180604 Salesforce\190219 Higienizar Docs 15+\docs_RJ_17+_1.txt";
            arquivo = @"C:\!adl\Ayesa\EPT_Suporte\180604 Salesforce\190219 Higienizar Docs 15+\docs_RJ_17+_2.txt";
            Negocio exec = new Negocio(ambiente, binding, null);
            using (Arquivo arq = new Arquivo(arquivo, '|'))
            {
                exec.HigienizarDocumentos(arq);
            }
        }


        
        [ComVisible(true)]
        public static void eliminarDuplicidadesSalesforceUra(string ambiente, string arquivo, SforceService binding)
        {
            List<Arquivo> lstArquivos = new List<Arquivo>();
            //lstArquivos.Add(new Arquivo("C:\\!adl\\Ayesa\\EPT_Suporte\\181211 Ura3 WsTecnica\\Atividaedes_Duplicadas\\NovoRelatorio\\Atends_RJ_201811 Novembro02.txt", '|'));
            //lstArquivos.Add(new Arquivo("C:\\!adl\\Ayesa\\EPT_Suporte\\181211 Ura3 WsTecnica\\Atividaedes_Duplicadas\\NovoRelatorio\\Atends_CE_201811 Novembro03.txt", '|'));
            //lstArquivos.Add(new Arquivo("C:\\!adl\\Ayesa\\EPT_Suporte\\181211 Ura3 WsTecnica\\Atividaedes_Duplicadas\\NovoRelatorio\\Atends_RJ_201810_Outubro.txt", '|'));
            //lstArquivos.Add(new Arquivo("C:\\!adl\\Ayesa\\EPT_Suporte\\181211 Ura3 WsTecnica\\Atividaedes_Duplicadas\\NovoRelatorio\\Atends_RJ_201811_Novembro.txt", '|'));
            //lstArquivos.Add(new Arquivo("C:\\!adl\\Ayesa\\EPT_Suporte\\181211 Ura3 WsTecnica\\Atividaedes_Duplicadas\\NovoRelatorio\\Atends_RJ_201812_Dezembro.txt", '|'));
            //lstArquivos.Add(new Arquivo("C:\\!adl\\Ayesa\\EPT_Suporte\\181211 Ura3 WsTecnica\\Atividaedes_Duplicadas\\NovoRelatorio\\Atend_RJ_201807_Julho.txt", '|'));
            //lstArquivos.Add(new Arquivo("C:\\!adl\\Ayesa\\EPT_Suporte\\181211 Ura3 WsTecnica\\Atividaedes_Duplicadas\\NovoRelatorio\\Atend_RJ_201808_Agosto.txt", '|'));
            //lstArquivos.Add(new Arquivo("C:\\!adl\\Ayesa\\EPT_Suporte\\181211 Ura3 WsTecnica\\Atividaedes_Duplicadas\\NovoRelatorio\\Atend_RJ_201809_Setembro.txt", '|'));
            //lstArquivos.Add(new Arquivo("C:\\!adl\\Ayesa\\EPT_Suporte\\181211 Ura3 WsTecnica\\Atividaedes_Duplicadas\\NovoRelatorio\\Atend_RJ_201804_Abril.txt", '|'));
            //lstArquivos.Add(new Arquivo("C:\\!adl\\Ayesa\\EPT_Suporte\\181211 Ura3 WsTecnica\\Atividaedes_Duplicadas\\NovoRelatorio\\Atend_RJ_201805_Maio.txt", '|'));
            //lstArquivos.Add(new Arquivo("C:\\!adl\\Ayesa\\EPT_Suporte\\181211 Ura3 WsTecnica\\Atividaedes_Duplicadas\\NovoRelatorio\\Atend_RJ_201806_Junho.txt", '|'));
            //lstArquivos.Add(new Arquivo("C:\\!adl\\Ayesa\\EPT_Suporte\\181211 Ura3 WsTecnica\\Atividaedes_Duplicadas\\NovoRelatorio\\_Atends_RJ_201801-03.txt", '|'));
            lstArquivos.Add(new Arquivo("C:\\!adl\\Ayesa\\EPT_Suporte\\181211 Ura3 WsTecnica\\Atividaedes_Duplicadas\\NovoRelatorio\\Atend_CE_201801-03.txt", '|'));
            lstArquivos.Add(new Arquivo("C:\\!adl\\Ayesa\\EPT_Suporte\\181211 Ura3 WsTecnica\\Atividaedes_Duplicadas\\NovoRelatorio\\Atend_CE_201804-06.txt", '|'));
            lstArquivos.Add(new Arquivo("C:\\!adl\\Ayesa\\EPT_Suporte\\181211 Ura3 WsTecnica\\Atividaedes_Duplicadas\\NovoRelatorio\\Atend_CE_201807-09.txt", '|'));
            lstArquivos.Add(new Arquivo("C:\\!adl\\Ayesa\\EPT_Suporte\\181211 Ura3 WsTecnica\\Atividaedes_Duplicadas\\NovoRelatorio\\Atend_CE_201810_Out.txt", '|'));
            lstArquivos.Add(new Arquivo("C:\\!adl\\Ayesa\\EPT_Suporte\\181211 Ura3 WsTecnica\\Atividaedes_Duplicadas\\NovoRelatorio\\Atend_CE_201812_Dez.txt", '|'));


            Negocio exec = new Negocio(ambiente, binding, null);
            foreach(Arquivo arq in lstArquivos)
            { 
                exec.EliminarDuplicidadesSalesforceUra(arq);
            }
        }


        [ComVisible(true)]
        public static void ConsultarAccountsPorExternalId(string ambiente, SforceService binding)
        {
            Negocio exec = new Negocio(ambiente, binding, null);
            exec.ConsultarAccountsPorExternalId();
        }


        /// <param name="binding"></param>
        [ComVisible(true)]
        public static void CarregarB2Win(string ambiente, string codigosEmpresa, string lote, string caminho, string tipoCliente, string temCabecalho, SforceService binding)
        {
            foreach (string empresa in codigosEmpresa.Split(','))
            {
                using (Negocio exec = new Negocio(ambiente, binding, empresa))
                {
                    exec.CarregarB2Win(empresa, caminho, Int32.Parse(lote), "gb".Equals(tipoCliente.ToLower()) ? TipoCliente.GB : TipoCliente.GA, Convert.ToBoolean(Int32.Parse(temCabecalho)));
                }
            }
        }


        /// <param name="binding"></param>
        [ComVisible(true)]
        public static void CarregarB2WinPorCliente(string ambiente, string codigosEmpresa, string lote, string numeroCliente, string tipoCliente, string temCabecalho, SforceService binding)
        {
            foreach (string empresa in codigosEmpresa.Split(','))
            {
                using (Negocio exec = new Negocio(ambiente, binding, empresa))
                {
                    exec.CarregarB2Win(empresa, numeroCliente, Int32.Parse(lote), "gb".Equals(tipoCliente.ToLower()) ? TipoCliente.GB : TipoCliente.GA, Convert.ToBoolean(Int32.Parse(temCabecalho)));
                }
            }
        }


        [ComVisible(true)]
        public static void RemoverAssetsDuplicados(string ambiente, string codigosEmpresa, string arquivoEntrada, string temCabecalho, SforceService binding)
        {
            foreach (string empresa in codigosEmpresa.Split(','))
            {
                using (Negocio exec = new Negocio(ambiente, binding, empresa))
                {
                    //10 prod 2003 C:\temp\200719-assets.txt" false
                    exec.ExcluirAssetsDuplicadosPorId(arquivoEntrada, Boolean.Parse(temCabecalho));
                }
            }
        }


        [ComVisible(true)]
        public static void ExtrairZonasSynergia(string arquivoEntrada, string temCabecalho, SforceService binding)
        {
            //11 C:\Users\adm\Downloads\clientes_zona.txt false
            Arquivo arqEntrada = new Arquivo(arquivoEntrada, ',', Boolean.Parse(temCabecalho));
            ConsultarSynergia da = new ConsultarSynergia("2005", TipoCliente.GB);
            Dictionary<string, string> zonas = da.GetZonasPorCliente(arqEntrada);
        }

        [ComVisible(true)]
        public static void ExtrairEmergenciaSynergia(string arquivoEntrada, string temCabecalho, SforceService binding)
        {
            Arquivo arqEntrada = new Arquivo(arquivoEntrada, ',', Boolean.Parse(temCabecalho));
            ConsultarSynergia da = new ConsultarSynergia("2005", TipoCliente.GB);
            
            //Consulta dos atendimentos por [protocolo]
            //12 C:\temp\ouvidoria\protocolosga.txt false
            //12 C:\temp\ouvidoria\protocolos.txt false
            Dictionary<string, string> zonas = da.GetAtendimentosEmergencia(arqEntrada);
            Debugger.Break();
            //Consulta apenas da classe e estado por [cliente]
            //12 C:\temp\ouvidoria\Clientes23kClasse.txt false
            //Dictionary<string, string> clientesClasse = da.GetClassesSynergia(arqEntrada);

            //Extrai menor aviso por cliente
            //12 C:\temp\ouvidoria\faltantesTecnica.txt false
            //Dictionary<string, string> clientesClasse = da.ExtrairMenorAviso(arqEntrada);


            //12 C:\temp\ouvidoria\faltantesTecnica.txt false
            Arquivo ouv = new Arquivo(@"D:\!adl\Ayesa\EPT_Suporte\200825 Relatorio Emergencia Auditoria\Nivel2\nivel2_protocolos_ouvidoria_revisado.txt",',', false);
            Arquivo tec = new Arquivo(@"D:\!adl\Ayesa\EPT_Suporte\200825 Relatorio Emergencia Auditoria\Nivel2\baseTecnica.txt",',',true);
            Dictionary<string, string> clientesClasse = da.ExtrairInterrupcaoIdeal(ouv,tec);
            
        }

        #region Métodos Privados

        /// <summary>
        /// 
        /// </summary>
        /// <param name="binding"></param>
        [ComVisible(true)]
        public static void gerarRelatorioIdContatoSalesforce(string ambiente, SforceService binding)
        {   
            Negocio exec = new Negocio(ambiente, binding, "2003");
            exec.GerarRelatorioIdContatoSalesforce();
        }



        /// <summary>
        /// 
        /// </summary>
        [ComVisible(true)]
        public void relatoriosContatosDuplicados(string ambiente, SforceService binding)
        {
            Negocio exec = new Negocio(ambiente, binding, "2003");

            string temp = "C:\\!adl\\Ayesa\\EPT_Suporte\\180604 Salesforce\\180616 Migração Salesforce\\02 Duplicidade SF\\";
            using (Arquivo arq = new Arquivo(temp, "407632_1", "TXT", '|'))
            {
                using (Arquivo arqSaida = new Arquivo(temp, "407632_1_SAIDA", "TXT"))
                {
                    if (File.Exists(arqSaida.CaminhoCompleto))
                        File.Delete(arqSaida.CaminhoCompleto);

                    exec.GerarRelatorioContatosDuplicados(arq, arqSaida);
                }
            }
        }
        

        /// <summary>
        /// 
        /// </summary>
        [ComVisible(true)]
        public static void atualizarRedesSociais(string ambiente, SforceService binding)
        {
            Negocio exec = new Negocio(ambiente, binding, null);
            var nomeArquivo = string.Empty;

            nomeArquivo = "C:\\!adl\\Ayesa\\EPT_Suporte\\180604 Salesforce\\_old\\180619 Redes Sociais\\";
            using (Arquivo arq = new Arquivo(nomeArquivo, "CE_TWITTER", "TXT", '|'))
            {
                exec.AtualizarRedeSocial(arq);
            }

            nomeArquivo = "C:\\!adl\\Ayesa\\EPT_Suporte\\180604 Salesforce\\_old\\180619 Redes Sociais\\";
            using (Arquivo arq = new Arquivo(nomeArquivo, "CE_FACEBOOK", "TXT", '|'))
            {
                exec.AtualizarRedeSocial(arq);
            }

            nomeArquivo = "C:\\!adl\\Ayesa\\EPT_Suporte\\180604 Salesforce\\_old\\180619 Redes Sociais\\";
            using (Arquivo arq = new Arquivo(nomeArquivo, "RJ_TWITTER", "TXT", '|'))
            {
                exec.AtualizarRedeSocial(arq);
            }

            nomeArquivo = "C:\\!adl\\Ayesa\\EPT_Suporte\\180604 Salesforce\\_old\\180619 Redes Sociais\\";
            using (Arquivo arq = new Arquivo(nomeArquivo, "RJ_FACEBOOK", "TXT", '|'))
            {
                exec.AtualizarRedeSocial(arq);
            }
        }


        /// <summary>
        /// Ingressa medidores no Salesforce. (objeto Device__c)
        /// </summary>
        [ComVisible(true)]
        public static void ingressarMedidores(string ambiente, SforceService binding)
        {
            Negocio exec = new Negocio(ambiente, binding, null);
            var nomeArquivo = string.Empty;

            nomeArquivo = "C:\\!adl\\Ayesa\\EPT_Suporte\\180604 Salesforce\\180926 Insert Device SF\\";
            using (Arquivo arq = new Arquivo(nomeArquivo, "Medidores", "TXT", '|'))
            {
                exec.IngressarDevices(arq);
            }
        }


        /// <summary>
        /// Ingressa Billings
        /// </summary>
        [ComVisible(true)]
        public static void ingressarBilling(string ambiente, SforceService binding)
        {
            Negocio exec = new Negocio(ambiente, binding, null);
            var nomeArquivo = string.Empty;

            nomeArquivo = @"C:\!adl\Ayesa\EPT_Suporte\180604 Salesforce\181024 Ingressar Billings\";
            using (Arquivo arq = new Arquivo(nomeArquivo, "Billings", "TXT", '|'))
            {
                exec.IngressarBillings(arq);
            }
        }


        /// <summary>
        /// Ingressa Talends no Salesforce. (objeto CNT_Talend_File__c)
        /// </summary>
        [ComVisible(true)]
        public static void IngressarTalends(string ambiente, SforceService binding)
        {
            Negocio exec = new Negocio(ambiente, binding, null);
            var nomeArquivo = string.Empty;

            nomeArquivo = "C:\\temp\\";
            using (Arquivo arq = new Arquivo(nomeArquivo, "Talends", "TXT", '|'))
            {
                exec.IngressarTalends(arq);
            }
        }


        /// <summary>
        /// Gera uma saída de dados do Salesforce, conforme o tipo de relatório solicitado.
        /// </summary>
        /// <param name="entrada">Parâmetros obrigatórios: </param>
        [ComVisible(true)]
        public static void GerarRelatorioEmergencia(string ambiente, ParRelatorioEmergencia entrada)
        {
            SforceService binding = new SforceService();
            Negocio exec = new Negocio(ambiente, binding, null);

            var nomeArquivo = string.Concat("\"", entrada.caminhoArquivoSaida, "\"");
            using (Arquivo arq = new Arquivo(nomeArquivo, ';'))
            {
                exec.IngressarTalends(arq);
            }
        }

        [ComVisible(true)]
        public static void AtualizarAtividadesCaso(string ambiente, SforceService binding)
        {
            Negocio exec = new Negocio(ambiente, binding, null);

            //var nomeArquivo = @"C:\!adl\Ayesa\EPT_Suporte\181211 Ura3 WsTecnica\190219 IdCaso Vazios\set-dez-2018-emerg.txt";
            //var nomeArquivo = @"C:\!adl\Ayesa\EPT_Suporte\181211 Ura3 WsTecnica\190219 IdCaso Vazios\2018-01-JAN_emerg.txt";
            var nomeArquivo = @"C:\!adl\Ayesa\EPT_Suporte\181211 Ura3 WsTecnica\190219 IdCaso Vazios\set-dez-2018-emerg_2.txt";

            using (Arquivo arq = new Arquivo(nomeArquivo, '|'))
            {
                exec.AtualizarAtividadesIdCaso(arq);
            }
        }


        [ComVisible(true)]
        public static void AtualizarCasosEmergenciaSemAviso(string ambiente, SforceService binding)
        {
            Negocio exec = new Negocio(ambiente, binding, null);

            //var nomeArquivo = @"C:\!adl\Ayesa\EPT_Suporte\181211 Ura3 WsTecnica\02 Restaurar DataLake\190119\Avisos_2018_Coelce.txt";
            //var nomeArquivo = @"C:\!adl\Ayesa\EPT_Suporte\181211 Ura3 WsTecnica\02 Restaurar DataLake\190220\Avisos_Dezembro.txt";
            //var nomeArquivo = @"C:\!adl\Ayesa\EPT_Suporte\181211 Ura3 WsTecnica\02 Restaurar DataLake\190402\Tecnica-CE-201903-2.txt";
            var nomeArquivo = @"C:\!adl\Ayesa\EPT_Suporte\181211 Ura3 WsTecnica\02 Restaurar DataLake\190402\Tecnica-RJ-201903.txt";
            nomeArquivo = @"C:\!adl\Ayesa\EPT_Suporte\181211 Ura3 WsTecnica\02 Restaurar DataLake\190508\201904-CE.txt";
            nomeArquivo = @"C:\!adl\Ayesa\EPT_Suporte\181211 Ura3 WsTecnica\02 Restaurar DataLake\190508\201904-RJ.txt";
            using (Arquivo arq = new Arquivo(nomeArquivo, '|'))
            {
                exec.AtualizarCasosEmergenciaSemAviso(arq);
            }
        }



        /// <summary>
        /// 
        /// </summary>
        /// <param name="binding"></param>
        [ComVisible(true)]
        public static void atualizarResponsaveis(string ambiente, SforceService binding)
        {
            Negocio exec = new Negocio(ambiente, binding, null);
            var diretorio = string.Empty;

            diretorio = "C:\\!adl\\Ayesa\\EPT_Suporte\\180604 Salesforce\\180616 Migração Salesforce\\04 Responsaveis GA\\";
            using (Arquivo arq = new Arquivo(diretorio, "Contatos_AC", "TXT", '|'))
            {
                exec.AtualizarResponsavelPorContato(arq,"2003");
            }
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="binding"></param>
        [ComVisible(true)]
        public static void AtualizarEmails(string ambiente, SforceService binding)
        {
            Negocio exec = new Negocio(ambiente, binding, null);
            var arquivo = "C:\\!adl\\Ayesa\\EPT_Suporte\\180604 Salesforce\\181221 Atualização Emails\\181221_Emails.txt";
            
            using (Arquivo arq = new Arquivo(arquivo, '|'))
            {
                exec.AtualizarEmails(arq,"2005");
            }
        }


        /// <summary>
        /// Atualiza campos do AssentItemAttribute e OrderItemAttribute que não foram carregados corretamente no processo da Alta de Contratação.
        /// </summary>
        /// <param name="binding"></param>
        [ComVisible(true)]
        public static void AtualizarItemAttibute(string ambiente, string codigoEmpresa, string tipoCliente, SforceService binding)
        {
            using (Negocio exec = new Negocio(ambiente, binding, codigoEmpresa))
            {
                var diretorio = string.Empty;
                #region historico de execução
                //diretorio = "C:\\!adl\\Ayesa\\EPT_Suporte\\180604 Salesforce\\180925 Update Alta Contrato\\";
                //using (Arquivo arq = new Arquivo("C:\\!adl\\Ayesa\\EPT_Suporte\\180604 Salesforce\\180925 Update Alta Contrato\\Contatos_AC_181127.txt", '|'))
                //using (Arquivo arq = new Arquivo(@"C:\!adl\Ayesa\EPT_Suporte\180604 Salesforce\180925 Update Alta Contrato\Contatos_AC_190129.txt", '|'))
                //using (Arquivo arq = new Arquivo(@"C:\!adl\Ayesa\EPT_Suporte\180604 Salesforce\180925 Update Alta Contrato\Contatos_AC_190314.txt", '|'))
                //using (Arquivo arq = new Arquivo(@"C:\!adl\Ayesa\EPT_Suporte\180604 Salesforce\180925 Update Alta Contrato\Contatos_AC_190315.txt", '|'))
                #endregion
                
                using (Arquivo arq = new Arquivo(@"C:\!adl\Ayesa\EPT_Suporte\180604 Salesforce\180925 Update Alta Contrato\Contatos_AC_190416.txt", '|'))
                {
                    List<ItemAttribute> listaArq = ArquivoLoader.GetItemsAttributes(arq, typeof(ItemAttribute), tipoCliente);
                    exec.AtualizarItemAttributes(listaArq, arq);
                }
            }

            Console.WriteLine("Processo Finalizado.");
            Console.ReadKey();
        }


        [ComVisible(true)]
        public static void ExtrairRechazosContact(string ambiente, SforceService binding)
        {
            using (Negocio exec = new Negocio(ambiente, binding, "2003"))
            {
                try
                {
                    exec.ExtrairRechazos();
                }
                catch(Exception ex)
                {
                    Debugger.Break();
                }
            }
        }



        /// <summary>
        /// Atualiza o número de Inscrição Municipal da entidade Account
        /// </summary>
        /// <param name="binding"></param>
        [ComVisible(true)]
        public static void atualizarInscricaoMunicipal(string ambiente, string diretorioRaiz)
        {
            using (Negocio exec = new Negocio(ambiente, new SforceService(), "2003"))
            {
                var diretorio = string.Empty;

                diretorio = string.IsNullOrWhiteSpace(diretorioRaiz) ? "C:\\!adl\\Ayesa\\EPT_Suporte\\180604 Salesforce\\181031 AtualizarInscricaoMunicipal\\" : diretorioRaiz;
                using (Arquivo arq = new Arquivo(diretorio, "InscricaoMunicipal", "CSV", ';'))
                {
                    exec.AtualizarInscricaoMunicipal(arq);
                }
            }
        }


        /// <summary>
        /// Configura massivamente o Agrupamento de Contas
        /// </summary>
        /// <param name="ambiente"></param>
        /// <param name="diretorioRaiz"></param>
        [ComVisible(true)]
        public static void CarregarAgrupamento(string codEmpresa, string ambiente, string diretorioRaiz, string complementoNomeArquivo, string atualizarPoDs, SforceService binding)
        {
            //ambiente = "uat";
            using (Negocio exec = new Negocio(ambiente, new SforceService(), codEmpresa))
            {
                var diretorio = string.IsNullOrWhiteSpace(diretorioRaiz) ? @"C:\temp\Carga\190724_Agrupamento\" : diretorioRaiz;
                
                Arquivo arqOrgaosControladores = new Arquivo(diretorio, "OrgaosControladores", "txt", '\t');
                Arquivo arqContratosColetivos = new Arquivo(diretorio, "ContratosColetivos", "txt", '\t');

                //arqOrgaosControladores = new Arquivo(diretorio, "OrgaosControladoresVideomar", "txt", '\t');
                //arqContratosColetivos = new Arquivo(diretorio, "ContratosColetivosVideomar", "txt", '\t');

                //arqOrgaosControladores = new Arquivo(diretorio, "OrgaosControladores7", "txt", '\t');
                //arqContratosColetivos = new Arquivo(diretorio, "ContratosColetivos7", "txt", '\t');

                //arqOrgaosControladores = new Arquivo(diretorio, "OrgaosControladores5", "txt", '\t');
                //arqContratosColetivos = new Arquivo(diretorio, "ContratosColetivos5", "txt", '\t');

                //arqOrgaosControladores = new Arquivo(diretorio, "OrgaosControladores1", "txt", '\t');
                //arqContratosColetivos = new Arquivo(diretorio, "ContratosColetivos1", "txt", '\t');

                //arqOrgaosControladores = new Arquivo(diretorio, "OrgaosControladores2404", "txt", '\t');
                //arqContratosColetivos = new Arquivo(diretorio, contratosColetivos, "txt", '\t');

                arqOrgaosControladores = new Arquivo(diretorio, string.Concat("OrgaosControladores",complementoNomeArquivo), "txt", '\t', false);
                arqContratosColetivos = new Arquivo(diretorio, string.Concat("ContratosColetivos", complementoNomeArquivo), "txt", '\t', false);

                exec.CarregarAgrupamento(arqOrgaosControladores, arqContratosColetivos, Convert.ToBoolean(atualizarPoDs));
            }
        }

        /// <summary>
        /// Realizara a higienização do campo Region__c em ADDRESS.
        /// </summary>
        /// <param name="binding"></param>
        [ComVisible(true)]
        public static void AtualizarRegions(string ambiente, string[] arquivos)
        {
            using (Negocio exec = new Negocio(ambiente, new SforceService(), "2003"))
            {
                var diretorio = string.Empty;

                //diretorio = args.Length < 1 ? "C:\\!adl\\Ayesa\\EPT_Suporte\\180604 Salesforce\\181122 Higienizar Regions\\" : args[0];

                using (Arquivo arq = new Arquivo(arquivos[0].ToString(), '|'))
                {
                    exec.AtualizarRegions(arq);
                }
            }
        }


        public static void AtualizarCaso(string ambiente, string codigoEmpresa, string idCaso, SforceService binding)
        {
            using (Negocio exec = new Negocio(ambiente, binding, codigoEmpresa.ToString()))
            {
                exec.AtualizarCasos(idCaso);
            }
        }


        public static void AtualizarEstadoCliente(string ambiente, string codigoEmpresa, string arquivoClientes, SforceService binding)
        {
            using (CadastroBO exec = new CadastroBO(ambiente, binding, codigoEmpresa.ToString()))
            {
                Arquivo arq = new Arquivo(arquivoClientes);
                exec.AtualizarEstadoClientes(arq);
            }
        }


        public static void AtualizarDebitoAutomatico(string ambiente, SforceService binding)
        {
            using (Negocio exec = new Negocio(ambiente, binding, "2003"))
            {
                exec.AtualizarDebitoAutomatico();
            }
        }


        [ComVisible(true)]
        public static void MergearContas(string ambiente, string codigoEmpresa, string arquivoClientes, SforceService binding)
        {
            using (AccountBO exec = new AccountBO(ambiente, codigoEmpresa, binding))
            {
                Arquivo arq = new Arquivo(arquivoClientes);
                exec.MergearContas(arq);
            }
        }



        /// <summary>
        /// 
        /// </summary>
        /// <param name="codigoEmpresa"></param>
        /// <param name="caminhoArquivo"></param>
        /// <param name="tipoCliente"></param>
        /// <param name="binding"></param>
        [ComVisible(true)]
        public static void CarregarGeracaoDistribuida(string ambiente, string codigoEmpresa, string caminhoArquivo, string tipoCliente, SforceService binding)
        {
            //Formato do arquivo de entrada:
            //idContratoGerador|numeroClienteGerador|idContratoConsumidor|numeroClienteConsumidor|percentual

            using (Negocio exec = new Negocio(ambiente, binding, codigoEmpresa))
            {
                exec.CarregarGeracaoDistribuida(codigoEmpresa, caminhoArquivo, tipoCliente);
            }
        }


        [ComVisible(true)]
        public static void AjustarExternalIdContas(string ambiente, string codigoEmpresa, string caminhoArquivo, SforceService binding)
        {
            //Formato do arquivo de entrada: 1 campo com o external id de account
            //externalIdAccount

            using (Negocio exec = new Negocio(ambiente, binding, codigoEmpresa))
            {
                exec.AjustarExternalIdContas(codigoEmpresa, caminhoArquivo);
            }
        }

        
        
        #endregion Métodos Privados

        #region Manipulação de Dados do Salesforce

        public static System.Xml.XmlElement GetNewXmlElement(string Name, string nodeValue)
        {
            System.Xml.XmlDocument doc = new System.Xml.XmlDocument();
            try
            {
                System.Xml.XmlElement xmlel = doc.CreateElement(Name);
                xmlel.InnerText = string.IsNullOrWhiteSpace(nodeValue) ? " " : nodeValue;
                return xmlel;
            }
            catch{}
            return doc.CreateElement("");
        }

        public static System.Xml.XmlElement GetNewXmlElement(string prefixo, string nome, string nodeValue)
        {
            System.Xml.XmlDocument doc = new System.Xml.XmlDocument();
            try
            {
                System.Xml.XmlElement xmlel = doc.CreateElement(prefixo, nome);
                XmlNode no1 = doc.CreateNode(XmlNodeType.Element, nome.Split(new char[] { ':' })[1], string.Empty);
                XmlNode no0 = doc.CreateNode(XmlNodeType.Element, nome.Split(new char[] { ':' })[0], string.Empty);
                no0.AppendChild(no1);
                xmlel.AppendChild(no0);
                xmlel.InnerText = string.IsNullOrWhiteSpace(nodeValue) ? " " : nodeValue;
                return xmlel;
            }
            catch { }
            return doc.CreateElement("");
        }

        public System.Collections.Hashtable makeFieldMap(Field[] fields)
        {
            System.Collections.Hashtable fieldMap = new System.Collections.Hashtable();
            for (int i = 0; i < fields.Length; i++)
            {
                Field field = fields[i];
                fieldMap.Add(field.name, field);
            }
            return fieldMap;
        }

        public string getFieldValue(string fieldName, System.Xml.XmlElement[] fields)
        {
            string returnValue = "";
            if (fields != null)
            {
                for (int i = 0; i < fields.Length; i++)
                {
                    if (fields[i].LocalName.ToLower().Equals(fieldName.ToLower()))
                    {
                        returnValue = fields[i].InnerText;
                        XmlNodeList nodeList = fields[i].ChildNodes;
                    }
                }
            }
            return returnValue;
        }

        public string getFieldValueMore(string fieldName, string entidade, string entidade2, string campo, System.Xml.XmlElement[] fields)
        {
            string returnValue = "";
            if (fields != null)
            {
                for (int i = 0; i < fields.Length; i++)
                {
                    if (fields[i].LocalName.ToLower().Equals(fieldName.ToLower()))
                    {
                        XmlNodeList nodeList = fields[i].ChildNodes;

                        foreach (System.Xml.XmlElement item in nodeList)
                        {
                            if (entidade != "")
                            {
                                if (item.LocalName.ToLower().Equals(entidade.ToLower()))
                                {
                                    XmlNodeList nodeList2 = item.ChildNodes;

                                    foreach (System.Xml.XmlElement item2 in nodeList2)
                                    {
                                        if (entidade2 != "")
                                        {
                                            if (item2.LocalName.ToLower().Equals(entidade2.ToLower()))
                                            {
                                                XmlNodeList nodeList3 = item2.ChildNodes;

                                                foreach (System.Xml.XmlElement item3 in nodeList3)
                                                {
                                                    if (item3.LocalName.ToLower().Equals(campo.ToLower()))
                                                    {
                                                        returnValue = item3.InnerText;
                                                    }   
                                                }
                                            }
                                        }
                                        else if (item2.LocalName.ToLower().Equals(campo.ToLower()))
                                        {
                                            returnValue = item2.InnerText;
                                        }
                                    }

                                }
                            }
                            else if (item.LocalName.ToLower().Equals(campo.ToLower()))
                            {
                                returnValue = item.InnerText;
                            }
                        }

                   }
                }
            }
            return returnValue;
        }

        //private void Update()
        //{
        //    if (!loggedIn)
        //    {
        //        if (!login())
        //            return;
        //    }

        //    try
        //    {

        //        StreamReader rd = new StreamReader("C:\\Users\\Bruno Werneck\\Desktop\\phone.csv");

        //        //Declaro uma string que será utilizada para receber a linha completa do arquivo 
        //        string linha = null;

        //        int i = 0;
        //        sObject update = new sObject();

        //        List<sObject> lista = new List<sObject>();

        //        //realizo o while para ler o conteudo da linha, Adriel aqui você tem que aplicar a sua lógica
        //        //Que pode ser de um arquivo de texto de uma lista que seja a preferência é sua!
        //        while ((linha = rd.ReadLine()) != null)
        //        {
        //            i++;

        //            //Aqui declaramos o campo
        //            update.type = "Contact";
        //            update = new sObject();

        //            //Aqui será o id do objeto recuperado
        //            update.Id = linha;

        //            //Aqui serão os campos que vão ser atualizados. Como é um array poderia passar mais de um de uma vez...
        //            update.Any = new System.Xml.XmlElement[] { this.GetNewXmlElement("PreferredChannelContact__c", "CAN010") };

        //            //O Sales Force tem uma limitação em que ele atualiza 200 registros de uma vez. 
        //            //Ou seja a cada 200 na lista mandamos para o SalesForce

        //            lista.Add(update);

        //            if (i == 200)
        //            {
        //                SaveResult[] saveResults = binding.update(lista.ToArray());
        //                i = 0;
        //                lista = new List<sObject>();
        //            }
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        Console.WriteLine("\nErro ao atualizar o registro: \n" + ex.Message);
        //    }

        //    Console.WriteLine("\nAperte para Sair...");
        //    Console.ReadLine();
        //}

        #endregion Manipulação de Dados do Salesforce
    }
} 

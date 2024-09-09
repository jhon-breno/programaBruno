using Pangea.Dados;
using Pangea.Dados.Base;
using Pangea.Entidades;
using Pangea.Entidades.Enumeracao;
using Pangea.Util;
using SalesforceExtractor.apex;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace Pangea.Swat.UI.Negocio
{
    public class ClienteBO
    {
        private Empresa empresa;
        string codEmpresa;
        private TipoCliente tipo;
        private ClienteDAO dao;
        private List<Pangea.Entidades.Cliente> lstClientesFilhos = new List<Pangea.Entidades.Cliente>();
        private Dictionary<string, Pangea.Entidades.Cliente> dicClientesTodos = new Dictionary<string, Pangea.Entidades.Cliente>();
        private Dictionary<int, int> dicClientesAnteriores = new Dictionary<int, int>();
        private Dictionary<int, int> dicClientesAtuais = new Dictionary<int, int>();

        string _arqLog;

        public ClienteBO(Empresa empresa, TipoCliente tipo)
        {
            this.empresa = empresa;
            this.tipo = tipo;
            this._arqLog = string.Format("C:\\Temp\\CLIENTE_ANTERIOR_{0}_{1}_LOG", this.codEmpresa, DateTime.Now.ToString("yyyy-MM-dd-HH-mm"));
            
            if (this.dao == null)
            {
                this.dao = new ClienteDAO(this.empresa);
                this.codEmpresa = ((int)empresa).ToString();
            }
        }

        public ClienteBO(Empresa empresa, string caminhoLog)
        {
            this.empresa = empresa;
            this._arqLog = caminhoLog;

            if (this.dao == null)
            {
                this.dao = new ClienteDAO(this.empresa);
                this.codEmpresa = ((int)empresa).ToString();
            }
        }


        /// <summary>
        /// Grava arquivo com todos os clientes anteriores, desde o primeiro até o cliente atual.
        /// </summary>
        /// <param name="arquivo"></param>
        /// <returns></returns>
        [Obsolete("Utilizar o método para todos os clientes 'GerarRelatorioClienteAnteriorTodosClientes'")]
        public string GerarRelatorioClienteAnterior(string arquivo)
        {
            //TODO: carregar numero_cliente, tipo_cliente, cliente_anterior, tipo_documento, documento
            //var clientes = (from lista in dao.Consultar("select numero_cliente, cliente_anterior from cliente").AsEnumerable()
            //                select new {
            //                    numero = lista.Field<int>("numero_cliente"),
            //                    anterior = lista.Field<int>("cliente_anterior") }
            //                    );
            //List<int> todos = dao.Consultar("select numero_cliente, cliente_anterior from cliente").AsEnumerable().Select(new { cliente =  x => x.Field<int>("numero_cliente")).ToList();

            //arquivo = "c:\\temp\\cliente_anterior\\190117\\clientesTodos.txt";
            arquivo = @"C:\temp\cliente_anterior\190911\CLIENTE_ANTERIOR_2003_2019-09-11-15-19.txt";

            if (string.IsNullOrWhiteSpace(arquivo))
            {
                return "Informe o arquivo de entrada com os clientes a serem processados.";
            }

            //validar arquivo de entrada
            if (!File.Exists(arquivo))
                return string.Format("Arquivo '{0}' não encontrado.", arquivo);

            //arquivos de entrada e saída
            StringBuilder sb = new StringBuilder();
            StringBuilder _sbNomeArq = new StringBuilder();
            StringBuilder _arqFinal = new StringBuilder();
            int cont = 0;
            //int total = 0;
            int numeroCliente;

            using (StreamReader sr = new StreamReader(arquivo, Encoding.GetEncoding("iso-8859-1")))
            {
                while (!sr.EndOfStream)
                {
                    string linha = sr.ReadLine();
                    string[] campos = linha.Split('|');
                    Pangea.Entidades.Cliente cliAux = new Pangea.Entidades.Cliente();
                    cliAux.Empresa = this.empresa.ToString();
                    cliAux.Numero_cliente = campos[0];
                    cliAux.Cliente_anterior = campos[1];
                    //cliAux.Rut = campos[4];
                    //cliAux.Tipo_Ident = campos[3];
                    //cliAux.Nombre = campos[6].Trim().Replace("\\", string.Empty);
                    //_lstClientes.Add(cliAux);
                    try
                    {
                        this.dicClientesTodos.Add(campos[0], cliAux);
                        this.dicClientesAnteriores.Add(Convert.ToInt32(campos[0]), Convert.ToInt32(string.IsNullOrWhiteSpace(campos[1]) ? "0" : campos[1]));
                    }
                    catch (Exception ex)
                    {
                        continue;
                    }
                }
            }

            string _arqSaida = string.Format(@"C:\temp\cliente_anterior\190911\CLIENTE_ANTERIOR_{0}_{1}", this.codEmpresa, DateTime.Now.ToString("yyyy-MM-dd-HH-mm"));
            string _arqSaidaUltimosClientes = string.Format(@"C:\temp\cliente_anterior\190911\ULTIMOS_CLIENTES_{0}_{1}", this.codEmpresa, DateTime.Now.ToString("yyyy-MM-dd-HH-mm"));

            if (File.Exists(string.Concat(_arqSaida, ".txt")))
                File.Delete(string.Concat(_arqSaida, ".txt"));

            //controles para o contrúdo do arquivo de saída
            StringBuilder _conteudo = new StringBuilder();
            StringBuilder _conteudoUltimoCliente = new StringBuilder();
            StringBuilder _externalId = new StringBuilder();
            StringBuilder _clienteAnterior = new StringBuilder();

            //arquivo = "c:\\temp\\cliente_anterior\\190117\\clienteAnterior_CE_GB.txt";  //lista de clientes selecionados da base ,com cliente_anterior, ordenado por numero_cliente DESCENDENTE
            arquivo = @"C:\temp\cliente_anterior\190911\cliente_anterior_190911.txt";
            
            using (StreamReader sr = new StreamReader(arquivo, Encoding.GetEncoding("iso-8859-1")))
            {
                while (cont >= 0)
                {
                    this.lstClientesFilhos.Clear();

                    _sbNomeArq.Clear();
                    _arqFinal.Clear();

                    if (sr.EndOfStream)
                    {
                        cont = -1;
                        continue;
                    }

                    try
                    {
                        var linha = sr.ReadLine();
                        if (string.IsNullOrWhiteSpace(linha))
                            continue;

                        if (!Int32.TryParse(linha, out numeroCliente))
                            continue;

                        if (File.Exists(string.Concat(_arqFinal, ".txt")))
                            File.Delete(string.Concat(_arqFinal, ".txt"));

                        Pangea.Entidades.Cliente _clientePai;
                        Pangea.Entidades.Cliente _clienteAtual;
                        //string cliteste = "7339353";
                        //var teste = _lista3.Values.Where(c => c.Cliente_anterior == cliteste);

                        //string clienteAtual = GetClienteAtual(numeroCliente);

                        try
                        {
                            _clientePai = BuscarClienteOriginal(linha);
                            _clienteAtual = BuscarClientePai(linha);   // dicClientesAnteriores.Where(x => x.Value == Int32.Parse(linha)).First().Key;
                        }
                        catch (KeyNotFoundException ex)
                        {
                            continue;
                            //TODO: logar cliente nao encontrado????
                        }

                        //impede a repetição da busca de clientes já identificados
                        if (this.dicClientesAtuais.Keys.Contains(numeroCliente))
                            continue;

                        for (int i = 0; i < this.lstClientesFilhos.Count; i++)
                        {
                            int tempNumeroCliente = Convert.ToInt32(this.lstClientesFilhos[i].Numero_cliente);

                            if (_clienteAtual != null && !_clienteAtual.Numero_cliente.Equals(linha))
                            {
                                if (!this.dicClientesAtuais.Keys.Contains(tempNumeroCliente))
                                {
                                    this.dicClientesAtuais.Add(tempNumeroCliente, Int32.Parse(_clienteAtual.Numero_cliente));
                                    _conteudoUltimoCliente.AppendLine(string.Format("{0}\t{1}", numeroCliente, _clienteAtual.Numero_cliente));
                                }
                            }

                            _externalId.Clear();
                            _clienteAnterior.Clear();

                            _clienteAnterior.Append((this.lstClientesFilhos[i].Cliente_anterior == null || "0".Equals(this.lstClientesFilhos[i].Cliente_anterior)) ? string.Empty : lstClientesFilhos[i].Cliente_anterior);

                            if (string.IsNullOrWhiteSpace(this.lstClientesFilhos[i].Nombre) || this.lstClientesFilhos[i].Nombre.Contains("CONSUMIDOR ATUALIZE"))
                            {
                                //DESPERSONALIZADO
                                _externalId.Append(string.Concat(this.codEmpresa, "D", this.lstClientesFilhos[0].Numero_cliente));
                                lstClientesFilhos[i].Nombre = string.Empty;
                            }
                            else if (string.IsNullOrWhiteSpace(this.lstClientesFilhos[i].Rut) || this.lstClientesFilhos[i].Rut.Contains("INVALIDO") || lstClientesFilhos[i].TipoDocumento == TipoDocumento.NaoIdentificado)
                            {
                                //DOC INVALIDO
                                _externalId.Append(string.Concat(this.codEmpresa, this.lstClientesFilhos[i].Numero_cliente, "INVALIDO"));
                            }
                            else
                            {
                                    #region Formato da apresentação dos documentos

                                string auxDoc = this.lstClientesFilhos[i].Rut;
                                    string descricaoDoc = EnumString.GetStringValue(this.lstClientesFilhos[i].TipoDocumento);

                                    try
                                    {
                                        if (this.lstClientesFilhos[i].TipoDocumento == TipoDocumento.CNPJ)
                                            auxDoc = Regex.Replace(this.lstClientesFilhos[i].Rut, " ", "").PadLeft(22,'0').Substring(22 - 14, 14);

                                        else
                                            auxDoc = Regex.Replace(this.lstClientesFilhos[i].Rut, " ", "").PadLeft(22, '0').Substring(22 - 11, 11);

                                        if (!Validacao.ValidarCNPJ(ref auxDoc) && !Validacao.ValidarCPF(ref auxDoc))
                                            throw new Exception();

                                        _externalId.Append(string.Concat(codEmpresa, auxDoc, this.lstClientesFilhos[i].Dv_rut, descricaoDoc.Substring(3, descricaoDoc.Length - 3)));
                                    }
                                    catch (Exception ex)
                                    {
                                        _externalId.Append(string.Concat(codEmpresa, this.lstClientesFilhos[i].Numero_cliente, "INVALIDO"));
                                    }

                                    #endregion
                            }

                            _conteudo.AppendLine(string.Format("{0}|{1}|{2}|{3}", this.lstClientesFilhos[i].Numero_cliente, _clienteAnterior.ToString(), _externalId.ToString(), lstClientesFilhos[i].Nombre.Trim()));
                        }

                        if (cont == 199)
                        {
                            if (!IO.EscreverArquivo(_arqSaida, _conteudo.ToString(), null))
                                throw new Exception(string.Format("Falha ao gravar o arquivo após o registro {0}", cont - 1));

                            if(!IO.EscreverArquivo(_arqSaidaUltimosClientes, _conteudoUltimoCliente.ToString(), null))
                                throw new Exception(string.Format("Falha ao gravar o arquivo ULTIMOS após o registro {0}", cont - 1));

                            _conteudo.Clear();
                            _conteudoUltimoCliente.Clear();
                            cont = 0;
                        }

                        //return string.Format("Encontrados {0} clientes relacionados ao cliente-pai.  Vide arquivo gerado.", _lstClientesFilhos.Count());;
                        //IO.EscreverArquivo(_arqFinal.ToString(), jsonObj, null);
                    }
                    catch (Exception ex)
                    {
                        if (!IO.EscreverArquivo(_arqSaida, _conteudo.ToString(), null))
                            throw ex;
                    }
                    cont++;
                }   //fim while arquivo entrada
            }

            IO.EscreverArquivo(_arqSaida, _conteudo.ToString(), null);
            IO.EscreverArquivo(_arqSaidaUltimosClientes, _conteudoUltimoCliente.ToString(), null);

            return string.Format("Relatório processado.");
        }




        /// <summary>
        /// Percorre toda a lista de clientes do arquivo de entrada, identificando cada registro com o External Id apropriado e o histórico de cliente anterior, desde sua origem.
        /// </summary>
        /// <param name="arquivo">Relação de clientes em ordem descrescente de Número de Cliente, que é a condição obrigatória.</param>
        /// <returns></returns>
        public string GerarRelatorioClienteAnteriorTodosClientes(string arquivo)
        {
            #region SQL versao 2020-12-18
            //set pdqpriority  80;
            //set isolation to dirty read;
            //unload to /synergia/archivos/clientesTodos.txt

            //select    to_number(replace(replace(numero_cliente, chr(13), ''), chr(10), ''))
            //    , trim(replace(replace(cliente_anterior, chr(13), ''), chr(10), ''))
            //    , trim(replace(replace(documento_cliente, chr(13), ''), chr(10), ''))
            //    , trim(replace(replace(tipo_identidade, chr(13), ''), chr(10), ''))
            //    , trim(replace(replace(nombre, chr(13), ''), chr(10), ''))
            //from (
            //select c.numero_cliente, c.cliente_anterior,
            //        case    when nvl(d.tipo_documento,'') = '' THEN nvl(trim(c.rut),'') || nvl(trim(c.dv_rut),'')
            //                else nvl(trim(d.numero_doc),'') || nvl(trim(d.dv_documento),'')
            //        end documento_cliente,
            //        case    when nvl(d.tipo_documento,'') = '' then trim(c.tipo_ident)
            //                else trim(d.tipo_documento)
            //        end tipo_identidade 
            //        , c.nombre
            //from cliente c, outer documento_cliente d
            //where c.numero_cliente = d.numero_cliente
            //    and d.tipo_documento in ('002','005')
            //    and c.Estado_cliente <> 8
            //UNION
            //select c.numero_cliente, c.cliente_anterior,
            //        nvl(trim(c.rut),'') || nvl(trim(c.dv_rut),'') documento_cliente,
            //        trim(c.tipo_ident) tipo_identidade 
            //        , c.nombre
            //from grandes:cliente c
            //where c.tipo_ident in ('002','003','004')
            //    and c.Estado_cliente = 8
            //)
            //order by 1 desc
            #endregion

            #region Historico de execuçoes
            //arquivo = "c:\\temp\\cliente_anterior\\190117\\clientesTodos.txt";
            //arquivo = "c:\\temp\\cliente_anterior\\190201\\clientesTodos.txt";
            //arquivo = "c:\\temp\\cliente_anterior\\190225\\clientesTodos.txt";
            //arquivo = "c:\\temp\\cliente_anterior\\190228\\clientesTodos.txt";
            //arquivo = @"c:\temp\cliente_anterior\RJ 190318\clientesTodos.txt";
            //arquivo = @"c:\temp\cliente_anterior\CE 190406\clientesTodos.txt";
            //arquivo = @"C:\!adl\Ayesa\EPT_Suporte\180604 Salesforce\181006 Carga SalesGeral\CE 190405\190417.txt";
            //arquivo = @"C:\!adl\Ayesa\EPT_Suporte\180604 Salesforce\181006 Carga SalesGeral\CE 190418\clientes_todos_190418_CE.txt";
            //arquivo = @"C:\!adl\Ayesa\EPT_Suporte\180604 Salesforce\181006 Carga SalesGeral\CE 190423\clientes_todos_190423_CE.txt";
            //arquivo = @"C:\!adl\Ayesa\EPT_Suporte\180604 Salesforce\181006 Carga SalesGeral\CE 190531\clientes_todos_190531_CE.txt";
            //arquivo = @"C:\!adl\Ayesa\EPT_Suporte\180604 Salesforce\181006 Carga SalesGeral\CE 190531\clientes_todos_190603_CE.txt";-
            //arquivo = @"C:\!adl\Ayesa\EPT_Suporte\180604 Salesforce\181006 Carga SalesGeral\CE 190615\clientes_todos_190615_CE.txt";
            //arquivo = @"C:\!adl\Ayesa\EPT_Suporte\180604 Salesforce\181006 Carga SalesGeral\CE 190617\clientes_todos_190617_CE.txt";
            //arquivo = @"C:\!adl\Ayesa\EPT_Suporte\180604 Salesforce\181006 Carga SalesGeral\CE 190618\clientes_todos_190618_CE.txt";
            //arquivo = @"C:\!adl\Ayesa\EPT_Suporte\180604 Salesforce\181006 Carga SalesGeral\CE 190626\clientes_todos_190626_CE.txt";
            //arquivo = @"C:\!adl\Ayesa\EPT_Suporte\180604 Salesforce\181006 Carga SalesGeral\CE 190628\clientes_todos_190628_CE.txt";
            //arquivo = @"C:\!adl\Ayesa\EPT_Suporte\180604 Salesforce\181006 Carga SalesGeral\RJ 190822\clientes_todos_190822_RJ.txt";
            //string diretorio = @"C:\temp\cliente_anterior\RJ 191104\";
            //string diretorio = @"C:\temp\cliente_anterior\CE 200413\";
            #endregion

            string diretorio = ConfigurationManager.AppSettings.Get("Log.Diretorio");
            //arquivo = string.Concat(diretorio,"clientesTodos.txt");

            string _arqSaida = string.Format(@"{0}CLIENTE_ANTERIOR_{1}_{2}", diretorio, this.codEmpresa, DateTime.Now.ToString("yyyy-MM-dd-HH-mm"));

            if (string.IsNullOrWhiteSpace(arquivo))
                return "Informe o arquivo de entrada com os clientes a serem processados.";

            if (!File.Exists(arquivo))
                return string.Format("Arquivo '{0}' não encontrado.", arquivo);

            StringBuilder sb = new StringBuilder();
            StringBuilder _sbNomeArq = new StringBuilder();
            StringBuilder _arqFinal = new StringBuilder();
            int cont = 0;
            int numeroCliente;

            //select c.numero_cliente, c.cliente_anterior,
            //        case    when nvl(d.tipo_documento,'') = '' THEN nvl(trim(c.rut),'') || nvl(trim(c.dv_rut),'')
            //                else nvl(trim(d.numero_doc),'') || nvl(trim(d.dv_documento),'')
            //        end documento_cliente,
            //        case    when nvl(d.tipo_documento,'') = '' then trim(c.tipo_ident)
            //                else trim(d.tipo_documento)
            //        end tipo_identidade 
            //        , c.nombre
            //from cliente c, outer documento_cliente d
            //where c.numero_cliente = d.numero_cliente
            //order by 1 desc

            using (StreamReader sr = new StreamReader(arquivo, Encoding.GetEncoding("iso-8859-1")))
            {
                string linha = null;
                string[] campos = null;
               
                while (!sr.EndOfStream)
                {
                    linha = sr.ReadLine();
                    campos = linha.Split('|');
                    if (campos.Count() == 1)
                        Debugger.Break();
                    try
                    {
                        Cliente cliAux = new Cliente();
                        
                        //if (campos.Count() < 5)
                            //Debugger.Break();

                        cliAux.Numero_cliente = campos[0].Trim();
                        cliAux.Cliente_anterior = campos[1].Trim();
                        cliAux.Rut = campos[2].Trim();      //RUT || DV_RUT     <<<<<<<<<<<<-------- unificar os campos de documento na consulta original
                        //if (cliAux.Rut.Length > 16)
                        //{
                        //    cliAux.Rut = "2003".Equals(cliAux.Rut.Substring(0, 4)) ?
                        //                cliAux.Rut.Substring(4, (cliAux.Rut.Length - (cliAux.Rut.Length - 16))) :
                        //                cliAux.Rut.Substring((cliAux.Rut.Length - 16), (cliAux.Rut.Length - (cliAux.Rut.Length - 16)));
                        //}
                        //if ("40975099".Equals(cliAux.Numero_cliente))
                        //    Debugger.Break();
                        cliAux.Tipo_Ident = campos.Count() < 4 ? string.Empty : DBNull.Value.Equals(campos[3]) ? string.Empty : campos[3].Trim().Replace("\\", string.Empty);
                        cliAux.Nombre = campos.Count() < 5 ? string.Empty : campos[4].Trim().Replace("\\", string.Empty);
                        cliAux.Apelido = campos.Count() < 6 ? string.Empty : campos[5].Trim().Replace("\\", string.Empty); 
                        //cliAux.Empresa = campos[5].Trim();
                        try
                        {
                            if(!this.dicClientesTodos.ContainsKey(cliAux.Numero_cliente))
                                this.dicClientesTodos.Add(cliAux.Numero_cliente, cliAux);
                            //this.dicClientesAnteriores.Add(Convert.ToInt32(campos[0]), Convert.ToInt32(string.IsNullOrWhiteSpace(campos[1]) ? "0" : campos[1]));
                        }
                        catch (Exception ex)
                        {
                            continue;
                        }
                    }
                    catch (Exception ex)
                    {
                        //Debugger.Break();
                    }
                    linha = null;
                    campos = null;
                }
            }

            if (File.Exists(string.Concat(_arqSaida, ".txt")))
                File.Delete(string.Concat(_arqSaida, ".txt"));

            //controles para o contrúdo do arquivo de saída
            StringBuilder _conteudo = new StringBuilder();
            StringBuilder _externalId = new StringBuilder();
            StringBuilder _clienteAnterior = new StringBuilder();

            foreach (KeyValuePair<string, Cliente> cli in this.dicClientesTodos)
            {
                string auxDoc = string.Empty;
                var proximo = this.dicClientesTodos.GetEnumerator().MoveNext();

                //impede a repetição da busca de clientes já identificados
                if (string.IsNullOrWhiteSpace(cli.Value.Numero_cliente) || this.dicClientesAtuais.Keys.Contains(Int32.Parse(cli.Value.Numero_cliente)))
                    continue;

                _externalId.Clear();
                _clienteAnterior.Clear();

                this.lstClientesFilhos.Clear();

                _sbNomeArq.Clear();
                _arqFinal.Clear();

                #region SEM CLIENTE ANTERIOR
                if (string.IsNullOrEmpty(cli.Value.Cliente_anterior) || "0".Equals(cli.Value.Cliente_anterior))
                {
                    if (!string.IsNullOrWhiteSpace(cli.Value.Numero_cliente))
                    {
                        //impede a repetição da busca de clientes já identificados
                        int tempNumeroCliente = Convert.ToInt32(cli.Value.Numero_cliente);
                        if (this.dicClientesAtuais.Keys.Contains(tempNumeroCliente))
                            continue;

                        this.dicClientesAtuais.Add(tempNumeroCliente, tempNumeroCliente);
                        //dicClientesTodos.GetType().GetProperty("Item").SetValue(dicClientesTodos[tempNumeroCliente.ToString()], null);
                    }

                    if (cli.Value.DespersonaSalesforce)
                    {
                        //DESPERSONALIZADO
                        _externalId.Append(string.Concat(codEmpresa, "D", cli.Value.Numero_cliente));
                        cli.Value.Nombre = string.Empty;
                    }
                    else if (string.IsNullOrWhiteSpace(cli.Value.Rut) || cli.Value.Rut.Contains("INVALIDO"))
                    {
                        //190617: retirada a validação do tipo_ident = 'Não Identificado'
                        //lstClientesFilhos[i].TipoDocumento == TipoDocumento.NaoIdentificado

                        //DOC INVALIDO
                        _externalId.Append(string.Concat(codEmpresa, cli.Value.Numero_cliente, "INVALIDO"));
                    }
                    else
                    {
                        #region Formato da apresentação dos documentos
                        string descricaoDoc = EnumString.GetStringValue(cli.Value.TipoDocumento);

                        try
                        {
                            auxDoc = Utils.FormatarDocumento(cli.Value.Rut, cli.Value.TipoDocumento);

                            if (cli.Value.TipoDocumento == TipoDocumento.NaoIdentificado)
                            {
                                auxDoc = validarDocumentoNaoIdentificado(Regex.Replace(cli.Value.Rut, " ", ""));
                                descricaoDoc = auxDoc.Length == 14 ? EnumString.GetStringValue(TipoDocumento.CNPJ) : EnumString.GetStringValue(TipoDocumento.CPFGB);

                                if(string.IsNullOrWhiteSpace(auxDoc))
                                    _externalId.Append(string.Concat(codEmpresa, cli.Value.Numero_cliente, "INVALIDO"));
                                else
                                    _externalId.Append(string.Concat(codEmpresa, auxDoc, cli.Value.Dv_rut, descricaoDoc.Substring(3, descricaoDoc.Length - 3)));
                            }
                            else
                            {
                                if (!Validacao.ValidarCNPJ(ref auxDoc) && !Validacao.ValidarCPF(ref auxDoc))
                                    _externalId.Append(string.Concat(codEmpresa, cli.Value.Numero_cliente, "INVALIDO"));
                                else
                                    _externalId.Append(string.Concat(codEmpresa, auxDoc, cli.Value.Dv_rut, descricaoDoc.Substring(3, descricaoDoc.Length - 3)));
                            }

                            //bool isCnpj = Validacao.ValidarCNPJ(ref auxDoc);
                            //if(isCnpj)
                            //{

                            //}
                            //bool isCpf = Validacao.ValidarCPF(ref auxDoc);

                            //if (!isCnpj && !isCpf)
                            //    _externalId.Append(string.Concat(codEmpresa, cli.Value.Numero_cliente, "INVALIDO"));
                            //else
                            //    _externalId.Append(string.Concat(codEmpresa, auxDoc, cli.Value.Dv_rut, descricaoDoc.Substring(3, descricaoDoc.Length - 3)));

                        }
                        catch (Exception ex)
                        {
                            _externalId.Append(string.Concat(codEmpresa, cli.Value.Numero_cliente, "INVALIDO"));
                        }
                        #endregion
                    }

                    _conteudo.AppendLine(string.Format("{0}|{1}|{2}|{3}|{4}|{5}"
                        , cli.Value.Numero_cliente
                        , _clienteAnterior.ToString()
                        , _externalId.ToString()
                        , cli.Value.Nombre.Trim()
                        , cli.Value.Apelido
                        , (string.IsNullOrWhiteSpace(auxDoc) ? cli.Value.Rut : auxDoc)
                        ));
                }
                #endregion

                #region COM CLIENTE ANTERIOR
                if (!string.IsNullOrWhiteSpace(cli.Value.Cliente_anterior) && !"0".Equals(cli.Value.Cliente_anterior))
                {
                    try
                    {
                        var linha = cli.Value.Numero_cliente;
                        if (string.IsNullOrWhiteSpace(linha))
                            continue;

                        if (!Int32.TryParse(linha, out numeroCliente))
                            continue;

                        try
                        {
                            BuscarClienteOriginal(linha);
                        }
                        catch (KeyNotFoundException ex)
                        {
                            throw ex;
                            //TODO: logar cliente nao encontrado????
                        }

                        for (int i = 0; i < this.lstClientesFilhos.Count; i++)
                        {
                            int tempNumeroCliente = Convert.ToInt32(lstClientesFilhos[i].Numero_cliente);

                            //impede a repetição da busca de clientes já identificados
                            if (this.dicClientesAtuais.Keys.Contains(tempNumeroCliente))
                                break;

                            if (!this.dicClientesAtuais.Keys.Contains(tempNumeroCliente))
                            {
                                this.dicClientesAtuais.Add(tempNumeroCliente, tempNumeroCliente);
                                //dicClientesTodos.GetType().GetProperty("Item").SetValue(dicClientesTodos[tempNumeroCliente.ToString()], null);
                            }

                            _externalId.Clear();
                            _clienteAnterior.Clear();

                            _clienteAnterior.Append((this.lstClientesFilhos[i].Cliente_anterior == null || "0".Equals(this.lstClientesFilhos[i].Cliente_anterior)) ? string.Empty : lstClientesFilhos[i].Cliente_anterior);

                            if (this.lstClientesFilhos[i].DespersonaSalesforce)
                            {
                                string clientePai = this.lstClientesFilhos.Where(c => c.DespersonaSalesforce == true).FirstOrDefault().Numero_cliente;
                                this.lstClientesFilhos.Where(c => c.DespersonaSalesforce == true).Select(d => { d.ClientePai = clientePai; return d; }).ToList();

                                //DESPERSONALIZADO
                                _externalId.Append(string.Concat(codEmpresa, "D", this.lstClientesFilhos[i].ClientePai));
                                lstClientesFilhos[i].Nombre = string.Empty;
                            }
                            else if (string.IsNullOrWhiteSpace(this.lstClientesFilhos[i].Rut) || this.lstClientesFilhos[i].Rut.Contains("INVALIDO"))
                            {
                                //190617: retirada a validação do tipo_ident = 'Não Identificado'
                                //lstClientesFilhos[i].TipoDocumento == TipoDocumento.NaoIdentificado

                                //DOC INVALIDO
                                _externalId.Append(string.Concat(codEmpresa, this.lstClientesFilhos[i].Numero_cliente, "INVALIDO"));
                            }
                            else
                            {
                                #region Formato da apresentação dos documentos

                                auxDoc = this.lstClientesFilhos[i].Rut;
                                string descricaoDoc = EnumString.GetStringValue(this.lstClientesFilhos[i].TipoDocumento);

                                try
                                {
                                    if (this.lstClientesFilhos[i].TipoDocumento == TipoDocumento.NaoIdentificado)
                                    {
                                        auxDoc = validarDocumentoNaoIdentificado(Regex.Replace(this.lstClientesFilhos[i].Rut, " ", ""));
                                        descricaoDoc = auxDoc.Length == 14 ? EnumString.GetStringValue(TipoDocumento.CNPJ) : EnumString.GetStringValue(TipoDocumento.CPFGB);

                                        if (string.IsNullOrWhiteSpace(auxDoc))
                                            _externalId.Append(string.Concat(codEmpresa, cli.Value.Numero_cliente, "INVALIDO"));
                                        else
                                            _externalId.Append(string.Concat(codEmpresa, auxDoc, cli.Value.Dv_rut, descricaoDoc.Substring(3, descricaoDoc.Length - 3)));

                                    }
                                    else
                                    {
                                        if (!Validacao.ValidarCNPJ(ref auxDoc) && !Validacao.ValidarCPF(ref auxDoc))
                                            _externalId.Append(string.Concat(codEmpresa, cli.Value.Numero_cliente, "INVALIDO"));
                                        else
                                            _externalId.Append(string.Concat(codEmpresa, auxDoc, cli.Value.Dv_rut, descricaoDoc.Substring(3, descricaoDoc.Length - 3)));

                                        //if (this.lstClientesFilhos[i].TipoDocumento == TipoDocumento.CNPJ)
                                        //    auxDoc = Regex.Replace(this.lstClientesFilhos[i].Rut, " ", "").PadLeft(22, '0').Substring(22 - 14, 14);
                                        //else
                                        //    auxDoc = Regex.Replace(this.lstClientesFilhos[i].Rut, " ", "").PadLeft(22, '0').Substring(22 - 11, 11);
                                    }

                                    //bool isCnpj = Validacao.ValidarCNPJ(ref auxDoc);
                                    //if (isCnpj)
                                    //{

                                    //}
                                    //bool isCpf = Validacao.ValidarCPF(ref auxDoc);

                                    //if (!isCnpj && !isCpf)
                                    //    _externalId.Append(string.Concat(codEmpresa, this.lstClientesFilhos[i].Numero_cliente, "INVALIDO"));
                                    //else
                                    //    _externalId.Append(string.Concat(codEmpresa, auxDoc, this.lstClientesFilhos[i].Dv_rut, descricaoDoc.Substring(3, descricaoDoc.Length - 3)));
                                }
                                catch (Exception ex)
                                {
                                    _externalId.Append(string.Concat(codEmpresa, this.lstClientesFilhos[i].Numero_cliente, "INVALIDO"));
                                }

                                #endregion
                            }

                            _conteudo.AppendLine(string.Format("{0}|{1}|{2}|{3}|{4}|{5}"
                                , this.lstClientesFilhos[i].Numero_cliente
                                , _clienteAnterior.ToString()
                                , _externalId.ToString()
                                , lstClientesFilhos[i].Nombre.Trim()
                                , lstClientesFilhos[i].Apelido.Trim()
                                , (string.IsNullOrWhiteSpace(auxDoc) ? lstClientesFilhos[i].Rut.Trim() : auxDoc)
                                ));
                        }

                    }
                    catch (Exception ex)
                    {
                        if (!IO.EscreverArquivo(_arqLog, cli.Value.Numero_cliente, null))
                            throw ex;
                    }
                }
                #endregion


                if (cont == 49999)
                {
                    if (!IO.EscreverArquivo(_arqSaida, _conteudo.ToString().Trim(), null))
                        throw new Exception(string.Format("Falha ao gravar o arquivo após o registro {0}", cont - 1));

                    _conteudo.Clear();
                    cont = 0;
                }
                cont++;
            }

            IO.EscreverArquivo(_arqSaida, _conteudo.ToString().Trim(), null);

            _conteudo = null;
            _externalId = null;
            _clienteAnterior = null;
            
            return string.Format("Relatório processado.");
        }


        private string validarDocumentoNaoIdentificado(string numeroDoc)
        {
            string auxDoc = string.Empty;
            try
            {
                auxDoc = Regex.Replace(numeroDoc, " ", "").PadLeft(22, '0').Substring(22 - 14, 14);
                if (!Validacao.ValidarCNPJ(ref auxDoc))
                {
                    try
                    {
                        auxDoc = Regex.Replace(numeroDoc, " ", "").PadLeft(22, '0').Substring(22 - 11, 11);
                        return Validacao.ValidarCPF(ref auxDoc) ? auxDoc : string.Empty;
                    }
                    catch
                    {
                        auxDoc = string.Empty;
                    }
                }
            }
            catch {}
            return auxDoc;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="empresa"></param>
        /// <param name="clienteAnterior"></param>
        /// <returns></returns>
        private string GetClienteAtual(int clienteAnterior)
        {
            if (clienteAnterior <= 0)
                return string.Empty;

            string cli = clienteAnterior.ToString();

            var cliTemp2 = dicClientesAnteriores.FirstOrDefault(c => c.Value == clienteAnterior);

            try
            {
                if (cliTemp2.Key > 0)
                    cli = GetClienteAtual(cliTemp2.Key);
            }
            catch(Exception ex)
            {
                IO.EscreverArquivo(_arqLog, clienteAnterior.ToString(), null);
            }

            return string.IsNullOrWhiteSpace(cli) ? clienteAnterior.ToString() : cli;
        }
        
        
        /// <summary>
        /// Percorre recursivamente a lista de clientes pré-carregada e identifica os 'clientes anteriores', desde o cliente atual até o cliente original.
        /// Necessário que a lista inicial de clientes-base esteja em ordem decrescente de número de cliente.
        /// </summary>
        /// <param name="numeroCliente">Cliente a partir do qual se deseja identificar os anteriores.</param>
        /// <returns></returns>
        private Pangea.Entidades.Cliente BuscarClienteOriginal(string numeroCliente)
        {
            try
            {
                if (!dicClientesTodos.ContainsKey(numeroCliente))
                    return null;

                var cliTemp2 = dicClientesTodos[numeroCliente];

                while (cliTemp2 != null && !string.IsNullOrWhiteSpace(cliTemp2.Numero_cliente))
                {
                    if (!lstClientesFilhos.Contains(cliTemp2))
                        lstClientesFilhos.Add(cliTemp2);

                    if (string.IsNullOrWhiteSpace(cliTemp2.Cliente_anterior) || "0".Equals(cliTemp2.Cliente_anterior))
                        return cliTemp2;

                    cliTemp2 = BuscarClienteOriginal(cliTemp2.Cliente_anterior);
                }

                return cliTemp2;
            }
            catch(Exception ex)
            {
                return null;
            }
        }

        private Pangea.Entidades.Cliente BuscarClientePai(string numeroCliente)
        {
            try
            {
                if (!dicClientesTodos.ContainsKey(numeroCliente))
                    return null;

                var cliTemp2 = dicClientesTodos.Where(x => x.Value.Cliente_anterior.Equals(numeroCliente)).FirstOrDefault().Value;

                if (cliTemp2 == null)
                    return dicClientesTodos[numeroCliente];

                //while (cliTemp2 != null && !string.IsNullOrWhiteSpace(cliTemp2.Numero_cliente))
                //{
                    if (!lstClientesFilhos.Contains(cliTemp2))
                        lstClientesFilhos.Add(cliTemp2);

                    if (string.IsNullOrWhiteSpace(cliTemp2.Cliente_anterior) || "0".Equals(cliTemp2.Cliente_anterior))
                        return cliTemp2;

                    cliTemp2 = BuscarClientePai(cliTemp2.Numero_cliente);
                //}

                return cliTemp2;
            }
            catch (Exception ex)
            {
                return null;
            }
        }
    }
}

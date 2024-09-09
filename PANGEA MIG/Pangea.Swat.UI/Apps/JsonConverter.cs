using Newtonsoft.Json;
using Pangea.Dados;
using Pangea.Dados.Base;
using Pangea.Entidades.Enumeracao;
using Pangea.Swat.Dados;
using Pangea.Swat.UI.Entidades;
//using Pangea.Util;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Pangea.Swat.UI.Apps
{
    public partial class JsonConverter : Form
    {
        private DataTable dt = new DataTable();
        private Empresa empresa = Empresa.NaoIdentificada;
        private TipoCliente tipoCliente = TipoCliente.GA;
        //private Hashtable lista = new Hashtable();

        public JsonConverter()
        {
            InitializeComponent();
        }

        private void btnSair_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            sistemaOcupado(true);
            limparResultado();

            #region Validar Tipo Cliente
            if(!rbTipoClienteGA.Checked && !rbTipoClienteGB.Checked)
            {
                MessageBox.Show("Selecione o TIPO CLIENTE.");
                return;
            }
            #endregion

            this.tipoCliente = rbTipoClienteGA.Checked ? TipoCliente.GA : TipoCliente.GB;
            string fullPath = txtCaminhoArq.Text;
            List<string> lstClientes = new List<string>();

            #region Validações de entrada
            if (rbModoTexto.Checked)
            {
                if (string.IsNullOrEmpty(fullPath))
                {
                    exibirResultado("Informe o arquivo a ser convertido em formato JSON.");
                    return;
                }

                //validar arquivo de entrada
                if (!File.Exists(fullPath))
                {
                    exibirResultado(string.Format("Arquivo '{0}' não encontrado.", fullPath));
                    return;
                }

                if (rbModoTexto.Checked)
                    registrarHistorico(fullPath);

                using (StreamReader sr = new StreamReader(fullPath, Encoding.GetEncoding("iso-8859-1")))
                {
                    while (!sr.EndOfStream)
                    {
                        string linha = sr.ReadLine();
                        if (string.IsNullOrWhiteSpace(linha))
                            continue;
                        lstClientes.Add(linha);
                    }
                    //sr.Close();
                }
            }
            else
            {
                if (string.IsNullOrWhiteSpace(txtListaClientes.Text))
                {
                    exibirResultado("Lista de clientes não informada.");
                    txtListaClientes.Focus();
                    return;
                }

                if(!Directory.Exists(fullPath))
                {
                    exibirResultado(string.Format("Diretório '{0}' não encontrado.", fullPath));
                    return;
                }
                fullPath = string.Format("{0}\\{1}.txt", new DirectoryInfo(fullPath).FullName, DateTime.Now.ToString("yyyyMMdd"));
                lstClientes.AddRange(txtListaClientes.Text.Split(new char[] {','}));
            }
            #endregion

            DirectoryInfo folderRaiz = new DirectoryInfo(fullPath);

            //TODO: selecionar empresa
            this.empresa = Empresa.CE;

            AltaContratacaoDAO altaDao = FabricaDAO.getInstanceFabricaDAO(((int)this.empresa).ToString()).getInstanceAltaContratacaoDAO(this.empresa, this.tipoCliente);

            StringBuilder sb = new StringBuilder();
            StringBuilder _sbNomeArq = new StringBuilder();
            StringBuilder _sbArqErros = new StringBuilder();

            StringBuilder _arqFinal = new StringBuilder();
            StringBuilder _arqErros = new StringBuilder();

            StringBuilder sql = new StringBuilder();
            StringBuilder jsonObj =  new StringBuilder();
            StringBuilder clienteNaoEncontrado = new StringBuilder();
            int contClausilaIn = 0;
            int contTotal = 0;
            StringBuilder clausulaIn = new StringBuilder();
            List<string> processados = new List<string>();

            FileInfo arq = new FileInfo(fullPath);
            string dataAtual = DateTime.Now.ToString("yyyyMMdd");
            StringBuilder x = new StringBuilder();

            foreach (string linha in lstClientes)
            {
                if (string.IsNullOrEmpty(linha))
                    return;

                if (contClausilaIn > 0)
                    clausulaIn.Append(", ");

                clausulaIn.Append(linha.Trim());
                contClausilaIn++;
                contTotal++;

                if (contClausilaIn < 100)
                {
                    if (contTotal != lstClientes.Count)
                       continue;
                }

                sql.Clear();
                _sbNomeArq.Clear();

                try
                {
                    sql.AppendFormat(altaDao.GetConsultaBase(), clausulaIn.ToString());

                    DataTable dt = null;
                    try
                    {
                        dt = Utils.GetExecutor(empresa, tipoCliente).ExecutarDataTable(sql.ToString().Trim(), true);
                        contClausilaIn = 0;
                        clausulaIn.Clear();
                    }
                    catch(Exception ex)
                    {
                        Debugger.Break();
                        sistemaOcupado(false);
                        txtResultado.Text = string.Concat(ex.Message, ex.StackTrace);
                        //return;
                    }

                    if (dt != null && dt.Rows.Count > 0)
                    {
                        List<DeltaAltaContratacaoDTO> lstResultado = Pangea.Util.DataHelper.ConvertDataTableToList<DeltaAltaContratacaoDTO>(dt);
                        foreach (DeltaAltaContratacaoDTO alta in lstResultado)
                        {
                            //impede a repetição do cliente no JSON
                            if (processados.Contains(alta.IM_ZZ_NUMUTE.ToString()))
                                continue;

                            if (string.IsNullOrWhiteSpace(alta.BP_BPEXT) || string.IsNullOrWhiteSpace(alta.BP_ZZ_CODFISC))
                            {
                                x.AppendFormat("{0},",alta.IM_ZZ_NUMUTE.ToString());
                                continue;
                            }

                            _arqFinal.Clear();
                            _arqErros.Clear();

                            if (chkArquivoUnico.Checked)
                                jsonObj.AppendLine(string.Empty);

                            #region Preencher JSON
                            DeltaAltaContratacao obj = alta.ToJsonObject();

                            #region Campos Fixos
                            obj.Body.ZZ_CANALE_STAMPA = string.IsNullOrWhiteSpace(obj.Body.ZZ_CANALE_STAMPA) ? "X" : obj.Body.ZZ_CANALE_STAMPA;
                            obj.Body.ATTIVITA = "DEFINITIVE";
                            obj.Body.CLASSE = "MOVE IN";
                            obj.Header.FechaHora = obj.Body.DATA_VALIDITA;
                            obj.Body.MI_VENDE = obj.Body.DATA_VALIDITA;
                            obj.Body.BP_PARTNER = string.Empty;
                            obj.Body.ID_RICHIESTA = "999";
                            obj.Body.ID_RICHIESTA_FO = "9999";
                            obj.Body.ID_CASE_EXTERNAL = "999";
                            obj.Body.BP_PARTNER = string.Empty;
                            obj.Body.BP_SMTP_ADDR = string.Empty;
                            obj.Body.AC_EZAWE = "B";
                            obj.Body.AC_GSBER = "1";
                            obj.Body.AC_VKTYP = "1";
                            obj.Body.IM_FACTOR_1 = string.Empty;
                            obj.Body.IM_TRASNF_LOSS = 0;
                            #endregion

                            #region Regras de Negócio

                            obj.Body.AC_VKONT = obj.Body.IM_ZZ_NUMUTE.ToString();
                            obj.Body.BP_ADEXT_ADDR = string.Concat(obj.Body.IM_ZZ_NUMUTE, "_FA");
                            obj.Body.IM_ANLAGE = obj.Body.IM_ZZ_NUMUTE.ToString();
                            obj.Body.IM_FACTOR_3 = obj.Body.IM_FACTOR_2;

                            obj.Body.CO_STREET = extrairNumeroEndereco(obj.Body.BP_STREET, false);
                            obj.Body.BP_HOUSE_NUM1 = string.IsNullOrEmpty(obj.Body.CO_HOUSE_NUM1) ? extrairNumeroEndereco(obj.Body.BP_STREET, true) : obj.Body.CO_HOUSE_NUM1;
                            obj.Body.BP_STREET = obj.Body.CO_STREET;
                            obj.Body.CO_HOUSE_NUM1 = obj.Body.BP_HOUSE_NUM1;
                            
                            obj.Body.CO_CITY1 = obj.Body.BP_CITY1;
                            obj.Body.BP_POST_CODE1 = obj.Body.BP_POST_CODE1.Replace("-", "");
                            obj.Body.CO_POST_CODE1 = obj.Body.BP_POST_CODE1;

                            obj.Body.BP_TYPE = obj.IsPessoaFisica ? "1" : "2";
                            obj.Body.BP_TAXTYPE = obj.IsPessoaFisica ? "BR2" : "BR1";

                            obj.Body.BPKIND = obj.IsPessoaFisica ? "9001" : "9002";
                            obj.Body.BPKIND = "50".Equals(obj.Body.AC_KOFIZ_SD) ? "9003" : obj.Body.BPKIND;     //PODER PUBLICO

                            obj.Body.BP_REGION = obj.Empresa;
                            obj.Body.CO_REGION = obj.Empresa;

                            obj.Body.BP_EXECUTIVE = (string.IsNullOrEmpty(obj.Body.BP_EXECUTIVE)) ? string.Empty : string.Concat("E", obj.Body.BP_EXECUTIVE);
                            obj.Body.IM_TARIFTYP = (string.IsNullOrEmpty(obj.Body.IM_TARIFTYP)) ? string.Empty : obj.Body.IM_TARIFTYP.Split(new char[] { ' ' }).First();
                            obj.Body.IM_TEMP_AREA = obj.Body.IM_TEMP_AREA.Split(new char[] { ' ' })[0];

                            
                            //classe
                            obj.Body.AC_KOFIZ_SD = (string.IsNullOrEmpty(obj.Body.AC_KOFIZ_SD)) ? string.Empty : obj.Body.AC_KOFIZ_SD.Split(new char[] { ' ' }).First();
                            
                            //data vencimento
                            obj.Body.AC_ZAHLKOND = "0".Equals(obj.Body.AC_ZAHLKOND) ? "CP10" : string.Concat("CP", obj.Body.AC_ZAHLKOND.PadLeft(2, '0'));
                            
                            //carga
                            obj.Body.IM_CHARGE = (0 == obj.Body.IM_CHARGE) ? 15 : obj.Body.IM_CHARGE;

                            //EXCEÇÕES
                            if (string.IsNullOrWhiteSpace(obj.Body.IM_FACTOR_4))
                            {
                                _sbArqErros.Append("ERRO_");
                                obj.Body.IM_FACTOR_4 = "ERRO_SEM.MEDIDOR";
                            }

                            if(string.IsNullOrWhiteSpace(obj.Body.AC_KOFIZ_SD))
                            {
                                _sbArqErros.Append("ERRO_");
                                obj.Body.AC_KOFIZ_SD = "ERRO_SEM.CLASSE";
                            }

                            if (string.IsNullOrWhiteSpace(obj.Body.IM_TARIFTYP))
                            {
                                _sbArqErros.Append("ERRO_");
                                obj.Body.IM_TARIFTYP = "ERRO_SEM.TARIFA";
                            }

                            if (string.IsNullOrWhiteSpace(obj.Body.IM_TEMP_AREA))
                            {
                                _sbArqErros.Append("ERRO_");
                                obj.Body.IM_TEMP_AREA = "ERRO_SEM.SUBCLASSE";
                            }

                            //DEMANDAS:
                            //comentados os campos que permanecerão como retornados do banco.  os demais serão sobrescritos
                            if (obj.IsClienteLivre)
                            {
                                if (obj.Body.MODTARIFBR.ToUpper().Contains("VERDE"))
                                {
                                    if ("0".Equals(obj.Body.IM_DI_CONTRAT))
                                    {
                                        _sbArqErros.Append("ERRO_");
                                        obj.Body.IM_TARIFTYP = "ERRO_DEMANDA.0.MODTARIFBR.LIVRE.VERDE";
                                    }
                                    obj.Body.IM_DI_CONTRAT = 0;
                                    obj.Body.IM_DI_CONTRPT = 0;
                                    obj.Body.IM_DI_CONTRFP = 0;
                                    obj.Body.IM_DI_CONTUL = obj.Body.IM_DI_CONTRAT;
                                    obj.Body.IM_DI_CONTPTL = 0;
                                    obj.Body.IM_DI_CONTFPL = 0;
                                }
                                if (obj.Body.MODTARIFBR.ToUpper().Contains("AZUL"))
                                {
                                    if ("0".Equals(obj.Body.IM_DI_CONTRPT) || "0".Equals(obj.Body.IM_DI_CONTRFP))
                                    {
                                        _sbArqErros.Append("ERRO_");
                                        obj.Body.IM_TARIFTYP = "ERRO_DEMANDA.0.MODTARIFBR.LIVRE.AZUL";
                                    }
                                    obj.Body.IM_DI_CONTRAT = 0;
                                    obj.Body.IM_DI_CONTRPT = 0;
                                    obj.Body.IM_DI_CONTRFP = 0;
                                    obj.Body.IM_DI_CONTUL = 0;
                                    obj.Body.IM_DI_CONTPTL = obj.Body.IM_DI_CONTRPT;
                                    obj.Body.IM_DI_CONTFPL = obj.Body.IM_DI_CONTRFP;
                                }
                            }
                            else
                            {
                                if (obj.Body.MODTARIFBR.ToUpper().Contains("VERDE"))
                                {
                                    if ("0".Equals(obj.Body.IM_DI_CONTRAT))
                                    {
                                        _sbArqErros.Append("ERRO_");
                                        obj.Body.IM_TARIFTYP = "ERRO_DEMANDA.0.MODTARIFBR.NAO.LIVRE.VERDE";
                                    }
                                    //obj.Body.IM_DI_CONTRAT = "0";
                                    obj.Body.IM_DI_CONTRPT = 0;
                                    obj.Body.IM_DI_CONTRFP = 0;
                                    obj.Body.IM_DI_CONTUL = 0;
                                    obj.Body.IM_DI_CONTPTL = 0;
                                    obj.Body.IM_DI_CONTFPL = 0;
                                }
                                if (obj.Body.MODTARIFBR.ToUpper().Contains("AZUL"))
                                {
                                    if ("0".Equals(obj.Body.IM_DI_CONTRPT) || "0".Equals(obj.Body.IM_DI_CONTRFP))
                                    {
                                        _sbArqErros.Append("ERRO_");
                                        obj.Body.IM_TARIFTYP = "ERRO_DEMANDA.0.MODTARIFBR.NAO.LIVRE.AZUL";
                                    }
                                    obj.Body.IM_DI_CONTRAT = 0;
                                    //obj.Body.IM_DI_CONTRPT = "0";
                                    //obj.Body.IM_DI_CONTRFP = "0";
                                    obj.Body.IM_DI_CONTUL = 0;
                                    obj.Body.IM_DI_CONTPTL = 0;
                                    obj.Body.IM_DI_CONTFPL = 0;
                                }
                            }

                            //TENSÃO
                            switch (obj.Body.IM_SPEBENE.Trim())
                            {
                                case "13":
                                    obj.Body.IM_SPEBENE = "31";
                                    break;
                                case "34":
                                    obj.Body.IM_SPEBENE = "44";
                                    break;
                                case "69":
                                    obj.Body.IM_SPEBENE = "53";
                                    break;
                                case "24":
                                    obj.Body.IM_SPEBENE = "58";
                                    break;
                                case "23":
                                    obj.Body.IM_SPEBENE = "60";
                                    break;
                            }

                            try
                            {
                                if (obj.IsPessoaFisica)
                                {
                                    //destaca o nome do sobrenome
                                    string[] _nomes = obj.Body.BP_NAME_ORG1.Split(new char[] { ' ' });
                                    string _sobrenome = string.Join(" ", _nomes, 1, (_nomes.Count() - 1));

                                    obj.Body.BP_NAME_ORG1 = string.Empty;
                                    obj.Body.BP_NAME_FIRST = _nomes[0];
                                    obj.Body.BP_NAME_LAST = _sobrenome;

                                    obj.Body.BP_BPEXT = string.Concat(Utils.FormatarDocumento(obj.Body.BP_BPEXT, TipoDocumento.CPFGB), "CPF");

                                    obj.Body.BP_ZZ_CODFISC = Utils.FormatarDocumento(obj.Body.BP_ZZ_CODFISC, obj.TipoIdentidade);
                                }
                                else
                                {
                                    obj.Body.BP_NAME_FIRST = string.Empty;
                                    obj.Body.BP_NAME_LAST = string.Empty;

                                    obj.Body.BP_BPEXT = string.Concat(Utils.FormatarDocumento(obj.Body.BP_BPEXT, TipoDocumento.CNPJ), "CNPJ");

                                    obj.Body.BP_ZZ_CODFISC = Utils.FormatarDocumento(obj.Body.BP_ZZ_CODFISC, obj.TipoIdentidade);
                                }
                            }
                            catch(Exception ex)
                            {
                                continue;
                            }

                            #endregion Regras de Negócio

                            #region Serialização para JSON
                            JsonSerializerSettings sett = new JsonSerializerSettings();
                            sett.Culture = CultureInfo.GetCultureInfo("pt-BR");
                            sett.NullValueHandling = NullValueHandling.Include;
                            sett.DefaultValueHandling = DefaultValueHandling.Populate;

                            jsonObj.Append(JsonConvert.SerializeObject(obj, (chkFormatJson.Checked) ? Formatting.Indented : Formatting.None, sett));
                            #endregion

                            #endregion  Preencher JSON

                            processados.Add(alta.IM_ZZ_NUMUTE.ToString());

                            _arqFinal.Append(string.Concat(arq.DirectoryName, "\\", _sbNomeArq.ToString(), "ALTA_MANUAL_", DateTime.Now.ToString("yyyyMMdd"), "_", obj.Body.IM_ZZ_NUMUTE.ToString().Trim()));

                            if (!chkArquivoUnico.Checked)
                            {
                                IO.EscreverArquivo(_arqFinal.ToString(), jsonObj.ToString().Trim(), null);
                                jsonObj.Clear();
                            }
                        }
                    }
                    else
                    {
                        _sbArqErros.AppendLine(string.Format("[NAO ENCONTRADO] {0}", linha));
                        _arqFinal.Clear();
                        clienteNaoEncontrado.AppendLine(string.Format("[NAO ENCONTRADO] {0}", linha));
                        _arqFinal.Append(string.Concat(arq.DirectoryName, "\\", _sbNomeArq.ToString(), "ALTA_MANUAL_", dataAtual));
                        IO.EscreverArquivo(_arqFinal.ToString(), clienteNaoEncontrado.ToString().Trim(), null);
                        clienteNaoEncontrado.Clear();
                    }

                    #region Identificar Clientes solicitados e não encontrados no Synergia
                    List<string> cliNotFound = new List<string>();
                    if (!chkArquivoUnico.Checked)
                    {
                        foreach (string cli in lstClientes)
                        {
                            var c = from clientes in dt.AsEnumerable()
                                      where clientes.Field<int>("IM_ZZ_NUMUTE") == Int32.Parse(cli)
                                      select new
                                      {
                                          cliente = clientes.Field<int>("IM_ZZ_NUMUTE")
                                      };
                            if (c.Count() == 0)
                                cliNotFound.Add(cli);
                        }

                        foreach (string cli in cliNotFound)
                        {
                            _arqFinal.Clear();
                            _arqFinal.Append(string.Concat(arq.DirectoryName, "\\", _sbNomeArq.ToString(), "NAO_ENCONTRADO_", DateTime.Now.ToString("yyyyMMdd"), "_", cli.Trim()));
                            IO.EscreverArquivo(_arqFinal.ToString(), jsonObj.ToString().Trim(), null);
                        }
                    }

                    cliNotFound = null;
                    #endregion

                    dt = null;
                }
                catch (Exception ex)
                {
                    //registrar log
                    //throw new Exception(string.Format("{0}{1}{1} O comando foi executado em {2} registros.", ex, Environment.NewLine, cont));
                }
            }

            if (chkArquivoUnico.Checked)
            {
                _arqFinal.Clear();
                _arqFinal.Append(string.Concat(folderRaiz.Parent.FullName, "\\", "ALTA_UNICA_SAP_", DateTime.Now.ToString("yyyyMMdd_HHmm")));
                IO.EscreverArquivo(_arqFinal.ToString(), jsonObj.ToString().Trim(), null);

                if (_sbArqErros.Length > 0)
                {
                    _arqErros.Clear();
                    _arqErros.Append(string.Concat(folderRaiz.Parent.FullName, "\\", "ALTA_UNICA_SAP_ERROS_", DateTime.Now.ToString("yyyyMMdd_HHmm")));
                    IO.EscreverArquivo(_arqErros.ToString(), _sbArqErros.ToString().Trim(), null);
                }
            }

            txtResultado.Text = "Processo finalizado.";
            sistemaOcupado(false);
        }



        private void sistemaOcupado(bool ocupado)
        {
            this.Cursor = ocupado ? Cursors.WaitCursor : Cursors.Default;
        }


        /// <summary>
        /// Tenta identificar o número de uma UC com base no endereço completo informado.
        /// Percorre o endereço e procura o primeiro número após o seguinte padrão: "TIPO LOGRADOURO|NOME LOGRADOURO|NUMERO"
        /// Exemplos: "RUA 2, 109", "AV. PROF. 80"
        /// </summary>
        /// <param name="rua"></param>
        /// <returns>Endereço sem o número ou somente o número.</returns>
        private string extrairNumeroEndereco(string rua, bool somenteNumero)
        {
            const string SEM_NUMERO = "S/N";
            if (!somenteNumero && string.IsNullOrEmpty(rua))
                return "ERRO ENDEREÇO";

            if (somenteNumero && string.IsNullOrEmpty(rua))
                return SEM_NUMERO;

            string[] e = rua.Split(new char[] { ' ' });
            if (e.Count() <= 2)
                return SEM_NUMERO;

            int numero = 0;
            for (int o = 2; o < e.Count(); o++ )
            {
                if(Int32.TryParse(e[o], out numero))
                {
                    if (somenteNumero)
                        return e[o];

                    List<string> temp = e.ToList();
                    temp.RemoveAt(o);
                    
                    return string.Join(" ", temp.ToArray());
                }
            }
            return somenteNumero ? SEM_NUMERO : rua;
        }

        private void registrarHistorico(string fullPath)
        {
            ConfigurationManager.AppSettings.Set("JsonHistorico3", ConfigurationManager.AppSettings.Get("JsonHistorico2"));
            ConfigurationManager.AppSettings.Set("JsonHistorico2", ConfigurationManager.AppSettings.Get("JsonHistorico1"));
            ConfigurationManager.AppSettings.Set("JsonHistorico1", fullPath);
        }



        private void exibirResultado(string msg)
        {
            if(string.IsNullOrEmpty(msg))
                return;

            this.txtResultado.Text = string.Empty;
            this.txtResultado.Text = msg.Trim();
        }

        private void JsonConverter_Load(object sender, EventArgs e)
        {
            txtListaClientes.Text = ConfigurationManager.AppSettings.Get("JsonHistorico1");
            txtHistorico.Text = string.Concat(ConfigurationManager.AppSettings.Get("JsonHistorico1"),Environment.NewLine,
                ConfigurationManager.AppSettings.Get("JsonHistorico2"), Environment.NewLine,
                ConfigurationManager.AppSettings.Get("JsonHistorico3"), Environment.NewLine);
        }

        private void radioButton2_CheckedChanged(object sender, EventArgs e)
        {
            lblModoGeracao.Text = "Arquivo";
            txtListaClientes.Text = @"";
            txtListaClientes.Focus();
        }

        private void rbModoTexto_CheckedChanged(object sender, EventArgs e)
        {
            limparResultado();
            txtListaClientes.Enabled = false;
            txtCaminhoArq.Focus();
        }

        private void rbModoLista_CheckedChanged(object sender, EventArgs e)
        {
            limparResultado();
            txtListaClientes.Enabled = true;
            txtListaClientes.Focus();
        }

        private void limparResultado()
        {
            txtResultado.Text = string.Empty;
        }
    }
}

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web.UI;
using System.Web.UI.WebControls;
using BotaoEmpresa.entidades;
using BotaoEmpresa.utils;
using Entidades.BotaoEmpresa.CalendarioLeitura;
using Entidades.BotaoEmpresa.CodFaturamentoLeiturista;
using Entidades.BotaoEmpresa.TarifaGa;
using Entidades.BotaoEmpresa.TarifaMedidor;
using Entidades.BotaoEmpresa.TarifasIp;
using Newtonsoft.Json;

namespace BotaoEmpresa
{
    public partial class _Default : Page
    {
        private List<PisCofinsTarifasGa> tarifasGaPisCofins;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (Session["AdminHistOrdem"] == null)
            {
                try
                {
                    Control loginRedeBr = (Control)Page.LoadControl("~/Controls/LoginRedeBR.ascx");
                    pnlHistOrdem.Controls.Add(loginRedeBr);
                }
                catch (Exception exception)
                {
                    Response.Redirect("/");
                }
            }
            else
            {
                Control historicoDeOrdem = (Control)Page.LoadControl("~/Controls/HistoricoDeOrdem.ascx");
                pnlHistOrdem.Controls.Add(historicoDeOrdem);
                ScriptManager.RegisterStartupScript(UpdatePanel1, UpdatePanel1.GetType(), System.Guid.NewGuid().ToString(), "$('.nav-tabs a[href=\"#HistoricoDeOrdens\"]').tab('show');", true);
            }
            UpdatePanel1.Update();

            if (string.IsNullOrEmpty(DropDownList_Empresa.SelectedValue))
            {
                return;
            }
            tarifasGaPisCofins = FormatarPisCofinsTarifasGa();
        }

        #region TARIFAS ILUMINAÇÃO PUBLICA

        protected void MunicipioTarifasIp_OnSelectedIndexChanged(object sender, EventArgs e)
        {
            if (Session["TarifasIluminacaoPublica"] != null)
            {
                if (MunicipioTarifasIp.SelectedValue != null)
                {
                    string lcodigoMunicipio = MunicipioTarifasIp.SelectedValue;
                    CarregarValoresTarifasIp(lcodigoMunicipio);
                }
                RefreshTablesWithScripts();
            }
            else
            {
                CarregarMunicipiosIluminacaoPublica();
            }
        }

        private void CarregarValoresTarifasIp(string municipio)
        {

            List<IluminacaoPublica> IpData = (List<IluminacaoPublica>)Session["TarifasIluminacaoPublica"];

            List<IluminacaoPublica> listaFiltrada = IpData.Where(x => x.Cidade == municipio).ToList();

            StringBuilder tarifasIP = new StringBuilder();
            if (listaFiltrada.Count > 0)
            {
                tarifasIP.Append(@"
                        <table id=""tarifas-ilum-pub"">
                            <thead>
                                <tr>
                                    <th>Válido Desde</th>
                                    <th style=""150px"">Classe de cálculo</th>
                                    <th style=""150px"">Escalão-de</th>
                                    <th style=""150px"">Até escalão</th>
                                    <th style=""150px"">Valor Da TIP</th>
                                    <th style=""150px"">Valor Origem</th>
                                    <th style=""150px"">Quota taxa conc.PN</th>
                                    <th style=""150px"">Tipo da Tarifa</th>
                                </tr>
                            </thead>
                            <tbody>");

                //POPULA OS HEADERS
                int indicePai = 1;
                bool controle = true;
                for (int i = 0; i < listaFiltrada.Count - 1; i++)
                {

                    string Data = listaFiltrada[i].DataVigencia;
                    Data = Data.Insert(6, " / ");
                    Data = Data.Insert(4, " / ");


                    tarifasIP.AppendFormat(@"
                                <tr data-tt-id=""{8}"">
                                    <td>{0}</td>
                                    <td>{1}</td>
                                    <td>{2}</td>
                                    <td>{3}</td>
                                    <td>{4}</td>
                                    <td>{5}</td>
                                    <td>{6}</td>
                                    <td>{7}</td>
                                </tr>",

                                  controle ? Convert.ToDateTime(Data).ToString("dd/MM/yyyy") : "",
                                  controle ? listaFiltrada[i].DescricaoGrupo : "",
                                          listaFiltrada[i].KwhMin,
                                          listaFiltrada[i].KwhMax,
                                          (Convert.ToDouble(listaFiltrada[i].Tarifa) * Convert.ToDouble(listaFiltrada[i].ValorOrigem) / 100).ToString("0.00"),
                                          listaFiltrada[i].ValorOrigem,
                                          Convert.ToDouble(listaFiltrada[i].Tarifa).ToString("0.00"),
                                          listaFiltrada[i].DescricaoTipo,
                                          indicePai);
                    controle = listaFiltrada[i].DataVigencia == listaFiltrada[i + 1].DataVigencia ? false : true;

                    indicePai++;
                }

                tarifasIP.Append(@"
                            </tbody>
                        </table>");
            }
            else
            {
                tarifasIP.Append(@"
                        <table id=""tarifas-ilum-pub"">
                            <thead>
                                <tr>
                                    <th>Nenhum dado encontrado!</th>
                                </tr>
                            </thead>
                            <tbody></tbody>
                        </table>");
            }
            TarifasIluminacaoPublica.InnerHtml = tarifasIP.ToString();

            RefreshTablesWithScripts();
        }


        public void CarregarMunicipiosIluminacaoPublica()
        {
            List<IluminacaoPublica> ipData = new List<IluminacaoPublica>();

            List<Municipio> municipios = new List<Municipio>();

            using (StreamReader tr = new StreamReader(Server.MapPath(string.Format("~/content/lst/botao-empresa/{0}/iluminacao_publica.lst", DropDownList_Empresa.SelectedValue)), Encoding.GetEncoding("iso-8859-1")))
            {
                string line;
                while ((line = tr.ReadLine()) != null)
                {
                    string[] items = line.Trim().Split('|');

                    //AABBXRTR|20190422|0|30|0,34|467,05

                    IluminacaoPublica item = new IluminacaoPublica();
                    item.DataVigencia = items[1];
                    item.KwhMin = items[2];
                    item.KwhMax = items[3];
                    item.Tarifa = items[4];
                    item.ValorOrigem = items[5];
                    item.Cidade = items[0].Substring(0, 2);
                    item.Grupo = items[0].Substring(2, 1);
                    item.Tipo = items[0].Substring(3, 3);
                    item.TipoTarifa = items[0].Substring(6, 2);
                    item.DescricaoTipo = TraduzirTipo(items[0].Substring(3, 3));
                    item.DescricaoGrupo = TraduzirGrupo(items[0].Substring(2, 1));
                    ipData.Add(item);

                    Municipio cod = new Municipio();
                    cod.CodGp = items[0].Substring(2, 1);
                    cod.DescricaoGp = TraduzirGrupo(cod.CodGp);
                    cod.CodINE = items[0].Substring(0, 2);
                    cod.Descricao = TraduzirCodigoCidadeSap(cod.CodINE);
                    municipios.Add(cod);
                }
                Session["TarifasIluminacaoPublica"] = ipData;

            }

            MunicipioTarifasIp.DataSource = municipios.GroupBy(id => id.CodINE).Select(g => g.First()).ToList().OrderBy(x => x.Descricao);
            MunicipioTarifasIp.DataTextField = "DescricaoGp";
            MunicipioTarifasIp.DataTextField = "Descricao";
            MunicipioTarifasIp.DataValueField = "CodINE";
            MunicipioTarifasIp.DataBind();
            MunicipioTarifasIp.Items.Insert(0, new ListItem("SELECIONE", "-1"));
        }

        private string TraduzirCodigoCidadeSap(string codigo)
        {
            string valor = string.Empty;
            switch (codigo)
            {
                case "AB":
                    valor = "ABAIARA";
                    break;

                case "AE":
                    valor = "ACARAPE";
                    break;

                case "AU":
                    valor = "ACARAU";
                    break;

                case "AR":
                    valor = "ACOPIARA";
                    break;

                case "AI":
                    valor = "AIUABA";
                    break;

                case "AL":
                    valor = "ALCANTARAS";
                    break;

                case "LR":
                    valor = "ALTANEIRA";
                    break;

                case "AS":
                    valor = "ALTO SANTO";
                    break;

                case "AM":
                    valor = "AMONTADA";
                    break;

                case "AN":
                    valor = "ANTONINA DO NORTE";
                    break;

                case "AP":
                    valor = "APUIARES";
                    break;

                case "AQ":
                    valor = "AQUIRAZ";
                    break;

                case "AT":
                    valor = "ARACATI";
                    break;

                case "AC":
                    valor = "ARACOIABA";
                    break;

                case "AD":
                    valor = "ARARENDA";
                    break;

                case "RR":
                    valor = "ARARIPE";
                    break;

                case "AA":
                    valor = "ARATUBA";
                    break;

                case "AZ":
                    valor = "ARNEIROZ";
                    break;

                case "SE":
                    valor = "ASSARE";
                    break;

                case "AO":
                    valor = "AURORA";
                    break;

                case "BX":
                    valor = "BAIXIO";
                    break;

                case "BN":
                    valor = "BANABUIU";
                    break;

                case "BL":
                    valor = "BARBALHA";
                    break;

                case "BA":
                    valor = "BARREIRA";
                    break;

                case "BR":
                    valor = "BARRO";
                    break;

                case "BQ":
                    valor = "BARROQUINHA";
                    break;

                case "BT":
                    valor = "BATURITE";
                    break;

                case "BE":
                    valor = "BEBERIBE";
                    break;

                case "BC":
                    valor = "BELA CRUZ";
                    break;

                case "BV":
                    valor = "BOA VIAGEM";
                    break;

                case "BS":
                    valor = "BREJO SANTO";
                    break;

                case "CM":
                    valor = "CAMOCIM";
                    break;

                case "CS":
                    valor = "CAMPOS SALES";
                    break;

                case "CN":
                    valor = "CANINDE";
                    break;

                case "CP":
                    valor = "CAPISTRANO";
                    break;

                case "CD":
                    valor = "CARIDADE";
                    break;

                case "RI":
                    valor = "CARIRE";
                    break;

                case "CC":
                    valor = "CARIRIACU";
                    break;

                case "US":
                    valor = "CARIUS";
                    break;

                case "CB":
                    valor = "CARNAUBAL";
                    break;

                case "CV":
                    valor = "CASCAVEL";
                    break;

                case "CI":
                    valor = "CATARINA";
                    break;

                case "CU":
                    valor = "CATUNDA";
                    break;

                case "CA":
                    valor = "CAUCAIA";
                    break;

                case "CE":
                    valor = "CEDRO";
                    break;

                case "CH":
                    valor = "CHAVAL";
                    break;

                case "HO":
                    valor = "CHORO";
                    break;

                case "CZ":
                    valor = "CHOROZINHO";
                    break;

                case "CO":
                    valor = "COREAU";
                    break;

                case "CT":
                    valor = "CRATEUS";
                    break;

                case "CR":
                    valor = "CRATO";
                    break;

                case "RO":
                    valor = "CROATA";
                    break;

                case "RZ":
                    valor = "CRUZ";
                    break;

                case "DI":
                    valor = "DEP.IRAP.PINHEIRO";
                    break;

                case "ER":
                    valor = "ERERE";
                    break;

                case "EB":
                    valor = "EUSEBIO";
                    break;

                case "FB":
                    valor = "FARIAS BRITO";
                    break;

                case "FQ":
                    valor = "FORQUILHA";
                    break;

                case "FO":
                    valor = "FORTALEZA";
                    break;

                case "FT":
                    valor = "FORTIM";
                    break;

                case "FC":
                    valor = "FRECHEIRINHA";
                    break;

                case "GS":
                    valor = "GENERAL SAMPAIO";
                    break;

                case "GC":
                    valor = "GRACA";
                    break;

                case "GJ":
                    valor = "GRANJA";
                    break;

                case "GG":
                    valor = "GRANJEIRO";
                    break;

                case "GR":
                    valor = "GROAIRAS";
                    break;

                case "GB":
                    valor = "GUAIUBA";
                    break;

                case "GN":
                    valor = "GUARACIABA DO NORTE";
                    break;

                case "GM":
                    valor = "GUARAMIRANGA";
                    break;

                case "HI":
                    valor = "HIDROLANDIA";
                    break;

                case "HZ":
                    valor = "HORIZONTE";
                    break;

                case "BM":
                    valor = "IBARETAMA";
                    break;

                case "BP":
                    valor = "IBIAPINA";
                    break;

                case "BG":
                    valor = "IBICUITINGA";
                    break;

                case "II":
                    valor = "ICAPUI";
                    break;

                case "IO":
                    valor = "ICO";
                    break;

                case "IG":
                    valor = "IGUATU";
                    break;

                case "ID":
                    valor = "INDEPENDENCIA";
                    break;

                case "IP":
                    valor = "IPAPORANGA";
                    break;

                case "IM":
                    valor = "IPAUMIRIM";
                    break;

                case "IU":
                    valor = "IPU";
                    break;

                case "IS":
                    valor = "IPUEIRAS";
                    break;

                case "IR":
                    valor = "IRACEMA";
                    break;

                case "IB":
                    valor = "IRAUÇUBA";
                    break;

                case "TB":
                    valor = "ITAICABA";
                    break;

                case "IT":
                    valor = "ITAITINGA";
                    break;

                case "IJ":
                    valor = "ITAPAJE";
                    break;

                case "IC":
                    valor = "ITAPIPOCA";
                    break;

                case "IN":
                    valor = "ITAPIUNA";
                    break;

                case "IE":
                    valor = "ITAREMA";
                    break;

                case "IA":
                    valor = "ITATIRA";
                    break;

                case "JJ":
                    valor = "J.DE JERICOACOARA";
                    break;

                case "JM":
                    valor = "JAGUARETAMA";
                    break;

                case "JR":
                    valor = "JAGUARIBARA";
                    break;

                case "JB":
                    valor = "JAGUARIBE";
                    break;

                case "JG":
                    valor = "JAGUARUANA";
                    break;

                case "JD":
                    valor = "JARDIM";
                    break;

                case "JT":
                    valor = "JATI";
                    break;

                case "JN":
                    valor = "JUAZEIRO DO NORTE";
                    break;

                case "JC":
                    valor = "JUCAS";
                    break;

                case "LM":
                    valor = "L. DA MANGABEIRA";
                    break;

                case "LN":
                    valor = "LIM. DO NORTE";
                    break;

                case "MD":
                    valor = "MADALENA";
                    break;

                case "MA":
                    valor = "MARACANAU";
                    break;

                case "MG":
                    valor = "MARANGUAPE";
                    break;

                case "MC":
                    valor = "MARCO";
                    break;

                case "MP":
                    valor = "MARTINOPOLE";
                    break;

                case "MS":
                    valor = "MASSAPE";
                    break;

                case "MR":
                    valor = "MAURITI";
                    break;

                case "ME":
                    valor = "MERUOCA";
                    break;

                case "MI":
                    valor = "MILAGRES";
                    break;

                case "ML":
                    valor = "MILHA";
                    break;

                case "MM":
                    valor = "MIRAIMA";
                    break;

                case "MV":
                    valor = "MISSAO VELHA";
                    break;

                case "MO":
                    valor = "MOMBACA";
                    break;

                case "MT":
                    valor = "MONSENHOR TABOSA";
                    break;

                case "MN":
                    valor = "MORADA NOVA";
                    break;

                case "MJ":
                    valor = "MORAUJO";
                    break;

                case "MH":
                    valor = "MORRINHOS";
                    break;

                case "MB":
                    valor = "MUCAMBO";
                    break;

                case "MU":
                    valor = "MULUNGU";
                    break;

                case "NO":
                    valor = "NOVA OLINDA";
                    break;

                case "NR":
                    valor = "NOVA RUSSAS";
                    break;

                case "NT":
                    valor = "NOVO ORIENTE";
                    break;

                case "OC":
                    valor = "OCARA";
                    break;

                case "OR":
                    valor = "OROS";
                    break;

                case "PJ":
                    valor = "PACAJUS";
                    break;

                case "PT":
                    valor = "PACATUBA";
                    break;

                case "PC":
                    valor = "PACOTI";
                    break;

                case "PA":
                    valor = "PACUJA";
                    break;

                case "PL":
                    valor = "PALHANO";
                    break;

                case "PM":
                    valor = "PALMACIA";
                    break;

                case "PR":
                    valor = "PARACURU";
                    break;

                case "PP":
                    valor = "PARAIPABA";
                    break;

                case "PU":
                    valor = "PARAMBU";
                    break;

                case "PI":
                    valor = "PARAMOTI";
                    break;

                case "PB":
                    valor = "PEDRA BRANCA";
                    break;

                case "PE":
                    valor = "PENAFORTE";
                    break;

                case "PS":
                    valor = "PENTECOSTE";
                    break;

                case "PO":
                    valor = "PEREIRO";
                    break;

                case "PD":
                    valor = "PINDORETAMA";
                    break;

                case "PQ":
                    valor = "PIQUET CARNEIRO";
                    break;

                case "PF":
                    valor = "PIRES FERREIRA";
                    break;

                case "PN":
                    valor = "PORANGA";
                    break;

                case "OT":
                    valor = "PORTEIRAS";
                    break;

                case "PG":
                    valor = "POTENGI";
                    break;

                case "OM":
                    valor = "POTIRETAMA";
                    break;

                case "QP":
                    valor = "QUITERIANOPOLIS";
                    break;

                case "QX":
                    valor = "QUIXADA";
                    break;

                case "QL":
                    valor = "QUIXELO";
                    break;

                case "QB":
                    valor = "QUIXERAMOBIM";
                    break;

                case "QR":
                    valor = "QUIXERE";
                    break;

                case "RE":
                    valor = "REDENCAO";
                    break;

                case "RT":
                    valor = "RERIUTABA";
                    break;

                case "RU":
                    valor = "RUSSAS";
                    break;

                case "SG":
                    valor = "S GONCALO DO AMARANTE";
                    break;

                case "SJ":
                    valor = "S.JOAO JAGUARIBE";
                    break;

                case "SL":
                    valor = "S.LUIS DO CURU";
                    break;

                case "SR":
                    valor = "SABOEIRO";
                    break;

                case "ST":
                    valor = "SALITRE";
                    break;

                case "SA":
                    valor = "SANTANA DO ACARAU";
                    break;

                case "SC":
                    valor = "SANTANA DO CARIRI";
                    break;

                case "SB":
                    valor = "SAO BENEDITO";
                    break;

                case "SP":
                    valor = "SENADOR POMPEU";
                    break;

                case "SS":
                    valor = "SENADOR SA";
                    break;

                case "SO":
                    valor = "SOBRAL";
                    break;

                case "SN":
                    valor = "SOLONOPOLE";
                    break;

                case "SQ":
                    valor = "STA QUITERIA";
                    break;

                case "TN":
                    valor = "TABULEIRO DO NORTE";
                    break;

                case "TL":
                    valor = "TAMBORIL";
                    break;

                case "TF":
                    valor = "TARRAFAS";
                    break;

                case "TA":
                    valor = "TAUA";
                    break;

                case "TE":
                    valor = "TEJUCUOCA";
                    break;

                case "TG":
                    valor = "TIANGUA";
                    break;

                case "TR":
                    valor = "TRAIRI";
                    break;

                case "TU":
                    valor = "TURURU";
                    break;

                case "UB":
                    valor = "UBAJARA";
                    break;

                case "UM":
                    valor = "UMARI";
                    break;

                case "UR":
                    valor = "UMIRIM";
                    break;

                case "UT":
                    valor = "URUBURETAMA";
                    break;

                case "UC":
                    valor = "URUOCA";
                    break;

                case "VJ":
                    valor = "VARJOTA";
                    break;

                case "VA":
                    valor = "VARZEA ALEGRE";
                    break;

                case "VC":
                    valor = "VICOSA CEARA";
                    break;


                default:
                    break;
            }
            return valor;
        }

        private string TraduzirTipo(string codigo)
        {
            string valor = string.Empty;
            switch (codigo)
            {
                case "BXR":
                    valor = "BAIXA RENDA";
                    break;

                case "COM":
                    valor = "COMERCIAL";
                    break;

                case "IND":
                    valor = "INDUSTRIAL";
                    break;

                case "RES":
                    valor = "RESIDENCIAL";
                    break;

                case "CPR":
                    valor = "CONSUMO PRÓPRIO";
                    break;

                case "PPB":
                    valor = "PODER PUBLICO MUNICIPAL";
                    break;

                case "PPE":
                    valor = "PODER PUBLICO ESTADUAL";
                    break;

                case "PPF":
                    valor = "PODER PUBLICO FEDERAL";
                    break;

                case "RUR":
                    valor = "RURAL";
                    break;

                case "SPU":
                    valor = "SERVIÇO PUBLICO";
                    break;


                default:
                    break;
            }
            return valor;
        }

        private string TraduzirGrupo(string codigo)
        {
            string valor = string.Empty;
            switch (codigo)
            {
                case "A":
                    valor = "GRUPO A";
                    break;

                case "B":
                    valor = "GRUPO B";
                    break;

                default:
                    break;
            }
            return valor;
        }



        #endregion

        #region TARIFAS GA
        private void PreencheTabelaComTarifasGa()
        {
            List<Tarifa> tarifas = FormatarTarifasGa();

            StringBuilder tarifasGa = new StringBuilder();

            tarifasGa.Append(@"
                        <table id=""tarifas-ga"">
                            <thead>
                                <tr>
                                    <th>Código</th>
                                    <th>Sub-Grupo</th>
                                    <th>Vigência</th>
                                    <th>Tarifa Base (R$)</th>
                                    <th>Tarifa com PIS/COFINS (R$)</th>
                                </tr>
                            </thead>
                            <tbody>");

            //POPULA OS HEADERS
            int indicePai = 1;
            foreach (var tarifa in tarifas)
            {
                int indiceFilha = 1;
                tarifasGa.AppendFormat(@"
                                <tr data-tt-id=""{5}"">
                                    <td>{0}</td>
                                    <td>{1}</td>
                                    <td>{2}</td>
                                    <td>{3}</td>
                                    <td>{4}</td>
                                </tr>",
                                      tarifa.CodigoTarifa,
                                      tarifa.DescricaoTarifa.Descricao,
                                      tarifa.PrecoTarifa[0].DataAumento,
                                      Convert.ToDouble(tarifa.PrecoTarifa[0].ValorTarifa.Replace('.', ',')).ToString("0.00000"),
                                      CalcularValorPisCofinsTarifa(tarifa.PrecoTarifa[0].DataAumento, tarifa.PrecoTarifa[0].ValorTarifa, tarifasGaPisCofins, true).ToString("0.00000"),
                                      indicePai);

                foreach (var preco in tarifa.PrecoTarifa.Skip(1))
                {
                    tarifasGa.AppendFormat(@"
                                <tr data-tt-id=""{4}"" data-tt-parent-id=""{3}"">
                                    <td style=""background-color: #efefef;""></td>
                                    <td style=""background-color: #efefef;""></td>
                                    <td style=""background-color: #efefef;"">{0}</td>
                                    <td style=""background-color: #efefef;"">{1}</td>
                                    <td style=""background-color: #efefef;"">{2}</td>
                                </tr>",
                                      preco.DataAumento,
                                      Math.Round(Convert.ToDouble(preco.ValorTarifa.Replace('.', ',')), 5).ToString("0.00000"),
                                      CalcularValorPisCofinsTarifa(preco.DataAumento, preco.ValorTarifa, tarifasGaPisCofins, false).ToString("0.00000"),
                                      indicePai,
                                      string.Format("{0}.{1}", indicePai, indiceFilha));
                    indiceFilha++;
                }
                indicePai++;
            }

            tarifasGa.Append(@"
                            </tbody>
                        </table>");

            TarifasGA.InnerHtml = tarifasGa.ToString();
        }
        private double CalcularValorPisCofinsTarifa(string dataAumento, string valorTarifa, List<PisCofinsTarifasGa> tabelaPisCofins, bool primeiraReferencia)
        {
            foreach (var pisCofins in tabelaPisCofins)
            {
                if (dataAumento.Length == 10)
                {
                    string referencia = Convert.ToDateTime(dataAumento).ToString("yyyyMM");
                    if (primeiraReferencia)
                    {
                        double tarifaComPisCofins = 1 - (pisCofins.Pis + pisCofins.Cofins) / 100;
                        return Math.Round((Convert.ToDouble(valorTarifa.Replace('.', ',')) / tarifaComPisCofins), 5);
                    }
                    else if (referencia.Equals(pisCofins.Referencia))
                    {
                        double tarifaComPisCofins = 1 - (pisCofins.Pis + pisCofins.Cofins) / 100;
                        return Math.Round((Convert.ToDouble(valorTarifa.Replace('.', ',')) / tarifaComPisCofins), 5);
                    }
                }
            }
            return 0;
        }

        private List<Tarifa> FormatarTarifasGa()
        {
            using (StreamReader tr = new StreamReader(Server.MapPath(string.Format("~/content/lst/botao-empresa/{0}/tarifasga.lst", DropDownList_Empresa.SelectedValue)), Encoding.GetEncoding("iso-8859-1")))
            {
                List<Tarifa> Ltarifas = new List<Tarifa>();

                Tarifa ltarifa = new Tarifa();
                String ldescricaoAux = "";
                string line;
                while ((line = tr.ReadLine()) != null)
                {
                    string[] items = line.Trim().Split('|');

                    if (!ldescricaoAux.Equals(items[0]))
                    {
                        ltarifa = new Tarifa();
                        ltarifa.CodigoTarifa = items[0];
                        ltarifa.DescricaoTarifa = new DescricaoTarifa
                        {
                            Codigo = items[0],
                            Descricao = items[1]
                        };

                        ltarifa.PrecoTarifa = new List<PrecoTarifa>();

                        Ltarifas.Add(ltarifa);
                    }

                    PrecoTarifa lprecoTarifa = new PrecoTarifa();
                    lprecoTarifa.CodigoTarifa = items[0];
                    lprecoTarifa.DataAumento = items[2];
                    lprecoTarifa.ValorTarifa = !string.IsNullOrEmpty(items[3]) ? Convert.ToDouble(items[3].Replace('.', ',')).ToString("0.00000") : "0,00000";

                    //Adiciona o preço à lista do preço da tarifas em questão
                    ltarifa.PrecoTarifa.Add(lprecoTarifa);
                    ldescricaoAux = items[0];
                }

                return Ltarifas;
            }
        }

        private List<PisCofinsTarifasGa> FormatarPisCofinsTarifasGa()
        {
            using (StreamReader tr = new StreamReader(Server.MapPath(string.Format("~/content/lst/botao-empresa/{0}/tarifasga-piscofins.lst", DropDownList_Empresa.SelectedValue)), Encoding.GetEncoding("iso-8859-1")))
            {
                List<PisCofinsTarifasGa> Ltarifas = new List<PisCofinsTarifasGa>();

                string line;
                while ((line = tr.ReadLine()) != null)
                {
                    string[] items = line.Trim().Split('|');
                    PisCofinsTarifasGa tarifa = new PisCofinsTarifasGa();
                    tarifa.Referencia = items[0];
                    tarifa.Pis = Math.Round(Convert.ToDouble(items[1].Replace('.', ',')), 2);
                    tarifa.Cofins = Math.Round(Convert.ToDouble(items[2].Replace('.', ',')), 2);
                    Ltarifas.Add(tarifa);
                }

                return Ltarifas;
            }
        }
        #endregion

        #region TARIFAS DIVERSAS

        private void PreencheTabelaComTarifasDiversas()
        {
            List<Tarifa> tarifas = FormatarTarifasDiversas();


            StringBuilder table = new StringBuilder();

            table.Append(@"
            
                        <table id=""tarifas-diversas"">
                            <thead>
                                <tr>
                                    <th>Código</th>
                                    <th>Taxa</th>
                                    <th>Vigência</th>
                                    <th>Valor (R$)</th>
                                </tr>
                            </thead>
                            <tbody>");

            int indicePai = 1;
            foreach (var tarifa in tarifas)
            {
                int indiceFilha = 1;
                table.AppendFormat(@"
                                <tr data-tt-id=""{4}"">
                                    <td>{0}</td>
                                    <td>{1}</td>
                                    <td>{2}</td>
                                    <td>{3}</td>
                                </tr>",
                                      tarifa.CodigoTarifa,
                                      tarifa.DescricaoTarifa.Descricao,
                                      tarifa.PrecoTarifa[0].DataAumento,
                                      Convert.ToDouble(tarifa.PrecoTarifa[0].ValorTarifa.Replace('.', ',')).ToString("0.00000"),
                                      indicePai);

                foreach (var preco in tarifa.PrecoTarifa.Skip(1))
                {
                    table.AppendFormat(@"
                                <tr data-tt-id=""{3}"" data-tt-parent-id=""{2}"">
                                    <td style=""background-color: #efefef;""></td>
                                    <td style=""background-color: #efefef;""></td>
                                    <td style=""background-color: #efefef;"">{0}</td>
                                    <td style=""background-color: #efefef;"">{1}</td>
                                </tr>",
                                      preco.DataAumento,
                                      Math.Round(Convert.ToDouble(preco.ValorTarifa.Replace('.', ',')), 5).ToString("0.00000"),
                                      indicePai,
                                      string.Format("{0}.{1}", indicePai, indiceFilha));
                    indiceFilha++;
                }
                indicePai++;
            }

            table.Append(@"
                            </tbody>
                        </table>");

            TarifasDiversas.InnerHtml = table.ToString();
        }

        private List<Tarifa> FormatarTarifasDiversas()
        {
            using (StreamReader tr = new StreamReader(Server.MapPath(string.Format("~/content/lst/botao-empresa/{0}/tarifas-diversas.lst", DropDownList_Empresa.SelectedValue)), Encoding.GetEncoding("iso-8859-1")))
            {
                List<Tarifa> Ltarifas = new List<Tarifa>();

                Tarifa ltarifa = new Tarifa();
                String ldescricaoAux = "";
                string line;
                while ((line = tr.ReadLine()) != null)
                {
                    string[] items = line.Trim().Split('|');

                    if (!ldescricaoAux.Equals(items[0]))
                    {
                        ltarifa = new Tarifa();
                        ltarifa.CodigoTarifa = items[0];
                        ltarifa.DescricaoTarifa = new DescricaoTarifa
                        {
                            Codigo = items[0],
                            Descricao = items[1]
                        };

                        ltarifa.PrecoTarifa = new List<PrecoTarifa>();

                        Ltarifas.Add(ltarifa);
                    }

                    PrecoTarifa lprecoTarifa = new PrecoTarifa();
                    lprecoTarifa.CodigoTarifa = items[0];
                    lprecoTarifa.DataAumento = items[3];
                    lprecoTarifa.ValorTarifa = !string.IsNullOrEmpty(items[2]) ? Convert.ToDouble(items[2].Replace('.', ',')).ToString("0.00000") : "0,00000";

                    //Adiciona o preço à lista do preço da tarifas em questão
                    ltarifa.PrecoTarifa.Add(lprecoTarifa);
                    ldescricaoAux = items[0];
                }

                return Ltarifas;
            }
        }

        #endregion

        #region COD FATURAMENTO / LEITURISTA


        private List<CodFaturamentoLeiturista> FormatarCodFaturamentoLeiturista()
        {
            using (StreamReader tr = new StreamReader(Server.MapPath(string.Format("~/content/lst/botao-empresa/{0}/codfaturamento-leiturista.lst", DropDownList_Empresa.SelectedValue)), Encoding.GetEncoding("iso-8859-1")))
            {
                List<CodFaturamentoLeiturista> listaCodFaturamento = new List<CodFaturamentoLeiturista>();

                string line;
                while ((line = tr.ReadLine()) != null)
                {
                    string[] items = line.Trim().Split('|');

                    CodFaturamentoLeiturista cod = new CodFaturamentoLeiturista();
                    cod.Codigo = items[0];
                    cod.Descricao = items[1];

                    listaCodFaturamento.Add(cod);
                }

                return listaCodFaturamento;
            }
        }

        private void PreencheTabelaComFaturamentoLeiturista()
        {
            List<CodFaturamentoLeiturista> codFaturamento = FormatarCodFaturamentoLeiturista();


            StringBuilder table = new StringBuilder();

            table.Append(@"
            
                        <table id=""faturamento-leiturista"">
                            <thead>
                                <tr>
                                    <th>Código</th>
                                    <th>Descrição</th>
                                </tr>
                            </thead>
                            <tbody>");

            //POPULA OS HEADERS
            int indicePai = 1;
            foreach (var tarifa in codFaturamento)
            {
                table.AppendFormat(@"
                                <tr>
                                    <td>{0}</td>
                                    <td>{1}</td>
                                </tr>",
                                      tarifa.Codigo,
                                      tarifa.Descricao);
            }

            table.Append(@"
                            </tbody>
                        </table>");

            FaturamentoLeiturista.InnerHtml = table.ToString();
        }

        #endregion

        #region TARIFAS MEDIDORES

        private List<TarifaMedidor> FormatarTarifasMedidores()
        {
            using (StreamReader tr = new StreamReader(Server.MapPath(string.Format("~/content/lst/botao-empresa/{0}/tarifas-medidores.lst", DropDownList_Empresa.SelectedValue)), Encoding.GetEncoding("iso-8859-1")))
            {
                List<TarifaMedidor> listaCodFaturamento = new List<TarifaMedidor>();

                string line;
                while ((line = tr.ReadLine()) != null)
                {
                    string[] items = line.Trim().Split('|');

                    TarifaMedidor cod = new TarifaMedidor();
                    cod.Codigo = items[0];
                    cod.Descricao = items[1];
                    cod.Valor = items[2];

                    listaCodFaturamento.Add(cod);
                }

                return listaCodFaturamento;
            }
        }

        private void PreencheTabelaComTarifasMedidores()
        {
            List<TarifaMedidor> codFaturamento = FormatarTarifasMedidores();


            StringBuilder table = new StringBuilder();

            table.Append(@"
            
                        <table id=""tarifas-medidores"">
                            <thead>
                                <tr>
                                    <th>Código</th>
                                    <th>Descrição</th>
                                    <th>Valor (R$)</th>
                                </tr>
                            </thead>
                            <tbody>");

            //POPULA OS HEADERS
            foreach (var tarifa in codFaturamento)
            {
                table.AppendFormat(@"
                                <tr>
                                    <td>{0}</td>
                                    <td>{1}</td>
                                    <td>{2}</td>
                                </tr>",
                                      tarifa.Codigo,
                                      tarifa.Descricao,
                                      Convert.ToDouble(tarifa.Valor.Replace('.', ',')).ToString("0.00000"));
            }

            table.Append(@"
                            </tbody>
                        </table>");

            TarifasMedidores.InnerHtml = table.ToString();
        }

        #endregion

        #region CALENDARIO DE LEITURA




        protected void Ler_OnClick(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(DropDownList_Empresa.SelectedValue))
            {
                ScriptManager.RegisterClientScriptBlock(Page, Page.GetType(), "alert", @"alert('Selecione uma Região');", true);
            }
            else
            {
                //string dataAgenda = string.Format("01/{0}/{1}", CalMes.SelectedValue.Length == 1 ? string.Format("0{0}", CalMes.SelectedValue) : CalMes.SelectedValue, CalAno.SelectedItem);
                string dataAgenda = string.Format("01/{0}/{1}", CalMes.SelectedValue.Length == 1 ? string.Format("0{0}", CalMes.SelectedValue) : CalMes.SelectedValue, CalAno.SelectedItem);
                string fechaProcesso = string.Format("{0}-01-{1}", CalMes.SelectedValue.Length == 1 ? string.Format("0{0}", CalMes.SelectedValue) : CalMes.SelectedValue, CalAno.SelectedItem);
                string lote = Convert.ToInt32(Lote.Text).ToString();

                List<CalendarioLeitura> codFaturamento = FormatarCalendarioLeituras(fechaProcesso, lote);

                bool nenhumDado = true;
                StringBuilder table = new StringBuilder();

                if (codFaturamento.Count > 0)
                {


                    table.Append(@"
            
                        <table id=""calendario-leituras"">
                            <thead>
                                <tr>
                                    <th>Data Leitura</th>
                                    <th>Data Faturamento</th>
                                    <th>Data Apresentação</th>
                                    <th>Região</th>
                                </tr>
                            </thead>
                            <tbody>");

                    foreach (var tarifa in codFaturamento)
                    {
                        if (lote.Equals(tarifa.Lote) && dataAgenda.Equals(tarifa.FechaProceso))
                        {
                            table.AppendFormat(@"
                                <tr>
                                    <td>{0}</td>
                                    <td>{1}</td>
                                    <td>{2}</td>
                                    <td>{3}</td>
                                </tr>",
                                tarifa.FechaLectura,
                                tarifa.FechaFactura,
                                tarifa.FechaReparto,
                                tarifa.Regiao);

                            nenhumDado = false;
                        }
                    }

                    table.Append(@"
                            </tbody>
                        </table>");

                }
                if (nenhumDado)
                {
                    table.Clear();
                    table.Append(@"
                        <table id=""tarifas-ilum-pub"">
                            <thead>
                                <tr>
                                    <th>Nenhum dado encontrado!</th>
                                </tr>
                            </thead>
                            <tbody></tbody>
                        </table>");
                }
                CalendarioDeLeituraTable.InnerHtml = table.ToString();

                RefreshTablesWithScripts();
            }
        }

        private List<CalendarioLeitura> FormatarCalendarioLeituras(string fechaProcesso, string lote)
        {
            List<CalendarioLeitura> calendario = null;
            if (string.IsNullOrEmpty(DropDownList_Empresa.SelectedValue))
            {
                ScriptManager.RegisterClientScriptBlock(Page, Page.GetType(), "alert", @"alert('Selecione uma Região');", true);

            }
            else
            {
                string empresa = DropDownList_Empresa.SelectedValue.Equals("ampla") ? "2005" : "2003";

                IDictionary<string, object> data = new Dictionary<string, object>();
                data["sector"] = lote;
                data["fechaProceso"] = fechaProcesso;
                string result = Ura.GetWebApiWithoutDeserializeObject(empresa, data, "/api/salesforce/GetCalendarioLeitura");
                calendario = JsonConvert.DeserializeObject<List<CalendarioLeitura>>(result);

            }
            return calendario;

            //using (StreamReader tr = new StreamReader(Server.MapPath(string.Format("~/content/lst/botao-empresa/{0}/calendario-leituras.lst", DropDownList_Empresa.SelectedValue)), Encoding.GetEncoding("iso-8859-1")))
            //{
            //    List<CalendarioLeitura> Ltarifas = new List<CalendarioLeitura>();

            //    string line;
            //    while ((line = tr.ReadLine()) != null)
            //    {
            //        string[] items = line.Trim().Split('|');
            //        CalendarioLeitura tarifa = new CalendarioLeitura();
            //        tarifa.Lote = items[0];
            //        tarifa.FechaProceso = items[1];
            //        tarifa.FechaLectura = items[2];
            //        tarifa.FechaFactura = items[3];
            //        tarifa.FechaReparto = items[4];
            //        tarifa.Regiao = items[5];
            //        Ltarifas.Add(tarifa);
            //    }

            //    return Ltarifas;
            //}

        }

        private void CarregarAnos()
        {
            for (int lcont = 1998; lcont <= DateTime.Today.Year + 1; lcont++)
            {
                CalAno.Items.Add(lcont.ToString());
                if (lcont == DateTime.Today.Year)
                    CalAno.SelectedIndex = CalAno.Items.Count - 1;
            }
        }
        #endregion

        public void RefreshTablesWithScripts()
        {
            ScriptManager.RegisterStartupScript(this.Page, this.Page.GetType(), "alert", @"RecarregarTreetable();", true);
        }

        protected void IcmsTarifasGrupoB_OnSelectedIndexChanged(object sender, EventArgs e)
        {
            CarregarDemaisTarifas();
            CarregarTarifasGbNormais();
            CarregarTarifasBaixaRenda();
            RefreshTablesWithScripts();
        }


        public void CarregarTarifasGbNormais()
        {
            List<Tarifa> tarifas = new List<Tarifa>();


            using (StreamReader tr = new StreamReader(Server.MapPath(string.Format("~/content/lst/botao-empresa/{0}/tarifasgb-normais.lst", DropDownList_Empresa.SelectedValue)), Encoding.GetEncoding("iso-8859-1")))
            {
                string line;
                while ((line = tr.ReadLine()) != null)
                {
                    string[] items = line.Trim().Split('|');

                    Tarifa Ltarifa = new Tarifa();
                    Ltarifa.CodigoTarifa = items[0];

                    //Descrição da tarifa
                    Ltarifa.DescricaoTarifa = new DescricaoTarifa();
                    Ltarifa.DescricaoTarifa.Codigo = items[0];
                    Ltarifa.DescricaoTarifa.Descricao = items[1];

                    //Preço da tarifa
                    List<PrecoTarifa> Lprecos = new List<PrecoTarifa>();
                    PrecoTarifa Lpreco = new PrecoTarifa();
                    Lpreco.CodigoTarifa = items[0];
                    Lpreco.DataAumento = items[3];
                    Lpreco.ValorTarifa = items[2];
                    Lprecos.Add(Lpreco);

                    Ltarifa.PrecoTarifa = Lprecos;

                    tarifas.Add(Ltarifa);
                }
            }

            StringBuilder table = new StringBuilder();

            table.Append(@"
            
                        <table id=""tarifas-gb-normais"">
                            <thead>
                                <tr>
                                    <th>Vigência</th>
                                    <th>Tarifa Base (R$)</th>
                                    <th>Tarifa PIS/COFINS (R$)</th>
                                    <th>Tarifa ICMS + PIS/COFINS (R$)</th>
                                </tr>
                            </thead>
                            <tbody>");

            //POPULA OS HEADERS

            for (int i = 0; i < tarifas.Count; i++)
            {
                string tarifaB = CalcularValorPisCofinsTarifa(tarifas[i].PrecoTarifa[0].DataAumento, tarifas[i].PrecoTarifa[0].ValorTarifa, tarifasGaPisCofins, i == 0).ToString("0.00000");
                table.AppendFormat(@"
                                <tr>
                                    <td>{0}</td>
                                    <td>{1}</td>
                                    <td>{2}</td>
                                    <td>{3}</td>
                                </tr>",
                    tarifas[i].PrecoTarifa[0].DataAumento,
                    Convert.ToDouble(tarifas[i].PrecoTarifa[0].ValorTarifa.Replace('.', ',')).ToString("0.00000"),
                    CalcularValorPisCofinsTarifa(tarifas[i].PrecoTarifa[0].DataAumento, tarifas[i].PrecoTarifa[0].ValorTarifa, tarifasGaPisCofins, i == 0).ToString("0.00000"),
                    CalcularValorPisCofinsIcmsTarifas(tarifaB));
                //CalcularValorPisCofinsIcmsTarifa(tarifas[i].PrecoTarifa[0].DataAumento, tarifas[i].PrecoTarifa[0].ValorTarifa, tarifasGaPisCofins, i == 0).ToString("0.00000"));
            }

            table.Append(@"
                            </tbody>
                        </table>");

            ResidenciaNormal.InnerHtml = table.ToString();

        }

        private double CalcularValorPisCofinsIcmsTarifas(string tarifaB)
        {
            return Math.Round(Convert.ToDouble(tarifaB.Replace('.', ',')) / (1 - 0.27), 5);
        }

        private double CalcularValorPisCofinsIcmsTarifa(string dataAumento, string valorTarifa, List<PisCofinsTarifasGa> tabelaPisCofins, bool primeiraReferencia)
        {
            foreach (var pisCofins in tabelaPisCofins)
            {
                if (dataAumento.Length == 10)
                {
                    string referencia = Convert.ToDateTime(dataAumento).ToString("yyyyMM");
                    double valorIcms = Convert.ToDouble(IcmsTarifasGrupoB.SelectedValue) / 100;
                    if (primeiraReferencia)
                    {
                        double tarifaComPisCofins = 1 - (pisCofins.Pis + pisCofins.Cofins) / 100;
                        return Math.Round(Convert.ToDouble(valorTarifa.Replace('.', ',')) / (tarifaComPisCofins - valorIcms), 5);
                    }
                    else if (referencia.Equals(pisCofins.Referencia))
                    {
                        double tarifaComPisCofins = 1 - (pisCofins.Pis + pisCofins.Cofins) / 100;
                        return Math.Round(Convert.ToDouble(valorTarifa.Replace('.', ',')) / (tarifaComPisCofins - valorIcms), 5);
                    }
                }
            }
            return 0;
        }

        private void CarregarImpostoIcms()
        {
            List<double> impostos = new List<double>();

            using (StreamReader tr = new StreamReader(Server.MapPath(string.Format("~/content/lst/botao-empresa/{0}/imposto-icms.lst", DropDownList_Empresa.SelectedValue)), Encoding.GetEncoding("iso-8859-1")))
            {
                string line;
                while ((line = tr.ReadLine()) != null)
                {
                    string[] items = line.Trim().Split('|');


                    impostos.Add(Convert.ToDouble(items[0].Replace('.', ',')));
                }
            }

            IcmsTarifasGrupoB.DataSource = impostos;
            IcmsTarifasGrupoB.DataBind();


        }

        private void CarregarTarifasBaixaRenda()
        {

            List<TarifaFaixa> ltarifas = new List<TarifaFaixa>();

            TarifaFaixa ltarifa = new TarifaFaixa();
            String ltramos = "";
            using (StreamReader tr = new StreamReader(Server.MapPath(string.Format("~/content/lst/botao-empresa/{0}/tarifasgb-baixa-renda.lst", DropDownList_Empresa.SelectedValue)), Encoding.GetEncoding("iso-8859-1")))
            {
                string line;
                while ((line = tr.ReadLine()) != null)
                {
                    string[] items = line.Trim().Split('|');

                    var ltramosAux = items[2] + "a" + items[3];
                    if (!ltramos.Equals(ltramosAux))
                    {
                        ltarifa = new TarifaFaixa();
                        ltarifa.CodigoTarifa = items[0];
                        ltarifa.DescricaoTarifa = new DescricaoTarifa();
                        ltarifa.DescricaoTarifa.Codigo = items[0];
                        ltarifa.DescricaoTarifa.Descricao = items[1];

                        ltarifa.LimiteInferior = items[2];

                        ltarifa.LimiteSuperior = items[3];
                        ltarifa.PrecoTarifa = new List<PrecoTarifa>();

                        ltarifas.Add(ltarifa);
                    }
                    PrecoTarifaFaixa Lpreco = new PrecoTarifaFaixa();
                    Lpreco.CodigoTarifa = items[0];

                    Lpreco.DataAumento = items[5];

                    Lpreco.ValorTarifa = Convert.ToDouble(items[4].Replace('.', ',')).ToString("0.00000");


                    ltarifa.PrecoTarifa.Add(Lpreco);


                    ltramos = ltramosAux;
                }
            }

            //IMPLEMENTAÇÃO PARA PREENCHER A TABELA AQUI!
            StringBuilder tarifasGa = new StringBuilder();

            tarifasGa.Append(@"
                        <table id=""tarifas-baixa-renda"">
                            <thead>
                                <tr>
                                    <th>Faixa (kWh)</th>
                                    <th>Vigência</th>
                                    <th>Tarifa Base (R$)</th>
                                    <th>Tarifa PIS/COFINS (R$)</th>
                                    <th>Tarifa ICMS + PIS/COFINS (R$)</th>
                                </tr>
                            </thead>
                            <tbody>");

            int indicePai = 1;
            foreach (var tarifa in ltarifas)
            {
                string tarifaB = CalcularValorPisCofinsTarifa(tarifa.PrecoTarifa[0].DataAumento, tarifa.PrecoTarifa[0].ValorTarifa, tarifasGaPisCofins, true).ToString("0.00000");
                int indiceFilha = 1;
                tarifasGa.AppendFormat(@"
                                <tr data-tt-id=""{5}"">
                                    <td>{0}</td>
                                    <td>{1}</td>
                                    <td>{2}</td>
                                    <td>{3}</td>
                                    <td>{4}</td>
                                </tr>",
                                      string.Format("{0} a {1}", tarifa.LimiteInferior, tarifa.LimiteSuperior),
                                      tarifa.PrecoTarifa[0].DataAumento,
                                      tarifa.PrecoTarifa[0].ValorTarifa,
                                      CalcularValorPisCofinsTarifa(tarifa.PrecoTarifa[0].DataAumento, tarifa.PrecoTarifa[0].ValorTarifa, tarifasGaPisCofins, true).ToString("0.00000"),
                                      CalcularValorPisCofinsIcmsTarifas(tarifaB),
                    //CalcularValorPisCofinsIcmsTarifa(tarifa.PrecoTarifa[0].DataAumento, tarifa.PrecoTarifa[0].ValorTarifa, tarifasGaPisCofins, true).ToString("0.00000"),
                                      indicePai);


                for (int i = 1; i < tarifa.PrecoTarifa.Count; i++)
                {
                    tarifasGa.AppendFormat(@"
                                <tr data-tt-id=""{5}"" data-tt-parent-id=""{4}"">
                                    <td style=""background-color: #efefef;""></td>
                                    <td style=""background-color: #efefef;"">{0}</td>
                                    <td style=""background-color: #efefef;"">{1}</td>
                                    <td style=""background-color: #efefef;"">{2}</td>
                                    <td style=""background-color: #efefef;"">{3}</td>
                                </tr>",
                        tarifa.PrecoTarifa[i].DataAumento,
                        tarifa.PrecoTarifa[i].ValorTarifa,
                        CalcularValorPisCofinsTarifa(tarifa.PrecoTarifa[i].DataAumento, tarifa.PrecoTarifa[i].ValorTarifa, tarifasGaPisCofins, true).ToString("0.00000"),
                        CalcularValorPisCofinsIcmsTarifa(tarifa.PrecoTarifa[i].DataAumento, tarifa.PrecoTarifa[i].ValorTarifa, tarifasGaPisCofins, true).ToString("0.00000"),
                        indicePai,
                        string.Format("{0}.{1}", indicePai, indiceFilha));
                    indiceFilha++;
                }




                indicePai++;
            }

            tarifasGa.Append(@"
                            </tbody>
                        </table>");

            BaixaRenda.InnerHtml = tarifasGa.ToString();

            RefreshTablesWithScripts();
        }


        private void CarregarDemaisTarifas()
        {

            List<Tarifa> ltarifas = new List<Tarifa>();

            Tarifa Ltarifa = new Tarifa();
            String LdescricaoAux = "";
            using (StreamReader tr = new StreamReader(Server.MapPath(string.Format("~/content/lst/botao-empresa/{0}/tarifasgb-demais-tarifas.lst", DropDownList_Empresa.SelectedValue)), Encoding.GetEncoding("iso-8859-1")))
            {
                string line;
                while ((line = tr.ReadLine()) != null)
                {
                    string[] items = line.Trim().Split('|');

                    if (!LdescricaoAux.Equals(items[1]))
                    {
                        Ltarifa = new Tarifa();

                        Ltarifa.CodigoTarifa = items[0];
                        Ltarifa.DescricaoTarifa = new DescricaoTarifa();
                        Ltarifa.DescricaoTarifa.Codigo = items[0];
                        Ltarifa.DescricaoTarifa.Descricao = items[1];
                        Ltarifa.PrecoTarifa = new List<PrecoTarifa>();

                        ltarifas.Add(Ltarifa);//Acrescenta na lista de tarifas                            
                    }
                    //Preço do último aumento
                    PrecoTarifa Lpreco = new PrecoTarifa();
                    Lpreco.CodigoTarifa = items[0];

                    Lpreco.DataAumento = items[3];

                    Lpreco.ValorTarifa = Convert.ToDouble(items[2].Replace('.', ',')).ToString("0.00000");

                    Ltarifa.PrecoTarifa.Add(Lpreco);

                    LdescricaoAux = items[1];
                }
            }

            //IMPLEMENTAÇÃO PARA PREENCHER A TABELA AQUI!
            StringBuilder tarifasGa = new StringBuilder();

            tarifasGa.Append(@"
                        <table id=""tarifas-baixa-renda"">
                            <thead>
                                <tr>
                                    <th>Tarifa</th>
                                    <th>Vigência</th>
                                    <th>Tarifa Base (R$)</th>
                                    <th>Tarifa PIS/COFINS (R$)</th>
                                    <th>Tarifa ICMS + PIS/COFINS (R$)</th>
                                </tr>
                            </thead>
                            <tbody>");

            int indicePai = 1;
            foreach (var tarifa in ltarifas)
            {
                string tarifaB = CalcularValorPisCofinsTarifa(tarifa.PrecoTarifa[0].DataAumento, tarifa.PrecoTarifa[0].ValorTarifa, tarifasGaPisCofins, true).ToString("0.00000");
                int indiceFilha = 1;
                tarifasGa.AppendFormat(@"
                                <tr data-tt-id=""{5}"">
                                    <td>{0}</td>
                                    <td>{1}</td>
                                    <td>{2}</td>
                                    <td>{3}</td>
                                    <td>{4}</td>
                                </tr>",
                                      tarifa.DescricaoTarifa.Descricao,
                                      tarifa.PrecoTarifa[0].DataAumento,
                                      tarifa.PrecoTarifa[0].ValorTarifa,
                                      CalcularValorPisCofinsTarifa(tarifa.PrecoTarifa[0].DataAumento, tarifa.PrecoTarifa[0].ValorTarifa, tarifasGaPisCofins, true).ToString("0.00000"),
                                      CalcularValorPisCofinsIcmsTarifas(tarifaB),
                    //CalcularValorPisCofinsIcmsTarifa(tarifa.PrecoTarifa[0].DataAumento, tarifa.PrecoTarifa[0].ValorTarifa, tarifasGaPisCofins, true).ToString("0.00000"),
                                      indicePai);


                for (int i = 1; i < tarifa.PrecoTarifa.Count; i++)
                {
                    tarifasGa.AppendFormat(@"
                                <tr data-tt-id=""{5}"" data-tt-parent-id=""{4}"">
                                    <td style=""background-color: #efefef;""></td>
                                    <td style=""background-color: #efefef;"">{0}</td>
                                    <td style=""background-color: #efefef;"">{1}</td>
                                    <td style=""background-color: #efefef;"">{2}</td>
                                    <td style=""background-color: #efefef;"">{3}</td>
                                </tr>",
                        tarifa.PrecoTarifa[i].DataAumento,
                        tarifa.PrecoTarifa[i].ValorTarifa,
                        CalcularValorPisCofinsTarifa(tarifa.PrecoTarifa[i].DataAumento, tarifa.PrecoTarifa[i].ValorTarifa, tarifasGaPisCofins, true).ToString("0.00000"),
                        CalcularValorPisCofinsIcmsTarifa(tarifa.PrecoTarifa[i].DataAumento, tarifa.PrecoTarifa[i].ValorTarifa, tarifasGaPisCofins, true).ToString("0.00000"),
                        indicePai,
                        string.Format("{0}.{1}", indicePai, indiceFilha));
                    indiceFilha++;
                }




                indicePai++;
            }

            tarifasGa.Append(@"
                            </tbody>
                        </table>");

            DemaisTarifas.InnerHtml = tarifasGa.ToString();

            RefreshTablesWithScripts();
        }

        protected void DropDownList_Empresa_OnSelectedIndexChanged(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(DropDownList_Empresa.SelectedValue))
            {

                CalendarioDeLeituraTable.InnerHtml = string.Empty;
                TarifasIluminacaoPublica.InnerHtml = string.Empty;
                Lote.Text = string.Empty;

                PreencheTabelaComTarifasGa();
                PreencheTabelaComFaturamentoLeiturista();
                PreencheTabelaComTarifasDiversas();
                PreencheTabelaComTarifasMedidores();
                CarregarMunicipiosIluminacaoPublica();

                CarregarAnos();
                CarregarImpostoIcms();
                CarregarDemaisTarifas();
                CarregarTarifasGbNormais();
                CarregarTarifasBaixaRenda();

            }
            else
            {
                Response.Redirect("/");
            }
        }
        protected void ShowMessage(string Message, string type)
        {
            ScriptManager.RegisterStartupScript(this, this.GetType(), System.Guid.NewGuid().ToString(), "ShowMessage('" + Message + "','" + type + "');", true);
        }
    }
}
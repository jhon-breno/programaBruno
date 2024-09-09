using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;
using BotaoEmpresa.entidades;
using BotaoEmpresa.@enum;
using BotaoEmpresa.utils;
using Newtonsoft.Json;

namespace BotaoEmpresa.controls
{
    public partial class HistoricoDeOrdem : UserControl
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!Page.IsPostBack)
            {
                if (Session["AdminHistOrdem"] != null)
                {
                    LoginRedeBR.AdUserInfo dadosUsuario = (LoginRedeBR.AdUserInfo) Session["AdminHistOrdem"];
                    Logout.Text = string.Format( "Seja bem vindo, {0} (Sair)", dadosUsuario.Nome);
                }

                if (!string.IsNullOrEmpty(NumCliente.Text) || !string.IsNullOrEmpty(NumeroOrdem.Text))
                {
                    BuscarHistoricoOrdens();
                }
                else
                {
                    ScriptManager.RegisterStartupScript(UpPanelStp1, UpPanelStp1.GetType(), System.Guid.NewGuid().ToString(), "$('.step-cliente').removeClass('hidden');", true);
                }
            }
        }

        private void PreencherDadosOrdem(HistoricoOrdem ordem)
        {

            if (!string.IsNullOrEmpty(ordem.NumeroCliente))
            {

                NumClienteOrdem.Text = ordem.NumeroCliente;
                NomeClienteOrdem.Text = ordem.NomeCliente;
                EstadoSuministroOrdem.Text = "0".Equals(ordem.StatusConexao) ? "COM FORNECIMENTO" : "SEM FORNECIMENTO";

            }

            NumOrdemLegend.InnerText = string.Format("Dados da Ordem {0}", ordem.NumeroOrdem);
            Tipo.Text = ordem.TipoOrdem;
            Servico.Text = ordem.DescricaoTipoServico;
            SucOrigem.Text = ordem.OrdemDetalhe.SucursalOrigem;
            SucDestino.Text = ordem.OrdemDetalhe.SucursalDestino;
            AreaDestino.Text = ordem.OrdemDetalhe.AreaDestinoo;
            AreaOrigem.Text = ordem.OrdemDetalhe.AreaOrigem;
            Etapa.Text = ordem.Etapa;
            DataIngresso.Text = ordem.DataIngresso.ToShortDateString();
            Estado.Text = ordem.Estado;
            Usuario.Text = ordem.OrdemDetalhe.Usuario;
            Atendimento.Text = ordem.OrdemDetalhe.Atendimento;
            OrdemOriginal.Text = ordem.OrdemDetalhe.SucursalOrigem;
            FinalizadaPor.Text = ordem.OrdemDetalhe.FinalzadaPorData;
            DataFinalizacao.Text = ordem.OrdemDetalhe.DataFinalizacao;
            ObsAtendimento.Text = ordem.OrdemDetalhe.ObsDoAtendimento;
            ObsExecutante.Text = ordem.OrdemDetalhe.ObsDoExecutante;

            if (ordem.HistoricoVisitas.Count > 0)
            {
                foreach (var visita in ordem.HistoricoVisitas)
                {
                    if (string.IsNullOrEmpty(visita.DataVisita) || Convert.ToDateTime(visita.DataVisita) == DateTime.MinValue)
                    {
                        visita.DataVisita = string.Empty;
                    }
                    if (string.IsNullOrEmpty(visita.DataRetorno) || Convert.ToDateTime(visita.DataRetorno) == DateTime.MinValue)
                    {
                        visita.DataRetorno = string.Empty;
                    }
                    if (string.IsNullOrEmpty(visita.DataExec) || Convert.ToDateTime(visita.DataExec) == DateTime.MinValue)
                    {
                        visita.DataExec = string.Empty;
                    }
                }

                GrdVisitas.DataBound += (object o, EventArgs ev) =>
                {
                    GrdVisitas.HeaderRow.TableSection = TableRowSection.TableHeader;
                };
                GrdVisitas.DataSource = ordem.HistoricoVisitas;
                GrdVisitas.DataBind();
            }
        }
        protected void GrdOrdens_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            string[] command = e.CommandArgument.ToString().Split(',');
            string ordemSelecionada = command[0];

            if (e.CommandName == "DetailsOrdemFilha")
            {
                ViewState["IsFilha"] = true;

                List<HistoricoOrdem> historico = BuscarOrdemFilha(ordemSelecionada);

                if (historico.Count > 0)
                {
                    ViewState["OrdemMae"] = NumOrdemLegend.InnerText.Substring(15);
                    ScriptManager.RegisterStartupScript(UpPanelStp1, UpPanelStp1.GetType(), System.Guid.NewGuid().ToString(), "$('.step-1').addClass('hidden');$('.step-2').removeClass('hidden');$('.step-cliente').addClass('hidden');", true);
                    PreencherDadosOrdem(historico[0]);
                }
            }

            if (e.CommandName == "Details")
            {
                ViewState["IsFilha"] = false;

                string ordemTipo = command[1];

                if ("OI".Equals(ordemTipo))
                {
                    #region OI
                    OrdemEntrada data = new OrdemEntrada
                    {
                        RegionCode = "2003",
                        SupplyOrderNumber = ordemSelecionada
                    };

                    string response = Ura.PostWebApiInUraIV(data, "api/HistoricoOrdem/GetDadosOrdemOi");

                    Resultado restultado = JsonConvert.DeserializeObject<Resultado>(response);
                    if (restultado.Codigo == 1)
                    {
                        NumOrdemOiLegend.InnerText = string.Format("Ordem de Inspeção O.I. {0}", ordemSelecionada);

                        HistoricoOrdemOiOt oiOt = JsonConvert.DeserializeObject<HistoricoOrdemOiOt>(restultado.Retorno.ToString(), new JsonSerializerSettings { DateFormatString = "dd/MM/yyyy HH:mm:ss" });

                        OiNumTdc.Text = oiOt.NumTdc;
                        OiStituacao.Text = oiOt.Situacao;
                        Executante.Text = oiOt.Executante;
                        Nome.Text = oiOt.Nome;
                        Endereco.Text = oiOt.Endereco;
                        OiCadNumCoelce.Text = oiOt.CadNumCoelce;
                        OiCadNumFabrica.Text = oiOt.CadNumFabrica;
                        OiCadAlg.Text = oiOt.CadAlg;
                        OiCadFabricante.Text = oiOt.CadFabricante;
                        OiCadDescricao.Text = oiOt.CadDescricao;
                        OiCadTipo.Text = oiOt.CadTipo;
                        OiCadModelo.Text = oiOt.CadModelo;
                        OiCadUltimaLeitura.Text = oiOt.CadUltimaLeitura;
                        OiCadUltimaInstalacao.Text = oiOt.CadUltimaInstalacao.ToString("MM/dd/yyyy");
                        OiEncNumCoelce.Text = oiOt.EncNumCoelce;
                        OiEncNumFabrica.Text = oiOt.EncNumFabrica;
                        OiEncAlg.Text = oiOt.EncAlg;
                        OiEncFabricante.Text = oiOt.EncFabricante;
                        OiEncDescricao.Text = oiOt.EncDescricao;
                        OiEncTipo.Text = oiOt.EncTipo;
                        OiEncModelo.Text = oiOt.EncModelo;
                        OiEncUltimaLeitura.Text = oiOt.EncUltimaLeitura;
                        if (oiOt.Anormalidades != null)
                        {
                            int anor01 = 0;
                            int anor02 = 0;
                            int anor03 = 0;
                            int anor04 = 0;
                            int anor05 = 0;
                            try { Anor01.Text = oiOt.Anormalidades[0] ?? string.Empty; anor01 = Convert.ToInt32(oiOt.Anormalidades[0].Substring(1)); }
                            catch { }
                            try { Anor02.Text = oiOt.Anormalidades[1] ?? string.Empty; anor02 = Convert.ToInt32(oiOt.Anormalidades[1].Substring(1)); }
                            catch { }
                            try { Anor03.Text = oiOt.Anormalidades[2] ?? string.Empty; anor03 = Convert.ToInt32(oiOt.Anormalidades[2].Substring(1)); }
                            catch { }
                            try { Anor04.Text = oiOt.Anormalidades[3] ?? string.Empty; anor04 = Convert.ToInt32(oiOt.Anormalidades[3].Substring(1)); }
                            catch { }
                            try { Anor05.Text = oiOt.Anormalidades[4] ?? string.Empty; anor05 = Convert.ToInt32(oiOt.Anormalidades[4].Substring(1)); }
                            catch { }

                            int anorTotal = anor01 + anor02 + anor03 + anor04 + anor05;
                            AnorTotal.Text = anorTotal.ToString();
                        }

                        if (oiOt.Ngs != null)
                        {
                            int ng01 = 0;
                            int ng02 = 0;
                            int ng03 = 0;
                            int ng04 = 0;
                            int ng05 = 0;

                            try { Ng01.Text = oiOt.Ngs[0] ?? string.Empty; ng01 = Convert.ToInt32(oiOt.Ngs[0]); }
                            catch { }
                            try { Ng02.Text = oiOt.Ngs[1] ?? string.Empty; ng02 = Convert.ToInt32(oiOt.Ngs[1]); }
                            catch { }
                            try { Ng03.Text = oiOt.Ngs[2] ?? string.Empty; ng03 = Convert.ToInt32(oiOt.Ngs[2]); }
                            catch { }
                            try { Ng04.Text = oiOt.Ngs[3] ?? string.Empty; ng04 = Convert.ToInt32(oiOt.Ngs[3]); }
                            catch { }
                            try { Ng05.Text = oiOt.Ngs[4] ?? string.Empty; ng05 = Convert.ToInt32(oiOt.Ngs[4]); }
                            catch { }

                            int ngTotal = ng01 + ng02 + ng03 + ng04 + ng05;
                            NgTotal.Text = ngTotal.ToString();
                        }
                        ObservacoesOi.Text = oiOt.Observacoes;
                        DataExecOi.Text = oiOt.DataExec.ToString("dd/MM/yyyy");
                        HoraIniExecOi.Text = oiOt.HoraIniExec.ToString("T");
                        HoraFimExecOi.Text = oiOt.HoraFimExec.ToString("T");

                    }

                    ScriptManager.RegisterStartupScript(UpPanelStp1, UpPanelStp1.GetType(), System.Guid.NewGuid().ToString(), "$('.step-1').addClass('hidden');$('.step-oi').removeClass('hidden');$('.step-cliente').addClass('hidden');", true);
                    #endregion
                }
                else if ("OT".Equals(ordemTipo))
                {
                    #region OT
                    OrdemEntrada data = new OrdemEntrada
                    {
                        RegionCode = "2003",
                        SupplyOrderNumber = ordemSelecionada
                    };

                    string response = Ura.PostWebApiInUraIV(data, "api/HistoricoOrdem/GetDadosOrdemOt");

                    Resultado restultado = JsonConvert.DeserializeObject<Resultado>(response);
                    if (restultado.Codigo == 1)
                    {
                        NumOrdemOtLegend.InnerText = string.Format("Ordem de Inspeção O.T. {0}", ordemSelecionada);
                        HistoricoOrdemOiOt oiOt = JsonConvert.DeserializeObject<HistoricoOrdemOiOt>(restultado.Retorno.ToString(), new JsonSerializerSettings { DateFormatString = "dd/MM/yyyy HH:mm:ss" });

                        OtNumTdc.Text = oiOt.NumTdc;
                        OtStituacao.Text = oiOt.Situacao;
                        OtExecutante.Text = oiOt.Executante;
                        OtNome.Text = oiOt.Nome;
                        OtEndereco.Text = oiOt.Endereco;
                        OtCadNumCoelce.Text = oiOt.CadNumCoelce;
                        OtCadNumFabrica.Text = oiOt.CadNumFabrica;
                        OtCadAlg.Text = oiOt.CadAlg;
                        OtCadFabricante.Text = oiOt.CadFabricante;
                        OtCadDescricao.Text = oiOt.CadDescricao;
                        OtCadTipo.Text = oiOt.CadTipo;
                        OtCadModelo.Text = oiOt.CadModelo;
                        OtCadUltimaLeitura.Text = oiOt.CadUltimaLeitura;
                        OtCadUltLeituraHp.Text = oiOt.CadUltLeituraHp;
                        OtCadUltLeituraReat.Text = oiOt.CadUltLeituraReat;
                        OtEncNumCoelce.Text = oiOt.EncNumCoelce;
                        OtEncNumFabrica.Text = oiOt.EncNumFabrica;
                        OtEncAlg.Text = oiOt.EncAlg;
                        OtEncFabricante.Text = oiOt.EncFabricante;
                        OtEncDescricao.Text = oiOt.EncDescricao;
                        OtEncTipo.Text = oiOt.EncTipo;
                        OtEncModelo.Text = oiOt.EncModelo;
                        OtEncUltimaLeitura.Text = oiOt.EncUltimaLeitura;
                        OtEncUltLeituraHp.Text = oiOt.EncUltLeituraHp;
                        OtEncUltLeituraReat.Text = oiOt.EncUltLeituraReat;

                        if (oiOt.Ngs != null)
                        {
                            int ng01 = 0;
                            int ng02 = 0;
                            int ng03 = 0;
                            int ng04 = 0;
                            int ng05 = 0;

                            try { NgOt01.Text = oiOt.Ngs[0] ?? string.Empty; ng01 = Convert.ToInt32(oiOt.Ngs[0]); }
                            catch { }
                            try { NgOt02.Text = oiOt.Ngs[1] ?? string.Empty; ng02 = Convert.ToInt32(oiOt.Ngs[1]); }
                            catch { }
                            try { NgOt03.Text = oiOt.Ngs[2] ?? string.Empty; ng03 = Convert.ToInt32(oiOt.Ngs[2]); }
                            catch { }
                            try { NgOt04.Text = oiOt.Ngs[3] ?? string.Empty; ng04 = Convert.ToInt32(oiOt.Ngs[3]); }
                            catch { }
                            try { NgOt05.Text = oiOt.Ngs[4] ?? string.Empty; ng05 = Convert.ToInt32(oiOt.Ngs[4]); }
                            catch { }

                            int ngTotal = ng01 + ng02 + ng03 + ng04 + ng05;
                            NgOtTotal.Text = ngTotal.ToString();
                        }

                        if (oiOt.RespNgs != null)
                        {
                            try { RespNg01.Text = oiOt.RespNgs[0] ?? string.Empty; }
                            catch { }
                            try { RespNg02.Text = oiOt.RespNgs[1] ?? string.Empty; }
                            catch { }
                        }

                        ObservacoesOt.Text = oiOt.Observacoes;
                        DataExecOt.Text = oiOt.DataExec.ToString("d");
                        HoraIniExecOt.Text = oiOt.HoraIniExec.ToString("T");
                        HoraFimExecOt.Text = oiOt.HoraFimExec.ToString("T");

                    }

                    ScriptManager.RegisterStartupScript(UpPanelStp1, UpPanelStp1.GetType(), System.Guid.NewGuid().ToString(), "$('.step-1').addClass('hidden');$('.step-ot').removeClass('hidden');$('.step-cliente').addClass('hidden');", true);
                    #endregion
                }
                else
                {
                    ScriptManager.RegisterStartupScript(UpPanelStp1, UpPanelStp1.GetType(), System.Guid.NewGuid().ToString(), "$('.step-1').addClass('hidden');$('.step-2').removeClass('hidden');$('.step-cliente').addClass('hidden');", true);

                    List<HistoricoOrdem> historico = (List<HistoricoOrdem>)ViewState["HistoricoOrdens"];

                    foreach (var ordem in historico.Where(ordem => ordemSelecionada.Equals(ordem.NumeroOrdem)))
                    {
                        PreencherDadosOrdem(ordem);
                        break;
                    }
                }
            }
        }

        protected void Voltar_OnClick(object sender, EventArgs e)
        {
            if ((bool)ViewState["IsFilha"])
            {
                ScriptManager.RegisterStartupScript(UpPanelStp1, UpPanelStp1.GetType(), System.Guid.NewGuid().ToString(), "$('.step-1').addClass('hidden');$('.step-2').removeClass('hidden');$('.step-cliente').addClass('hidden');", true);
                List<HistoricoOrdem> historico = BuscarOrdemFilha(ViewState["OrdemMae"].ToString());
                if (historico.Count > 0)
                {
                    HistoricoOrdem ordem = historico[0];
                    PreencherDadosOrdem(ordem);
                    ViewState["IsFilha"] = false;
                }
            }
            else
            {
                List<HistoricoOrdem> historico = (List<HistoricoOrdem>)ViewState["HistoricoOrdens"];
                if (historico.Count > 0)
                {
                    GrdOrdens.DataBound += (object o, EventArgs ev) =>
                    {
                        GrdOrdens.HeaderRow.TableSection = TableRowSection.TableHeader;
                    };

                    GrdOrdens.DataSource = historico;
                    GrdOrdens.DataBind();
                }
                ScriptManager.RegisterStartupScript(UpPanelStp1, UpPanelStp1.GetType(), System.Guid.NewGuid().ToString(), "$('.step-ot').addClass('hidden');$('.step-oi').addClass('hidden');$('.step-2').addClass('hidden');$('.step-1').removeClass('hidden');$('.step-cliente').removeClass('hidden');", true);
            }
        }


        protected void Buscar_OnClick(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(NumCliente.Text) || !string.IsNullOrEmpty(NumeroOrdem.Text))
            {
                BuscarHistoricoOrdens();
            }
            else
            {
                ScriptManager.RegisterStartupScript(UpPanelStp1, UpPanelStp1.GetType(), System.Guid.NewGuid().ToString(), string.Format("ShowToast('{0}','{1}');", "Os campos número de cliente ou ordem de serviço são obrigatórios.", ToastTypes.Alerta), true);
            }
        }

        public void BuscarHistoricoOrdens()
        {
            OrdemEntrada data = new OrdemEntrada
            {
                RegionCode = "2003",
                SupplyCode = NumCliente.Text,
                SupplyOrderNumber = NumeroOrdem.Text

            };

            string response = Ura.PostWebApiInUraIV(data, "api/HistoricoOrdem/GetHistoricoOrdem");

            Resultado restultado = JsonConvert.DeserializeObject<Resultado>(response);
            if (restultado.Codigo == 1)
            {
                List<HistoricoOrdem> ordens = JsonConvert.DeserializeObject<List<HistoricoOrdem>>(restultado.Retorno.ToString(), new JsonSerializerSettings { DateFormatString = "dd/MM/yyyy" });

                if (ordens.Count > 0)
                {
                    GrdOrdens.DataBound += (object o, EventArgs ev) =>
                    {
                        GrdOrdens.HeaderRow.TableSection = TableRowSection.TableHeader;
                    };

                    foreach (var ordem in ordens)
                    {

                        ordem.DataIngresso = Convert.ToDateTime(ordem.DataIngresso);

                        if ("ERRO DADOS DA ORDEM NAO ENCONTRADA".Equals(ordem.Prazo))
                        {
                            ordem.Prazo = string.Empty;
                        }
                    }

                    List<HistoricoOrdem> ordensOrdenadas = ordens.OrderByDescending(x => x.DataIngresso).ToList();
                    ViewState["HistoricoOrdens"] = ordensOrdenadas;
                    GrdOrdens.DataSource = ordensOrdenadas;
                    GrdOrdens.DataBind();
                }

            }
            ScriptManager.RegisterStartupScript(UpPanelStp1, UpPanelStp1.GetType(), System.Guid.NewGuid().ToString(), "$('.step-1').removeClass('hidden');", true);
        }

        public List<HistoricoOrdem> BuscarOrdemFilha(string numeroOrdem)
        {
            List<HistoricoOrdem> ordem = new List<HistoricoOrdem>();

            OrdemEntrada data = new OrdemEntrada
            {
                RegionCode = "2003",
                SupplyCode = string.Empty,
                SupplyOrderNumber = numeroOrdem
            };

            string response = Ura.PostWebApiInUraIV(data, "api/HistoricoOrdem/GetHistoricoOrdem");

            Resultado restultado = JsonConvert.DeserializeObject<Resultado>(response);
            if (restultado.Codigo == 1)
            {
                ordem = JsonConvert.DeserializeObject<List<HistoricoOrdem>>(restultado.Retorno.ToString(), new JsonSerializerSettings { DateFormatString = "dd/MM/yyyy" });
            }

            return ordem;
        }

        protected void Logout_OnClick(object sender, EventArgs e)
        {
            Response.Clear();
            Session.Abandon();
            Response.Redirect("/");
        }
    }
}
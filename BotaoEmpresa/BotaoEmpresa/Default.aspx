<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Default.aspx.cs" Inherits="BotaoEmpresa._Default" %>

<%@ Register Src="~/content/Spinner/load-spin.ascx" TagPrefix="uc1" TagName="loadspin" %>
<%@ Register Src="~/controls/ConsultarMultasIndevidas.ascx" TagPrefix="uc1" TagName="ConsultarMultasIndevidas" %>
<%@ Register Src="~/controls/DescadastramentoEnel.ascx" TagPrefix="uc1" TagName="DescadastramentoEnel" %>
<%@ Register Src="~/controls/ConsultarClientesFaturaPorEmail.ascx" TagPrefix="uc1" TagName="ConsultarClientesFaturaPorEmail" %>
<%@ Register Src="~/controls/ConsultarBeneficio.ascx" TagPrefix="uc1" TagName="ConsultarBeneficio" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>Enel | Informações Empresa</title>


    <link href="content/css/global.css" rel="stylesheet" />
    <link href="content/css/bootstrap.css" rel="stylesheet" />
    <link href="content/css/agencia.css" rel="stylesheet" />
    <link href="content/css/jquery.treetable.css" rel="stylesheet" />
    <link href="content/css/jquery.treetable.theme.default.css" rel="stylesheet" />
    <link href="content/css/onehub-stylish/forms.css" rel="stylesheet" />
    <link href="content/css/historico-ordem.css" rel="stylesheet" />
    <style>
        .nav > li > a {
            font-size: 14px;
        }

        .combo-style {
            height: 45px;
            width: 100%;
            padding: 10px;
            margin-bottom: 10px;
        }

        .input-group-addon {
            border-bottom: 0;
        }

        .no-padding {
            padding: 0 !important;
        }

        .table-responsive {
            margin-top: 15px;
        }

        .margin-top-15 {
            margin-top: 15px;
        }

        .input-group-addon:last-child {
            border: none;
        }
    </style>
</head>
<body>
    <form id="form1" runat="server">
        <asp:UpdateProgress ID="UpdateMessagesProgress" DynamicLayout="false" runat="server" DisplayAfter="0">
            <ProgressTemplate>
                <uc1:loadspin runat="server" ID="loadspin" />
            </ProgressTemplate>
        </asp:UpdateProgress>

        <div style="margin-left: 50px; margin-right: 50px;">
            <div class="row center">
                <img src="https://www.eneldistribuicao.com.br/assets/images/navbar-logo-color.png" alt="Enel" style="position: absolute; top: 25px; left: 50px;" />
                <div class="col-xs-12">
                    <h1 class="text-center padding-bottom-20">INFORMAÇÕES EMPRESA</h1>
                </div>
                <asp:DropDownList ID="DropDownList_Empresa" CssClass="combo-style" runat="Server" AutoPostBack="True" OnSelectedIndexChanged="DropDownList_Empresa_OnSelectedIndexChanged" Width="250" Style="margin-left: 15px;">
                    <asp:ListItem Value="">Selecione uma Região</asp:ListItem>
                    <asp:ListItem Value="coelce">Ceará</asp:ListItem>
                    <asp:ListItem Value="ampla">Rio de Janeiro</asp:ListItem>
                </asp:DropDownList>
            </div>


            <asp:ScriptManager runat="server"></asp:ScriptManager>
            <div>
                <ul class="nav nav-tabs">
                    <li class="active"><a data-toggle="tab" href="#TarifasGA">Tarifas Grupo A</a></li>
                    <li><a data-toggle="tab" href="#TarifasGB">Tarifas Grupo B</a></li>
                    <li><a data-toggle="tab" href="#TarifasIP">Tarifas Iluminação Pública</a></li>
                    <li><a data-toggle="tab" href="#TarifasDiversas">Taxas Diversas</a></li>
                    <li><a data-toggle="tab" href="#TarifasMedidores">Tarifas Medidores</a></li>
                    <li><a data-toggle="tab" href="#CalendarioDeLeitura">Calendário de Leituras</a></li>
                    <li><a data-toggle="tab" href="#FaturamentoLeiturista">Cod. Faturamento/Leiturista</a></li>
                    <li><a data-toggle="tab" href="#HistoricoDeOrdens">Histórico de Ordens Ceará</a></li>
                    <li><a data-toggle="tab" href="#Multas">Consultar Multas Indevidas</a></li>
                    <li><a data-toggle="tab" href="#Descad">Consultar Descadastramento Massivo</a></li>
                    <li><a data-toggle="tab" href="#ConsFatEmail">Consultar Referencias Fatura por E-mail</a></li>
                    <li><a data-toggle="tab" href="#ConsBeneficio">Consultar Benefício</a></li>
                </ul>

                <div class="tab-content">
                    <div id="TarifasGA" class="tab-pane fade in active" runat="server"></div>
                    <div id="TarifasGB" class="tab-pane fade in">
                        <div class="row margin-top-15">
                            <div class="col-md-3">
                                <div class='input-group'>
                                    <span class='input-group-addon'>Percentual de ICMS</span>
                                    <asp:DropDownList ID="IcmsTarifasGrupoB" CssClass="combo-style" runat="Server" AutoPostBack="True" OnSelectedIndexChanged="IcmsTarifasGrupoB_OnSelectedIndexChanged"></asp:DropDownList>
                                    <span class='input-group-addon'>%</span>
                                </div>
                            </div>
                        </div>
                        <ul class="nav nav-tabs margin-top-15">
                            <li class="active"><a data-toggle="tab" href="#TabBaixaRenda">Baixa Renda</a></li>
                            <li><a data-toggle="tab" href="#TabResidenciaNormal">Residencial Normal</a></li>
                            <li><a data-toggle="tab" href="#TabDemaisTarifas">Demais Tarifas</a></li>
                        </ul>
                        <div class="tab-content">
                            <div id="TabBaixaRenda" class="tab-pane fade in active">
                                <asp:UpdatePanel runat="server" UpdateMode="Conditional">
                                    <ContentTemplate>
                                        <div id="BaixaRenda" runat="server"></div>
                                    </ContentTemplate>
                                    <Triggers>
                                        <asp:AsyncPostBackTrigger ControlID="IcmsTarifasGrupoB" EventName="SelectedIndexChanged" />
                                    </Triggers>
                                </asp:UpdatePanel>
                            </div>
                            <div id="TabResidenciaNormal" class="tab-pane fade in">
                                <asp:UpdatePanel runat="server" UpdateMode="Conditional">
                                    <ContentTemplate>
                                        <div id="ResidenciaNormal" runat="server"></div>
                                    </ContentTemplate>
                                    <Triggers>
                                        <asp:AsyncPostBackTrigger ControlID="IcmsTarifasGrupoB" EventName="SelectedIndexChanged" />
                                    </Triggers>
                                </asp:UpdatePanel>
                            </div>
                            <div id="TabDemaisTarifas" class="tab-pane fade in">
                                <asp:UpdatePanel runat="server" UpdateMode="Conditional">
                                    <ContentTemplate>
                                        <div id="DemaisTarifas" runat="server"></div>
                                    </ContentTemplate>
                                    <Triggers>
                                        <asp:AsyncPostBackTrigger ControlID="IcmsTarifasGrupoB" EventName="SelectedIndexChanged" />
                                    </Triggers>
                                </asp:UpdatePanel>
                            </div>
                        </div>
                    </div>
                    <div id="TarifasIP" class="tab-pane fade in">
                        <asp:UpdatePanel runat="server" ID="PainelTarifasIp" UpdateMode="Conditional">
                            <ContentTemplate>
                                <div class="row margin-top-15">
                                    <div class="col-md-4">
                                        <div class='input-group'>
                                            <span class='input-group-addon'>Município:</span>
                                            <asp:DropDownList ID="MunicipioTarifasIp" CssClass="combo-style" runat="Server" AutoPostBack="True" OnSelectedIndexChanged="MunicipioTarifasIp_OnSelectedIndexChanged"></asp:DropDownList>
                                        </div>
                                    </div>
                                </div>
                                <div id="TarifasIluminacaoPublica" runat="server"></div>
                            </ContentTemplate>
                            <Triggers>
                                <asp:AsyncPostBackTrigger ControlID="MunicipioTarifasIp" EventName="SelectedIndexChanged" />
                            </Triggers>
                        </asp:UpdatePanel>
                    </div>
                    <div id="TarifasDiversas" class="tab-pane fade in" runat="server"></div>
                    <div id="TarifasMedidores" class="tab-pane fade in" runat="server"></div>
                    <div id="CalendarioDeLeitura" class="tab-pane fade in">
                        <asp:UpdatePanel ID="pnlCalLeituras" runat="server" UpdateMode="Conditional">
                            <ContentTemplate>
                                <div class="row margin-top-15">
                                    <div class="col-md-2">
                                        <div class='input-group'>
                                            <span class='input-group-addon'>Lote</span>
                                            <asp:TextBox runat="server" ID="Lote" Style="text-align: center; width: 100%; height: 45px;" MaxLength="2" ValidationGroup="caleit"></asp:TextBox>
                                        </div>
                                        <asp:RequiredFieldValidator runat="server" ValidationGroup="caleit" CssClass="text-error-field" SetFocusOnError="True" Display="Dynamic" ControlToValidate="Lote" ErrorMessage="Campo de preenchimento obrigatório"></asp:RequiredFieldValidator>
                                    </div>
                                    <div class="col-md-4 no-padding">
                                        <div class='input-group'>
                                            <span class='input-group-addon'>Período (Mês/Ano)</span>
                                            <asp:DropDownList ID="CalMes" runat="Server" CssClass="combo-style">
                                                <asp:ListItem Value="1" Text="Janeiro" />
                                                <asp:ListItem Value="2" Text="Fevereiro" />
                                                <asp:ListItem Value="3" Text="Março" />
                                                <asp:ListItem Value="4" Text="Abril" />
                                                <asp:ListItem Value="5" Text="Maio" />
                                                <asp:ListItem Value="6" Text="Junho" />
                                                <asp:ListItem Value="7" Text="Julho" />
                                                <asp:ListItem Value="8" Text="Agosto" />
                                                <asp:ListItem Value="9" Text="Setembro" />
                                                <asp:ListItem Value="10" Text="Outubro" />
                                                <asp:ListItem Value="11" Text="Novembro" />
                                                <asp:ListItem Value="12" Text="Dezembro" />
                                            </asp:DropDownList>
                                        </div>
                                    </div>
                                    <div class="col-md-2 no-padding">
                                        <div class='input-group'>
                                            <span class='input-group-addon'>de</span>
                                            <asp:DropDownList ID="CalAno" runat="Server" CssClass="combo-style">
                                            </asp:DropDownList>
                                        </div>
                                    </div>
                                    <div class="col-md-2">
                                        <asp:Button ID="Ler" Text="Ler" runat="server" CssClass="map__cta btn-cta max-width" OnClick="Ler_OnClick" ValidationGroup="caleit" />
                                    </div>
                                </div>


                                <div id="CalendarioDeLeituraTable" runat="server"></div>
                            </ContentTemplate>
                            <Triggers>
                                <asp:AsyncPostBackTrigger ControlID="Ler" EventName="Click" />
                            </Triggers>
                        </asp:UpdatePanel>
                    </div>
                    <div id="FaturamentoLeiturista" class="tab-pane fade in" runat="server"></div>
                    <div id="HistoricoDeOrdens" class="tab-pane fade in" runat="server">
                        <asp:UpdatePanel ID="UpdatePanel1" runat="server" UpdateMode="Conditional">
                            <ContentTemplate>
                                <asp:Panel ID="pnlHistOrdem" runat="server"></asp:Panel>
                            </ContentTemplate>
                        </asp:UpdatePanel>

                        <%--<uc1:historicoordem runat="server" ID="formulario" />--%>
                    </div>
                    <div id="Multas" class="tab-pane fade in" runat="server">
                        <uc1:ConsultarMultasIndevidas runat="server" ID="ConsultarMultasIndevidas" />
                    </div>
                    <div id="Descad" class="tab-pane fade in" runat="server">
                        <uc1:DescadastramentoEnel runat="server" ID="DescadastramentoEnel" />
                    </div>
                    <div id="ConsFatEmail" class="tab-pane fade in" runat="server">
                        <uc1:ConsultarClientesFaturaPorEmail runat="server" ID="ConsultarClientesFaturaPorEmail" />
                    </div>
                    <div id="ConsBeneficio" class="tab-pane fade in" runat="server">
                        <uc1:ConsultarBeneficio runat="server" ID="ConsultarBeneficio" />
                    </div>
                </div>
            </div>
        </div>
    </form>
    <script src="content/js/jquery.js"></script>
    <script src="content/js/bootstrap.min.js"></script>
    <script src="content/js/bootstrap-formhelpers.min.js"></script>
    <script src="content/js/jquery.treetable.js"></script>
    <script src="content/js/CpfCnpjMask.js"></script>
    <script src="content/js/toast-oh.js"></script>
    <script src="content/js/forms-oh.js"></script>
    <script>
        $("#tarifas-ga, #faturamento-leiturista, #tarifas-diversas, #tarifas-medidores,#tarifas-baixa-renda,#tarifas-gb-normais,#calendario-leituras, #tarifas-ilum-pub").treetable({
            expandable: true
        });
        function RecarregarTreetable() {
            $("#tarifas-ga, #faturamento-leiturista, #tarifas-diversas, #tarifas-medidores,#tarifas-baixa-renda,#tarifas-gb-normais,#calendario-leituras, #tarifas-ilum-pub").treetable({
                expandable: true
            });
        }
    </script>
</body>
</html>

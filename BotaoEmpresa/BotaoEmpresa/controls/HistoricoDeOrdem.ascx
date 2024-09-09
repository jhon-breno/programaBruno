<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="HistoricoDeOrdem.ascx.cs" Inherits="BotaoEmpresa.Controls.HistoricoDeOrdem" %>


<style>
    .input-group-addon {
        background-color: #fff;
        border: 0;
        border-bottom: 0;
        -ms-border-radius: 0;
        border-radius: 0;
        border-bottom: 0;
    }
</style>
<div class="row center">
    <img src="https://www.eneldistribuicao.com.br/assets/images/navbar-logo-color.png" alt="Enel" style="position: absolute; top: 25px; left: 50px;" />
    <div class="col-xs-12">
        <h1 class="text-center padding-bottom-20">Histórico de Ordens Ceará</h1>
    </div>
    <div class="container">
        <div class="col-xs-12" style="text-align: right;">
            <asp:Button runat="server" ID="Logout" OnClick="Logout_OnClick" Style="background: none!important; border: none; padding: 0!important; color: #069; cursor: pointer;"></asp:Button>
        </div>
    </div>
</div>

<asp:UpdatePanel ID="UpPanelStp1" runat="server" UpdateMode="Conditional">
    <ContentTemplate>
        <div class="container">

            <asp:Panel runat="server" DefaultButton="Buscar">
                <div class="step-cliente">
                    <div class="row">
                        <div class="col-xs-12 col-md-3 grupo-field">
                            <div class="form-group">
                                <span>Número de Cliente</span>
                                <asp:TextBox ID="NumCliente" CssClass="text-field" runat="server"></asp:TextBox>
                            </div>
                        </div>
                        <div class="col-xs-12 col-md-3 grupo-field">
                            <div class="form-group">
                                <span>Número da Ordem</span>
                                <asp:TextBox ID="NumeroOrdem" CssClass="text-field" runat="server"></asp:TextBox>
                            </div>
                        </div>
                        <div class="col-xs-12 col-md-2" style="margin-top: 15px;">
                            <asp:Button runat="server" ID="Buscar" CssClass="btn-base btn-send" Text="Buscar" OnClick="Buscar_OnClick" />
                        </div>
                    </div>
                </div>

            </asp:Panel>

            <div class="step-1 hidden">

                <div class="row">
                    <div class="col-xs-12 col-md-12 grupo-field">
                        <div class="form-group">
                            <input type="text" id="txtBusca" class="text-field" placeholder="Digite uma ordem para filtrar..." />
                        </div>
                    </div>
                </div>

                <div class="table-responsive">
                    <asp:GridView ID="GrdOrdens" runat="server" AutoGenerateColumns="false" OnRowCommand="GrdOrdens_RowCommand" CssClass="table table-striped fit" EmptyDataText="Nenhuma ordem encontrada para o cliente informado.">
                        <Columns>
                            <asp:TemplateField HeaderText="Ordem">
                                <ItemTemplate>
                                    <asp:LinkButton ID="Details" runat="server" Text='<%# Eval("NumeroOrdem") %>' CommandName="Details" CommandArgument='<%#Eval("NumeroOrdem")+","+ Eval("TipoOrdem")%>'></asp:LinkButton>
                                </ItemTemplate>
                            </asp:TemplateField>
                            <asp:BoundField DataField="TipoOrdem" HeaderText="Tipo Ordem" />
                            <asp:BoundField DataField="DescricaoTipoServico" HeaderText="Descrição Tipo Serviço" />
                            <asp:BoundField DataField="DataIngresso" HeaderText="Data Ingresso" DataFormatString="{0:dd/MM/yyyy}" />
                            <asp:BoundField DataField="Estado" HeaderText="Estado" />
                            <asp:BoundField DataField="Prazo" HeaderText="Prazo" />
                            <asp:BoundField DataField="Etapa" HeaderText="Etapa" />
                        </Columns>
                    </asp:GridView>
                </div>
            </div>

            <div class="step-2 hidden">
                <div class="row padding-bottom-20">
                    <div class="col-xs-12 col-md-2">
                        <asp:Button runat="server" ID="Voltar" CssClass="btn-base btn-cancel" Text="Voltar" OnClick="Voltar_OnClick" />
                    </div>
                </div>
                <fieldset class="field-set">
                    <legend runat="server" id="Legend1">Dados do Cliente</legend>
                    <div class="row">
                        <div class="col-xs-12 col-md-3 grupo-field">
                            <div class="form-group">
                                <span>Número do Cliente</span>
                                <asp:TextBox ID="NumClienteOrdem" CssClass="text-field" runat="server" ReadOnly="True"></asp:TextBox>
                            </div>
                        </div>
                        <div class="col-xs-12 col-md-6 grupo-field">
                            <div class="form-group">
                                <span>Nome do Cliente</span>
                                <asp:TextBox ID="NomeClienteOrdem" CssClass="text-field" runat="server" ReadOnly="True"></asp:TextBox>
                            </div>
                        </div>
                        <div class="col-xs-12 col-md-3 grupo-field">
                            <div class="form-group">
                                <span>Status da Conexão</span>
                                <asp:TextBox ID="EstadoSuministroOrdem" CssClass="text-field" runat="server" ReadOnly="True"></asp:TextBox>
                            </div>
                        </div>
                    </div>
                </fieldset>

                <fieldset class="field-set">
                    <legend runat="server" id="NumOrdemLegend">Dados da Ordem</legend>

                    <div class="row">
                        <div class="col-xs-12 col-md-6 grupo-field">
                            <div class="form-group">
                                <span>Tipo</span>
                                <asp:TextBox ID="Tipo" CssClass="text-field" runat="server" ReadOnly="True"></asp:TextBox>
                            </div>
                        </div>
                        <div class="col-xs-12 col-md-6 grupo-field">
                            <div class="form-group">
                                <span>Serviço</span>
                                <asp:TextBox ID="Servico" CssClass="text-field" runat="server" ReadOnly="True"></asp:TextBox>
                            </div>
                        </div>
                    </div>

                    <div class="row">
                        <div class="col-xs-12 col-md-6 grupo-field">
                            <div class="form-group">
                                <span>Sucursal Origem</span>
                                <asp:TextBox ID="SucOrigem" CssClass="text-field" runat="server" ReadOnly="True"></asp:TextBox>
                            </div>
                        </div>
                        <div class="col-xs-12 col-md-6 grupo-field">
                            <div class="form-group">
                                <span>Área Origem</span>
                                <asp:TextBox ID="AreaOrigem" CssClass="text-field" runat="server" ReadOnly="True"></asp:TextBox>
                            </div>
                        </div>
                    </div>

                    <div class="row">
                        <div class="col-xs-12 col-md-6 grupo-field">
                            <div class="form-group">
                                <span>Sucursal Destino</span>
                                <asp:TextBox ID="SucDestino" CssClass="text-field" runat="server" ReadOnly="True"></asp:TextBox>
                            </div>
                        </div>
                        <div class="col-xs-12 col-md-6 grupo-field">
                            <div class="form-group">
                                <span>Área Destino</span>
                                <asp:TextBox ID="AreaDestino" CssClass="text-field" runat="server" ReadOnly="True"></asp:TextBox>
                            </div>
                        </div>
                    </div>

                    <div class="row">
                        <div class="col-xs-12 col-md-6 grupo-field">
                            <div class="form-group">
                                <span>Etapa</span>
                                <asp:TextBox ID="Etapa" CssClass="text-field" runat="server" ReadOnly="True"></asp:TextBox>
                            </div>
                        </div>
                        <div class="col-xs-12 col-md-6 grupo-field">
                            <div class="form-group">
                                <span>Data Ingressso</span>
                                <asp:TextBox ID="DataIngresso" CssClass="text-field" runat="server" ReadOnly="True"></asp:TextBox>
                            </div>
                        </div>
                    </div>

                    <div class="row">
                        <div class="col-xs-12 col-md-6 grupo-field">
                            <div class="form-group">
                                <span>Estado</span>
                                <asp:TextBox ID="Estado" CssClass="text-field" runat="server" ReadOnly="True"></asp:TextBox>
                            </div>
                        </div>
                        <div class="col-xs-12 col-md-6 grupo-field">
                            <div class="form-group">
                                <span>Usuário</span>
                                <asp:TextBox ID="Usuario" CssClass="text-field" runat="server" ReadOnly="True"></asp:TextBox>
                            </div>
                        </div>
                    </div>

                    <div class="row">
                        <div class="col-xs-12 col-md-6 grupo-field">
                            <div class="form-group">
                                <span>Atendimento</span>
                                <asp:TextBox ID="Atendimento" CssClass="text-field" runat="server" ReadOnly="True"></asp:TextBox>
                            </div>
                        </div>
                        <div class="col-xs-12 col-md-6 grupo-field">
                            <div class="form-group">
                                <span>Ordem Original</span>
                                <asp:TextBox ID="OrdemOriginal" CssClass="text-field" runat="server" ReadOnly="True"></asp:TextBox>
                            </div>
                        </div>
                    </div>


                </fieldset>

                <fieldset class="field-set">
                    <legend>Dados da Finalização</legend>
                    <div class="row">
                        <div class="col-xs-12 col-md-6 grupo-field">
                            <div class="form-group">
                                <span>Finalizada Por</span>
                                <asp:TextBox ID="FinalizadaPor" CssClass="text-field" runat="server" ReadOnly="True"></asp:TextBox>
                            </div>
                        </div>
                        <div class="col-xs-12 col-md-6 grupo-field">
                            <div class="form-group">
                                <span>Data</span>
                                <asp:TextBox ID="DataFinalizacao" CssClass="text-field" runat="server" ReadOnly="True"></asp:TextBox>
                            </div>
                        </div>
                    </div>
                    <div class="row">
                        <div class="col-xs-12 col-md-6 grupo-field">
                            <div class="form-group">
                            </div>
                        </div>
                    </div>
                </fieldset>

                <div class="row">
                    <div class="col-xs-12 col-md-6">
                        <fieldset class="field-set">
                            <legend>Observações de Atendimento</legend>
                            <asp:Literal runat="server" ID="ObsAtendimento"></asp:Literal>
                        </fieldset>
                    </div>
                    <div class="col-xs-12 col-md-6">
                        <fieldset class="field-set">
                            <legend>Observações do Executante</legend>
                            <asp:Literal runat="server" ID="ObsExecutante"></asp:Literal>
                        </fieldset>
                    </div>
                </div>

                <div class="row" style="overflow-x: overlay;">
                    <div class="col-xs-12 col-md-12">
                        <fieldset class="field-set">
                            <legend>Visitas da Ordem</legend>
                            <div style="overflow-x: overlay;">
                                <asp:GridView ID="GrdVisitas" runat="server" AutoGenerateColumns="false" OnRowCommand="GrdOrdens_RowCommand" CssClass="table table-striped fit" EmptyDataText="Nenhuma visita encontrada para a ordem selecionada.">
                                    <Columns>
                                        <asp:BoundField DataField="NumeroVisita" HeaderText="Nº" />
                                        <asp:BoundField DataField="Etapa" HeaderText="Etapa" />
                                        <asp:BoundField DataField="DataVisita" HeaderText="Data Visita" DataFormatString="{0:dd/MM/yyyy}" />
                                        <asp:BoundField DataField="AreaExecutante" HeaderText="Área Executante" />
                                        <asp:BoundField DataField="ResponsavelDespacho" HeaderText="Responsável Despacho" />
                                        <asp:BoundField DataField="DataExec" HeaderText="Data Executante" />
                                        <asp:BoundField DataField="HoraExec" HeaderText="Hora Executante" />
                                        <asp:BoundField DataField="DescricaoRetorno" HeaderText="Descrição Retorno" />
                                        <asp:BoundField DataField="DataRetorno" HeaderText="Data Retorno" />
                                        <asp:TemplateField HeaderText="Ordem Relacionada">
                                            <ItemTemplate>
                                                <asp:LinkButton ID="DetailsOrdemRelacionada" runat="server" Text='<%# Eval("OrdemRelacionada") %>' CommandName="DetailsOrdemFilha" CommandArgument='<%#Eval("OrdemRelacionada")%>'></asp:LinkButton>
                                            </ItemTemplate>
                                        </asp:TemplateField>
                                    </Columns>
                                </asp:GridView>
                            </div>
                        </fieldset>
                    </div>
                </div>

            </div>

            <div class="step-oi hidden">
                <div class="row padding-bottom-20">
                    <div class="col-xs-12 col-md-2">
                        <asp:Button runat="server" ID="VoltarOi" CssClass="btn-base btn-cancel" Text="Voltar" OnClick="Voltar_OnClick" />
                    </div>
                </div>

                <fieldset class="field-set">
                    <legend>Dados do Cliente</legend>
                    <div class="row">
                        <div class="col-xs-12 col-md-12 grupo-field">
                            <div class="form-group">
                                <span>Endereco</span>
                                <asp:TextBox ID="Endereco" CssClass="text-field" runat="server" ReadOnly="True"></asp:TextBox>
                            </div>
                        </div>
                    </div>
                </fieldset>

                <fieldset class="field-set">
                    <legend>Executante</legend>
                    <div class="row">
                        <div class="col-xs-12 col-md-6 grupo-field">
                            <div class="form-group">
                                <span>Executante</span>
                                <asp:TextBox ID="Executante" CssClass="text-field" runat="server" ReadOnly="True"></asp:TextBox>
                            </div>
                        </div>
                        <div class="col-xs-12 col-md-6 grupo-field">
                            <div class="form-group">
                                <span>Nome</span>
                                <asp:TextBox ID="Nome" CssClass="text-field" runat="server" ReadOnly="True"></asp:TextBox>
                            </div>
                        </div>

                    </div>
                </fieldset>

                <fieldset class="field-set">
                    <legend runat="server" id="NumOrdemOiLegend">Ordem de Inspeção O.I.</legend>
                    <div class="row">
                        <div class="col-xs-12 col-md-6 grupo-field">
                            <div class="form-group">
                                <span>Número TDC</span>
                                <asp:TextBox ID="OiNumTdc" CssClass="text-field" runat="server" ReadOnly="True"></asp:TextBox>
                            </div>
                        </div>
                        <div class="col-xs-12 col-md-6 grupo-field">
                            <div class="form-group">
                                <span>Situação</span>
                                <asp:TextBox ID="OiStituacao" CssClass="text-field" runat="server" ReadOnly="True"></asp:TextBox>
                            </div>
                        </div>
                    </div>
                </fieldset>

                <fieldset class="field-set">
                    <legend>Medidor</legend>

                    <div class="row">
                        <div class="col-xs-12 col-md-6">
                            <fieldset class="field-set">
                                <legend>Origem Cadastrado</legend>
                                <div class="row">

                                    <div class="col-xs-12 col-md-5 grupo-field">
                                        <div class="form-group">
                                            <span>Nº (COELCE)</span>
                                            <asp:TextBox ID="OiCadNumCoelce" CssClass="text-field" runat="server" ReadOnly="True"></asp:TextBox>
                                        </div>
                                    </div>
                                    <div class="col-xs-12 col-md-5 grupo-field">
                                        <div class="form-group">
                                            <span>Nº (Fabrica)</span>
                                            <asp:TextBox ID="OiCadNumFabrica" CssClass="text-field" runat="server" ReadOnly="True"></asp:TextBox>
                                        </div>
                                    </div>
                                    <div class="col-xs-12 col-md-2 grupo-field">
                                        <div class="form-group">
                                            <span>Alg.</span>
                                            <asp:TextBox ID="OiCadAlg" CssClass="text-field" runat="server" ReadOnly="True"></asp:TextBox>
                                        </div>
                                    </div>
                                </div>
                                <div class="row">
                                    <div class="col-xs-12 col-md-3 grupo-field">
                                        <div class="form-group">
                                            <span style="white-space: nowrap;">Fabricante (Cod/Desc.)</span>
                                            <asp:TextBox ID="OiCadFabricante" CssClass="text-field" runat="server" ReadOnly="True"></asp:TextBox>
                                        </div>
                                    </div>
                                    <div class="col-xs-12 col-md-6 grupo-field">
                                        <div class="form-group">
                                            <span style="color: #fff">&nbsp;</span>
                                            <asp:TextBox ID="OiCadDescricao" CssClass="text-field" runat="server" ReadOnly="True"></asp:TextBox>
                                        </div>
                                    </div>
                                    <div class="col-xs-12 col-md-3 grupo-field">
                                        <div class="form-group">
                                            <span>Tipo</span>
                                            <asp:TextBox ID="OiCadTipo" CssClass="text-field" runat="server" ReadOnly="True"></asp:TextBox>
                                        </div>
                                    </div>

                                </div>
                                <div class="row">
                                    <div class="col-xs-12 col-md-3 grupo-field">
                                        <div class="form-group">
                                            <span>Modelo</span>
                                            <asp:TextBox ID="OiCadModelo" CssClass="text-field" runat="server" ReadOnly="True"></asp:TextBox>
                                        </div>
                                    </div>
                                    <div class="col-xs-12 col-md-4 grupo-field">
                                        <div class="form-group">
                                            <span>Última Leitura</span>
                                            <asp:TextBox ID="OiCadUltimaLeitura" CssClass="text-field" runat="server" ReadOnly="True"></asp:TextBox>
                                        </div>
                                    </div>
                                    <div class="col-xs-12 col-md-5 grupo-field">
                                        <div class="form-group">
                                            <span>Data Última Instalação</span>
                                            <asp:TextBox ID="OiCadUltimaInstalacao" CssClass="text-field" runat="server" ReadOnly="True"></asp:TextBox>
                                        </div>
                                    </div>
                                </div>
                            </fieldset>
                        </div>
                        <div class="col-xs-12 col-md-6">
                            <fieldset class="field-set">
                                <legend>Origem Encontrado</legend>
                                <div class="row">

                                    <div class="col-xs-12 col-md-5 grupo-field">
                                        <div class="form-group">
                                            <span>Nº (COELCE)</span>
                                            <asp:TextBox ID="OiEncNumCoelce" CssClass="text-field" runat="server" ReadOnly="True"></asp:TextBox>
                                        </div>
                                    </div>
                                    <div class="col-xs-12 col-md-5 grupo-field">
                                        <div class="form-group">
                                            <span>Nº (Fabrica)</span>
                                            <asp:TextBox ID="OiEncNumFabrica" CssClass="text-field" runat="server" ReadOnly="True"></asp:TextBox>
                                        </div>
                                    </div>
                                    <div class="col-xs-12 col-md-2 grupo-field">
                                        <div class="form-group">
                                            <span>Alg.</span>
                                            <asp:TextBox ID="OiEncAlg" CssClass="text-field" runat="server" ReadOnly="True"></asp:TextBox>
                                        </div>
                                    </div>
                                </div>
                                <div class="row">
                                    <div class="col-xs-12 col-md-3 grupo-field">
                                        <div class="form-group">
                                            <span style="white-space: nowrap;">Fabricante (Cod/Desc.)</span>
                                            <asp:TextBox ID="OiEncFabricante" CssClass="text-field" runat="server" ReadOnly="True"></asp:TextBox>
                                        </div>
                                    </div>
                                    <div class="col-xs-12 col-md-4 grupo-field">
                                        <div class="form-group">
                                            <span style="color: #fff">&nbsp;</span>
                                            <asp:TextBox ID="OiEncDescricao" CssClass="text-field" runat="server" ReadOnly="True"></asp:TextBox>
                                        </div>
                                    </div>
                                    <div class="col-xs-12 col-md-2 grupo-field">
                                        <div class="form-group">
                                            <span>Tipo</span>
                                            <asp:TextBox ID="OiEncTipo" CssClass="text-field" runat="server" ReadOnly="True"></asp:TextBox>
                                        </div>
                                    </div>
                                    <div class="col-xs-12 col-md-3 grupo-field">
                                        <div class="form-group">
                                            <span>Modelo</span>
                                            <asp:TextBox ID="OiEncModelo" CssClass="text-field" runat="server" ReadOnly="True"></asp:TextBox>
                                        </div>
                                    </div>
                                </div>
                                <div class="row">
                                    <div class="col-xs-12 col-md-6 grupo-field">
                                        <div class="form-group">
                                            <span>Última Leitura</span>
                                            <asp:TextBox ID="OiEncUltimaLeitura" CssClass="text-field" runat="server" ReadOnly="True"></asp:TextBox>
                                        </div>
                                    </div>
                                </div>
                            </fieldset>
                        </div>
                    </div>

                    <div class="row">
                    </div>
                </fieldset>

                <fieldset class="field-set">
                    <legend>Anormalidades</legend>
                    <div class="row">
                        <div class="col-xs-12 col-md-2 grupo-field">
                            <div class="form-group">
                                <span>01</span>
                                <asp:TextBox ID="Anor01" CssClass="text-field" runat="server" ReadOnly="True" title="MEDIDOR NÃO REGISTRA CONSUMO REAL"></asp:TextBox>
                            </div>
                        </div>

                        <div class="col-xs-12 col-md-2 grupo-field">
                            <div class="form-group">
                                <span>02</span>
                                <asp:TextBox ID="Anor02" CssClass="text-field" runat="server" ReadOnly="True" title="TESTE MEDIDOR COM ADR M2000/4000"></asp:TextBox>
                            </div>
                        </div>
                        <div class="col-xs-12 col-md-2 grupo-field">
                            <div class="form-group">
                                <span>03</span>
                                <asp:TextBox ID="Anor03" CssClass="text-field" runat="server" ReadOnly="True" title="CLIENTE NÃO ASSINOU TOI"></asp:TextBox>
                            </div>
                        </div>
                        <div class="col-xs-12 col-md-2 grupo-field">
                            <div class="form-group">
                                <span>04</span>
                                <asp:TextBox ID="Anor04" CssClass="text-field" runat="server" ReadOnly="True" title="MEDIÇÃO AGRUPADA INTERNA"></asp:TextBox>
                            </div>
                        </div>
                        <div class="col-xs-12 col-md-2 grupo-field">
                            <div class="form-group">
                                <span>05</span>
                                <asp:TextBox ID="Anor05" CssClass="text-field" runat="server" ReadOnly="True" title="RESIDENCIAL DE MÉDIO PORTE"></asp:TextBox>
                            </div>
                        </div>
                        <div class="col-xs-12 col-md-2 grupo-field">
                            <div class="form-group">
                                <span>Total</span>
                                <asp:TextBox ID="AnorTotal" CssClass="text-field" runat="server" ReadOnly="True"></asp:TextBox>
                            </div>
                        </div>
                    </div>
                </fieldset>


                <fieldset class="field-set">
                    <legend>NG's</legend>
                    <div class="row">
                        <div class="col-xs-12 col-md-2 grupo-field">
                            <div class="form-group">
                                <span>01</span>
                                <asp:TextBox ID="Ng01" CssClass="text-field" runat="server" ReadOnly="True"></asp:TextBox>
                            </div>
                        </div>

                        <div class="col-xs-12 col-md-2 grupo-field">
                            <div class="form-group">
                                <span>02</span>
                                <asp:TextBox ID="Ng02" CssClass="text-field" runat="server" ReadOnly="True"></asp:TextBox>
                            </div>
                        </div>
                        <div class="col-xs-12 col-md-2 grupo-field">
                            <div class="form-group">
                                <span>03</span>
                                <asp:TextBox ID="Ng03" CssClass="text-field" runat="server" ReadOnly="True"></asp:TextBox>
                            </div>
                        </div>
                        <div class="col-xs-12 col-md-2 grupo-field">
                            <div class="form-group">
                                <span>04</span>
                                <asp:TextBox ID="Ng04" CssClass="text-field" runat="server" ReadOnly="True"></asp:TextBox>
                            </div>
                        </div>
                        <div class="col-xs-12 col-md-2 grupo-field">
                            <div class="form-group">
                                <span>05</span>
                                <asp:TextBox ID="Ng05" CssClass="text-field" runat="server" ReadOnly="True"></asp:TextBox>
                            </div>
                        </div>
                        <div class="col-xs-12 col-md-2 grupo-field">
                            <div class="form-group">
                                <span>Total</span>
                                <asp:TextBox ID="NgTotal" CssClass="text-field" runat="server" ReadOnly="True"></asp:TextBox>
                            </div>
                        </div>
                    </div>
                </fieldset>

                <div class="row">
                    <div class="col-xs-12 col-md-6">
                        <fieldset class="field-set">
                            <legend>Observações</legend>
                            <asp:Literal runat="server" ID="ObservacoesOi"></asp:Literal>
                        </fieldset>
                    </div>
                    <div class="col-xs-12 col-md-6">
                        <fieldset class="field-set">
                            <legend>Data/Hora Execução</legend>
                            <div class="col-xs-12 col-md-4  grupo-field">
                                <div class="form-group">
                                    <span>Data</span>
                                    <asp:TextBox ID="DataExecOi" CssClass="text-field" runat="server" ReadOnly="True"></asp:TextBox>
                                </div>
                            </div>
                            <div class="col-xs-12 col-md-4 grupo-field">
                                <div class="form-group">
                                    <span>Hora Inic.</span>
                                    <asp:TextBox ID="HoraIniExecOi" CssClass="text-field" runat="server" ReadOnly="True"></asp:TextBox>
                                </div>
                            </div>
                            <div class="col-xs-12 col-md-4 grupo-field">
                                <div class="form-group">
                                    <span>Hora Exec.</span>
                                    <asp:TextBox ID="HoraFimExecOi" CssClass="text-field" runat="server" ReadOnly="True"></asp:TextBox>
                                </div>
                            </div>
                        </fieldset>
                    </div>
                </div>
            </div>
            <div class="step-ot hidden">
                <div class="row padding-bottom-20">
                    <div class="col-xs-12 col-md-2">
                        <asp:Button runat="server" ID="VoltarOt" CssClass="btn-base btn-cancel" Text="Voltar" OnClick="Voltar_OnClick" />
                    </div>
                </div>

                <fieldset class="field-set">
                    <legend>Dados do Cliente</legend>
                    <div class="row">
                        <div class="col-xs-12 col-md-12 grupo-field">
                            <div class="form-group">
                                <span>Endereco</span>
                                <asp:TextBox ID="OtEndereco" CssClass="text-field" runat="server" ReadOnly="True"></asp:TextBox>
                            </div>
                        </div>
                    </div>
                </fieldset>

                <fieldset class="field-set">
                    <legend>Executante</legend>
                    <div class="row">
                        <div class="col-xs-12 col-md-6 grupo-field">
                            <div class="form-group">
                                <span>Eletricista</span>
                                <asp:TextBox ID="OtExecutante" CssClass="text-field" runat="server" ReadOnly="True"></asp:TextBox>
                            </div>
                        </div>
                        <div class="col-xs-12 col-md-6 grupo-field">
                            <div class="form-group">
                                <span>Nome</span>
                                <asp:TextBox ID="OtNome" CssClass="text-field" runat="server" ReadOnly="True"></asp:TextBox>
                            </div>
                        </div>
                    </div>
                </fieldset>

                <fieldset class="field-set">
                    <legend runat="server" id="NumOrdemOtLegend">Ordem de Inspeção O.T.</legend>
                    <div class="row">
                        <div class="col-xs-12 col-md-6 grupo-field">
                            <div class="form-group">
                                <span>Número TDC</span>
                                <asp:TextBox ID="OtNumTdc" CssClass="text-field" runat="server" ReadOnly="True"></asp:TextBox>
                            </div>
                        </div>
                        <div class="col-xs-12 col-md-6 grupo-field">
                            <div class="form-group">
                                <span>Situação</span>
                                <asp:TextBox ID="OtStituacao" CssClass="text-field" runat="server" ReadOnly="True"></asp:TextBox>
                            </div>
                        </div>
                    </div>
                </fieldset>

                <fieldset class="field-set">
                    <legend>Medidor</legend>

                    <div class="row">
                        <div class="col-xs-12 col-md-6">
                            <fieldset class="field-set">
                                <legend>Origem Encontrado</legend>
                                <div class="row">

                                    <div class="col-xs-12 col-md-5 grupo-field">
                                        <div class="form-group">
                                            <span>Nº (COELCE)</span>
                                            <asp:TextBox ID="OtCadNumCoelce" CssClass="text-field" runat="server" ReadOnly="True"></asp:TextBox>
                                        </div>
                                    </div>
                                    <div class="col-xs-12 col-md-5 grupo-field">
                                        <div class="form-group">
                                            <span>Nº (Fabrica)</span>
                                            <asp:TextBox ID="OtCadNumFabrica" CssClass="text-field" runat="server" ReadOnly="True"></asp:TextBox>
                                        </div>
                                    </div>
                                    <div class="col-xs-12 col-md-2 grupo-field">
                                        <div class="form-group">
                                            <span>Alg.</span>
                                            <asp:TextBox ID="OtCadAlg" CssClass="text-field" runat="server" ReadOnly="True"></asp:TextBox>
                                        </div>
                                    </div>
                                </div>
                                <div class="row">
                                    <div class="col-xs-12 col-md-3 grupo-field">
                                        <div class="form-group">
                                            <span style="white-space: nowrap;">Fabricante (Cod/Desc.)</span>
                                            <asp:TextBox ID="OtCadFabricante" CssClass="text-field" runat="server" ReadOnly="True"></asp:TextBox>
                                        </div>
                                    </div>
                                    <div class="col-xs-12 col-md-6 grupo-field">
                                        <div class="form-group">
                                            <span style="color: #fff">&nbsp;</span>
                                            <asp:TextBox ID="OtCadDescricao" CssClass="text-field" runat="server" ReadOnly="True"></asp:TextBox>
                                        </div>
                                    </div>
                                    <div class="col-xs-12 col-md-3 grupo-field">
                                        <div class="form-group">
                                            <span>Tipo</span>
                                            <asp:TextBox ID="OtCadTipo" CssClass="text-field" runat="server" ReadOnly="True"></asp:TextBox>
                                        </div>
                                    </div>

                                </div>
                                <div class="row">
                                    <div class="col-xs-12 col-md-3 grupo-field">
                                        <div class="form-group">
                                            <span>Modelo</span>
                                            <asp:TextBox ID="OtCadModelo" CssClass="text-field" runat="server" ReadOnly="True"></asp:TextBox>
                                        </div>
                                    </div>
                                    <div class="col-xs-12 col-md-4 grupo-field">
                                        <div class="form-group">
                                            <span>Última Leitura</span>
                                            <asp:TextBox ID="OtCadUltimaLeitura" CssClass="text-field" runat="server" ReadOnly="True"></asp:TextBox>
                                        </div>
                                    </div>
                                    <div class="col-xs-12 col-md-5 grupo-field">
                                        <div class="form-group">
                                            <span>Leitura Hp</span>
                                            <asp:TextBox ID="OtCadUltLeituraHp" CssClass="text-field" runat="server" ReadOnly="True"></asp:TextBox>
                                        </div>
                                    </div>
                                    <div class="col-xs-12 col-md-6 grupo-field">
                                        <div class="form-group">
                                            <span>Leitura Reat</span>
                                            <asp:TextBox ID="OtCadUltLeituraReat" CssClass="text-field" runat="server" ReadOnly="True"></asp:TextBox>
                                        </div>
                                    </div>
                                </div>
                            </fieldset>
                        </div>
                        <div class="col-xs-12 col-md-6">
                            <fieldset class="field-set">
                                <legend>Origem Instalado</legend>
                                <div class="row">

                                    <div class="col-xs-12 col-md-5 grupo-field">
                                        <div class="form-group">
                                            <span>Nº (COELCE)</span>
                                            <asp:TextBox ID="OtEncNumCoelce" CssClass="text-field" runat="server" ReadOnly="True"></asp:TextBox>
                                        </div>
                                    </div>
                                    <div class="col-xs-12 col-md-5 grupo-field">
                                        <div class="form-group">
                                            <span>Nº (Fabrica)</span>
                                            <asp:TextBox ID="OtEncNumFabrica" CssClass="text-field" runat="server" ReadOnly="True"></asp:TextBox>
                                        </div>
                                    </div>
                                    <div class="col-xs-12 col-md-2 grupo-field">
                                        <div class="form-group">
                                            <span>Alg.</span>
                                            <asp:TextBox ID="OtEncAlg" CssClass="text-field" runat="server" ReadOnly="True"></asp:TextBox>
                                        </div>
                                    </div>
                                </div>
                                <div class="row">
                                    <div class="col-xs-12 col-md-3 grupo-field">
                                        <div class="form-group">
                                            <span style="white-space: nowrap;">Fabricante (Cod/Desc.)</span>
                                            <asp:TextBox ID="OtEncFabricante" CssClass="text-field" runat="server" ReadOnly="True"></asp:TextBox>
                                        </div>
                                    </div>
                                    <div class="col-xs-12 col-md-4 grupo-field">
                                        <div class="form-group">
                                            <span style="color: #fff">&nbsp;</span>
                                            <asp:TextBox ID="OtEncDescricao" CssClass="text-field" runat="server" ReadOnly="True"></asp:TextBox>
                                        </div>
                                    </div>
                                    <div class="col-xs-12 col-md-2 grupo-field">
                                        <div class="form-group">
                                            <span>Tipo</span>
                                            <asp:TextBox ID="OtEncTipo" CssClass="text-field" runat="server" ReadOnly="True"></asp:TextBox>
                                        </div>
                                    </div>
                                    <div class="col-xs-12 col-md-3 grupo-field">
                                        <div class="form-group">
                                            <span>Modelo</span>
                                            <asp:TextBox ID="OtEncModelo" CssClass="text-field" runat="server" ReadOnly="True"></asp:TextBox>
                                        </div>
                                    </div>
                                </div>
                                <div class="row">
                                    <div class="col-xs-12 col-md-6 grupo-field">
                                        <div class="form-group">
                                            <span>Última Leitura</span>
                                            <asp:TextBox ID="OtEncUltimaLeitura" CssClass="text-field" runat="server" ReadOnly="True"></asp:TextBox>
                                        </div>
                                    </div>
                                    <div class="col-xs-12 col-md-6 grupo-field">
                                        <div class="form-group">
                                            <span>Leitura Hp</span>
                                            <asp:TextBox ID="OtEncUltLeituraHp" CssClass="text-field" runat="server" ReadOnly="True"></asp:TextBox>
                                        </div>
                                    </div>
                                    <div class="col-xs-12 col-md-6 grupo-field">
                                        <div class="form-group">
                                            <span>Leitura Reat</span>
                                            <asp:TextBox ID="OtEncUltLeituraReat" CssClass="text-field" runat="server" ReadOnly="True"></asp:TextBox>
                                        </div>
                                    </div>
                                </div>
                            </fieldset>
                        </div>
                    </div>

                    <div class="row">
                    </div>
                </fieldset>

                <fieldset class="field-set">
                    <legend>NG's</legend>
                    <div class="row">
                        <div class="col-xs-12 col-md-2 grupo-field">
                            <div class="form-group">
                                <span>01</span>
                                <asp:TextBox ID="NgOt01" CssClass="text-field" runat="server" ReadOnly="True"></asp:TextBox>
                            </div>
                        </div>

                        <div class="col-xs-12 col-md-2 grupo-field">
                            <div class="form-group">
                                <span>02</span>
                                <asp:TextBox ID="NgOt02" CssClass="text-field" runat="server" ReadOnly="True"></asp:TextBox>
                            </div>
                        </div>
                        <div class="col-xs-12 col-md-2 grupo-field">
                            <div class="form-group">
                                <span>03</span>
                                <asp:TextBox ID="NgOt03" CssClass="text-field" runat="server" ReadOnly="True"></asp:TextBox>
                            </div>
                        </div>
                        <div class="col-xs-12 col-md-2 grupo-field">
                            <div class="form-group">
                                <span>04</span>
                                <asp:TextBox ID="NgOt04" CssClass="text-field" runat="server" ReadOnly="True"></asp:TextBox>
                            </div>
                        </div>
                        <div class="col-xs-12 col-md-2 grupo-field">
                            <div class="form-group">
                                <span>05</span>
                                <asp:TextBox ID="NgOt05" CssClass="text-field" runat="server" ReadOnly="True"></asp:TextBox>
                            </div>
                        </div>
                        <div class="col-xs-12 col-md-2 grupo-field">
                            <div class="form-group">
                                <span>Total</span>
                                <asp:TextBox ID="NgOtTotal" CssClass="text-field" runat="server" ReadOnly="True"></asp:TextBox>
                            </div>
                        </div>
                    </div>
                </fieldset>

                <fieldset class="field-set">
                    <legend>Resp. NG's</legend>
                    <div class="row">
                        <div class="col-xs-12 col-md-2 grupo-field">
                            <div class="form-group">
                                <span>01</span>
                                <asp:TextBox ID="RespNg01" CssClass="text-field" runat="server" ReadOnly="True"></asp:TextBox>
                            </div>
                        </div>

                        <div class="col-xs-12 col-md-2 grupo-field">
                            <div class="form-group">
                                <span>02</span>
                                <asp:TextBox ID="RespNg02" CssClass="text-field" runat="server" ReadOnly="True"></asp:TextBox>
                            </div>
                        </div>
                    </div>
                </fieldset>

                <div class="row">
                    <div class="col-xs-12 col-md-6">
                        <fieldset class="field-set">
                            <legend>Observações</legend>
                            <asp:Literal runat="server" ID="ObservacoesOt"></asp:Literal>
                        </fieldset>
                    </div>
                    <div class="col-xs-12 col-md-6">
                        <fieldset class="field-set">
                            <legend>Data/Hora Execução</legend>
                            <div class="col-xs-12 col-md-4 grupo-field">
                                <div class="form-group">
                                    <span>Data</span>
                                    <asp:TextBox ID="DataExecOt" CssClass="text-field" runat="server" ReadOnly="True"></asp:TextBox>
                                </div>
                            </div>
                            <div class="col-xs-12 col-md-4 grupo-field">
                                <div class="form-group">
                                    <span>Hora Inic.</span>
                                    <asp:TextBox ID="HoraIniExecOt" CssClass="text-field" runat="server" ReadOnly="True"></asp:TextBox>
                                </div>
                            </div>
                            <div class="col-xs-12 col-md-4 grupo-field">
                                <div class="form-group">
                                    <span>Hora Exec.</span>
                                    <asp:TextBox ID="HoraFimExecOt" CssClass="text-field" runat="server" ReadOnly="True"></asp:TextBox>
                                </div>
                            </div>
                        </fieldset>
                    </div>
                </div>
            </div>
            <div class="row">
                <div class="col-xs-12 col-md-6 col-md-offset-3">
                    <div class="toast-alert"></div>
                </div>
            </div>
        </div>
    </ContentTemplate>
    <Triggers>
        <asp:AsyncPostBackTrigger ControlID="Buscar" EventName="Click" />
    </Triggers>
</asp:UpdatePanel>




<script>
    function InitConfig() {
        $("#txtBusca").on("keyup", function () {
            var value = $(this).val().toLowerCase();
            $("#<%= GrdOrdens.ClientID%> tbody tr").filter(function () {
                $(this).toggle($(this).text().toLowerCase().indexOf(value) > -1);
            });
        });
    }

    Sys.WebForms.PageRequestManager.getInstance().add_endRequest(InitConfig);

    $(function () {
        InitConfig();
    });

</script>

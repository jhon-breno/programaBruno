<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="ConsultarClientesFaturaPorEmail.ascx.cs" Inherits="BotaoEmpresa.controls.ConsultarClientesFaturaPorEmail" %>
<%@ Register TagPrefix="uc1" TagName="loadspin" Src="~/Content/Spinner/load-spin.ascx" %>


<style>
    .table > tbody > tr > th {
        white-space: nowrap;
    }

    .table {
        text-align: center;
    }
</style>
<asp:UpdateProgress ID="UpdateMessagesProgress" DynamicLayout="false" runat="server" DisplayAfter="0">
    <ProgressTemplate>
        <uc1:loadspin runat="server" ID="loadspin" />
    </ProgressTemplate>
</asp:UpdateProgress>
<div class="container">



    <div class="col-xs-12 ">
        <div class="row padding-top-20">
            <asp:UpdatePanel ID="UpPanelFormGenerico" runat="server" UpdateMode="Conditional">
                <ContentTemplate>
                    <asp:Panel runat="server" DefaultButton="Consultar">

                        <div class="row" style="position: absolute; left: 860px;">
                            <div class="col-xs-12" style="padding-top: 15px">
                                <asp:FileUpload ID="FileUpload1" runat="server" Width="400px" />
                            </div>
                            <div class="col-xs-12" style="padding-top: 15px">
                                <asp:Button ID="btnEnviarArquivo" runat="server" Text="Enviar Arquivo" OnClick="btnEnviarArquivo_OnClickuivo_Click" OnClientClick="MostrarProgresso();" />
                            </div>
                            <div class="col-xs-12" style="padding-top: 15px">
                                <asp:Label ID="lblmsg" runat="server" Text=""></asp:Label>
                            </div>
                        </div>

                        <div class="col-xs-12 col-md-6 col-md-offset-3">
                            <h2 class="text-center">CONSULTAR REFERENCIAS FATURA POR E-MAIL</h2>
                            <div class="row">
                                <div class="col-xs-12 grupo-field">
                                    <div class="form-group">
                                        <span>Número Cliente</span>
                                        <asp:TextBox ID="UCDescadFat" CssClass="text-field" runat="server" MaxLength="20"></asp:TextBox>
                                    </div>
                                    <asp:RequiredFieldValidator runat="server" ID="ValidationEmailRequired" ValidationGroup="FatEmai" CssClass="text-error-field" SetFocusOnError="True" Display="Dynamic" ControlToValidate="UCDescadFat" ErrorMessage="Campo de preenchimento obrigatório"></asp:RequiredFieldValidator>
                                </div>
                            </div>

                            <%--<div class="table-responsive">
                                <asp:GridView ID="GrdMultas" runat="server" AutoGenerateColumns="true" CssClass="table table-striped fit" EmptyDataText="NENHUM RESULTADO ENCONTRADO.">
                                </asp:GridView>
                            </div>--%>
                            <%--<h3 style="color: green">
                                <asp:Label runat="server" ID="ResultadoConsultaOk"></asp:Label>
                            </h3>
                            <h3 style="color: red">
                                <asp:Label runat="server" ID="ResultadoConsultaKo"></asp:Label>
                            </h3>--%>
                        </div>
                        <div class="row row-btn">
                            <div class="col-xs-12 col-md-6 col-md-offset-3">
                                <%--<asp:Button runat="server" ID="Consultar" class="btn-base btn-send" OnClientClick="return LmparFormAutoLeituraFat()" UseSubmitBehavior="True" Text="Consultar" OnClick="Consultar_OnClick" ValidationGroup="Descad" />--%>
                                <asp:Button runat="server" ID="Consultar" class="btn-base btn-send" UseSubmitBehavior="True" Text="Consultar" OnClick="Consultar_OnClick" ValidationGroup="FatEmai" />
                            </div>
                        </div>
                        <div class="col-xs-12">
                            <div class="table-responsive">
                                <asp:GridView ID="grdDados" runat="server" AutoGenerateColumns="true" CssClass="table table-striped fit" EmptyDataText="NENHUM RESULTADO ENCONTRADO.">

                                    <%--<Columns>
                                        <asp:BoundField DataField="E-mail" HeaderText="E-mail" />
                                        <asp:BoundField DataField="Conta-Contrato" HeaderText="Conta-Contrato" />
                                        <asp:BoundField DataField="" HeaderText="" />
                                        <asp:BoundField DataField="Nome do Cliente" HeaderText="Nome do Cliente" />
                                        <asp:BoundField DataField="Abertura" HeaderText="Abertura" />
                                        <asp:BoundField DataField="Data de Abertura" HeaderText="Data de Abertura" />
                                        <asp:BoundField DataField="Clique" HeaderText="Clique" />
                                        <asp:BoundField DataField="Data do Clique" HeaderText="Data do Clique" />
                                        <asp:BoundField DataField="Data de Envio" HeaderText="Data de Envio" />
                                        <asp:BoundField DataField="BounceMask" HeaderText="BounceMask" />
                                        <asp:BoundField DataField="BounceSubMask" HeaderText="BounceSubMask" />
                                        <asp:BoundField DataField="Data de Vencimento" HeaderText="Data de Vencimento" />
                                        <asp:BoundField DataField="Valor da Fatura" HeaderText="Valor da Fatura" />
                                        <asp:BoundField DataField="InvoiceReference__c" HeaderText="InvoiceReference__c" />

                                    </Columns>--%>
                                </asp:GridView>
                            </div>
                        </div>
                    </asp:Panel>
                </ContentTemplate>
                <Triggers>
                    <asp:AsyncPostBackTrigger ControlID="Consultar" EventName="Click" />
                    <asp:PostBackTrigger ControlID="btnEnviarArquivo" />
                </Triggers>
            </asp:UpdatePanel>

        </div>
    </div>




    <asp:UpdatePanel ID="UpdPanelToast" runat="server">
        <ContentTemplate>
            <div class="row">
                <div class="col-xs-12 col-md-6 col-md-offset-3">
                    <div class="toast-alert"></div>
                </div>
            </div>
        </ContentTemplate>
        <Triggers>
            <asp:AsyncPostBackTrigger ControlID="Consultar" EventName="Click" />
        </Triggers>
    </asp:UpdatePanel>
</div>

<style>
    .input-group-addon {
        border-top: 0;
        border-bottom: 0;
    }
</style>
<%--<script type="text/javascript">
    function LmparFormAutoLeituraFat() {
        if (!Page_ClientValidate("FatMail")) {
            $("#<%= ResultadoConsultaOk.ClientID%>").text('');
            $("#<%= ResultadoConsultaKo.ClientID%>").text('');
        }
        return true;
    }
</script>--%>

<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="DescadastramentoEnel.ascx.cs" Inherits="BotaoEmpresa.controls.DescadastramentoEnel" %>
<%@ Register TagPrefix="uc1" TagName="loadspin" Src="~/Content/Spinner/load-spin.ascx" %>



<asp:UpdateProgress ID="UpdateMessagesProgress" DynamicLayout="false" runat="server" DisplayAfter="0">
    <ProgressTemplate>
        <uc1:loadspin runat="server" ID="loadspin" />
    </ProgressTemplate>
</asp:UpdateProgress>
<div class="container">

    <div class="row padding-top-20">
        <div class="col-xs-12 col-md-6 col-md-offset-3">
            <asp:UpdatePanel ID="UpPanelFormGenerico" runat="server" UpdateMode="Conditional">
                <ContentTemplate>
                    <h2 class="text-center">CONSULTAR DESCADASTRAMENTO MASSIVO</h2>
                    <asp:Panel runat="server" DefaultButton="Consultar">
                        <div class="row">
                            <div class="col-xs-12 grupo-field">
                                <div class="form-group">
                                    <span>Número Cliente</span>
                                    <asp:TextBox ID="UCDescad" CssClass="text-field" runat="server" MaxLength="20"></asp:TextBox>
                                </div>
                                <asp:RequiredFieldValidator runat="server" ID="ValidationEmailRequired" ValidationGroup="Descad" CssClass="text-error-field" SetFocusOnError="True" Display="Dynamic" ControlToValidate="UCDescad" ErrorMessage="Campo de preenchimento obrigatório"></asp:RequiredFieldValidator>
                            </div>
                        </div>

                        <div class="table-responsive">
                            <asp:GridView ID="GrdMultas" runat="server" AutoGenerateColumns="true" CssClass="table table-striped fit" EmptyDataText="Nenhuma ordem encontrada para o cliente informado.">
                            </asp:GridView>
                        </div>
                        <h3 style="color: green">
                            <asp:Label runat="server" ID="ResultadoConsultaOk"></asp:Label>
                        </h3>
                        <h3 style="color: red">
                            <asp:Label runat="server" ID="ResultadoConsultaKo"></asp:Label>
                        </h3>
                    </asp:Panel>
                </ContentTemplate>
                <Triggers>
                    <asp:AsyncPostBackTrigger ControlID="Consultar" EventName="Click" />
                </Triggers>
            </asp:UpdatePanel>
            <div class="row row-btn">
                <div class="col-xs-12">
                    <asp:Button runat="server" ID="Consultar" class="btn-base btn-send" OnClientClick="return LmparFormAutoLeitura()" UseSubmitBehavior="True" Text="Consultar" OnClick="Consultar_OnClick" ValidationGroup="Descad" />
                </div>
            </div>
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
<script type="text/javascript">
    function LmparFormAutoLeitura() {
        if (!Page_ClientValidate("Descad")) {
            $("#<%= ResultadoConsultaOk.ClientID%>").text('');
            $("#<%= ResultadoConsultaKo.ClientID%>").text('');
        }
        return true;
    }
</script>

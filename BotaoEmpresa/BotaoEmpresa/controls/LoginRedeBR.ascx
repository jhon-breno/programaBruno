<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="LoginRedeBR.ascx.cs" Inherits="BotaoEmpresa.Controls.LoginRedeBR" %>
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
                    <h2 class="text-center">INFORME SEU BR PARA ACESSAR</h2>
                    <asp:Panel runat="server" DefaultButton="Acessar">
                        <div class="row">
                            <div class="col-xs-12 grupo-field">
                                <div class="form-group">
                                    <span>BR</span>
                                    <asp:TextBox ID="BR" CssClass="text-field" runat="server" MaxLength="20"></asp:TextBox>
                                </div>
                                <asp:RequiredFieldValidator runat="server" ID="ValidationEmailRequired" CssClass="text-error-field" SetFocusOnError="True" Display="Dynamic" ControlToValidate="BR" ErrorMessage="Campo de preenchimento obrigatório"></asp:RequiredFieldValidator>
                            </div>
                        </div>

                        <div class="row">
                            <div class="col-xs-12 grupo-field">
                                <div class="form-group">
                                    <span>Senha</span>
                                    <asp:TextBox ID="Senha" TextMode="Password" CssClass="text-field" runat="server"></asp:TextBox>
                                </div>
                                <asp:RequiredFieldValidator runat="server" CssClass="text-error-field" SetFocusOnError="True" Display="Dynamic" ControlToValidate="Senha" ErrorMessage="Campo de preenchimento obrigatório"></asp:RequiredFieldValidator>
                            </div>
                        </div>

                    </asp:Panel>
                </ContentTemplate>
                <Triggers>
                    <asp:AsyncPostBackTrigger ControlID="Acessar" EventName="Click" />
                </Triggers>
            </asp:UpdatePanel>
            <div class="row row-btn">
                <div class="col-xs-12">
                    <asp:Button runat="server" ID="Acessar" class="btn-base btn-send" UseSubmitBehavior="True" Text="Acessar" OnClick="Acessar_OnClick" />
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
            <asp:AsyncPostBackTrigger ControlID="Acessar" EventName="Click" />
        </Triggers>
    </asp:UpdatePanel>
</div>

<style>
    .input-group-addon {
        border-top: 0;
        border-bottom: 0;
    }
</style>

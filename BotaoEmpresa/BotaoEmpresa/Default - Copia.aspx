<%@ Page Title="Consultar Histórico de Ordem" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Default.aspx.cs" Inherits="ConsultarOrdem._Default" %>
<%@ Register Src="~/Content/Spinner/load-spin.ascx" TagPrefix="uc1" TagName="loadspin" %>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">
    <asp:UpdateProgress ID="UpdateProgress" DynamicLayout="false" runat="server" DisplayAfter="0">
        <ProgressTemplate>
            <uc1:loadspin runat="server" ID="loadspin" />
        </ProgressTemplate>
    </asp:UpdateProgress>
    <asp:UpdatePanel ID="UpdatePanel1" runat="server" UpdateMode="Conditional">
        <ContentTemplate>
            <asp:Panel ID="pnlHistOrdem" runat="server"></asp:Panel>
        </ContentTemplate>
    </asp:UpdatePanel>
</asp:Content>

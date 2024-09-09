<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Telemediacao.aspx.cs" Inherits="BotaoEmpresa.rj.Telemediacao" %>
<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title></title>
    
    <style>
        .mydatagrid
        {
        width: 80%;
        border: solid 2px black;
        min-width: 80%;
        }
        .header
        {
        background-color: #646464;
        font-family: Arial;
        color: White;
        border: none 0px transparent;
        height: 25px;
        text-align: left;
        font-size: 16px;
        }

        .rows
        {
        background-color: #fff;
        font-family: Arial;
        font-size: 14px;
        color: #000;
        min-height: 25px;
        text-align: left;
        border: none 0px transparent;
        }
        .rows:hover
        {
        background-color: #ff8000;
        font-family: Arial;
        color: #fff;
        text-align: left;
        }
        .selectedrow
        {
        background-color: #ff8000;
        font-family: Arial;
        color: #fff;
        font-weight: bold;
        text-align: left;
        }
        .mydatagrid a /** FOR THE PAGING ICONS **/
        {
        background-color: Transparent;
        padding: 5px 5px 5px 5px;
        color: #fff;
        text-decoration: none;
        font-weight: bold;
        }

        .mydatagrid a:hover /** FOR THE PAGING ICONS HOVER STYLES**/
        {
        background-color: #000;
        color: #fff;
        }
        .mydatagrid span /** FOR THE PAGING ICONS CURRENT PAGE INDICATOR **/
        {
        background-color: #c9c9c9;
        color: #000;
        padding: 5px 5px 5px 5px;
        }
        .pager
        {
        background-color: #646464;
        font-family: Arial;
        color: White;
        height: 30px;
        text-align: left;
        }

        .mydatagrid td
        {
        padding: 5px;
        }
        .mydatagrid th
        {
        padding: 5px;
        }
    </style>
</head>
<body>
    <form id="form1" runat="server">
    <div>
        <asp:GridView ID="GridTelemedicaoCe" runat="server" AutoGenerateColumns="False" CssClass="mydatagrid" HeaderStyle-CssClass="header" RowStyle-CssClass="rows">
            <Columns>
                <asp:BoundField DataField="ClaveLectura" HeaderText="Tipo" />
                <asp:BoundField DataField="CodigoCp" HeaderText="CP" />
                <asp:BoundField DataField="CodigoCs" HeaderText="CS" />
                <asp:BoundField DataField="CodigoPs" HeaderText="PS" />
                <asp:BoundField DataField="Sector" HeaderText="Lote" />
                <asp:BoundField DataField="FechaLectura" HeaderText="Data Leitura" />
                <asp:BoundField DataField="HoraLectura" DataFormatString="{0:HH:mm:ss}" HeaderText="Hora Leitura" />
                <asp:BoundField DataField="LecturaTerreno" HeaderText="Leitura" />
                <asp:BoundField DataField="ConsumoActivo" HeaderText="Consumo" />
                <asp:BoundField DataField="EstadoSuministro" HeaderText="Estado Cliente" />
                <asp:BoundField DataField="TipoConsumo" HeaderText="Tipo de Consumo" />
            </Columns>
            <HeaderStyle Font-Size="Smaller" />
                <PagerStyle Font-Size="Smaller" />
                <RowStyle Font-Size="Smaller" />
        </asp:GridView>      
    </div>
    <div id="superDIV" class="someCssClass" runat="server"></div>
    </form>
</body>
</html>

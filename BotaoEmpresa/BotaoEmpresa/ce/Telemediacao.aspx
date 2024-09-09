<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Telemediacao.aspx.cs" Inherits="BotaoEmpresa.ce.Telemediacao" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title></title>
    
</head>
<body>
    <form id="form1" runat="server">
    <div>
    <asp:GridView ID="GridTelemedicaoCe" runat="server" AutoGenerateColumns="false" AllowPaging="false" CssClass="gridview">
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

            </Columns>
            <HeaderStyle Font-Size="Smaller" />
                <PagerStyle Font-Size="Smaller" />
                <RowStyle Font-Size="Smaller" BackColor="#DFDFDF" />
        </asp:GridView>
        
    </div>
    </form>
</body>
</html>

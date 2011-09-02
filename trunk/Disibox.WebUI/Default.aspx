<%@ Page Title="Home Page" Language="C#" MasterPageFile="~/Site.master" AutoEventWireup="true"
    CodeBehind="Default.aspx.cs" Inherits="Disibox.WebUI._Default" %>

<%@ Register assembly="Disibox.WebUI" namespace="Disibox.WebUI.Controls" tagprefix="cc1" %>

<asp:Content ID="HeaderContent" runat="server" ContentPlaceHolderID="HeadContent">
</asp:Content>
<asp:Content ID="BodyContent" runat="server" ContentPlaceHolderID="MainContent">
    <h2>
        Files for ADMIN USER
    </h2>
    <p>
        <cc1:FilesTable ID="FilesTable" runat="server">
        </cc1:FilesTable>
    </p>
    <p>
        <asp:FileUpload ID="FileUpload" runat="server" />
        <asp:Button ID="UploadButton" runat="server" onclick="UploadButton_Click" 
            Text="Upload" />
    </p>
    <p>
        You can also find <a href="http://go.microsoft.com/fwlink/?LinkID=152368&amp;clcid=0x409"
            title="MSDN ASP.NET Docs">documentation on ASP.NET at MSDN</a>.
    </p>
</asp:Content>

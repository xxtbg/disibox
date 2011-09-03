<%@ Page Title="Home Page" Language="C#" MasterPageFile="~/SiteMemberOnly.master" AutoEventWireup="true"
    CodeBehind="MemberOnly.aspx.cs" Inherits="Disibox.WebUI.MemberOnly" %>

<%@ Register assembly="Disibox.WebUI" namespace="Disibox.WebUI.Controls" tagprefix="cc1" %>

<asp:Content ID="HeaderContent" runat="server" ContentPlaceHolderID="HeadContent">
</asp:Content>
<asp:Content ID="BodyContent" runat="server" ContentPlaceHolderID="MainContent">
    <h2>
        Files for
        <asp:Literal ID="Literal_Logged" runat="server"></asp:Literal>
    </h2>
    <p>
        <asp:FileUpload ID="FileUpload" runat="server" />
        <asp:Button ID="UploadButton" runat="server" onclick="UploadButton_Click" 
            Text="Upload" />
        <asp:Button ID="DeleteFilesButton" runat="server" onclick="DeleteFilesButton_Click" 
            Text="Delete" />
        <br/>
        <asp:Literal ID="UploadMessage" runat="server"></asp:Literal>
    </p>
    <p>
        <cc1:FilesTable ID="FilesTable" runat="server" BorderStyle="Solid" 
            GridLines="Both" Width="350px">
        </cc1:FilesTable>
    </p>
    <h2>
        Administrators</h2>
    <p>
        <asp:Button ID="DeleteAdminUsersButton" runat="server" onclick="DeleteAdminUsersButton_Click" 
            Text="Delete" />
        <br/>
        <asp:Literal ID="DeleteAdminMessage" runat="server"></asp:Literal>
    </p>
    <p>
        <cc1:UsersTable ID="AdminUsersTable" runat="server" BorderStyle="Solid" 
            GridLines="Both" Width="350px">
        </cc1:UsersTable>
    </p>
    <h2>
        Users</h2>
    <p>
        <asp:Button ID="DeleteCommonUsersButton" runat="server" onclick="DeleteCommonUsersButton_Click" 
            Text="Delete" />
        <br/>
        <asp:Literal ID="DeleteUserCommonMessage" runat="server"></asp:Literal>
    </p>
    <p>
        <cc1:UsersTable ID="CommonUsersTable" runat="server" BorderStyle="Solid" 
            GridLines="Both" Width="350px">
        </cc1:UsersTable>
    </p>
    <p>
        You can also find <a href="http://go.microsoft.com/fwlink/?LinkID=152368&amp;clcid=0x409"
            title="MSDN ASP.NET Docs">documentation on ASP.NET at MSDN</a>.
    </p>
</asp:Content>

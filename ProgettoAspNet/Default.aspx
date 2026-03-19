<%@ Page Async="true" Title="Home Page" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Default.aspx.cs" Inherits="ProgettoAspNet._Default" %>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">
    <main>
        <h4>Send a message</h4>
        <div class="input-group mb-3">
            <asp:TextBox ID="Message" data-testid="message-text" placeholder="Type something..." CssClass="form-control" runat="server" />
            <div class="input-group-append">
                <asp:Button data-testid="send-message" OnClick="Send_Click" Text="Send" runat="server" CssClass="btn btn-primary" />
            </div>
        </div>
    </main>
</asp:Content>

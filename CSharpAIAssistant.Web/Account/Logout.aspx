<%@ Page Title="Sign Out" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Logout.aspx.cs" Inherits="CSharpAIAssistant.Web.Account.Logout" %>

<asp:Content ID="HeadContent" ContentPlaceHolderID="HeadContent" runat="server">
</asp:Content>

<asp:Content ID="MainContent" ContentPlaceHolderID="MainContent" runat="server">
    <div class="container mt-4">
        <div class="row justify-content-center">
            <div class="col-md-6">
                <div class="card">
                    <div class="card-body text-center">
                        <h4>Signing out...</h4>
                        <p>Please wait while we sign you out.</p>
                        <div class="spinner-border" role="status">
                            <span class="sr-only">Loading...</span>
                        </div>
                    </div>
                </div>
            </div>
        </div>
    </div>
    
    <script type="text/javascript">
        // Auto-redirect after a short delay to show the logout message
        setTimeout(function() {
            window.location.href = '<%= ResolveUrl("~/Account/Login.aspx?message=loggedout") %>';
        }, 2000);
    </script>
</asp:Content>

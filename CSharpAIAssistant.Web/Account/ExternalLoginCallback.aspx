<%@ Page Title="External Login" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="ExternalLoginCallback.aspx.cs" Inherits="CSharpAIAssistant.Web.Account.ExternalLoginCallback" %>

<asp:Content ID="HeadContent" ContentPlaceHolderID="HeadContent" runat="server">
</asp:Content>

<asp:Content ID="MainContent" ContentPlaceHolderID="MainContent" runat="server">
    <div class="container mt-4">
        <div class="row justify-content-center">
            <div class="col-md-6">
                <div class="card">
                    <div class="card-body text-center">
                        <h4>Completing sign-in...</h4>
                        <p>Please wait while we process your external login.</p>
                        <div class="spinner-border" role="status">
                            <span class="sr-only">Loading...</span>
                        </div>
                    </div>
                </div>
            </div>
        </div>
    </div>
</asp:Content>

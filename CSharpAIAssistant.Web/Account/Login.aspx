<%@ Page Title="Sign In" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Login.aspx.cs" Inherits="CSharpAIAssistant.Web.Account.Login" %>

<asp:Content ID="HeadContent" ContentPlaceHolderID="HeadContent" runat="server">
</asp:Content>

<asp:Content ID="MainContent" ContentPlaceHolderID="MainContent" runat="server">
    <div class="container mt-4">
        <div class="row justify-content-center">
            <div class="col-md-6">
                <div class="card">
                    <div class="card-header">
                        <h3 class="card-title mb-0">Sign In</h3>
                    </div>
                    <div class="card-body">
                        <asp:Panel ID="pnlError" runat="server" CssClass="alert alert-danger" Visible="false">
                            <asp:Literal ID="litError" runat="server"></asp:Literal>
                        </asp:Panel>
                        
                        <asp:Panel ID="pnlInfo" runat="server" CssClass="alert alert-info" Visible="false">
                            <asp:Literal ID="litInfo" runat="server"></asp:Literal>
                        </asp:Panel>
                        
                        <!-- Forms Authentication Section -->
                        <div class="form-group">
                            <label for="<%= txtUsername.ClientID %>">Username</label>
                            <asp:TextBox ID="txtUsername" runat="server" CssClass="form-control" 
                                placeholder="Enter username" MaxLength="50"></asp:TextBox>
                            <asp:RequiredFieldValidator ID="rfvUsername" runat="server" 
                                ControlToValidate="txtUsername" ErrorMessage="Username is required" 
                                CssClass="text-danger small" Display="Dynamic" ValidationGroup="LoginGroup"></asp:RequiredFieldValidator>
                        </div>
                        
                        <div class="form-group">
                            <label for="<%= txtPassword.ClientID %>">Password</label>
                            <asp:TextBox ID="txtPassword" runat="server" CssClass="form-control" 
                                TextMode="Password" placeholder="Enter password" MaxLength="100"></asp:TextBox>
                            <asp:RequiredFieldValidator ID="rfvPassword" runat="server" 
                                ControlToValidate="txtPassword" ErrorMessage="Password is required" 
                                CssClass="text-danger small" Display="Dynamic" ValidationGroup="LoginGroup"></asp:RequiredFieldValidator>
                        </div>
                        
                        <div class="form-group form-check">
                            <asp:CheckBox ID="chkRememberMe" runat="server" CssClass="form-check-input" />
                            <label class="form-check-label" for="<%= chkRememberMe.ClientID %>">
                                Remember me
                            </label>
                        </div>
                        
                        <div class="form-group">
                            <asp:Button ID="btnLogin" runat="server" Text="Sign In" 
                                CssClass="btn btn-primary btn-block" OnClick="btnLogin_Click" 
                                ValidationGroup="LoginGroup" />
                        </div>
                        
                        <hr />
                        
                        <!-- Google OAuth Section -->
                        <div class="form-group">
                            <asp:Button ID="btnLoginWithGoogle" runat="server" Text="Sign in with Google" 
                                CssClass="btn btn-danger btn-block" OnClick="btnLoginWithGoogle_Click" 
                                CausesValidation="false" />
                        </div>
                        
                        <asp:Panel ID="pnlGoogleDisabled" runat="server" CssClass="alert alert-warning" Visible="false">
                            <small>Google sign-in is not configured. Please contact the administrator.</small>
                        </asp:Panel>
                        
                        <hr />
                        
                        <div class="text-center">
                            <p>Don't have an account? <a href="Register.aspx">Create one here</a></p>
                        </div>
                    </div>
                </div>
            </div>
        </div>
    </div>
</asp:Content>

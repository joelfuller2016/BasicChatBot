<%@ Page Title="Register" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Register.aspx.cs" Inherits="CSharpAIAssistant.Web.Account.Register" %>

<asp:Content ID="HeadContent" ContentPlaceHolderID="HeadContent" runat="server">
</asp:Content>

<asp:Content ID="MainContent" ContentPlaceHolderID="MainContent" runat="server">
    <div class="container mt-4">
        <div class="row justify-content-center">
            <div class="col-md-6">
                <div class="card">
                    <div class="card-header">
                        <h3 class="card-title mb-0">Create New Account</h3>
                    </div>
                    <div class="card-body">
                        <asp:Panel ID="pnlError" runat="server" CssClass="alert alert-danger" Visible="false">
                            <asp:Literal ID="litError" runat="server"></asp:Literal>
                        </asp:Panel>
                        
                        <asp:Panel ID="pnlSuccess" runat="server" CssClass="alert alert-success" Visible="false">
                            <asp:Literal ID="litSuccess" runat="server"></asp:Literal>
                        </asp:Panel>
                        
                        <div class="form-group">
                            <label for="<%= txtUsername.ClientID %>">Username *</label>
                            <asp:TextBox ID="txtUsername" runat="server" CssClass="form-control" 
                                placeholder="Enter username" MaxLength="50" required="required"></asp:TextBox>
                            <asp:RequiredFieldValidator ID="rfvUsername" runat="server" 
                                ControlToValidate="txtUsername" ErrorMessage="Username is required" 
                                CssClass="text-danger small" Display="Dynamic"></asp:RequiredFieldValidator>
                        </div>
                        
                        <div class="form-group">
                            <label for="<%= txtEmail.ClientID %>">Email Address *</label>
                            <asp:TextBox ID="txtEmail" runat="server" CssClass="form-control" 
                                TextMode="Email" placeholder="Enter email address" MaxLength="255" required="required"></asp:TextBox>
                            <asp:RequiredFieldValidator ID="rfvEmail" runat="server" 
                                ControlToValidate="txtEmail" ErrorMessage="Email is required" 
                                CssClass="text-danger small" Display="Dynamic"></asp:RequiredFieldValidator>
                            <asp:RegularExpressionValidator ID="revEmail" runat="server" 
                                ControlToValidate="txtEmail" ErrorMessage="Please enter a valid email address" 
                                ValidationExpression="\w+([-+.']\w+)*@\w+([-.]\w+)*\.\w+([-.]\w+)*" 
                                CssClass="text-danger small" Display="Dynamic"></asp:RegularExpressionValidator>
                        </div>
                        
                        <div class="form-group">
                            <label for="<%= txtPassword.ClientID %>">Password *</label>
                            <asp:TextBox ID="txtPassword" runat="server" CssClass="form-control" 
                                TextMode="Password" placeholder="Enter password" MaxLength="100" required="required"></asp:TextBox>
                            <asp:RequiredFieldValidator ID="rfvPassword" runat="server" 
                                ControlToValidate="txtPassword" ErrorMessage="Password is required" 
                                CssClass="text-danger small" Display="Dynamic"></asp:RequiredFieldValidator>
                            <small class="form-text text-muted">Password should be at least 8 characters long</small>
                        </div>
                        
                        <div class="form-group">
                            <label for="<%= txtConfirmPassword.ClientID %>">Confirm Password *</label>
                            <asp:TextBox ID="txtConfirmPassword" runat="server" CssClass="form-control" 
                                TextMode="Password" placeholder="Confirm password" MaxLength="100" required="required"></asp:TextBox>
                            <asp:RequiredFieldValidator ID="rfvConfirmPassword" runat="server" 
                                ControlToValidate="txtConfirmPassword" ErrorMessage="Please confirm your password" 
                                CssClass="text-danger small" Display="Dynamic"></asp:RequiredFieldValidator>
                            <asp:CompareValidator ID="cvPasswords" runat="server" 
                                ControlToValidate="txtConfirmPassword" ControlToCompare="txtPassword" 
                                ErrorMessage="Passwords do not match" 
                                CssClass="text-danger small" Display="Dynamic"></asp:CompareValidator>
                        </div>
                        
                        <div class="form-group">
                            <asp:Button ID="btnRegister" runat="server" Text="Create Account" 
                                CssClass="btn btn-primary btn-block" OnClick="btnRegister_Click" />
                        </div>
                        
                        <hr />
                        
                        <div class="text-center">
                            <p>Already have an account? <a href="Login.aspx">Sign in here</a></p>
                        </div>
                    </div>
                </div>
            </div>
        </div>
    </div>
</asp:Content>

<%@ Page Title="User Management" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="UserManagement.aspx.cs" Inherits="CSharpAIAssistant.Web.Admin.UserManagement" %>

<asp:Content ID="HeadContent" ContentPlaceHolderID="HeadContent" runat="server">
</asp:Content>

<asp:Content ID="MainContent" ContentPlaceHolderID="MainContent" runat="server">
    <div class="container-fluid mt-4">
        <div class="row">
            <div class="col-12">
                <h2>User Management</h2>
                <p class="text-muted">Manage user accounts and administrative privileges.</p>
                
                <asp:Panel ID="pnlError" runat="server" CssClass="alert alert-danger" Visible="false">
                    <asp:Literal ID="litError" runat="server"></asp:Literal>
                </asp:Panel>
                
                <asp:Panel ID="pnlSuccess" runat="server" CssClass="alert alert-success" Visible="false">
                    <asp:Literal ID="litSuccess" runat="server"></asp:Literal>
                </asp:Panel>

                <div class="card">
                    <div class="card-header">
                        <div class="row align-items-center">
                            <div class="col">
                                <h5 class="card-title mb-0">Registered Users</h5>
                            </div>
                            <div class="col-auto">
                                <asp:Button ID="btnRefresh" runat="server" Text="Refresh" 
                                    CssClass="btn btn-outline-secondary btn-sm" OnClick="btnRefresh_Click" />
                            </div>
                        </div>
                    </div>
                    <div class="card-body">
                        <asp:GridView ID="gvUsers" runat="server" CssClass="table table-striped table-hover"
                            AutoGenerateColumns="false" DataKeyNames="Id" 
                            OnRowDataBound="gvUsers_RowDataBound"
                            GridLines="None" BorderStyle="None">
                            
                            <Columns>
                                <asp:BoundField DataField="Username" HeaderText="Username" 
                                    ItemStyle-CssClass="font-weight-bold" />
                                
                                <asp:BoundField DataField="Email" HeaderText="Email Address" 
                                    ItemStyle-CssClass="text-muted" />
                                
                                <asp:TemplateField HeaderText="Registration Date">
                                    <ItemTemplate>
                                        <%# ((DateTime)Eval("RegistrationDate")).ToString("MMM dd, yyyy") %>
                                    </ItemTemplate>
                                </asp:TemplateField>
                                
                                <asp:TemplateField HeaderText="Last Login">
                                    <ItemTemplate>
                                        <%# Eval("LastLoginDate") != null ? 
                                            ((DateTime)Eval("LastLoginDate")).ToString("MMM dd, yyyy") : 
                                            "<span class='text-muted'>Never</span>" %>
                                    </ItemTemplate>
                                </asp:TemplateField>
                                
                                <asp:TemplateField HeaderText="Authentication">
                                    <ItemTemplate>
                                        <%# !string.IsNullOrEmpty(Eval("GoogleId")?.ToString()) ? 
                                            "<span class='badge badge-info'>Google</span>" : "" %>
                                        <%# !string.IsNullOrEmpty(Eval("PasswordHash")?.ToString()) ? 
                                            "<span class='badge badge-secondary'>Password</span>" : "" %>
                                    </ItemTemplate>
                                </asp:TemplateField>
                                
                                <asp:TemplateField HeaderText="Administrator">
                                    <ItemTemplate>
                                        <asp:CheckBox ID="chkIsAdmin" runat="server" 
                                            Checked='<%# Convert.ToBoolean(Eval("IsAdmin")) %>'
                                            AutoPostBack="true" OnCheckedChanged="chkIsAdmin_CheckedChanged"
                                            CssClass="form-check-input" />
                                    </ItemTemplate>
                                </asp:TemplateField>
                            </Columns>
                            
                            <EmptyDataTemplate>
                                <div class="text-center text-muted py-4">
                                    <p>No users found.</p>
                                </div>
                            </EmptyDataTemplate>
                        </asp:GridView>
                    </div>
                </div>
                
                <div class="mt-4">
                    <div class="card">
                        <div class="card-header">
                            <h6 class="card-title mb-0">User Management Guidelines</h6>
                        </div>
                        <div class="card-body">
                            <div class="row">
                                <div class="col-md-6">
                                    <h6>Administrator Privileges</h6>
                                    <ul class="mb-3">
                                        <li>Access to admin panel and all settings</li>
                                        <li>Ability to manage other users</li>
                                        <li>View and manage AI model configurations</li>
                                        <li>Access to sensitive application settings</li>
                                    </ul>
                                </div>
                                <div class="col-md-6">
                                    <h6>Safety Notes</h6>
                                    <ul class="mb-3">
                                        <li><strong>Self-Protection:</strong> You cannot remove your own admin privileges</li>
                                        <li><strong>Last Admin:</strong> System prevents removal of the last administrator</li>
                                        <li><strong>Google Users:</strong> Users with Google authentication may not have passwords</li>
                                        <li><strong>Audit Trail:</strong> All changes are logged for security</li>
                                    </ul>
                                </div>
                            </div>
                        </div>
                    </div>
                </div>
            </div>
        </div>
    </div>
</asp:Content>

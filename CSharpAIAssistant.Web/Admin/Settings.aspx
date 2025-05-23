<%@ Page Title="Application Settings" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Settings.aspx.cs" Inherits="CSharpAIAssistant.Web.Admin.Settings" %>

<asp:Content ID="HeadContent" ContentPlaceHolderID="HeadContent" runat="server">
</asp:Content>

<asp:Content ID="MainContent" ContentPlaceHolderID="MainContent" runat="server">
    <div class="container-fluid mt-4">
        <div class="row">
            <div class="col-12">
                <h2>Application Settings Management</h2>
                <p class="text-muted">Configure all application settings including sensitive/encrypted values.</p>
                
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
                                <h5 class="card-title mb-0">Settings</h5>
                            </div>
                            <div class="col-auto">
                                <div class="form-check">
                                    <asp:CheckBox ID="chkShowSensitive" runat="server" CssClass="form-check-input" 
                                        AutoPostBack="true" OnCheckedChanged="chkShowSensitive_CheckedChanged" />
                                    <label class="form-check-label text-danger" for="<%= chkShowSensitive.ClientID %>">
                                        <strong>Show Decrypted Sensitive Values</strong>
                                    </label>
                                </div>
                            </div>
                        </div>
                    </div>
                    <div class="card-body">
                        <asp:Panel ID="pnlSensitiveWarning" runat="server" CssClass="alert alert-warning" Visible="false">
                            <i class="fas fa-exclamation-triangle"></i>
                            <strong>Security Warning:</strong> Sensitive values are now displayed in plaintext. 
                            Ensure no unauthorized personnel can view this screen.
                        </asp:Panel>

                        <asp:GridView ID="gvSettings" runat="server" CssClass="table table-striped table-hover"
                            AutoGenerateColumns="false" DataKeyNames="SettingKey" 
                            OnRowEditing="gvSettings_RowEditing" OnRowCancelingEdit="gvSettings_RowCancelingEdit"
                            OnRowUpdating="gvSettings_RowUpdating" OnRowDataBound="gvSettings_RowDataBound"
                            GridLines="None" BorderStyle="None">
                            
                            <Columns>
                                <asp:BoundField DataField="GroupName" HeaderText="Group" ReadOnly="true" 
                                    ItemStyle-CssClass="text-muted small" />
                                
                                <asp:BoundField DataField="SettingKey" HeaderText="Setting Key" ReadOnly="true" 
                                    ItemStyle-CssClass="font-weight-bold" />
                                
                                <asp:TemplateField HeaderText="Value">
                                    <ItemTemplate>
                                        <asp:Literal ID="litValue" runat="server"></asp:Literal>
                                    </ItemTemplate>
                                    <EditItemTemplate>
                                        <asp:TextBox ID="txtValue" runat="server" CssClass="form-control" 
                                            TextMode="MultiLine" Rows="3" MaxLength="2000"></asp:TextBox>
                                    </EditItemTemplate>
                                </asp:TemplateField>
                                
                                <asp:BoundField DataField="SettingDescription" HeaderText="Description" ReadOnly="true" 
                                    ItemStyle-CssClass="text-muted small" />
                                
                                <asp:TemplateField HeaderText="Type">
                                    <ItemTemplate>
                                        <span class="badge badge-secondary"><%# Eval("DataType") %></span>
                                        <%# Convert.ToBoolean(Eval("IsSensitive")) ? "<span class=\"badge badge-danger ml-1\">Sensitive</span>" : "" %>
                                    </ItemTemplate>
                                </asp:TemplateField>
                                
                                <asp:CommandField ShowEditButton="true" ControlStyle-CssClass="btn btn-sm btn-outline-primary"
                                    EditText="Edit" UpdateText="Save" CancelText="Cancel" />
                            </Columns>
                            
                            <EmptyDataTemplate>
                                <div class="text-center text-muted py-4">
                                    <p>No settings found.</p>
                                </div>
                            </EmptyDataTemplate>
                        </asp:GridView>
                    </div>
                </div>
                
                <div class="mt-4">
                    <h5>Quick Actions</h5>
                    <div class="btn-group" role="group">
                        <asp:Button ID="btnRefresh" runat="server" Text="Refresh" CssClass="btn btn-outline-secondary" 
                            OnClick="btnRefresh_Click" />
                        <asp:Button ID="btnTestEncryption" runat="server" Text="Test Encryption Service" 
                            CssClass="btn btn-outline-info" OnClick="btnTestEncryption_Click" />
                    </div>
                </div>
                
                <div class="mt-4">
                    <div class="card">
                        <div class="card-header">
                            <h6 class="card-title mb-0">Setting Guidelines</h6>
                        </div>
                        <div class="card-body">
                            <ul class="mb-0">
                                <li><strong>Sensitive Settings:</strong> Values marked as sensitive are automatically encrypted when saved.</li>
                                <li><strong>Encrypted Settings:</strong> Enter new values in plaintext - they will be encrypted automatically.</li>
                                <li><strong>Google OAuth:</strong> Update GoogleClientId and GoogleClientSecret_Encrypted to enable Google sign-in.</li>
                                <li><strong>OpenAI Integration:</strong> Configure OpenAI API keys for AI functionality.</li>
                                <li><strong>Session Management:</strong> Adjust SessionTimeoutMinutes to control user session length.</li>
                            </ul>
                        </div>
                    </div>
                </div>
            </div>
        </div>
    </div>
</asp:Content>

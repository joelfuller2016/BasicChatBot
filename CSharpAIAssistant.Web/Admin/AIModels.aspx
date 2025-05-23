<%@ Page Title="AI Model Management" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="AIModels.aspx.cs" Inherits="CSharpAIAssistant.Web.Admin.AIModels" %>

<asp:Content ID="Content1" ContentPlaceHolderID="HeadContent" runat="server">
    <style>
        .admin-container {
            max-width: 1200px;
            margin: 0 auto;
        }
        .models-grid {
            margin-top: 20px;
        }
        .models-grid .gridview {
            border: 1px solid #dee2e6;
            border-radius: 0.375rem;
            overflow: hidden;
        }
        .models-grid .gridview th {
            background-color: #f8f9fa;
            border-bottom: 2px solid #dee2e6;
            padding: 12px;
            font-weight: 600;
            color: #495057;
            text-align: center;
        }
        .models-grid .gridview td {
            padding: 12px;
            border-bottom: 1px solid #dee2e6;
            vertical-align: middle;
            text-align: center;
        }
        .models-grid .gridview tr:hover {
            background-color: #f8f9fa;
        }
        .model-status {
            display: inline-block;
            padding: 0.25rem 0.5rem;
            border-radius: 0.25rem;
            font-size: 0.875rem;
            font-weight: 500;
        }
        .status-active {
            background-color: #dcfce7;
            color: #166534;
        }
        .status-inactive {
            background-color: #fecaca;
            color: #dc2626;
        }
        .add-model-card {
            background-color: #f8f9fa;
            border: 2px dashed #dee2e6;
            border-radius: 0.5rem;
            padding: 2rem;
            text-align: center;
            margin-bottom: 2rem;
            transition: all 0.3s ease;
        }
        .add-model-card:hover {
            border-color: #007bff;
            background-color: #e7f3ff;
        }
        .add-model-card i {
            font-size: 3rem;
            color: #6c757d;
            margin-bottom: 1rem;
        }
        .form-control, .form-select {
            margin-bottom: 0.5rem;
        }
        .edit-controls {
            min-width: 200px;
        }
        .edit-controls input, .edit-controls select {
            width: 100%;
            margin-bottom: 5px;
        }
        .btn-group-sm .btn {
            margin: 2px;
        }
        .notes-preview {
            max-width: 200px;
            overflow: hidden;
            text-overflow: ellipsis;
            white-space: nowrap;
        }
        .api-key-info {
            background-color: #fff3cd;
            border: 1px solid #ffeaa7;
            border-radius: 0.375rem;
            padding: 1rem;
            margin-bottom: 1rem;
        }
        .validation-error {
            color: #dc3545;
            font-size: 0.875rem;
            margin-top: 0.25rem;
        }
    </style>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
    <div class="container admin-container">
        <div class="row">
            <div class="col-md-12">
                <!-- Header -->
                <div class="d-flex justify-content-between align-items-center mb-4">
                    <h2><i class="fas fa-brain"></i> AI Model Management</h2>
                    <a href="../Default.aspx" class="btn btn-secondary">
                        <i class="fas fa-arrow-left"></i> Back to Dashboard
                    </a>
                </div>

                <!-- Messages -->
                <asp:Panel ID="pnlMessage" runat="server" Visible="false" CssClass="alert" role="alert">
                    <asp:Literal ID="litMessage" runat="server"></asp:Literal>
                </asp:Panel>

                <!-- API Key Information -->
                <div class="api-key-info">
                    <h5><i class="fas fa-info-circle"></i> API Key Configuration</h5>
                    <p class="mb-2">
                        Before adding AI models, ensure you have configured the corresponding API keys in the 
                        <a href="Settings.aspx" class="fw-bold">Application Settings</a> page.
                    </p>
                    <small class="text-muted">
                        API keys should be stored as encrypted settings with names like "OpenAIApiKey_ModelName_Encrypted" 
                        and marked as sensitive data.
                    </small>
                </div>

                <!-- Add New Model Card -->
                <asp:Panel ID="pnlAddModel" runat="server" Visible="true">
                    <div class="add-model-card">
                        <i class="fas fa-plus-circle"></i>
                        <h4>Add New AI Model</h4>
                        <p class="text-muted">Configure a new AI model for users to interact with</p>
                        <asp:Button ID="btnShowAddForm" runat="server" Text="Add New Model" 
                            CssClass="btn btn-primary" OnClick="btnShowAddForm_Click" />
                    </div>
                </asp:Panel>

                <!-- Add/Edit Model Form -->
                <asp:Panel ID="pnlModelForm" runat="server" Visible="false" CssClass="card mb-4">
                    <div class="card-header">
                        <h5 class="mb-0">
                            <i class="fas fa-cog"></i> 
                            <asp:Literal ID="litFormTitle" runat="server">Add New AI Model</asp:Literal>
                        </h5>
                    </div>
                    <div class="card-body">
                        <div class="row">
                            <div class="col-md-6">
                                <div class="form-group mb-3">
                                    <label for="<%= txtDisplayName.ClientID %>" class="form-label">
                                        Display Name <span class="text-danger">*</span>
                                    </label>
                                    <asp:TextBox ID="txtDisplayName" runat="server" CssClass="form-control" 
                                        placeholder="e.g., GPT-4 Turbo"></asp:TextBox>
                                    <div class="help-text">Friendly name shown to users</div>
                                </div>

                                <div class="form-group mb-3">
                                    <label for="<%= txtModelIdentifier.ClientID %>" class="form-label">
                                        Model Identifier <span class="text-danger">*</span>
                                    </label>
                                    <asp:TextBox ID="txtModelIdentifier" runat="server" CssClass="form-control" 
                                        placeholder="e.g., gpt-4-turbo-preview"></asp:TextBox>
                                    <div class="help-text">Exact model name from OpenAI API</div>
                                </div>

                                <div class="form-group mb-3">
                                    <label for="<%= ddlApiKeySetting.ClientID %>" class="form-label">
                                        API Key Setting <span class="text-danger">*</span>
                                    </label>
                                    <asp:DropDownList ID="ddlApiKeySetting" runat="server" CssClass="form-select">
                                        <asp:ListItem Text="-- Select API Key Setting --" Value="" />
                                    </asp:DropDownList>
                                    <div class="help-text">Encrypted API key from Application Settings</div>
                                </div>
                            </div>
                            <div class="col-md-6">
                                <div class="form-group mb-3">
                                    <label for="<%= txtDefaultMaxTokens.ClientID %>" class="form-label">
                                        Default Max Tokens
                                    </label>
                                    <asp:TextBox ID="txtDefaultMaxTokens" runat="server" CssClass="form-control" 
                                        TextMode="Number" min="1" max="4000" placeholder="1000"></asp:TextBox>
                                    <div class="help-text">Default token limit for this model</div>
                                </div>

                                <div class="form-group mb-3">
                                    <label for="<%= txtDefaultTemperature.ClientID %>" class="form-label">
                                        Default Temperature
                                    </label>
                                    <asp:TextBox ID="txtDefaultTemperature" runat="server" CssClass="form-control" 
                                        TextMode="Number" step="0.1" min="0.0" max="2.0" placeholder="0.7"></asp:TextBox>
                                    <div class="help-text">Default creativity level (0.0 = focused, 2.0 = creative)</div>
                                </div>

                                <div class="form-group mb-3">
                                    <div class="form-check">
                                        <asp:CheckBox ID="chkIsActive" runat="server" CssClass="form-check-input" Checked="true" />
                                        <label class="form-check-label" for="<%= chkIsActive.ClientID %>">
                                            Active (available to users)
                                        </label>
                                    </div>
                                </div>
                            </div>
                        </div>

                        <div class="form-group mb-3">
                            <label for="<%= txtNotes.ClientID %>" class="form-label">Notes</label>
                            <asp:TextBox ID="txtNotes" runat="server" CssClass="form-control" TextMode="MultiLine" 
                                Rows="3" placeholder="Optional notes about this model..."></asp:TextBox>
                        </div>

                        <div class="form-group">
                            <asp:Button ID="btnSaveModel" runat="server" Text="Save Model" 
                                CssClass="btn btn-success me-2" OnClick="btnSaveModel_Click" />
                            <asp:Button ID="btnCancelEdit" runat="server" Text="Cancel" 
                                CssClass="btn btn-secondary" OnClick="btnCancelEdit_Click" CausesValidation="false" />
                        </div>
                    </div>
                </asp:Panel>

                <!-- Models Grid -->
                <div class="models-grid">
                    <asp:GridView ID="gvModels" runat="server" AutoGenerateColumns="false" 
                        CssClass="table table-striped gridview" DataKeyNames="Id" 
                        OnRowCommand="gvModels_RowCommand" OnRowDataBound="gvModels_RowDataBound"
                        EmptyDataText="No AI models configured. Add your first model to get started.">
                        <Columns>
                            <asp:TemplateField HeaderText="Display Name">
                                <ItemTemplate>
                                    <strong><%# Eval("DisplayName") %></strong>
                                    <br />
                                    <small class="text-muted"><%# Eval("ModelIdentifier") %></small>
                                </ItemTemplate>
                            </asp:TemplateField>

                            <asp:TemplateField HeaderText="API Key Setting">
                                <ItemTemplate>
                                    <code><%# Eval("OpenAISettingKeyForApiKey") %></code>
                                </ItemTemplate>
                            </asp:TemplateField>

                            <asp:TemplateField HeaderText="Defaults">
                                <ItemTemplate>
                                    <div>
                                        <strong>Tokens:</strong> <%# Eval("DefaultMaxTokens") ?? "Not set" %>
                                    </div>
                                    <div>
                                        <strong>Temp:</strong> <%# Eval("DefaultTemperature") != null ? string.Format("{0:F1}", Eval("DefaultTemperature")) : "Not set" %>
                                    </div>
                                </ItemTemplate>
                            </asp:TemplateField>

                            <asp:TemplateField HeaderText="Status">
                                <ItemTemplate>
                                    <span class='<%# (bool)Eval("IsActive") ? "model-status status-active" : "model-status status-inactive" %>'>
                                        <%# (bool)Eval("IsActive") ? "Active" : "Inactive" %>
                                    </span>
                                </ItemTemplate>
                            </asp:TemplateField>

                            <asp:TemplateField HeaderText="Notes">
                                <ItemTemplate>
                                    <div class="notes-preview" title='<%# Eval("Notes") %>'>
                                        <%# !string.IsNullOrEmpty(Eval("Notes")?.ToString()) ? Eval("Notes") : "-" %>
                                    </div>
                                </ItemTemplate>
                            </asp:TemplateField>

                            <asp:TemplateField HeaderText="Created">
                                <ItemTemplate>
                                    <%# ((DateTime)Eval("CreatedAt")).ToString("MMM dd, yyyy") %>
                                </ItemTemplate>
                            </asp:TemplateField>

                            <asp:TemplateField HeaderText="Actions">
                                <ItemTemplate>
                                    <div class="btn-group btn-group-sm">
                                        <asp:Button ID="btnEdit" runat="server" Text="Edit" CssClass="btn btn-primary btn-sm" 
                                            CommandName="EditModel" CommandArgument='<%# Eval("Id") %>' />
                                        <asp:Button ID="btnToggleStatus" runat="server" CssClass="btn btn-sm" 
                                            CommandName="ToggleStatus" CommandArgument='<%# Eval("Id") %>' />
                                        <asp:Button ID="btnDelete" runat="server" Text="Delete" CssClass="btn btn-danger btn-sm" 
                                            CommandName="DeleteModel" CommandArgument='<%# Eval("Id") %>' 
                                            OnClientClick="return confirm('Are you sure you want to delete this AI model? This action cannot be undone.');" />
                                    </div>
                                </ItemTemplate>
                            </asp:TemplateField>
                        </Columns>
                        <EmptyDataTemplate>
                            <div class="text-center py-4">
                                <i class="fas fa-brain fa-3x text-muted mb-3"></i>
                                <h4 class="text-muted">No AI Models Configured</h4>
                                <p class="text-muted">Add your first AI model to enable AI-powered features for users.</p>
                            </div>
                        </EmptyDataTemplate>
                    </asp:GridView>
                </div>
            </div>
        </div>
    </div>
</asp:Content>

<asp:Content ID="Content3" ContentPlaceHolderID="ScriptsPlaceHolder" runat="server">
    <script>
        // Form validation
        function validateModelForm() {
            var displayName = document.getElementById('<%= txtDisplayName.ClientID %>').value.trim();
            var modelId = document.getElementById('<%= txtModelIdentifier.ClientID %>').value.trim();
            var apiKey = document.getElementById('<%= ddlApiKeySetting.ClientID %>').value;

            if (!displayName) {
                alert('Display Name is required.');
                return false;
            }

            if (!modelId) {
                alert('Model Identifier is required.');
                return false;
            }

            if (!apiKey) {
                alert('Please select an API Key Setting.');
                return false;
            }

            return true;
        }

        // Attach validation to save button
        document.addEventListener('DOMContentLoaded', function() {
            var saveBtn = document.getElementById('<%= btnSaveModel.ClientID %>');
            if (saveBtn) {
                saveBtn.addEventListener('click', function(e) {
                    if (!validateModelForm()) {
                        e.preventDefault();
                        return false;
                    }
                });
            }
        });

        // Confirmation for status toggle
        function confirmStatusToggle(modelName, currentStatus) {
            var action = currentStatus ? 'deactivate' : 'activate';
            return confirm('Are you sure you want to ' + action + ' the "' + modelName + '" model?');
        }
    </script>
</asp:Content>

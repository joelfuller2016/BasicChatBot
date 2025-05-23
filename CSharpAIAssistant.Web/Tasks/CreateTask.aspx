<%@ Page Title="Create AI Task" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="CreateTask.aspx.cs" Inherits="CSharpAIAssistant.Web.Tasks.CreateTask" %>

<asp:Content ID="Content1" ContentPlaceHolderID="HeadContent" runat="server">
    <style>
        .task-form {
            max-width: 800px;
            margin: 0 auto;
        }
        .form-group {
            margin-bottom: 1.5rem;
        }
        .form-control {
            margin-bottom: 0.5rem;
        }
        .parameter-section {
            background-color: #f8f9fa;
            padding: 20px;
            border-radius: 5px;
            margin-top: 20px;
        }
        .parameter-section h5 {
            color: #495057;
            margin-bottom: 15px;
        }
        .help-text {
            font-size: 0.875rem;
            color: #6c757d;
            margin-top: 5px;
        }
        #txtPrompt {
            min-height: 120px;
            resize: vertical;
        }
        .btn-submit {
            padding: 12px 30px;
            font-size: 1.1rem;
        }
    </style>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
    <div class="container task-form">
        <div class="row">
            <div class="col-md-12">
                <h2 class="mb-4">
                    <i class="fas fa-robot"></i> Create New AI Task
                </h2>
                
                <asp:Panel ID="pnlMessage" runat="server" Visible="false" CssClass="alert" role="alert">
                    <asp:Literal ID="litMessage" runat="server"></asp:Literal>
                </asp:Panel>

                <div class="card">
                    <div class="card-body">
                        <form class="needs-validation" novalidate>
                            <!-- Task Name (Optional) -->
                            <div class="form-group">
                                <label for="<%= txtTaskName.ClientID %>" class="form-label">
                                    <i class="fas fa-tag"></i> Task Name (Optional)
                                </label>
                                <asp:TextBox ID="txtTaskName" runat="server" CssClass="form-control" 
                                    placeholder="Enter a descriptive name for your task..."></asp:TextBox>
                                <div class="help-text">
                                    Give your task a memorable name to help you find it later
                                </div>
                            </div>

                            <!-- AI Model Selection -->
                            <div class="form-group">
                                <label for="<%= ddlAIModels.ClientID %>" class="form-label">
                                    <i class="fas fa-brain"></i> AI Model <span class="text-danger">*</span>
                                </label>
                                <asp:DropDownList ID="ddlAIModels" runat="server" CssClass="form-select" required>
                                    <asp:ListItem Text="-- Select an AI Model --" Value="" />
                                </asp:DropDownList>
                                <div class="invalid-feedback">
                                    Please select an AI model.
                                </div>
                                <div class="help-text">
                                    Choose the AI model that best fits your task requirements
                                </div>
                            </div>

                            <!-- Prompt Text -->
                            <div class="form-group">
                                <label for="<%= txtPrompt.ClientID %>" class="form-label">
                                    <i class="fas fa-comment-dots"></i> Prompt <span class="text-danger">*</span>
                                </label>
                                <asp:TextBox ID="txtPrompt" runat="server" TextMode="MultiLine" CssClass="form-control" 
                                    placeholder="Enter your prompt here..." required></asp:TextBox>
                                <div class="invalid-feedback">
                                    Please enter a prompt for the AI.
                                </div>
                                <div class="help-text">
                                    Be specific and clear about what you want the AI to do. The better your prompt, the better the results.
                                </div>
                            </div>

                            <!-- Advanced Parameters -->
                            <div class="parameter-section">
                                <h5><i class="fas fa-cogs"></i> Advanced Parameters</h5>
                                <div class="row">
                                    <div class="col-md-6">
                                        <div class="form-group">
                                            <label for="<%= txtMaxTokens.ClientID %>" class="form-label">
                                                Max Tokens
                                            </label>
                                            <asp:TextBox ID="txtMaxTokens" runat="server" CssClass="form-control" 
                                                TextMode="Number" min="1" max="4000"></asp:TextBox>
                                            <div class="help-text">
                                                Maximum number of tokens in the response (leave blank for model default)
                                            </div>
                                        </div>
                                    </div>
                                    <div class="col-md-6">
                                        <div class="form-group">
                                            <label for="<%= txtTemperature.ClientID %>" class="form-label">
                                                Temperature
                                            </label>
                                            <asp:TextBox ID="txtTemperature" runat="server" CssClass="form-control" 
                                                TextMode="Number" step="0.1" min="0.0" max="2.0"></asp:TextBox>
                                            <div class="help-text">
                                                Controls randomness: 0.0 = focused, 2.0 = creative (leave blank for model default)
                                            </div>
                                        </div>
                                    </div>
                                </div>
                            </div>

                            <!-- Submit Button -->
                            <div class="form-group text-center mt-4">
                                <asp:Button ID="btnSubmitTask" runat="server" Text="Create AI Task" 
                                    CssClass="btn btn-primary btn-submit" OnClick="btnSubmitTask_Click" />
                                <a href="../Default.aspx" class="btn btn-secondary btn-submit ms-3">Cancel</a>
                            </div>
                        </form>
                    </div>
                </div>
            </div>
        </div>
    </div>
</asp:Content>

<asp:Content ID="Content3" ContentPlaceHolderID="ScriptsPlaceHolder" runat="server">
    <script>
        // Bootstrap form validation
        (function() {
            'use strict';
            window.addEventListener('load', function() {
                var forms = document.getElementsByClassName('needs-validation');
                var validation = Array.prototype.filter.call(forms, function(form) {
                    form.addEventListener('submit', function(event) {
                        if (form.checkValidity() === false) {
                            event.preventDefault();
                            event.stopPropagation();
                        }
                        form.classList.add('was-validated');
                    }, false);
                });
            }, false);
        })();

        // Character counter for prompt
        document.addEventListener('DOMContentLoaded', function() {
            const promptTextArea = document.getElementById('<%= txtPrompt.ClientID %>');
            if (promptTextArea) {
                const charCountDiv = document.createElement('div');
                charCountDiv.className = 'help-text text-end';
                charCountDiv.id = 'charCount';
                promptTextArea.parentNode.appendChild(charCountDiv);
                
                function updateCharCount() {
                    const count = promptTextArea.value.length;
                    charCountDiv.textContent = count + ' characters';
                }
                
                promptTextArea.addEventListener('input', updateCharCount);
                updateCharCount();
            }
        });
    </script>
</asp:Content>

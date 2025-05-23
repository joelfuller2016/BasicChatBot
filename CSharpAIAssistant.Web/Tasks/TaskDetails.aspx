<%@ Page Title="Task Details" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="TaskDetails.aspx.cs" Inherits="CSharpAIAssistant.Web.Tasks.TaskDetails" %>

<asp:Content ID="Content1" ContentPlaceHolderID="HeadContent" runat="server">
    <style>
        .task-details {
            max-width: 1000px;
            margin: 0 auto;
        }
        .task-status {
            display: inline-block;
            padding: 0.5rem 1rem;
            border-radius: 0.5rem;
            font-size: 1rem;
            font-weight: 600;
            text-transform: uppercase;
        }
        .status-pending {
            background-color: #fef3c7;
            color: #92400e;
        }
        .status-queued {
            background-color: #dbeafe;
            color: #1e40af;
        }
        .status-processing {
            background-color: #fed7aa;
            color: #ea580c;
        }
        .status-completed {
            background-color: #dcfce7;
            color: #166534;
        }
        .status-failed {
            background-color: #fecaca;
            color: #dc2626;
        }
        .detail-card {
            margin-bottom: 1.5rem;
            border: 1px solid #dee2e6;
            border-radius: 0.5rem;
            box-shadow: 0 0.125rem 0.25rem rgba(0, 0, 0, 0.075);
        }
        .detail-card .card-header {
            background-color: #f8f9fa;
            border-bottom: 1px solid #dee2e6;
            font-weight: 600;
            padding: 1rem 1.25rem;
        }
        .detail-card .card-body {
            padding: 1.25rem;
        }
        .prompt-content {
            background-color: #f8f9fa;
            border: 1px solid #dee2e6;
            border-radius: 0.375rem;
            padding: 1rem;
            white-space: pre-wrap;
            font-family: 'Courier New', monospace;
            max-height: 300px;
            overflow-y: auto;
        }
        .result-content {
            background-color: #ffffff;
            border: 1px solid #dee2e6;
            border-radius: 0.375rem;
            padding: 1rem;
            white-space: pre-wrap;
            max-height: 400px;
            overflow-y: auto;
        }
        .error-content {
            background-color: #f8d7da;
            border: 1px solid #f5c6cb;
            border-radius: 0.375rem;
            padding: 1rem;
            color: #721c24;
        }
        .metadata-grid {
            display: grid;
            grid-template-columns: 1fr 1fr;
            gap: 1rem;
        }
        .metadata-item {
            display: flex;
            justify-content: space-between;
            padding: 0.5rem 0;
            border-bottom: 1px solid #eee;
        }
        .metadata-label {
            font-weight: 600;
            color: #495057;
        }
        .metadata-value {
            color: #212529;
        }
        .token-usage {
            display: grid;
            grid-template-columns: repeat(auto-fit, minmax(120px, 1fr));
            gap: 1rem;
            margin-top: 1rem;
        }
        .token-stat {
            text-align: center;
            padding: 1rem;
            background-color: #f8f9fa;
            border-radius: 0.375rem;
        }
        .token-stat .number {
            font-size: 1.5rem;
            font-weight: bold;
            color: #495057;
        }
        .token-stat .label {
            font-size: 0.875rem;
            color: #6c757d;
            margin-top: 0.25rem;
        }
        .loading-indicator {
            text-align: center;
            padding: 2rem;
            color: #6c757d;
        }
        .loading-indicator i {
            font-size: 2rem;
            margin-bottom: 1rem;
            animation: spin 1s linear infinite;
        }
        @keyframes spin {
            0% { transform: rotate(0deg); }
            100% { transform: rotate(360deg); }
        }
    </style>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
    <div class="container task-details">
        <div class="row">
            <div class="col-md-12">
                <!-- Header -->
                <div class="d-flex justify-content-between align-items-center mb-4">
                    <h2><i class="fas fa-info-circle"></i> Task Details</h2>
                    <div>
                        <a href="TaskList.aspx" class="btn btn-secondary">
                            <i class="fas fa-arrow-left"></i> Back to Tasks
                        </a>
                        <asp:Button ID="btnRefresh" runat="server" Text="Refresh" CssClass="btn btn-outline-primary" OnClick="btnRefresh_Click" />
                    </div>
                </div>

                <!-- Error Panel -->
                <asp:Panel ID="pnlError" runat="server" Visible="false" CssClass="alert alert-danger" role="alert">
                    <i class="fas fa-exclamation-triangle"></i>
                    <asp:Literal ID="litErrorMessage" runat="server"></asp:Literal>
                </asp:Panel>

                <!-- Task Information Card -->
                <asp:Panel ID="pnlTaskInfo" runat="server" Visible="false">
                    <div class="detail-card">
                        <div class="card-header">
                            <i class="fas fa-tasks"></i> Task Information
                        </div>
                        <div class="card-body">
                            <div class="row mb-3">
                                <div class="col-md-8">
                                    <h4 class="mb-2">
                                        <asp:Literal ID="litTaskName" runat="server"></asp:Literal>
                                    </h4>
                                    <span class="task-status" id="statusBadge" runat="server">
                                        <asp:Literal ID="litStatus" runat="server"></asp:Literal>
                                    </span>
                                </div>
                                <div class="col-md-4 text-end">
                                    <strong>Task ID:</strong> <asp:Literal ID="litTaskId" runat="server"></asp:Literal>
                                </div>
                            </div>

                            <div class="metadata-grid">
                                <div>
                                    <div class="metadata-item">
                                        <span class="metadata-label">AI Model:</span>
                                        <span class="metadata-value"><asp:Literal ID="litAIModel" runat="server"></asp:Literal></span>
                                    </div>
                                    <div class="metadata-item">
                                        <span class="metadata-label">Max Tokens:</span>
                                        <span class="metadata-value"><asp:Literal ID="litMaxTokens" runat="server"></asp:Literal></span>
                                    </div>
                                    <div class="metadata-item">
                                        <span class="metadata-label">Temperature:</span>
                                        <span class="metadata-value"><asp:Literal ID="litTemperature" runat="server"></asp:Literal></span>
                                    </div>
                                </div>
                                <div>
                                    <div class="metadata-item">
                                        <span class="metadata-label">Created:</span>
                                        <span class="metadata-value"><asp:Literal ID="litCreatedAt" runat="server"></asp:Literal></span>
                                    </div>
                                    <div class="metadata-item">
                                        <span class="metadata-label">Queued:</span>
                                        <span class="metadata-value"><asp:Literal ID="litQueuedAt" runat="server"></asp:Literal></span>
                                    </div>
                                    <div class="metadata-item">
                                        <span class="metadata-label">Processing Started:</span>
                                        <span class="metadata-value"><asp:Literal ID="litProcessingStartedAt" runat="server"></asp:Literal></span>
                                    </div>
                                    <div class="metadata-item">
                                        <span class="metadata-label">Completed:</span>
                                        <span class="metadata-value"><asp:Literal ID="litCompletedAt" runat="server"></asp:Literal></span>
                                    </div>
                                </div>
                            </div>
                        </div>
                    </div>

                    <!-- Prompt Card -->
                    <div class="detail-card">
                        <div class="card-header">
                            <i class="fas fa-comment-dots"></i> Prompt
                        </div>
                        <div class="card-body">
                            <div class="prompt-content">
                                <asp:Literal ID="litPromptText" runat="server"></asp:Literal>
                            </div>
                        </div>
                    </div>

                    <!-- Results Card -->
                    <asp:Panel ID="pnlResults" runat="server" Visible="false">
                        <div class="detail-card">
                            <div class="card-header">
                                <i class="fas fa-robot"></i> AI Response
                            </div>
                            <div class="card-body">
                                <!-- Success Result -->
                                <asp:Panel ID="pnlSuccessResult" runat="server" Visible="false">
                                    <div class="result-content">
                                        <asp:Literal ID="litGeneratedContent" runat="server"></asp:Literal>
                                    </div>

                                    <!-- Token Usage Statistics -->
                                    <asp:Panel ID="pnlTokenUsage" runat="server" Visible="false">
                                        <div class="token-usage">
                                            <div class="token-stat">
                                                <div class="number"><asp:Literal ID="litPromptTokens" runat="server"></asp:Literal></div>
                                                <div class="label">Prompt Tokens</div>
                                            </div>
                                            <div class="token-stat">
                                                <div class="number"><asp:Literal ID="litCompletionTokens" runat="server"></asp:Literal></div>
                                                <div class="label">Completion Tokens</div>
                                            </div>
                                            <div class="token-stat">
                                                <div class="number"><asp:Literal ID="litTotalTokens" runat="server"></asp:Literal></div>
                                                <div class="label">Total Tokens</div>
                                            </div>
                                            <div class="token-stat">
                                                <div class="number"><asp:Literal ID="litProcessingTime" runat="server"></asp:Literal>ms</div>
                                                <div class="label">Processing Time</div>
                                            </div>
                                        </div>
                                    </asp:Panel>
                                </asp:Panel>

                                <!-- Error Result -->
                                <asp:Panel ID="pnlErrorResult" runat="server" Visible="false">
                                    <div class="error-content">
                                        <h5><i class="fas fa-exclamation-triangle"></i> Error Details</h5>
                                        <asp:Literal ID="litErrorDetails" runat="server"></asp:Literal>
                                    </div>
                                </asp:Panel>
                            </div>
                        </div>
                    </asp:Panel>

                    <!-- Processing Indicator -->
                    <asp:Panel ID="pnlProcessing" runat="server" Visible="false">
                        <div class="detail-card">
                            <div class="card-body">
                                <div class="loading-indicator">
                                    <i class="fas fa-cog"></i>
                                    <h4>Processing Your Request</h4>
                                    <p>Your AI task is currently being processed. This page will automatically refresh to show the results.</p>
                                    <small class="text-muted">This may take a few moments depending on the complexity of your request.</small>
                                </div>
                            </div>
                        </div>
                    </asp:Panel>
                </asp:Panel>
            </div>
        </div>
    </div>
</asp:Content>

<asp:Content ID="Content3" ContentPlaceHolderID="ScriptsPlaceHolder" runat="server">
    <script>
        // Auto-refresh for processing tasks
        $(document).ready(function() {
            var isProcessing = <%# IsTaskProcessing().ToString().ToLower() %>;
            
            if (isProcessing) {
                // Refresh every 15 seconds for processing tasks
                setTimeout(function() {
                    $('#<%= btnRefresh.ClientID %>').click();
                }, 15000);
            }
        });

        // Copy to clipboard functionality
        function copyToClipboard(text) {
            navigator.clipboard.writeText(text).then(function() {
                // Show a temporary success message
                var alert = $('<div class="alert alert-success alert-dismissible fade show" role="alert">' +
                    '<i class="fas fa-check"></i> Content copied to clipboard!' +
                    '<button type="button" class="btn-close" data-bs-dismiss="alert"></button>' +
                    '</div>');
                
                $('.container').prepend(alert);
                
                setTimeout(function() {
                    alert.fadeOut();
                }, 3000);
            }).catch(function(err) {
                console.error('Failed to copy: ', err);
            });
        }

        // Add copy buttons to content areas
        $(document).ready(function() {
            $('.result-content, .prompt-content').each(function() {
                var $this = $(this);
                var $copyBtn = $('<button type="button" class="btn btn-sm btn-outline-secondary float-end mb-2" onclick="copyToClipboard(\'' + 
                    $this.text().replace(/'/g, "\\'") + '\')">' +
                    '<i class="fas fa-copy"></i> Copy</button>');
                $this.before($copyBtn);
            });
        });
    </script>
</asp:Content>

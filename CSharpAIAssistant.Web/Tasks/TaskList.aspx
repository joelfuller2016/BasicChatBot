<%@ Page Title="My AI Tasks" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="TaskList.aspx.cs" Inherits="CSharpAIAssistant.Web.Tasks.TaskList" %>

<asp:Content ID="Content1" ContentPlaceHolderID="HeadContent" runat="server">
    <style>
        .task-list {
            max-width: 1200px;
            margin: 0 auto;
        }
        .task-status {
            display: inline-block;
            padding: 0.25rem 0.5rem;
            border-radius: 0.25rem;
            font-size: 0.875rem;
            font-weight: 500;
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
        .task-grid {
            margin-top: 20px;
        }
        .task-grid .gridview {
            border: 1px solid #dee2e6;
            border-radius: 0.375rem;
            overflow: hidden;
        }
        .task-grid .gridview th {
            background-color: #f8f9fa;
            border-bottom: 2px solid #dee2e6;
            padding: 12px;
            font-weight: 600;
            color: #495057;
        }
        .task-grid .gridview td {
            padding: 12px;
            border-bottom: 1px solid #dee2e6;
            vertical-align: middle;
        }
        .task-grid .gridview tr:hover {
            background-color: #f8f9fa;
        }
        .task-name {
            font-weight: 500;
            color: #212529;
        }
        .task-prompt-preview {
            color: #6c757d;
            font-size: 0.9rem;
            max-width: 300px;
            overflow: hidden;
            text-overflow: ellipsis;
            white-space: nowrap;
        }
        .pagination-container {
            margin-top: 20px;
            display: flex;
            justify-content: center;
        }
        .task-actions {
            text-align: center;
        }
        .task-actions .btn {
            margin: 2px;
        }
        .no-tasks {
            text-align: center;
            padding: 40px;
            color: #6c757d;
        }
        .no-tasks i {
            font-size: 3rem;
            margin-bottom: 1rem;
            color: #dee2e6;
        }
        .stats-cards {
            margin-bottom: 30px;
        }
        .stat-card {
            background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
            color: white;
            border-radius: 0.5rem;
            padding: 1.5rem;
            text-align: center;
        }
        .stat-card h3 {
            margin: 0;
            font-size: 2rem;
            font-weight: bold;
        }
        .stat-card p {
            margin: 0;
            opacity: 0.9;
        }
    </style>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
    <div class="container task-list">
        <div class="row">
            <div class="col-md-12">
                <!-- Header -->
                <div class="d-flex justify-content-between align-items-center mb-4">
                    <h2><i class="fas fa-tasks"></i> My AI Tasks</h2>
                    <a href="CreateTask.aspx" class="btn btn-primary">
                        <i class="fas fa-plus"></i> Create New Task
                    </a>
                </div>

                <!-- Task Statistics -->
                <div class="row stats-cards">
                    <div class="col-md-3">
                        <div class="stat-card">
                            <h3><asp:Literal ID="litTotalTasks" runat="server">0</asp:Literal></h3>
                            <p>Total Tasks</p>
                        </div>
                    </div>
                    <div class="col-md-3">
                        <div class="stat-card" style="background: linear-gradient(135deg, #f093fb 0%, #f5576c 100%);">
                            <h3><asp:Literal ID="litCompletedTasks" runat="server">0</asp:Literal></h3>
                            <p>Completed</p>
                        </div>
                    </div>
                    <div class="col-md-3">
                        <div class="stat-card" style="background: linear-gradient(135deg, #4facfe 0%, #00f2fe 100%);">
                            <h3><asp:Literal ID="litProcessingTasks" runat="server">0</asp:Literal></h3>
                            <p>Processing</p>
                        </div>
                    </div>
                    <div class="col-md-3">
                        <div class="stat-card" style="background: linear-gradient(135deg, #43e97b 0%, #38f9d7 100%);">
                            <h3><asp:Literal ID="litFailedTasks" runat="server">0</asp:Literal></h3>
                            <p>Failed</p>
                        </div>
                    </div>
                </div>

                <!-- Filter Controls -->
                <div class="card mb-3">
                    <div class="card-body">
                        <div class="row align-items-end">
                            <div class="col-md-3">
                                <label for="<%= ddlStatusFilter.ClientID %>" class="form-label">Filter by Status:</label>
                                <asp:DropDownList ID="ddlStatusFilter" runat="server" CssClass="form-select" AutoPostBack="true" OnSelectedIndexChanged="ddlStatusFilter_SelectedIndexChanged">
                                    <asp:ListItem Text="All Statuses" Value="" />
                                    <asp:ListItem Text="Pending" Value="Pending" />
                                    <asp:ListItem Text="Queued" Value="Queued" />
                                    <asp:ListItem Text="Processing" Value="Processing" />
                                    <asp:ListItem Text="Completed" Value="Completed" />
                                    <asp:ListItem Text="Failed" Value="Failed" />
                                </asp:DropDownList>
                            </div>
                            <div class="col-md-3">
                                <label for="<%= ddlPageSize.ClientID %>" class="form-label">Tasks per page:</label>
                                <asp:DropDownList ID="ddlPageSize" runat="server" CssClass="form-select" AutoPostBack="true" OnSelectedIndexChanged="ddlPageSize_SelectedIndexChanged">
                                    <asp:ListItem Text="10" Value="10" />
                                    <asp:ListItem Text="20" Value="20" Selected="True" />
                                    <asp:ListItem Text="50" Value="50" />
                                    <asp:ListItem Text="100" Value="100" />
                                </asp:DropDownList>
                            </div>
                            <div class="col-md-6">
                                <asp:Button ID="btnRefresh" runat="server" Text="Refresh" CssClass="btn btn-secondary" OnClick="btnRefresh_Click" />
                            </div>
                        </div>
                    </div>
                </div>

                <!-- Tasks Grid -->
                <asp:Panel ID="pnlTasksGrid" runat="server" CssClass="task-grid">
                    <asp:GridView ID="gvTasks" runat="server" AutoGenerateColumns="false" CssClass="table table-striped gridview" 
                        DataKeyNames="Id" OnRowDataBound="gvTasks_RowDataBound" EmptyDataText="No tasks found.">
                        <Columns>
                            <asp:TemplateField HeaderText="Task">
                                <ItemTemplate>
                                    <div class="task-name">
                                        <%# !string.IsNullOrEmpty(Eval("TaskName")?.ToString()) ? Eval("TaskName") : "Untitled Task" %>
                                    </div>
                                    <div class="task-prompt-preview">
                                        <%# GetPromptPreview(Eval("PromptText")?.ToString()) %>
                                    </div>
                                </ItemTemplate>
                            </asp:TemplateField>
                            
                            <asp:TemplateField HeaderText="AI Model">
                                <ItemTemplate>
                                    <asp:Literal ID="litModelName" runat="server"></asp:Literal>
                                </ItemTemplate>
                            </asp:TemplateField>
                            
                            <asp:TemplateField HeaderText="Status">
                                <ItemTemplate>
                                    <span class='<%# GetStatusCssClass(Eval("Status")?.ToString()) %>'>
                                        <%# Eval("Status") %>
                                    </span>
                                </ItemTemplate>
                            </asp:TemplateField>
                            
                            <asp:TemplateField HeaderText="Created">
                                <ItemTemplate>
                                    <div><%# ((DateTime)Eval("CreatedAt")).ToString("MMM dd, yyyy") %></div>
                                    <small class="text-muted"><%# ((DateTime)Eval("CreatedAt")).ToString("HH:mm") %></small>
                                </ItemTemplate>
                            </asp:TemplateField>
                            
                            <asp:TemplateField HeaderText="Completed">
                                <ItemTemplate>
                                    <%# Eval("CompletedAt") != null ? ((DateTime)Eval("CompletedAt")).ToString("MMM dd, yyyy HH:mm") : "-" %>
                                </ItemTemplate>
                            </asp:TemplateField>
                            
                            <asp:TemplateField HeaderText="Actions">
                                <ItemTemplate>
                                    <div class="task-actions">
                                        <a href='<%# "TaskDetails.aspx?taskId=" + Eval("Id") %>' class="btn btn-sm btn-primary">
                                            <i class="fas fa-eye"></i> View
                                        </a>
                                    </div>
                                </ItemTemplate>
                            </asp:TemplateField>
                        </Columns>
                        <EmptyDataTemplate>
                            <div class="no-tasks">
                                <i class="fas fa-robot"></i>
                                <h4>No AI tasks found</h4>
                                <p>Create your first AI task to get started!</p>
                                <a href="CreateTask.aspx" class="btn btn-primary">
                                    <i class="fas fa-plus"></i> Create New Task
                                </a>
                            </div>
                        </EmptyDataTemplate>
                    </asp:GridView>
                </asp:Panel>

                <!-- Pagination -->
                <div class="pagination-container">
                    <asp:Literal ID="litPagination" runat="server"></asp:Literal>
                </div>
            </div>
        </div>
    </div>
</asp:Content>

<asp:Content ID="Content3" ContentPlaceHolderID="ScriptsPlaceHolder" runat="server">
    <script>
        // Auto-refresh functionality for active tasks
        $(document).ready(function() {
            var hasActiveTasks = <%# HasActiveTasks().ToString().ToLower() %>;
            
            if (hasActiveTasks) {
                // Refresh every 30 seconds if there are active tasks
                setTimeout(function() {
                    $('#<%= btnRefresh.ClientID %>').click();
                }, 30000);
            }
        });

        // Confirmation for actions
        function confirmAction(action, taskName) {
            return confirm('Are you sure you want to ' + action + ' the task "' + taskName + '"?');
        }
    </script>
</asp:Content>

<%@ Page Title="Home" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Default.aspx.cs" Inherits="CSharpAIAssistant.Web.Default" %>

<asp:Content ID="HeadContent" ContentPlaceHolderID="HeadContent" runat="server">
</asp:Content>

<asp:Content ID="MainContent" ContentPlaceHolderID="MainContent" runat="server">
    <div class="container mt-4">
        <div class="row">
            <div class="col-12">
                <div class="jumbotron bg-primary text-white">
                    <div class="container">
                        <h1 class="display-4">Welcome to AI Assistant</h1>
                        <p class="lead">Your intelligent task processing companion powered by advanced AI models.</p>
                        <asp:Panel ID="pnlAuthenticated" runat="server" Visible="false">
                            <hr class="my-4" style="border-color: rgba(255,255,255,0.3);">
                            <p>Hello, <strong><asp:Literal ID="litUsername" runat="server"></asp:Literal></strong>! Ready to get started with AI-powered tasks?</p>
                            <a class="btn btn-light btn-lg" href="~/Tasks/CreateTask.aspx" runat="server" role="button">Create New Task</a>
                            <a class="btn btn-outline-light btn-lg ml-2" href="~/Tasks/TaskList.aspx" runat="server" role="button">View My Tasks</a>
                        </asp:Panel>
                        <asp:Panel ID="pnlGuest" runat="server" Visible="false">
                            <hr class="my-4" style="border-color: rgba(255,255,255,0.3);">
                            <p>Sign in to start creating and managing AI tasks.</p>
                            <a class="btn btn-light btn-lg" href="~/Account/Login.aspx" runat="server" role="button">Sign In</a>
                            <a class="btn btn-outline-light btn-lg ml-2" href="~/Account/Register.aspx" runat="server" role="button">Create Account</a>
                        </asp:Panel>
                    </div>
                </div>
            </div>
        </div>

        <asp:Panel ID="pnlUserDashboard" runat="server" Visible="false">
            <div class="row">
                <div class="col-md-4 mb-4">
                    <div class="card h-100">
                        <div class="card-body text-center">
                            <i class="fas fa-tasks fa-3x text-primary mb-3"></i>
                            <h5 class="card-title">My Tasks</h5>
                            <p class="card-text">View and manage all your AI processing tasks.</p>
                            <p class="card-text">
                                <small class="text-muted">
                                    Total: <asp:Literal ID="litTotalTasks" runat="server" Text="0"></asp:Literal> | 
                                    Completed: <asp:Literal ID="litCompletedTasks" runat="server" Text="0"></asp:Literal>
                                </small>
                            </p>
                            <a href="~/Tasks/TaskList.aspx" runat="server" class="btn btn-primary">View Tasks</a>
                        </div>
                    </div>
                </div>
                
                <div class="col-md-4 mb-4">
                    <div class="card h-100">
                        <div class="card-body text-center">
                            <i class="fas fa-plus-circle fa-3x text-success mb-3"></i>
                            <h5 class="card-title">Create Task</h5>
                            <p class="card-text">Start a new AI processing task with custom prompts.</p>
                            <p class="card-text">
                                <small class="text-muted">
                                    Available Models: <asp:Literal ID="litAvailableModels" runat="server" Text="0"></asp:Literal>
                                </small>
                            </p>
                            <a href="~/Tasks/CreateTask.aspx" runat="server" class="btn btn-success">Create New</a>
                        </div>
                    </div>
                </div>
                
                <div class="col-md-4 mb-4">
                    <div class="card h-100">
                        <div class="card-body text-center">
                            <i class="fas fa-chart-bar fa-3x text-info mb-3"></i>
                            <h5 class="card-title">Usage Stats</h5>
                            <p class="card-text">Track your AI usage and token consumption.</p>
                            <p class="card-text">
                                <small class="text-muted">
                                    Total Tokens: <asp:Literal ID="litTotalTokens" runat="server" Text="0"></asp:Literal>
                                </small>
                            </p>
                            <a href="~/Tasks/TaskList.aspx" runat="server" class="btn btn-info">View Details</a>
                        </div>
                    </div>
                </div>
            </div>
        </asp:Panel>

        <div class="row mt-4">
            <div class="col-12">
                <h3>What can you do with AI Assistant?</h3>
                <div class="row">
                    <div class="col-md-6">
                        <div class="card mb-3">
                            <div class="card-body">
                                <h5 class="card-title"><i class="fas fa-pen-fancy text-primary mr-2"></i>Content Creation</h5>
                                <p class="card-text">Generate articles, stories, marketing copy, and other written content with AI assistance.</p>
                            </div>
                        </div>
                    </div>
                    <div class="col-md-6">
                        <div class="card mb-3">
                            <div class="card-body">
                                <h5 class="card-title"><i class="fas fa-code text-success mr-2"></i>Code Generation</h5>
                                <p class="card-text">Get help with programming tasks, code reviews, and technical documentation.</p>
                            </div>
                        </div>
                    </div>
                    <div class="col-md-6">
                        <div class="card mb-3">
                            <div class="card-body">
                                <h5 class="card-title"><i class="fas fa-question-circle text-info mr-2"></i>Q&A and Analysis</h5>
                                <p class="card-text">Ask complex questions and get detailed, analytical responses from advanced AI models.</p>
                            </div>
                        </div>
                    </div>
                    <div class="col-md-6">
                        <div class="card mb-3">
                            <div class="card-body">
                                <h5 class="card-title"><i class="fas fa-language text-warning mr-2"></i>Language Tasks</h5>
                                <p class="card-text">Translation, summarization, editing, and other language processing tasks.</p>
                            </div>
                        </div>
                    </div>
                </div>
            </div>
        </div>

        <asp:Panel ID="pnlAdminNotice" runat="server" Visible="false">
            <div class="row mt-4">
                <div class="col-12">
                    <div class="alert alert-info" role="alert">
                        <h5 class="alert-heading"><i class="fas fa-crown mr-2"></i>Administrator Access</h5>
                        <p>You have administrator privileges. You can manage application settings, user accounts, and AI model configurations.</p>
                        <hr>
                        <div class="btn-group" role="group">
                            <a href="~/Admin/Settings.aspx" runat="server" class="btn btn-outline-info">App Settings</a>
                            <a href="~/Admin/UserManagement.aspx" runat="server" class="btn btn-outline-info">User Management</a>
                        </div>
                    </div>
                </div>
            </div>
        </asp:Panel>
    </div>
</asp:Content>

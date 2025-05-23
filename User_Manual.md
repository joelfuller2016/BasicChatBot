# CSharpAIAssistant User Manual

**Version:** 1.0  
**Last Updated:** May 23, 2025

---

## Table of Contents

1. [Introduction](#introduction)
2. [Getting Started](#getting-started)
3. [User Account Management](#user-account-management)
4. [Using the AI Assistant](#using-the-ai-assistant)
5. [Administrator Functions](#administrator-functions)
6. [Troubleshooting](#troubleshooting)
7. [Testing Scenarios](#testing-scenarios)
8. [Test Data Management](#test-data-management)

---

## Introduction

### What is CSharpAIAssistant?

CSharpAIAssistant is a web-based AI task management application that allows users to interact with various AI models (like OpenAI's GPT) through a secure, user-friendly interface. Users can submit prompts, track processing status, and review AI-generated responses with detailed metadata including token usage and processing times.

### Target Audience

- **General Users**: Anyone who needs to interact with AI models for text generation, analysis, or assistance
- **Administrators**: IT staff responsible for managing the application, users, and AI model configurations

### Key Features

- Submit AI tasks with customizable parameters
- Real-time task status tracking
- Multiple AI model support
- Secure authentication (forms-based and Google OAuth)
- Comprehensive admin panel for system management
- Token usage monitoring and cost tracking

---

## Getting Started

### System Requirements

- **Web Browser**: Modern browser (Chrome, Firefox, Safari, Edge)
- **Internet Connection**: Required for AI model interactions
- **Account**: User account created by administrator or self-registration (if enabled)

### Accessing the Application

1. Navigate to the application URL provided by your administrator
2. You'll be presented with the login page
3. Choose your authentication method:
   - **Username/Password**: Use your registered credentials
   - **Google Login**: Use your Google account (if configured)

---

## User Account Management

### Registering a New Account

1. **Navigate to Registration**:
   - Click "Register" link on the login page
   - Fill out the registration form:
     - **Username**: Choose a unique username
     - **Email**: Enter a valid email address
     - **Password**: Create a strong password
     - **Confirm Password**: Re-enter your password

2. **Complete Registration**:
   - Click "Register" to create your account
   - You'll be redirected to the login page
   - Login with your new credentials

### Logging in with Username/Password

1. **Access Login Page**:
   - Enter your username and password
   - Check "Remember Me" to stay logged in (optional)
   - Click "Login"

2. **Successful Login**:
   - You'll be redirected to the dashboard
   - Navigation menu will show user-specific options

### Logging in with Google

1. **Google OAuth Login**:
   - Click "Login with Google" button
   - You'll be redirected to Google's authentication page
   - Authorize the application to access your Google account
   - You'll be redirected back to the application

2. **Account Linking**:
   - If this is your first Google login, an account will be created automatically
   - If you have an existing account with the same email, it will be linked to your Google account

### Logging Out

- Click your username in the navigation bar
- Select "Log Off" from the dropdown menu
- You'll be redirected to the login page

---

## Using the AI Assistant

### Dashboard Overview

Upon successful login, you'll see the main dashboard with:
- **Welcome message** with your username
- **Quick statistics** about your tasks
- **Available AI models** count
- **Recent activity** summary
- **Quick action buttons** to create new tasks

### Creating a New AI Task

1. **Navigate to Task Creation**:
   - Click "Create New Task" from the dashboard
   - Or use "My Tasks" → "Create Task" from the navigation menu

2. **Fill Out Task Details**:
   - **Task Name** (Optional): Give your task a descriptive name
   - **AI Model**: Select from available AI models
   - **Prompt**: Enter your text prompt for the AI
   - **Advanced Parameters** (Optional):
     - **Max Tokens**: Maximum response length (leave blank for default)
     - **Temperature**: Creativity level (0.0 = focused, 2.0 = creative)

3. **Submit Your Task**:
   - Click "Create AI Task"
   - Your task will be queued for processing
   - You'll receive a confirmation message

### Understanding Task Parameters

- **Max Tokens**: Controls the maximum length of the AI response
  - Higher values = longer responses
  - Each model has different token limits
  - Tokens affect cost (if using paid models)

- **Temperature**: Controls creativity and randomness
  - 0.0 = Deterministic, focused responses
  - 1.0 = Balanced creativity
  - 2.0 = Highly creative, more random

### Viewing Your Tasks

1. **Access Task List**:
   - Click "My Tasks" in the navigation menu
   - View all your submitted tasks with status information

2. **Task List Features**:
   - **Filter by Status**: View tasks by their current status
   - **Tasks per Page**: Control how many tasks to display
   - **Auto-refresh**: Active tasks automatically refresh
   - **Task Statistics**: See counts of total, completed, processing, and failed tasks

3. **Task Status Meanings**:
   - **Pending**: Task created but not yet queued
   - **Queued**: Task waiting to be processed
   - **Processing**: AI is currently working on your task
   - **Completed**: Task finished successfully
   - **Failed**: Task encountered an error

### Viewing Task Details

1. **Access Task Details**:
   - Click "View" button next to any task in your task list
   - Or click on a task name to see full details

2. **Task Information Display**:
   - **Task metadata**: Name, model used, parameters, timestamps
   - **Original prompt**: The text you submitted
   - **AI response**: The generated content (if completed)
   - **Processing statistics**: Token usage, processing time
   - **Error details**: If the task failed, detailed error information

3. **Special Features**:
   - **Copy to clipboard**: Copy prompt or response text
   - **Auto-refresh**: Processing tasks automatically refresh
   - **Token usage breakdown**: See exact token consumption

---

## Administrator Functions

*Note: Admin functions are only available to users with administrator privileges.*

### Accessing the Admin Panel

- Admin menu appears in the navigation bar for administrators
- Click "Admin" to see dropdown with options:
  - App Settings
  - User Management  
  - AI Models

### Application Settings Management

1. **Navigate to Settings**:
   - Admin → App Settings
   - View all application configuration settings

2. **Viewing Settings**:
   - Settings are organized by group and type
   - Sensitive settings show as "********" by default
   - Use "Show Decrypted Values" checkbox to reveal sensitive data
   - ⚠️ **Security Warning**: Only enable decryption when necessary

3. **Editing Settings**:
   - Click "Edit" next to any setting
   - Modify the value in the text box
   - For sensitive settings, enter the new value in plaintext
   - Click "Update" to save changes
   - Changes are automatically encrypted if marked as sensitive

4. **Key Settings Explained**:
   - **GoogleClientId**: OAuth client ID for Google authentication
   - **GoogleClientSecret_Encrypted**: OAuth client secret (encrypted)
   - **OpenAI API Keys**: Various encrypted API keys for different models
   - **SessionTimeoutMinutes**: How long users stay logged in
   - **Default AI Parameters**: Default token limits and temperature settings

### User Management

1. **Navigate to User Management**:
   - Admin → User Management
   - View all registered users

2. **Managing User Roles**:
   - View user details: username, email, registration date, last login
   - **Promote to Admin**: Check the "Admin" checkbox next to a user
   - **Demote from Admin**: Uncheck the "Admin" checkbox
   - Changes are saved automatically

3. **Important Notes**:
   - You cannot demote yourself (prevents lockout)
   - Ensure at least one admin account remains active
   - Admin users have access to all administrative functions

### AI Model Configuration

1. **Navigate to AI Models**:
   - Admin → AI Models
   - View all configured AI models

2. **Adding a New AI Model**:
   - Click "Add New Model"
   - Fill out the form:
     - **Display Name**: User-friendly name (e.g., "GPT-4 Turbo")
     - **Model Identifier**: Exact API model name (e.g., "gpt-4-turbo-preview")
     - **API Key Setting**: Select from encrypted API key settings
     - **Default Max Tokens**: Default token limit for this model
     - **Default Temperature**: Default creativity setting
     - **Active**: Whether users can select this model
     - **Notes**: Optional description or notes
   - Click "Save Model"

3. **Editing Existing Models**:
   - Click "Edit" next to any model
   - Modify settings as needed
   - Click "Update Model"

4. **Managing Model Status**:
   - **Activate/Deactivate**: Control whether users can select the model
   - **Delete**: Soft delete (deactivates) the model to preserve data integrity

5. **API Key Requirements**:
   - Before adding models, ensure corresponding API keys are configured in Application Settings
   - API keys must be stored as encrypted settings
   - Use naming convention like "OpenAIApiKey_ModelName_Encrypted"

---

## Troubleshooting

### Common Login Issues

**Problem**: Can't login with username/password
- **Solution**: Verify username and password are correct
- **Solution**: Contact administrator to reset password
- **Solution**: Try Google login if available

**Problem**: Google login not working
- **Solution**: Ensure Google OAuth is configured by administrator
- **Solution**: Clear browser cookies and try again
- **Solution**: Check that you're using the correct Google account

### Task Processing Issues

**Problem**: Task remains "Pending" or "Queued" for too long
- **Solution**: Check if AI models are properly configured
- **Solution**: Verify API keys are valid and have sufficient quota
- **Solution**: Contact administrator to check background processing service

**Problem**: Task "Failed" with error message
- **Solution**: Review error message in task details
- **Solution**: Check if your prompt is within the model's guidelines
- **Solution**: Verify the selected model is active and properly configured
- **Solution**: Contact administrator if API key issues are suspected

**Problem**: No AI models available for selection
- **Solution**: Contact administrator to configure AI models
- **Solution**: Administrator needs to activate models in AI Models management

### General Application Issues

**Problem**: Pages loading slowly or not at all
- **Solution**: Check internet connection
- **Solution**: Try refreshing the page
- **Solution**: Clear browser cache and cookies
- **Solution**: Try a different browser

**Problem**: Can't access admin functions
- **Solution**: Verify you have administrator privileges
- **Solution**: Contact system administrator for role assignment

---

## Testing Scenarios

### Proving Functionality - Step-by-Step Test Cases

#### Test Case 1: Admin API Key and Model Configuration

**User Story**: "As an admin, I want to configure a new OpenAI API key and associate it with a new AI model, so users can utilize this model."

**Test Steps**:
1. Login as admin using default credentials (admin/adminpassword)
2. Navigate to Admin → App Settings
3. Click "Add New Setting" or find existing OpenAI key setting
4. Add setting: "ApiKey_GPT4_Encrypted"
   - Enter your actual OpenAI API key in plaintext
   - Set DataType to "EncryptedString"
   - Mark as sensitive
   - Set group to "AI"
5. Click "Save" and verify it shows as "********"
6. Navigate to Admin → AI Models
7. Click "Add New Model"
8. Fill form:
   - Display Name: "GPT-4 Turbo"
   - Model Identifier: "gpt-4-turbo-preview"
   - API Key Setting: Select "ApiKey_GPT4_Encrypted"
   - Default Max Tokens: 1000
   - Default Temperature: 0.7
   - Active: Checked
9. Click "Save Model"
10. Log out and log in as regular user
11. Navigate to "Create New Task"
12. Verify "GPT-4 Turbo" appears in model dropdown

**Expected Result**: New model successfully configured and available to users

#### Test Case 2: Complete User Registration and Task Flow

**User Story**: "As a new user, I want to register, create an AI task, and view the results."

**Test Steps**:
1. Go to registration page
2. Fill form:
   - Username: "testuser1"
   - Email: "test@example.com"
   - Password: "TestPass123!"
   - Confirm Password: "TestPass123!"
3. Click "Register"
4. Login with new credentials
5. Navigate to "Create New Task"
6. Fill form:
   - Task Name: "Test Task"
   - Prompt: "Write a short poem about technology"
   - Select available AI model
   - Leave parameters as default
7. Click "Create AI Task"
8. Navigate to "My Tasks"
9. Wait for task to process (or refresh manually)
10. Click "View" on completed task
11. Verify AI response is displayed with metadata

**Expected Result**: Successful user registration and task completion

#### Test Case 3: Google OAuth Authentication

**User Story**: "As a user, I want to login using my Google account."

**Test Steps**:
1. Ensure Google OAuth is configured in Application Settings
2. Click "Login with Google" on login page
3. Complete Google authentication
4. Verify automatic account creation/linking
5. Check dashboard shows correct user information
6. Verify normal task creation functionality works

**Expected Result**: Successful Google authentication and account access

#### Test Case 4: Admin User Management

**User Story**: "As an admin, I want to promote a regular user to admin status."

**Test Steps**:
1. Login as admin
2. Navigate to Admin → User Management
3. Find a regular user account
4. Check the "Admin" checkbox next to their name
5. Verify change is saved automatically
6. Ask that user to login
7. Verify they now see Admin menu options

**Expected Result**: User successfully promoted to admin with full privileges

#### Test Case 5: Task Parameter Override

**User Story**: "As a user, I want to customize AI parameters for specific tasks."

**Test Steps**:
1. Login as regular user
2. Navigate to "Create New Task"
3. Fill form:
   - Prompt: "Explain quantum computing"
   - Max Tokens: 500 (custom value)
   - Temperature: 0.1 (low creativity)
4. Submit task
5. View task details when completed
6. Verify token usage respects the 500 token limit
7. Verify response style matches low temperature setting

**Expected Result**: Custom parameters properly applied to AI generation

#### Test Case 6: Error Handling and Recovery

**User Story**: "As a user, I want to understand why my task failed and how to fix it."

**Test Steps**:
1. Admin: Configure an AI model with invalid API key
2. User: Create task using that model
3. Wait for processing to complete
4. View task details
5. Verify clear error message is displayed
6. Admin: Fix the API key configuration
7. User: Create new task with same model
8. Verify task now processes successfully

**Expected Result**: Clear error messaging and successful recovery after fix

---

## Test Data Management

### Adding Test Data

For testing specific scenarios, an administrator might need to create test users or specific AI tasks. This should be done through the application's standard UI rather than direct database manipulation.

**Creating Test Users**:
- Use the registration interface to create test accounts
- Use consistent naming convention (e.g., "test_user_1", "test_user_2")
- Use test email addresses that are clearly identifiable

**Creating Test Tasks**:
- Login with test accounts and create tasks normally
- Use identifying task names like "[TEST] Sample Task"
- Use various AI models and parameters to test different scenarios

### Identifying Test Data

It is recommended that any data created purely for testing purposes be clearly identifiable:

- **Test Users**: Use prefix "test_user_" in usernames
- **Test Tasks**: Use prefix "[TEST]" in task names
- **Test Settings**: Use prefix "TEST_" in setting keys (for non-production environments)

### Removing Test Data

#### User Accounts
Test user accounts can be managed via the 'Admin → User Management' page where you can:
- Revoke admin status from test accounts
- Monitor test account activity
- Add notes about test accounts

For full deletion, direct database intervention by a database administrator would be required since no UI for user deletion is implemented. 

**SQL Script for User Deletion** (Use with extreme caution):
```sql
-- Backup database first!
-- This will delete users and all associated tasks
DELETE FROM AITaskResults WHERE AITaskId IN (
    SELECT Id FROM AITasks WHERE UserId IN (
        SELECT Id FROM Users WHERE Username LIKE 'test_user_%'
    )
);
DELETE FROM AITasks WHERE UserId IN (
    SELECT Id FROM Users WHERE Username LIKE 'test_user_%'
);
DELETE FROM Users WHERE Username LIKE 'test_user_%';
```

#### AI Tasks & Results
There is no built-in UI for bulk deleting tasks. Test tasks will remain in the system unless manually deleted from the database by a database administrator.

**SQL Script for Task Deletion** (Use with extreme caution):
```sql
-- Backup database first!
DELETE FROM AITaskResults WHERE AITaskId IN (
    SELECT Id FROM AITasks WHERE TaskName LIKE '[TEST]%'
);
DELETE FROM AITasks WHERE TaskName LIKE '[TEST]%';
```

#### Application Settings/AI Models
Test configurations can be deleted or deactivated via their respective admin pages:
- Use Admin → App Settings to remove test settings
- Use Admin → AI Models to deactivate or remove test model configurations

### Production Data Integrity

⚠️ **CRITICAL WARNING**: Any direct database manipulation for removing test data must be performed with extreme care:

1. **Always backup the database first** before any deletion operations
2. **Test on a staging environment** before running on production
3. **Verify SQL statements** thoroughly before execution
4. **Have a rollback plan** in case of issues
5. **Consider the impact** on referential integrity and related data

**Backup Command**:
```bash
# Copy the entire database file
copy "App_Data\CSharpAIAssistant.db" "App_Data\CSharpAIAssistant_backup_YYYY-MM-DD.db"
```

### Recommended Approach

For production environments, it's recommended to:
1. Use a separate test/staging environment for testing
2. Implement soft deletion features for better data management
3. Create automated cleanup scripts for test data
4. Use database migrations for schema changes
5. Implement proper audit logging for data changes

---

## Support and Additional Resources

### Getting Help
- Check application logs in Windows Event Viewer
- Review error messages carefully - they often contain helpful information
- Contact your system administrator for configuration issues
- Create detailed bug reports including steps to reproduce issues

### Best Practices
- Use descriptive task names for easier management
- Monitor token usage to control costs
- Regularly backup important AI responses
- Keep API keys secure and rotate them periodically
- Use appropriate temperature settings for your use case

### Performance Tips
- Avoid extremely long prompts (>4000 tokens) unless necessary
- Use mock AI service for development and testing
- Clear browser cache if experiencing performance issues
- Monitor background task processing for bottlenecks

---

**Document Version**: 1.0  
**Last Updated**: May 23, 2025  
**Application Version**: CSharpAIAssistant v1.0

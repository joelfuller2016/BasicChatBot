# CSharpAIAssistant

A comprehensive AI-powered task assistant web application built with ASP.NET Web Forms, .NET Framework 4.8, and SQLite. The application enables users to submit prompts to various AI models (like OpenAI's GPT) and manage the responses through a secure, feature-rich web interface.

## 🚀 Features

### Core Functionality
- **AI Task Management**: Create, queue, and process AI tasks with various models
- **Multi-Model Support**: Configure and manage multiple AI models (OpenAI GPT-3.5, GPT-4, etc.)
- **Background Processing**: Asynchronous task processing with real-time status updates
- **Token Usage Tracking**: Monitor and display token consumption for cost management

### Security & Authentication
- **Dual Authentication**: Forms-based login and Google OAuth integration
- **Role-Based Access**: Admin and regular user roles with appropriate permissions
- **Data Encryption**: AES encryption for sensitive configuration data
- **Secure Password Hashing**: PBKDF2-based password security

### Administration
- **Application Settings Management**: Encrypted storage and management of API keys and configurations
- **User Management**: Admin interface for promoting/demoting users
- **AI Model Configuration**: Add, edit, and manage AI model settings
- **Privileged Data Viewing**: Secure decryption and viewing of sensitive settings

### User Experience
- **Responsive Design**: Bootstrap-powered responsive interface
- **Real-time Updates**: Auto-refresh for processing tasks
- **Comprehensive Dashboard**: User statistics and quick access to features
- **Task History**: Complete audit trail of AI interactions

## 🛠️ Technology Stack

- **.NET Framework 4.8**
- **ASP.NET Web Forms**
- **C# Backend**
- **SQLite Database** with ADO.NET
- **OWIN Middleware** for authentication
- **Bootstrap 5** for responsive UI
- **jQuery** for client-side interactions
- **Font Awesome** for icons

## 🗄️ Database Schema

The application uses SQLite with the following main entities:
- **Users**: User accounts and authentication data
- **ApplicationSettings**: Encrypted application configuration
- **AIModelConfigurations**: AI model settings and API key references
- **AITasks**: User-submitted AI tasks and their status
- **AITaskResults**: Generated AI responses and metadata

## 📋 Prerequisites

- **Windows Server 2012 R2** or later / **Windows 10** or later
- **Internet Information Services (IIS) 8.0** or later
- **.NET Framework 4.8 Runtime**
- **Visual Studio 2019** or later (for development)
- **OpenAI API Key** (for live AI functionality)

## 🚀 Quick Start

### 1. Initial Setup

1. **Clone the repository**:
   ```bash
   git clone [repository-url]
   cd BasicChatBot
   ```

2. **Run the PowerShell setup script**:
   ```powershell
   # Run as Administrator
   .\Build-Project.ps1
   ```
   This script will:
   - Create the project structure
   - Download NuGet packages
   - Generate solution and project files
   - Set up all dependencies

3. **Build the solution**:
   ```bash
   # Using MSBuild
   msbuild CSharpAIAssistant.sln /p:Configuration=Release
   
   # Or using Visual Studio
   # Open CSharpAIAssistant.sln and build (Ctrl+Shift+B)
   ```

### 2. Database Initialization

The application **automatically initializes** its SQLite database on first run:
- Database schema is created automatically
- Default admin user is seeded
- Essential application settings are configured

### 3. IIS Deployment

1. **Create IIS Application**:
   - Open IIS Manager
   - Create new application pointing to `CSharpAIAssistant.Web` folder
   - Set application pool to `.NET Framework v4.0` (Integrated Mode)

2. **Set Permissions**:
   ```cmd
   # Grant IIS_IUSRS permissions to App_Data folder
   icacls "CSharpAIAssistant.Web\App_Data" /grant "IIS_IUSRS:(OI)(CI)F"
   ```

### 4. First-Run Configuration

1. **Access the application** via your IIS application URL
2. **Login with default admin credentials**:
   - Username: `admin`
   - Password: `adminpassword`
   - ⚠️ **Change this password immediately after first login**

3. **Configure API Keys** (Admin Panel → App Settings):
   - Add your OpenAI API key as an encrypted setting
   - Configure Google OAuth credentials (if using Google login)
   - Set other application preferences

4. **Configure AI Models** (Admin Panel → AI Models):
   - Add AI models (e.g., GPT-3.5-turbo, GPT-4)
   - Link models to their respective API keys
   - Set default parameters (max tokens, temperature)

## ⚙️ Configuration

### Application Settings
Key settings managed through the admin interface:

| Setting | Description | Type |
|---------|-------------|------|
| `GoogleClientId` | Google OAuth Client ID | String |
| `GoogleClientSecret_Encrypted` | Google OAuth Client Secret | EncryptedString |
| `OpenAIApiKey_Default_Encrypted` | Default OpenAI API Key | EncryptedString |
| `SessionTimeoutMinutes` | User session timeout | Integer |
| `DefaultAITaskMaxTokens` | Default max tokens for AI tasks | Integer |
| `DefaultAITaskTemperature` | Default temperature setting | Real |
| `UseMockAIService` | Use mock AI service for testing | Boolean |

### AI Model Configuration
- **Model Identifier**: Exact model name from OpenAI API (e.g., `gpt-4-turbo-preview`)
- **Display Name**: User-friendly name shown in the interface
- **API Key Reference**: Links to encrypted API key in application settings
- **Default Parameters**: Max tokens and temperature defaults for the model

## 🔒 Security Considerations

### Production Deployment
- **Change default admin password** immediately
- **Use HTTPS** in production environments
- **Secure API keys** using proper key management solutions
- **Regular backups** of the SQLite database
- **Monitor token usage** to prevent unexpected costs

### Encryption
- Sensitive data is encrypted using AES-256
- ⚠️ **For production**: Replace hardcoded encryption keys with secure key management
- API keys are stored encrypted and only decrypted when needed

## 🧪 Testing

### Mock AI Service
The application includes a `MockAIService` for testing:
- Set `UseMockAIService = true` in Application Settings
- Generates realistic fake responses for development/testing
- No API calls or costs incurred

### Test Scenarios
1. **User Registration/Login**: Test both forms and Google OAuth
2. **Task Creation**: Submit various prompts with different parameters
3. **Admin Functions**: Manage settings, users, and AI models
4. **Background Processing**: Verify asynchronous task processing
5. **Error Handling**: Test with invalid API keys or malformed requests

## 📊 Monitoring & Maintenance

### Logs
- Application events are logged using `System.Diagnostics.Trace`
- Monitor Windows Event Log for application errors
- Background task processing includes detailed logging

### Database Maintenance
- SQLite database is stored in `App_Data/CSharpAIAssistant.db`
- Regular backups recommended
- Monitor database size and performance

### Performance
- Background task processor handles concurrent AI requests
- Application settings cached for performance
- Consider connection pooling for high-traffic scenarios

## 🤝 Contributing

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/amazing-feature`)
3. Commit your changes (`git commit -m 'Add amazing feature'`)
4. Push to the branch (`git push origin feature/amazing-feature`)
5. Open a Pull Request

## 📝 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## 🆘 Support & Troubleshooting

### Common Issues

**Database Connection Errors**:
- Ensure IIS_IUSRS has write permissions to App_Data folder
- Check that SQLite database was created successfully

**API Key Issues**:
- Verify API keys are properly encrypted and stored
- Check that AI models reference correct API key settings
- Monitor OpenAI account for usage limits and billing

**Authentication Problems**:
- Verify Google OAuth credentials are correctly configured
- Check that redirect URIs match in Google Cloud Console
- Ensure HTTPS is used for Google OAuth in production

**Background Processing Issues**:
- Check Application Event Log for AITaskProcessor errors
- Verify API connectivity and credentials
- Monitor task queue and processing times

### Getting Help
- Check the build log (`Build_Log.md`) for implementation details
- Review the user manual (`User_Manual.md`) for usage instructions
- Create an issue in the repository for bugs or feature requests

---

**Built with ❤️ using ASP.NET Web Forms and modern web technologies**

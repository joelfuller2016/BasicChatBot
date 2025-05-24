@echo off
echo Starting CSharpAI Assistant Web Server...
cd /d "C:\Users\joelf\OneDrive\Joels Files\Documents\GitHub\BasicChatBot\CSharpAIAssistant.Web"

REM Start a simple HTTP server using dotnet (if available)
if exist "C:\Program Files\dotnet\dotnet.exe" (
    echo Using dotnet server...
    dotnet exec "C:\Program Files\IIS Express\iisexpress.exe" /path:"%cd%" /port:8080
) else (
    echo Please install IIS or use Visual Studio...
    pause
)

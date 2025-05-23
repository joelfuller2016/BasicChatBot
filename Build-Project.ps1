param(
    [Parameter(Mandatory=$false)]
    [string]$SolutionRoot = (Split-Path -Parent $PSScriptRoot)
)

Write-Host "CSharpAIAssistant Project Setup Script" -ForegroundColor Green
Write-Host "=====================================" -ForegroundColor Green
Write-Host "Solution Root: $SolutionRoot" -ForegroundColor Yellow

# Ensure solution root exists
if (!(Test-Path $SolutionRoot)) {
    New-Item -ItemType Directory -Path $SolutionRoot -Force | Out-Null
    Write-Host "Created solution root directory: $SolutionRoot" -ForegroundColor Cyan
}

# Define project structure
$projectStructure = @{
    "CSharpAIAssistant.Web" = @("App_Data", "Account", "Admin", "Content", "Scripts", "Tasks")
    "CSharpAIAssistant.DataAccess" = @()
    "CSharpAIAssistant.BusinessLogic" = @()
    "CSharpAIAssistant.Models" = @("Interfaces")
    "packages" = @()
}

Write-Host "`nCreating project directory structure..." -ForegroundColor Yellow

# Create directories
foreach ($project in $projectStructure.Keys) {
    $projectPath = Join-Path $SolutionRoot $project
    if (!(Test-Path $projectPath)) {
        New-Item -ItemType Directory -Path $projectPath -Force | Out-Null
        Write-Host "Created: $project" -ForegroundColor Green
    }
    
    # Create subdirectories
    foreach ($subDir in $projectStructure[$project]) {
        $subDirPath = Join-Path $projectPath $subDir
        if (!(Test-Path $subDirPath)) {
            New-Item -ItemType Directory -Path $subDirPath -Force | Out-Null
            Write-Host "  Created: $project\$subDir" -ForegroundColor Green
        }
    }
}

# Define NuGet packages to install
$nugetPackages = @(
    @{ Name = "System.Data.SQLite.Core"; Version = "1.0.118" }
    @{ Name = "Microsoft.Owin.Host.SystemWeb"; Version = "4.2.2" }
    @{ Name = "Microsoft.Owin.Security.Cookies"; Version = "4.2.2" }
    @{ Name = "Microsoft.Owin.Security.Google"; Version = "4.2.2" }
    @{ Name = "Newtonsoft.Json"; Version = "13.0.3" }
    @{ Name = "Microsoft.Owin"; Version = "4.2.2" }
    @{ Name = "Owin"; Version = "1.0" }
)

Write-Host "`nDownloading NuGet packages..." -ForegroundColor Yellow

# Check if nuget.exe exists, if not try to download it
$nugetPath = "nuget.exe"
if (!(Test-Path $nugetPath)) {
    $nugetPath = Join-Path $SolutionRoot "nuget.exe"
    if (!(Test-Path $nugetPath)) {
        Write-Host "Downloading nuget.exe..." -ForegroundColor Cyan
        try {
            Invoke-WebRequest -Uri "https://dist.nuget.org/win-x86-commandline/latest/nuget.exe" -OutFile $nugetPath
            Write-Host "Downloaded nuget.exe successfully" -ForegroundColor Green
        }
        catch {
            Write-Error "Failed to download nuget.exe: $($_.Exception.Message)"
            Write-Host "Please ensure nuget.exe is available in PATH or current directory" -ForegroundColor Red
            exit 1
        }
    }
}

$packagesDir = Join-Path $SolutionRoot "packages"

# Install NuGet packages
foreach ($package in $nugetPackages) {
    Write-Host "Installing $($package.Name) v$($package.Version)..." -ForegroundColor Cyan
    try {
        & $nugetPath install $package.Name -Version $package.Version -OutputDirectory $packagesDir -NonInteractive
        if ($LASTEXITCODE -eq 0) {
            Write-Host "  Successfully installed $($package.Name)" -ForegroundColor Green
        } else {
            Write-Warning "Failed to install package $($package.Name)"
        }
    }
    catch {
        Write-Warning "Error installing package $($package.Name): $($_.Exception.Message)"
    }
}

# Generate unique project GUIDs
function New-ProjectGuid {
    return [System.Guid]::NewGuid().ToString().ToUpper()
}

$projectGuids = @{
    "CSharpAIAssistant.Web" = New-ProjectGuid
    "CSharpAIAssistant.DataAccess" = New-ProjectGuid
    "CSharpAIAssistant.BusinessLogic" = New-ProjectGuid
    "CSharpAIAssistant.Models" = New-ProjectGuid
}

Write-Host "`nGenerating solution file..." -ForegroundColor Yellow

# Generate .sln file content
$slnContent = @"
Microsoft Visual Studio Solution File, Format Version 12.00
# Visual Studio Version 16
VisualStudioVersion = 16.0.30114.105
MinimumVisualStudioVersion = 10.0.40219.1
Project("{FAE04EC0-301F-11D3-BF4B-00C04F79EFBC}") = "CSharpAIAssistant.Web", "CSharpAIAssistant.Web\CSharpAIAssistant.Web.csproj", "{$($projectGuids["CSharpAIAssistant.Web"])}"
EndProject
Project("{FAE04EC0-301F-11D3-BF4B-00C04F79EFBC}") = "CSharpAIAssistant.DataAccess", "CSharpAIAssistant.DataAccess\CSharpAIAssistant.DataAccess.csproj", "{$($projectGuids["CSharpAIAssistant.DataAccess"])}"
EndProject
Project("{FAE04EC0-301F-11D3-BF4B-00C04F79EFBC}") = "CSharpAIAssistant.BusinessLogic", "CSharpAIAssistant.BusinessLogic\CSharpAIAssistant.BusinessLogic.csproj", "{$($projectGuids["CSharpAIAssistant.BusinessLogic"])}"
EndProject
Project("{FAE04EC0-301F-11D3-BF4B-00C04F79EFBC}") = "CSharpAIAssistant.Models", "CSharpAIAssistant.Models\CSharpAIAssistant.Models.csproj", "{$($projectGuids["CSharpAIAssistant.Models"])}"
EndProject
Global
	GlobalSection(SolutionConfigurationPlatforms) = preSolution
		Debug|Any CPU = Debug|Any CPU
		Release|Any CPU = Release|Any CPU
	EndGlobalSection
	GlobalSection(ProjectConfigurationPlatforms) = postSolution
		{$($projectGuids["CSharpAIAssistant.Web"])}.Debug|Any CPU.ActiveCfg = Debug|Any CPU
		{$($projectGuids["CSharpAIAssistant.Web"])}.Debug|Any CPU.Build.0 = Debug|Any CPU
		{$($projectGuids["CSharpAIAssistant.Web"])}.Release|Any CPU.ActiveCfg = Release|Any CPU
		{$($projectGuids["CSharpAIAssistant.Web"])}.Release|Any CPU.Build.0 = Release|Any CPU
		{$($projectGuids["CSharpAIAssistant.DataAccess"])}.Debug|Any CPU.ActiveCfg = Debug|Any CPU
		{$($projectGuids["CSharpAIAssistant.DataAccess"])}.Debug|Any CPU.Build.0 = Debug|Any CPU
		{$($projectGuids["CSharpAIAssistant.DataAccess"])}.Release|Any CPU.ActiveCfg = Release|Any CPU
		{$($projectGuids["CSharpAIAssistant.DataAccess"])}.Release|Any CPU.Build.0 = Release|Any CPU
		{$($projectGuids["CSharpAIAssistant.BusinessLogic"])}.Debug|Any CPU.ActiveCfg = Debug|Any CPU
		{$($projectGuids["CSharpAIAssistant.BusinessLogic"])}.Debug|Any CPU.Build.0 = Debug|Any CPU
		{$($projectGuids["CSharpAIAssistant.BusinessLogic"])}.Release|Any CPU.ActiveCfg = Release|Any CPU
		{$($projectGuids["CSharpAIAssistant.BusinessLogic"])}.Release|Any CPU.Build.0 = Release|Any CPU
		{$($projectGuids["CSharpAIAssistant.Models"])}.Debug|Any CPU.ActiveCfg = Debug|Any CPU
		{$($projectGuids["CSharpAIAssistant.Models"])}.Debug|Any CPU.Build.0 = Debug|Any CPU
		{$($projectGuids["CSharpAIAssistant.Models"])}.Release|Any CPU.ActiveCfg = Release|Any CPU
		{$($projectGuids["CSharpAIAssistant.Models"])}.Release|Any CPU.Build.0 = Release|Any CPU
	EndGlobalSection
	GlobalSection(SolutionProperties) = preSolution
		HideSolutionNode = FALSE
	EndGlobalSection
EndGlobal
"@

$slnPath = Join-Path $SolutionRoot "CSharpAIAssistant.sln"
$slnContent | Out-File -FilePath $slnPath -Encoding UTF8
Write-Host "Generated: CSharpAIAssistant.sln" -ForegroundColor Green

# Function to get package hint path
function Get-PackageHintPath {
    param($PackageName, $PackagesDir, $DllName)
    
    $packageDirs = Get-ChildItem -Path $PackagesDir -Directory | Where-Object { $_.Name -like "$PackageName.*" }
    if ($packageDirs) {
        $latestPackage = $packageDirs | Sort-Object Name -Descending | Select-Object -First 1
        $libDirs = Get-ChildItem -Path $latestPackage.FullName -Directory -Recurse | Where-Object { $_.Name -eq "lib" }
        foreach ($libDir in $libDirs) {
            $netDirs = Get-ChildItem -Path $libDir.FullName -Directory | Where-Object { $_.Name -match "net4" } | Sort-Object Name -Descending
            if ($netDirs) {
                $dllPath = Join-Path $netDirs[0].FullName $DllName
                if (Test-Path $dllPath) {
                    return $dllPath.Replace($SolutionRoot + "\", "")
                }
            }
        }
    }
    return $null
}

Write-Host "`nGenerating project files..." -ForegroundColor Yellow

# Generate Web project file
$webProjectContent = @"
<?xml version="1.0" encoding="utf-8"?>
<Project ToolsVersion="15.0" DefaultTargets="Build" xmlns="http://schemas.microsoft.com/developer/msbuild/2003">
  <Import Project="`$(MSBuildExtensionsPath)\`$(MSBuildToolsVersion)\Microsoft.Common.props" Condition="Exists('`$(MSBuildExtensionsPath)\`$(MSBuildToolsVersion)\Microsoft.Common.props')" />
  <PropertyGroup>
    <Configuration Condition=" '`$(Configuration)' == '' ">Debug</Configuration>
    <Platform Condition=" '`$(Platform)' == '' ">AnyCPU</Platform>
    <ProductVersion>
    </ProductVersion>
    <SchemaVersion>2.0</SchemaVersion>
    <ProjectGuid>{$($projectGuids["CSharpAIAssistant.Web"])}</ProjectGuid>
    <ProjectTypeGuids>{349c5851-65df-11da-9384-00065b846f21};{fae04ec0-301f-11d3-bf4b-00c04f79efbc}</ProjectTypeGuids>
    <OutputType>Library</OutputType>
    <AppDesignerFolder>Properties</AppDesignerFolder>
    <RootNamespace>CSharpAIAssistant.Web</RootNamespace>
    <AssemblyName>CSharpAIAssistant.Web</AssemblyName>
    <TargetFrameworkVersion>v4.8</TargetFrameworkVersion>
    <UseIISExpress>true</UseIISExpress>
    <Use64BitIISExpress />
    <IISExpressSSLPort />
    <IISExpressAnonymousAuthentication />
    <IISExpressWindowsAuthentication />
    <IISExpressUseClassicPipelineMode />
    <UseGlobalApplicationHostFile />
  </PropertyGroup>
  <PropertyGroup Condition=" '`$(Configuration)|`$(Platform)' == 'Debug|AnyCPU' ">
    <DebugSymbols>true</DebugSymbols>
    <DebugType>full</DebugType>
    <Optimize>false</Optimize>
    <OutputPath>bin\</OutputPath>
    <DefineConstants>DEBUG;TRACE</DefineConstants>
    <ErrorReport>prompt</ErrorReport>
    <WarningLevel>4</WarningLevel>
  </PropertyGroup>
  <PropertyGroup Condition=" '`$(Configuration)|`$(Platform)' == 'Release|AnyCPU' ">
    <DebugSymbols>true</DebugSymbols>
    <DebugType>pdbonly</DebugType>
    <Optimize>true</Optimize>
    <OutputPath>bin\</OutputPath>
    <DefineConstants>TRACE</DefineConstants>
    <ErrorReport>prompt</ErrorReport>
    <WarningLevel>4</WarningLevel>
  </PropertyGroup>
  <ItemGroup>
    <Reference Include="Microsoft.CSharp" />
    <Reference Include="System" />
    <Reference Include="System.Data" />
    <Reference Include="System.Drawing" />
    <Reference Include="System.Web" />
    <Reference Include="System.Web.ApplicationServices" />
    <Reference Include="System.Web.Services" />
    <Reference Include="System.Web.Abstractions" />
    <Reference Include="System.Web.Routing" />
    <Reference Include="System.Xml" />
    <Reference Include="System.Configuration" />
    <Reference Include="System.Web.Services" />
    <Reference Include="System.EnterpriseServices" />
    <Reference Include="Microsoft.Web.Infrastructure, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31BF3856AD364E35, processorArchitecture=MSIL">
      <Private>True</Private>
    </Reference>
    <Reference Include="System.Net.Http">
    </Reference>
    <Reference Include="System.ComponentModel.DataAnnotations" />
    <Reference Include="System.Core" />
    <Reference Include="System.Data.DataSetExtensions" />
    <Reference Include="System.Xml.Linq" />
  </ItemGroup>
  <ItemGroup>
    <Content Include="Web.config" />
  </ItemGroup>
  <ItemGroup>
    <Compile Include="Properties\AssemblyInfo.cs" />
  </ItemGroup>
  <ItemGroup>
    <ProjectReference Include="..\CSharpAIAssistant.BusinessLogic\CSharpAIAssistant.BusinessLogic.csproj">
      <Project>{$($projectGuids["CSharpAIAssistant.BusinessLogic"])}</Project>
      <Name>CSharpAIAssistant.BusinessLogic</Name>
    </ProjectReference>
    <ProjectReference Include="..\CSharpAIAssistant.DataAccess\CSharpAIAssistant.DataAccess.csproj">
      <Project>{$($projectGuids["CSharpAIAssistant.DataAccess"])}</Project>
      <Name>CSharpAIAssistant.DataAccess</Name>
    </ProjectReference>
    <ProjectReference Include="..\CSharpAIAssistant.Models\CSharpAIAssistant.Models.csproj">
      <Project>{$($projectGuids["CSharpAIAssistant.Models"])}</Project>
      <Name>CSharpAIAssistant.Models</Name>
    </ProjectReference>
  </ItemGroup>
  <PropertyGroup>
    <VisualStudioVersion Condition="'`$(VisualStudioVersion)' == ''">10.0</VisualStudioVersion>
    <VSToolsPath Condition="'`$(VSToolsPath)' == ''">MsBuild\Microsoft\VisualStudio\v`$(VisualStudioVersion)</VSToolsPath>
  </PropertyGroup>
  <Import Project="`$(MSBuildBinPath)\Microsoft.CSharp.targets" />
  <Import Project="`$(VSToolsPath)\WebApplications\Microsoft.WebApplication.targets" Condition="'`$(VSToolsPath)' != ''" />
  <Import Project="`$(MSBuildExtensionsPath32)\Microsoft\VisualStudio\v10.0\WebApplications\Microsoft.WebApplication.targets" Condition="false" />
  <ProjectExtensions>
    <VisualStudio>
      <FlavorProperties GUID="{349c5851-65df-11da-9384-00065b846f21}">
        <WebProjectProperties>
          <UseIIS>True</UseIIS>
          <AutoAssignPort>True</AutoAssignPort>
          <DevelopmentServerPort>0</DevelopmentServerPort>
          <DevelopmentServerVPath>/</DevelopmentServerVPath>
          <IISUrl>http://localhost:12345/</IISUrl>
          <NTLMAuthentication>False</NTLMAuthentication>
          <UseCustomServer>False</UseCustomServer>
          <CustomServerUrl>
          </CustomServerUrl>
          <SaveServerSettingsInUserFile>False</SaveServerSettingsInUserFile>
        </WebProjectProperties>
      </FlavorProperties>
    </VisualStudio>
  </ProjectExtensions>
</Project>
"@

$webProjectPath = Join-Path $SolutionRoot "CSharpAIAssistant.Web\CSharpAIAssistant.Web.csproj"
$webProjectContent | Out-File -FilePath $webProjectPath -Encoding UTF8
Write-Host "Generated: CSharpAIAssistant.Web.csproj" -ForegroundColor Green

# Generate class library project template
function Generate-ClassLibraryProject {
    param($ProjectName, $ProjectGuid, $References = @())
    
    $referencesXml = ""
    foreach ($ref in $References) {
        $referencesXml += "    <ProjectReference Include=`"..\$($ref.Name)\$($ref.Name).csproj`">`n"
        $referencesXml += "      <Project>{$($ref.Guid)}</Project>`n"
        $referencesXml += "      <Name>$($ref.Name)</Name>`n"
        $referencesXml += "    </ProjectReference>`n"
    }
    
    return @"
<?xml version="1.0" encoding="utf-8"?>
<Project ToolsVersion="15.0" xmlns="http://schemas.microsoft.com/developer/msbuild/2003">
  <Import Project="`$(MSBuildExtensionsPath)\`$(MSBuildToolsVersion)\Microsoft.Common.props" Condition="Exists('`$(MSBuildExtensionsPath)\`$(MSBuildToolsVersion)\Microsoft.Common.props')" />
  <PropertyGroup>
    <Configuration Condition=" '`$(Configuration)' == '' ">Debug</Configuration>
    <Platform Condition=" '`$(Platform)' == '' ">AnyCPU</Platform>
    <ProjectGuid>{$ProjectGuid}</ProjectGuid>
    <OutputType>Library</OutputType>
    <AppDesignerFolder>Properties</AppDesignerFolder>
    <RootNamespace>$ProjectName</RootNamespace>
    <AssemblyName>$ProjectName</AssemblyName>
    <TargetFrameworkVersion>v4.8</TargetFrameworkVersion>
    <FileAlignment>512</FileAlignment>
    <Deterministic>true</Deterministic>
  </PropertyGroup>
  <PropertyGroup Condition=" '`$(Configuration)|`$(Platform)' == 'Debug|AnyCPU' ">
    <DebugSymbols>true</DebugSymbols>
    <DebugType>full</DebugType>
    <Optimize>false</Optimize>
    <OutputPath>bin\Debug\</OutputPath>
    <DefineConstants>DEBUG;TRACE</DefineConstants>
    <ErrorReport>prompt</ErrorReport>
    <WarningLevel>4</WarningLevel>
  </PropertyGroup>
  <PropertyGroup Condition=" '`$(Configuration)|`$(Platform)' == 'Release|AnyCPU' ">
    <DebugType>pdbonly</DebugType>
    <Optimize>true</Optimize>
    <OutputPath>bin\Release\</OutputPath>
    <DefineConstants>TRACE</DefineConstants>
    <ErrorReport>prompt</ErrorReport>
    <WarningLevel>4</WarningLevel>
  </PropertyGroup>
  <ItemGroup>
    <Reference Include="System" />
    <Reference Include="System.Core" />
    <Reference Include="System.Xml.Linq" />
    <Reference Include="System.Data.DataSetExtensions" />
    <Reference Include="Microsoft.CSharp" />
    <Reference Include="System.Data" />
    <Reference Include="System.Net.Http" />
    <Reference Include="System.Xml" />
  </ItemGroup>
  <ItemGroup>
    <Compile Include="Properties\AssemblyInfo.cs" />
  </ItemGroup>
  <ItemGroup>
$referencesXml  </ItemGroup>
  <Import Project="`$(MSBuildToolsPath)\Microsoft.CSharp.targets" />
</Project>
"@
}

# Generate other project files
$modelsProjectContent = Generate-ClassLibraryProject -ProjectName "CSharpAIAssistant.Models" -ProjectGuid $projectGuids["CSharpAIAssistant.Models"]
$modelsProjectPath = Join-Path $SolutionRoot "CSharpAIAssistant.Models\CSharpAIAssistant.Models.csproj"
$modelsProjectContent | Out-File -FilePath $modelsProjectPath -Encoding UTF8
Write-Host "Generated: CSharpAIAssistant.Models.csproj" -ForegroundColor Green

$dataAccessRefs = @(@{Name="CSharpAIAssistant.Models"; Guid=$projectGuids["CSharpAIAssistant.Models"]})
$dataAccessProjectContent = Generate-ClassLibraryProject -ProjectName "CSharpAIAssistant.DataAccess" -ProjectGuid $projectGuids["CSharpAIAssistant.DataAccess"] -References $dataAccessRefs
$dataAccessProjectPath = Join-Path $SolutionRoot "CSharpAIAssistant.DataAccess\CSharpAIAssistant.DataAccess.csproj"
$dataAccessProjectContent | Out-File -FilePath $dataAccessProjectPath -Encoding UTF8
Write-Host "Generated: CSharpAIAssistant.DataAccess.csproj" -ForegroundColor Green

$businessLogicRefs = @(
    @{Name="CSharpAIAssistant.Models"; Guid=$projectGuids["CSharpAIAssistant.Models"]},
    @{Name="CSharpAIAssistant.DataAccess"; Guid=$projectGuids["CSharpAIAssistant.DataAccess"]}
)
$businessLogicProjectContent = Generate-ClassLibraryProject -ProjectName "CSharpAIAssistant.BusinessLogic" -ProjectGuid $projectGuids["CSharpAIAssistant.BusinessLogic"] -References $businessLogicRefs
$businessLogicProjectPath = Join-Path $SolutionRoot "CSharpAIAssistant.BusinessLogic\CSharpAIAssistant.BusinessLogic.csproj"
$businessLogicProjectContent | Out-File -FilePath $businessLogicProjectPath -Encoding UTF8
Write-Host "Generated: CSharpAIAssistant.BusinessLogic.csproj" -ForegroundColor Green

# Generate AssemblyInfo files for each project
function Generate-AssemblyInfo {
    param($ProjectName, $ProjectPath)
    
    $assemblyInfoContent = @"
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

[assembly: AssemblyTitle("$ProjectName")]
[assembly: AssemblyDescription("CSharpAIAssistant - AI-Powered Task Assistant")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCompany("")]
[assembly: AssemblyProduct("CSharpAIAssistant")]
[assembly: AssemblyCopyright("Copyright © 2025")]
[assembly: AssemblyTrademark("")]
[assembly: AssemblyCulture("")]

[assembly: ComVisible(false)]

[assembly: AssemblyVersion("1.0.0.0")]
[assembly: AssemblyFileVersion("1.0.0.0")]
"@

    $assemblyInfoDir = Join-Path $ProjectPath "Properties"
    if (!(Test-Path $assemblyInfoDir)) {
        New-Item -ItemType Directory -Path $assemblyInfoDir -Force | Out-Null
    }
    
    $assemblyInfoPath = Join-Path $assemblyInfoDir "AssemblyInfo.cs"
    $assemblyInfoContent | Out-File -FilePath $assemblyInfoPath -Encoding UTF8
}

Write-Host "`nGenerating AssemblyInfo files..." -ForegroundColor Yellow

Generate-AssemblyInfo -ProjectName "CSharpAIAssistant.Web" -ProjectPath (Join-Path $SolutionRoot "CSharpAIAssistant.Web")
Generate-AssemblyInfo -ProjectName "CSharpAIAssistant.Models" -ProjectPath (Join-Path $SolutionRoot "CSharpAIAssistant.Models")
Generate-AssemblyInfo -ProjectName "CSharpAIAssistant.DataAccess" -ProjectPath (Join-Path $SolutionRoot "CSharpAIAssistant.DataAccess")
Generate-AssemblyInfo -ProjectName "CSharpAIAssistant.BusinessLogic" -ProjectPath (Join-Path $SolutionRoot "CSharpAIAssistant.BusinessLogic")

Write-Host "Generated AssemblyInfo files for all projects" -ForegroundColor Green

Write-Host "`nProject setup completed successfully!" -ForegroundColor Green
Write-Host "Solution file: $slnPath" -ForegroundColor Cyan
Write-Host "You can now open the solution in Visual Studio." -ForegroundColor Cyan

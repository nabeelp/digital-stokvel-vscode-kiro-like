# Developer Setup Guide - Digital Stokvel Banking Platform

**Version:** 1.0  
**Last Updated:** March 24, 2026  
**Status:** Active

---

## Table of Contents

1. [Overview](#overview)
2. [Prerequisites](#prerequisites)
3. [.NET 10 SDK Installation](#net-10-sdk-installation)
4. [IDE Setup](#ide-setup)
5. [Project Setup](#project-setup)
6. [Code Style and Formatting](#code-style-and-formatting)
7. [Debugging Configuration](#debugging-configuration)
8. [Verification Steps](#verification-steps)
9. [Common Issues](#common-issues)
10. [Next Steps](#next-steps)

---

## Overview

This guide helps developers set up their development environment for the Digital Stokvel Banking platform. The platform uses .NET 10, PostgreSQL 15+, Redis 7+, and Docker for local development.

### Development Stack

- **Backend:** .NET 10 (C#)
- **Database:** PostgreSQL 15+
- **Cache:** Redis 7+
- **Containers:** Docker Desktop
- **Version Control:** Git
- **IDEs:** Visual Studio Code, Visual Studio 2022, or JetBrains Rider

---

## Prerequisites

### Required Tools

| Tool | Version | Purpose |
|------|---------|---------|
| **.NET SDK** | 10.0+ | Building and running .NET applications |
| **Git** | 2.40+ | Version control |
| **Docker Desktop** | 4.20+ | Local containers for PostgreSQL, Redis |
| **IDE** | Latest | Code editing and debugging |

### Optional Tools

| Tool | Purpose |
|------|---------|
| **Azure CLI** | Azure resource management |
| **GitHub CLI** | GitHub automation |
| **Postman/Insomnia** | API testing |
| **pgAdmin** | PostgreSQL database management |
| **RedisInsight** | Redis cache management |

---

## .NET 10 SDK Installation

### Windows

#### Option 1: Using Visual Studio Installer (Recommended)

1. Download **Visual Studio 2022** (Community, Professional, or Enterprise) from:
   https://visualstudio.microsoft.com/downloads/

2. Run the Visual Studio Installer

3. Select workloads:
   - ✅ **ASP.NET and web development**
   - ✅ **.NET Multi-platform App UI development** (optional, for future mobile work)

4. In **Individual components**, ensure:
   - ✅ **.NET 10 SDK**
   - ✅ **NuGet package manager**
   - ✅ **Debugging tools for .NET**

5. Click **Install** and wait for completion

6. Verify installation:
   ```powershell
   dotnet --version
   # Expected output: 10.0.x
   ```

#### Option 2: Standalone SDK Installation

1. Download **.NET 10 SDK** from:
   https://dotnet.microsoft.com/download/dotnet/10.0

2. Run the installer (`dotnet-sdk-10.0.xxx-win-x64.exe`)

3. Follow the installation wizard

4. Verify installation:
   ```powershell
   dotnet --version
   dotnet --list-sdks
   dotnet --list-runtimes
   ```

   **Expected output:**
   ```
   10.0.x
   
   SDK versions:
   10.0.x [C:\Program Files\dotnet\sdk]
   
   Runtime versions:
   Microsoft.AspNetCore.App 10.0.x
   Microsoft.NETCore.App 10.0.x
   ```

#### Option 3: Using winget (Windows Package Manager)

```powershell
# Install .NET 10 SDK
winget install Microsoft.DotNet.SDK.10

# Verify installation
dotnet --version
```

### macOS

#### Option 1: Using Homebrew (Recommended)

```bash
# Install Homebrew (if not already installed)
/bin/bash -c "$(curl -fsSL https://raw.githubusercontent.com/Homebrew/install/HEAD/install.sh)"

# Install .NET 10 SDK
brew install --cask dotnet-sdk

# Verify installation
dotnet --version
```

#### Option 2: Manual Installation

1. Download **.NET 10 SDK** for macOS from:
   https://dotnet.microsoft.com/download/dotnet/10.0

2. Open the downloaded `.pkg` file

3. Follow the installation wizard

4. Verify installation:
   ```bash
   dotnet --version
   ```

### Linux (Ubuntu/Debian)

```bash
# Add Microsoft package repository
wget https://packages.microsoft.com/config/ubuntu/22.04/packages-microsoft-prod.deb -O packages-microsoft-prod.deb
sudo dpkg -i packages-microsoft-prod.deb
rm packages-microsoft-prod.deb

# Update package index
sudo apt update

# Install .NET 10 SDK
sudo apt install -y dotnet-sdk-10.0

# Verify installation
dotnet --version
```

### Linux (Fedora/RHEL/CentOS)

```bash
# Add Microsoft package repository
sudo rpm -Uvh https://packages.microsoft.com/config/rhel/9/packages-microsoft-prod.rpm

# Install .NET 10 SDK
sudo dnf install dotnet-sdk-10.0

# Verify installation
dotnet --version
```

### Docker-based Development (Alternative)

If you prefer not to install .NET locally, you can use Docker:

```bash
# Run .NET container
docker run -it --rm -v $(pwd):/app -w /app mcr.microsoft.com/dotnet/sdk:10.0 bash

# Inside container
dotnet --version
dotnet build
dotnet test
```

---

## IDE Setup

### Visual Studio Code

#### Installation

**Windows:**
```powershell
winget install Microsoft.VisualStudioCode
```

**macOS:**
```bash
brew install --cask visual-studio-code
```

**Linux:**
```bash
sudo snap install code --classic
```

#### Required Extensions

Install the following extensions via the Extensions panel (`Ctrl+Shift+X` or `Cmd+Shift+X`):

1. **C# for Visual Studio Code** (`ms-dotnettools.csharp`)
   - IntelliSense, debugging, code navigation
   - **Install:** `code --install-extension ms-dotnettools.csharp`

2. **C# Dev Kit** (`ms-dotnettools.csdevkit`)
   - Enhanced C# development experience
   - **Install:** `code --install-extension ms-dotnettools.csdevkit`

3. **.NET Extension Pack** (Recommended)
   - Includes C#, NuGet, and other .NET tools
   - **Install:** Search for ".NET Extension Pack" in Extensions

4. **GitLens** (`eamodio.gitlens`)
   - Advanced Git features
   - **Install:** `code --install-extension eamodio.gitlens`

5. **EditorConfig** (`editorconfig.editorconfig`)
   - Consistent code formatting across IDEs
   - **Install:** `code --install-extension editorconfig.editorconfig`

6. **REST Client** (`humao.rest-client`)
   - Test APIs directly from VS Code
   - **Install:** `code --install-extension humao.rest-client`

7. **Docker** (`ms-azuretools.vscode-docker`)
   - Docker container management
   - **Install:** `code --install-extension ms-azuretools.vscode-docker`

8. **PostgreSQL** (`ckolkman.vscode-postgres`)
   - PostgreSQL database management
   - **Install:** `code --install-extension ckolkman.vscode-postgres`

#### VS Code Configuration

Create or update `.vscode/settings.json` in the workspace:

```json
{
  "dotnet.defaultSolution": "DigitalStokvel.slnx",
  "omnisharp.enableRoslynAnalyzers": true,
  "omnisharp.enableEditorConfigSupport": true,
  "omnisharp.organizeImportsOnFormat": true,
  "editor.formatOnSave": true,
  "editor.codeActionsOnSave": {
    "source.organizeImports": "explicit"
  },
  "files.exclude": {
    "**/bin": true,
    "**/obj": true
  },
  "[csharp]": {
    "editor.defaultFormatter": "ms-dotnettools.csharp",
    "editor.tabSize": 4,
    "editor.insertSpaces": true
  },
  "csharp.format.enable": true,
  "csharp.semanticHighlighting.enabled": true
}
```

#### Launch Configuration for Debugging

Create `.vscode/launch.json`:

```json
{
  "version": "0.2.0",
  "configurations": [
    {
      "name": ".NET Core Launch (API Gateway)",
      "type": "coreclr",
      "request": "launch",
      "preLaunchTask": "build",
      "program": "${workspaceFolder}/src/gateways/DigitalStokvel.ApiGateway/bin/Debug/net10.0/DigitalStokvel.ApiGateway.dll",
      "args": [],
      "cwd": "${workspaceFolder}/src/gateways/DigitalStokvel.ApiGateway",
      "stopAtEntry": false,
      "serverReadyAction": {
        "action": "openExternally",
        "pattern": "\\bNow listening on:\\s+(https?://\\S+)"
      },
      "env": {
        "ASPNETCORE_ENVIRONMENT": "Development"
      }
    },
    {
      "name": ".NET Core Launch (Group Service)",
      "type": "coreclr",
      "request": "launch",
      "preLaunchTask": "build",
      "program": "${workspaceFolder}/src/services/DigitalStokvel.GroupService/bin/Debug/net10.0/DigitalStokvel.GroupService.dll",
      "args": [],
      "cwd": "${workspaceFolder}/src/services/DigitalStokvel.GroupService",
      "stopAtEntry": false,
      "env": {
        "ASPNETCORE_ENVIRONMENT": "Development"
      }
    },
    {
      "name": ".NET Core Attach",
      "type": "coreclr",
      "request": "attach"
    }
  ]
}
```

Create `.vscode/tasks.json`:

```json
{
  "version": "2.0.0",
  "tasks": [
    {
      "label": "build",
      "command": "dotnet",
      "type": "process",
      "args": [
        "build",
        "${workspaceFolder}/DigitalStokvel.slnx",
        "/property:GenerateFullPaths=true",
        "/consoleloggerparameters:NoSummary"
      ],
      "problemMatcher": "$msCompile"
    },
    {
      "label": "test",
      "command": "dotnet",
      "type": "process",
      "args": [
        "test",
        "${workspaceFolder}/DigitalStokvel.slnx"
      ],
      "problemMatcher": "$msCompile"
    }
  ]
}
```

---

### Visual Studio 2022

#### Installation

1. Download **Visual Studio 2022** from:
   https://visualstudio.microsoft.com/downloads/

2. Select edition:
   - **Community:** Free for individuals, open-source, and small teams
   - **Professional:** For professional developers
   - **Enterprise:** For large organizations

3. Install required workloads:
   - ✅ **ASP.NET and web development**
   - ✅ **Azure development** (optional, for Azure deployments)

4. Install optional components:
   - **Git for Windows** (if not already installed)
   - **GitHub Extension for Visual Studio**
   - **Docker Desktop** integration

#### Visual Studio Configuration

1. **Code Style Settings:**
   - Go to **Tools** → **Options** → **Text Editor** → **C#** → **Code Style** → **Formatting**
   - Enable **Organize usings** on save
   - Enable **Format document** on save

2. **Solution Explorer Settings:**
   - Enable **Show All Files** to see bin/obj folders (for reference)
   - Use **Scope to This** to focus on specific projects

3. **Performance Settings:**
   - **Tools** → **Options** → **Environment** → **General**
   - Disable **Automatically adjust visual experience** for better performance
   - Reduce **Recent files** and **Recent projects** count

4. **NuGet Settings:**
   - **Tools** → **NuGet Package Manager** → **Package Manager Settings**
   - Enable **Allow NuGet to download missing packages**

#### Recommended Extensions

Install via **Extensions** → **Manage Extensions**:

1. **ReSharper** (optional, paid)
   - Advanced code analysis and refactoring
   - https://www.jetbrains.com/resharper/

2. **GitHub Copilot** (optional, subscription)
   - AI-powered code completion
   - https://marketplace.visualstudio.com/items?itemName=GitHub.copilotvs

3. **Web Essentials** (for web development)
   - https://marketplace.visualstudio.com/items?itemName=MadsKristensen.WebEssentials2022

---

### JetBrains Rider

#### Installation

1. Download **Rider** from:
   https://www.jetbrains.com/rider/download/

2. Install using the standard installer

3. Activate with:
   - **30-day free trial**
   - **JetBrains license** (paid)
   - **Educational license** (free for students/teachers)

#### Rider Configuration

1. **On first launch:**
   - Select **Import Settings** if migrating from another IDE
   - Choose **Color Scheme** (Light/Dark/Custom)
   - Select **Keymap** (Visual Studio, VS Code, Default)

2. **Plugin Installation:**
   - **Settings** → **Plugins**
   - Install **GitToolBox** for enhanced Git integration
   - Install **Docker** plugin for container management

3. **Code Style Settings:**
   - **Settings** → **Editor** → **Code Style** → **C#**
   - Import `.editorconfig` settings (automatic)
   - Enable **Reformat code** on file save

4. **Solution Explorer Configuration:**
   - Enable **Show All Files** to see hidden items
   - Use **File System** view for Docker/config files

#### Rider Advantages

- **Faster build times** compared to Visual Studio
- **Integrated decompiler** for exploring dependencies
- **Better refactoring tools** out of the box
- **Cross-platform** (Windows, macOS, Linux)

---

## Project Setup

### Clone Repository

```bash
# Clone repository
git clone https://github.com/nabeelp/digital-stokvel-vscode-kiro-like.git
cd digital-stokvel-vscode-kiro-like

# Create feature branch (recommended)
git checkout -b feature/your-feature-name
```

### Restore Dependencies

```bash
# Restore NuGet packages for all projects
dotnet restore

# Or restore for specific solution
dotnet restore DigitalStokvel.slnx
```

### Build Solution

```bash
# Build all projects in Debug mode
dotnet build

# Build in Release mode
dotnet build --configuration Release

# Build specific project
dotnet build src/services/DigitalStokvel.GroupService/DigitalStokvel.GroupService.csproj
```

### Run Tests

```bash
# Run all unit tests
dotnet test

# Run tests with detailed output
dotnet test --verbosity normal

# Run tests with code coverage
dotnet test --collect:"XPlat Code Coverage"
```

### Run Services Locally

```bash
# Run API Gateway
cd src/gateways/DigitalStokvel.ApiGateway
dotnet run

# Or use watch mode for hot reload
dotnet watch run
```

The application will start on:
- HTTPS: `https://localhost:5001`
- HTTP: `http://localhost:5000`
- Swagger UI: `https://localhost:5001/swagger`

---

## Code Style and Formatting

### EditorConfig

The project uses `.editorconfig` for consistent code formatting across all IDEs. The configuration is already in the repository root.

**Key rules:**
- Indent style: **Spaces**
- Indent size: **4**
- End of line: **LF** (Unix-style)
- Charset: **UTF-8**
- Trim trailing whitespace: **Yes**
- Insert final newline: **Yes**

### Code Style Conventions

Based on `docs/CODING_STANDARDS.md`:

1. **Naming Conventions:**
   - Classes: `PascalCase`
   - Methods: `PascalCase`
   - Variables: `camelCase`
   - Constants: `PascalCase`
   - Private fields: `_camelCase` (with underscore prefix)

2. **Code Organization:**
   - One class per file
   - Group members by access modifier
   - Order: Fields → Constructors → Properties → Methods

3. **Using Statements:**
   - Use C# 10 global usings (defined in GlobalUsings.cs)
   - Organize usings: System namespaces first, then third-party, then project

### Formatting Tools

#### Using .NET Format (CLI)

```bash
# Install dotnet-format tool globally
dotnet tool install -g dotnet-format

# Format entire solution
dotnet format

# Format with explicit style
dotnet format --verify-no-changes

# Format specific project
dotnet format src/services/DigitalStokvel.GroupService/DigitalStokvel.GroupService.csproj
```

#### Using IDE Formatters

**Visual Studio Code:**
- Press `Shift+Alt+F` (Windows/Linux) or `Shift+Option+F` (macOS)

**Visual Studio 2022:**
- Press `Ctrl+K, Ctrl+D` to format document
- Press `Ctrl+K, Ctrl+E` to format selection

**Rider:**
- Press `Ctrl+Alt+L` (Windows/Linux) or `Cmd+Option+L` (macOS)

---

## Debugging Configuration

### Debugging in Visual Studio Code

1. Set breakpoints by clicking in the gutter (left of line numbers)

2. Press `F5` or go to **Run and Debug** (`Ctrl+Shift+D`)

3. Select configuration (e.g., ".NET Core Launch (API Gateway)")

4. Use debug controls:
   - `F5` - Continue
   - `F10` - Step Over
   - `F11` - Step Into
   - `Shift+F11` - Step Out
   - `Shift+F5` - Stop Debugging

5. View variables in **Variables** panel

6. Add watch expressions in **Watch** panel

### Debugging in Visual Studio 2022

1. Set breakpoints by clicking in the gutter

2. Press `F5` to start debugging

3. Use **Autos** and **Locals** windows to inspect variables

4. Use **Call Stack** window to navigate execution flow

5. Use **Immediate Window** to execute code during debugging

### Debugging in Rider

1. Set breakpoints by clicking in the gutter

2. Press `Alt+Shift+F9` to run with debugging

3. Use **Variables** view to inspect state

4. Use **Evaluate Expression** (`Alt+F8`) to run code in context

### Debugging Docker Containers

If running services in Docker:

```bash
# Attach debugger to running container
docker ps  # Get container ID
docker exec -it <container-id> bash

# Or use VS Code Docker extension to attach debugger
```

---

## Verification Steps

### Automated Verification Script

Create a file `scripts/verify-dev-setup.sh` (Bash) or `scripts/verify-dev-setup.ps1` (PowerShell).

**Bash version:**

```bash
#!/bin/bash
echo "=== Digital Stokvel Development Environment Verification ==="
echo ""

# Check .NET SDK
echo "Checking .NET SDK..."
if command -v dotnet &> /dev/null; then
    DOTNET_VERSION=$(dotnet --version)
    echo "✅ .NET SDK installed: $DOTNET_VERSION"
    if [[ $DOTNET_VERSION == 10.* ]]; then
        echo "✅ .NET 10 SDK detected"
    else
        echo "⚠️  Warning: .NET 10 required, but $DOTNET_VERSION found"
    fi
else
    echo "❌ .NET SDK not found"
    exit 1
fi
echo ""

# Check Git
echo "Checking Git..."
if command -v git &> /dev/null; then
    GIT_VERSION=$(git --version)
    echo "✅ Git installed: $GIT_VERSION"
else
    echo "❌ Git not found"
    exit 1
fi
echo ""

# Check Docker
echo "Checking Docker..."
if command -v docker &> /dev/null; then
    DOCKER_VERSION=$(docker --version)
    echo "✅ Docker installed: $DOCKER_VERSION"
else
    echo "⚠️  Docker not found (optional, but recommended for local dev)"
fi
echo ""

# Build solution
echo "Building solution..."
if dotnet build --configuration Debug > /dev/null 2>&1; then
    echo "✅ Solution builds successfully"
else
    echo "❌ Build failed. Run 'dotnet build' for details"
    exit 1
fi
echo ""

# Run tests
echo "Running tests..."
if dotnet test --no-build --verbosity quiet > /dev/null 2>&1; then
    echo "✅ All tests passing"
else
    echo "❌ Some tests failing. Run 'dotnet test' for details"
    exit 1
fi
echo ""

echo "=== ✅ Development Environment Ready! ==="
echo ""
echo "Next steps:"
echo "  1. Start local databases: docker-compose up -d"
echo "  2. Run services: dotnet run --project src/gateways/DigitalStokvel.ApiGateway"
echo "  3. Open Swagger UI: https://localhost:5001/swagger"
```

**PowerShell version:**

```powershell
Write-Host "=== Digital Stokvel Development Environment Verification ===" -ForegroundColor Blue
Write-Host ""

# Check .NET SDK
Write-Host "Checking .NET SDK..." -ForegroundColor Cyan
if (Get-Command dotnet -ErrorAction SilentlyContinue) {
    $dotnetVersion = dotnet --version
    Write-Host "✅ .NET SDK installed: $dotnetVersion" -ForegroundColor Green
    if ($dotnetVersion -like "10.*") {
        Write-Host "✅ .NET 10 SDK detected" -ForegroundColor Green
    } else {
        Write-Host "⚠️  Warning: .NET 10 required, but $dotnetVersion found" -ForegroundColor Yellow
    }
} else {
    Write-Host "❌ .NET SDK not found" -ForegroundColor Red
    exit 1
}
Write-Host ""

# Check Git
Write-Host "Checking Git..." -ForegroundColor Cyan
if (Get-Command git -ErrorAction SilentlyContinue) {
    $gitVersion = git --version
    Write-Host "✅ Git installed: $gitVersion" -ForegroundColor Green
} else {
    Write-Host "❌ Git not found" -ForegroundColor Red
    exit 1
}
Write-Host ""

# Check Docker
Write-Host "Checking Docker..." -ForegroundColor Cyan
if (Get-Command docker -ErrorAction SilentlyContinue) {
    $dockerVersion = docker --version
    Write-Host "✅ Docker installed: $dockerVersion" -ForegroundColor Green
} else {
    Write-Host "⚠️  Docker not found (optional, but recommended for local dev)" -ForegroundColor Yellow
}
Write-Host ""

# Build solution
Write-Host "Building solution..." -ForegroundColor Cyan
$buildOutput = dotnet build --configuration Debug 2>&1
if ($LASTEXITCODE -eq 0) {
    Write-Host "✅ Solution builds successfully" -ForegroundColor Green
} else {
    Write-Host "❌ Build failed. Run 'dotnet build' for details" -ForegroundColor Red
    exit 1
}
Write-Host ""

# Run tests
Write-Host "Running tests..." -ForegroundColor Cyan
$testOutput = dotnet test --no-build --verbosity quiet 2>&1
if ($LASTEXITCODE -eq 0) {
    Write-Host "✅ All tests passing" -ForegroundColor Green
} else {
    Write-Host "❌ Some tests failing. Run 'dotnet test' for details" -ForegroundColor Red
    exit 1
}
Write-Host ""

Write-Host "=== ✅ Development Environment Ready! ===" -ForegroundColor Green
Write-Host ""
Write-Host "Next steps:" -ForegroundColor Cyan
Write-Host "  1. Start local databases: docker-compose up -d"
Write-Host "  2. Run services: dotnet run --project src/gateways/DigitalStokvel.ApiGateway"
Write-Host "  3. Open Swagger UI: https://localhost:5001/swagger"
```

### Manual Verification Checklist

- [ ] .NET 10 SDK installed (`dotnet --version` returns `10.0.x`)
- [ ] Git installed and configured
- [ ] IDE installed (VS Code, Visual Studio, or Rider)
- [ ] Required IDE extensions installed
- [ ] Repository cloned successfully
- [ ] Dependencies restored (`dotnet restore`)
- [ ] Solution builds without errors (`dotnet build`)
- [ ] All tests passing (`dotnet test`)
- [ ] Docker Desktop installed (optional)
- [ ] Can access Swagger UI at `https://localhost:5001/swagger`

---

## Common Issues

### Issue: "SDK Not Found" Error

**Symptoms:**
```
The command could not be loaded, possibly because:
  * You intended to execute a .NET application:
      The application 'build' does not exist.
  * You intended to execute a .NET SDK command:
      A compatible .NET SDK was not found.
```

**Solution:**
1. Verify .NET 10 SDK is installed: `dotnet --list-sdks`
2. If not listed, reinstall .NET 10 SDK
3. Check global.json doesn't pin to a different version
4. Restart terminal/IDE after installation

### Issue: "NU1510 Warning" - Unnecessary Package Reference

**Symptoms:**
```
warning NU1510: PackageReference Microsoft.Extensions.Logging.Abstractions will not be pruned.
Consider removing this package from your dependencies, as it is likely unnecessary.
```

**Solution:**
This is a non-blocking warning. The package is likely transitively referenced. You can:
1. Safely ignore (it's just a warning, not an error)
2. Remove explicit reference if it's not directly used in the project

### Issue: Build Fails with "Project Not Restored"

**Symptoms:**
```
error MSB4057: The target "Build" does not exist in the project.
```

**Solution:**
```bash
# Clean solution
dotnet clean

# Remove bin and obj folders
rm -rf **/bin **/obj  # Linux/macOS
Remove-Item -Recurse -Force **/bin, **/obj  # PowerShell

# Restore dependencies
dotnet restore

# Rebuild
dotnet build
```

### Issue: Tests Fail with Database Connection Error

**Symptoms:**
```
Npgsql.NpgsqlException: Connection refused
```

**Solution:**
1. Ensure PostgreSQL is running (see next setup guide)
2. Check connection string in `appsettings.Development.json`
3. Use Docker Compose to start local databases:
   ```bash
   docker-compose up -d postgres redis
   ```

### Issue: Hot Reload Not Working

**Symptoms:**
Code changes not reflected when using `dotnet watch run`

**Solution:**
1. Stop the running process (`Ctrl+C`)
2. Clear build artifacts: `dotnet clean`
3. Restart with watch: `dotnet watch run`
4. Ensure `<Watch>` elements are configured in `.csproj` files

### Issue: IDE IntelliSense Not Working

**Symptoms:**
- No code completion
- Red squiggly lines everywhere
- "OmniSharp" errors in VS Code

**Solution for VS Code:**
1. Open Command Palette (`Ctrl+Shift+P`)
2. Run **OmniSharp: Restart OmniSharp**
3. If still failing, run **Developer: Reload Window**
4. Check **Output** → **OmniSharp Log** for errors

**Solution for Visual Studio:**
1. **Tools** → **Options** → **Text Editor** → **C#** → **IntelliSense**
2. Uncheck **Show completion list after a character is typed**
3. Close and reopen solution
4. If still failing, delete `.vs` folder and restart

**Solution for Rider:**
1. **File** → **Invalidate Caches / Restart**
2. Choose **Invalidate and Restart**

---

## Next Steps

After completing this setup:

1. **Set up local databases:**
   - See `docs/LOCAL_POSTGRES_SETUP.md` (Task 0.1.3) - *To be created*
   - See `docs/LOCAL_REDIS_SETUP.md` (Task 0.1.4) - *To be created*

2. **Configure Docker Desktop:**
   - See `docs/DOCKER_SETUP.md` (Task 0.1.5) - *To be created*

3. **Review coding standards:**
   - Read `docs/CODING_STANDARDS.md`
   - Read `docs/TESTING_STANDARDS.md`

4. **Explore the codebase:**
   - Start with `src/shared/DigitalStokvel.Common` for shared utilities
   - Review `src/services/DigitalStokvel.GroupService` as a reference implementation

5. **Run the application:**
   ```bash
   # Start databases
   docker-compose up -d
   
   # Run API Gateway
   cd src/gateways/DigitalStokvel.ApiGateway
   dotnet run
   
   # Open Swagger UI
   # Navigate to: https://localhost:5001/swagger
   ```

6. **Join the team channels:**
   - Slack: `#engineering`, `#backend-dev`
   - Stand-up: Daily at 9:30 AM SAST
   - Code reviews: Submit PRs to `develop` branch

---

## Additional Resources

### Documentation
- [.NET 10 Documentation](https://docs.microsoft.com/en-us/dotnet/core/whats-new/dotnet-10)
- [ASP.NET Core Documentation](https://docs.microsoft.com/en-us/aspnet/core/)
- [Entity Framework Core Documentation](https://docs.microsoft.com/en-us/ef/core/)
- [xUnit Documentation](https://xunit.net/)

### Team Resources
- **Technical Design:** `.kiro-like/specs/design.md`
- **API Documentation:** Swagger UI at `https://localhost:5001/swagger`
- **Coding Standards:** `docs/CODING_STANDARDS.md`
- **Testing Standards:** `docs/TESTING_STANDARDS.md`
- **Deployment Guide:** `docs/DEPLOYMENT_GUIDE.md`
- **ACR Setup Guide:** `docs/ACR_SETUP_GUIDE.md`

### Contact
- **Tech Lead:** @tech-lead (Slack)
- **DevOps:** @devops-team (Slack: `#platform-engineering`)
- **Questions:** Slack: `#engineering-help`

---

**Document Version:** 1.0  
**Last Updated:** March 24, 2026  
**Maintained by:** Engineering Team  
**Review Frequency:** Quarterly or when .NET version updates

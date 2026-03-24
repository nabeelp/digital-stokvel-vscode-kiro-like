# Development Environment Verification Script
# Digital Stokvel Banking Platform
#
# This script verifies that all required tools and dependencies are properly installed
#

$ErrorActionPreference = "Continue"

# Counters
$script:Passed = 0
$script:Failed = 0
$script:Warnings = 0

# Functions
function Write-Header {
    param([string]$Message)
    Write-Host ""
    Write-Host "================================" -ForegroundColor Blue
    Write-Host $Message -ForegroundColor Blue
    Write-Host "================================" -ForegroundColor Blue
    Write-Host ""
}

function Write-Success {
    param([string]$Message)
    Write-Host "✅ $Message" -ForegroundColor Green
    $script:Passed++
}

function Write-ErrorMessage {
    param([string]$Message)
    Write-Host "❌ $Message" -ForegroundColor Red
    $script:Failed++
}

function Write-WarningMessage {
    param([string]$Message)
    Write-Host "⚠️  $Message" -ForegroundColor Yellow
    $script:Warnings++
}

function Write-InfoMessage {
    param([string]$Message)
    Write-Host "ℹ️  $Message" -ForegroundColor Cyan
}

function Test-CommandExists {
    param([string]$Command)
    $exists = $null -ne (Get-Command $Command -ErrorAction SilentlyContinue)
    return $exists
}

# Main verification
Write-Header "Digital Stokvel Development Environment Verification"

# Check .NET SDK
Write-Host "Checking .NET SDK..." -ForegroundColor Cyan
if (Test-CommandExists "dotnet") {
    $dotnetVersion = dotnet --version
    Write-Success ".NET SDK installed: $dotnetVersion"
    
    # Check if .NET 10
    if ($dotnetVersion -like "10.*") {
        Write-Success ".NET 10 SDK detected"
    } else {
        Write-WarningMessage "Expected .NET 10, but found $dotnetVersion"
    }
    
    # List all SDKs
    Write-Host ""
    Write-InfoMessage "Installed SDKs:"
    dotnet --list-sdks | ForEach-Object { Write-Host "  $_" }
    Write-Host ""
} else {
    Write-ErrorMessage ".NET SDK not found"
    Write-Host "  Install from: https://dotnet.microsoft.com/download/dotnet/10.0"
    Write-Host ""
}

# Check Git
Write-Host "Checking Git..." -ForegroundColor Cyan
if (Test-CommandExists "git") {
    $gitVersion = git --version
    Write-Success "Git installed: $gitVersion"
    
    # Check Git config
    $gitUser = git config --global user.name 2>$null
    $gitEmail = git config --global user.email 2>$null
    
    if ([string]::IsNullOrEmpty($gitUser) -or [string]::IsNullOrEmpty($gitEmail)) {
        Write-WarningMessage "Git user not configured. Run:"
        Write-Host '  git config --global user.name "Your Name"'
        Write-Host '  git config --global user.email "your.email@example.com"'
    } else {
        Write-Success "Git configured as: $gitUser <$gitEmail>"
    }
} else {
    Write-ErrorMessage "Git not found"
    Write-Host "  Install from: https://git-scm.com/downloads"
}
Write-Host ""

# Check Docker
Write-Host "Checking Docker..." -ForegroundColor Cyan
if (Test-CommandExists "docker") {
    $dockerVersion = docker --version
    Write-Success "Docker installed: $dockerVersion"
    
    # Check Docker daemon
    try {
        docker ps 2>&1 | Out-Null
        if ($LASTEXITCODE -eq 0) {
            Write-Success "Docker daemon is running"
        } else {
            Write-WarningMessage "Docker daemon is not running. Start Docker Desktop."
        }
    } catch {
        Write-WarningMessage "Docker daemon is not running. Start Docker Desktop."
    }
} else {
    Write-WarningMessage "Docker not found (optional, but recommended)"
    Write-Host "  Install from: https://www.docker.com/products/docker-desktop"
}
Write-Host ""

# Check Azure CLI (optional)
Write-Host "Checking Azure CLI..." -ForegroundColor Cyan
if (Test-CommandExists "az") {
    $azVersion = (az --version 2>$null | Select-Object -First 1)
    Write-Success "Azure CLI installed: $azVersion"
} else {
    Write-InfoMessage "Azure CLI not found (optional, for Azure deployments)"
    Write-Host "  Install from: https://docs.microsoft.com/en-us/cli/azure/install-azure-cli"
}
Write-Host ""

# Check GitHub CLI (optional)
Write-Host "Checking GitHub CLI..." -ForegroundColor Cyan
if (Test-CommandExists "gh") {
    $ghVersion = (gh --version 2>$null | Select-Object -First 1)
    Write-Success "GitHub CLI installed: $ghVersion"
} else {
    Write-InfoMessage "GitHub CLI not found (optional, for GitHub operations)"
    Write-Host "  Install from: https://cli.github.com/"
}
Write-Host ""

# Check if in repository
Write-Header "Repository Status"

if (Test-Path ".git" -PathType Container) {
    Write-Success "In Git repository"
    
    # Check current branch
    $currentBranch = git branch --show-current
    Write-InfoMessage "Current branch: $currentBranch"
    
    # Check for uncommitted changes
    $statusOutput = git status --porcelain
    if ([string]::IsNullOrEmpty($statusOutput)) {
        Write-Success "Working directory clean"
    } else {
        Write-InfoMessage "Uncommitted changes detected"
    }
} else {
    Write-WarningMessage "Not in a Git repository. Run 'git clone <repo-url>' first."
}
Write-Host ""

# Check dependencies
Write-Header "Project Dependencies"

if (Test-Path "DigitalStokvel.slnx") {
    Write-Success "Solution file found"
    
    # Restore dependencies
    Write-Host "Restoring dependencies..." -ForegroundColor Cyan
    $restoreOutput = dotnet restore 2>&1
    if ($LASTEXITCODE -eq 0) {
        Write-Success "Dependencies restored successfully"
    } else {
        Write-ErrorMessage "Dependency restore failed. Run 'dotnet restore' for details."
    }
} else {
    Write-WarningMessage "Solution file not found. Ensure you're in the repository root."
}
Write-Host ""

# Build solution
Write-Header "Build Verification"

if (Test-Path "DigitalStokvel.slnx") {
    Write-Host "Building solution (Debug configuration)..." -ForegroundColor Cyan
    
    $buildOutput = dotnet build --configuration Debug 2>&1
    $buildExitCode = $LASTEXITCODE
    
    if ($buildExitCode -eq 0) {
        Write-Success "Solution builds successfully"
        
        # Count warnings
        $warningCount = ($buildOutput | Select-String -Pattern "warning" -AllMatches).Matches.Count
        if ($warningCount -gt 0) {
            Write-WarningMessage "$warningCount build warning(s) found"
        }
    } else {
        Write-ErrorMessage "Build failed. Run 'dotnet build' for details."
    }
} else {
    Write-WarningMessage "Skipping build (solution file not found)"
}
Write-Host ""

# Run tests
Write-Header "Test Verification"

if (Test-Path "DigitalStokvel.slnx") {
    Write-Host "Running tests..." -ForegroundColor Cyan
    
    $testOutput = dotnet test --no-build --verbosity quiet 2>&1
    $testExitCode = $LASTEXITCODE
    
    if ($testExitCode -eq 0) {
        Write-Success "All tests passing"
        
        # Extract test summary
        $testSummary = $testOutput | Select-String -Pattern "Test summary:"
        if ($testSummary) {
            Write-InfoMessage "$testSummary"
        }
    } else {
        Write-ErrorMessage "Some tests failing. Run 'dotnet test' for details."
    }
} else {
    Write-WarningMessage "Skipping tests (solution file not found)"
}
Write-Host ""

# Summary
Write-Header "Verification Summary"

Write-Host "Passed:   $script:Passed" -ForegroundColor Green
if ($script:Failed -gt 0) {
    Write-Host "Failed:   $script:Failed" -ForegroundColor Red
}
if ($script:Warnings -gt 0) {
    Write-Host "Warnings: $script:Warnings" -ForegroundColor Yellow
}
Write-Host ""

# Final status
if ($script:Failed -eq 0) {
    Write-Success "Development environment is ready!"
    Write-Host ""
    Write-Host "Next steps:" -ForegroundColor Cyan
    Write-Host "  1. Start local databases: docker-compose up -d"
    Write-Host "  2. Run API Gateway: dotnet run --project src/gateways/DigitalStokvel.ApiGateway"
    Write-Host "  3. Open Swagger UI: https://localhost:5001/swagger"
    Write-Host ""
    exit 0
} else {
    Write-ErrorMessage "Development environment has issues. Please resolve the errors above."
    exit 1
}

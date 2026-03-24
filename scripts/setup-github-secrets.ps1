# GitHub Secrets Setup Script for Digital Stokvel Banking Platform
# This script helps configure required secrets for CI/CD pipelines
#
# Prerequisites:
# - GitHub CLI (gh) installed and authenticated
# - Azure CLI (az) installed and authenticated
# - Appropriate permissions on the GitHub repository and Azure subscriptions
#
# Usage:
#   .\setup-github-secrets.ps1
#

[CmdletBinding()]
param()

$ErrorActionPreference = "Stop"

# Configuration
$RepoOwner = if ($env:GITHUB_REPO_OWNER) { $env:GITHUB_REPO_OWNER } else { "nabeelp" }
$RepoName = if ($env:GITHUB_REPO_NAME) { $env:GITHUB_REPO_NAME } else { "digital-stokvel-vscode-kiro-like" }
$AcrName = if ($env:ACR_NAME) { $env:ACR_NAME } else { "digitalstokvel" }
$StagingRg = if ($env:STAGING_RG) { $env:STAGING_RG } else { "rg-digitalstokvel-staging" }
$StagingCluster = if ($env:STAGING_CLUSTER) { $env:STAGING_CLUSTER } else { "aks-digitalstokvel-staging" }
$ProductionRg = if ($env:PRODUCTION_RG) { $env:PRODUCTION_RG } else { "rg-digitalstokvel-production" }
$ProductionCluster = if ($env:PRODUCTION_CLUSTER) { $env:PRODUCTION_CLUSTER } else { "aks-digitalstokvel-production" }

# Helper functions
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
}

function Write-ErrorMessage {
    param([string]$Message)
    Write-Host "❌ $Message" -ForegroundColor Red
}

function Write-Warning {
    param([string]$Message)
    Write-Host "⚠️  $Message" -ForegroundColor Yellow
}

function Write-Info {
    param([string]$Message)
    Write-Host "ℹ️  $Message" -ForegroundColor Cyan
}

function Test-CommandExists {
    param([string]$Command)
    $exists = $null -ne (Get-Command $Command -ErrorAction SilentlyContinue)
    return $exists
}

function Test-Prerequisites {
    Write-Header "Checking Prerequisites"
    
    # Check GitHub CLI
    if (-not (Test-CommandExists "gh")) {
        Write-ErrorMessage "GitHub CLI (gh) is not installed"
        Write-Host "Install from: https://cli.github.com/"
        exit 1
    }
    Write-Success "GitHub CLI is installed"
    
    # Check Azure CLI
    if (-not (Test-CommandExists "az")) {
        Write-ErrorMessage "Azure CLI (az) is not installed"
        Write-Host "Install from: https://docs.microsoft.com/en-us/cli/azure/install-azure-cli"
        exit 1
    }
    Write-Success "Azure CLI is installed"
    
    # Check GitHub authentication
    try {
        gh auth status 2>&1 | Out-Null
        if ($LASTEXITCODE -ne 0) {
            throw
        }
        Write-Success "GitHub CLI is authenticated"
    }
    catch {
        Write-ErrorMessage "GitHub CLI is not authenticated"
        Write-Host "Run: gh auth login"
        exit 1
    }
    
    # Check Azure authentication
    try {
        az account show 2>&1 | Out-Null
        if ($LASTEXITCODE -ne 0) {
            throw
        }
        Write-Success "Azure CLI is authenticated"
    }
    catch {
        Write-ErrorMessage "Azure CLI is not authenticated"
        Write-Host "Run: az login"
        exit 1
    }
}

function Show-AzureSubscription {
    Write-Header "Azure Subscription Information"
    
    $subscription = az account show | ConvertFrom-Json
    $subscriptionId = $subscription.id
    $subscriptionName = $subscription.name
    $tenantId = $subscription.tenantId
    
    Write-Host "Subscription ID:   " -NoNewline
    Write-Host $subscriptionId -ForegroundColor Green
    Write-Host "Subscription Name: " -NoNewline
    Write-Host $subscriptionName -ForegroundColor Green
    Write-Host "Tenant ID:         " -NoNewline
    Write-Host $tenantId -ForegroundColor Green
    Write-Host ""
    
    $confirmation = Read-Host "Is this the correct subscription? (y/n)"
    if ($confirmation -notmatch '^[Yy]$') {
        Write-ErrorMessage "Please switch to the correct subscription with: az account set --subscription <subscription-id>"
        exit 1
    }
    
    return @{
        SubscriptionId = $subscriptionId
        TenantId = $tenantId
    }
}

function New-AcrServicePrincipal {
    param([string]$SubscriptionId)
    
    Write-Header "Creating ACR Service Principal"
    
    Write-Info "This service principal will be used for pushing Docker images to ACR"
    
    # Check if ACR exists
    try {
        az acr show --name $AcrName 2>&1 | Out-Null
        if ($LASTEXITCODE -ne 0) {
            throw
        }
    }
    catch {
        Write-ErrorMessage "Azure Container Registry '$AcrName' not found"
        Write-Info "Please run scripts/setup-acr.sh or scripts/setup-acr.ps1 first"
        exit 1
    }
    
    $acrResourceId = "/subscriptions/$SubscriptionId/resourceGroups/rg-digitalstokvel-shared/providers/Microsoft.ContainerRegistry/registries/$AcrName"
    
    Write-Info "Creating service principal with acrpush role..."
    
    # Create service principal
    $spOutput = az ad sp create-for-rbac `
        --name "sp-digitalstokvel-acr-github" `
        --role acrpush `
        --scopes $acrResourceId `
        --output json | ConvertFrom-Json
    
    $acrUsername = $spOutput.appId
    $acrPassword = $spOutput.password
    
    Write-Success "Service principal created"
    Write-Host ""
    
    # Set GitHub secrets
    Write-Info "Setting REGISTRY_USERNAME secret..."
    $acrUsername | gh secret set REGISTRY_USERNAME --repo="$RepoOwner/$RepoName"
    Write-Success "REGISTRY_USERNAME secret set"
    
    Write-Info "Setting REGISTRY_PASSWORD secret..."
    $acrPassword | gh secret set REGISTRY_PASSWORD --repo="$RepoOwner/$RepoName"
    Write-Success "REGISTRY_PASSWORD secret set"
    
    Write-Host ""
}

function New-StagingServicePrincipal {
    param([string]$SubscriptionId)
    
    Write-Header "Creating Staging Environment Service Principal"
    
    Write-Info "This service principal will be used for deploying to the staging environment"
    
    # Check if resource group exists
    try {
        az group show --name $StagingRg 2>&1 | Out-Null
        if ($LASTEXITCODE -ne 0) {
            Write-Warning "Resource group '$StagingRg' not found. It will need to be created before deployment."
        }
    }
    catch {
        Write-Warning "Resource group '$StagingRg' not found. It will need to be created before deployment."
    }
    
    Write-Info "Creating service principal with contributor role for staging..."
    
    # Create service principal
    $spOutput = az ad sp create-for-rbac `
        --name "sp-digitalstokvel-staging-github" `
        --role contributor `
        --scopes "/subscriptions/$SubscriptionId/resourceGroups/$StagingRg" `
        --sdk-auth
    
    Write-Success "Service principal created"
    Write-Host ""
    
    # Set GitHub secrets
    Write-Info "Setting AZURE_CREDENTIALS_STAGING secret..."
    $spOutput | gh secret set AZURE_CREDENTIALS_STAGING --repo="$RepoOwner/$RepoName"
    Write-Success "AZURE_CREDENTIALS_STAGING secret set"
    
    Write-Info "Setting STAGING_RESOURCE_GROUP secret..."
    $StagingRg | gh secret set STAGING_RESOURCE_GROUP --repo="$RepoOwner/$RepoName"
    Write-Success "STAGING_RESOURCE_GROUP secret set"
    
    Write-Info "Setting STAGING_CLUSTER_NAME secret..."
    $StagingCluster | gh secret set STAGING_CLUSTER_NAME --repo="$RepoOwner/$RepoName"
    Write-Success "STAGING_CLUSTER_NAME secret set"
    
    Write-Host ""
}

function New-ProductionServicePrincipal {
    param([string]$SubscriptionId)
    
    Write-Header "Creating Production Environment Service Principal"
    
    Write-Info "This service principal will be used for deploying to the production environment"
    
    Write-Warning "Production deployments have elevated privileges. Ensure proper access controls are in place."
    $confirmation = Read-Host "Continue with production service principal creation? (y/n)"
    if ($confirmation -notmatch '^[Yy]$') {
        Write-Warning "Skipping production service principal creation"
        return
    }
    Write-Host ""
    
    # Check if resource group exists
    try {
        az group show --name $ProductionRg 2>&1 | Out-Null
        if ($LASTEXITCODE -ne 0) {
            Write-Warning "Resource group '$ProductionRg' not found. It will need to be created before deployment."
        }
    }
    catch {
        Write-Warning "Resource group '$ProductionRg' not found. It will need to be created before deployment."
    }
    
    Write-Info "Creating service principal with contributor role for production..."
    
    # Create service principal
    $spOutput = az ad sp create-for-rbac `
        --name "sp-digitalstokvel-production-github" `
        --role contributor `
        --scopes "/subscriptions/$SubscriptionId/resourceGroups/$ProductionRg" `
        --sdk-auth
    
    Write-Success "Service principal created"
    Write-Host ""
    
    # Set GitHub secrets
    Write-Info "Setting AZURE_CREDENTIALS_PRODUCTION secret..."
    $spOutput | gh secret set AZURE_CREDENTIALS_PRODUCTION --repo="$RepoOwner/$RepoName"
    Write-Success "AZURE_CREDENTIALS_PRODUCTION secret set"
    
    Write-Info "Setting PRODUCTION_RESOURCE_GROUP secret..."
    $ProductionRg | gh secret set PRODUCTION_RESOURCE_GROUP --repo="$RepoOwner/$RepoName"
    Write-Success "PRODUCTION_RESOURCE_GROUP secret set"
    
    Write-Info "Setting PRODUCTION_CLUSTER_NAME secret..."
    $ProductionCluster | gh secret set PRODUCTION_CLUSTER_NAME --repo="$RepoOwner/$RepoName"
    Write-Success "PRODUCTION_CLUSTER_NAME secret set"
    
    Write-Host ""
}

function Test-GitHubSecrets {
    Write-Header "Verifying GitHub Secrets"
    
    Write-Info "Listing configured secrets..."
    gh secret list --repo="$RepoOwner/$RepoName"
    Write-Host ""
    
    Write-Success "All secrets have been configured successfully!"
    Write-Host ""
}

function Show-Summary {
    Write-Header "Setup Summary"
    
    Write-Host "✅ GitHub Secrets Configuration Complete" -ForegroundColor Green
    Write-Host ""
    Write-Host "The following secrets have been configured for the repository:"
    Write-Host "  $RepoOwner/$RepoName" -ForegroundColor Blue
    Write-Host ""
    Write-Host "Container Registry:" -ForegroundColor Yellow
    Write-Host "  - REGISTRY_USERNAME"
    Write-Host "  - REGISTRY_PASSWORD"
    Write-Host ""
    Write-Host "Staging Environment:" -ForegroundColor Yellow
    Write-Host "  - AZURE_CREDENTIALS_STAGING"
    Write-Host "  - STAGING_RESOURCE_GROUP"
    Write-Host "  - STAGING_CLUSTER_NAME"
    Write-Host ""
    Write-Host "Production Environment:" -ForegroundColor Yellow
    Write-Host "  - AZURE_CREDENTIALS_PRODUCTION"
    Write-Host "  - PRODUCTION_RESOURCE_GROUP"
    Write-Host "  - PRODUCTION_CLUSTER_NAME"
    Write-Host ""
    Write-Host "Next Steps:" -ForegroundColor Blue
    Write-Host "  1. Verify Azure infrastructure is provisioned:"
    Write-Host "     - ACR: $AcrName"
    Write-Host "     - Staging AKS: $StagingCluster in $StagingRg"
    Write-Host "     - Production AKS: $ProductionCluster in $ProductionRg"
    Write-Host ""
    Write-Host "  2. Test staging deployment:"
    Write-Host "     git checkout develop"
    Write-Host "     git push origin develop"
    Write-Host "     # Monitor: https://github.com/$RepoOwner/$RepoName/actions"
    Write-Host ""
    Write-Host "  3. Test production deployment:"
    Write-Host "     git tag -a v1.0.0 -m 'Release v1.0.0'"
    Write-Host "     git push origin v1.0.0"
    Write-Host "     # Create release in GitHub: https://github.com/$RepoOwner/$RepoName/releases"
    Write-Host ""
    Write-Host "  4. Review deployment guide:"
    Write-Host "     docs/DEPLOYMENT_GUIDE.md"
    Write-Host ""
    Write-Host "Security Reminders:" -ForegroundColor Yellow
    Write-Host "  - Store service principal credentials securely"
    Write-Host "  - Rotate credentials every 90 days"
    Write-Host "  - Review service principal permissions quarterly"
    Write-Host "  - Enable Azure AD Conditional Access for production"
    Write-Host "  - Monitor service principal activity in Azure AD logs"
    Write-Host ""
}

# Main execution
try {
    Write-Header "Digital Stokvel Banking - GitHub Secrets Setup"
    Write-Host "This script will configure GitHub Secrets for CI/CD pipelines"
    Write-Host ""
    
    Test-Prerequisites
    $azureInfo = Show-AzureSubscription
    
    # Create service principals and set secrets
    New-AcrServicePrincipal -SubscriptionId $azureInfo.SubscriptionId
    New-StagingServicePrincipal -SubscriptionId $azureInfo.SubscriptionId
    New-ProductionServicePrincipal -SubscriptionId $azureInfo.SubscriptionId
    
    # Verify configuration
    Test-GitHubSecrets
    Show-Summary
    
    Write-Success "Setup completed successfully! 🎉"
}
catch {
    Write-ErrorMessage "An error occurred: $_"
    exit 1
}

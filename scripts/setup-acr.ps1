# Azure Container Registry Setup Script (PowerShell)
# This script provisions ACR for Digital Stokvel Banking platform

param(
    [string]$ResourceGroup = "rg-digitalstokvel-shared",
    [string]$Location = "southafricanorth",
    [string]$RegistryName = "digitalstokvel",
    [ValidateSet("Basic", "Standard", "Premium")]
    [string]$Sku = "Premium",
    [bool]$EnableGeoreplication = $true,
    [string]$SecondaryLocation = "westeurope"
)

$ErrorActionPreference = "Stop"

Write-Host "╔══════════════════════════════════════════════════════╗" -ForegroundColor Green
Write-Host "║   Digital Stokvel - ACR Setup Script                ║" -ForegroundColor Green
Write-Host "╚══════════════════════════════════════════════════════╝" -ForegroundColor Green
Write-Host ""

# Check if Azure CLI is installed
try {
    $azVersion = az version | ConvertFrom-Json
    Write-Host "✓ Azure CLI found (version $($azVersion.'azure-cli'))" -ForegroundColor Green
} catch {
    Write-Host "✗ Azure CLI is not installed" -ForegroundColor Red
    Write-Host "Please install Azure CLI: https://docs.microsoft.com/en-us/cli/azure/install-azure-cli"
    exit 1
}

# Check if logged in
Write-Host ""
Write-Host "Checking Azure login status..."
try {
    $account = az account show | ConvertFrom-Json
    Write-Host "✓ Logged in to Azure" -ForegroundColor Green
    Write-Host "  Subscription: $($account.name)"
    Write-Host "  ID: $($account.id)"
} catch {
    Write-Host "⚠ Not logged in to Azure" -ForegroundColor Yellow
    Write-Host "Please login:"
    az login
    $account = az account show | ConvertFrom-Json
}

# Confirm before proceeding
Write-Host ""
$confirmation = Read-Host "Continue with ACR setup? (y/N)"
if ($confirmation -ne 'y' -and $confirmation -ne 'Y') {
    Write-Host "Setup cancelled"
    exit 0
}

# Create resource group
Write-Host ""
Write-Host "Creating resource group..."
try {
    az group show --name $ResourceGroup *> $null
    Write-Host "⚠ Resource group already exists" -ForegroundColor Yellow
} catch {
    az group create `
        --name $ResourceGroup `
        --location $Location `
        --tags Environment=Production Project=DigitalStokvel
    Write-Host "✓ Resource group created" -ForegroundColor Green
}

# Create ACR
Write-Host ""
Write-Host "Creating Azure Container Registry..."
try {
    az acr show --name $RegistryName *> $null
    Write-Host "⚠ ACR already exists" -ForegroundColor Yellow
} catch {
    az acr create `
        --name $RegistryName `
        --resource-group $ResourceGroup `
        --location $Location `
        --sku $Sku `
        --tags Environment=Production Project=DigitalStokvel
    Write-Host "✓ ACR created" -ForegroundColor Green
}

# Enable geo-replication
if ($Enable Georeplication -and $Sku -eq "Premium") {
    Write-Host ""
    Write-Host "Enabling geo-replication..."
    try {
        az acr replication show --registry $RegistryName --location $SecondaryLocation *> $null
        Write-Host "⚠ Geo-replication already enabled" -ForegroundColor Yellow
    } catch {
        az acr replication create `
            --registry $RegistryName `
            --location $SecondaryLocation
        Write-Host "✓ Geo-replication enabled" -ForegroundColor Green
    }
}

# Get ACR login server
$loginServer = az acr show --name $RegistryName --query loginServer -o tsv

# Create service principal for CI/CD
Write-Host ""
Write-Host "Creating service principal for CI/CD..."
$acrId = az acr show --name $RegistryName --query id -o tsv

$spName = "digitalstokvel-cicd"
try {
    $spJson = az ad sp create-for-rbac `
        --name $spName `
        --role AcrPush `
        --scope $acrId `
        --sdk-auth 2>$null | ConvertFrom-Json
    
    Write-Host "✓ Service principal created" -ForegroundColor Green
    
    Write-Host ""
    Write-Host "╔══════════════════════════════════════════════════════╗" -ForegroundColor Green
    Write-Host "║   GitHub Secrets Configuration                       ║" -ForegroundColor Green
    Write-Host "╚══════════════════════════════════════════════════════╝" -ForegroundColor Green
    Write-Host ""
    Write-Host "Add these secrets to GitHub repository:"
    Write-Host ""
    Write-Host "REGISTRY_USERNAME:"
    Write-Host $spJson.clientId
    Write-Host ""
    Write-Host "REGISTRY_PASSWORD:"
    Write-Host $spJson.clientSecret
    Write-Host ""
    Write-Host "AZURE_CREDENTIALS_STAGING:"
    Write-Host ($spJson | ConvertTo-Json -Compress)
    Write-Host ""
    Write-Host "AZURE_CREDENTIALS_PRODUCTION:"
    Write-Host ($spJson | ConvertTo-Json -Compress)
    Write-Host ""
} catch {
    Write-Host "⚠ Service principal already exists or creation failed" -ForegroundColor Yellow
}

# Summary
Write-Host ""
Write-Host "╔══════════════════════════════════════════════════════╗" -ForegroundColor Green
Write-Host "║   ACR Setup Complete                                 ║" -ForegroundColor Green
Write-Host "╚══════════════════════════════════════════════════════╝" -ForegroundColor Green
Write-Host ""
Write-Host "Registry: $RegistryName"
Write-Host "Login Server: $loginServer"
Write-Host "SKU: $Sku"
Write-Host "Geo-replication: $EnableGeoreplication"
Write-Host ""
Write-Host "Next steps:"
Write-Host "1. Add secrets to GitHub repository (see above)"
Write-Host "2. Login to ACR: az acr login --name $RegistryName"
Write-Host "3. Build and push images: .\scripts\build-and-push.ps1"
Write-Host ""

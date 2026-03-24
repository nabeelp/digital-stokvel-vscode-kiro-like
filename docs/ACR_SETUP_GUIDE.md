# Azure Container Registry Setup Guide

**Version:** 1.0  
**Last Updated:** March 24, 2026  
**Status:** Active

---

## Overview

This document provides step-by-step instructions for setting up Azure Container Registry (ACR) for the Digital Stokvel Banking platform. ACR will store all Docker images for microservices, gateways, and supporting components.

---

## Table of Contents

1. [Prerequisites](#prerequisites)
2. [Registry Configuration](#registry-configuration)
3. [Provisioning with Azure CLI](#provisioning-with-azure-cli)
4. [Infrastructure as Code (Bicep)](#infrastructure-as-code-bicep)
5. [Authentication Setup](#authentication-setup)
6. [Image Naming and Tagging](#image-naming-and-tagging)
7. [GitHub Actions Integration](#github-actions-integration)
8. [Local Development](#local-development)
9. [Maintenance and Operations](#maintenance-and-operations)

---

## Prerequisites

### Required Tools

- **Azure CLI** 2.50+ ([Install](https://docs.microsoft.com/en-us/cli/azure/install-azure-cli))
- **Docker Desktop** 4.0+ ([Install](https://www.docker.com/products/docker-desktop))
- **Azure Subscription** with Contributor role or higher
- **GitHub Account** with repository admin access

### Required Permissions

| Resource | Required Role | Purpose |
|----------|---------------|---------|
| **Azure Subscription** | Contributor | Create ACR resource |
| **Resource Group** | Contributor | Manage resources |
| **ACR** | AcrPush | Push images |
| **ACR** | AcrPull | Pull images |
| **GitHub Repository** | Admin | Configure secrets |

---

## Registry Configuration

### Naming Convention

**Registry Name:** `digitalstokvel`  
**Full URL:** `digitalstokvel.azurecr.io`

**Naming Rules:**
- Must be globally unique
- 5-50 characters
- Alphanumeric only (no hyphens or special characters)
- Lowercase only

### SKU Selection

| SKU | Storage | Throughput | Geo-replication | Use Case |
|-----|---------|------------|-----------------|----------|
| **Basic** | 10 GB | Low | No | Development |
| **Standard** | 100 GB | Medium | No | Staging |
| **Premium** | 500 GB | High | Yes | Production |

**Recommended:**
- **Development:** Basic SKU
- **Staging:** Standard SKU
- **Production:** Premium SKU (with geo-replication to secondary region)

### Resource Specifications

**Location:** South Africa North (primary)  
**Secondary Location:** West Europe (geo-replication for production)  
**Resource Group:** `rg-digitalstokvel-shared`  
**Tags:**
```json
{
  "Environment": "Production",
  "Project": "DigitalStokvel",
  "CostCenter": "Engineering",
  "ManagedBy": "Terraform"
}
```

---

## Provisioning with Azure CLI

### Step 1: Login and Set Subscription

```bash
# Login to Azure
az login

# List available subscriptions
az account list --output table

# Set active subscription
az account set --subscription "Your-Subscription-Name"

# Verify current subscription
az account show --output table
```

### Step 2: Create Resource Group

```bash
# Create resource group
az group create \
  --name rg-digitalstokvel-shared \
  --location southafricanorth \
  --tags Environment=Production Project=DigitalStokvel

# Verify resource group
az group show --name rg-digitalstokvel-shared --output table
```

### Step 3: Create Azure Container Registry

**For Development (Basic SKU):**
```bash
az acr create \
  --name digitalstokvel \
  --resource-group rg-digitalstokvel-shared \
  --location southafricanorth \
  --sku Basic \
  --tags Environment=Development Project=DigitalStokvel
```

**For Production (Premium SKU with geo-replication):**
```bash
# Create ACR
az acr create \
  --name digitalstokvel \
  --resource-group rg-digitalstokvel-shared \
  --location southafricanorth \
  --sku Premium \
  --tags Environment=Production Project=DigitalStokvel

# Enable geo-replication
az acr replication create \
  --registry digitalstokvel \
  --location westeurope
```

### Step 4: Enable Admin Account (Optional - Not Recommended for Production)

```bash
# Enable admin account (for development only)
az acr update --name digitalstokvel --admin-enabled true

# Get admin credentials
az acr credential show --name digitalstokvel --output table
```

⚠️ **Warning:** Admin account should only be used for development. Production should use service principals or managed identities.

### Step 5: Verify ACR

```bash
# Check ACR details
az acr show --name digitalstokvel --output table

# List all repositories (should be empty initially)
az acr repository list --name digitalstokvel --output table

# Test ACR health
az acr check-health --name digitalstokvel --yes
```

---

## Infrastructure as Code (Bicep)

### Bicep Template

**File:** `infrastructure/acr.bicep`

```bicep
@description('Location for all resources')
param location string = resourceGroup().location

@description('Container Registry name')
param registryName string = 'digitalstokvel'

@description('Container Registry SKU')
@allowed([
  'Basic'
  'Standard'
  'Premium'
])
param skuName string = 'Premium'

@description('Enable geo-replication')
param enableGeoreplication bool = true

@description('Secondary location for geo-replication')
param secondaryLocation string = 'westeurope'

@description('Resource tags')
param tags object = {
  Environment: 'Production'
  Project: 'DigitalStokvel'
  CostCenter: 'Engineering'
  ManagedBy: 'Bicep'
}

resource containerRegistry 'Microsoft.ContainerRegistry/registries@2023-01-01-preview' = {
  name: registryName
  location: location
  sku: {
    name: skuName
  }
  tags: tags
  properties: {
    adminUserEnabled: false
    publicNetworkAccess: 'Enabled'
    networkRuleBypassOptions: 'AzureServices'
    policies: {
      quarantinePolicy: {
        status: 'enabled'
      }
      trustPolicy: {
        type: 'Notary'
        status: 'enabled'
      }
      retentionPolicy: {
        days: 30
        status: 'enabled'
      }
    }
    encryption: {
      status: 'disabled'
    }
    dataEndpointEnabled: false
    anonymousPullEnabled: false
    zoneRedundancy: skuName == 'Premium' ? 'Enabled' : 'Disabled'
  }
}

// Geo-replication (Premium SKU only)
resource replication 'Microsoft.ContainerRegistry/registries/replications@2023-01-01-preview' = if (enableGeoreplication && skuName == 'Premium') {
  parent: containerRegistry
  name: secondaryLocation
  location: secondaryLocation
  properties: {
    regionEndpointEnabled: true
    zoneRedundancy: 'Enabled'
  }
}

// Diagnostic settings
resource diagnostics 'Microsoft.Insights/diagnosticSettings@2021-05-01-preview' = {
  name: 'acr-diagnostics'
  scope: containerRegistry
  properties: {
    logs: [
      {
        category: 'ContainerRegistryRepositoryEvents'
        enabled: true
        retentionPolicy: {
          days: 90
          enabled: true
        }
      }
      {
        category: 'ContainerRegistryLoginEvents'
        enabled: true
        retentionPolicy: {
          days: 90
          enabled: true
        }
      }
    ]
    metrics: [
      {
        category: 'AllMetrics'
        enabled: true
        retentionPolicy: {
          days: 90
          enabled: true
        }
      }
    ]
  }
}

output registryName string = containerRegistry.name
output registryLoginServer string = containerRegistry.properties.loginServer
output registryId string = containerRegistry.id
```

### Deploy with Bicep

```bash
# Create deployment
az deployment group create \
  --resource-group rg-digitalstokvel-shared \
  --template-file infrastructure/acr.bicep \
  --parameters skuName=Premium enableGeoreplication=true

# Verify deployment
az deployment group show \
  --resource-group rg-digitalstokvel-shared \
  --name acr \
  --query properties.outputs
```

---

## Authentication Setup

### Option 1: Service Principal (Recommended for CI/CD)

**Create Service Principal:**
```bash
# Get ACR resource ID
ACR_ID=$(az acr show --name digitalstokvel --query id --output tsv)

# Create service principal with AcrPush role
SP_OUTPUT=$(az ad sp create-for-rbac \
  --name digitalstokvel-cicd \
  --role AcrPush \
  --scope $ACR_ID \
  --sdk-auth)

# Display credentials
echo $SP_OUTPUT
```

**Output (save securely):**
```json
{
  "clientId": "00000000-0000-0000-0000-000000000000",
  "clientSecret": "your-secret-here",
  "subscriptionId": "00000000-0000-0000-0000-000000000000",
  "tenantId": "00000000-0000-0000-0000-000000000000",
  "activeDirectoryEndpointUrl": "https://login.microsoftonline.com",
  "resourceManagerEndpointUrl": "https://management.azure.com/",
  "activeDirectoryGraphResourceId": "https://graph.windows.net/",
  "sqlManagementEndpointUrl": "https://management.core.windows.net:8443/",
  "galleryEndpointUrl": "https://gallery.azure.com/",
  "managementEndpointUrl": "https://management.core.windows.net/"
}
```

### Option 2: Managed Identity (Recommended for Azure Resources)

**For AKS Cluster:**
```bash
# Get AKS managed identity
AKS_IDENTITY=$(az aks show \
  --resource-group rg-digitalstokvel-prod \
  --name aks-digitalstokvel-prod \
  --query identityProfile.kubeletidentity.clientId \
  --output tsv)

# Get ACR resource ID
ACR_ID=$(az acr show --name digitalstokvel --query id --output tsv)

# Assign AcrPull role to AKS
az role assignment create \
  --assignee $AKS_IDENTITY \
  --role AcrPull \
  --scope $ACR_ID
```

### Option 3: Azure CLI (Local Development)

```bash
# Login to ACR using Azure CLI
az acr login --name digitalstokvel

# Verify login
docker info | grep -i registry
```

---

## Image Naming and Tagging

### Repository Naming Convention

```
digitalstokvel.azurecr.io/<service-name>:<tag>
```

**Service Names:**
- `digitalstokvel-apigateway`
- `digitalstokvel-ussdgateway`
- `digitalstokfel-groupservice`
- `digitalstokvel-contributionservice`
- `digitalstokvel-payoutservice`
- `digitalstokvel-governanceservice`
- `digitalstokvel-notificationservice`
- `digitalstokvel-creditprofileservice`

### Tag Format

| Environment | Tag Format | Example |
|-------------|------------|---------|
| **Development** | `dev` | `digitalstokvel-groupservice:dev` |
| **Staging** | `staging-latest`, `develop-{sha}` | `digitalstokvel-groupservice:staging-latest` |
| **Production** | `v{semver}`, `latest` | `digitalstokvel-groupservice:v1.0.0` |
| **Feature Branch** | `feature-{branch}` | `digitalstokvel-groupservice:feature-auth` |
| **Pull Request** | `pr-{number}` | `digitalstokvel-groupservice:pr-123` |

### Example Tags

```bash
# Development
digitalstokvel.azurecr.io/digitalstokvel-groupservice:dev

# Staging
digitalstokvel.azurecr.io/digitalstokvel-groupservice:staging-latest
digitalstokvel.azurecr.io/digitalstokvel-groupservice:develop-a1b2c3d

# Production
digitalstokvel.azurecr.io/digitalstokvel-groupservice:v1.0.0
digitalstokvel.azurecr.io/digitalstokvel-groupservice:v1.0.0-20260324
digitalstokvel.azurecr.io/digitalstokvel-groupservice:latest
```

---

## GitHub Actions Integration

### Step 1: Configure GitHub Secrets

Navigate to: Repository → Settings → Secrets and variables → Actions

**Add the following secrets:**

| Secret Name | Value | Purpose |
|-------------|-------|---------|
| `REGISTRY_USERNAME` | Service Principal Client ID | ACR authentication |
| `REGISTRY_PASSWORD` | Service Principal Client Secret | ACR authentication |
| `AZURE_CREDENTIALS_STAGING` | Service Principal JSON (staging) | Azure deployment |
| `AZURE_CREDENTIALS_PRODUCTION` | Service Principal JSON (production) | Azure deployment |

**REGISTRY_USERNAME:**
```
00000000-0000-0000-0000-000000000000
```

**REGISTRY_PASSWORD:**
```
your-service-principal-secret
```

**AZURE_CREDENTIALS_STAGING:**
```json
{
  "clientId": "00000000-0000-0000-0000-000000000000",
  "clientSecret": "your-secret-here",
  "subscriptionId": "00000000-0000-0000-0000-000000000000",
  "tenantId": "00000000-0000-0000-0000-000000000000"
}
```

### Step 2: Verify Workflows

The following workflows are already configured:
- `.github/workflows/ci-build.yml` - Builds Docker images on PRs
- `.github/workflows/deploy-staging.yml` - Pushes to ACR and deploys to staging
- `.github/workflows/deploy-production.yml` - Pushes to ACR and deploys to production

### Step 3: Test Push

```bash
# Trigger staging deployment
git checkout develop
git push origin develop

# Monitor workflow
# GitHub → Actions → Deploy to Staging
```

---

## Local Development

### Build and Tag Locally

```bash
# Build image
docker build -t digitalstokvel-groupservice:dev -f src/services/DigitalStokvel.GroupService/Dockerfile .

# Tag for ACR
docker tag digitalstokvel-groupservice:dev digitalstokvel.azurecr.io/digitalstokvel-groupservice:dev

# Login to ACR
az acr login --name digitalstokvel

# Push to ACR
docker push digitalstokvel.azurecr.io/digitalstokvel-groupservice:dev
```

### Pull from ACR

```bash
# Login to ACR
az acr login --name digitalstokvel

# Pull image
docker pull digitalstokvel.azurecr.io/digitalstokvel-groupservice:dev

# Run container
docker run -p 8080:8080 digitalstokvel.azurecr.io/digitalstokvel-groupservice:dev
```

---

## Maintenance and Operations

### Image Management

**List repositories:**
```bash
az acr repository list --name digitalstokvel --output table
```

**List tags for a repository:**
```bash
az acr repository show-tags \
  --name digitalstokvel \
  --repository digitalstokvel-groupservice \
  --output table
```

**Delete image tag:**
```bash
az acr repository delete \
  --name digitalstokvel \
  --image digitalstokvel-groupservice:dev \
  --yes
```

### Cleanup Old Images

**Enable retention policy (Premium SKU only):**
```bash
az acr config retention update \
  --registry digitalstokvel \
  --status enabled \
  --days 30 \
  --type UntaggedManifests
```

**Manual purge:**
```bash
# Delete untagged images older than 7 days
az acr run \
  --cmd "acr purge --filter 'digitalstokvel-*:.*' --ago 7d --untagged" \
  --registry digitalstokvel \
  /dev/null
```

### Monitoring

**View metrics:**
```bash
##  Pull/Push requests over Last 24 hours
az monitor metrics list \
  --resource $ACR_ID \
  --metric TotalPullCount \
  --start-time $(date -u -d '24 hours ago' +%Y-%m-%dT%H:%M:%SZ) \
  --end-time $(date -u +%Y-%m-%dT%H:%M:%SZ) \
  --interval PT1H
```

**View logs:**
```bash
# Container registry login events
az monitor activity-log list \
  --resource-id $ACR_ID \
  --start-time $(date -u -d '24 hours ago' +%Y-%m-%dT%H:%M:%SZ) \
  --query "[?category=='ContainerRegistryLoginEvents']"
```

### Security Scanning

**Enable Microsoft Defender for Containers:**
```bash
az security pricing create \
  --name Containers \
  --tier standard
```

**Scan image with Trivy:**
```bash
# Install Trivy
# See: https://aquasecurity.github.io/trivy/

# Scan image in ACR
trivy image digitalstokvel.azurecr.io/digitalstokvel-groupservice:latest
```

---

## Troubleshooting

### Issue: Login Failed

**Error:**
```
Error response from daemon: Get "https://digitalstokvel.azurecr.io/v2/": unauthorized
```

**Solution:**
```bash
# Re-login to ACR
az acr login --name digitalstokvel

# Or use docker login with service principal
docker login digitalstokvel.azurecr.io \
  --username $SP_CLIENT_ID \
  --password $SP_CLIENT_SECRET
```

### Issue: Push Permission Denied

**Error:**
```
denied: requested access to the resource is denied
```

**Solution:**
```bash
# Check role assignments
az role assignment list \
  --scope $ACR_ID \
  --query "[].{Principal:principalName, Role:roleDefinitionName}" \
  --output table

# Assign AcrPush role
az role assignment create \
  --assignee $PRINCIPAL_ID \
  --role AcrPush \
  --scope $ACR_ID
```

### Issue: Image Not Found

**Error:**
```
Error: manifest for digitalstokvel.azurecr.io/digitalstokvel-groupservice:v1.0.0 not found
```

**Solution:**
```bash
# List available tags
az acr repository show-tags \
  --name digitalstokvel \
  --repository digitalstokvel-groupservice
```

---

## Cost Optimization

### Estimated Costs (South Africa North Region)

| SKU | Storage | Price/Month | Annual Cost |
|-----|---------|-------------|-------------|
| **Basic** | 10 GB | ~$5 | ~$60 |
| **Standard** | 100 GB | ~$20 | ~$240 |
| **Premium** | 500 GB | ~$50 | ~$600 |

**Additional Costs:**
- **Geo-replication:** +$50/region/month (Premium only)
- **Storage overage:** $0.10/GB/month
- **Bandwidth:** Outbound data transfer charges

### Cost Saving Tips

1. **Enable retention policy** to auto-delete old images
2. **Compress Docker layers** in multi-stage builds
3. **Use Basic SKU** for development environments
4. **Schedule purge jobs** to clean up unused images
5. **Monitor storage usage** and adjust SKU if needed

---

## Security Best Practices

### Access Control

✅ **DO:**
- Use service principals for CI/CD workflows
- Use managed identities for Azure services (AKS, App Service)
- Assign least privilege roles (AcrPull for reading, AcrPush for writing)
- Enable Microsoft Defender for Containers
- Enable Azure AD authentication

❌ **DON'T:**
- Use admin account in production
- Share credentials across teams
- Store credentials in code or config files
- Grant Owner role to service principals
- Allow anonymous pull access

### Network Security

**Enable private endpoint (Premium SKU):**
```bash
# Create private endpoint
az network private-endpoint create \
  --name pe-digitalstokvel-acr \
  --resource-group rg-digitalstokvel-shared \
  --vnet-name vnet-digitalstokvel \
  --subnet snet-privatelink \
  --private-connection-resource-id $ACR_ID \
  --group-id registry \
  --connection-name acr-connection
```

**Configure firewall rules:**
```bash
# Allow specific IP ranges
az acr network-rule add \
  --name digitalstokvel \
  --ip-address 197.242.0.0/16  # Example: South African IP range
```

---

## Next Steps

After setting up ACR, proceed to:

1. **Task 0.2.5:** Configure deployment pipelines for staging and production
2. **Task 1.3.1:** Provision Kubernetes cluster (AKS) for staging
3. **Task 1.3.6:** Set up monitoring infrastructure (Prometheus/Grafana)

---

## References

- [Azure Container Registry Documentation](https://docs.microsoft.com/en-us/azure/container-registry/)
- [ACR Best Practices](https://docs.microsoft.com/en-us/azure/container-registry/container-registry-best-practices)
- [Docker Image Naming Conventions](https://docs.docker.com/engine/reference/commandline/tag/)
- [GitHub Actions with ACR](https://docs.microsoft.com/en-us/azure/container-registry/container-registry-github-action)

---

**Document Status:** Active  
**Last Updated:** March 24, 2026  
**Next Review:** After ACR provisioning

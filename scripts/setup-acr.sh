#!/bin/bash
# Azure Container Registry Setup Script
# This script provisions ACR for Digital Stokvel Banking platform

set -e  # Exit on error

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
NC='\033[0m' # No Color

# Configuration
RESOURCE_GROUP="rg-digitalstokvel-shared"
LOCATION="southafricanorth"
REGISTRY_NAME="digitalstokvel"
SKU="Premium"
ENABLE_GEOREPLICATION=true
SECONDARY_LOCATION="westeurope"

echo -e "${GREEN}╔══════════════════════════════════════════════════════╗${NC}"
echo -e "${GREEN}║   Digital Stokvel - ACR Setup Script                ║${NC}"
echo -e "${GREEN}╚══════════════════════════════════════════════════════╝${NC}"
echo ""

# Check if Azure CLI is installed
if ! command -v az &> /dev/null; then
    echo -e "${RED}✗ Azure CLI is not installed${NC}"
    echo "Please install Azure CLI: https://docs.microsoft.com/en-us/cli/azure/install-azure-cli"
    exit 1
fi

echo -e "${GREEN}✓ Azure CLI found${NC}"

# Check if logged in
echo ""
echo "Checking Azure login status..."
if ! az account show &> /dev/null; then
    echo -e "${YELLOW}⚠ Not logged in to Azure${NC}"
    echo "Please login:"
    az login
fi

# Display current subscription
SUBSCRIPTION_NAME=$(az account show --query name -o tsv)
SUBSCRIPTION_ID=$(az account show --query id -o tsv)
echo -e "${GREEN}✓ Logged in to Azure${NC}"
echo "  Subscription: $SUBSCRIPTION_NAME"
echo "  ID: $SUBSCRIPTION_ID"

# Confirm before proceeding
echo ""
read -p "Continue with ACR setup? (y/N) " -n 1 -r
echo
if [[ ! $REPLY =~ ^[Yy]$ ]]; then
    echo "Setup cancelled"
    exit 0
fi

# Create resource group
echo ""
echo "Creating resource group..."
if az group show --name $RESOURCE_GROUP &> /dev/null; then
    echo -e "${YELLOW}⚠ Resource group already exists${NC}"
else
    az group create \
        --name $RESOURCE_GROUP \
        --location $LOCATION \
        --tags Environment=Production Project=DigitalStokvel
    echo -e "${GREEN}✓ Resource group created${NC}"
fi

# Create ACR
echo ""
echo "Creating Azure Container Registry..."
if az acr show --name $REGISTRY_NAME &> /dev/null; then
    echo -e "${YELLOW}⚠ ACR already exists${NC}"
else
    az acr create \
        --name $REGISTRY_NAME \
        --resource-group $RESOURCE_GROUP \
        --location $LOCATION \
        --sku $SKU \
        --tags Environment=Production Project=DigitalStokvel
    echo -e "${GREEN}✓ ACR created${NC}"
fi

# Enable geo-replication
if [ "$ENABLE_GEOREPLICATION" = true ] && [ "$SKU" = "Premium" ]; then
    echo ""
    echo "Enabling geo-replication..."
    if az acr replication show --registry $REGISTRY_NAME --location $SECONDARY_LOCATION &> /dev/null; then
        echo -e "${YELLOW}⚠ Geo-replication already enabled${NC}"
    else
        az acr replication create \
            --registry $REGISTRY_NAME \
            --location $SECONDARY_LOCATION
        echo -e "${GREEN}✓ Geo-replication enabled${NC}"
    fi
fi

# Get ACR login server
LOGIN_SERVER=$(az acr show --name $REGISTRY_NAME --query loginServer -o tsv)

# Create service principal for CI/CD
echo ""
echo "Creating service principal for CI/CD..."
ACR_ID=$(az acr show --name $REGISTRY_NAME --query id -o tsv)

SP_NAME="digitalstokvel-cicd"
SP_JSON=$(az ad sp create-for-rbac \
    --name $SP_NAME \
    --role AcrPush \
    --scope $ACR_ID \
    --sdk-auth 2>/dev/null || true)

if [ -n "$SP_JSON" ]; then
    echo -e "${GREEN}✓ Service principal created${NC}"
    
    # Extract credentials
    CLIENT_ID=$(echo $SP_JSON | jq -r .clientId)
    CLIENT_SECRET=$(echo $SP_JSON | jq -r .clientSecret)
    
    echo ""
    echo -e "${GREEN}╔══════════════════════════════════════════════════════╗${NC}"
    echo -e "${GREEN}║   GitHub Secrets Configuration                       ║${NC}"
    echo -e "${GREEN}╚══════════════════════════════════════════════════════╝${NC}"
    echo ""
    echo "Add these secrets to GitHub repository:"
    echo ""
    echo "REGISTRY_USERNAME:"
    echo "$CLIENT_ID"
    echo ""
    echo "REGISTRY_PASSWORD:"
    echo "$CLIENT_SECRET"
    echo ""
    echo "AZURE_CREDENTIALS_STAGING:"
    echo "$SP_JSON"
    echo ""
    echo "AZURE_CREDENTIALS_PRODUCTION:"
    echo "$SP_JSON"
    echo ""
else
    echo -e "${YELLOW}⚠ Service principal already exists or creation failed${NC}"
fi

# Summary
echo ""
echo -e "${GREEN}╔══════════════════════════════════════════════════════╗${NC}"
echo -e "${GREEN}║   ACR Setup Complete                                 ║${NC}"
echo -e "${GREEN}╚══════════════════════════════════════════════════════╝${NC}"
echo ""
echo "Registry: $REGISTRY_NAME"
echo "Login Server: $LOGIN_SERVER"
echo "SKU: $SKU"
echo "Geo-replication: $ENABLE_GEOREPLICATION"
echo ""
echo "Next steps:"
echo "1. Add secrets to GitHub repository (see above)"
echo "2. Login to ACR: az acr login --name $REGISTRY_NAME"
echo "3. Build and push images: ./scripts/build-and-push.sh"
echo ""

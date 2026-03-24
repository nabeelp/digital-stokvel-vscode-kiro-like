#!/bin/bash
#
# GitHub Secrets Setup Script for Digital Stokvel Banking Platform
# This script helps configure required secrets for CI/CD pipelines
#
# Prerequisites:
# - GitHub CLI (gh) installed and authenticated
# - Azure CLI (az) installed and authenticated
# - Appropriate permissions on the GitHub repository and Azure subscriptions
#
# Usage:
#   ./setup-github-secrets.sh
#

set -euo pipefail

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

# Configuration
REPO_OWNER="${GITHUB_REPO_OWNER:-nabeelp}"
REPO_NAME="${GITHUB_REPO_NAME:-digital-stokvel-vscode-kiro-like}"
ACR_NAME="${ACR_NAME:-digitalstokvel}"
STAGING_RG="${STAGING_RG:-rg-digitalstokvel-staging}"
STAGING_CLUSTER="${STAGING_CLUSTER:-aks-digitalstokvel-staging}"
PRODUCTION_RG="${PRODUCTION_RG:-rg-digitalstokvel-production}"
PRODUCTION_CLUSTER="${PRODUCTION_CLUSTER:-aks-digitalstokvel-production}"

# Functions
print_header() {
    echo -e "${BLUE}================================${NC}"
    echo -e "${BLUE}$1${NC}"
    echo -e "${BLUE}================================${NC}"
    echo ""
}

print_success() {
    echo -e "${GREEN}✅ $1${NC}"
}

print_error() {
    echo -e "${RED}❌ $1${NC}"
}

print_warning() {
    echo -e "${YELLOW}⚠️  $1${NC}"
}

print_info() {
    echo -e "${BLUE}ℹ️  $1${NC}"
}

check_prerequisites() {
    print_header "Checking Prerequisites"
    
    # Check GitHub CLI
    if ! command -v gh &> /dev/null; then
        print_error "GitHub CLI (gh) is not installed"
        echo "Install from: https://cli.github.com/"
        exit 1
    fi
    print_success "GitHub CLI is installed"
    
    # Check Azure CLI
    if ! command -v az &> /dev/null; then
        print_error "Azure CLI (az) is not installed"
        echo "Install from: https://docs.microsoft.com/en-us/cli/azure/install-azure-cli"
        exit 1
    fi
    print_success "Azure CLI is installed"
    
    # Check GitHub authentication
    if ! gh auth status &> /dev/null; then
        print_error "GitHub CLI is not authenticated"
        echo "Run: gh auth login"
        exit 1
    fi
    print_success "GitHub CLI is authenticated"
    
    # Check Azure authentication
    if ! az account show &> /dev/null; then
        print_error "Azure CLI is not authenticated"
        echo "Run: az login"
        exit 1
    fi
    print_success "Azure CLI is authenticated"
    
    echo ""
}

display_azure_subscription() {
    print_header "Azure Subscription Information"
    
    SUBSCRIPTION_ID=$(az account show --query id -o tsv)
    SUBSCRIPTION_NAME=$(az account show --query name -o tsv)
    TENANT_ID=$(az account show --query tenantId -o tsv)
    
    echo -e "Subscription ID:   ${GREEN}$SUBSCRIPTION_ID${NC}"
    echo -e "Subscription Name: ${GREEN}$SUBSCRIPTION_NAME${NC}"
    echo -e "Tenant ID:         ${GREEN}$TENANT_ID${NC}"
    echo ""
    
    read -p "Is this the correct subscription? (y/n): " -r
    if [[ ! $REPLY =~ ^[Yy]$ ]]; then
        print_error "Please switch to the correct subscription with: az account set --subscription <subscription-id>"
        exit 1
    fi
    echo ""
}

create_acr_service_principal() {
    print_header "Creating ACR Service Principal"
    
    print_info "This service principal will be used for pushing Docker images to ACR"
    
    # Check if ACR exists
    if ! az acr show --name "$ACR_NAME" &> /dev/null; then
        print_error "Azure Container Registry '$ACR_NAME' not found"
        print_info "Please run scripts/setup-acr.sh or scripts/setup-acr.ps1 first"
        exit 1
    fi
    
    SUBSCRIPTION_ID=$(az account show --query id -o tsv)
    ACR_RESOURCE_ID="/subscriptions/$SUBSCRIPTION_ID/resourceGroups/rg-digitalstokvel-shared/providers/Microsoft.ContainerRegistry/registries/$ACR_NAME"
    
    print_info "Creating service principal with acrpush role..."
    
    # Create service principal
    SP_OUTPUT=$(az ad sp create-for-rbac \
        --name "sp-digitalstokvel-acr-github" \
        --role acrpush \
        --scopes "$ACR_RESOURCE_ID" \
        --output json)
    
    ACR_USERNAME=$(echo "$SP_OUTPUT" | jq -r '.appId')
    ACR_PASSWORD=$(echo "$SP_OUTPUT" | jq -r '.password')
    
    print_success "Service principal created"
    echo ""
    
    # Set GitHub secrets
    print_info "Setting REGISTRY_USERNAME secret..."
    echo "$ACR_USERNAME" | gh secret set REGISTRY_USERNAME --repo="$REPO_OWNER/$REPO_NAME"
    print_success "REGISTRY_USERNAME secret set"
    
    print_info "Setting REGISTRY_PASSWORD secret..."
    echo "$ACR_PASSWORD" | gh secret set REGISTRY_PASSWORD --repo="$REPO_OWNER/$REPO_NAME"
    print_success "REGISTRY_PASSWORD secret set"
    
    echo ""
}

create_staging_service_principal() {
    print_header "Creating Staging Environment Service Principal"
    
    print_info "This service principal will be used for deploying to the staging environment"
    
    SUBSCRIPTION_ID=$(az account show --query id -o tsv)
    TENANT_ID=$(az account show --query tenantId -o tsv)
    
    # Check if resource group exists
    if ! az group show --name "$STAGING_RG" &> /dev/null; then
        print_warning "Resource group '$STAGING_RG' not found. It will need to be created before deployment."
    fi
    
    print_info "Creating service principal with contributor role for staging..."
    
    # Create service principal
    SP_OUTPUT=$(az ad sp create-for-rbac \
        --name "sp-digitalstokvel-staging-github" \
        --role contributor \
        --scopes "/subscriptions/$SUBSCRIPTION_ID/resourceGroups/$STAGING_RG" \
        --sdk-auth)
    
    print_success "Service principal created"
    echo ""
    
    # Set GitHub secrets
    print_info "Setting AZURE_CREDENTIALS_STAGING secret..."
    echo "$SP_OUTPUT" | gh secret set AZURE_CREDENTIALS_STAGING --repo="$REPO_OWNER/$REPO_NAME"
    print_success "AZURE_CREDENTIALS_STAGING secret set"
    
    print_info "Setting STAGING_RESOURCE_GROUP secret..."
    echo "$STAGING_RG" | gh secret set STAGING_RESOURCE_GROUP --repo="$REPO_OWNER/$REPO_NAME"
    print_success "STAGING_RESOURCE_GROUP secret set"
    
    print_info "Setting STAGING_CLUSTER_NAME secret..."
    echo "$STAGING_CLUSTER" | gh secret set STAGING_CLUSTER_NAME --repo="$REPO_OWNER/$REPO_NAME"
    print_success "STAGING_CLUSTER_NAME secret set"
    
    echo ""
}

create_production_service_principal() {
    print_header "Creating Production Environment Service Principal"
    
    print_info "This service principal will be used for deploying to the production environment"
    
    print_warning "Production deployments have elevated privileges. Ensure proper access controls are in place."
    read -p "Continue with production service principal creation? (y/n): " -r
    if [[ ! $REPLY =~ ^[Yy]$ ]]; then
        print_warning "Skipping production service principal creation"
        return
    fi
    echo ""
    
    SUBSCRIPTION_ID=$(az account show --query id -o tsv)
    TENANT_ID=$(az account show --query tenantId -o tsv)
    
    # Check if resource group exists
    if ! az group show --name "$PRODUCTION_RG" &> /dev/null; then
        print_warning "Resource group '$PRODUCTION_RG' not found. It will need to be created before deployment."
    fi
    
    print_info "Creating service principal with contributor role for production..."
    
    # Create service principal
    SP_OUTPUT=$(az ad sp create-for-rbac \
        --name "sp-digitalstokvel-production-github" \
        --role contributor \
        --scopes "/subscriptions/$SUBSCRIPTION_ID/resourceGroups/$PRODUCTION_RG" \
        --sdk-auth)
    
    print_success "Service principal created"
    echo ""
    
    # Set GitHub secrets
    print_info "Setting AZURE_CREDENTIALS_PRODUCTION secret..."
    echo "$SP_OUTPUT" | gh secret set AZURE_CREDENTIALS_PRODUCTION --repo="$REPO_OWNER/$REPO_NAME"
    print_success "AZURE_CREDENTIALS_PRODUCTION secret set"
    
    print_info "Setting PRODUCTION_RESOURCE_GROUP secret..."
    echo "$PRODUCTION_RG" | gh secret set PRODUCTION_RESOURCE_GROUP --repo="$REPO_OWNER/$REPO_NAME"
    print_success "PRODUCTION_RESOURCE_GROUP secret set"
    
    print_info "Setting PRODUCTION_CLUSTER_NAME secret..."
    echo "$PRODUCTION_CLUSTER" | gh secret set PRODUCTION_CLUSTER_NAME --repo="$REPO_OWNER/$REPO_NAME"
    print_success "PRODUCTION_CLUSTER_NAME secret set"
    
    echo ""
}

verify_secrets() {
    print_header "Verifying GitHub Secrets"
    
    print_info "Listing configured secrets..."
    gh secret list --repo="$REPO_OWNER/$REPO_NAME"
    echo ""
    
    print_success "All secrets have been configured successfully!"
    echo ""
}

display_summary() {
    print_header "Setup Summary"
    
    cat << EOF
${GREEN}✅ GitHub Secrets Configuration Complete${NC}

The following secrets have been configured for the repository:
  ${BLUE}$REPO_OWNER/$REPO_NAME${NC}

${YELLOW}Container Registry:${NC}
  - REGISTRY_USERNAME
  - REGISTRY_PASSWORD

${YELLOW}Staging Environment:${NC}
  - AZURE_CREDENTIALS_STAGING
  - STAGING_RESOURCE_GROUP
  - STAGING_CLUSTER_NAME

${YELLOW}Production Environment:${NC}
  - AZURE_CREDENTIALS_PRODUCTION
  - PRODUCTION_RESOURCE_GROUP
  - PRODUCTION_CLUSTER_NAME

${BLUE}Next Steps:${NC}
  1. Verify Azure infrastructure is provisioned:
     - ACR: $ACR_NAME
     - Staging AKS: $STAGING_CLUSTER in $STAGING_RG
     - Production AKS: $PRODUCTION_CLUSTER in $PRODUCTION_RG
  
  2. Test staging deployment:
     git checkout develop
     git push origin develop
     # Monitor: https://github.com/$REPO_OWNER/$REPO_NAME/actions
  
  3. Test production deployment:
     git tag -a v1.0.0 -m "Release v1.0.0"
     git push origin v1.0.0
     # Create release in GitHub: https://github.com/$REPO_OWNER/$REPO_NAME/releases
  
  4. Review deployment guide:
     docs/DEPLOYMENT_GUIDE.md

${YELLOW}Security Reminders:${NC}
  - Store service principal credentials securely
  - Rotate credentials every 90 days
  - Review service principal permissions quarterly
  - Enable Azure AD Conditional Access for production
  - Monitor service principal activity in Azure AD logs

EOF
}

main() {
    print_header "Digital Stokvel Banking - GitHub Secrets Setup"
    echo "This script will configure GitHub Secrets for CI/CD pipelines"
    echo ""
    
    check_prerequisites
    display_azure_subscription
    
    # Create service principals and set secrets
    create_acr_service_principal
    create_staging_service_principal
    create_production_service_principal
    
    # Verify configuration
    verify_secrets
    display_summary
    
    print_success "Setup completed successfully! 🎉"
}

# Run main function
main

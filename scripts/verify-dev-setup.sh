#!/bin/bash
#
# Development Environment Verification Script
# Digital Stokvel Banking Platform
#
# This script verifies that all required tools and dependencies are properly installed
#

set -e

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

# Counters
PASSED=0
FAILED=0
WARNINGS=0

# Functions
print_header() {
    echo -e "${BLUE}================================${NC}"
    echo -e "${BLUE}$1${NC}"
    echo -e "${BLUE}================================${NC}"
    echo ""
}

print_success() {
    echo -e "${GREEN}✅ $1${NC}"
    ((PASSED++))
}

print_error() {
    echo -e "${RED}❌ $1${NC}"
    ((FAILED++))
}

print_warning() {
    echo -e "${YELLOW}⚠️  $1${NC}"
    ((WARNINGS++))
}

print_info() {
    echo -e "${BLUE}ℹ️  $1${NC}"
}

check_command() {
    if command -v "$1" &> /dev/null; then
        return 0
    else
        return 1
    fi
}

# Main verification
print_header "Digital Stokvel Development Environment Verification"

# Check .NET SDK
echo "Checking .NET SDK..."
if check_command dotnet; then
    DOTNET_VERSION=$(dotnet --version)
    print_success ".NET SDK installed: $DOTNET_VERSION"
    
    # Check if .NET 10
    if [[ $DOTNET_VERSION == 10.* ]]; then
        print_success ".NET 10 SDK detected"
    else
        print_warning "Expected .NET 10, but found $DOTNET_VERSION"
    fi
    
    # List all SDKs
    echo ""
    print_info "Installed SDKs:"
    dotnet --list-sdks | sed 's/^/  /'
    echo ""
else
    print_error ".NET SDK not found"
    echo "  Install from: https://dotnet.microsoft.com/download/dotnet/10.0"
    echo ""
fi

# Check Git
echo "Checking Git..."
if check_command git; then
    GIT_VERSION=$(git --version)
    print_success "Git installed: $GIT_VERSION"
    
    # Check Git config
    GIT_USER=$(git config --global user.name || echo "")
    GIT_EMAIL=$(git config --global user.email || echo "")
    
    if [[ -z "$GIT_USER" || -z "$GIT_EMAIL" ]]; then
        print_warning "Git user not configured. Run:"
        echo "  git config --global user.name \"Your Name\""
        echo "  git config --global user.email \"your.email@example.com\""
    else
        print_success "Git configured as: $GIT_USER <$GIT_EMAIL>"
    fi
else
    print_error "Git not found"
    echo "  Install from: https://git-scm.com/downloads"
fi
echo ""

# Check Docker
echo "Checking Docker..."
if check_command docker; then
    DOCKER_VERSION=$(docker --version)
    print_success "Docker installed: $DOCKER_VERSION"
    
    # Check Docker daemon
    if docker ps &> /dev/null; then
        print_success "Docker daemon is running"
    else
        print_warning "Docker daemon is not running. Start Docker Desktop."
    fi
else
    print_warning "Docker not found (optional, but recommended)"
    echo "  Install from: https://www.docker.com/products/docker-desktop"
fi
echo ""

# Check Azure CLI (optional)
echo "Checking Azure CLI..."
if check_command az; then
    AZ_VERSION=$(az --version | head -n 1)
    print_success "Azure CLI installed: $AZ_VERSION"
else
    print_info "Azure CLI not found (optional, for Azure deployments)"
    echo "  Install from: https://docs.microsoft.com/en-us/cli/azure/install-azure-cli"
fi
echo ""

# Check GitHub CLI (optional)
echo "Checking GitHub CLI..."
if check_command gh; then
    GH_VERSION=$(gh --version | head -n 1)
    print_success "GitHub CLI installed: $GH_VERSION"
else
    print_info "GitHub CLI not found (optional, for GitHub operations)"
    echo "  Install from: https://cli.github.com/"
fi
echo ""

# Check if in repository
print_header "Repository Status"

if [[ -d ".git" ]]; then 
    print_success "In Git repository"
    
    # Check current branch
    CURRENT_BRANCH=$(git branch --show-current)
    print_info "Current branch: $CURRENT_BRANCH"
    
    # Check for uncommitted changes
    if [[ -n $(git status --porcelain) ]]; then
        print_info "Uncommitted changes detected"
    else
        print_success "Working directory clean"
    fi
else
    print_warning "Not in a Git repository. Run 'git clone <repo-url>' first."
fi
echo ""

# Check dependencies
print_header "Project Dependencies"

if [[ -f "DigitalStokvel.slnx" ]]; then
    print_success "Solution file found"
    
    # Restore dependencies
    echo "Restoring dependencies..."
    if dotnet restore > /dev/null 2>&1; then
        print_success "Dependencies restored successfully"
    else
        print_error "Dependency restore failed. Run 'dotnet restore' for details."
    fi
else
    print_warning "Solution file not found. Ensure you're in the repository root."
fi
echo ""

# Build solution
print_header "Build Verification"

if [[ -f "DigitalStokvel.slnx" ]]; then
    echo "Building solution (Debug configuration)..."
    
    BUILD_OUTPUT=$(dotnet build --configuration Debug 2>&1)
    BUILD_EXIT_CODE=$?
    
    if [[ $BUILD_EXIT_CODE -eq 0 ]]; then
        print_success "Solution builds successfully"
        
        # Count warnings
        WARNING_COUNT=$(echo "$BUILD_OUTPUT" | grep -c "warning" || true)
        if [[ $WARNING_COUNT -gt 0 ]]; then
            print_warning "$WARNING_COUNT build warning(s) found"
        fi
    else
        print_error "Build failed. Run 'dotnet build' for details."
    fi
else
    print_warning "Skipping build (solution file not found)"
fi
echo ""

# Run tests
print_header "Test Verification"

if [[ -f "DigitalStokvel.slnx" ]]; then
    echo "Running tests..."
    
    TEST_OUTPUT=$(dotnet test --no-build --verbosity quiet 2>&1)
    TEST_EXIT_CODE=$?
    
    if [[ $TEST_EXIT_CODE -eq 0 ]]; then
        print_success "All tests passing"
        
        # Extract test summary
        TEST_SUMMARY=$(echo "$TEST_OUTPUT" | grep "Test summary:" || echo "")
        if [[ -n "$TEST_SUMMARY" ]]; then
            print_info "$TEST_SUMMARY"
        fi
    else
        print_error "Some tests failing. Run 'dotnet test' for details."
    fi
else
    print_warning "Skipping tests (solution file not found)"
fi
echo ""

# Summary
print_header "Verification Summary"

echo -e "${GREEN}Passed:   $PASSED${NC}"
if [[ $FAILED -gt 0 ]]; then
    echo -e "${RED}Failed:   $FAILED${NC}"
fi
if [[ $WARNINGS -gt 0 ]]; then
    echo -e "${YELLOW}Warnings: $WARNINGS${NC}"
fi
echo ""

# Final status
if [[ $FAILED -eq 0 ]]; then
    print_success "Development environment is ready!"
    echo ""
    echo "Next steps:"
    echo "  1. Start local databases: docker-compose up -d"
    echo "  2. Run API Gateway: dotnet run --project src/gateways/DigitalStokvel.ApiGateway"
    echo "  3. Open Swagger UI: https://localhost:5001/swagger"
    echo ""
    exit 0
else
    print_error "Development environment has issues. Please resolve the errors above."
    exit 1
fi

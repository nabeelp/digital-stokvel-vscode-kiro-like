# Docker Build Guide - Digital Stokvel Banking

**Version:** 1.0  
**Last Updated:** March 24, 2026  
**Status:** Active

---

## Overview

This document provides instructions for building and running Docker containers for all Digital Stokvel Banking microservices and gateways. Each service has an optimized multi-stage Dockerfile using .NET 10 Alpine Linux images.

---

## Prerequisites

- **Docker Desktop** 4.0+ installed and running
- **Docker Compose** 2.0+ (included with Docker Desktop)
- **.NET 10 SDK** installed locally (for development)
- **Minimum 8GB RAM** allocated to Docker
- **20GB free disk space** for images

---

## Architecture

### Services

The platform consists of 8 containerized services:

| Service | Container Name | Port | Dockerfile Location |
|---------|---------------|------|---------------------|
| **API Gateway** | digitalstokvel-apigateway | 8080 | `src/gateways/DigitalStokvel.ApiGateway/Dockerfile` |
| **USSD Gateway** | digitalstokvel-ussdgateway | 8080 | `src/gateways/DigitalStokvel.UssdGateway/Dockerfile` |
| **Group Service** | digitalstokvel-groupservice | 8080 | `src/services/DigitalStokvel.GroupService/Dockerfile` |
| **Contribution Service** | digitalstokvel-contributionservice | 8080 | `src/services/DigitalStokvel.ContributionService/Dockerfile` |
| **Payout Service** | digitalstokvel-payoutservice | 8080 | `src/services/DigitalStokvel.PayoutService/Dockerfile` |
| **Governance Service** | digitalstokvel-governanceservice | 8080 | `src/services/DigitalStokvel.GovernanceService/Dockerfile` |
| **Notification Service** | digitalstokvel-notificationservice | 8080 | `src/services/DigitalStokvel.NotificationService/Dockerfile` |
| **Credit Profile Service** | digitalstokvel-creditprofileservice | 8080 | `src/services/DigitalStokvel.CreditProfileService/Dockerfile` |

---

## Dockerfile Structure

All services use the same optimized multi-stage build pattern:

### Stage 1: Build (SDK Image)
- Base: `mcr.microsoft.com/dotnet/sdk:10.0-alpine`
- Copies solution files and restores NuGet packages
- Builds and publishes the application in Release mode
- Output: `/app/publish` directory

### Stage 2: Runtime (Minimal Image)
- Base: `mcr.microsoft.com/dotnet/aspnet:10.0-alpine`
- Creates non-root user `appuser:appuser` (UID/GID 1000)
- Copies published artifacts from build stage
- Runs as non-root user (security best practice)
- Exposes port 8080
- Includes health check endpoint

### Key Features

- **Multi-stage build:** Minimizes final image size (SDK not included)
- **Alpine Linux:** Smaller footprint (~200MB vs ~500MB for Debian)
- **Non-root user:** Enhanced security posture
- **Layer caching:** Efficient rebuilds by copying project files before source code
- **Health checks:** Built-in `/health` endpoint monitoring
- **Globalization support:** `DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=false` for multilingual support

---

## Building Docker Images

### Build Individual Service

```bash
# Build API Gateway
docker build -t digitalstokvel-apigateway:latest -f src/gateways/DigitalStokvel.ApiGateway/Dockerfile .

# Build Group Service
docker build -t digitalstokvel-groupservice:latest -f src/services/DigitalStokvel.GroupService/Dockerfile .

# Build Contribution Service
docker build -t digitalstokvel-contributionservice:latest -f src/services/DigitalStokvel.ContributionService/Dockerfile .
```

### Build All Services (Script)

**PowerShell:**
```powershell
# Build all services in parallel
$services = @(
    @{Name="apigateway"; Path="src/gateways/DigitalStokvel.ApiGateway"},
    @{Name="ussdgateway"; Path="src/gateways/DigitalStokvel.UssdGateway"},
    @{Name="groupservice"; Path="src/services/DigitalStokvel.GroupService"},
    @{Name="contributionservice"; Path="src/services/DigitalStokvel.ContributionService"},
    @{Name="payoutservice"; Path="src/services/DigitalStokvel.PayoutService"},
    @{Name="governanceservice"; Path="src/services/DigitalStokvel.GovernanceService"},
    @{Name="notificationservice"; Path="src/services/DigitalStokvel.NotificationService"},
    @{Name="creditprofileservice"; Path="src/services/DigitalStokvel.CreditProfileService"}
)

foreach ($svc in $services) {
    Write-Host "Building digitalstokvel-$($svc.Name)..." -ForegroundColor Cyan
    docker build -t "digitalstokvel-$($svc.Name):latest" -f "$($svc.Path)/Dockerfile" .
    if ($LASTEXITCODE -eq 0) {
        Write-Host "✓ digitalstokvel-$($svc.Name) built successfully" -ForegroundColor Green
    } else {
        Write-Host "✗ Failed to build digitalstokvel-$($svc.Name)" -ForegroundColor Red
    }
}
```

**Bash:**
```bash
#!/bin/bash
# Build all services

services=(
  "apigateway:src/gateways/DigitalStokvel.ApiGateway"
  "ussdgateway:src/gateways/DigitalStokvel.UssdGateway"
  "groupservice:src/services/DigitalStokvel.GroupService"
  "contributionservice:src/services/DigitalStokvel.ContributionService"
  "payoutservice:src/services/DigitalStokvel.PayoutService"
  "governanceservice:src/services/DigitalStokvel.GovernanceService"
  "notificationservice:src/services/DigitalStokvel.NotificationService"
  "creditprofileservice:src/services/DigitalStokvel.CreditProfileService"
)

for svc in "${services[@]}"; do
  IFS=':' read -r name path <<< "$svc"
  echo "Building digitalstokvel-$name..."
  docker build -t "digitalstokvel-$name:latest" -f "$path/Dockerfile" .
  if [ $? -eq 0 ]; then
    echo "✓ digitalstokvel-$name built successfully"
  else
    echo "✗ Failed to build digitalstokvel-$name"
  fi
done
```

---

## Running Containers

### Run Individual Service

```bash
# Run API Gateway
docker run -d \
  --name apigateway \
  -p 8080:8080 \
  -e ASPNETCORE_ENVIRONMENT=Development \
  digitalstokvel-apigateway:latest

# Check health
curl http://localhost:8080/health
```

### Run with Docker Compose

The `docker-compose.yml` file orchestrates all services with dependencies:

```bash
# Start all services
docker-compose up -d

# View logs
docker-compose logs -f

# Stop all services
docker-compose down

# Rebuild and restart
docker-compose up -d --build
```

---

## Image Tagging Strategy

### Local Development
```bash
docker build -t digitalstokvel-{service}:dev .
```

### CI/CD Pipeline (GitHub Actions)

**Develop Branch → Staging:**
```bash
docker tag digitalstokvel-{service}:latest digitalstokvel.azurecr.io/{service}:staging-latest
docker tag digitalstokvel-{service}:latest digitalstokvel.azurecr.io/{service}:develop-${GITHUB_SHA::8}
```

**Main Branch → Production:**
```bash
docker tag digitalstokvel-{service}:latest digitalstokvel.azurecr.io/{service}:${VERSION}
docker tag digitalstokvel-{service}:latest digitalstokvel.azurecr.io/{service}:latest
```

### Tag Formats

| Environment | Tag Format | Example |
|-------------|------------|---------|
| **Development** | `dev` | `digitalstokvel-groupservice:dev` |
| **Staging** | `staging-latest`, `develop-{sha}` | `digitalstokvel.azurecr.io/groupservice:staging-latest` |
| **Production** | `v{semver}`, `latest` | `digitalstokvel.azurecr.io/groupservice:v1.0.0` |

---

## Debugging Docker Builds

### View Build Logs

```bash
# Build with verbose output
docker build --progress=plain -t digitalstokvel-apigateway:test -f src/gateways/DigitalStokvel.ApiGateway/Dockerfile .

# Build without cache (force rebuild)
docker build --no-cache -t digitalstokvel-apigateway:test -f src/gateways/DigitalStokvel.ApiGateway/Dockerfile .
```

### Inspect Build Stages

```bash
# Build and tag intermediate stage
docker build --target build -t digitalstokvel-apigateway:build-stage -f src/gateways/DigitalStokvel.ApiGateway/Dockerfile .

# Run build stage interactively
docker run -it --rm digitalstokvel-apigateway:build-stage /bin/sh
```

### Common Issues

#### 1. **Build Context Too Large**
**Symptom:** "Sending build context to Docker daemon... X GB"

**Solution:** Check `.dockerignore` file excludes unnecessary directories:
```bash
# View what's being sent to Docker
docker build --no-cache --progress=plain 2>&1 | grep "transferring context"
```

#### 2. **NuGet Restore Fails**
**Symptom:** "NU1301: Unable to load service index"

**Solution:** Check Docker DNS settings or use NuGet mirror:
```dockerfile
RUN dotnet nuget add source https://api.nuget.org/v3/index.json -n nuget.org
RUN dotnet restore
```

#### 3. **Out of Memory During Build**
**Symptom:** Build hangs or crashes

**Solution:** Increase Docker memory limit (Docker Desktop → Settings → Resources → Memory → 8GB+)

#### 4. **Permission Denied in Container**
**Symptom:** "System.UnauthorizedAccessException"

**Solution:** Check file permissions and ownership:
```dockerfile
RUN chown -R appuser:appuser /app
USER appuser
```

---

## Optimization Tips

### 1. **Use BuildKit**
```bash
# Enable BuildKit for better caching
export DOCKER_BUILDKIT=1
docker build -t digitalstokvel-apigateway:latest -f src/gateways/DigitalStokvel.ApiGateway/Dockerfile .
```

### 2. **Multi-platform Builds**
```bash
# Build for ARM64 (Apple Silicon) and AMD64
docker buildx build --platform linux/amd64,linux/arm64 -t digitalstokvel-apigateway:latest -f src/gateways/DigitalStokvel.ApiGateway/Dockerfile .
```

### 3. **Layer Caching**
Order Dockerfile instructions from least to most frequently changed:
1. Solution/project files → 2. NuGet restore → 3. Source code → 4. Build/publish

### 4. **Reduce Image Size**
- Use Alpine Linux base images (already done)
- Remove unnecessary files in publish step
- Use `--self-contained false` for .NET publish (default)
- Enable trimming for smaller runtime (AOT in future)

---

## CI/CD Integration

The GitHub Actions workflows in `.github/workflows/` use these Dockerfiles:

### 1. **CI Build Workflow** (`ci-build.yml`)
- Triggered on every push/PR
- Builds all 8 Docker images
- Runs Trivy security scans
- Pushes images to GitHub Container Registry (dev)

### 2. **Staging Deployment** (`deploy-staging.yml`)
- Triggered on push to `develop` branch
- Builds and tags with `staging-latest`
- Pushes to Azure Container Registry
- Deploys to AKS staging cluster

### 3. **Production Deployment** (`deploy-production.yml`)
- Triggered on release publish
- Validates semantic versioning
- Builds and tags with version number
- Deploys to AKS production cluster (blue-green)

---

## Health Checks

### In-Container Health Endpoint

Each service exposes `/health` endpoint:

```bash
# Check from host
curl http://localhost:8080/health

# Check from within container
docker exec digitalstokvel-apigateway wget --spider http://localhost:8080/health
```

### Docker Health Check

Defined in Dockerfile:
```dockerfile
HEALTHCHECK --interval=30s --timeout=3s --start-period=10s --retries=3 \
  CMD wget --no-verbose --tries=1 --spider http://localhost:8080/health || exit 1
```

View health status:
```bash
docker ps
# CONTAINER ID   STATUS
# abc123def456   Up 2 minutes (healthy)
```

---

## Security Considerations

### 1. **Non-Root User**
All containers run as `appuser:appuser` (UID/GID 1000), not root.

### 2. **Minimal Base Image**
Alpine Linux reduces attack surface (fewer packages, smaller image).

### 3. **No Secrets in Images**
Use environment variables and secret management:
```bash
docker run -e DATABASE_CONNECTION_STRING="..." digitalstokvel-apigateway:latest
```

### 4. **Vulnerability Scanning**
CI/CD pipeline includes Trivy scan:
```bash
# Manual scan
docker run --rm -v /var/run/docker.sock:/var/run/docker.sock \
  aquasec/trivy:latest image digitalstokvel-apigateway:latest
```

### 5. **Read-Only File System**
Consider running with `--read-only` flag:
```bash
docker run --read-only --tmpfs /tmp digitalstokvel-apigateway:latest
```

---

## Performance Monitoring

### Container Stats

```bash
# View resource usage
docker stats

# Get specific container stats
docker stats digitalstokvel-apigateway --no-stream
```

### Logs and Diagnostics

```bash
# View logs
docker logs digitalstokvel-apigateway

# Follow logs
docker logs -f digitalstokvel-apigateway

# View last 100 lines
docker logs --tail 100 digitalstokvel-apigateway

# Export logs
docker logs digitalstokvel-apigateway > apigateway.log
```

---

## Cleanup

### Remove Stopped Containers

```bash
docker container prune -f
```

### Remove Unused Images

```bash
# Remove dangling images
docker image prune -f

# Remove all unused images
docker image prune -a -f
```

### Remove Build Cache

```bash
docker builder prune -f
```

### Complete Cleanup

```bash
# WARNING: Removes all containers, images, volumes, networks
docker system prune -a --volumes -f
```

---

## Next Steps

After configuring automated build pipelines (Task 0.2.2), proceed to:

- **Task 0.2.3:** Configure automated testing in pipeline
- **Task 0.2.4:** Set up container registry (Azure ACR / Docker Hub)
- **Task 0.2.5:** Configure deployment pipelines for staging and production

---

## References

- [.NET Docker Official Images](https://hub.docker.com/_/microsoft-dotnet)
- [Docker Best Practices](https://docs.docker.com/develop/dev-best-practices/)
- [Multi-stage Builds](https://docs.docker.com/build/building/multi-stage/)
- [Dockerfile Reference](https://docs.docker.com/engine/reference/builder/)
- [CICD_STANDARDS.md](CICD_STANDARDS.md) - GitHub Actions workflows

---

**Document Status:** Active  
**Last Updated:** March 24, 2026  
**Next Review:** After Task 0.2.3 completion

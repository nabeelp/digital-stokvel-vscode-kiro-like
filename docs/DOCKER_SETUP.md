# Docker Desktop Setup Guide

**Version:** 1.0  
**Date:** March 2026  
**Applies To:** Digital Stokvel Banking - Development Environment

---

## Table of Contents

1. [Overview](#overview)
2. [System Requirements](#system-requirements)
3. [Docker Desktop Installation](#docker-desktop-installation)
4. [Docker Desktop Configuration](#docker-desktop-configuration)
5. [Docker Compose for Digital Stokvel](#docker-compose-for-digital-stokvel)
6. [Verification](#verification)
7. [Common Operations](#common-operations)
8. [Development Workflow](#development-workflow)
9. [Performance Optimization](#performance-optimization)
10. [Troubleshooting](#troubleshooting)

---

## Overview

### Purpose

Docker Desktop provides containerization infrastructure for the Digital Stokvel Banking platform development environment, enabling:

- **Consistent Development Environment**: Same infrastructure across all developer machines
- **Service Isolation**: PostgreSQL, Redis, RabbitMQ, and pgAdmin run in isolated containers
- **Easy Setup**: One-command startup of all development dependencies
- **Version Control**: Infrastructure defined as code in `docker-compose.yml`
- **Production Parity**: Development environment mirrors production Kubernetes deployment

### Architecture Role

```
┌─────────────────────────────────────────────────────────┐
│              Docker Desktop (Local Machine)             │
│                                                         │
│  ┌──────────────────────────────────────────────────┐ │
│  │         Docker Compose Services                  │ │
│  │                                                  │ │
│  │  ┌──────────┐  ┌──────────┐  ┌──────────────┐  │ │
│  │  │PostgreSQL│  │PostgreSQL│  │    Redis     │  │ │
│  │  │ Primary  │  │  Ledger  │  │    Cache     │  │ │
│  │  │ :5432    │  │  :5433   │  │    :6379     │  │ │
│  │  └──────────┘  └──────────┘  └──────────────┘  │ │
│  │                                                  │ │
│  │  ┌──────────┐  ┌──────────┐                     │ │
│  │  │RabbitMQ  │  │ pgAdmin  │                     │ │
│  │  │  :5672   │  │  :5050   │                     │ │
│  │  │ :15672   │  │          │                     │ │
│  │  └──────────┘  └──────────┘                     │ │
│  │                                                  │ │
│  └──────────────────────────────────────────────────┘ │
│                                                         │
│  ┌──────────────────────────────────────────────────┐ │
│  │        .NET 10 Services (Host Machine)           │ │
│  │                                                  │ │
│  │  - API Gateway                                   │ │
│  │  - Group Service                                 │ │
│  │  - Contribution Service                          │ │
│  │  - Payout Service                                │ │
│  │  - Governance Service                            │ │
│  │  - Notification Service                          │ │
│  │  - Credit Profile Service                        │ │
│  │  - USSD Gateway                                  │ │
│  │                                                  │ │
│  └──────────────────────────────────────────────────┘ │
│                                                         │
└─────────────────────────────────────────────────────────┘
         │
         ▼
    Connects to containerized services via localhost ports
```

### What Gets Containerized

The Digital Stokvel project uses Docker for **infrastructure services only**:

✅ **Containerized (Development Dependencies)**:
- PostgreSQL Primary Database (port 5432)
- PostgreSQL Ledger Database (port 5433)
- Redis Cache (port 6379)
- RabbitMQ Message Queue (ports 5672, 15672)
- pgAdmin Database Management UI (port 5050)

❌ **Not Containerized (Run Directly on Host)**:
- .NET 10 Services (for easier debugging with IDE)
- Android/iOS/Web applications (native development)

**Why this approach?**
- **Fast Development**: No container rebuild for code changes
- **Native Debugging**: Full IDE integration (breakpoints, hot reload)
- **Simplified Workflow**: `dotnet run` works as expected
- **Production Parity**: Services still containerized for production deployment

---

## System Requirements

### Hardware Requirements

**Minimum**:
- CPU: 2 cores
- RAM: 4 GB (8 GB recommended)
- Disk Space: 10 GB free space

**Recommended**:
- CPU: 4+ cores
- RAM: 16 GB
- Disk Space: 50 GB+ free space (SSD preferred)

### Operating System Requirements

#### Windows

- **Windows 10 64-bit**: Pro, Enterprise, or Education (Build 19041 or higher)
- **Windows 11 64-bit**: Any edition
- **WSL 2** (Windows Subsystem for Linux 2) must be enabled
- **Hyper-V** and **Containers** Windows features must be enabled

**Windows Home Users**:
- Docker Desktop works on Windows 10/11 Home with WSL 2 backend
- Hyper-V is NOT required for WSL 2 backend

#### macOS

- **macOS 11 (Big Sur)** or newer
- **Apple Silicon (M1/M2/M3)** or **Intel processor**
- At least 4 GB RAM

#### Linux

- **64-bit Ubuntu 20.04 LTS or newer**
- **Debian 10 or newer**
- **Fedora 34 or newer**
- **CentOS/RHEL 8 or newer**

### Software Prerequisites

- **Administrator/sudo access** for installation
- **Internet connection** for downloading Docker images
- **Virtualization enabled** in BIOS (Intel VT-x or AMD-V)

---

## Docker Desktop Installation

### Windows Installation

#### Step 1: Enable WSL 2 (Windows Subsystem for Linux)

1. **Open PowerShell as Administrator**:
   ```powershell
   # Enable WSL
   dism.exe /online /enable-feature /featurename:Microsoft-Windows-Subsystem-Linux /all /norestart

   # Enable Virtual Machine Platform
   dism.exe /online /enable-feature /featurename:VirtualMachinePlatform /all /norestart
   ```

2. **Restart your computer**

3. **Download and install WSL 2 Linux kernel update**:
   - Download from: https://aka.ms/wsl2kernel
   - Run the installer

4. **Set WSL 2 as default**:
   ```powershell
   wsl --set-default-version 2
   ```

5. **Install Ubuntu distribution** (optional but recommended):
   ```powershell
   wsl --install -d Ubuntu
   ```

#### Step 2: Install Docker Desktop for Windows

1. **Download Docker Desktop**:
   - Visit: https://www.docker.com/products/docker-desktop
   - Click **"Download for Windows"**
   - File: `Docker Desktop Installer.exe`

2. **Run the Installer**:
   - Double-click `Docker Desktop Installer.exe`
   - Check **"Use WSL 2 instead of Hyper-V"** (recommended)
   - Click **"OK"**
   - Wait for installation to complete (may take 5-10 minutes)

3. **Restart your computer** when prompted

4. **Launch Docker Desktop**:
   - Docker Desktop should start automatically after restart
   - If not, launch from Start Menu

5. **Accept Docker Desktop Service Agreement**

6. **Complete Setup**:
   - Docker Desktop will initialize (may take 2-3 minutes first time)
   - Look for **"Docker Desktop is running"** in system tray

#### Step 3: Verify Installation

Open PowerShell or Command Prompt:

```powershell
# Check Docker version
docker --version
# Expected: Docker version 24.0.0 or higher

# Check Docker Compose version
docker compose version
# Expected: Docker Compose version v2.20.0 or higher

# Test Docker is working
docker run hello-world
# Expected: "Hello from Docker!" message
```

### macOS Installation

#### Step 1: Download Docker Desktop for Mac

1. **Identify your Mac processor**:
   - Click Apple menu () → **About This Mac**
   - Check if you have **Apple Silicon (M1/M2/M3)** or **Intel**

2. **Download appropriate version**:
   - Visit: https://www.docker.com/products/docker-desktop
   - Download:
     - **Apple Silicon**: Docker Desktop for Mac with Apple Silicon
     - **Intel**: Docker Desktop for Mac with Intel chip

#### Step 2: Install Docker Desktop

1. **Open the downloaded `.dmg` file**

2. **Drag Docker icon to Applications folder**

3. **Launch Docker Desktop** from Applications

4. **Grant permissions**:
   - macOS may ask for administrator password
   - Click **"OK"** to allow Docker to install networking components

5. **Complete Setup**:
   - Accept Docker Desktop Service Agreement
   - Optionally enable usage statistics
   - Wait for initialization (2-3 minutes)

6. **Docker Desktop is ready** when whale icon in menu bar is steady

#### Step 3: Verify Installation

Open Terminal:

```bash
# Check Docker version
docker --version

# Check Docker Compose version
docker compose version

# Test Docker is working
docker run hello-world
```

### Linux Installation (Ubuntu/Debian)

#### Option 1: Docker Desktop for Linux (Recommended)

**Supported Distributions**:
- Ubuntu 20.04 LTS, 22.04 LTS, 24.04 LTS
- Debian 10, 11, 12

1. **Update package index**:
   ```bash
   sudo apt update
   sudo apt install -y ca-certificates curl gnupg lsb-release
   ```

2. **Download Docker Desktop `.deb` package**:
   - Visit: https://docs.docker.com/desktop/install/linux/
   - Download the `.deb` file for your distribution

3. **Install Docker Desktop**:
   ```bash
   sudo apt install ./docker-desktop-<version>-<arch>.deb
   ```

4. **Launch Docker Desktop**:
   ```bash
   systemctl --user start docker-desktop
   ```

5. **Enable Docker Desktop to start on boot**:
   ```bash
   systemctl --user enable docker-desktop
   ```

#### Option 2: Docker Engine + Docker Compose (CLI Only)

If you prefer command-line only (no GUI):

1. **Install Docker Engine**:
   ```bash
   # Add Docker's official GPG key
   sudo install -m 0755 -d /etc/apt/keyrings
   curl -fsSL https://download.docker.com/linux/ubuntu/gpg | sudo gpg --dearmor -o /etc/apt/keyrings/docker.gpg
   sudo chmod a+r /etc/apt/keyrings/docker.gpg

   # Add Docker repository
   echo \
     "deb [arch=$(dpkg --print-architecture) signed-by=/etc/apt/keyrings/docker.gpg] https://download.docker.com/linux/ubuntu \
     $(. /etc/os-release && echo "$VERSION_CODENAME") stable" | \
     sudo tee /etc/apt/sources.list.d/docker.list > /dev/null

   # Install Docker Engine
   sudo apt update
   sudo apt install -y docker-ce docker-ce-cli containerd.io docker-buildx-plugin docker-compose-plugin
   ```

2. **Add your user to docker group** (avoid sudo):
   ```bash
   sudo usermod -aG docker $USER
   newgrp docker
   ```

3. **Enable Docker to start on boot**:
   ```bash
   sudo systemctl enable docker.service
   sudo systemctl enable containerd.service
   ```

#### Step 3: Verify Installation

```bash
# Check Docker version
docker --version

# Check Docker Compose version
docker compose version

# Test Docker is working
docker run hello-world
```

---

## Docker Desktop Configuration

### Resource Allocation

#### Windows/macOS

1. **Open Docker Desktop**
2. **Click Settings (⚙️) icon**
3. **Navigate to Resources**

**Recommended Settings**:
```
CPUs: 4 (minimum 2)
Memory: 8 GB (minimum 4 GB)
Swap: 2 GB
Disk Image Size: 64 GB (adjust based on available space)
```

**For Digital Stokvel Development**:
- PostgreSQL containers: ~512 MB each (2 databases = ~1 GB)
- Redis: ~256 MB
- RabbitMQ: ~512 MB
- pgAdmin: ~256 MB
- **Total**: ~2 GB minimum, 4-6 GB recommended with overhead

#### Linux (Docker Engine)

Docker Engine on Linux uses host resources directly (no VM). No configuration needed.

### File Sharing (Windows/macOS)

Docker Desktop needs access to project directories.

**Windows**:
1. Settings → Resources → **File Sharing**
2. Ensure your workspace drive (e.g., `C:\`) is listed
3. If using a different drive, add it

**macOS**:
1. Settings → Resources → **File Sharing**
2. Default locations:
   - `/Users`
   - `/Volumes`
   - `/tmp`
   - `/private`
3. Add additional directories if needed

**Project Location**:
- Ensure `C:\Repos\GitHub\nabeelp\digital-stokvel-vscode-kiro-like` (Windows)
- Or `/Users/your-username/projects/digital-stokvel` (macOS)
- Is accessible to Docker

### WSL 2 Integration (Windows)

1. **Settings → Resources → WSL Integration**
2. **Enable integration with my default WSL distro**: ✅ ON
3. **Enable additional distros**: Select Ubuntu or your distro
4. **Apply & Restart**

This allows running Docker commands inside WSL 2 terminal.

### Docker Engine Settings

1. **Settings → Docker Engine**
2. **Configuration JSON**:

```json
{
  "builder": {
    "gc": {
      "defaultKeepStorage": "20GB",
      "enabled": true
    }
  },
  "experimental": false,
  "features": {
    "buildkit": true
  },
  "log-driver": "json-file",
  "log-opts": {
    "max-size": "10m",
    "max-file": "3"
  }
}
```

**Key Settings**:
- `buildkit: true`: Use BuildKit for faster builds
- `log-opts`: Limit log file size (prevent disk bloat)
- `gc`: Garbage collection for unused images

### Kubernetes (Optional - Not Required for Digital Stokvel)

Docker Desktop includes Kubernetes, but **it is NOT required** for Digital Stokvel development.

If you want to enable it:
1. Settings → **Kubernetes**
2. ✅ **Enable Kubernetes**
3. Click **Apply & Restart**

**Note**: Kubernetes consumes significant resources (~2 GB RAM). Only enable if you plan to test Kubernetes deployments locally.

---

## Docker Compose for Digital Stokvel

### Project Docker Compose File

The project includes `docker-compose.yml` with all development dependencies.

**Location**: `C:\Repos\GitHub\nabeelp\digital-stokvel-vscode-kiro-like\docker-compose.yml`

**Services Defined**:
1. **postgres**: Primary database (port 5432)
2. **postgres-ledger**: Ledger database (port 5433)
3. **redis**: Cache (port 6379)
4. **rabbitmq**: Message queue (ports 5672, 15672)
5. **pgadmin**: Database management UI (port 5050)

### Environment Variables Setup

**Required**: Create `.env` file in project root with passwords.

#### Step 1: Copy `.env.example` (if exists) or create manually

```bash
# Navigate to project root
cd C:\Repos\GitHub\nabeelp\digital-stokvel-vscode-kiro-like

# Copy example (if exists)
cp .env.example .env
```

#### Step 2: Edit `.env` file

Create or edit `.env` with the following content:

```env
# PostgreSQL Primary Database
POSTGRES_USER=stokvel_admin
POSTGRES_PASSWORD=your_strong_postgres_password
POSTGRES_DB=digitalstokvel

# PostgreSQL Ledger Database
POSTGRES_LEDGER_USER=ledger_admin
POSTGRES_LEDGER_PASSWORD=your_strong_ledger_password
POSTGRES_LEDGER_DB=digitalstokvel_ledger

# Redis Cache
REDIS_PASSWORD=your_strong_redis_password

# RabbitMQ Message Queue
RABBITMQ_USER=stokvel_admin
RABBITMQ_PASSWORD=your_strong_rabbitmq_password

# pgAdmin
PGADMIN_EMAIL=admin@digitalstokvel.local
PGADMIN_PASSWORD=your_strong_pgadmin_password
```

**Password Security Best Practices**:
- Use strong passwords (16+ characters, alphanumeric + special characters)
- Never commit `.env` to version control
- Use a password manager to generate and store passwords
- Different passwords for each service

#### Step 3: Verify `.gitignore` excludes `.env`

Check `.gitignore` includes:
```
.env
.env.local
```

### Starting Services

#### Start All Services

```bash
# Navigate to project root
cd C:\Repos\GitHub\nabeelp\digital-stokvel-vscode-kiro-like

# Start all services in detached mode
docker compose up -d
```

Expected output:
```
[+] Running 5/5
 ✔ Container digitalstokvel-postgres        Started
 ✔ Container digitalstokvel-ledger          Started
 ✔ Container digitalstokvel-redis           Started
 ✔ Container digitalstokvel-rabbitmq        Started
 ✔ Container digitalstokvel-pgadmin         Started
```

#### Start Specific Services

```bash
# Start only PostgreSQL and Redis
docker compose up -d postgres redis
```

#### View Logs

```bash
# View all service logs
docker compose logs

# Follow logs in real-time
docker compose logs -f

# View logs for specific service
docker compose logs -f postgres

# View last 100 lines
docker compose logs --tail=100
```

#### Check Service Status

```bash
docker compose ps
```

Expected output:
```
NAME                        STATUS      PORTS
digitalstokvel-postgres     Up          0.0.0.0:5432->5432/tcp
digitalstokvel-ledger       Up          0.0.0.0:5433->5432/tcp
digitalstokvel-redis        Up          0.0.0.0:6379->6379/tcp
digitalstokvel-rabbitmq     Up          0.0.0.0:5672->5672/tcp, 0.0.0.0:15672->15672/tcp
digitalstokvel-pgadmin      Up          0.0.0.0:5050->80/tcp
```

### Stopping Services

```bash
# Stop all services (containers remain, data persists)
docker compose stop

# Stop specific service
docker compose stop redis

# Stop and remove containers (data still persists in volumes)
docker compose down

# Stop, remove containers AND volumes (WARNING: deletes all data)
docker compose down -v
```

### Restarting Services

```bash
# Restart all services
docker compose restart

# Restart specific service
docker compose restart postgres
```

### Accessing Service Logs

```bash
# PostgreSQL logs
docker compose logs -f postgres

# Redis logs
docker compose logs -f redis

# RabbitMQ logs
docker compose logs -f rabbitmq
```

---

## Verification

### Verify Docker Installation

```bash
# Check Docker version
docker --version
# Expected: Docker version 24.0.0 or higher

# Check Docker Compose version
docker compose version
# Expected: Docker Compose version v2.20.0 or higher

# Check Docker is running
docker info
# Should show server info without errors

# Test Docker is functional
docker run hello-world
# Should download image and print "Hello from Docker!"
```

### Verify Services are Running

```bash
# Check all services
docker compose ps

# Expected: All services show "Up" status
```

### Test Service Connectivity

#### PostgreSQL Primary Database (Port 5432)

```bash
# Option 1: Using Docker exec
docker exec -it digitalstokvel-postgres psql -U stokvel_admin -d digitalstokvel -c "SELECT version();"

# Option 2: Using psql from host (if installed)
psql -h localhost -p 5432 -U stokvel_admin -d digitalstokvel
# Password: your_strong_postgres_password
```

#### PostgreSQL Ledger Database (Port 5433)

```bash
docker exec -it digitalstokvel-ledger psql -U ledger_admin -d digitalstokvel_ledger -c "SELECT version();"
```

#### Redis (Port 6379)

```bash
# Test connection and authentication
docker exec -it digitalstokvel-redis redis-cli -a your_strong_redis_password PING

# Expected: PONG
```

#### RabbitMQ (Ports 5672, 15672)

**Management UI**:
1. Open browser: http://localhost:15672
2. Login:
   - Username: `stokvel_admin`
   - Password: `your_strong_rabbitmq_password`
3. You should see RabbitMQ Management dashboard

**AMQP Connection** (from .NET):
```csharp
// Test in Program.cs
var factory = new ConnectionFactory() 
{ 
    HostName = "localhost",
    Port = 5672,
    UserName = "stokvel_admin",
    Password = "your_strong_rabbitmq_password"
};
var connection = factory.CreateConnection();
Console.WriteLine("RabbitMQ connected successfully!");
```

#### pgAdmin (Port 5050)

1. Open browser: http://localhost:5050
2. Login:
   - Email: `admin@digitalstokvel.local`
   - Password: `your_strong_pgadmin_password`
3. Add PostgreSQL servers (see [LOCAL_POSTGRES_SETUP.md](LOCAL_POSTGRES_SETUP.md))

### Health Checks

Docker Compose includes health checks for all services.

Check health status:

```bash
# View health status
docker compose ps

# Inspect specific service health
docker inspect --format='{{.State.Health.Status}}' digitalstokvel-postgres
# Expected: healthy

docker inspect --format='{{.State.Health.Status}}' digitalstokvel-redis
# Expected: healthy
```

If a service shows `unhealthy`, check logs:

```bash
docker compose logs <service-name>
```

---

## Common Operations

### View Running Containers

```bash
# All containers (Docker Compose and others)
docker ps

# Only project containers
docker compose ps

# All containers including stopped
docker ps -a
```

### Execute Commands in Containers

```bash
# Open bash shell in container
docker exec -it digitalstokvel-postgres bash

# Execute single command
docker exec -it digitalstokvel-redis redis-cli -a your_password INFO
```

### View Container Resource Usage

```bash
# Real-time stats for all containers
docker stats

# Stats for project containers only
docker stats $(docker compose ps -q)
```

### Inspect Container Details

```bash
# Detailed container information
docker inspect digitalstokvel-postgres

# View environment variables
docker inspect --format='{{.Config.Env}}' digitalstokvel-postgres

# View port mappings
docker inspect --format='{{.NetworkSettings.Ports}}' digitalstokvel-postgres
```

### Backup and Restore Volumes

#### Backup PostgreSQL Data

```bash
# Backup primary database volume
docker run --rm \
  -v digitalstokvel_postgres_data:/data \
  -v $(pwd):/backup \
  alpine tar czf /backup/postgres-backup-$(date +%Y%m%d).tar.gz /data

# Backup ledger database volume
docker run --rm \
  -v digitalstokvel_postgres_ledger_data:/data \
  -v $(pwd):/backup \
  alpine tar czf /backup/ledger-backup-$(date +%Y%m%d).tar.gz /data
```

#### Restore PostgreSQL Data

```bash
# Stop services
docker compose stop postgres

# Restore from backup
docker run --rm \
  -v digitalstokvel_postgres_data:/data \
  -v $(pwd):/backup \
  alpine tar xzf /backup/postgres-backup-20260324.tar.gz -C /

# Start services
docker compose start postgres
```

### Clean Up Docker Resources

```bash
# Remove stopped containers
docker container prune

# Remove unused images
docker image prune

# Remove unused volumes (WARNING: may delete data)
docker volume prune

# Remove all unused resources (containers, images, volumes, networks)
docker system prune -a --volumes
```

### Update Docker Images

```bash
# Pull latest images defined in docker-compose.yml
docker compose pull

# Recreate containers with new images
docker compose up -d --force-recreate
```

---

## Development Workflow

### Daily Workflow

#### 1. Start Development Session

```bash
# Navigate to project
cd C:\Repos\GitHub\nabeelp\digital-stokvel-vscode-kiro-like

# Start all services
docker compose up -d

# Verify services are healthy
docker compose ps
```

#### 2. Develop and Test

```bash
# Run .NET services on host machine (not in Docker)
dotnet run --project src/services/DigitalStokvel.GroupService

# Services connect to containerized dependencies via localhost
# - PostgreSQL: localhost:5432
# - Redis: localhost:6379
# - RabbitMQ: localhost:5672
```

#### 3. View Logs

```bash
# Monitor PostgreSQL logs in separate terminal
docker compose logs -f postgres

# Monitor Redis logs
docker compose logs -f redis
```

#### 4. End Development Session

```bash
# Stop services (data persists)
docker compose stop

# Or stop and remove containers (data still persists in volumes)
docker compose down
```

### Testing Workflow

```bash
# Start services
docker compose up -d

# Wait for services to be healthy
sleep 5

# Run tests
dotnet test

# Stop services
docker compose stop
```

### Database Migration Workflow

```bash
# Start databases
docker compose up -d postgres postgres-ledger

# Run migrations
dotnet ef database update --project src/services/DigitalStokvel.GroupService

# Verify migration
docker exec -it digitalstokvel-postgres psql -U stokvel_admin -d digitalstokvel -c "\dt"
```

### Reset Development Environment

**WARNING**: This deletes all data!

```bash
# Stop and remove all containers and volumes
docker compose down -v

# Remove all images (optional)
docker compose down --rmi all

# Start fresh
docker compose up -d
```

### CI/CD Integration

For CI/CD pipelines (GitHub Actions):

```yaml
# .github/workflows/test.yml
name: Run Tests

on: [push, pull_request]

jobs:
  test:
    runs-on: ubuntu-latest

    steps:
      - uses: actions/checkout@v3

      - name: Start Docker services
        run: |
          docker compose up -d postgres redis
          docker compose ps

      - name: Wait for services
        run: sleep 10

      - name: Setup .NET
        uses: actions/setup-dotnet@v3
        with:
          dotnet-version: '10.0.x'

      - name: Run tests
        run: dotnet test

      - name: Stop services
        run: docker compose down
```

---

## Performance Optimization

### Resource Allocation

#### Allocate Sufficient Resources

**Windows/macOS**:
- Docker Desktop → Settings → Resources
- CPUs: 4 (50% of available)
- Memory: 8 GB (50% of available)

#### Limit Container Resources

Edit `docker-compose.yml` to add resource limits:

```yaml
services:
  postgres:
    # ... existing config
    deploy:
      resources:
        limits:
          cpus: '2.0'
          memory: 2G
        reservations:
          cpus: '0.5'
          memory: 512M
```

### Volume Performance (Windows)

**Best Practice**: Use WSL 2 filesystem, not Windows filesystem.

**❌ Slow** (Windows filesystem):
```
C:\Users\YourName\Projects\digital-stokvel
```

**✅ Fast** (WSL 2 filesystem):
```bash
# Inside WSL 2 terminal
cd ~
mkdir projects
cd projects
git clone <repository>
```

Access WSL 2 filesystem from Windows:
```
\\wsl$\Ubuntu\home\your-username\projects\digital-stokvel
```

### Use BuildKit

Enable BuildKit in Docker Desktop settings (likely already enabled):

Settings → Docker Engine:
```json
{
  "features": {
    "buildkit": true
  }
}
```

### Named Volumes vs Bind Mounts

**Named Volumes** (Used in docker-compose.yml):
```yaml
volumes:
  - postgres_data:/var/lib/postgresql/data
```
✅ Better performance  
✅ Managed by Docker  
✅ Portable across platforms

**Bind Mounts** (Avoid for databases):
```yaml
volumes:
  - ./data/postgres:/var/lib/postgresql/data
```
❌ Slower on Windows/macOS  
❌ Filesystem compatibility issues  
❌ Only use for code/config files

### Prune Regularly

```bash
# Remove unused images, containers, networks (weekly)
docker system prune

# Remove unused volumes (monthly, with caution)
docker volume prune
```

### Update Docker Desktop

Keep Docker Desktop up-to-date for performance improvements and bug fixes.

Check for updates: Docker Desktop → Settings → Software Updates

---

## Troubleshooting

### Common Issues

#### 1. "Docker Desktop is starting..." (stuck)

**Symptoms**: Docker Desktop remains in "starting" state indefinitely.

**Solutions**:

**Windows**:
```powershell
# Restart Docker Desktop service
net stop com.docker.service
net start com.docker.service

# Or reset Docker Desktop
# Settings → Troubleshoot → Reset to factory defaults
```

**macOS**:
```bash
# Quit Docker Desktop completely
pkill Docker

# Restart Docker Desktop from Applications
```

**Linux**:
```bash
sudo systemctl restart docker
```

#### 2. "Cannot connect to the Docker daemon"

**Symptoms**:
```
Cannot connect to the Docker daemon at unix:///var/run/docker.sock. Is the docker daemon running?
```

**Solutions**:

- Verify Docker Desktop is running (look for icon in system tray/menu bar)
- Restart Docker Desktop
- Check Docker service status:
  ```bash
  # Linux
  sudo systemctl status docker

  # Windows (PowerShell)
  Get-Service com.docker.service
  ```

#### 3. Port Already in Use

**Symptoms**:
```
Error: Bind for 0.0.0.0:5432 failed: port is already allocated
```

**Solutions**:

**Option 1**: Stop conflicting service
```bash
# Windows: Find process using port
netstat -ano | findstr :5432

# Kill process (PowerShell as Admin)
Stop-Process -Id <PID> -Force

# Linux/macOS: Find and kill process
sudo lsof -i :5432
sudo kill -9 <PID>
```

**Option 2**: Change port in `docker-compose.yml`
```yaml
services:
  postgres:
    ports:
      - "5433:5432"  # Use different host port
```

#### 4. "No space left on device"

**Symptoms**:
```
Error response from daemon: no space left on device
```

**Solutions**:

```bash
# Check disk usage
docker system df

# Remove unused resources
docker system prune -a --volumes

# Increase Docker disk image size
# Docker Desktop → Settings → Resources → Disk Image Size
```

#### 5. Container Keeps Restarting

**Symptoms**:
```
docker compose ps
# Shows container with "Restarting" status
```

**Solutions**:

```bash
# Check container logs for errors
docker compose logs <service-name>

# Check environment variables
docker compose config

# Verify .env file exists and has correct values
cat .env
```

#### 6. Cannot Access Service from Host

**Symptoms**:
- Cannot connect to PostgreSQL on `localhost:5432`
- Cannot access pgAdmin on `http://localhost:5050`

**Solutions**:

```bash
# Verify port mapping
docker compose ps
# Ensure ports show 0.0.0.0:5432->5432/tcp

# Check firewall (Windows)
# Allow Docker Desktop through Windows Firewall

# Try 127.0.0.1 instead of localhost
psql -h 127.0.0.1 -p 5432 -U stokvel_admin
```

#### 7. WSL 2 Issues (Windows)

**Symptoms**:
- Docker Desktop fails to start
- "WSL 2 installation is incomplete"

**Solutions**:

```powershell
# Update WSL
wsl --update

# Set WSL 2 as default
wsl --set-default-version 2

# Verify WSL 2 is running
wsl --list --verbose

# Restart WSL
wsl --shutdown
# Then restart Docker Desktop
```

#### 8. High CPU/Memory Usage

**Symptoms**:
- Docker Desktop consuming excessive resources
- Computer becomes slow

**Solutions**:

```bash
# Check container resource usage
docker stats

# Limit resources in docker-compose.yml (see Performance Optimization)

# Reduce allocated resources
# Docker Desktop → Settings → Resources
# Lower CPU and Memory allocations
```

#### 9. "Invalid reference format"

**Symptoms**:
```
docker compose up
invalid reference format
```

**Cause**: Usually syntax error in `docker-compose.yml`

**Solutions**:

```bash
# Validate docker-compose.yml
docker compose config

# Common issues:
# - Incorrect indentation (YAML is whitespace-sensitive)
# - Missing quotes around values with special characters
# - Typos in service names or image names
```

#### 10. Permission Denied (Linux)

**Symptoms**:
```
docker: Got permission denied while trying to connect to the Docker daemon socket
```

**Solutions**:

```bash
# Add user to docker group
sudo usermod -aG docker $USER

# Apply group membership (logout/login or use)
newgrp docker

# Verify
docker ps
```

### Diagnostic Commands

```bash
# Docker system info
docker info

# Docker version
docker version

# Docker Compose version
docker compose version

# Check service health
docker compose ps

# View all logs
docker compose logs

# Inspect specific service
docker inspect digitalstokvel-postgres

# Check resource usage
docker stats

# Validate docker-compose.yml
docker compose config
```

### Getting Help

1. **Docker Desktop Troubleshoot Menu**:
   - Settings → Troubleshoot
   - Run diagnostics
   - View logs

2. **Docker Community**:
   - Docker Forums: https://forums.docker.com/
   - Stack Overflow: Tag `docker` or `docker-compose`

3. **Project Team**:
   - Check project documentation
   - Ask in team chat
   - Create issue in repository

---

## Best Practices

### Security

1. **Never Commit `.env` Files**
   ```bash
   # Verify .gitignore contains
   .env
   .env.local
   ```

2. **Use Strong Passwords** in `.env`

3. **Limit Container Privileges**
   ```yaml
   services:
     postgres:
       # ... existing config
       security_opt:
         - no-new-privileges:true
   ```

4. **Keep Docker Desktop Updated**

### Performance

1. **Use Named Volumes** (not bind mounts for databases)

2. **Allocate Sufficient Resources** (8 GB RAM, 4 CPUs)

3. **Prune Regularly**
   ```bash
   docker system prune -a
   ```

4. **Use WSL 2 Filesystem** (Windows)

### Development Workflow

1. **Start Services Before Development**
   ```bash
   docker compose up -d
   ```

2. **Stop Services After Development**
   ```bash
   docker compose stop
   ```

3. **Don't Commit Volumes**
   - Volumes are in `.gitignore`
   - Each developer has their own data

4. **Share `docker-compose.yml`**
   - Commit to repository
   - Ensures consistent environment

5. **Document Required Environment Variables**
   - Create `.env.example` template
   - Never commit `.env` with real passwords

### Maintenance

1. **Weekly**: Prune unused resources
   ```bash
   docker system prune
   ```

2. **Monthly**: Update Docker Desktop

3. **Before Major Changes**: Backup volumes
   ```bash
   docker run --rm -v digitalstokvel_postgres_data:/data -v $(pwd):/backup alpine tar czf /backup/postgres-backup.tar.gz /data
   ```

---

## Next Steps

### Post-Setup Checklist

- [ ] Docker Desktop installed and running
- [ ] `docker --version` shows Docker 24.0.0+
- [ ] `docker compose version` shows v2.20.0+
- [ ] `.env` file created with secure passwords
- [ ] `docker compose up -d` started all services successfully
- [ ] All services show "Up" status (`docker compose ps`)
- [ ] PostgreSQL accessible on localhost:5432 and localhost:5433
- [ ] Redis accessible on localhost:6379
- [ ] RabbitMQ Management UI accessible on http://localhost:15672
- [ ] pgAdmin accessible on http://localhost:5050

### Integration with Development

1. **Configure .NET Connection Strings**:
   - Update `appsettings.Development.json` with localhost endpoints
   - See [DEVELOPER_SETUP.md](DEVELOPER_SETUP.md)

2. **Initialize Databases**:
   - Run Entity Framework migrations
   - See [LOCAL_POSTGRES_SETUP.md](LOCAL_POSTGRES_SETUP.md)

3. **Test Redis Connection**:
   - Test from .NET services
   - See [LOCAL_REDIS_SETUP.md](LOCAL_REDIS_SETUP.md)

### Further Learning

- **Docker Documentation**: https://docs.docker.com/
- **Docker Compose Documentation**: https://docs.docker.com/compose/
- **Docker Best Practices**: https://docs.docker.com/develop/dev-best-practices/
- **WSL 2 Backend**: https://docs.docker.com/desktop/wsl/

---

## Summary

This guide covered:

✅ **System Requirements** for Windows, macOS, Linux  
✅ **Docker Desktop Installation** with step-by-step instructions  
✅ **Configuration** for optimal development experience  
✅ **Docker Compose** usage for Digital Stokvel services  
✅ **Verification** of installation and service connectivity  
✅ **Common Operations** for daily development  
✅ **Development Workflow** integration  
✅ **Performance Optimization** tips  
✅ **Troubleshooting** common issues  

**Docker Desktop is now configured for Digital Stokvel development!** 🐳

All infrastructure dependencies (PostgreSQL, Redis, RabbitMQ, pgAdmin) run in isolated containers with one command: `docker compose up -d`

For questions or issues, refer to the troubleshooting section or consult the team.

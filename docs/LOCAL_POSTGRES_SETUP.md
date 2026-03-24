# Local PostgreSQL Setup Guide - Digital Stokvel Banking Platform

**Version:** 1.0  
**Last Updated:** March 24, 2026  
**Status:** Active

---

## Table of Contents

1. [Overview](#overview)
2. [Quick Start](#quick-start)
3. [Docker Compose Setup](#docker-compose-setup)
4. [Manual PostgreSQL Installation](#manual-postgresql-installation)
5. [Database Initialization](#database-initialization)
6. [Connection Configuration](#connection-configuration)
7. [Database Migrations](#database-migrations)
8. [pgAdmin Setup](#pgadmin-setup)
9. [Common Operations](#common-operations)
10. [Troubleshooting](#troubleshooting)

---

## Overview

The Digital Stokvel Banking platform uses PostgreSQL 15+ for two primary databases:

1. **Primary Database** (`digitalstokvel`): Application data (groups, members, contributions, payouts, etc.)
2. **Ledger Database** (`digitalstokvel_ledger`): Immutable append-only audit trail

### Database Architecture

```
┌─────────────────────────────────────┐
│   Application Services              │
└─────────┬───────────────────┬───────┘
          │                   │
          ▼                   ▼
┌──────────────────┐  ┌──────────────────┐
│ Primary Database │  │ Ledger Database  │
│  digitalstokvel  │  │digitalstokvel    │
│                  │  │     _ledger      │
│  - groups        │  │  - contribution  │
│  - users         │  │    _ledger       │
│  - contributions │  │  - audit_log     │
│  - payouts       │  │  (append-only)   │
└──────────────────┘  └──────────────────┘
        PostgreSQL 15+
```

### Key Requirements

| Requirement | Specification |
|-------------|---------------|
| **Version** | PostgreSQL 15.0 or higher |
| **RAM** | Minimum 2GB for local dev |
| **Disk Space** | Minimum 10GB available |
| **Extensions** | `uuid-ossp`, `pgcrypto`, `pg_stat_statements` |

---

## Quick Start

### Option 1: Docker Compose (Recommended)

The fastest way to get started with PostgreSQL for local development.

```bash
# Clone repository (if not already done)
git clone https://github.com/nabeelp/digital-stokvel-vscode-kiro-like.git
cd digital-stokvel-vscode-kiro-like

# Start PostgreSQL (+ Redis) with Docker Compose
docker-compose up -d postgres

# Verify PostgreSQL is running
docker-compose ps

# View logs
docker-compose logs postgres

# Connect to database
docker-compose exec postgres psql -U postgres -d digitalstokvel
```

**Connection Details:**
- **Host:** `localhost`
- **Port:** `5432`
- **Username:** `postgres`
- **Password:** `postgres` (for local dev only)
- **Database:** `digitalstokvel`

### Option 2: Manual Installation

See [Manual PostgreSQL Installation](#manual-postgresql-installation) section below.

---

## Docker Compose Setup

### Docker Compose Configuration

The project includes a `docker-compose.yml` file in the repository root with PostgreSQL configuration:

```yaml
version: '3.8'

services:
  postgres:
    image: postgres:15-alpine
    container_name: digitalstokvel-postgres
    restart: unless-stopped
    environment:
      POSTGRES_USER: postgres
      POSTGRES_PASSWORD: postgres
      POSTGRES_DB: digitalstokvel
    ports:
      - "5432:5432"
    volumes:
      - postgres_data:/var/lib/postgresql/data
      - ./database/init:/docker-entrypoint-initdb.d
      - ./database/migrations:/migrations
    healthcheck:
      test: ["CMD-SHELL", "pg_isready -U postgres"]
      interval: 10s
      timeout: 5s
      retries: 5
    networks:
      - digitalstokvel-network

  postgres-ledger:
    image: postgres:15-alpine
    container_name: digitalstokvel-postgres-ledger
    restart: unless-stopped
    environment:
      POSTGRES_USER: postgres
      POSTGRES_PASSWORD: postgres
      POSTGRES_DB: digitalstokvel_ledger
    ports:
      - "5433:5432"  # Different port to avoid conflicts
    volumes:
      - postgres_ledger_data:/var/lib/postgresql/data
      - ./database/init-ledger:/docker-entrypoint-initdb.d
    healthcheck:
      test: ["CMD-SHELL", "pg_isready -U postgres"]
      interval: 10s
      timeout: 5s
      retries: 5
    networks:
      - digitalstokvel-network

  pgadmin:
    image: dpage/pgadmin4:latest
    container_name: digitalstokvel-pgadmin
    restart: unless-stopped
    environment:
      PGADMIN_DEFAULT_EMAIL: admin@digitalstokvel.local
      PGADMIN_DEFAULT_PASSWORD: admin
      PGADMIN_CONFIG_SERVER_MODE: 'False'
    ports:
      - "5050:80"
    volumes:
      - pgadmin_data:/var/lib/pgadmin
    depends_on:
      - postgres
      - postgres-ledger
    networks:
      - digitalstokvel-network

volumes:
  postgres_data:
    driver: local
  postgres_ledger_data:
    driver: local
  pgadmin_data:
    driver: local

networks:
  digitalstokvel-network:
    driver: bridge
```

### Docker Compose Commands

```bash
# Start all databases
docker-compose up -d

# Start only PostgreSQL (primary)
docker-compose up -d postgres

# Start both PostgreSQL databases
docker-compose up -d postgres postgres-ledger

# Stop databases
docker-compose stop

# Stop and remove containers (data is preserved in volumes)
docker-compose down

# Stop and remove containers AND volumes (⚠️ deletes all data)
docker-compose down -v

# View logs
docker-compose logs -f postgres

# Check container status
docker-compose ps

# Restart PostgreSQL
docker-compose restart postgres
```

### Connecting to PostgreSQL in Docker

#### Using psql (Command Line)

```bash
# Connect to primary database
docker-compose exec postgres psql -U postgres -d digitalstokvel

# Connect to ledger database
docker-compose exec postgres-ledger psql -U postgres -d digitalstokvel_ledger

# Or from host (if psql is installed)
psql -h localhost -p 5432 -U postgres -d digitalstokvel
psql -h localhost -p 5433 -U postgres -d digitalstokvel_ledger
```

#### Using pgAdmin (Web UI)

1. Open browser: `http://localhost:5050`
2. Login:
   - **Email:** `admin@digitalstokvel.local`
   - **Password:** `admin`
3. Add Server:
   - **Name:** DigitalStokvel Primary
   - **Host:** `postgres` (use service name within Docker network)
   - **Port:** `5432`
   - **Username:** `postgres`
   - **Password:** `postgres`

---

## Manual PostgreSQL Installation

### Windows

#### Option 1: Using PostgreSQL Installer (Recommended)

1. Download **PostgreSQL 15** installer from:
   https://www.postgresql.org/download/windows/

2. Run the installer (`postgresql-15.x-windows-x64.exe`)

3. Follow installation wizard:
   - **Installation Directory:** `C:\Program Files\PostgreSQL\15`
   - **Data Directory:** `C:\Program Files\PostgreSQL\15\data`
   - **Password:** Set a strong password for `postgres` user
   - **Port:** `5432` (default)
   - **Locale:** Default locale

4. Install **Stack Builder** components (optional):
   - Select components as needed

5. Verify installation:
   ```powershell
   # Check version
   psql --version
   
   # Connect to default database
   psql -U postgres
   ```

#### Option 2: Using Chocolatey (Package Manager)

```powershell
# Install Chocolatey (if not installed)
Set-ExecutionPolicy Bypass -Scope Process -Force; `
[System.Net.ServicePointManager]::SecurityProtocol = [System.Net.ServicePointManager]::SecurityProtocol -bor 3072; `
iex ((New-Object System.Net.WebClient).DownloadString('https://community.chocolatey.org/install.ps1'))

# Install PostgreSQL 15
choco install postgresql15 -y

# Verify installation
psql --version
```

### macOS

#### Option 1: Using Homebrew (Recommended)

```bash
# Install Homebrew (if not installed)
/bin/bash -c "$(curl -fsSL https://raw.githubusercontent.com/Homebrew/install/HEAD/install.sh)"

# Install PostgreSQL 15
brew install postgresql@15

# Start PostgreSQL service
brew services start postgresql@15

# Verify installation
psql --version

# Connect to default database
psql postgres
```

#### Option 2: Using Postgres.app (GUI)

1. Download **Postgres.app** from:
   https://postgresapp.com/

2. Drag to Applications folder

3. Launch Postgres.app

4. Click **Initialize** to create a new server

5. Add to PATH (add to `~/.zshrc` or `~/.bash_profile`):
   ```bash
   export PATH="/Applications/Postgres.app/Contents/Versions/15/bin:$PATH"
   ```

### Linux (Ubuntu/Debian)

```bash
# Add PostgreSQL APT repository
sudo sh -c 'echo "deb http://apt.postgresql.org/pub/repos/apt $(lsb_release -cs)-pgdg main" > /etc/apt/sources.list.d/pgdg.list'

# Import repository signing key
wget --quiet -O - https://www.postgresql.org/media/keys/ACCC4CF8.asc | sudo apt-key add -

# Update package lists
sudo apt update

# Install PostgreSQL 15
sudo apt install postgresql-15 postgresql-contrib-15 -y

# Start PostgreSQL service
sudo systemctl start postgresql
sudo systemctl enable postgresql

# Verify installation
psql --version

# Connect as postgres user
sudo -u postgres psql
```

### Linux (Fedora/RHEL/CentOS)

```bash
# Install PostgreSQL 15 repository
sudo dnf install -y https://download.postgresql.org/pub/repos/yum/reporpms/F-37-x86_64/pgdg-fedora-repo-latest.noarch.rpm

# Install PostgreSQL 15
sudo dnf install -y postgresql15-server postgresql15-contrib

# Initialize database
sudo /usr/pgsql-15/bin/postgresql-15-setup initdb

# Start and enable service
sudo systemctl start postgresql-15
sudo systemctl enable postgresql-15

# Verify installation
psql --version
```

---

## Database Initialization

### Automated Initialization with Docker

When using Docker Compose, databases are automatically initialized using scripts in `database/init/` and `database/init-ledger/`.

### Manual Database Creation

#### Create Primary Database

```sql
-- Connect to PostgreSQL as postgres user
psql -U postgres

-- Create database
CREATE DATABASE digitalstokvel
    WITH 
    OWNER = postgres
    ENCODING = 'UTF8'
    LC_COLLATE = 'en_US.UTF-8'
    LC_CTYPE = 'en_US.UTF-8'
    TABLESPACE = pg_default
    CONNECTION LIMIT = -1;

-- Connect to the new database
\c digitalstokvel

-- Enable required extensions
CREATE EXTENSION IF NOT EXISTS "uuid-ossp";
CREATE EXTENSION IF NOT EXISTS "pgcrypto";
CREATE EXTENSION IF NOT EXISTS "pg_stat_statements";

-- Verify extensions
\dx
```

#### Create Ledger Database

```sql
-- Connect to PostgreSQL as postgres user
psql -U postgres

-- Create ledger database
CREATE DATABASE digitalstokvel_ledger
    WITH 
    OWNER = postgres
    ENCODING = 'UTF8'
    LC_COLLATE = 'en_US.UTF-8'
    LC_CTYPE = 'en_US.UTF-8'
    TABLESPACE = pg_default
    CONNECTION LIMIT = -1;

-- Connect to the ledger database
\c digitalstokvel_ledger

-- Enable required extensions
CREATE EXTENSION IF NOT EXISTS "uuid-ossp";
CREATE EXTENSION IF NOT EXISTS "pgcrypto";

-- Verify extensions
\dx
```

### Database Initialization Scripts

Create `database/init/01-init-database.sql`:

```sql
-- Digital Stokvel Banking - Primary Database Initialization
-- Version: 1.0
-- Date: 2026-03-24

-- Enable required PostgreSQL extensions
CREATE EXTENSION IF NOT EXISTS "uuid-ossp";
CREATE EXTENSION IF NOT EXISTS "pgcrypto";
CREATE EXTENSION IF NOT EXISTS "pg_stat_statements";

-- Create enum types
CREATE TYPE group_type AS ENUM ('rotating_payout', 'savings_pot', 'investment_club');
CREATE TYPE contribution_frequency AS ENUM ('weekly', 'monthly');
CREATE TYPE contribution_status AS ENUM ('pending', 'paid', 'failed', 'refunded');
CREATE TYPE payout_type AS ENUM ('rotating', 'year_end_pot', 'emergency');
CREATE TYPE payout_status AS ENUM ('pending_approval', 'approved', 'executed', 'failed');
CREATE TYPE member_role AS ENUM ('member', 'secretary', 'treasurer', 'chairperson');
CREATE TYPE member_status AS ENUM ('active', 'suspended', 'left');
CREATE TYPE group_status AS ENUM ('active', 'suspended', 'closed');

-- Success message
SELECT 'Primary database initialized successfully' AS status;
```

Create `database/init-ledger/01-init-ledger-database.sql`:

```sql
-- Digital Stokvel Banking - Ledger Database Initialization
-- Version: 1.0
-- Date: 2026-03-24

-- Enable required PostgreSQL extensions
CREATE EXTENSION IF NOT EXISTS "uuid-ossp";
CREATE EXTENSION IF NOT EXISTS "pgcrypto";

-- Create immutable ledger table
CREATE TABLE IF NOT EXISTS contribution_ledger (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    contribution_id UUID NOT NULL,
    group_id UUID NOT NULL,
    member_id UUID NOT NULL,
    amount DECIMAL(10, 2) NOT NULL CHECK (amount > 0),
    transaction_ref VARCHAR(100) NOT NULL,
    metadata JSONB NOT NULL DEFAULT '{}',
    recorded_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP NOT NULL
);

-- Create indexes for query performance
CREATE INDEX idx_contribution_ledger_contribution_id ON contribution_ledger(contribution_id);
CREATE INDEX idx_contribution_ledger_group_id ON contribution_ledger(group_id);
CREATE INDEX idx_contribution_ledger_member_id ON contribution_ledger(member_id);
CREATE INDEX idx_contribution_ledger_recorded_at ON contribution_ledger(recorded_at DESC);

-- Prevent updates and deletes (enforce append-only)
CREATE OR REPLACE FUNCTION prevent_ledger_modifications()
RETURNS TRIGGER AS $$
BEGIN
    IF TG_OP = 'UPDATE' OR TG_OP = 'DELETE' THEN
        RAISE EXCEPTION 'Cannot modify immutable ledger records';
    END IF;
    RETURN NEW;
END;
$$ LANGUAGE plpgsql;

CREATE TRIGGER prevent_ledger_update_delete
    BEFORE UPDATE OR DELETE ON contribution_ledger
    FOR EACH ROW
    EXECUTE FUNCTION prevent_ledger_modifications();

-- Success message
SELECT 'Ledger database initialized successfully' AS status;
```

---

## Connection Configuration

### Connection Strings

#### Primary Database

```bash
# Development (Docker)
Host=localhost;Port=5432;Database=digitalstokvel;Username=postgres;Password=postgres

# Development (Manual Installation)
Host=localhost;Port=5432;Database=digitalstokvel;Username=postgres;Password=your_password
```

#### Ledger Database

```bash
# Development (Docker)
Host=localhost;Port=5433;Database=digitalstokvel_ledger;Username=postgres;Password=postgres

# Development (Manual Installation)
Host=localhost;Port=5432;Database=digitalstokvel_ledger;Username=postgres;Password=your_password
```

### Application Configuration

Update `appsettings.Development.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=digitalstokvel;Username=postgres;Password=postgres;Include Error Detail=true",
    "LedgerConnection": "Host=localhost;Port=5433;Database=digitalstokvel_ledger;Username=postgres;Password=postgres;Include Error Detail=true"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.EntityFrameworkCore.Database.Command": "Information"
    }
  }
}
```

### Environment Variables (Alternative)

```bash
# Primary Database
export DB_HOST=localhost
export DB_PORT=5432
export DB_NAME=digitalstokvel
export DB_USER=postgres
export DB_PASSWORD=postgres

# Ledger Database
export LEDGER_DB_HOST=localhost
export LEDGER_DB_PORT=5433
export LEDGER_DB_NAME=digitalstokvel_ledger
export LEDGER_DB_USER=postgres
export LEDGER_DB_PASSWORD=postgres
```

---

## Database Migrations

The project uses **Entity Framework Core Migrations** and **Flyway** for database schema management.

### Entity Framework Core Migrations

#### Creating a Migration

```bash
# Navigate to Infrastructure project
cd src/shared/DigitalStokvel.Infrastructure

# Create new migration
dotnet ef migrations add InitialCreate --context ApplicationDbContext

# Review generated migration files in Migrations/ folder
```

#### Applying Migrations

```bash
# Apply all pending migrations
dotnet ef database update --context ApplicationDbContext

# Apply to specific migration
dotnet ef database update MigrationName --context ApplicationDbContext

# Rollback to previous migration
dotnet ef database update PreviousMigrationName --context ApplicationDbContext
```

#### Removing Last Migration (if not applied)

```bash
# Remove last migration
dotnet ef migrations remove --context ApplicationDbContext
```

### SQL Migration Scripts

Located in `database/migrations/`:

```
database/migrations/
├── V001__create_users_table.sql
├── V002__create_groups_table.sql
├── V003__create_group_members_table.sql
├── V004__create_contributions_table.sql
├── V005__create_payouts_table.sql
├── V006__create_votes_table.sql
└── ...
```

---

## pgAdmin Setup

### Accessing pgAdmin

1. **Start pgAdmin** (if using Docker Compose):
   ```bash
   docker-compose up -d pgadmin
   ```

2. **Open browser:** `http://localhost:5050`

3. **Login credentials:**
   - Email: `admin@digitalstokvel.local`
   - Password: `admin`

### Adding Server Connection

1. Right-click **Servers** → **Create** → **Server**

2. **General Tab:**
   - Name: `DigitalStokvel Primary`

3. **Connection Tab:**
   - Host name/address: `postgres` (if in Docker network) or `localhost`
   - Port: `5432`
   - Maintenance database: `digitalstokvel`
   - Username: `postgres`
   - Password: `postgres`
   - Save password: Yes (optional)

4. Click **Save**

5. Repeat for Ledger Database:
   - Name: `DigitalStokvel Ledger`
   - Host: `postgres-ledger` or `localhost`
   - Port: `5433`
   - Database: `digitalstokvel_ledger`

---

## Common Operations

### Backup Database

```bash
# Backup using pg_dump (Docker)
docker-compose exec postgres pg_dump -U postgres digitalstokvel > backup_$(date +%Y%m%d).sql

# Backup using pg_dump (Manual installation)
pg_dump -U postgres -d digitalstokvel -f backup_$(date +%Y%m%d).sql

# Backup with compression
pg_dump -U postgres -d digitalstokvel | gzip > backup_$(date +%Y%m%d).sql.gz
```

### Restore Database

```bash
# Restore from SQL file (Docker)
docker-compose exec -T postgres psql -U postgres -d digitalstokvel < backup.sql

# Restore from SQL file (Manual installation)
psql -U postgres -d digitalstokvel < backup.sql

# Restore from compressed file
gunzip -c backup.sql.gz | psql -U postgres -d digitalstokvel
```

### Reset Database

```bash
# Drop and recreate database (Docker)
docker-compose exec postgres psql -U postgres -c "DROP DATABASE IF EXISTS digitalstokvel;"
docker-compose exec postgres psql -U postgres -c "CREATE DATABASE digitalstokvel;"

# Or use reset script
./scripts/reset-database.sh
```

### Check Database Size

```sql
-- Connect to database
psql -U postgres -d digitalstokvel

-- Check database size
SELECT 
    pg_database.datname AS database_name,
    pg_size_pretty(pg_database_size(pg_database.datname)) AS size
FROM pg_database
WHERE datname = 'digitalstokvel';

-- Check table sizes
SELECT 
    schemaname || '.' || tablename AS table_name,
    pg_size_pretty(pg_total_relation_size(schemaname || '.' || tablename)) AS size
FROM pg_tables
WHERE schemaname = 'public'
ORDER BY pg_total_relation_size(schemaname || '.' || tablename) DESC;
```

### Monitor Active Connections

```sql
-- View active connections
SELECT 
    pid,
    usename,
    application_name,
    client_addr,
    state,
    query_start,
    LEFT(query, 50) AS query
FROM pg_stat_activity
WHERE datname = 'digitalstokvel'
ORDER BY query_start DESC;

-- Kill specific connection
SELECT pg_terminate_backend(pid)
FROM pg_stat_activity
WHERE datname = 'digitalstokvel' AND pid = <pid_number>;
```

---

## Troubleshooting

### Issue: "psql: error: connection to server at localhost (::1), port 5432 failed"

**Cause:** PostgreSQL service not running.

**Solution:**
```bash
# Docker
docker-compose up -d postgres

# Windows (Manual installation)
# Services → PostgreSQL → Start

# macOS (Homebrew)
brew services start postgresql@15

# Linux
sudo systemctl start postgresql
```

### Issue: "FATAL: password authentication failed for user postgres"

**Cause:** Incorrect password or authentication method.

**Solution:**
1. Reset password (Docker):
   ```bash
   docker-compose exec postgres psql -U postgres -c "ALTER USER postgres PASSWORD 'postgres';"
   ```

2. Edit `pg_hba.conf` (Manual installation):
   - Windows: `C:\Program Files\PostgreSQL\15\data\pg_hba.conf`
   - macOS/Linux: `/var/lib/postgresql/15/data/pg_hba.conf`
   
   Change authentication method to `trust` temporarily:
   ```
   # TYPE  DATABASE        USER            ADDRESS                 METHOD
   local   all             all                                     trust
   host    all             all             127.0.0.1/32            trust
   host    all             all             ::1/128                 trust
   ```
   
   Restart PostgreSQL service.

### Issue: "Port 5432 is already in use"

**Cause:** Another PostgreSQL instance or application using port 5432.

**Solution:**
```bash
# Find process using port 5432
# Windows
netstat -ano | findstr :5432
taskkill /PID <PID> /F

# macOS/Linux
lsof -i :5432
kill -9 <PID>

# Or change Docker port in docker-compose.yml
ports:
  - "5433:5432"  # Use different host port
```

### Issue: "Could not connect to database: Connection refused"

**Cause:** PostgreSQL not accepting connections.

**Solution:**
1. Check `postgresql.conf`:
   ```ini
   listen_addresses = '*'  # or 'localhost'
   port = 5432
   ```

2. Check firewall rules

3. Restart PostgreSQL

### Issue: Database migrations not applying

**Cause:** Entity Framework context issues.

**Solution:**
```bash
# Clean build
dotnet clean
dotnet build

# Remove last migration and recreate
dotnet ef migrations remove
dotnet ef migrations add InitialCreate

# Apply with verbose output
dotnet ef database update --verbose
```

---

## Next Steps

After setting up PostgreSQL:

1. **Set up Redis:**
   - See `docs/LOCAL_REDIS_SETUP.md` (Task 0.1.4)

2. **Configure Docker Desktop:**
   - See `docs/DOCKER_SETUP.md` (Task 0.1.5)

3. **Run database migrations:**
   ```bash
   dotnet ef database update
   ```

4. **Run the application:**
   ```bash
   cd src/gateways/DigitalStokvel.ApiGateway
   dotnet run
   ```

5. **Verify database connection:**
   - Open Swagger UI: `https://localhost:5001/swagger`
   - Test API endpoints that interact with database

---

## Additional Resources

### Documentation
- [PostgreSQL Documentation](https://www.postgresql.org/docs/15/index.html)
- [Entity Framework Core Documentation](https://docs.microsoft.com/en-us/ef/core/)
- [Npgsql Documentation](https://www.npgsql.org/doc/)
- [pgAdmin Documentation](https://www.pgadmin.org/docs/)

### Team Resources
- **Technical Design:** `.kiro-like/specs/design.md` (Section 4: Data Models)
- **Database Schemas:** `database/migrations/`
- **Entity Models:** `src/shared/DigitalStokvel.Domain/Entities/`
- **DbContext:** `src/shared/DigitalStokvel.Infrastructure/Data/ApplicationDbContext.cs`

### Contact
- **Database Team:** Slack: `#database-engineering`
- **DevOps:** Slack: `#platform-engineering`
- **Questions:** Slack: `#engineering-help`

---

**Document Version:** 1.0  
**Last Updated:** March 24, 2026  
**Maintained by:** Engineering Team  
**Review Frequency:** Quarterly or when PostgreSQL version updates

# Digital Stokvel - Database Migrations

This directory contains versioned SQL migration scripts for the Digital Stokvel Banking platform database schema.

## Overview

The Digital Stokvel platform uses **versioned SQL migrations** to manage database schema changes across environments. Migrations are applied using **DbUp**, a lightweight .NET library that tracks which migrations have been executed and applies only new migrations.

### Key Features

- **Versioned Migrations:** Each migration has a version number (V001, V002, etc.) ensuring predictable execution order
- **Idempotent:** Migrations are tracked and only run once per database
- **Rollback Support:** Each migration has a corresponding rollback script for emergency reversals
- **Audit Trail:** DbUp maintains a `schemaversions` table tracking all applied migrations
- **Cross-Environment:** Same scripts work across development, staging, and production

---

## Directory Structure

```
database/
├── init/
│   └── 01-init-database.sql       # Database initialization (extensions, enums)
├── init-ledger/
│   └── 01-init-ledger-database.sql # Ledger database initialization
├── migrations/
│   ├── V001__create_core_schema.sql
│   ├── V002__create_contributions_tables.sql
│   ├── V003__create_payouts_tables.sql
│   ├── V004__create_votes_tables.sql
│   ├── V005__create_disputes_table.sql
│   ├── V006__create_notifications_table.sql
│   └── V007__create_credit_profiles_table.sql
└── rollback/
    ├── U001__drop_core_schema.sql
    ├── U002__drop_contributions_tables.sql
    ├── U003__drop_payouts_tables.sql
    ├── U004__drop_votes_tables.sql
    ├── U005__drop_disputes_table.sql
    ├── U006__drop_notifications_table.sql
    └── U007__drop_credit_profiles_table.sql
```

---

## Migration Naming Convention

### Forward Migrations (Vxxx__description.sql)

- **Format:** `V{version}__{description}.sql`
- **Version:** Zero-padded 3-digit number (V001, V002, ..., V999)
- **Description:** Lowercase with underscores, describes what the migration does
- **Examples:**
  - `V001__create_core_schema.sql`
  - `V010__add_user_preferences_table.sql`
  - `V025__add_index_to_contributions.sql`

### Rollback Scripts (Uxxx__description.sql)

- **Format:** `U{version}__{description}.sql`
- **Version:** Must match the corresponding forward migration version
- **Description:** Same as forward migration, describing what is being undone
- **Examples:**
  - `U001__drop_core_schema.sql`
  - `U010__drop_user_preferences_table.sql`
  - `U025__remove_index_from_contributions.sql`

---

## Migration Tool: DbUp

We use **DbUp** as our migration framework. DbUp is a .NET library that:

1. Connects to the target database
2. Creates a `schemaversions` table to track applied migrations
3. Compares available scripts with executed scripts
4. Runs pending migrations in version order
5. Records successful executions in the tracking table

### Installation

The migration runner is a .NET console application located in `tools/DigitalStokvel.DbMigrator/`.

```bash
cd tools/DigitalStokvel.DbMigrator
dotnet restore
dotnet build
```

---

## Running Migrations

### Prerequisites

- PostgreSQL 15+ server running
- Database created (or use auto-create feature)
- Connection string with appropriate permissions

### Using the Migration Runner

#### Option 1: Command Line Argument

```bash
cd tools/DigitalStokvel.DbMigrator

# Run all pending migrations
dotnet run migrate "Host=localhost;Port=5432;Database=digitalstokvel;Username=stokvel_admin;Password=Dev_Password_2026!"

# Check migration status
dotnet run status "Host=localhost;Port=5432;Database=digitalstokvel;Username=stokvel_admin;Password=Dev_Password_2026!"
```

#### Option 2: Environment Variable

```bash
# Windows (PowerShell)
$env:POSTGRES_CONNECTION_STRING="Host=localhost;Port=5432;Database=digitalstokvel;Username=stokvel_admin;Password=Dev_Password_2026!"
dotnet run migrate

# Linux/macOS
export POSTGRES_CONNECTION_STRING="Host=localhost;Port=5432;Database=digitalstokvel;Username=stokvel_admin;Password=Dev_Password_2026!"
dotnet run migrate
```

#### Option 3: Individual Environment Variables (Docker Compose Style)

```bash
# Windows (PowerShell)
$env:POSTGRES_HOST="localhost"
$env:POSTGRES_PORT="5432"
$env:POSTGRES_DB="digitalstokvel"
$env:POSTGRES_USER="stokvel_admin"
$env:POSTGRES_PASSWORD="Dev_Password_2026!"
dotnet run migrate

# Linux/macOS
export POSTGRES_HOST=localhost
export POSTGRES_PORT=5432
export POSTGRES_DB=digitalstokvel
export POSTGRES_USER=stokvel_admin
export POSTGRES_PASSWORD=Dev_Password_2026!
dotnet run migrate
```

### Migration Output

Successful migration:
```
=======================================================
Digital Stokvel - Database Migration Runner
=======================================================

Connecting to database...

Found 7 migration(s) to execute:
  - V001__create_core_schema.sql
  - V002__create_contributions_tables.sql
  - V003__create_payouts_tables.sql
  - V004__create_votes_tables.sql
  - V005__create_disputes_table.sql
  - V006__create_notifications_table.sql
  - V007__create_credit_profiles_table.sql

Executing migrations...
✓ SUCCESS! Database migrations completed successfully.
  ✓ V001__create_core_schema.sql
  ✓ V002__create_contributions_tables.sql
  ✓ V003__create_payouts_tables.sql
  ✓ V004__create_votes_tables.sql
  ✓ V005__create_disputes_table.sql
  ✓ V006__create_notifications_table.sql
  ✓ V007__create_credit_profiles_table.sql
```

Already up to date:
```
✓ Database is up to date. No migrations to run.
```

---

## Current Migrations

### V001: Core Schema (March 24, 2026)
**Tables:** users, groups, group_members, group_savings_accounts  
**Indexes:** 24 performance-optimized indexes  
**Features:**
- User authentication and KYC tracking
- Group management with type and contribution settings
- Member roles (Chairperson, Treasurer, Secretary, Member)
- Group savings account tracking with interest rates

### V002: Contributions (March 24, 2026)
**Tables:** contributions, contribution_ledger, recurring_payments  
**Indexes:** 12 indexes for query optimization  
**Features:**
- Contribution tracking with status lifecycle
- Immutable ledger for audit trail (append-only)
- Automated recurring payment scheduling

### V003: Payouts (March 24, 2026)
**Tables:** payouts, payout_disbursements  
**Indexes:** 10 indexes for payout queries  
**Features:**
- Dual approval workflow (Chairperson + Treasurer)
- Three payout types: rotating, year_end_pot, emergency
- Individual disbursement tracking per member

### V004: Votes (March 24, 2026)
**Tables:** votes, vote_responses  
**Indexes:** 8 indexes for voting queries  
**Features:**
- Democratic decision-making within groups
- Quorum calculation based on constitution
- Vote participation rate tracking

### V005: Disputes (March 24, 2026)
**Tables:** disputes  
**Indexes:** 6 indexes for dispute management  
**Features:**
- Four dispute types: missed_payment, fraud, constitution_violation, other
- Evidence storage as JSONB
- Auto-escalation for overdue disputes (21+ days)

### V006: Notifications (March 24, 2026)
**Tables:** notifications  
**Indexes:** 7 indexes for delivery tracking  
**Features:**
- Multi-channel support: push, sms, ussd
- Template-based messaging with dynamic payload
- Retry logic for failed deliveries (max 3 retries)

### V007: Credit Profiles (March 24, 2026)
**Tables:** credit_profiles  
**Indexes:** 6 indexes for score calculation  
**Features:**
- Stokvel Score (0-100) based on payment behavior
- Contribution streak and consistency tracking
- Optional credit bureau reporting with consent
- Pre-qualification eligibility for loans

---

## Rollback Process

**WARNING:** Rollbacks should only be executed in emergency situations and require careful validation.

### When to Rollback

- Critical bug discovered in production immediately after deployment
- Data corruption detected
- Performance issues requiring immediate reversion
- Only rollback the **most recent** migration if possible

### Rollback Procedure

1. **Identify the migration to rollback:** Check `schemaversions` table
2. **Run the corresponding Uxxx script manually:**
   ```sql
   psql -h localhost -U stokvel_admin -d digitalstokvel -f database/rollback/U007__drop_credit_profiles_table.sql
   ```
3. **Remove the entry from schemaversions table:**
   ```sql
   DELETE FROM public.schemaversions 
   WHERE scriptname = 'V007__create_credit_profiles_table.sql';
   ```
4. **Verify the rollback:**
   ```bash
   dotnet run status
   ```

### Rollback Best Practices

- **Always test rollback scripts in non-production first**
- **Take a database backup before rollback**
- **Communicate with the team before executing**
- **Document the reason for rollback**
- **Consider forward-fixing instead of rollback when possible**

---

## Development Workflow

### Creating a New Migration

1. **Determine the next version number:**
   - Check existing migrations: `ls database/migrations/`
   - Next version is highest + 1

2. **Create the migration file:**
   ```bash
   touch database/migrations/V008__add_feature_name.sql
   ```

3. **Write the migration SQL:**
   ```sql
   -- Migration: V008 - Add Feature Name
   -- Description: Detailed description of changes
   -- Author: Your Name
   -- Date: YYYY-MM-DD
   -- Dependencies: V007

   CREATE TABLE IF NOT EXISTS new_table (
       id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
       ...
   );

   CREATE INDEX idx_new_table_field ON new_table(field);
   ```

4. **Create the rollback script:**
   ```bash
   touch database/rollback/U008__drop_feature_name.sql
   ```

5. **Write the rollback SQL:**
   ```sql
   -- Rollback Migration: U008 - Drop Feature Name
   -- Rollback For: V008__add_feature_name.sql

   DROP INDEX IF EXISTS idx_new_table_field;
   DROP TABLE IF EXISTS new_table CASCADE;
   ```

6. **Test the migration:**
   ```bash
   # Apply migration
   cd tools/DigitalStokvel.DbMigrator
   dotnet run migrate

   # Verify
   dotnet run status

   # Test rollback
   psql -h localhost -U stokvel_admin -d digitalstokvel -f ../../database/rollback/U008__drop_feature_name.sql
   ```

7. **Commit both files:**
   ```bash
   git add database/migrations/V008__add_feature_name.sql
   git add database/rollback/U008__drop_feature_name.sql
   git commit -m "feat(database): add feature name (Task X.X.X)"
   ```

---

## CI/CD Integration

### GitHub Actions Example

```yaml
name: Database Migrations

on:
  push:
    branches: [main]
    paths:
      - 'database/migrations/**'

jobs:
  migrate:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v3
      
      - name: Setup .NET
        uses: actions/setup-dotnet@v3
        with:
          dotnet-version: '10.0.x'
      
      - name: Run Migrations
        run: |
          cd tools/DigitalStokvel.DbMigrator
          dotnet run migrate
        env:
          POSTGRES_CONNECTION_STRING: ${{ secrets.POSTGRES_CONNECTION_STRING }}
```

---

## Troubleshooting

### Migration Fails with "Permission Denied"

**Problem:** User lacks necessary database permissions.

**Solution:** Ensure the migration user has:
```sql
GRANT CREATE ON DATABASE digitalstokvel TO stokvel_admin;
GRANT ALL PRIVILEGES ON SCHEMA public TO stokvel_admin;
```

### Migration Already Applied But Not in schemaversions

**Problem:** Manual schema changes or corrupted tracking table.

**Solution:** Manually insert the missing entry:
```sql
INSERT INTO public.schemaversions (scriptname, applied)
VALUES ('V007__create_credit_profiles_table.sql', NOW());
```

### Port 5432 Already in Use

**Problem:** Another PostgreSQL instance is running.

**Solution:**
- Use a different port in connection string
- Stop the conflicting instance
- Use Docker Compose to isolate databases

### Connection Timeout

**Problem:** Database not accessible or network issues.

**Solution:**
- Verify PostgreSQL is running: `docker ps` or `pg_ctl status`
- Check firewall rules
- Verify connection string accuracy

---

## Production Deployment Checklist

- [ ] All new migrations tested in development environment
- [ ] Rollback scripts tested in development environment
- [ ] Database backup completed
- [ ] Downtime window communicated (if required)
- [ ] Migration script reviewed by senior developer
- [ ] Performance impact assessed (especially for large tables)
- [ ] Connection string for production verified
- [ ] Post-migration verification queries prepared
- [ ] Rollback plan documented and ready
- [ ] Team on standby during deployment

---

## Maintenance

### Viewing Applied Migrations

```sql
SELECT * FROM public.schemaversions 
ORDER BY applied DESC;
```

### Database Size Monitoring

```sql
SELECT 
    pg_size_pretty(pg_database_size('digitalstokvel')) AS database_size,
    pg_size_pretty(pg_total_relation_size('users')) AS users_table_size,
    pg_size_pretty(pg_total_relation_size('contributions')) AS contributions_table_size;
```

### Index Usage Statistics

```sql
SELECT 
    schemaname,
    tablename,
    indexname,
    idx_scan,
    idx_tup_read,
    idx_tup_fetch
FROM pg_stat_user_indexes
WHERE schemaname = 'public'
ORDER BY idx_scan DESC;
```

---

## Additional Resources

- **DbUp Documentation:** https://dbup.readthedocs.io/
- **PostgreSQL Documentation:** https://www.postgresql.org/docs/15/
- **Design Document:** See `.kiro-like/specs/design.md` for schema rationale
- **Tasks Tracker:** See `.kiro-like/specs/tasks.md` for implementation roadmap

---

## Support

For questions or issues with migrations:

1. Check this README first
2. Review the troubleshooting section
3. Consult the team lead or database administrator
4. Check PostgreSQL logs: `docker logs postgres` or `/var/log/postgresql/`

---

**Last Updated:** March 24, 2026  
**Migration Version:** V007  
**Total Tables:** 13 core tables  
**Total Indexes:** 73+ performance-optimized indexes

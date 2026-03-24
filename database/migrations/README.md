# Digital Stokvel Banking Platform - Database Migrations

This directory contains SQL migration scripts for database schema changes.

## Migration Naming Convention

Migrations follow the format: `V{version}__{description}.sql`

- **V** prefix indicates a versioned migration
- **Version** is a sequential number with leading zeros (e.g., 001, 002, 003)
- **Double underscore** separates version from description
- **Description** uses snake_case

### Examples:
- `V001__create_users_table.sql`
- `V002__create_groups_table.sql`
- `V003__add_constitution_to_groups.sql`

## Migration Tools

The project supports two migration approaches:

### 1. Entity Framework Core Migrations (C# Code-First)

Located in: `src/shared/DigitalStokvel.Infrastructure/Migrations/`

Generate migrations:
```bash
cd src/shared/DigitalStokvel.Infrastructure
dotnet ef migrations add MigrationName --context ApplicationDbContext
```

Apply migrations:
```bash
dotnet ef database update --context ApplicationDbContext
```

### 2. SQL-Based Migrations (Flyway/Manual)

Located in: `database/migrations/`

SQL migrations are versioned and can be applied using:
- Flyway CLI
- Manual execution via psql
- CI/CD pipelines

## Rollback Strategy

For each migration, create a corresponding rollback script in `database/rollback/`:

- Forward: `V001__create_users_table.sql`
- Rollback: `U001__drop_users_table.sql`

## Migration Best Practices

1. **Always test migrations** on a local database first
2. **Make migrations reversible** when possible
3. **Use transactions** for data migrations
4. **Document breaking changes** in the migration file
5. **Never modify applied migrations** - create new ones instead
6. **Backup production** before applying migrations

## Migration Status

Track applied migrations in the `flyway_schema_history` table:

```sql
SELECT * FROM flyway_schema_history ORDER BY installed_rank DESC;
```

## Upcoming Migrations

List of planned migrations (to be created):

- [ ] V001__create_users_table.sql
- [ ] V002__create_groups_table.sql
- [ ] V003__create_group_members_table.sql
- [ ] V004__create_group_savings_accounts_table.sql
- [ ] V005__create_contributions_table.sql
- [ ] V006__create_recurring_payments_table.sql
- [ ] V007__create_payouts_table.sql
- [ ] V008__create_payout_disbursements_table.sql
- [ ] V009__create_votes_table.sql
- [ ] V010__create_vote_responses_table.sql
- [ ] V011__create_disputes_table.sql
- [ ] V012__create_notifications_table.sql
- [ ] V013__create_credit_profiles_table.sql

---

**Note:** Migrations will be created incrementally as Entity Framework models are finalized.

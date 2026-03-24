# Security Best Practices - Digital Stokvel Banking

**Version:** 1.0  
**Last Updated:** March 24, 2026  
**Status:** Active

---

## Overview

This document outlines security best practices for the Digital Stokvel Banking platform, with a focus on preventing credential leaks and protecting sensitive data.

---

## ⚠️ Critical Security Rules

### 1. NEVER Commit Passwords or Secrets to Version Control

**What NOT to commit:**
- ❌ Database passwords
- ❌ API keys
- ❌ JWT signing keys
- ❌ Service account credentials
- ❌ Azure/AWS credentials
- ❌ SSL/TLS private keys
- ❌ Any `.env` files with real credentials

**What IS safe to commit:**
- ✅ `.env.example` (template with placeholder values)
- ✅ Configuration files referencing environment variables
- ✅ Public keys (but not private keys)
- ✅ Documentation showing how to set up secrets

---

## Environment Variables Management

### Local Development Setup

1. **Copy the example file:**
   ```bash
   cp .env.example .env
   ```

2. **Edit `.env` with secure passwords:**
   ```bash
   # Use a password manager or generate secure passwords
   # Example: openssl rand -base64 32
   
   POSTGRES_PASSWORD=your_secure_password_here
   POSTGRES_LEDGER_PASSWORD=another_secure_password
   REDIS_PASSWORD=redis_secure_password
   RABBITMQ_PASSWORD=rabbitmq_secure_password
   PGADMIN_PASSWORD=pgadmin_secure_password
   ```

3. **Verify `.env` is in `.gitignore`:**
   ```bash
   # .gitignore should contain:
   .env
   .env.local
   .env.*.local
   ```

4. **Start services:**
   ```bash
   docker-compose up -d
   ```

### Required Environment Variables

| Variable | Description | Example |
|----------|-------------|---------|
| `POSTGRES_PASSWORD` | Primary PostgreSQL password | `(generate secure password)` |
| `POSTGRES_LEDGER_PASSWORD` | Ledger PostgreSQL password | `(generate secure password)` |
| `REDIS_PASSWORD` | Redis cache password | `(generate secure password)` |
| `RABBITMQ_PASSWORD` | RabbitMQ password | `(generate secure password)` |
| `PGADMIN_PASSWORD` | pgAdmin UI password | `(generate secure password)` |

---

## Docker Compose Security

### Environment Variable Syntax

The `docker-compose.yml` uses the following syntax:

```yaml
environment:
  # Required variable - fails if not set
  POSTGRES_PASSWORD: ${POSTGRES_PASSWORD:?POSTGRES_PASSWORD must be set in .env file}
  
  # Optional variable with default value
  POSTGRES_USER: ${POSTGRES_USER:-stokvel_admin}
```

**Syntax explanation:**
- `${VAR:?error message}` - Variable is required, fail with error if not set
- `${VAR:-default}` - Variable is optional, use default if not set
- `${VAR}` - Use variable value

### Validation Before Starting

Docker Compose will validate that all required variables are set:

```bash
$ docker-compose up
ERROR: POSTGRES_PASSWORD must be set in .env file
```

---

## GitHub Secrets Management

### Setting up GitHub Secrets

1. **Navigate to:** Repository Settings → Secrets and Variables → Actions

2. **Add required secrets:**
   - `REGISTRY_USERNAME` - Container registry username
   - `REGISTRY_PASSWORD` - Container registry password/token
   - `AZURE_CREDENTIALS_STAGING` - Azure service principal JSON
   - `AZURE_CREDENTIALS_PRODUCTION` - Azure service principal JSON
   - `STAGING_RESOURCE_GROUP` - Azure resource group name
   - `STAGING_CLUSTER_NAME` - AKS cluster name
   - `PRODUCTION_RESOURCE_GROUP` - Azure resource group name
   - `PRODUCTION_CLUSTER_NAME` - AKS cluster name

3. **Use in workflows:**
   ```yaml
   - name: Login to registry
     uses: docker/login-action@v3
     with:
       registry: digitalstokvel.azurecr.io
       username: ${{ secrets.REGISTRY_USERNAME }}
       password: ${{ secrets.REGISTRY_PASSWORD }}
   ```

### GitHub Environments

Create protected environments with approval gates:

1. **Staging Environment:**
   - No approval required
   - Automatic deployment on `develop` branch

2. **Production Environment:**
   - **Required reviewers:** 2 approvers
   - **Wait timer:** 5 minutes
   - **Branch restrictions:** Only `main` branch
   - Manual approval required

---

## Password Generation

### Generating Secure Passwords

**Using OpenSSL:**
```bash
# Generate 32-character password
openssl rand -base64 32

# Generate 16-character alphanumeric
openssl rand -hex 16
```

**Using PowerShell:**
```powershell
# Generate 32-character password
-join ((48..57) + (65..90) + (97..122) | Get-Random -Count 32 | % {[char]$_})
```

**Password Requirements:**
- Minimum 16 characters
- Mix of uppercase, lowercase, numbers, special characters
- Different password for each service
- Store in password manager (1Password, LastPass, Bitwarden)

---

## Azure Key Vault (Production)

### Using Key Vault for Secrets

For production deployments, store secrets in Azure Key Vault:

**1. Create Key Vault:**
```bash
az keyvault create \
  --name stokvel-prod-keyvault \
  --resource-group rg-stokvel-prod \
  --location southafricanorth
```

**2. Add secrets:**
```bash
az keyvault secret set \
  --vault-name stokvel-prod-keyvault \
  --name postgres-password \
  --value "$(openssl rand -base64 32)"
```

**3. Grant access to services:**
```bash
# Managed Identity
az keyvault set-policy \
  --name stokvel-prod-keyvault \
  --object-id <managed-identity-id> \
  --secret-permissions get list
```

**4. Reference in application:**
```csharp
// Use Azure.Security.KeyVault.Secrets
var client = new SecretClient(
    new Uri("https://stokvel-prod-keyvault.vault.azure.net/"),
    new DefaultAzureCredential()
);

KeyVaultSecret secret = await client.GetSecretAsync("postgres-password");
string password = secret.Value;
```

---

## Connection Strings

### Secure Connection String Format

**PostgreSQL:**
```
Host=localhost;Port=5432;Database=digitalstokvel;Username=stokvel_admin;Password=${POSTGRES_PASSWORD}
```

**Never hardcode:**
```csharp
// ❌ BAD - Hardcoded password
var connectionString = "Host=localhost;Password=Dev@Password123";

// ✅ GOOD - From environment variable
var connectionString = Environment.GetEnvironmentVariable("DATABASE_URL");

// ✅ BETTER - From Azure Key Vault
var connectionString = await keyVaultClient.GetSecretAsync("database-connection");
```

---

## Secret Scanning

### Pre-commit Hooks

Install git-secrets to prevent accidental commits:

```bash
# Install git-secrets
git clone https://github.com/awslabs/git-secrets
cd git-secrets
make install

# Set up for repository
cd /path/to/digital-stokvel
git secrets --install
git secrets --register-aws
```

### GitHub Secret Scanning

GitHub automatically scans for known secret patterns:
- API keys
- OAuth tokens
- Private keys
- Cloud credentials

**If a secret is detected:**
1. **Revoke** the exposed secret immediately
2. **Rotate** to a new secret
3. **Update** in GitHub Secrets / Key Vault
4. **Remove** from git history:
   ```bash
   git filter-branch --force --index-filter \
     "git rm --cached --ignore-unmatch path/to/file" \
     --prune-empty --tag-name-filter cat -- --all
   ```

### TruffleHog Scanning

The PR validation workflow includes TruffleHog for secret detection:

```yaml
- name: Check for secrets
  uses: trufflesecurity/trufflehog@main
  with:
    path: ./
    base: ${{ github.event.pull_request.base.sha }}
    head: ${{ github.event.pull_request.head.sha }}
```

---

## Team Access Control

### Principle of Least Privilege

**Repository Access:**
- **Admins:** Product Owner, Tech Lead (2 people)
- **Maintainers:** Senior Developers (3-4 people)
- **Developers:** Write access, cannot approve own PRs
- **Contractors:** Read access, require approval for PRs

**Secret Access:**
- **Admins:** Full access to all secrets
- **Developers:** No access to production secrets
- **CI/CD:** Read-only access via service accounts

---

## Security Checklist

### Before Committing

- [ ] No hardcoded passwords in code
- [ ] No API keys in source files
- [ ] `.env` file is in `.gitignore`
- [ ] `.env.example` has placeholder values only
- [ ] Connection strings use environment variables
- [ ] Secrets referenced via `${{ secrets.NAME }}` in workflows

### Before Deploying

- [ ] All secrets are in GitHub Secrets or Key Vault
- [ ] Service accounts have minimal required permissions
- [ ] SSL/TLS certificates are valid
- [ ] Firewall rules are properly configured
- [ ] Audit logging is enabled
- [ ] Backup and recovery tested

### Production Security

- [ ] Use Azure Key Vault for all secrets
- [ ] Enable Azure AD authentication where possible
- [ ] Use Managed Identities (no passwords)
- [ ] Enable network security groups (NSGs)
- [ ] Enable Azure Firewall
- [ ] Configure Private Link/Private Endpoints
- [ ] Enable Azure Security Center
- [ ] Set up Azure Sentinel for SIEM

---

## Incident Response

### If a Secret is Leaked

**Immediate Actions (within 1 hour):**
1. **Revoke** the compromised secret
2. **Rotate** to a new secret
3. **Notify** security team and stakeholders
4. **Monitor** for unauthorized access
5. **Document** incident in security log

**Follow-up (within 24 hours):**
1. **Investigate** how leak occurred
2. **Review** git history for other leaks
3. **Update** security procedures
4. **Train** team on prevention
5. **Implement** additional safeguards

### Emergency Contacts

- **Security Team:** security@digitalstokvel.local
- **On-call Engineer:** +27 XX XXX XXXX
- **Incident Commander:** TBD

---

## Compliance Requirements

### POPIA (Protection of Personal Information Act)

- **Data at Rest:** AES-256 encryption
- **Data in Transit:** TLS 1.3
- **Access Logs:** 7-year retention
- **Secret Rotation:** Every 90 days for production

### FICA (Financial Intelligence Centre Act)

- **Audit Trail:** All credential access logged
- **Access Control:** Role-based, least privilege
- **Encryption:** All financial data encrypted
- **Backup:** Encrypted backups, offsite storage

---

## References

- [OWASP Secrets Management Cheat Sheet](https://cheatsheetsecurity.gitbook.io/owasp-cheat-sheet/secrets-management-cheat-sheet)
- [Azure Key Vault Best Practices](https://docs.microsoft.com/en-us/azure/key-vault/general/best-practices)
- [GitHub Secret Scanning](https://docs.github.com/en/code-security/secret-scanning)
- [12 Factor App - Config](https://12factor.net/config)

---

**Document Status:** Active  
**Last Updated:** March 24, 2026  
**Next Review:** Quarterly

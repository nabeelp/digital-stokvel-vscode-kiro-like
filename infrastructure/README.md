# Digital Stokvel - Azure Infrastructure

This directory contains Infrastructure as Code (IaC) templates using **Azure Bicep** for provisioning the Digital Stokvel Banking platform on Azure.

## Overview

The infrastructure is designed for a cloud-native microservices architecture deployed on **Azure Kubernetes Service (AKS)** with managed Azure services for data, caching, messaging, storage, and monitoring.

### Architecture Components

1. **Compute:** Azure Kubernetes Service (AKS) with system and application node pools
2. **Database:** Azure Database for PostgreSQL Flexible Server (primary + ledger)
3. **Caching:** Azure Cache for Redis with clustering for Production
4. **Messaging:** Azure Service Bus with queues and topics
5. **Storage:** Azure Blob Storage for receipts, evidence, and documents
6. **Monitoring:** Azure Monitor, Application Insights, Log Analytics, and Managed Grafana
7. **Container Registry:** Azure Container Registry (ACR) for Docker images

---

## Directory Structure

```
infrastructure/
├── main.bicep                     # Main orchestration template
├── main.parameters.json           # Main parameters file
├── aks.bicep                      # AKS cluster configuration
├── aks.parameters.json
├── postgres.bicep                 # PostgreSQL databases (primary + ledger)
├── postgres.parameters.json
├── redis.bicep                    # Redis cache configuration
├── redis.parameters.json
├── servicebus.bicep               # Service Bus queues and topics
├── servicebus.parameters.json
├── storage.bicep                  # Blob Storage containers
├── storage.parameters.json
├── monitoring.bicep               # Monitoring infrastructure
├── monitoring.parameters.json
├── acr.bicep                      # Container Registry
├── acr.parameters.json
└── README.md                      # This file
```

---

## Prerequisites

### Required Tools

- **Azure CLI:** Version 2.50.0 or later
  ```bash
  az --version
  az upgrade
  ```

- **Azure Bicep:** Installed with Azure CLI or standalone
  ```bash
  az bicep version
  az bicep upgrade
  ```

- **Permissions:** Subscription Contributor or Owner role

### Azure Subscription Setup

1. **Login to Azure:**
   ```bash
   az login
   az account list --output table
   az account set --subscription "<subscription-id>"
   ```

2. **Create Resource Group:**
   ```bash
   # Staging
   az group create --name rg-digitalstokvel-staging --location southafricanorth

   # Production
   az group create --name rg-digitalstokvel-prod --location southafricanorth
   ```

---

## Deployment

### Option 1: Deploy All Infrastructure (Recommended)

Deploy all components together using the main template:

```bash
# Set variables
RESOURCE_GROUP="rg-digitalstokvel-staging"
LOCATION="southafricanorth"
ENVIRONMENT="Staging"
POSTGRES_PASSWORD="YourSecurePassword123!"  # Use secure password!

# Deploy all infrastructure
az deployment group create \
  --resource-group $RESOURCE_GROUP \
  --template-file main.bicep \
  --parameters main.parameters.json \
  --parameters postgresAdminPassword=$POSTGRES_PASSWORD \
  --parameters environment=$ENVIRONMENT \
  --parameters location=$LOCATION
```

### Option 2: Deploy Individual Components

Deploy components separately for testing or incremental provisioning:

#### 1. AKS Cluster

```bash
az deployment group create \
  --resource-group rg-digitalstokvel-staging \
  --template-file aks.bicep \
  --parameters aks.parameters.json
```

**Post-Deployment:**
```bash
# Get AKS credentials
az aks get-credentials \
  --resource-group rg-digitalstokvel-staging \
  --name digitalstokvel-aks-staging

# Verify cluster access
kubectl get nodes
kubectl get pods --all-namespaces
```

#### 2. PostgreSQL Databases

```bash
POSTGRES_PASSWORD="YourSecurePassword123!"

az deployment group create \
  --resource-group rg-digitalstokvel-staging \
  --template-file postgres.bicep \
  --parameters postgres.parameters.json \
  --parameters administratorPassword=$POSTGRES_PASSWORD
```

**Post-Deployment:**
```bash
# Get connection strings from outputs
az deployment group show \
  --resource-group rg-digitalstokvel-staging \
  --name postgres-deployment \
  --query properties.outputs

# Test connection
psql "host=digitalstokvel-postgres-staging.postgres.database.azure.com port=5432 dbname=digitalstokvel user=stokvel_admin password=$POSTGRES_PASSWORD sslmode=require"
```

#### 3. Redis Cache

```bash
az deployment group create \
  --resource-group rg-digitalstokvel-staging \
  --template-file redis.bicep \
  --parameters redis.parameters.json
```

**Post-Deployment:**
```bash
# Get Redis access key
az redis list-keys \
  --resource-group rg-digitalstokvel-staging \
  --name digitalstokvel-redis-staging

# Test connection using redis-cli
redis-cli -h digitalstokvel-redis-staging.redis.cache.windows.net -p 6380 -a "<access-key>" --tls
```

#### 4. Service Bus

```bash
az deployment group create \
  --resource-group rg-digitalstokvel-staging \
  --template-file servicebus.bicep \
  --parameters servicebus.parameters.json
```

**Post-Deployment:**
```bash
# List queues and topics
az servicebus queue list \
  --resource-group rg-digitalstokvel-staging \
  --namespace-name digitalstokvel-sb-staging --output table

az servicebus topic list \
  --resource-group rg-digitalstokvel-staging \
  --namespace-name digitalstokvel-sb-staging --output table
```

#### 5. Storage Account

```bash
az deployment group create \
  --resource-group rg-digitalstokvel-staging \
  --template-file storage.bicep \
  --parameters storage.parameters.json
```

**Post-Deployment:**
```bash
# List containers
az storage container list \
  --account-name digitalstokvelststaging \
  --output table

# Upload test file
az storage blob upload \
  --account-name digitalstokvelststaging \
  --container-name dispute-evidence \
  --name test.txt \
  --file test.txt
```

#### 6. Monitoring

```bash
az deployment group create \
  --resource-group rg-digitalstokvel-staging \
  --template-file monitoring.bicep \
  --parameters monitoring.parameters.json
```

**Post-Deployment:**
```bash
# Get Application Insights instrumentation key
az monitor app-insights component show \
  --resource-group rg-digitalstokvel-staging \
  --app digitalstokvel-insights-Staging \
  --query instrumentationKey --output tsv

# Access Grafana dashboards
az grafana show \
  --resource-group rg-digitalstokvel-staging \
  --name digitalstokvel-grafana-staging \
  --query properties.endpoint --output tsv
```

---

## Environment Configuration

### Development

```json
{
  "environment": "Development",
  "aks": {
    "nodePoolSize": "Small",
    "nodeCount": 3,
    "vmSize": "Standard_D2s_v3"
  },
  "postgres": {
    "sku": "Standard_B2ms",
    "storage": "32GB",
    "highAvailability": false
  },
  "redis": {
    "sku": "Basic C1",
    "size": "1GB"
  },
  "serviceBus": {
    "sku": "Basic"
  },
  "monitoring": {
    "retentionDays": 30,
    "prometheusGrafana": false
  }
}
```

### Staging

```json
{
  "environment": "Staging",
  "aks": {
    "nodePoolSize": "Small",
    "nodeCount": 3,
    "vmSize": "Standard_D4s_v3"
  },
  "postgres": {
    "sku": "Standard_D4ds_v5",
    "storage": "128GB",
    "highAvailability": false
  },
  "redis": {
    "sku": "Standard C2",
    "size": "2.5GB"
  },
  "serviceBus": {
    "sku": "Standard"
  },
  "monitoring": {
    "retentionDays": 30,
    "prometheusGrafana": false
  }
}
```

### Production

```json
{
  "environment": "Production",
  "aks": {
    "nodePoolSize": "Large",
    "nodeCount": 9,
    "vmSize": "Standard_D8s_v3",
    "availabilityZones": ["1", "2", "3"]
  },
  "postgres": {
    "sku": "Standard_D16ds_v5",
    "storage": "512GB",
    "highAvailability": true,
    "zoneRedundant": true
  },
  "redis": {
    "sku": "Premium P2",
    "size": "13GB",
    "clustering": true,
    "replication": true
  },
  "serviceBus": {
    "sku": "Premium",
    "messagingUnits": 1
  },
  "monitoring": {
    "retentionDays": 90,
    "prometheusGrafana": true
  }
}
```

---

## Cost Estimation

### Staging Environment (Monthly)

| Service | SKU | Quantity | Cost (USD) |
|---------|-----|----------|------------|
| AKS Cluster | Standard_D4s_v3 | 3 nodes | ~$365 |
| PostgreSQL (Primary) | Standard_D4ds_v5 | 1 server | ~$250 |
| PostgreSQL (Ledger) | Standard_D2ds_v5 | 1 server | ~$125 |
| Redis | Standard C2 | 1 cache | ~$100 |
| Service Bus | Standard | 1 namespace | ~$10 |
| Storage | Standard_ZRS | 100GB | ~$25 |
| Monitoring | Standard | - | ~$50 |
| **Total** | | | **~$925/month** |

### Production Environment (Monthly)

| Service | SKU | Quantity | Cost (USD) |
|---------|-----|----------|------------|
| AKS Cluster | Standard_D8s_v3 | 9 nodes | ~$1,970 |
| PostgreSQL (Primary) | Standard_D16ds_v5 + HA | 1 server | ~$1,200 |
| PostgreSQL (Ledger) | Standard_D8ds_v5 | 1 server | ~$600 |
| Redis | Premium P2 + Replication | 1 cluster | ~$660 |
| Service Bus | Premium | 1 namespace | ~$670 |
| Storage | Standard_GRS | 500GB | ~$140 |
| Monitoring | Standard + Grafana | - | ~$300 |
| **Total** | | | **~$5,540/month** |

> Note: Costs are estimates and may vary based on region, usage, and pricing changes.

---

## Post-Deployment Configuration

### 1. Configure AKS Cluster

```bash
# Get AKS credentials
az aks get-credentials \
  --resource-group rg-digitalstokvel-staging \
  --name digitalstokvel-aks-staging

# Install NGINX Ingress Controller
helm repo add ingress-nginx https://kubernetes.github.io/ingress-nginx
helm repo update
helm install ingress-nginx ingress-nginx/ingress-nginx \
  --namespace ingress-nginx \
  --create-namespace

# Install cert-manager for TLS
kubectl apply -f https://github.com/cert-manager/cert-manager/releases/download/v1.13.0/cert-manager.yaml
```

### 2. Configure Database

```bash
# Run initial database migrations
cd tools/DigitalStokvel.DbMigrator

# Set connection string
export POSTGRES_CONNECTION_STRING="Host=digitalstokvel-postgres-staging.postgres.database.azure.com;Port=5432;Database=digitalstokvel;Username=stokvel_admin;Password=***;Ssl Mode=Require;"

# Run migrations
dotnet run migrate
```

### 3. Configure Secrets

```bash
# Create Kubernetes secret for database connection
kubectl create secret generic postgres-credentials \
  --from-literal=connection-string="Host=...;Database=digitalstokvel;Username=stokvel_admin;Password=***;Ssl Mode=Require;"

# Create secret for Redis
kubectl create secret generic redis-credentials \
  --from-literal=connection-string="digitalstokvel-redis-staging.redis.cache.windows.net:6380,password=***,ssl=True,abortConnect=False"

# Create secret for Service Bus
kubectl create secret generic servicebus-credentials \
  --from-literal=connection-string="Endpoint=sb://digitalstokvel-sb-staging.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=***"
```

---

## Troubleshooting

### AKS Deployment Issues

**Problem:** Node pool creation fails due to quota limits.

**Solution:**
```bash
# Check current quota
az vm list-usage --location southafricanorth --query "[?name.value=='cores'].{Name:name.value,Current:currentValue,Limit:limit}" --output table

# Request quota increase
az support quota show --scope /subscriptions/<subscription-id>
```

### PostgreSQL Connection Issues

**Problem:** Cannot connect to PostgreSQL from local machine.

**Solution:**
```bash
# Add your IP to firewall rules
MY_IP=$(curl -s ifconfig.me)
az postgres flexible-server firewall-rule create \
  --resource-group rg-digitalstokvel-staging \
  --name digitalstokvel-postgres-staging \
  --rule-name AllowMyIP \
  --start-ip-address $MY_IP \
  --end-ip-address $MY_IP
```

### Redis Connection Issues

**Problem:** Redis connection timeouts.

**Solution:**
```bash
# Check Redis status
az redis show \
  --resource-group rg-digitalstokvel-staging \
  --name digitalstokvel-redis-staging \
  --query provisioningState

# Verify network rules
az redis firewall-rules list \
  --resource-group rg-digitalstokvel-staging \
  --name digitalstokvel-redis-staging
```

---

## Security Best Practices

1. **Use Azure Key Vault for Secrets:**
   - Store all passwords, connection strings, and API keys in Azure Key Vault
   - Use Managed Identity to access Key Vault from AKS

2. **Enable Private Endpoints (Production):**
   - Configure private endpoints for PostgreSQL, Redis, Storage, and Service Bus
   - Disable public network access

3. **Configure Network Security Groups:**
   - Restrict inbound traffic to AKS cluster
   - Allow only required ports (443 for HTTPS, 5432 for PostgreSQL, etc.)

4. **Enable Microsoft Defender:**
   - Enable Defender for Cloud for all resources
   - Enable Defender for Containers on AKS

5. **Implement RBAC:**
   - Use Azure AD integration for AKS authentication
   - Assign least privilege roles to service accounts

---

## Maintenance

### Backup and Restore

**PostgreSQL:**
```bash
# Automated backups are enabled by default
# Check backup retention
az postgres flexible-server show \
  --resource-group rg-digitalstokvel-staging \
  --name digitalstokvel-postgres-staging \
  --query backup.backupRetentionDays

# Restore from backup
az postgres flexible-server restore \
  --resource-group rg-digitalstokvel-staging \
  --name digitalstokvel-postgres-restored \
  --source-server digitalstokvel-postgres-staging \
  --restore-time "2026-03-24T12:00:00Z"
```

### Scaling

**AKS:**
```bash
# Scale application node pool
az aks nodepool scale \
  --resource-group rg-digitalstokvel-staging \
  --cluster-name digitalstokvel-aks-staging \
  --name apppool \
  --node-count 6
```

**Redis:**
```bash
# Scale up Redis cache
az redis update \
  --resource-group rg-digitalstokvel-staging \
  --name digitalstokvel-redis-staging \
  --sku Standard \
  --vm-size C4
```

---

## Additional Resources

- **Azure Bicep Documentation:** https://learn.microsoft.com/azure/azure-resource-manager/bicep/
- **AKS Best Practices:** https://learn.microsoft.com/azure/aks/best-practices
- **PostgreSQL Flexible Server:** https://learn.microsoft.com/azure/postgresql/flexible-server/
- **Azure Cache for Redis:** https://learn.microsoft.com/azure/azure-cache-for-redis/
- **Azure Service Bus:** https://learn.microsoft.com/azure/service-bus-messaging/

---

**Last Updated:** March 24, 2026  
**Infrastructure Version:** 1.0  
**Target Environment:** Azure (South Africa North region)

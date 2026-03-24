@description('Location for all resources')
param location string = resourceGroup().location

@description('PostgreSQL server name (primary)')
param serverName string = 'digitalstokvel-postgres'

@description('PostgreSQL server name (ledger)')
param ledgerServerName string = 'digitalstokvel-postgres-ledger'

@description('Administrator login username')
@secure()
param administratorLogin string

@description('Administrator login password')
@secure()
param administratorPassword string

@description('PostgreSQL version')
@allowed([
  '15'
  '16'
])
param postgresVersion string = '15'

@description('Environment name')
@allowed([
  'Development'
  'Staging'
  'Production'
])
param environment string = 'Staging'

@description('Resource tags')
param tags object = {
  Environment: environment
  Project: 'DigitalStokvel'
  CostCenter: 'Engineering'
  ManagedBy: 'Bicep'
  CreatedDate: utcNow('yyyy-MM-dd')
}

// SKU configuration based on environment
var skuByEnvironment = {
  Development: {
    name: 'Standard_B2ms'        // 2 vCPU, 8GB RAM, Burstable
    tier: 'Burstable'
  }
  Staging: {
    name: 'Standard_D4ds_v5'     // 4 vCPU, 16GB RAM, General Purpose
    tier: 'GeneralPurpose'
  }
  Production: {
    name: 'Standard_D16ds_v5'    // 16 vCPU, 64GB RAM, General Purpose
    tier: 'GeneralPurpose'
  }
}

var ledgerSkuByEnvironment = {
  Development: {
    name: 'Standard_B1ms'        // 1 vCPU, 2GB RAM, Burstable
    tier: 'Burstable'
  }
  Staging: {
    name: 'Standard_D2ds_v5'     // 2 vCPU, 8GB RAM, General Purpose
    tier: 'GeneralPurpose'
  }
  Production: {
    name: 'Standard_D8ds_v5'     // 8 vCPU, 32GB RAM, General Purpose
    tier: 'GeneralPurpose'
  }
}

var sku = skuByEnvironment[environment]
var ledgerSku = ledgerSkuByEnvironment[environment]

// Storage size based on environment (in MB)
var storageSizeByEnvironment = {
  Development: 32768      // 32GB
  Staging: 131072         // 128GB
  Production: 524288      // 512GB
}

var ledgerStorageSizeByEnvironment = {
  Development: 32768      // 32GB
  Staging: 65536          // 64GB
  Production: 262144      // 256GB
}

// Backup retention based on environment (days)
var backupRetentionDays = environment == 'Production' ? 35 : 7

// Primary PostgreSQL Server
resource postgresServer 'Microsoft.DBforPostgreSQL/flexibleServers@2023-12-01-preview' = {
  name: serverName
  location: location
  tags: union(tags, { Purpose: 'Primary Database' })
  sku: {
    name: sku.name
    tier: sku.tier
  }
  properties: {
    version: postgresVersion
    administratorLogin: administratorLogin
    administratorLoginPassword: administratorPassword
    
    // High availability configuration (Production only)
    highAvailability: {
      mode: environment == 'Production' ? 'ZoneRedundant' : 'Disabled'
      standbyAvailabilityZone: environment == 'Production' ? '2' : null
    }
    
    // Storage configuration
    storage: {
      storageSizeGB: storageSizeByEnvironment[environment] / 1024
      autoGrow: 'Enabled'
      iops: environment == 'Production' ? 3000 : 500
      tier: environment == 'Production' ? 'P30' : 'P10'
    }
    
    // Backup configuration
    backup: {
      backupRetentionDays: backupRetentionDays
      geoRedundantBackup: environment == 'Production' ? 'Enabled' : 'Disabled'
    }
    
    // Network configuration (public access for now, private endpoints in production)
    network: {
      publicNetworkAccess: environment == 'Production' ? 'Disabled' : 'Enabled'
    }
    
    // Authentication config
    authConfig: {
      activeDirectoryAuth: 'Enabled'
      passwordAuth: 'Enabled'
      tenantId: subscription().tenantId
    }
  }
}

// Primary database configuration
resource primaryDatabase 'Microsoft.DBforPostgreSQL/flexibleServers/databases@2023-12-01-preview' = {
  parent: postgresServer
  name: 'digitalstokvel'
  properties: {
    charset: 'UTF8'
    collation: 'en_US.utf8'
  }
}

// Enable extensions on primary database
resource postgresConfig 'Microsoft.DBforPostgreSQL/flexibleServers/configurations@2023-12-01-preview' = {
  parent: postgresServer
  name: 'azure.extensions'
  properties: {
    value: 'uuid-ossp,pgcrypto,pg_stat_statements'
    source: 'user-override'
  }
}

// Firewall rule for Azure services
resource firewallRuleAzure 'Microsoft.DBforPostgreSQL/flexibleServers/firewallRules@2023-12-01-preview' = if (environment != 'Production') {
  parent: postgresServer
  name: 'AllowAzureServices'
  properties: {
    startIpAddress: '0.0.0.0'
    endIpAddress: '0.0.0.0'
  }
}

// Ledger PostgreSQL Server (Append-Only, Write-Optimized)
resource ledgerServer 'Microsoft.DBforPostgreSQL/flexibleServers@2023-12-01-preview' = {
  name: ledgerServerName
  location: location
  tags: union(tags, { Purpose: 'Immutable Ledger' })
  sku: {
    name: ledgerSku.name
    tier: ledgerSku.tier
  }
  properties: {
    version: postgresVersion
    administratorLogin: administratorLogin
    administratorLoginPassword: administratorPassword
    
    // No HA for ledger (cost optimization)
    highAvailability: {
      mode: 'Disabled'
    }
    
    // Storage configuration (write-optimized)
    storage: {
      storageSizeGB: ledgerStorageSizeByEnvironment[environment] / 1024
      autoGrow: 'Enabled'
      iops: environment == 'Production' ? 3000 : 500
      tier: environment == 'Production' ? 'P30' : 'P10'
    }
    
    // Backup configuration (7-year retention required)
    backup: {
      backupRetentionDays: 35  // Maximum for Flexible Server
      geoRedundantBackup: environment == 'Production' ? 'Enabled' : 'Disabled'
    }
    
    // Network configuration
    network: {
      publicNetworkAccess: environment == 'Production' ? 'Disabled' : 'Enabled'
    }
    
    // Authentication config
    authConfig: {
      activeDirectoryAuth: 'Enabled'
      passwordAuth: 'Enabled'
      tenantId: subscription().tenantId
    }
  }
}

// Ledger database
resource ledgerDatabase 'Microsoft.DBforPostgreSQL/flexibleServers/databases@2023-12-01-preview' = {
  parent: ledgerServer
  name: 'digitalstokvel_ledger'
  properties: {
    charset: 'UTF8'
    collation: 'en_US.utf8'
  }
}

// Enable extensions on ledger database
resource ledgerConfig 'Microsoft.DBforPostgreSQL/flexibleServers/configurations@2023-12-01-preview' = {
  parent: ledgerServer
  name: 'azure.extensions'
  properties: {
    value: 'uuid-ossp,pgcrypto,pg_stat_statements'
    source: 'user-override'
  }
}

// Firewall rule for Azure services (ledger)
resource ledgerFirewallRule 'Microsoft.DBforPostgreSQL/flexibleServers/firewallRules@2023-12-01-preview' = if (environment != 'Production') {
  parent: ledgerServer
  name: 'AllowAzureServices'
  properties: {
    startIpAddress: '0.0.0.0'
    endIpAddress: '0.0.0.0'
  }
}

// Outputs
output primaryServerName string = postgresServer.name
output primaryServerId string = postgresServer.id
output primaryServerFqdn string = postgresServer.properties.fullyQualifiedDomainName
output primaryDatabaseName string = primaryDatabase.name

output ledgerServerName string = ledgerServer.name
output ledgerServerId string = ledgerServer.id
output ledgerServerFqdn string = ledgerServer.properties.fullyQualifiedDomainName
output ledgerDatabaseName string = ledgerDatabase.name

output primaryConnectionString string = 'Server=${postgresServer.properties.fullyQualifiedDomainName};Database=${primaryDatabase.name};Port=5432;User Id=${administratorLogin};Password=***;Ssl Mode=Require;'
output ledgerConnectionString string = 'Server=${ledgerServer.properties.fullyQualifiedDomainName};Database=${ledgerDatabase.name};Port=5432;User Id=${administratorLogin};Password=***;Ssl Mode=Require;'

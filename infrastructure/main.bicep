@description('Location for all resources')
param location string = 'southafricanorth'

@description('Environment name')
@allowed([
  'Development'
  'Staging'
  'Production'
])
param environment string = 'Staging'

@description('Base name for all resources')
param baseName string = 'digitalstokvel'

@description('PostgreSQL administrator login')
@secure()
param postgresAdminLogin string

@description('PostgreSQL administrator password')
@secure()
param postgresAdminPassword string

@description('Deploy AKS cluster')
param deployAks bool = true

@description('Deploy PostgreSQL databases')
param deployPostgres bool = true

@description('Deploy Redis cache')
param deployRedis bool = true

@description('Deploy Service Bus')
param deployServiceBus bool = true

@description('Deploy Storage')
param deployStorage bool = true

@description('Deploy Monitoring')
param deployMonitoring bool = true

// Common tags
var tags = {
  Environment: environment
  Project: 'DigitalStokvel'
  CostCenter: 'Engineering'
  ManagedBy: 'Bicep'
  DeployedDate: utcNow('yyyy-MM-dd')
}

// AKS Cluster
module aks 'aks.bicep' = if (deployAks) {
  name: 'aks-deployment'
  params: {
    location: location
    clusterName: '${baseName}-aks-${toLower(environment)}'
    environment: environment
    nodePoolSize: environment == 'Production' ? 'Large' : (environment == 'Staging' ? 'Small' : 'Small')
    enableAzureCni: true
    enableManagedIdentity: true
    enableMonitoring: true
    tags: tags
  }
}

// PostgreSQL Databases
module postgres 'postgres.bicep' = if (deployPostgres) {
  name: 'postgres-deployment'
  params: {
    location: location
    serverName: '${baseName}-postgres-${toLower(environment)}'
    ledgerServerName: '${baseName}-postgres-ledger-${toLower(environment)}'
    administratorLogin: postgresAdminLogin
    administratorPassword: postgresAdminPassword
    postgresVersion: '15'
    environment: environment
    tags: tags
  }
}

// Redis Cache
module redis 'redis.bicep' = if (deployRedis) {
  name: 'redis-deployment'
  params: {
    location: location
    redisCacheName: '${baseName}-redis-${toLower(environment)}'
    environment: environment
    tags: tags
  }
}

// Service Bus
module serviceBus 'servicebus.bicep' = if (deployServiceBus) {
  name: 'servicebus-deployment'
  params: {
    location: location
    namespaceName: '${baseName}-sb-${toLower(environment)}'
    environment: environment
    tags: tags
  }
}

// Storage Account
module storage 'storage.bicep' = if (deployStorage) {
  name: 'storage-deployment'
  params: {
    location: location
    storageAccountName: '${baseName}st${toLower(environment)}'
    environment: environment
    tags: tags
  }
}

// Monitoring Infrastructure
module monitoring 'monitoring.bicep' = if (deployMonitoring) {
  name: 'monitoring-deployment'
  params: {
    location: location
    baseName: baseName
    environment: environment
    enablePrometheusGrafana: environment == 'Production'
    tags: tags
  }
}

// Outputs
output aksClusterName string = deployAks ? aks.outputs.clusterName : ''
output aksClusterFqdn string = deployAks ? aks.outputs.clusterFqdn : ''
output kubeletIdentityObjectId string = deployAks ? aks.outputs.kubeletIdentityObjectId : ''

output primaryPostgresServer string = deployPostgres ? postgres.outputs.primaryServerFqdn : ''
output ledgerPostgresServer string = deployPostgres ? postgres.outputs.ledgerServerFqdn : ''
output primaryDatabaseName string = deployPostgres ? postgres.outputs.primaryDatabaseName : ''
output ledgerDatabaseName string = deployPostgres ? postgres.outputs.ledgerDatabaseName : ''

output redisHostName string = deployRedis ? redis.outputs.redisHostName : ''
output redisSslPort int = deployRedis ? redis.outputs.redisSslPort : 0

output serviceBusNamespace string = deployServiceBus ? serviceBus.outputs.namespaceName : ''
output serviceBusEndpoint string = deployServiceBus ? serviceBus.outputs.namespaceEndpoint : ''

output storageAccountName string = deployStorage ? storage.outputs.storageAccountName : ''
output primaryBlobEndpoint string = deployStorage ? storage.outputs.primaryBlobEndpoint : ''

output logAnalyticsWorkspaceId string = deployMonitoring ? monitoring.outputs.logAnalyticsWorkspaceId : ''
output appInsightsConnectionString string = deployMonitoring ? monitoring.outputs.appInsightsConnectionString : ''
output appInsightsInstrumentationKey string = deployMonitoring ? monitoring.outputs.appInsightsInstrumentationKey : ''
output grafanaEndpoint string = deployMonitoring ? monitoring.outputs.grafanaEndpoint : ''

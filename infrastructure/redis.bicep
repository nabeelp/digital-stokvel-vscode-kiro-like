@description('Location for all resources')
param location string = resourceGroup().location

@description('Redis Cache name')
param redisCacheName string = 'digitalstokvel-redis'

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
    name: 'Basic'
    family: 'C'
    capacity: 1  // 1GB cache
  }
  Staging: {
    name: 'Standard'
    family: 'C'
    capacity: 2  // 2.5GB cache
  }
  Production: {
    name: 'Premium'
    family: 'P'
    capacity: 2  // 13GB cache with clustering
  }
}

var sku = skuByEnvironment[environment]

// Redis Cache
resource redisCache 'Microsoft.Cache/redis@2024-03-01' = {
  name: redisCacheName
  location: location
  tags: tags
  properties: {
    sku: sku
    enableNonSslPort: false
    minimumTlsVersion: '1.2'
    publicNetworkAccess: environment == 'Production' ? 'Disabled' : 'Enabled'
    
    // Redis configuration
    redisConfiguration: {
      'maxmemory-policy': 'allkeys-lru'  // LRU eviction for cache
      'maxmemory-reserved': environment == 'Production' ? '10' : '2'
      'maxfragmentationmemory-reserved': environment == 'Production' ? '12' : '2'
      'notify-keyspace-events': 'Ex'     // Enable keyspace notifications for expired keys
      'aof-backup-enabled': sku.name == 'Premium' ? 'true' : 'false'  // AOF persistence for Premium
      'rdb-backup-enabled': sku.name == 'Premium' ? 'true' : 'false'  // RDB snapshots for Premium
      'rdb-backup-frequency': sku.name == 'Premium' ? '60' : null     // Hourly snapshots
      'rdb-backup-max-snapshot-count': sku.name == 'Premium' ? '1' : null
    }
    
    // Zone redundancy (Premium only)
    zones: (environment == 'Production' && sku.name == 'Premium') ? [
      '1'
      '2'
      '3'
    ] : null
    
    // Replication (Premium only)
    replicasPerMaster: sku.name == 'Premium' ? 1 : 0
    replicasPerPrimary: sku.name == 'Premium' ? 1 : 0
    
    // Sharding (Premium P2 and above)
    shardCount: (sku.name == 'Premium' && sku.capacity >= 2) ? 3 : 0
  }
}

// Outputs
output redisCacheName string = redisCache.name
output redisCacheId string = redisCache.id
output redisHostName string = redisCache.properties.hostName
output redisSslPort int = redisCache.properties.sslPort
output redisPort int = redisCache.properties.port
output redisConnectionString string = '${redisCache.properties.hostName}:${redisCache.properties.sslPort},password=***,ssl=True,abortConnect=False'

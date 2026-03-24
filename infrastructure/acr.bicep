@description('Location for all resources')
param location string = resourceGroup().location

@description('Container Registry name')
param registryName string = 'digitalstokvel'

@description('Container Registry SKU')
@allowed([
  'Basic'
  'Standard'
  'Premium'
])
param skuName string = 'Premium'

@description('Enable geo-replication')
param enableGeoreplication bool = true

@description('Secondary location for geo-replication')
param secondaryLocation string = 'westeurope'

@description('Environment name')
@allowed([
  'Development'
  'Staging'
  'Production'
])
param environment string = 'Production'

@description('Resource tags')
param tags object = {
  Environment: environment
  Project: 'DigitalStokvel'
  CostCenter: 'Engineering'
  ManagedBy: 'Bicep'
  CreatedDate: utcNow('yyyy-MM-dd')
}

// Azure Container Registry
resource containerRegistry 'Microsoft.ContainerRegistry/registries@2023-01-01-preview' = {
  name: registryName
  location: location
  sku: {
    name: skuName
  }
  tags: tags
  properties: {
    adminUserEnabled: false
    publicNetworkAccess: 'Enabled'
    networkRuleBypassOptions: 'AzureServices'
    policies: {
      quarantinePolicy: {
        status: 'enabled'
      }
      trustPolicy: {
        type: 'Notary'
        status: 'enabled'
      }
      retentionPolicy: {
        days: 30
        status: 'enabled'
      }
      exportPolicy: {
        status: 'enabled'
      }
    }
    encryption: {
      status: 'disabled'
    }
    dataEndpointEnabled: false
    anonymousPullEnabled: false
    zoneRedundancy: skuName == 'Premium' ? 'Enabled' : 'Disabled'
  }
}

// Geo-replication (Premium SKU only)
resource replication 'Microsoft.ContainerRegistry/registries/replications@2023-01-01-preview' = if (enableGeoreplication && skuName == 'Premium') {
  parent: containerRegistry
  name: secondaryLocation
  location: secondaryLocation
  tags: tags
  properties: {
    regionEndpointEnabled: true
    zoneRedundancy: 'Enabled'
  }
}

// Outputs
output registryName string = containerRegistry.name
output registryLoginServer string = containerRegistry.properties.loginServer
output registryId string = containerRegistry.id
output registryResourceGroup string = resourceGroup().name

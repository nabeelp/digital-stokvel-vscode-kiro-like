@description('Location for all resources')
param location string = resourceGroup().location

@description('Storage account name')
param storageAccountName string = 'digitalstokvelstorage'

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

// SKU based on environment
var skuByEnvironment = {
  Development: 'Standard_LRS'      // Locally redundant
  Staging: 'Standard_ZRS'          // Zone redundant
  Production: 'Standard_GRS'       // Geo-redundant
}

var sku = skuByEnvironment[environment]

// Storage Account
resource storageAccount 'Microsoft.Storage/storageAccounts@2023-05-01' = {
  name: storageAccountName
  location: location
  tags: tags
  sku: {
    name: sku
  }
  kind: 'StorageV2'
  properties: {
    accessTier: 'Hot'
    minimumTlsVersion: 'TLS1_2'
    supportsHttpsTrafficOnly: true
    allowBlobPublicAccess: false
    allowSharedKeyAccess: true
    publicNetworkAccess: environment == 'Production' ? 'Disabled' : 'Enabled'
    
    // Encryption
    encryption: {
      services: {
        blob: {
          enabled: true
          keyType: 'Account'
        }
        file: {
          enabled: true
          keyType: 'Account'
        }
      }
      keySource: 'Microsoft.Storage'
    }
    
    // Network rules
    networkAcls: {
      bypass: 'AzureServices'
      defaultAction: environment == 'Production' ? 'Deny' : 'Allow'
      ipRules: []
      virtualNetworkRules: []
    }
  }
}

// Blob Service
resource blobService 'Microsoft.Storage/storageAccounts/blobServices@2023-05-01' = {
  parent: storageAccount
  name: 'default'
  properties: {
    cors: {
      corsRules: [
        {
          allowedOrigins: [
            '*'  // TODO: Restrict to specific origins in production
          ]
          allowedMethods: [
            'GET'
            'POST'
            'PUT'
          ]
          maxAgeInSeconds: 3600
          exposedHeaders: [
            '*'
          ]
          allowedHeaders: [
            '*'
          ]
        }
      ]
    }
    deleteRetentionPolicy: {
      enabled: true
      days: environment == 'Production' ? 30 : 7
    }
    containerDeleteRetentionPolicy: {
      enabled: true
      days: environment == 'Production' ? 30 : 7
    }
    changeFeed: {
      enabled: environment == 'Production'
      retentionInDays: environment == 'Production' ? 7 : null
    }
    isVersioningEnabled: environment == 'Production'
  }
}

// Container: Dispute Evidence
resource evidenceContainer 'Microsoft.Storage/storageAccounts/blobServices/containers@2023-05-01' = {
  parent: blobService
  name: 'dispute-evidence'
  properties: {
    publicAccess: 'None'
    metadata: {
      purpose: 'Store dispute evidence files (images, PDFs)'
    }
  }
}

// Container: Contribution Receipts
resource receiptsContainer 'Microsoft.Storage/storageAccounts/blobServices/containers@2023-05-01' = {
  parent: blobService
  name: 'contribution-receipts'
  properties: {
    publicAccess: 'None'
    metadata: {
      purpose: 'Store generated contribution receipts'
    }
  }
}

// Container: Group Constitutions
resource constitutionsContainer 'Microsoft.Storage/storageAccounts/blobServices/containers@2023-05-01' = {
  parent: blobService
  name: 'group-constitutions'
  properties: {
    publicAccess: 'None'
    metadata: {
      purpose: 'Store group constitution documents'
    }
  }
}

// Container: Audit Reports
resource auditContainer 'Microsoft.Storage/storageAccounts/blobServices/containers@2023-05-01' = {
  parent: blobService
  name: 'audit-reports'
  properties: {
    publicAccess: 'None'
    metadata: {
      purpose: 'Store generated audit and compliance reports'
    }
  }
}

// Lifecycle management policy
resource lifecyclePolicy 'Microsoft.Storage/storageAccounts/managementPolicies@2023-05-01' = {
  parent: storageAccount
  name: 'default'
  properties: {
    policy: {
      rules: [
        {
          enabled: true
          name: 'move-old-receipts-to-cool'
          type: 'Lifecycle'
          definition: {
            filters: {
              blobTypes: [
                'blockBlob'
              ]
              prefixMatch: [
                'contribution-receipts/'
              ]
            }
            actions: {
              baseBlob: {
                tierToCool: {
                  daysAfterModificationGreaterThan: 90  // Move to cool tier after 90 days
                }
                tierToArchive: {
                  daysAfterModificationGreaterThan: 365  // Archive after 1 year
                }
              }
            }
          }
        }
        {
          enabled: true
          name: 'delete-old-evidence'
          type: 'Lifecycle'
          definition: {
            filters: {
              blobTypes: [
                'blockBlob'
              ]
              prefixMatch: [
                'dispute-evidence/'
              ]
            }
            actions: {
              baseBlob: {
                delete: {
                  daysAfterModificationGreaterThan: 2555  // Delete after 7 years (FICA compliance)
                }
              }
            }
          }
        }
      ]
    }
  }
}

// Outputs
output storageAccountName string = storageAccount.name
output storageAccountId string = storageAccount.id
output primaryBlobEndpoint string = storageAccount.properties.primaryEndpoints.blob
output evidenceContainerName string = evidenceContainer.name
output receiptsContainerName string = receiptsContainer.name
output constitutionsContainerName string = constitutionsContainer.name
output auditContainerName string = auditContainer.name

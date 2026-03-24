@description('Location for all resources')
param location string = resourceGroup().location

@description('Service Bus namespace name')
param namespaceName string = 'digitalstokvel-servicebus'

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
  CostCenter:'Engineering'
  ManagedBy: 'Bicep'
  CreatedDate: utcNow('yyyy-MM-dd')
}

// SKU based on environment
var skuByEnvironment = {
  Development: 'Basic'
  Staging: 'Standard'
  Production: 'Premium'
}

var sku = skuByEnvironment[environment]

// Service Bus Namespace
resource serviceBusNamespace 'Microsoft.ServiceBus/namespaces@2022-10-01-preview' = {
  name: namespaceName
  location: location
  tags: tags
  sku: {
    name: sku
    tier: sku
    capacity: sku == 'Premium' ? 1 : null  // 1 messaging unit for Premium
  }
  properties: {
    minimumTlsVersion: '1.2'
    publicNetworkAccess: environment == 'Production' ? 'Disabled' : 'Enabled'
    zoneRedundant: sku == 'Premium'
    premiumMessagingPartitions: sku == 'Premium' ? 1 : 0
  }
}

// Notification Queue
resource notificationQueue 'Microsoft.ServiceBus/namespaces/queues@2022-10-01-preview' = {
  parent: serviceBusNamespace
  name: 'notifications'
  properties: {
    lockDuration: 'PT5M'           // 5 minutes lock
    maxSizeInMegabytes: 1024       // 1GB queue size
    requiresDuplicateDetection: true
    requiresSession: false
    defaultMessageTimeToLive: 'P14D'  // 14 days TTL
    deadLetteringOnMessageExpiration: true
    duplicateDetectionHistoryTimeWindow: 'PT10M'
    maxDeliveryCount: 3            // 3 retry attempts
    enableBatchedOperations: true
    enablePartitioning: sku != 'Premium'
  }
}

// Contribution Queue
resource contributionQueue 'Microsoft.ServiceBus/namespaces/queues@2022-10-01-preview' = {
  parent: serviceBusNamespace
  name: 'contributions'
  properties: {
    lockDuration: 'PT5M'
    maxSizeInMegabytes: 2048       // 2GB for higher volume
    requiresDuplicateDetection: true
    requiresSession: false
    defaultMessageTimeToLive: 'P7D'
    deadLetteringOnMessageExpiration: true
    duplicateDetectionHistoryTimeWindow: 'PT10M'
    maxDeliveryCount: 5
    enableBatchedOperations: true
    enablePartitioning: sku != 'Premium'
  }
}

// Payout Queue
resource payoutQueue 'Microsoft.ServiceBus/namespaces/queues@2022-10-01-preview' = {
  parent: serviceBusNamespace
  name: 'payouts'
  properties: {
    lockDuration: 'PT10M'          // Longer lock for approval workflow
    maxSizeInMegabytes: 1024
    requiresDuplicateDetection: true
    requiresSession: false
    defaultMessageTimeToLive: 'P30D'  // 30 days for audit
    deadLetteringOnMessageExpiration: true
    duplicateDetectionHistoryTimeWindow: 'PT10M'
    maxDeliveryCount: 3
    enableBatchedOperations: true
    enablePartitioning: sku != 'Premium'
  }
}

// Interest Calculation Topic (Pub/Sub pattern)
resource interestTopic 'Microsoft.ServiceBus/namespaces/topics@2022-10-01-preview' = {
  parent: serviceBusNamespace
  name: 'interest-calculation'
  properties: {
    maxSizeInMegabytes: 1024
    requiresDuplicateDetection: true
    defaultMessageTimeToLive: 'P1D'
    enableBatchedOperations: true
    enablePartitioning: sku != 'Premium'
  }
}

// Subscription for Interest Service
resource interestSubscription 'Microsoft.ServiceBus/namespaces/topics/subscriptions@2022-10-01-preview' = {
  parent: interestTopic
  name: 'interest-service'
  properties: {
    lockDuration: 'PT5M'
    requiresSession: false
    defaultMessageTimeToLive: 'P1D'
    deadLetteringOnMessageExpiration: true
    maxDeliveryCount: 3
  }
}

// Outputs
output namespaceName string = serviceBusNamespace.name
output namespaceId string = serviceBusNamespace.id
output namespaceEndpoint string = serviceBusNamespace.properties.serviceBusEndpoint
output notificationQueueName string = notificationQueue.name
output contributionQueueName string = contributionQueue.name
output payoutQueueName string = payoutQueue.name
output interestTopicName string = interestTopic.name

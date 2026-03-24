@description('Location for all resources')
param location string = resourceGroup().location

@description('AKS cluster name')
param clusterName string = 'digitalstokvel-aks'

@description('Kubernetes version')
param kubernetesVersion string = '1.30.0'

@description('Environment name')
@allowed([
  'Development'
  'Staging'
  'Production'
])
param environment string = 'Staging'

@description('Node pool size')
@allowed([
  'Small'   // 3 nodes: Dev/Staging
  'Medium'  // 6 nodes: Small Production
  'Large'   // 9 nodes: Production
])
param nodePoolSize string = 'Small'

@description('Enable Azure CNI networking')
param enableAzureCni bool = true

@description('Enable managed identity')
param enableManagedIdentity bool = true

@description('Enable Azure Monitor for containers')
param enableMonitoring bool = true

@description('Resource tags')
param tags object = {
  Environment: environment
  Project: 'DigitalStokvel'
  CostCenter: 'Engineering'
  ManagedBy: 'Bicep'
  CreatedDate: utcNow('yyyy-MM-dd')
}

// Calculate node pool size based on environment
var nodePoolSizes = {
  Small: 3
  Medium: 6
  Large: 9
}

var nodeCount = nodePoolSizes[nodePoolSize]

// VM sizes by environment
var vmSizeByEnvironment = {
  Development: 'Standard_D2s_v3'    // 2 vCPU, 8GB RAM
  Staging: 'Standard_D4s_v3'        // 4 vCPU, 16GB RAM
  Production: 'Standard_D8s_v3'     // 8 vCPU, 32GB RAM
}

var nodeVmSize = vmSizeByEnvironment[environment]

// Log Analytics Workspace for monitoring
resource logAnalytics 'Microsoft.OperationalInsights/workspaces@2023-09-01' = if (enableMonitoring) {
  name: '${clusterName}-logs'
  location: location
  tags: tags
  properties: {
    sku: {
      name: 'PerGB2018'
    }
    retentionInDays: environment == 'Production' ? 90 : 30
    features: {
      enableLogAccessUsingOnlyResourcePermissions: true
    }
  }
}

// AKS Cluster
resource aksCluster 'Microsoft.ContainerService/managedClusters@2024-05-01' = {
  name: clusterName
  location: location
  tags: tags
  identity: {
    type: enableManagedIdentity ? 'SystemAssigned' : 'None'
  }
  properties: {
    kubernetesVersion: kubernetesVersion
    dnsPrefix: '${clusterName}-dns'
    
    // Default node pool (system)
    agentPoolProfiles: [
      {
        name: 'systempool'
        count: 3
        vmSize: 'Standard_D2s_v3'
        osType: 'Linux'
        osDiskSizeGB: 128
        osDiskType: 'Managed'
        vnetSubnetID: null
        maxPods: 30
        type: 'VirtualMachineScaleSets'
        mode: 'System'
        enableAutoScaling: true
        minCount: 3
        maxCount: 5
        availabilityZones: environment == 'Production' ? [
          '1'
          '2'
          '3'
        ] : []
        nodeTaints: [
          'CriticalAddonsOnly=true:NoSchedule'
        ]
      }
      {
        name: 'apppool'
        count: nodeCount
        vmSize: nodeVmSize
        osType: 'Linux'
        osDiskSizeGB: 256
        osDiskType: 'Managed'
        vnetSubnetID: null
        maxPods: 50
        type: 'VirtualMachineScaleSets'
        mode: 'User'
        enableAutoScaling: true
        minCount: nodeCount
        maxCount: nodeCount * 2
        availabilityZones: environment == 'Production' ? [
          '1'
          '2'
          '3'
        ] : []
      }
    ]
    
    // Networking configuration
    networkProfile: {
      networkPlugin: enableAzureCni ? 'azure' : 'kubenet'
      networkPolicy: 'azure'
      serviceCidr: '10.100.0.0/16'
      dnsServiceIP: '10.100.0.10'
      loadBalancerSku: 'Standard'
      outboundType: 'loadBalancer'
    }
    
    // Enable Azure Active Directory integration
    aadProfile: {
      managed: true
      enableAzureRBAC: true
      adminGroupObjectIDs: []
      tenantID: subscription().tenantId
    }
    
    // API server access profile
    apiServerAccessProfile: {
      enablePrivateCluster: environment == 'Production'
      enablePrivateClusterPublicFQDN: false
    }
    
    // Monitoring
    addonProfiles: {
      omsagent: {
        enabled: enableMonitoring
        config: enableMonitoring ? {
          logAnalyticsWorkspaceResourceID: logAnalytics.id
        } : null
      }
      azurePolicy: {
        enabled: environment == 'Production'
      }
    }
    
    // Auto-upgrade settings
    autoUpgradeProfile: {
      upgradeChannel: environment == 'Production' ? 'stable' : 'rapid'
    }
    
    // Security settings
    securityProfile: {
      defender: {
        logAnalyticsWorkspaceResourceId: enableMonitoring ? logAnalytics.id : null
        securityMonitoring: {
          enabled: environment == 'Production'
        }
      }
    }
    
    // Enable workload identity
    oidcIssuerProfile: {
      enabled: true
    }
    
    securityProfile: {
      workloadIdentity: {
        enabled: true
      }
    }
  }
}

// Outputs
output clusterName string = aksCluster.name
output clusterId string = aksCluster.id
output clusterFqdn string = aksCluster.properties.fqdn
output kubeletIdentityObjectId string = aksCluster.properties.identityProfile.kubeletidentity.objectId
output clusterPrincipalId string = enableManagedIdentity ? aksCluster.identity.principalId : ''
output logAnalyticsWorkspaceId string = enableMonitoring ? logAnalytics.id : ''
output nodeResourceGroup string = aksCluster.properties.nodeResourceGroup

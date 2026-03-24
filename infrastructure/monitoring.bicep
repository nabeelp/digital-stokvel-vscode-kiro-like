@description('Location for all resources')
param location string = resourceGroup().location

@description('Base name for monitoring resources')
param baseName string = 'digitalstokvel'

@description('Environment name')
@allowed([
  'Development'
  'Staging'
  'Production'
])
param environment string = 'Staging'

@description('Enable Prometheus and Grafana')
param enablePrometheusGrafana bool = environment == 'Production'

@description('Resource tags')
param tags object = {
  Environment: environment
  Project: 'DigitalStokvel'
  CostCenter: 'Engineering'
  ManagedBy: 'Bicep'
  CreatedDate: utcNow('yyyy-MM-dd')
}

// Data retention based on environment
var dataRetentionDays = environment == 'Production' ? 90 : 30

// Log Analytics Workspace
resource logAnalytics 'Microsoft.OperationalInsights/workspaces@2023-09-01' = {
  name: '${baseName}-logs-${environment}'
  location: location
  tags: tags
  properties: {
    sku: {
      name: 'PerGB2018'
    }
    retentionInDays: dataRetentionDays
    features: {
      enableLogAccessUsingOnlyResourcePermissions: true
    }
    workspaceCapping: {
      dailyQuotaGb: environment == 'Production' ? -1 : 5  // 5GB cap for non-prod
    }
    publicNetworkAccessForIngestion: 'Enabled'
    publicNetworkAccessForQuery: 'Enabled'
  }
}

// Application Insights
resource appInsights 'Microsoft.Insights/components@2020-02-02' = {
  name: '${baseName}-insights-${environment}'
  location: location
  tags: tags
  kind: 'web'
  properties: {
    Application_Type: 'web'
    WorkspaceResourceId: logAnalytics.id
    RetentionInDays: dataRetentionDays
    IngestionMode: 'LogAnalytics'
    publicNetworkAccessForIngestion: 'Enabled'
    publicNetworkAccessForQuery: 'Enabled'
    DisableIpMasking: false
  }
}

// Diagnostic Settings for Log Analytics
resource logAnalyticsDiag 'Microsoft.Insights/diagnosticSettings@2021-05-01-preview' = {
  name: 'log-analytics-diagnostics'
  scope: logAnalytics
  properties: {
    workspaceId: logAnalytics.id
    logs: [
      {
        category: 'Audit'
        enabled: true
        retentionPolicy: {
          enabled: true
          days: dataRetentionDays
        }
      }
    ]
    metrics: [
      {
        category: 'AllMetrics'
        enabled: true
        retentionPolicy: {
          enabled: true
          days: dataRetentionDays
        }
      }
    ]
  }
}

// Azure Monitor Workspace (for Prometheus metrics)
resource azureMonitorWorkspace 'Microsoft.Monitor/accounts@2023-04-03' = if (enablePrometheusGrafana) {
  name: '${baseName}-azmon-${environment}'
  location: location
  tags: tags
  properties: {}
}

// Azure Managed Grafana
resource managedGrafana 'Microsoft.Dashboard/grafana@2023-09-01' = if (enablePrometheusGrafana) {
  name: '${baseName}-grafana-${environment}'
  location: location
  tags: tags
  sku: {
    name: 'Standard'
  }
  identity: {
    type: 'SystemAssigned'
  }
  properties: {
    grafanaIntegrations: {
      azureMonitorWorkspaceIntegrations: enablePrometheusGrafana ? [
        {
          azureMonitorWorkspaceResourceId: azureMonitorWorkspace.id
        }
      ] : []
    }
    zoneRedundancy: environment == 'Production' ? 'Enabled' : 'Disabled'
    publicNetworkAccess: 'Enabled'
    apiKey: 'Enabled'
    deterministicOutboundIP: 'Enabled'
  }
}

// Action Group for Alerts
resource actionGroup 'Microsoft.Insights/actionGroups@2023-01-01' = {
  name: '${baseName}-alerts-${environment}'
  location: 'global'
  tags: tags
  properties: {
    groupShortName: 'StokvelOps'
    enabled: true
    emailReceivers: [
      {
        name: 'Operations Team'
        emailAddress: 'ops@digitalstokvel.local'  // TODO: Replace with real email
        useCommonAlertSchema: true
      }
    ]
    smsReceivers: []
    webhookReceivers: []
    azureAppPushReceivers: []
    automationRunbookReceivers: []
    voiceReceivers: []
    logicAppReceivers: []
    azureFunctionReceivers: []
    armRoleReceivers: [
      {
        name: 'Monitoring Contributor'
        roleId: '749f88d5-cbae-40b8-bcfc-e573ddc772fa'  // Monitoring Contributor role
        useCommonAlertSchema: true
      }
    ]
  }
}

// Alert Rule: High API Error Rate
resource apiErrorRateAlert 'Microsoft.Insights/metricAlerts@2018-03-01' = {
  name: '${baseName}-api-errors-${environment}'
  location: 'global'
  tags: tags
  properties: {
    description: 'Alert when API error rate exceeds 5%'
    severity: 2
    enabled: true
    scopes: [
      appInsights.id
    ]
    evaluationFrequency: 'PT5M'
    windowSize: 'PT15M'
    criteria: {
      'odata.type': 'Microsoft.Azure.Monitor.SingleResourceMultipleMetricCriteria'
      allOf: [
        {
          name: 'High Error Rate'
          metricName: 'requests/failed'
          operator: 'GreaterThan'
          threshold: 5
          timeAggregation: 'Average'
          criterionType: 'StaticThresholdCriterion'
        }
      ]
    }
    actions: [
      {
        actionGroupId: actionGroup.id
      }
    ]
  }
}

// Alert Rule: Slow Response Time
resource slowResponseAlert 'Microsoft.Insights/metricAlerts@2018-03-01' = {
  name: '${baseName}-slow-response-${environment}'
  location: 'global'
  tags: tags
  properties: {
    description: 'Alert when average response time exceeds 2 seconds'
    severity: 3
    enabled: true
    scopes: [
      appInsights.id
    ]
    evaluationFrequency: 'PT5M'
    windowSize: 'PT15M'
    criteria: {
      'odata.type': 'Microsoft.Azure.Monitor.SingleResourceMultipleMetricCriteria'
      allOf: [
        {
          name: 'Slow Response'
          metricName: 'requests/duration'
          operator: 'GreaterThan'
          threshold: 2000  // 2 seconds in milliseconds
          timeAggregation: 'Average'
          criterionType: 'StaticThresholdCriterion'
        }
      ]
    }
    actions: [
      {
        actionGroupId: actionGroup.id
      }
    ]
  }
}

// Outputs
output logAnalyticsWorkspaceId string = logAnalytics.id
output logAnalyticsWorkspaceName string = logAnalytics.name
output logAnalyticsCustomerId string = logAnalytics.properties.customerId

output appInsightsName string = appInsights.name
output appInsightsId string = appInsights.id
output appInsightsInstrumentationKey string = appInsights.properties.InstrumentationKey
output appInsightsConnectionString string = appInsights.properties.ConnectionString

output azureMonitorWorkspaceId string = enablePrometheusGrafana ? azureMonitorWorkspace.id : ''
output grafanaEndpoint string = enablePrometheusGrafana ? managedGrafana.properties.endpoint : ''
output grafanaId string = enablePrometheusGrafana ? managedGrafana.id : ''

output actionGroupId string = actionGroup.id

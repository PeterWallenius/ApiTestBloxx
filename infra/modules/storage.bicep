param location string

var storageAccountName = take('stapitestbloxx${uniqueString(resourceGroup().id)}', 24)
var tableNames = [
  'ApiCallHistory'
  'SavedRequests'
]

resource storage 'Microsoft.Storage/storageAccounts@2024-01-01' = {
  name: storageAccountName
  location: location
  kind: 'StorageV2'
  sku: {
    name: 'Standard_LRS'
  }
  properties: {
    accessTier: 'Hot'
    allowBlobPublicAccess: false
    allowSharedKeyAccess: false
    defaultToOAuthAuthentication: true
    minimumTlsVersion: 'TLS1_2'
    supportsHttpsTrafficOnly: true
    publicNetworkAccess: 'Enabled'
  }
}

resource tableService 'Microsoft.Storage/storageAccounts/tableServices@2024-01-01' = {
  parent: storage
  name: 'default'
}

resource tables 'Microsoft.Storage/storageAccounts/tableServices/tables@2024-01-01' = [
  for tableName in tableNames: {
    parent: tableService
    name: tableName
  }
]

output name string = storage.name
output tableEndpoint string = storage.properties.primaryEndpoints.table

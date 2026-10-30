targetScope = 'resourceGroup'

@description('Azure region for all resources.')
param location string = resourceGroup().location

@description('Globally unique name of the Web App.')
param webAppName string

@description('App Service plan SKU. F1 is free; B1 adds Always On and removes the WebSocket/CPU limits.')
@allowed([
  'F1'
  'B1'
])
param appServicePlanSku string = 'F1'

module storage 'modules/storage.bicep' = {
  params: {
    location: location
  }
}

module keyVault 'modules/keyVault.bicep' = {
  params: {
    location: location
  }
}

module appService 'modules/appService.bicep' = {
  params: {
    location: location
    webAppName: webAppName
    skuName: appServicePlanSku
    keyVaultUri: keyVault.outputs.uri
    tableEndpoint: storage.outputs.tableEndpoint
  }
}

module roleAssignments 'modules/roleAssignments.bicep' = {
  params: {
    principalId: appService.outputs.principalId
    webAppName: webAppName
    storageAccountName: storage.outputs.name
    keyVaultName: keyVault.outputs.name
  }
}

output webAppUrl string = 'https://${appService.outputs.defaultHostName}'
output keyVaultName string = keyVault.outputs.name
output storageAccountName string = storage.outputs.name

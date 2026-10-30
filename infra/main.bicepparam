using 'main.bicep'

param location = readEnvironmentVariable('AZURE_LOCATION', 'denmarkeast')
param webAppName = readEnvironmentVariable('AZURE_WEBAPP_NAME', 'app-apitestbloxx')
param appServicePlanSku = 'F1'

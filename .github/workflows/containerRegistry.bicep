
param acrName string
param envName string
param location string = resourceGroup().location

resource containerRegistry 'Microsoft.ContainerRegistry/registries@2021-06-01-preview' = {
  name: '${acrName}${envName}'
  location: location
  sku: {
    name: 'Basic'
  }
  properties: {
    adminUserEnabled: false
  }
  identity: {
    type: 'SystemAssigned'
  }
}

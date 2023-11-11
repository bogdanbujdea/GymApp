param location string = resourceGroup().location
param containerAppName string
param managedEnvironmentName string
param envName string
param acrName string

module containerApp 'containerApp.bicep'  = {
  name: 'gym-app-container-app'
  params:{
    location: location
    containerAppName: containerAppName
    managedEnvironmentName: managedEnvironmentName
    envName: envName
    acrName: acrName
  }
}

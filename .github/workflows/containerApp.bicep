param location string = resourceGroup().location
param containerAppName string
param managedEnvironmentName string
param envName string
param acrName string
var acrPullRoleId = '7f951dda-4ed3-4680-a7ca-43fe172d538d'

resource managedEnvironment 'Microsoft.App/managedEnvironments@2023-05-02-preview' = {
  name: '${managedEnvironmentName}-${envName}'
  location: location
  properties: {
    zoneRedundant: false
    kedaConfiguration: {}
    daprConfiguration: {}
    customDomainConfiguration: {}
    workloadProfiles: [
      {
        workloadProfileType: 'Consumption'
        name: 'Consumption'
      }
    ]
    peerAuthentication: {
      mtls: {
        enabled: false
      }
    }
  }
}

resource userAssignedIdentity 'Microsoft.ManagedIdentity/userAssignedIdentities@2018-11-30' = {
  name: 'gymappapp-${envName}-identity'
  location: location
}


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

resource acrPullRoleAssignment 'Microsoft.Authorization/roleAssignments@2020-04-01-preview' = {
  name: guid(containerRegistry.id, 'acrpull')
  scope: containerRegistry
  properties: {
    roleDefinitionId: subscriptionResourceId('Microsoft.Authorization/roleDefinitions', acrPullRoleId) // Role definition ID for AcrPull
    principalId: userAssignedIdentity.properties.principalId
    principalType: 'ServicePrincipal'
  }
}

resource containerApp 'Microsoft.App/containerapps@2023-05-02-preview' = {
  name: '${containerAppName}-${envName}'
  location: location
  identity: {
    type: 'UserAssigned'
    userAssignedIdentities:{
      '${userAssignedIdentity.id}': {}
    }
  }
  properties: {
    managedEnvironmentId: managedEnvironment.id
    environmentId: managedEnvironment.id
    workloadProfileName: 'Consumption'
    configuration: {
      activeRevisionsMode: 'Single'
      ingress: {
        external: true
        targetPort: 80
        exposedPort: 0
        transport: 'Auto'
        traffic: [
          {
            weight: 100
            latestRevision: true
          }
        ]
        allowInsecure: false
      }
      registries: [
        {
          server: 'gymappregistry${envName}.azurecr.io'
          identity: userAssignedIdentity.id
        }
      ]
    }
    template: {
      containers: [
        {
          image: 'gymappregistry${envName}.azurecr.io/gymappbackend:latest'
          name: 'gymapp-api-image'
          resources: {
            cpu: 2
            memory: '4Gi'
          }
          probes: []
        }
      ]
      scale: {
        minReplicas: 0
        maxReplicas: 10
      }
      volumes: []
    }
  }
}

//resource containerAppName_current 'Microsoft.App/containerApps/sourcecontrols@2023-05-02-preview' = {
//  parent: containerApp
//  name: 'current'
//  properties: {
//    repoUrl: 'https://github.com/gymapp/gymapp-Backend'
//    branch: 'main'
//    githubActionConfiguration: {
//      registryInfo: {
//        registryUrl: 'gymappdev.azurecr.io'
//      }
//      contextPath: './'
//      image: 'gym-app:\${{ github.sha }}'
//    }
//  }
//}

//resource roleAssignment 'Microsoft.Authorization/roleAssignments@2020-04-01-preview' = {
//  name: guid(subscription().id, containerApp.id, acrPullRoleId)
//  properties: {
//    roleDefinitionId: subscriptionResourceId('Microsoft.Authorization/roleDefinitions', acrPullRoleId)
//    principalId: containerApp.identity.principalId
//    principalType: 'ServicePrincipal'
//  }
//  scope: containerRegistry
//}

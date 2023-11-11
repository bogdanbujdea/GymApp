
$resourceGroupName = "rg-gymapp-test"
$location = "eastus"
$configFileName = "./configurations/qa.json"
$acrName = "gymappregistryqa"

##!/bin/bash
#
## Modify these variables according to your environment
#ACR_NAME="gymappdev"
#SERVICE_PRINCIPAL_NAME="gym-app-qa"
#
## Obtain the full registry ID
#ACR_REGISTRY_ID=$(az acr show --name $ACR_NAME --query "id" --output tsv)
#
## Create the service principal with pull access to the registry
#PASSWORD=$(az ad sp create-for-rbac --name $SERVICE_PRINCIPAL_NAME --scopes $ACR_REGISTRY_ID --role acrpull --query "password" --output tsv)
#USER_NAME=$(az ad sp list --display-name $SERVICE_PRINCIPAL_NAME --query "[].appId" --output tsv)
#
## Output the credentials
#echo "Service principal ID: $USER_NAME"
#echo "Service principal password: $PASSWORD"



az group create --name $resourceGroupName --location $location


# Deploy ACR with Bicep
az deployment group create --resource-group $resourceGroupName --template-file ./containerRegistry.bicep --parameters $configFileName

# Build and Push Docker Image
docker build -t <ACRLoginServer>/<ImageName>:<Tag> <DockerContext>
az acr login --name <ACRName>
docker push <ACRLoginServer>/<ImageName>:<Tag>

# Deploy Container App with Bicep
az deployment group create --resource-group <ResourceGroupName> --template-file <BicepFileForContainerApp> --parameters <ParametersIfAny>






az deployment group create --resource-group $resourceGroupName `
 --template-file ./main.bicep `
 --parameters $configFileName 
 
 az deployment group create --resource-group $resourceGroupName `
 --template-file ./main.bicep `
 --parameters $configFileName 

#containerAppPrincipalId=az containerapp show --name $containerAppName --resource-group $resourceGroupName --query "identity.principalId" --output tsv
#
#acrResourceId=$(az acr show --name $acrName --query "id" --output tsv)
#
#z role assignment create --assignee $containerAppPrincipalId --role acrpull --scope $acrResourceId

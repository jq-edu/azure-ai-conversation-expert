# assign data contributor in cosmos https://learn.microsoft.com/en-us/azure/cosmos-db/nosql/security/how-to-grant-data-plane-role-based-access?tabs=built-in-definition%2Ccsharp&pivots=azure-interface-cli#permission-model

$subscriptionId = ""
$resourceGroup = ""
$cosmosAccount = ""
$principalId = ""

# cosmos contributor
$roleId = "/subscriptions/$($subscriptionId)/resourceGroups/$($resourceGroup)/providers/Microsoft.DocumentDB/databaseAccounts/$($cosmosAccount)/sqlRoleDefinitions/00000000-0000-0000-0000-000000000002"
$scope = "/subscriptions/$($subscriptionId)/resourceGroups/$($resourceGroup)/providers/Microsoft.DocumentDB/databaseAccounts/$($cosmosAccount)"

az cosmosdb sql role assignment create --resource-group $resourceGroup --account-name $cosmosAccount --role-definition-id $roleId --principal-id $principalId --scope $scope
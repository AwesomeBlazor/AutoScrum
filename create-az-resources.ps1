param(
    [Parameter()]
    [ValidateNotNullOrEmpty()]
    [string] $AppName = 'auto-scrum-app',

    [Parameter()]
    [ValidateNotNullOrEmpty()]
    [string] $ResourceGroup = 'auto-scrum',

    [Parameter()]
    [ValidateSet('CentralUS', 'EastAsia', 'EastUS2', 'WestEurope', 'WestUS2')]
    [string] $Location = 'CentralUS',

    [Parameter()]
    [ValidateNotNullOrEmpty()]
    [string] $GitHubPat = $null)

If ($null -eq $GitHubPat -or "" -eq $GitHubPat) {
    Write-Warning "Required GitHub Personal Access Token."
    exit
}

az staticwebapp create \
    -n $AppName \
    -g $ResourceGroup  \
    -s https://github.com/<YOUR_GITHUB_ACCOUNT_NAME>/my-first-static-web-app \
    -l $Location \
    -b main \
    --token $GitHubPat


RESOURCE_GROUP_NAME=<your resource group name>
STORAGE_ACC_NAME=<your storage account name>
LOCATION=<Azure region>

az group create -n $RESOURCE_GROUP_NAME -l $LOCATION
az storage account create -n $STORAGE_ACC_NAME -g $RESOURCE_GROUP_NAME -l $LOCATION --sku Standard_LRS
az storage blob service-properties update --account-name $STORAGE_ACC_NAME --static-website --404-document error.html --index-document index.html
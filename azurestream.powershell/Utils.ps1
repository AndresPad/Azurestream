#---------------------------------------------------------[Global Variables]-----------------------------------------------------
#region
    $Global:CloneAuthenticationToken = $null

    $activeDirectoryEndpointUrl    = "https://login.microsoftonline.com/common"  #Endpoint for Microsoft Identity Platform
    $resourceManagerEndpointUrl    = "https://management.azure.com/.default"     #Endpoint for Azure Resource Manager REST API
    $synapseDevelopmentEndpointUrl = "https://dev.azuresynapse.net/.default"     #Endpoint for Synapse Rest API

    $psGetDocs     = "Documentation is available at https://docs.microsoft.com/en-us/powershell/module/powershellget/?view=powershell-7.1"
    $azPSDocs      = "Documentation is available at https://docs.microsoft.com/en-us/powershell/azure/new-azureps-module-az?view=azps-5.5.0"
    $msalPSDocs    = "Documentation is available at https://www.powershellgallery.com/packages/MSAL.PS/4.2.1.3"

    #Default App Settings JSON
    $defaultConfigContent = '{
        "AzureSettings": {
            "TenantID": "<fill in the Tenant ID from the Azure Portal>",
            "SubscriptionId": "<fill in the Subscription ID from the Azure Portal>",
            "ClientID": "<fill in the Service Principal ID from the Azure Portal>",
            "ClientSecret": "<fill in the Service Principal Secret from the Azure Portal>",
            "region": "<select from available regions: australiaeast, eastus, eastus2, japaneast, northeurope, southcentralus, southeastasia, uksouth, westeurope, westus2>"
        },
        "DataFactory": {
            "ResourceGroup": "<resource group name where data factory is located>",
            "Name": "<Data Factory Name>",
            "ResourceId": "<Data Factory Resource ID i.e., /subscriptions/YOURSUBSCRIPTIONID/resourceGroups/RESOURCEGROUPOFDATAFACTORY/providers/Microsoft.DataFactory/factories/YOURDATAFACTORYNAME>",
            "apiVersion": "<api version i.e., 2018-06-01>"
        },
        "SynapseWorkspace": {
            "ResourceGroup": "<resource group name were synapse is located>",
            "Name": "<Synapse Workspace Name>",
            "ResourceId": "<Synapse Resource ID i.e., /subscriptions/YOURSUBSCRIPTIONID/resourceGroups/RESOURCEGROUPOFSYNAPSEWORKSPACE/providers/Microsoft.Synapse/workspaces/YOURSYNAPSEWORKSPACENAME>",
            "apiVersion": "<api version i.e., 2018-06-01>"
        }
    }'
#endregion

#---------------------------------------------------------[Prerequisites]-----------------------------------------------------
#region
    function CheckPrerequisites()
    {
        $azPSGetInstalled = Get-Module -ListAvailable -Name PowerShellGet
        if (-Not $azPSGetInstalled) {
            WriteErrorResponse "  PowerShellGet module is not installed - Install it via 'Install-Module -Name PowerShellGet -Force'. $($psGetDocs)"
            #'Install-Module -Name PowerShellGet -Force
            return $False
        }

        $azResourcesInstalled = Get-Module -ListAvailable -Name Az.Resources
        if (-Not $azResourcesInstalled) {
            WriteErrorResponse "  Az.Resources module is not installed - Install it via 'Install-Module -Name Az.Resources -AllowClobber -Force'. $($azPSDocs)"
            #'Install-Module -Name Az.Resources -AllowClobber -Force
            return $False
        }

        $azAccountsInstalled = Get-Module -ListAvailable -Name Az.Accounts
        if (-Not $azAccountsInstalled) {
            WriteErrorResponse "  Az.Accounts module is not installed - Install it via 'Install-Module -Name Az.Accounts -AllowClobber -Force'. $($azPSDocs)"
            #Install-Module -Name Az.Accounts -AllowClobber -Force
            return $False
        }

        $azMSALInstalled = Get-Module -ListAvailable -Name MSAL.PS
        if (-Not $azMSALInstalled) {
            WriteErrorResponse "  MSAL.PS module is not installed - Install it via 'Install-Module -Name MSAL.PS -AllowClobber -Force -AcceptLicense'. $($msalPSDocs)"
            #'Install-Module -Name MSAL.PS -AllowClobber -Force -AcceptLicense
            return $False
        }

        return $True
    }
#endregion

#---------------------------------------------------------[Login]-----------------------------------------------------
#region
    function Login($config)
    {
        try
        {
            #Login to Azure (programmatically)
            $pscredential = New-Object -TypeName System.Management.Automation.PSCredential($config.AzureSettings.ClientID, (ConvertTo-SecureString $config.AzureSettings.ClientSecret -AsPlainText -Force))
            Connect-AzAccount -Credential $pscredential -Tenant $config.AzureSettings.TenantId -ServicePrincipal

            return $true
        }
        catch {
            WriteErrorResponse "You were not able to log in. Please check the Service Principal Client ID and Secret or log in via the Connect-AzAccount command"
            Write-Host $_
            throw
        }
    }

    #-------------------------------------------------------------------
    function CheckLogin()
    {
        $context = Get-AzContext
        if (!$context)
        {
            WriteErrorResponse "Not logged into a subscription. You need to log in via the Connect-AzAccount command."
            return $False
        }

        Write-Host "#--------------------------------------------------------------------------------------------------------"
        WriteSuccess "  Logged into Subscription: '$($context.Name)' TenantId: '$($context.Tenant.Id)'"
        Write-Host "#--------------------------------------------------------------------------------------------------------"

        return $True
    }

    #-------------------------------------------------------------------
    function GetAuthenticationToken() {
        if ($Global:CloneAuthenticationToken) {
            return $Global:CloneAuthenticationToken
        }
        else {
            WriteLine
            WriteInformation ("Getting an authentication token ...")

            if ($Global:CloneAuthenticationToken -eq $true) {
                #$Global:CloneAuthenticationToken = (Get-MsalToken -ClientId $config.AzureSettings.ClientID -Scope  $resourceManagerEndpointUrl -RedirectUri "http://localhost" -Authority "$activeDirectoryEndpointUrl/$($config.AzureSettings.TenantID)").AccessToken
                 $Global:CloneAuthenticationToken = (Get-MsalToken -ClientId $config.AzureSettings.ClientID -ClientSecret (ConvertTo-SecureString $config.AzureSettings.ClientSecret -AsPlainText -Force) -TenantId $config.AzureSettings.TenantID -Scope 'https://management.azure.com/.default').AccessToken
                return $Global:CloneAuthenticationToken
            }

            $Global:CloneAuthenticationToken = (Get-MsalToken -ClientId $config.AzureSettings.ClientID -Scope $synapseDevelopmentEndpointUrl -RedirectUri "http://localhost" -Authority "$activeDirectoryEndpointUrl/$($config.AzureSettings.TenantID)").AccessToken
            return $Global:CloneAuthenticationToken
        }
    }

    #-------------------------------------------------------------------
    function GetHeader {
        Param (
            [bool]$armToken
        )

            $token = AcquireToken -armToken $armToken

            return @{
                'Authorization' = "Bearer $token"
            }
    }

    #-------------------------------------------------------------------
    function AcquireToken {
        Param (
            [bool]$armToken
        )


        if ($armToken -eq $true) {
            return (Get-MsalToken -ClientId $config.AzureSettings.ClientID -ClientSecret (ConvertTo-SecureString $config.AzureSettings.ClientSecret -AsPlainText -Force) -TenantId $config.AzureSettings.TenantID -Scope 'https://management.azure.com/.default').AccessToken
        }

        return (Get-MsalToken -ClientId $config.AzureSettings.ClientID -ClientSecret (ConvertTo-SecureString $config.AzureSettings.ClientSecret -AsPlainText -Force) -TenantId $config.AzureSettings.TenantID -Scope 'https://dev.azuresynapse.net/.default').AccessToken
    }
#endregion

#---------------------------------------------------------[Load Config File]-----------------------------------------------------
#region
    function GetDefaultConfig() {
        $defaultConfig = ConvertFrom-Json($defaultConfigContent)
        return $defaultConfig
    }

    function LoadConfig(
        [string] $fileLocation,
        [string] $TenantID,
        [string] $SubscriptionId,
        [string] $ResourceGroupDataFactory,
        [string] $DataFactoryName,
        [string] $ResourceGroupSynapse,
        [string] $SynapseName
    ) {

        try {
            $configFromFile = Get-Content -Path $fileLocation -Raw | ConvertFrom-Json
        }
        catch {
            WriteError("Could not parse config json file at: $fileLocation. Please ensure that it is a valid json file (use a json linter, often a stray comma can make your file invalid)")
            return $null
        }

        $defaultConfig = GetDefaultConfig
        $config = $defaultConfig
        if ([bool]($configFromFile | get-member -name "AzureSettings")) {
            $configFromFile.AzureSettings.psobject.properties | ForEach-Object {
                $config.AzureSettings | Add-Member -MemberType $_.MemberType -Name $_.Name -Value $_.Value -Force
            }
        }

        if ([bool]($configFromFile | get-member -name "DataFactory")) {
            $configFromFile.DataFactory.psobject.properties | ForEach-Object {
                $config.DataFactory | Add-Member -MemberType $_.MemberType -Name $_.Name -Value $_.Value -Force
            }
        }

        if ([bool]($configFromFile | get-member -name "SynapseWorkspace")) {
            $configFromFile.SynapseWorkspace.psobject.properties | ForEach-Object {
                $config.SynapseWorkspace | Add-Member -MemberType $_.MemberType -Name $_.Name -Value $_.Value -Force
            }
        }

        #Override App Settings Tenant ID
        if (-Not [string]::IsNullOrEmpty($TenantID)) {
            $config.AzureSettings.TenantID = $TenantID
        }

        #Override App Settings Subscription ID
        if (-Not [string]::IsNullOrEmpty($SubscriptionId)) {
            $config.AzureSettings.SubscriptionId = $SubscriptionId
        }

        #Override App Settings Data Factory Resource Group
        if (-Not [string]::IsNullOrEmpty($ResourceGroupDataFactory)) {
            $config.DataFactory.ResourceGroup = $ResourceGroupDataFactory
        }

        #Override App Settings Data Factory Name Entry
        if (-Not [string]::IsNullOrEmpty($DataFactoryName)) {
            $config.DataFactory.Name = $DataFactoryName
        }

        #Override App Settings Synapse Resource Group Entry
        if (-Not [string]::IsNullOrEmpty($ResourceGroupSynapse)) {
            $config.SynapseWorkspace.ResourceGroup = $ResourceGroupSynapse
        }

        #Override App Settings Synapse Name Entry
        if (-Not [string]::IsNullOrEmpty($SynapseName)) {
            $config.SynapseWorkspace.Name = $SynapseName
        }

        $config.DataFactory.ResourceId = "/subscriptions/$($config.AzureSettings.SubscriptionId)/resourceGroups/$($config.DataFactory.ResourceGroup)/providers/Microsoft.DataFactory/factories/$($config.DataFactory.Name)"
        $config.SynapseWorkspace.ResourceId = "/subscriptions/$($config.AzureSettings.SubscriptionId)/resourceGroups/$($config.SynapseWorkspace.ResourceGroup)/providers/Microsoft.Synapse/workspaces/$($config.SynapseWorkspace.Name)"

        return $config
    }
#endregion

#---------------------------------------------------------[Format Output Messages]-----------------------------------------------------
#region
    function WriteError([string] $message) {
        Write-Host -ForegroundColor Red $message;
    }

    function WriteSuccess([string] $message) {
        Write-Host -ForegroundColor Green $message;
    }
    function WriteSuccessResponse([string] $message) {
        Write-Host -ForegroundColor Green "#--------------------------------------------------------------------------------------------------------";
        WriteInformation($message)
        Write-Host -ForegroundColor Green "#--------------------------------------------------------------------------------------------------------";
    }

    function WriteErrorResponse([string] $message) {
        Write-Host -ForegroundColor Red "#--------------------------------------------------------------------------------------------------------";
        WriteInformation($message)
        Write-Host -ForegroundColor Red "#--------------------------------------------------------------------------------------------------------";
    }

    function WriteInformation([string] $message) {
        Write-Host -ForegroundColor White $message;
    }

    function WriteLine {
        Write-Host `n;
        Write-Host "--------------------------------------------------------------------------------------------------------------------" ;
        Write-Host `n;
    }

    function WriteProgress($activity, $status) {
        Write-Progress -Activity $activity -Status $status;
    }
#endregion
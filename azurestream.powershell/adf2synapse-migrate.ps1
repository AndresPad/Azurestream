<#
 .NOTES
        =========================================================================================================
        Created by:       Author: Fast Track, Synapse Analytics and Azure Data Factory team
        Created on:       03/15/2021
        =========================================================================================================

 .DESCRIPTION
    ADF to Synapse Migration PowerShell script
    .\adf2synapse-migrate.ps1 -sourceADFResourceId "/subscriptions/YOURSUBSCRIPTIONID/resourceGroups/azu-data-rg/providers/Microsoft.DataFactory/factories/airstream" -destSynapseResourceId "/subscriptions/YOURSUBSCRIPTIONID/resourceGroups/AZU-DW-RG/providers/Microsoft.Synapse/workspaces/air"
    .\adf2synapse-migrate.ps1 -ConfigFile "appsettings.json"
 .LINK
 .EXAMPLE
#>

[CmdletBinding(DefaultParameterSetName = "ByResourceID")]
param(
    [Parameter(ParameterSetName = 'ByResourceID')]
    [string]$sourceADFResourceId,

    [Parameter(ParameterSetName = 'ByResourceID')]
    [string]$destSynapseResourceId,

    [Parameter(ParameterSetName = 'ByConfigFile')]
    [string]$ConfigFile
)

#Load Utility File
. "$PSScriptRoot\Utils.ps1" #include Utils for Console Messages, Logging, Config parsing

function Custom-Write-Host($str) {
    Write-Host "$(Get-Date) $str"
}
function MigrateADFSynapse {

    # if config file is specified, use that to load the config file
    Write-Host "-----------------------------------------------------------"
    if ( $ConfigFile ) {
        Write-Host -ForegroundColor Yellow "Migration Config File: $ConfigFile"

        #Load Config file
        try {
            if ( Test-Path $ConfigFile ) {
                $config = LoadConfig `
                -fileLocation $ConfigFile `
                -TenantId $TenantId `
                -SubscriptionId $SubscriptionId `
                -ResourceGroupDataFactory $ResourceGroupDataFactory `
                -DataFactoryName $DataFactoryName `
                -ResourceGroupSynapse $ResourceGroupSynapse `
                -SynapseName $SynapseName `
                -ClientID $ClientID `
                -ClientSecret $ClientSecret
            }
            else {
                Custom-Write-Host("[Error] reading config file - Exiting.")
                exit -1
            }

        }
        catch {
            Custom-Write-Host("[Error] reading config file - Exiting.")
            exit -1
        }

        $sourceADFResourceId = $config.DataFactory.ResourceId
        $destSynapseResourceId = $config.SynapseWorkspace.ResourceId

    }
    elseif  ($sourceADFResourceId)   {
        # check whether destinationId is specified
        if ($destSynapseResourceId) {

        }
        else {
            Custom-Write-Host("[Error] Specify a Destination Resource ID for Synapse Analytics Workspace")
            exit -1
        }
    }
    else {
        Write-Host "ADF to Synapse Migration PowerShell script"
        Write-Host ""
        Write-Host "Syntax"
        Write-Host "   adf2synapse-migrate.ps [-ConfigFile <Filename>] "
        Write-Host "   adf2synapse-migrate.ps [-sourceADFResourceId <String>] [-destADFResourceId <String>]"
        Write-Host ""
        Write-Host "Docs: https://aka.ms/adf/adf2synapse-migration "
        exit 0
    }

    Write-Host -ForegroundColor Yellow "Resource Ids"
    Write-Host "$sourceADFResourceId"
    Write-Host "$destSynapseResourceId"


    #Write-Host "$ADFAPIVersion $SynapseAPIVersion"
    Write-Host
    Write-Host -ForegroundColor Yellow "Migration Details"
    $match = "$sourceADFResourceId" | Select-String -Pattern '^\/subscriptions\/(\S+)\/resourceGroups\/(\w+)\/providers\/Microsoft.DataFactory\/factories\/(\w+)'
    $ADFSubscriptionId, $ADFresourceGroupName, $dataFactoryName =  $match.Matches[0].Groups[1..3].Value
    Write-Host "Subscrption Id: $ADFSubscriptionId "
    Write-Host "Resource Group: $ADFresourceGroupName "
    Write-host "Data Factory Name: $dataFactoryName"


    $match = "$destSynapseResourceId" | Select-String -Pattern '^\/subscriptions\/(\S+)\/resourceGroups\/(\w+)\/providers\/Microsoft.Synapse\/workspaces\/(\w+)'
    $SynapseSubscriptionId, $SynapseresourceGroupName, $SynapseName =  $match.Matches[0].Groups[1..3].Value
    Write-Host "Subscrption Id: $SynapseSubscriptionId "
    Write-Host "Resourec Group $SynapseresourceGroupName "
    Write-Host "Synapse Analytics Workspace Name:  $SynapseName"

    Write-Host "-----------------------------------------------------------"

    # At this point, we have the resource ID for both source and destination
    # Let's login
    Connect-AzAccount
    Select-AzSubscription -SubscriptionId $ADFSubscriptionId

    # check whether resource ex
    $adf = Get-AzDataFactoryV2  -ResourceGroupName $ADFresourceGroupName -Name $dataFactoryName -ErrorAction Continue
    if (-Not $adf) {
        Custom-Write-Host("[Error] Data Factory does not exist, or you do not have access to it.")
        exit -1
    }

    $syn = Get-AzSynapseWorkspace -ResourceGroupName  $SynapseresourceGroupName   -Name $SynapseName -ErrorAction Continue
    if (-Not $syn) {
        Custom-Write-Host("[Error] Synapse Workspace does not exist or you do not have access to it.")
        exit -1
    }

    # ready to migrate
    Custom-Write-Host("[Info] Start Migration")
    StartMigration $sourceADFResourceId $destSynapseResourceId
    Custom-Write-Host("[Info] Migration Completed")
}

function PollUntilCompletion {
    Param (
        [string] $uri,
        [string] $originalUri,
        [string] $resourceName,
        [bool] $isArmToken
        )

        Write-Output "Waiting for operation to complete..."

        try
        {
            $token = GetAuthenticationToken -armToken $isArmToken -signIn $signIn
            $response = Invoke-WebRequest -UseBasicParsing -Uri $uri -Method Get -ContentType "application/json" -Headers @{ Authorization = "Bearer $token" }

            if ($response.StatusCode -ge 203)
            {
                Write-Error "Error migrating resource $originalUri"
                throw
            }

            if ($response.StatusCode -ne 200)
            {
                Start-Sleep -Seconds 1
                PollUntilCompletion $uri $originalUri $resourceName $isArmToken
                return;
            }

            if ($response.StatusCode -eq 200)
            {
                WriteSuccess "Successfully migrated $resourceName"
                return;
            }

            #if ((ConvertFrom-Json -InputObject $response.Content).status -eq 'Failed') {
                #Write-Error "Error on creating resource $originalUri. Details: $response.Content"
                #throw
            #}
        }
        catch [Exception] {
            Write-Error "An occur has occured. Error Message: $($_.Exception.Message)"
            Write-Error "Error Details: $($_.ErrorDetails.Message)"
            throw
        }
}
function StartMigration {
    [CmdletBinding()]
    Param (
        [string]$srcResourceId,
        [string]$destResourceId
    )

    $allResources = New-Object Collections.Generic.List[String]
    $allResources.Add("integrationRuntimes");
    $allResources.Add("linkedServices");
    $allResources.Add("datasets");
    $allResources.Add("dataflows");
    $allResources.Add("pipelines");
    $allResources.Add("triggers");

    $allResources | ForEach-Object -Process { ProcessResource -srcResourceId $srcResourceId -destResourceId $destResourceId -resourceType $_ }
    Write-Host "#--------------------------------------------------------------------------------------------------------`n"
}

function ProcessResource {
    [CmdletBinding()]
    Param (
        [string]$srcResourceId,
        [string]$destResourceId,
        [string]$resourceType
    )

    #$srcResource = Get-AzResource -ResourceId $DataFactoryResourceId -ApiVersion $config.DataFactory.apiVersion
    $destResource = Get-AzResource -ResourceId $destResourceId -ApiVersion $SynapseAPIVersion

    $srcUri = "https://management.azure.com" + $srcResourceId

    if ($resourceType -eq "integrationRuntimes" -or $resourceType -eq "sqlPools" -or $resourceType -eq "sparkPools") {
        $isDestArm = $true;
        $destUri = "https://management.azure.com" + $destResourceId
    } else {
        $isDestArm = $false;
        $destUri = $destResource.Properties.connectivityEndpoints.dev
    }

    $resourcesToBeCopied =  New-Object Collections.Generic.List[Object]
    $uri = "$srcUri/$($resourceType)?api-version=$($ADFAPIVersion)"

    try {
        $token = GetAuthenticationToken -armToken $true -signIn $Global:SignIn
        $srcResponse = Invoke-RestMethod -UseBasicParsing -Uri $uri -Method Get -ContentType "application/json" -Headers @{ Authorization = "Bearer $token" }

        #For future versions of this tool think about think about deleting all schema entries inside of datasets before running invoke-restmethod

        if ($srcResponse.Value.Length -gt 0) {
            Write-Host "  Processing $resourceType" -ForegroundColor Blue
            $resourcesToBeCopied.AddRange($srcResponse.Value);
            WriteSuccessResponse("  Currently there are $($resourcesToBeCopied.Count) $resourceType")
        }
        elseif($resourcesToBeCopied.Count -le 0) {
            return;
        }
    }
    catch [Exception] {
        Write-Error "Error listing $resourceType : $_"
        throw
    }

    $resourcesToBeCopied | ForEach-Object -Process {
        $uri = "$destUri/$resourceType/$($_.name)?api-version=$SynapseAPIVersion";
        $jsonBody = ConvertTo-Json $_ -Depth 30
        $name = $_.name

        $destResponse = $null;

        #Check if you have an Azure-SSIS Integration Runtime. Azure-SSIS is currently not supported in Synapse Workspaces
        #Check if you have an Self-Hosted Integration Runtime that is Shared or Linked. Linked IR are currently not supported in this PowerShell script because you of Managed Identity references.
        #Check if you have an Azure Integration Runtime that is using a Managed Virtual Network. Managed VNets are currently not supported in Synapse Workspaces
        $is_ssis = $false
        $is_link = $false
        $is_vnet = $false

        if($resourceType -eq "integrationRuntimes"){
            $ssisObj = $_.PSObject.Properties["properties"].value.typeProperties | Get-Member -Name "ssisProperties"
            $linkObj = $_.PSObject.Properties["properties"].value.typeProperties | Get-Member -Name "linkedInfo"
            $vnetObj = $_.PSObject.Properties["properties"].value | Get-Member -Name "managedVirtualNetwork"

            if ([bool]$ssisObj){
                $is_ssis = $true
            }
            else{
                $is_ssis = $false
            }

            if ([bool]$linkObj){
                $is_link = $true
            }
            else{
                $is_link = $false
            }

            if ([bool]$vnetObj){
                $is_vnet = $true
            }
            else{
                $is_vnet = $false
            }
        }

        try {
            #If Integration Runtime is SSIS then Skip
            if (-Not $is_ssis)
            {
                #If Integration Runtime has is a Linked IR then Skip
                if (-Not $is_link)
                {
                    #If Integration Runtime has a VNet then Skip
                    if (-Not $is_vnet)
                    {
                        $token = GetAuthenticationToken -armToken $isDestArm -signIn $Global:SignIn
                        $destResponse = Invoke-WebRequest -UseBasicParsing -Uri $uri -Method Put -ContentType "application/json" -Body $jsonBody -Headers @{ Authorization = "Bearer $token" }

                        Write-Output "Migrating $resourceType $($name)"

                        if ($destResponse.StatusCode -eq 202)
                        {
                            PollUntilCompletion $destResponse.Headers.Location $uri $name $isDestArm
                        }
                        elseif ($null -eq $destResponse -or $destResponse.StatusCode -ne 200) {
                            Write-Error "Creation failed for $($name). Error: $($_.Exception.Message)"
                            throw
                        }
                    }
                    else{
                        Write-Host "    Managed VNet Integration Runtime with the following name will be filtered and will NOT be migrated: $($name)" -ForegroundColor Yellow
                        Write-Host ""
                    }
                }
                else{
                    Write-Host "    Self-Hosted (Linked) Integration Runtime with the following name will be filtered and will NOT be migrated: $($name)" -ForegroundColor Yellow
                    Write-Host ""
                }
            }
            else{
                Write-Host "    Azure-SSIS Integration Runtime with the following name will be filtered and will NOT be migrated: $($name)" -ForegroundColor Yellow
                Write-Host ""
            }
        }
        catch [Exception] {
            Write-Error "An error occured during migration for $($name). Error: $($_.Exception.Message)"
            throw
        }
    }
}

#$sourceADFResourceId = "subscriptions/YOURSUBSCRIPTIONID/resourceGroups/azu-data-rg/providers/Microsoft.DataFactory/factories/airstream"
#$destSynapseResourceId = "subscriptions/YOURSUBSCRIPTIONID/resourceGroups/AZU-DW-RG/providers/Microsoft.Synapse/workspaces/air"
$ConfigFile = "appsettings.json"

# Specify API version for ADF and Synapse
Set-Variable ADFAPIVersion -Option Constant -Value '2018-06-01'
Set-Variable SynapseAPIVersion  -Option Constant -Value '2019-06-01-preview'

# Entry point function to migration Script
MigrateADFSynapse

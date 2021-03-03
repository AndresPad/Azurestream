<#
 .NOTES
        =========================================================================================================
        Created by:       Author: Your Name | @twitterhandle | your.name@azurestream.io | azurestream.io
        Created on:       03/01/2021
        =========================================================================================================

 .DESCRIPTION

 .LINK

 .EXAMPLE

#>

#---------------------------------------------------------[Parameters]-----------------------------------------------------
#region Parameters
    Param(
        [string] $TenantID, #optional override for TenantID in config file
        [string] $SubscriptionId, #optional override for SubscriptionId in config file
        [string] $ResourceGroupDataFactory, #optional override for ResourceGroupDataFactory in config file
        [string] $DataFactoryName, #optional override for DataFactoryName in config file
        [string] $ResourceGroupSynapse, #optional override for ResourceGroupSynapse in config file
        [string] $SynapseName, #optional override for SynapseName in config file
        [string] $ConfigFile
    )

#endregion Parameters

Clear-Host
Set-PSDebug -Trace 0 -Strict
Set-ExecutionPolicy Unrestricted -Scope CurrentUser
Set-StrictMode -Version Latest

#---------------------------------------------------------[Cloneworkspace]-----------------------------------------------------
#region
    function CloneWorkspace {
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
#endregion

#---------------------------------------------------------[ProcessResource]-----------------------------------------------------
#region
function ProcessResource {
    [CmdletBinding()]
    Param (
        [string]$srcResourceId,
        [string]$destResourceId,
        [string]$resourceType
    )

    Write-Host "  Processing $resourceType" -ForegroundColor Blue

    $srcResource = Get-AzResource -ResourceId $config.DataFactory.ResourceId -ApiVersion $config.DataFactory.apiVersion
    $destResource = Get-AzResource -ResourceId $config.SynapseWorkspace.ResourceId -ApiVersion $config.SynapseWorkspace.apiVersion

    $srcUri = "https://management.azure.com" + $config.DataFactory.ResourceId

    if ($resourceType -eq "integrationRuntimes" -or $resourceType -eq "sqlPools" -or $resourceType -eq "sparkPools") {
        $isDestArm = $true;
        $destUri = "https://management.azure.com" + $config.SynapseWorkspace.ResourceId
    } else {
        $isDestArm = $false;
        $destUri = $destResource.Properties.connectivityEndpoints.dev
    }

    $resourcesToBeCopied =  New-Object Collections.Generic.List[Object]
    $uri = "$srcUri/$($resourceType)?api-version=$($config.DataFactory.apiVersion)"

    try {
        $token = AcquireToken -armToken $true
        #$srcResponse = Invoke-WebRequest -UseBasicParsing -Uri $uri -Method GET -ContentType "application/json" -Headers @{ Authorization = "Bearer $token" }
        $srcResponse = Invoke-RestMethod -UseBasicParsing -Uri $uri -Method Get -ContentType "application/json" -Headers @{ Authorization = "Bearer $token" }

        if ($srcResponse.Value.Length -gt 0) {
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
        $uri = "$destUri/$resourceType/$($_.name)?api-version=$($config.SynapseWorkspace.apiVersion)";
        $jsonBody = ConvertTo-Json $_ -Depth 30

        Write-Output "Cloning $resourceType $($_.name)"

        $destResponse = $null;

        try {
            $token = AcquireToken -armToken $isDestArm
            #$destResponse = Invoke-WebRequest -Method Put -Uri $uri -Headers $destHeaders -ContentType "application/json" -Body $jsonBody
            $destResponse = Invoke-WebRequest -UseBasicParsing -Uri $uri -Method Put -ContentType "application/json" -Body $jsonBody -Headers @{ Authorization = "Bearer $token" }
        }
        catch [Exception] {
            Write-Error "Creation failure for $($_.name). Error: $($_.Exception.Message)"
            throw
        }

        if ($destResponse.StatusCode -eq 202)
        {
            PollUntilCompletion $destResponse.Headers.Location $uri $isDestArm
        }
        elseif ($null -eq $destResponse -or $destResponse.StatusCode -ne 200) {
            Write-Error "Creation failed for $($_.name). Error: $($_.Exception.Message)"
            throw
        }
    }
}
#endregion

#---------------------------------------------------------[Entry Point - Execution of Script Starts Here]-----------------------------------------------------
#region Entry Point
    Write-Host "#--------------------------------------------------------------------------------------------------------";
    Write-Host "  Start Cloning Pipeline Script";
    Write-Host "#--------------------------------------------------------------------------------------------------------";

    #1. Load Utilities File
    . "$PSScriptRoot\Utils.ps1" #include Utils for Console Messages, Logging, Config parsing

    #2. Check for PowerShell Module Prerequisites
    $PrerequisitesInstalled = CheckPrerequisites
    if (-Not $PrerequisitesInstalled) {
        WriteError("Prerequisites are not installed - Exiting.")
        exit 1
    }

    #3. Load App Settings File
    if ([string]::IsNullOrEmpty($ConfigFile)) {
        $ConfigFile = "$PSScriptRoot\appsettings.json"
    }

    #4. Parse and Override App Settings File
    $config = LoadConfig `
        -fileLocation $ConfigFile `
        -TenantID $TenantID `
        -SubscriptionId $SubscriptionId

        if ($null -eq $config) {
            WriteError("Error reading config file - Exiting.")
            exit 1
        }

    #5. Login in with Service Principal
    Login $config
    $LoggedIn = CheckLogin
    if (-Not $LoggedIn) {
        WriteError("User not logged in - Exiting.")
        exit 1
    }

    #6. Begin Clonning Pipelines from ADF to Synapse
    CloneWorkspace $config.DataFactory.ResourceId $config.SynapseWorkspace.ResourceId

    #7. End Script and Logout
    Write-Host "#--------------------------------------------------------------------------------------------------------";
    Write-Host "  Azure Data Factory to Synapse Workspace Cloning Complete" -ForegroundColor Green
    Write-Host "#--------------------------------------------------------------------------------------------------------";
    Disconnect-AzAccount

#endregion Entry Point
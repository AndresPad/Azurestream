<#
 .NOTES
        =========================================================================================================
        Created by:       Author: 
        Created on:       03/23/2021
        =========================================================================================================

 .DESCRIPTION

      
 .LINK

 .EXAMPLE
        

#>

#---------------------------------------------------------[Parameters]-----------------------------------------------------
#region Parameters
    [string] $TenantId, #optional override for TenantId in config file
    [string] $SubscriptionId, #optional override for SubscriptionId in config file
    [string] $ResourceGroupDataFactory, #optional override for ResourceGroupDataFactory in config file
    [string] $DataFactoryName, #optional override for DataFactoryName in config file
    [string] $ResourceGroupSynapse, #optional override for ResourceGroupSynapse in config file
    [string] $SynapseName, #optional override for SynapseName in config file
    [string] $ClientID, #optional override for SynapseName in config file
    [string] $ClientSecret #optional override for SynapseName in config file
    )
#endregion Parameters

Clear-Host
Set-PSDebug -Trace 0 -Strict
Set-ExecutionPolicy Unrestricted -Scope CurrentUser
Set-StrictMode -Version Latest

#---------------------------------------------------------[Load Utility File]-----------------------------------------------------
#region Utility

    #Load Utility File
    . "$PSScriptRoot\Utils.ps1" #include Utils for Console Messages, Logging, Config parsing

#endregion Utility

#---------------------------------------------------------[StartYourMainFunction]-----------------------------------------------------
#region
function StartYourMainFunction {
    #If Config file is specified, use the appsettings.json file
    if($ConfigFile) {
        Write-Host -ForegroundColor Yellow "Migration Config File: $ConfigFile"

        #Load Config file
        try{
            if (-Not [string]::IsNullOrEmpty($ConfigFile)) {
                $ConfigFile = "$PSScriptRoot\$ConfigFile"

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

                    if ($null -eq $config) {
                        WriteError("[Error] reading config file - check the syntax within your config file. Make sure the JSON is properly formatted.")
                        exit 1
                    }
                }
                else {
                    WriteError("[Error] reading config file - File path, file name or directory does not exist: $($ConfigFile)")
                    exit -1
                }
            }
            else {
                WriteError("[Error] reading config file - Please provide the name of your config file (i.e. appsettings.json)")
                exit -1
            }
        }
        catch {
            CustomWriteHostError("[Error] $_.Exception.Message")
            exit -1
        }
    }
    else {
        Write-Host "    Write out any Messages Here" -ForegroundColor Red
        Write-Host "    Please make sure you are using the correct syntax" -ForegroundColor Red
        Write-Host "#--------------------------------------------------------------------------------------------------------";
        exit 0
    }

    #At this point, we have the resource ID for both source and destination
    #Disconnect the Azure account connect so it is a clean login
    Disconnect-AzAccount | Out-null

    try {
         #Let's login
        $LoggedIn = CheckLogin
        if($ConfigFile)
        {
            #Login with Service Principal or Interactively
            if (-Not $LoggedIn) {
                Write-Host "PowerShell supports authentication using Service Principal (S) or UserName/Password (I)" -ForegroundColor Yellow
                $Global:SignIn = Read-Host -prompt "Choose the Authentication Method: (S)Service Principal (I)Interactively. (S/I)?"
                $(if ($Global:SignIn -eq 'S') { $AuthenticationType = 'ServicePrincipal' } else { $AuthenticationType = 'Interactive' })
                Login $config $Global:SignIn
            }
        }
    }
    catch {
        CustomWriteHostError("[Error] $_.Exception.Message")
        CustomWriteHostError("[Error] Connecting to Azure using Authentication Type: $AuthenticationType")
        exit -1
    }

    $CheckResources = CheckResources
    if (-Not $CheckResources) {
        WriteError ("Please double check your appsettings.json entries and your RBAC roles within the portal.")
        exit 1
    }

    #Begin Migration
    Write-Host ""
    Write-Host "#--------------------------------------------------------------------------------------------------------";
    Write-Host ""
    CustomWriteHost("[Info] Start Migration")
    CallAnotherFunction $config.DataFactory.ResourceId $config.SynapseWorkspace.ResourceId
    CustomWriteHost("[Info] Migration Completed")
    Write-Host ""
}
#endregion StartYourMainFunction

#---------------------------------------------------------[CallAnotherFunction]-----------------------------------------------------
#region
function CallAnotherFunction {
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
                return;
            }

            if ($response.StatusCode -eq 200)
            {
                WriteSuccess "Successfully migrated $resourceName"
                return;
            }
        }
        catch [Exception] {
            Write-Error "An occur has occured. Error Message: $($_.Exception.Message)"
            Write-Error "Error Details: $($_.ErrorDetails.Message)"
            throw
        }
}
#endregion

#---------------------------------------------------------[Entry Point - Execution of Script Starts Here]-----------------------------------------------------
#region Entry Point
    Write-Host "#--------------------------------------------------------------------------------------------------------";
    Write-Host "  PowerShell Seed Template Starts Here" -ForegroundColor Blue
    Write-Host ""
    Write-Host "#--------------------------------------------------------------------------------------------------------";

    #1. Check for PowerShell Module Prerequisites
    $PrerequisitesInstalled = CheckPrerequisites
    if (-Not $PrerequisitesInstalled) {
        WriteError("Pre-requisites are not installed. Please install the pre-requisites before running the PowerShell script.")
        exit 1
    }

    #2. Entry Function to migration Script
    StartYourMainFunction

    #3 End Script and Logout
    Write-Host "#--------------------------------------------------------------------------------------------------------"; g d
    Write-Host "  Disconnecting from Azure" -ForegroundColor Blue
    Disconnect-AzAccount | Out-null
    Write-Host "#--------------------------------------------------------------------------------------------------------";

    Set-PSDebug -Off
#endregion Entry Point
param (
    [string]
    $srcDataFactoryResourceId,

    [string]
    $destWorkspaceResourceId)

# Sample usage
#\users\ajarora\desktop\importFactoryTool.ps1 -srcDataFactoryResourceId "/subscriptions/YOURSUBSCRIPTIONID/resourcegroups/AjayRg/providers/Microsoft.DataFactory/factories/ajayfreshdemo" -destWorkspaceResourceId "/subscriptions/YOURSUBSCRIPTIONID/resourcegroups/AjayRg/providers/Microsoft.DataFactory/factories/ajayfreshdemo"


function PollUntilCompletion {
    Param (
        [string]$uri,
        [string] $originalUri,
        [bool] $isArmToken
        )

        Write-Output "Waiting for operation to..."
        
        try
        {
            $destHeaders = GetHeaders -armToken $isArmToken
            $response = Invoke-WebRequest -Method Get -Uri $uri -Headers $destHeaders
            if ($response.StatusCode -ge 203)
            {
                Write-Error "Error creating resource $originalUri"
                throw
            }

            if ($response.StatusCode -ne 200)
            {
                Start-Sleep -Seconds 1
                PollUntilCompletion $uri $originalUri $isArmToken
                return;
            }

            if ((ConvertFrom-Json -InputObject $response.Content).status -eq 'Failed') {
                Write-Error "Error creating resource $originalUri. Details: $response.Content"
                throw
            }
        }
        catch [Exception] { 
            Write-Error "Error occured - for $($_.name). Error: $($_.Exception.Message)"
            throw
        }
}

function ProcessResource {
    Param (
        [string]$srcResourceId,
        [string]$destResourceId,
        [string]$resourceType
        )

        Write-Output ""
        Write-Output ""
        Write-Output "Processing $resourceType"
        
        if ($srcResourceId.Contains("Microsoft.Synapse")) {
            $srcApiVersion = "2019-06-01-preview"
        } else {
            $srcApiVersion = "2018-06-01"
        }

        if ($destResourceId.Contains("Microsoft.Synapse")) {
            $destApiVersion = "2019-06-01-preview"
        } else {
            $destApiVersion = "2018-06-01"
        }

        $srcResource = Get-AzResource -ResourceId $srcResourceId -ApiVersion $srcApiVersion
        $destResource = Get-AzResource -ResourceId $destResourceId -ApiVersion $destApiVersion

        $srcUri = "https://management.azure.com/" + $srcResourceId
        
        if ($resourceType -eq "integrationRuntimes" -or $resourceType -eq "sqlPools" -or $resourceType -eq "sparkPools") {
            $isDestArm = $true;
            $destUri = "https://management.azure.com/" + $destResourceId
        } else {
            $isDestArm = $false;
            $destUri = $destResource.Properties.connectivityEndpoints.dev
        }

        $resourcesToBeCopied =  New-Object Collections.Generic.List[Object]
        $uri = "$srcUri/$($resourceType)?api-version=$($srcApiVersion)"

        try
        {
            while ($true)
            {
                $srcHeaders = GetHeaders -armToken $true
                $srcResponse = Invoke-RestMethod -Method Get -Uri $uri -Headers $srcHeaders
                $resourcesToBeCopied.AddRange($srcResponse.Value);
                if ($srcResponse.nextLink -eq $null)
                {
                    break;
                }
                $uri = $srcResponse.nextLink
            }
        }
        catch
        {
            Write-Error "Error listing $resourceType : $_"
            throw
        }

        Write-Output "Found $($resourcesToBeCopied.Count) $resourceType"

        if ($resourcesToBeCopied.Count -le 0) {
            return;
        }

        $resourcesToBeCopied | ForEach-Object -Process {
            $uri = "$destUri/$resourceType/$($_.name)?api-version=$($destApiVersion)";
            $jsonBody = ConvertTo-Json $_ -Depth 30
            
            Write-Output ""
            Write-Output "Cloning $resourceType $($_.name)"
            
            $destResponse = $null;

            try {
                $destHeaders = GetHeaders -armToken $isDestArm
                $destResponse = Invoke-WebRequest -Method Put -Uri $uri -Headers $destHeaders -ContentType "application/json" -Body $jsonBody
            }
            catch [Exception] { 
                Write-Error "Error occured for $($_.name). Error: $($_.Exception.Message)"
                throw
            }

            if ($destResponse.StatusCode -eq 202)
            {
                PollUntilCompletion $destResponse.Headers.Location $uri $isDestArm
            }
            elseif ($destResponse -eq $null -or $destResponse.StatusCode -ne 200) {
                Write-Error "Creation failed for $($_.name). Error: $($_.Exception.Message)"
                throw
            }
        }
}

function Cloneworkspace {
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
}


function GetHeaders {
    Param (
        [bool]$armToken
        )
        
        $token = AcquireToken -armToken $armToken

        return @{
            'Authorization' = "Bearer $token"
        }
}


function AcquireToken {
    Param (
        [bool]$armToken
        )
        
        if ($armToken -eq $true) {
            return (Get-MsalToken -ClientId "SERVICEPRINCIPALID" -Scope 'https://management.azure.com/.default' -RedirectUri "http://localhost" -Authority "https://login.microsoftonline.com/common/TENANTID").AccessToken
        }

        return (Get-MsalToken -ClientId "SERVICEPRINCIPALID" -Scope 'https://dev.azuresynapse.net/.default' -RedirectUri "http://localhost" -Authority "https://login.microsoftonline.com/common/TENANTID").AccessToken
}

function Login()
{
    $TenantID= "TENANTID"
    $ClientID = "SERVICEPRINCIPALID"
    $ClientSecret = "SERVICEPRINCIPALSECRET"

    try
    {
        #Login to Azure (programmatically)
        $pscredential = New-Object -TypeName System.Management.Automation.PSCredential($ClientID, (ConvertTo-SecureString $ClientSecret -AsPlainText -Force))
        Connect-AzAccount -Credential $pscredential -Tenant $TenantId -ServicePrincipal

        return $true
    }
    catch {
        WriteErrorResponse "You were not able to log in. Please check the Service Principal Client ID and Secret or log in via the Connect-AzAccount command"
        Write-Host $_
        throw
    }
}


if (Get-Module -ListAvailable -Name Az.Resources) {
    Write-Host "Az.Resources Module exists"
} 
else {
    Install-Module -Name Az.Resources -Scope CurrentUser -Force
}

if (Get-Module -ListAvailable -Name MSAL.PS) {
    Write-Host "MSAL.PS Module exists"
} 
else {
    Install-Module -Name MSAL.PS -Scope CurrentUser -Force
}

$srcDataFactoryResourceId = "/subscriptions/YOURSUBSCRIPTIONID/resourcegroups/AZU-Data-RG/providers/Microsoft.DataFactory/factories/airstream"
$destWorkspaceResourceId = "/subscriptions/YOURSUBSCRIPTIONID/resourcegroups/AZU-DW-RG/providers/Microsoft.Synapse/workspaces/air"


CloneWorkspace $srcDataFactoryResourceId $destWorkspaceResourceId

Write-Output "Cloning done"
<#
 .NOTES
  =========================================================================================================
  Created by:       Author: Your Name | @twitterhandle | your.name@domain.io | domainio.io
  Created on:       3/24/2021
  =========================================================================================================

 .DESCRIPTION

 .LINKS

 .EXAMPLE
  <Example goes here. Repeat this attribute for more than one example>

 .PARAMETERS

         .PARAMETER azUser
            The powershell user login
  
         .PARAMETER azPass
            The powershell user password

         .PARAMETER subscriptionName
            The subscription Name where the template will be deployed.

         .PARAMETER subscriptionId
            The subscription id where the template will be deployed.
 
         .PARAMETER tenantID
            The Azure Active Directory Tenant ID
   
         .PARAMETER rgName
            The resource group where the template will be deployed. Can be the name of an existing or a new resource group.

         .PARAMETER location
            If specified, will try to create the resource or resources in this location. If not specified, assumes resource group is existing.

         .PARAMETER templateFilePath
            Optional, path to the template file. Defaults to template.json.

         .PARAMETER parametersFilePath
            Optional, path to the parameters file. Defaults to parameters.json. If file is not found, will prompt for parameter values based on template.

 .SUBSCRIPTION(s): PRODUCTION and R&D
         PROD 
                *****************BE VERY CAREFUL WITH THIS AZURE SUBSCRIPTION - THIS IS PRODUCTION*************
             -Subscription Name      : YOURSUBSCRIPTIONNAME
             -Subscription ID        : YOURSUBSCRIPTIONID
             -Subscription TenantID  : YOURTENANTID
             -Login Service Principal: YOURSERVICEPRINCIPALNAME | SERVICEPRINCIPALID | SERVICEPRINCIPALPASSWORD
             -Login User: node.agent.ps@yourmail.onmicrosoft.com | password
    
    Install Azure PowerShell
    Azure PowerShell is designed for managing and administering Azure resources from the command line.
    https://docs.microsoft.com/en-us/powershell/azure/install-az-ps?view=azps-5.7.0

    Check PowerShell version
    $PSVersionTable.PSVersion

    Install the Azure Modules for PowerShell
    Type the following command, and then press Enter.
    This command installs the module for all users by default. (It's controlled by the scope parameter.) 
    AllowClobber overwrites the previous PowerShell module.

    Install-Module -Name Az -AllowClobber -Scope AllUsers
    Install-Module -Name Az -AllowClobber -Scope CurrentUser
    Install-Module -Name Az -AllowClobber

    Get-InstalledModule -Name Az -AllVersions | select Name,Version
    Get-Module -ListAvailable | ? Name -like "AZ*"

    powershell –ExecutionPolicy Bypass
#>
#---------------------------------------------------------[Parameters]-----------------------------------------------------
    $subscriptionName   = "YOURSUBSCRIPTIONNAME"
    $subscriptionId     = "YOURSUBSCRIPTIONID"
    $tenantID           = "YOURTENANTID"
    $azUser             = "YOURSERVICEPRINCIPALNAME"
    $appID              = "SERVICEPRINCIPALID"
    $azPass             = "SERVICEPRINCIPALPASSWORD"
    $location           = "East US"
    $rgName             = "AS-Demo-RG"
    $templateFilePath   = "C:\YOURFOLDER\Azure\PowerShell\template.json"
    $parametersFilePath = "C:\YOURFOLDER\Azure\PowerShell\parameters.json"
    

#---------------------------------------------------------[Login]-----------------------------------------------------
#region Login to Azure
    Write-Host "Logging in...";

    #Login to Azure (interactively)
    Connect-AzAccount
    
    #Login to Azure (programmatically)
    $pscredential = New-Object -TypeName System.Management.Automation.PSCredential($appID, (ConvertTo-SecureString $azPass -AsPlainText -Force))
    Connect-AzAccount -ServicePrincipal -Credential $pscredential -Tenant $tenantId

    Get-AzSubscription
    
    #Set Context subscription
    Write-Host "Selecting Subscription '$subscriptionId' and Context";

    #To add a new context after sign-in, use Set-AzContext (or its alias, Select-AzSubscription).
    #Sets the tenant, subscription, and environment for cmdlets to use in the current session.
    Set-AzContext -Subscription $subscriptionName -Name "AzurePass1"
    Select-AzContext AzurePass1 -Scope Process

    Select-AzSubscription -Subscription $subscriptionName

    #To remove a context, use the Remove-AzContext cmdlet.
    Remove-AzContext AzurePass1

    #Logout
    Disconnect-AzAccount
#endregion Login to Azure


#-----------------------------------------------------------[Execution]------------------------------------------------------------
#Demonstration - Cloud Shell
#In this demonstration, we will experiment with the Cloud Shell @ https://portal.azure.com
#---------------------------------------------------------[Demonstration - Cloud Shell]-----------------------------------------------------
#region Demonstration - Cloud Shell

    #Experiment with Azure PowerShell
        #View Subscription Information
        Get-AzSubscription

        #View Resource Group Information
       Get-AzResourceGroup

    #Experiment with the Bash Shell
        #Use the drop-down menu to switch to the Bash shell, and confirm your choice.
        #View Resource Information
        az resource list

#endregion Demonstration - Cloud Shell

#-------------------------------------------------------------------------------------
#region Resource Groups

    #List Resource Groups
    Get-AzResourceGroup | Format-Table
    Get-AzResourceGroup | Sort Location,ResourceGroupName | Format-Table -GroupBy Location ResourceGroupName,ProvisioningState,Tags

    #Create Resource Groups
    New-AzResourceGroup -Name RG01 -Location eastus
    New-AzResourceGroup -Name RG01 -Location $location 
    New-AzResourceGroup -Name RG01 -Location $location  -Tag @{Department="Marketing"}
    
    #Delete Resource Group
    Remove-AzResourceGroup -Name RG01
    Remove-AzResourceGroup -Name RG01 -Verbose -Force

#endregion

#Demonstration - Resource Groups
#In this demonstration, we will create and delete resource locks from within Azure Cloud Shell @ https://portal.azure.com
#---------------------------------------------------------[Demonstration - Resource Groups]-----------------------------------------------------
#region Demonstration - Resource Groups
    #View Subscription Information
    Get-AzSubscription

    #Select Subscription
    #Syntax
    Select-AzSubscription -Subscription "Azure Pass - AZ204"
    #Actual
    Select-AzSubscription -Subscription $subscriptionName 

    #Create Resource Group
    #Syntax
    #New-AzResourceGroup -Name <Name> -Location <Location>
    #Actual
    New-AzResourceGroup -Name AIRS-Lock-RG -Location EastUS    

    #Create the Resource Lock
    #Syntax
    #New-AzResourceLock -LockName <lockName> -LockLevel CanNotDelete -ResourceGroupName <resourceGroupName>
    #Actual
    New-AzResourceLock -LockName Name -LockLevel CanNotDelete -ResourceGroupName AIRS-Lock-RG

    #Verify the Resource Lock
    #Record the LockId Value
    Get-AzResourceLock

    #Remove the Resource Lock
    #The LockID Value is determined from the previous step.
    #Remove-AzResourceLock -LockId <ID>

    #Verify the Resource Lock
    Get-AzResourceLock

    #Remove Resource Group
    #Syntax
    #Remove-AzResourceGroup -Name <Name>
    #Actual
    Remove-AzResourceGroup -Name AIRS-Lock-RG

#endregion Demonstration - Resource Groups

#-------------------------------------------------------------------------------------
#region Virtual Machines CREATE VM WITH NEW-AZVM COMMAND

    #Connect-AzureRmAccount

    New-AzResourceGroup -Name Demo2 -Location EastUS

    New-AzVm `
        -ResourceGroupName "Demo2" `
        -Name "myVM" `
        -Location "East US" `
        -VirtualNetworkName "myVnet" `
        -SubnetName "mySubnet" `
        -SecurityGroupName "myNetworkSecurityGroup" `
        -PublicIpAddressName "myPublicIpAddress" `
        -OpenPorts 80,3389

    #$ip = Get-AzPublicIpAddress -ResourceGroupName "Demo2" | Select "IpAddress“

    #mstsc /v:($ip.IpAddress)

    #Install-WindowsFeature -name Web-Server –IncludeManagementTools

#endregion Execution


#-------------------------------------------------------------------------------------
#region Virtual Machines

#https://docs.microsoft.com/en-us/powershell/azure/azureps-vm-tutorial?view=azps-2.5.0

#1. Before you can create a new virtual machine, you must create a credential object containing the username and password for the administrator account of the Windows VM.
    #username: USERNAME
    #password: PASSWORD
    
    $cred = Get-Credential -Message "Enter a username and password for the virtual machine."

#2 Virtual machines in Azure have a large number of dependencies. The Azure PowerShell creates these resources for you based on the command-line arguments you specify. 
    $vmParams = @{
      ResourceGroupName = 'NYC-VM-RG2'
      Name = 'NodeVM2'
      Location = 'eastus'
      ImageName = 'Win2016Datacenter'
      PublicIpAddressName = 'NodeVM2-ip'
      Credential = $cred
      OpenPorts = 3389,80
    }
    $newVM1 = New-AzVM @vmParams

    #Once the VM is ready, we can view the results in the Azure Portal or by inspecting the $newVM1 variable.
    $newVM1
    #Verify the Name of the VM and the admin account we created.
    $newVM1.OSProfile | Select-Object ComputerName,AdminUserName
    #To confirm that the VM is running, we need to connect via Remote Desktop. For that, we need to know the Public IP address.
    $publicIp = Get-AzPublicIpAddress -Name NodeVM2-ip -ResourceGroupName NYC-VM-RG2
    $publicIp | Select-Object Name,IpAddress,@{label='FQDN';expression={$_.DnsSettings.Fqdn}}

    #Run the following command to connect to the VM over Remote Desktop
    mstsc.exe /v <PUBLIC_IP_ADDRESS>

#3 Install Web Server on VM
Install-WindowsFeature -name Web-Server -IncludeManagementTools

    #Example of Creating VM using Azure RM module
    New-AzResourceGroup -Name NYC-VM-RG3 -Location eastus
    New-AzureRmVm `
        -ResourceGroupName "NYC-VM-RG3" `
        -Name "NodeVM3" `
        -Location "East US" `
        -VirtualNetworkName "NYC-VNet3" `
        -SubnetName "default" `
        -SecurityGroupName "NodeVM3-nsg" `
        -PublicIpAddressName "NodeVM3-ip" `
        -OpenPorts 80,3389

    #Clean Up
    Remove-AzResourceGroup -Name NYC-VM-RG2 -Verbose -Force
    Remove-AzResourceGroup -Name NYC-VM-RG3 -Verbose -Force

#endregion Execution

#-------------------------------------------------------------------------------------
#region Azure Key Vault

    $rgKeyVault        = "AS-KeyVault-RG3"
    $rgVM              = "AS-VM-RG3"
    $region            = "eastus"
    $keyVault          = "ASKeyVault3"
    $VM                = "VM3"

    #Create Resource Group
    New-AzResourceGroup -Name $rgKeyVault -Location $region

    #Create Azure Key Vault by using Azure PowerShell
    New-AzKeyVault -VaultName $keyVault -ResourceGroupName $rgKeyVault `
        -Location $region -EnabledForDiskEncryption

    #Encrypt existing VM disks by using PowerShell
    Set-AzKeyVaultAccessPolicy -VaultName $keyVault -ResourceGroupName $rgKeyVault -EnabledForDiskEncryption
   
    $KV = Get-AzKeyVault -VaultName $keyVault -ResourceGroupName $rgKeyVault
    Set-AzVMDiskEncryptionExtension -ResourceGroupName $rgVM `
        -VMName $VM `
        -DiskEncryptionKeyVaultUrl $KV.VaultUri `
        -DiskEncryptionKeyVaultId  $KV.ResourceId `
        -VolumeType All `
        -SkipVmBackup
    
    Get-AzVmDiskEncryptionStatus -ResourceGroupName $rgVM -VMName $VM

    #Clean Up
    Remove-AzResourceGroup -Name $rgKeyVault
    Remove-AzResourceGroup -Name $rgVM

#endregion Execution

#---------------------------------------------------------[VMs Install IIS]-----------------------------------------------------
#region VMs Install IIS

    Install-WindowsFeature -name Web-Server -IncludeManagementTools

#endregion VMs Install IIS

#---------------------------------------------------------[Create Storage Account]-----------------------------------------------------
#region Create Storage Account in Azure

    #View Subscription Information
    Get-AzSubscription

    #Select Subscription
    Select-AzSubscription -Subscription $subscriptionName

    #Verify Resource Group
    Get-AzResourceGroup | Format-Table ResourceGroupName, Location 

    #Set Location
    Get-AzLocation | select Location
    $location = "eastus"
    $resourceGroup = "AS-Storage-RG"

    New-AzResourceGroup -Name $resourceGroup -Location $location
    New-AzStorageAccount -ResourceGroupName $resourceGroup -AccountName "asstordemo1" -Location $location -SkuName Standard_LRS -Kind StorageV2

    #Check to see if Storage Account was created
    Get-AzStorageAccount

    #Delete Resource Group that contains storage account
    Remove-AzResourceGroup -Name $resourceGroup 

#endregion Create Storage Account in Azure

#---------------------------------------------------------[Create virtual network using PowerShell]-----------------------------------------------------
#region Create virtual network using PowerShell

    #1 Create a virtual network. Use values as appropriate.
    $myVNet2 = New-AzVirtualNetwork -ResourceGroupName AS-VNets-RG -Location EastUS -Name ASVNet2 -AddressPrefix 10.0.0.0/16

    #2 Verify your new virtual network information.
    Get-AzVirtualNetwork -Name ASVNet2

    #3 Create a subnet. Use values as appropriate.
    $mySubnet2 = Add-AzVirtualNetworkSubnetConfig -Name mySubnet2 -AddressPrefix 10.0.0.0/24 -VirtualNetwork $myVNet2

    #4 Verify your new subnet information
    Get-AzVirtualNetworkSubnetConfig -Name mySubnet2 -VirtualNetwork $myVNet2

    #5 Associate the subnet to the virtual network.
    $mySubnet2 | Set-AzVirtualNetwork

#endregion Create Virtual Network Using PowerShell

#---------------------------------------------------------[Azure VPN Gateway ]-----------------------------------------------------
#region Verify Azure VPN Gateway
 
    # Verify the connection
    Get-AzVirtualNetworkGatewayConnection -Name MyGWConnection -ResourceGroupName MyRG

    # Review the status
    "connectionStatus": "Connected",
    "ingressBytesTransferred": 33509044,
    "egressBytesTransferred": 4142431

#endregion Verify Azure VPN Gateway

#---------------------------------------------------------[Create File Share]-----------------------------------------------------
#region Create File Share in Azure

    # Retrieve storage account and storage account key
    $storageContext = New-AzStorageContext asstordemo1 xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx==
    # Create the file share, in this case “logs”
    $share = New-AzStorageShare fileshare -Context $storageContext

    sudo mount -t cifs //<storage-account-name>.file.core.windows.net/<share-name> <mount-point> -o vers=<smb-version>,username=<storage-account-name>,password=<storage-account-key>,dir_mode=0777,file_mode=0777,serverino
#endregion Create Storage Account in Azure

#---------------------------------------------------------[Mapping File Share]-----------------------------------------------------
#region Mapping File Share

    Test-NetConnection -ComputerName yourstorageaccount.file.core.windows.net -Port 445
    # Save the password so the drive will persist on reboot
    Invoke-Expression -Command "cmdkey /add:yourstorageaccount.file.core.windows.net /user:Azure\yourstorageaccount /pass:xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx=="
    # Mount the drive
    New-PSDrive -Name K -PSProvider FileSystem -Root "\\yourstorageaccount.file.core.windows.net\fileshare" -Persist
    # Save the password so the drive will persist on reboot

    Remove-PSDrive K 
    
#endregion Mapping File Share

#---------------------------------------------------------[Create Management Group]-----------------------------------------------------
#region Create Management Group
    Get-AzSubscription

    #Select Subscription
    Select-AzSubscription -Subscription $subscriptionName

    New-AzManagementGroup -GroupName 'ASHR'
#endregion Create Management Group

#---------------------------------------------------------[Tags]-----------------------------------------------------
#region Create Tags
    Get-AzSubscription

    #Select Subscription
    Select-AzSubscription -Subscription $subscriptionName

    New-AzTag -Name "CostCenter" -Value "0001"

    Set-AzResourceGroup -Name "EngineerBlog" -Tag @{Name="CostCenter";Value="0001"}

    Get-AzTag -Name "CostCenter"
    
    Get-AzTag

    Get-AzResourceGroup -Tag @{Name="CostCenter"}
#endregion Create Tags

#---------------------------------------------------------[Create User]-----------------------------------------------------
#region Create User
    Get-AzSubscription

    #Select Subscription
    Select-AzSubscription -Subscription "Visual Studio Enterprise"

    #Create a password object
    $PasswordProfile = New-Object -TypeName Microsoft.Open.AzureAD.Model.PasswordProfile
    $PasswordProfile.Password = "YOURPASSWORD"

    # Create the new user
    New-AzureADUser -DisplayName "Abby Brown" -PasswordProfile $PasswordProfile -UserPrincipalName "AbbyB@yourdomain.onmicrosoft.com" -AccountEnabled $true -MailNickName "Newuser"
#endregion Create User

#---------------------------------------------------------[API Management]-----------------------------------------------------
#region API Management Service

    #Create resource group
    New-AzResourceGroup -Name drcResourceGroup -Location eastus

    #Create API Management Service
    ew-AzApiManagement -ResourceGroupName "apiResourceGroup" -Location "East US" -Name "airs-apim" -Organization "apiOrganization" -AdminEmail "demo.bot@YOURDOMAIN.io" -Sku "Developer"
#endregion AzCopy
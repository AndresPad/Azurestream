# Introduction 
This Repo contains a .NET 6.0 Visual 2022 Project called **Azurestream**. This Project was built for the purposes of giving Azure Engineers
a functioning Visual Studio 2022 and Visual Studio Code Project that they could leverage for demos, presentations or simply just improving
their C# and Azure skills. The solution contains a number of demos that could come in extremely useful during your deliveries.


![Screenshot of the staging deployment slot to test the deployments](Assets/images/azurestreamhomepage2.jpg)

# Getting Started
Getting your code up and running on their own system. 
1.  **Download/Clone** Azurestream Repository and place in C:/Git folder
<br/><br/>
2.	Open **C:\Git\azurestream\azurestream\azurestream.ARM\1.Environment Installs.txt** and make sure you have the following installed
    - Demo Possibilities:	
    - .NET Core 6.0
    - Azure CLI
    - Visual Studio 2022 - Community Edition is fine. 
    - Visual Studio Code and Extensions
    - Postman
<br/><br/>
3.	Open the Visual Studio Solution file in VS 2019 **C:\Git\azurestreamrepo\azurestream\azurestream.sln**
<br/><br/>
4.	Build the whole solution
<br/><br/>
5.  YOU WILL NEED THE FOLLOWING RESOURCES DEPLOYED IN AZURE:
    - AZURE KEY VAULT
    - AZURE REDIS CACHE INSTANCE
    - AZURE BLOB STORAGE ACCOUNT
    - AZURE IOT
    - AZURE SIGNALR
    - AZURE SQL DB 
    - AZURE COSMOSDB
    - Web APP and App Insights Instance
<br/><br/>
6.  Configure and update the following files in your Project:
    - To get the Azurestream website to work:
        - **C:\Git\azurestream\azurestream\azurestream\appsettings.Development.json** 
            - Configure Redis and SQL Connections
        - **C:\Git\azurestream\azurestream\azurestream\appsettings.json**
            - Configure Redis and SQL Connections
            - Configure CosmosDB Connection
            - Configure AzureAd
            - Configure Cloud Storage Account
            - Configure App Insights Instrumentation Key
        - **C:\Git\azurestream\azurestream\azurestream\azurekeyvault.json**
            - Configure Connection to Azure Key Vault
    - To get the Azurestream.AD.WebApp website to work:
        - **C:\Git\azurestream\azurestream\azurestream.AD.WebApp\appsettings.json**
            - Configure Azure AD Domain, Tenant and Service Principal Introduction
    - To get the Azurestream.console to work:
        - **C:\Git\azurestream\azurestream\azurestream.console\App.config**
            - Configure your connections and subscription and Tenant
    - To get the Azurestream.FuncAPP to work:
        - **C:\Git\azurestream\azurestream\azurestream.funcapp\local.settings.json**
            - Configure your Connections 
<br/><br/>
        
# Contents
1.	**Azurestream** (.NET Core 6.0 Web Project) 
    - Demo Possibilities:
        - Demo how to connect to Azure Key Vault
        - Demo how to connect to Azure Redis Cache, populate Key/Values in cache and use an invalidation policy.
        - Demo how to use Application Insights and properly configure the SDK on both the client and server sides.
        - Demo how to configure Azure AD Authentication and bring back an Authentication Token
        - Demo how to use NLog and send logs to Blob Storage Containers.
<br/><br/>
![Screenshot of the staging deployment slot to test the deployments](Assets/images/azurestreamhomepage2.jpg)
5.	**Azurestream.Console** (.Net Core 6.0 Console Application) 
    - Demo Possibilities:

        - Demo Azure Blobs with C#
        - Demo Azure Tables with C#
        - Demo Azure Queues with C#
        - Demo Azure Search with C#
        - Demo Event Hubs with C#
        - I will be continually growing the pool of samples as this evolves. 

        ![Screenshot of the staging deployment slot to test the deployments](Assets/images/azurestreamconsole.jpg)
<br/><br/>
6.	**Azurestream.FuncApp** (.Net Core 3.1 Azure Function Project) 
    - Demo Possibilities:
        - Demo Timer Trigger
        - Demo HTTP Trigger
        - Demo Blob Trigger
        - Demo Blob Trigger to CosmosDB
        - Demo Queue Trigger
        - Demo Durable Functions.
     ![Screenshot of the staging deployment slot to test the deployments](Assets/images/azurestreamfuncapp.jpg)
<br/><br/>
7.	**Azurestream.LogicApp** (ARM Logic App Project) 
    - Demo Possibilities:
        - Demo Twitter Sentiment Analysis 
        - Demo Logic App Workspace
        - Demo Logic App Actions
        - Demo Logic App ARM Template
        - Demo Logic App Development in Visual Studio
    ![Screenshot of the staging deployment slot to test the deployments](Assets/images/azurestreamlogicapp.jpg)
<br/><br/>
8.	**apa.BOL** (.NET Standard Business Object Layer) 
9.	**apa.DAL** (.NET Standard Data Access Layer) 

# Build and Test
TODO: Describe and show how to build your code and run the tests. 

# Contribute
TODO: Explain how other users and developers can contribute to make your code better. 

If you want to learn more about creating good readme files then refer the following [guidelines](https://docs.microsoft.com/en-us/azure/devops/repos/git/create-a-readme?view=azure-devops). You can also seek inspiration from the below readme files:
- [ASP.NET Core](https://github.com/aspnet/Home)
- [Visual Studio Code](https://github.com/Microsoft/vscode)
- [Chakra Core](https://github.com/Microsoft/ChakraCore)
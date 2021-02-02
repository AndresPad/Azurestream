SELECT COUNT(1)
FROM c

--Query using UDF
SELECT Families.id, Families.address.city
	FROM Families
	WHERE udf.REGEX_MATCH(Families.address.city, ".*eattle")

	--If an attendee ask why we are "telling" the benchmark application about our throughput, this is to avoid rate limiting. 
	--Our benchmark application is using a simple math formula to determine how much data it can send in parallel while avoiding rate limiting for the entire collection. 
	--If we didn't tell the benchmark application about our throughput limit, it could potentially send too much data at once and get a HTTP 429 error code.
	./cosmosbenchmark --location westus --type write --database TestDatabase --collection ThroughputDemo --throughput 2000
	./cosmosbenchmark --location westus --type write --database TestDatabase --collection ThroughputDemo --throughput 10000

	--This is the record we will use when we test read performance.
	SELECT TOP 1 * FROM data WHERE data.DeviceId = "00000000-0000-0000-0000-000000000000"

	--Point out to attendees that you are selecting a container that is running in the same region as your Azure Cosmos DB account
	./cosmosbenchmark --location westus --type read --database IoTDatabase --collection DeviceDataCollection
	./cosmosbenchmark --location southeastasia --type read --database IoTDatabase --collection DeviceDataCollection
	./cosmosbenchmark --location northeurope --type read --database IoTDatabase --collection DeviceDataCollection


	 SELECT * FROM c WHERE c.DeviceId = 'XMS-0001'

--Optimize JOIN expressions
	--Run This First 
	SELECT Count(1) AS Count
	FROM c
	JOIN t IN c.tags
	JOIN n IN c.nutrients
	JOIN s IN c.servings
	WHERE t.name = 'infant formula' AND (n.nutritionValue > 0 
	AND n.nutritionValue < 10) AND s.amount > 1

	--Run Second and compare RUs
	SELECT Count(1) AS Count
	FROM c
	JOIN (SELECT VALUE t FROM t IN c.tags WHERE t.name = 'infant formula')
	JOIN (SELECT VALUE n FROM n IN c.nutrients WHERE n.nutritionValue > 0 AND n.nutritionValue < 10)
	JOIN (SELECT VALUE s FROM s IN c.servings WHERE s.amount > 1)



//Identify which collections received 429s in last 24 hours, which occur when consumed RU/s exceeds provisioned RU/s.
//Occasional 429s are fine, as Cosmos client SDKs and data import tools (Azure Data Factory, bulk executor library) automatically retry on 429s. 
//High number of 429s may indidcate you have a hot partition or need to scale up throughput. Review https://aka.ms/cosmos-partition-key and https://aka.ms/cosmos-estimate-ru
AzureDiagnostics
| where TimeGenerated >= ago(24hr)
| where Category == "DataPlaneRequests"
| where statusCode_s == 429 
| summarize numberOfThrottles = count() by Resource, databaseName_s, collectionName_s, requestResourceType_s, bin(TimeGenerated, 1hr)
| order by numberOfThrottles 


//Identify top operations and consumed RUs per operation in last 24 hours
AzureDiagnostics
| where TimeGenerated >= ago(24hr)
| where Category == "DataPlaneRequests"
| summarize numberOfOperations = count(), totalConsumedRU = sum(todouble(requestCharge_s)) by Resource, databaseName_s, collectionName_s, OperationName, requestResourceType_s, requestResourceId_s
| extend averageRUPerOperation = totalConsumedRU / numberOfOperations 
| order by numberOfOperations 


//Identify top queries that consumed the most RUs in past 24 hours
//We join DataPlaneRequests, which gives us RU charge with QueryRuntimeStatistics, which gives the obfuscated query text
let queryRUChargeData = AzureDiagnostics
| where Category == "DataPlaneRequests" 
| where OperationName == "Query" 
| summarize by requestCharge_s, activityId_g, databaseName_s, collectionName_s, requestResourceType_s, requestResourceId_s, OperationName, TimeGenerated, callerId_s, clientIpAddress_s, userAgent_s;
AzureDiagnostics
| where TimeGenerated >= ago(24hr)
| where Category == "QueryRuntimeStatistics"
| join queryRUChargeData on $left.activityId_g == $right.activityId_g
| summarize numberOfTimesRun = count(), totalConsumedRU = sum(todouble(requestCharge_s1)) by databasename_s, collectionname_s, OperationName1, requestResourceType_s1, requestResourceId_s1, querytext_s, callerId_s1, clientIpAddress_s1, userAgent_s1, bin(TimeGenerated1, 1min) //bin by 1 minute
| extend averageRUPerExecution = totalConsumedRU / numberOfTimesRun
| order by averageRUPerExecution desc 


//Identify top logical partition keys by storage. 
//PartitionKeyStatistics will emit data for top logical partition keys by storage
//As a best practice, choose a partition key for your collections that evenly distributes throughput (RU/s) and storage. 
// Azure Cosmos DB supports 10GB of data for a single logical partition key. Review https://aka.ms/cosmos-partition-key for guidance.
AzureDiagnostics
| where Category == "PartitionKeyStatistics"
//| where Resource == "CosmosAccountName" and collectionName_s == "CollectionToAnalyze" //Replace to target query to specific account and collection
| summarize arg_max(TimeGenerated, *) by Resource, databaseName_s, collectionName_s, partitionKey_s //Get the latest storage size
| extend utilizationOf10GBLogicalPartition = sizeKb_d / 10000000 //10GB
| project TimeGenerated, Resource , databaseName_s , collectionName_s , partitionKey_s, sizeKb_d, utilizationOf10GBLogicalPartition 
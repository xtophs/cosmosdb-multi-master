# Multi Master Cosmos DB

.NET Core app implementing a containerized version of the [multi-master setup](https://docs.microsoft.com/en-us/azure/cosmos-db/multi-region-writers).

2 Master Setup. Primary and Secondary both store a "partition" of the data. Since CosmosDb only allows for one writeable region at this point, each database manages writes to its "partition". Data from both partitions is "joined" in the application layer at query time.

The following diagram illustrates setup of federated services and CosmosDb "partitions".
![](images/multi-master-cosmos.png)

## Setup
Set the following environment variables:
- `PRIMARY_KEY`: Key to the first database
- `PRIMARY_ENDPOINT`: Cosmos DB Endpoint for the first database
- `PRIMARY_LOCATIONS`:&nbsp;:-separated list of Azure regions for 1st partition database reads.
- `SECONDARY_KEY`: Key to the second database
- `SECONDARY_ENDPOINT`: Cosmos DB Endpoint for the second database
- `SECONDARY_LOCATIONS`:&nbsp;:-separated list of Azure regions for 2nd partition database reads.

## Example
If you have 2 CosmosDb databases.

1. Cosmos-West. Primary (write) Region: WestUS, Secondary (read-only) Region EastUS
2. Cosmos_East. Primary (write) Region: EastUS, Secondary (read-only) Region WestUS

You launch 2 containers:

1.  One container that writes to the "partition" homed in the `cosmos-west` database and aggregates reads from both databases

```
docker run -e PRIMARY_ENDPOINT=https://cosmos-west.documents.azure.com:443/ -e PRIMARY_KEY=[key] -e PRIMARY_LOCATIONS=westus:eastus -e SECONDARY_ENDPOINT=https://cosmos-east.documents.azure.com:443/  -e SECONDARY_KEY=[key] -e SECONDARY_LOCATIONS=eastus:westus -d cosmos-multi-master
```

2. Another container that writes to the "partition" homed in the `cosmos-east` database and aggregates reads from both databases:

```
docker run -e PRIMARY_ENDPOINT=https://cosmos-east.documents.azure.com:443/ -e PRIMARY_KEY=[key] -e PRIMARY_LOCATIONS=eastus:westus -e SECONDARY_ENDPOINT=https://cosmos-west.documents.azure.com:443/  -e SECONDARY_KEY=[key] -e SECONDARY_LOCATIONS=westus:eastus -d cosmos-multi-master
```


## Usage
Add an item to the data store: `curl -X POST -H "Content-Type: application/json" -d '{"text":"bladeebla" }' http://localhost:5000/api/items`

Query the data store: `curl http://localhost:5000/api/newitems`

## Building the container
Follow these steps:

```
dotnet restore
dotnet build
```

and then, because we have to have a folder with all dependencies

```
dotnet publish
```

Then you can build am Ubuntu-based container:

```
docker build -f Dockerfile -t cosmosdb-multi-master:latest .
```

or a nanoserver-based Windows container:
```
docker build -f Dockerfile.win -t cosmosdb-multi-master:latest .
```
# Multi Master Cosmos DB

.NET Core app implementing a containerized version of the [multi-master setup](https://docs.microsoft.com/en-us/azure/cosmos-db/multi-region-writers).

2 Master Setup. Both Primary and Secondary are writeable and geo-replicated.

## Setup
Set the following environment variables:
- `PRIMARY_KEY`: Key to the first database
- `PRIMARY_ENDPOINT`: Cosmos DB Endpoint for the first database
- `WRITE_REGIONS`: :-separated list of Azure regions of the first database
- `SECONDARY_KEY`: Key to the second database
- `SECONDARY_ENDPOINT`: Cosmos DB Endpoint for the second database
- `SECONDARY_REGIONS`: :-separated list of Azure regions of the second regions

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

Then you can build the container

```
docker build -f Dockerfile -t cosmosdb-multi-master:latest .
```

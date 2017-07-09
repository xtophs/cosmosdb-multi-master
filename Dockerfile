
FROM microsoft/aspnetcore:1.1.2
LABEL Name=cosmosdata Version=0.0.1 
ARG source=./bin/Debug/netcoreapp1.1/publish
WORKDIR /app
EXPOSE 5000
ENV ASPNETCORE_URLS=http://+:5000
COPY $source .
ENTRYPOINT dotnet cosmosdata.dll

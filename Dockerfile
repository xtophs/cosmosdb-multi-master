
FROM microsoft/aspnetcore:1.1.2
LABEL Name=cosmosdata Version=0.0.1 
ARG source=./bin/Debug/netcoreapp1.1/publish
WORKDIR /app
EXPOSE 5000
ENV ASPNETCORE_URLS=http://+:5000
#ENV KEY=uV6WXRLcLGM3cdNZIGc6JA4haHq9Ba2VnrXiGv1O7SRe7wPjatOGhTSZyeUtHgYO320n7ETKDYqLq3hc2w3EKQ==
#ENV ENDPOINT=https://xtoph.documents.azure.com:443/
COPY $source .
ENTRYPOINT dotnet cosmosdata.dll

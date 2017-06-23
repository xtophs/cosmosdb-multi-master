using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

using System.Net;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Newtonsoft.Json;

namespace cosmosdata.Controllers
{

    public class Item 
    {
        public string id { get; set; }
        public string text { get; set; }
        public string location { get; set; }

        public string fromEndpoint { get; set; }

        public string localEnv { get; set; }

    }
    [Route("api/[controller]")]
    public class ItemsController : Controller
    {
        const string databaseName = "ToDoList";
        const string collectionName = "Items";
        public ItemsController()
        {
            if (!string.IsNullOrEmpty(Environment.GetEnvironmentVariable("ENDPOINT")))
            {
                EndpointUri = Environment.GetEnvironmentVariable("ENDPOINT");
            }
            else
            {
                throw new Exception("Could not initialize. Missing environment variable ENDPOINT ");
            }

            if (!string.IsNullOrEmpty(Environment.GetEnvironmentVariable("KEY")))
            {
                PrimaryKey = Environment.GetEnvironmentVariable("KEY");
            }
            else
            {
                throw new Exception("Could not initialize. Missing environment variable KEY ");
            }
            this.writeClient = GetWriteClient();
            this.readClient = GetReadClient();
        }

        private DocumentClient GetWriteClient()
        {
            return GetDocumentClient( EnvReader.GetWriteRegions() );
        }

        private DocumentClient GetReadClient()
        {
            return GetDocumentClient( EnvReader.GetReadRegions() );
        }

        private DocumentClient GetDocumentClient( IEnumerable<string> locations  )
        {
            ConnectionPolicy policy = new ConnectionPolicy { ConnectionMode = ConnectionMode.Direct, ConnectionProtocol = Protocol.Tcp };
            foreach (var l in locations )
            {
                Console.WriteLine(string.Format("Adding location: {0}", l));
                policy.PreferredLocations.Add(l);
            }

            return new DocumentClient(new Uri(EndpointUri),
                PrimaryKey,
                policy);
        }
        //private const string EndpointUri = "https://172.27.229.165:8081/";
        //private const string PrimaryKey = "C2y6yDjf5/R+ob0N8A7Cgv30VRDJIWEHLM+4QDU5DE2nQ9nDuVTqobD4b8mGGyPMbIZnqyMsEcaGQy67XIw/Jw==";

        //private const string EndpointUri = "https://xtoph.documents.azure.com:443/";
        //private const string PrimaryKey = "uV6WXRLcLGM3cdNZIGc6JA4haHq9Ba2VnrXiGv1O7SRe7wPjatOGhTSZyeUtHgYO320n7ETKDYqLq3hc2w3EKQ==";

        private string EndpointUri;
        private string PrimaryKey;

        private DocumentClient readClient;
        private DocumentClient writeClient;


        // GET api/values
        [HttpGet]
        public IEnumerable<Item> Get()
        {
            FeedOptions queryOptions = new FeedOptions { MaxItemCount = -1 };

            // TODO: create client locally to avoid stale connections
            IQueryable<Item> itemQuery = this.readClient.CreateDocumentQuery<Item>(
                UriFactory.CreateDocumentCollectionUri(databaseName, collectionName), queryOptions);

            return itemQuery.AsEnumerable<Item>();
        }

        // POST api/values
        [HttpPost]
        public async Task<string> Post([FromBody]Item value)
        {
            var response = "";

            value.id = DateTime.Now.Ticks.ToString();
            value.location = System.Net.Dns.GetHostName();
            value.fromEndpoint = writeClient.WriteEndpoint.ToString();
            if( ! string.IsNullOrEmpty(Environment.GetEnvironmentVariable("MYVVAR")) ) 
            {
                value.localEnv = Environment.GetEnvironmentVariable("MYVAR");
            }
            else{
                value.localEnv = "unset";
            }

            try{
                // TODO: create client locally to avoid stale connections

                var uri = UriFactory.CreateDocumentCollectionUri(databaseName, collectionName);
                var doc = await this.writeClient.CreateDocumentAsync(uri, value);
                doc.Resource.SetPropertyValue( "location", writeClient.WriteEndpoint.ToString() );
                await this.writeClient.UpsertDocumentAsync( uri, doc.Resource );
                response = "Added data in " + writeClient.WriteEndpoint + "\n";
            }
            catch (Exception ex )
            {
                response = ex.ToString();
            }
            
            return response;
        }

        // PUT api/values/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE api/values/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}

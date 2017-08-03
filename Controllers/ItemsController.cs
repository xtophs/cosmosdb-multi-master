using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using System.Runtime.InteropServices;

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
    }

class ItemComparer : EqualityComparer<Item>
{
    public override bool Equals(Item i1, Item i2)
    {
        var isEqual = false;

        if( i1.id == i2.id )
        {
            isEqual = true;
        }
        return isEqual;
    }


    public override int GetHashCode(Item s)
    {
        return base.GetHashCode();
    }
}
    [Route("api/[controller]")]
    public class ItemsController : Controller
    {
        const string databaseName = "mydb";
        const string collectionName = "items";
        public ItemsController()
        {
            PrimaryEndpointUri = EnvReader.GetSafeEnvVar("PRIMARY_ENDPOINT");
            SecondaryEndpointUri = EnvReader.GetSafeEnvVar("SECONDARY_ENDPOINT");
            PrimaryKey = EnvReader.GetSafeEnvVar("PRIMARY_KEY");
            SecondaryKey = EnvReader.GetSafeEnvVar("SECONDARY_KEY");
        }

        private DocumentClient primaryClient { 
            get 
            {
                return GetDocumentClient(PrimaryEndpointUri, PrimaryKey, EnvReader.GetPrimaryRegions());
            }
        }

        private DocumentClient secondaryClient { 
            get 
            {
                return GetDocumentClient(SecondaryEndpointUri, SecondaryKey, EnvReader.GetSecondaryRegions());
            }
        }

        private DocumentClient GetDocumentClient( string endpoint, string key, IEnumerable<string> locations  )
        {
            // Addressing .NETCore issue on Linux
            // https://github.com/Azure/azure-documentdb-dotnet/issues/194 
            ConnectionPolicy policy = new ConnectionPolicy();
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) 
            {
                policy.ConnectionMode = ConnectionMode.Direct;
                policy.ConnectionProtocol = Protocol.Tcp;
            }            
            foreach (var l in locations )
            {
                Console.WriteLine(string.Format("Adding location: {0}", l));
                policy.PreferredLocations.Add(l);
            }

            return new DocumentClient(new Uri(endpoint),
                key,
                policy);
        }

        private string PrimaryEndpointUri;
        private string SecondaryEndpointUri;
        private string PrimaryKey;
        private string SecondaryKey;

        // GET api/values
        [HttpGet]
        public IEnumerable<Item> Get()
        {
            var results = new HashSet<Item>(new ItemComparer() );

            // re-init DocumentClient for each GET
            // just in case
            Console.WriteLine( "First Read");
            ExecuteQuery( primaryClient, results );
            Console.WriteLine("Second");
            ExecuteQuery( secondaryClient, results );

            return results.AsEnumerable<Item>();
        }

        private void ExecuteQuery( DocumentClient client, HashSet<Item> results )
        {
            FeedOptions queryOptions = new FeedOptions { MaxItemCount = -1 };
            var uri = UriFactory.CreateDocumentCollectionUri(databaseName, collectionName);

            Console.WriteLine( "Executing Query at: {0}", uri );
            IQueryable<Item> itemQuery = client.CreateDocumentQuery<Item>(
                uri, queryOptions);
            Console.WriteLine( string.Format("Query from endpoint: {0}", client.ReadEndpoint.ToString()));
            var counter = 0;
            foreach ( var i in itemQuery.AsEnumerable<Item>() )
            {
                var added = results.Add(i);
                if( ! added ) {
                    Console.WriteLine( string.Format("Element not added {0}", i.id));
                }
                counter++;
            } 
            Console.WriteLine( string.Format( "Query with {0} results, collection with {1}", counter, results.Count));
        }

        // POST api/values
        [HttpPost]
        public async Task<string> Post([FromBody]Item value)
        {
            // initialize the client for every POST, but keep it
            // consistent for the duration of the method execution
            var client = primaryClient;

            var response = "";

            value.id = DateTime.Now.Ticks.ToString();
            value.location = System.Net.Dns.GetHostName();
            value.fromEndpoint = client.WriteEndpoint.ToString();

            try{
                var uri = UriFactory.CreateDocumentCollectionUri(databaseName, collectionName);
                Console.WriteLine( string.Format( "DocDB Uri is: {0}", uri));

                var doc = await client.CreateDocumentAsync(uri, value);
                doc.Resource.SetPropertyValue( "location", client.WriteEndpoint.ToString() );
                await client.UpsertDocumentAsync( uri, doc.Resource );
                response = "Added data in " + client.WriteEndpoint + "\n";
            }
            catch (Exception ex )
            {
                response = ex.ToString();
            }
            
            return response;
        }

        // DELETE api/values/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
            // TODO
        }
    }
}

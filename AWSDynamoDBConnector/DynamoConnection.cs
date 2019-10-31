using Amazon;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.DocumentModel;
using Amazon.DynamoDBv2.Model;
using Amazon.Runtime;
using Amazon.Util;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace AWSDynamoDBConnector
{
    public class DynamoConnection
    {
        private readonly static string accessKey = "";
        private readonly static string secretKey = ""; // TODO: DO NOT COMMIT
        private readonly static string tableName = "DailySoupData";

        private readonly DynamoDBContext context;
        private readonly AmazonDynamoDBClient client;

        public DynamoConnection()
        {
            var credentials = new BasicAWSCredentials(accessKey, secretKey);
            client = new AmazonDynamoDBClient(credentials, RegionEndpoint.USEast1);

            context = new DynamoDBContext(client);
        }

        public bool AddSoups(List<string> Soups)
        {

                var soup = new Soup()
                {
                    ID = Guid.NewGuid().ToString(),
                    Soups = Soups,
                    UpdTimeStamp = DateTime.UtcNow
                };

                var task = context.SaveAsync<Soup>(soup);
                task.Wait();

                return true;
        }
       
        public Soup GetMostRecentSoup()
        {
            return GetSoupsFromPastDays(1).LastOrDefault();
        }
        
        public List<Soup> GetSoupsFromPastDays(int days)
        {
            Table ThreadTable = Table.LoadTable(client, tableName);

            ScanFilter scanFilter = new ScanFilter();
            scanFilter.AddCondition("UpdTimeStamp", ScanOperator.GreaterThan, DateTime.UtcNow.AddDays(-days).ToString(AWSSDKUtils.ISO8601DateFormat));


            ScanOperationConfig config = new ScanOperationConfig()
            {
                Filter = scanFilter,
            };

            Search search = ThreadTable.Scan(config);

            List<Document> documentList = new List<Document>();
            do
            {
                var nextSetTask = search.GetNextSetAsync();
                nextSetTask.Wait();

                var nextSet = nextSetTask.Result;
                documentList.AddRange(nextSet);

            } while (!search.IsDone);

            var soups = documentList.Select(arg => JsonConvert.DeserializeObject<Soup>(arg.ToJson())).OrderBy(arg => arg.UpdTimeStamp).ToList();

            return soups;
        }
    }
}

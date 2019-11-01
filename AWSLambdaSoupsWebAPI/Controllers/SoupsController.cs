using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using AWSLambdaSoupsWebAPI.Models;
using CopenhagenSoupCrawler;
using AWSLambdaSoupsWebAPI.Utilities;
using System.Net.Http;
using Newtonsoft.Json;
using System.Text;

namespace AWSLambdaSoupsWebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SoupsController : ControllerBase
    {

        [HttpGet]
        public string Get()
        {
            try
            {
                AWSDynamoDBConnector.DynamoConnection dynamoConnection = new AWSDynamoDBConnector.DynamoConnection(
                    accessKey: Startup.Configuration.GetSection("DynamoDBConnector:accessKey").Value,
                    secretKey: Startup.Configuration.GetSection("DynamoDBConnector:secretKey").Value,
                    tableName: Startup.Configuration.GetSection("DynamoDBConnector:tableName").Value);



                var soupsOrderedByDate = dynamoConnection.GetSoupsFromPastDays(3);
                if(soupsOrderedByDate == null || soupsOrderedByDate.Count == 0)
                {
                    return "unable to find any soups in the past 72h";
                }

                var LatestSoupUpdate = soupsOrderedByDate.LastOrDefault();

                var soupLastUpdated = soupsOrderedByDate.FirstOrDefault(arg => arg.GetSoupsHash() == LatestSoupUpdate.GetSoupsHash());

                // Move Special soup to end of list
                List<String> Soups = LatestSoupUpdate.Soups;
                var specialLoc = Soups.FirstOrDefault(arg => arg.Contains(SoupCrawler.SpecialSoupTitle));
                if (specialLoc != null && Soups.Remove(specialLoc))
                {
                    Soups.Insert(Soups.Count, specialLoc);
                }

                TimeSpan lastPollTimespan = DateTime.UtcNow - LatestSoupUpdate.UpdTimeStamp;
                TimeSpan soupChange = DateTime.UtcNow - soupLastUpdated.UpdTimeStamp;

                return string.Format("Last polled {0}\nLast soup change was {1}\n\n{2}",
                    lastPollTimespan.FormatSlackTime(),
                    soupChange.FormatSlackTime(),
                    String.Join("\n", Soups));
            } catch (Exception e)
            {
                return "Exception while retreiving DB update: " + e;
            }
        }

        // POST api/soups
        [HttpPost]
        public void Post([FromForm] SlackRequest slackRequest)
        {
            HttpClient client = new HttpClient();

            var obj = new { text = Get() };

            string json = JsonConvert.SerializeObject(obj);

            var response = client.PostAsync(
                slackRequest.response_url,
                    new StringContent(json, Encoding.UTF8, "application/json")).Result;

            var responseString = response.Content.ReadAsStringAsync();
            responseString.Wait();
        }
    }
}

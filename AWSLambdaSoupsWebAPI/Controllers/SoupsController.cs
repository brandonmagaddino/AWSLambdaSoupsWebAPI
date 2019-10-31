using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using AWSLambdaSoupsWebAPI.Models;
using CopenhagenSoupCrawler;

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
                AWSDynamoDBConnector.DynamoConnection dynamoConnection = new AWSDynamoDBConnector.DynamoConnection();

                var LatestSoupUpdate = dynamoConnection.GetMostRecentSoup();
            

                // Move Special soup to end of list
                List<String> Soups = LatestSoupUpdate.Soups;
                var specialLoc = Soups.FirstOrDefault(arg => arg.Contains(SoupCrawler.SpecialSoupTitle));
                if (specialLoc != null && Soups.Remove(specialLoc))
                {
                    Soups.Insert(Soups.Count, specialLoc);
                }
                
                return "Last updated " + LatestSoupUpdate.UpdTimeStamp.ToLocalTime().ToString("dddd, MMMM dd h:mm tt") + "\n" + String.Join("\n", Soups);
            } catch (Exception e)
            {
                return "Exception while retreiving DB update: " + e;
            }
        }

        // POST api/soups
        [HttpPost]
        public string Post([FromForm] SlackRequest value)
        {
            return Get();
        }
    }
}

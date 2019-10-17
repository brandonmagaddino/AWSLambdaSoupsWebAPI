using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using AWSLambdaSoupsWebAPI.Crawler;
using AWSLambdaSoupsWebAPI.Models;

namespace AWSLambdaSoupsWebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SoupsController : ControllerBase
    {

        [HttpGet]
        public string Get()
        {
            SoupCrawler soupCrawler = new SoupCrawler();

            return String.Join("\n", soupCrawler.GetTodaysSoups().Select(arg => arg.ToString()));
        }

        // POST api/soups
        [HttpPost]
        public string Post([FromForm] SlackRequest value)
        {
            SoupCrawler soupCrawler = new SoupCrawler();
            
            return String.Join("\n", soupCrawler.GetTodaysSoups().Select(arg => arg.ToString()));
        }
    }
}

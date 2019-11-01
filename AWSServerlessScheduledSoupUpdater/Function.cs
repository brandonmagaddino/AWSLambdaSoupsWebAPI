using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

using Amazon.Lambda.Core;
using Amazon.Lambda.APIGatewayEvents;
using CopenhagenSoupCrawler;
using AWSServerlessScheduledSoupUpdater.Utilities;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.Json.JsonSerializer))]

namespace AWSServerlessScheduledSoupUpdater
{
    public class Functions
    {
        /// <summary>
        /// Default constructor that Lambda will invoke.
        /// </summary>
        public Functions()
        {
        }


        /// <summary>
        /// A Lambda function to respond to HTTP Get methods from API Gateway
        /// </summary>
        /// <param name="request"></param>
        /// <returns>The list of blogs</returns>
        public APIGatewayProxyResponse Get(APIGatewayProxyRequest request, ILambdaContext context)
        {
            context.Logger.LogLine("Get Request at " + DateTime.UtcNow.ToString() + "\n");
            int statusCode = (int)HttpStatusCode.OK;

            try
            {
                AWSDynamoDBConnector.DynamoConnection dynamoConnection = new AWSDynamoDBConnector.DynamoConnection(
                    accessKey: ConfigurationManager.AppSettings.DynamoDBConnector.accessKey.Value,
                    secretKey: ConfigurationManager.AppSettings.DynamoDBConnector.secretKey.Value,
                    tableName: ConfigurationManager.AppSettings.DynamoDBConnector.tableName.Value);

                SoupCrawler crawler = new SoupCrawler();
                var soups = crawler.GetTodaysSoups();

                dynamoConnection.AddSoups(soups);

                context.Logger.LogLine("Successfully received " + soups.Count + " soups \n");
            }
            catch (Exception e)
            {
                context.Logger.LogLine("Failted to parse/update data with Exception: " + e + "\n");

                statusCode = (int)HttpStatusCode.InternalServerError;
            }

            var response = new APIGatewayProxyResponse
            {
                StatusCode = statusCode,
                Body = "",
                Headers = new Dictionary<string, string> { { "Content-Type", "text/plain" } }
            };

            return response;
        }
    }
}

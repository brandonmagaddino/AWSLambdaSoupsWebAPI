﻿using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.Model;
using System;
using System.Collections.Generic;
using System.Text;

namespace AWSDynamoDBConnector
{
    [DynamoDBTable("DailySoupData")]
    public class Soup
    {
        public int GetSoupsHash()
        {
            var soupClone = new List<string>(Soups);
            soupClone.Sort();

            return string.Join("|", soupClone).GetHashCode();
        }

        [DynamoDBHashKey] //Partition key
        public string ID
        {
            get; set;
        }
        [DynamoDBProperty]
        public DateTime UpdTimeStamp
        {
            get; set;
        }
        [DynamoDBProperty]
        public List<String> Soups
        {
            get; set;
        }
    }
}
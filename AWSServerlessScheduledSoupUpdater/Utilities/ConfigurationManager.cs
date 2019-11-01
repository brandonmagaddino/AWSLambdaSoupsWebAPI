using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Text;

namespace AWSServerlessScheduledSoupUpdater.Utilities { 
    public static class ConfigurationManager
    {
        public static readonly dynamic AppSettings;

        static ConfigurationManager()
        {
            AppSettings =  JsonConvert.DeserializeObject(File.ReadAllText("appsettings.json"));
        }
    }
}

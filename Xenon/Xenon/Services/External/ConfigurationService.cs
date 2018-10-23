﻿#region

using System.IO;
using System.Text;
using Newtonsoft.Json;
using Xenon.Core;

#endregion

namespace Xenon.Services.External
{
    public class ConfigurationService
    {
        public static Configuration LoadNewConfig()
        {
            const string fileName = "config.json";
            if (!File.Exists(fileName))
            {
                File.CreateText(fileName).Close();
                File.WriteAllText(fileName, JsonConvert.SerializeObject(new ConfigurationService()));
            }

            var json = File.ReadAllText(fileName, Encoding.UTF8);
            return JsonConvert.DeserializeObject<Configuration>(json);
        }
    }
}
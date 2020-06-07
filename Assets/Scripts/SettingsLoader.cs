using Assets.Contracts.Settings;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace Assets.Scripts
{
    public class SettingsLoader
    {
        private static SettingsLoader _instance;

        public static SettingsLoader Instance => _instance ?? (_instance = new SettingsLoader());
        
        private Config _settings;

        private SettingsLoader()
        {
            var path = Path.Combine(Application.dataPath, "Config", "map.json");
            var jsonContent = File.ReadAllText(path);
            _settings = JsonConvert.DeserializeObject<Config>(jsonContent, new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.Auto });
        }

        public IDictionary<Type, double> ResourceSettings
            => _settings.Resources.ToDictionary(r => Type.GetType($"Assets.Scripts.Economy.Resources.{r.Name}"), r => r.Value);

        public ICollection<string> MajorCountryNames => _settings.MajorCountryNames;

        public ICollection<string> MinorCountryNames => _settings.MinorCountryNames;
    }
}
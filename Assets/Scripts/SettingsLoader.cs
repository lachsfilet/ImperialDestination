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
        private Config _settings;

        public SettingsLoader(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
                throw new ArgumentException("Invalid settings file path", nameof(path));

            var jsonContent = File.ReadAllText(path);
            _settings = JsonConvert.DeserializeObject<Config>(jsonContent, new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.Auto });
        }

        public IDictionary<Type, double> GetResourceSettings()
        {
            var resources = _settings.Resources.ToDictionary(r => Type.GetType($"Assets.Scripts.Economy.Resources.{r.Name}"), r => r.Value);
            return resources;
        }
    }
}
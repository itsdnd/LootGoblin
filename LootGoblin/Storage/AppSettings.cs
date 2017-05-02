using Newtonsoft.Json;
using System;
using System.IO;

namespace LootGoblin.Storage
{
    public class AppSettings
    {
        public bool SuppressRandomMagicItemListPopups { get; set; }

        public bool SuppressEncounterListPopups { get; set; }

        public bool SuppressMagicItemEditPopups { get; set; }

        public bool SuppressMagicItemListPopups { get; set; }

        public bool SuppressContainerEditPopups { get; set; }

        public void Save()
        {
            string configPath = Path.Combine(Environment.CurrentDirectory, @"config\");
            Directory.CreateDirectory(configPath);
            var filePath = Path.Combine(configPath, "config.json");

            string output = JsonConvert.SerializeObject(this, Formatting.Indented);
            File.WriteAllText(filePath, output);
        }
    }
}

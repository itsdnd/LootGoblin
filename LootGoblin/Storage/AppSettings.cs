using Newtonsoft.Json;
using System;
using System.IO;

namespace LootGoblin.Storage
{
    public class AppSettings
    {
        public bool SuppressEncounterListPopups { get; set; }
        public bool SuppressMagicItemListPopups { get; set; }
        public bool SuppressRandomMagicItemListPopups { get; set; }
        public bool SuppressContainerEditPopups { get; set; }
        public bool SuppressMagicItemEditPopups { get; set; }
        public bool SuppressExternalLinksPopups { get; set; }
        public bool OverrideContainerListMouseWheelScrolling { get; set; }
        public bool OverrideMagicItemListMouseWheelScrolling { get; set; }
        public bool OverrideEncounterListMouseWheelScrolling { get; set; }
        public bool OverrideGuaranteedMagicItemsMouseWheelScrolling { get; set; }
        public bool OverrideRandomMagicItemsMouseWheelScrolling { get; set; }
        public bool OverrideContainerEditGridMouseWheelScrolling { get; set; }

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

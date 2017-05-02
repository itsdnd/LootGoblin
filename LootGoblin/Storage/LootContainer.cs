using System.Collections.Generic;

namespace LootGoblin.Storage
{
    public class LootContainer
    {
        public string Name { get; set; } = "New Loot Container";
        public string Type { get; set; } = "Other";
        public string Description { get; set; } = "Placeholder container description";
        public int CopperMin { get; set; }
        public int CopperMax { get; set; }
        public int SilverMin { get; set; }
        public int SilverMax { get; set; }
        public int ElectrumMin { get; set; }
        public int ElectrumMax { get; set; }
        public int GoldMin { get; set; }
        public int GoldMax { get; set; }
        public int PlatinumMin { get; set; }
        public int PlatinumMax { get; set; }
        public int ArmorMin { get; set; }
        public int ArmorMax { get; set; }
        public List<Item> Armor { get; set; } = new List<Item>();
        public int WeaponsMin { get; set; }
        public int WeaponsMax { get; set; }
        public List<Item> Weapons { get; set; } = new List<Item>();
        public int GearMin { get; set; }
        public int GearMax { get; set; }
        public List<Item> Gear { get; set; } = new List<Item>();
        public int MundaneMin { get; set; }
        public int MundaneMax { get; set; }
    }
}

using System.Collections.Generic;

namespace LootGoblin.Storage
{
    public class Loot
    {
        public string Name { get; set; }
        public int Copper { get; set; }
        public int Silver { get; set; }
        public int Electrum { get; set; }
        public int Gold { get; set; }
        public int Platinum { get; set; }
        public Dictionary<Item, int> Armor { get; set; }
        public Dictionary<Item, int> Weapons { get; set; }
        public Dictionary<Item, int> Gear { get; set; }
        public Dictionary<string, int> MundaneItems { get; set; }
    }
}

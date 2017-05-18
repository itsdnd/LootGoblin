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
        public Dictionary<string, int> MundaneItems { get; set; }
        public Dictionary<string, int> Trinkets { get; set; }
        public Dictionary<Item, int> ArmorSets { get; set; }
        public Dictionary<Item, int> ArmorPieces { get; set; }
        public Dictionary<Item, int> Weapons { get; set; }
        public Dictionary<Item, int> Ammo { get; set; }
        public Dictionary<Item, int> Clothing { get; set; }
        public Dictionary<Item, int> ClothingAccessories { get; set; }
        public Dictionary<Item, int> FoodDrinks { get; set; }
        public Dictionary<Item, int> TradeGoods { get; set; }
        public Dictionary<Item, int> PreciousItems { get; set; }
        public Dictionary<Item, int> ArtDecor { get; set; }
        public Dictionary<Item, int> BooksPapers { get; set; }
        public Dictionary<Item, int> OtherItems { get; set; }
    }
}

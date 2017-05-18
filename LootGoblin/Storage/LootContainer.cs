using System.Collections.Generic;

namespace LootGoblin.Storage
{
    public class LootContainer
    {
        public string Name { get; set; } = "New Loot Container";
        public string Type { get; set; } = "No Type";
        public string Description { get; set; } = "Placeholder container description";
        public int CopperMin { get; set; } = 0;
        public int CopperMax { get; set; } = 0;
        public int SilverMin { get; set; } = 0;
        public int SilverMax { get; set; } = 0;
        public int ElectrumMin { get; set; } = 0;
        public int ElectrumMax { get; set; } = 0;
        public int GoldMin { get; set; } = 0;
        public int GoldMax { get; set; } = 0;
        public int PlatinumMin { get; set; } = 0;
        public int PlatinumMax { get; set; } = 0;
        public int MundaneMin { get; set; } = 0;
        public int MundaneMax { get; set; } = 0;
        public int TrinketMin { get; set; } = 0;
        public int TrinketMax { get; set; } = 0;
        public int ArmorSetsMin { get; set; } = 0;
        public int ArmorSetsMax { get; set; } = 0;
        public List<Item> ArmorSets { get; set; } = new List<Item>();
        public int ArmorPiecesMin { get; set; } = 0;
        public int ArmorPiecesMax { get; set; } = 0;
        public List<Item> ArmorPieces { get; set; } = new List<Item>();
        public int WeaponsMin { get; set; } = 0;
        public int WeaponsMax { get; set; } = 0;
        public List<Item> Weapons { get; set; } = new List<Item>();
        public int AmmoMin { get; set; } = 0;
        public int AmmoMax { get; set; } = 0;
        public List<Item> Ammo { get; set; } = new List<Item>();
        public int ClothingMin { get; set; } = 0;
        public int ClothingMax { get; set; } = 0;
        public List<Item> Clothing { get; set; } = new List<Item>();
        public int ClothingAccessoriesMin { get; set; } = 0;
        public int ClothingAccessoriesMax { get; set; } = 0;
        public List<Item> ClothingAccessories { get; set; } = new List<Item>();
        public int FoodDrinksMin { get; set; } = 0;
        public int FoodDrinksMax { get; set; } = 0;
        public List<Item> FoodDrinks { get; set; } = new List<Item>();
        public int TradeGoodsMin { get; set; } = 0;
        public int TradeGoodsMax { get; set; } = 0;
        public List<Item> TradeGoods { get; set; } = new List<Item>();
        public int PreciousItemsMin { get; set; } = 0;
        public int PreciousItemsMax { get; set; } = 0;
        public List<Item> PreciousItems { get; set; } = new List<Item>();
        public int ArtDecorMin { get; set; } = 0;
        public int ArtDecorMax { get; set; } = 0;
        public List<Item> ArtDecor { get; set; } = new List<Item>();
        public int BooksPapersMin { get; set; } = 0;
        public int BooksPapersMax { get; set; } = 0;
        public List<Item> BooksPapers { get; set; } = new List<Item>();
        public int OtherItemsMin { get; set; } = 0;
        public int OtherItemsMax { get; set; } = 0;
        public List<Item> OtherItems { get; set; } = new List<Item>();
    }
}

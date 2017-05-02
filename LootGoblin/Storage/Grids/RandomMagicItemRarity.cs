namespace LootGoblin.Storage
{
    public class RandomMagicItemRarity
    {
        public RandomMagicItemRarity(string Rarity, int Min, int Max)
        {
            this.Rarity = Rarity;
            this.Min = Min;
            this.Max = Max;
        }

        public string Rarity { get; set; }

        public int Min { get; set; }

        public int Max { get; set; }

    }
}

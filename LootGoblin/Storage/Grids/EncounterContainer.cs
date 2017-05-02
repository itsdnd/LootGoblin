namespace LootGoblin.Storage
{
    public class EncounterContainer
    {
        public int Quantity { get; set; }
        public string Name { get; set; }

        public EncounterContainer(int Quantity, string Name)
        {
            this.Quantity = Quantity;
            this.Name = Name;
        }
    }
}

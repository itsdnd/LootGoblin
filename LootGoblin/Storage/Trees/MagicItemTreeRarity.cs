using System.Collections.Generic;

namespace LootGoblin.Storage.Trees
{
    public class MagicItemTreeRarity
    {
        public MagicItemTreeRarity()
        {
            Names = new List<MagicItemTreeName>();
        }

        public string Name { get; set; }

        public List<MagicItemTreeName> Names { get; set; }

        public override string ToString()
        {
            return Name;
        }
    }
}

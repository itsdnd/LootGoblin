using System.Collections.Generic;

namespace LootGoblin.Storage.Trees
{
    public class MagicItemTreeType
    {
        public MagicItemTreeType()
        {
            RarityList = new List<MagicItemTreeRarity>();
        }

        public string Name { get; set; }

        public List<MagicItemTreeRarity> RarityList { get; set; }

        public override string ToString()
        {
            return Name;
        }
    }
}

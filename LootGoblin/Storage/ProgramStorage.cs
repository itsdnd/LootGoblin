using LootGoblin.Storage;
using LootGoblin.Storage.Grids;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace LootGoblin
{
    class ProgramStorage
    {
        private static ProgramStorage instance = null;

        public ProgramStorage()
        {
            LootContainers = new List<LootContainer>();
            MagicItems = new List<MagicItem>();
            MundaneItems = new List<string>();

            EncounterList = new ObservableCollection<EncounterContainer>();
            MagicItemList = new ObservableCollection<MagicItem>();
            RandomMagicItemList = new ObservableCollection<RandomMagicItem>();
        }

        public static ProgramStorage GetInstance()
        {
            if (instance == null)
            {
                instance = new ProgramStorage();
            }

            return instance;
        }

        public List<LootContainer> LootContainers { get; set; }

        public List<MagicItem> MagicItems { get; set; }

        public ObservableCollection<EncounterContainer> EncounterList { get; set; }

        public ObservableCollection<MagicItem> MagicItemList { get; set; }
        
        public ObservableCollection<RandomMagicItem> RandomMagicItemList { get; set; }

        public List<string> ContainerTypes { get; set; }

        public List<string> MagicItemTypes { get; set; }

        public List<string> MagicItemRarities { get; set; }

        public List<string> MundaneItems { get; set; }

        public string[] BasicContainerTypes
        {
            get
            {
                return new string[] { "Aberrations", "Beasts", "Celestials", "Constructs", "Dragons", "Elemental", "Fey", "Fiends", "Giants", "Humanoids", "Monstrosities", "Oozes", "Other", "Plants", "Undead" };
            }
        }

        public string[] BasicMagicTypes
        {
            get
            {
                return new string[] { "Armor", "Clothing", "Focus (Arcane)", "Focus (Holy)", "Potion", "Scroll", "Weapon", "Wondrous" };
            }
        }

        public string[] BasicMagicRarities
        {
            get
            {
                return new string[] { "Common", "Uncommon", "Rare", "Very Rare", "Legendary" };
            }
        }

        public AppSettings Settings { get; set; }

        public void GenerateMagicTypesAndRaritiesLists()
        {
            MagicItemTypes = new List<string>(BasicMagicTypes);
            MagicItemRarities = new List<string>(BasicMagicRarities);

            foreach (MagicItem item in MagicItems)
            {
                if (!MagicItemTypes.Contains(item.Type))
                {
                    MagicItemTypes.Add(item.Type);
                }

                if (!MagicItemRarities.Contains(item.Rarity))
                {
                    MagicItemRarities.Add(item.Rarity);
                }
            }

            MagicItemTypes.Sort();
        }

        public void GenerateLootContainerTypesLists()
        {
            ContainerTypes = new List<string>(BasicContainerTypes);

            foreach (LootContainer container in LootContainers)
            {
                if (!ContainerTypes.Contains(container.Type))
                {
                    ContainerTypes.Add(container.Type);
                }
            }
        }
    }
}

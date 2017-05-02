using System.Collections.Generic;

namespace LootGoblin.Storage.Trees
{
    public class ContainerTreeType
    {
        public ContainerTreeType()
        {
            NameList = new List<ContainerTreeName>();
        }

        public string Name { get; set; }

        public List<ContainerTreeName> NameList { get; set; }

        public override string ToString()
        {
            return Name;
        }
    }
}

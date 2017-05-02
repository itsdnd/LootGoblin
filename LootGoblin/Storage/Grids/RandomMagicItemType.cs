using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LootGoblin.Storage.Grids
{
    class RandomMagicItemType
    {
        public RandomMagicItemType(string Type, int Min, int Max)
        {
            this.Type = Type;
            this.Min = Min;
            this.Max = Max;
        }

        public string Type { get; set; }

        public int Min { get; set; }

        public int Max { get; set; }

    }
}

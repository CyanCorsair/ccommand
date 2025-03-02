using System.Collections.Generic;

namespace Terrain
{
    public enum BiomeTypes
    {
        Ocean,
        Plains,
        Hills,
        Mountains,
    }

    public class BiomeData
    {
        public Dictionary<BiomeTypes, List<TerrainTypes>> BiomeTerrains =
            new Dictionary<BiomeTypes, List<TerrainTypes>>();

        public BiomeData()
        {
            BiomeTerrains.Add(
                BiomeTypes.Ocean,
                new List<TerrainTypes> { TerrainTypes.ShallowWater }
            );
            BiomeTerrains.Add(BiomeTypes.Plains, new List<TerrainTypes> { TerrainTypes.Plains });
            BiomeTerrains.Add(BiomeTypes.Hills, new List<TerrainTypes> { TerrainTypes.Hills });
            BiomeTerrains.Add(
                BiomeTypes.Mountains,
                new List<TerrainTypes> { TerrainTypes.Mountains }
            );
        }
    }
}

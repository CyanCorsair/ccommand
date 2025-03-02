using System.Collections.Generic;
using Godot;

namespace Terrain
{
    public class ColorCodes
    {
        public Dictionary<TerrainTypes, Color> TerrainColors =
            new Dictionary<TerrainTypes, Color>();

        public ColorCodes()
        {
            TerrainColors.Add(TerrainTypes.ShallowWater, new Color(0, 0, 255));
            TerrainColors.Add(TerrainTypes.Plains, new Color(0, 255, 0));
            TerrainColors.Add(TerrainTypes.Hills, new Color(255, 255, 0));
            TerrainColors.Add(TerrainTypes.Mountains, new Color(255, 0, 0));
        }
    }

    public class TerrainData
    {
        public Dictionary<TerrainTypes, int> TerrainDepths = new Dictionary<TerrainTypes, int>();

        public TerrainData()
        {
            TerrainDepths.Add(TerrainTypes.ShallowWater, -1);
            TerrainDepths.Add(TerrainTypes.Plains, 0);
            TerrainDepths.Add(TerrainTypes.Hills, 1);
            TerrainDepths.Add(TerrainTypes.Mountains, 2);
        }
    }
}

public enum TerrainTypes
{
    ShallowWater,
    Plains,
    Hills,
    Mountains,
}

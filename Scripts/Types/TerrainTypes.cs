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
            TerrainColors.Add(TerrainTypes.ShallowWater, new Color(0.416f, 0.827f, 1.0f));
            TerrainColors.Add(TerrainTypes.Plains, new Color(0.659f, 0.871f, 0.341f));
            TerrainColors.Add(TerrainTypes.Hills, new Color(0.749f, 0.62f, 0.141f));
            TerrainColors.Add(TerrainTypes.Mountains, new Color(0.412f, 0.247f, 0.063f));
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

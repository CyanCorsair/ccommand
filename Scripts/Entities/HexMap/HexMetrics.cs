using Godot;

namespace CCommandCore.HexMap.HexMetrics;

public enum HexEdgeType
{   
    Flat,
    Slope,
    Cliff
}

public static class HexMetrics
{
    public const float outerRadius = 10f;
    public const float innerRadius = outerRadius * 0.866025404f;

    public const float solidFactor = 0.75f;
    public const float blendFactor = 1.0f - solidFactor;

    public const float elevationStep = 2.5f;

    public const int terracesPerSlope = 2;
    public const float terraceSteps = terracesPerSlope * 2 + 1;

    public const float horizontalTerraceStep = 1f / terraceSteps;
    public const float verticalTerraceStep = 1f / (terracesPerSlope + 1);

    static Vector3[] corners =
    {
        new Vector3(0f, 0f, outerRadius),
        new Vector3(innerRadius, 0f, 0.5f * outerRadius),
        new Vector3(innerRadius, 0f, -0.5f * outerRadius),
        new Vector3(0f, 0f, -outerRadius),
        new Vector3(-innerRadius, 0f, -0.5f * outerRadius),
        new Vector3(-innerRadius, 0f, 0.5f * outerRadius),
        new Vector3(0f, 0f, outerRadius)
    };

    public static Vector3 TerraceLerp(Vector3 a, Vector3 b, int step)
    {
        float h = step * HexMetrics.horizontalTerraceStep;
        a.X += (b.X - a.X) * h;
        a.Z += (b.Z - a.Z) * h;
        float v = ((step + 1) / 2) * HexMetrics.verticalTerraceStep;
        a.Y += (b.Y - a.Y) * v;
        return a;
    }

    public static Color TerraceColorLerp(Color c1, Color c2, int step)
    {
        float h = step * HexMetrics.horizontalTerraceStep;
        return c1.Lerp(c2, h);
    }

    public static HexEdgeType GetEdgeType(int elevation1, int elevation2)
    {
        if (elevation1 == elevation2)
        {
            return HexEdgeType.Flat;
        }

        int delta = elevation2 - elevation1;
        if (delta == 1 || delta == -1)
        {
            return HexEdgeType.Slope;
        }
        
        return HexEdgeType.Cliff;
    }

    public static Vector3 GetFirstCorner(HexDirection direction)
    {
        return corners[(int)direction];
    }

    public static Vector3 GetSecondCorner(HexDirection direction)
    {
        return corners[(int)direction + 1];
    }

    public static Vector3 GetFirstSolidCorner(HexDirection direction)
    {
        return corners[(int)direction] * solidFactor;
    }

    public static Vector3 GetSecondSolidCorner(HexDirection direction)
    {
        return corners[(int)direction + 1] * solidFactor;
    }

    public static Vector3 GetBridge(HexDirection direction)
    {
        return (corners[(int)direction] + corners[(int)direction + 1]) * blendFactor;
    }
}

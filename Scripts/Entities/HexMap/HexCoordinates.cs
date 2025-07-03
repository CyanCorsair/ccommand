using Godot;

using CCommandCore.HexMap.HexMetrics;

[System.Serializable]
public struct HexCoordinates
{
    [ExportGroup("Hex coordinates")]
    [Export]
    public int X { get; set; }

    [Export]
    public int Z { get; set; }

    [Export]
    public int Y
    {
        get { return -X - Z; }
    }

    public HexCoordinates(int newX, int newZ)
    {
        X = newX;
        Z = newZ;
    }

    public static HexCoordinates FromPosition(Vector3 position)
    {
        float x = position.X / (HexMetrics.innerRadius * 2f);
        float y = -x;
        float offset = position.Z / (HexMetrics.outerRadius * 3f);
        x -= offset;
        y -= offset;
        int iX = Mathf.RoundToInt(x);
        int iY = Mathf.RoundToInt(y);
        int iZ = Mathf.RoundToInt(-x - y);

        if (iX + iY + iZ != 0)
        {
            float dX = Mathf.Abs(x - iX);
            float dY = Mathf.Abs(y - iY);
            float dZ = Mathf.Abs(-x - y - iZ);

            if (dX > dY && dX > dZ)
            {
                iX = -iY - iZ;
            }
            else if (dZ > dY)
            {
                iZ = -iX - iY;
            }
        }

        return new HexCoordinates(iX, iZ);
    }

    public static HexCoordinates FromOffsetCoordinates(int newX, int newZ)
    {
        return new HexCoordinates(newX - newZ / 2, newZ);
    }

    public override string ToString()
    {
        return "(" + X.ToString() + ", " + Y.ToString() + ", " + Z.ToString() + ")";
    }

    public string ToLineSeparatedString()
    {
        return X.ToString() + "\n" + Y.ToString() + "\n" + Z.ToString();
    }
}

using Godot;

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

public enum HexDirection
{
    NorthEast,
    East,
    SouthEast,
    SouthWest,
    West,
    NorthWest
}

public static class HexDirectionExtensions
{
    public static HexDirection Opposite(this HexDirection direction)
    {
        return (int)direction < 3
            ? (HexDirection)((int)direction + 3)
            : (HexDirection)((int)direction - 3);
    }
}

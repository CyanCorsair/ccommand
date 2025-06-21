using System.Numerics;

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

    public static HexDirection Previous(this HexDirection direction)
    {
        return direction == HexDirection.NorthEast ? HexDirection.NorthWest : (direction - 1);
    }

    public static HexDirection Next(this HexDirection direction)
    {
        return direction == HexDirection.NorthWest ? HexDirection.NorthEast : (direction + 1);
    }
}

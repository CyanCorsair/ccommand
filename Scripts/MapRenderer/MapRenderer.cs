using Godot;
using System;

public partial class MapRenderer : Node2D
{
    private WorldMapGenerator MapDataGenerator;

    private Random random = new Random();

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        MapDataGenerator = new WorldMapGenerator();
        MapDataGenerator._Ready();
        MapDataGenerator.QueueRedraw();
    }

    public override void _Draw()
    {
        base._Draw();

        Tile[,] tiles = MapDataGenerator.tiles;

        for (int y = 0, lenY = tiles.GetLength(0); y < lenY; y++)
        {
            for (int x = 0, lenX = tiles.GetLength(1); x < lenX; x++)
            {
                Tile tile = tiles[y, x];

                if (tile == null)
                {
                    continue;
                }

                Vector2[] hexPoints = tile.GetPoints();
                DrawPolygon(hexPoints, new Color[] { tile.TileColor });
            }
        }
    }
}

using Godot;
using Terrain;

public partial class Tile : Node2D
{
    // Read only props START
    public int width = 6;
    public int height = 6;
    public HexCell cellInstance = GD.Load<HexCell>("res://Assets/Entities/HexCell.tscn");

    HexCell[] cells;
    private readonly int tileX;
    private readonly int tileY;
    private readonly int tileDepth;
    private readonly TerrainTypes tileTerrain;
    private readonly Vector2[] tilePoints;
    private readonly ColorCodes colorCodes = new ColorCodes();
    private readonly TerrainData terrainData = new TerrainData();

    // Read only props END

    private Color tileColor;
    public Color TileColor
    {
        get { return tileColor; }
        set { tileColor = value; }
    }

    private bool isOwned;
    public bool IsOwned
    {
        get { return isOwned; }
        set { isOwned = value; }
    }

    private bool isDiscovered;
    public bool IsDiscovered
    {
        get { return isDiscovered; }
        set { isDiscovered = value; }
    }

    private bool isScouted;
    public bool IsScouted
    {
        get { return isScouted; }
        set { isScouted = value; }
    }

    public Tile(int yIndex, int xIndex, TerrainTypes terrain, Vector2[] points)
    {
        tileX = xIndex;
        tileY = yIndex;
        tileTerrain = terrain;
        tilePoints = points;

        tileDepth = terrainData.TerrainDepths[terrain];
        TileColor = colorCodes.TerrainColors[terrain];

        IsOwned = false;
        IsDiscovered = false;
        IsScouted = false;
    }

    public Vector2[] GetPoints()
    {
        return tilePoints;
    }

    public int[] GetTileXAndY()
    {
        return new int[] { tileX, tileY };
    }

    public TerrainTypes GetTerrain()
    {
        return tileTerrain;
    }

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        cells = new HexCell[width * height];
        for (int z = 0, i = 0; z < height; z++)
        {
            for (int x = 0; x < width; x++)
            {
                HexCell.CreateCell(x, z, i++);
            }
        }
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta) { }
}

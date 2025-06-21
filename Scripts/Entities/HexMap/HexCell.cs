using System.Collections.Generic;
using Godot;

public partial class HexCell : Node3D
{
    private MeshInstance3D meshInstanceNode;
    private CollisionShape3D collisionShapeNode;
    private Label3D hexLabelNode;

    private SurfaceTool surfaceTool = new SurfaceTool();
    private SurfaceTool surfaceTool2 = new SurfaceTool();
    private bool hasCenterBeenAdded = false;

    // Colors
    public Color defaultColourOne = new Color(1.0f, 1.0f, 0.0f);
    public Color defaultColourTwo = new Color(0.0f, 1.0f, 1.0f);
    public Color defaultColourThree = new Color(1.0f, 0.0f, 1.0f);

    public PackedScene hexCellScene = ResourceLoader.Load<PackedScene>(
        "res://Scenes/CommonComponents/HexCell.tscn"
    );
    public HexCell sceneInstance;

    public HexCoordinates coordinates;

    ArrayMesh hexMesh;
    ArrayMesh hexagon;
    List<Vector3> hexVertices = new List<Vector3>();
    List<Vector3> quadVertices = new List<Vector3>();
    List<Vector2> hexUvs = new List<Vector2>();
    List<Vector2> quadUvs = new List<Vector2>();
    List<int> triangles = new List<int>();
    List<int> quads = new List<int>();
    List<Color> hexColours = new List<Color>();
    List<Color> quadColours = new List<Color>();

    [Export]
    HexCell[] neighbours = new HexCell[6];

    public HexCell CreateSceneInstance(string name, Transform3D transform)
    {
        sceneInstance = hexCellScene.Instantiate<HexCell>();
        sceneInstance.Name = name;
        sceneInstance.Transform = transform;
        return sceneInstance;
    }

    public void SetChildNodeReferences()
    {
        meshInstanceNode = sceneInstance.GetNode<MeshInstance3D>("HexMeshInstance");
        hexLabelNode = sceneInstance.GetNode<Label3D>("HexMeshInstance/HexLabel");
    }

    public void SetCellText()
    {
        hexLabelNode.Text = $"X: {coordinates.X}, Y: {coordinates.Y}, Z: {coordinates.Z}";
        var tempTransform = sceneInstance.Transform;
        tempTransform.Origin.Y = 0.001f;
        hexLabelNode.Transform = tempTransform;
        hexLabelNode.Rotate(new Vector3(1f, 0f, 0f), Mathf.DegToRad(-90f));
    }

    public void SetCellMesh()
    {
        surfaceTool.Begin(Mesh.PrimitiveType.Triangles);
        surfaceTool2.Begin(Mesh.PrimitiveType.TriangleStrip);
        Triangulate();
        surfaceTool.Index();
        surfaceTool.GenerateNormals();
        surfaceTool.GenerateTangents();
        hexagon = surfaceTool2.Commit();
        hexMesh = surfaceTool.Commit(hexagon);
        meshInstanceNode.Mesh = hexMesh;

        for (int i = 0; i < hexMesh.GetSurfaceCount(); i++)
        {
            StandardMaterial3D defaultMaterial = new StandardMaterial3D();
            defaultMaterial.VertexColorUseAsAlbedo = true;
            meshInstanceNode.SetSurfaceOverrideMaterial(i, defaultMaterial);
        }

        meshInstanceNode.CreateTrimeshCollision();
        collisionShapeNode = meshInstanceNode.GetNode<CollisionShape3D>(
            "HexMeshInstance_col/CollisionShape3D"
        );
        meshInstanceNode.Transform.Translated(Vector3.Zero);
    }

    void Triangulate(HexCell cell)
    {
        for (HexDirection dir = HexDirection.NorthEast; dir <= HexDirection.NorthWest; dir++)
        {
            Triangulate(dir, cell);
        }
    }

    void Triangulate(HexDirection direction, HexCell cell)
    {
        Godot.Collections.Array arrays = new Godot.Collections.Array();
        arrays.Resize((int)ArrayMesh.ArrayType.Max);

        Vector3 center = Transform.Origin;
        Vector3 v1 = center + HexMetrics.GetFirstSolidCorner(direction);
        Vector3 v2 = center + HexMetrics.GetSecondSolidCorner(direction);

        if (!hasCenterBeenAdded)
        {
            AddTriangle(center, v1, v2);
            AddTriangleColor(cell.defaultColourOne);
            hasCenterBeenAdded = true;
        }
        else
        {
            ContinueTriangle(v1, v2);
            ContinueTriangleColor(cell.defaultColourOne);
        }

        Vector3 v3 = center + HexMetrics.GetFirstCorner(direction);
        Vector3 v4 = center + HexMetrics.GetSecondCorner(direction);

        HexCell nextNeighbour = cell.GetNeighbour(direction.Next()) ?? cell;
        HexCell prevNeighbour = cell.GetNeighbour(direction.Previous()) ?? cell;
        HexCell neighbour = cell.GetNeighbour(direction) ?? cell;

        Color edgeColourPrev =
            (cell.defaultColourOne + prevNeighbour.defaultColourOne + neighbour.defaultColourOne)
            / 3f;
        Color innerEdgeColourPrev =
            (cell.defaultColourOne + prevNeighbour.defaultColourOne + neighbour.defaultColourOne)
            / 2f;
        Color edgeColourNext =
            (cell.defaultColourOne + neighbour.defaultColourOne + nextNeighbour.defaultColourOne)
            / 3f;
        Color innerEdgeColourNext =
            (cell.defaultColourOne + neighbour.defaultColourOne + nextNeighbour.defaultColourOne)
            / 2f;

        AddQuad(
            v1,
            v2,
            v3,
            v4,
            edgeColourNext,
            edgeColourPrev,
            innerEdgeColourPrev,
            innerEdgeColourNext
        );

        AddQuadColour(cell.defaultColourOne, cell.defaultColourOne, edgeColourPrev, edgeColourNext);
    }

    public void Triangulate()
    {
        ClearMeshData();
        hexagon ??= new ArrayMesh();
        for (int i = 0; i < 6; i++)
        {
            Triangulate(this);

            Godot.Collections.Array quadArrays = new Godot.Collections.Array();
            quadArrays.Resize((int)ArrayMesh.ArrayType.Max);
            quadArrays[(int)ArrayMesh.ArrayType.Vertex] = quadVertices.ToArray();
            quadArrays[(int)ArrayMesh.ArrayType.Color] = quadColours.ToArray();

            if (hexVertices.Count != hexColours.Count)
            {
                GD.PrintErr(
                    $"Vertex count ({hexVertices.Count}) does not match color count ({hexColours.Count})!"
                );
            }
            //hexagon.AddSurfaceFromArrays(Mesh.PrimitiveType.TriangleStrip, quadArrays);
        }

        surfaceTool.AddTriangleFan(hexVertices.ToArray(), hexUvs.ToArray(), hexColours.ToArray());
    }

    public HexCell[] GetAllNeighbours()
    {
        return neighbours;
    }

    public HexCell GetNeighbour(HexDirection direction)
    {
        return neighbours[(int)direction];
    }

    public void SetNeighbour(HexDirection direction, HexCell cell)
    {
        neighbours[(int)direction] = cell;
        cell.neighbours[(int)direction.Opposite()] = this;
    }

    void AddTriangle(Vector3 v1, Vector3 v2, Vector3 v3)
    {
        int vertexIndex = hexVertices.Count;
        hexVertices.Add(v1);
        hexUvs.Add(new Vector2(v1.X, v1.Y));
        hexVertices.Add(v2);
        hexUvs.Add(new Vector2(v2.X, v2.Y));
        hexVertices.Add(v3);
        hexUvs.Add(new Vector2(v3.X, v3.Y));
        triangles.Add(vertexIndex);
        triangles.Add(vertexIndex + 1);
        triangles.Add(vertexIndex + 2);
    }

    void AddTriangleColor(Color colourOne)
    {
        hexColours.Add(colourOne);
        hexColours.Add(colourOne);
        hexColours.Add(colourOne);
    }

    void AddQuad(Vector3 v1, Vector3 v2, Vector3 v3, Vector3 v4)
    {
        int vertexIndex = quadVertices.Count;
        quadVertices.Add(v1);
        quadUvs.Add(new Vector2(v1.X, v1.Y));
        quadVertices.Add(v2);
        quadUvs.Add(new Vector2(v2.X, v2.Y));
        quadVertices.Add(v3);
        quadUvs.Add(new Vector2(v3.X, v3.Y));
        quadVertices.Add(v4);
        quadUvs.Add(new Vector2(v4.X, v4.Y));
        quads.Add(vertexIndex);
        quads.Add(vertexIndex + 2);
        quads.Add(vertexIndex + 1);
        quads.Add(vertexIndex + 1);
        quads.Add(vertexIndex + 2);
        quads.Add(vertexIndex + 3);
    }

    void AddQuad(
        Vector3 v1,
        Vector3 v2,
        Vector3 v3,
        Vector3 v4,
        Color c1,
        Color c2,
        Color c3,
        Color c4
    )
    {
        int vertexIndex = quadVertices.Count;
        surfaceTool2.SetColor(c1);
        surfaceTool2.AddVertex(v1);

        surfaceTool2.SetColor(c2);
        surfaceTool2.AddVertex(v2);

        surfaceTool2.SetColor(c3);
        surfaceTool2.AddVertex(v3);

        surfaceTool2.SetColor(c4);
        surfaceTool2.AddVertex(v4);

        quads.Add(vertexIndex);
        quads.Add(vertexIndex + 2);
        quads.Add(vertexIndex + 1);
        quads.Add(vertexIndex + 1);
        quads.Add(vertexIndex + 2);
        quads.Add(vertexIndex + 3);
    }

    void AddQuadColour(Color c1, Color c2, Color c3, Color c4)
    {
        quadColours.Add(c1);
        quadColours.Add(c2);
        quadColours.Add(c3);
        quadColours.Add(c4);
    }

    void ContinueTriangle(Vector3 v1, Vector3 v2)
    {
        int vertexIndex = hexVertices.Count;
        hexVertices.Add(v1);
        hexUvs.Add(new Vector2(v1.X, v1.Y));
        hexVertices.Add(v2);
        hexUvs.Add(new Vector2(v2.X, v2.Y));
        triangles.Add(vertexIndex + 1);
        triangles.Add(vertexIndex + 2);
    }

    void ContinueTriangleColor(Color colourOne)
    {
        hexColours.Add(colourOne);
        hexColours.Add(colourOne);
    }

    void ClearMeshData()
    {
        if (hexagon != null)
        {
            hexMesh.ClearSurfaces();
            hexagon.ClearSurfaces();
            hexVertices.Clear();
            quadVertices.Clear();
            hexUvs.Clear();
            quadUvs.Clear();
            triangles.Clear();
            quads.Clear();
            hexColours.Clear();
            quadColours.Clear();
        }
    }
}

using System;
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
        hexLabelNode.Text = $"X: {coordinates.X}, Z: {coordinates.Z}";
        var tempTransform = sceneInstance.Transform;
        tempTransform.Origin.Y = 0.001f;
        hexLabelNode.Transform = tempTransform;
        hexLabelNode.Rotate(new Vector3(1f, 0f, 0f), Mathf.DegToRad(-90f));
    }

    public void SetCellMesh()
    {
        surfaceTool.Clear();
        surfaceTool2.Clear();

        surfaceTool.Begin(Mesh.PrimitiveType.Triangles);
        surfaceTool2.Begin(Mesh.PrimitiveType.Triangles);
        Triangulate();
        surfaceTool.Index();
        surfaceTool.GenerateNormals();
        surfaceTool.GenerateTangents();
        surfaceTool2.Index();
        surfaceTool2.GenerateNormals();
        surfaceTool2.GenerateTangents();
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
        Vector3 center = Transform.Origin;
        Vector3 v1 = center + HexMetrics.GetFirstSolidCorner(direction);
        Vector3 v2 = center + HexMetrics.GetSecondSolidCorner(direction);

        SetTriangleVerticesMono(center, v1, v2,
            cell.defaultColourOne);
        
        if (direction <= HexDirection.SouthEast)
        {
            TriangulateConnection(direction, cell, v1, v2);
        }
    }

    public void Triangulate()
    {
        ClearMeshData();
        hexagon ??= new ArrayMesh();
        for (int i = 0; i < 6; i++)
        {
            Triangulate(this);
        }
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

    void SetTriangleVertices(
        Vector3 v1, Vector3 v2, Vector3 v3,
        Color c1, Color c2, Color c3)
    {
        surfaceTool.SetSmoothGroup(UInt32.MaxValue);

        surfaceTool.SetColor(c3);
        surfaceTool.SetUV(new Vector2(v3.X, v3.Y));
        surfaceTool.AddVertex(v3);

        surfaceTool.SetColor(c2);
        surfaceTool.SetUV(new Vector2(v2.X, v2.Y));
        surfaceTool.AddVertex(v2);

        surfaceTool.SetColor(c1);
        surfaceTool.SetUV(new Vector2(v1.X, v1.Y));
        surfaceTool.AddVertex(v1);
    }

    void SetTriangleVerticesMono(
        Vector3 v1, Vector3 v2, Vector3 v3,
        Color c1)
    {
        surfaceTool.SetSmoothGroup(UInt32.MaxValue);

        surfaceTool.SetColor(c1);
        surfaceTool.SetUV(new Vector2(v3.X, v3.Y));
        surfaceTool.AddVertex(v3);

        surfaceTool.SetColor(c1);
        surfaceTool.SetUV(new Vector2(v2.X, v2.Y));
        surfaceTool.AddVertex(v2);

        surfaceTool.SetColor(c1);
        surfaceTool.SetUV(new Vector2(v1.X, v1.Y));
        surfaceTool.AddVertex(v1);
    }

    void TriangulateConnection(
        HexDirection direction,
        HexCell cell,
        Vector3 v1,
        Vector3 v2)
    {
        HexCell neighbour = cell.GetNeighbour(direction);
        HexCell nextNeighbour = cell.GetNeighbour(direction.Next());

        Color edgeColor = neighbour != null ? neighbour.defaultColourOne : cell.defaultColourOne;

        if (neighbour == null)
        {
            return;
        }

        Vector3 bridge = HexMetrics.GetBridge(direction);
        Vector3 v3 = v1 + bridge;
        Vector3 v4 = v2 + bridge;

        AddQuad(
            v1, v2, v3, v4,
            cell.defaultColourOne, edgeColor);

        if (direction <= HexDirection.East && nextNeighbour != null)
        {
            SetTriangleVertices(
                v2, v4, v2 + HexMetrics.GetBridge(direction.Next()),
                cell.defaultColourOne, neighbour.defaultColourOne, nextNeighbour.defaultColourOne);
        }
    }

    void AddQuad(
        Vector3 v1,
        Vector3 v2,
        Vector3 v3,
        Vector3 v4,
        Color c1,
        Color c2
    )
    {
        surfaceTool2.SetSmoothGroup(UInt32.MaxValue);

        // Triangle 1
        surfaceTool2.SetColor(c1);
        surfaceTool2.SetUV(new Vector2(v1.X, v1.Y));
        surfaceTool2.AddVertex(v1);

        surfaceTool2.SetColor(c1);
        surfaceTool2.SetUV(new Vector2(v2.X, v2.Y));
        surfaceTool2.AddVertex(v2);

        surfaceTool2.SetColor(c2);
        surfaceTool2.SetUV(new Vector2(v4.X, v4.Y));
        surfaceTool2.AddVertex(v4);

        // Triangle 2
        surfaceTool2.SetColor(c1);
        surfaceTool2.SetUV(new Vector2(v1.X, v1.Y));
        surfaceTool2.AddVertex(v1);

        surfaceTool2.SetColor(c2);
        surfaceTool2.SetUV(new Vector2(v4.X, v4.Y));
        surfaceTool2.AddVertex(v4);

        surfaceTool2.SetColor(c2);
        surfaceTool2.SetUV(new Vector2(v3.X, v3.Y));
        surfaceTool2.AddVertex(v3);
    }

    void ClearMeshData()
    {
        if (hexagon != null)
        {
            hexMesh.ClearSurfaces();
            hexagon.ClearSurfaces();
        }
    }
}

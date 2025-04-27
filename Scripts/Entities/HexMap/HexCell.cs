using System.Collections.Generic;
using Godot;

public partial class HexCell : Node3D
{
    private MeshInstance3D meshInstanceNode;
    private CollisionShape3D collisionShapeNode;
    private Label3D hexLabelNode;

    // Colors
    public Color defaultColour = new Color(0.0f, 0.0f, 0.0f);

    public PackedScene hexCellScene = ResourceLoader.Load<PackedScene>(
        "res://Scenes/CommonComponents/HexCell.tscn"
    );
    public HexCell sceneInstance;

    public HexCoordinates coordinates;

    ArrayMesh hexagon;
    List<Vector3> vertices = new List<Vector3>();
    List<int> triangles = new List<int>();
    List<Color> colours = new List<Color>();

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
        Triangulate();
        meshInstanceNode.Mesh = hexagon;

        for (int i = 0; i < hexagon.GetSurfaceCount(); i++)
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

    public void Triangulate()
    {
        ClearMeshData();
        hexagon ??= new ArrayMesh();

        Vector3 center = Transform.Origin;
        for (int i = 0; i < 6; i++)
        {
            AddTriangle(center, center + HexMetrics.corners[i], center + HexMetrics.corners[i + 1]);
            AddTriangleColor(defaultColour);

            Godot.Collections.Array arrays = new Godot.Collections.Array();
            arrays.Resize((int)ArrayMesh.ArrayType.Max);
            arrays[(int)ArrayMesh.ArrayType.Vertex] = vertices.ToArray();
            arrays[(int)ArrayMesh.ArrayType.Color] = colours.ToArray();

            hexagon.AddSurfaceFromArrays(Mesh.PrimitiveType.Triangles, arrays);
        }
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

    private void AddTriangle(Vector3 v1, Vector3 v2, Vector3 v3)
    {
        int vertexIndex = vertices.Count;
        vertices.Add(v1);
        vertices.Add(v2);
        vertices.Add(v3);
        triangles.Add(vertexIndex);
        triangles.Add(vertexIndex + 1);
        triangles.Add(vertexIndex + 2);
    }

    private void AddTriangleColor(Color colour)
    {
        colours.Add(colour);
        colours.Add(colour);
        colours.Add(colour);
    }

    private void ClearMeshData()
    {
        if (hexagon != null)
        {
            hexagon.ClearSurfaces();
            vertices.Clear();
            triangles.Clear();
            colours.Clear();
        }
    }
}

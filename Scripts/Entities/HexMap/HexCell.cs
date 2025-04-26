using System.Collections.Generic;
using Godot;

public partial class HexCell : Node3D
{
    private MeshInstance3D meshInstanceNode;
    private CollisionShape3D collisionShapeNode;
    private Label3D hexLabelNode;

    public PackedScene hexCellScene = ResourceLoader.Load<PackedScene>(
        "res://Scenes/CommonComponents/HexCell.tscn"
    );
    public HexCell sceneInstance;

    public HexCoordinates coordinates;

    ArrayMesh hexagon;
    List<Vector3> vertices = new List<Vector3>();
    List<int> triangles = new List<int>();

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
        hexLabelNode.Text = coordinates.ToLineSeparatedString();
        var tempTransform = sceneInstance.Transform;
        tempTransform.Origin.Y = 0.001f;
        hexLabelNode.Transform = tempTransform;
        hexLabelNode.Rotate(new Vector3(1f, 0f, 0f), Mathf.DegToRad(-90f));
    }

    public void SetCellMesh()
    {
        if (hexagon == null)
        {
            Triangulate();
        }

        meshInstanceNode.Mesh = hexagon;
        meshInstanceNode.CreateTrimeshCollision();
        collisionShapeNode = meshInstanceNode.GetNode<CollisionShape3D>(
            "HexMeshInstance_col/CollisionShape3D"
        );
        meshInstanceNode.Transform.Translated(Vector3.Zero);
    }

    public void Triangulate()
    {
        ClearMeshData();

        Vector3 center = Transform.Origin;
        for (int i = 0; i < 6; i++)
        {
            AddTriangle(center, center + HexMetrics.corners[i], center + HexMetrics.corners[i + 1]);
        }

        hexagon ??= new ArrayMesh();

        Godot.Collections.Array arrays = new Godot.Collections.Array();
        arrays.Resize((int)ArrayMesh.ArrayType.Max);
        arrays[(int)ArrayMesh.ArrayType.Vertex] = vertices.ToArray();

        hexagon.AddSurfaceFromArrays(Mesh.PrimitiveType.Triangles, arrays);
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

    private void ClearMeshData()
    {
        if (hexagon != null)
        {
            hexagon.ClearSurfaces();
            vertices.Clear();
            triangles.Clear();
        }
    }
}

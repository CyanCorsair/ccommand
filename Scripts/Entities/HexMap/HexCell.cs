using CCommandCore.HexMap.HexMetrics;
using Godot;
using System;
using System.Collections.Generic;
using static System.Runtime.InteropServices.JavaScript.JSType;

public partial class HexCell : Node3D
{
    private MeshInstance3D meshInstanceNode;
    private CollisionShape3D collisionShapeNode;
    private Label3D hexLabelNode;

    private SurfaceTool surfaceTool = new SurfaceTool();

    // Colors
    public Color defaultColourOne = new Color(1.0f, 1.0f, 0.0f);
    public Color defaultColourTwo = new Color(0.0f, 1.0f, 1.0f);
    public Color defaultColourThree = new Color(1.0f, 0.0f, 1.0f);

    public PackedScene hexCellScene = ResourceLoader.Load<PackedScene>(
        "res://Scenes/CommonComponents/HexCell.tscn"
    );
    public HexCell sceneInstance;

    public HexCoordinates coordinates;

    private int elevation;

    ArrayMesh hexMesh;

    [Export]
    HexCell[] neighbours = new HexCell[6];

    public int Elevation
    {
        get => elevation;
        set
        {
            elevation = value;
        }
    }

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
        tempTransform.Origin.Y = 0.001f + (elevation * HexMetrics.elevationStep);
        hexLabelNode.Transform = tempTransform;
        hexLabelNode.Rotate(new Vector3(1f, 0f, 0f), Mathf.DegToRad(-90f));
    }

    public void SetCellMesh()
    {
        surfaceTool.Begin(Mesh.PrimitiveType.Triangles);
        Triangulate();
        surfaceTool.Index();
        surfaceTool.GenerateNormals();
        surfaceTool.GenerateTangents();
        hexMesh = surfaceTool.Commit();
        meshInstanceNode.Mesh = hexMesh;

        for (int i = 0; i < hexMesh.GetSurfaceCount(); i++)
        {
            StandardMaterial3D defaultMaterial = new StandardMaterial3D();
            defaultMaterial.VertexColorUseAsAlbedo = true;
            meshInstanceNode.SetSurfaceOverrideMaterial(i, defaultMaterial);
        }

        if (meshInstanceNode.HasNode("HexMeshInstance_col"))
        {
            StaticBody3D hexMeshColContainer = meshInstanceNode.GetNode<StaticBody3D>(
                "HexMeshInstance_col"
            );
            hexMeshColContainer.Free();
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

        center.Y = v1.Y = v2.Y = cell.Elevation * HexMetrics.elevationStep;

        SetTriangleVerticesMono(center, v1, v2,
            cell.defaultColourOne);
        
        if (direction <= HexDirection.SouthEast)
        {
            TriangulateConnection(direction, cell, v1, v2);
        }
    }

    public HexEdgeType GetEdgeTypeByDirection(HexDirection direction)
    {
        return HexMetrics.GetEdgeType(
            elevation, neighbours[(int)direction].elevation);
    }

    public HexEdgeType GetEdgeTypeByCell(HexCell otherCell)
    {
        return HexMetrics.GetEdgeType(
            elevation, otherCell.elevation);
    }

    public void Triangulate()
    {
        ClearMeshData();
        hexMesh ??= new ArrayMesh();
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
        v3.Y = v4.Y = neighbour.Elevation * HexMetrics.elevationStep;

        if (cell.GetEdgeTypeByDirection(direction) == HexEdgeType.Slope)
        {
            TriangulateEdgeTerraces(
            v1, v2, cell,
            v3, v4, neighbour);
        }
        else
        {
            AddQuad(
            v1, v2, v3, v4,
            cell.defaultColourOne, neighbour.defaultColourOne);
        }

        if (direction <= HexDirection.East && nextNeighbour != null)
        {
            Vector3 v5 = v2 + HexMetrics.GetBridge(direction.Next());
            v5.Y = nextNeighbour.Elevation * HexMetrics.elevationStep;

            if (cell.Elevation <= neighbour.Elevation)
            {
                if (cell.Elevation <= nextNeighbour.Elevation)
                {
                    TriangulateCorner(v2, cell, v4, neighbour, v5, nextNeighbour);
                }
                else
                {
                    TriangulateCorner(v5, nextNeighbour, v2, cell, v4, neighbour);
                }
            }
            else if (neighbour.Elevation <= nextNeighbour.Elevation)
            {
                TriangulateCorner(v4, neighbour, v5, nextNeighbour, v2, cell);
            }
            else
            {
                TriangulateCorner(v5, nextNeighbour, v2, cell, v4, neighbour);
            }
        }
    }

    void TriangulateBoundaryTriangle(
        Vector3 begin, HexCell beginCell,
        Vector3 left, HexCell leftCell,
        Vector3 boundary, Color boundaryColor)
    {
        Vector3 v2 = HexMetrics.TerraceLerp(begin, left, 1);
        Color c2 = HexMetrics.TerraceColorLerp(
            beginCell.defaultColourOne, leftCell.defaultColourOne, 1);

        SetTriangleVertices(
            begin, v2, boundary,
            beginCell.defaultColourOne, c2, boundaryColor);

        for (int i = 2; i < HexMetrics.terraceSteps; i++)
        {
            Vector3 v1 = v2;
            Color c1 = c2;
            v2 = HexMetrics.TerraceLerp(begin, left, i);
            c2 = HexMetrics.TerraceColorLerp(
                beginCell.defaultColourOne, leftCell.defaultColourOne, i);

            SetTriangleVertices(
            v1, v2, boundary,
            c1, c2, boundaryColor);
        }

        SetTriangleVertices(
        v2, left, boundary,
        c2, leftCell.defaultColourOne, boundaryColor);
    }

    void TriangulateCliffRightCornerTerraces(
        Vector3 begin, HexCell beginCell,
        Vector3 left, HexCell leftCell,
        Vector3 right, HexCell rightCell)
    {
        float b = 1f / (rightCell.Elevation - beginCell.Elevation);
        if (b < 0)
        {
            b = -b;
        }
        Vector3 boundary = begin.Lerp(right, b);
        Color boundaryColor = beginCell.defaultColourOne.Lerp(
            rightCell.defaultColourOne, b);

        TriangulateBoundaryTriangle(
            begin, beginCell, left, leftCell, boundary, boundaryColor);

        if (leftCell.GetEdgeTypeByCell(rightCell) == HexEdgeType.Slope)
        {
            TriangulateBoundaryTriangle(
                left, leftCell, right, rightCell, boundary, boundaryColor);
        }
        else
        {
            SetTriangleVertices(
                left, right, boundary,
                leftCell.defaultColourOne, rightCell.defaultColourOne, boundaryColor);
        }
    }

    void TriangulateCliffLeftCornerTerraces(
        Vector3 begin, HexCell beginCell,
        Vector3 left, HexCell leftCell,
        Vector3 right, HexCell rightCell)
    {
        float b = 1f / (leftCell.Elevation - beginCell.Elevation);
        if (b < 0)
        {
            b = -b;
        }
        Vector3 boundary = begin.Lerp(left, b);
        Color boundaryColor = beginCell.defaultColourOne.Lerp(
            leftCell.defaultColourOne, b);

        TriangulateBoundaryTriangle(
            right, rightCell, begin, beginCell, boundary, boundaryColor);

        if (leftCell.GetEdgeTypeByCell(rightCell) == HexEdgeType.Slope)
        {
            TriangulateBoundaryTriangle(
                left, leftCell, right, rightCell, boundary, boundaryColor);
        }
        else
        {
            SetTriangleVertices(
                left, right, boundary,
                leftCell.defaultColourOne, rightCell.defaultColourOne, boundaryColor);
        }
    }

    void TriangulateEdgeTerraces(
        Vector3 beginLeft, Vector3 beginRight, HexCell beginCell,
        Vector3 endLeft, Vector3 endRight, HexCell endCell)
    {
        Vector3 v3 = HexMetrics.TerraceLerp(beginLeft, endLeft, 1);
        Vector3 v4 = HexMetrics.TerraceLerp(beginRight, endRight, 1);
        Color c2 = HexMetrics.TerraceColorLerp(
            beginCell.defaultColourOne, endCell.defaultColourOne, 1);

        AddQuad(
            beginLeft, beginRight, v3, v4,
            beginCell.defaultColourOne, c2);

        for (int i = 2; i < HexMetrics.terraceSteps; i++)
        {
            Vector3 v1 = v3;
            Vector3 v2 = v4;
            Color c1 = c2;

            v3 = HexMetrics.TerraceLerp(beginLeft, endLeft, i);
            v4 = HexMetrics.TerraceLerp(beginRight, endRight, i);
            c2 = HexMetrics.TerraceColorLerp(
                beginCell.defaultColourOne, endCell.defaultColourOne, i);
            AddQuad(v1, v2, v3, v4,
                c1, c2);
        }

        AddQuad(
        v3, v4, endLeft, endRight,
        c2, endCell.defaultColourOne);
    }

    void TriangulateCornerTerraces(
        Vector3 begin, HexCell beginCell,
        Vector3 left, HexCell leftCell,
        Vector3 right, HexCell rightCell)
    {
        Vector3 v3 = HexMetrics.TerraceLerp(begin, left, 1);
        Vector3 v4 = HexMetrics.TerraceLerp(begin, right, 1);
        Color c3 = HexMetrics.TerraceColorLerp(
            beginCell.defaultColourOne, leftCell.defaultColourOne, 1);
        Color c4 = HexMetrics.TerraceColorLerp(
            beginCell.defaultColourOne, rightCell.defaultColourOne, 1);

        SetTriangleVertices(
            begin, v3, v4,
            beginCell.defaultColourOne, c3, c4);

        for (int i = 2; i < HexMetrics.terraceSteps; i++)
        {
            Vector3 v1 = v3;
            Vector3 v2 = v4;
            Color c1 = c3;
            Color c2 = c4;

            v3 = HexMetrics.TerraceLerp(begin, left, i);
            v4 = HexMetrics.TerraceLerp(begin, right, i);
            c3 = HexMetrics.TerraceColorLerp(
                beginCell.defaultColourOne, leftCell.defaultColourOne, i);
            c4 = HexMetrics.TerraceColorLerp(
                beginCell.defaultColourOne, rightCell.defaultColourOne, i);

            AddQuad(
                v1, v2, v3, v4,
                c3, c4);
        }

        AddQuad(
            v3, v4, left, right,
            leftCell.defaultColourOne, rightCell.defaultColourTwo);
    }

    void TriangulateCorner(
        Vector3 bottom, HexCell bottomCell,
        Vector3 left, HexCell leftCell,
        Vector3 right, HexCell rightCell)
    {
        HexEdgeType leftEdgeType = bottomCell.GetEdgeTypeByCell(leftCell);
        HexEdgeType rightEdgeType = bottomCell.GetEdgeTypeByCell(rightCell);

        if (leftEdgeType == HexEdgeType.Slope)
        {
            if (rightEdgeType == HexEdgeType.Slope)
            {
                TriangulateCornerTerraces(
                    bottom, bottomCell, left, leftCell, right, rightCell);
            }
            else if (rightEdgeType == HexEdgeType.Flat)
            {
                TriangulateCornerTerraces(
                    left, leftCell, right, rightCell, bottom, bottomCell);
            }
            else
            {
                TriangulateCliffRightCornerTerraces(
                    bottom, bottomCell, left, leftCell, right, rightCell);
            }
        }
        else if (rightEdgeType == HexEdgeType.Slope)
        {
            if (leftEdgeType == HexEdgeType.Flat)
            {
                TriangulateCornerTerraces(
                    right, rightCell, bottom, bottomCell, left, leftCell);
            }
            else
            {
                TriangulateCliffLeftCornerTerraces(
                        bottom, bottomCell, left, leftCell, right, rightCell);
            }
        }
        else if (leftCell.GetEdgeTypeByCell(rightCell) == HexEdgeType.Slope)
        {
            if (leftCell.Elevation < rightCell.Elevation)
            {
                TriangulateCliffRightCornerTerraces(
                    right, rightCell, bottom, bottomCell, left, leftCell);
            }
            else
            {
                TriangulateCliffLeftCornerTerraces(
                    left, leftCell, right, rightCell, bottom, bottomCell);
            }
        }
        else
        {
            SetTriangleVertices(
                bottom, left, right,
                bottomCell.defaultColourOne, leftCell.defaultColourOne, rightCell.defaultColourOne);
        }
    }

    void AddQuad(
        Vector3 v1,
        Vector3 v2,
        Vector3 v3,
        Vector3 v4,
        Color c1,
        Color c2)
    {
        surfaceTool.SetSmoothGroup(UInt32.MaxValue);

        // Triangle 1
        surfaceTool.SetColor(c1);
        surfaceTool.SetUV(new Vector2(v1.X, v1.Y));
        surfaceTool.AddVertex(v1);

        surfaceTool.SetColor(c1);
        surfaceTool.SetUV(new Vector2(v2.X, v2.Y));
        surfaceTool.AddVertex(v2);

        surfaceTool.SetColor(c2);
        surfaceTool.SetUV(new Vector2(v4.X, v4.Y));
        surfaceTool.AddVertex(v4);

        // Triangle 2
        surfaceTool.SetColor(c1);
        surfaceTool.SetUV(new Vector2(v1.X, v1.Y));
        surfaceTool.AddVertex(v1);

        surfaceTool.SetColor(c2);
        surfaceTool.SetUV(new Vector2(v4.X, v4.Y));
        surfaceTool.AddVertex(v4);

        surfaceTool.SetColor(c2);
        surfaceTool.SetUV(new Vector2(v3.X, v3.Y));
        surfaceTool.AddVertex(v3);
    }

    void ClearMeshData()
    {
        if (hexMesh != null)
        {
            hexMesh.ClearSurfaces();
            meshInstanceNode.Mesh = null;
        }
    }
}

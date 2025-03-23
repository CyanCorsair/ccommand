using Godot;
using System;
using System.Collections.Generic;

[GlobalClass]
public partial class HexGrid : Node3D
{
    public int width = 24;
    public int height = 24;

    private HexCell[] cells;
    private PackedScene hexGridScene = ResourceLoader.Load<PackedScene>(
        "res://Scenes/CommonComponents/HexGrid.tscn"
    );
    public HexGrid hexGridInstance;

    public HexGrid InstanceHexGrid()
    {
        hexGridInstance = hexGridScene.Instantiate<HexGrid>();
        hexGridInstance.Name = "Hex Grid";
        hexGridInstance.Transform = new Transform3D(Basis.Identity, Vector3.Zero);
        return hexGridInstance;
    }

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        base._Ready();
        cells = new HexCell[height * width];

        for (int z = 0, i = 0; z < height; z++)
        {
            for (int x = 0; x < width; x++)
            {
                CreateCell(x, z, i++);
            }
        }
    }

    private void CreateCell(int x, int z, int i)
    {
        Vector3 position;
        position.X = (x + z * 0.5f - z / 2) * (HexMetrics.innerRadius * 2f);
        position.Y = 0f;
        position.Z = z * (HexMetrics.outerRadius * 1.5f);

        HexCell cell = cells[i] = new HexCell();
        cell.coordinates = HexCoordinates.FromOffsetCoordinates(x, z);

        cell.Name = "Hex Cell " + x + ", " + z;
        cell.Transform = new Transform3D(Basis.Identity, position / -2f);
        cell.CreateSceneInstance(cell.Name, cell.Transform);
        AddChild(cell.sceneInstance);

        cell.SetChildNodeReferences();
        cell.SetCellMesh();
        cell.SetCellText();
    }
}

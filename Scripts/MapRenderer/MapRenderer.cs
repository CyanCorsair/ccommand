using Godot;
using System;

public partial class MapRenderer : Node3D
{
    private Random random = new Random();

    private HexGrid HexGrid;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        this.HexGrid = new HexGrid();
        HexGrid.InstanceHexGrid();
        AddChild(HexGrid.hexGridInstance);
    }
}

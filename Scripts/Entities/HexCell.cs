using Godot;
using System;

public partial class HexCell : Node2D
{

    public static HexCell CreateCell(int x, int z, int i)
    {
        Vector2 position;
        position.X = x * 10f;
        position.Y = 0f;

        HexCell cell = new HexCell();
        return cell;
    }

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
    {
    }
}

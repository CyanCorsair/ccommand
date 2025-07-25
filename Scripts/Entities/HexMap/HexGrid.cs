using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

using CCommandCore.HexMap.HexMetrics;

[GlobalClass]
public partial class HexGrid : Node3D
{
    public int width = 8;
    public int height = 8;

    private HexCell[] cells;
    private PackedScene hexGridScene = ResourceLoader.Load<PackedScene>(
        "res://Scenes/CommonComponents/HexGrid.tscn"
    );

    // Colors
    [ExportGroup("Hex Colors")]
    [Export]
    public Color defaultCellColour = new Color(0.0f, 0.0f, 0.0f);

    [Export]
    public Color touchedCellColour = new Color(0.6f, 1.0f, 0.7f);

    Color ShallowWater = new Color(0.416f, 0.827f, 1.0f);
    Color Plains = new Color(0.659f, 0.871f, 0.341f);
    Color Hills = new Color(0.749f, 0.62f, 0.141f);
    Color Mountains = new Color(0.412f, 0.247f, 0.063f);

    // Height values
    [ExportGroup("Hex Height")]
    [Export]
    public int activeElevation = 0;

    [Export]
    NoiseTexture3D testNoiseTexture;

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

        for (int z = 0, i = 0; z < height; z++)
        {
            for (int x = 0; x < width; x++)
            {
                SetCellWorldData(i++);
            }
        }
    }

    public void TouchCell(HexCoordinates position)
    {
        HexCell cell = GetHexCell(position);

        if (cell != null)
        {
            cell.defaultColourOne = touchedCellColour;
            cell.Elevation = activeElevation;
            cell.SetCellMesh();
            cell.SetCellText();
        }

        HexCell[] neighbours = cell.GetAllNeighbours();

        foreach (HexCell neighbour in neighbours)
        {
            if (neighbour != null)
            {
                neighbour.SetCellMesh();
                neighbour.SetCellText();
            }
        }
    }

    public HexCell GetHexCell(HexCoordinates position)
    {
        return cells.FirstOrDefault(
            cell =>
                cell.coordinates.X == position.X
                && cell.coordinates.Z == position.Z
                && cell.coordinates.Y == position.Y
        );
    }

    private void CreateCell(int x, int z, int i)
    {
        Vector3 position;
        position.X = (x + z * 0.5f - z / 2) * (HexMetrics.innerRadius * 2f);
        position.Y = 0f;
        position.Z = z * (HexMetrics.outerRadius * 1.5f);

        HexCell cell = cells[i] = new HexCell();
        cell.coordinates = HexCoordinates.FromOffsetCoordinates(x, z);

        float noiseValue2D = testNoiseTexture.Noise.GetNoise2D(x, z) * 10f;
        int adjustedHeightValue;

        if (noiseValue2D <= 0.25 && noiseValue2D >= 0)
        {
            adjustedHeightValue = Mathf.FloorToInt(noiseValue2D);
            cell.defaultColourOne = Plains;
        }
        else if (noiseValue2D < 0)
        {
            adjustedHeightValue = Mathf.FloorToInt(noiseValue2D) - 1;
            cell.defaultColourOne = ShallowWater;
        }
        else if (noiseValue2D > 0.25 && noiseValue2D <= 0.5)
        {
            adjustedHeightValue = 1 + Mathf.CeilToInt(noiseValue2D);
            cell.defaultColourOne = Hills;
        }
        else
        {
            adjustedHeightValue = 1 + Mathf.CeilToInt(noiseValue2D * 2f);
            cell.defaultColourOne = Mountains;
        }
        cell.Elevation = adjustedHeightValue;

        if (x > 0)
        {
            cell.SetNeighbour(HexDirection.West, cells[i - 1]);
        }
        if (z > 0)
        {
            if ((z & 1) == 0)
            {
                cell.SetNeighbour(HexDirection.SouthEast, cells[i - width]);
                if (x > 0)
                {
                    cell.SetNeighbour(HexDirection.SouthWest, cells[i - width - 1]);
                }
            }
            else
            {
                cell.SetNeighbour(HexDirection.SouthWest, cells[i - width]);
                if (x < width - 1)
                {
                    cell.SetNeighbour(HexDirection.SouthEast, cells[i - width + 1]);
                }
            }
        }

        cell.Name = "Hex Cell " + x + ", " + z;
        cell.Transform = new Transform3D(Basis.Identity, position / 2f);
        cell.CreateSceneInstance(cell.Name, cell.Transform);
        AddChild(cell.sceneInstance);
    }

    private void SetCellWorldData(int i)
    {
        HexCell cell = cells[i];

        cell.SetChildNodeReferences();
        cell.SetCellMesh();
        cell.SetCellText();
    }
}

namespace CCommandCore.DevTools;

using Godot;
using System;
using CCommandCore.CTerrainEditor.Observables;
using Terrain;
using System.Collections.Generic;

public partial class DevToolsUi : Control
{
    TerrainEditor terrainEditor;

    public ActiveTerrainHandler terrainHandler = new ActiveTerrainHandler();

    private PackedScene uiScene;
    private Node uiSceneInstance;

    public override void _Ready()
    {
        uiScene = GD.Load<PackedScene>("res://Scenes/StrategicLayer/DevToolsUi.tscn");
        uiSceneInstance = uiScene.Instantiate();
        AddChild(uiSceneInstance);

        terrainEditor = GetNode<TerrainEditor>("DevToolsUi/TerrainWrapper/Terrainlist");
        terrainEditor._activeTerrain = new ActiveTerrainReporter();
        terrainHandler.Subscribe(terrainEditor._activeTerrain);
    }

    public Color getActiveTerrainColor()
    {
        ColorCodes colors = new ColorCodes();
        TerrainTypes currentTerrain = terrainEditor._activeTerrain.activeTerrain.terrainType;
        return colors.TerrainColors.GetValueOrDefault(currentTerrain);
    }
}

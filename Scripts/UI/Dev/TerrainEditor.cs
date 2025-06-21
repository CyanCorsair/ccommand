using System;
using Godot;
using CCommandCore.CTerrainEditor.Observables;

public partial class TerrainEditor : ItemList
{
    TerrainCheckbox plainsCheckbox;
    TerrainCheckbox hillsCheckbox;
    TerrainCheckbox shallowWaterCheckbox;

    public TerrainTypes activeTerrain;
    public ActiveTerrainReporter _activeTerrain;

    public override void _Ready()
    {
        plainsCheckbox = GetNode<TerrainCheckbox>("PlainsContainer/PlainsCheckbox");
        hillsCheckbox = GetNode<TerrainCheckbox>("HillsContainer/HillsCheckbox");
        shallowWaterCheckbox = GetNode<TerrainCheckbox>(
            "ShallowWaterContainer/ShallowWaterCheckbox"
        );

        plainsCheckbox.TerrainSwitchToggled += OnCheckboxToggled;
        hillsCheckbox.TerrainSwitchToggled += OnCheckboxToggled;
        shallowWaterCheckbox.TerrainSwitchToggled += OnCheckboxToggled;
    }

    private void OnCheckboxToggled(Variant terrainType, bool buttonPressed)
    {
        TerrainTypes selectedType = terrainType.As<TerrainTypes>();
        activeTerrain = selectedType;

        TerrainInfo terrainInfo = new TerrainInfo(selectedType);
        _activeTerrain.OnNext(terrainInfo);

        if (selectedType == TerrainTypes.ShallowWater)
        {
            plainsCheckbox.SetPressedNoSignal(false);
            hillsCheckbox.SetPressedNoSignal(false);
        }
        else if (selectedType == TerrainTypes.Plains)
        {
            shallowWaterCheckbox.SetPressedNoSignal(false);
            hillsCheckbox.SetPressedNoSignal(false);
        }
        else if (selectedType == TerrainTypes.Hills)
        {
            shallowWaterCheckbox.SetPressedNoSignal(false);
            plainsCheckbox.SetPressedNoSignal(false);
        }
    }
}

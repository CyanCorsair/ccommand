using Godot;
using System;

public partial class TerrainCheckbox : CheckButton
{
    [Signal]
    public delegate void TerrainSwitchToggledEventHandler(Variant terrainType, bool buttonPressed);

    [Export]
    public TerrainTypes terrain;

    private void EmitTerrainSwitchedEvent(Variant terrainType, bool buttonPressed)
    {
        EmitSignal(SignalName.TerrainSwitchToggled, terrainType, buttonPressed);
    }

    public override void _Toggled(bool toggledOn)
    {
        Variant terrainType = Variant.From(terrain);
        EmitTerrainSwitchedEvent(terrainType, toggledOn);
    }
}

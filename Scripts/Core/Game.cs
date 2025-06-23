using Godot;
using CCommand.CCDatabase;

namespace CCommandCore;

public partial class Game() : Node3D
{
    DatabaseProvider CCDb = new();
}
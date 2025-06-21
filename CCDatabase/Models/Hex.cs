using System;

namespace Database.Models;

public class Hex
{
    public int HexId { get; set; }
    public int NationId { get; set; }
    public Guid HexSceneGuid { get; set; }
    public int HexGridDataId { get; set; }
}

using System.Collections.Generic;

namespace Database.Models;

public class Nation
{
    public int NationId { get; set; }
    public int NationControlStateId { get; set; }
    public List<int> HexIds;
    public string Name { get; set; }
}

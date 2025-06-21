using System.Collections.Generic;

namespace Database.Models;

public class Player
{
    public int PlayerId { get; set; }
    public int PlayableFactionId { get; set; }
    public List<int> NationControlStateIds { get; set; }
}

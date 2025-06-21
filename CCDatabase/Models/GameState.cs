using System.Collections.Generic;

namespace Database.Models;

public class GameState
{
    public int GameId { get; set; }
    public int ActiveWorldId { get; set; }
    public int PlayerCount { get; set; }
    public List<int> PlayerIds { get; set; }
}

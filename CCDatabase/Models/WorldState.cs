using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;

namespace Database.Models;

public class WorldState
{
    public int WorldStateId { get; set; }
    public int WorldId { get; set; }
}

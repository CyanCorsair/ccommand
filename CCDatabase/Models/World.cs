using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;

namespace Database.Models;

public class World
{
    public int WorldId { get; set; }
    public int WorldStateId { get; set; }
    public List<int> NationIds { get; set; }
}

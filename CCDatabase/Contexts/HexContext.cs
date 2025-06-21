using Database.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;

namespace Database.Contexts;

public class HexContext : DbContext
{
    public DbSet<Hex> Hexes { get; set; }
    public DbSet<Nation> Nations { get; set; }

    public string DbPath { get; }

    public int WorldId { get; set; }

    public HexContext(int worldId)
    {
        var folder = Environment.SpecialFolder.LocalApplicationData;
        var path = Environment.GetFolderPath(folder);
        DbPath = System.IO.Path.Join(path, "hexes.db");
        WorldId = worldId;
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSqlite($"Data Source={DbPath}");
    }
}

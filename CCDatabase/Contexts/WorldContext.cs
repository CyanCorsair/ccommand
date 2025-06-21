using Database.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;

namespace Database.Contexts;

public class WorldContext : DbContext
{
    public DbSet<Player> Players { get; set; }
    public DbSet<PlayerState> PlayerStates { get; set; }
    public DbSet<PlayableFaction> PlayableFactions { get; set; }
    public DbSet<World> Worlds { get; set; }
    public DbSet<WorldState> WorldStates { get; set; }

    public string DbPath { get; }

    public WorldContext()
    {
        var folder = Environment.SpecialFolder.LocalApplicationData;
        var path = Environment.GetFolderPath(folder);
        DbPath = System.IO.Path.Join(path, "worlds.db");
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSqlite($"Data Source={DbPath}");
    }
}

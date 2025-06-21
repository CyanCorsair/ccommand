using Microsoft.EntityFrameworkCore;
using System;
using Database.Models;

namespace Database.Contexts;

public class GameContext : DbContext
{
    public DbSet<GameState> GameState;
    public DbSet<Player> Players;
    public DbSet<HexGridData> HexGridData;

    public string DbPath { get; }

    public GameContext()
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

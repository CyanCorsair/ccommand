using Godot;
using CCommand.CCDatabase;
using System.Threading.Tasks;
using CCommand.CCDatabase.Models;
using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;

namespace CCommandCore
{
    public partial class Game() : Node3D
    {
        DatabaseProvider _databaseProvider = new();

        private GameState _gameState;

        public GameState GameState
        {
            get => _gameState;
        }

        public override async void _Ready()
        {
            DatabaseProvider.Configure(OS.GetUserDataDir());
            _databaseProvider = DatabaseProvider.Instance;

            try
            {
                await _databaseProvider.Database.MigrateAsync();
                await AddTestDataEntriesAsync();
                await RunDatabaseTests();
            }
            catch (Exception exception)
            {
                GD.PrintErr(exception);
                throw;
            }
        }

        // TODO: Move to CCDatabase tests
        private async Task AddTestDataEntriesAsync()
        {
            SaveGameManifest SaveGameManifest = new SaveGameManifest("Test Save Game");
            await DatabaseProvider.Instance.SaveGameManifests.AddAsync(SaveGameManifest);
            await DatabaseProvider.Instance.SaveChangesAsync();
        }

        // TODO: Move to CCDatabase tests
        private async Task RunDatabaseTests()
        {
            Console.WriteLine("Fetching SaveGameManifests...");

            DbSet<SaveGameManifest> saveGameManifests = DatabaseProvider.Instance.SaveGameManifests;

            Console.WriteLine("Results:");
            await foreach (SaveGameManifest manifest in saveGameManifests.AsAsyncEnumerable())
            {
                Console.WriteLine($"- {manifest.SaveName} (Last Saved: {manifest.LastSaved})");
            }

            Console.WriteLine($"Results count: {saveGameManifests.Count()}");
        }
    }
}
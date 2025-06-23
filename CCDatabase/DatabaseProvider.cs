using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using CCommand.CCDatabase.Models;
using System.IO;

namespace CCommand.CCDatabase
{
    public sealed class DatabaseProvider : DbContext
    {
        private static DatabaseProvider _connectionInstance;
        private static bool _isConfgured = false;

        public static void Configure(string userFolder)
        {
            if (_connectionInstance == null)
            {
                _connectionInstance = new DatabaseProvider(userFolder);
                _isConfgured = true;
            }
        }

        public static DatabaseProvider Instance
        {
            get
            {
                if (!_isConfgured) throw new InvalidOperationException("DatabaseProvider is not configured. Call Configure() first.");
                return _connectionInstance;
            }
        }

        private string _userFolder;
        private string _databaseFolder = "database";
        private string _coreDbName = "CCommandCore.db";
        private string _fullDatabaseFilePath;

        public DbSet<SaveGameManifest> SaveGameManifests { get; set; }

        public DatabaseProvider() { }

        public DatabaseProvider(string userFolder)
        {
            _userFolder = userFolder;
            GeneratePaths();
            TryCreateDatabaseFile(_fullDatabaseFilePath);
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite($"Data Source={_fullDatabaseFilePath}");
        }

        private void GeneratePaths()
        {
            _fullDatabaseFilePath = Path.Combine(_userFolder, _databaseFolder, _coreDbName);
        }

        private static void TryCreateDatabaseFile(string fullDatabaseFilePath)
        {
            bool databaseExists = File.Exists(fullDatabaseFilePath);
            if (databaseExists) return;

            try
            {
                string fullPath = Path.GetDirectoryName(fullDatabaseFilePath);
                Directory.CreateDirectory(fullPath ?? throw new InvalidOperationException());
                File.Create(fullDatabaseFilePath).Dispose();
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to create database file at {fullDatabaseFilePath}.", ex);
            }

            if (!File.Exists(fullDatabaseFilePath))
            {
                throw new InvalidOperationException($"Database file was not created at {fullDatabaseFilePath}.");
            }
        }
    }
}

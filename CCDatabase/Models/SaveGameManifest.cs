using System;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace CCommand.CCDatabase.Models
{
    [Table(nameof(SaveGameManifest))]
    [PrimaryKey(nameof(DatabaseId))]
    public sealed class SaveGameManifest
    {
        public Guid DatabaseId { get; set; } = Guid.NewGuid();
        public string SaveName { get; set; }
        public DateTime LastSaved { get; set; }

        public SaveGameManifest() { }

        public SaveGameManifest(string saveName)
        {
            SaveName = saveName;
            LastSaved = DateTime.UtcNow;
        }
    }
}

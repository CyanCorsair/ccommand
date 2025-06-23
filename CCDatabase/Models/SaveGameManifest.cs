using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace CCommand.CCDatabase.Models
{
    [Table("SaveGameManifest")]
    [PrimaryKey(nameof(DatabaseId))]
    public sealed class SaveGameManifest
    {
        [Column(nameof(DatabaseId))]
        public Guid DatabaseId { get; set; }

        [Column(nameof(SaveName))]
        public string SaveName { get; set; } = string.Empty;

        [Column(nameof(LastSaved))]
        public DateTime LastSaved { get; set; }

        public SaveGameManifest() { }

        public SaveGameManifest(Guid databaseId, string saveName)
        {
            DatabaseId = databaseId;
            SaveName = saveName;
        }
    }
}

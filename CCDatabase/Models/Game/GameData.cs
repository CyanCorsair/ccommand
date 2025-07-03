using Microsoft.EntityFrameworkCore;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CCommand.CCDatabase.Models.Game
{
    [Table(nameof(GameData))]
    [PrimaryKey(nameof(GameDataId))]
    public sealed class GameData
    {
        [Key]
        public Guid GameDataId { get; set; } = Guid.NewGuid();

        [ForeignKey(nameof(GameState))]
        public Guid GameStateId { get; set; }

        [ForeignKey(nameof(SaveGameManifest))]
        public Guid LastSavedGameManifestId { get; set; }
    }
}

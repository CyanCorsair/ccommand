using Microsoft.EntityFrameworkCore;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CCommand.CCDatabase.Models
{
    [Table(nameof(Player))]
    [PrimaryKey(nameof(PlayerId))]
    public class Player
    {
        [Key]
        public Guid PlayerId { get; set; } = Guid.NewGuid();
        public string PlayerName { get; set; }

        [ForeignKey(nameof(PlayerState))]
        public Guid PlayerStateId { get; set; }
    }
}

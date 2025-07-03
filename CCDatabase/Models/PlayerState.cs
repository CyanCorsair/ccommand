using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CCommand.CCDatabase.Models
{
    [Table(nameof(PlayerState))]
    [PrimaryKey(nameof(PlayerStateId))]
    public class PlayerState
    {
        [Key]
        public Guid PlayerStateId { get; set; } = Guid.NewGuid();
    }
}

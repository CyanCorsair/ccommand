using System.Collections.Generic;

namespace Database.Models;

public class NationControlState
{
    public int NationControlStateId { get; set; }
    public int NationId { get; set; }
    public int PlayerId { get; set; }
}

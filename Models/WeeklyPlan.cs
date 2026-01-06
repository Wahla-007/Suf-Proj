using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace mess_management.Models;

public class WeeklyPlan
{
    [Key]
    public int Id { get; set; }

    public DateTime WeekStart { get; set; }

    public string? CreatedById { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.Now;

    public virtual List<WeeklyPlanDay> Days { get; set; } = new();

    public virtual AspNetUser? CreatedBy { get; set; }
}

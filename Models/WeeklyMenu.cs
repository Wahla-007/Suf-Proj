using System;
using System.Collections.Generic;

namespace mess_management.Models;

public partial class WeeklyMenu
{
    public int Id { get; set; }

    public DateTime? WeekStartDate { get; set; }

    public decimal? BreakfastRate { get; set; }

    public decimal? LunchRate { get; set; }

    public decimal? DinnerRate { get; set; }

    public string? CreatedById { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual AspNetUser? CreatedBy { get; set; }
}

using System;
using System.ComponentModel.DataAnnotations;

namespace mess_management.Models;

public class WeeklyPlanDay
{
    [Key]
    public int Id { get; set; }

    public int WeeklyPlanId { get; set; }

    // 0 = Sunday .. 6 = Saturday (match DateTime.DayOfWeek)
    public int DayOfWeek { get; set; }

    // Editable meal names
    public string BreakfastName { get; set; } = "Breakfast";
    public string LunchName { get; set; } = "Lunch";
    public string DinnerName { get; set; } = "Dinner";

    public decimal BreakfastPrice { get; set; }
    public decimal LunchPrice { get; set; }
    public decimal DinnerPrice { get; set; }

    public virtual WeeklyPlan? WeeklyPlan { get; set; }
}
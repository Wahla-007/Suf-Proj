using System;
using System.Collections.Generic;

namespace mess_management.Models;

public partial class MonthlyBill
{
    public int Id { get; set; }

    public string? TeacherId { get; set; }

    public int? Year { get; set; }

    public int? Month { get; set; }

    public int? TotalMeals { get; set; }

    public decimal? FoodAmount { get; set; }

    public decimal? WaterShare { get; set; }

    public decimal? PreviousDue { get; set; }

    public decimal? TotalDue { get; set; }

    public decimal? PaidAmount { get; set; }

    public string? Status { get; set; }

    public DateTime? GeneratedOn { get; set; }

    public DateTime? PaidOn { get; set; }

    public string? PaymentToken { get; set; }

    public virtual AspNetUser? Teacher { get; set; }
}

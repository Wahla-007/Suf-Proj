using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace mess_management.Models;

public class Bill
{
    [Key]
    public int Id { get; set; }

    public string? TeacherId { get; set; }

    public int Year { get; set; }
    public int Month { get; set; }

    public decimal TotalMealsAmount { get; set; }
    public decimal WaterFee { get; set; }
    public decimal PreviousDue { get; set; }
    public decimal TotalDue { get; set; }
    public decimal PaidAmount { get; set; }

    public string? Status { get; set; } = "Pending";

    public DateTime GeneratedOn { get; set; } = DateTime.Now;
    public DateTime? PaidOn { get; set; }

    public virtual List<BillLine> Lines { get; set; } = new();
    public virtual List<Payment> Payments { get; set; } = new();

    public virtual AspNetUser? Teacher { get; set; }
}
using System;
using System.ComponentModel.DataAnnotations;

namespace mess_management.Models;

public class BillLine
{
    [Key]
    public int Id { get; set; }

    public int BillId { get; set; }
    public DateTime Date { get; set; }

    // MealType: Breakfast/Lunch/Dinner
    public string? MealType { get; set; }
    public decimal Price { get; set; }
    public bool IsVerified { get; set; } = true; // default assume valid

    public int? ReviewRequestId { get; set; }

    public virtual Bill? Bill { get; set; }
    public virtual ReviewRequest? ReviewRequest { get; set; }
}
using System;
using System.ComponentModel.DataAnnotations;

namespace mess_management.Models;

public class ReviewRequest
{
    [Key]
    public int Id { get; set; }

    public int BillLineId { get; set; }
    public string? UserId { get; set; }
    public string? Reason { get; set; }
    public string? Status { get; set; } = "Open"; // Open / Approved / Rejected
    public DateTime CreatedAt { get; set; } = DateTime.Now;

    public virtual BillLine? BillLine { get; set; }
}
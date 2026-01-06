using System;
using System.Collections.Generic;

namespace mess_management.Models;

public partial class TeacherAttendance
{
    public int Id { get; set; }

    public string? TeacherId { get; set; }

    public DateOnly? Date { get; set; }

    public bool Breakfast { get; set; }
    public bool Lunch { get; set; }
    public bool Dinner { get; set; }

    public string? MarkedBy { get; set; }

    public bool? IsVerified { get; set; }
    public string? VerificationNote { get; set; }
    public DateTime? VerifiedAt { get; set; }

    // Dispute Features
    public string? DisputeStatus { get; set; } = "None"; // None, Raised, Approved, Rejected
    public string? DisputeReason { get; set; }

    public virtual AspNetUser? Teacher { get; set; }
}

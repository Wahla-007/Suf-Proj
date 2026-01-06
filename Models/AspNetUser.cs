using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace mess_management.Models;

public partial class AspNetUser
{
    public string Id { get; set; } = null!;

    [Required, EmailAddress, MaxLength(256)]
    public string? Email { get; set; }

    [MaxLength(256)]
    public string? PasswordHash { get; set; }

    public bool? IsAdmin { get; set; }

    public string? FullName { get; set; }

    public DateTime? JoinedDate { get; set; }

    public bool? IsPasswordChanged { get; set; }

    public virtual ICollection<MonthlyBill> MonthlyBills { get; set; } = new List<MonthlyBill>();

    public virtual ICollection<TeacherAttendance> TeacherAttendances { get; set; } = new List<TeacherAttendance>();

    public virtual ICollection<WeeklyMenu> WeeklyMenus { get; set; } = new List<WeeklyMenu>();
}

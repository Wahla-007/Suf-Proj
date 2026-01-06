using System.Collections.Generic;
using mess_management.Models;

namespace mess_management.Models
{
    public class TeacherDashboardViewModel
    {
        public AspNetUser Teacher { get; set; } = null!;
        public List<TeacherAttendance> AttendanceRecords { get; set; } = new();
        public WeeklyPlan? CurrentPlan { get; set; }
        public List<MonthlyBill> MonthlyBills { get; set; } = new();
        public int TotalAttendance { get; set; }
        public int VerifiedRecords { get; set; }
        public int PendingRecords { get; set; }
        public decimal ActiveCharges { get; set; }
    }
}

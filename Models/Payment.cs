using System;
using System.ComponentModel.DataAnnotations;

namespace mess_management.Models;

public class Payment
{
    [Key]
    public int Id { get; set; }

    public int BillId { get; set; }
    public decimal Amount { get; set; }
    public string? Method { get; set; } // e.g. 'Simulated' or 'Stripe'
    public string? TransactionId { get; set; }
    public DateTime PaidOn { get; set; } = DateTime.Now;

    public virtual Bill? Bill { get; set; }
}
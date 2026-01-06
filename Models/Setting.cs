using System.ComponentModel.DataAnnotations;

namespace mess_management.Models;

public class Setting
{
    [Key]
    public string Key { get; set; } = null!;
    public string? Value { get; set; }
}
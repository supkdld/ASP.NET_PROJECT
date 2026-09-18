using System.ComponentModel.DataAnnotations;

namespace ProjectASPNET.DTOs;

public class PatchOrderDto
{
    [Range(1, int.MaxValue, ErrorMessage = "UserId должен быть положительным числом.")]
    public int? UserId { get; set; }

    [StringLength(50, MinimumLength = 1, ErrorMessage = "Status должен содержать от 1 до 50 символов.")]
    public string? Status { get; set; }

    [Range(0, 9999999999.99, ErrorMessage = "TotalAmount должен быть в диапазоне от 0 до 9999999999.99.")]
    public decimal? TotalAmount { get; set; }
}

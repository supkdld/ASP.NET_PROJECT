using System.ComponentModel.DataAnnotations;

namespace ProjectASPNET.DTOs;

public class UpdateOrderDto
{
    [Range(1, int.MaxValue, ErrorMessage = "UserId должен быть положительным числом.")]
    public required int UserId { get; set; }

    [Required(ErrorMessage = "Status обязателен.")]
    [StringLength(50, ErrorMessage = "Status не может быть длиннее 50 символов.")]
    public required string Status { get; set; }

    [Range(0, 9999999999.99, ErrorMessage = "TotalAmount должен быть в диапазоне от 0 до 9999999999.99.")]
    public required decimal TotalAmount { get; set; }
}

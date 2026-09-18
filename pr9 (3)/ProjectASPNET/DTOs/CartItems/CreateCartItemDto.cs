using System.ComponentModel.DataAnnotations;

namespace ProjectASPNET.DTOs;

public class CreateCartItemDto
{
    [Range(1, int.MaxValue, ErrorMessage = "CartId должен быть положительным числом.")]
    public required int CartId { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "ProductId должен быть положительным числом.")]
    public required int ProductId { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Quantity должно быть не меньше 1.")]
    public required int Quantity { get; set; }
}

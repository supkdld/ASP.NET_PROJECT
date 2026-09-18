using System.ComponentModel.DataAnnotations;

namespace ProjectASPNET.DTOs;

public class CreateOrderItemDto
{
    [Range(1, int.MaxValue, ErrorMessage = "OrderId должен быть положительным числом.")]
    public required int OrderId { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "ProductId должен быть положительным числом.")]
    public required int ProductId { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Quantity должно быть не меньше 1.")]
    public required int Quantity { get; set; }

    [Range(0, 99999999.99, ErrorMessage = "PriceAtOrder должна быть в диапазоне от 0 до 99999999.99.")]
    public required decimal PriceAtOrder { get; set; }
}

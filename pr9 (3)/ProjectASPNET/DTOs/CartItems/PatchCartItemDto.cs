using System.ComponentModel.DataAnnotations;

namespace ProjectASPNET.DTOs;

public class PatchCartItemDto
{
    [Range(1, int.MaxValue, ErrorMessage = "CartId должен быть положительным числом.")]
    public int? CartId { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "ProductId должен быть положительным числом.")]
    public int? ProductId { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Quantity должно быть не меньше 1.")]
    public int? Quantity { get; set; }
}

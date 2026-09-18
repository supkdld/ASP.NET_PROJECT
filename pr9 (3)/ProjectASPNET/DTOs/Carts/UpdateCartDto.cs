using System.ComponentModel.DataAnnotations;

namespace ProjectASPNET.DTOs;

public class UpdateCartDto
{
    [Range(1, int.MaxValue, ErrorMessage = "UserId должен быть положительным числом.")]
    public required int UserId { get; set; }
}

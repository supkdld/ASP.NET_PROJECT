using System.ComponentModel.DataAnnotations;

namespace ProjectASPNET.DTOs;

public class PatchCartDto
{
    [Range(1, int.MaxValue, ErrorMessage = "UserId должен быть положительным числом.")]
    public int? UserId { get; set; }
}

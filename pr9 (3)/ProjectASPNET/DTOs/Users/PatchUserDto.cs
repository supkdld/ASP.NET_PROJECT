using System.ComponentModel.DataAnnotations;

namespace ProjectASPNET.DTOs;

public class PatchUserDto
{
    [StringLength(50, MinimumLength = 1, ErrorMessage = "Login должен содержать от 1 до 50 символов.")]
    public string? Login { get; set; }

    [StringLength(255, ErrorMessage = "PasswordUser не может быть длиннее 255 символов.")]
    public string? PasswordUser { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "RoleId должен быть положительным числом.")]
    public int? RoleId { get; set; }
}

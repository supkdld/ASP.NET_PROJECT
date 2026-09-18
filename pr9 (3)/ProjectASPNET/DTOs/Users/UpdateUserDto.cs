using System.ComponentModel.DataAnnotations;

namespace ProjectASPNET.DTOs;

public class UpdateUserDto
{
    [Required(ErrorMessage = "Login обязателен.")]
    [StringLength(50, ErrorMessage = "Login не может быть длиннее 50 символов.")]
    public required string Login { get; set; }

    [StringLength(255, ErrorMessage = "PasswordUser не может быть длиннее 255 символов.")]
    public required string? PasswordUser { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "RoleId должен быть положительным числом.")]
    public required int RoleId { get; set; }
}

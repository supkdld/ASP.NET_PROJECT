using System.ComponentModel.DataAnnotations;

namespace ProjectASPNET.DTOs;

public class PatchRoleDto
{
    [StringLength(50, MinimumLength = 1, ErrorMessage = "NameOfRole должно содержать от 1 до 50 символов.")]
    public string? NameOfRole { get; set; }
}

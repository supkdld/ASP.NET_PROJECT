using System.ComponentModel.DataAnnotations;

namespace ProjectASPNET.DTOs;

public class CreateRoleDto
{
    [Required(ErrorMessage = "NameOfRole обязательно.")]
    [StringLength(50, ErrorMessage = "NameOfRole не может быть длиннее 50 символов.")]
    public required string NameOfRole { get; set; }
}

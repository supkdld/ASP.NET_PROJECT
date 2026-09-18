using System.ComponentModel.DataAnnotations;

namespace ProjectASPNET.DTOs;

public class PatchProductCategoryDto
{
    [StringLength(50, MinimumLength = 1, ErrorMessage = "NameOfCategory должно содержать от 1 до 50 символов.")]
    public string? NameOfCategory { get; set; }
}

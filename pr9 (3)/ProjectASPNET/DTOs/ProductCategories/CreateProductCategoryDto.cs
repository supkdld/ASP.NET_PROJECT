using System.ComponentModel.DataAnnotations;

namespace ProjectASPNET.DTOs;

public class CreateProductCategoryDto
{
    [Required(ErrorMessage = "NameOfCategory обязательно.")]
    [StringLength(50, ErrorMessage = "NameOfCategory не может быть длиннее 50 символов.")]
    public required string NameOfCategory { get; set; }
}

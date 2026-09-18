using System.ComponentModel.DataAnnotations;

namespace ProjectASPNET.DTOs;

public class UpdateProductDto
{
    [Required(ErrorMessage = "NameOfProduct обязательно.")]
    [StringLength(50, ErrorMessage = "NameOfProduct не может быть длиннее 50 символов.")]
    public required string NameOfProduct { get; set; }

    public required string? DescriptionProduct { get; set; }

    [Range(0, 99999999.99, ErrorMessage = "Price должна быть в диапазоне от 0 до 99999999.99.")]
    public required decimal Price { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "CategoryId должен быть положительным числом.")]
    public required int CategoryId { get; set; }

    [Range(0, int.MaxValue, ErrorMessage = "InStock не может быть отрицательным.")]
    public required int InStock { get; set; }

    public required bool? IsActive { get; set; }

    [StringLength(255, ErrorMessage = "ImagePath не может быть длиннее 255 символов.")]
    public required string? ImagePath { get; set; }

    [StringLength(500, ErrorMessage = "ImageUrl не может быть длиннее 500 символов.")]
    public required string? ImageUrl { get; set; }
}

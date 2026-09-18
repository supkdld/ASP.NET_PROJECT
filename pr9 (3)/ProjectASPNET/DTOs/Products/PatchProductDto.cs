using System.ComponentModel.DataAnnotations;

namespace ProjectASPNET.DTOs;

public class PatchProductDto
{
    [StringLength(50, MinimumLength = 1, ErrorMessage = "NameOfProduct должно содержать от 1 до 50 символов.")]
    public string? NameOfProduct { get; set; }

    public string? DescriptionProduct { get; set; }

    [Range(0, 99999999.99, ErrorMessage = "Price должна быть в диапазоне от 0 до 99999999.99.")]
    public decimal? Price { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "CategoryId должен быть положительным числом.")]
    public int? CategoryId { get; set; }

    [Range(0, int.MaxValue, ErrorMessage = "InStock не может быть отрицательным.")]
    public int? InStock { get; set; }

    public bool? IsActive { get; set; }

    [StringLength(255, ErrorMessage = "ImagePath не может быть длиннее 255 символов.")]
    public string? ImagePath { get; set; }

    [StringLength(500, ErrorMessage = "ImageUrl не может быть длиннее 500 символов.")]
    public string? ImageUrl { get; set; }
}

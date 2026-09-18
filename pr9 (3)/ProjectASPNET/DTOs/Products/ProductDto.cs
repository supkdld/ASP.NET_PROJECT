namespace ProjectASPNET.DTOs;

public class ProductDto
{
    public int Id { get; set; }

    public string NameOfProduct { get; set; } = string.Empty;

    public string? DescriptionProduct { get; set; }

    public decimal Price { get; set; }

    public int CategoryId { get; set; }

    public int InStock { get; set; }

    public bool? IsActive { get; set; }

    public string? ImagePath { get; set; }

    public string? ImageUrl { get; set; }
}

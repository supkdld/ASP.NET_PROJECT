using ProjectASPNET.DTOs;
using ProjectASPNET.Models;

namespace ProjectASPNET.Mappers;

public static class ProductMapper
{
    public static ProductDto ToDto(Product product)
    {
        return new ProductDto
        {
            Id = product.IdProduct,
            NameOfProduct = product.NameOfProduct,
            DescriptionProduct = product.DescriptionProduct,
            Price = product.Price,
            CategoryId = product.CategoryId,
            InStock = product.InStock,
            IsActive = product.IsActive,
            ImagePath = product.ImagePath,
            ImageUrl = product.ImageUrl
        };
    }
}

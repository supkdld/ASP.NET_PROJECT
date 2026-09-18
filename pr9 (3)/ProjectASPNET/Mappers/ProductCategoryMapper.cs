using ProjectASPNET.DTOs;
using ProjectASPNET.Models;

namespace ProjectASPNET.Mappers;

public static class ProductCategoryMapper
{
    public static ProductCategoryDto ToDto(ProductCategory category)
    {
        return new ProductCategoryDto
        {
            Id = category.IdCategory,
            NameOfCategory = category.NameOfCategory
        };
    }
}

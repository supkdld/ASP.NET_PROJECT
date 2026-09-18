using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProjectASPNET.DTOs;
using ProjectASPNET.Mappers;
using ProjectASPNET.Models;

namespace ProjectASPNET.Controllers;

[ApiController]
[Route("api/product-categories")]
[Produces("application/json")]
public class ProductCategoriesController : ApiControllerBase
{
    private readonly PharmacyContext _context;

    public ProductCategoriesController(PharmacyContext context)
    {
        _context = context;
    }

    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<ProductCategoryDto>), StatusCodes.Status200OK)]
    public ActionResult<IEnumerable<ProductCategoryDto>> GetAll([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        var query = _context.ProductCategories
            .AsNoTracking()
            .OrderBy(c => c.IdCategory);

        return Ok(GetPage(query, page, pageSize).Select(ProductCategoryMapper.ToDto).ToList());
    }

    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(ProductCategoryDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public ActionResult<ProductCategoryDto> GetById(int id)
    {
        var category = _context.ProductCategories
            .AsNoTracking()
            .FirstOrDefault(c => c.IdCategory == id);

        if (category == null)
        {
            return NotFound();
        }

        return Ok(ProductCategoryMapper.ToDto(category));
    }

    [HttpPost]
    [ProducesResponseType(typeof(ProductCategoryDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public ActionResult<ProductCategoryDto> Create(CreateProductCategoryDto dto)
    {
        if (NameExists(dto.NameOfCategory))
        {
            return ConflictError($"Категория '{dto.NameOfCategory}' уже существует.");
        }

        var category = new ProductCategory
        {
            NameOfCategory = dto.NameOfCategory
        };

        _context.ProductCategories.Add(category);
        _context.SaveChanges();

        var result = ProductCategoryMapper.ToDto(category);

        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpPut("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public IActionResult Update(int id, UpdateProductCategoryDto dto)
    {
        var category = _context.ProductCategories.Find(id);

        if (category == null)
        {
            return NotFound();
        }

        if (NameExists(dto.NameOfCategory, id))
        {
            return ConflictError($"Категория '{dto.NameOfCategory}' уже существует.");
        }

        category.NameOfCategory = dto.NameOfCategory;

        _context.SaveChanges();

        return NoContent();
    }

    [HttpPatch("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public IActionResult Patch(int id, PatchProductCategoryDto dto)
    {
        var category = _context.ProductCategories.Find(id);

        if (category == null)
        {
            return NotFound();
        }

        if (dto.NameOfCategory != null)
        {
            if (NameExists(dto.NameOfCategory, id))
            {
                return ConflictError($"Категория '{dto.NameOfCategory}' уже существует.");
            }

            category.NameOfCategory = dto.NameOfCategory;
        }

        _context.SaveChanges();

        return NoContent();
    }

    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public IActionResult Delete(int id)
    {
        var category = _context.ProductCategories.Find(id);

        if (category == null)
        {
            return NotFound();
        }

        if (_context.Products.Any(p => p.CategoryId == id))
        {
            return ConflictError("Нельзя удалить категорию, в которой есть товары.");
        }

        _context.ProductCategories.Remove(category);
        _context.SaveChanges();

        return NoContent();
    }

    private bool NameExists(string name, int? excludeId = null)
    {
        return _context.ProductCategories
            .Any(c => c.NameOfCategory == name && (excludeId == null || c.IdCategory != excludeId.Value));
    }
}

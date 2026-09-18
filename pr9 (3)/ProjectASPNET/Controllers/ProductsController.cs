using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProjectASPNET.DTOs;
using ProjectASPNET.Mappers;
using ProjectASPNET.Models;

namespace ProjectASPNET.Controllers;

[ApiController]
[Route("api/products")]
[Produces("application/json")]
public class ProductsController : ApiControllerBase
{
    private readonly PharmacyContext _context;

    public ProductsController(PharmacyContext context)
    {
        _context = context;
    }

    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<ProductDto>), StatusCodes.Status200OK)]
    public ActionResult<IEnumerable<ProductDto>> GetAll([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        var query = _context.Products
            .AsNoTracking()
            .OrderBy(p => p.IdProduct);

        return Ok(GetPage(query, page, pageSize).Select(ProductMapper.ToDto).ToList());
    }

    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(ProductDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public ActionResult<ProductDto> GetById(int id)
    {
        var product = _context.Products
            .AsNoTracking()
            .FirstOrDefault(p => p.IdProduct == id);

        if (product == null)
        {
            return NotFound();
        }

        return Ok(ProductMapper.ToDto(product));
    }

    [HttpPost]
    [ProducesResponseType(typeof(ProductDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public ActionResult<ProductDto> Create(CreateProductDto dto)
    {
        if (!CategoryExists(dto.CategoryId))
        {
            return FieldError(nameof(dto.CategoryId), $"Категория с Id = {dto.CategoryId} не найдена.");
        }

        var product = new Product
        {
            NameOfProduct = dto.NameOfProduct,
            DescriptionProduct = dto.DescriptionProduct,
            Price = dto.Price,
            CategoryId = dto.CategoryId,
            InStock = dto.InStock,
            IsActive = dto.IsActive,
            ImagePath = dto.ImagePath,
            ImageUrl = dto.ImageUrl
        };

        _context.Products.Add(product);
        _context.SaveChanges();

        var result = ProductMapper.ToDto(product);

        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpPut("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult Update(int id, UpdateProductDto dto)
    {
        var product = _context.Products.Find(id);

        if (product == null)
        {
            return NotFound();
        }

        if (!CategoryExists(dto.CategoryId))
        {
            return FieldError(nameof(dto.CategoryId), $"Категория с Id = {dto.CategoryId} не найдена.");
        }

        product.NameOfProduct = dto.NameOfProduct;
        product.DescriptionProduct = dto.DescriptionProduct;
        product.Price = dto.Price;
        product.CategoryId = dto.CategoryId;
        product.InStock = dto.InStock;
        product.IsActive = dto.IsActive;
        product.ImagePath = dto.ImagePath;
        product.ImageUrl = dto.ImageUrl;

        _context.SaveChanges();

        return NoContent();
    }

    [HttpPatch("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult Patch(int id, PatchProductDto dto)
    {
        var product = _context.Products.Find(id);

        if (product == null)
        {
            return NotFound();
        }

        if (dto.CategoryId.HasValue)
        {
            if (!CategoryExists(dto.CategoryId.Value))
            {
                return FieldError(nameof(dto.CategoryId), $"Категория с Id = {dto.CategoryId.Value} не найдена.");
            }

            product.CategoryId = dto.CategoryId.Value;
        }

        if (dto.NameOfProduct != null)
        {
            product.NameOfProduct = dto.NameOfProduct;
        }

        if (dto.DescriptionProduct != null)
        {
            product.DescriptionProduct = dto.DescriptionProduct;
        }

        if (dto.Price.HasValue)
        {
            product.Price = dto.Price.Value;
        }

        if (dto.InStock.HasValue)
        {
            product.InStock = dto.InStock.Value;
        }

        if (dto.IsActive.HasValue)
        {
            product.IsActive = dto.IsActive.Value;
        }

        if (dto.ImagePath != null)
        {
            product.ImagePath = dto.ImagePath;
        }

        if (dto.ImageUrl != null)
        {
            product.ImageUrl = dto.ImageUrl;
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
        var product = _context.Products.Find(id);

        if (product == null)
        {
            return NotFound();
        }

        if (_context.CartItems.Any(ci => ci.ProductId == id) || _context.OrderItems.Any(oi => oi.ProductId == id))
        {
            return ConflictError("Нельзя удалить товар, который есть в корзинах или заказах.");
        }

        _context.Products.Remove(product);
        _context.SaveChanges();

        return NoContent();
    }

    private bool CategoryExists(int categoryId)
    {
        return _context.ProductCategories.Any(c => c.IdCategory == categoryId);
    }
}

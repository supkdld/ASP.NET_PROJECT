using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProjectASPNET.DTOs;
using ProjectASPNET.Mappers;
using ProjectASPNET.Models;

namespace ProjectASPNET.Controllers;

[ApiController]
[Route("api/cart-items")]
[Produces("application/json")]
public class CartItemsController : ApiControllerBase
{
    private readonly PharmacyContext _context;

    public CartItemsController(PharmacyContext context)
    {
        _context = context;
    }

    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<CartItemDto>), StatusCodes.Status200OK)]
    public ActionResult<IEnumerable<CartItemDto>> GetAll([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        var query = _context.CartItems
            .AsNoTracking()
            .OrderBy(ci => ci.IdCartItem);

        return Ok(GetPage(query, page, pageSize).Select(CartItemMapper.ToDto).ToList());
    }

    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(CartItemDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public ActionResult<CartItemDto> GetById(int id)
    {
        var cartItem = _context.CartItems
            .AsNoTracking()
            .FirstOrDefault(ci => ci.IdCartItem == id);

        if (cartItem == null)
        {
            return NotFound();
        }

        return Ok(CartItemMapper.ToDto(cartItem));
    }

    [HttpPost]
    [ProducesResponseType(typeof(CartItemDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public ActionResult<CartItemDto> Create(CreateCartItemDto dto)
    {
        if (!_context.Carts.Any(c => c.IdCart == dto.CartId))
        {
            return FieldError(nameof(dto.CartId), $"Корзина с Id = {dto.CartId} не найдена.");
        }

        if (!_context.Products.Any(p => p.IdProduct == dto.ProductId))
        {
            return FieldError(nameof(dto.ProductId), $"Товар с Id = {dto.ProductId} не найден.");
        }

        if (PairExists(dto.CartId, dto.ProductId))
        {
            return ConflictError("Этот товар уже есть в корзине.");
        }

        var cartItem = new CartItem
        {
            CartId = dto.CartId,
            ProductId = dto.ProductId,
            Quantity = dto.Quantity
        };

        _context.CartItems.Add(cartItem);
        _context.SaveChanges();

        var result = CartItemMapper.ToDto(cartItem);

        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpPut("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public IActionResult Update(int id, UpdateCartItemDto dto)
    {
        var cartItem = _context.CartItems.Find(id);

        if (cartItem == null)
        {
            return NotFound();
        }

        if (!_context.Carts.Any(c => c.IdCart == dto.CartId))
        {
            return FieldError(nameof(dto.CartId), $"Корзина с Id = {dto.CartId} не найдена.");
        }

        if (!_context.Products.Any(p => p.IdProduct == dto.ProductId))
        {
            return FieldError(nameof(dto.ProductId), $"Товар с Id = {dto.ProductId} не найден.");
        }

        if (PairExists(dto.CartId, dto.ProductId, id))
        {
            return ConflictError("Этот товар уже есть в корзине.");
        }

        cartItem.CartId = dto.CartId;
        cartItem.ProductId = dto.ProductId;
        cartItem.Quantity = dto.Quantity;

        _context.SaveChanges();

        return NoContent();
    }

    [HttpPatch("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public IActionResult Patch(int id, PatchCartItemDto dto)
    {
        var cartItem = _context.CartItems.Find(id);

        if (cartItem == null)
        {
            return NotFound();
        }

        if (dto.CartId.HasValue && !_context.Carts.Any(c => c.IdCart == dto.CartId.Value))
        {
            return FieldError(nameof(dto.CartId), $"Корзина с Id = {dto.CartId.Value} не найдена.");
        }

        if (dto.ProductId.HasValue && !_context.Products.Any(p => p.IdProduct == dto.ProductId.Value))
        {
            return FieldError(nameof(dto.ProductId), $"Товар с Id = {dto.ProductId.Value} не найден.");
        }

        var newCartId = dto.CartId ?? cartItem.CartId;
        var newProductId = dto.ProductId ?? cartItem.ProductId;

        if ((dto.CartId.HasValue || dto.ProductId.HasValue) && PairExists(newCartId, newProductId, id))
        {
            return ConflictError("Этот товар уже есть в корзине.");
        }

        cartItem.CartId = newCartId;
        cartItem.ProductId = newProductId;

        if (dto.Quantity.HasValue)
        {
            cartItem.Quantity = dto.Quantity.Value;
        }

        _context.SaveChanges();

        return NoContent();
    }

    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult Delete(int id)
    {
        var cartItem = _context.CartItems.Find(id);

        if (cartItem == null)
        {
            return NotFound();
        }

        _context.CartItems.Remove(cartItem);
        _context.SaveChanges();

        return NoContent();
    }

    private bool PairExists(int cartId, int productId, int? excludeId = null)
    {
        return _context.CartItems
            .Any(ci => ci.CartId == cartId
                && ci.ProductId == productId
                && (excludeId == null || ci.IdCartItem != excludeId.Value));
    }
}

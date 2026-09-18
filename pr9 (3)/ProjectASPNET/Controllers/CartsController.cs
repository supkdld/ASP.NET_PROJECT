using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProjectASPNET.DTOs;
using ProjectASPNET.Mappers;
using ProjectASPNET.Models;

namespace ProjectASPNET.Controllers;

[ApiController]
[Route("api/carts")]
[Produces("application/json")]
public class CartsController : ApiControllerBase
{
    private readonly PharmacyContext _context;

    public CartsController(PharmacyContext context)
    {
        _context = context;
    }

    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<CartDto>), StatusCodes.Status200OK)]
    public ActionResult<IEnumerable<CartDto>> GetAll([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        var query = _context.Carts
            .AsNoTracking()
            .OrderBy(c => c.IdCart);

        return Ok(GetPage(query, page, pageSize).Select(CartMapper.ToDto).ToList());
    }

    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(CartDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public ActionResult<CartDto> GetById(int id)
    {
        var cart = _context.Carts
            .AsNoTracking()
            .FirstOrDefault(c => c.IdCart == id);

        if (cart == null)
        {
            return NotFound();
        }

        return Ok(CartMapper.ToDto(cart));
    }

    [HttpPost]
    [ProducesResponseType(typeof(CartDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public ActionResult<CartDto> Create(CreateCartDto dto)
    {
        if (!UserExists(dto.UserId))
        {
            return FieldError(nameof(dto.UserId), $"Пользователь с Id = {dto.UserId} не найден.");
        }

        var cart = new Cart
        {
            UserId = dto.UserId,
            CreatedDate = DateTime.UtcNow
        };

        _context.Carts.Add(cart);
        _context.SaveChanges();

        var result = CartMapper.ToDto(cart);

        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpPut("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult Update(int id, UpdateCartDto dto)
    {
        var cart = _context.Carts.Find(id);

        if (cart == null)
        {
            return NotFound();
        }

        if (!UserExists(dto.UserId))
        {
            return FieldError(nameof(dto.UserId), $"Пользователь с Id = {dto.UserId} не найден.");
        }

        cart.UserId = dto.UserId;

        _context.SaveChanges();

        return NoContent();
    }

    [HttpPatch("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult Patch(int id, PatchCartDto dto)
    {
        var cart = _context.Carts.Find(id);

        if (cart == null)
        {
            return NotFound();
        }

        if (dto.UserId.HasValue)
        {
            if (!UserExists(dto.UserId.Value))
            {
                return FieldError(nameof(dto.UserId), $"Пользователь с Id = {dto.UserId.Value} не найден.");
            }

            cart.UserId = dto.UserId.Value;
        }

        _context.SaveChanges();

        return NoContent();
    }

    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult Delete(int id)
    {
        var cart = _context.Carts
            .Include(c => c.CartItems)
            .FirstOrDefault(c => c.IdCart == id);

        if (cart == null)
        {
            return NotFound();
        }

        _context.CartItems.RemoveRange(cart.CartItems);
        _context.Carts.Remove(cart);
        _context.SaveChanges();

        return NoContent();
    }

    private bool UserExists(int userId)
    {
        return _context.Users.Any(u => u.IdUser == userId);
    }
}

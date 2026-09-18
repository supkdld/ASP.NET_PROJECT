using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProjectASPNET.DTOs;
using ProjectASPNET.Mappers;
using ProjectASPNET.Models;

namespace ProjectASPNET.Controllers;

[ApiController]
[Route("api/orders")]
[Produces("application/json")]
public class OrdersController : ApiControllerBase
{
    private readonly PharmacyContext _context;

    public OrdersController(PharmacyContext context)
    {
        _context = context;
    }

    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<OrderDto>), StatusCodes.Status200OK)]
    public ActionResult<IEnumerable<OrderDto>> GetAll([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        var query = _context.Orders
            .AsNoTracking()
            .OrderBy(o => o.IdOrder);

        return Ok(GetPage(query, page, pageSize).Select(OrderMapper.ToDto).ToList());
    }

    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(OrderDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public ActionResult<OrderDto> GetById(int id)
    {
        var order = _context.Orders
            .AsNoTracking()
            .FirstOrDefault(o => o.IdOrder == id);

        if (order == null)
        {
            return NotFound();
        }

        return Ok(OrderMapper.ToDto(order));
    }

    [HttpPost]
    [ProducesResponseType(typeof(OrderDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public ActionResult<OrderDto> Create(CreateOrderDto dto)
    {
        if (!UserExists(dto.UserId))
        {
            return FieldError(nameof(dto.UserId), $"Пользователь с Id = {dto.UserId} не найден.");
        }

        var order = new Order
        {
            UserId = dto.UserId,
            Status = dto.Status,
            TotalAmount = dto.TotalAmount,
            OrderDate = DateTime.UtcNow
        };

        _context.Orders.Add(order);
        _context.SaveChanges();

        var result = OrderMapper.ToDto(order);

        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpPut("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult Update(int id, UpdateOrderDto dto)
    {
        var order = _context.Orders.Find(id);

        if (order == null)
        {
            return NotFound();
        }

        if (!UserExists(dto.UserId))
        {
            return FieldError(nameof(dto.UserId), $"Пользователь с Id = {dto.UserId} не найден.");
        }

        order.UserId = dto.UserId;
        order.Status = dto.Status;
        order.TotalAmount = dto.TotalAmount;

        _context.SaveChanges();

        return NoContent();
    }

    [HttpPatch("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult Patch(int id, PatchOrderDto dto)
    {
        var order = _context.Orders.Find(id);

        if (order == null)
        {
            return NotFound();
        }

        if (dto.UserId.HasValue)
        {
            if (!UserExists(dto.UserId.Value))
            {
                return FieldError(nameof(dto.UserId), $"Пользователь с Id = {dto.UserId.Value} не найден.");
            }

            order.UserId = dto.UserId.Value;
        }

        if (dto.Status != null)
        {
            order.Status = dto.Status;
        }

        if (dto.TotalAmount.HasValue)
        {
            order.TotalAmount = dto.TotalAmount.Value;
        }

        _context.SaveChanges();

        return NoContent();
    }

    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult Delete(int id)
    {
        var order = _context.Orders
            .Include(o => o.OrderItems)
            .FirstOrDefault(o => o.IdOrder == id);

        if (order == null)
        {
            return NotFound();
        }

        _context.OrderItems.RemoveRange(order.OrderItems);
        _context.Orders.Remove(order);
        _context.SaveChanges();

        return NoContent();
    }

    private bool UserExists(int userId)
    {
        return _context.Users.Any(u => u.IdUser == userId);
    }
}

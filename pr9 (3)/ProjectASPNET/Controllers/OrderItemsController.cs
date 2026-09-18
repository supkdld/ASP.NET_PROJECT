using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProjectASPNET.DTOs;
using ProjectASPNET.Mappers;
using ProjectASPNET.Models;

namespace ProjectASPNET.Controllers;

[ApiController]
[Route("api/order-items")]
[Produces("application/json")]
public class OrderItemsController : ApiControllerBase
{
    private readonly PharmacyContext _context;

    public OrderItemsController(PharmacyContext context)
    {
        _context = context;
    }

    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<OrderItemDto>), StatusCodes.Status200OK)]
    public ActionResult<IEnumerable<OrderItemDto>> GetAll([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        var query = _context.OrderItems
            .AsNoTracking()
            .OrderBy(oi => oi.IdOrderItem);

        return Ok(GetPage(query, page, pageSize).Select(OrderItemMapper.ToDto).ToList());
    }

    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(OrderItemDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public ActionResult<OrderItemDto> GetById(int id)
    {
        var orderItem = _context.OrderItems
            .AsNoTracking()
            .FirstOrDefault(oi => oi.IdOrderItem == id);

        if (orderItem == null)
        {
            return NotFound();
        }

        return Ok(OrderItemMapper.ToDto(orderItem));
    }

    [HttpPost]
    [ProducesResponseType(typeof(OrderItemDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public ActionResult<OrderItemDto> Create(CreateOrderItemDto dto)
    {
        if (!_context.Orders.Any(o => o.IdOrder == dto.OrderId))
        {
            return FieldError(nameof(dto.OrderId), $"Заказ с Id = {dto.OrderId} не найден.");
        }

        if (!_context.Products.Any(p => p.IdProduct == dto.ProductId))
        {
            return FieldError(nameof(dto.ProductId), $"Товар с Id = {dto.ProductId} не найден.");
        }

        if (PairExists(dto.OrderId, dto.ProductId))
        {
            return ConflictError("Этот товар уже есть в заказе.");
        }

        var orderItem = new OrderItem
        {
            OrderId = dto.OrderId,
            ProductId = dto.ProductId,
            Quantity = dto.Quantity,
            PriceAtOrder = dto.PriceAtOrder
        };

        _context.OrderItems.Add(orderItem);
        _context.SaveChanges();

        var result = OrderItemMapper.ToDto(orderItem);

        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpPut("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public IActionResult Update(int id, UpdateOrderItemDto dto)
    {
        var orderItem = _context.OrderItems.Find(id);

        if (orderItem == null)
        {
            return NotFound();
        }

        if (!_context.Orders.Any(o => o.IdOrder == dto.OrderId))
        {
            return FieldError(nameof(dto.OrderId), $"Заказ с Id = {dto.OrderId} не найден.");
        }

        if (!_context.Products.Any(p => p.IdProduct == dto.ProductId))
        {
            return FieldError(nameof(dto.ProductId), $"Товар с Id = {dto.ProductId} не найден.");
        }

        if (PairExists(dto.OrderId, dto.ProductId, id))
        {
            return ConflictError("Этот товар уже есть в заказе.");
        }

        orderItem.OrderId = dto.OrderId;
        orderItem.ProductId = dto.ProductId;
        orderItem.Quantity = dto.Quantity;
        orderItem.PriceAtOrder = dto.PriceAtOrder;

        _context.SaveChanges();

        return NoContent();
    }

    [HttpPatch("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public IActionResult Patch(int id, PatchOrderItemDto dto)
    {
        var orderItem = _context.OrderItems.Find(id);

        if (orderItem == null)
        {
            return NotFound();
        }

        if (dto.OrderId.HasValue && !_context.Orders.Any(o => o.IdOrder == dto.OrderId.Value))
        {
            return FieldError(nameof(dto.OrderId), $"Заказ с Id = {dto.OrderId.Value} не найден.");
        }

        if (dto.ProductId.HasValue && !_context.Products.Any(p => p.IdProduct == dto.ProductId.Value))
        {
            return FieldError(nameof(dto.ProductId), $"Товар с Id = {dto.ProductId.Value} не найден.");
        }

        var newOrderId = dto.OrderId ?? orderItem.OrderId;
        var newProductId = dto.ProductId ?? orderItem.ProductId;

        if ((dto.OrderId.HasValue || dto.ProductId.HasValue) && PairExists(newOrderId, newProductId, id))
        {
            return ConflictError("Этот товар уже есть в заказе.");
        }

        orderItem.OrderId = newOrderId;
        orderItem.ProductId = newProductId;

        if (dto.Quantity.HasValue)
        {
            orderItem.Quantity = dto.Quantity.Value;
        }

        if (dto.PriceAtOrder.HasValue)
        {
            orderItem.PriceAtOrder = dto.PriceAtOrder.Value;
        }

        _context.SaveChanges();

        return NoContent();
    }

    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult Delete(int id)
    {
        var orderItem = _context.OrderItems.Find(id);

        if (orderItem == null)
        {
            return NotFound();
        }

        _context.OrderItems.Remove(orderItem);
        _context.SaveChanges();

        return NoContent();
    }

    private bool PairExists(int orderId, int productId, int? excludeId = null)
    {
        return _context.OrderItems
            .Any(oi => oi.OrderId == orderId
                && oi.ProductId == productId
                && (excludeId == null || oi.IdOrderItem != excludeId.Value));
    }
}

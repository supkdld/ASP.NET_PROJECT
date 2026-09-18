using ProjectASPNET.DTOs;
using ProjectASPNET.Models;

namespace ProjectASPNET.Mappers;

public static class OrderItemMapper
{
    public static OrderItemDto ToDto(OrderItem orderItem)
    {
        return new OrderItemDto
        {
            Id = orderItem.IdOrderItem,
            OrderId = orderItem.OrderId,
            ProductId = orderItem.ProductId,
            Quantity = orderItem.Quantity,
            PriceAtOrder = orderItem.PriceAtOrder
        };
    }
}

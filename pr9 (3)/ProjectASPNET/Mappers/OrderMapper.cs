using ProjectASPNET.DTOs;
using ProjectASPNET.Models;

namespace ProjectASPNET.Mappers;

public static class OrderMapper
{
    public static OrderDto ToDto(Order order)
    {
        return new OrderDto
        {
            Id = order.IdOrder,
            UserId = order.UserId,
            Status = order.Status,
            TotalAmount = order.TotalAmount,
            CreatedAt = order.OrderDate
        };
    }
}

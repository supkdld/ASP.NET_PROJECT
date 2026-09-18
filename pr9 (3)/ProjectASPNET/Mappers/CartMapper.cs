using ProjectASPNET.DTOs;
using ProjectASPNET.Models;

namespace ProjectASPNET.Mappers;

public static class CartMapper
{
    public static CartDto ToDto(Cart cart)
    {
        return new CartDto
        {
            Id = cart.IdCart,
            UserId = cart.UserId,
            CreatedAt = cart.CreatedDate
        };
    }
}

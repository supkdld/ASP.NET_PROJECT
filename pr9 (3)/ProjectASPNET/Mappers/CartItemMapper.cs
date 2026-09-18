using ProjectASPNET.DTOs;
using ProjectASPNET.Models;

namespace ProjectASPNET.Mappers;

public static class CartItemMapper
{
    public static CartItemDto ToDto(CartItem cartItem)
    {
        return new CartItemDto
        {
            Id = cartItem.IdCartItem,
            CartId = cartItem.CartId,
            ProductId = cartItem.ProductId,
            Quantity = cartItem.Quantity
        };
    }
}

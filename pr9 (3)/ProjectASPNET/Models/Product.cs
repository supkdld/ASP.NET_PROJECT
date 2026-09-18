namespace ProjectASPNET.Models;

public partial class Product
{
    public int IdProduct { get; set; }

    public string NameOfProduct { get; set; } = null!;

    public string? DescriptionProduct { get; set; }

    public decimal Price { get; set; }

    public int CategoryId { get; set; }

    public int InStock { get; set; }

    public bool? IsActive { get; set; }

    public string? ImagePath { get; set; }

    public string? ImageUrl { get; set; }

    public virtual ICollection<CartItem> CartItems { get; set; } = new List<CartItem>();

    public virtual ProductCategory Category { get; set; } = null!;

    public virtual ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
}

namespace ProjectASPNET.Models;

public partial class ProductCategory
{
    public int IdCategory { get; set; }

    public string NameOfCategory { get; set; } = null!;

    public virtual ICollection<Product> Products { get; set; } = new List<Product>();
}

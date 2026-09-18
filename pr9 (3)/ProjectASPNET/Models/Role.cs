namespace ProjectASPNET.Models;

public partial class Role
{
    public int IdRole { get; set; }

    public string NameOfRole { get; set; } = null!;

    public virtual ICollection<User> Users { get; set; } = new List<User>();
}

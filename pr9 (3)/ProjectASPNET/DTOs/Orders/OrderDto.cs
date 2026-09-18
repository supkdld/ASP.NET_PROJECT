namespace ProjectASPNET.DTOs;

public class OrderDto
{
    public int Id { get; set; }

    public int UserId { get; set; }

    public string Status { get; set; } = string.Empty;

    public decimal TotalAmount { get; set; }

    public DateTime? CreatedAt { get; set; }
}

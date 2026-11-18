namespace FranchiseApi.Models;

public class Product
{
    public int Id { get; set; }
    public int BranchId { get; set; }
    public string Name { get; set; } = string.Empty;
    public int Stock { get; set; }

    public Branch? Branch { get; set; }
}
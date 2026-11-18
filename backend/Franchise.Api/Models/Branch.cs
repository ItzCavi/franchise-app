namespace FranchiseApi.Models;

public class Branch
{
    public int Id { get; set; }
    public int FranchiseId { get; set; }
    public string Name { get; set; } = string.Empty;

    public Franchise? Franchise { get; set; }
    public ICollection<Product> Products { get; set; } = new List<Product>();
}

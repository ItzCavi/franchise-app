using System.ComponentModel.DataAnnotations;

namespace FranchiseApi.Dtos;

public class CreateProductRequest
{
    [Required(ErrorMessage = "Name is required.")]
    [StringLength(100, ErrorMessage = "Name must be at most 100 characters.")]
    public string Name { get; set; } = string.Empty;

    [Range(0, int.MaxValue, ErrorMessage = "Stock must be zero or greater.")]
    public int Stock { get; set; }
}
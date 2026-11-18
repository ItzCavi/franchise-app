using System.ComponentModel.DataAnnotations;

namespace FranchiseApi.Dtos;

public class UpdateStockRequest
{
    [Required(ErrorMessage = "Stock is required.")]
    [Range(0, int.MaxValue, ErrorMessage = "Stock must be zero or greater.")]
    public int? Stock { get; set; }
}
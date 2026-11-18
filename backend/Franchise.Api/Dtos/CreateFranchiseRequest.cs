using System.ComponentModel.DataAnnotations;

namespace FranchiseApi.Dtos;

public class CreateFranchiseRequest
{
    [Required(ErrorMessage = "Name is required.")]
    [StringLength(100, ErrorMessage = "Name must be at most 100 characters.")]
    public string Name { get; set; } = string.Empty;
}
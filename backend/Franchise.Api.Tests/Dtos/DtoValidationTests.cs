using System.ComponentModel.DataAnnotations;
using FranchiseApi.Dtos;

namespace FranchiseApi.Tests.Dtos;

public class DtoValidationTests
{
    private static List<ValidationResult> Validate(object model)
    {
        var results = new List<ValidationResult>();
        Validator.TryValidateObject(model, new ValidationContext(model), results, validateAllProperties: true);
        return results;
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void CreateFranchiseRequest_WithEmptyName_IsInvalid(string name)
    {
        var errors = Validate(new CreateFranchiseRequest { Name = name });

        Assert.Contains(errors, e => e.MemberNames.Contains(nameof(CreateFranchiseRequest.Name)));
    }

    [Fact]
    public void CreateFranchiseRequest_WithNameLongerThan100_IsInvalid()
    {
        var errors = Validate(new CreateFranchiseRequest { Name = new string('a', 101) });

        Assert.NotEmpty(errors);
    }

    [Fact]
    public void CreateProductRequest_WithNegativeStock_IsInvalid()
    {
        var errors = Validate(new CreateProductRequest { Name = "Coffee", Stock = -1 });

        Assert.Contains(errors, e => e.MemberNames.Contains(nameof(CreateProductRequest.Stock)));
    }

    [Fact]
    public void UpdateStockRequest_WithoutStock_IsInvalid()
    {
        var errors = Validate(new UpdateStockRequest { Stock = null });

        Assert.NotEmpty(errors);
    }

    [Fact]
    public void UpdateStockRequest_WithZeroStock_IsValid()
    {
        var errors = Validate(new UpdateStockRequest { Stock = 0 });

        Assert.Empty(errors);
    }
}

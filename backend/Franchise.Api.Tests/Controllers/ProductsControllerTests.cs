using FranchiseApi.Controllers;
using FranchiseApi.Dtos;
using FranchiseApi.Models;
using FranchiseApi.Tests.Helpers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace FranchiseApi.Tests.Controllers;

public class ProductsControllerTests
{
    private static async Task<Branch> SeedBranchAsync(FranchiseApi.Data.AppDbContext context)
    {
        var branch = new Branch
        {
            Name = "Downtown",
            Franchise = new Franchise { Name = "Subway" },
        };
        context.Branches.Add(branch);
        await context.SaveChangesAsync();
        return branch;
    }

    [Fact]
    public async Task Create_WithValidData_ReturnsCreatedProduct()
    {
        using var context = TestDbContextFactory.Create();
        var branch = await SeedBranchAsync(context);
        var controller = new ProductsController(context);

        var result = await controller.Create(branch.Id, new CreateProductRequest { Name = "Coffee", Stock = 10 });

        var created = Assert.IsType<CreatedAtActionResult>(result.Result);
        var product = Assert.IsType<ProductResponse>(created.Value);
        Assert.Equal("Coffee", product.Name);
        Assert.Equal(10, product.Stock);
        Assert.Equal(branch.Id, product.BranchId);
    }

    [Fact]
    public async Task Create_WhenBranchDoesNotExist_ReturnsNotFound()
    {
        using var context = TestDbContextFactory.Create();
        var controller = new ProductsController(context);

        var result = await controller.Create(999, new CreateProductRequest { Name = "Coffee", Stock = 10 });

        var problem = Assert.IsType<ObjectResult>(result.Result);
        Assert.Equal(StatusCodes.Status404NotFound, problem.StatusCode);
    }

    [Fact]
    public async Task UpdateStock_WithValidValue_UpdatesProduct()
    {
        using var context = TestDbContextFactory.Create();
        var branch = await SeedBranchAsync(context);
        var product = new Product { BranchId = branch.Id, Name = "Coffee", Stock = 10 };
        context.Products.Add(product);
        await context.SaveChangesAsync();
        var controller = new ProductsController(context);

        var result = await controller.UpdateStock(product.Id, new UpdateStockRequest { Stock = 25 });

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var response = Assert.IsType<ProductResponse>(ok.Value);
        Assert.Equal(25, response.Stock);

        var saved = await context.Products.FindAsync(product.Id);
        Assert.Equal(25, saved!.Stock);
    }

    [Fact]
    public async Task UpdateStock_WhenProductDoesNotExist_ReturnsNotFound()
    {
        using var context = TestDbContextFactory.Create();
        var controller = new ProductsController(context);

        var result = await controller.UpdateStock(999, new UpdateStockRequest { Stock = 5 });

        var problem = Assert.IsType<ObjectResult>(result.Result);
        Assert.Equal(StatusCodes.Status404NotFound, problem.StatusCode);
    }
}
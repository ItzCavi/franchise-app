using FranchiseApi.Controllers;
using FranchiseApi.Dtos;
using FranchiseApi.Models;
using FranchiseApi.Tests.Helpers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FranchiseApi.Tests.Controllers;

public class FranchisesControllerTests
{
    [Fact]
    public async Task Create_WithNewName_ReturnsCreatedAndTrimsName()
    {
        // Arrange
        using var context = TestDbContextFactory.Create();
        var controller = new FranchisesController(context);

        // Act
        var result = await controller.Create(new CreateFranchiseRequest { Name = "  Subway  " });

        // Assert
        var created = Assert.IsType<CreatedAtActionResult>(result.Result);
        var franchise = Assert.IsType<FranchiseResponse>(created.Value);
        Assert.Equal("Subway", franchise.Name);
        Assert.Equal(1, await context.Franchises.CountAsync());
    }

    [Fact]
    public async Task Create_WithDuplicateName_ReturnsConflict()
    {
        using var context = TestDbContextFactory.Create();
        context.Franchises.Add(new Franchise { Name = "Subway" });
        await context.SaveChangesAsync();
        var controller = new FranchisesController(context);

        var result = await controller.Create(new CreateFranchiseRequest { Name = "Subway" });

        var problem = Assert.IsType<ObjectResult>(result.Result);
        Assert.Equal(StatusCodes.Status409Conflict, problem.StatusCode);
        Assert.Equal(1, await context.Franchises.CountAsync());
    }

    [Fact]
    public async Task GetAll_ReturnsFranchisesOrderedByName()
    {
        using var context = TestDbContextFactory.Create();
        context.Franchises.AddRange(
            new Franchise { Name = "Subway" },
            new Franchise { Name = "Burger King" });
        await context.SaveChangesAsync();
        var controller = new FranchisesController(context);

        var result = await controller.GetAll();

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var franchises = Assert.IsAssignableFrom<IEnumerable<FranchiseResponse>>(ok.Value).ToList();
        Assert.Equal(new[] { "Burger King", "Subway" }, franchises.Select(f => f.Name));
    }

    [Fact]
    public async Task GetById_WhenFranchiseDoesNotExist_ReturnsNotFound()
    {
        using var context = TestDbContextFactory.Create();
        var controller = new FranchisesController(context);

        var result = await controller.GetById(999);

        var problem = Assert.IsType<ObjectResult>(result.Result);
        Assert.Equal(StatusCodes.Status404NotFound, problem.StatusCode);
    }
}
using FranchiseApi.Controllers;
using FranchiseApi.Dtos;
using FranchiseApi.Models;
using FranchiseApi.Tests.Helpers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace FranchiseApi.Tests.Controllers;

public class BranchesControllerTests
{
    [Fact]
    public async Task Create_WhenFranchiseDoesNotExist_ReturnsNotFound()
    {
        using var context = TestDbContextFactory.Create();
        var controller = new BranchesController(context);

        var result = await controller.Create(999, new CreateBranchRequest { Name = "Downtown" });

        var problem = Assert.IsType<ObjectResult>(result.Result);
        Assert.Equal(StatusCodes.Status404NotFound, problem.StatusCode);
    }

    [Fact]
    public async Task Create_SameNameInDifferentFranchises_IsAllowed()
    {
        using var context = TestDbContextFactory.Create();
        var subway = new Franchise { Name = "Subway" };
        var burgerKing = new Franchise { Name = "Burger King" };
        context.Franchises.AddRange(subway, burgerKing);
        context.Branches.Add(new Branch { Franchise = subway, Name = "Downtown" });
        await context.SaveChangesAsync();
        var controller = new BranchesController(context);

        var result = await controller.Create(burgerKing.Id, new CreateBranchRequest { Name = "Downtown" });

        var created = Assert.IsType<CreatedAtActionResult>(result.Result);
        var branch = Assert.IsType<BranchResponse>(created.Value);
        Assert.Equal(burgerKing.Id, branch.FranchiseId);
    }

    [Fact]
    public async Task Create_DuplicateNameInSameFranchise_ReturnsConflict()
    {
        using var context = TestDbContextFactory.Create();
        var subway = new Franchise { Name = "Subway" };
        context.Branches.Add(new Branch { Franchise = subway, Name = "Downtown" });
        await context.SaveChangesAsync();
        var controller = new BranchesController(context);

        var result = await controller.Create(subway.Id, new CreateBranchRequest { Name = "Downtown" });

        var problem = Assert.IsType<ObjectResult>(result.Result);
        Assert.Equal(StatusCodes.Status409Conflict, problem.StatusCode);
    }

    [Fact]
    public async Task GetByFranchise_ReturnsOnlyBranchesOfThatFranchise()
    {
        using var context = TestDbContextFactory.Create();
        var subway = new Franchise { Name = "Subway" };
        var burgerKing = new Franchise { Name = "Burger King" };
        context.Branches.AddRange(
            new Branch { Franchise = subway, Name = "Downtown" },
            new Branch { Franchise = burgerKing, Name = "Airport" });
        await context.SaveChangesAsync();
        var controller = new BranchesController(context);

        var result = await controller.GetByFranchise(subway.Id);

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var branches = Assert.IsAssignableFrom<IEnumerable<BranchResponse>>(ok.Value).ToList();
        var branch = Assert.Single(branches);
        Assert.Equal("Downtown", branch.Name);
    }
}
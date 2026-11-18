using FranchiseApi.Controllers;
using FranchiseApi.Dtos;
using FranchiseApi.Models;
using FranchiseApi.Tests.Helpers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace FranchiseApi.Tests.Controllers;

public class ReportsControllerTests
{
    [Fact]
    public async Task GetFranchiseReport_ReturnsTotalsAndTopProductPerBranch()
    {
        using var context = TestDbContextFactory.Create();
        var franchise = new Franchise { Name = "Subway" };
        var downtown = new Branch { Franchise = franchise, Name = "Downtown" };
        var airport = new Branch { Franchise = franchise, Name = "Airport" };
        context.Branches.AddRange(downtown, airport);
        context.Products.AddRange(
            new Product { Branch = downtown, Name = "Coffee", Stock = 50 },
            new Product { Branch = downtown, Name = "Tea", Stock = 20 },
            new Product { Branch = downtown, Name = "Cookie", Stock = 10 });
        await context.SaveChangesAsync();
        var controller = new ReportsController(context);

        var result = await controller.GetFranchiseReport(franchise.Id);

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var report = Assert.IsType<FranchiseReportResponse>(ok.Value);
        Assert.Equal(2, report.BranchCount);
        Assert.Equal(3, report.ProductCount);
        Assert.Equal(80, report.TotalStock);

        var downtownSummary = Assert.Single(report.Branches, b => b.BranchName == "Downtown");
        Assert.Equal(80, downtownSummary.TotalStock);
        Assert.Equal("Coffee", downtownSummary.TopProductName);
        Assert.Equal(50, downtownSummary.TopProductStock);

        var airportSummary = Assert.Single(report.Branches, b => b.BranchName == "Airport");
        Assert.Equal(0, airportSummary.ProductCount);
        Assert.Null(airportSummary.TopProductName);
    }

    [Fact]
    public async Task GetFranchiseReport_WhenFranchiseDoesNotExist_ReturnsNotFound()
    {
        using var context = TestDbContextFactory.Create();
        var controller = new ReportsController(context);

        var result = await controller.GetFranchiseReport(999);

        var problem = Assert.IsType<ObjectResult>(result.Result);
        Assert.Equal(StatusCodes.Status404NotFound, problem.StatusCode);
    }
}
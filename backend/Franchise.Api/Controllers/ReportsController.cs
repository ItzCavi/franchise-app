using FranchiseApi.Data;
using FranchiseApi.Dtos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FranchiseApi.Controllers;

[ApiController]
[Route("api/franchises/{franchiseId:int}/report")]
public class ReportsController : ControllerBase
{
    private readonly AppDbContext _context;

    public ReportsController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<FranchiseReportResponse>> GetFranchiseReport(int franchiseId)
    {
        var franchise = await _context.Franchises
            .AsNoTracking()
            .Include(f => f.Branches)
                .ThenInclude(b => b.Products)
            .FirstOrDefaultAsync(f => f.Id == franchiseId);

        if (franchise is null)
        {
            return Problem(
                statusCode: StatusCodes.Status404NotFound,
                title: "Franchise not found.",
                detail: $"No franchise exists with id {franchiseId}.");
        }

        var branches = franchise.Branches
            .OrderBy(b => b.Name)
            .Select(b =>
            {
                var topProduct = b.Products
                    .OrderByDescending(p => p.Stock)
                    .ThenBy(p => p.Name)
                    .FirstOrDefault();

                return new BranchStockSummary(
                    b.Id,
                    b.Name,
                    b.Products.Count,
                    b.Products.Sum(p => p.Stock),
                    topProduct?.Name,
                    topProduct?.Stock);
            })
            .ToList();

        var report = new FranchiseReportResponse(
            franchise.Id,
            franchise.Name,
            branches.Count,
            branches.Sum(b => b.ProductCount),
            branches.Sum(b => b.TotalStock),
            branches);

        return Ok(report);
    }
}
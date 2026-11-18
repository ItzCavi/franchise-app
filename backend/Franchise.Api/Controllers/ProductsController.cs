using FranchiseApi.Data;
using FranchiseApi.Dtos;
using FranchiseApi.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FranchiseApi.Controllers;

[ApiController]
[Route("api")]
public class ProductsController : ControllerBase
{
    private readonly AppDbContext _context;

    public ProductsController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet("branches/{branchId:int}/products")]
    public async Task<ActionResult<IEnumerable<ProductResponse>>> GetByBranch(int branchId)
    {
        var branchExists = await _context.Branches.AnyAsync(b => b.Id == branchId);
        if (!branchExists)
        {
            return BranchNotFound(branchId);
        }

        var products = await _context.Products
            .Where(p => p.BranchId == branchId)
            .OrderBy(p => p.Name)
            .Select(p => new ProductResponse(p.Id, p.BranchId, p.Name, p.Stock))
            .ToListAsync();

        return Ok(products);
    }

    [HttpGet("products/{id:int}")]
    public async Task<ActionResult<ProductResponse>> GetById(int id)
    {
        var product = await _context.Products.FindAsync(id);

        if (product is null)
        {
            return ProductNotFound(id);
        }

        return Ok(ToResponse(product));
    }

    [HttpPost("branches/{branchId:int}/products")]
    public async Task<ActionResult<ProductResponse>> Create(int branchId, CreateProductRequest request)
    {
        var branchExists = await _context.Branches.AnyAsync(b => b.Id == branchId);
        if (!branchExists)
        {
            return BranchNotFound(branchId);
        }

        var name = request.Name.Trim();

        var exists = await _context.Products
            .AnyAsync(p => p.BranchId == branchId && p.Name == name);
        if (exists)
        {
            return Problem(
                statusCode: StatusCodes.Status409Conflict,
                title: "Product already exists.",
                detail: $"The branch already has a product named '{name}'.");
        }

        var product = new Product { BranchId = branchId, Name = name, Stock = request.Stock };
        _context.Products.Add(product);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetById), new { id = product.Id }, ToResponse(product));
    }

    [HttpPatch("products/{id:int}/stock")]
    public async Task<ActionResult<ProductResponse>> UpdateStock(int id, UpdateStockRequest request)
    {
        var product = await _context.Products.FindAsync(id);

        if (product is null)
        {
            return ProductNotFound(id);
        }

        product.Stock = request.Stock!.Value;
        await _context.SaveChangesAsync();

        return Ok(ToResponse(product));
    }

    private static ProductResponse ToResponse(Product product)
    {
        return new ProductResponse(product.Id, product.BranchId, product.Name, product.Stock);
    }

    private ObjectResult BranchNotFound(int branchId)
    {
        return Problem(
            statusCode: StatusCodes.Status404NotFound,
            title: "Branch not found.",
            detail: $"No branch exists with id {branchId}.");
    }

    private ObjectResult ProductNotFound(int id)
    {
        return Problem(
            statusCode: StatusCodes.Status404NotFound,
            title: "Product not found.",
            detail: $"No product exists with id {id}.");
    }
}
using FranchiseApi.Data;
using FranchiseApi.Dtos;
using FranchiseApi.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FranchiseApi.Controllers;

[ApiController]
[Route("api")]
public class BranchesController : ControllerBase
{
    private readonly AppDbContext _context;

    public BranchesController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet("franchises/{franchiseId:int}/branches")]
    public async Task<ActionResult<IEnumerable<BranchResponse>>> GetByFranchise(int franchiseId)
    {
        var franchiseExists = await _context.Franchises.AnyAsync(f => f.Id == franchiseId);
        if (!franchiseExists)
        {
            return FranchiseNotFound(franchiseId);
        }

        var branches = await _context.Branches
            .Where(b => b.FranchiseId == franchiseId)
            .OrderBy(b => b.Name)
            .Select(b => new BranchResponse(b.Id, b.FranchiseId, b.Name))
            .ToListAsync();

        return Ok(branches);
    }

    [HttpGet("branches/{id:int}")]
    public async Task<ActionResult<BranchResponse>> GetById(int id)
    {
        var branch = await _context.Branches.FindAsync(id);

        if (branch is null)
        {
            return Problem(
                statusCode: StatusCodes.Status404NotFound,
                title: "Branch not found.",
                detail: $"No branch exists with id {id}.");
        }

        return Ok(new BranchResponse(branch.Id, branch.FranchiseId, branch.Name));
    }

    [HttpPost("franchises/{franchiseId:int}/branches")]
    public async Task<ActionResult<BranchResponse>> Create(int franchiseId, CreateBranchRequest request)
    {
        var franchiseExists = await _context.Franchises.AnyAsync(f => f.Id == franchiseId);
        if (!franchiseExists)
        {
            return FranchiseNotFound(franchiseId);
        }

        var name = request.Name.Trim();

        var exists = await _context.Branches
            .AnyAsync(b => b.FranchiseId == franchiseId && b.Name == name);
        if (exists)
        {
            return Problem(
                statusCode: StatusCodes.Status409Conflict,
                title: "Branch already exists.",
                detail: $"The franchise already has a branch named '{name}'.");
        }

        var branch = new Branch { FranchiseId = franchiseId, Name = name };
        _context.Branches.Add(branch);
        await _context.SaveChangesAsync();

        var response = new BranchResponse(branch.Id, branch.FranchiseId, branch.Name);
        return CreatedAtAction(nameof(GetById), new { id = branch.Id }, response);
    }

    private ObjectResult FranchiseNotFound(int franchiseId)
    {
        return Problem(
            statusCode: StatusCodes.Status404NotFound,
            title: "Franchise not found.",
            detail: $"No franchise exists with id {franchiseId}.");
    }
}
using FranchiseApi.Data;
using FranchiseApi.Dtos;
using FranchiseApi.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FranchiseApi.Controllers;

[ApiController]
[Route("api/franchises")]
public class FranchisesController : ControllerBase
{
    private readonly AppDbContext _context;

    public FranchisesController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<FranchiseResponse>>> GetAll()
    {
        var franchises = await _context.Franchises
            .OrderBy(f => f.Name)
            .Select(f => new FranchiseResponse(f.Id, f.Name))
            .ToListAsync();

        return Ok(franchises);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<FranchiseResponse>> GetById(int id)
    {
        var franchise = await _context.Franchises.FindAsync(id);

        if (franchise is null)
        {
            return Problem(
                statusCode: StatusCodes.Status404NotFound,
                title: "Franchise not found.",
                detail: $"No franchise exists with id {id}.");
        }

        return Ok(new FranchiseResponse(franchise.Id, franchise.Name));
    }

    [HttpPost]
    public async Task<ActionResult<FranchiseResponse>> Create(CreateFranchiseRequest request)
    {
        var name = request.Name.Trim();

        var exists = await _context.Franchises.AnyAsync(f => f.Name == name);
        if (exists)
        {
            return Problem(
                statusCode: StatusCodes.Status409Conflict,
                title: "Franchise already exists.",
                detail: $"A franchise named '{name}' already exists.");
        }

        var franchise = new Franchise { Name = name };
        _context.Franchises.Add(franchise);
        await _context.SaveChangesAsync();

        var response = new FranchiseResponse(franchise.Id, franchise.Name);
        return CreatedAtAction(nameof(GetById), new { id = franchise.Id }, response);
    }
}
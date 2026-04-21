using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PropertyRentalManagement.Data;
using PropertyRentalManagement.Models;

namespace PropertyRentalManagement.Controllers.Api;

[ApiController]
[Route("api/buildings")] // FIXED: Web API controllers (not just MVC views)
[Authorize]
public class BuildingsApiController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public BuildingsApiController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll() =>
        Ok(await _context.Buildings.AsNoTracking().ToListAsync());

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var building = await _context.Buildings.AsNoTracking().FirstOrDefaultAsync(b => b.Id == id);
        return building == null ? NotFound() : Ok(building);
    }

    [HttpPost]
    [Authorize(Roles = $"{UserRoles.Owner},{UserRoles.Manager}")]
    public async Task<IActionResult> Create(Building model)
    {
        if (!ModelState.IsValid) return ValidationProblem(ModelState);
        _context.Buildings.Add(model);
        await _context.SaveChangesAsync();
        return CreatedAtAction(nameof(GetById), new { id = model.Id }, model);
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = $"{UserRoles.Owner},{UserRoles.Manager}")]
    public async Task<IActionResult> Update(int id, Building model)
    {
        if (id != model.Id) return BadRequest();
        if (!ModelState.IsValid) return ValidationProblem(ModelState);

        var exists = await _context.Buildings.AnyAsync(b => b.Id == id);
        if (!exists) return NotFound();

        _context.Update(model);
        await _context.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = $"{UserRoles.Owner},{UserRoles.Manager}")]
    public async Task<IActionResult> Delete(int id)
    {
        var building = await _context.Buildings.FindAsync(id);
        if (building == null) return NotFound();
        _context.Buildings.Remove(building);
        await _context.SaveChangesAsync();
        return NoContent();
    }
}

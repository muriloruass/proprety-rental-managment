using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PropertyRentalManagement.Data;
using PropertyRentalManagement.Models;

namespace PropertyRentalManagement.Controllers.Api;

[ApiController]
[Route("api/apartments")] // FIXED: Web API controllers (not just MVC views)
[Authorize]
public class ApartmentsApiController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public ApartmentsApiController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll() =>
        Ok(await _context.Apartments.AsNoTracking().Include(a => a.Building).ToListAsync());

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var apartment = await _context.Apartments.AsNoTracking().Include(a => a.Building).FirstOrDefaultAsync(a => a.Id == id);
        return apartment == null ? NotFound() : Ok(apartment);
    }

    [HttpPost]
    [Authorize(Roles = $"{UserRoles.Owner},{UserRoles.Manager}")]
    public async Task<IActionResult> Create(Apartment model)
    {
        if (!ModelState.IsValid) return ValidationProblem(ModelState);
        _context.Apartments.Add(model);
        await _context.SaveChangesAsync();
        return CreatedAtAction(nameof(GetById), new { id = model.Id }, model);
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = $"{UserRoles.Owner},{UserRoles.Manager}")]
    public async Task<IActionResult> Update(int id, Apartment model)
    {
        if (id != model.Id) return BadRequest();
        if (!ModelState.IsValid) return ValidationProblem(ModelState);
        if (!await _context.Apartments.AnyAsync(a => a.Id == id)) return NotFound();

        _context.Update(model);
        await _context.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = $"{UserRoles.Owner},{UserRoles.Manager}")]
    public async Task<IActionResult> Delete(int id)
    {
        var apartment = await _context.Apartments.FindAsync(id);
        if (apartment == null) return NotFound();
        _context.Apartments.Remove(apartment);
        await _context.SaveChangesAsync();
        return NoContent();
    }
}
